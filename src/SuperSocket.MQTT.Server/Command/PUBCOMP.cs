using SuperSocket.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.MQTT.Packets
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
