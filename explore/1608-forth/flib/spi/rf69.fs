\ rf69 driver

\ ******** OUTDATED ********
\ ******** NOT FLASHED ********

\ --------------------------------------------------
\  Configuration
\ --------------------------------------------------

\ TODO need to move registers to central location
NVIC-EN0R $304 + constant NVIC-IPR1

       $00 constant RF:FIFO
       $01 constant RF:OP
       $07 constant RF:FRF
       $10 constant RF:REG_VERSION
       $11 constant RF:PA_LEVEL
       $18 constant RF:LNA
       $1F constant RF:AFC
       $24 constant RF:RSSI
       $27 constant RF:IRQ1
       $28 constant RF:IRQ2
       $29 constant RF:RSSI_THRESH
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
         0 variable rf.recvd#     \ payload received counter
         0 variable rf.fixed-pkt# \ length of fixed packet or 0 for variable
        66 buffer:  rf.buf        \ buffer with last received packet data
    rf.buf constant rf.len
 rf.buf 1+ constant rf.data

      8683 variable rf.freq    \ frequency (auto-scaled to 100..999 MHz)
        42 variable rf.group   \ network group (1..250)
        61 variable rf.nodeid  \ node ID of this node (1..63)

create rf:init  \ initialise the radio, each 16-bit word is <reg#,val>
hex
  0B20 h, \ low M
  119F h, \ pa level
  1E0C h, \ AFC auto-clear, auto-on
  2607 h, \ disable clkout
  29C4 h, \ RSSI thres -98dB
  \ 2B40 h, \ RSSI timeout after 128 bytes
  2E90 h, \ sync size 3 bytes
  2FAA h, \ sync1: 0xAA -- this is really the last preamble byte
  302D h, \ sync2: 0x2D -- actual sync byte
  312A h, \ sync3: network group
  3842 h, \ max 62 byte payload
  3C8F h, \ fifo thres
  3D12 h, \ PacketConfig2, interpkt = 1, autorxrestart on
  6F20 h, \ Test DAGC
  0 h,  \ sentinel
decimal align

create rf:GFSK_Rb250Fd250   \ GFSK, Whitening, Rb = 250kbs,  Fd = 250kHz
hex
  0201 h,           \ GFSK BT = 1.0
  0300 h, 0480 h,   \ bit rate  250kbs
  0510 h, 0600 h,   \ Fdev 250kHz
  19E0 h, 1AE0 h,   \ RxBw 125khz, AFCBw 125khz
  37D2 h,           \ variable, dc white, crc, node filt
  0 h,
decimal

\ --------------------------------------------------
\  Internal Helpers
\ --------------------------------------------------

: rf-recvd-s! ( -- )  true rf.recvd ! ;
: rf-recvd-c! ( -- ) false rf.recvd ! ;

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

: rf-mode-ready
  \ TODO interrupts DIO5
  begin  RF:IRQ1 rf@  RF:IRQ1_MRDY and  until
  ;
: rf-mode! ( b -- )  \ set the radio mode, and store a copy in a variable
  dup rf.mode @ <> if
    dup rf.mode !
    RF:OP rf@  $E3 and  or RF:OP rf!
    rf-mode-ready
  else
    drop
  then
  ;
: rf-idle-mode! ( -- ) rf.idle-mode @ rf-mode! ;
: rf-rx-mode! ( -- ) RF:M_RX rf-mode! ;
: rf-tx-mode! ( -- ) RF:M_TX rf-mode! ;
: rf-sleep ( -- ) RF:M_SLEEP rf-mode! ;

\ TODO use burst mode
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

: rf-fixed-pkt! ( -- )                     \ determine fixed packet and size
  RF:CONF rf@ 7 bit and if
    0 rf.fixed-pkt# !
  else
    RF:PAYLOAD_LEN rf@ rf.fixed-pkt# !
  then
  ;
: rf-pkt# ( -- n )                         \ pkt length, fetch from radio if variable
  rf.fixed-pkt# @ if
    rf.fixed-pkt# @
  else
    RF:FIFO rf@ RF:MAXDATA min
  then
  dup rf.len c!
  ;
: rf-fifo@ ( -- ) rf.data rf-pkt# rf-n@spi ;

: rf-irq-exit ( -- ) 1 bit EXTI-PR bis! ;
: rf-irq-tx
  RF:IRQ2 rf@ RF:IRQ2_SENT and if
    rf-idle-mode!
    ." sent "
    rf-irq-exit
  then
  ;
: rf-irq-rx
  RF:IRQ2 rf@ RF:IRQ2_RECVD and if
    RF:RSSI rf@ 1 rshift negate rf.rssi !
    rf-idle-mode!
    rf-fifo@
    1 rf.recvd# +!
    ." rcvd " cr
    rf-recvd-s!
    rf-irq-exit
  then
  ;
: rf-handle-irq ( -- )      \ setup interrupt from rf69 -> DI00 -> PB0 (exti0) -> jnz
  ." int " binary rf:irq2 rf@ rf:irq1 rf@ ." (" . space . rf.mode @ . ." )" hex
  \ RF:IRQ2 rf@
  rf.mode @
  case
    RF:M_TX of rf-irq-tx endof
    RF:M_RX of rf-irq-rx endof
  endcase
  ;
: rf-irq-init ( -- )             \ set up interrupt handler for radio
  ['] rf-handle-irq irq-exti0_1 !

     0 bit RCC-APB2ENR  bis!     \ enable setting SYSCFGEN
     1 bit RCC_IOPENR   bis!     \ enable GPIO B
     1 bit RCC_IOPSMENR bis!     \ enable GPIO B during sleep

    %001 AFIO-EXTICR1   bis!     \ select P<B>0
        0 bit EXTI-IMR  bis!     \ enable PB<0>
        0 bit EXTI-RTSR bis!     \ trigger on PB<0> rising edge

        5 bit NVIC-EN0R bis!     \ enable EXTI0_1 interrupt 5
         \ $F00 NVIC-IPR1 bis!     \ interrupt priority
         $100 NVIC-IPR1 bis!     \ interrupt priority

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

: rf-info ( -- )  \ display reception parameters as hex string
  rf.freq @ h.4 rf.group @ h.2 rf.rssi @ h.2 rf.lna @ h.2 rf.afc @ h.4 ;

: rf-correct ( -- ) \ correct the freq based on the AFC measurement of the last packet
  rf.afc @ 16 lshift 16 arshift 61 *         \ AFC correction applied in Hz
  2 arshift                                  \ apply 1/4 of measured offset as correction
  5000 over 0< if negate max else min then   \ don't apply more than 5khz
  rf.freq @ + dup rf.freq ! rf-freq!         \ apply correction
  ;

: rf-check ( -- )  \ check that the register can be accessed over SPI
  RF:REG_VERSION rf@ 0= RF:REG_VERSION rf@ $ff = or
  drop
  ;

\ --------------------------------------------------
\   External API
\ --------------------------------------------------

: rf-init ( freq group node conf -- )       \ init RFM69
  spi-init

  rf-check                                  \ will hang if there is no radio!

  ( conf )  rf-config!

  ( node )  rf-nodeid!
  ( group ) rf-group!
  ( freq )  rf-freq!

  rf-fixed-pkt!                             \ set fixed packet flag/length
  rf-irq-init                               \ setup interrupts for radio
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

\ https://andrehessling.de/2015/02/07/figuring-out-the-power-level-settings-of-hoperfs-rfm69-hwhcw-modules/
\ RFM69(C)W only has PA0
\ RFM69H(C)W has PA1 and PA2
\ feather wing is RFM69HCW - PA1 and PA2 - don't use PA0
\ jeenode zero if RFM69CW (probably) - only PA0
\ : rf-power ( power -- )  \ change TX power level (0..31)
\   \ RF:PA_LEVEL rf@ $E0 and or RF:PA_LEVEL rf!
\   $80 or RF:PA_LEVEL rf!          \ only use PA0
\   ;
\ : rf-low-power ( n -- n ) ( power ) 18 + $1F and 7 bit or ;
\ : rf-mid-power ( n -- n ) ( power ) 14 + $1F and 6 bit or 5 bit or ;
\ : rf-high-power ( n -- n ) ( power ) 11 + $1F and 6 bit or 5 bit or ;
\ : rf-power ( power -- )                        \ change TX power level in dbm
  \ dup -18 max
  \ dup 13 <= if dup rf-low-power                 \ -18dBm to +13dBm
  \ else  dup 18 >= if dup rf-high-power          \ +18dBm to +20dBm - need PA1+PA2
  \ else dup rf-mid-power                         \ +14dBm to +17dBm
  \ then then
  \ ( pa_level ) RF:PA_LEVEL rf!
  \ ( power ) rf.power !
  \ drop
  \ ;
: rf-power ( power -- )                        \ change TX power level 0..31
  $80 or RF:PA_LEVEL rf!                       \ only use PA0
  ;

\ TODO move (copy) received data to addr rf.buf -> addr
\ :  rf-recv ( addr -- n )                \ set rx mode and return if received packet
: rf-recv ( -- n )                      \ set rx mode and return if received packet
  rf.mode @ RF:M_TX = if
    false exit
  then

  rf-rx-mode!
  $40 $25 rf!                           \ set trigger for PacketReady on DIO0

  rf.recvd @ if rf-recvd-c! true else false then
  ;

\ variable packet len < 66 per packet - can send 64 bytes
: rf-send ( buffer len -- n )           \ send out one packet for node
  rf.mode @ RF:M_TX = if                \ still sending packet drop stack and return
    drop drop false exit
  then
  rf-idle-mode!

  $0 $25 rf!                \ set trigger for PacketReady on DIO0

  ( buffer len ) rf-n!spi
  rf-tx-mode!
  true
  ;

\ : rf-listener-mode! ( on/off -- )
  \ ( flag ) if
    \ 6 bit RF:OP rf@ or  RF:OP rf!
  \ else
    \ ." off "
    \ RF:OP rf@ dup
    \ %01 5 lshift or RF:OP rf!
    \ %00 5 lshift or RF:OP rf!
  \ then
  \ ;

\ : rf-auto ( -- )
  \ \ automode rf-sleep -> tx
  \ rf-idle-mode!
  \ %010 5 lshift \ enter condition - fifo not empty
  \ %110 2 lshift \ exit condition - packet sent
  \ %11           \ intermediat state - tx
  \ or or $3B rf!
  \ ;

