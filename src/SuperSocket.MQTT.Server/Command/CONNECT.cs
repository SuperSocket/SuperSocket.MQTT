using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.Command;
using System.Threading.Tasks;

namespace SuperSocket.MQTT.Packets
{
    [Command(Key = ControlPacketType.CONNECT)]
    public class CONNECT : IAsyncCommand<MQTTPacket>
    {
        public ValueTask ExecuteAsync(IAppSession session, MQTTPacket package)
        {
            throw new System.NotImplementedException();
        }
    }
}