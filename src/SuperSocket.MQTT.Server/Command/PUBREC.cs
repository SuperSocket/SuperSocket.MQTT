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
    /// Handles PUBREC (Publish Received) control packets for QoS 2 publish flow.
    /// When server receives PUBREC, it responds with PUBREL (Publish Release).
    /// </summary>
    [Command(Key = ControlPacketType.PUBREC)]
    public class PUBREC : IAsyncCommand<MQTTPacket>
    {
        private ArrayPool<byte> _memoryPool = ArrayPool<byte>.Shared;
        
        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package, CancellationToken cancellationToken)
        {
            var pubRecPacket = package as PubRecPacket;
            
            // Respond with PUBREL (Publish Release) - fixed header byte: 0x62 (type 6 with reserved bits 0010)
            var buffer = _memoryPool.Rent(4);
            
            buffer[0] = 0x62; // PUBREL packet type (0110 0010)
            buffer[1] = 2;    // Remaining length
            buffer[2] = (byte)(pubRecPacket.PacketIdentifier >> 8);
            buffer[3] = (byte)(pubRecPacket.PacketIdentifier & 0xFF);
            
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
