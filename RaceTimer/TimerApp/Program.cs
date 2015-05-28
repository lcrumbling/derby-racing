using System;
using System.Threading;
using Microsoft.SPOT;
using SecretLabs.NETMF.Hardware.Netduino;

namespace TimerApp
{
    public class Program
    {
        private const int ACK = 0x06;
        static RaceData rd = new RaceData();
        static RaceDevices dev = new RaceDevices();

        public static void Main()
        {
            dev.OnCloseGateReceived += new RaceDevices.CloseGateReceivedEventHandler(dev_OnCloseGateReceived);
            dev.OnStartRaceReceived += new RaceDevices.StartRaceReceivedEventHandler(dev_OnStartRaceReceived);
            rd.AddTrack(1, Pins.GPIO_PIN_A0);
            rd.AddTrack(2, Pins.GPIO_PIN_A1);
            rd.AddTrack(3, Pins.GPIO_PIN_A2);
            rd.AddTrack(4, Pins.GPIO_PIN_A3);
            dev.SendSerialData(((char)ACK).ToString());

            while (true)
            {
                if (rd.RaceState == RaceData.RaceStates.StartingRace)
                {
                    rd.Start();
                    dev.SetServoState(true);
                    dev.Lcd.Clear();
                    dev.Lcd.WriteText("Race Started!");
                    rd.RaceState = RaceData.RaceStates.RaceStarted;
                }

                if (rd.RaceState == RaceData.RaceStates.RaceStarted)
                {
                    if (rd.TestFinishedAllTracks())
                    {
                        rd.RaceState = RaceData.RaceStates.NotInRace;
                        dev.SetLedState(false);
                        dev.SendSerialData(rd.GetRaceData());
                        Debug.Print("Race Done! Results: ");
                        Debug.Print("Lane 1: " + rd.GetTrackTime(1).ToString("F3"));
                        Debug.Print("Lane 2: " + rd.GetTrackTime(2).ToString("F3"));
                        Debug.Print("Lane 3: " + rd.GetTrackTime(3).ToString("F3"));
                        Debug.Print("Lane 4: " + rd.GetTrackTime(4).ToString("F3"));
                        dev.SendSerialData(((char)ACK).ToString());
                    }
                }

                dev.Lcd.Clear();
                dev.Lcd.WriteText("Lane 1:  " + Utils.PadLeft(rd.GetTrackHighWaterMark(1).ToString(), 3) + "    ");
                dev.Lcd.WriteText("Lane 2:  " + Utils.PadLeft(rd.GetTrackHighWaterMark(2).ToString(), 3) + "    ");
                dev.Lcd.WriteText("Lane 3:  " + Utils.PadLeft(rd.GetTrackHighWaterMark(3).ToString(), 3) + "    ");
                dev.Lcd.WriteText("Lane 4:  " + Utils.PadLeft(rd.GetTrackHighWaterMark(4).ToString(), 3) + "    ");

                Thread.Sleep(20);
            }
        }

        static void dev_OnStartRaceReceived(object sender, EventArgs e)
        {
            dev.SetLedState(true);
            if (rd.RaceState != RaceData.RaceStates.NotInRace)
                return;

            rd.RaceState = RaceData.RaceStates.StartingRace;
        }

        static void dev_OnCloseGateReceived(object sender, EventArgs e)
        {
            dev.SetServoState(false);
        }
    }
}
