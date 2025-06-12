using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.MQTT.Packets
{
    public class PingReqPacket : MQTTPacket
    {
        public override int EncodeBody(IBufferWriter<byte> writer)
        {
            // PINGREQ has no payload
            return 0;
        }

        protected internal override void DecodeBody(ref SequenceReader<byte> reader, object context)
        {
            // PINGREQ has no payload to decode
        }
    }
}
