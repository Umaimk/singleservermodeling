using System;
using System.Linq;

namespace OPDQueueSimulator
{
    /// <summary>
    /// Single-Server Queueing System Simulator
    /// Case study: NICVD Cardiac OPD (Group 1) - one doctor (server),
    /// First-Come-First-Served discipline.
    /// </summary>
    public static class Program
    {
        private static QueueSimulator? _sim;

        public static void Main()
        {
            Console.Title = "Single-Server Queue Simulator - NICVD Cardiac OPD";
            var patients = OpdDataset.Load();
            _sim = new QueueSimulator(patients);
            _sim.Run();

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
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please choose 1-4.\n");
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
            Console.WriteLine(" 4. Exit");
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
            Console.WriteLine("---- Queueing-theory measures ----");
            Console.WriteLine($"Wq  (avg wait time in queue)      : {sim.Wq:F2} min");
            Console.WriteLine($"W   (avg time in system)          : {sim.W:F2} min");
            Console.WriteLine($"lambda (arrival rate)              : {sim.ArrivalRatePerMinute:F3} customers/min");
            Console.WriteLine($"Lq  (avg # waiting in queue)       : {sim.Lq:F2}");
            Console.WriteLine($"L   (avg # in system)             : {sim.L:F2}");
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

        private static string Truncate(string s, int max) =>
            s.Length <= max ? s : s.Substring(0, max - 1) + "…";
    }
}
