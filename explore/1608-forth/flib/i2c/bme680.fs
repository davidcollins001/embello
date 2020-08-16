\ read out the BME680 sensor
\ needs i2c

[ifndef] BME.ADDR  $77 constant BME.ADDR  [then]

\ $72 constant BME:CTRL_HUM
\ $73 constant BME:STATUS
\ $74 constant BME:CTRL_MEAS
\ $75 constant BME:CONFIG
\ $E0 constant BME:RESET

   31 buffer: bme.params       \ calibration data
   10 buffer: bme.values       \ last reading
1000 variable bme.tfine        \ used for p & h compensation

create bme:gas_range			\ gas resistance constants
  \ array1 , array2
  2147483647 , 4096000000 ,
  2147483647 , 2048000000 ,
  2147483647 , 1024000000 ,
  2147483647 , 512000000 ,
  2147483647 , 255744255 ,
  2126008810 , 127110228 ,
  2147483647 , 64000000 ,
  2130303777 , 32258064 ,
  2147483647 , 16016016 ,
  2147483647 , 8000000 ,
  2143188679 , 4000000 ,
  2136746228 , 2000000 ,
  2147483647 , 1000000 ,
  2126008810 , 500000 ,
  2147483647 , 250000 ,
  2147483647 , 125000 ,
align

: bme-reset ( -- ) \ software reset of the bme680
  $E0 >i2c $B6 >i2c
  0 i2c-xfer drop
  ;

: bme-i2c+ ( addr -- addr+1 ) i2c> over c! 1+ ;

: bme-rd ( addr n reg -- addr+n )
  >i2c
  dup i2c-xfer drop
  0 do bme-i2c+ loop
  ;

: bme-u8 ( off -- val ) bme.params + c@ ;
: bme-s8 ( off -- val ) bme-u8 8 lshift 8 arshift ;
: bme-u12 ( off -- val ) dup 4 bme-u8 rshift swap 1+ bme-u8 4 lshift or ;
: bme-s12 ( off -- val ) bme-u12 12 lshift 12 arshift ;
: bme-u16 ( off -- val ) dup bme-u8 swap 1+ bme-u8 8 lshift or ;
: bme-s16 ( off -- val ) bme-u16 16 lshift 16 arshift ;
: bme-u20be ( off -- val )
  bme.values + dup c@ 12 lshift swap 1+
           dup c@  4 lshift swap 1+
               c@  4 rshift  or or ;
: bme-res-heat-val ( -- val )
  28 bme-s8 4 rshift %11 and
  ;

: bme-res-heat ( temp -- )
  \ var1 = (((int32_t)amb_temp * par_g3) / 10) << 8;
  \ var2 = (par_g1 + 784) *
  \     (((((par_g2 + 154009) * target_temp * 5) / 100) + 3276800) / 10);
  \ var3 = var1 + (var2 >> 1);
  \ var4 = (var3 / (res_heat_range + 4));
  \ var5 = (131 * res_heat_val) + 65536;
  \ res_heat_x100 = (int32_t)(((var4 / var5) - 250) * 34);

  bme.tfine @ 5 * 128 + 8 arshift ( par_g3 ) 27 bme-s8 * 10 / 8 lshift
  26 bme-s8 784 +
  24 bme-s16 154009 + ( temp ) * 5 * 100 / 3276800 + 10 / *
  ( var1 ) ( var2 ) 1 arshift +
  ( var3 ) ( res_heat_range ) 29 bme-s8 4 + /
  131 bme-res-heat-val 65536 +
  ( var4 ) ( var5 ) / 250 - 34 *
  ;

: bme-init ( -- nak )               \ init the bme680 into continuous mode
  i2c-init bme-reset
  BME.ADDR i2c-addr

  \ chip id
  \ bme.values 1 $D0 bme-rd
  \ 1 i2c-xfer

  \ set oversampling and sleep mode
  $72 >i2c %1 >i2c                  \ ctrl_hum - hum 1x oversamp
  $74 >i2c %100100 >i2c             \ ctrl_meas - temp/pres 1x oversamp
  $71 >i2c %000 >i2c                \ config - filter off
                                    \ ctrl_gas_1 - gas heater setpoint
  \ \ gat wait regs - $64-$6D
  \ $64 >i2c %01011001 >i2c           \ gas wait 0 100ms
  \ $5A >i2c bme-res-heat
  \ \ $50
  \ \ $71 >i2c %1000 >i2c               \ use profile 0

  \ $71 >i2c %10000 >i2c              \ enable run_gas
  $74 >i2c %100101 >i2c             \ forced mode
  0 i2c-xfer
  ;

: bme-run ( -- )
  \ enable run_gas and mode
  ;

: bme-sleep ( -- )                  \ force bme680 to sleep
  $72 >i2c 0 >i2c                   \ ctrl_hum - hum 1x oversamp
  $74 >i2c 0 >i2c
  $70 >i2c 1 >i2c                   \ switch heater off
  0 i2c-xfer drop
  ;

: bme-convert ( -- ms )             \ forced reading, return ms before data is ready
  $72 >i2c %1 >i2c                  \ ctrl_hum - hum 1x oversamp
  $74 >i2c %100101 >i2c             \ forced mode, 1x oversampling of temp/pressure
  0 i2c-xfer drop
  10
  ;

: bme-calib ( -- )                  \ get calibration parameters reading
  bme.params
  2 $E9 bme-rd      \ t1
  3 $8A bme-rd      \ t2-t3
  6 $94 bme-rd      \ p1-p5, p7, p6
  4 $9C bme-rd      \ p8-9
  1 $A0 bme-rd      \ p10
  8 $E4 bme-rd      \ h1-7
  \ TODO gas
  4 $EB bme-rd      \ g2, g1, g3
  1  $0 bme-rd      \ res_heat_val [5:4]
  1  $2 bme-rd      \ res_heat_range
  1  $4 bme-rd      \ range_switching_err
  drop
  ;

: bme-hpt ( -- rawh rawp rawt )
  bme.values 10 $1F bme-rd          \ get a sensor reading from the BME680
  drop
  bme.values 6 + dup c@ 8 lshift swap 1+ c@ or  0 bme-u20be  3 bme-u20be
  ;

: *>> ( n1 n2 u -- n ) >r * r> arshift ;  \ (n1 * n2) >> u
: ^2>> ( n1 u -- n ) >r dup * r> arshift ;  \ (n1 * n1) >> u

: bme-tcalc ( rawt -- t100 )
  \ var1 = ((int32_t)temp_adc >> 3) - ((int32_t)par_t1 << 1)
  \ var2 = (var1 * (int32_t)par_t2) >> 11
  \ var3 = ((((var1 >> 1) * (var1 >> 1)) >> 12) * ((int32_t)par_t3 << 4)) >> 14;
  \ t_fine = var2 + var3;
  \ temp_comp = ((t_fine * 5) + 128) >> 8;

  \ var1 = ((((adc_T>>3) - ((BME280_S32_t)dig_T1<<1))) * ((BME280_S32_t)dig_T2)) >> 11;
  \ var2 = (((((adc_T>>4) - ((BME280_S32_t)dig_T1)) * ((adc_T>>4) - ((BME280_S32_t)dig_T1))) >> 12)
  \             * ((BME280_S32_t)dig_T3)) >> 14;
  \ t_fine = var1 + var2;
  \ T  = (t_fine * 5 + 128) >> 8;

  \ var1 = (temperature_adc >> 3) - (self.calibration_data.par_t1 << 1)
  \ var2 = (var1 * self.calibration_data.par_t2) >> 11
  \ var3 = ((var1 >> 1) * (var1 >> 1)) >> 12
  \ var3 = ((var3) * (self.calibration_data.par_t3 << 4)) >> 14

  \ # Save teperature data for pressure calculations
  \ self.calibration_data.t_fine = (var2 + var3) + self.offset_temp_in_t_fine
  \ calc_temp = (((self.calibration_data.t_fine * 5) + 128) >> 8)

  \ ( temp_adc ) 3 arshift ( par_t1 ) 0 bme-u16 -
  \ ( var1 ) dup ( par_t2 ) 2 bme-s16 11 *>>
  \ ( var1 ) swap shr 12 ^2>>  ( par_t3 ) 4 bme-s8 4 lshift 14 *>>
  \ + dup bme.tfine !
  \ 5 * 128 + 8 arshift
  \ ;

  ( temp_adc ) 3 rshift ( par_t1 ) 0 bme-u16 shl -
  ( var1 ) dup ( par_t2 ) 2 bme-s16 11 *>>
  ( var1 ) swap 1 arshift 12 ^2>>  ( par_t3 ) 4 bme-s8 4 lshift 14 *>>
  + dup bme.tfine !
  5 * 128 + 8 arshift
  ;

: bme-pcalc ( rawp -- p1 )
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

  \ + 5

  bme.tfine @ shr 64000 -
  ( var1 ) dup 2 arshift 11 ^2>>  ( par_p6 ) 15 bme-u8 2 arshift *
  ( var1 ) over  ( par_p5 ) 12 bme-s16 shl * +
  ( var2 ) 2 arshift ( par_p4 ) 10 bme-s16 16 lshift +  \ swap
  ( var1 ) over 2 arshift 13 ^2>> ( par_p3 ) 9 bme-u8 5 lshift 3 *>>
  rot ( par_p2 ) 7 bme-s16 1 *>> + 18 arshift
  ( var1 ) 32768 + ( par_p1 ) 5 bme-u16 15 *>>
  rot ( press_raw ) 1048576  swap -
  ( press_comp ) rot 12 arshift - 3125 *
  dup 1 30 lshift >= if swap / shl else shl swap / then
  ( par_p9 ) 18 bme-s16 over 3 arshift 13 ^2>> 12 *>>
  over 2 arshift ( par_p8 ) 16 bme-s16 13 *>>
  rot dup 8 arshift dup dup * * ( par_p10 ) 20 bme-u8 17 *>>
  rot + rot + ( par_p7 ) 14 bme-u8 7 lshift + 4 arshift +
  ;

: bme-hcalc ( rawh -- h100 )
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

  bme.tfine @ 5 * 128 + 8 arshift
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

: bme-gas-range-r ( n -- u )
  bme:gas_range swap 29 bme-s8 $f and 2 * + cells + @
  ;

: gcalc ( rawg -- g1 )
  \ int64_t var1 = (int64_t)(((1340 + (5 * (int64_t)range_switching_error))
  \     * (( int64_t)const_array1_int[gas_range])) >> 16);
  \ int64_t var2 = (int64_t)(gas_adc << 15) - (int64_t)(1 << 24) + var1;
  \ int32_t gas_res = (int32_t)((((int64_t)(const_array2_int[gas_range]
  \     * (int64_t)var1) >> 9) + (var2 >> 1)) / var2);

  1340 5 24 bme-s8 * + 0 bme-gas-range-r 16 *>>
  dup 28 bme-s12 15 lshift 1 24 lshift - ( var1 ) +
  ( var1 ) 9 arshift ( var2 ) dup 1 arshift 1 bme-gas-range-r /
  ;

: bme-calc ( -- h p t )  \ convert reading to calibrated bme.values
  bme-hpt bme-tcalc -rot bme-pcalc -rot bme-hcalc -rot
  \ gcalc
  ;
  \ 3 bme-u20be bme-tcalc
  \ 0 bme-u20be bme-pcalc

: bme-data-ready ( -- ? )
  bme.values 1 $1D  bme-rd  7 bit bme.values bit@
  ;

\ bme-init .
\ bme-calib bme.params 32 dump
\ bme-calc . . .
