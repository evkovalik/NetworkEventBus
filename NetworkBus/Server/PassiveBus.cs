using System.Collections.Generic;
using NetworkBus.Models;
using Newtonsoft.Json;

namespace NetworkBus.Server
{
    public class PassiveBus : BusBase
    {
        private record Client(string Id, List<Packet> Buffer);
        private List<Client> _clients = new(){new Client(string.Empty, new())};

        public PassiveBus(INetworkTransport networkTransport)
        : base(networkTransport)
        {}

        public override void AddRecipient(string recipientId)
            => _clients.Add(new Client(recipientId, new()));

        public override void RemoveRecipient(string recipientId)
        {
            if(string.IsNullOrEmpty(recipientId)) return;
            _clients.RemoveAll(c => c.Id == recipientId);
        }

        public override void SendTo<T>(string recipientId, T dto) where T : class
        {
            var jsonData = JsonConvert.SerializeObject(dto);
            var packet = new Packet(typeof(T).Name, jsonData);

            var index = _clients.FindIndex(c => c.Id == recipientId);
            if(index == -1)
                _clients.Add(new Client(recipientId, new(){packet}));
            else
                _clients[index].Buffer.Add(packet);
        }

        public override void SendToAll<T>(T dto) where T : class
        {
            var jsonData = JsonConvert.SerializeObject(dto);
            _clients[0].Buffer.Add(new Packet(typeof(T).Name, jsonData));
        }

        public void Release()
        {
            if(_clients.Count < 2)
            {
                _clients[0].Buffer.Clear();
                return;
            }

            if(_clients[0].Buffer.Count != 0)
            {
                var buffer = _clients[0].Buffer;
                if(_clients[0].Buffer.Count == 1)
                {
                    for(int i = 1; i < _clients.Count; i++)
                        Transport.Send(_clients[i].Id, buffer[0]);
                }
                else
                {
                    for(int i = 1; i < _clients.Count; i++)
                        Transport.Send(_clients[i].Id, buffer);
                }
                buffer.Clear();
            }
            for(int i = 1; i < _clients.Count; i++)
            {
                if(_clients[i].Buffer.Count != 0)
                {
                    if(_clients[i].Buffer.Count == 1)
                        Transport.Send(_clients[i].Id, _clients[i].Buffer[0]);
                    else
                        Transport.Send(_clients[i].Id, _clients[i].Buffer);
                    _clients[i].Buffer.Clear();
                }
            }
        }
    }
}