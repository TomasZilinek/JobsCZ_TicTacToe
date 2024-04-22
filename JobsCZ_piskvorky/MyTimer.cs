using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsCZ_piskvorky
{
    public static class MyTimer
    {
        public static Dictionary<string, Stopwatch> stopWatchDict = new Dictionary<string, Stopwatch>();

        public static void CreateStopWatch(string tag)
        {
            stopWatchDict[tag] = new Stopwatch();
        }

        public static void ResumeStopWatch(string tag)
        {
            if (stopWatchDict.ContainsKey(tag))
                stopWatchDict[tag].Start();
        }

        public static void StopStopWatch(string tag)
        {
            if (stopWatchDict.ContainsKey(tag))
                stopWatchDict[tag].Stop();
        }

        public static void ResetAllStopwatches()
        {
            foreach (Stopwatch stopwatch in stopWatchDict.Values)
                stopwatch.Reset();
        }

        public static TimeSpan GetStopWatchTime(string tag)
        {
            if (stopWatchDict.ContainsKey(tag))
                return stopWatchDict[tag].Elapsed;
            else
                return new TimeSpan();
        }

        public static string StopWatchToString(string tag)
        {
            if (stopWatchDict.ContainsKey(tag))
                return $"Stopwatch {tag}: elapsed time = {GetStopWatchTime(tag).TotalSeconds} seconds";
            else
                return "";
        }
    }
}
