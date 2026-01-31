using System;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Command;
using SuperSocket.MQTT.Packets;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.MQTT.Server.Command
{
    /// <summary>
    /// Handles DISCONNECT control packets from clients.
    /// When a client sends DISCONNECT, the server should close the connection cleanly.
    /// </summary>
    [Command(Key = ControlPacketType.DISCONNECT)]
    public class DISCONNECT : IAsyncCommand<MQTTPacket>
    {
        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package, CancellationToken cancellationToken)
        {
            // Close the session gracefully when client sends DISCONNECT
            // The session cleanup (unsubscribing from topics) is handled by TopicMiddleware.UnRegisterSession
            await session.CloseAsync(SuperSocket.Connection.CloseReason.RemoteClosing);
        }
    }
}
