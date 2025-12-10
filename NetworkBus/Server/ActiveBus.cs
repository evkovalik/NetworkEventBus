using System.Collections.Generic;
using NetworkBus.Models;
using Newtonsoft.Json;

namespace NetworkBus.Server
{
    public class ActiveBus : BusBase
    {
        private List<string> _clientIds = new();

        public ActiveBus(IBusTransport transport)
        : base(transport)
        {}

        public override void AddRecipient(string recipientId)
            => _clientIds.Add(recipientId);

        public override void RemoveRecipient(string recipientId)
            => _clientIds.Remove(recipientId);

        public override void SendTo<T>(string recipientId, T dto)
        {
            var jsonData = JsonConvert.SerializeObject(dto);
            var packet = new Packet(typeof(T).Name, jsonData);
            Transport.Send(recipientId, packet);
        }

        public override void SendToAll<T>(T dto)
        {
            var jsonData = JsonConvert.SerializeObject(dto);
            var packet = new Packet(typeof(T).Name, jsonData);
            foreach (var id in _clientIds) Transport.Send(id, packet);
        }
    }
}