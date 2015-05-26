using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace GSRacing.RacingObjects
{
    public class RaceEvent : ObservableObject
    {
        private Guid _eventID;
        public Guid EventID
        {
            get { return _eventID; }
            set
            {
                this.Set(ref this._eventID, value);
            }
        }

        private string _eventName;
        public string EventName
        {
            get { return _eventName; }
            set
            {
                this.Set(ref this._eventName, value);
            }
        }

        private DateTime _eventDate;
        public DateTime EventDate
        {
            get { return _eventDate; }
            set
            {
                this.Set(ref this._eventDate, value);
            }
        }
        private ObservableCollection<Racer> _racers;
        public ObservableCollection<Racer> Racers
        {
            get { return _racers; }
            set
            {
                this.Set(ref this._racers, value);
            }
        }

        private ObservableCollection<RaceHeat> _heats;
        public ObservableCollection<RaceHeat> Heats
        {
            get { return _heats; }
            set
            {
                this.Set(ref this._heats, value);
            }
        }

        private ObservableCollection<RaceResult> _results;
        public ObservableCollection<RaceResult> Results
        {
            get { return _results; }
            set
            {
                this.Set(ref this._results, value);
            }
        }

        private int _trackCount;
        public int TrackCount
        {
            get { return _trackCount; }
            set
            {
                this.Set(ref this._trackCount, value);
            }
        }

        public IEnumerable<HeatTime> AllHeats
        {
            get
            {
                var allHeats = from t in Heats.
                            SelectMany(l => l.HeatTimes)
                               select t;
                return allHeats;
            }

        }

        public RaceEvent()
        {
            this.Racers = new ObservableCollection<Racer>();
            this.Heats = new ObservableCollection<RaceHeat>();
            this.Results = new ObservableCollection<RaceResult>();
        }

        public string GetRacerName(Guid racerGuid)
        {
            Racer r = this.Racers.First(x => x.RacerID == racerGuid);
            return string.Format("{0} {1}.", r.FirstName, r.LastName.Substring(0, 1));
        }

        public void CreateHeats()
        {
            // each race must run on each lane once.
            // x lanes are available to race on simultaneously
            // heatcount = racers * x / x = racers
            int HeatCount = this.Racers.Count;
            int iTracks = this.TrackCount;

            //generate shuffled list of racers
            Random rng = new Random();
            List<Racer> shuffledRacers = this.Racers.Shuffle(rng).ToList();

            // create the heats + heat times
            for (int i = 0; i < HeatCount; i++)
            {
                RaceHeat h = new RaceHeat();
                h.EventID = this.EventID;
                h.HeatID = Guid.NewGuid();
                h.HeatNumber = (i + 1);
                this.Heats.Add(h);

                for (int j = 0; j < iTracks; j++)
                {
                    HeatTime ht = new HeatTime();
                    ht.HeatID = h.HeatID;
                    ht.TrackNumber = j + 1;
                    ht.RaceTime = null;
                    ht.Racer = null;
                    h.HeatTimes.Add(ht);
                }
            }

            for (int i = 0; i < iTracks; i++)
            {
                using (IEnumerator<Racer> ienumShuffled = shuffledRacers.GetEnumerator())
                {
                    foreach (RaceHeat heat in this.Heats)
                    {
                        if (!ienumShuffled.MoveNext())
                            break;
                        HeatTime ht = heat.HeatTimes.First(x => x.TrackNumber == (i + 1));
                        ht.Racer = ienumShuffled.Current;
                    }
                }
                Racer[] rgShuffledRacers = shuffledRacers.ToArray();
                int RotateBy = HeatCount / iTracks;

                // if less tracks than racers, we'll run into an issue where we won't rotate
                if (RotateBy == 0)
                    RotateBy = 1;

                rgShuffledRacers.RotateLeft(RotateBy);
                shuffledRacers = new List<Racer>(rgShuffledRacers);
            }

#if DEBUG
            StringBuilder sb = new StringBuilder();
            foreach (RaceHeat rh in this.Heats)
            {
                sb.AppendFormat("Heat {0}:  ", rh.HeatNumber.ToString().PadLeft(2));
                foreach (HeatTime ht in rh.HeatTimes)
                {
                    sb.AppendFormat("({0}): {1} {2}. ", ht.TrackNumber, ht.Racer.FirstName, ht.Racer.LastName.Substring(0, 1));
                }
                sb.AppendLine();
            }
            System.Diagnostics.Debug.Print(sb.ToString());
#endif

        }

        public void CalculateResults()
        {
            IEnumerable<HeatTime> allHeats = this.AllHeats;
            foreach (Racer r in this.Racers)
            {
                RaceResult res = new RaceResult();
                res.Racer = r;
                res.AvgRaceTime = allHeats.Where(hi => hi.Racer == r).Average(x => x.RaceTime.Value);
                this.Results.Add(res);
            }
        }
    }
}
