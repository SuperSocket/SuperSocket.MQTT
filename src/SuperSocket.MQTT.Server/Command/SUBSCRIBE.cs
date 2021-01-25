using System;
using System.Threading.Tasks;
using System.Buffers;
using System.Buffers.Binary;
using SuperSocket.MQTT.Packets;
using SuperSocket.Command;

namespace SuperSocket.MQTT.Server.Command
{
    [Command(Key = ControlPacketType.SUBSCRIBE)]
    public class SUBSCRIBE : IAsyncCommand<MQTTPacket>
    {
        private ArrayPool<byte> _memoryPool = ArrayPool<byte>.Shared;

        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package)
        {           
            var mqttSession = session as MQTTSession;
            var subpacket = package as SubscribePacket;

            mqttSession.TopicNames.Add(subpacket);

            byte[] numberBytes = BitConverter.GetBytes(subpacket.PacketIdentifier);

            byte[] data = new byte[] { 144, 3, numberBytes[1], numberBytes[0], (byte)subpacket.Qos };
            
            await session.SendAsync(data);
        }
    }
}
