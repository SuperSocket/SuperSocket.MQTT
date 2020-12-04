using SuperSocket.Command;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace SuperSocket.MQTT.Packets
{
    [Command(Key = ControlPacketType.UNSUBSCRIBE)]
    public class UNSUBSCRIBE : IAsyncCommand<MQTTPacket>
    {
        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package)
        {
            var Session = session as MQTTSession;
            var UnsubscribePacket = package as UnsubscribePacket;
            var buff = new byte[] { 176, 2, 0, 02 };
            buff[3] = UnsubscribePacket.PacketIdentifier;
            await session.SendAsync(buff);
        }
    }
}
