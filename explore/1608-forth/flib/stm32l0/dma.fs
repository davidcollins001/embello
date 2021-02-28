
\ TODO dma-mem-init and dma-spi-init api don't match
\ TODO make init take cpar, +dma-en takes cmar??

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
2 constant DMA1:SPI-RX-CHAN                 \ dma spi rx channel
3 constant DMA1:SPI-TX-CHAN                 \ dma spi tx channel
6 constant DMA1:I2C-TX-CHAN                 \ dma i2c tx channel
7 constant DMA1:I2C-RX-CHAN                 \ dma i2c rx channel

\ index into channel
0 constant DMA:STREAM                       \ set dma channel stream
1 constant DMA:CPAR                         \ set dma channel stream
2 constant DMA:IRQ-POS                      \ irq priority position in nvic
3 constant DMA:IRQ-XT                       \ irq handler
4 constant DMA:DIR                          \ dma direction
5 constant DMA:EN-XT                        \ enable dma xt

6 constant DMA:CONF-SZ                      \ number of channel conf items

false variable dma.complete
0     variable dma.error

: SPI1_CR2_RXDMAEN   %1 0 lshift SPI1-CR2 bis! ;  \ Rx buffer DMA enable
: SPI1_CR2_TXDMAEN   %1 1 lshift SPI1-CR2 bis! ;  \ Tx buffer DMA enable
: I2C1_CR1_TXDMAEN   %1 14 lshift I2C1-CR1 bis! ;  \ DMA Tx requests  enable
: I2C1_CR1_RXDMAEN   %1 15 lshift I2C1-CR1 bis! ;  \ DMA Rx requests  enable

\ dma channel configurations
create DMA:CHAN-CONF
\ stream,    reg   , irq, irq xt     , dir    , enable xt
     0 , 0         ,  9 , irq-dma1   , DMA:RX , ['] nop ,
 %0001 , SPI1-DR   , 10 , irq-dma2_3 , DMA:RX , ['] SPI1_CR2_RXDMAEN ,
 %0001 , SPI1-DR   , 10 , irq-dma2_3 , DMA:TX , ['] SPI1_CR2_TXDMAEN ,
 %0110 , I2C1-RXDR , 11 , irq-dma4_7 , DMA:RX , ['] I2C1_CR1_RXDMAEN ,
 %0110 , I2C1-TXDR , 11 , irq-dma4_7 , DMA:TX , ['] I2C1_CR1_TXDMAEN ,

\ TODO replace with better
: dma-conf ( ndx chan -- )
  ( chan ) case
    DMA1:MEM-CHAN    of 0 endof
    DMA1:SPI-RX-CHAN of 1 endof
    DMA1:SPI-TX-CHAN of 2 endof
    DMA1:I2C-RX-CHAN of 3 endof
    DMA1:I2C-TX-CHAN of 4 endof
  endcase
  \ pick desired elem
  DMA:CONF-SZ *
  DMA:CHAN-CONF ( row-ndx ) swap cells +
                ( col-ndx ) swap cells + @
  ;
: dma-reg ( reg chan -- addr ) ( reg ) 20 swap ( channel ) 1- * + ;

: -dma ( chan -- ) 0 bit DMA1-CCR rot ( chan ) dma-reg bic! inline ;
: -dma-mem ( -- ) DMA1:MEM-CHAN -dma inline ;
: -dma-spi ( -- )
  DMA1:SPI-TX-CHAN -dma
  DMA1:SPI-RX-CHAN -dma
  inline
  ;
: -dma-i2c ( -- )
  DMA1:I2C-TX-CHAN -dma
  DMA1:I2C-RX-CHAN -dma
  inline
  ;
: +dma-en ( n chan -- )
  tuck
  0 dma.error !
  false dma.complete !
  ( n ) DMA1-CNDTR swap ( chan ) dma-reg    !               \ bytes to transfer
  0 bit DMA1-CCR    rot ( chan ) dma-reg bis!               \ enable dma
  inline
  ;
: +dma ( addr n chan -- )
  >r swap
  ( addr ) DMA1-CMAR r@ ( chan ) dma-reg !
  ( n ) r@ ( chan ) +dma-en

  \ enable chan dma
  DMA:EN-XT r> dma-conf ( dmaen-xt ) execute
  ;

: dma-wait ( -- ) begin dma.complete @ not while yield repeat ;
\ wait for dma to complete then wait for i2c to finish
: dma-i2c-wait ( -- ) dma-wait begin 6 bit I2C1-ISR bit@ until ;

: dma-irq-exit ( -- ) DMA1-ISR @ $1111111 and DMA1-IFCR bis! inline ;  \ clear irq flag
: dma-irq-handler ( -- )
  $8888888 DMA1-ISR @ and dma.error !                     \ write error channel
  true dma.complete !
  dma-irq-exit
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

  \ enable dma stream
  DMA:STREAM r@ ( chan ) dma-conf r@ ( chan ) 1- 4 * lshift DMA1-CSELR bis!
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

: spi-wait ( -- )
  begin SPI:RXNE bit SPI1-SR bit@ not while yield repeat
  ;

\ master drives read on spi by writing `reg` data n times
: spi>buf-dma ( addr n reg -- )
  \ disable minc for tx
  7 bit DMA1-CCR DMA1:SPI-TX-CHAN dma-reg bic!

  ( reg ) spi.tx !
  tuck
  +spi
	\ send command byte and discard the initial value
	spi.tx @ >spi
  \ enable tx + rx transfer
  ( addr )    ( n ) DMA1:SPI-RX-CHAN  +dma
  spi.tx swap ( n ) DMA1:SPI-TX-CHAN  +dma dma-wait
  -dma-spi
  -spi

  7 bit DMA1-CCR DMA1:SPI-TX-CHAN dma-reg bis!
  ;
: buf>spi-dma ( addr n reg -- )
  +spi
  ( reg ) >spi
  ( addr n ) DMA1:SPI-TX-CHAN +dma dma-wait
  -dma-spi
  -spi
  ;

\ --------------------------------------------------
\   I2C DMA utils
\ --------------------------------------------------

: buf>i2c-dma ( addr n -- )
  tuck
  ( addr n ) DMA1:I2C-TX-CHAN +dma
  ( n ) i2c-setn 0 i2c-start dma-i2c-wait -dma-i2c
  ;
: i2c>buf-dma ( addr n -- )
  tuck
  ( addr n ) DMA1:I2C-RX-CHAN +dma
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
