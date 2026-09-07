using System;
using System.Collections.Generic;
using System.Linq;

namespace OPDQueueSimulator
{
    /// <summary>
    /// Implements a deterministic single-server (single-channel), FCFS
    /// queueing simulation:
    ///
    ///   ServiceStart[i] = max( Arrival[i], ServiceEnd[i-1] )
    ///   ServiceEnd[i]   = ServiceStart[i] + ServiceTime[i]
    ///   Wait[i]         = ServiceStart[i] - Arrival[i]
    ///   TimeInSystem[i] = ServiceEnd[i] - Arrival[i]
    ///   Idle[i]         = max( 0, ServiceStart[i] - ServiceEnd[i-1] )
    ///
    /// This is the standard single-server queue recurrence taught in
    /// simulation/operations-research courses. The server can only ever be
    /// doing one of: idle, or serving exactly one customer, at any instant.
    /// </summary>
    public class QueueSimulator
    {
        public List<Patient> Patients { get; }

        public QueueSimulator(List<Patient> patients)
        {
            // Defensive copy, sorted by arrival order (FCFS = arrival order).
            Patients = patients.OrderBy(p => p.SerialNo).ToList();
        }

        public void Run()
        {
            TimeSpan? previousServiceEnd = null;

            foreach (var p in Patients)
            {
                TimeSpan serviceStart = previousServiceEnd is null
                    ? p.ActualServiceStart   // server has no prior history yet;
                                              // anchor the very first customer to
                                              // the actual opening-of-service time
                    : Max(p.ArrivalTime, previousServiceEnd.Value);

                TimeSpan serviceEnd = serviceStart + p.ServiceTime;

                p.SimulatedServiceStart = serviceStart;
                p.SimulatedServiceEnd = serviceEnd;
                p.WaitingTimeInQueue = serviceStart - p.ArrivalTime;
                p.TimeInSystem = serviceEnd - p.ArrivalTime;
                p.ServerIdleTimeBefore = previousServiceEnd is null
                    ? TimeSpan.Zero
                    : Max(TimeSpan.Zero, serviceStart - previousServiceEnd.Value);

                previousServiceEnd = serviceEnd;
            }
        }

        private static TimeSpan Max(TimeSpan a, TimeSpan b) => a > b ? a : b;

        // ---------------- Summary statistics ----------------

        public int TotalCustomers => Patients.Count;

        public TimeSpan TotalServiceTime =>
            TimeSpan.FromMinutes(Patients.Sum(p => p.ServiceTime.TotalMinutes));

        public TimeSpan TotalIdleTime =>
            TimeSpan.FromMinutes(Patients.Sum(p => p.ServerIdleTimeBefore.TotalMinutes));

        public TimeSpan TotalWaitingTime =>
            TimeSpan.FromMinutes(Patients.Sum(p => p.WaitingTimeInQueue.TotalMinutes));

        // Wall-clock span the server was open, first arrival's service start
        // through the last customer's service end.
        public TimeSpan TotalElapsedTime =>
            Patients.Count == 0
                ? TimeSpan.Zero
                : Patients.Last().SimulatedServiceEnd - Patients.First().SimulatedServiceStart;

        public double AverageWaitingTimeMinutes =>
            TotalCustomers == 0 ? 0 : TotalWaitingTime.TotalMinutes / TotalCustomers;

        public double AverageServiceTimeMinutes =>
            TotalCustomers == 0 ? 0 : TotalServiceTime.TotalMinutes / TotalCustomers;

        public double AverageTimeInSystemMinutes =>
            TotalCustomers == 0 ? 0 : Patients.Sum(p => p.TimeInSystem.TotalMinutes) / TotalCustomers;

        // Server utilization = fraction of the open period spent busy serving.
        public double ServerUtilizationPercent =>
            TotalElapsedTime.TotalMinutes == 0
                ? 0
                : (TotalServiceTime.TotalMinutes / TotalElapsedTime.TotalMinutes) * 100.0;

        public int CustomersWhoWaited =>
            Patients.Count(p => p.WaitingTimeInQueue.TotalMinutes > 0);

        public double ProbabilityOfWaiting =>
            TotalCustomers == 0 ? 0 : (double)CustomersWhoWaited / TotalCustomers * 100.0;

        public Patient? LongestWait =>
            Patients.OrderByDescending(p => p.WaitingTimeInQueue).FirstOrDefault();
    }
}
