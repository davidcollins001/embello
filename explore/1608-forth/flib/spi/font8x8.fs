
\ https://github.com/dhepper/font8x8/blob/master/font8x8_basic.h

\ rotate bitmap 90 deg
\ def rot(orig):
 \ dest = [[] for _ in range(len(orig))]
 \ for c in orig:
  \ for i, b in enumerate(reversed(f'{c:08b}')):
   \ dest[i].append(b)
 \ return [hex(int(''.join(b), 2)) for b in dest]

\ create ASCII:SET
\ hex
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0000 (nul)
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0001
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0002
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0003
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0004
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0005
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0006
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0007
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0008
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0009
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+000A
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+000B
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+000C
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+000D
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+000E
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+000F
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0010
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0011
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0012
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0013
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0014
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0015
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0016
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0017
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0018
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0019
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+001A
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+001B
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+001C
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+001D
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+001E
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+001F
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0020 (space)
  \ 18 c, 3C c, 3C c, 18 c, 18 c, 00 c, 18 c, 00 c,   \ U+0021 (!)
  \ 36 c, 36 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0022 (")
  \ 36 c, 36 c, 7F c, 36 c, 7F c, 36 c, 36 c, 00 c,   \ U+0023 (#)
  \ 0C c, 3E c, 03 c, 1E c, 30 c, 1F c, 0C c, 00 c,   \ U+0024 ($)
  \ 00 c, 63 c, 33 c, 18 c, 0C c, 66 c, 63 c, 00 c,   \ U+0025 (%)
  \ 1C c, 36 c, 1C c, 6E c, 3B c, 33 c, 6E c, 00 c,   \ U+0026 (&)
  \ 06 c, 06 c, 03 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0027 (')
  \ 18 c, 0C c, 06 c, 06 c, 06 c, 0C c, 18 c, 00 c,   \ U+0028 (()
  \ 06 c, 0C c, 18 c, 18 c, 18 c, 0C c, 06 c, 00 c,   \ U+0029 ())
  \ 00 c, 66 c, 3C c, FF c, 3C c, 66 c, 00 c, 00 c,   \ U+002A (*)
  \ 00 c, 0C c, 0C c, 3F c, 0C c, 0C c, 00 c, 00 c,   \ U+002B (+)
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 0C c, 0C c, 06 c,   \ U+002C (,)
  \ 00 c, 00 c, 00 c, 3F c, 00 c, 00 c, 00 c, 00 c,   \ U+002D (-)
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 0C c, 0C c, 00 c,   \ U+002E (.)
  \ 60 c, 30 c, 18 c, 0C c, 06 c, 03 c, 01 c, 00 c,   \ U+002F (/)
  \ 3E c, 63 c, 73 c, 7B c, 6F c, 67 c, 3E c, 00 c,   \ U+0030 (0)
  \ 0C c, 0E c, 0C c, 0C c, 0C c, 0C c, 3F c, 00 c,   \ U+0031 (1)
  \ 1E c, 33 c, 30 c, 1C c, 06 c, 33 c, 3F c, 00 c,   \ U+0032 (2)
  \ 1E c, 33 c, 30 c, 1C c, 30 c, 33 c, 1E c, 00 c,   \ U+0033 (3)
  \ 38 c, 3C c, 36 c, 33 c, 7F c, 30 c, 78 c, 00 c,   \ U+0034 (4)
  \ 3F c, 03 c, 1F c, 30 c, 30 c, 33 c, 1E c, 00 c,   \ U+0035 (5)
  \ 1C c, 06 c, 03 c, 1F c, 33 c, 33 c, 1E c, 00 c,   \ U+0036 (6)
  \ 3F c, 33 c, 30 c, 18 c, 0C c, 0C c, 0C c, 00 c,   \ U+0037 (7)
  \ 1E c, 33 c, 33 c, 1E c, 33 c, 33 c, 1E c, 00 c,   \ U+0038 (8)
  \ 1E c, 33 c, 33 c, 3E c, 30 c, 18 c, 0E c, 00 c,   \ U+0039 (9)
  \ 00 c, 0C c, 0C c, 00 c, 00 c, 0C c, 0C c, 00 c,   \ U+003A (:)
  \ 00 c, 0C c, 0C c, 00 c, 00 c, 0C c, 0C c, 06 c,   \ U+003B (;)
  \ 18 c, 0C c, 06 c, 03 c, 06 c, 0C c, 18 c, 00 c,   \ U+003C (<)
  \ 00 c, 00 c, 3F c, 00 c, 00 c, 3F c, 00 c, 00 c,   \ U+003D (=)
  \ 06 c, 0C c, 18 c, 30 c, 18 c, 0C c, 06 c, 00 c,   \ U+003E (>)
  \ 1E c, 33 c, 30 c, 18 c, 0C c, 00 c, 0C c, 00 c,   \ U+003F (?)
  \ 3E c, 63 c, 7B c, 7B c, 7B c, 03 c, 1E c, 00 c,   \ U+0040 (@)
  \ 0C c, 1E c, 33 c, 33 c, 3F c, 33 c, 33 c, 00 c,   \ U+0041 (A)
  \ 3F c, 66 c, 66 c, 3E c, 66 c, 66 c, 3F c, 00 c,   \ U+0042 (B)
  \ 3C c, 66 c, 03 c, 03 c, 03 c, 66 c, 3C c, 00 c,   \ U+0043 (C)
  \ 1F c, 36 c, 66 c, 66 c, 66 c, 36 c, 1F c, 00 c,   \ U+0044 (D)
  \ 7F c, 46 c, 16 c, 1E c, 16 c, 46 c, 7F c, 00 c,   \ U+0045 (E)
  \ 7F c, 46 c, 16 c, 1E c, 16 c, 06 c, 0F c, 00 c,   \ U+0046 (F)
  \ 3C c, 66 c, 03 c, 03 c, 73 c, 66 c, 7C c, 00 c,   \ U+0047 (G)
  \ 33 c, 33 c, 33 c, 3F c, 33 c, 33 c, 33 c, 00 c,   \ U+0048 (H)
  \ 1E c, 0C c, 0C c, 0C c, 0C c, 0C c, 1E c, 00 c,   \ U+0049 (I)
  \ 78 c, 30 c, 30 c, 30 c, 33 c, 33 c, 1E c, 00 c,   \ U+004A (J)
  \ 67 c, 66 c, 36 c, 1E c, 36 c, 66 c, 67 c, 00 c,   \ U+004B (K)
  \ 0F c, 06 c, 06 c, 06 c, 46 c, 66 c, 7F c, 00 c,   \ U+004C (L)
  \ 63 c, 77 c, 7F c, 7F c, 6B c, 63 c, 63 c, 00 c,   \ U+004D (M)
  \ 63 c, 67 c, 6F c, 7B c, 73 c, 63 c, 63 c, 00 c,   \ U+004E (N)
  \ 1C c, 36 c, 63 c, 63 c, 63 c, 36 c, 1C c, 00 c,   \ U+004F (O)
  \ 3F c, 66 c, 66 c, 3E c, 06 c, 06 c, 0F c, 00 c,   \ U+0050 (P)
  \ 1E c, 33 c, 33 c, 33 c, 3B c, 1E c, 38 c, 00 c,   \ U+0051 (Q)
  \ 3F c, 66 c, 66 c, 3E c, 36 c, 66 c, 67 c, 00 c,   \ U+0052 (R)
  \ 1E c, 33 c, 07 c, 0E c, 38 c, 33 c, 1E c, 00 c,   \ U+0053 (S)
  \ 3F c, 2D c, 0C c, 0C c, 0C c, 0C c, 1E c, 00 c,   \ U+0054 (T)
  \ 33 c, 33 c, 33 c, 33 c, 33 c, 33 c, 3F c, 00 c,   \ U+0055 (U)
  \ 33 c, 33 c, 33 c, 33 c, 33 c, 1E c, 0C c, 00 c,   \ U+0056 (V)
  \ 63 c, 63 c, 63 c, 6B c, 7F c, 77 c, 63 c, 00 c,   \ U+0057 (W)
  \ 63 c, 63 c, 36 c, 1C c, 1C c, 36 c, 63 c, 00 c,   \ U+0058 (X)
  \ 33 c, 33 c, 33 c, 1E c, 0C c, 0C c, 1E c, 00 c,   \ U+0059 (Y)
  \ 7F c, 63 c, 31 c, 18 c, 4C c, 66 c, 7F c, 00 c,   \ U+005A (Z)
  \ 1E c, 06 c, 06 c, 06 c, 06 c, 06 c, 1E c, 00 c,   \ U+005B ([)
  \ 03 c, 06 c, 0C c, 18 c, 30 c, 60 c, 40 c, 00 c,   \ U+005C (\)
  \ 1E c, 18 c, 18 c, 18 c, 18 c, 18 c, 1E c, 00 c,   \ U+005D (])
  \ 08 c, 1C c, 36 c, 63 c, 00 c, 00 c, 00 c, 00 c,   \ U+005E (^)
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, FF c,   \ U+005F (_)
  \ 0C c, 0C c, 18 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+0060 (`)
  \ 00 c, 00 c, 1E c, 30 c, 3E c, 33 c, 6E c, 00 c,   \ U+0061 (a)
  \ 07 c, 06 c, 06 c, 3E c, 66 c, 66 c, 3B c, 00 c,   \ U+0062 (b)
  \ 00 c, 00 c, 1E c, 33 c, 03 c, 33 c, 1E c, 00 c,   \ U+0063 (c)
  \ 38 c, 30 c, 30 c, 3e c, 33 c, 33 c, 6E c, 00 c,   \ U+0064 (d)
  \ 00 c, 00 c, 1E c, 33 c, 3f c, 03 c, 1E c, 00 c,   \ U+0065 (e)
  \ 1C c, 36 c, 06 c, 0f c, 06 c, 06 c, 0F c, 00 c,   \ U+0066 (f)
  \ 00 c, 00 c, 6E c, 33 c, 33 c, 3E c, 30 c, 1F c,   \ U+0067 (g)
  \ 07 c, 06 c, 36 c, 6E c, 66 c, 66 c, 67 c, 00 c,   \ U+0068 (h)
  \ 0C c, 00 c, 0E c, 0C c, 0C c, 0C c, 1E c, 00 c,   \ U+0069 (i)
  \ 30 c, 00 c, 30 c, 30 c, 30 c, 33 c, 33 c, 1E c,   \ U+006A (j)
  \ 07 c, 06 c, 66 c, 36 c, 1E c, 36 c, 67 c, 00 c,   \ U+006B (k)
  \ 0E c, 0C c, 0C c, 0C c, 0C c, 0C c, 1E c, 00 c,   \ U+006C (l)
  \ 00 c, 00 c, 33 c, 7F c, 7F c, 6B c, 63 c, 00 c,   \ U+006D (m)
  \ 00 c, 00 c, 1F c, 33 c, 33 c, 33 c, 33 c, 00 c,   \ U+006E (n)
  \ 00 c, 00 c, 1E c, 33 c, 33 c, 33 c, 1E c, 00 c,   \ U+006F (o)
  \ 00 c, 00 c, 3B c, 66 c, 66 c, 3E c, 06 c, 0F c,   \ U+0070 (p)
  \ 00 c, 00 c, 6E c, 33 c, 33 c, 3E c, 30 c, 78 c,   \ U+0071 (q)
  \ 00 c, 00 c, 3B c, 6E c, 66 c, 06 c, 0F c, 00 c,   \ U+0072 (r)
  \ 00 c, 00 c, 3E c, 03 c, 1E c, 30 c, 1F c, 00 c,   \ U+0073 (s)
  \ 08 c, 0C c, 3E c, 0C c, 0C c, 2C c, 18 c, 00 c,   \ U+0074 (t)
  \ 00 c, 00 c, 33 c, 33 c, 33 c, 33 c, 6E c, 00 c,   \ U+0075 (u)
  \ 00 c, 00 c, 33 c, 33 c, 33 c, 1E c, 0C c, 00 c,   \ U+0076 (v)
  \ 00 c, 00 c, 63 c, 6B c, 7F c, 7F c, 36 c, 00 c,   \ U+0077 (w)
  \ 00 c, 00 c, 63 c, 36 c, 1C c, 36 c, 63 c, 00 c,   \ U+0078 (x)
  \ 00 c, 00 c, 33 c, 33 c, 33 c, 3E c, 30 c, 1F c,   \ U+0079 (y)
  \ 00 c, 00 c, 3F c, 19 c, 0C c, 26 c, 3F c, 00 c,   \ U+007A (z)
  \ 38 c, 0C c, 0C c, 07 c, 0C c, 0C c, 38 c, 00 c,   \ U+007B ({)
  \ 18 c, 18 c, 18 c, 00 c, 18 c, 18 c, 18 c, 00 c,   \ U+007C (|)
  \ 07 c, 0C c, 0C c, 38 c, 0C c, 0C c, 07 c, 00 c,   \ U+007D (})
  \ 6E c, 3B c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+007E (~)
  \ 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,   \ U+007F
\ decimal

create ASCII:SET
hex
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+0000 (nul)
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+0001
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+0002
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+0003
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+0004
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+0005
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+0006
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+0007
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+0008
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+0009
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+000A
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+000B
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+000C
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+000D
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+000E
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+000F
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+0010
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+0011
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+0012
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+0013
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+0014
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+0015
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+0016
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+0017
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+0018
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+0019
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+001A
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+001B
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+001C
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+001D
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+001E
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+001F
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+0020 (space)
  00 c, 00 c, 60 c, fa c, fa c, 60 c, 00 c, 00 c,  \ U+0021 (!)
  00 c, c0 c, c0 c, 00 c, c0 c, c0 c, 00 c, 00 c,  \ U+0022 (")
  28 c, fe c, fe c, 28 c, fe c, fe c, 28 c, 00 c,  \ U+0023 (#)
  24 c, 74 c, d6 c, d6 c, 5c c, 48 c, 00 c, 00 c,  \ U+0024 ($)
  62 c, 66 c, 0c c, 18 c, 30 c, 66 c, 46 c, 00 c,  \ U+0025 (%)
  0c c, 5e c, f2 c, ba c, ec c, 5e c, 12 c, 00 c,  \ U+0026 (&)
  20 c, e0 c, c0 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+0027 (')
  00 c, 38 c, 7c c, c6 c, 82 c, 00 c, 00 c, 00 c,  \ U+0028 (()
  00 c, 82 c, c6 c, 7c c, 38 c, 00 c, 00 c, 00 c,  \ U+0029 ())
  10 c, 54 c, 7c c, 38 c, 38 c, 7c c, 54 c, 10 c,  \ U+002A (*)
  10 c, 10 c, 7c c, 7c c, 10 c, 10 c, 00 c, 00 c,  \ U+002B (+)
  00 c, 01 c, 07 c, 06 c, 00 c, 00 c, 00 c, 00 c,  \ U+002C (,)
  10 c, 10 c, 10 c, 10 c, 10 c, 10 c, 00 c, 00 c,  \ U+002D (-)
  00 c, 00 c, 06 c, 06 c, 00 c, 00 c, 00 c, 00 c,  \ U+002E (.)
  06 c, 0c c, 18 c, 30 c, 60 c, c0 c, 80 c, 00 c,  \ U+002F (/)
  7c c, fe c, 8e c, 9a c, b2 c, fe c, 7c c, 00 c,  \ U+0030 (0)
  02 c, 42 c, fe c, fe c, 02 c, 02 c, 00 c, 00 c,  \ U+0031 (1)
  46 c, ce c, 9a c, 92 c, f6 c, 66 c, 00 c, 00 c,  \ U+0032 (2)
  44 c, c6 c, 92 c, 92 c, fe c, 6c c, 00 c, 00 c,  \ U+0033 (3)
  18 c, 38 c, 68 c, ca c, fe c, fe c, 0a c, 00 c,  \ U+0034 (4)
  e4 c, e6 c, a2 c, a2 c, be c, 9c c, 00 c, 00 c,  \ U+0035 (5)
  3c c, 7e c, d2 c, 92 c, 9e c, 0c c, 00 c, 00 c,  \ U+0036 (6)
  c0 c, c0 c, 8e c, 9e c, f0 c, e0 c, 00 c, 00 c,  \ U+0037 (7)
  6c c, fe c, 92 c, 92 c, fe c, 6c c, 00 c, 00 c,  \ U+0038 (8)
  60 c, f2 c, 92 c, 96 c, fc c, 78 c, 00 c, 00 c,  \ U+0039 (9)
  00 c, 00 c, 66 c, 66 c, 00 c, 00 c, 00 c, 00 c,  \ U+003A (:)
  00 c, 01 c, 67 c, 66 c, 00 c, 00 c, 00 c, 00 c,  \ U+003B (;)
  10 c, 38 c, 6c c, c6 c, 82 c, 00 c, 00 c, 00 c,  \ U+003C (<)
  24 c, 24 c, 24 c, 24 c, 24 c, 24 c, 00 c, 00 c,  \ U+003D (=)
  00 c, 82 c, c6 c, 6c c, 38 c, 10 c, 00 c, 00 c,  \ U+003E (>)
  40 c, c0 c, 8a c, 9a c, f0 c, 60 c, 00 c, 00 c,  \ U+003F (?)
  7c c, fe c, 82 c, ba c, ba c, f8 c, 78 c, 00 c,  \ U+0040 (@)
  3e c, 7e c, c8 c, c8 c, 7e c, 3e c, 00 c, 00 c,  \ U+0041 (A)
  82 c, fe c, fe c, 92 c, 92 c, fe c, 6c c, 00 c,  \ U+0042 (B)
  38 c, 7c c, c6 c, 82 c, 82 c, c6 c, 44 c, 00 c,  \ U+0043 (C)
  82 c, fe c, fe c, 82 c, c6 c, 7c c, 38 c, 00 c,  \ U+0044 (D)
  82 c, fe c, fe c, 92 c, ba c, 82 c, c6 c, 00 c,  \ U+0045 (E)
  82 c, fe c, fe c, 92 c, b8 c, 80 c, c0 c, 00 c,  \ U+0046 (F)
  38 c, 7c c, c6 c, 82 c, 8a c, ce c, 4e c, 00 c,  \ U+0047 (G)
  fe c, fe c, 10 c, 10 c, fe c, fe c, 00 c, 00 c,  \ U+0048 (H)
  00 c, 82 c, fe c, fe c, 82 c, 00 c, 00 c, 00 c,  \ U+0049 (I)
  0c c, 0e c, 02 c, 82 c, fe c, fc c, 80 c, 00 c,  \ U+004A (J)
  82 c, fe c, fe c, 10 c, 38 c, ee c, c6 c, 00 c,  \ U+004B (K)
  82 c, fe c, fe c, 82 c, 02 c, 06 c, 0e c, 00 c,  \ U+004C (L)
  fe c, fe c, 70 c, 38 c, 70 c, fe c, fe c, 00 c,  \ U+004D (M)
  fe c, fe c, 60 c, 30 c, 18 c, fe c, fe c, 00 c,  \ U+004E (N)
  38 c, 7c c, c6 c, 82 c, c6 c, 7c c, 38 c, 00 c,  \ U+004F (O)
  82 c, fe c, fe c, 92 c, 90 c, f0 c, 60 c, 00 c,  \ U+0050 (P)
  78 c, fc c, 84 c, 8e c, fe c, 7a c, 00 c, 00 c,  \ U+0051 (Q)
  82 c, fe c, fe c, 90 c, 98 c, fe c, 66 c, 00 c,  \ U+0052 (R)
  64 c, f6 c, b2 c, 9a c, ce c, 4c c, 00 c, 00 c,  \ U+0053 (S)
  c0 c, 82 c, fe c, fe c, 82 c, c0 c, 00 c, 00 c,  \ U+0054 (T)
  fe c, fe c, 02 c, 02 c, fe c, fe c, 00 c, 00 c,  \ U+0055 (U)
  f8 c, fc c, 06 c, 06 c, fc c, f8 c, 00 c, 00 c,  \ U+0056 (V)
  fe c, fe c, 0c c, 18 c, 0c c, fe c, fe c, 00 c,  \ U+0057 (W)
  c2 c, e6 c, 3c c, 18 c, 3c c, e6 c, c2 c, 00 c,  \ U+0058 (X)
  e0 c, f2 c, 1e c, 1e c, f2 c, e0 c, 00 c, 00 c,  \ U+0059 (Y)
  e2 c, c6 c, 8e c, 9a c, b2 c, e6 c, ce c, 00 c,  \ U+005A (Z)
  00 c, fe c, fe c, 82 c, 82 c, 00 c, 00 c, 00 c,  \ U+005B ([)
  80 c, c0 c, 60 c, 30 c, 18 c, 0c c, 06 c, 00 c,  \ U+005C (\)
  00 c, 82 c, 82 c, fe c, fe c, 00 c, 00 c, 00 c,  \ U+005D (])
  10 c, 30 c, 60 c, c0 c, 60 c, 30 c, 10 c, 00 c,  \ U+005E (^)
  01 c, 01 c, 01 c, 01 c, 01 c, 01 c, 01 c, 10 c,  \ U+005F (_)
  00 c, 00 c, c0 c, e0 c, 20 c, 00 c, 00 c, 00 c,  \ U+0060 (`)
  04 c, 2e c, 2a c, 2a c, 3c c, 1e c, 02 c, 00 c,  \ U+0061 (a)
  82 c, fe c, fc c, 12 c, 12 c, 1e c, 0c c, 00 c,  \ U+0062 (b)
  1c c, 3e c, 22 c, 22 c, 36 c, 14 c, 00 c, 00 c,  \ U+0063 (c)
  0c c, 1e c, 12 c, 92 c, fc c, fe c, 02 c, 00 c,  \ U+0064 (d)
  1c c, 3e c, 2a c, 2a c, 3a c, 18 c, 00 c, 00 c,  \ U+0065 (e)
  12 c, 7e c, fe c, 92 c, c0 c, 40 c, 00 c, 00 c,  \ U+0066 (f)
  19 c, 3d c, 25 c, 25 c, 1f c, 3e c, 20 c, 00 c,  \ U+0067 (g)
  82 c, fe c, fe c, 10 c, 20 c, 3e c, 1e c, 00 c,  \ U+0068 (h)
  00 c, 22 c, be c, be c, 02 c, 00 c, 00 c, 00 c,  \ U+0069 (i)
  06 c, 07 c, 01 c, 01 c, bf c, be c, 00 c, 00 c,  \ U+006A (j)
  82 c, fe c, fe c, 08 c, 1c c, 36 c, 22 c, 00 c,  \ U+006B (k)
  00 c, 82 c, fe c, fe c, 02 c, 00 c, 00 c, 00 c,  \ U+006C (l)
  3e c, 3e c, 18 c, 1c c, 38 c, 3e c, 1e c, 00 c,  \ U+006D (m)
  3e c, 3e c, 20 c, 20 c, 3e c, 1e c, 00 c, 00 c,  \ U+006E (n)
  1c c, 3e c, 22 c, 22 c, 3e c, 1c c, 00 c, 00 c,  \ U+006F (o)
  21 c, 3f c, 1f c, 25 c, 24 c, 3c c, 18 c, 00 c,  \ U+0070 (p)
  18 c, 3c c, 24 c, 25 c, 1f c, 3f c, 21 c, 00 c,  \ U+0071 (q)
  22 c, 3e c, 1e c, 32 c, 20 c, 38 c, 18 c, 00 c,  \ U+0072 (r)
  12 c, 3a c, 2a c, 2a c, 2e c, 24 c, 00 c, 00 c,  \ U+0073 (s)
  00 c, 20 c, 7c c, fe c, 22 c, 24 c, 00 c, 00 c,  \ U+0074 (t)
  3c c, 3e c, 02 c, 02 c, 3c c, 3e c, 02 c, 00 c,  \ U+0075 (u)
  38 c, 3c c, 06 c, 06 c, 3c c, 38 c, 00 c, 00 c,  \ U+0076 (v)
  3c c, 3e c, 0e c, 1c c, 0e c, 3e c, 3c c, 00 c,  \ U+0077 (w)
  22 c, 36 c, 1c c, 08 c, 1c c, 36 c, 22 c, 00 c,  \ U+0078 (x)
  39 c, 3d c, 05 c, 05 c, 3f c, 3e c, 00 c, 00 c,  \ U+0079 (y)
  32 c, 26 c, 2e c, 3a c, 32 c, 26 c, 00 c, 00 c,  \ U+007A (z)
  10 c, 10 c, 7c c, ee c, 82 c, 82 c, 00 c, 00 c,  \ U+007B ({)
  00 c, 00 c, 00 c, ee c, ee c, 00 c, 00 c, 00 c,  \ U+007C (|)
  82 c, 82 c, ee c, 7c c, 10 c, 10 c, 00 c, 00 c,  \ U+007D (})
  40 c, c0 c, 80 c, c0 c, 40 c, c0 c, 80 c, 00 c,  \ U+007E (~)
  00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c, 00 c,  \ U+007F
decimal

\ reverse lookup for 8 bit numbers, eg 1 reversed is $80
create REV:8BIT
hex
  00 c, 80 c, 40 c, C0 c, 20 c, A0 c, 60 c, E0 c,
  10 c, 90 c, 50 c, D0 c, 30 c, B0 c, 70 c, F0 c,
  08 c, 88 c, 48 c, C8 c, 28 c, A8 c, 68 c, E8 c,
  18 c, 98 c, 58 c, D8 c, 38 c, B8 c, 78 c, F8 c,
  04 c, 84 c, 44 c, C4 c, 24 c, A4 c, 64 c, E4 c,
  14 c, 94 c, 54 c, D4 c, 34 c, B4 c, 74 c, F4 c,
  0C c, 8C c, 4C c, CC c, 2C c, AC c, 6C c, EC c,
  1C c, 9C c, 5C c, DC c, 3C c, BC c, 7C c, FC c,
  02 c, 82 c, 42 c, C2 c, 22 c, A2 c, 62 c, E2 c,
  12 c, 92 c, 52 c, D2 c, 32 c, B2 c, 72 c, F2 c,
  0A c, 8A c, 4A c, CA c, 2A c, AA c, 6A c, EA c,
  1A c, 9A c, 5A c, DA c, 3A c, BA c, 7A c, FA c,
  06 c, 86 c, 46 c, C6 c, 26 c, A6 c, 66 c, E6 c,
  16 c, 96 c, 56 c, D6 c, 36 c, B6 c, 76 c, F6 c,
  0E c, 8E c, 4E c, CE c, 2E c, AE c, 6E c, EE c,
  1E c, 9E c, 5E c, DE c, 3E c, BE c, 7E c, FE c,
  01 c, 81 c, 41 c, C1 c, 21 c, A1 c, 61 c, E1 c,
  11 c, 91 c, 51 c, D1 c, 31 c, B1 c, 71 c, F1 c,
  09 c, 89 c, 49 c, C9 c, 29 c, A9 c, 69 c, E9 c,
  19 c, 99 c, 59 c, D9 c, 39 c, B9 c, 79 c, F9 c,
  05 c, 85 c, 45 c, C5 c, 25 c, A5 c, 65 c, E5 c,
  15 c, 95 c, 55 c, D5 c, 35 c, B5 c, 75 c, F5 c,
  0D c, 8D c, 4D c, CD c, 2D c, AD c, 6D c, ED c,
  1D c, 9D c, 5D c, DD c, 3D c, BD c, 7D c, FD c,
  03 c, 83 c, 43 c, C3 c, 23 c, A3 c, 63 c, E3 c,
  13 c, 93 c, 53 c, D3 c, 33 c, B3 c, 73 c, F3 c,
  0B c, 8B c, 4B c, CB c, 2B c, AB c, 6B c, EB c,
  1B c, 9B c, 5B c, DB c, 3B c, BB c, 7B c, FB c,
  07 c, 87 c, 47 c, C7 c, 27 c, A7 c, 67 c, E7 c,
  17 c, 97 c, 57 c, D7 c, 37 c, B7 c, 77 c, F7 c,
  0F c, 8F c, 4F c, CF c, 2F c, AF c, 6F c, EF c,
  1F c, 9F c, 5F c, DF c, 3F c, BF c, 7F c, FF c,
decimal
