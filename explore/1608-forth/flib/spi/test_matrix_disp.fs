
\ : >spi ;

include matrix_disp.fs

\ \ repeat lines across matrix
\ : run ( xt -- ) begin dup ( xt ) execute key? until drop ;

\ ------------------------------------------------------------------------------

create suck-it-luke
hex
  \ S      u      c      k             i      t
  $53 c, $75 c, $63 c, $6B c, $00 c, $69 c, $74 c, $00 c,
  \ L      u      k      e
  $4C c, $75 c, $6B c, $65 c, $00 c,
decimal

\ ------------------------------------------------------------------------------

\ count down from 9
: test-rows ( -- ) 8 0 do 0 md-clear $ff i 0 md-row! 100 ms loop ;
: test-cols ( -- ) 0 md-clear 8 0 do $ff i 0 md-col! 100 ms loop ;
: test-scroll ( -- ) suck-it-luke 14 md-scroll ;
: test-count-down ( -- ) 10 0 do 9 i - 0 md-digit! 100 ms loop ;
\ display all letters in ascii set
: test-alphabet ( -- ) $7F $20 do i 0 md-char! 100 ms loop ;

\ ------------------------------------------------------------------------------

led-off

1 md-init
100 md.scroll-ms !
test-rows
test-cols
\ test-count-down
test-scroll
test-alphabet
