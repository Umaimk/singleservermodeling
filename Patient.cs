using System;

namespace OPDQueueSimulator
{
    /// <summary>
    /// Represents one patient/customer arriving at the single-server system
    /// (NICVD Cardiac OPD, one doctor = one server, FCFS discipline).
    /// Raw fields come straight from the source data. Simulated* fields are
    /// computed by QueueSimulator using standard single-server queue rules.
    /// </summary>
    public class Patient
    {
        // ---- Raw input data (from the OPD register) ----
        public int SerialNo { get; set; }
        public string Name { get; set; } = string.Empty;
        public TimeSpan ArrivalTime { get; set; }
        public TimeSpan ActualServiceStart { get; set; }
        public TimeSpan ActualServiceEnd { get; set; }

        // ---- Derived input ----
        // Service time is treated as the "given"/random-variable duration for
        // the simulation, taken from the recorded start/end in the register.
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
