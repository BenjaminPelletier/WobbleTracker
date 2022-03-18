using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataCollector
{
    class SensorListener : CommPortListener
    {
        public class SensorMeasurement
        {
            private static int Parse(string sequence, int offset)
            {
                int value = Convert.ToInt32(sequence.Substring(offset, 4), 16);
                if (value > 0x7FFF) value = -(value ^ 0xFFFF);
                return value;
            }

            public Vector3 Accel;
            public Vector3 Gyro;

            public SensorMeasurement(string line, int offset)
            {
                /*Accel = new Vector3(
                    int.Parse(cols[offset + 1]),
                    int.Parse(cols[offset + 2]),
                    int.Parse(cols[offset + 3]));
                Gyro = new Vector3(
                    int.Parse(cols[offset + 4]),
                    int.Parse(cols[offset + 5]),
                    int.Parse(cols[offset + 6]));*/
                Accel = new Vector3(
                    Parse(line, offset),
                    Parse(line, offset + 4),
                    Parse(line, offset + 8));
                Gyro = new Vector3(
                    Parse(line, offset + 12),
                    Parse(line, offset + 16),
                    Parse(line, offset + 20));
            }

            public bool Invalid { get { return (Accel.X == 0 && Accel.Y == 0 && Accel.Z == 0) || (Gyro.X == 0 && Gyro.Y == 0 && Gyro.Z == 0); } }
        }

        public class MeasurementEventArgs : EventArgs
        {
            public DateTime Timestamp;
            public SensorMeasurement[] Sensors;

            public MeasurementEventArgs(DateTime timestamp, SensorMeasurement[] sensors)
            {
                Timestamp = timestamp;
                Sensors = sensors;
            }

            public string ToCSV(DateTime reference)
            {
                return (Timestamp - reference).TotalSeconds.ToString("0.####") + "," + Sensors
                    .Select(sensor => string.Format("{0},{1},{2},{3},{4},{5}", sensor.Accel.X, sensor.Accel.Y, sensor.Accel.Z, sensor.Gyro.X, sensor.Gyro.Y, sensor.Gyro.Z))
                    .Aggregate((a, b) => a + "," + b);
            }

            public string ToDisplay(DateTime reference)
            {
                var sb = new StringBuilder();
                sb.AppendLine((Timestamp - reference).TotalSeconds.ToString("0.####"));
                foreach (SensorMeasurement m in Sensors)
                {
                    if (m.Invalid) sb.Append("!!!!! ");
                    sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5}", m.Accel.X, m.Accel.Y, m.Accel.Z, m.Gyro.X, m.Gyro.Y, m.Gyro.Z));
                }
                return sb.ToString();
            }
        }

        public class UnrecognizedEventArgs : EventArgs
        {
            public readonly string Line;

            public UnrecognizedEventArgs(string line)
            {
                Line = line;
            }
        }

        public event EventHandler<MeasurementEventArgs> NewMeasurement;
        public event EventHandler<UnrecognizedEventArgs> UnrecognizedMessage;

        public SensorListener(SerialPort port) : base(port) { }

        public void Reset()
        {
            _Port.WriteLine("");
        }

        private StringBuilder _Line = new StringBuilder();
        protected override void Parse(string line)
        {
            DateTime timestamp = DateTime.Now;
            if (line.Length % 24 != 0)
            {
                UnrecognizedMessage?.Invoke(this, new UnrecognizedEventArgs(line));
                return;
            }

            NewMeasurement?.Invoke(this, new MeasurementEventArgs(timestamp,
                Enumerable.Range(0, line.Length / 24)
                          .Select(s => new SensorMeasurement(line, s * 24))
                          .ToArray()));
        }
    }
}
