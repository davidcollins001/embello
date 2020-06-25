\ compiletoflash
\ ( datagram start: ) here dup hex.

\ TODO find where to put dg.seq# in packet
\ TODO pass dg.seq# on the stack

\ datagram packet format:
\   [len, to, from, flags/seq, data, ...]

\ --------------------------------------------------
\  Configuration
\ --------------------------------------------------

  3 constant DG:RETRIES
 40 constant DG:TIMEOUT  \ ms
  3 constant DG:HDR#     \ doesn't include length byte

\ TODO use packet object eg, functions and mem that points to:
 rf.len     constant dg.len
 rf.buf 1+  constant dg.addr
 rf.buf 2+  constant dg.from
 rf.buf 3 + constant dg.flags
 rf.buf 4 + constant dg.data

  7 variable dg.seq#
RF:MAXDATA buffer: dg.buf

\ --------------------------------------------------
\  Internal Helpers
\ --------------------------------------------------

: yield ( -- ) ;

: dg-seq@ ( -- n ) dg.seq# @ ;
: dg-seq! ( n -- ) dg.seq# ! ;
\ : dg-seq! ( -- ) 1 dg.seq# +!  dg.seq# @ dg.data c! ;
\ : dg-seq! ( -- ) dg.seq# @ dg.data c! ;

\ TODO exponential backoff timeout?
: dg-timeout? ( time -- ) millis swap - DG:TIMEOUT > ;

\ TODO dg.seq# on stack?
: dg-ack? ( from -- ? )
  \ TODO cope with new message/lost ack
  \ re-ack - new message, ack might have gotten lost
  \ seen? if dg-send-ack then
  \ discard unknown message
  \ dg-send-ack
  ( from ) dg.from c@ =  dg.seq# @ dg.data c@ = and
  ;

: dg-recv-ack ( from -- ? )
  millis
  1 0 do
    rf-recv if
      dg.buf rf-recv-done2
      over dg-ack? if ." ack'd " true leave then
    then
    yield
    dup dg-timeout? if false leave then
  0 +loop                                     \ loop until timeout/ack
  nip nip
  rf-idle-mode!                               \ put radio to sleep
  ;

: dg-set-header ( len addr -- )
  ( addr )          dg.addr c!                \ set header
  rf.nodeid @       dg.from c!
  ( len ) DG:HDR# + dg.len c!
  0 dg.flags c!                               \ set flags
  ;

: dg-send-ack ( seq addr -- )
  1 swap ( addr ) .v dg-set-header
  dg.seq# @ dg.data c!                        \ copy user data to radio buffer
  0 dg.data 1+ c!                             \ empty rest of ack data
  drop \ dg.data c!
  rf.buf DG:HDR# 1+ rf-send
  drop
  ;

: dg-wait-sent ( -- )                         \ wait until the radio can send
  begin rf-sending? while yield repeat
  ;

\ --------------------------------------------------
\   External API
\ --------------------------------------------------

: dg-send-to ( buffer len addr -- n )         \ send out one packet for node
  2dup
  ( len addr ) dg-set-header
  -rot ( 62 min ) tuck
  \ TODO bound check
  ( buffer len ) dg.data swap move            \ copy user data to radio buffer
  DG:HDR# 1+ +                                \ add header len to packet len

  DG:RETRIES 0 do
    begin dup rf.buf  swap ( len ) rf-send while yield repeat
    dg-wait-sent

    over dg-recv-ack if rot ( true ) leave then
  loop
  2drop
  \ TODO return success or failure
  rf-idle-mode!
  ;

: dg-recv ( -- addr n )
    rf-recv if
      dg.buf rf-recv-done2
      dg-seq@ dg.from c@ dg-send-ack
      dg-wait-sent
      true
    else
      false
    then
    ;

: dg-show-packet ( -- )
  ." RF69 " rf-info space ." ( " rf.rssi @ . ." )" space
  RF:CONF rf@ 7 bit and 0= if            \ check if payload is fixed/variable
    RF:PAYLOAD_LEN rf@
  else
    dg.buf c@ 1+
    dup h.2 space ." : "
  then
  \ 11 debug rf.fixed-pkt# @ .
  0 do dg.buf i + c@ h.2 space loop cr
  ;

: dg-listen ( -- )
  cr
  begin
    dg-recv if
      dg-show-packet
    else
      yield
    then
  key? until
  rf-idle-mode!
  ;

\ ( rf12demo end, size: ) here dup hex. swap - .
compiletoram? not [if]  cornerstone <<<datagram>>> compiletoram [then]
