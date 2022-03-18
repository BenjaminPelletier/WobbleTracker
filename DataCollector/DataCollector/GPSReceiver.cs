using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataCollector
{
    class GPSReceiver : CommPortListener
    {
        private Dictionary<string, Action<string>> _Parsers = null;

        public class RMCEventArgs : EventArgs
        {
            public readonly DateTime Timestamp;
            public readonly double Latitude;
            public readonly double Longitude;
            public readonly double Speed;

            public RMCEventArgs(DateTime timestamp, double lat, double lng, double speed)
            {
                Timestamp = timestamp;
                Latitude = lat;
                Longitude = lng;
                Speed = speed;
            }

            public string ToCSV(DateTime reference)
            {
                return (Timestamp - reference).TotalSeconds.ToString("0.###") + "," + string.Format("{0},{1},{2}", Latitude, Longitude, Speed);
            }
        }

        public class UnrecognizedEventArgs: EventArgs
        {
            public readonly string Line;

            public UnrecognizedEventArgs(string line)
            {
                Line = line;
            }
        }

        public event EventHandler<RMCEventArgs> RecommendedMinimumMessage;
        public event EventHandler<UnrecognizedEventArgs> UnrecognizedMessage;

        public GPSReceiver(SerialPort port) : base(port) { }

        protected override void Parse(string line)
        {
            if (_Parsers == null)
            {
                _Parsers = new Dictionary<string, Action<string>>()
                {
                    { "$GPRMC", ParseRecommendedMinimum },
                    { "$GPGGA", ParseFix },
                    { "$GPGSA", ParseDilutionOfPosition },
                    { "$GPGSV", ParseSatellitesInView },
                };
            }

            bool parsed = false;
            foreach (string key in _Parsers.Keys)
            {
                if (line.StartsWith(key))
                {
                    _Parsers[key](line);
                    parsed = true;
                    break;
                }
            }
            if (!parsed)
            {
                UnrecognizedMessage?.Invoke(this, new UnrecognizedEventArgs(line));
            }
        }

        private void ParseRecommendedMinimum(string line)
        {
            string[] cols = line.Split(',');
            if (cols.Length != 13)
            {
                // Format error
                return;
            }
            string col;

            string colTime = cols[1];
            string colDate = cols[9];
            DateTime timestamp = new DateTime(
                2000 + int.Parse(colDate.Substring(4, 2)), int.Parse(colDate.Substring(2, 2)), int.Parse(colDate.Substring(0, 2)),
                int.Parse(colTime.Substring(0, 2)), int.Parse(colTime.Substring(2, 2)), int.Parse(colTime.Substring(4, 2)),
                DateTimeKind.Utc).ToLocalTime();

            col = cols[3];
            double lat = int.Parse(col.Substring(0, 2)) + double.Parse(col.Substring(2)) / 60;
            if (cols[4] == "N")
            {
                // Do nothing; latitude is already positive
            } else if (cols[4] == "S")
            {
                lat = -lat;
            } else
            {
                // Format error
                return;
            }

            col = cols[5];
            double lng = int.Parse(col.Substring(0, 3)) + double.Parse(col.Substring(3)) / 60;
            if (cols[6] == "E")
            {
                // Do nothing; longitude is already positive
            } else if (cols[6] == "W")
            {
                lng = -lng;
            } else
            {
                // Format error
                return;
            }

            double speed = double.Parse(cols[7]);

            RecommendedMinimumMessage?.Invoke(this, new RMCEventArgs(timestamp, lat, lng, speed));
        }

        private void ParseFix(string line)
        {

        }

        private void ParseDilutionOfPosition(string line)
        {

        }

        private void ParseSatellitesInView(string line)
        {

        }
    }
}
