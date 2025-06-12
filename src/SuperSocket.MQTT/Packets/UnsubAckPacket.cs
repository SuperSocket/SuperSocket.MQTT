using System;
using System.Buffers;
using System.Collections.Generic;

namespace SuperSocket.MQTT.Packets
{
    public class UnsubAckPacket : MQTTPacket
    {
        public ushort PacketIdentifier { get; set; }

        public override int EncodeBody(IBufferWriter<byte> writer)
        {
            var span = writer.GetSpan(2);
            
            // Write packet identifier (2 bytes, big endian)
            span[0] = (byte)(PacketIdentifier >> 8);
            span[1] = (byte)(PacketIdentifier & 0xFF);
            
            writer.Advance(2);
            return 2;
        }

        internal protected override void DecodeBody(ref SequenceReader<byte> reader, object context)
        {
            // Read packet identifier
            if (reader.TryReadBigEndian(out short packetId))
            {
                PacketIdentifier = (ushort)packetId;
            }
        }
    }
}
