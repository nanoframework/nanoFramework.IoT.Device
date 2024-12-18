# System.Buffers.Helpers

Contains an `IBitConverter` interface with both Big and Little Endian implementations.
Comes in utile when received data is encoded in another Endian way. Examples are:
- Networking data which is encoded in Big Endian
- serialized data could be encoded in another Endian way
