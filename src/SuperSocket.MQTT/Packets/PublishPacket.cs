using System;
using System.Buffers;
using System.Linq;
using System.Text;

namespace SuperSocket.MQTT.Packets
{
    public class PublishPacket : MQTTPacket
    {
        public byte Qos { get; set; }
        public bool Dup { get; set; }
        public bool Retain { get; set; }
        public string TopicName { get; set; }
        public ushort PacketIdentifier { get; set; }
        public ReadOnlyMemory<byte> Payload { get; set; }

        public override int EncodeBody(IBufferWriter<byte> writer)
        {
            var topicBytes = Encoding.UTF8.GetBytes(TopicName ?? string.Empty);
            var payloadSize = Payload.Length;
            var totalSize = 2 + topicBytes.Length + payloadSize;
            
            // Add 2 bytes for packet identifier if QoS > 0
            if (Qos > 0)
                totalSize += 2;
            
            var span = writer.GetSpan(totalSize);
            var offset = 0;
            
            // Write topic name length (2 bytes, big endian)
            span[offset++] = (byte)(topicBytes.Length >> 8);
            span[offset++] = (byte)(topicBytes.Length & 0xFF);
            
            // Write topic name
            topicBytes.CopyTo(span.Slice(offset));
            offset += topicBytes.Length;
            
            // Write packet identifier if QoS > 0
            if (Qos > 0)
            {
                span[offset++] = (byte)(PacketIdentifier >> 8);
                span[offset++] = (byte)(PacketIdentifier & 0xFF);
            }
            
            // Write payload
            Payload.Span.CopyTo(span.Slice(offset));
            offset += payloadSize;
            
            writer.Advance(totalSize);
            return totalSize;
        }

        internal protected override void DecodeBody(ref SequenceReader<byte> reader, object context)
        {
            // Extract QoS, DUP, and RETAIN from flags
            Qos = (byte)((Flags >> 1) & 0x03);
            Dup = (Flags & 0x08) != 0;
            Retain = (Flags & 0x01) != 0;
            
            // Read topic name
            if (reader.TryReadBigEndian(out short topicNameLen))
            {
                TopicName = reader.ReadString(topicNameLen, Encoding.UTF8);
            }
            
            // Read packet identifier if QoS > 0
            if (Qos > 0)
            {
                if (reader.TryReadBigEndian(out short packetId))
                {
                    PacketIdentifier = (ushort)packetId;
                }
            }
            
            // Read payload (remaining bytes)
            if (reader.Remaining > 0)
            {
                Payload = reader.Sequence.Slice(reader.Consumed).ToArray();
                reader.Advance(reader.Remaining);
            }
        }
    }
}