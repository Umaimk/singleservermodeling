using System;

namespace OPDQueueSimulator
{
   
    public class Patient
    {
      
        public int SerialNo { get; set; }
        public string Name { get; set; } = string.Empty;
        public TimeSpan ArrivalTime { get; set; }
        public TimeSpan ActualServiceStart { get; set; }
        public TimeSpan ActualServiceEnd { get; set; }

        public TimeSpan ServiceTime => ActualServiceEnd - ActualServiceStart;

        // ---- Computed by the single-server simulation ----
        public TimeSpan SimulatedServiceStart { get; set; }
        public TimeSpan SimulatedServiceEnd { get; set; }
        public TimeSpan WaitingTimeInQueue { get; set; }
        public TimeSpan TimeInSystem { get; set; }
        public TimeSpan ServerIdleTimeBefore { get; set; }

        public static string FormatTime(TimeSpan t) => t.ToString(@"hh\:mm");

        public static string FormatDuration(TimeSpan t)
        {
            int totalMinutes = (int)t.TotalMinutes;
            return totalMinutes + " min";
        }
    }
}
