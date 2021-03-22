\ board definitions

\ eraseflash
compiletoflash
( board start: ) here dup hex.

include ../flib/mecrisp/calltrace.fs
include ../flib/mecrisp/cond.fs
include ../flib/mecrisp/quotation.fs
include ../flib/mecrisp/hexdump.fs
include ../flib/stm32f1/clock.fs
include ../flib/stm32f1/io.fs
include ../flib/pkg/pins64.fs
include ../flib/stm32f1/hal.fs
include ../flib/stm32f1/timer.fs
include ../flib/stm32f1/pwm.fs
include ../flib/stm32f1/adc.fs
include ../flib/stm32f1/rtc.fs
include ../flib/mecrisp/multi.fs

[ifndef] LED  PC13 constant LED  [then]

: led-on  LED ioc! ;
: led-off  LED ios! ;

: nvic! ( irq-pos -- )                                      \ enable interrupt
      dup ( irq-pos ) bit NVIC-EN0R   bis!
  $C over ( irq-pos ) 4 mod 4 * lshift
          ( irq-pos ) 4 / cells NVIC-IPR1 + bis!
  ;

: hello ( -- ) flash-kb . ." KB <bpx> " hwid hex.
  $10000 compiletoflash here -  flashvar-here compiletoram here -
  ." ram/flash: " . . ." free " ;

: init ( -- )  \ board initialisation
  init  \ this is essential to start up USB comms!
  ['] ct-irq irq-fault !  \ show call trace in unhandled exceptions
  $00 hex.empty !  \ empty flash shows up as $00 iso $FF on these chips
  jtag-deinit  \ disable JTAG, we only need SWD
  OMODE-PP LED io-mode!
  1000 systick-hz
  hello ." ok." cr
;

: rx-connected? ( -- f )  \ true if RX is connected (and idle)
  IMODE-PULL PA9 io-mode!  PA9 io@ 0<>  OMODE-AF-PP PA9 io-mode!
  dup if 1 ms serial-key? if serial-key drop then then \ flush any input noise
  ;

: fake-key? ( -- f )  \ check for RX pin being pulled high
  rx-connected? if reset then false ;

\ unattended quits to the interpreter if the RX pin is connected, not floating
\ else it replaces the key? hook with a test to keep checking for RX reconnect
\ if so, it will reset to end up in the interpreter on the next startup
\ for use with a turnkey app in flash, i.e. ": init init unattended ... ;"

: unattended
  rx-connected? if quit then \ return to command prompt
  ['] fake-key? hook-key? ! ;

cornerstone <<<board>>>
hello
