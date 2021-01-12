using System.Threading.Tasks;
using SuperSocket.Command;
using SuperSocket.MQTT.Packets;


namespace SuperSocket.MQTT.Server.Command
{
    [Command(Key = ControlPacketType.PUBREL)]
    public class PUBREL : IAsyncCommand<MQTTPacket>
    {
        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package)
        {
            var pubRelPacket = package as PubRelPacket;
            await session.SendAsync(pubRelPacket.PacketData);
        }
    }
}
