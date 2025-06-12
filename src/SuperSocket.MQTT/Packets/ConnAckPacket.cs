using System.Buffers;
using SuperSocket.ProtoBase;

namespace SuperSocket.MQTT.Packets
{
    public class ConnAckPacket : MQTTPacket
    {
        public byte SessionPresent { get; set; }
        public byte ReturnCode { get; set; }

        public override int EncodeBody(IBufferWriter<byte> writer)
        {
            var span = writer.GetSpan(2);
            span[0] = SessionPresent;
            span[1] = ReturnCode;
            writer.Advance(2);
            return 2;
        }

        internal protected override void DecodeBody(ref SequenceReader<byte> reader, object context)
        {
            reader.TryRead(out byte sessionPresent);
            SessionPresent = sessionPresent;
            
            reader.TryRead(out byte returnCode);
            ReturnCode = returnCode;
        }
    }
}