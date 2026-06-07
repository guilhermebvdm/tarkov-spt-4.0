# ByteBanger Binary Layout V 1.0

```unformatted
42 59 42 41                 # Identifier "BYBA" for ByteBanger
01 00                       # File version 1.0

00 89 54 98                 # Original file length, Int32
00 01 02 03 04 05 06 07     # -\
08 09 0A 0B 0C 0D 0E 0F     #  | Original checksum, 32 Bytes
10 11 12 13 14 15 16 17     #  | SHA-256
18 19 1A 1B 1C 1D 1E 1F     # -/
00 87 B8 00                 # Patched file length, Int32
20 21 22 23 24 25 26 27     # -\
28 29 2A 2B 2C 2D 2E 2F     #  | Original checksum, 32 Bytes
30 31 32 33 34 35 36 37     #  | SHA-256
38 39 3A 3B 3C 3D 3E 3F     # -/
00 00 00 04                 # Count of patch items, Int32

00 00 00 9D                 # Offset from file start, Int32
00 00 00 04                 # Patch content length
70 71 72 73                 # Patch content

00 00 00 A8                 # Offset, Int32
00 00 00 04                 # Patch content length
80 81 82 83                 # Content

00 00 00 B1                 # Offset
00 00 00 04                 # Patch content length
90 91 92 93                 # Content

00 00 00 D1                 # Offset
00 00 00 04                 # Patch content length
A0 A1 A2 A3                 # Content

Binary Length: 204 Bytes
```
