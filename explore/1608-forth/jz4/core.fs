\ core libraries

compiletoflash
( core start: ) here dup hex.

PA15 variable ssel  \ can be changed at run time
PB3 constant SCLK
PB4 constant MISO
PB5 constant MOSI

include ../flib/stm32l0/spi.fs
include ../flib/stm32l0/i2c.fs
include ../flib/stm32l0/dma.fs

include ../flib/i2c/ssd1306.fs
include ../flib/mecrisp/graphics.fs
include ../flib/any/digits.fs
include ../flib/any/varint.fs

( board end, size: ) here dup hex. swap - .
compiletoram? not [if]  cornerstone <<<core>>> [then]
