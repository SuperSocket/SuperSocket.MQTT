using System;
using System.Buffers;
using SuperSocket.ProtoBase;

namespace SuperSocket.MQTT
{
    public class MQTTPipelineFilter : IPipelineFilter<MQTTPacket>
    {
        /// <summary>
        /// Singleton MQTTPacketDecoder instance shared across all MQTTPipelineFilter instances.
        /// MQTTPacketDecoder is stateless, so a single instance can be safely reused.
        /// </summary>
        private static readonly MQTTPacketDecoder _sharedDecoder = new MQTTPacketDecoder();

        private IPackageDecoder<MQTTPacket> _decoder;

        public IPackageDecoder<MQTTPacket> Decoder 
        { 
            get => _decoder ?? _sharedDecoder;
            set => _decoder = value;
        }

        public IPipelineFilter<MQTTPacket> NextFilter { get; private set; }

        public object Context { get; set; }

        private int _currentLenUnit;

        private int _totalSize;

        private int _headerParsed;

        public MQTTPacket Filter(ref SequenceReader<byte> reader)
        {
            if (_currentLenUnit >= 0)
            {
                if (_headerParsed > 0)
                    reader.Advance(_headerParsed);

                while (true)
                {
                    if (!reader.TryRead(out byte b))
                        break;

                    _headerParsed++;

                    if (_currentLenUnit == 0)
                    {
                        _currentLenUnit = 1;
                    }
                    else
                    {
                        var hasNext = (b & 0x80) == 0x80;
                        _totalSize += (b & (0x80-1)) * _currentLenUnit;

                        if (!hasNext || _headerParsed >= 4)
                        {
                            _currentLenUnit = -1;
                            break;
                        }
                        else
                        {
                            _currentLenUnit *= 0x80;
                        }
                    }
                }

                reader.Rewind(_headerParsed);

                // has not loaded the length yet
                if (_currentLenUnit >= 0)
                {
                    return null;
                }

                // got the length, so we can set the totalSize right now
                _totalSize += _headerParsed;
            }

            if (reader.Remaining < _totalSize)
                return null;

            try
            {
                var buffer = reader.Sequence.Slice(0, _totalSize);
                return Decoder.Decode(ref buffer, Context);
            }
            finally
            {
                reader.Advance(_totalSize);
            }
        }

        public void Reset()
        {
            _currentLenUnit = 0;
            _headerParsed = 0;
            _totalSize = 0;
        }
    }
}