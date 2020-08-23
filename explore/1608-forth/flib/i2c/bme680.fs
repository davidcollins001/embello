\ read out the BME680 sensor
\ needs i2c

\ BME680 I2C addresses
$76 constant BME:I2C_ADDR_PRIMARY
$77 constant BME:I2C_ADDR_SECONDARY

[ifndef] BME.ADDR  BME:I2C_ADDR_SECONDARY constant BME.ADDR  [then]

\ BME680 unique chip identifier
$61 constant BME:CHIP_ID
\ Chip identifier
$D0 constant BME:CHIP_ID_ADDR

\ BME680 coefficients related defines
41 constant BME:COEFF_SIZE
25 constant BME:COEFF_ADDR1_LEN
16 constant BME:COEFF_ADDR2_LEN

\ BME680 field_x related defines
15 constant BME:FIELD_LENGTH
17 constant BME:FIELD_ADDR_OFFSET

\ Soft reset command
$B6 constant BME:SOFT_RESET_CMD
$E0 constant BME:SOFT_RESET_ADDR
10  constant BME:RESET_PERIOD

\ Register map
\ Other coefficient's address
$00 constant BME:ADDR_RES_HEAT_VAL_ADDR
$02 constant BME:ADDR_RES_HEAT_RANGE_ADDR
$04 constant BME:ADDR_RANGE_SW_ERR_ADDR
$5A constant BME:ADDR_SENS_CONF_START
$64 constant BME:ADDR_GAS_CONF_START

\ Field settings
$1D constant BME:FIELD0_ADDR

\ Heater settings
$5A constant BME:RES_HEAT0_ADDR
$64 constant BME:GAS_WAIT0_ADDR

\ Sensor configuration registers
$70 constant BME:CONF_HEAT_CTRL_ADDR
$71 constant BME:CONF_ODR_RUN_GAS_NBC_ADDR
$72 constant BME:CONF_OS_H_ADDR
$74 constant BME:CONF_T_P_MODE_ADDR
$75 constant BME:CONF_ODR_FILT_ADDR

\ Coefficient's address
$89 constant BME:COEFF_ADDR1
$e1 constant BME:COEFF_ADDR2

\ Heater control settings
$00 constant BME:ENABLE_HEATER
$08 constant BME:DISABLE_HEATER

\ Gas measurement settings
$00 constant BME:DISABLE_GAS_MEAS
$01 constant BME:ENABLE_GAS_MEAS

\ Over-sampling settings
\ 0     NONE
\ 1     1X
\ 2     2X
\ 3     4X
\ 4     8X
\ 5     16X

\ IIR filter settings
\ 0     0
\ 1     1
\ 2     3
\ 3     7
\ 4     15
\ 5     31
\ 6     63
\ 7     127

\ \ Power mode settings
0 constant BME:SLEEP_MODE
1 constant BME:FORCED_MODE

\ Ambient humidity shift value for compensation
4 constant BME:HUM_REG_SHIFT_VAL

\ \ Settings selector
\ 1 constant BME:OST_SEL
\ 2 constant BME:OSP_SEL
\ 4 constant BME:OSH_SEL
\ 8 constant BME:GAS_MEAS_SEL
\ 16 constant BME:FILTER_SEL
\ 32 constant BME:HCNTRL_SEL
\ 64 constant BME:RUN_GAS_SEL
\ 128 constant BME:NBCONV_SEL
\ \ BME:GAS_MEAS_SEL BME:RUN_GAS_SEL or BME:NBCONV_SEL or variable BME:GAS_SENSOR_SEL !

\ \ Mask definitions
\ $30 constant BME:GAS_MEAS_MSK
\ $0F constant BME:NBCONV_MSK
$1C constant BME:FILTER_MSK
$E0 constant BME:OST_MSK
$1C constant BME:OSP_MSK
$07 constant BME:OSH_MSK
\ $08 constant BME:HCTRL_MSK
$10 constant BME:RUN_GAS_MSK
$03 constant BME:MODE_MSK
\ $30 constant BME:RHRANGE_MSK
\ $F0 constant BME:RSERROR_MSK
$80 constant BME:NEW_DATA_MSK
$0F constant BME:GAS_INDEX_MSK
$0F constant BME:GAS_RANGE_MSK
\ $20 constant BME:GASM_VALID_MSK
\ $10 constant BME:HEAT_STAB_MSK
\ $10 constant BME:MEM_PAGE_MSK
\ $80 constant BME:SPI_RD_MSK
\ $7F constant BME:SPI_WR_MSK
\ $0F constant BME:BIT_H1_DATA_MSK

\ \ Bit position definitions for sensor settings
\ 4 constant BME:GAS_MEAS_POS
\ 2 constant BME:FILTER_POS
\ 5 constant BME:OST_POS
\ 2 constant BME:OSP_POS
\ 0 constant BME:OSH_POS
\ 4 constant BME:RUN_GAS_POS
0 constant BME:MODE_POS
\ 0 constant BME:NBCONV_POS

\ \ TODO replace with bme-par-t1 etc
\ \ Array Index to Field data mapping for Calibration Data
 \ 1 constant BME:T2_LSB_REG
 \ 2 constant BME:T2_MSB_REG
 \ 3 constant BME:T3_REG
 \ 5 constant BME:P1_LSB_REG
 \ 6 constant BME:P1_MSB_REG
 \ 7 constant BME:P2_LSB_REG
 \ 8 constant BME:P2_MSB_REG
 \ 9 constant BME:P3_REG
\ 11 constant BME:P4_LSB_REG
\ 12 constant BME:P4_MSB_REG
\ 13 constant BME:P5_LSB_REG
\ 14 constant BME:P5_MSB_REG
\ 15 constant BME:P7_REG
\ 16 constant BME:P6_REG
\ 19 constant BME:P8_LSB_REG
\ 20 constant BME:P8_MSB_REG
\ 21 constant BME:P9_LSB_REG
\ 22 constant BME:P9_MSB_REG
\ 23 constant BME:P10_REG
\ 25 constant BME:H2_MSB_REG
\ 26 constant BME:H2_LSB_REG
\ 26 constant BME:H1_LSB_REG
\ 27 constant BME:H1_MSB_REG
\ 28 constant BME:H3_REG
\ 29 constant BME:H4_REG
\ 30 constant BME:H5_REG
\ 31 constant BME:H6_REG
\ 32 constant BME:H7_REG
\ 33 constant BME:T1_LSB_REG
\ 34 constant BME:T1_MSB_REG
\ 35 constant BME:GH2_LSB_REG
\ 36 constant BME:GH2_MSB_REG
\ 37 constant BME:GH1_REG
\ 38 constant BME:GH3_REG

\ \ BME680 register buffer index settings
\ 5 constant BME:REG_FILTER_INDEX
\ 4 constant BME:REG_TEMP_INDEX
\ 4 constant BME:REG_PRES_INDEX
\ 2 constant BME:REG_HUM_INDEX
\ 1 constant BME:REG_NBCONV_INDEX
\ 1 constant BME:REG_RUN_GAS_INDEX
\ 0 constant BME:REG_HCTRL_INDEX

\ $72 constant BME:CTRL_HUM
\ $73 constant BME:STATUS
\ $74 constant BME:CTRL_MEAS
\ $75 constant BME:CONFIG
\ $E0 constant BME:RESET

BME:COEFF_SIZE buffer: bme.calib        \ calibration data
            15 buffer: bme.values       \ last reading
         1000 variable bme.tfine        \ used for p & h compensation default ~20 deg
            0 variable bme.tfine_adj    \ adjust tfine

create bme:gas_range_const1    \ gas resistance constants array1
  2147483647 , 2147483647 , 2147483647 , 2147483647 , 2147483647 ,
  2126008810 , 2147483647 , 2130303777 , 2147483647 , 2147483647 ,
  2143188679 , 2136746228 , 2147483647 , 2126008810 , 2147483647 ,
  2147483647 ,
align

create bme:gas_range_const2     \ gas resistance constants array2
  4096000000 , 2048000000 , 1024000000 , 512000000 , 255744255 ,
  127110228 , 64000000 ,  32258064 , 16016016 ,  8000000 ,
  4000000 ,   2000000 ,   1000000 ,   500000 , 250000 ,    125000 ,
align

: bme-reset ( -- ) \ software reset of the bme680
  BME.ADDR i2c-addr
  BME:SOFT_RESET_ADDR >i2c BME:SOFT_RESET_CMD >i2c
  0 i2c-xfer drop
  ;

: bme-reg@ ( reg -- val )               \ get single register
  BME.ADDR i2c-addr
  ( reg ) >i2c 1 i2c-xfer
  drop
  i2c.ptr @ c@
  ;

: bme-regs! ( val reg -- )              \ set single register
  BME.ADDR i2c-addr
  ( reg ) >i2c ( val ) >i2c 0 i2c-xfer
  drop
  ;

: bme-set-bits ( pos value mask reg -- )
  dup >r bme-reg@ swap ( mask ) not and  -rot swap lshift or
  r> bme-regs!
  ;

: bme-mode! ( mode -- )
  BME:MODE_POS swap ( mode ) BME:MODE_MSK BME:CONF_T_P_MODE_ADDR
  bme-set-bits
  ;

: bme-i2c+ ( addr -- addr+1 ) i2c> over c! 1+ ;
: bme-rd ( addr n reg -- addr+n )
  >i2c
  dup i2c-xfer drop
  0 do bme-i2c+ loop
  i2c-reset
  ;

: bme-u8 ( off -- val ) bme.calib  + c@ ;
: bme-s8 ( off -- val ) bme-u8 8 lshift 8 arshift ;
: bme-u12 ( off -- val ) dup 4 bme-u8 rshift swap 1+ bme-u8 4 lshift or ;
: bme-s12 ( off -- val ) bme-u12 12 lshift 12 arshift ;
: bme-u16 ( off -- val ) dup bme-u8 swap 1+ bme-u8 8 lshift or ;
: bme-s16 ( off -- val ) bme-u16 16 lshift 16 arshift ;
: bme-u20be ( off -- val )
  bme.values + dup c@ 12 lshift swap 1+
               dup c@  4 lshift swap 1+
                   c@  4 rshift  or or ;
: bme-res-heat-val ( -- val ) 28 bme-s8 4 rshift %11 and ;

: set-humidity-oversample ( value -- )
  \ BME:OSH_POS swap BME:OSH_MSK BME:CONF_OS_H_ADDR bme-set-bits ;
  0 swap $07  $72 bme-set-bits ;
: set-pressure-oversample ( value -- )
  \ BME:OSP_POS swap BME:OSP_MSK BME:CONF_T_P_MODE_ADDR bme-set-bits ;
  2 swap BME:OSP_MSK BME:CONF_T_P_MODE_ADDR bme-set-bits ;
: set-temperature-oversample ( value -- )
  \ BME:OST_POS swap BME:OST_MSK BME:CONF_T_P_MODE_ADDR bme-set-bits ;
  5 swap BME:OST_MSK BME:CONF_T_P_MODE_ADDR bme-set-bits ;
: set-filter ( value -- )
  \ BME:FILTER_POS swap BME:FILTER_MSK BME:CONF_ODR_FILT_ADDR bme-set-bits ;
  2 swap BME:FILTER_MSK BME:CONF_ODR_FILT_ADDR bme-set-bits ;
: set-gas-status ( value -- )
  \ BME:RUN_GAS_POS swap BME:RUN_GAS_MSK BME:CONF_ODR_RUN_GAS_NBC_ADDR bme-set-bits ;
  4 swap BME:RUN_GAS_MSK BME:CONF_ODR_RUN_GAS_NBC_ADDR bme-set-bits ;
: set-temp-offset ( n -- )
  dup 0<> if
    \ int(copysign((((int(abs(value) * 100)) << 8) - 128) / 5, value))
    dup dup abs 100 * 8 lshift 128 - 5 * -rot abs / *
  then
  bme.tfine_adj !
  ;

: bme-calib> ( -- )                     \ retrieve sensor calibration data
  \ calibration = self._get_regs(constants.COEFF_ADDR1, constants.COEFF_ADDR1_LEN)
  \ calibration += self._get_regs(constants.COEFF_ADDR2, constants.COEFF_ADDR2_LEN)
  bme.calib BME:COEFF_ADDR1_LEN BME:COEFF_ADDR1 bme-rd
  ( addr )  BME:COEFF_ADDR2_LEN BME:COEFF_ADDR2 bme-rd

  \ heat_range = self._get_regs(constants.ADDR_RES_HEAT_RANGE_ADDR, 1)
  ( addr ) 1 BME:ADDR_RES_HEAT_RANGE_ADDR bme-rd

  \ heat_value = constants.twos_comp(self._get_regs(constants.ADDR_RES_HEAT_VAL_ADDR, 1)
  \ sw_error = constants.twos_comp(self._get_regs(constants.ADDR_RANGE_SW_ERR_ADDR, 1)
  ( addr ) 1 BME:ADDR_RES_HEAT_VAL_ADDR bme-rd
  ( addr ) 1 BME:ADDR_RANGE_SW_ERR_ADDR bme-rd

  \ self.calibration_data.set_from_array(calibration)
  \ self.calibration_data.set_other(heat_range, heat_value, sw_error)
  drop
  ;

: twos-comp ( u -- n )                  \ twos complement (signed) for 8 bit number
    \ if val & (1 << (bits - 1)) != 0:
        \ val = val - (1 << bits)
        \ return val
  dup
  1 7 ( 8 bits -1 ) lshift and 0<> if
    ( val ) 1 8 lshift -
  then
  ;

: bme-hptg ( -- gres gr rawh rawp rawt )
  bme.values BME:FIELD_LENGTH BME:FIELD0_ADDR bme-rd        \ read sensor data
  drop

  \ adc_pres = (regs[2] << 12) | (regs[3] << 4) | (regs[4] >> 4)
  \ adc_temp = (regs[5] << 12) | (regs[6] << 4) | (regs[7] >> 4)
  \ adc_hum = (regs[8] << 8) | regs[9]
  \ adc_gas_res = (regs[13] << 2) | (regs[14] >> 6)
  \ gas_range = regs[14] & constants.GAS_RANGE_MSK

  bme.values 9 + c@ 2 lshift                    \ gas_r <9:2>
  bme.values 10 + c@ 6 rshift                   \ gas_r <1:0>
  or

  bme.values 10 + c@ BME:GAS_RANGE_MSK and      \ gas_range_r

  bme.values 7 + dup c@ 8 lshift swap 1+ c@ or  \ humidity
  1 bme-u20be                                   \ pressure
  4 bme-u20be                                   \ temperature
  ;

: bme-sensor> ( -- )
  \ for attempt in range(10):
  \     status = self._get_regs(constants.FIELD0_ADDR, 1)

  \     if (status & constants.NEW_DATA_MSK) == 0:
  \   	  time.sleep(constants.POLL_PERIOD_MS / 1000.0)
  \   	  continue

  \     regs = self._get_regs(constants.FIELD0_ADDR, constants.FIELD_LENGTH)

  \     self.data.status = regs[0] & constants.NEW_DATA_MSK
  \     # Contains the nb_profile used to obtain the current measurement
  \     self.data.gas_index = regs[0] & constants.GAS_INDEX_MSK
  \     self.data.meas_index = regs[1]

  \     adc_pres = (regs[2] << 12) | (regs[3] << 4) | (regs[4] >> 4)
  \     adc_temp = (regs[5] << 12) | (regs[6] << 4) | (regs[7] >> 4)
  \     adc_hum = (regs[8] << 8) | regs[9]
  \     adc_gas_res = (regs[13] << 2) | (regs[14] >> 6)
  \     gas_range = regs[14] & constants.GAS_RANGE_MSK

  \     self.data.status |= regs[14] & constants.GASM_VALID_MSK
  \     self.data.status |= regs[14] & constants.HEAT_STAB_MSK

  \     self.data.heat_stable = (self.data.status & constants.HEAT_STAB_MSK) > 0

  \     temperature = self._calc_temperature(adc_temp)
  \     self.data.temperature = temperature / 100.0
  \     self.ambient_temperature = temperature  # Saved for heater calc

  \     self.data.pressure = self._calc_pressure(adc_pres) / 100.0
  \     self.data.humidity = self._calc_humidity(adc_hum) / 1000.0
  \     self.data.gas_resistance = self._calc_gas_resistance(adc_gas_res, gas_range)
  \     return True

  \ return False

  BME:FORCED_MODE bme-mode!

  10 0 do
    BME:FIELD0_ADDR bme-reg@
    ( status ) BME:NEW_DATA_MSK 0= if
      10 millis
    else
      \ bme.values BME:FIELD_LENGTH BME:FIELD0_ADDR bme-rd
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
  i2c-init bme-reset
  BME:SLEEP_MODE bme-mode!

  \ chip id
  \ bme.values 1 $D0 bme-rd
  \ 1 i2c-xfer

  bme-calib>

  \ set oversampling and sleep mode
  2 set-humidity-oversample		\ 2x
  3 set-pressure-oversample		\ 4x
  4 set-temperature-oversample  \ 8x
  2 set-filter                  \ 3
  1 set-gas-status              \ enable gas
  0 set-temp-offset

  \ TODO move this out of init?
  bme-sensor>
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

\ : bme-pcalc ( rawp -- p1 )
  \ \ var1 = ((int32_t)t_fine >> 1) - 64000;
  \ \ var2 = ((((var1 >> 2) * (var1 >> 2)) >> 11) * (int32_t)par_p6) >> 2;
  \ \ var2 = var2 + ((var1 * (int32_t)par_p5) << 1);
  \ \ var2 = (var2 >> 2) + ((int32_t)par_p4 << 16);
  \ \ var1 = (((((var1 >> 2) * (var1 >> 2)) >> 13) * (( int32_t)par_p3 << 5)) >> 3)
  \ \             + (((int32_t)par_p2 * var1) >> 1);
  \ \ var1 = var1 >> 18;
  \ \ var1 = ((32768 + var1) * (int32_t)par_p1) >> 15;
  \ \ press_comp = 1048576 - press_raw;
  \ \ press_comp = (uint32_t)((press_comp - (var2 >> 12)) * ((uint32_t)3125));
  \ \ if (press_comp >= (1 << 30))
  \ \  press_comp = ((press_comp / (uint32_t)var1) << 1);
  \ \ else
  \ \  press_comp = ((press_comp << 1) / (uint32_t)var1);
  \ \ var1 = ((int32_t)par_p9 * (int32_t)(((press_comp >> 3) * (press_comp >> 3)) >> 13)) >> 12;
  \ \ var2 = ((int32_t)(press_comp >> 2) * (int32_t)par_p8) >> 13;
  \ \ var3 = ((int32_t)(press_comp >> 8) * (int32_t)(press_comp >> 8) * (int32_t)(press_comp >> 8)
  \ \             * (int32_t)par_p10) >> 17;
  \ \ press_comp = (int32_t)(press_comp) +  ((var1 + var2 + var3 + ((int32_t)par_p7 << 7)) >> 4);

  \ \ + 5

  \ bme.tfine @ shr 64000 -
  \ ( var1 ) dup 2 arshift 11 ^2>>  ( par_p6 ) 15 bme-u8 2 arshift *
  \ ( var1 ) over  ( par_p5 ) 12 bme-s16 shl * +
  \ ( var2 ) 2 arshift ( par_p4 ) 10 bme-s16 16 lshift +  \ swap
  \ ( var1 ) over 2 arshift 13 ^2>> ( par_p3 ) 9 bme-u8 5 lshift 3 *>>
  \ rot ( par_p2 ) 7 bme-s16 1 *>> + 18 arshift
  \ ( var1 ) 32768 + ( par_p1 ) 5 bme-u16 15 *>>
  \ rot ( press_raw ) 1048576  swap -
  \ ( press_comp ) rot 12 arshift - 3125 *
  \ dup 1 30 lshift >= if swap / shl else shl swap / then
  \ ( par_p9 ) 18 bme-s16 over 3 arshift 13 ^2>> 12 *>>
  \ over 2 arshift ( par_p8 ) 16 bme-s16 13 *>>
  \ rot dup 8 arshift dup dup * * ( par_p10 ) 20 bme-u8 17 *>>
  \ rot + rot + ( par_p7 ) 14 bme-u8 7 lshift + 4 arshift +
  \ ;

\ : bme-hcalc ( rawh -- h100 )
  \ \ temp_scaled = (int32_t)temp_comp;
  \ \ var1 = (int32_t)hum_adc - (int32_t)((int32_t)par_h1 << 4)
  \ \           - (((temp_scaled * (int32_t)par_h3) / ((int32_t)100)) >> 1);
  \ \ var2 = ((int32_t)par_h2
  \ \         * (((temp_scaled * (int32_t)par_h4) / ((int32_t)100))
  \ \       + (((temp_scaled * ((temp_scaled * (int32_t)par_h5) /((int32_t)100))) >> 6)
  \ \             / ((int32_t)100))
  \ \         + ((int32_t)(1 << 14))))
  \ \         >> 10;
  \ \ var3 = var1 * var2;
  \ \ var4 = (((int32_t)par_h6 << 7) + ((temp_scaled * (int32_t)par_h7) / ((int32_t)100))) >> 4;
  \ \ var5 = ((var3 >> 14) * (var3 >> 14)) >> 10;
  \ \ var6 = (var4 * var5) >> 1;
  \ \ hum_comp = (var3 + var6) >> 12;
  \ \ hum_comp = (((var3 + var6) >> 10) * ((int32_t) 1000)) >> 12;

  \ bme.tfine @ 5 * 128 + 8 arshift
  \ swap 4 ( par_h1 ) 21 bme-s12 lshift -
  \ over ( par_h3 ) 25 bme-s16 * 100 / 1 swap arshift
  \ over ( par_h2 ) 23 bme-s12
  \ ( temp_scaled ) over ( par_h4 ) 26 bme-s16 * 100 /
  \ ( temp_scaled ) over over ( par_h5 ) 27 bme-s16 100 / 6 *>> 100 / +
  \ 1 14 lshift + 10 *>> *
  \ ( temp_scaled ) rot ( par_h7 ) 29 bme-s16 * 100 /
  \ 7 ( par_h6 ) 28 bme-s16 lshift + 4 swap arshift
  \ over 14 swap 10 *>> 1 *>>
  \ ( v3+v6 )
  \ + dup 12 swap arshift                 \ hum_comp
  \ \ swap 10 swap arshift 1000 12 *>>      \ hum_comp
  \ ( hum_comp1 hum_comp2 )
  \ ;

\ : gcalc ( rawg -- g1 )
  \ \ int64_t var1 = (int64_t)(((1340 + (5 * (int64_t)range_switching_error))
  \ \     * (( int64_t)const_array1_int[gas_range])) >> 16);
  \ \ int64_t var2 = (int64_t)(gas_adc << 15) - (int64_t)(1 << 24) + var1;
  \ \ int32_t gas_res = (int32_t)((((int64_t)(const_array2_int[gas_range]
  \ \     * (int64_t)var1) >> 9) + (var2 >> 1)) / var2);

  \ 1340 5 24 bme-s8 * + bme:gas_range_const1 16 *>>
  \ dup 28 bme-s12 15 lshift 1 24 lshift - ( var1 ) +
  \ ( var1 ) 9 arshift ( var2 ) dup 1 arshift bme:gas_range_const2 /
  \ ;

\ : bme-calc ( -- h p t )  \ convert reading to calibrated bme.values
  \ bme-hptg bme-tcalc -rot bme-pcalc -rot bme-hcalc -rot
  \ \ gcalc
  \ ;
  \ 3 bme-u20be bme-tcalc
  \ 0 bme-u20be bme-pcalc

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

: bme-data-ready ( -- ? )
  bme.values 1 $1D  bme-rd  7 bit bme.values bit@
  ;

\ bme-init .
\ bme-calib bme.calib  32 dump
\ bme-calc . . .
