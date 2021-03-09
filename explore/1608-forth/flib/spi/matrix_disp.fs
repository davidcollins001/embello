
include font8x8.fs

\ https://github.com/wayoda/LedControl/blob/master/src/LedControl.cpp

\ [ifndef] ssel  PA3 variable ssel  [then]  \ can be changed at run time
\ set up bit-banged spi on PA3
PA3 variable ssel
PA5 constant SCLK
PA7 constant MOSI

include ../../../../embello/explore/1608-forth/flib/any/spi-bb.fs


\ //the opcodes for the MAX7221 and MAX7219
 0 constant MD:NOOP
 1 constant MD:DIGIT0
 2 constant MD:DIGIT1
 3 constant MD:DIGIT2
 4 constant MD:DIGIT3
 5 constant MD:DIGIT4
 6 constant MD:DIGIT5
 7 constant MD:DIGIT6
 8 constant MD:DIGIT7
 9 constant MD:DECODEMODE
$A constant MD:INTENSITY
$B constant MD:SCANLIMIT
$C constant MD:SHUTDOWN
$F constant MD:DISPLAYTEST

8    buffer:  md.data
1    variable md.devices#
100  variable md.scroll-ms
true variable md.reverse?

: md! ( data opcode dev -- )
	drop
  \ ( addr ) 2* tuck
     \ ( offset ) md.data + c!
  \ 1+ ( offset ) md.data + c!

  \ send all bytes to display
  \ +spi md.devices# @ 0 do i md.data + c@ >spi loop -spi
  +spi ( opcode ) >spi ( data ) >spi -spi
  ;

\ get ascii font for letter
: ascii ( char -- c-addr ) ASCII:SET swap ( char ) 8 * + ;
: ascii-rev ( char -- c-addr ) REV:8BIT + ;

\ update display with changes to memory
: md-update ( -- )
  \ +spi md.devices# @ 0 do md.data i + c@ i md! loop -spi
  md.devices# @ 0 do
    8 0 do
      \ send opcode/digit and data value
      +spi
      i 1+ >spi md.data i + c@
      md.reverse? @ if ascii-rev c@ then >spi
      -spi
    loop
  loop
  ;

: md-clear ( dev -- ) 8 * md.data 8 0 fill md-update ;
: md-shutdown ( ? dev -- ) swap not MD:SHUTDOWN rot ( dev ) md! ;
: md-off  ( dev -- ) 			 0 MD:SHUTDOWN rot ( dev ) md! ;
: md-on   ( dev -- ) 			 1 MD:SHUTDOWN rot ( dev ) md! ;
: md-test ( ? dev -- ) ( ? ) MD:DISPLAYTEST swap ( dev ) md! ;
: md-scanlimit! ( scan dev -- )      MD:SCANLIMIT swap md! ;
: md-decodemode! ( mode dev -- )     MD:DECODEMODE swap md! ;
: md-intensity! ( intensity dev -- ) MD:INTENSITY swap md! ;

: md-init ( #devs -- )
  ssel @ spi!ssel spi-init

	dup ( # ) md.devices# !
  md.data over ( # ) 8 * 0 fill

	( # ) 0 do
		0 i md-test
		\ 1 i md-shutdown
		i md-on
		0 i md-intensity!
		0 i md-decodemode!
		7 i md-scanlimit!
		i md-clear
	loop
	;

: md-led! ( state col row dev -- )
  ( dev ) md.devices# @ *
  rot ( row ) + md.data +
  swap ( col ) bit swap
  rot ( state ) if cbis! else cbic! then
  md-update
  ;

: md-row! ( value row dev -- )
  ( value row dev ) 8 * md.data + + c!
  md-update
  ;
: md-col! ( value col dev -- )
  \ 8 0 do dup i 0 md.data c! loop drop
  ;

\ TODO sort for n devices
: md-shift-row ( next -- )
  \ shift data across array
  md.data 1+ md.data md.devices# @ 8 * 1- move
  \ ( next ) md.data 7 + c!
  \ ( next ) md.data md.devices# @ 8 + c!
  ( next ) md.data md.devices# @ 8 * 1- + c!
  md-update
  ;
\ scroll ascii chars stored in `addr` across display
: md-scroll ( addr len -- )
  ( len ) 0 do
    dup ( addr ) i + c@
      cr
      8 0 do
        dup ascii i + dup hex. c@ md-shift-row
        md.scroll-ms @ ms
      loop
      drop
  loop
  drop
  ;

: md-char! ( addr dev -- )
  swap ( addr ) ascii swap ( dev ) 8 * md.data + 8 move
  md-update
  ;

: md-digit! ( n dev -- ) swap $30 + swap md-char! ;
