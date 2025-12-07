using System;
using NetworkBus.Models;

namespace NetworkBus.Client
{
    public interface INetworkTransport
    {
        event Action<Packet> OnReceive;
        void Send(Packet packet);
    }
}