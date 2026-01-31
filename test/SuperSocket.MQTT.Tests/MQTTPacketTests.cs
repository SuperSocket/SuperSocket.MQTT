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

        [Fact]
        public void PubRecPacket_EncodeBody_ShouldWork()
        {
            // Arrange
            var pubRecPacket = new PubRecPacket
            {
                PacketIdentifier = 1111
            };

            // Act - Encode
            var buffer = new ArrayBufferWriter<byte>();
            var encodedLength = pubRecPacket.EncodeBody(buffer);
            var encodedData = buffer.WrittenSpan.ToArray();

            // Assert
            Assert.Equal(2, encodedLength);
            Assert.Equal(1111, (encodedData[0] << 8) | encodedData[1]);
        }

        [Fact]
        public void PubRelPacket_EncodeBody_ShouldWork()
        {
            // Arrange
            var pubRelPacket = new PubRelPacket
            {
                PacketIdentifier = 2222
            };

            // Act - Encode
            var buffer = new ArrayBufferWriter<byte>();
            var encodedLength = pubRelPacket.EncodeBody(buffer);
            var encodedData = buffer.WrittenSpan.ToArray();

            // Assert
            Assert.Equal(2, encodedLength);
            Assert.Equal(2222, (encodedData[0] << 8) | encodedData[1]);
        }

        [Fact]
        public void PubCompPacket_EncodeBody_ShouldWork()
        {
            // Arrange
            var pubCompPacket = new PubCompPacket
            {
                PacketIdentifier = 3333
            };

            // Act - Encode
            var buffer = new ArrayBufferWriter<byte>();
            var encodedLength = pubCompPacket.EncodeBody(buffer);
            var encodedData = buffer.WrittenSpan.ToArray();

            // Assert
            Assert.Equal(2, encodedLength);
            Assert.Equal(3333, (encodedData[0] << 8) | encodedData[1]);
        }

        [Fact]
        public void PublishPacket_QoS1_EncodeBody_ShouldIncludePacketIdentifier()
        {
            // Arrange
            var payload = Encoding.UTF8.GetBytes("test payload");
            var publishPacket = new PublishPacket
            {
                TopicName = "test/qos1",
                Qos = 1,
                PacketIdentifier = 4567,
                Payload = payload
            };

            // Act - Encode
            var buffer = new ArrayBufferWriter<byte>();
            var encodedLength = publishPacket.EncodeBody(buffer);
            var encodedData = buffer.WrittenSpan.ToArray();

            // Assert
            var topicBytes = Encoding.UTF8.GetBytes("test/qos1");
            // Topic length (2) + topic + packet ID (2) + payload
            var expectedLength = 2 + topicBytes.Length + 2 + payload.Length;
            Assert.Equal(expectedLength, encodedLength);

            // Verify topic length
            Assert.Equal(topicBytes.Length, (encodedData[0] << 8) | encodedData[1]);

            // Verify packet identifier (at offset 2 + topicLength)
            var packetIdOffset = 2 + topicBytes.Length;
            Assert.Equal(4567, (encodedData[packetIdOffset] << 8) | encodedData[packetIdOffset + 1]);
        }

        [Fact]
        public void ConnectPacket_WithWillMessage_EncodeBody_ShouldWork()
        {
            // Arrange
            var connectPacket = new ConnectPacket
            {
                ProtocolName = "MQTT",
                ProtocolLevel = 4,
                KeepAlive = 30,
                ClientId = "WillClient",
                WillTopic = "will/topic",
                WillMessage = "Client disconnected unexpectedly"
            };

            // Act - Encode
            var buffer = new ArrayBufferWriter<byte>();
            var encodedLength = connectPacket.EncodeBody(buffer);
            var encodedData = buffer.WrittenSpan.ToArray();

            // Assert - Basic validation that will message is included
            Assert.True(encodedLength > 0);
            
            // The connect flags byte (at offset 7 after protocol name, level) should have WillFlag set
            var connectFlags = encodedData[7];
            Assert.True((connectFlags & 0x04) != 0, "WillFlag should be set");
        }

        [Fact]
        public void MQTTPacketDecoder_ShouldRegisterAllPacketTypes()
        {
            // Arrange & Act
            var decoder = new MQTTPacketDecoder();

            // Assert - verify that all packet types are registered by creating packets
            // This test verifies the decoder initialization doesn't throw
            Assert.NotNull(decoder);
        }

        [Fact]
        public void MQTTPacketDecoder_ShouldDecodePublishPacket()
        {
            // Arrange
            var decoder = new MQTTPacketDecoder();
            var topicName = "test/topic";
            var topicBytes = Encoding.UTF8.GetBytes(topicName);
            var payload = Encoding.UTF8.GetBytes("test data");
            
            // Build PUBLISH packet: fixed header + variable header + payload
            // Fixed header: 0x3D = PUBLISH with DUP=1, QoS=2, RETAIN=1
            // Remaining length = topic length prefix (2) + topic + packet ID (2) + payload
            var remainingLength = 2 + topicBytes.Length + 2 + payload.Length;
            
            var data = new byte[2 + remainingLength]; // 1 byte type + 1 byte length + payload
            data[0] = 0x3D; // PUBLISH with flags
            data[1] = (byte)remainingLength;
            
            var offset = 2;
            data[offset++] = (byte)(topicBytes.Length >> 8);
            data[offset++] = (byte)(topicBytes.Length & 0xFF);
            topicBytes.CopyTo(data.AsSpan(offset));
            offset += topicBytes.Length;
            data[offset++] = 0x00;
            data[offset++] = 0x10; // Packet ID = 16
            payload.CopyTo(data.AsSpan(offset));
            
            var sequence = new ReadOnlySequence<byte>(data);

            // Act
            var packet = decoder.Decode(ref sequence, null);

            // Assert
            Assert.IsType<PublishPacket>(packet);
            var publishPacket = (PublishPacket)packet;
            Assert.Equal(topicName, publishPacket.TopicName);
            Assert.Equal(2, publishPacket.Qos);
            Assert.True(publishPacket.Dup);
            Assert.True(publishPacket.Retain);
            Assert.Equal(16, publishPacket.PacketIdentifier);
        }

        [Fact]
        public void MQTTPacketDecoder_ShouldDecodeSubscribePacket()
        {
            // Arrange
            var decoder = new MQTTPacketDecoder();
            var topic1 = "home/temp";
            var topic2 = "home/humidity";
            var topic1Bytes = Encoding.UTF8.GetBytes(topic1);
            var topic2Bytes = Encoding.UTF8.GetBytes(topic2);
            
            // Calculate remaining length
            var remainingLength = 2 + // Packet identifier
                                 2 + topic1Bytes.Length + 1 + // Topic 1
                                 2 + topic2Bytes.Length + 1;  // Topic 2
            
            var data = new byte[2 + remainingLength];
            data[0] = 0x82; // SUBSCRIBE packet type with reserved bits
            data[1] = (byte)remainingLength;
            
            var offset = 2;
            // Packet identifier: 1234
            data[offset++] = (byte)(1234 >> 8);
            data[offset++] = (byte)(1234 & 0xFF);
            
            // Topic 1
            data[offset++] = (byte)(topic1Bytes.Length >> 8);
            data[offset++] = (byte)(topic1Bytes.Length & 0xFF);
            topic1Bytes.CopyTo(data.AsSpan(offset));
            offset += topic1Bytes.Length;
            data[offset++] = 1; // QoS 1
            
            // Topic 2
            data[offset++] = (byte)(topic2Bytes.Length >> 8);
            data[offset++] = (byte)(topic2Bytes.Length & 0xFF);
            topic2Bytes.CopyTo(data.AsSpan(offset));
            offset += topic2Bytes.Length;
            data[offset++] = 2; // QoS 2
            
            var sequence = new ReadOnlySequence<byte>(data);

            // Act
            var packet = decoder.Decode(ref sequence, null);

            // Assert
            Assert.IsType<SubscribePacket>(packet);
            var subscribePacket = (SubscribePacket)packet;
            Assert.Equal(1234, subscribePacket.PacketIdentifier);
            Assert.Equal(2, subscribePacket.TopicFilters.Count);
            Assert.Equal("home/temp", subscribePacket.TopicFilters[0].Topic);
            Assert.Equal(1, subscribePacket.TopicFilters[0].QoS);
            Assert.Equal("home/humidity", subscribePacket.TopicFilters[1].Topic);
            Assert.Equal(2, subscribePacket.TopicFilters[1].QoS);
        }

        [Fact]
        public void MQTTPacketDecoder_ShouldDecodePubAckPacket()
        {
            // Arrange
            var decoder = new MQTTPacketDecoder();
            var data = new byte[] { 0x40, 0x02, 0x12, 0x34 }; // PUBACK with packet ID 0x1234
            var sequence = new ReadOnlySequence<byte>(data);

            // Act
            var packet = decoder.Decode(ref sequence, null);

            // Assert
            Assert.IsType<PubAckPacket>(packet);
            var pubAckPacket = (PubAckPacket)packet;
            Assert.Equal(0x1234, pubAckPacket.PacketIdentifier);
        }

        [Fact]
        public void MQTTPacketDecoder_ShouldDecodePubRecPacket()
        {
            // Arrange
            var decoder = new MQTTPacketDecoder();
            var data = new byte[] { 0x50, 0x02, 0xAB, 0xCD }; // PUBREC with packet ID 0xABCD
            var sequence = new ReadOnlySequence<byte>(data);

            // Act
            var packet = decoder.Decode(ref sequence, null);

            // Assert
            Assert.IsType<PubRecPacket>(packet);
            var pubRecPacket = (PubRecPacket)packet;
            Assert.Equal(0xABCD, pubRecPacket.PacketIdentifier);
        }

        [Fact]
        public void MQTTPacketDecoder_ShouldDecodePubRelPacket()
        {
            // Arrange
            var decoder = new MQTTPacketDecoder();
            var data = new byte[] { 0x62, 0x02, 0x56, 0x78 }; // PUBREL with packet ID 0x5678
            var sequence = new ReadOnlySequence<byte>(data);

            // Act
            var packet = decoder.Decode(ref sequence, null);

            // Assert
            Assert.IsType<PubRelPacket>(packet);
            var pubRelPacket = (PubRelPacket)packet;
            Assert.Equal(0x5678, pubRelPacket.PacketIdentifier);
        }

        [Fact]
        public void MQTTPacketDecoder_ShouldDecodePubCompPacket()
        {
            // Arrange
            var decoder = new MQTTPacketDecoder();
            var data = new byte[] { 0x70, 0x02, 0x9A, 0xBC }; // PUBCOMP with packet ID 0x9ABC
            var sequence = new ReadOnlySequence<byte>(data);

            // Act
            var packet = decoder.Decode(ref sequence, null);

            // Assert
            Assert.IsType<PubCompPacket>(packet);
            var pubCompPacket = (PubCompPacket)packet;
            Assert.Equal(0x9ABC, pubCompPacket.PacketIdentifier);
        }

        [Fact]
        public void MQTTPacketDecoder_ShouldDecodePingReqPacket()
        {
            // Arrange
            var decoder = new MQTTPacketDecoder();
            var data = new byte[] { 0xC0, 0x00 }; // PINGREQ packet
            var sequence = new ReadOnlySequence<byte>(data);

            // Act
            var packet = decoder.Decode(ref sequence, null);

            // Assert
            Assert.IsType<PingReqPacket>(packet);
        }

        [Fact]
        public void MQTTPacketDecoder_ShouldDecodeDisconnectPacket()
        {
            // Arrange
            var decoder = new MQTTPacketDecoder();
            var data = new byte[] { 0xE0, 0x00 }; // DISCONNECT packet
            var sequence = new ReadOnlySequence<byte>(data);

            // Act
            var packet = decoder.Decode(ref sequence, null);

            // Assert
            Assert.IsType<DisconnectPacket>(packet);
        }
    }
}
