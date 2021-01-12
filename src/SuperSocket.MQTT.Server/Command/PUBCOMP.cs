using System.Threading.Tasks;
using SuperSocket.Command;
using SuperSocket.MQTT.Packets;


namespace SuperSocket.MQTT.Server.Command
{
    [Command(Key = ControlPacketType.PUBCOMP)]
    public class PUBCOMP : IAsyncCommand<MQTTPacket>
    {
        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package)
        {
            var pubCompPacket = package as PubCompPacket;
            await session.SendAsync(pubCompPacket.PacketData);
        }
    }
}
