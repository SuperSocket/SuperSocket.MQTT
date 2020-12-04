using System.Buffers;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.MQTT
{
    public static class SequenceReaderExtensions
    {
        public static string ReadLengthEncodedString(this ref SequenceReader<byte> reader)
        {
            reader.TryReadBigEndian(out short length);

            if (length == 0)
                return string.Empty;

            return reader.ReadString(length, Encoding.UTF8);
        }

        public static string ReadString(this ref SequenceReader<byte> reader, int length, Encoding encoding)
        {
            var buffer = reader.Sequence.Slice(reader.Consumed, length);

            try
            {
                return buffer.GetString(encoding);
            }
            finally
            {
                reader.Advance(length);
            }
        }
    }
}