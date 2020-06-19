\ core libraries

<<<board>>>
compiletoflash
( core start: ) here dup hex.

include ../flib/i2c/ssd1306.fs
include ../flib/mecrisp/graphics.fs
include ../flib/any/digits.fs
include ../flib/mecrisp/quotation.fs
include ../flib/mecrisp/multi.fs
include ../flib/any/varint.fs
cornerstone <<<core>>>

include ../flib/spi/rf69.fs

( rf69 end, size: ) here dup hex. swap - .
compiletoram
