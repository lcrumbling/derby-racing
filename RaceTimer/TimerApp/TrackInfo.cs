using System;
using Microsoft.SPOT;

namespace TimerApp
{
    public class TrackInfo
    {
        private const Int64 ticks_per_millisecond = System.TimeSpan.TicksPerMillisecond;
        public const int MAX_THRESHOLD = 500;

        public int TrackNumber { get; set; }
        public bool HasFinished { get; set; }
        public long StartTicks { get; set; }
        public long EndTicks { get; set; }
        public int HighWaterMark { get; set; }

        private SecretLabs.NETMF.Hardware.AnalogInput MonitoredInput;
        public long ElapsedTicks
        {
            get
            {
                return (EndTicks - StartTicks);
            }
        }

        public TrackInfo(int iTrackNumber, Microsoft.SPOT.Hardware.Cpu.Pin thePin)
        {
            this.TrackNumber = iTrackNumber;
            this.MonitoredInput = new SecretLabs.NETMF.Hardware.AnalogInput(thePin);
        }

        public void TestFinished(long CurrEndTime)
        {
            if (this.HasFinished)
                return;

            int CurrentSignal = MonitoredInput.Read();
            if (CurrentSignal > HighWaterMark)
                HighWaterMark = CurrentSignal;

            if (CurrentSignal > MAX_THRESHOLD)
            {
                this.EndTicks = CurrEndTime;
                this.HasFinished = true;
            }

        }

    }
}
