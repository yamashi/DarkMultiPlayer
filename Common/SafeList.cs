using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkMultiPlayerCommon
{
    public class SafeList<T>
    {
        private List<T> m_values;
        private List<T> m_toAdd;
        private List<T> m_toRemove;

        public SafeList()
        {
            m_values = new List<T>();
            m_toAdd = new List<T>();
            m_toRemove = new List<T>();
        }

        public void Add(T aValue)
        {
            lock(m_toAdd)
            {
                m_toAdd.Add(aValue);
            }
        }
        public void Remove(T aValue)
        {
            lock(m_toRemove)
            {
                m_toRemove.Add(aValue);
            }
        }

        public void Flush()
        {
            lock (m_values)
            {
                lock (m_toAdd)
                {
                    foreach (var entry in m_toAdd)
                    {
                        m_values.Add(entry);
                    }
                }

                lock (m_toRemove)
                {
                    foreach (var entry in m_toRemove)
                    {
                        m_values.Remove(entry);
                    }
                }
            }
        }

        public void Iterate(Action<T> aFunctor)
        {
            lock(m_values)
            {
                foreach(var entry in m_values)
                {
                    aFunctor(entry);
                }
            }
        }

        public T[] ToArray()
        {
            return (T[])m_values.ToArray().Clone();
        }
    }
}
