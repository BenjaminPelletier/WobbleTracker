using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataCollector
{
    abstract class CommPortListener : IDisposable
    {
        protected readonly SerialPort _Port;
        private readonly Thread _SerialPoller;
        private readonly CancellationTokenSource _Stopper;
        private readonly TaskCompletionSource<bool> _Completer;

        public CommPortListener(SerialPort port)
        {
            _Port = port;
            _Stopper = new CancellationTokenSource();
            _Completer = new TaskCompletionSource<bool>();
            _SerialPoller = new Thread(() =>
            {
                try
                {
                    Poll(_Stopper.Token);
                    _Completer.SetResult(true);
                }
                catch (Exception ex)
                {
                    _Completer.SetException(ex);
                }
            });
            _SerialPoller.Start();
        }

        public void Dispose()
        {
            Stop();
        }

        public Task Stop()
        {
            _Stopper.Cancel();
            return _Completer.Task;
        }

        private void Poll(CancellationToken token)
        {
            _Port.Open();
            var buffer = new byte[4096];
            var sb = new StringBuilder();
            while (!token.IsCancellationRequested)
            {
                int n = _Port.Read(buffer, 0, buffer.Length);
                for (int i = 0; i < n; i++)
                {
                    char c = (char)buffer[i];
                    if (c == 13)
                    {
                        // Carriage return; process line
                        Parse(sb.ToString());
                        sb.Clear();
                    }
                    else if (c == 10)
                    {
                        // Skip line feeds
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                Thread.Sleep(50);
            }
            _Port.Close();
        }

        protected abstract void Parse(string line);
    }
}
