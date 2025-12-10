using Newtonsoft.Json;

namespace NetworkBus.Models
{
    public record Signal(string Name)
    {
        public const string PacketName = "Signal";
        
        public static Packet AsPacket(string signalName)
            => new(PacketName, JsonConvert.SerializeObject(new Signal(signalName)));
    }
}