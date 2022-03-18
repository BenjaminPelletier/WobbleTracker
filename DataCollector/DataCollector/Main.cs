using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataCollector
{
    public partial class Main : Form
    {
        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
            // Legacy flag, should not be used.
            // ES_USER_PRESENT = 0x00000004
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        private const string BASE_PATH = @"C:\bjp\WobbleTracker\data";

        private GPSReceiver _GPS = null;
        private SensorListener _Sensors = null;

        private StreamWriter _GPSStream;
        private StreamWriter _SensorStream;

        private DateTime _SessionStart = DateTime.Now;
        private DateTime _LastSensorUIUpdate = DateTime.MinValue;
        private int _SensorMeasurementCount = 0;
        private DateTime _LastGPSFlush = DateTime.MinValue;
        private DateTime _LastSensorsFlush = DateTime.MinValue;

        public Main()
        {
            InitializeComponent();
        }

        private void StartSession()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED);

            _SessionStart = DateTime.Now;
            _SessionStart -= TimeSpan.FromMilliseconds(_SessionStart.Millisecond);
            string session = _SessionStart.ToString("yyyyMMddTHHmmss");

            _LastGPSFlush = _SessionStart;
            _GPSStream = new StreamWriter(Path.Combine(BASE_PATH, session + "_gps.csv"));
            _LastSensorsFlush = _SessionStart;
            _SensorStream = new StreamWriter(Path.Combine(BASE_PATH, session + "_sensors.csv"));

            var serial = new SerialPort("COM5", 4800, Parity.None, 8);
            _GPS = new GPSReceiver(serial);
            _GPS.RecommendedMinimumMessage += gps_RecommendedMinimumMessage;
            _GPS.UnrecognizedMessage += gps_UnrecognizedMessage;

            serial = new SerialPort("COM3", 115200, Parity.None, 8);
            _Sensors = new SensorListener(serial);
            _Sensors.NewMeasurement += sensors_NewMeasurement;
            _Sensors.UnrecognizedMessage += sensors_UnrecognizedMessage;

            cmdStartStop.BackColor = Color.DarkGreen;
            cmdStartStop.Text = "Stop";
        }

        private void StopSession()
        {
            cmdStartStop.BackColor = Color.DarkGoldenrod;
            cmdStartStop.Text = "Stopping";

            var gpsStop = _GPS.Stop();
            var sensorsStop = _Sensors.Stop();
            try
            {
                Task.WaitAll(gpsStop, sensorsStop);
            }
            catch (Exception ex)
            {
                tsslError.Text = ex.ToString();
            }
            _GPS.Dispose();
            _Sensors.Dispose();
            _GPSStream.Close();
            _SensorStream.Close();
            _GPS = null;
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);

            cmdStartStop.BackColor = Color.DarkRed;
            cmdStartStop.Text = "Start";
        }

        private void gps_RecommendedMinimumMessage(object sender, GPSReceiver.RMCEventArgs e)
        {
            _GPSStream.WriteLine(e.ToCSV(_SessionStart));
            if ((DateTime.Now - _LastGPSFlush).TotalSeconds > 15)
            {
                _GPSStream.Flush();
                _LastGPSFlush = DateTime.Now;
            }
            Invoke(new Action(() =>
            {
                txtGPS.Text = e.ToCSV(_SessionStart);
            }));
        }

        private void gps_UnrecognizedMessage(object sender, GPSReceiver.UnrecognizedEventArgs e)
        {
            Invoke(new Action(() =>
            {
                tsslError.Text = "!! GPS error: " + e.Line;
            }));
        }

        private void sensors_NewMeasurement(object sender, SensorListener.MeasurementEventArgs e)
        {
            _SensorStream.WriteLine(e.ToCSV(_SessionStart));
            if (!e.Sensors.Select(s => !s.Invalid).All(s => s))
            {
                //_Sensors.Reset();
            }
            if ((DateTime.Now - _LastSensorsFlush).TotalSeconds > 15)
            {
                _SensorStream.Flush();
                _LastSensorsFlush = DateTime.Now;
            }
            _SensorMeasurementCount++;
            if ((DateTime.Now - _LastSensorUIUpdate).TotalSeconds > 0.5)
            {
                int n = _SensorMeasurementCount;
                _SensorMeasurementCount = 0;
                BeginInvoke(new Action(() =>
                {
                    txtSensors.Text = n + " " + e.ToDisplay(_SessionStart);
                }));
                _LastSensorUIUpdate = DateTime.Now;
            }
        }

        private void sensors_UnrecognizedMessage(object sender, SensorListener.UnrecognizedEventArgs e)
        {
            Invoke(new Action(() =>
            {
                tsslError.Text = "!! Sensors error: " + e.Line;
            }));
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_GPS != null) StopSession();
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }

        private void tsbReset_Click(object sender, EventArgs e)
        {
            _Sensors?.Reset();
        }

        private void cmdStartStop_Click(object sender, EventArgs e)
        {
            if (_GPS == null)
            {
                StartSession();
            }
            else
            {
                StopSession();
            }
        }
    }
}
