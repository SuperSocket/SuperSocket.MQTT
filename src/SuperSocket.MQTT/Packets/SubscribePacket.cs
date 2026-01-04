using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.MQTT.Packets
{
    public class TopicFilter
    {
        public string Topic { get; set; }
        public byte QoS { get; set; }

        private Lazy<IReadOnlyList<string>> _topicSegmentsLazy;

        public IReadOnlyList<string> TopicSegments => _topicSegmentsLazy.Value;

        public TopicFilter()
        {
            _topicSegmentsLazy = new Lazy<IReadOnlyList<string>>(() => string.IsNullOrEmpty(this.Topic) ? Array.Empty<string>() : this.Topic.Split(MQTTConst.TopicLevelSeparator, StringSplitOptions.RemoveEmptyEntries));
        }
    }

    public class SubscribePacket : MQTTPacket
    {
        public ushort PacketIdentifier { get; set; }
        public List<TopicFilter> TopicFilters { get; set; } = new List<TopicFilter>();

        public override int EncodeBody(IBufferWriter<byte> writer)
        {
            var totalSize = 2; // Packet identifier
            
            // Calculate total size needed
            foreach (var filter in TopicFilters)
            {
                var topicBytes = Encoding.UTF8.GetBytes(filter.Topic ?? string.Empty);
                totalSize += 2 + topicBytes.Length + 1; // length + topic + QoS
            }
            
            var span = writer.GetSpan(totalSize);
            var offset = 0;
            
            // Write packet identifier
            span[offset++] = (byte)(PacketIdentifier >> 8);
            span[offset++] = (byte)(PacketIdentifier & 0xFF);
            
            // Write topic filters
            foreach (var filter in TopicFilters)
            {
                var topicBytes = Encoding.UTF8.GetBytes(filter.Topic ?? string.Empty);
                
                // Write topic length
                span[offset++] = (byte)(topicBytes.Length >> 8);
                span[offset++] = (byte)(topicBytes.Length & 0xFF);
                
                // Write topic
                topicBytes.AsSpan().CopyTo(span.Slice(offset));
                offset += topicBytes.Length;
                
                // Write QoS
                span[offset++] = filter.QoS;
            }
            
            writer.Advance(totalSize);
            return totalSize;
        }

        protected internal override void DecodeBody(ref SequenceReader<byte> reader, object context)
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
                if (reader.TryReadBigEndian(out short topicLen) && reader.Remaining >= topicLen + 1)
                {
                    var topic = reader.ReadString(topicLen, Encoding.UTF8);
                    
                    if (reader.TryRead(out byte qos))
                    {
                        TopicFilters.Add(new TopicFilter { Topic = topic, QoS = qos });
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }
}
