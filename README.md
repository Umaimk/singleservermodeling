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
   - **View G/G/1 analysis** calculated from the observed arrival and service
     data

## G/G/1 analysis

### System description and model justification

The 20 register rows represent patients arriving to a cardiac OPD staffed by
one doctor. The doctor is the single server and patients are handled
first-come, first-served (FCFS). A G/G/1 model is appropriate because there is
one server, while both inter-arrival and service times are allowed to vary
according to the observations. No uniform-distribution assumption is made.

### Definitions and formulas

- Arrival time is the receipt/token time.
- Service time is `Service End - Service Start`.
- Inter-arrival time is `current arrival - previous arrival`.
- `lambda = 1 / mean inter-arrival time`
- `mu = 1 / mean service time`
- `rho = lambda / mu`
- `Idle Factor = 1 - rho`
- `Ca^2 = Var(inter-arrival times) / (1/lambda)^2`
- `Cs^2 = Var(service times) / (1/mu)^2`

The implemented G/G/1 approximation is:

```text
Lq = [rho^2(1 + Cs^2)(Ca^2 + rho^2 Cs^2)]
    / [2(1-rho)(1 + rho^2 Cs^2)]
Wq = Lq / lambda
W  = Wq + 1 / mu
L  = lambda W
```

These are the mathematically required G/G/1 formulas; they must not be
replaced by heuristic or placeholder queue calculations.

Variance is the population variance of the observed sample. If `rho >= 1`,
the CLI warns that steady-state results are not valid.

### Output metrics

The G/G/1 view displays model, arrival/service (`lambda`, `mu`, mean
inter-arrival, mean service, `Ca^2`, `Cs^2`) and performance (`rho`, Idle
Factor, `Lq`, `Wq`, `W`, `L`) tables. It also displays observed waiting time,
time in system, utilization, and throughput where the register data supports
them. Queue length is reported by the required G/G/1 `Lq` formula and the
simulated per-patient queue view.

### Assumptions and limitations

- The 20 observations are treated as one arrival/service sample.
- The arrival order is the serial-number order in the supplied register.
- The G/G/1 result is an approximation based on sample moments, not a
  long-run simulation.
- The observed register contains administrative gaps and one recorded overlap;
  these can differ from the ideal FCFS recurrence.
- The first patient's recorded service start anchors the existing simulation's
  opening time.

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
 4. Show G/G/1 single-server analysis
 5. Exit

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
