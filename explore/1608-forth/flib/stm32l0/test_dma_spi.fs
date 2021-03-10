
\ include ../../../../embello/explore/1608-forth/flib/stm32l0/dma.fs
\ include ../../../../embello/explore/1608-forth/flib/stm32l0/spi.fs

\ ------------------- utils -------------------

66 variable count
count @ buffer: buf1
count @ buffer: buf2

\ set the radio mode, and store a copy in a variable
: reset ( -- ) count @ 0 do i 1+ buf1 i + c! 0 buf2 i + c! loop ;
: show ( buf -- ) ." -> " count @ . ( count @ ) 40 0 do dup i + c@ . loop drop ;

\ ------------------- tools -------------------
0 constant RF:FIFO

: reset-fifo ( -- )
	RF:M_STDBY rf-mode! rf-mode-ready
	RF:M_RX rf-mode! 		rf-mode-ready
	RF:M_STDBY rf-mode! rf-mode-ready
  ;

\ ------------------- testing -------------------

: spi-test-init
  led-off
	spi-init
	reset-fifo

  true  DMA1:SPI-RX-CHAN dma-init
  true  DMA1:SPI-TX-CHAN dma-init
  ;

: spi-test
  spi-init
  +spi $10 >spi spi> -spi
  $24 = .
  ;

: buf-test
  reset                                    cr ." buf1  " ." buf1 " buf1 show
	reset-fifo
  reset  buf1 66 rf-n!spi buf2 66 rf-n@spi cr ." test  " ." buf2 " buf2 show
	reset-fifo
  reset  buf1 66 RF:FIFO $80 or buf>spi-dma
         buf2 66 rf-n@spi                  cr ." >dma  " ." buf2 " buf2 show
	reset-fifo
  reset  buf1 66 rf-n!spi
         buf2 66 RF:FIFO spi>buf-dma       cr ."  dma> " ." buf2 " buf2 show
	reset-fifo
  reset  buf1 66 rf-n!spi
         buf2 66 RF:FIFO spi>buf-dma       cr ."  dma> " ." buf2 " buf2 show
	reset-fifo
  reset  buf1 66 RF:FIFO $80 or buf>spi-dma
         buf2 66 RF:FIFO spi>buf-dma       cr ." >dma> " ." buf2 " buf2 show
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
cr ." >>> " mem-test
cr ." >>> " spi-test
cr ." >>> " buf-test
