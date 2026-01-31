using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Command;
using SuperSocket.MQTT.Packets;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.MQTT.Server.Command
{
    [Command(Key = ControlPacketType.SUBSCRIBE)]
    public class SUBSCRIBE : IAsyncCommand<MQTTPacket>
    {
        private ArrayPool<byte> _memoryPool = ArrayPool<byte>.Shared;

        private readonly ITopicManager _topicManager;

        public SUBSCRIBE(ITopicManager topicManager)
        {
            _topicManager = topicManager;
        }

        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package, CancellationToken cancellationToken)
        {
            var mqttSession = session as MQTTSession;
            var subpacket = package as SubscribePacket;

            mqttSession.Topics.AddRange(subpacket.TopicFilters);

            // Subscribe to all topics in the packet
            foreach (var topicFilter in subpacket.TopicFilters)
            {
                _topicManager.SubscribeTopic(mqttSession, topicFilter.Topic);
            }

            // SUBACK: 2 bytes for packet identifier + 1 byte per topic filter for return code
            var topicCount = subpacket.TopicFilters.Count;
            var responseLength = 2 + topicCount;
            var buffer = _memoryPool.Rent(2 + responseLength);

            WriteBuffer(buffer, subpacket, responseLength);

            try
            {
                await session.SendAsync(buffer.AsMemory()[..(2 + responseLength)]);
            }
            finally
            {
                _memoryPool.Return(buffer);
            }
        }
        
        private void WriteBuffer(byte[] buffer, SubscribePacket packet, int remainingLength)
        {
            buffer[0] = 144; // SUBACK packet type (0x90)
            buffer[1] = (byte)remainingLength;

            BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan().Slice(2), packet.PacketIdentifier);

            // Write return code (granted QoS) for each topic filter
            for (int i = 0; i < packet.TopicFilters.Count; i++)
            {
                // Return the granted QoS (same as requested for now)
                // Valid values: 0x00 (QoS 0), 0x01 (QoS 1), 0x02 (QoS 2), 0x80 (Failure)
                buffer[4 + i] = packet.TopicFilters[i].QoS;
            }
        }
    }
}
