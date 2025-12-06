using System;
using System.Collections.Generic;
using System.Linq;

namespace NetworkBus.Models
{
    public class Registrar
    {
        public readonly struct DtoHandler
        {
            public readonly Type dtoType;
            public readonly Delegate callback;
            public DtoHandler(Type dtoType, Delegate callback)
            {
                this.dtoType = dtoType;
                this.callback = callback;
            }
        }

        private readonly Dictionary<Type, Delegate> _dtoHandlers = new();
        private readonly Dictionary<string, Delegate> _signalHandlers = new();

        public void AddHandlerFor(string signalName, Delegate handler)
        {
            if (_signalHandlers.TryGetValue(signalName, out var existing))
                _signalHandlers[signalName] = Delegate.Combine(existing, handler);
            else
                _signalHandlers[signalName] = handler;
        }

        public void AddHandlerFor<T>(Delegate handler) where T : class
        {
            var type = typeof(T);
            if (_dtoHandlers.TryGetValue(type, out var existing))
                _dtoHandlers[type] = Delegate.Combine(existing, handler);
            else
                _dtoHandlers[type] = handler;
        }

        public void RemoveHandlerFor(string signalName, Delegate handler)
        {
            if (_signalHandlers.TryGetValue(signalName, out var existing))
            {
                var result = Delegate.Remove(existing, handler);
                if(result == null)
                    _signalHandlers.Remove(signalName);
                else
                    _signalHandlers[signalName] = result;
            }
        }

        public void RemoveHandlerFor<T>(Delegate handler) where T : class
        {
            var type = typeof(T);
            if (_dtoHandlers.TryGetValue(type, out var existing))
            {
                var result = Delegate.Remove(existing, handler);
                if(result == null)
                    _dtoHandlers.Remove(type);
                else
                    _dtoHandlers[type] = result;
            }
        }

        public Delegate? GetSignalHandler(string signalName)
        {
            if(_signalHandlers.TryGetValue(signalName, out var handler))
                return handler;
            return null;
        }

        public DtoHandler? GetDtoHandler(string typeName)
        {
            var type = _dtoHandlers.Keys.FirstOrDefault(t => t.Name == typeName);
            if (type != null && _dtoHandlers.TryGetValue(type, out var handler))
            {
                return new DtoHandler(type, handler);
            }
            return null;
        }
    }
}