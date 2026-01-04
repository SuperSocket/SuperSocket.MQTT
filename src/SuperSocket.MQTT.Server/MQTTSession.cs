using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.MQTT.Packets;
using SuperSocket.Server;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.MQTT.Server
{
    public class MQTTSession : AppSession
    {
        public List<TopicFilter> Topics = new List<TopicFilter>();

        public ValueTask SendAsync(ReadOnlyMemory<byte> data)
        {
            return (this as IAppSession).SendAsync(data);
        }
    }
}
