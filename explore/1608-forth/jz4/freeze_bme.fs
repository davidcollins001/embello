\ frozen application, this runs tests and wipes to a clean slate if they pass
compiletoflash

include always.fs
include board.fs
include core.fs

include ../flib/spi/rf69.fs
include ../flib/any/datagram.fs

include ../flib/i2c/bme680.fs
include ../flib/any/clime.fs

\ run tests, even when connected (especially so, in fact!)
\ : init init ( unattended ) blip ;
\ : init ( -- ) init unattended main ;
