\ NOTE requires irq-dma-chX to be enabled in the image which isn't the case by
\      default for stm32f103

\ TODO dma-mem-init and dma-spi-init api don't match
\ TODO make init take cpar, +dma-en takes cmar??
\ TODO add i2c>buf-dma for i2c2
\ TODO can only run 1 dma at a time because of ISR
\       irq set <channel> bit in dma.complete
\       -dma disables any channels set

[ifndef] DMA1      $40020000 constant DMA1 [then]

0 constant DMA:RX
1 constant DMA:TX

DMA1       constant DMA1-ISR
DMA1 4   + constant DMA1-IFCR
DMA1 $A8 + constant DMA1-CSELR

DMA1
dup $08 + constant DMA1-CCR
dup $0c + constant DMA1-CNDTR
dup $10 + constant DMA1-CPAR
dup $14 + constant DMA1-CMAR
drop

1 constant DMA1:MEM-CHAN                    \ dma memory channel
2 constant DMA1:SPI1-RX-CHAN                \ dma spi1 rx channel
3 constant DMA1:SPI1-TX-CHAN                \ dma spi1 tx channel
4 constant DMA1:SPI2-RX-CHAN                \ dma spi2 rx channel
5 constant DMA1:SPI2-TX-CHAN                \ dma spi2 tx channel
4 constant DMA1:I2C2-TX-CHAN                \ dma i2c2 tx channel
5 constant DMA1:I2C2-RX-CHAN                \ dma i2c2 rx channel
6 constant DMA1:I2C1-TX-CHAN                \ dma i2c1 tx channel
7 constant DMA1:I2C1-RX-CHAN                \ dma i2c1 rx channel

\ index into channel
0 constant DMA:CPAR                         \ set dma channel stream
1 constant DMA:IRQ-POS                      \ irq priority position in nvic
2 constant DMA:IRQ-XT                       \ irq handler
3 constant DMA:DIR                          \ dma direction
4 constant DMA:EN-XT                        \ enable dma xt

5 constant DMA:CONF-LEN                     \ number of channel conf elements

false variable dma.complete
0     variable dma.error

: SPI1_CR2_RXDMAEN   %1 0  lshift SPI1-CR2 bis! ;  \ Rx buffer DMA enable
: SPI1_CR2_TXDMAEN   %1 1  lshift SPI1-CR2 bis! ;  \ Tx buffer DMA enable
: SPI2_CR2_RXDMAEN   %1 0  lshift SPI2-CR2 bis! ;  \ Rx buffer DMA enable
: SPI2_CR2_TXDMAEN   %1 1  lshift SPI2-CR2 bis! ;  \ Tx buffer DMA enable
: I2C2_CR1_TXDMAEN   %1 14 lshift I2C2-CR1 bis! ;  \ DMA Tx requests  enable
: I2C2_CR1_RXDMAEN   %1 15 lshift I2C2-CR1 bis! ;  \ DMA Rx requests  enable
: I2C1_CR1_TXDMAEN   %1 14 lshift I2C1-CR1 bis! ;  \ DMA Tx requests  enable
: I2C1_CR1_RXDMAEN   %1 15 lshift I2C1-CR1 bis! ;  \ DMA Rx requests  enable

\ dma channel configurations
create DMA:CHAN-CONF
\    reg   , irq, irq xt      , dir    , enable xt
 0         , 11 , irq-dma_ch1 , DMA:RX , ['] nop ,
 SPI1-DR   , 12 , irq-dma_ch2 , DMA:RX , ['] SPI1_CR2_RXDMAEN ,
 SPI1-DR   , 13 , irq-dma_ch3 , DMA:TX , ['] SPI1_CR2_TXDMAEN ,
 SPI2-DR   , 14 , irq-dma_ch4 , DMA:RX , ['] SPI2_CR2_RXDMAEN ,
 SPI2-DR   , 15 , irq-dma_ch5 , DMA:TX , ['] SPI2_CR2_TXDMAEN ,
 I2C2-RXDR , 14 , irq-dma_ch4 , DMA:TX , ['] I2C2_CR1_TXDMAEN ,
 I2C2-TXDR , 15 , irq-dma_ch5 , DMA:RX , ['] I2C2_CR1_RXDMAEN ,
 I2C1-TXDR , 16 , irq-dma_ch6 , DMA:TX , ['] I2C1_CR1_TXDMAEN ,
 I2C1-TXDR , 17 , irq-dma_ch7 , DMA:RX , ['] I2C1_CR1_RXDMAEN ,

\ TODO replace with better
: dma-conf ( ndx chan -- )
  \ index into DMA:CHAN-CONF
  ( chan ) case
    DMA1:MEM-CHAN     of 0 endof
    DMA1:SPI1-RX-CHAN of 1 endof
    DMA1:SPI1-TX-CHAN of 2 endof
    DMA1:SPI2-RX-CHAN of 3 endof
    DMA1:SPI2-TX-CHAN of 4 endof
    DMA1:I2C2-TX-CHAN of 5 endof
    DMA1:I2C2-RX-CHAN of 6 endof
    DMA1:I2C1-TX-CHAN of 7 endof
    DMA1:I2C1-RX-CHAN of 8 endof
  endcase
  \ pick desired elem
  DMA:CONF-LEN *
  DMA:CHAN-CONF ( row-ndx ) swap cells +
                ( col-ndx ) swap cells + @
  ;
: dma-reg ( reg chan -- addr ) ( reg ) 20 swap ( channel ) 1- * + ;

: -dma ( chan -- ) 0 bit DMA1-CCR rot ( chan ) dma-reg bic! ; \ inline ;
: -dma-mem ( -- ) DMA1:MEM-CHAN -dma ; \ inline ;
: -dma-spi1 ( -- )
  DMA1:SPI1-TX-CHAN -dma
  DMA1:SPI1-RX-CHAN -dma
  \ inline
  ;
: -dma-spi2 ( -- )
  DMA1:SPI2-TX-CHAN -dma
  DMA1:SPI2-RX-CHAN -dma
  \ inline
  ;
: -dma-i2c ( -- )
  DMA1:I2C1-TX-CHAN -dma
  DMA1:I2C1-RX-CHAN -dma
  \ inline
  ;
: +dma-en ( n chan -- )
  tuck
  0 dma.error !
  false dma.complete !
  ( n ) DMA1-CNDTR swap ( chan ) dma-reg    !               \ bytes to transfer
  0 bit DMA1-CCR    rot ( chan ) dma-reg bis!               \ enable dma
  \ inline
  ;
: +dma ( addr n chan -- )
  >r swap
  ( addr ) DMA1-CMAR r@ ( chan ) dma-reg !
  ( n ) r@ ( chan )  +dma-en

  \ enable chan dma
  DMA:EN-XT r> dma-conf ( dmaen-xt ) execute
  ;

: dma-wait ( -- ) begin dma.complete @ not while yield repeat ;
\ wait for dma to complete then wait for i2c to finish
: dma-i2c-wait ( -- ) dma-wait begin 6 bit I2C1-ISR bit@ until ;

: dma-irq-handler ( -- )
  $8888888 DMA1-ISR @ and dma.error !                     \ write error channel
  DMA1-ISR @ $1111111 and DMA1-IFCR bis!                  \ clear irq flag
  true dma.complete !
  ;
: dma-irq! ( chan -- )
  \ for the channel get dma channel in NVIC and irq handler address
  DMA:IRQ-XT over ( chan ) dma-conf ( irq-handler )
  ['] dma-irq-handler swap ( irq-handler ) !

  dup %1010 DMA1-CCR rot ( chan ) dma-reg bis!

  \ enable interrupt on dma1
  DMA:IRQ-POS swap ( chan ) dma-conf ( irq-pos )

  ( irq-pos ) nvic!
  ;
\ NOTE: ensure spi rx is setup before tx in master mode
: dma-init ( irq? chan -- )
  0 bit RCC-AHBENR  bis!                            \ set DMAEN clock enable

  >r
  \ reset dma config
  $7FFF DMA1-CCR r@ ( chan ) dma-reg bic!

  \ add interrupt handler
  ( irq? ) if r@ ( chan ) dma-irq! then

  \ setup memory and peripheral data registers
  \ ( addr ) DMA1-CMAR r@ ( chan ) dma-reg !
  DMA:CPAR r@ ( chan ) dma-conf
  ( cpar )  DMA1-CPAR r@ ( chan ) dma-reg !

  \ channels dir
  DMA:DIR r@ ( chan ) dma-conf
  ( dir ) 4 lshift %10000000 or DMA1-CCR r@ ( chan ) dma-reg bis!
  rdrop
  ;
: dma-mem-init ( to-addr from-addr -- )
  true DMA1:MEM-CHAN dma-init

  \ TODO move to +dma-en
  ( to )   DMA1-CPAR DMA1:MEM-CHAN dma-reg !
  ( from ) DMA1-CMAR DMA1:MEM-CHAN dma-reg !
  %100000001000000 DMA1-CCR DMA1:MEM-CHAN dma-reg bis!  \ m2m, pinc
  ;

\ --------------------------------------------------
\   SPI DMA utils
\ --------------------------------------------------

0 variable spi.tx

\ TODO use channel to get SPIn-SR
: spi-wait ( reg -- )
  begin SPI:RXNE bit over bit@ not while yield repeat drop ;
: spi1-wait ( -- ) SPI1-SR spi-wait ;
: spi2-wait ( -- ) SPI2-SR spi-wait ;

\ master drives read on spi by writing `reg` data n times
: spi2>buf-dma ( addr n reg -- )
  \ disable minc for tx
  7 bit DMA1-CCR DMA1:SPI2-TX-CHAN dma-reg bic!

  ( reg ) spi.tx !
  tuck
  +spi
	\ send command byte and discard the initial value
	spi.tx @ >spi
  \ enable tx + rx transfer
  ( addr )    ( n ) DMA1:SPI2-RX-CHAN +dma
  spi.tx swap ( n ) DMA1:SPI2-TX-CHAN +dma dma-wait
  -dma-spi2
  -spi

  7 bit DMA1-CCR DMA1:SPI2-TX-CHAN dma-reg bis!
  ;
: buf>spi2-dma ( addr n reg -- )
  +spi
  ( reg ) >spi
  ( addr n ) DMA1:SPI2-TX-CHAN +dma dma-wait
  -dma-spi2
  -spi
  ;
: spi1>buf-dma ( addr n reg -- ) spi>buf-dma ; \ -dma-spi1 -spi ;
\ : spi2>buf-dma ( addr n reg -- ) spi>buf-dma ; \ -dma-spi2 -spi ;
: buf>spi1-dma ( addr n reg -- ) buf>spi-dma ; \ -dma-spi1 -spi ;
\ : buf>spi2-dma ( addr n reg -- ) buf>spi-dma ; \ -dma-spi2 -spi ;

\ --------------------------------------------------
\   I2C DMA utils
\ --------------------------------------------------

: buf>i2c-dma ( addr n -- )
  tuck
  ( addr n ) DMA1:I2C1-TX-CHAN +dma
  ( n ) i2c-setn 0 i2c-start dma-i2c-wait -dma-i2c
  ;
: i2c>buf-dma ( addr n -- )
  tuck
  ( addr n ) DMA1:I2C1-RX-CHAN +dma
  ( n ) i2c-setn 1 i2c-start dma-i2c-wait -dma-i2c
  ;

: i2c-xfer-dma ( u -- nak )
  +i2c

  i2c.ptr @ i2c.buf -
  ( n ) ?dup if i2c.buf swap buf>i2c-dma then          \ tx>0
  ( u ) ?dup if i2c.buf swap i2c>buf-dma then          \ rx>0

  4 bit I2C1-ISR bit@ 0<>           \ NAKF
  -i2c
  ;

\ ( dma end, size: ) here dup hex. swap - .
compiletoram? not [if]  cornerstone <<<dma>>> [then]
