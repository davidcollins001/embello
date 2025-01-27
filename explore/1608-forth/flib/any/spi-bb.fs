\ bit-banged SPI driver

\ SPI1
\ [ifndef] ssel  PA4 variable ssel  [then]  \ can be changed at run time
\ [ifndef] SCLK  PA5 constant SCLK  [then]
\ [ifndef] MISO  PA6 constant MISO  [then]
\ [ifndef] MOSI  PA7 constant MOSI  [then]

\ SPI2 - soldering for SPI1/SCLK bad connection
[ifndef] ssel  PA12 variable ssel  [then]  \ can be changed at run time
[ifndef] SCLK  PA13 constant SCLK  [then]
[ifndef] MISO  PA14 constant MISO  [then]
[ifndef] MOSI  PA15 constant MOSI  [then]

: +spi ( -- ) ssel @ ioc! ;  \ select SPI
: -spi ( -- ) ssel @ ios! ;  \ deselect SPI

: >spi> ( c -- c )  \ bit-banged SPI, 8 bits
  8 0 do
    dup $80 and MOSI io!
    SCLK ios!
    shl
    MISO io@ 1 and or
    SCLK ioc!
  loop
  $FF and ;

\ single byte transfers
: spi> ( -- c ) 0 >spi> ;  \ read byte from SPI
: >spi ( c -- ) >spi> drop ;  \ write byte to SPI

: spi-init ( -- )  \ set up bit-banged SPI
  OMODE-PP    ssel @ io-mode! -spi
  OMODE-PP    SCLK   io-mode!
  IMODE-FLOAT MISO   io-mode!
  OMODE-PP    MOSI   io-mode! ;
