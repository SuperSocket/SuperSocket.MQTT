using SuperSocket.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.MQTT.Packets
{
    [Command(Key = ControlPacketType.PUBACK)]
    public class PUBACK : IAsyncCommand<MQTTPacket>
    {
        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package)
        {
            var PubAckPacket = package as PubAckPacket;
           await session.SendAsync(PubAckPacket.PacketData);
        }
    }
}
