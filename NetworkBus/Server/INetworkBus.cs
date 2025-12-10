using System;

namespace NetworkBus.Server
{
    public interface INetworkBus
    {
        void Meet(string signalName, Action<string> handler);
        void Meet<T>(Action<string, T> handler) where T : class;
        void Forget(string signalName, Action handler);
        void Forget<T>(Action<T> handler) where T : class;
        void SendTo(string recipientId, string signalName);
        void SendToAll(string signalName);
        void SendTo<T>(string recipientId, T dto) where T : class;
        void SendToAll<T>(T dto) where T : class;
    }
}