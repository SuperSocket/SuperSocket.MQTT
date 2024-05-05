using System;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Command;
using SuperSocket.MQTT.Packets;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.MQTT.Server.Command
{
    [Command(Key = ControlPacketType.CONNECT)]
    public class CONNECT : IAsyncCommand<MQTTPacket>
    {
        private static readonly byte[] _connectData = new byte[] { 32, 2, 0, 0 };

        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package, CancellationToken cancellationToken)
        {
            var connectPacket = package as ConnectPacket;
            await session.SendAsync(_connectData);
        }
    }
}