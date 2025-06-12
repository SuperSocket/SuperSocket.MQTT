using System;
using System.Buffers;

namespace SuperSocket.MQTT.Packets
{
    public class PingRespPacket : MQTTPacket
    {
        public override int EncodeBody(IBufferWriter<byte> writer)
        {
            // PINGRESP has no payload
            return 0;
        }

        internal protected override void DecodeBody(ref SequenceReader<byte> reader, object context)
        {
            // PINGRESP has no payload to decode
        }
    }
}
