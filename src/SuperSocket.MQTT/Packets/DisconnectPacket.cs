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
            throw new NotImplementedException();
        }

        protected internal override void DecodeBody(ref SequenceReader<byte> reader, object context)
        {
            
        }
    }
}
