using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Command;
using SuperSocket.MQTT.Packets;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.MQTT.Server.Command
{
    [Command(Key = ControlPacketType.UNSUBSCRIBE)]
    public class UNSUBSCRIBE : IAsyncCommand<MQTTPacket>
    {
        private ArrayPool<byte> _memoryPool = ArrayPool<byte>.Shared;

        private readonly ITopicManager _topicManager;

        public UNSUBSCRIBE(ITopicManager topicManager)
        {
            _topicManager = topicManager;
        }

        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package, CancellationToken cancellationToken)
        {
            var mqttSession = session as MQTTSession;
            var unsubscribePacket = package as UnsubscribePacket;

            // Unsubscribe from all topics in the packet
            foreach (var topic in unsubscribePacket.TopicFilters)
            {
                _topicManager.UnsubscribeTopic(mqttSession, topic);
            }

            var buffer = _memoryPool.Rent(4);

            buffer[0] = 176;
            buffer[1] = 2;
            buffer[2] = (byte)(unsubscribePacket.PacketIdentifier >> 8);
            buffer[3] = (byte)(unsubscribePacket.PacketIdentifier & 0xFF);

            try
            {
                await session.SendAsync(buffer.AsMemory()[..4]);
            }
            finally
            {
                _memoryPool.Return(buffer);
            }            
        }
    }
}
