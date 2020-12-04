using System.Buffers;
using SuperSocket.ProtoBase;

namespace SuperSocket.MQTT.Packets
{
    public class PubAckPacket : MQTTPacket
    {
        public override int EncodeBody(IBufferWriter<byte> writer)
        {
            throw new System.NotImplementedException();
        }

        internal protected override void DecodeBody(ref SequenceReader<byte> reader, object context)
        {

        }
    }
}