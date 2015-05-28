using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.IO.Ports;

namespace TimerApp
{
    public class RaceDevices
    {
        public delegate void StartRaceReceivedEventHandler(object sender, EventArgs e);
        public delegate void CloseGateReceivedEventHandler(object sender, EventArgs e);

        public event StartRaceReceivedEventHandler OnStartRaceReceived;
        public event CloseGateReceivedEventHandler OnCloseGateReceived;

        private OutputPort _led;
        private SerialPort _com1;
        private Object LockThis = new Object();
        private DateTime _LastGateChange;

        // servo range is (800,2000), neutral = 1500.
        private SecretLabs.NETMF.Hardware.PWM _servo;
        private bool _fGateOpen;
        public bool IsGateOpen
        {
            get { return _fGateOpen; }
        }

        private Nokia_5110 _lcd;
        public Nokia_5110 Lcd
        {
            get { return _lcd; }
        }
        private InterruptPort _gatebutton;
        public RaceDevices()
        {
            _gatebutton = new InterruptPort(Pins.GPIO_PIN_D2, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);
            _gatebutton.OnInterrupt += new NativeEventHandler(_gatebutton_OnInterrupt);
            _LastGateChange = DateTime.Now;

            _com1 = new SerialPort(SerialPorts.COM1, 9600, Parity.None, 8, StopBits.One);
            _com1.DataReceived += new SerialDataReceivedEventHandler(SerialPort_DataReceived);
            _com1.Open();

            _led = new OutputPort(Pins.ONBOARD_LED, false);
            _lcd = new Nokia_5110(true, Pins.GPIO_PIN_D10, Pins.GPIO_PIN_D9, Pins.GPIO_PIN_D7, Pins.GPIO_PIN_D8);

            _servo = new SecretLabs.NETMF.Hardware.PWM(Pins.GPIO_PIN_D5);

            _lcd.BacklightBrightness = 100;
            _lcd.Refresh();
        }

        void _gatebutton_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            _gatebutton.ClearInterrupt();

            // debounce
            if (_LastGateChange.AddSeconds(2) > DateTime.Now)
            {
                Debug.Print(DateTime.Now.ToString() + ": debounce");
                return;
            }

            this.SetServoState(!this._fGateOpen);
            Debug.Print(DateTime.Now.ToString() + ": servo change");

        }

        public void SetLedState(bool fTurnedOn)
        {
            _led.Write(fTurnedOn);
        }

        public void SetServoState(bool fIsGateOpen)
        {
            _LastGateChange = DateTime.Now;

            if (fIsGateOpen)
                _servo.SetPulse(20000, 1600);
            else
                _servo.SetPulse(20000, 800);

            System.Threading.Thread.Sleep(500);
            _servo.SetPulse(0, 800);

            this._fGateOpen = fIsGateOpen;
        }

        public void SendSerialData(string data)
        {
            // make sure that only one thread (interrupt) is writing to the serial port at a time
            lock (LockThis)
            {
                byte[] sendData = System.Text.Encoding.UTF8.GetBytes(data);
                _com1.Write(sendData, 0, sendData.Length);
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int bytesToRead = _com1.BytesToRead;
                byte[] recBytes = new byte[bytesToRead];
                _com1.Read(recBytes, 0, bytesToRead);
                string SerialPortText = new String(System.Text.Encoding.UTF8.GetChars(recBytes));
                if (string.Compare(SerialPortText, "s") == 0)
                {
                    OnStartRaceReceived(this, new EventArgs());
                    return;
                }
                if (string.Compare(SerialPortText, "c") == 0)
                {
                    OnCloseGateReceived(this, new EventArgs());
                    return;
                }
            }
            catch (Exception)
            {
            }

            //Debug.Print(SerialPortText);
            //M5\rM6\rM7\rCR
            //if (SerialPortText.IndexOf("\r") > 0)
            //{
            //    string[] multipleCommands = SerialPortText.Split("\r".ToCharArray());
            //    foreach (string Command in multipleCommands)
            //    {
            //        if (Command != "")
            //            ProcessSerialCommand(Command);
            //        if (Command == "CR")
            //        {
            //            ProcessSerialCommand("C");
            //            ProcessSerialCommand("R");
            //        }
            //    }
            //}
            //else
            //{
            //    ProcessSerialCommand(SerialPortText);
            //}
        }

        #region Sample bitmap drawing code
        //Lcd.ByteMap = Bitmap1;
        //Lcd.Refresh();
        //Thread.Sleep(2000);

        //byte[] Bitmap1 = { 0, 128, 128, 192, 192, 192, 192, 128, 128, 0, 0, 0, 0, 128,
        //        128, 192, 192, 192, 192, 192, 128, 0, 0, 0, 240, 248, 240, 192, 192, 192, 192,
        //        0, 0, 128, 192, 192, 192, 192, 192, 128, 254, 254, 254, 0, 192, 192, 0, 0, 0, 0,
        //        0, 128, 192, 128, 0, 0, 152, 216, 152, 0, 0, 0, 128, 128, 192, 192, 192, 192,
        //        128, 128, 0, 0, 0, 0, 128, 128, 192, 192, 192, 192, 128, 128, 0, 0, 254, 255,
        //        255, 1, 0, 0, 1, 3, 255, 255, 0, 62, 255, 255, 145, 185, 24, 28, 141, 143, 231,
        //        243, 112, 0, 255, 255, 255, 128, 129, 129, 0, 126, 255, 195, 129, 0, 0, 0, 129,
        //        193, 255, 127, 31, 0, 255, 255, 192, 128, 0, 0, 128, 255, 255, 63, 0, 0, 255,
        //        255, 255, 0, 0, 254, 255, 3, 1, 0, 0, 1, 3, 255, 254, 0, 126, 255, 227, 129,
        //        128, 0, 0, 129, 129, 255, 255, 62, 3, 3, 1, 0, 0, 0, 0, 0, 3, 3, 0, 0, 0, 1, 3,
        //        3, 3, 3, 3, 3, 1, 0, 0, 0, 0, 1, 3, 3, 3, 3, 3, 0, 0, 1, 3, 3, 3, 3, 3, 1, 1, 0,
        //        0, 0, 0, 1, 3, 3, 3, 3, 3, 1, 0, 0, 0, 0, 3, 3, 1, 0, 0, 3, 3, 0, 0, 0, 0, 0, 0,
        //        3, 3, 0, 0, 0, 1, 3, 3, 3, 3, 3, 1, 1, 0, 0, 0, 0, 0, 32, 32, 248, 32, 32, 0, 0,
        //        0, 0, 254, 12, 16, 96, 254, 0, 112, 136, 136, 112, 0, 254, 32, 112, 136, 0, 250,
        //        0, 200, 168, 168, 248, 0, 0, 0, 0, 88, 142, 138, 114, 0, 0, 4, 254, 0, 0, 0, 4,
        //        254, 0, 0, 252, 130, 130, 124, 0, 0, 0, 0, 0, 254, 128, 128, 128, 0, 124, 130,
        //        130, 130, 68, 0, 0, 254, 130, 130, 130, 124, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //byte[] Bitmap2 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 224, 192, 0, 0, 224, 0, 0, 128, 128, 0, 0,
        //        224, 0, 0, 128, 0, 160, 0, 128, 128, 128, 128, 0, 0, 0, 0, 128, 224, 160, 32, 0,
        //        0, 64, 224, 0, 0, 0, 64, 224, 0, 0, 192, 32, 32, 192, 0, 0, 0, 0, 0, 128, 128,
        //        224, 0, 160, 0, 0, 128, 128, 128, 0, 128, 128, 128, 0, 0, 224, 0, 128, 128, 128,
        //        128, 0, 128, 0, 0, 0, 128, 0, 0, 0, 0, 0, 0, 15, 0, 1, 6, 15, 0, 7, 8, 8, 7, 0,
        //        15, 2, 7, 8, 0, 15, 0, 12, 10, 10, 15, 0, 0, 0, 0, 5, 8, 8, 7, 0, 0, 0, 15, 0,
        //        0, 0, 0, 15, 0, 0, 15, 8, 8, 7, 0, 0, 0, 0, 7, 8, 8, 15, 0, 15, 0, 9, 10, 10, 4,
        //        0, 63, 8, 8, 7, 0, 15, 0, 12, 10, 10, 15, 0, 0, 39, 24, 7, 0, 0, 0, 0, 0, 0,
        //        112, 136, 136, 254, 0, 248, 8, 8, 250, 0, 8, 112, 128, 112, 8, 0, 112, 168, 168,
        //        176, 0, 248, 8, 8, 0, 0, 0, 254, 136, 136, 112, 0, 8, 112, 128, 112, 8, 0, 0, 0,
        //        0, 0, 124, 130, 130, 130, 124, 0, 248, 8, 8, 248, 8, 8, 240, 0, 200, 168, 168,
        //        248, 0, 248, 8, 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        0, 0, 0, 0, 0, 0, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        0, 0, 0, 0 };
        //byte[] Bitmap3 = { 0, 0, 0, 128, 128, 128, 0, 0, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 0, 0,
        //        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 63, 36, 36, 31, 0, 62, 2, 63,
        //        34, 0, 62, 2, 2, 62, 2, 2, 60, 0, 50, 42, 42, 62, 0, 254, 34, 34, 28, 0, 0, 0,
        //        0, 36, 42, 42, 18, 0, 30, 32, 32, 62, 0, 254, 34, 34, 28, 0, 254, 34, 34, 28, 0,
        //        28, 34, 34, 28, 0, 62, 2, 2, 63, 34, 0, 0, 0, 0, 63, 34, 34, 28, 0, 2, 156, 96,
        //        28, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 48, 72, 72, 144, 32, 240, 32, 0, 192, 160,
        //        160, 192, 0, 32, 240, 40, 0, 32, 160, 160, 224, 0, 224, 32, 32, 192, 0, 0, 0, 0,
        //        8, 8, 248, 8, 8, 248, 32, 32, 192, 0, 192, 32, 32, 192, 0, 192, 32, 32, 192, 0,
        //        248, 0, 192, 160, 160, 192, 0, 224, 32, 32, 192, 0, 0, 0, 0, 128, 112, 72, 200,
        //        56, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 130, 130, 1, 0, 3, 2, 0, 1,
        //        2, 2, 2, 0, 0, 3, 0, 0, 3, 2, 2, 3, 0, 3, 0, 0, 3, 0, 0, 0, 0, 0, 0, 3, 128, 0,
        //        3, 0, 128, 3, 0, 1, 2, 2, 1, 0, 1, 2, 2, 1, 128, 3, 0, 1, 2, 2, 2, 0, 3, 128,
        //        128, 131, 128, 0, 0, 0, 1, 2, 2, 2, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        19, 36, 36, 25, 2, 63, 34, 0, 28, 42, 42, 44, 0, 2, 28, 32, 28, 2, 0, 28, 42,
        //        42, 44, 0, 62, 2, 2, 60, 0, 0, 0, 0, 0, 63, 4, 4, 4, 63, 0, 28, 42, 42, 44, 0,
        //        62, 2, 2, 60, 0, 63, 8, 28, 34, 0, 0, 0, 0, 0, 63, 32, 32, 32, 31, 0, 28, 34,
        //        34, 28, 0, 62, 2, 2, 60, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        #endregion
    }
}
