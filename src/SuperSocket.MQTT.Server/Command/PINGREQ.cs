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
        private static readonly byte[] _pingData = new byte[] { 208, 0 };

        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package, CancellationToken cancellationToken)
        {
            await session.SendAsync(_pingData);
        }
    }
}
