using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSRacing.RacingObjects;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using System.IO.Ports;

namespace GSRacing.RacingAdmin
{
    public class MainViewModel: ViewModelBase
    {
        private System.Windows.Threading.DispatcherTimer timerCountdown = new System.Windows.Threading.DispatcherTimer();
        private SerialPort sp = null;

        private string szSerialBuffer = string.Empty;

        private bool _isRaceComputerReady = false;
        public bool IsRaceComputerReady
        {
            get
            {
                return _isRaceComputerReady;
            }
            set
            {
                this.Set(ref _isRaceComputerReady, value);
            }
        }

        private bool _inRace = false;
        public bool InRace
        {
            get
            {
                return _inRace;
            }
            set
            {
                this.Set(ref _inRace, value);
            }
        }

        private RaceEvent _event;
        public RaceEvent Event
        {
            get
            {
                return _event;
            }
            set
            {
                this.Set(ref _event, value);
            }
        }

        private bool _heatsVisible = false;
        public bool HeatsVisible
        {
            get
            {
                return _heatsVisible;
            }
            set
            {
                this.Set(ref _heatsVisible, value);
            }
        }

        private bool _racersVisible = false;
        public bool RacersVisible
        {
            get
            {
                return _racersVisible;
            }
            set
            {
                this.Set(ref _racersVisible, value);
            }
        }

        private bool _currentHeatVisible = false;
        public bool CurrentHeatVisible
        {
            get
            {
                return _currentHeatVisible;
            }
            set
            {
                this.Set(ref _currentHeatVisible, value);
            }
        }

        private bool _finalResultsVisible = false;
        public bool FinalResultsVisible
        {
            get
            {
                return _finalResultsVisible;
            }
            set
            {
                this.Set(ref _finalResultsVisible, value);
            }
        }



        private bool _countdownVisible = false;
        public bool CountdownVisible
        {
            get
            {
                return _countdownVisible;
            }
            set
            {
                this.Set(ref _countdownVisible, value);
            }
        }

        private int _countdownAmt = 0;
        public int CountdownAmt
        {
            get
            {
                return _countdownAmt;
            }
            set
            {
                this.Set(ref _countdownAmt, value);
                RaisePropertyChanged("CountdownText");
            }
        }

        public string CountdownText
        {
            get
            {
                if (_countdownAmt == 0)
                    return "Go!";

                return _countdownAmt.ToString();
            }

        }

        private RaceHeat _currentHeat;
        public RaceHeat CurrentHeat
        {
            get
            {
                return _currentHeat;
            }
            set
            {
                this.Set(ref _currentHeat, value);
                RaisePropertyChanged("NextHeat");
                RaisePropertyChanged("HeatText");
            }
        }

        public string HeatText
        {
            get
            {
                if (CurrentHeat == null)
                    return string.Empty;

                return string.Format("Heat {0} of {1}", this.CurrentHeat.HeatNumber, this.Event.Heats.Count);
            }
        }

        public RaceHeat NextHeat
        {
            get
            {
                int currIndex = this.Event.Heats.IndexOf(this.CurrentHeat);
                if (currIndex == this.Event.Heats.Count - 1)
                    return null;

                return this.Event.Heats[currIndex + 1];
            }
        }

        public MainViewModel()
        {
            timerCountdown.Interval = new TimeSpan(0, 0, 1);
            timerCountdown.Tick += timer_Tick;

            this.sp = new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One);
            this.sp.DataReceived += sp_DataReceived;
            this.sp.Open();
            szSerialBuffer = string.Empty;

            this._event = new RaceEvent();
            this.Event.EventID = new Guid("{7B24B1E9-98A3-4187-A97E-E4F795BD59EA}");
            this.Event.EventName = "GS Troop 70068 Pinewood Derby Race";
            this.Event.EventDate = new DateTime(2015, 5, 29);
            this.Event.TrackCount = 4;

            Racer r1 = new Racer();
            r1.RacerID = Guid.NewGuid();
            r1.RegNumber = 1;
            r1.EventID = this.Event.EventID;
            r1.LastName = "Crumbling";
            r1.FirstName = "Imogen";
            this.Event.Racers.Add(r1);

            Racer r2 = new Racer();
            r2.RacerID = Guid.NewGuid();
            r2.RegNumber = 2;
            r2.EventID = this.Event.EventID;
            r2.LastName = "Moore";
            r2.FirstName = "Cat";
            this.Event.Racers.Add(r2);

            Racer r3 = new Racer();
            r3.RacerID = Guid.NewGuid();
            r3.RegNumber = 3;
            r3.EventID = this.Event.EventID;
            r3.LastName = "Edelman";
            r3.FirstName = "Emily";
            this.Event.Racers.Add(r3);

            Racer r4 = new Racer();
            r4.RacerID = Guid.NewGuid();
            r4.RegNumber = 4;
            r4.EventID = this.Event.EventID;
            r4.LastName = "Martin";
            r4.FirstName = "Norah";
            this.Event.Racers.Add(r4);

            Racer r5 = new Racer();
            r5.RacerID = Guid.NewGuid();
            r5.RegNumber = 5;
            r5.EventID = this.Event.EventID;
            r5.LastName = "Westphal";
            r5.FirstName = "Maya";
            this.Event.Racers.Add(r5);

            Racer r6 = new Racer();
            r6.RacerID = Guid.NewGuid();
            r6.RegNumber = 6;
            r6.EventID = this.Event.EventID;
            r6.LastName = "Luther";
            r6.FirstName = "Georgia";
            this.Event.Racers.Add(r6);

            Racer r7 = new Racer();
            r7.RacerID = Guid.NewGuid();
            r7.RegNumber = 7;
            r7.EventID = this.Event.EventID;
            r7.LastName = "Celot";
            r7.FirstName = "Olivia";
            this.Event.Racers.Add(r7);

            Racer r8 = new Racer();
            r8.RacerID = Guid.NewGuid();
            r8.RegNumber = 8;
            r8.EventID = this.Event.EventID;
            r8.LastName = "Phillips";
            r8.FirstName = "Genevieve";
            this.Event.Racers.Add(r8);

            Racer r9 = new Racer();
            r9.RacerID = Guid.NewGuid();
            r9.RegNumber = 9;
            r9.EventID = this.Event.EventID;
            r9.LastName = "Fox";
            r9.FirstName = "Tori";
            this.Event.Racers.Add(r9);

            Racer r10 = new Racer();
            r10.RacerID = Guid.NewGuid();
            r10.RegNumber = 10;
            r10.EventID = this.Event.EventID;
            r10.LastName = "Speizer";
            r10.FirstName = "Megan";
            this.Event.Racers.Add(r10);

            Racer r11 = new Racer();
            r11.RacerID = Guid.NewGuid();
            r11.RegNumber = 11;
            r11.EventID = this.Event.EventID;
            r11.LastName = "Smith";
            r11.FirstName = "Bella";
            this.Event.Racers.Add(r11);

            Racer r12 = new Racer();
            r12.RacerID = Guid.NewGuid();
            r12.RegNumber = 12;
            r12.EventID = this.Event.EventID;
            r12.LastName = "Scott";
            r12.FirstName = "Rosalyn";
            this.Event.Racers.Add(r12);

            this.RacersVisible = true;

        }

        ~MainViewModel()
        {
            sp.Close();
            sp.Dispose();
        }

        private const char ACK = (char)0x06;
        private const char STX = (char)0x02;
        private const char ETX = (char)0x03;

        void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            szSerialBuffer = szSerialBuffer + sp.ReadExisting();

            // if got ack, set ready
            int iACKIndex = szSerialBuffer.IndexOf(ACK);
            if (iACKIndex > -1)
            {
                IsRaceComputerReady = true;
                szSerialBuffer = szSerialBuffer.Remove(iACKIndex, 1);
            }
            
            // if got ack, set ready
            int iSTXIndex = szSerialBuffer.IndexOf(STX);
            if (iSTXIndex > -1)
            {
                int iETXIndex = szSerialBuffer.IndexOf(ETX);
                if (iSTXIndex > -1)
                {
                    string szMessage = szSerialBuffer.Substring(iSTXIndex + 1, iSTXIndex - iETXIndex - 1);
                    szSerialBuffer = szSerialBuffer.Remove(iSTXIndex, iSTXIndex - iETXIndex);
                    InRace = false;
                    ParseMessage(szMessage);
                }
            }
            
        }


        private void ParseMessage(string szMessage)
        {
            string[] rgszTimes = szMessage.Split('|');
            foreach (string szTime in rgszTimes)
            {
                string[] rgszTimeItems = szMessage.Split(':');
                int iTrackNum = 0;
                decimal dcRaceTime = 0;
                if (int.TryParse(rgszTimeItems[0], out iTrackNum))
                {
                    HeatTime ht = this.CurrentHeat.HeatTimes.First(x => x.TrackNumber == iTrackNum);
                    if (decimal.TryParse(rgszTimeItems[1], out dcRaceTime))
                    {
                        ht.RaceTime = dcRaceTime;
                    }
                }
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            this.timerCountdown.Stop();
            CountdownAmt--;
            if (this.CountdownAmt > 0)
            {
                this.timerCountdown.Start();
            }
            else
            {
                this.CountdownVisible = false;
                InRace = true;
                SendStartRaceSerialCommand();
            }

        }

        RelayCommand _addNewRacerCommand;
        public RelayCommand AddNewRacerCommand
        {
            get
            {
                if (_addNewRacerCommand == null)
                {
                    _addNewRacerCommand = new RelayCommand(this.AddNewRacer);
                }
                return _addNewRacerCommand;
            }
        }

        RelayCommand _createHeatsCommand;
        public RelayCommand CreateHeatsCommand
        {
            get
            {
                if (_createHeatsCommand == null)
                {
                    _createHeatsCommand = new RelayCommand(this.CreateHeats);
                }
                return _createHeatsCommand;
            }
        }

        RelayCommand _startRaceCommand;
        public RelayCommand StartRaceCommand
        {
            get
            {
                if (_startRaceCommand == null)
                {
                    _startRaceCommand = new RelayCommand(this.StartRace);
                }
                return _startRaceCommand;
            }
        }

        RelayCommand _sendStartRaceCommand;
        public RelayCommand SendStartRaceCommand
        {
            get
            {
                if (_sendStartRaceCommand == null)
                {
                    _sendStartRaceCommand = new RelayCommand(this.SendStartRace);
                }
                return _sendStartRaceCommand;
            }
        }

        RelayCommand _sendCloseGateCommand;
        public RelayCommand SendCloseGateCommand
        {
            get
            {
                if (_sendCloseGateCommand == null)
                {
                    _sendCloseGateCommand = new RelayCommand(this.SendCloseGateSerialCommand);
                }
                return _sendCloseGateCommand;
            }
        }

        private void SendStartRace()
        {
            if (!this.CurrentHeat.Completed)
            {
                System.Windows.Media.MediaPlayer mp = new System.Windows.Media.MediaPlayer();
                Uri uri1 = new Uri(@"pack://siteoforigin:,,,/Sounds/lights.wav", UriKind.Absolute);
                mp.Open(uri1);
                mp.Play();
                this.CountdownVisible = true;
                this.CountdownAmt = 4;
                this.timerCountdown.Start();
            }
            else
            {
                if (NextHeat != null)
                {
                    this.CurrentHeat = NextHeat;
                }
                else
                {
                    this.Event.CalculateResults();
                    this.CurrentHeatVisible = false;
                    this.FinalResultsVisible = true;
                }
            }
        }

        private void CreateHeats()
        {
            this.Event.CreateHeats();
            this.HeatsVisible = true;
            this.RacersVisible = false;
        }

        private void AddNewRacer()
        {
            Racer r = new Racer();
            r.RegNumber = this.Event.Racers.Count + 1;
            r.RacerID = Guid.NewGuid();
            r.EventID = this.Event.EventID;
            this._event.Racers.Add(r);
        }

        private void StartRace()
        {
            this.HeatsVisible = false;
            this.CurrentHeatVisible = true;
            this.CurrentHeat = this.Event.Heats[0];
            System.Windows.Media.MediaPlayer mp = new System.Windows.Media.MediaPlayer();
            Uri uri1 = new Uri(@"../../Resources/race1.wav", UriKind.Relative);
            mp.Open(uri1);
            mp.Play();
        }

        private void SendStartRaceSerialCommand()
        {
            sp.Write("s");
        }

        private void SendCloseGateSerialCommand()
        {
            sp.Write("c");
        }

    }
}
