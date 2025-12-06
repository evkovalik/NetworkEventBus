using NetworkBus.Models;
using NetworkBus.Client.Interfaces;
using Newtonsoft.Json;

namespace NetworkBus.Client
{
    public class ActiveBus : BusBase
    {
        public ActiveBus(INetworkTransport networkTransport)
        : base(networkTransport)
        {}

        public override void Send<T>(T dto) where T : class
        {
            var jsonData = JsonConvert.SerializeObject(dto);
            Transport.Send(new Packet(typeof(T).Name, jsonData));
        }
    }
}