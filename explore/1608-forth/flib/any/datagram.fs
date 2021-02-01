\ compiletoflash
\ ( datagram start: ) here dup hex.

\ datagram packet format:
\   [len, to, from, flags/seq, data[0], ..., data[n]]

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
 \ rf.buf 3 + constant dg.flags
 rf.buf 3 + constant dg.pkt-seq
 rf.buf 4 + constant dg.data

  0 variable dg.seq#
RF:MAXDATA buffer: dg.buf

\ --------------------------------------------------
\  Internal Helpers
\ --------------------------------------------------

: dg-seq@ ( -- n ) dg.seq# @ ;
: dg-seq! ( n -- ) dg.seq# ! ;
: dg-seq++ ( -- n ) dg.seq# @ 1+ $ff mod dup dg-seq! ;        \ increment seq
\ : dg-seq! ( -- ) 1 dg.seq# +!  dg.seq@ dg.data c! ;
\ : dg-seq! ( -- ) dg.seq@ dg.data c! ;

\ TODO exponential backoff timeout?
: dg-timeout? ( time -- ) millis swap - DG:TIMEOUT > ;

: dg-ack? ( from -- ? )
  ( from ) dg.from c@ =  dg-seq@ dg.pkt-seq c@ = and
  ;

: dg-recv-ack ( from -- ? )
  millis
  1 0 do
    rf-recv if
      0 rf-recv-done
      over dg-ack? if ( ." ack'd " ) true leave then
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
  \ 0 dg.flags c!                               \ set flags
  \ seq isn't changed after receiving so no need to set
  \ dg.pkt-seq c@     dg.pkt-seq c!             \ set sequence number
  ;

: dg-wait-sent ( -- )                         \ wait until the radio can send
  begin rf-sending? while yield repeat
  ;
: dg-send ( buffer len -- )                   \ try until packet has sent
  begin 2dup  rf-send not while yield repeat
  2drop
  ;

: dg-send-ack ( addr -- )
  0 swap ( addr ) dg-set-header
  rf.buf DG:HDR# 1+ dg-send
  dg-wait-sent
  ;

: dg-send-retry ( buffer len -- ? )      	    \ retry sending
  rot false                                   \ set return value

  DG:RETRIES 0 do
    2over dg-send dg-wait-sent
    over dg-recv-ack if drop true leave then
  loop
  2nip nip
  ;

\ --------------------------------------------------
\   External API
\ --------------------------------------------------

: dg-send-to ( addr len addr -- ? )         	\ send out one packet for node
  ( swap DG:MAXDATA min swap )                  \ bounds check payload
  2dup
  ( len addr ) dg-set-header
  dg-seq++ dg.pkt-seq c!                      \ increment and set seq
  -rot  tuck                                  \ push addr to back and copy len
  ( buffer len ) dg.data swap move            \ copy user data to radio buffer
  DG:HDR# 1+ + rf.buf swap dg-send-retry      \ add header len to packet len

  rf-idle-mode!
  ;

\ TODO should take address
: dg-recv ( -- addr n )
  rf-recv if
    dg.buf rf-recv-done
    dg.from c@ dg-send-ack
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
  0 do dg.buf i + c@ h.2 space loop cr
  ;

: dg-listen ( -- )
  cr
  begin
    dg-recv if
	  rf.pkt-handler @ execute
    then
    yield
  key? until
  rf-idle-mode!
  ;

\ ( datagram end, size: ) here dup hex. swap - .
compiletoram? not [if]  cornerstone <<<datagram>>> compiletoram [then]
