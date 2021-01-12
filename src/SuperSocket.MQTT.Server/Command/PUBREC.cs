using SuperSocket.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.MQTT.Packets
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
