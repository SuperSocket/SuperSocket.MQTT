using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Command;
using SuperSocket.MQTT.Packets;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.MQTT.Server.Command
{
    /// <summary>
    /// Handles PUBREL (Publish Release) control packets for QoS 2 publish flow.
    /// When server receives PUBREL, it responds with PUBCOMP (Publish Complete).
    /// </summary>
    [Command(Key = ControlPacketType.PUBREL)]
    public class PUBREL : IAsyncCommand<MQTTPacket>
    {
        private ArrayPool<byte> _memoryPool = ArrayPool<byte>.Shared;
        
        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package, CancellationToken cancellationToken)
        {
            var pubRelPacket = package as PubRelPacket;
            
            // Respond with PUBCOMP (Publish Complete) - fixed header byte: 0x70
            var buffer = _memoryPool.Rent(4);
            
            buffer[0] = 0x70; // PUBCOMP packet type
            buffer[1] = 2;    // Remaining length
            buffer[2] = (byte)(pubRelPacket.PacketIdentifier >> 8);
            buffer[3] = (byte)(pubRelPacket.PacketIdentifier & 0xFF);
            
            try
            {
                await session.SendAsync(buffer.AsMemory()[..4]);
            }
            finally
            {
                _memoryPool.Return(buffer);
            }
        }
    }
}
