using SuperSocket.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.MQTT.Packets
{
    [Command(Key = ControlPacketType.PUBREL)]
    public class PUBREL : IAsyncCommand<MQTTPacket>
    {
        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package)
        {
            var PubRelPacket = package as PubRelPacket;
           await session.SendAsync(PubRelPacket.PacketData);
        }
    }
}
