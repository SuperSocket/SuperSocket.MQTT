using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SuperSocket.MQTT.Packets;
using SuperSocket.MQTT.Server;
using SuperSocket.MQTT.Client;
using SuperSocket.Server;
using SuperSocket.Server.Host;

namespace SuperSocket.MQTT.Tests
{
    /// <summary>
    /// End-to-end integration tests using the MQTTClient to connect to the MQTT server.
    /// These tests verify the full request/response flow between client and server.
    /// </summary>
    public class MQTTClientE2ETests : IAsyncLifetime
    {
        private IHost? _host;
        private const int TestPort = 21883;
        private readonly IPEndPoint _serverEndPoint = new IPEndPoint(IPAddress.Loopback, TestPort);

        public async Task InitializeAsync()
        {
            _host = SuperSocketHostBuilder
                .Create<MQTTPacket>()
                .UseMQTT()
                .UseInProcSessionContainer()
                .ConfigureAppConfiguration((hostCtx, configApp) =>
                {
                    configApp.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        { "serverOptions:name", "MQTTTestServer" },
                        { "serverOptions:listeners:0:ip", "Any" },
                        { "serverOptions:listeners:0:port", TestPort.ToString() }
                    });
                })
                .Build();

            await _host.StartAsync();
            // Give the server time to start listening
            await Task.Delay(100);
        }

        public async Task DisposeAsync()
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
        }

        [Fact]
        public async Task E2E_ConnectToServer_ShouldReceiveConnAck()
        {
            // Arrange
            await using var client = new MQTTClient();
            
            // Act
            var connected = await client.ConnectAsync(_serverEndPoint);
            Assert.True(connected, "Should connect to server");

            var connAck = await client.SendConnectAsync("TestClient_" + Guid.NewGuid().ToString("N")[..8]);

            // Assert
            Assert.NotNull(connAck);
            Assert.Equal(ControlPacketType.CONNACK, connAck.Type);
            Assert.Equal(0, connAck.ReturnCode); // Connection accepted
        }

        [Fact]
        public async Task E2E_PingServer_ShouldReceivePingResp()
        {
            // Arrange
            await using var client = new MQTTClient();
            var connected = await client.ConnectAsync(_serverEndPoint);
            Assert.True(connected, "Should connect to server");

            await client.SendConnectAsync("TestClient_" + Guid.NewGuid().ToString("N")[..8]);

            // Act
            var pingResp = await client.SendPingAsync();

            // Assert
            Assert.NotNull(pingResp);
            Assert.Equal(ControlPacketType.PINGRESP, pingResp.Type);
        }

        [Fact]
        public async Task E2E_SubscribeToTopic_ShouldReceiveSubAck()
        {
            // Arrange
            await using var client = new MQTTClient();
            var connected = await client.ConnectAsync(_serverEndPoint);
            Assert.True(connected, "Should connect to server");

            await client.SendConnectAsync("TestClient_" + Guid.NewGuid().ToString("N")[..8]);

            // Act
            var subAck = await client.SendSubscribeAsync("test/topic", qos: 0);

            // Assert
            Assert.NotNull(subAck);
            Assert.Equal(ControlPacketType.SUBACK, subAck.Type);
            Assert.Single(subAck.ReturnCodes);
            Assert.Equal(0, subAck.ReturnCodes[0]); // QoS 0 granted
        }

        [Fact]
        public async Task E2E_SubscribeMultipleTopics_ShouldReceiveSubAckWithAllReturnCodes()
        {
            // Arrange
            await using var client = new MQTTClient();
            var connected = await client.ConnectAsync(_serverEndPoint);
            Assert.True(connected, "Should connect to server");

            await client.SendConnectAsync("TestClient_" + Guid.NewGuid().ToString("N")[..8]);

            var topicFilters = new List<TopicFilter>
            {
                new TopicFilter { Topic = "home/temperature", QoS = 0 },
                new TopicFilter { Topic = "home/humidity", QoS = 1 },
                new TopicFilter { Topic = "home/pressure", QoS = 2 }
            };

            // Act
            var subAck = await client.SendSubscribeAsync(topicFilters);

            // Assert
            Assert.NotNull(subAck);
            Assert.Equal(ControlPacketType.SUBACK, subAck.Type);
            Assert.Equal(3, subAck.ReturnCodes.Count);
            Assert.Equal(0, subAck.ReturnCodes[0]); // QoS 0 granted
            Assert.Equal(1, subAck.ReturnCodes[1]); // QoS 1 granted
            Assert.Equal(2, subAck.ReturnCodes[2]); // QoS 2 granted
        }

        [Fact]
        public async Task E2E_PublishQoS0_ShouldSucceed()
        {
            // Arrange
            await using var client = new MQTTClient();
            var connected = await client.ConnectAsync(_serverEndPoint);
            Assert.True(connected, "Should connect to server");

            await client.SendConnectAsync("TestClient_" + Guid.NewGuid().ToString("N")[..8]);

            // Act - QoS 0 publish should not return an acknowledgement packet
            var result = await client.SendPublishAsync("test/topic", Encoding.UTF8.GetBytes("Hello MQTT!"), qos: 0);

            // Assert - for QoS 0, no acknowledgement is expected
            Assert.Null(result);
        }

        [Fact]
        public async Task E2E_DisconnectFromServer_ShouldComplete()
        {
            // Arrange
            await using var client = new MQTTClient();
            var connected = await client.ConnectAsync(_serverEndPoint);
            Assert.True(connected, "Should connect to server");

            await client.SendConnectAsync("TestClient_" + Guid.NewGuid().ToString("N")[..8]);

            // Act - disconnect should complete without exception
            await client.SendDisconnectAsync();
        }

        [Fact]
        public async Task E2E_FullFlow_ConnectSubscribePingDisconnect()
        {
            // Arrange
            await using var client = new MQTTClient();

            // Act & Assert - Connect
            var connected = await client.ConnectAsync(_serverEndPoint);
            Assert.True(connected, "Should connect to server");

            // Act & Assert - MQTT Connect
            var connAck = await client.SendConnectAsync("TestClient_FullFlow");
            Assert.NotNull(connAck);
            Assert.Equal(0, connAck.ReturnCode);

            // Act & Assert - Subscribe
            var subAck = await client.SendSubscribeAsync("test/fullflow", qos: 1);
            Assert.NotNull(subAck);
            Assert.Single(subAck.ReturnCodes);

            // Act & Assert - Ping
            var pingResp = await client.SendPingAsync();
            Assert.NotNull(pingResp);
            Assert.Equal(ControlPacketType.PINGRESP, pingResp.Type);

            // Act & Assert - Disconnect
            await client.SendDisconnectAsync();
        }
    }
}
