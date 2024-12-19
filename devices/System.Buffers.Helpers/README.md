# System.Buffers.Helpers

Contains an `IBitConverter` interface with both Big and Little Endian implementations.
Comes in utile when received data is encoded in another Endian way. Examples are:

- Networking data which is encoded in Big Endian. Hence when reading or writing UDP or TCP packets the data needs to be decoded or encoded with a Big Endian converter. 
- Serialized data could be encoded in one Endian way or another. Hence the data needs to be read with the appropriate converter.

Suppose a byte[] is received which contains a double. The transmitter of the data could be Big or Little Endian encoded. The IBitConverter allows to use the implementation as needed:

```csharp
    internal class SerializationComponent
    {
        private readonly IBitConverter bitConverter;

        public SerializationComponent(bool dataIsLittleEndian)
        {
            bitConverter = dataIsLittleEndian ? EndianBitConverter.Little : EndianBitConverter.Big;
        }

        public double ReceivedDoubleInData(byte[] data)
        {
            return bitConverter.ToDouble(data);
        }
    }
```
