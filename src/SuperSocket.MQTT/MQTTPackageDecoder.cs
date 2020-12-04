using System.Buffers;
using SuperSocket.MQTT.Packets;
using SuperSocket.ProtoBase;

namespace SuperSocket.MQTT
{
    public class MQTTPacketDecoder : IPackageDecoder<MQTTPacket>
    {
        public MQTTPacketDecoder()
        {
            _packetFactories = new IPacketFactory[20];
            RegisterPacketType<ConnectPacket>(ControlPacketType.CONNECT);
            RegisterPacketType<PingReqPacket>(ControlPacketType.PINGREQ);
            RegisterPacketType<DisconnectPacket>(ControlPacketType.DISCONNECT);
            RegisterPacketType<PublishPacket>(ControlPacketType.PUBLISH);
            RegisterPacketType<PubAckPacket>(ControlPacketType.PUBACK);
            RegisterPacketType<PubRecPacket>(ControlPacketType.PUBREC);
            RegisterPacketType<PubRelPacket>(ControlPacketType.PUBREL);
            RegisterPacketType<PubCompPacket>(ControlPacketType.PUBCOMP);
            RegisterPacketType<SubscribePacket>(ControlPacketType.SUBSCRIBE);
            RegisterPacketType<UnsubscribePacket>(ControlPacketType.UNSUBSCRIBE);

        }
        interface IPacketFactory
        {
            MQTTPacket Create();
        }

        class DefaultPacketFactory<TPacket> : IPacketFactory
            where TPacket : MQTTPacket, new()
        {
            public MQTTPacket Create()
            {
                return new TPacket();
            }
        }

        private IPacketFactory[] _packetFactories ;

        public void RegisterPacketType<TPacket>(ControlPacketType packetType)
            where TPacket : MQTTPacket, new()
        {
            _packetFactories[(int)packetType] = new DefaultPacketFactory<TPacket>();
        }

        public MQTTPacket Decode(ref ReadOnlySequence<byte> buffer, object context)
        {
            var reader = new SequenceReader<byte>(buffer);

            reader.TryRead(out byte firstByte);

            var packetType = (ControlPacketType)(firstByte >> 4);

            var packetFactory = _packetFactories[(int)packetType];

            var packet = packetFactory.Create();

            packet.Type = packetType;
            packet.Flags = firstByte;

            var lenSize = 0;

            while (true)
            {
                if (!reader.TryRead(out byte lenByte))
                    break;

                lenSize = +1;

                if ((lenByte & 0x80) != 0x80)
                    break;

                if (lenSize == 3)
                    break;
            }

            packet.DecodeBody(ref reader, context);
            return packet;
        }
    }
}