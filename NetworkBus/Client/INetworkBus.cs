using System;

namespace NetworkBus.Client
{
    public interface INetworkBus
    {
        void Meet(string signalName, Action handler);
        void Meet<T>(Action<T> handler) where T : class;
        void Forget(string signalName, Delegate handler);
        void Forget<T>(Delegate handler) where T : class;
        void Send(string signalName);
        void Send<T>(T dto) where T : class;
    }
}