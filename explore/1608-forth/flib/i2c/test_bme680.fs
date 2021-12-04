
\ ------------------- utils -------------------
44   variable count
44 buffer: buf1
$77 constant bme:addr

: reset ( -- ) count @ 0 do 0 buf1 i + c! loop ;
: show ( buf -- ) ." -> " count @ . count @ 0 do dup i + c@ . loop drop ;

\ ------------------- add to bme680.fs -------------------

: bme-i2c+ ( addr -- addr+1 ) *i2c@ over c! 1+ ;
: bme-rd-xfer ( addr n reg -- addr+n )
  i2c-reset
  bme.addr i2c-addr
  *i2c!
  dup i2c-xfer drop
  0 do bme-i2c+ loop
  ;

: bme-calib> ( -- )                     \ retrieve sensor calibration data
  buf1 ( addr )  BME:COEFF_ADDR1_LEN BME:COEFF_ADDR1 bme-rd
       ( addr )  BME:COEFF_ADDR2_LEN BME:COEFF_ADDR2 bme-rd

  dup ( addr )  BME:ADDR_RES_HEAT_RANGE_ADDR bme-reg@ swap c!

  dup ( addr ) 1+ BME:ADDR_RES_HEAT_VAL_ADDR bme-reg@ swap c!
      ( addr ) 2+ BME:ADDR_RANGE_SW_ERR_ADDR bme-reg@ swap c!
  ;

: bme-calib>-xfer ( -- )                     \ retrieve sensor calibration data
  buf1 			BME:COEFF_ADDR1_LEN BME:COEFF_ADDR1 bme-rd-xfer
  ( addr )  BME:COEFF_ADDR2_LEN BME:COEFF_ADDR2 bme-rd-xfer

  ( addr ) 1 BME:ADDR_RES_HEAT_RANGE_ADDR bme-rd-xfer

  ( addr ) 1 BME:ADDR_RES_HEAT_VAL_ADDR bme-rd-xfer
  ( addr ) 1 BME:ADDR_RANGE_SW_ERR_ADDR bme-rd-xfer
  drop
  ;

: bme-heater-profile! ( t T nb_profile -- )
  swap
  \ set gas heater temperature
  ( T ) bme-heater-res over BME:RES_HEAT0_ADDR ( nb_profile ) + bme-reg!
  swap
  \ set gas heater duration
  ( t ) bme-heater-duration swap BME:GAS_WAIT0_ADDR ( nb_profile ) + bme-reg!
  ;

0 variable bme.gas_wait
: bme-sensor> ( -- g h1000 p100 T100 ? )

  BME:FORCED_MODE bme-mode!
  false                                         \ return value

	\ bme-select-gas-heater-profile!
	\ $71 bme-reg@ $0F and  dup .
	\ BME:GAS_WAIT0_ADDR + bme-reg@ .

	bme.gas_wait @

  10 0 do
  $1d bme-reg@ .
    bme-data-ready? if                          \ wait for bme data
      drop                                      \ drop return value
      bme-hptg
      true leave
    else
      10 + dup . ms
      0
    then
  loop
  ;

: bme-init ( -- nak )               \ init the bme680 into continuous mode
  i2c-init
  true DMA1:I2C1-RX-CHAN dma-init
  bme-reset
  BME:SLEEP_MODE bme-mode!

  \ chip id
  \ bme.values 1 BME:CHIP_ID_ADDR bme-rd
  \ 1 i2c-xfer

  bme-calib>

  \ set oversampling and sleep mode
  2 bme-humidity-oversample!     \ 2x
  3 bme-pressure-oversample!     \ 2x
  4 bme-temperature-oversample!  \ 2x
  0 bme-temp-offset!
  2 bme-filter!                  \ 3
  true bme-gas-status!           \ enable gas
  ;


\ ------------------- testing -------------------

: bme-test-init
  led-off
  \ i2c-init bme-reset
  bme-init

	0 bme-select-gas-heater-profile!
	150 320 0 bme-heater-profile!
	150 bme.gas_wait !

	\ get gas heat wait
	BME:GAS_WAIT0_ADDR bme-reg@ .
	100 BME:GAS_WAIT0_ADDR bme-reg!
	BME:GAS_WAIT0_ADDR bme-reg@ .

  reset

  true DMA1:I2C1-RX-CHAN dma-init
  \ true DMA1:I2C1-TX-CHAN dma-init
  ;

: test-bme-reg ( -- )
  BME:ADDR_RANGE_SW_ERR_ADDR
  BME:ADDR_RES_HEAT_VAL_ADDR
  BME:ADDR_RES_HEAT_RANGE_ADDR
  3
  0 do
    dup
    i2c-reset
    bme.addr i2c-addr
    ( reg ) *i2c! 1 i2c-xfer drop *i2c@
    swap
    ( reg ) bme-reg@
dup .
    = .
  loop
  ;

: test-bme-calib> ( -- )
	reset
	bme-calib>
	cr buf1 show

	reset
	bme-calib>-xfer
	cr buf1 show
  ;

bme-test-init
test-bme-reg
test-bme-calib>
