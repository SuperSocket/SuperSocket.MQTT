using System.Buffers;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.MQTT.Packets
{
    public class ConnectPacket : MQTTPacket
    {
        public string ProtocolName { get; private set; }

        public int ProtocolLevel { get; private set; }

        public short KeepAlive { get; private set; }

        public string ClientId { get; private set; }

        public string WillTopic { get; private set; }

        public string WillMessage { get; private set; }

        public string UserName { get; private set; }

        public string Password { get; private set; }

        public override int EncodeBody(IBufferWriter<byte> writer)
        {
            throw new System.NotImplementedException();
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