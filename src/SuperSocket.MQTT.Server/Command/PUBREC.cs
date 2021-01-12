using System.Threading.Tasks;
using SuperSocket.Command;
using SuperSocket.MQTT.Packets;

namespace SuperSocket.MQTT.Server.Command
{
    [Command(Key = ControlPacketType.PUBREC)]
    public class PUBREC : IAsyncCommand<MQTTPacket>
    {
        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package)
        {
            var pubRecPacket = package as PubRecPacket;
            await session.SendAsync(pubRecPacket.PacketData);
        }
    }
}
