using System.Threading.Tasks;
using SuperSocket.Command;
using SuperSocket.MQTT.Packets;

namespace SuperSocket.MQTT.Server.Command
{
    [Command(Key = ControlPacketType.PUBACK)]
    public class PUBACK : IAsyncCommand<MQTTPacket>
    {
        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package)
        {
            var pubAckPacket = package as PubAckPacket;
           await session.SendAsync(pubAckPacket.PacketData);
        }
    }
}
