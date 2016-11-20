\ Tiny RF node

\ define some missing constants
4 constant io-ports  \ A..D
RCC $18 + constant RCC-APB2ENR

include ../mlib/hexdump.fs
include ../flib/stm32f1/io.fs
include ../flib/pkg/pins48.fs
include ../flib/stm32f1/spi.fs
include ../flib/spi/rf69.fs

6 rf69.group !
rf69-listen
