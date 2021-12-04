
\ include ../../../../embello/explore/1608-forth/flib/stm32l0/dma.fs
\ include ../../../../embello/explore/1608-forth/flib/stm32l0/spi.fs

\ ------------------- utils -------------------

66 variable count
count @ buffer: buf1
count @ buffer: buf2

\ set the radio mode, and store a copy in a variable
: reset ( -- ) count @ 0 do i 1+ buf1 i + c! 0 buf2 i + c! loop ;
: show ( buf -- ) ." -> " count @ . ( count @ ) 40 0 do dup i + c@ . loop drop ;


\ ------------------- testing -------------------

: spi-test-init
  led-off

  true  DMA1:SPI1-RX-CHAN dma-init
  true  DMA1:SPI1-TX-CHAN dma-init
  ;

: mem-test
	reset
  cr buf1 show cr buf2 show

  buf2 buf1 dma-mem-init
  buf2 66 DMA1:MEM-CHAN +dma dma-wait DMA1:MEM-CHAN -dma

  cr buf2 show

  reset
  0 buf1 dma-mem-init
  buf2 66 DMA1:MEM-CHAN +dma dma-wait -dma-mem
  cr buf2 show
  ;

spi-test-init
\ cr ." >>> " mem-test
\ cr ." >>> " spi-test
\ cr ." >>> " buf-test
