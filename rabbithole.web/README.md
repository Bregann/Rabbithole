# rabbithole.web

Vite + React + Mantine SPA. In production it's built into `Rabbithole.Core/wwwroot` and served by the API (one image).

```bash
npm ci
npm run dev      # proxies /api to the API (Aspire injects the address; falls back to http://localhost:5180)
npm run lint
npx tsc -b
npm run build
```

Usually you don't run this on its own: `aspire run` (or run `Rabbithole.AppHost`) starts Postgres, the API and this dev server together.
See the root `CLAUDE.md` for structure and conventions.
