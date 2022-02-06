\ frozen application, this runs tests and wipes to a clean slate if they pass

\ reflash from laptop
\ sudo python2 Downloads/stm32loader.py -e -p /dev/cuaU0 -w -v -b 9600 \
\   tmp/mecrisp-stellaris-2.5.6/mecrisp-stellaris-source/stm32f103/mecrisp-stellaris-stm32f103.bin
\ (cd ~/tmp/stm32loader/build/lib/stm32loader;
\ python main.py -e -p /dev/ttyU0 -w -v -b 112500
\ 	~/tmp/mecrisp-stellaris-2.5.6/stm32f103/mecrisp-stellaris-stm32f103.bin)
\ Downloads/stm32flash-0.6/stm32flash -v -b 115200 -w
\	tmp/mecrisp-stellaris-2.5.6/stm32f103/mecrisp-stellaris-stm32f103.hex /dev/ttyU0
\ python -m serial.tools.miniterm --raw /dev/cuaU0 115200

compiletoflash

\ : init ( -- ) ;

include always.fs
include board.fs

\ include ../flib/spi/rf69.fs
\ include ../flib/any/datagram.fs

\ run tests, even when connected (especially so, in fact!)
\ : init init ( unattended ) blip ;
