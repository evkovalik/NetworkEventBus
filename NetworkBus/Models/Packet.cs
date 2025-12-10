using Newtonsoft.Json;

namespace NetworkBus.Models
{
    public record Packet(string Name, string JsonData)
    {
        public static Packet Create<T>(T dto)
            => new(typeof(T).Name, JsonConvert.SerializeObject(dto));

        public static Packet Create(string signalName)
            => new(Signal.PacketName, JsonConvert.SerializeObject(new Signal(signalName)));

        public bool TryReadSignal(out string? signalName)
        {
            signalName = null;
            if(Name == Signal.PacketName)
                signalName = JsonConvert.DeserializeObject<Signal>(JsonData)?.Name;

            return signalName != null;
        }
    }
}