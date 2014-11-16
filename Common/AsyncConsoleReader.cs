using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DarkMultiPlayerCommon
{
    public class AsyncConsoleReader
    {
        private static Thread m_inputThread;
        private static List<string> m_inputs;
        private static readonly object _lock = new object();

        public static void Initialize()
        {
            m_inputs = new List<string>();
            m_inputThread = new Thread(BackgroundReader);
            m_inputThread.IsBackground = true;
            m_inputThread.Start();
        }

        private static void BackgroundReader()
        {
            while (true)
            {
                try
                {
                    string input = Console.ReadLine();
                    lock(_lock)
                    {
                        m_inputs.Add(input);
                    }
                }
                catch { }
            }
        }

        public static string ReadLine()
        {
            lock(_lock)
            {
                if(m_inputs.Count > 0)
                {
                    string input = m_inputs.First();
                    m_inputs.RemoveAt(0);
                    return input;
                }
            }
            return "";
        }
    }
}
