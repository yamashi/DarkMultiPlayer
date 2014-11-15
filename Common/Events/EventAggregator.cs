#region

using System;
using System.Collections.Generic;

#endregion

namespace CoreNet.Events
{
    public class EventAggregator : IEventAggregator
    {
        private readonly IDictionary<Type, IList<EventHandler<IEvent>>> _handlers;
        private readonly IList<IEvent> _events;
        private readonly Object _eventsLock;

        public EventAggregator()
        {
            _handlers = new Dictionary<Type, IList<EventHandler<IEvent>>>();
            _events = new List<IEvent>();
            _eventsLock = new Object();
        }

        public void Register(IRegisterable registerable)
        {
            registerable.Register(this);
        }

        public void RegisterAll(IEnumerable<IRegisterable> registerables)
        {
            foreach (var registerable in registerables)
            {
                Register(registerable);
            }
        }

        public void Register<T>(EventHandler<T> handler) where T : IEvent
        {
            if (!_handlers.ContainsKey(typeof (T)))
            {
                _handlers[typeof (T)] = new List<EventHandler<IEvent>>();
            }

            var handlerList = _handlers[typeof (T)];

            handlerList.Add(evt => handler((T) evt));
        }

        public void Run()
        {
            lock (_eventsLock)
            {
                foreach (var e in _events)
                {
                    DoTrigger(e);
                }
                _events.Clear();
            }
        }

        public void Trigger(IEvent evt)
        {
            lock(_eventsLock)
            {
                _events.Add(evt);
            }
        }

        private void DoTrigger(IEvent evt)
        {
            IList<EventHandler<IEvent>> handlerList;

            if (!_handlers.TryGetValue(evt.GetType(), out handlerList)) return;
            foreach (var handle in handlerList)
            {
                try
                {
                    handle.Invoke(evt);
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}