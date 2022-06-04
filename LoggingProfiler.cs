using System.Diagnostics;
using UnityEngine;

namespace UGameCore.Utilities
{
    public class LoggingProfiler
    {
        public string Text { get; private set; }
        private Stopwatch m_stopwatch = new Stopwatch();


        public LoggingProfiler(string text)
        {
            this.Text = text;
            UnityEngine.Debug.Log(text);
            m_stopwatch.Start();
        }

        public void Restart()
        {
            m_stopwatch.Restart();
        }

        public void Restart(string newText)
        {
            this.Text = newText;
            UnityEngine.Debug.Log(newText);
            m_stopwatch.Restart();
        }

        public void LogElapsed()
        {
            UnityEngine.Debug.Log($"{this.Text} finished - elapsed {m_stopwatch.Elapsed}");
        }
    }
}
