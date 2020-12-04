using SuperSocket.Command;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace SuperSocket.MQTT.Packets
{
    [Command(Key = ControlPacketType.SUBSCRIBE)]
    public class SUBSCRIBE : IAsyncCommand<MQTTPacket>
    {
        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package)
        {
           
            var Session = session as MQTTSession;
            var subpacket = package as SubscribePacket;
            Session.TopicNames.Add(subpacket);
            var buff = new byte[] { 144, 3, 0, 0, 0 };
            var PacketIdentifier = BitConverter.GetBytes(subpacket.PacketIdentifier);
            Buffer.BlockCopy(PacketIdentifier, 0, buff, 3, PacketIdentifier.Length);
            await session.SendAsync(buff);
        }
    }
}
