using System;
using System.Threading.Tasks;
using SuperSocket.Command;
using SuperSocket.MQTT.Packets;

namespace SuperSocket.MQTT.Server.Command
{
    [Command(Key = ControlPacketType.CONNECT)]
    public class CONNECT : IAsyncCommand<MQTTPacket>
    {
        private static readonly byte[] _connectData = new byte[] { 32, 2, 0, 0 };

        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package)
        {
            var connectPacket = package as ConnectPacket;
            await session.SendAsync(_connectData);
        }
    }
}