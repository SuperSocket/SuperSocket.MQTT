using SuperSocket.Command;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.MQTT.Packets
{
    [Command(Key = ControlPacketType.PINGREQ)]
    public class PINGREQ : IAsyncCommand<MQTTPacket>
    {
        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package)
        {
            await session.SendAsync(new byte[] { 208, 0 });
        }
    }
}
