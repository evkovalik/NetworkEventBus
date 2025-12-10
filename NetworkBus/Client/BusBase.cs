using System;
using NetworkBus.Models;
using Newtonsoft.Json;

namespace NetworkBus.Client
{
    public abstract class BusBase : INetworkBus
    {
        public abstract void Send<T>(T dto) where T : class;
        
        private IBusTransport _transport;
        private bool _opened = false;
        private Registrar _registrar = new();

        public BusBase(IBusTransport transport)
        {
            _transport = transport;
            Open();
        }

        public IBusTransport Transport => _transport;

        public void Open()
        {
            if(_opened) return;

            _transport.OnReceive += HandlePacket;
            _opened = true;
        }

        public void Close(bool removeHandlers=true)
        {
            if(!_opened) return;

            _transport.OnReceive -= HandlePacket;
            if(removeHandlers) _registrar.Clear();
            _opened = false;
        }

        public void ChangeTransport(IBusTransport transport)
        {
            Close();
            _transport = transport;
            Open();
        }

        public void Meet(string signalName, Action handler)
            => _registrar.AddHandlerFor(signalName, handler);

        public void Meet<T>(Action<T> handler) where T : class
            => _registrar.AddHandlerFor<T>(handler);

        public void Forget(string signalName, Action handler)
            => _registrar.RemoveHandlerFor(signalName, handler);

        public void Forget<T>(Action<T> handler) where T : class
            => _registrar.RemoveHandlerFor<T>(handler);

        public void Send(string signalName) => Send(new Signal(signalName));

        private void HandlePacket(Packet packet)
        {
            if(packet.Name == Signal.PacketName)
            {
                var signalDto = JsonConvert.DeserializeObject<Signal>(packet.JsonData);
                if(signalDto != null)
                    _registrar.GetSignalHandler(signalDto.Name)?.DynamicInvoke();
            }
            else
            {
                var result = _registrar.GetDtoHandler(packet.Name);
                if(result != null)
                {
                    var handler = result.Value;
                    var dto = JsonConvert.DeserializeObject(packet.JsonData, handler.dtoType);
                    handler.callback?.DynamicInvoke(dto);
                }
            }
        }
    }
}