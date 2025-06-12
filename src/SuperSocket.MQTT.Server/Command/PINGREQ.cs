using System;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Command;
using SuperSocket.MQTT.Packets;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.MQTT.Server.Command
{
    [Command(Key = ControlPacketType.PINGREQ)]
    public class PINGREQ : IAsyncCommand<MQTTPacket>
    {
        private static readonly byte[] _pingRespData = new byte[] { 0xD0, 0x00 }; // PINGRESP packet

        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package, CancellationToken cancellationToken)
        {
            // Send PINGRESP
            await session.SendAsync(_pingRespData);
        }
    }
}
