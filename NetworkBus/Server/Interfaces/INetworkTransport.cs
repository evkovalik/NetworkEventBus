using System;
using System.Collections.Generic;
using NetworkBus.Models;

namespace NetworkBus.Server.Interfaces
{
    public interface INetworkTransport
    {
        event Action<string, Packet> OnReceive;
        void Send(string recipientId, Packet packet);
        void Send(string recipientId, List<Packet> packets);
    }
}