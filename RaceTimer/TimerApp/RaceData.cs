using System;
using System.Collections;
using Microsoft.SPOT;

namespace TimerApp
{
    public class RaceData
    {
        private const Int64 ticks_per_millisecond = System.TimeSpan.TicksPerMillisecond;
        private const long ONE_SECOND_IN_TICKS = 1 * 1000 * ticks_per_millisecond;
        private const int RACE_TIMEOUT = 8;        

        public RaceData()
        {
            _tracks = new ArrayList();
            this.RaceState = RaceData.RaceStates.NotInRace;
        }

        public enum RaceStates
        {
            NotInRace,
            StartingRace,
            RaceStarted,
        }

        public RaceStates RaceState;
        private ArrayList _tracks { get; set; }
        private long _maxTicksBeforeDNF;

        public void AddTrack(int iTrackNumber, Microsoft.SPOT.Hardware.Cpu.Pin thePin)
        {
            TrackInfo ti = new TrackInfo(iTrackNumber, thePin);
            this._tracks.Add(ti);
        }
        
        public void Reset()
        {
            foreach (TrackInfo o in this._tracks)
            {
                o.HasFinished = false;
                o.HighWaterMark = 0;
                o.StartTicks = 0;
                o.EndTicks = 0;
            }
        }

        private bool RaceIsFinished
        {
            get
            {
                bool fFinished = true;
                foreach (TrackInfo o in this._tracks)
                {
                    fFinished = fFinished && o.HasFinished;
                }
                return fFinished;
            }
        }

        public double GetTrackTime(int iTrackNumber)
        {
            TrackInfo ti = this.GetTrack(iTrackNumber);
            return TicksToSeconds(ti.ElapsedTicks);
        }

        private double TicksToSeconds(long lTicks)
        {
            return ((lTicks / ticks_per_millisecond) / 1000d);
        }

        public int GetTrackHighWaterMark(int iTrackNumber)
        {
            TrackInfo ti = this.GetTrack(iTrackNumber);
            return ti.HighWaterMark;
        }

        private TrackInfo GetTrack(int iTrackNumber)
        {
            foreach (TrackInfo ti in this._tracks)
            {
                if (ti.TrackNumber == iTrackNumber)
                    return ti;
            }
            return null;
        }

        private long GetCurrentTimeInTicks()
        {
            return Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks;
        }

        public void Start()
        {
            this.Reset();
            long CurrTicks = GetCurrentTimeInTicks();
            _maxTicksBeforeDNF = CurrTicks + (ONE_SECOND_IN_TICKS * RACE_TIMEOUT);
            foreach (TrackInfo o in this._tracks)
            {
                o.StartTicks = CurrTicks;
            }
        }

        internal bool TestFinishedAllTracks()
        {
            long CurrEndTime = GetCurrentTimeInTicks();
            if (CurrEndTime > _maxTicksBeforeDNF)
            {
                // force any unfinished track to finish
                foreach (TrackInfo o in this._tracks)
                {
                    if (!o.HasFinished)
                    {
                        o.HasFinished = true;
                        o.EndTicks = CurrEndTime;
                    }
                }
            }

            foreach (TrackInfo o in this._tracks)
            {
                o.TestFinished(CurrEndTime);
            }
            return this.RaceIsFinished;
        }

        private const int STX = 0x02;
        private const int ETX = 0x03;
        internal string GetRaceData()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append((char)STX);
            foreach (TrackInfo ti in _tracks)
            {
                if (sb.Length > 1)
                    sb.Append("|");
                sb.Append(ti.TrackNumber);
                sb.Append(":");
                sb.Append(TicksToSeconds(ti.ElapsedTicks).ToString("F3"));
            }
            sb.Append((char)ETX);
            return sb.ToString();
        }
    }
}
