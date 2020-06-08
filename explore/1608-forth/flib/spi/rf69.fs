\ rf69 driver

\ --------------------------------------------------
\  Configuration
\ --------------------------------------------------

       $00 constant RF:FIFO
       $01 constant RF:OP
       $07 constant RF:FRF
       $11 constant RF:PA_LEVEL
       $18 constant RF:LNA
       $1F constant RF:AFC
       $24 constant RF:RSSI
       $27 constant RF:IRQ1
       $28 constant RF:IRQ2
       $2F constant RF:SYN1
       $31 constant RF:SYN3
       $37 constant RF:CONF
       $38 constant RF:PAYLOAD_LEN
       $39 constant RF:ADDR
       $3A constant RF:BCAST
       $3C constant RF:THRESH
       $3D constant RF:PCONF2
       $3E constant RF:AES

0 2 lshift constant RF:M_SLEEP
1 2 lshift constant RF:M_STDBY
2 2 lshift constant RF:M_FS
3 2 lshift constant RF:M_TX
4 2 lshift constant RF:M_RX

       $C2 constant RF:START_TX
       $42 constant RF:STOP_TX
       $80 constant RF:RCCALSTART

     7 bit constant RF:IRQ1_MRDY
     6 bit constant RF:IRQ1_RXRDY
     5 bit constant RF:IRQ1_TXRDY
     4 bit constant RF:IRQ1_PLLLOCK
     3 bit constant RF:IRQ1_RSSI
     2 bit constant RF:IRQ1_TIMEOUT
     1 bit constant RF:IRQ1_AUTO
     0 bit constant RF:IRQ1_SYNC

     7 bit constant RF:IRQ2_FIFO_FULL
     6 bit constant RF:IRQ2_FIFO_NE
     5 bit constant RF:IRQ2_FIFO_LEVEL
     4 bit constant RF:IRQ2_FIFO_OVERRUN
     3 bit constant RF:IRQ2_SENT
     2 bit constant RF:IRQ2_RECVD
     1 bit constant RF:IRQ2_CRCOK

         2 constant RF:HDR_LEN
        66 constant RF:MAXDATA

\ TODO use idle mode instead of RF:M_STDBY
         0 variable rf.mode       \ last set chip mode
RF:M_STDBY variable rf.idle-mode  \ default idle mode
     false variable rf.last       \ flag used to fetch RSSI only once per packet
         0 variable rf.rssi       \ RSSI signal strength of last reception
         0 variable rf.lna        \ Low Noise Amplifier setting (set by AGC)
         0 variable rf.power      \ power setting
         0 variable rf.afc        \ Auto Frequency Control offset
     false variable rf.recvd      \ flag to show packet was received
      true variable rf.can-send   \ flag to show packet can be sent
         0 variable rf.fixed-pkt# \ flag to show fixed packet length
        66 buffer:  rf.buf        \ buffer with last received packet data

8683 variable rf.freq    \ frequency (auto-scaled to 100..999 MHz)
  42 variable rf.group   \ network group (1..250)
  61 variable rf.nodeid  \ node ID of this node (1..63)

create rf:init  \ initialise the radio, each 16-bit word is <reg#,val>
hex
  0200 h, \ packet mode, fsk
  0302 h, 048A h, \ bit rate 49,261 hz
  0505 h, 06C3 h, \ 90.3kHzFdev -> modulation index = 2
  0B20 h, \ low M
  1942 h, 1A42 h, \ RxBw 125khz, AFCBw 125khz
  1E0C h, \ AFC auto-clear, auto-on
  \ 1E2C h,
  2607 h, \ disable clkout
  29C4 h, \ RSSI thres -98dB
  2B40 h, \ RSSI timeout after 128 bytes
  2E90 h, \ sync size 3 bytes
  2FAA h, \ sync1: 0xAA -- this is really the last preamble byte
  302D h, \ sync2: 0x2D -- actual sync byte
  312A h, \ sync3: network group
  \ 37D0 h, \ drop pkt if CRC fails
  37DA h, \ deliver even if CRC fails
  3842 h, \ max 62 byte payload
  3C8F h, \ fifo thres
  3D12 h, \ PacketConfig2, interpkt = 1, autorxrestart on
  6F20 h, \ Test DAGC
  0 h,  \ sentinel
decimal align

\ --------------------------------------------------
\  Internal Helpers
\ --------------------------------------------------

: rf-recvd-s! ( -- )  true rf.recvd ! ;
: rf-recvd-c! ( -- ) false rf.recvd ! ;
: rf-can-send-s! ( -- )   true  rf.can-send ! ;
: rf-can-send-c! ( -- )  false  rf.can-send ! ;

\ r/w access to the RF registers
: rf!@ ( b reg -- b ) +spi >spi >spi> -spi ;
: rf! ( b reg -- ) $80 or rf!@ drop ;
: rf@ ( reg -- b ) 0 swap rf!@ ;
: rf-h! ( h -- ) dup $FF and swap 8 rshift rf! ;
: rf-n@spi ( addr len -- )  \ read N bytes from the FIFO
  +spi RF:FIFO >spi 0 ?do spi> over c! 1+  loop drop -spi
  ;
: rf-n!spi ( addr len -- )  \ write N bytes to the FIFO
  +spi RF:FIFO $80 or >spi 0 ?do dup c@ >spi 1+ loop drop -spi
  ;

: rf-mode! ( b -- )  \ set the radio mode, and store a copy in a variable
  dup rf.mode @ <> if
    dup rf.mode !
    RF:OP rf@  $E3 and  or RF:OP rf!
    begin  RF:IRQ1 rf@  RF:IRQ1_MRDY and  until
  else
    drop
  then
  ;
: rf-idle-mode! ( -- ) rf.idle-mode @ rf-mode! ;
: rf-rx-mode! ( -- ) RF:M_RX rf-mode! ;
: rf-tx-mode! ( -- ) RF:M_TX rf-mode! ;
: rf-sleep ( -- ) RF:M_SLEEP rf-mode! ;

: rf-config! ( addr -- ) \ load many registers from <reg,value> array, zero-terminated
  RF:M_STDBY rf-mode!    \ some regs don't program in sleep mode, go figure...
  begin  dup h@  ?dup while  rf-h!  2+ repeat drop
  ;

: rf-freq! ( u -- )  \ set the frequency, supports any input precision
  dup rf.freq !
  begin dup 100000000 < while 10 * repeat
  ( f ) 2 lshift  32000000 11 rshift u/mod nip  \ avoid / use u/ instead
  ( u ) dup 10 rshift  RF:FRF rf!
  ( u ) dup 2 rshift  RF:FRF 1+ rf!
  ( u ) 6 lshift RF:FRF 2+ rf!
  ;
: rf-group!  ( u -- ) dup rf.group  ! RF:SYN3 rf! ;  \ set the net group (1..250)
: rf-nodeid! ( u -- ) dup rf.nodeid ! RF:ADDR rf! ; \ set the filter node id

: rf-fixed-pkt! ( -- )                      \ determine fixed packet and size
  RF:CONF rf@ 7 bit and if
    RF:PAYLOAD_LEN rf@ rf.fixed-pkt# !
  else
    0 rf.fixed-pkt# !
  then
  ;
: rf-pkt#@ ( -- n )                         \ pkt length, fetch from radio if variable
  rf.fixed-pkt# @ if
    rf.fixed-pkt# @
  else
    RF:FIFO rf@ RF:MAXDATA min
  then
  ;
: rf-fifo@ ( -- ) rf.buf rf-pkt#@ rf-n@spi ;

: rf-irq-exit ( -- ) 1 bit EXTI-PR bis! ;
: rf-handle-irq ( -- ) 		\ setup interrupt from rf69 -> DI00 -> PB0 (exti0) -> jnz
  \ payload sent
  rf.mode @ RF:M_TX = if
    RF:IRQ2 rf@ RF:IRQ2_SENT and if
      rf-idle-mode!
      ." sent "
      rf-can-send-s!
      rf-irq-exit
    then
  else
    \ packet received
    rf.mode @ RF:M_RX = if
      RF:IRQ2 rf@ RF:IRQ2_RECVD and if
        RF:RSSI rf@ 1 rshift negate rf.rssi !
        rf-idle-mode!
        rf-fifo@
        ." rcvd "
        rf-recvd-s!
        rf-irq-exit
      then
    then
  then
  ;
: rf-irq-init ( -- )             \ set up interrupt handler for radio
  \ link exti1 irq with radio
  ['] rf-handle-irq irq-exti0_1 !

     0 bit RCC-APB2ENR  bis!     \ enable setting SYSCFGEN
     1 bit RCC_IOPENR   bis!     \ enable GPIO B
     1 bit RCC_IOPSMENR bis!     \ enable GPIO B during sleep

    %001 AFIO-EXTICR1   bis!     \ select P<B>0
        0 bit EXTI-IMR  bis!     \ enable PB<0>
        0 bit EXTI-RTSR bis!     \ trigger on PB<0> rising edge

        5 bit NVIC-EN0R bis!     \ enable EXTI0_1 interrupt 5

     IMODE-HIGH PB0 io-mode!
  ;

\ rf-status fetches the IRQ1 reg, checks whether rx_sync is set and was not set
\ in rf.last. If so, it saves rssi, lna, and afc values; and then updates rf.last.
\ rf.last ensures that the info is grabbed only once per packet.
: rf-status ( -- )  \ update status values on sync match
  RF:IRQ1 rf@  RF:IRQ1_SYNC and  rf.last @ <> if
    rf.last  RF:IRQ1_SYNC over xor!  @ if
      RF:RSSI rf@  rf.rssi !
      RF:LNA rf@  3 rshift  7 and  rf.lna !
      RF:AFC rf@  8 lshift  RF:AFC 1+ rf@  or rf.afc !
    then
  then ;

\ TODO new header
: rf-parity ( -- u )  \ calculate group parity bits
  RF:SYN3 rf@ dup 4 lshift xor dup 2 lshift xor $C0 and ;

: rf-info ( -- )  \ display reception parameters as hex string
  rf.freq @ h.4 rf.group @ h.2 rf.rssi @ h.2 rf.lna @ h.2 rf.afc @ h.4 ;

: rf-correct ( -- ) \ correct the freq based on the AFC measurement of the last packet
  rf.afc @ 16 lshift 16 arshift 61 *         \ AFC correction applied in Hz
  2 arshift                                  \ apply 1/4 of measured offset as correction
  5000 over 0< if negate max else min then   \ don't apply more than 5khz
  rf.freq @ + dup rf.freq ! rf-freq!         \ apply correction
  ;

: rf-check ( b -- )  \ check that the register can be accessed over SPI
  begin  dup RF:SYN1 rf!  RF:SYN1 rf@  over = until
  drop ;

\ --------------------------------------------------
\   External API
\ --------------------------------------------------

: rf-init ( freq group node conf -- )  \ init RFM69
  spi-init

  $AA rf-check  $55 rf-check  \ will hang if there is no radio!

  ( conf )  rf-config!
  ( node )  rf-nodeid!
  ( group ) rf-group!
  ( freq )  rf-freq!

  rf-fixed-pkt!					\ set fixed packet flag/length
  rf-irq-init 					\ setup interrupts for radio
  rf-idle-mode!
  ;

: rf. ( -- )  \ print out all the RF69 registers
  cr 4 spaces  base @ hex  16 0 do space i . loop  base !
  $60 $00 do
    cr
    i h.2 ." :"
    16 0 do  space
      i j + ?dup if rf@ h.2 else ." --" then
    loop
  $10 +loop ;

\ : rf-power ( power -- )  \ change TX power level (0..31)
\   RF:PA_LEVEL rf@ $E0 and or RF:PA_LEVEL rf! ;
: rf-low-power ( n -- n ) ( power ) 18 + $1F and 7 bit or ;
: rf-mid-power ( n -- n ) ( power ) 14 + $1F and 6 bit or 5 bit or ;
: rf-high-power ( n -- n ) ( power ) 11 + $1F and 6 bit or 5 bit or ;
: rf-power ( power -- )                        \ change TX power level in dbm
  dup -18 max
  dup 13 <= if dup rf-low-power                 \ -18dBm to +13dBm
  else  dup 18 >= if dup rf-high-power          \ +18dBm to +20dBm - need PA1+PA2
  else dup rf-mid-power                         \ +14dBm to +17dBm
  then then
  ( pa_level ) RF:PA_LEVEL rf!
  ( power ) rf.power !
  drop
  ;

: rf-recv ( -- n )                    \ set rx mode and return if received packet
  rf.mode @ RF:M_TX = if
    false exit
  then

  rf-rx-mode!
  %01 6 lshift $25 rf!                \ set trigger for PacketReady on DIO0

  rf.recvd @ if rf-recvd-c! true else false then
  ;

: rf-send ( buffer len -- n )         	\ send out one packet for node
  rf.mode @ RF:M_TX = if 				\ still sending packet drop stack/return
    drop drop false exit              	\ still sending, early return
  then
  rf-idle-mode!  rf-can-send-c!

  ( buffer len ) rf-n!spi
  rf-tx-mode!
  true
  ;
