using SuperSocket.ProtoBase;

namespace SuperSocket.MQTT.Server
{
    /// <summary>
    /// Factory for creating MQTTPipelineFilter instances with properly initialized decoders.
    /// </summary>
    public class MQTTPipelineFilterFactory : PipelineFilterFactoryBase<MQTTPacket>
    {
        /// <summary>
        /// Creates a new MQTTPipelineFilter instance with the decoder properly initialized.
        /// </summary>
        /// <returns>A new MQTTPipelineFilter instance.</returns>
        protected override IPipelineFilter<MQTTPacket> Create()
        {
            return new MQTTPipelineFilter();
        }
    }
}
