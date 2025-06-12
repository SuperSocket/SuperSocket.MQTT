using System;
using System.Buffers;
using System.Text;
using Xunit;
using SuperSocket.MQTT.Packets;

namespace SuperSocket.MQTT.Tests
{
    public class MQTTPacketTests
    {
        [Fact]
        public void ConnectPacket_EncodeBody_ShouldWork()
        {
            // Arrange
            var connectPacket = new ConnectPacket
            {
                ProtocolName = "MQTT",
                ProtocolLevel = 4,
                KeepAlive = 60,
                ClientId = "TestClient123",
                UserName = "user1",
                Password = "pass1"
            };

            // Act - Encode
            var buffer = new ArrayBufferWriter<byte>();
            var encodedLength = connectPacket.EncodeBody(buffer);
            var encodedData = buffer.WrittenSpan.ToArray();

            // Assert - Basic validation
            Assert.True(encodedLength > 0);
            Assert.True(encodedData.Length > 0);
            
            // Verify protocol name is at the beginning
            var protocolNameBytes = Encoding.UTF8.GetBytes("MQTT");
            Assert.Equal(protocolNameBytes.Length, (encodedData[0] << 8) | encodedData[1]);
        }

        [Fact]
        public void ConnAckPacket_EncodeBody_ShouldWork()
        {
            // Arrange
            var connAckPacket = new ConnAckPacket
            {
                SessionPresent = 1,
                ReturnCode = 0 // Connection Accepted
            };

            // Act - Encode
            var buffer = new ArrayBufferWriter<byte>();
            var encodedLength = connAckPacket.EncodeBody(buffer);
            var encodedData = buffer.WrittenSpan.ToArray();

            // Assert
            Assert.Equal(2, encodedLength);
            Assert.Equal(1, encodedData[0]); // SessionPresent
            Assert.Equal(0, encodedData[1]); // ReturnCode
        }

        [Fact]
        public void PublishPacket_QoS0_EncodeBody_ShouldWork()
        {
            // Arrange
            var payload = Encoding.UTF8.GetBytes("Hello MQTT!");
            var publishPacket = new PublishPacket
            {
                TopicName = "test/topic",
                Qos = 0,
                Dup = false,
                Retain = false,
                Payload = payload
            };

            // Act - Encode
            var buffer = new ArrayBufferWriter<byte>();
            var encodedLength = publishPacket.EncodeBody(buffer);
            var encodedData = buffer.WrittenSpan.ToArray();

            // Assert
            Assert.True(encodedLength > 0);
            Assert.True(encodedData.Length > 0);
            
            // Verify topic name length and content
            var topicBytes = Encoding.UTF8.GetBytes("test/topic");
            Assert.Equal(topicBytes.Length, (encodedData[0] << 8) | encodedData[1]);
        }

        [Fact]
        public void SubscribePacket_EncodeBody_ShouldWork()
        {
            // Arrange
            var subscribePacket = new SubscribePacket
            {
                PacketIdentifier = 1234,
                TopicFilters = new()
                {
                    new TopicFilter { Topic = "test/topic1", QoS = 0 },
                    new TopicFilter { Topic = "test/topic2", QoS = 1 }
                }
            };

            // Act - Encode
            var buffer = new ArrayBufferWriter<byte>();
            var encodedLength = subscribePacket.EncodeBody(buffer);
            var encodedData = buffer.WrittenSpan.ToArray();

            // Assert
            Assert.True(encodedLength > 0);
            Assert.True(encodedData.Length > 0);
            
            // Verify packet identifier
            Assert.Equal(1234, (encodedData[0] << 8) | encodedData[1]);
        }

        [Fact]
        public void SubAckPacket_EncodeBody_ShouldWork()
        {
            // Arrange
            var subAckPacket = new SubAckPacket
            {
                PacketIdentifier = 1234,
                ReturnCodes = new() { 0, 1, 2, 0x80 } // QoS 0, 1, 2, and failure
            };

            // Act - Encode
            var buffer = new ArrayBufferWriter<byte>();
            var encodedLength = subAckPacket.EncodeBody(buffer);
            var encodedData = buffer.WrittenSpan.ToArray();

            // Assert
            Assert.Equal(6, encodedLength); // 2 bytes packet ID + 4 return codes
            Assert.Equal(1234, (encodedData[0] << 8) | encodedData[1]);
            Assert.Equal(0, encodedData[2]);
            Assert.Equal(1, encodedData[3]);
            Assert.Equal(2, encodedData[4]);
            Assert.Equal(0x80, encodedData[5]);
        }

        [Fact]
        public void UnsubscribePacket_EncodeBody_ShouldWork()
        {
            // Arrange
            var unsubscribePacket = new UnsubscribePacket
            {
                PacketIdentifier = 5678,
                TopicFilters = new() { "test/topic1", "test/topic2" }
            };

            // Act - Encode
            var buffer = new ArrayBufferWriter<byte>();
            var encodedLength = unsubscribePacket.EncodeBody(buffer);
            var encodedData = buffer.WrittenSpan.ToArray();

            // Assert
            Assert.True(encodedLength > 0);
            Assert.True(encodedData.Length > 0);
            
            // Verify packet identifier
            Assert.Equal(5678, (encodedData[0] << 8) | encodedData[1]);
        }

        [Fact]
        public void UnsubAckPacket_EncodeBody_ShouldWork()
        {
            // Arrange
            var unsubAckPacket = new UnsubAckPacket
            {
                PacketIdentifier = 5678
            };

            // Act - Encode
            var buffer = new ArrayBufferWriter<byte>();
            var encodedLength = unsubAckPacket.EncodeBody(buffer);
            var encodedData = buffer.WrittenSpan.ToArray();

            // Assert
            Assert.Equal(2, encodedLength);
            Assert.Equal(5678, (encodedData[0] << 8) | encodedData[1]);
        }

        [Fact]
        public void PubAckPacket_EncodeBody_ShouldWork()
        {
            // Arrange
            var pubAckPacket = new PubAckPacket
            {
                PacketIdentifier = 9999
            };

            // Act - Encode
            var buffer = new ArrayBufferWriter<byte>();
            var encodedLength = pubAckPacket.EncodeBody(buffer);
            var encodedData = buffer.WrittenSpan.ToArray();

            // Assert
            Assert.Equal(2, encodedLength);
            Assert.Equal(9999, (encodedData[0] << 8) | encodedData[1]);
        }

        [Fact]
        public void PingReqPacket_EncodeBody_ShouldWork()
        {
            // Arrange
            var pingReqPacket = new PingReqPacket();

            // Act - Encode
            var buffer = new ArrayBufferWriter<byte>();
            var encodedLength = pingReqPacket.EncodeBody(buffer);

            // Assert - PINGREQ has no payload
            Assert.Equal(0, encodedLength);
        }

        [Fact]
        public void PingRespPacket_EncodeBody_ShouldWork()
        {
            // Arrange
            var pingRespPacket = new PingRespPacket();

            // Act - Encode
            var buffer = new ArrayBufferWriter<byte>();
            var encodedLength = pingRespPacket.EncodeBody(buffer);

            // Assert - PINGRESP has no payload
            Assert.Equal(0, encodedLength);
        }

        [Fact]
        public void DisconnectPacket_EncodeBody_ShouldWork()
        {
            // Arrange
            var disconnectPacket = new DisconnectPacket();

            // Act - Encode
            var buffer = new ArrayBufferWriter<byte>();
            var encodedLength = disconnectPacket.EncodeBody(buffer);

            // Assert - DISCONNECT has no payload
            Assert.Equal(0, encodedLength);
        }
    }
}
