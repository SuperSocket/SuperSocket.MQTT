using System;
using System.Buffers;
using SuperSocket.ProtoBase;

namespace SuperSocket.MQTT.Client
{
    /// <summary>
    /// Encodes MQTT packets to bytes for transmission.
    /// This class is stateless and should be used as a singleton.
    /// </summary>
    public class MQTTPacketEncoder : IPackageEncoder<MQTTPacket>
    {
        /// <summary>
        /// Singleton instance of the encoder.
        /// MQTTPacketEncoder is stateless, so a single instance can be safely reused.
        /// </summary>
        public static readonly MQTTPacketEncoder Default = new MQTTPacketEncoder();

        /// <summary>
        /// Encodes an MQTT packet into the specified buffer writer.
        /// </summary>
        /// <param name="writer">The buffer writer to write the encoded packet to.</param>
        /// <param name="pack">The MQTT packet to encode.</param>
        /// <returns>The number of bytes written to the buffer.</returns>
        public int Encode(IBufferWriter<byte> writer, MQTTPacket pack)
        {
            var totalBytes = 0;
            
            // First, encode the body to determine its length
            var bodyWriter = new ArrayBufferWriter<byte>();
            var bodyLength = pack.EncodeBody(bodyWriter);
            var bodyData = bodyWriter.WrittenSpan;
            
            // Calculate the fixed header
            var packetTypeAndFlags = ((byte)pack.Type << 4) | (pack.Flags & 0x0F);
            
            // Write fixed header
            writer.GetSpan(1)[0] = (byte)packetTypeAndFlags;
            writer.Advance(1);
            totalBytes++;
            
            // Write remaining length (variable length encoding)
            totalBytes += WriteRemainingLength(writer, bodyLength);
            
            // Write body
            if (bodyLength > 0)
            {
                var destSpan = writer.GetSpan(bodyLength);
                bodyData.CopyTo(destSpan);
                writer.Advance(bodyLength);
                totalBytes += bodyLength;
            }
            
            return totalBytes;
        }
        
        private static int WriteRemainingLength(IBufferWriter<byte> writer, int length)
        {
            var bytesWritten = 0;
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
                bytesWritten++;
            }
            while (length > 0);
            
            return bytesWritten;
        }
    }
}
