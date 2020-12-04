using System;
using System.Buffers;
using SuperSocket.ProtoBase;

namespace SuperSocket.MQTT.Packets
{
    public class PubRecPacket : MQTTPacket
    {
        public ReadOnlyMemory<byte> PacketData { get; set; }
        public override int EncodeBody(IBufferWriter<byte> writer)
        {
            throw new System.NotImplementedException();
        }

        internal protected override void DecodeBody(ref SequenceReader<byte> reader, object context)
        {
            PacketData = reader.Sequence.ToArray();
        }
    }
}