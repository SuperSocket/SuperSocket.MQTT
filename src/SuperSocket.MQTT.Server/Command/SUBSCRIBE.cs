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

            var buffer = _memoryPool.Rent(5);

            WriteBuffer(buffer, subpacket);

            try
            {
                await session.SendAsync(buffer.AsMemory()[..5]);
            }
            finally
            {
                _memoryPool.Return(buffer);
            }
        }
        
        private void WriteBuffer(byte[] buffer, SubscribePacket packet)
        {
            buffer[0] = 144;
            buffer[1] = 3;

            BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan().Slice(2), packet.PacketIdentifier);

            // Use the first topic filter's QoS, or default to 0
            buffer[4] = packet.TopicFilters.Count > 0 ? packet.TopicFilters[0].QoS : (byte)0;
        }
    }
}
