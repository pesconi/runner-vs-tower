# Store listing — Stampeep — English (en-US)

Copy each block into Play Console > Grow users > Store presence > Main store listing. Character counts were checked by
`Tools/store/check_listing.py` (limits: title 30, short description 80, full description 4000).

## App name (title)

```text
Stampeep: Storm the Tower
```

## Short description

```text
Grow your stampede through number gates, dodge the traps and storm the tower.
```

## Full description

```text
Lead a stampede of up to 300 runners and bring the enemy tower down.

PICK THE RIGHT GATE
Every gate changes your army: add, multiply, or lose runners. Read the numbers fast, drag left or right and choose the path that makes your stampede bigger.

DODGE THE TRAPS
Swinging spiked logs, sliding saws, slamming hammers, open pits and rolling spikes cut through the crowd. Keep your runners out of the way or watch the count drop.

STORM THE TOWER
At the end of the run your army throws itself at the tower. Cannons, shields, sweeping arms and walls fight back. Drag to aim, hold to launch and break through before you run out of runners.

FOUR WORLDS, FOUR BOSSES
40 levels across four worlds, each with its own colors, defenses and a boss tower waiting at level 10.

BUILD YOUR STAMPEDE
Spend the coins you win on upgrades, elite units (Brute, Sprinter, Warden), colors and five crowd shapes, each with its own way of hopping.

PLAY YOUR WAY
- One-finger controls, short levels.
- Daily gift for coming back.
- No account and no login: your progress stays on your phone.
- Optional rewarded ads: watch one only if you want double coins or a second chance.

Stampeep contains ads. Privacy policy and data choices are in the game's Settings screen.
```

## Notes for the reviewer (not for the store)

- Facts behind each claim: 300 = `ArmyLimit.Units`; 40 levels = D12 (4 worlds × 10); obstacle kinds =
  `ObstacleKind` (SwingingLog, Saw, Hammer, Hole, SpikedRoller); defenses = `CannonDefense`, `ShieldDefense`,
  `SweeperDefense`, `AssaultWall`; elites and shapes = `WorldContent` (BRUTE, SPRINTER, WARDEN; BALL, CHICK, BUNNY,
  FROGGY, PENGUIN); daily gift = `DailyReward`; rewarded ad offers = `ClaybreakFlow` ("x2 COINS", continue once);
  save only on the phone = `SaveStore` (PlayerPrefs).
- Adult direction (D25 A, R8): action verbs, no "kids", "children", "cute", "family", "educational", "baby", "toddler".
  No "best", "#1", "top", "free", "new", prices, rankings, emojis or calls to action like "download now" (Play metadata
  policy). No other game, brand or person named.
- Not promised: online play, events, leaderboards, offline play (ads need the network), real-money purchases.
