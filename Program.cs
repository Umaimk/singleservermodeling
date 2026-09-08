using System;
using System.Linq;

namespace OPDQueueSimulator
{
   
    public static class Program
    {
        private static QueueSimulator? _sim;

        public static void Main()
        {
            Console.Title = "Single-Server Queue Simulator - NICVD Cardiac OPD";
            var patients = OpdDataset.Load();
            _sim = new QueueSimulator(patients);
            _sim.Run();
            var gg1 = new Gg1Analysis(patients);

            bool exit = false;
            while (!exit)
            {
                ShowMenu();
                string? choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        PrintPatientTable();
                        break;
                    case "2":
                        PrintSummaryStatistics();
                        break;
                    case "3":
                        PrintComparisonWithActual();
                        break;
                    case "4":
                        PrintGg1Analysis(gg1);
                        break;
                    case "5":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please choose 1-5.\n");
                        break;
                }

                if (!exit)
                {
                    Console.WriteLine("\nPress Enter to continue...");
                    Console.ReadLine();
                    Console.WriteLine();
                    Console.WriteLine(new string('=', 100));
                    Console.WriteLine();
                }
            }

            Console.WriteLine("Goodbye.");
        }

        private static void ShowMenu()
        {
            Console.WriteLine("==================================================================");
            Console.WriteLine("   SINGLE-SERVER QUEUE SIMULATOR  -  NICVD CARDIAC OPD (Group 1)");
            Console.WriteLine("==================================================================");
            Console.WriteLine(" 1. Show simulated queue table (per-patient)");
            Console.WriteLine(" 2. Show summary statistics");
            Console.WriteLine(" 3. Compare simulated vs. actual recorded times");
            Console.WriteLine(" 4. Show G/G/1 single-server analysis");
            Console.WriteLine(" 5. Exit");
            Console.Write("\nEnter your choice: ");
        }

        private static void PrintPatientTable()
        {
            var sim = _sim!;
            Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("{0,-4}{1,-26}{2,-9}{3,-9}{4,-9}{5,-9}{6,-9}{7,-9}{8,-9}",
                "S#", "Name", "Arrive", "SvcTime", "Start", "End", "Wait", "InSys", "Idle");
            Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");

            foreach (var p in sim.Patients)
            {
                Console.WriteLine("{0,-4}{1,-26}{2,-9}{3,-9}{4,-9}{5,-9}{6,-9}{7,-9}{8,-9}",
                    p.SerialNo,
                    Truncate(p.Name, 25),
                    Patient.FormatTime(p.ArrivalTime),
                    (int)p.ServiceTime.TotalMinutes + "m",
                    Patient.FormatTime(p.SimulatedServiceStart),
                    Patient.FormatTime(p.SimulatedServiceEnd),
                    (int)p.WaitingTimeInQueue.TotalMinutes + "m",
                    (int)p.TimeInSystem.TotalMinutes + "m",
                    (int)p.ServerIdleTimeBefore.TotalMinutes + "m");
            }
            Console.WriteLine("--------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("SvcTime = service duration, Wait = time queuing before service,");
            Console.WriteLine("InSys = total time in system (wait + service), Idle = server idle time just before this patient.");
        }

        private static void PrintSummaryStatistics()
        {
            var sim = _sim!;
            Console.WriteLine("---------------- SUMMARY STATISTICS ----------------");
            Console.WriteLine($"Total customers served        : {sim.TotalCustomers}");
            Console.WriteLine($"Total service (busy) time      : {(int)sim.TotalServiceTime.TotalMinutes} min");
            Console.WriteLine($"Total server idle time          : {(int)sim.TotalIdleTime.TotalMinutes} min");
            Console.WriteLine($"Total waiting time (all queue)  : {(int)sim.TotalWaitingTime.TotalMinutes} min");
            Console.WriteLine($"Total elapsed (open) time       : {(int)sim.TotalElapsedTime.TotalMinutes} min");
            Console.WriteLine();
            Console.WriteLine($"Average service time / patient  : {sim.AverageServiceTimeMinutes:F2} min");
            Console.WriteLine($"Server utilization               : {sim.ServerUtilizationPercent:F2} %");
            Console.WriteLine();
            Console.WriteLine($"Patients who had to wait          : {sim.CustomersWhoWaited} / {sim.TotalCustomers} " +
                               $"({sim.ProbabilityOfWaiting:F1} %)");

            var longest = sim.LongestWait;
            if (longest != null)
            {
                Console.WriteLine($"Longest wait                     : {(int)longest.WaitingTimeInQueue.TotalMinutes} min " +
                                   $"(Patient #{longest.SerialNo}, {longest.Name})");
            }
        }

        private static void PrintComparisonWithActual()
        {
            var sim = _sim!;
            Console.WriteLine("---- SIMULATED END TIME vs. ACTUAL RECORDED END TIME ----");
            Console.WriteLine("{0,-4}{1,-26}{2,-12}{3,-12}{4,-10}", "S#", "Name", "Simulated", "Actual", "Diff(min)");
            Console.WriteLine(new string('-', 65));

            foreach (var p in sim.Patients)
            {
                int diff = (int)(p.SimulatedServiceEnd - p.ActualServiceEnd).TotalMinutes;
                string flag = diff == 0 ? "match" : (diff > 0 ? $"+{diff} late" : $"{diff} early");
                Console.WriteLine("{0,-4}{1,-26}{2,-12}{3,-12}{4,-10}",
                    p.SerialNo, Truncate(p.Name, 25),
                    Patient.FormatTime(p.SimulatedServiceEnd),
                    Patient.FormatTime(p.ActualServiceEnd),
                    flag);
            }

            Console.WriteLine("\nNote: differences arise where the register shows small gaps between");
            Console.WriteLine("consecutive patients (extra handling/admin time) beyond pure FCFS queueing,");
            Console.WriteLine("or, in one case in this dataset, an overlap in the recorded times.");
        }

        private static void PrintGg1Analysis(Gg1Analysis analysis)
        {
            PrintSectionHeader("1. Model Identification - G/G/1");
            Console.WriteLine("{0,-28}{1,12}", "Metric", "Value");
            Console.WriteLine("{0,-28}{1,12}", "Queue model", "G/G/1");
            Console.WriteLine("{0,-28}{1,12}", "Server", "One doctor");
            Console.WriteLine();
            Console.WriteLine("Why G/G/1?");
            Console.WriteLine("- One doctor -> one server.");
            Console.WriteLine("- Arrival and service variability are represented from observed data.");
            Console.WriteLine("- No uniform-distribution assumption.");
            Console.WriteLine();

            PrintSectionHeader("2. Arrival & Service Statistics");
            Console.WriteLine("{0,-28}{1,12}", "Metric", "Value");
            Console.WriteLine("{0,-28}{1,12:F4}", "lambda (patients/min)", analysis.Lambda);
            Console.WriteLine("{0,-28}{1,12:F4}", "mu (patients/min)", analysis.Mu);
            Console.WriteLine("{0,-28}{1,12:F2}", "Mean inter-arrival (min)", analysis.MeanInterArrivalMinutes);
            Console.WriteLine("{0,-28}{1,12:F2}", "Mean service (min)", analysis.MeanServiceMinutes);
            Console.WriteLine("{0,-28}{1,12:F4}", "Ca^2", analysis.CaSquared);
            Console.WriteLine("{0,-28}{1,12:F4}", "Cs^2", analysis.CsSquared);
            Console.WriteLine();

            PrintSectionHeader("3. G/G/1 Performance Measures");
            Console.WriteLine("{0,-28}{1,12}", "Metric", "Value");
            Console.WriteLine("{0,-28}{1,12:P2}", "rho (utilization)", analysis.Rho);
            Console.WriteLine("{0,-28}{1,12:P2}", "Idle Factor", analysis.IdleFactor);
            PrintMetric("Lq (mean in queue)", analysis.Lq, "patients");
            PrintMetric("Wq (mean wait)", analysis.Wq, "min");
            PrintMetric("W (mean in system)", analysis.W, "min");
            PrintMetric("L (mean in system)", analysis.L, "patients");
            Console.WriteLine();
            Console.WriteLine("Observed data");
            PrintMetric("Waiting time", analysis.ObservedWaitingMinutes, "min");
            PrintMetric("Time in system", analysis.ObservedTimeInSystemMinutes, "min");
            Console.WriteLine("{0,-28}{1,12:P2}", "Utilization", analysis.ObservedUtilization);
            Console.WriteLine("{0,-28}{1,12:F4}", "Throughput (patients/min)", analysis.ThroughputPerMinute);
            Console.WriteLine();

            if (analysis.Rho >= 1.0)
            {
                Console.WriteLine();
                Console.WriteLine("WARNING: rho >= 1. Steady-state analytical results are not valid.");
            }

            PrintSectionHeader("4. Simulated Queue - Per Patient");
            PrintPatientTable();
            Console.WriteLine();
            PrintSectionHeader("5. Actual vs Simulated Comparison");
            PrintComparisonWithActual();
            Console.WriteLine();
            PrintSectionHeader("6. Final Interpretation");
            if (analysis.Rho < 1.0)
            {
                Console.WriteLine($"rho is below 1 ({analysis.Rho:P2}), so the steady-state approximation");
                Console.WriteLine($"is valid for this sample and predicts Lq = {analysis.Lq:F2} patients.");
            }
            else
            {
                Console.WriteLine("Steady-state analytical results are not valid because rho is at least 1.");
            }
            Console.WriteLine($"Observed utilization is {analysis.ObservedUtilization:P2} and throughput is " +
                              $"{analysis.ThroughputPerMinute:F4} patients/min.");
        }

        private static void PrintSectionHeader(string title)
        {
            Console.WriteLine($"---------------- {title} ----------------");
        }

        private static void PrintMetric(string label, double value, string unit) =>
            Console.WriteLine("{0,-28}{1,12}", label, double.IsNaN(value) ? "N/A" : $"{value:F2} {unit}");

        private static string Truncate(string s, int max) =>
            s.Length <= max ? s : s.Substring(0, max - 1) + "…";
    }
}
