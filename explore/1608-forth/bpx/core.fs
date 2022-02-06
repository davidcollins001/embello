\ core definitions

\ <<<board>>>
compiletoflash
( core start: ) here hex.

include ../flib/any/timed.fs

include ../flib/stm32f1/spi.fs
include ../flib/stm32f1/i2c.fs
\ include ../flib/stm32f1/dma.fs

include ../flib/any/varint.fs

cornerstone <<<core>>>
hello
