
\ implementation details
\ https://github.com/G6EJD/BME680-Example
\ https://forums.pimoroni.com/t/bme680-observed-gas-ohms-readings/6608/23

   3 constant IAQ:RATE
 300 constant IAQ:UPDATE_COUNT
 \ 3 constant IAQ:UPDATE_COUNT
\ true constant DEBUG
false constant DEBUG

\ This sets the balance between humidity and gas reading in the
\ calculation of air_quality_score (25:75, humidity:gas)
25 constant IAQ:HUM_WEIGHTING

$b variable rf.base_addr
 0 variable iaq.idle

\ set the humidity baseline to 40%, an optimal indoor humidity
     0 variable iaq.prev_temp
     0 variable iaq.prev_gas
     0 variable iaq.gas_baseline
     0 variable iaq.baseline+
 40000 variable iaq.hum_baseline
  5000 variable iaq.gas_lower_limit         \ Bad air quality limit
 50000 variable iaq.gas_upper_limit         \ Good air quality limit

: iaq-drop-stack ( n -- )  ( n ) depth min 0 do drop loop ;

: low-power-sleep ( -- )
  rf-sleep
  DEBUG if
    IAQ:RATE 1000 * ms
  else
    IAQ:RATE 0 do stop1s ( key? if leave then ) loop
  then
  ;

: iaq-show-iaq-baseline ( i -- )
  cr ." Gas: " iaq.gas_baseline @ ( swap ( i ) . ." Ohms "
  ;
: iaq-gas-ref+ ( gas -- ) iaq.baseline+ +! ;
: iaq-gas-ref ( -- )
  iaq.baseline+ @ IAQ:UPDATE_COUNT / iaq.gas_baseline !
  0 iaq.baseline+ !
  DEBUG if iaq-show-iaq-baseline then
  ;

: iaq-hum-score ( hum100 -- hum_score100 )
  \ calculate hum_score as the distance from the hum_baseline
  ( hum ) iaq.hum_baseline @ -                    \ hum_offset

  dup 0< if
    ( hum_offset ) iaq.hum_baseline @ + iaq.hum_baseline @ 1000 / /
  else
    100000 iaq.hum_baseline @ - swap ( hum-offset ) -
    100 iaq.hum_baseline @ 1000 / - /
  then
  IAQ:HUM_WEIGHTING * 10 /
  ;
: iaq-gas-score ( gas -- gas_score100 )
  \ calculate gas_score as the distance from the gas_baseline
  iaq.gas_baseline @ over ( gas ) -                    \ gas offset

  0< if ( gas ) drop 100 else ( gas ) 100 * iaq.gas_baseline @ / then
  100 IAQ:HUM_WEIGHTING - *
  ;
: iaq-score ( gas hum1000 -- iaq )
  iaq-gas-score swap iaq-hum-score + 100 /
  ;
: iaq-score2 ( gas hum1000 -- iaq1000 )
  \ (score >= 301):                   IAQ_text += "Hazardous"
  \ (score >= 201 and score <= 300 ): IAQ_text += "Very Unhealthy"
  \ (score >= 176 and score <= 200 ): IAQ_text += "Unhealthy"
  \ (score >= 151 and score <= 175 ): IAQ_text += "Unhealthy for Sensitive Groups"
  \ (score >=  51 and score <= 150 ): IAQ_text += "Moderate"
  \ (score >=  00 and score <=  50 ): IAQ_text += "Good"
  swap iaq-hum-score swap

  ( gas ) iaq.gas_upper_limit @ min iaq.gas_lower_limit @ max       ( g h p t hs g )
  iaq.gas_lower_limit @ - 75 * iaq.gas_upper_limit @ iaq.gas_lower_limit @ - / 1000 *
  ( hum score ) ( gas score ) +
  100000 swap - 5 *
  ;

: iaq-send-update?  ( g h p t tadc iaq iaq2 -- g h p t tadc iaq iaq2 ? )
  1 iaq.idle +!                             \ increment  counter
  3 pick dup >r  7 pick dup >r              \ store T and gas in return stack

  ( gas )  iaq.prev_gas @ -  abs 1000 > swap
  ( temp ) iaq.prev_temp @ - abs 50 >
  iaq.idle @ IAQ:UPDATE_COUNT mod 0=  or or

  \ if sending update store current values and reset counter
  dup if 2r> iaq.prev_gas !  iaq.prev_temp !  0 iaq.idle !  else 2rdrop then
  DEBUG if ." ." then
  ;
: iaq-show-data ( n -- )                        \ g h p t tadc iaq iaq2
  cr ." iaq2 iaq t_adc t p h g " ( n ) 0 do i pick . loop
  ;
: iaq-send-packet ( g h p t tadc iaq iaq2 -- )
  hwid <pkt  7 0 do +pkt loop  pkt>
  rf.base_addr @ dg-send-to
  not DEBUG and if ." failed to send " then
  ;
: iaq-if-send ( g h p t tadc iaq iaq2 -- )
  \ send packet every degree temp change or UPDATE/RATE secs
  iaq-send-update? if
    DEBUG if 7 iaq-show-data then
    iaq-send-packet
  else
    7 iaq-drop-stack
  then
  ;

: highz-gpio
  IMODE-ADC PA0  io-mode!
  IMODE-ADC PA1  io-mode!
  IMODE-ADC PA2  io-mode!
  IMODE-ADC PA3  io-mode!
  \ IMODE-ADC PA4  io-mode!   \ ssel
  \ IMODE-ADC PA5  io-mode!   \ SCLK
  \ IMODE-ADC PA6  io-mode!   \ MISO
  \ IMODE-ADC PA7  io-mode!   \ MOSI
  \ IMODE-ADC PA8  io-mode!   \ LED
  DEBUG not if
    \ IMODE-ADC PA9  io-mode!   \ UART - TX1
    \ IMODE-ADC PA10 io-mode!   \ UART - RX1
  then
  IMODE-ADC PA11 io-mode!
  IMODE-ADC PA12 io-mode!
  IMODE-ADC PA13 io-mode!
  \ IMODE-ADC PA14 io-mode!     \ SDO
  IMODE-ADC PA15 io-mode!
  \ IMODE-ADC PB0  io-mode!   \ rf INT
  IMODE-ADC PB1  io-mode!
  IMODE-ADC PB3  io-mode!
  IMODE-ADC PB4  io-mode!
  IMODE-ADC PB5  io-mode!
  \ IMODE-ADC PB6  io-mode!   \ SCL
  \ IMODE-ADC PB7  io-mode!   \ SDA
  IMODE-ADC PC14 io-mode!
  IMODE-ADC PC15 io-mode!
  ;

: iaq-adc-init ( -- tadc )
  \ enable ADC to be able to set low power modes
  9 bit RCC-APB2ENR bis!  \ ADCEN

  \ low freq mode enable
  25 bit ADC-CCR bis!     \ LFMEN

  \ use adc clock for low freq/stop modes
  %11 30 lshift ADC-CFGR2 bis!   \ CKMODE

  15 bit ADC-CFGR1 bis!   \ AUTOFF

  adc-init
  ;

: iaq-sensor-init ( -- )
  2.1MHz  1000 systick-hz  lptim-init iaq-adc-init highz-gpio

  \ configure important RF69 registers
  86926 $B6 30 rf:GFSK_Rb250Fd250 RF:INIT rf-init
  RF:M_SLEEP rf.idle-mode !

  \ set PA14/SDO low to use BME:I2C_ADDR_SECONDARY
  IMODE-LOW PA14 io-mode!
  bme-init

  150 320 0 bme-heater-profile!
  0 bme-select-gas-heater-profile!

  \ offset caused by the gas heater being on 0.7 deg @ 1sec
  \ humidity is offset by 7 percent too
  -70 bme-temp-offset!
  ;

singletask
task: run-iaq

: main ( -- )
  \ run-iaq activate
  iaq-sensor-init

  begin
    iaq-gas-ref

    IAQ:UPDATE_COUNT 0 do
      bme-sensor> bme-heat-stable? and if
        3 pick ( gas ) iaq-gas-ref+

        led-off
        adc-temp                              ( stack: g h p t tadc )
        4 pick ( gas ) 4 pick ( hum1000 ) iaq-score
        5 pick ( gas ) 5 pick ( hum1000 ) iaq-score2 1000 /
        iaq-if-send                           ( stack: g h p t tadc iaq iaq2 )
        low-power-sleep
        led-on
        key? if leave then
      then
    loop
  key? until
  adc-deinit
  ;

\ ( iaq end, size: ) here dup hex. swap - .
compiletoram? not [if]  cornerstone <<<iaq>>> [then]

\ multitask lowpower& iaq tasks
\ multitask iaq tasks
