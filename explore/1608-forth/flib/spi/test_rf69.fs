
\ include rf69.fs

66 variable count
count @ buffer: buf1
count @ buffer: buf2
: reset ( -- ) count @ 0 do 0 rf.buf i + c! loop ;
: show ( buf -- ) ." -> " count @ . count @ 0 do dup i + c@ . loop drop ;
: rfshow ( -- )
	rf-show-packet
  buf1 c@ 1+ count !
	buf1 show
	;

\ variable packet len < 66 per packet - can send 64 bytes

: tx-test
  86926 $B6 30 rf:GFSK_Rb250Fd250 RF:INIT rf-init
  \ 0 rf.buf 4 + dma-mem-init
  \ 1 4 lshift DMA1-CCR  DMA1:MEM-CHAN dma-reg bic!
	reset

  35992 50224 101890 1919 18 97 113
  hwid <pkt  7 0 do +pkt loop  pkt>

  rf.buf 4 + swap move
	\ ( addr n ) DMA1:MEM-CHAN +dma

  24  rf.buf c!
  $b  rf.buf 1+ c!
  30  rf.buf 2+ c!
  0   rf.buf 3 + c!

	rf.buf rf.buf c@ 1+ rf-send
	;

: rx-test
  86926 $B6 30 rf:GFSK_Rb250Fd250 RF:INIT rf-init
	['] rfshow rf.packet-handler !
	buf1 rf-listen
	." done "
	;

tx-test
\ rx-test
