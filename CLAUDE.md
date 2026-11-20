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
The shared engine now exists: package `com.jungledegree.studiokit` 0.1.0 (repo `studiokit`, tag
`v0.1.0`), extracted from the Wild Pack by the studiokit K plan (RT-1 done). This project consumes it
via `file:../../studiokit/com.jungledegree.studiokit` with the repos side by side in
`C:\projects\games\` (setup: `../studiokit/docs/consumo-runner-vs-tower.md`). What is and is not in
the package: `docs/reuso-tecnico.md` section 5.3.
- Reuse goes through the package only. Never copy C# or assets from the Wild Pack repo
  (MultiplierRunner). If something that stayed in the Wild Pack is needed here (gates, course,
  screens, tools), move it into the kit first, as a studiokit delivery, keeping the `.meta` files.
- The kit never names a game or theme; game content and this game's isolation term list
  (`StudioKitIsolationTerms`) live here.
- Changing the package also changes the Wild Pack: one session writing per repository, and the
  studiokit acceptance (Wild Pack validators and save test) applies.
