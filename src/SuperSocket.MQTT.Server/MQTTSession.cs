using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.MQTT.Packets;
using SuperSocket.Server;

namespace SuperSocket.MQTT.Server
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
