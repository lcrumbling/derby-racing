using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace GSRacing.RacingObjects
{
    public class RaceHeat: ObservableObject
    {
        private Guid _heatID;
        public Guid HeatID
        {
            get { return _heatID; }
            set
            {
                this.Set(ref this._heatID, value);
            }
        }

        private int _heatNumber;
        public int HeatNumber
        {
            get { return _heatNumber; }
            set
            {
                this.Set(ref this._heatNumber, value);
            }
        }

        private Guid _eventID;
        public Guid EventID
        {
            get { return _eventID; }
            set
            {
                this.Set(ref this._eventID, value);
            }
        }

        private ObservableCollection<HeatTime> _heatTimes;
        public ObservableCollection<HeatTime> HeatTimes
        {
            get { return _heatTimes; }
            set
            {
                this.Set(ref this._heatTimes, value);
            }
        }
        private bool _completed = false;
        public bool Completed
        {
            get { return _completed; }
            set
            {
                this.Set(ref this._completed, value);
            }
        }

        public RaceHeat()
        {
            HeatTimes = new ObservableCollection<HeatTime>();
        }

        public bool HasRacer(Guid RacerID)
        {
            // is this racer already in this heat?
            return (this.HeatTimes.Count(x => x.Racer.RacerID == RacerID) > 0);
        }
    }

    public class HeatTime: ObservableObject
    {
        private Guid _heatID;
        public Guid HeatID
        {
            get { return _heatID; }
            set
            {
                this.Set(ref this._heatID, value);
            }
        }

        private Racer _racer;
        public Racer Racer
        {
            get { return _racer; }
            set
            {
                this.Set(ref this._racer, value);
            }
        }

        private int _trackNumber;
        public int TrackNumber
        {
            get { return _trackNumber; }
            set
            {
                this.Set(ref this._trackNumber, value);
            }
        }

        private decimal? _raceTime;
        public decimal? RaceTime
        {
            get { return _raceTime; }
            set
            {
                this.Set(ref this._raceTime, value);
                RaisePropertyChanged("FormattedRaceTime");
            }
        }

        public string FormattedRaceTime
        {
            get
            {
                if (this.RaceTime.HasValue)
                    return string.Format("{0}s", this.RaceTime.Value);

                return "---";
            }

        }

    }

}
