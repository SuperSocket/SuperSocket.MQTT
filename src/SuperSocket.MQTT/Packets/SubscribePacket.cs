using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.MQTT.Packets
{
    /// <summary>
    /// 订阅
    /// </summary>
    public class SubscribePacket : MQTTPacket
    {
        public string TopicName { get; set; }
        public int Qos { get; set; }
        public short PacketIdentifier { get; set; }
        public SubscribePacket()
        {
        }
        /// <summary>
        /// 编码
        /// </summary>
        public override int EncodeBody(IBufferWriter<byte> writer)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 解码
        /// </summary>
        protected internal override void DecodeBody(ref SequenceReader<byte> reader, object context)
        {
            reader.TryReadBigEndian(out short packetIdentifier);
            PacketIdentifier = packetIdentifier;
            reader.TryReadBigEndian(out short protocolNameLen);
            TopicName = reader.ReadString(protocolNameLen, Encoding.UTF8);
            reader.TryRead(out byte qos);
            Qos = qos;
        }
    }
}
