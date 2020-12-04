using System.Buffers;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.MQTT.Packets
{
    public class UnsubscribePacket : MQTTPacket
    {
        public string TopicName { get; set; }
        public byte PacketIdentifier { get; set; }
        public override int EncodeBody(IBufferWriter<byte> writer)
        {
            throw new System.NotImplementedException();
        }

        internal protected override void DecodeBody(ref SequenceReader<byte> reader, object context)
        {
            reader.TryRead(out byte a);
            reader.TryRead(out byte packetIdentifier);
            PacketIdentifier = packetIdentifier;
            reader.TryReadBigEndian(out short protocolNameLen);
            TopicName = reader.ReadString(protocolNameLen, Encoding.UTF8);
        }
    }
}