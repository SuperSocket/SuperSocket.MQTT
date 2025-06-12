using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Middleware;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.MQTT.Server
{
    public class TopicMiddleware : MiddlewareBase, ITopicManager
    {
        private ConcurrentDictionary<string, ConcurrentDictionary<string, MQTTSession>> _topics = new ConcurrentDictionary<string, ConcurrentDictionary<string, MQTTSession>>(StringComparer.OrdinalIgnoreCase);

        public override void Shutdown(IServer server)
        {
            _topics.Clear();
        }

        void ITopicManager.SubscribeTopic(MQTTSession session, string topic)
        {
            _topics.AddOrUpdate(topic,
                t =>
                {
                    var subscribedSessions = new ConcurrentDictionary<string, MQTTSession>(StringComparer.OrdinalIgnoreCase);
                    subscribedSessions.TryAdd(session.SessionID, session);
                    return subscribedSessions;
                },
                (t, subscribedSessions) =>
                {
                    subscribedSessions.TryAdd(session.SessionID, session);
                    return subscribedSessions;
                });
        }

        void ITopicManager.UnsubscribeTopic(MQTTSession session, string topic)
        {
            if (_topics.TryGetValue(topic, out var subscribedSessions))
            {
                subscribedSessions.Remove(session.SessionID, out _);
            }
        }

        public override ValueTask<bool> UnRegisterSession(IAppSession session)
        {
            var topics = (session as MQTTSession).TopicNames;            

            foreach (var topic in topics)
            {
                foreach (var topicFilter in topic.TopicFilters)
                {
                    if (_topics.TryGetValue(topicFilter.Topic, out var subscribedSessions))
                    {
                        subscribedSessions.Remove(session.SessionID, out _);
                    }
                }
            }

            return ValueTask.FromResult(true);
        }

        public IEnumerable<MQTTSession> GetSubscribedSessions(string topic)
        {
            if (_topics.TryGetValue(topic, out var subscribedSubscriptions))
            {
                return subscribedSubscriptions.Values;
            }

            return Array.Empty<MQTTSession>();
        }
    }
}