using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSRacing.RacingObjects;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using System.IO.Ports;
using SQLite;

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

        private Guid _selectedEventID;
        public Guid SelectedEventID
        {
            get { return _selectedEventID; }
            set { this.Set(ref _selectedEventID, value); }
        }

        private bool _eventMenuVisible = false;
        public bool EventMenuVisible
        {
            get
            {
                return _eventMenuVisible;
            }
            set
            {
                this.Set(ref _eventMenuVisible, value);
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
                RaisePropertyChanged("PreviousHeat");
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
                if (this.Event == null)
                    return null;

                int currIndex = this.Event.Heats.IndexOf(this.CurrentHeat);
                if (currIndex == this.Event.Heats.Count - 1)
                    return null;

                return this.Event.Heats[currIndex + 1];
            }
        }
        public RaceHeat PreviousHeat
        {
            get
            {
                if (this.Event == null)
                    return null;

                int currIndex = this.Event.Heats.IndexOf(this.CurrentHeat);
                if (currIndex == 0)
                    return null;

                return this.Event.Heats[currIndex - 1];
            }
        }

        public ObservableCollection<RaceEvent> AllEvents
        {
            get
            {
                return RaceEvent.AllEvents(this.conn);
            }
        }


        public SQLiteConnection conn { get; set; }
        public MainViewModel()
        {
            this.conn = new SQLite.SQLiteConnection("racedata.db3");
            this.conn.CreateTable<RaceEvent>();
            this.conn.CreateTable<Racer>();
            this.conn.CreateTable<RaceHeat>();
            this.conn.CreateTable<HeatTime>();
            this.conn.CreateTable<RaceResult>();

            timerCountdown.Interval = new TimeSpan(0, 0, 1);
            timerCountdown.Tick += timer_Tick;

            this.sp = new SerialPort("COM11", 9600, Parity.None, 8, StopBits.One);
            this.sp.DataReceived += sp_DataReceived;
            szSerialBuffer = string.Empty;

            //Racer r1 = new Racer();
            //r1.RacerID = new Guid("{304E5445-8740-4853-94CD-B5BE08778449}");
            //r1.RegNumber = 1;
            //r1.EventID = this.Event.EventID;
            //r1.LastName = "Crumbling";
            //r1.FirstName = "Imogen";
            //this.Event.Racers.Add(r1);

            //Racer r2 = new Racer();
            //r2.RacerID = new Guid("{607962D9-735F-4EDC-8452-48B49CD5C089}");
            //r2.RegNumber = 2;
            //r2.EventID = this.Event.EventID;
            //r2.LastName = "Moore";
            //r2.FirstName = "Cat";
            //this.Event.Racers.Add(r2);

            //Racer r3 = new Racer();
            //r3.RacerID = new Guid("{816B7723-C258-4548-BE98-E44F164EEB26}");
            //r3.RegNumber = 3;
            //r3.EventID = this.Event.EventID;
            //r3.LastName = "Edelman";
            //r3.FirstName = "Emily";
            //this.Event.Racers.Add(r3);

            //Racer r4 = new Racer();
            //r4.RacerID = new Guid("{E86F7D84-339D-4C93-B925-89E75F51542B}");
            //r4.RegNumber = 4;
            //r4.EventID = this.Event.EventID;
            //r4.LastName = "Martin";
            //r4.FirstName = "Norah";
            //this.Event.Racers.Add(r4);

            //Racer r5 = new Racer();
            //r5.RacerID = new Guid("{C0660090-0340-496D-AA35-E91F54335EBE}");
            //r5.RegNumber = 5;
            //r5.EventID = this.Event.EventID;
            //r5.LastName = "Westphal";
            //r5.FirstName = "Maya";
            //this.Event.Racers.Add(r5);

            //Racer r6 = new Racer();
            //r6.RacerID = new Guid("{BFB087D6-F727-4702-A8C0-4710A06A5EFD}");
            //r6.RegNumber = 6;
            //r6.EventID = this.Event.EventID;
            //r6.LastName = "Luther";
            //r6.FirstName = "Georgia";
            //this.Event.Racers.Add(r6);

            //Racer r7 = new Racer();
            //r7.RacerID = new Guid("{A04F2B4A-CD64-4742-8983-10A25EDADC30}");
            //r7.RegNumber = 7;
            //r7.EventID = this.Event.EventID;
            //r7.LastName = "Celot";
            //r7.FirstName = "Olivia";
            //this.Event.Racers.Add(r7);

            //Racer r8 = new Racer();
            //r8.RacerID = new Guid("{CEC03690-E58F-4B9D-AD34-7F6D29D8D5DA}");
            //r8.RacerID = Guid.NewGuid();
            //r8.RegNumber = 8;
            //r8.EventID = this.Event.EventID;
            //r8.LastName = "Phillips";
            //r8.FirstName = "Genevieve";
            //this.Event.Racers.Add(r8);

            //Racer r9 = new Racer();
            //r9.RacerID = new Guid("{5A3558F0-EDB3-45D0-B051-30F8CCDCD797}");
            //r9.RegNumber = 9;
            //r9.EventID = this.Event.EventID;
            //r9.LastName = "Fox";
            //r9.FirstName = "Tori";
            //this.Event.Racers.Add(r9);

            //Racer r10 = new Racer();
            //r10.RacerID = new Guid("{E1407FE5-FA44-4828-A70E-F9E7BC3B7702}");
            //r10.RegNumber = 10;
            //r10.EventID = this.Event.EventID;
            //r10.LastName = "Speizer";
            //r10.FirstName = "Megan";
            //this.Event.Racers.Add(r10);

            //Racer r11 = new Racer();
            //r11.RacerID = new Guid("{F25C821B-3A40-4206-800E-2AE8320838BD}");
            //r11.RegNumber = 11;
            //r11.EventID = this.Event.EventID;
            //r11.LastName = "Smith";
            //r11.FirstName = "Bella";
            //this.Event.Racers.Add(r11);

            //Racer r12 = new Racer();
            //r12.RacerID = new Guid("{B11F1501-08BD-469F-8FC2-6A7AA44D7800}");
            //r12.RegNumber = 12;
            //r12.EventID = this.Event.EventID;
            //r12.LastName = "Scott";
            //r12.FirstName = "Rosalyn";
            //this.Event.Racers.Add(r12);

            this.EventMenuVisible = true;

        }

        ~MainViewModel()
        {
            this.conn.Close();
            this.conn.Dispose();
            this.conn = null;
            sp.Dispose();
            sp = null;
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
                if (iETXIndex > -1)
                {
                    string szMessage = szSerialBuffer.Substring(iSTXIndex + 1, iETXIndex - iSTXIndex -1);
                    szSerialBuffer = szSerialBuffer.Remove(iSTXIndex, (iETXIndex + 1) - iSTXIndex);
                    InRace = false;
                    ParseMessage(szMessage);
                    this.CurrentHeat.Completed = true;
                }
            }
            
        }


        private void ParseMessage(string szMessage)
        {
            string[] rgszTimes = szMessage.Split('|');
            foreach (string szTime in rgszTimes)
            {
                string[] rgszTimeItems = szTime.Split(':');
                int iTrackNum = 0;
                decimal dcRaceTime = 0;
                if (int.TryParse(rgszTimeItems[0], out iTrackNum))
                {
                    HeatTime ht = this.CurrentHeat.HeatTimes.First(x => x.TrackNumber == iTrackNum);
                    if (ht != null)
                    {
                        if (decimal.TryParse(rgszTimeItems[1], out dcRaceTime))
                        {
                            ht.RaceTime = dcRaceTime;
                        }
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

        #region RelayCommands

        RelayCommand _loadEventCommand;
        public RelayCommand LoadEventCommand
        {
            get
            {
                if (_loadEventCommand == null)
                {
                    _loadEventCommand = new RelayCommand(this.LoadEvent);
                }
                return _loadEventCommand;
            }
        }

        RelayCommand _createEventCommand;
        public RelayCommand CreateEventCommand
        {
            get
            {
                if (_createEventCommand == null)
                {
                    _createEventCommand = new RelayCommand(this.CreateEvent);
                }
                return _createEventCommand;
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

        RelayCommand<Racer> _removeRacerCommand;
        public RelayCommand<Racer> RemoveRacerCommand
        {
            get
            {
                if (_removeRacerCommand == null)
                {
                    _removeRacerCommand = new RelayCommand<Racer>(r => this.RemoveRacer(r));
                }
                return _removeRacerCommand;
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

        RelayCommand _resetHeatCommand;
        public RelayCommand ResetHeatCommand
        {
            get
            {
                if (_resetHeatCommand == null)
                {
                    _resetHeatCommand = new RelayCommand(this.ResetHeat, () => { return this.CurrentHeatVisible; });
                }
                return _resetHeatCommand;
            }
        }

        RelayCommand _sendStartRaceCommand;
        public RelayCommand SendStartRaceCommand
        {
            get
            {
                if (_sendStartRaceCommand == null)
                {
                    _sendStartRaceCommand = new RelayCommand(this.SendStartRace, () => { return this.CurrentHeatVisible; });
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
                    _sendCloseGateCommand = new RelayCommand(this.SendCloseGateSerialCommand, () => { return this.CurrentHeatVisible; });
                }
                return _sendCloseGateCommand;
            }
        }

        RelayCommand _goBackToLastHeatCommand;
        public RelayCommand GoBackToLastHeatCommand
        {
            get
            {
                if (_goBackToLastHeatCommand == null)
                {
                    _goBackToLastHeatCommand = new RelayCommand(this.GoBackToLastHeat, () => { return this.CurrentHeatVisible; });
                }
                return _goBackToLastHeatCommand;
            }
        }

        #endregion

        private void GoBackToLastHeat()
        {
            if (PreviousHeat != null)
            {
                this.CurrentHeat = PreviousHeat;
            }
        }

        private void ResetHeat()
        {
            if (CurrentHeat == null)
                return;

            this.CurrentHeat.Completed = false;
            foreach (HeatTime ht in this.CurrentHeat.HeatTimes)
            {
                ht.RaceTime = null;
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
                    if (sp.IsOpen)
                        sp.Close();

                    if (this.Event.Results.Count == 0)
                        this.Event.CalculateResults(this.conn);

                    this.CurrentHeatVisible = false;
                    this.FinalResultsVisible = true;
                    this.Event.Completed = true;
                }
                this.Event.Save(this.conn);
            }
        }

        private void CreateHeats()
        {
            if (this.Event.Racers.Count == 0)
                return;

            if (this.Event.Heats.Count == 0)
            {
                this.Event.CreateHeats(conn);
                this.Event.Save(this.conn);
            }

            this.HeatsVisible = true;
            this.RacersVisible = false;
        }

        private void LoadEvent()
        {
            this.Event = new RaceEvent(this.SelectedEventID, this.conn);

            this.EventMenuVisible = false;
            this.RacersVisible = true;
        }

        private void CreateEvent()
        {
            this.Event = new RaceEvent();
            this.Event.EventName = "New Event";
            this.Event.EventDate = DateTime.Now.Date;
            this.Event.EventID = Guid.NewGuid();
            this.Event.Save(this.conn);

            this.EventMenuVisible = false;
            this.RacersVisible = true;
        }

        private void AddNewRacer()
        {
            Racer r = new Racer();
            r.RegNumber = this.Event.Racers.Count + 1;
            r.RacerID = Guid.NewGuid();
            r.EventID = this.Event.EventID;
            this._event.Racers.Add(r);
        }

        private void RemoveRacer(Racer r)
        {
            this.Event.Racers.Remove(r);
        }

        private void StartRace()
        {
            try
            {
                this.sp.Open();
            }
            catch (Exception exc)
            {
                if (System.Windows.MessageBox.Show("Failed to open COM Port (" + sp.PortName + "): " + exc.Message, "Error", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.No)
                   return;
            }
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
            if (sp.IsOpen)
                sp.Write("s");
        }

        private void SendCloseGateSerialCommand()
        {
            if (sp.IsOpen)
                sp.Write("c");
        }

    }
}
