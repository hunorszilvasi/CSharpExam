PonyExpress --- Delivery Manager
UMFST Practical Programming Exam · Variant A
Emergency deliveries, fair scheduling, persistent storage, and
background processing.

  Exam detail                         Value
  Duration                            120 minutes
  Maximum score                       20 points
  Allowed tools                       Visual Studio, GitHub Copilot,
                                      internet resources, and
                                  documentation
Student:
______________________________
Group: ______________________________
Date: ______________________________
1. Scenario
PonyExpress delivers emergency packages using a fleet of unusually
disciplined ponies. The task is to create a C# Windows Forms application
that registers deliveries, saves them in a local SQLite database, and
processes them one by one according to a specified priority rule.
2. Critical dataset requirement
Non-negotiable grading rule: The application must have a working
Generate dataset feature, and the submitted SQLite database must
contain at least 100 valid delivery records. If either condition
fails, the examination score is fixed at 1/20, regardless of other
functionality.

Dataset specification
- Generate at least 100 Delivery records programmatically and store
  them in SQLite.
- Include at least 30 records with Priority = 1, at least 30 with
  Priority = 2, and at least 30 with Priority = 3. Any remaining
  records may use any valid priority.
- Every generated record must have:
  - a non-empty Recipient;
  - a unique, automatically generated Id;
  - an initial Status of Pending.
- The generator must work with a fresh database.
- Records must remain in the database after processing, even if their
  statuses change.
- The generator must create actual SQLite rows, not entries that exist
  only in the UI.
3. General requirements
- Create a C# Windows Forms desktop application.
- Use a local SQLite database.
- Use System.Threading.Thread for background processing.
- Implement at least two meaningful LINQ operations.
- Write at least two specified event types to a persistent trace.log
  file.
- The application must compile, remain responsive during processing,
  and preserve data across restarts.
- GitHub Copilot, internet resources, and documentation are allowed,
  but the student must understand and be able to explain the
  implementation.
4. Object-oriented design --- 3 points
- Create an abstract Delivery class with:
  - Id
  - Recipient
  - Priority
  - Status
  - abstract GetProcessingDelayMs()
- Create RegularDelivery for priorities 1 and 2. It must return a
  processing delay of 600 ms.
- Create ExpressDelivery for priority 3. It must return a processing
  delay of 300 ms.
- Define an IDeliveryRepository interface for insert, load, and
  status-update operations.
- Implement the interface in SqliteDeliveryRepository.
- Enforce valid data through encapsulation.
- Reconstruct the correct subtype when loading persisted data.
- Call the overridden delay method polymorphically.
5. SQLite database --- 4 points
Create ponyexpress.db automatically if it does not already exist.
Create the following table and use parameterized SQL.
  Column        SQLite type / constraint
  Id          INTEGER PRIMARY KEY AUTOINCREMENT
  Recipient   TEXT NOT NULL
  Priority    INTEGER NOT NULL
  Status      TEXT NOT NULL
The application must:
- Insert new deliveries.
- Load all deliveries.
- Update delivery status.
- Preserve records after the application restarts.
- Support the valid statuses Pending, Processing, and Completed.
- Persist every status transition.
- Ensure that Generate dataset inserts real records into SQLite.
6. Windows Forms interface --- 2 points
The interface must include:
- A recipient text box.
- A priority selector with options 1, 2, and 3.
- An Add delivery button.
- A Generate dataset button.
- A DataGridView displaying Id, Recipient, Priority, and
  Status.
- Start processing and Stop processing buttons.
- Counts of deliveries in Pending, Processing, and Completed
  states.
- Validation that rejects an empty recipient and an invalid priority.
7. Background processing with threads --- 4 points
- Start exactly one background System.Threading.Thread when
  processing is requested.
- Do not use Task or async/await as a replacement for the
  required Thread.
- Repeatedly choose the next pending delivery according to the
  priority scheduler.
- Set its status to Processing in SQLite.
- Sleep for the delivery's polymorphic processing delay.
- Set its status to Completed.
- Refresh UI controls using Invoke or BeginInvoke.
- Keep the UI responsive and prevent a second worker from starting
  while one is active.
- Stop cooperatively after the active delivery finishes.
- Allow processing to be started again after stopping.
- Do not use Thread.Abort().
8. Priority scheduler --- 4 points
The scheduler must follow these rules:
1. Priority 3 normally comes before priorities 2 and 1.
2. For deliveries with equal priority, process the lower Id first.
3. After two consecutive priority-3 deliveries, process the oldest
   waiting lower-priority delivery, if one exists. This is the delivery
   with the smallest Id among priorities 1 and 2.
4. Reset the consecutive-priority-3 counter after processing a
   lower-priority delivery.
5. If no lower-priority delivery is waiting, continue processing
   priority-3 deliveries.
6. The algorithm must work for any valid pending dataset. Hardcoded ID
   sequences are not accepted.
Example
Id Recipient     Priority
 1 Alex                 3
 2 Bea                  3
 3 Cora                 3
 4 Deni                 1
 5 Eva                  2
 6 Feri                 3
Expected processing order:
1 → 2 → 4 → 3 → 6 → 5
9. LINQ --- two required uses, 2 points
Both requirements must be implemented using LINQ over Delivery
objects.
1. Scheduling: Filter and order candidate deliveries as part of
   selecting the next delivery. Suitable operations include Where,
   OrderBy, and FirstOrDefault.
2. UI status counts: Use GroupBy over Delivery objects to
   calculate counts for each delivery status. SQL GROUP BY alone does
   not satisfy this requirement.
10. Trace logging --- 1 point
- Configure System.Diagnostics.Trace with a
  TextWriterTraceListener that writes to trace.log.
- Log these two event types:
  - a delivery being added;
  - a delivery being completed.
- Each entry must include a timestamp and the delivery Id.
- Flush the listener so the log can be inspected after shutdown.
11. Submission requirements
Submit the Git repository URL before the 120-minute deadline.
The repository must include:
- The Visual Studio solution.
- All source code.
- A short README with instructions for running the application.
- The completed SQLite database.
- The exact database file location stated in the README.
- The dataset generator in source code, because the instructor may
  test it with a fresh database.
- trace.log, or instructions explaining how to generate it during a
  run.
The submitted ponyexpress.db must contain at least 100 delivery rows
and must be the database file actually used by the application.
The instructor may request a brief explanation of any implementation
detail.
12. Dataset grading rule
A missing or non-working dataset generator, fewer than 100 valid
delivery rows in the submitted SQLite database, or fake UI-only rows
results in a fixed score of 1/20.
13. Pre-submission checklist
- [ ] Verify that the actual SQLite database contains at least 100
  main-entity rows.
- [ ] Verify that the generator works with a fresh database.
- [ ] Verify that at least 30 rows exist for each priority: 1, 2, and
  3.
- [ ] Restart the application and confirm that saved records remain
  visible.
- [ ] Start, stop, and restart the processing thread without UI
  freezes or errors.
- [ ] Verify both required LINQ operations in the source code.
- [ ] Verify both required Trace event types in the source code and
  log output.
- [ ] Confirm that the repository contains the database, solution,
  source code, and README.
- [ ] State the exact database file location in the README.
- [ ] Confirm that the submitted database is the same file the
  application uses.
Exam: UMFST Practical Programming Exam · Variant A
Project: PonyExpress --- Delivery Manager
