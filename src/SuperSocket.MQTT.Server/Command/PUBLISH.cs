using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Command;
using SuperSocket.MQTT.Packets;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.MQTT.Server.Command
{
    [Command(Key = ControlPacketType.PUBLISH)]
    public class PUBLISH : IAsyncCommand<MQTTPacket>
    {
        private readonly ISessionContainer sessionContainer;

        public PUBLISH(ISessionContainer sessionContainer)
        {
            this.sessionContainer = sessionContainer;
        }
        public async ValueTask ExecuteAsync(IAppSession session, MQTTPacket package, CancellationToken cancellationToken)
        {
            var pubpacket = package as PublishPacket;

            if (string.IsNullOrEmpty(pubpacket.TopicName))
            {
                return;
            }

            var sessions = sessionContainer.GetSessions<MQTTSession>();

            var topicSegments = pubpacket.TopicName.Split(MQTTConst.TopicLevelSeparator, StringSplitOptions.RemoveEmptyEntries);

            var subscribedSessions = sessions.Where(x => x.Topics.Any(subscription => 
                string.Equals(pubpacket.TopicName, subscription.Topic, StringComparison.Ordinal)
                || IsMatchBySegment(topicSegments, subscription.TopicSegments)));

            // Broadcast the message to all subscribed sessions
            // To be optimized by parellel sending later
            foreach (var subSession in subscribedSessions)
            {
                await subSession.SendAsync(pubpacket.Payload);
            }
        }

        private bool IsMatchBySegment(IReadOnlyList<string> topicSegments, IReadOnlyList<string> filterSegments)
        {
            for (var i = 0; i < filterSegments.Count; i++)
            {
                if (filterSegments[i] == "+")
                {
                    continue;
                }

                if (filterSegments[i] == "#" && i == filterSegments.Count - 1)
                {
                    return true;
                }

                if (i >= topicSegments.Count)
                {
                    return false;
                }

                if (!string.Equals(filterSegments[i], topicSegments[i]))
                {
                    return false;
                }
            }

            if (topicSegments.Count > filterSegments.Count)
            {
                return false;
            }

            return true;
        }
    }
}
