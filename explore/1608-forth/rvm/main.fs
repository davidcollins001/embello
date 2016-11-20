\ application setup and main loop
\ assumes that the Analog Plug is connected to PB6..PB7

1 constant debug  \ 0 = send RF packets, 1 = display on serial port

: show-readings ( vy vb vg vr -- )
  hwid hex. ." = "
  ." Vr: " . ." Vg: " . ." Vb " . ." Vy " . ;

: send-packet ( vy vb vg vr -- )
  3 <pkt  hwid u+>  4 0 do n+> loop  pkt>rf ;

: opamp-on
  VCC1 ios!  VCC2 ios!  \ tied together: must always be the same!
  OMODE-PP VCC1 io-mode!  OMODE-PP VCC2 io-mode! ;

: adc-pins
  IMODE-ADC ANA1 io-mode!
  IMODE-ADC ANA2 io-mode!
  IMODE-ADC ANA3 io-mode!
  IMODE-ADC ANA4 io-mode! ;

: main
  2.1MHz 1000 systick-hz  +lptim opamp-on adc-pins

  8686 rf69.freq ! 6 rf69.group ! 62 rf69.nodeid !
  rf69-init 16 rf-power

  mcp-init if ." can't find MCP3424" exit then

  begin
    led-off rf-sleep

    4 0 do i mcp-data loop

    led-on
    debug if
      hsi-on show-readings cr 1 ms
    else
      send-packet
    then
  key? until ;
