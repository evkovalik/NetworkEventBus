using Newtonsoft.Json;

namespace NetworkBus.Models
{
    public readonly struct Packet
    {
        public readonly string Name;
        public readonly string JsonData;
        public Packet(string name, string jsonData)
        {
            Name = name;
            JsonData = jsonData;
        }

        public static Packet Create<T>(T dto)
            => new(typeof(T).Name, JsonConvert.SerializeObject(dto));
    }
}