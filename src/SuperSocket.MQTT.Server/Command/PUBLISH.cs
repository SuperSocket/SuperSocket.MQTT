using SuperSocket.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.MQTT.Packets
{
    [Command(Key = ControlPacketType.PUBLISH)]
    public class PUBLISH : IAsyncCommand<MQTTPacket>
    {
        private const char TopicLevelSeparator = '/';
        private readonly ISessionContainer sessionContainer;

        public PUBLISH(ISessionContainer sessionContainer)
        {
            this.sessionContainer = sessionContainer;
        }
        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package)
        {
            var pubpacket = package as PublishPacket;
            var sessions = sessionContainer.GetSessions<MQTTSession>();
            var b = sessions.Where(x => x.TopicNames.Any(x => IsMatch(pubpacket.TopicName,x.TopicName)));
            await Task.Run(() =>
            {
                foreach (var item in b)
                {
                    item.SendAsync(pubpacket.TopicData);
                }
            });
        }
        private bool IsMatch(string topic, string filter)
        {
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            if (string.Equals(topic, filter, StringComparison.Ordinal))
            {
                return true;
            }

            var fragmentsTopic = topic.Split(new[] { TopicLevelSeparator }, StringSplitOptions.None);
            var fragmentsFilter = filter.Split(new[] { TopicLevelSeparator }, StringSplitOptions.None);

            for (var i = 0; i < fragmentsFilter.Length; i++)
            {
                if (fragmentsFilter[i] == "+")
                {
                    continue;
                }

                if (fragmentsFilter[i] == "#" && i == fragmentsFilter.Length - 1)
                {
                    return true;
                }

                if (i >= fragmentsTopic.Length)
                {
                    return false;
                }

                if (!string.Equals(fragmentsFilter[i], fragmentsTopic[i]))
                {
                    return false;
                }
            }

            if (fragmentsTopic.Length > fragmentsFilter.Length)
            {
                return false;
            }

            return true;
        }
    }
}
