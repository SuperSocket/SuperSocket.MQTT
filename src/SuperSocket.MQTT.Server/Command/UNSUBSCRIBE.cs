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

            _topicManager.UnsubscribeTopic(mqttSession, unsubscribePacket.TopicName);

            var buffer = _memoryPool.Rent(4);

            buffer[0] = 176;
            buffer[1] = 2;
            buffer[2] = unsubscribePacket.PacketIdentifier;
            buffer[3] = 2;

            try
            {
                await session.SendAsync(buffer);
            }
            finally
            {
                _memoryPool.Return(buffer);
            }            
        }
    }
}
