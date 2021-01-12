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
        private static readonly byte[] _pingData = new byte[] { 208, 0 };

        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package)
        {
            await session.SendAsync(_pingData);
        }
    }
}
