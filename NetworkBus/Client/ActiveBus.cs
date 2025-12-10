using NetworkBus.Models;
using Newtonsoft.Json;

namespace NetworkBus.Client
{
    public class ActiveBus : BusBase
    {
        public ActiveBus(IBusTransport transport)
        : base(transport)
        {}

        public override void Send<T>(T dto) where T : class
        {
            var jsonData = JsonConvert.SerializeObject(dto);
            Transport.Send(new Packet(typeof(T).Name, jsonData));
        }
    }
}