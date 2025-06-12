using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.MQTT.Packets
{
    public class DisconnectPacket : MQTTPacket
    {
        public override int EncodeBody(IBufferWriter<byte> writer)
        {
            // DISCONNECT has no payload
            return 0;
        }

        protected internal override void DecodeBody(ref SequenceReader<byte> reader, object context)
        {
            // DISCONNECT has no payload to decode
        }
    }
}
