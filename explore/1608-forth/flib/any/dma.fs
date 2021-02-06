\ <<<rf69>>>
\ compiletoflash
\ $E000E100  constant NVIC-EN0R           \ IRQ 0 to 31 Set Enable Register
\ NVIC-EN0R $304 + constant NVIC-IPR1     \ interrupt priority
\ : yield ( -- ) sleep ;

\ TODO dma-mem-init and dma-spi-init api don't match
\ TODO make init take cpar, +dma takes cmar??

[ifndef] DMA1      $40020000 constant DMA1 [then]

0 constant DMA:RX
1 constant DMA:TX

\ 0 constant DMA:CHAN-EN
\ 1 constant DMA:CHAN-DIR
\ 2 constant DMA:CHAN-CPAR
\ 3 constant DMA:CHAN-IRQ-POS
\ 4 constant DMA:CHAN-IRQ-XT

1 constant DMA1:MEM-CHAN                   \ dma memory channel
2 constant DMA1:SPI-RX-CHAN                \ dma spi rx channel
3 constant DMA1:SPI-TX-CHAN                \ dma spi tx channel
6 constant DMA1:I2C-TX-CHAN                \ dma i2c tx channel
7 constant DMA1:I2C-RX-CHAN                \ dma i2c rx channel

DMA1       constant DMA1-ISR
DMA1 4   + constant DMA1-IFCR
DMA1 $A8 + constant DMA1-CSELR

DMA1
dup $08 + constant DMA1-CCR
dup $0c + constant DMA1-CNDTR
dup $10 + constant DMA1-CPAR
dup $14 + constant DMA1-CMAR
drop

false variable dma.complete
0     variable dma.error
0     variable '-dma

: SPI1_CR2_RXDMAEN   %1 0 lshift SPI1-CR2 bis! ;  \ Rx buffer DMA enable
: SPI1_CR2_TXDMAEN   %1 1 lshift SPI1-CR2 bis! ;  \ Tx buffer DMA enable
: I2C1_CR1_TXDMAEN   %1 14 lshift I2C1-CR1 bis! ;  \ DMA Tx requests  enable
: I2C1_CR1_RXDMAEN   %1 15 lshift I2C1-CR1 bis! ;  \ DMA Rx requests  enable

\ : dma-reg ( reg chan -- addr ) ( reg ) 20 swap ( channel ) 1- * + ;
\ : dma-chan ( dma-en-xt dir data-reg irq-pos irq-xt -- )
  \ create , , , , ,
  \ does> ( section -- data )
  \ swap cells + @
  \ ;

\ \ store channel configurations
\ \ irq-dma2_3 10 SPI1-DR   DMA:RX ' SPI1_CR2_RXDMAEN dma-chan dma1.spi-rx-chan
\ \ irq-dma2_3 10 SPI1-DR   DMA:TX ' SPI1_CR2_TXDMAEN dma-chan dma1.spi-tx-chan
\ irq-dma4_7 11 I2C1-RXDR DMA:RX ' I2C1_CR1_RXDMAEN dma-chan dma1.i2c-rx-chan
\ irq-dma4_7 11 I2C1-TXDR DMA:TX ' I2C1_CR1_TXDMAEN dma-chan dma1.i2c-tx-chan
: dma-conf ( ndx chan -- )
  ( chan ) case
    DMA1:SPI-RX-CHAN of 10 irq-dma2_3  DMA:RX ['] SPI1_CR2_RXDMAEN endof
    DMA1:SPI-TX-CHAN of 10 irq-dma2_3  DMA:TX ['] SPI1_CR2_TXDMAEN endof
    DMA1:I2C-RX-CHAN of 11 irq-dma4_7  DMA:RX ['] I2C1_CR1_RXDMAEN endof
    DMA1:I2C-TX-CHAN of 11 irq-dma4_7  DMA:TX ['] I2C1_CR1_TXDMAEN endof
  endcase
  \ take elem
  4 pick ( ndx ) 0 = if nip nip nip nip else
  4 pick ( ndx ) 1 = if drop nip nip nip else
  4 pick ( ndx ) 2 = if drop drop nip nip else
  4 pick ( ndx ) 3 = if drop drop drop nip else
  then then then then
  ;

: -dma ( chan -- ) 0 bit DMA1-CCR rot ( chan ) dma-reg bic! ; inline
: -dma-mem ( -- ) DMA1:MEM-CHAN -dma ; inline
: -dma-spi ( -- )
  DMA1:SPI-TX-CHAN -dma
  DMA1:SPI-RX-CHAN -dma
  ; inline
\ TODO check which channel is complete
: -dma-i2c ( -- )
  DMA1:I2C-TX-CHAN -dma
  DMA1:I2C-RX-CHAN -dma
  ; inline
: +dma ( n chan -- )
  tuck
  0 dma.error !
  false dma.complete !
  ( n ) DMA1-CNDTR swap ( chan ) dma-reg    !               \ bytes to transfer
  0 bit DMA1-CCR    rot ( chan ) dma-reg bis!               \ enable dma
  ;
: +dma-periph ( addr n chan -- )
  >r swap
  ( addr ) DMA1-CMAR r@ ( chan ) dma-reg !
  ( n ) r@ ( chan ) +dma

  \ enable chan spi dma
  0 r> dma-conf ( dmaen ) execute
  ;
\ TODO replace these
: +dma-spi ( addr n chan -- ) +dma-periph ;
: +dma-i2c ( addr n chan -- ) +dma-periph ;

: dma-wait ( -- ) begin dma.complete @ not while yield repeat ;
  \ wait for dma to complete then wait for i2c to finish
: dma-i2c-wait ( -- ) dma-wait begin 6 bit I2C1-ISR bit@ until ;

: nvic! ( irq-pos -- )                                      \ enable interrupt
      dup ( irq-pos ) bit NVIC-EN0R   bis!
  $C over ( irq-pos ) 4 mod 4 * lshift
          ( irq-pos ) 4 / cells NVIC-IPR1 + bis!
  ;
: dma-irq-exit ( -- ) DMA1-ISR @ $1111111 and DMA1-IFCR bis! ; inline  \ clear irq flag
: dma-irq-handler ( -- )
  '-dma @ execute                                         \ disable dma
  $8888888 DMA1-ISR @ and dma.error !                     \ write error channel
  dma-irq-exit
  true dma.complete !
  ." >dma-irq "
  ;
: dma-irq! ( -dma-xt chan -- )
  \ for the channel get dma channel in NVIC and irq handler address
  2 over ( chan ) dma-conf ( irq-handler )
  ['] dma-irq-handler swap ( irq-handler ) !
  swap ( disable-dma-xt ) '-dma !

  dup %1010 DMA1-CCR rot ( chan ) dma-reg bis!

  \ enable interrupt on dma1
  3 swap ( chan ) dma-conf ( irq-pos )
  ( irq-pos ) nvic!
  ;
: dma-mem-init ( from-addr to-addr -- )
  0 bit RCC-AHBENR  bis!                            \ set DMAEN clock enable

  \ TODO move to +dma
  ( to )   DMA1-CPAR DMA1:MEM-CHAN dma-reg !
  ( from ) DMA1-CMAR DMA1:MEM-CHAN dma-reg !
  %100000011011010 DMA1-CCR DMA1:MEM-CHAN dma-reg bis!  \ m2m, minc, dir, irq

  ['] -dma-mem DMA1:MEM-CHAN dma-irq!
  ;
: dma-init ( irq? chan -dma-xt cpar -- )
  0 bit RCC-AHBENR  bis!                            \ set DMAEN clock enable

  swap 2swap ( chan ) >r
  \ reset dma config
  $7FFF DMA1-CCR r@ ( chan ) dma-reg bic!

  \ needed to occasionally get dma to work???
  \ 14 bit dma1-ccr DMA1:SPI-RX-CHAN dma-reg bis!
  \ 14 bit dma1-ccr DMA1:SPI-RX-CHAN dma-reg bic!

  \ add interrupt handler
  ( irq? ) if r@ ( chan ) dma-irq! else drop then

  \ setup memory and peripheral data registers
  \ ( addr ) DMA1-CMAR r@ ( chan ) dma-reg !
  ( cpar )  DMA1-CPAR r@ ( chan ) dma-reg !

  \ config channels - dir minc, no pinc, irq
  1 r@ ( chan ) dma-conf
  ( dir ) 4 lshift %10000000 or DMA1-CCR r@ ( chan ) dma-reg bis!
  rdrop
  ;
\ NOTE: ensure spi rx is setup before tx in master mode
: dma-spi-init ( irq? channel -- )
  tuck
  ( irq? ) ( chan ) ['] -dma-spi SPI1-DR   dma-init

  \ enable spi rx/tx streams
  %0001 swap ( chan ) 1- 4 * lshift DMA1-CSELR bis!
  ;
: dma-i2c-init ( irq? channel -- )
  >r
  r@ ( chan ) case
    DMA1:I2C-RX-CHAN of I2C1-RXDR endof
    DMA1:I2C-TX-CHAN of I2C1-TXDR endof
  endcase
  ( irq? ) r@ swap ( chan ) ['] -dma-i2c swap ( cpar ) dma-init

  \ enable i2c rx/tx streams
  %0110 r> ( chan ) 1- 4 * lshift DMA1-CSELR bis!
  ;

\ : i2c-rd-dma ( n -- )
  \ ." overriding i2c-rd "
  \ dup ( n ) i2c-setn  1 i2c-start

  \ dup ( n ) i2c.buf DMA1:I2C-RX-CHAN +dma-i2c
      \ ( n ) i2c-setn 1 i2c-start

  \ dma-i2c-wait
  \ ;

\ : i2c-wr-dma ( n -- )
  \ ." overriding i2c-wd "
  \ dup ( n )	i2c-setn  0 i2c-start

  \ dup ( n ) i2c.buf DMA1:I2C-TX-CHAN +dma-i2c
      \ ( n ) i2c-setn 0 i2c-start

  \ dma-i2c-wait
  \ ;

\ : i2c-xfer2 ( u -- nak )
  \ 0 bit I2C1-CR1 bis!               \ toggle PE high to enable

  \ i2c.ptr @ i2c.buf - ?dup if
	\ ( n )  i2c-wr-dma   \ tx>0
  \ then
  \ ?dup if
	\ ( n )  i2c-rd-dma   \ rx>0
  \ then

  \ i2c-stop i2c-reset
  \ 4 bit I2C1-ISR bit@ 0<>           \ NAKF
  \ 0 bit I2C1-CR1 bic!               \ toggle PE high to disable
  \ ;

\ ( dma end, size: ) here dup hex. swap - .
compiletoram? not [if]  cornerstone <<<dma>>> [then]
