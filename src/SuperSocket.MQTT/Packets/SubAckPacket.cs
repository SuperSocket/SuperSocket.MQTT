using System;
using System.Buffers;
using System.Collections.Generic;

namespace SuperSocket.MQTT.Packets
{
    public class SubAckPacket : MQTTPacket
    {
        public ushort PacketIdentifier { get; set; }
        public List<byte> ReturnCodes { get; set; } = new List<byte>();

        public override int EncodeBody(IBufferWriter<byte> writer)
        {
            var span = writer.GetSpan(2 + ReturnCodes.Count);
            
            // Write packet identifier (2 bytes, big endian)
            span[0] = (byte)(PacketIdentifier >> 8);
            span[1] = (byte)(PacketIdentifier & 0xFF);
            
            // Write return codes
            for (int i = 0; i < ReturnCodes.Count; i++)
            {
                span[2 + i] = ReturnCodes[i];
            }
            
            writer.Advance(2 + ReturnCodes.Count);
            return 2 + ReturnCodes.Count;
        }

        internal protected override void DecodeBody(ref SequenceReader<byte> reader, object context)
        {
            // Read packet identifier
            if (reader.TryReadBigEndian(out short packetId))
            {
                PacketIdentifier = (ushort)packetId;
            }

            // Read return codes
            ReturnCodes.Clear();
            while (reader.Remaining > 0)
            {
                if (reader.TryRead(out byte returnCode))
                {
                    ReturnCodes.Add(returnCode);
                }
            }
        }
    }
}
