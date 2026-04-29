# Event-Based Scheduled Matching System (Final Plan)

Transition the existing on-demand AI matching architecture into a scheduled, event-based system. Investors and startups will register for specific events. A background worker will process the matches when the event's scheduled time arrives, utilizing Python's fast vector similarity search. All code comments will be written in English.

## Proposed Changes

### Database Models & Context

#### [NEW] [MatchEvent.cs](file:///c:/Users/nefise/Desktop/BridgeApi/backend/MatchingApi/Models/MatchEvent.cs)

Create a new entity representing a matching event.

- Fields: `Id`, `Title`, `ScheduledAt`, `Status` (Upcoming, Open, Processing, Completed).
- Additional configuration fields: `TopMatchingCount` (default to 5), `EventType`, `FilterValue`.

#### [NEW] [EventParticipation.cs](file:///c:/Users/nefise/Desktop/BridgeApi/backend/MatchingApi/Models/EventParticipation.cs)

Create a new entity for mapping startups and investors to an event.

- Fields: `Id`, `EventId`, `ParticipantId` (string), `ParticipantType` (Investor/Startup), `JoinedAt`.

#### [MODIFY] [MatchResult.cs](file:///c:/Users/nefise/Desktop/BridgeApi/backend/MatchingApi/Models/MatchResult.cs)

- Add a nullable `EventId` foreign key to track which event generated the match.

#### [MODIFY] [AppDbContext.cs](file:///c:/Users/nefise/Desktop/BridgeApi/backend/MatchingApi/Data/AppDbContext.cs)

- Add `DbSet<MatchEvent>` and `DbSet<EventParticipation>`.
- Configure foreign keys and indexes appropriately.

---

### Background Workers

#### [NEW] [StartupIndexingWorker.cs](file:///c:/Users/nefise/Desktop/BridgeApi/backend/MatchingApi/Services/StartupIndexingWorker.cs)

- A generic `BackgroundService` that runs periodically (Every 24 hours / Daily).
- Purpose: Execute `Where(s => s.Embedding == null)` to retrieve startups missing AI embeddings and call the Python service's `/api/v1/index-startups` to pre-compute them.

#### [NEW] [EventMatchingWorker.cs](file:///c:/Users/nefise/Desktop/BridgeApi/backend/MatchingApi/Services/EventMatchingWorker.cs)

- A `BackgroundService` that wakes up periodically to check for `MatchEvent`s where `Status = 'Open'` and `ScheduledAt <= DateTime.UtcNow`.
- Routine:
  1. Change event status to `Processing`.
  2. Fetch all participating Investors and Startups for this specific event.
  3. Call `AiMatchingService` for each investor using _only_ the participating startups. Note: The Python backend automatically detects missing vectors (via `missing_ids` logic) and vectorizes them on the fly if necessary.
  4. Save results to `MatchResults` setting the respective `EventId`.
  5. Change event status to `Completed`.

---

### API Endpoints

#### [MODIFY] [MatchController.cs](file:///c:/Users/nefise/Desktop/BridgeApi/backend/MatchingApi/Controllers/MatchController.cs)

- Add `POST /api/v1/match/event/{id}/join`: Endpoint for checking user validity and adding an `EventParticipation` record.
- Add `DELETE /api/v1/match/event/{id}/leave`: Endpoint to allow users to opt-out of an event before it starts processing.
- Modify `GET /api/v1/match/history`: Allow filtering by `EventId`.

#### [NEW] [EventController.cs](file:///c:/Users/nefise/Desktop/BridgeApi/backend/MatchingApi/Controllers/EventController.cs)

- CRUD endpoints to create, fetch, and list active matching events for admins and users.

---

### Service Layer

#### [MODIFY] [AiMatchingService.cs](file:///c:/Users/nefise/Desktop/BridgeApi/backend/MatchingApi/Services/AiMatchingService.cs)

- Refactor the core match logic so it honors the provided pool of candidates (event participants) instead of querying the whole database.
- Utilize the `TopMatchingCount` specified by the event instead of a predefined static limit.
