using System;
using System.Buffers;

namespace SuperSocket.MQTT.Client
{
    /// <summary>
    /// Encodes MQTT packets to bytes for transmission.
    /// </summary>
    public static class MQTTPacketEncoder
    {
        /// <summary>
        /// Encodes an MQTT packet to a byte array.
        /// </summary>
        /// <param name="packet">The MQTT packet to encode.</param>
        /// <returns>The encoded byte array.</returns>
        public static byte[] Encode(MQTTPacket packet)
        {
            var writer = new ArrayBufferWriter<byte>();
            
            // First, encode the body to determine its length
            var bodyWriter = new ArrayBufferWriter<byte>();
            var bodyLength = packet.EncodeBody(bodyWriter);
            var bodyData = bodyWriter.WrittenSpan;
            
            // Calculate the fixed header
            var packetTypeAndFlags = ((byte)packet.Type << 4) | (packet.Flags & 0x0F);
            
            // Write fixed header
            writer.GetSpan(1)[0] = (byte)packetTypeAndFlags;
            writer.Advance(1);
            
            // Write remaining length (variable length encoding)
            WriteRemainingLength(writer, bodyLength);
            
            // Write body
            if (bodyLength > 0)
            {
                var destSpan = writer.GetSpan(bodyLength);
                bodyData.CopyTo(destSpan);
                writer.Advance(bodyLength);
            }
            
            return writer.WrittenSpan.ToArray();
        }
        
        private static void WriteRemainingLength(ArrayBufferWriter<byte> writer, int length)
        {
            do
            {
                var encodedByte = length % 128;
                length /= 128;
                
                if (length > 0)
                {
                    encodedByte |= 0x80;
                }
                
                writer.GetSpan(1)[0] = (byte)encodedByte;
                writer.Advance(1);
            }
            while (length > 0);
        }
    }
}
