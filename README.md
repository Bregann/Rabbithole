# Rabbithole

Self-hosted music discovery built from what you actually listen to.

Rabbithole reads each person's listening history from **ListenBrainz** (scrobbled from **Navidrome**), ranks suggestions with **AudioMuse**, builds a private weekly playlist for each user in Navidrome, and lets you explore new artists visually and request albums through **Lidarr**.

## What it does

- **Discover:** artists you might like, built from your ListenBrainz listening (or Navidrome's most played), each labelled *In your library*, *In Lidarr* or *New to you*.
- **Rabbithole mode:** a visual graph of similar artists. Click one to make it the centre and keep going; a trail shows how you got there.
- **Artist pages:** top tracks with 30-second previews, similar artists, and every album with a one-click Lidarr request.
- **Your Navidrome playlists:** sign in with your Navidrome account and your playlists are there. Play songs, add from your library, remove, create new ones.
- **Playlist suggestions:** songs that fit a playlist, from your library (ranked by AudioMuse if you run it) and new ones you can preview and request. Requested songs are added to the playlist automatically once Lidarr has them.
- **Weekly picks:** your ListenBrainz Weekly Exploration with previews, plus a private *Rabbithole Weekly* playlist in Navidrome rebuilt every Tuesday from the picks you already own.
- **Not for me:** hide an artist and they never show up in suggestions again.

## Running it

Rabbithole ships as **one Docker image** (API + web UI) and needs a Postgres database.

- **Unraid:** use `unraid/rabbithole.xml`.
- **Docker Compose:** see `docker-compose.example.yml`.

Sign in with your Navidrome account; the first person to sign in becomes the admin.

Every setting can be passed as an environment variable (they're stored in the database and env vars win on startup):

| Variable | Required | What it's for |
|---|---|---|
| `ConnectionStrings__rabbithole` | yes | Postgres connection string |
| `JwtKey` | yes | Random string, 32+ characters |
| `NavidromeUrl` | yes | Sign-in, library matching, playlists |
| `LidarrUrl`, `LidarrApiKey` | for requests | Sending albums to Lidarr |
| `AudioMuseUrl` | optional | Ranking suggestions by sound |
| `AudioMuseApiToken` | optional | AudioMuse `API_TOKEN`, if it has auth turned on |
| `ListenBrainzApiUrl` | optional | Defaults to `https://api.listenbrainz.org` |
| `WeeklyRequestCap` | optional | Albums per user per week (default 5) |
| `HangfireUsername`, `HangfirePassword` | optional | Job dashboard at `/hangfire` |
| `SeqUrl` | optional | Send logs to Seq |

## Developing

Requires the .NET 11 SDK, Node 26, Docker and the [Aspire CLI](https://aspire.dev).

The first EF Core migration hasn't been created yet. Before the first run:

```bash
dotnet ef migrations add InitialCreate --project Rabbithole.Domain --output-dir Database/Migrations
```

Then:

```bash
aspire run          # Postgres + API + Vite dev server, with the Aspire dashboard
dotnet test         # integration tests against a throwaway Postgres container
```

Project structure and coding conventions are in [`CLAUDE.md`](CLAUDE.md). Read it before adding anything.
