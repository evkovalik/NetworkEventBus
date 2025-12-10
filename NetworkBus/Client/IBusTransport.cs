using System;
using NetworkBus.Models;

namespace NetworkBus.Client
{
    public interface IBusTransport
    {
        event Action<Packet> OnReceive;
        void Send(Packet packet);
    }
}