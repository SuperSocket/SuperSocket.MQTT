using System;
using System.Buffers;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.MQTT.Packets
{
    public class ConnectPacket : MQTTPacket
    {
        public string ProtocolName { get; set; } = "MQTT";

        public int ProtocolLevel { get; set; } = 4;

        public short KeepAlive { get; set; }

        public string ClientId { get; set; }

        public string WillTopic { get; set; }

        public string WillMessage { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public override int EncodeBody(IBufferWriter<byte> writer)
        {
            var protocolNameBytes = Encoding.UTF8.GetBytes(ProtocolName ?? "MQTT");
            var clientIdBytes = Encoding.UTF8.GetBytes(ClientId ?? string.Empty);
            var willTopicBytes = Encoding.UTF8.GetBytes(WillTopic ?? string.Empty);
            var willMessageBytes = Encoding.UTF8.GetBytes(WillMessage ?? string.Empty);
            var userNameBytes = Encoding.UTF8.GetBytes(UserName ?? string.Empty);
            var passwordBytes = Encoding.UTF8.GetBytes(Password ?? string.Empty);

            // Calculate total size
            var totalSize = 2 + protocolNameBytes.Length + // Protocol name
                           1 + // Protocol level
                           1 + // Connect flags
                           2 + // Keep alive
                           2 + clientIdBytes.Length; // Client ID

            var connectFlags = (ConnectFlags)0;
            
            if (!string.IsNullOrEmpty(WillTopic))
            {
                connectFlags |= ConnectFlags.WillFlag;
                totalSize += 2 + willTopicBytes.Length + 2 + willMessageBytes.Length;
            }
            
            if (!string.IsNullOrEmpty(UserName))
            {
                connectFlags |= ConnectFlags.UserNameFlag;
                totalSize += 2 + userNameBytes.Length;
            }
            
            if (!string.IsNullOrEmpty(Password))
            {
                connectFlags |= ConnectFlags.PasswordFlag;
                totalSize += 2 + passwordBytes.Length;
            }

            var span = writer.GetSpan(totalSize);
            var offset = 0;

            // Protocol name
            span[offset++] = (byte)(protocolNameBytes.Length >> 8);
            span[offset++] = (byte)(protocolNameBytes.Length & 0xFF);
            protocolNameBytes.AsSpan().CopyTo(span.Slice(offset));
            offset += protocolNameBytes.Length;

            // Protocol level
            span[offset++] = (byte)ProtocolLevel;

            // Connect flags
            span[offset++] = (byte)connectFlags;

            // Keep alive
            span[offset++] = (byte)(KeepAlive >> 8);
            span[offset++] = (byte)(KeepAlive & 0xFF);

            // Client ID
            span[offset++] = (byte)(clientIdBytes.Length >> 8);
            span[offset++] = (byte)(clientIdBytes.Length & 0xFF);
            clientIdBytes.AsSpan().CopyTo(span.Slice(offset));
            offset += clientIdBytes.Length;

            // Will topic and message
            if (!string.IsNullOrEmpty(WillTopic))
            {
                span[offset++] = (byte)(willTopicBytes.Length >> 8);
                span[offset++] = (byte)(willTopicBytes.Length & 0xFF);
                willTopicBytes.AsSpan().CopyTo(span.Slice(offset));
                offset += willTopicBytes.Length;

                span[offset++] = (byte)(willMessageBytes.Length >> 8);
                span[offset++] = (byte)(willMessageBytes.Length & 0xFF);
                willMessageBytes.AsSpan().CopyTo(span.Slice(offset));
                offset += willMessageBytes.Length;
            }

            // User name
            if (!string.IsNullOrEmpty(UserName))
            {
                span[offset++] = (byte)(userNameBytes.Length >> 8);
                span[offset++] = (byte)(userNameBytes.Length & 0xFF);
                userNameBytes.AsSpan().CopyTo(span.Slice(offset));
                offset += userNameBytes.Length;
            }

            // Password
            if (!string.IsNullOrEmpty(Password))
            {
                span[offset++] = (byte)(passwordBytes.Length >> 8);
                span[offset++] = (byte)(passwordBytes.Length & 0xFF);
                passwordBytes.AsSpan().CopyTo(span.Slice(offset));
                offset += passwordBytes.Length;
            }

            writer.Advance(totalSize);
            return totalSize;
        }

        internal protected override void DecodeBody(ref SequenceReader<byte> reader, object context)
        {
            reader.TryReadBigEndian(out short protocolNameLen);
            ProtocolName = reader.ReadString(protocolNameLen, Encoding.UTF8);

            reader.TryRead(out byte protocolLevel);
            ProtocolLevel = protocolLevel;

            reader.TryRead(out byte flags);
            Flags = flags;

            reader.TryReadBigEndian(out short keepAlive);
            KeepAlive = keepAlive;
            if (ProtocolLevel == 5)
                reader.TryRead(out byte keep);
            ClientId = reader.ReadLengthEncodedString();

            var connectFlags = (ConnectFlags)Flags;

            if ((connectFlags & ConnectFlags.WillFlag) == ConnectFlags.WillFlag)
            {
                WillTopic = reader.ReadLengthEncodedString();
                WillMessage = reader.ReadLengthEncodedString();
            }

            if ((connectFlags & ConnectFlags.UserNameFlag) == ConnectFlags.UserNameFlag)
            {
                UserName = reader.ReadLengthEncodedString();
            }

            if ((connectFlags & ConnectFlags.PasswordFlag) == ConnectFlags.PasswordFlag)
            {
                Password = reader.ReadLengthEncodedString();
            }
        }
    }
}