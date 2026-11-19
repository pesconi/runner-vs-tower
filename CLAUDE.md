# Runner vs Tower — project rules

Studio: Jungle Degree. Working title; repo name is provisional. Unity 6000.6.7f1, URP, Android, portrait.
Game: crowd runner with math gates, the army then attacks an enemy tower. Docs in `docs/`
(`gdd.md`, `plano-de-entregas.md`, `reuso-tecnico.md`, `riscos-e-decisoes.md`).

## Rules (inherited from Wild Pack)
- No secrets in the repo (keystores, signing.properties, service accounts, API keys, AdMob/Play IDs of real accounts).
- Sessions never upload anything to Play Console or AdMob.
- Performance is approved only by the 3x headroom gate on the OPPO CPH2791.
- Work directly on `main`; no branches/worktrees unless the user asks. Never force-push.

## Reuse caveat
Reusable Wild Pack code has NOT been extracted yet. It will become the `studiokit` package
(delivery RT-1). Do not copy C# or assets from the Wild Pack repo (MultiplierRunner) before that.
