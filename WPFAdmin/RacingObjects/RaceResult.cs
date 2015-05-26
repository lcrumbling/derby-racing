using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace GSRacing.RacingObjects
{
    public class RaceResult : ObservableObject
    {
        private int _placeNumber;
        public int PlaceNumber
        {
            get { return _placeNumber; }
            set
            {
                this.Set(ref this._placeNumber, value);
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

        private decimal? _avgRaceTime;
        public decimal? AvgRaceTime
        {
            get { return _avgRaceTime; }
            set
            {
                this.Set(ref this._avgRaceTime, value);
            }
        }

    }
}
