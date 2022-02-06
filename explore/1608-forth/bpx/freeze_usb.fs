\ frozen application, this runs tests and wipes to a clean slate if they pass

\ reflash from laptop
\ sudo python2 Downloads/stm32loader.py -e -p /dev/cuaU0 -w -v -b 9600 \
\   tmp/mecrisp-stellaris-2.5.6/mecrisp-stellaris-source/stm32f103/mecrisp-stellaris-stm32f103.bin
\ python -m serial.tools.miniterm --raw /dev/cuaU0 115200

true constant USB-COMM

compiletoflash

include always.fs

\ driver to run the forth shell over USB
include ../suf/hal-stm32f1.fs
include ../flib/any/ring.fs
include ../flib/stm32f1/usb.fs

: init  ( -- )
  $3D RCC-APB2ENR !  \ enable AFIO and GPIOA..D clocks
  72MHz  \ this is required for USB use

  \ board-specific way to enable USB
  flash-kb 128 = if
    \ it looks like it's a HyTiny ...
    %1111 $40010800 bic!  \ PA0: output, push-pull, 2 MHz
    %0010 $40010800 bis!
    0 bit $4001080C bis!  \ set PA0 high
    100000 0 do loop
    0 bit $4001080C bic!  \ set PA0 low
  else
    \ nope, it must be a Blue Pill or similar ...
    %1111 16 lshift $40010804 bic!  \ PA12: output, push-pull, 2 MHz
    %0010 16 lshift $40010804 bis!  \ ... this affects CRH iso CRL
    12 bit $4001080C bic!  \ set PA12 low
    100000 0 do loop
    12 bit $4001080C bis!  \ set PA12 high
  then

  usb-io  \ switch to USB as console
;

( usb end: ) here hex.
cornerstone eraseflash

include board.fs
include core.fs

\ include ../flib/spi/rf69.fs
\ include ../flib/any/datagram.fs

\ run tests, even when connected (especially so, in fact!)
\ : init init ( unattended ) blip ;
