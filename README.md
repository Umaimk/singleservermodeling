# Single-Server Queue Simulator (C#, Console)

Case study: **NICVD Cardiac OPD – Group 1** register (20 patients: Serial No.,
Name, Arrival Time, Service Start Time, Service End Time).

A cardiac OPD staffed by one doctor is a classic **single-server (single-channel),
First-Come-First-Served queueing system**: one customer (patient) is served at
a time, and anyone who arrives while the doctor is busy waits in the queue.

## What the program does

1. Loads the 20-patient dataset (embedded in `OpdDataset.cs`, taken directly
   from the supplied spreadsheet).
2. Runs a deterministic single-server simulation using the standard
   recurrence:

   ```
   ServiceStart[i] = max( Arrival[i], ServiceEnd[i-1] )
   ServiceEnd[i]   = ServiceStart[i] + ServiceTime[i]
   Wait[i]         = ServiceStart[i] - Arrival[i]
   TimeInSystem[i] = ServiceEnd[i] - Arrival[i]
   Idle[i]         = max( 0, ServiceStart[i] - ServiceEnd[i-1] )
   ```

   `ServiceTime[i]` is taken as the actual recorded duration
   (`Service End Time − Service Start Time`) for each patient.

3. Presents a terminal menu to:
   - **View the per-patient queue table** (arrival, service time, simulated
     start/end, waiting time, time in system, idle time before that patient)
   - **View summary statistics**: average waiting time, average service
     time, average time in system, server utilization %, probability a
     patient has to wait, and the longest single wait
   - **Compare simulated vs. actual recorded end times**, to check where the
     FCFS single-server model matches the real register and where it
     diverges (e.g. small admin gaps between patients)

## Project structure

| File                  | Purpose                                              |
|-----------------------|-------------------------------------------------------|
| `Patient.cs`          | Data model: raw + computed fields for one patient      |
| `OpdDataset.cs`       | The 20-row dataset from the spreadsheet                |
| `QueueSimulator.cs`   | Core single-server simulation engine + statistics      |
| `Program.cs`          | Terminal menu / console UI (entry point)               |

## How to build and run

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download) (or later).

```bash
cd OPDQueueSimulator
dotnet run
```

You'll get an interactive menu:

```
==================================================================
   SINGLE-SERVER QUEUE SIMULATOR  -  NICVD CARDIAC OPD (Group 1)
==================================================================
 1. Show simulated queue table (per-patient)
 2. Show summary statistics
 3. Compare simulated vs. actual recorded times
 4. Exit

Enter your choice:
```

## Notes on the data

- The register's first patient's actual service start (10:30) is used as the
  simulation's opening time, since the doctor's exact start-of-day isn't in
  the data.
- Most consecutive patients show the server going idle for 0–2 minutes
  between services (recording/admin time) rather than starting the very
  instant the previous patient finished — the program reports this as
  `Idle` time per patient and totals it in the summary.
- One row in the source register (patient #16, Ali Ahmed) has a recorded
  service-start time that overlaps the previous patient's recorded end time.
  This is flagged automatically in the "Compare simulated vs. actual" view
  rather than silently hidden, since a true single-server system cannot
  serve two patients at once.

## Extending it

- Swap `OpdDataset.Load()` for a CSV/file reader if you want to feed in a
  different day's register without recompiling.
- Add a Monte-Carlo mode that samples random service times from a
  distribution fitted to this data, if your assignment asks for a
  stochastic (rather than trace-driven) simulation.
