\ rf69 driver
\ uses spi

       $00 constant RF:FIFO
       $01 constant RF:OP
       $07 constant RF:FRF
       $11 constant RF:PA
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

   0 variable rf.mode  \ last set chip mode
   0 variable rf.last  \ flag used to fetch RSSI only once per packet
   0 variable rf.rssi  \ RSSI signal strength of last reception
   0 variable rf.lna   \ Low Noise Amplifier setting (set by AGC)
   0 variable rf.afc   \ Auto Frequency Control offset
   0 variable rf.recvd \ flag to show packet was received
  66 buffer:  rf.buf   \ buffer with last received packet data
      rf.buf constant rf.len
  rf.buf 1 + constant rf.to_addr
  rf.buf 2 + constant rf.data


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
  37D8 h, \ deliver even if CRC fails
  3842 h, \ max 62 byte payload
  3C8F h, \ fifo thres
  3D12 h, \ PacketConfig2, interpkt = 1, autorxrestart on
  6F20 h, \ Test DAGC
  0 h,  \ sentinel
decimal align

\ r/w access to the RF registers
: rf!@ ( b reg -- b ) +spi >spi >spi> -spi ;
: rf! ( b reg -- ) $80 or rf!@ drop ;
: rf@ ( reg -- b ) 0 swap rf!@ ;
: rf-h! ( h -- ) dup $FF and swap 8 rshift rf! ;
: rf-n@spi ( addr len -- )  \ read N bytes from the FIFO
  +spi RF:FIFO >spi 0 do spi> over c! 1+  loop drop -spi
  ;
: rf-n!spi ( addr len -- )  \ write N bytes to the FIFO
  +spi RF:FIFO $80 or >spi 0 do dup c@ >spi 1+ loop drop -spi
  ;

: rf-mode! ( b -- )  \ set the radio mode, and store a copy in a variable
  dup rf.mode !
  RF:OP rf@  $E3 and  or RF:OP rf!
  begin  RF:IRQ1 rf@  RF:IRQ1_MRDY and  until
  ;

: rf-config! ( addr -- ) \ load many registers from <reg,value> array, zero-terminated
  RF:M_STDBY rf-mode! \ some regs don't program in sleep mode, go figure...
  begin  dup h@  ?dup while  rf-h!  2+ repeat drop
  ;

: rf-freq ( u -- )  \ set the frequency, supports any input precision
  begin dup 100000000 < while 10 * repeat
  ( f ) 2 lshift  32000000 11 rshift u/mod nip  \ avoid / use u/ instead
  ( u ) dup 10 rshift  RF:FRF rf!
  ( u ) dup 2 rshift  RF:FRF 1+ rf!
  ( u ) 6 lshift RF:FRF 2+ rf!
  ;
: rf-group ( u -- ) RF:SYN3 rf! ;  \ set the net group (1..250)

: rf-correct ( -- ) \ correct the freq based on the AFC measurement of the last packet
  rf.afc @ 16 lshift 16 arshift 61 *         \ AFC correction applied in Hz
  2 arshift                                  \ apply 1/4 of measured offset as correction
  5000 over 0< if negate max else min then   \ don't apply more than 5khz
  rf.freq @ + dup rf.freq ! rf-freq          \ apply correction
  ;

\ TODO interrput and sleep
: rf-check ( b -- )  \ check that the register can be accessed over SPI
  begin  dup RF:SYN1 rf!  RF:SYN1 rf@  over = until
  drop ;

: rf-ini ( group freq config -- )  \ internal init of the RFM69 radio module
  spi-init
  $AA rf-check  $55 rf-check  \ will hang if there is no radio!
  ( config ) rf-config!
  rf-freq rf-group ;

\ rf-rssi checks whether the rssi bit is set in IRQ1 reg and sets the LED to match.
\ It also checks whether there is an rssi timeout and restarts the receiver if so.
: rf-rssi ( -- )
  RF:IRQ1 rf@
  dup RF:IRQ1_RSSI and 3 rshift 1 swap - LED io!
  dup RF:IRQ1_TIMEOUT and if
      RF:M_FS rf-mode!
    then
  drop
  ;

\ rf-timeout checks whether there is an rssi timeout and restarts the receiver if so.
: rf-timeout ( -- )
  RF:IRQ1 rf@ RF:IRQ1_TIMEOUT and if
    RF:M_FS rf-mode!
  then ;

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

: rf-pkt-len ( -- n )
  RF:PAYLOAD_LEN rf@
  \ check for fixed/variable payload and compare with payload len reg
  dup RF:CONF rf@ 7 bit and if RF:FIFO rf@ min swap drop then
  ;
: rf-read-fifo ( n -- )                    \ read n bytes from radio into internal buffer
  \ might have to happen before standby
  rf-pkt-len dup rf.len c! rf.buf 1+ swap rf-n@spi

  1 rf.recvd !
  ;

: rf-info ( -- )  \ display reception parameters as hex string
  rf.freq @ h.4 rf.group @ h.2 rf.rssi @ h.2 rf.lna @ h.2 rf.afc @ h.4 ;

: rf-irq-exit ( -- ) 1 bit EXTI-PR bis! ;
: rf-handle-irq ( -- ) 		\ setup interrupt from rf69 -> DI00 -> PB0 (exti0) -> jnz
  \ packet received
  rf.mode @ RF:M_RX = if
    RF:IRQ2 rf@ RF:IRQ2_RECVD and if
      0 rf.rssi !  0 rf.afc ! rf-rssi rf-status
      RF:M_STDBY rf-mode!
      rf-read-fifo
      ." rcvd "
      rf-irq-exit
    then
  then

  \ payload sent
  rf.mode @ RF:M_TX = if
    RF:IRQ2 rf@ RF:IRQ2_SENT and if
      RF:M_STDBY rf-mode!
      ." sent "
      rf-irq-exit
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

\ \\\\\\\\\\\\\\\\\\\\\\\\\ PUBLIC API \\\\\\\\\\\\\\\\\\\\\\\\\ \

: rf-power ( n -- )  \ change TX power level (0..31)
  RF:PA rf@ $E0 and or RF:PA rf! ;

: rf-sleep ( -- ) RF:M_SLEEP rf-mode! ;  \ put radio module to sleep

: rf-init ( addr -- )  \ init RFM69 with current rf.group and rf.freq values
  rf.group @ rf.freq @ rot rf-ini ;

: rf. ( -- )  \ print out all the RF69 registers
  cr 4 spaces  base @ hex  16 0 do space i . loop  base !
  $60 $00 do
    cr
    i h.2 ." :"
    16 0 do  space
      i j + ?dup if rf@ h.2 else ." --" then
    loop
  $10 +loop ;

: rf-recv ( -- )
  %01 6 lshift $25 rf!                \ set trigger for PacketReady on DIO0
  rf-irq-init
  \ TODO check for existing outgoing message
  RF:M_RX rf-mode!
  0 rf.recvd !
  ;

: rf-raw-send ( buf len -- )          \ send out one packet for node
  %00 6 lshift $25 rf!                \ set trigger for PacketSent on DIO0
  rf-irq-init
  \ TODO check for existing outgoing message

  RF:M_STDBY rf-mode!
  ( addr count ) rf-n!spi
  RF:M_TX rf-mode!
  ;

: rf-send ( buf addr count -- )       \ send out one packet for node running rf12demo
  swap 2 + tuck                       \ add header size to len
  0 rf! 0 rf! rf-raw-send
  ;

\ rf.
\ rf-listen
\ 12345 rf-txtest
