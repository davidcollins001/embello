\ rf69 driver

\ ******** UPDATED ********
\ ******** FLASHED ********

\ TODO only read sent bytes from radio not entire buffer

\ --------------------------------------------------
\  Configuration
\ --------------------------------------------------

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

        66 constant RF:MAXDATA
		31 constant RF:MAXPOWER

         0 variable rf.mode       \ last set chip mode
RF:M_STDBY variable rf.idle-mode  \ default idle mode
         0 variable rf.rssi       \ RSSI signal strength of last reception
         0 variable rf.lna        \ Low Noise Amplifier setting (set by AGC)
         0 variable rf.power      \ power setting
         0 variable rf.afc        \ Auto Frequency Control offset
     false variable rf.recvd      \ flag to show packet was received
     false variable rf.sending    \ flag to show payload is being sent
         0 variable rf.sent#      \ packet sent counter
         0 variable rf.recvd#     \ payload received counter
         0 variable rf.fixed-pkt# \ length of fixed packet or 0 for variable
RF:MAXDATA buffer:  rf.buf        \ buffer with last received packet data
    rf.buf constant rf.len        \ packet len, not including itself
         0 variable rf.packet-handler \ variable for handling packet in rf-listen

      8683 variable rf.freq    \ frequency (auto-scaled to 100..999 MHz)
        42 variable rf.group   \ network group (1..250)
        61 variable rf.nodeid  \ node ID of this node (1..63)

create rf:init  \ initialise the radio, each 16-bit word is <reg#,val>
hex
  0B20 h, \ low M
  119F h, \ pa level
  \ 1E0C h, \ AFC auto-clear, auto-on
  \ 29C4 h, \ RSSI thres -98dB
  29E4 h, \ RSSI thres -98dB
  \ 2B40 h, \ RSSI timeout after 128 bytes
  2B00 h, \ RSSI timeout after 128 bytes
  2E90 h, \ sync size 3 bytes
  2FAA h, \ sync1: 0xAA -- this is really the last preamble byte
  302D h, \ sync2: 0x2D -- actual sync byte
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

: rf-recvd-s! ( -- )     true    rf.recvd ! ;
: rf-recvd-c! ( -- )     false   rf.recvd ! ;
: rf-recvd? ( -- n )             rf.recvd @ ;
: rf-sending-s! ( -- n ) true  rf.sending ! ;
: rf-sending-c! ( -- n ) false rf.sending ! ;
: rf-sending? ( -- n )         rf.sending @ ;

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
: rf>buf ( addr len -- )
  \ TODO deal with dma config better
  \ disable minc for tx
  7 bit DMA1-CCR DMA1:MEM-CHAN dma-reg bic!

  RF:FIFO spi.cmd !
  over
    1   spi.cmd rot ( addr ) +dma-spi  spi-wait
  ( n ) spi.cmd rot ( addr ) +dma-spi  spi-wait
  -spi
  ;
: buf>rf ( addr len -- )
  \ use 2 dma operations to send command and data separately
  RF:FIFO $80 or spi.cmd c!
         1   spi.cmd  0 +dma-spi  spi-wait
  swap ( n ) ( addr ) 0 +dma-spi  spi-wait
  -spi
  ;

: rf-mode-ready
  \ TODO interrupts DIO5
  begin  RF:IRQ1 rf@  RF:IRQ1_MRDY and  until
  ;
: rf-mode! ( b -- )  \ set the radio mode, and store a copy in a variable
  dup rf.mode @ <> if
    dup rf.mode !
    RF:OP rf@  $E3 and  or RF:OP rf!
    \ rf-mode-ready
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
  \ Frequency steps are in units of (32,000,000 >> 19) = 61.03515625 Hz
  \ use multiples of 64 to avoid multi-precision arithmetic, i.e. 3906.25 Hz
  \ due to this, the lower 6 bits of the calculated factor will always be 0
  \ this is still 4 ppm, i.e. well below the radio's 32 MHz crystal accuracy
  \ 868.0 MHz = 0xD90000, 868.3 MHz = 0xD91300, 915.0 MHz = 0xE4C000
  dup rf.freq !
  begin dup 100000000 < while 10 * repeat
  ( f ) 2 lshift  32000000 11 rshift u/mod nip  \ avoid / use u/ instead
  ( u ) dup 10 rshift  RF:FRF rf!
  ( u ) dup 2 rshift  RF:FRF 1+ rf!
  ( u ) 6 lshift RF:FRF 2+ rf!
  ;
: rf-group!  ( u -- ) dup rf.group  ! RF:SYN3 rf! ;  \ set the net group (1..250)
: rf-nodeid! ( u -- ) dup rf.nodeid ! RF:ADDR rf! ; \ set the filter node id

\ read full radio buffer instead of first byte for variable packet format
: rf-pkt# ( -- n )                         \ pkt length, fetch from radio if variable
  RF:PAYLOAD_LEN rf@ RF:MAXDATA min
  ;
: rf-fifo@ ( -- ) rf.buf rf.fixed-pkt# @ rf>buf ;

: rf-status ( -- )                      \ update status values on sync match
  RF:RSSI rf@  rf.rssi !
  RF:LNA rf@  3 rshift  7 and  rf.lna !
  RF:AFC rf@  8 lshift  RF:AFC 1+ rf@  or rf.afc !
  ;

: rf-irq-exit ( -- ) 1 bit EXTI-PR bis! ;
: rf-irq-handler ( -- )      \ setup interrupt from rf69 -> DI00 -> PB0 (exti0) -> jnz
  \ don't check irq flags - type of interrupt was set on tx/rx
  rf.mode @
  case
    RF:M_TX of rf-idle-mode! 1 rf.sent# +! rf-sending-c! endof
    RF:M_RX of 1 rf.recvd# +! rf-recvd-s! endof
  endcase
  rf-irq-exit
  ;
: rf-irq-init ( -- )                    \ set up interrupt handler for radio
  ['] rf-irq-handler irq-exti0_1 !

     0 bit RCC-APB2ENR  bis!     \ enable setting SYSCFGEN
     1 bit RCC_IOPENR   bis!     \ enable GPIO B
     1 bit RCC_IOPSMENR bis!     \ enable GPIO B during sleep

    %001 AFIO-EXTICR1   bis!     \ select P<B>0
        0 bit EXTI-IMR  bis!     \ enable PB<0>
        0 bit EXTI-RTSR bis!     \ trigger on PB<0> rising edge

        5 bit NVIC-EN0R bis!     \ enable EXTI0_1 interrupt 5
        $0C00 NVIC-IPR1 bis!     \ interrupt priority

     IMODE-HIGH PB0 io-mode!
  ;
: rf-recv-done ( addr -- )              \ userland handler for irq
  rf-status
  rf-idle-mode!
  rf-fifo@
  ( addr ) dup if
    rf.buf ( addr ) swap rf.fixed-pkt# @ move  \ copy data for user if addr provided
  else
    drop
  then
  rf-recvd-c!
  ;

: rf-info ( -- )  \ display reception parameters as hex string
  rf.freq @ h.4 rf.group @ h.2 rf.rssi @ h.2 rf.lna @ h.2 rf.afc @ h.4 ;

: rf-show-packet ( -- )
  ." RF69 " rf-info space ." ( " rf.rssi @ . ." )" space
  RF:CONF rf@ 7 bit and 0= if            \ check if payload is fixed/variable
    RF:PAYLOAD_LEN rf@
  else
    rf.len c@ 1+
    dup h.2 space ." : "
  then
  \ 11 debug rf.fixed-pkt# @ .
  0 do rf.buf i + c@ h.2 space loop cr
  ;
' rf-show-packet rf.packet-handler !

: rf-correct ( -- ) \ correct the freq based on the AFC measurement of the last packet
  rf.afc @ 16 lshift 16 arshift 61 *         \ AFC correction applied in Hz
  2 arshift                                  \ apply 1/4 of measured offset as correction
  5000 over 0< if negate max else min then   \ don't apply more than 5khz
  rf.freq @ + rf-freq!                       \ apply correction
  ;

: rf-check ( -- )  \ check that the register can be accessed over SPI
  RF:REG_VERSION rf@ 0= RF:REG_VERSION rf@ $ff = or
  drop
  ;

\ --------------------------------------------------
\   External API
\ --------------------------------------------------

: rf-init ( freq group node modem conf -- )       \ init RFM69
  spi-init

  \ enable dma
  false DMA1:SPI-RX-CHAN dma-spi-init
  true  DMA1:SPI-TX-CHAN dma-spi-init

  rf-check                                  \ will hang if there is no radio!

  ( conf )  rf-config!
  ( modem ) rf-config!

  ( node )  rf-nodeid!
  ( group ) rf-group!
  ( freq )  rf-freq!

  rf-irq-init                               \ setup interrupts for radio
  rf-idle-mode!

  rf-pkt# rf.fixed-pkt# !                   \ get max payload size
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
: rf-power ( power -- )                            \ change TX power level 0..31
  RF:MAXPOWER min 0 max                            \ bounds check
  dup rf.power @ <> if
    ( power ) dup rf.power !
    $80 or  RF:PA_LEVEL rf!                        \ only use PA0
  else
    drop
  then
  ;

: rf-recv ( -- n )                      \ set rx mode and return if received packet
  rf-sending? if
    false exit
  then

  rf-rx-mode!
  $40 $25 rf!                           \ set trigger for PacketReady on DIO0

  rf-recvd?
  ;

: rf-listen ( addr -- )
  cr
  begin
    rf-recv if
      ( addr ) rf-recv-done
      rf.packet-handler @ execute
    then
  key? until
  rf-idle-mode!
  ;

\ variable packet len < 66 per packet - can send 64 bytes
: rf-send ( buffer len -- n )           \ send out one packet for node
  rf-sending? if                        \ still sending packet drop stack and return
    2drop false  exit
  then
  rf-idle-mode!

  $0 $25 rf!                            \ set trigger for PacketReady on DIO0
  rf-sending-s!

  ( buffer len ) buf>rf
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
  \ %010 5 lshift    \ enter condition - fifo not empty
  \ %110 2 lshift or \ exit condition - packet sent
  \ %11           or \ intermediat state - tx
  \ $3B rf!
  \ ;

compiletoram? not [if]  cornerstone <<<rf69>>> [then]
