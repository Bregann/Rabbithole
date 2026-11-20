# Rabbithole — Project Conventions

Rabbithole is a self-hosted music discovery app. It reads each user's listening data from
ListenBrainz (scrobbled from Navidrome), ranks suggestions with AudioMuse, builds private
per-user playlists in Navidrome, and lets users explore and request missing albums via Lidarr.

These rules follow the same design ethos as ShutterStream, Orbit and RetroTrack. **Follow them strictly.**
When in doubt, copy how ShutterStream does it (the newest reference: `Result` instead of exceptions).
Orbit's `GlobalExceptionHandler` and RetroTrack's try/catch controllers are the old style — do not copy those.

Core principles:
- **Small components.** One responsibility per class/component/file. If a file is doing two jobs, split it.
- **Everything through DI.** Every service and helper has an interface and is injected. No `new`-ing services, no statics with state.
- **Split properly.** Controllers, services, DTOs, models, interfaces, enums and exceptions each live in their own folder, one type per file.
- **Tests for every service.** No service ships without an integration test fixture.

---

## Solution layout

```
Rabbithole.sln
├── Rabbithole.AppHost/          # Aspire orchestration (local dev + publish)
├── Rabbithole.Core/             # ASP.NET Core API host — thin
│   ├── Controllers/
│   ├── Extensions/ResultExtensions.cs
│   └── Program.cs
├── Rabbithole.Domain/           # All business logic
│   ├── Database/
│   │   ├── Context/AppDbContext.cs
│   │   ├── Models/
│   │   └── Migrations/
│   ├── DTOs/<Feature>/Requests/ # <Action>Request.cs — what the API accepts
│   ├── DTOs/<Feature>/Responses/# <Action>Dto.cs — what the API returns (with its …Item types in the same file)
│   ├── DTOs/<Feature>/          # Internal service types that never cross the API (e.g. LibrarySnapshot)
│   ├── DTOs/Shared/             # …Item types used by several features
│   ├── DTOs/ThirdParty/<Service>/ # Upstream shapes (Navidrome, Deezer, ListenBrainz, Lidarr, AudioMuse)
│   ├── Enums/
│   ├── Exceptions/
│   ├── Extensions/
│   ├── Helpers/                 # Our own helpers only: settings, user context, credentials, JWT, library state, seeding, Hangfire
│   ├── ThirdParty/<Service>/    # <Service>ApiClient.cs — one folder per external API
│   ├── Interfaces/Api/<Feature>/
│   ├── Interfaces/Helpers/
│   ├── Interfaces/ThirdParty/   # I<Service>ApiClient.cs
│   ├── Results/Result.cs        # Result / Result<T> (copied from ShutterStream)
│   └── Services/<Feature>/
├── Rabbithole.Tests/
│   ├── Infrastructure/          # TestContainerSetup, DatabaseIntegrationTestBase, TestDatabaseSeedHelper, MockFactory
│   ├── Services/<Feature>/      # <Service>IntegrationTests.cs
│   └── Extensions/              # Plain unit tests
├── rabbithole.web/              # Vite + React + Mantine SPA (built into Core's wwwroot), in the .sln as rabbithole.web.esproj
├── Dockerfile                   # Single production image: web build → API publish → one container
├── docker-compose.example.yml
├── unraid/rabbithole.xml        # Unraid template
├── .editorconfig
├── renovate.json
└── .github/workflows/           # ci.yml, PushToDocker.yml
```

Features (use these names for folders across Domain, Tests and web; never name a feature `System`, it shadows the .NET namespace):
`Auth`, `Artists`, `Discover` (feed + weekly picks), `Library`, `Playlists`, `Player`, `Requests` (Lidarr),
`Feedback` (blocklist), `Settings`, `Status` (setup health), `Sync` (weekly playlist job).
DTOs used by several features (`ArtistSummaryItem`, `TrackItem`, `LibrarySongItem`, `SuggestedTrackItem`) live in `DTOs/Shared`.

---

## Aspire and the single image

**Rabbithole ships as ONE Docker image.** ASP.NET Core serves both the API (`/api/*`) and the built React SPA (from `wwwroot`).
This is why the frontend is a static Vite SPA, not Next.js: a Next.js server would need its own Node runtime and a second container.

- `Rabbithole.AppHost` wires everything for local development:

```csharp
var postgres = builder.AddPostgres("postgres").WithDataVolume();
var db = postgres.AddDatabase("rabbithole");

var api = builder.AddProject<Projects.Rabbithole_Core>("api")
    .WithReference(db)
    .WaitFor(db);

var web = builder.AddViteApp("web", "../rabbithole.web")
    .WithReference(api)
    .WaitFor(api);

// At publish time, copy the built SPA into the API container's wwwroot → one image
api.PublishWithContainerFiles(web, "./wwwroot");
```

- In dev (`aspire run`), the Vite dev server runs separately with hot reload and proxies `/api` to the API (see Frontend).
- In production, `Program.cs` serves the SPA. `PublishWithContainerFiles` only copies the files; the API must serve them:

```csharp
app.UseDefaultFiles();
app.UseStaticFiles();
// ... auth, controllers, hangfire ...
app.MapControllers();
app.MapFallbackToFile("index.html"); // client-side routes
```

- Aspire **replaces** the `#if DEBUG` Testcontainers block used in Orbit's `Program.cs`. `Program.cs` gets its connection string from configuration
  (`builder.Configuration.GetConnectionString("rabbithole")`), which Aspire injects in dev via `WithReference(db)` and the `ConnectionStrings__rabbithole` env var provides in production.
  No `#if DEBUG` connection-string switching.
- There is no ServiceDefaults project: Rabbithole is one service, so it isn't worth a shared project.
  Core maps `/health` itself. Logs go to Serilog/Seq as in the other projects.
- Because API and SPA share an origin in production, **no CORS policy is needed in production**. Cookies are same-site.
- Tests still use Testcontainers directly (see Tests), not Aspire.
- Postgres is external to the image (users point `ConnectionStrings__rabbithole` at their existing Postgres container).

---

## Backend (C#)

### Style
- .NET latest, `Nullable` and `ImplicitUsings` enabled, `LangVersion latest`.
- Follow `.editorconfig` (copied from Orbit): 4-space indent, LF, final newline.
- **Block-scoped namespaces** (`namespace X { ... }`), matching the reference repos. Namespace must match the folder.
- **Primary constructors** for DI: `public class FooService(AppDbContext context, IBarHelper barHelper) : IFooService`.
- One public type per file. File name = type name.
- CI runs `dotnet format --verify-no-changes`. Format before committing.

### Controllers (`Rabbithole.Core/Controllers`)
- Thin. One service call per action. **No business logic, no try/catch, no DbContext.**
- Return `ActionResult<T>` / `ActionResult`. A service that can fail returns a `Result`, and the action returns
  `result.ToActionResult()`. A service that can't fail returns the DTO, and the action returns `Ok(dto)`.
- `[Route("api/[controller]/[action]")]`, `[ApiController]`, `[Authorize]` on the class (opt out with `[AllowAnonymous]` per action).
- Inject `I<Feature>Service` via primary constructor.
- GET returns the response DTO directly. Mutations return `IActionResult` (`Ok()` / `Ok(id)`).
- Query params via `[FromQuery]`, bodies via `[FromBody] <Name>Request`.

```csharp
namespace Rabbithole.Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class FeedbackController(IFeedbackService feedbackService) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<int>> BlockArtist([FromBody] BlockArtistRequest request)
        {
            var result = await feedbackService.BlockArtist(request);
            return result.ToActionResult();
        }

        [HttpDelete]
        public async Task<ActionResult> UnblockArtist([FromQuery] int blockedArtistId)
        {
            var result = await feedbackService.UnblockArtist(blockedArtistId);
            return result.ToActionResult();
        }
    }
}
```

### Errors: `Result`, not exceptions
- Expected, user-facing failures are part of the method signature: services return `Result` or `Result<T>`
  (`Domain/Results/Result.cs`, same as ShutterStream).
  - `Result.Fail(...)` → 400, `Result.NotFoundFail(...)` → 404, `Result.ConflictFail(...)` → 409, `Result.UnauthorizedFail(...)` → 401.
  - Generic forms: `Result.Ok(value)`, `Result.Fail<T>(...)`, etc. Check `Success` before reading `Value`.
  - `Error` must always be safe to show the user.
- `Core/Extensions/ResultExtensions.cs` maps a failed result to ProblemDetails with the right status code
  (`ToActionResult()` / `ToErrorResponse()`). To add a new failure kind, add it to `ResultErrorType` and map it there.
- **There is no `GlobalExceptionHandler`.** Genuinely exceptional situations still throw and become a generic 500
  via `AddProblemDetails()` + `UseExceptionHandler()`: an upstream that's unreachable (`ExternalServiceException`),
  missing config, broken invariants, database failures.
- API clients return `null` (or `false`) for "not found" upstream and let the service decide which `Result` that is.
  Clients never return `Result`.

### Services (`Domain/Services/<Feature>`)
- Every service implements an interface in `Domain/Interfaces/Api/<Feature>/I<Name>Service.cs`.
- Registered **scoped** in `Program.cs`, grouped and commented by feature.
- Get the current user through `IUserContextHelper` (claims), never by passing HttpContext around.
- Keep services focused. If a service needs more than ~5 dependencies or grows past a few hundred lines, split it
  (e.g. `DiscoverFeedService`, `DiscoverRankingService`, `DiscoverMatchingService`).
- Return DTOs, never EF models.

### Third-party APIs (`Domain/ThirdParty/<Service>` + `Interfaces/ThirdParty` + `DTOs/ThirdParty/<Service>`)
Every external system sits behind its own interface so it can be mocked:

| Interface | Talks to | Registered with |
|---|---|---|
| `INavidromeApiClient` | Navidrome Subsonic API (auth check, search, playlists) | `AddHttpClient` |
| `IListenBrainzApiClient` | ListenBrainz recommendations / playlists | `AddHttpClient` |
| `ILidarrApiClient` | Lidarr artist/album lookup + add | `AddHttpClient` |
| `IAudioMuseApiClient` | AudioMuse similarity | `AddHttpClient` |
| `IDeezerApiClient` | Artist search, related artists, top tracks, albums, 30-second previews (no key) | `AddHttpClient` |

- Clients only translate HTTP ↔ `DTOs/ThirdParty/<Service>` types. No business rules in clients.
- Use `HttpClient` typed clients with explicit timeouts.
- Upstream failures (unreachable, unexpected response) throw `ExternalServiceException`. "Not found" returns `null`.

### Helpers
- `IUserContextHelper` — current user id / username from claims.
- `IEnvironmentalSettingHelper` — singleton, settings stored in the `EnvironmentalSettings` table, keyed by `EnvironmentalSettingEnum`
  (service URLs, API keys, schedule, weekly request cap). Loaded at startup.
- `ICredentialProtector` — wraps ASP.NET Data Protection to encrypt stored Navidrome credentials. Never store them in plain text.
  Keys persist in Postgres (`DataProtectionKeys`), so encrypted passwords survive container rebuilds.
- `ICurrentUserHelper` — the signed-in user's id, decrypted Navidrome credentials and `LibrarySnapshot`, in one scoped helper.
  Use it instead of injecting `IUserContextHelper` + `INavidromeCredentialHelper` + `ILibraryStateHelper` separately.
- `ILibraryStateHelper` / `LibrarySnapshot` — labels any artist name as `InLibrary`, `InLidarr` or `New`, and knows the user's
  blocked artists. Build one snapshot per request; never call Navidrome/Lidarr per artist.
- Names are compared across services with `NormaliseArtistName()` / `NormaliseTitle()` (`Extensions/StringNormalisationExtensions`). Never compare raw names.

### Database (`Domain/Database`)
- EF Core + Npgsql + **lazy-loading proxies**. Navigation properties are `virtual`.
- Models use data annotations like the reference repos: `[Key, DatabaseGenerated(...)]`, `[Required]`, `required` properties,
  `[ForeignKey("...")]`, `[DeleteBehavior(...)]`.
- Queries project straight into DTOs with `.Select(...)` and `ToArrayAsync()`.
- Updates and deletes use `ExecuteUpdateAsync` / `ExecuteDeleteAsync`; check the affected row count and return `Result.NotFoundFail(...)` when it's 0.
- Store all times as UTC (`DateTime.SpecifyKind(..., DateTimeKind.Utc)`).
- Migrations live in `Database/Migrations`. Never edit an applied migration.

### DTOs (`Domain/DTOs/<Feature>`) — same as ShutterStream
- What the API accepts: `DTOs/<Feature>/Requests/<Action>Request.cs`, namespace `…DTOs.<Feature>.Requests`.
- What the API returns: `DTOs/<Feature>/Responses/<Action>Dto.cs` (e.g. `GetArtistDto`, `CreateRequestDto`), namespace `…DTOs.<Feature>.Responses`.
- `…Item` types that belong to one response live **in that response's file** (e.g. `AlbumItem` inside `GetArtistDto.cs`).
  `…Item` types used across features live in `DTOs/Shared/`, one per file.
- Internal types that never cross the API (`LoginResult`, `LibrarySnapshot`, `SeedArtistsResult`) sit directly in `DTOs/<Feature>/`.
- Third-party shapes live in `DTOs/ThirdParty/<Service>/` and may group one service's types in a file. Their properties are
  nullable/defaulted, never `required`, because upstream data can't be trusted.
- Use `required` properties; arrays default to `[]`. Return arrays, not `List<T>`.
- The `Requests` feature ends up with `DTOs/Requests/Requests/`; that's expected, keep the rule consistent.

### Scheduled jobs
- Hangfire with Postgres storage; dashboard at `/hangfire` behind basic auth (credentials from `EnvironmentalSettingHelper`).
- All recurring jobs are registered in `Helpers/HangfireJobSetup.RegisterJobs()` and point at **interfaces**:
  `RecurringJob.AddOrUpdate<IWeeklyDiscoverySyncService>("WeeklyDiscoverySync", s => s.RunForAllUsers(), "0 3 * * 1");`
- Job services fan out per user and must be idempotent (re-running a week must not duplicate playlists or requests).

### Auth
- Users sign in with their **Navidrome** username/password. `AuthService` validates via `INavidromeApiClient` (Subsonic `ping`),
  then creates/updates the local `User` row. There is no separate registration.
- JWT access token in a cookie + HttpOnly refresh token cookie, refreshed on 401 — same as Orbit.

### Logging
- Serilog: async file sink at `/app/Logs/log.log` (7-day retention), console, and Seq.
- Log with structured templates: `Log.Information("[Sync] Built playlist for {Username} with {Count} tracks", username, count);`
- Prefix with `[Area]` like the reference repos.

---

## Tests (`Rabbithole.Tests`)

- **NUnit + Moq + Testcontainers (PostgreSQL).** `Using Include="NUnit.Framework"` in the csproj.
- `TestContainerSetup` (`[SetUpFixture]`) starts one Postgres container for the whole run.
- `DatabaseIntegrationTestBase` gives every test a **fresh migrated database** (`EnsureDeletedAsync` + `MigrateAsync`) and exposes
  `DbContext`, `CustomSetUp()` and `CustomTearDown()`.
- `TestDatabaseSeedHelper` has small, focused seed methods (`SeedTestUser`, `SeedTestBlockedArtists`, ...). Seed the minimum.
- `MockFactory` creates mocks for external dependencies only (`CreateNavidromeApiClient()`, `CreateListenBrainzApiClient()`,
  `CreateLidarrApiClient()`, `CreateAudioMuseApiClient()`, `CreateUserContextHelper()`, ...).
- **Never mock the database or `DbSet`s.**
- One `<Service>IntegrationTests` fixture per service, under `Services/<Feature>/`.
- Test names: `Method_Scenario_ExpectedResult` (e.g. `BlockArtist_WhenAlreadyBlocked_ShouldReturnConflict`).
- Arrange / Act / Assert comments; NUnit constraint syntax (`Assert.That(x, Is.EqualTo(y))`).
- Every service method gets a happy-path test **and** its failure cases: assert `result.Success`, `result.ErrorType`
  and `result.Error` for each `Result` failure, and `Assert.ThrowsAsync` only for genuinely exceptional paths.
- Pure logic (ranking maths, matching, extensions) gets plain unit tests without the database.

```csharp
namespace Rabbithole.Tests.Services.Feedback
{
    [TestFixture]
    public class FeedbackServiceIntegrationTests : DatabaseIntegrationTestBase
    {
        private FeedbackService _feedbackService = null!;

        protected override async Task CustomSetUp()
        {
            await TestDatabaseSeedHelper.SeedTestUser(DbContext);
            _feedbackService = new FeedbackService(DbContext, MockFactory.CreateUserContextHelper().Object, new MemoryCache(new MemoryCacheOptions()));
        }

        [Test]
        public async Task BlockArtist_WithNewArtist_ShouldPersistToDatabase()
        {
            // Arrange
            var request = new BlockArtistRequest { ArtistMbid = "a74b1b7f-71a5-4011-9441-d0b5e4122711" };

            // Act
            var result = await _feedbackService.BlockArtist(request);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(await DbContext.BlockedArtists.CountAsync(), Is.EqualTo(1));
        }
    }
}
```

---

## Frontend (`rabbithole.web`)

Vite + React + TypeScript SPA with React Router, Mantine, `@tabler/icons-react`, TanStack Query and dayjs.
The structure and habits follow `orbit.web`, adapted for a SPA (no server components or SSR prefetch).

```
rabbithole.web/
├── index.html
├── vite.config.ts                 # dev proxy: /api → API URL injected by Aspire
├── src/
│   ├── main.tsx                   # mounts <Providers><App/></Providers>
│   ├── App.tsx                    # route table only
│   ├── providers.tsx              # MantineProvider, QueryClientProvider, Notifications, AuthProvider
│   ├── theme.ts                   # Mantine theme (dark, violet)
│   ├── components/pages/<X>Component.tsx   # one per route: orchestrates data + layout
│   ├── components/<feature>/      # small feature components
│   ├── components/common/         # shared bits (DeleteConfirmationModal, EmptyState, ...)
│   ├── components/navigation/     # Navigation, ClientLayout, ProtectedRoute
│   ├── context/authContext.tsx
│   ├── helpers/apiClient.ts
│   ├── helpers/QueryKeys.ts
│   ├── helpers/mutations/useMutationPost|Put|Patch|Delete.ts
│   ├── helpers/notificationHelper.ts
│   ├── helpers/<feature>Helper.ts # pure formatting/label functions
│   ├── interfaces/api/<feature>/  # mirrors the C# DTOs: <Action>Request.ts / <Action>Dto.ts (+ its …Item interfaces)
│   └── css/<name>.module.css
```

### Rules
- **`App.tsx` is only the route table.** Each route points to a `components/pages/<X>Component`. Protected routes are wrapped in `ProtectedRoute`.
- **Page components orchestrate; feature components render.** Queries and mutations live in the page component
  (or a dedicated `use<Thing>` hook in `hooks/`). Feature components get typed props and callbacks and don't fetch.
- **Small components.** A card, a modal, a toolbar, a filter bar = separate files. Modals are their own components
  (`RequestAlbumModal`, `BlockArtistModal`).
- Props interface named `<Component>Props`, declared above the component; default export the component.
- Unused callback params are prefixed with `_` (`onRequest: (_albumId: string) => void`).
- All API calls go through `apiClient` (`doQueryGet`, `doPost`, ...) using **relative URLs** (`/api/discover/GetFeed`).
  Same origin in production; the Vite proxy handles dev. Keep Orbit's 401 → `RefreshToken` → retry-once behaviour.
- Queries use the `QueryKeys` enum. Mutations use the `useMutation*` hooks with `invalidateQuery` and `notificationHelper`
  success/error notifications.
- Types in `interfaces/api/<feature>/` match the C# DTOs exactly (camelCase): one file per C# file, same name,
  with the `…Item` interfaces in the same file as their `…Dto` (like ShutterStream's web app).
- Pure helpers (labels, colours, formatting) live in `helpers/`, not inside components.
- ESLint rules copied from orbit.web: single quotes, no semicolons, no trailing spaces. CI runs `npm run lint`, `npx tsc --noEmit`, `npm run build`.

### Shared state and hooks
- `context/authContext.tsx` — current user (from `GetCurrentUser`), `login`, `logout`. Auth cookies are HttpOnly; the SPA never reads tokens.
- `context/playerContext.tsx` — the single app-wide `<audio>`: `playPreview(track)` (fresh Deezer URL every time) and `playSong(song)` (proxied Navidrome stream).
- `hooks/` — cross-page actions: `useRequestActions` (album/track requests), `useBlockArtist`, `useOwnPlaylists`.
- Modals that search (e.g. `AddSongsModal`) may own their query, like Orbit's modals; everything else gets data via props.

---

## Docker & CI

- **One image: `rabbithole`.** The root `Dockerfile` is multi-stage:
  1. `node` stage: `npm ci && npm run build` in `rabbithole.web` → `dist/`
  2. `dotnet/sdk` stage: restore, then `dotnet publish Rabbithole.Core`
  3. `dotnet/aspnet` final stage: copy the publish output, copy `dist/` into `wwwroot/`, run as `$APP_UID`, expose 8080, volume `/app/Logs`.
- The root Dockerfile and the AppHost's `PublishWithContainerFiles` must produce the same shape (SPA in `wwwroot`). If one changes, change the other.
- Ship one Unraid template (port 8080, `ConnectionStrings__rabbithole`, `JwtKey`, logs path) and a `docker-compose.yml` example with Postgres.
- `ci.yml` runs on PRs and pushes to main:
  - **api:** `dotnet restore` → `dotnet format --verify-no-changes` → `dotnet build -c Release` → `dotnet test` (Testcontainers uses the runner's Docker).
    The solution includes `rabbithole.web.esproj`, but building it doesn't run npm or build the SPA (`ShouldRunNpmInstall`/`ShouldRunBuildScript` are false).
  - **web:** `npm ci` → `npm run lint` → `npx tsc --noEmit` → `npm run build`.
  - **image:** `docker build .`, to prove the single image still builds.
- `PushToDocker.yml` builds and pushes the single image on main.
- Renovate keeps dependencies current; it only auto-merges once CI passes.

---

## Definition of done (every change)

1. Code sits in the right project and folder, one type per file.
2. New services and clients have an interface and are registered in `Program.cs`.
3. New service methods have integration tests covering success and failure paths.
4. New web data has a DTO interface, a `QueryKeys` entry and uses the shared fetch and mutation helpers.
5. Expected failures are `Result`s, not exceptions.
6. `dotnet format`, `dotnet test`, `npm run lint` and `npx tsc -b` all pass.
