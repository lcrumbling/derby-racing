using System;

namespace TimerApp
{
    public static class Utils
    {
        public static string PadLeft(string input, int len)
        {
            string output = input;
            while (output.Length <= len)
            {
                output = " " + output;
            }
            return output;
        }
    }
}
