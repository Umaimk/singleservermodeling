using System;
using System.Collections.Generic;
using System.Linq;

namespace OPDQueueSimulator
{
   
    public sealed class Gg1Analysis
    {
        public Gg1Analysis(IReadOnlyList<Patient> patients)
        {
            if (patients.Count < 2)
            {
                throw new ArgumentException("At least two patients are required to calculate inter-arrival statistics.", nameof(patients));
            }

            var orderedPatients = patients.OrderBy(p => p.SerialNo).ToList();
            var interArrivalTimes = orderedPatients
                .Zip(orderedPatients.Skip(1), (previous, current) =>
                    (current.ArrivalTime - previous.ArrivalTime).TotalMinutes)
                .ToList();
            var serviceTimes = orderedPatients
                .Select(p => p.ServiceTime.TotalMinutes)
                .ToList();

            MeanInterArrivalMinutes = Mean(interArrivalTimes);
            MeanServiceMinutes = Mean(serviceTimes);
            if (MeanInterArrivalMinutes <= 0 || MeanServiceMinutes <= 0)
            {
                throw new InvalidOperationException("Arrival and service means must be positive.");
            }
            Lambda = 1.0 / MeanInterArrivalMinutes;
            Mu = 1.0 / MeanServiceMinutes;
            Rho = Lambda / Mu;
            IdleFactor = 1.0 - Rho;
            CaSquared = Variance(interArrivalTimes, MeanInterArrivalMinutes) /
                        Math.Pow(1.0 / Lambda, 2);
            CsSquared = Variance(serviceTimes, MeanServiceMinutes) /
                        Math.Pow(1.0 / Mu, 2);

            if (Rho < 1.0)
            {
                Lq = (Rho * Rho * (1.0 + CsSquared) *
                      (CaSquared + Rho * Rho * CsSquared)) /
                     (2.0 * (1.0 - Rho) * (1.0 + Rho * Rho * CsSquared));
                Wq = Lq / Lambda;
                W = Wq + 1.0 / Mu;
                L = Lambda * W;
            }
            else
            {
                Lq = double.NaN;
                Wq = double.NaN;
                W = double.NaN;
                L = double.NaN;
            }

            ObservedWaitingMinutes = Mean(orderedPatients.Select(p =>
                (p.ActualServiceStart - p.ArrivalTime).TotalMinutes));
            ObservedTimeInSystemMinutes = Mean(orderedPatients.Select(p =>
                (p.ActualServiceEnd - p.ArrivalTime).TotalMinutes));
            ObservedUtilization = orderedPatients.Sum(p => p.ServiceTime.TotalMinutes) /
                                  (orderedPatients.Last().ActualServiceEnd -
                                   orderedPatients.First().ActualServiceStart).TotalMinutes;
            ThroughputPerMinute = orderedPatients.Count /
                                  (orderedPatients.Last().ActualServiceEnd -
                                   orderedPatients.First().ActualServiceStart).TotalMinutes;
        }

        public double Lambda { get; }
        public double Mu { get; }
        public double MeanInterArrivalMinutes { get; }
        public double MeanServiceMinutes { get; }
        public double Rho { get; }
        public double IdleFactor { get; }
        public double CaSquared { get; }
        public double CsSquared { get; }
        public double Lq { get; }
        public double Wq { get; }
        public double W { get; }
        public double L { get; }
        public double ObservedWaitingMinutes { get; }
        public double ObservedTimeInSystemMinutes { get; }
        public double ObservedUtilization { get; }
        public double ThroughputPerMinute { get; }

        private static double Mean(IEnumerable<double> values)
        {
            var sample = values.ToList();
            return sample.Average();
        }

        private static double Variance(IReadOnlyList<double> values, double mean) =>
            values.Sum(value => Math.Pow(value - mean, 2)) / values.Count;
    }
}
