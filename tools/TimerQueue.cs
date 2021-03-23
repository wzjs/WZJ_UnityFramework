using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wzj
{
    class Event
    {
        public int id;
        public long time;
        public long repeatInterval;
        public Action<int> callBack;
        public LinkedListNode<Event> node;
    }
    class TimerQueue
    {
        private Dictionary<int, Event> _ids = new Dictionary<int, Event>();
        private LinkedList<Event> _events = new LinkedList<Event>();

        private int _idCounter = 0;
        int IdCounter
        {
            get
            {
                return unchecked(_idCounter++);
            }
        }
        public TimerQueue() { }
        public void Add(long delay, long now, long interval, Action<int> callBack)
        {
            Insert(new Event() { id = IdCounter, time = delay + now, repeatInterval = interval, callBack = callBack });
        }

        public void RunOne(long delay, long now, Action<int> callBack)
        {
            Add(delay, now, 0, callBack);
        }

        private void Insert(Event @event)
        {
            var pEvent = _events.FirstOrDefault(i => i.time > @event.time);
            if (pEvent == null)
            {
                @event.node = _events.AddLast(@event);
            }
            else
            {
                @event.node = _events.AddBefore(pEvent.node, @event);
            }
            _ids.Add(@event.id, @event);
        }

        private void Remove(int id)
        {
            Event target;
            if (_ids.TryGetValue(id, out target))
            {
                _events.Remove(target);
                _ids.Remove(id);
            }
        }

        private long NextTime { get { return _events.Count > 0 ? _events.First.Value.time : int.MaxValue; } }

        public void RunInterval(long curTime)
        {
            while (NextTime <= curTime)
            {
                var targetEvents = _events.TakeWhile(i => i.time <= curTime).ToList();
                foreach (var item in targetEvents)
                {
                    Remove(item.id);
                    if (item.repeatInterval != 0)
                    {
                        item.time += item.repeatInterval;
                        Insert(item);
                    }
                    item.callBack.Invoke(item.id);
                }
            }
        }
    }
}

