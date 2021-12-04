
\ include ../../../../embello/explore/1608-forth/flib/stm32l0/dma.fs
\ include ../../../../embello/explore/1608-forth/flib/stm32l0/spi.fs

\ ------------------- utils -------------------

66 variable count
count @ buffer: buf1
count @ buffer: buf2

\ set the radio mode, and store a copy in a variable
: reset ( -- ) count @ 0 do i 1+ buf1 i + c! 0 buf2 i + c! loop ;
: show ( buf -- ) ." -> " count @ . ( count @ ) 40 0 do dup i + c@ . loop drop ;


\ ------------------------------ SPI2.fs ------------------------------

$40003800 constant SPI2
     SPI2 $0 + constant SPI2-CR1
     SPI2 $4 + constant SPI2-CR2
     SPI2 $8 + constant SPI2-SR
     SPI2 $C + constant SPI2-DR

: +spi ( -- ) ssel @ ioc! ;  \ select SPI
: -spi ( -- ) ssel @ ios! ;  \ deselect SPI

: >spi> ( c -- c )  \ hardware SPI, 8 bits
  SPI2-DR !  begin SPI2-SR @ 1 and until  SPI2-DR @ ;

\ single byte transfers
: spi> ( -- c ) 0 >spi> ;  \ read byte from SPI
: >spi ( c -- ) >spi> drop ;  \ write byte to SPI

\ ===== initialization

: spi!ssel ( ssel -- ) \ set chip-select pin, e.g. "PA4 spi!ssel"
  ssel !
  ;

: spi2-init ( -- )  \ set up hardware SPI
  OMODE-PP    ssel @ io-mode! -spi
  OMODE-AF-PP SCLK   io-mode!
  IMODE-FLOAT MISO   io-mode!
  OMODE-AF-PP MOSI   io-mode!

  14 bit RCC-APB1ENR bis!  \ set SPI2EN
  %0000000001010100 SPI2-CR1 !  \ clk/8, i.e. 9 MHz, master
  SPI2-SR @ drop  \ appears to be needed to avoid hang in some cases
  2 bit SPI2-CR2 bis!  \ SS output enable
;


$40005400 constant I2C2
     I2C1 $00 + constant I2C2-CR1
     I2C1 $04 + constant I2C2-CR2
   \ I2C1 $08 + constant I2C2-OAR1
   \ I2C1 $0C + constant I2C2-OAR2
     I2C1 $10 + constant I2C2-TIMINGR
   \ I2C1 $14 + constant I2C2-TIMEOUTR
     I2C1 $18 + constant I2C2-ISR
     I2C1 $1C + constant I2C2-ICR
     I2C1 $20 + constant I2C2-PECR
     I2C1 $24 + constant I2C2-RXDR
     I2C1 $28 + constant I2C2-TXDR


\ include embello/explore/1608-forth/flib/stm32f1/dma.fs
include dma.fs

\ ------------------- testing -------------------

: spi-test-init
  led-off

  true  DMA1:SPI2-RX-CHAN dma-init
  true  DMA1:SPI2-TX-CHAN dma-init
  ;

: mem-test
	reset
  cr buf1 show cr buf2 show

  buf2 buf1 dma-mem-init
  buf2 66 DMA1:MEM-CHAN +dma dma-wait -dma-mem
  111111111111 .

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
