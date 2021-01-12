
using System;
using System.Threading.Tasks;
using System.Buffers;
using SuperSocket.Command;

namespace SuperSocket.MQTT.Packets
{
    [Command(Key = ControlPacketType.UNSUBSCRIBE)]
    public class UNSUBSCRIBE : IAsyncCommand<MQTTPacket>
    {
        private ArrayPool<byte> _memoryPool = ArrayPool<byte>.Shared;
        
        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package)
        {
            var pqttSession = session as MQTTSession;
            var unsubscribePacket = package as UnsubscribePacket;

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
