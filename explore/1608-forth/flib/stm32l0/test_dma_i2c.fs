
include ../../../../embello/explore/1608-forth/flib/stm32l0/dma.fs
\ include ../../../../embello/explore/1608-forth/flib/stm32l0/i2c.fs

\ ------------------- utils -------------------
40   variable count
25 buffer: buf1
$77 constant bme:addr

: reset ( -- ) count @ 0 do 0 i2c.buf i + c! loop ;
: show ( buf -- ) ." -> " count @ . count @ 0 do dup i + c@ . loop drop ;

: bme-reset ( -- ) \ software reset of the bme680
  bme:addr i2c-addr
  $E0 >i2c $B6 >i2c
  0 i2c-xfer drop
  ;

\ ------------------- add to bme680.fs -------------------

: bme-rd ( addr n reg -- )
  bme:addr i2c-addr
	+i2c
  ( reg ) >i2c
  ( addr n ) i2c>buf-dma
  -i2c
  ;

\ ------------------- testing -------------------

: i2c-test-init
  led-off
  i2c-init bme-reset
  reset

  true DMA1:I2C-RX-CHAN dma-init
  true DMA1:I2C-TX-CHAN dma-init
  ;

: i2c-test
  bme:addr i2c-addr reset
  $d0 *i2c! 1 i2c-xfer  -i2c
  cr i2c.buf show

  bme:addr i2c-addr reset
  $d0 +i2c >i2c i2c> -i2c
  cr .

  bme:addr i2c-addr reset
  i2c.buf 25 $89 ( $D0 ) bme-rd
  cr i2c.buf show

  bme:addr i2c-addr reset
  $D0 i2c.buf c!
  +i2c i2c.buf 1 buf>i2c-dma i2c> -i2c
  cr .

  bme:addr i2c-addr reset
  $89 ( $D0 ) *i2c! 25 i2c-xfer-dma
  cr i2c.buf show

  bme:addr i2c-addr reset
  $89 ( $d0 ) *i2c! 25 i2c-xfer
  cr i2c.buf show

  bme:addr i2c-addr reset
  $89 ( $D0 ) *i2c! 25 i2c-xfer-dma
  cr i2c.buf show

  \ check output
  \ bme:addr i2c-addr reset
  \ buf1 25 $89 bme-rd drop
  \ cr buf1 show
  ;

i2c-test-init
i2c-test
i2c-test

i2c-test-init
i2c-test
i2c-test
