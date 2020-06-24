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
\ 40 constant DG:TIMEOUT  \ ms
100 constant DG:TIMEOUT  \ ms
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
      rf-recv-done
      over dg-ack? if ." ack'd " true leave then
    then
    yield
    dup dg-timeout? if false leave then
  0 +loop                                     \ loop forever or until timeout/ack
  nip nip
  rf-idle-mode!                               \ put radio to sleep
  ;

: dg-wait-sent ( -- )                         \ wait until the radio can send
  begin rf-sending? while yield repeat
  ;

\ : dg-seq! ( -- ) 1 dg.seq# +!  dg.seq# @ dg.data c! ;
: dg-seq! ( -- ) dg.seq# @ dg.data c! ;
: dg-set-header ( len addr -- )
  ( addr )          dg.addr c!                \ set header
  rf.nodeid @       dg.from c!
  ( len ) DG:HDR# + dg.len c!
  0 dg.flags c!                               \ set flags
  \ dg-seq!                                   \ update/add seq to payload
  ;
: dg-send-to ( buffer len addr -- n )         \ send out one packet for node
  2dup
  ( len addr ) dg-set-header
  -rot ( 62 min ) tuck
  \ TODO bound check
  ( buffer len ) dg.data swap move            \ copy user data to radio buffer
  DG:HDR# +                                   \ add header len to packet len

  DG:RETRIES 0 do
    begin dup rf.buf  swap ( len ) rf-send while yield repeat
    dg-wait-sent

    over dg-recv-ack if rot ( true ) leave then
  loop
  \ nip nip
  2drop
  \ TODO return success or failure
  ;

: dg-send-ack ( seq addr -- )
  dg.data c@ dg.seq# !
  ( len addr ) 1 swap dg-set-header
  ;

: dg-recv ( -- addr n )
  begin
    rf-recv if
      rf-recv-done

      DG:RETRIES 0 do
        \ begin dup rf.buf  swap ( len ) rf-send while yield repeat
        dg-send-ack
        dg-wait-sent

        over dg-recv-ack if rot true leave then
      loop
      2drop

    then
  key? until
  rf-idle-mode!
  ;

\ ( rf12demo end, size: ) here dup hex. swap - .
compiletoram? not [if]  cornerstone <<<datagram>>> compiletoram [then]
