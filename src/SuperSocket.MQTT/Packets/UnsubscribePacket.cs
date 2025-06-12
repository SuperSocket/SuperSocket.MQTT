using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.MQTT.Packets
{
    public class UnsubscribePacket : MQTTPacket
    {
        public ushort PacketIdentifier { get; set; }
        public List<string> TopicFilters { get; set; } = new List<string>();

        public override int EncodeBody(IBufferWriter<byte> writer)
        {
            var totalSize = 2; // Packet identifier
            
            // Calculate total size needed
            foreach (var topic in TopicFilters)
            {
                var topicBytes = Encoding.UTF8.GetBytes(topic ?? string.Empty);
                totalSize += 2 + topicBytes.Length; // length + topic
            }
            
            var span = writer.GetSpan(totalSize);
            var offset = 0;
            
            // Write packet identifier
            span[offset++] = (byte)(PacketIdentifier >> 8);
            span[offset++] = (byte)(PacketIdentifier & 0xFF);
            
            // Write topic filters
            foreach (var topic in TopicFilters)
            {
                var topicBytes = Encoding.UTF8.GetBytes(topic ?? string.Empty);
                
                // Write topic length
                span[offset++] = (byte)(topicBytes.Length >> 8);
                span[offset++] = (byte)(topicBytes.Length & 0xFF);
                
                // Write topic
                topicBytes.AsSpan().CopyTo(span.Slice(offset));
                offset += topicBytes.Length;
            }
            
            writer.Advance(totalSize);
            return totalSize;
        }

        internal protected override void DecodeBody(ref SequenceReader<byte> reader, object context)
        {
            // Read packet identifier
            if (reader.TryReadBigEndian(out short packetIdentifier))
            {
                PacketIdentifier = (ushort)packetIdentifier;
            }
            
            // Read topic filters
            TopicFilters.Clear();
            while (reader.Remaining > 0)
            {
                if (reader.TryReadBigEndian(out short topicLen) && reader.Remaining >= topicLen)
                {
                    var topic = reader.ReadString(topicLen, Encoding.UTF8);
                    TopicFilters.Add(topic);
                }
                else
                {
                    break;
                }
            }
        }
    }
}