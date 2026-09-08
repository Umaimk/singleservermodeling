# Single-Server Queue Simulator (C#, Console)

Case study: **NICVD Cardiac OPD – Group 1** register (20 patients: Serial No.,
Name, Arrival Time, Service Start Time, Service End Time).

A cardiac OPD staffed by one doctor is a classic **single-server, First-Come-
First-Served (FCFS) queueing system**: one patient is served at a time, and
anyone who arrives while the doctor is busy waits in the queue.

This project analyzes the register two different ways:

1. **Trace-driven simulation** — replays the actual 20 patients through FCFS
   rules to see what a pure single-server model predicts for each of them.
2. **G/G/1 statistical analysis** — treats the arrival/service pattern as a
   sample and uses Kingman's approximation to predict long-run queue
   behavior (average wait, queue length, utilization) if this pattern
   continued indefinitely.

## Project structure

| File                 | Purpose                                                        |
|-----------------------|------------------------------------------------------------------|
| `Patient.cs`          | Data model: raw fields + fields computed by the simulation       |
| `OpdDataset.cs`       | The 20-row dataset, hardcoded from the spreadsheet                |
| `QueueSimulator.cs`   | Trace-driven single-server FCFS simulation + summary statistics   |
| `Gg1Analysis.cs`      | G/G/1 statistical model (λ, μ, ρ, Ca², Cs², Kingman's Lq/Wq/W/L)   |
| `Program.cs`          | Terminal menu / console UI (entry point)                         |

## How the simulation works

For each patient `i`, in arrival order:

ServiceStart[i] = max( Arrival[i], ServiceEnd[i-1] )
ServiceEnd[i] = ServiceStart[i] + ServiceTime[i]
Wait[i] = ServiceStart[i] - Arrival[i]
TimeInSystem[i] = ServiceEnd[i] - Arrival[i]
Idle[i] = max( 0, ServiceStart[i] - ServiceEnd[i-1] )


`ServiceTime[i]` is the actual recorded duration
(`Service End Time − Service Start Time`) for that patient. The first
patient is anchored to their actual recorded start time, since there's no
prior service end to compare against.

## How the G/G/1 analysis works

Instead of replaying individual patients, this model:

1. Computes inter-arrival gaps and service durations across the sample.
2. Derives λ (arrival rate), μ (service rate), and ρ = λ/μ (utilization).
3. Computes Ca² and Cs² — squared coefficients of variation, measuring how
   irregular arrivals and services are (this is the "General" in G/G/1,
   as opposed to assuming a specific distribution like M/M/1 does).
4. Applies **Kingman's approximation** to estimate steady-state `Lq`, `Wq`,
   `W`, `L` — but only if ρ < 1 (a stable system). If ρ ≥ 1, arrivals
   outpace service capacity on average and no steady-state exists, so the
   program reports this instead of a misleading number.
5. Separately reports the *actual observed* average wait, time in system,
   utilization, and throughput straight from the real data, so the model's
   predictions can be compared against what really happened.

## Project structure / menu

Requires the [.NET SDK](https://dotnet.microsoft.com/download) (project
targets `net10.0`).

```bash
cd OPDQueueSimulator
dotnet run
```
==================================================================
SINGLE-SERVER QUEUE SIMULATOR - NICVD CARDIAC OPD (Group 1)
Show simulated queue table (per-patient)
Show summary statistics
Compare simulated vs. actual recorded times
Show G/G/1 single-server analysis
Exit

- **Option 1** — per-patient table: arrival, service time, simulated
  start/end, wait, time in system, idle time before them.
- **Option 2** — summary statistics: totals, averages, server utilization %,
  how many patients waited, and the longest single wait.
- **Option 3** — simulated vs. actual end times, flagged as match / early /
  late per patient, to show where the pure FCFS model diverges from the
  real register (e.g. small admin gaps, or a recorded overlap).
- **Option 4** — full G/G/1 report: model identification, arrival/service
  statistics, Kingman's predicted Lq/Wq/W/L next to observed real numbers,
  the per-patient table, the comparison table, and a final interpretation.

## Notes on the data

- Most consecutive patients show the server idle for 0–2 minutes between
  services (admin/recording time) rather than starting the instant the
  previous patient finished — reported per patient as `Idle` and totaled in
  the summary.
- Patient #16 (Ali Ahmed) has a recorded service-start time that overlaps
  the previous patient's recorded end time in the source register — flagged
  automatically in the comparison view, since a true single server cannot
  serve two patients at once.
- Since all patients arrived well before the doctor's actual first
  recorded start (10:30), the trace-driven simulation shows the queue
  backlog stacking up early, and small real-world admin gaps accumulate
  into a growing difference between simulated and actual end times by the
  last few patients (see Option 3).

## Extending it

- Swap `OpdDataset.Load()` for a file/CSV reader to feed in a different
  day's register without recompiling.
- Add a Monte-Carlo mode that samples random service times from a fitted
  distribution, if a stochastic (rather than trace-driven) simulation is
  needed for comparison.
