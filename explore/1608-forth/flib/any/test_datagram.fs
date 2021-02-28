
include datagram.fs

66 variable count
count @ buffer: buf1
: show ( buf -- ) ." -> " count @ . count @ 0 do dup i + c@ . loop drop ;
\ : show ( -- )
	\ rf-show-packet
  \ buf1 c@ 1+ count !
	\ buf1 show
	\ ;

40 buffer: buf1
40 buffer: buf2
40 variable count
: reset ( -- ) count @ 0 do i 1+ buf1 i + c! 0 buf2 i + c! 0 i2c.buf i + c! loop ;
: show ( buf -- ) ." -> " count @ . count @ 0 do dup i + c@ . loop drop ;

\ --------------------------------------------------------------------------------

: radio-init
  86926 $B6 30 rf:GFSK_Rb250Fd250 RF:INIT rf-init
	;

: tx-test
  radio-init

  35992 50224 101890 1919 18 97 113
  hwid <pkt  7 0 do +pkt loop  pkt>

  \ over ( buf1 ) buf2 dma-mem-init
  \ reset
  \ 40 DMA1:MEM-CHAN +dma
  \ begin dma.complete @ true = until       \ wait until transfer completes
  \ cr buf2 show

  ( buffer len ) $b dg-send-to
  ;

\ : rx-test
   \ radio-init

   \ reset
   \ buf1 40 rf-n!spi
   \ cr ." buf2 " buf2 show
   \ buf2 40 rf-n@spi
   \ cr ." buf1 " buf1 show
   \ cr ." buf2 " buf2 show
   \ reset

   \ 40 40 DMA1:SPI-RX-CHAN +dma-spi
   \ cr 111 . .s
   \ 2 2 DMA1:SPI-RX-CHAN +dma-spi

   \ DMA1-CCR DMA1:SPI-RX-CHAN dma-reg @ .
   \ DMA1-CCR DMA1:TX-CHAN dma-reg @ .

   \ ." rx "
   \ RF:FIFO spi.cmd !
   \ $2f spi.cmd !
   \ $2f buf1 c! 0 buf1 1+ c!
   \ spi-wait -spi

   \ buf2 40 rf>buf

   \ cr 3 . .s
   \ cr buf1 show
   \ cr buf2 show
   \ ;

led-off
cr ." >>> " tx-test
