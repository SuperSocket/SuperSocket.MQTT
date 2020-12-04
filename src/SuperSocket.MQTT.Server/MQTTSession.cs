using SuperSocket.MQTT.Packets;
using SuperSocket.Server;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.MQTT
{
    public class MQTTSession : AppSession
    {
        public List<SubscribePacket> TopicNames = new List<SubscribePacket>();

        public ValueTask SendAsync(ReadOnlyMemory<byte> data)
        {
            return (this as IAppSession).SendAsync(data);
        }
    }
}
