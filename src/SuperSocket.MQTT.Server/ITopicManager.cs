namespace SuperSocket.MQTT.Server
{
    public interface ITopicManager
    {
        void SubscribeTopic(MQTTSession session, string topic);

        void UnsubscribeTopic(MQTTSession session, string topic);
    }
}