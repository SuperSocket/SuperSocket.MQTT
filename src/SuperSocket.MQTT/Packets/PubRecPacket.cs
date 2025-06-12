using System;
using System.Buffers;
using SuperSocket.ProtoBase;

namespace SuperSocket.MQTT.Packets
{
    public class PubRecPacket : MQTTPacket
    {
        public ushort PacketIdentifier { get; set; }

        public override int EncodeBody(IBufferWriter<byte> writer)
        {
            var span = writer.GetSpan(2);
            span[0] = (byte)(PacketIdentifier >> 8);
            span[1] = (byte)(PacketIdentifier & 0xFF);
            writer.Advance(2);
            return 2;
        }

        internal protected override void DecodeBody(ref SequenceReader<byte> reader, object context)
        {
            if (reader.TryReadBigEndian(out ushort packetId))
            {
                PacketIdentifier = packetId;
            }
        }
    }
}