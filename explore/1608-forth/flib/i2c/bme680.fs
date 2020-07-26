\ read out the BME680 sensor
\ needs i2c

[ifndef] BME.ADDR  $76 constant BME.ADDR  [then]

: bme-reset ( -- ) \ software reset of the bme680
  BME.ADDR i2c-addr
  $E0 >i2c $B6 >i2c
  0 i2c-xfer drop ;

: bme-init ( -- nak ) \ init the bme680 into continuous mode
  i2c-init bme-reset
  BME.ADDR i2c-addr
  $72 >i2c %1 >i2c                  \ ctrl_hum - hum 1x oversamp
  $74 >i2c %100101 >i2c             \ ctrl_meas - temp/pres 1x oversamp, forced mode
  $71 >i2c %000 >i2c                \ config - filter off
                                    \ ctrl_gas_1 - gas heater setpoint
  0 i2c-xfer ;

: bme-sleep ( -- )                  \ force bme680 to sleep
  BME.ADDR i2c-addr
  $74 >i2c %00 >i2c
  0 i2c-xfer drop ;

: bme-convert ( -- ms ) \ perform a one-shot forced reading, return ms before data is ready
  BME.ADDR i2c-addr
  $74 >i2c %100101 >i2c             \ forced mode, 1x oversampling of temp/pressure
  0 i2c-xfer drop
  10 ;

30 buffer: params       \ calibration data
 8 buffer: values       \ last reading
0 variable tfine        \ used for p & h compensation

: bme-i2c+ ( addr -- addr+1 ) i2c> over c! 1+ ;

: bme-rd ( addr n reg -- addr+n )
  BME.ADDR i2c-addr >i2c
  dup i2c-xfer drop
  0 do bme-i2c+ loop
  ;

: bme-calib ( -- )                  \ get calibration parameters reading
  params
  5 $8E bme-rd      \ t1-t3
  6 $94 bme-rd      \ p1-p5, p7, p6
  4 $9C bme-rd      \ p8-9
  1 $A0 bme-rd      \ p10
  1 $E2 bme-rd      \ h1 (LSB) <7:4>
  1 $E1 bme-rd      \ h1 (MSB)
  1 $E2 bme-rd      \ h2 (LSB) <7:4>
  1 $E3 bme-rd      \ h2 (MSB)
  1 $E1 bme-rd      \ h1
  5 $E4 bme-rd      \ h3-7
  \ TODO gas
  \ 1 $ED bme-rd        \ g1
  \ 2 $EB bme-rd        \ g2
  \ 1 $EE bme-rd        \ g3
  drop ;

: bme-data ( -- )                   \ get a sensor reading from the BME680
  values  8 $1F bme-rd
  \ TODO add gas
  drop
  ;

: bme-u8 ( off -- val ) params + c@ ;
: bme-u12 ( off -- val ) dup 4 bme-u8 rshift swap 1+ bme-u8 4 lshift or ;
: bme-s12 ( off -- val ) bme-u12 12 lshift 12 arshift ;
: bme-u16 ( off -- val ) dup bme-u8 swap 1+ bme-u8 8 lshift or ;
: bme-s16 ( off -- val ) bme-u16 16 lshift 16 arshift ;
: bme-u20be ( off -- val )
  values + dup c@ 12 lshift swap 1+
           dup c@  4 lshift swap 1+
               c@  4 rshift  or or ;

: bme-hpt ( -- rawh rawp rawt )
  values 6 + dup c@ 8 lshift swap 1+ c@ or  0 bme-u20be  3 bme-u20be ;

: *>> ( n1 n2 u -- n ) >r * r> arshift ;  \ (n1 * n2) >> u
: ^2>> ( n1 u -- n ) >r dup * r> arshift ;  \ (n1 * n1) >> u

: tcalc ( rawt -- t100 )
  \ var1 = ((int32_t)temp_adc >> 3) - ((int32_t)par_t1 << 1)
  \ var2 = (var1 * (int32_t)par_t2) >> 11
  \ var3 = ((((var1 >> 1) * (var1 >> 1)) >> 12) * ((int32_t)par_t3 << 4)) >> 14;

  3 rshift dup shr swap
  ( temp_adc ) 3 3 bme-s16 arshift ( par_t1 ) 0 bme-s16 shl -
  ( var1 ) dup  ( par_t2 ) 2 bme-s16 11 *>>
  ( var1 ) over shr 12 ^2>>  4 ( par_t3 ) 4 bme-s16 lshift * 14 swap arshift
  ( var2 ) ( var3 ) + dup tfine !
  5 * 128 + 8 arshift
  ;

: pcalc ( rawp -- p1 )
  \ var1 = ((int32_t)t_fine >> 1) - 64000;
  \ var2 = ((((var1 >> 2) * (var1 >> 2)) >> 11) * (int32_t)par_p6) >> 2;
  \ var2 = var2 + ((var1 * (int32_t)par_p5) << 1);
  \ var2 = (var2 >> 2) + ((int32_t)par_p4 << 16);
  \ var1 = (((((var1 >> 2) * (var1 >> 2)) >> 13) * (( int32_t)par_p3 << 5)) >> 3)
  \             + (((int32_t)par_p2 * var1) >> 1);
  \ var1 = var1 >> 18;
  \ var1 = ((32768 + var1) * (int32_t)par_p1) >> 15;
  \ press_comp = 1048576 - press_raw;
  \ press_comp = (uint32_t)((press_comp - (var2 >> 12)) * ((uint32_t)3125));

  \ if (press_comp >= (1 << 30))
  \  press_comp = ((press_comp / (uint32_t)var1) << 1);
  \ else
  \  press_comp = ((press_comp << 1) / (uint32_t)var1);

  \ var1 = ((int32_t)par_p9 * (int32_t)(((press_comp >> 3) * (press_comp >> 3)) >> 13)) >> 12;

  \ var2 = ((int32_t)(press_comp >> 2) * (int32_t)par_p8) >> 13;

  \ var3 = ((int32_t)(press_comp >> 8) * (int32_t)(press_comp >> 8) * (int32_t)(press_comp >> 8)
  \             * (int32_t)par_p10) >> 17;

  \ press_comp = (int32_t)(press_comp) +  ((var1 + var2 + var3 + ((int32_t)par_p7 << 7)) >> 4);

  tfine @ shr 64000 -
  dup 2 arshift 11 ^2>>  ( par_p6 ) 15 bme-s16 *
  ( var1 ) over  ( par_p5 ) 13 bme-s16 shl * +
  ( var2 ) 2 arshift ( par_p4 ) 11 bme-s16 16 lshift + swap
  dup 2 arshift 13 ^2>> ( par_p3 ) 5 10 bme-s16 lshift 3 *>>
  swap ( par_p2 ) 8 bme-s16 1 *>> + 18 arshift
  32768 + ( par_p1 ) 5 bme-u16 15 *>>
  rot 1048576 swap - rot 12 arshift - 3125 *
  dup 1 30 lshift >= if swap / shr else shr swap / then
  ( par_p9 ) 19 bme-s16 over 3 arshift 13 ^2>> 12 *>>
  over 2 arshift ( par_p8 ) 17 bme-s16 13 *>>
  rot 8 over arshift dup dup * * ( par_p10 ) 21 bme-s16 17 *>>
  rot + rot + 7 ( par_p7 ) 15 bme-s16 lshift + 4 arshift +
  ;

: hcalc ( rawh -- h100 )
  \ temp_scaled = (int32_t)temp_comp;
  \ var1 = (int32_t)hum_adc - (int32_t)((int32_t)par_h1 << 4)
  \			- (((temp_scaled * (int32_t)par_h3) / ((int32_t)100)) >> 1);
  \ var2 = ((int32_t)par_h2
  \         * (((temp_scaled * (int32_t)par_h4) / ((int32_t)100))
  \ 		+ (((temp_scaled * ((temp_scaled * (int32_t)par_h5) /((int32_t)100))) >> 6)
  \             / ((int32_t)100))
  \         + ((int32_t)(1 << 14))))
  \         >> 10;
  \ var3 = var1 * var2;
  \ var4 = (((int32_t)par_h6 << 7) + ((temp_scaled * (int32_t)par_h7) / ((int32_t)100))) >> 4;
  \ var5 = ((var3 >> 14) * (var3 >> 14)) >> 10;
  \ var6 = (var4 * var5) >> 1;
  \ hum_comp = (var3 + var6) >> 12;
  \ hum_comp = (((var3 + var6) >> 10) * ((int32_t) 1000)) >> 12;

  tfine @ 5 * 128 + 8 arshift
  swap 4 ( par_h1 ) 21 bme-s12 lshift -
  over ( par_h3 ) 25 bme-s16 * 100 / 1 swap arshift
  over ( par_h2 ) 23 bme-s12
  ( temp_scaled ) over ( par_h4 ) 26 bme-s16 * 100 /
  ( temp_scaled ) over over ( par_h5 ) 27 bme-s16 100 / 6 *>> 100 / +
  1 14 lshift + 10 *>> *
  ( temp_scaled ) rot ( par_h7 ) 29 bme-s16 * 100 /
  7 ( par_h6 ) 28 bme-s16 lshift + 4 swap arshift
  over 14 swap 10 *>> 1 *>>
  ( v3+v6 )
  + dup 12 swap arshift                 \ hum_comp
  \ swap 10 swap arshift 1000 12 *>>      \ hum_comp
  ( hum_comp1 hum_comp2 )
  ;

: bme-calc ( -- h p t )  \ convert reading to calibrated values
  bme-hpt tcalc -rot pcalc -rot hcalc -rot ;

\ bme-init .
\ bme-calib params 32 dump
\ bme-data bme-calc . . .
