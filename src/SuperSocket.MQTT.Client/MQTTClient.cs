using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Client;
using SuperSocket.MQTT.Packets;

namespace SuperSocket.MQTT.Client
{
    /// <summary>
    /// MQTT client implementation based on SuperSocket.Client.
    /// Uses the same pipeline filter as the MQTT server for protocol compatibility.
    /// </summary>
    public class MQTTClient : IAsyncDisposable
    {
        private readonly IEasyClient<MQTTPacket> _client;
        private ushort _packetIdentifier = 0; // Will be incremented to 1 before first use (0 is invalid per MQTT spec)

        /// <summary>
        /// Creates a new MQTT client instance.
        /// </summary>
        public MQTTClient()
        {
            var pipelineFilter = new MQTTPipelineFilter();
            _client = new EasyClient<MQTTPacket>(pipelineFilter);
        }

        /// <summary>
        /// Connects to an MQTT broker at the specified endpoint.
        /// </summary>
        /// <param name="endPoint">The endpoint of the MQTT broker.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the connection was successful, false otherwise.</returns>
        public async ValueTask<bool> ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken = default)
        {
            return await _client.ConnectAsync(endPoint, cancellationToken);
        }

        /// <summary>
        /// Sends an MQTT CONNECT packet and waits for CONNACK response.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="keepAlive">Keep alive interval in seconds.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The CONNACK packet received from the broker.</returns>
        public async ValueTask<ConnAckPacket> SendConnectAsync(string clientId, short keepAlive = 60, CancellationToken cancellationToken = default)
        {
            var connectPacket = new ConnectPacket
            {
                Type = ControlPacketType.CONNECT,
                ProtocolName = "MQTT",
                ProtocolLevel = 4,
                ClientId = clientId,
                KeepAlive = keepAlive
            };

            var data = MQTTPacketEncoder.Encode(connectPacket);
            await _client.SendAsync(data);

            var response = await _client.ReceiveAsync();
            return response as ConnAckPacket;
        }

        /// <summary>
        /// Sends an MQTT PINGREQ packet and waits for PINGRESP response.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The PINGRESP packet received from the broker.</returns>
        public async ValueTask<PingRespPacket> SendPingAsync(CancellationToken cancellationToken = default)
        {
            var pingPacket = new PingReqPacket
            {
                Type = ControlPacketType.PINGREQ
            };

            var data = MQTTPacketEncoder.Encode(pingPacket);
            await _client.SendAsync(data);

            var response = await _client.ReceiveAsync();
            return response as PingRespPacket;
        }

        /// <summary>
        /// Sends an MQTT SUBSCRIBE packet and waits for SUBACK response.
        /// </summary>
        /// <param name="topicFilters">The topics to subscribe to.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The SUBACK packet received from the broker.</returns>
        public async ValueTask<SubAckPacket> SendSubscribeAsync(IEnumerable<TopicFilter> topicFilters, CancellationToken cancellationToken = default)
        {
            var packetId = GetNextPacketIdentifier();
            var subscribePacket = new SubscribePacket
            {
                Type = ControlPacketType.SUBSCRIBE,
                Flags = 0x02, // SUBSCRIBE must have flag bits set to 0010
                PacketIdentifier = packetId,
                TopicFilters = new List<TopicFilter>(topicFilters)
            };

            var data = MQTTPacketEncoder.Encode(subscribePacket);
            await _client.SendAsync(data);

            var response = await _client.ReceiveAsync();
            return response as SubAckPacket;
        }

        /// <summary>
        /// Sends an MQTT SUBSCRIBE packet for a single topic and waits for SUBACK response.
        /// </summary>
        /// <param name="topic">The topic to subscribe to.</param>
        /// <param name="qos">The QoS level for the subscription.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The SUBACK packet received from the broker.</returns>
        public async ValueTask<SubAckPacket> SendSubscribeAsync(string topic, byte qos = 0, CancellationToken cancellationToken = default)
        {
            return await SendSubscribeAsync(new[] { new TopicFilter { Topic = topic, QoS = qos } }, cancellationToken);
        }

        /// <summary>
        /// Sends an MQTT PUBLISH packet.
        /// </summary>
        /// <param name="topic">The topic to publish to.</param>
        /// <param name="payload">The message payload.</param>
        /// <param name="qos">The QoS level.</param>
        /// <param name="retain">Whether to retain the message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the async operation. For QoS > 0, returns the acknowledgement packet.</returns>
        public async ValueTask<MQTTPacket> SendPublishAsync(string topic, byte[] payload, byte qos = 0, bool retain = false, CancellationToken cancellationToken = default)
        {
            var packetId = qos > 0 ? GetNextPacketIdentifier() : (ushort)0;
            
            byte flags = (byte)((qos & 0x03) << 1);
            if (retain)
            {
                flags |= 0x01;
            }
            
            var publishPacket = new PublishPacket
            {
                Type = ControlPacketType.PUBLISH,
                Flags = flags,
                TopicName = topic,
                PacketIdentifier = packetId,
                Qos = qos,
                Retain = retain,
                Payload = new ReadOnlyMemory<byte>(payload)
            };

            var data = MQTTPacketEncoder.Encode(publishPacket);
            await _client.SendAsync(data);

            if (qos > 0)
            {
                var response = await _client.ReceiveAsync();
                return response;
            }

            return null;
        }

        /// <summary>
        /// Sends an MQTT DISCONNECT packet.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async ValueTask SendDisconnectAsync(CancellationToken cancellationToken = default)
        {
            var disconnectPacket = new DisconnectPacket
            {
                Type = ControlPacketType.DISCONNECT
            };

            var data = MQTTPacketEncoder.Encode(disconnectPacket);
            await _client.SendAsync(data);
        }

        /// <summary>
        /// Receives an MQTT packet from the broker.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The received MQTT packet.</returns>
        public async ValueTask<MQTTPacket> ReceiveAsync(CancellationToken cancellationToken = default)
        {
            return await _client.ReceiveAsync();
        }

        /// <summary>
        /// Closes the connection to the broker.
        /// </summary>
        /// <returns>A task representing the async close operation.</returns>
        public async ValueTask CloseAsync()
        {
            await _client.CloseAsync();
        }

        /// <summary>
        /// Disposes the MQTT client asynchronously.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await CloseAsync();
        }

        private ushort GetNextPacketIdentifier()
        {
            return ++_packetIdentifier;
        }
    }
}
