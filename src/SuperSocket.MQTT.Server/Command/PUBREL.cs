using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Command;
using SuperSocket.MQTT.Packets;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.MQTT.Server.Command
{
    [Command(Key = ControlPacketType.PUBREL)]
    public class PUBREL : IAsyncCommand<MQTTPacket>
    {
        private ArrayPool<byte> _memoryPool = ArrayPool<byte>.Shared;
        
        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package, CancellationToken cancellationToken)
        {
            var pubRelPacket = package as PubRelPacket;
            
            // Create a response with the same packet identifier
            var buffer = _memoryPool.Rent(4);
            
            buffer[0] = 0x60; // PUBREL packet type
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
