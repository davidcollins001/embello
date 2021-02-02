
\ read out the BME680 sensor
\ needs i2c

\ BME680 I2C addresses
$76 constant BME:I2C_ADDR_PRIMARY
$77 constant BME:I2C_ADDR_SECONDARY

[ifndef] bme.addr  BME:I2C_ADDR_SECONDARY constant bme.addr  [then]

\ BME680 unique chip identifier
$61 constant BME:CHIP_ID
\ chip identifier
$D0 constant BME:CHIP_ID_ADDR

\ BME680 coefficients related defines
 44 constant BME:COEFF_SIZE
 25 constant BME:COEFF_ADDR1_LEN
 16 constant BME:COEFF_ADDR2_LEN
$89 constant BME:COEFF_ADDR1
$E1 constant BME:COEFF_ADDR2

\ BME680 field_x related defines
15 constant BME:FIELD_LEN
\ 17 constant BME:FIELD_ADDR_OFFSET
$1D constant BME:FIELD0_ADDR

\ soft reset command
$B6 constant BME:SOFT_RESET_CMD
$E0 constant BME:SOFT_RESET_ADDR
10  constant BME:RESET_PERIOD

\ register map
\ other coefficient's address
$00 constant BME:ADDR_RES_HEAT_VAL_ADDR
$02 constant BME:ADDR_RES_HEAT_RANGE_ADDR
$04 constant BME:ADDR_RANGE_SW_ERR_ADDR
$5A constant BME:ADDR_SENS_CONF_START
$64 constant BME:ADDR_GAS_CONF_START

\ heater settings
$5A constant BME:RES_HEAT0_ADDR
$64 constant BME:GAS_WAIT0_ADDR

\ sensor configuration registers
$70 constant BME:CONF_HEAT_CTRL_ADDR
$71 constant BME:CONF_ODR_RUN_GAS_NBC_ADDR
$72 constant BME:CONF_OS_H_ADDR
$74 constant BME:CONF_T_P_MODE_ADDR
$75 constant BME:CONF_ODR_FILT_ADDR

\ heater control settings
$00 constant BME:ENABLE_HEATER
$08 constant BME:DISABLE_HEATER

\ gas measurement settings
$00 constant BME:DISABLE_GAS_MEAS
$01 constant BME:ENABLE_GAS_MEAS

\ over-sampling settings
\ 0     none
\ 1     1x
\ 2     2x
\ 3     4x
\ 4     8x
\ 5     16x

\ IIR filter settings
\ 0     0
\ 1     1
\ 2     3
\ 3     7
\ 4     15
\ 5     31
\ 6     63
\ 7     127

\ \ power mode settings
0 constant BME:SLEEP_MODE
1 constant BME:FORCED_MODE

\ ambient humidity shift value for compensation
4 constant BME:HUM_REG_SHIFT_VAL

\ \ mask definitions
\ $30 constant BME:GAS_MEAS_MSK
\ $0f constant BME:NBCONV_MSK
$1c constant BME:FILTER_MSK
$e0 constant BME:OST_MSK
$1c constant BME:OSP_MSK
$07 constant BME:OSH_MSK
\ $08 constant BME:HCTRL_MSK
$10 constant BME:RUN_GAS_MSK
$03 constant BME:MODE_MSK
\ $30 constant BME:RHRANGE_MSK
\ $f0 constant BME:RSERROR_MSK
$80 constant BME:NEW_DATA_MSK
$0f constant BME:GAS_INDEX_MSK
$0f constant BME:GAS_RANGE_MSK
\ $20 constant BME:GASM_VALID_MSK
\ $10 constant BME:HEAT_STAB_MSK
\ $10 constant BME:MEM_PAGE_MSK
\ $80 constant BME:SPI_RD_MSK
\ $7f constant BME:SPI_WR_MSK
\ $0f constant BME:BIT_H1_DATA_MSK

\ \ bit position definitions for sensor settings
\ 4 constant BME:GAS_MEAS_POS
\ 2 constant BME:FILTER_POS
\ 5 constant BME:OST_POS
\ 2 constant BME:OSP_POS
\ 0 constant BME:OSH_POS
\ 4 constant BME:RUN_GAS_POS
0 constant BME:MODE_POS
\ 0 constant BME:NBCONV_POS

BME:COEFF_SIZE buffer: bme.calib        \ calibration data
            15 BUFFER: bme.values       \ last reading
         1000 variable bme.tfine        \ used for p & h compensation default ~20 deg
            0 variable bme.tfine_adj    \ adjust tfine

create BME:GAS_RANGE_CONST1    \ gas resistance constants array1
  2147483647 , 2147483647 , 2147483647 , 2147483647 , 2147483647 ,
  2126008810 , 2147483647 , 2130303777 , 2147483647 , 2147483647 ,
  2143188679 , 2136746228 , 2147483647 , 2126008810 , 2147483647 ,
  2147483647 ,
align

create BME:GAS_RANGE_CONST2     \ gas resistance constants array2
  4096000000 , 2048000000 , 1024000000 , 512000000 , 255744255 ,
  127110228 , 64000000 ,  32258064 , 16016016 ,  8000000 ,
  4000000 ,   2000000 ,   1000000 ,   500000 , 250000 ,    125000 ,
align

: bme-reset ( -- ) \ software reset of the bme680
  bme.addr i2c-addr
  BME:SOFT_RESET_ADDR >i2c bme:soft_reset_cmd >i2c
  0 i2c-xfer drop
  ;

: bme-reg@ ( reg -- val )               \ get single register
  bme.addr i2c-addr
  i2c-reset
  ( reg ) >i2c 1 i2c-xfer
  drop
  i2c.buf c@
  ;

: bme-reg! ( val reg -- )              \ set single register
  bme.addr i2c-addr
  i2c-reset
  ( reg ) >i2c ( val ) >i2c 0 i2c-xfer
  drop
  ;

: bme-bits! ( pos value mask reg -- )
  dup >r bme-reg@ swap ( mask ) not and  -rot swap lshift or
  r> bme-reg!
  ;

: bme-i2c+ ( addr -- addr+1 ) i2c> over c! 1+ ;
: bme-rd ( addr n reg -- addr+n )
  bme.addr i2c-addr
  ( reg ) >i2c
  dup ( n ) i2c-xfer drop
  0 do bme-i2c+ loop
  ;

:  *>> ( n1 n2 u -- n ) >r * r> arshift ;   \ (n1 * n2) >> u
: ^2>> ( n1 u -- n ) >r dup * r> arshift ;  \ (n1 * n1) >> u
: darshift ( d n -- ) 0 do d2/ loop ;
: dlshift ( d n -- ) 0 do d2* loop ;

: bme-u20be ( off -- val )
  bme.values + dup c@ 12 lshift swap 1+
               dup c@  4 lshift swap 1+
                   c@  4 rshift  or or
  ;
: twos-comp ( u bits -- n )             \ n bit twos complement (signed)
  2dup
  1- 1 swap lshift and 0<> if           \ if val & (1 << (bits - 1)) != 0:
    1 swap lshift -                     \   return val - (1 << bits)
  else
    drop
  then
  ;

: bme-par-t1 ( -- par-t1 )                  \ par_t1 0xe9 / 0xea
  bme.calib 33 + c@
  bme.calib 34 + c@ 8 lshift or
  \ inline
  ;
: bme-par-t2 ( -- par-t2 )                  \ par_t2 0x8a / 0x8b
  bme.calib 1+ c@
  bme.calib 2+ c@ 8 lshift or
  16 twos-comp
  \ inline
  ;
: bme-par-t3 ( -- par-t3 )                  \ par_t3 0x8c
  bme.calib 3 + c@ 8 twos-comp
  \ inline
  ;

: bme-par-p1 ( -- par-p1 )                  \ par-p1 0x8e / 0x8f
  bme.calib 5 + c@
  bme.calib 6 + c@ 8 lshift or
  \ inline
  ;
: bme-par-p2 ( -- par-p2 )                  \ par-p2  0x90 / 0x91
  bme.calib 7 + c@
  bme.calib 8 + c@ 8 lshift or
  16 twos-comp
  \ inline
  ;
: bme-par-p3 ( -- par-p3 )                  \ par-p3  0x92
  bme.calib 9 + c@
  8 twos-comp
  \ inline
  ;
: bme-par-p4 ( -- par-p4 )                  \ par-p4  0x94 / 0x95
  bme.calib 11 + c@
  bme.calib 12 + c@ 8 lshift or
  16 twos-comp
  \ inline
  ;
: bme-par-p5 ( -- par-p5 )                  \ par-p5  0x96 / 0x97
  bme.calib 13 + c@
  bme.calib 14 + c@ 8 lshift or
  16 twos-comp
  \ inline
  ;
: bme-par-p6 ( -- par-p6 )                  \ par-p60x99
  bme.calib 16 + c@
  8 twos-comp
  \ inline
  ;
: bme-par-p7 ( -- par-p7 )                  \ par-p7  0x98
  bme.calib 15 + c@
  8 twos-comp
  \ inline
  ;
: bme-par-p8 ( -- par-p8 )                  \ par-p8  0x9c / 0x9d
  bme.calib 19 + c@
  bme.calib 20 + c@ 8 lshift or
  16 twos-comp
  \ inline
  ;
: bme-par-p9 ( -- par-p9 )                  \ par-p9  0x9e / 0x9f
  bme.calib 21 + c@
  bme.calib 22 + c@ 8 lshift or
  16 twos-comp
  \ inline
  ;
: bme-par-p10 ( -- par-p10 )                \ par-p10 0xa0
  bme.calib 23 + c@
  \ inline
  ;

: bme-par-h1 ( -- par-h1 )                  \ par-h1 0xe2<7:4> / 0xe3
  \ calibration[h1_msb_reg] << hum_reg_shift_val) |
  \     (calibration[h1_lsb_reg] & bit_h1_data_msk)
  bme.calib 26 + c@ $f and
  bme.calib 27 + c@ 4 lshift or
  \ $e3 bme-reg@ 4 lshift $e2 bme-reg@ $f and or
  \ inline
  ;
: bme-par-h2 ( -- par-h2 )                  \ par-h2 0xe2<7:4> / 0xe1
  bme.calib 26 + c@ 4 rshift
  bme.calib 25 + c@ 4 lshift or
  \ $e1 bme-reg@ 4 lshift $e2 bme-reg@ 4 rshift or
  ;
: bme-par-h3 ( -- par-h3 )                  \ par-h3 0xe4
  bme.calib 28 + c@
  8 twos-comp
  ;
: bme-par-h4 ( -- par-h4 )                  \ par-h4 0xe5
  bme.calib 29 + c@
  8 twos-comp
  ;
: bme-par-h5 ( -- par-h5 )                  \ par-h5 0xe6
  bme.calib 30 + c@
  8 twos-comp
  ;
: bme-par-h6 ( -- par-h6 )                  \ par-h6 0xe7
  bme.calib 31 + c@
  ;
: bme-par-h7 ( -- par-h7 )                  \ par-h7 0xe8
  bme.calib 32 + c@
  8 twos-comp
  ;

: bme-par-gh1 ( -- )
  bme.calib 37 + c@
  8 twos-comp
  ;
: bme-par-gh2 ( -- )
  bme.calib 35 + c@
  bme.calib 36 + c@ 8 lshift or
  16 twos-comp
  ;
: bme-par-gh3 ( -- )
  bme.calib 38 + c@
  8 twos-comp
  ;

: bme-heat-stable? ( -- )
  \ BME:FIELD0_ADDR bme-reg@
  \ bme.values 14 + c@ $20 and   \ gas_valid_r
  \ or
  \ bme.values 14 + c@ $10 and    \ heat_stab_r
  \ or
  \ $10 and    \ heat_stab_r
  bme.values 14 + c@ $10 and    \ heat_stab_r
  ;
: bme-res-heat-range ( -- )
  bme.calib 42 + c@
  $30 and 4 rshift
  \ inline
  ;
: bme-res-heat-val ( -- )
  bme.calib 43 + c@
  8 twos-comp
  \ inline
  ;
: bme-range-sw-err ( -- )
  bme.calib 44 + c@
  8 twos-comp
  $f0 and 4 rshift
  \ inline
  ;
: bme-data-ready? ( -- ? )
  \ bme.values 1 $1d  bme-rd  7 bit bme.values bit@
  BME:FIELD0_ADDR bme-reg@ ( status ) BME:NEW_DATA_MSK and
  ;

: bme-mode! ( mode -- )
  BME:MODE_POS swap ( mode ) BME:MODE_MSK BME:CONF_T_P_MODE_ADDR
  bme-bits!
  ;

: bme-humidity-oversample! ( value -- )
  \ BME:OSH_POS swap BME:OSH_MSK BME:CONF_OS_H_ADDR bme-bits! ;
  0 swap $07  $72 bme-bits! ;
: bme-pressure-oversample! ( value -- )
  \ BME:OSP_POS swap BME:OSP_MSK BME:CONF_T_P_MODE_ADDR bme-bits! ;
  2 swap BME:OSP_MSK BME:CONF_T_P_MODE_ADDR bme-bits! ;
: bme-temperature-oversample! ( value -- )
  \ BME:OST_POS swap BME:OST_MSK BME:CONF_T_P_MODE_ADDR bme-bits! ;
  5 swap BME:OST_MSK BME:CONF_T_P_MODE_ADDR bme-bits! ;
: bme-filter! ( value -- )
  \ BME:FILTER_POS swap BME:FILTER_MSK BME:CONF_ODR_FILT_ADDR bme-bits! ;
  2 swap BME:FILTER_MSK BME:CONF_ODR_FILT_ADDR bme-bits! ;
: bme-gas-status! ( value -- )
  4 swap ( nb ) BME:RUN_GAS_MSK $71 bme-bits!
  ;
: bme-select-gas-heater-profile! ( nb_profile -- )
  $0 swap ( nb ) $0F $71 bme-bits!
  ;
: bme-temp-offset! ( n -- )
  dup 0<> if
    \ int(copysign((((int(abs(value) * 100)) << 8) - 128) / 5, value))
    dup dup abs 8 lshift 128 - 5 / -rot abs / *
  then
  bme.tfine_adj !
  ;

: bme-calib> ( -- )                     \ retrieve sensor calibration data
  bme.calib BME:COEFF_ADDR1_LEN BME:COEFF_ADDR1 bme-rd
  ( addr )  BME:COEFF_ADDR2_LEN BME:COEFF_ADDR2 bme-rd

  ( addr ) 1 BME:ADDR_RES_HEAT_RANGE_ADDR bme-rd

  ( addr ) 1 BME:ADDR_RES_HEAT_VAL_ADDR bme-rd
  ( addr ) 1 BME:ADDR_RANGE_SW_ERR_ADDR bme-rd
  drop
  ;

: bme-amb-temp ( -- T ) 5 * 128 + 8 arshift ;

: bme-tcalc ( rawt -- t100 )
  \ var1 = ((int32_t)temp_adc >> 3) - ((int32_t)par_t1 << 1)
  \ var2 = (var1 * (int32_t)par_t2) >> 11
  \ var3 = ((((var1 >> 1) * (var1 >> 1)) >> 12) * ((int32_t)par_t3 << 4)) >> 14;
  \ t_fine = var2 + var3;
  \ temp_comp = ((t_fine * 5) + 128) >> 8;

  ( temp_adc ) 3 rshift bme-par-t1 shl -
  ( var1 ) dup bme-par-t2 11 *>>
  ( var1 ) swap 1 arshift dup 12 *>>
  ( var3 ) bme-par-t3 4 lshift 14 *>>
  ( var2 ) ( var3 ) + bme.tfine_adj @ +
  bme.tfine !
  bme.tfine @ bme-amb-temp
  ;

: bme-pcalc ( rawp -- p1 )
  \ var1 = ((int32_t)t_fine >> 1) - 64000;
  \ var2 = ((((var1 >> 2) * (var1 >> 2)) >> 11) * (int32_t)par-p6) >> 2;
  \ var2 = var2 + ((var1 * (int32_t)par-p5) << 1);
  \ var2 = (var2 >> 2) + ((int32_t)par-p4 << 16);
  \ var1 = (((((var1 >> 2) * (var1 >> 2)) >> 13) * (( int32_t)par-p3 << 5)) >> 3)
  \             + (((int32_t)par-p2 * var1) >> 1);
  \ var1 = var1 >> 18;
  \ var1 = ((32768 + var1) * (int32_t)par-p1) >> 15;
  \ press_comp = 1048576 - press_raw;
  \ press_comp = (uint32_t)((press_comp - (var2 >> 12)) * ((uint32_t)3125));
  \ if (press_comp >= (1 << 30))
  \  press_comp = ((press_comp / (uint32_t)var1) << 1);
  \ else
  \  press_comp = ((press_comp << 1) / (uint32_t)var1);
  \ var1 = ((int32_t)par-p9 * (int32_t)(((press_comp >> 3) * (press_comp >> 3)) >> 13)) >> 12;
  \ var2 = ((int32_t)(press_comp >> 2) * (int32_t)par-p8) >> 13;
  \ var3 = ((int32_t)(press_comp >> 8) * (int32_t)(press_comp >> 8) * (int32_t)(press_comp >> 8)
  \             * (int32_t)par-p10) >> 17;
  \ press_comp = (int32_t)(press_comp) +  ((var1 + var2 + var3 + ((int32_t)par-p7 << 7)) >> 4);

  bme.tfine @ shr 64000 -
  ( var1 ) dup 2 arshift 11 ^2>> bme-par-p6 * 2 arshift
  ( var1 ) over  bme-par-p5 * shl +
  ( var2 ) 2 arshift bme-par-p4 16 lshift +  \ swap
  ( var1 ) over 2 arshift 13 ^2>> bme-par-p3 5 lshift 3 *>>
  rot bme-par-p2 1 *>> + 18 arshift
  ( var1 ) 32768 + bme-par-p1 15 *>>
  rot ( press_raw ) 1048576  swap -
  ( press_comp ) rot 12 arshift - 3125 *
  dup 1 30 lshift >= if swap / shl else shl swap / then
  bme-par-p9 over 3 arshift 13 ^2>> 12 *>>
  over 2 arshift bme-par-p8 13 *>>
  rot dup 8 arshift dup dup * * bme-par-p10 17 *>>
  rot + rot + bme-par-p7 7 lshift + 4 arshift +
  ;

: bme-hcalc ( rawh -- h100 )
  \ temp_scaled = (int32_t)temp_comp;
  \ var1 = (int32_t)hum_adc - (int32_t)((int32_t)par_h1 << 4)
  \           - (((temp_scaled * (int32_t)par_h3) / ((int32_t)100)) >> 1);
  \ var2 = ((int32_t)par_h2
  \         * (((temp_scaled * (int32_t)par_h4) / ((int32_t)100))
  \       + (((temp_scaled * ((temp_scaled * (int32_t)par_h5) /((int32_t)100))) >> 6)
  \             / ((int32_t)100))
  \         + ((int32_t)(1 << 14))))
  \         >> 10;
  \ var3 = var1 * var2;
  \ var4 = (((int32_t)par_h6 << 7) + ((temp_scaled * (int32_t)par_h7) / ((int32_t)100))) >> 4;
  \ var5 = ((var3 >> 14) * (var3 >> 14)) >> 10;
  \ var6 = (var4 * var5) >> 1;
  \ hum_comp = (var3 + var6) >> 12;
  \ hum_comp = (((var3 + var6) >> 10) * ((int32_t) 1000)) >> 12;

  \ bme.tfine @ 5 * 128 + 8 arshift >r
  bme.tfine @ bme-amb-temp >r
  bme-par-h1 4 lshift -
  r@ ( temp_scaled ) bme-par-h3 * 100 / 1 arshift -
  r@ ( temp_scaled ) bme-par-h4 * 100 /
  r@ ( temp_scaled ) dup ( temp_scaled ) bme-par-h5 * 100 / 6 *>>
  100 /  16384 + + bme-par-h2 10 *>>
  ( var1 ) ( var2 ) *                                       \ <- var3
  ( var3 ) dup 14 rshift dup 10 *>>                         \ <- var5
  bme-par-h6 7 lshift r> bme-par-h7 * 100 / + 4 arshift \ <- var4
  ( var4 ) ( var5 ) * 1 arshift                             \ <- var6
  ( var3 ) ( var6 ) + 10 arshift 1000 12 *>>                \ hum_comp
  0 max 100000 min
  ;

: bme-gres ( rawg grange -- r )
  >r
  1340 bme-range-sw-err 5 * +
  BME:GAS_RANGE_CONST1 r@ ( grange ) cells + @
  m* 16 darshift 	     	                                    \ var1
  ( var1 ) 2>r
  ( rawg ) s>d 15 dlshift 16777216 s>d d- ( var1 ) 2r@ d+     	\ var2
  2r> r>
  BME:GAS_RANGE_CONST2 ( grange ) swap cells + @ s>d
  ( var1 ) ud* 9 darshift                               		\ var3
  ( var3 ) ( var2 ) 2over 1 darshift d+ ( var2 ) 2swap d/    	\ gas_res
  drop  								\ convert double to single length
  ;

: bme-heater-duration ( t -- )
  dup ( t ) $fc0 < if
    0 swap
    begin dup $3f > while 4 / swap 1+ swap repeat
    swap 64 * +
  else
    $ff
  then
  ;

: bme-heater-res ( T -- ) 			\ calculate data to write to res_heat_x reg
  \ var1 = (((int32_t)amb_temp * par_g3) / 10) << 8;
  \ var2 = (par_g1 + 784) *
  \ 		(((((par_g2 + 154009) * target_temp * 5) / 100) + 3276800) / 10);
  \ var3 = var1 + (var2 >> 1);
  \ var4 = (var3 / (res_heat_range + 4));
  \ var5 = (131 * res_heat_val) + 65536;
  \ res_heat_x100 = (int32_t)(((var4 / var5) - 250) * 34);
  \ res_heat_x = (uint8_t)((res_heat_x100 + 50) / 100);

  200 max 400 min

  bme.tfine @ bme-amb-temp bme-par-gh3 * 10 / 8 lshift
  swap
  bme-par-gh2 154009 + ( temp ) * 5 * 100 / 3276800 + 10 /
  bme-par-gh1 784 + *
  ( var1 ) ( var2 ) 1 arshift +
  ( var3 ) bme-res-heat-range  4 + /
  131 bme-res-heat-val * 65536 +
  ( var4 ) ( var5 ) / 250 - 34 *
  ( heat_res100 ) 50 + 100 /
  ;

: bme-heater-profile! ( t T nb_profile -- )
  swap
  \ set gas heater temperature
  ( T ) bme-heater-res over BME:RES_HEAT0_ADDR ( nb_profile ) + bme-reg!
  swap
  \ set gas heater duration
  ( t ) bme-heater-duration swap BME:GAS_WAIT0_ADDR ( nb_profile ) + bme-reg!
  ;

: bme-hptg ( -- g h1000 p100 T100 )
  bme.values BME:FIELD_LEN BME:FIELD0_ADDR bme-rd        	 \ read sensor data
  drop

  \ gas resistance meanings
  \ 521177 - 431331 - good?
  \ 297625 - 213212 - average ?
  \ 148977 - 108042 - little bad ?
  \ 75010 - 54586 - bad ?
  \ 37395 - 27080 - worse ?
  \ 18761 - 13591 - very bad ?
  \ 9008 - 8371 - can't see the exit ?

  \ TODO move these into their own word
  bme.values 13 + c@ 2 lshift
  bme.values 14 + c@ 6 rshift or                            \ gas_adc

  bme.values 14 + c@ BME:GAS_RANGE_MSK and                  \ gas_range_r
  bme-gres

  bme.values 8 + dup c@ 8 lshift swap 1+ c@ or              \ humidity
  bme-hcalc
  2 bme-u20be                                               \ pressure
  bme-pcalc
  5 bme-u20be                                               \ temperature
  bme-tcalc
  ;

: bme-sensor> ( -- g h1000 p100 T100 ? )
  BME:FORCED_MODE bme-mode!
  false                                         \ return value

  10 0 do
    BME:FIELD0_ADDR bme-reg@
    ( status ) BME:NEW_DATA_MSK and 0= if
      10 ms
    else
      drop                                      \ drop return value
      \ bme.values BME:FIELD_LEN BME:FIELD0_ADDR bme-rd
      bme-hptg

      \ self.data.status = regs[0] & constants.NEW_DATA_MSK
      \ # Contains the nb_profile used to obtain the current measurement
      \ self.data.gas_index = regs[0] & constants.GAS_INDEX_MSK
      \ self.data.meas_index = regs[1]

      true leave
    then
  loop
  ;

: bme-init ( -- nak )               \ init the bme680 into continuous mode
  i2c-init
  true DMA1:I2C-RX-CHAN dma-i2c-init
  true DMA1:I2C-TX-CHAN dma-i2c-init
  bme-reset
  BME:SLEEP_MODE bme-mode!

  \ chip id
  \ bme.values 1 $D0 bme-rd
  \ 1 i2c-xfer

  \ TODO why is this necessary for bme-calib> to work first time?
  bme.calib BME:COEFF_ADDR1_LEN BME:COEFF_ADDR1 bme-rd drop

  bme-calib>

  \ set oversampling and sleep mode
  2 bme-humidity-oversample!     \ 2x
  3 bme-pressure-oversample!     \ 2x
  4 bme-temperature-oversample!  \ 2x
  \ TODO fix temp offset
  0 bme-temp-offset!
  2 bme-filter!                  \ 3
  true bme-gas-status!           \ enable gas
  ;

\ bme-init .
\ bme-calib bme.calib  32 dump
\ bme-calc . . .

compiletoram? not [if]  cornerstone <<<bme680>>> compiletoram [then]
