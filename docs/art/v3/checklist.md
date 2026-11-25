# Checklist de estilo (gerado)

Gerado de `StyleGuide` por `Claybreak.Editor.StyleChecklist`. Não editar à mão: mude o guia em `StyleGuideContent` e rode `Tools/rt4-check.sh` (passo `scene`) ou `-executeMethod Claybreak.Editor.StyleChecklist.WriteAndQuit`.
Cada linha é uma regra que a captura deve cumprir; classe `legado` = ainda no visual anterior (o guia descreve o que o jogo tem hoje).

## Classes

- [ ] **Unit** (legado): teto atual 600 triângulos e 1 chamadas por objeto; meta v3 800–1200 triângulos, até 1 chamadas, nenhuma aresta viva acima de 60° (chanfro).
- [ ] **Leader** (legado): teto atual 400 triângulos e 1 chamadas por objeto; meta v3 3000–5000 triângulos, até 1 chamadas, nenhuma aresta viva acima de 60° (chanfro).
- [ ] **Enemy** (legado): teto atual 2200 triângulos e 16 chamadas por objeto; meta v3 1500–3000 triângulos, até 1 chamadas, nenhuma aresta viva acima de 60° (chanfro).
- [ ] **Tower** (legado): teto atual 6000 triângulos e 70 chamadas por objeto; meta v3 6000–12000 triângulos, até 4 chamadas, nenhuma aresta viva acima de 60° (chanfro).
- [ ] **Boss** (legado): teto atual 13000 triângulos e 75 chamadas por objeto; meta v3 8000–15000 triângulos, até 4 chamadas, nenhuma aresta viva acima de 60° (chanfro).
- [ ] **Gate** (legado): teto atual 400 triângulos e 10 chamadas por objeto; meta v3 100–2000 triângulos, até 2 chamadas, nenhuma aresta viva acima de 60° (chanfro).
- [ ] **Obstacle** (legado): teto atual 700 triângulos e 1 chamadas por objeto; meta v3 100–2500 triângulos, até 1 chamadas, nenhuma aresta viva acima de 60° (chanfro).
- [ ] **Floor** (legado): teto atual 200 triângulos e 2 chamadas por objeto; meta v3 100–4000 triângulos, até 1 chamadas, nenhuma aresta viva acima de 60° (chanfro).
- [ ] **Scenery** (legado): teto atual 300 triângulos e 1 chamadas por objeto; meta v3 300–1500 triângulos, até 1 chamadas, nenhuma aresta viva acima de 60° (chanfro).
- [ ] **Effect** (legado): sem malha própria.
- [ ] **Sound** (legado): sem malha própria.
- [ ] **Screen** (legado): sem malha própria.

## Paletas (antes do pós; tolerância ΔE CIE76 ≤ 3 sobre os três níveis: base, sombra, destaque)

### Pastel

| Papel | Classe | Base | Sombra | Destaque |
|---|---|---|---|---|
| skyTop | Scenery | #ffd9cf | #ffd9cf | #ffd9cf |
| skyBottom | Scenery | #cfe8ff | #cfe8ff | #cfe8ff |
| fog | Scenery | #f3e3ea | #f3e3ea | #f3e3ea |
| ground | Floor | #bfe6cf | #bfe6cf | #bfe6cf |
| track | Floor | #fff4ea | #fff4ea | #fff4ea |
| trackStripe | Floor | #ffe3d1 | #ffe3d1 | #ffe3d1 |
| crowd | Unit | #ff8fab | #ff8fab | #ff8fab |
| foliageDark | Scenery | #7cc6a4 | #7cc6a4 | #7cc6a4 |
| foliageLight | Scenery | #a8dcbf | #a8dcbf | #a8dcbf |
| trunk | Scenery | #c99076 | #c99076 | #c99076 |
| rock | Scenery | #e7d3f0 | #e7d3f0 | #e7d3f0 |
| uiAccent | Screen | #ff6f91 | #ff6f91 | #ff6f91 |
| uiInk | Screen | #3b2f4a | #3b2f4a | #3b2f4a |
| structureLight | Tower | #f1d2e1 | #f1d2e1 | #f1d2e1 |
| structureDark | Tower | #ab86b8 | #ab86b8 | #ab86b8 |
| structureDetail | Tower | #ff6f91 | #ff6f91 | #ff6f91 |
| defense | Enemy | #7d6b91 | #7d6b91 | #7d6b91 |

- [ ] Luz: sol (50, -30, 0) cor #ffffff intensidade 1; ambiente céu #ffffff chão #bbaacc (ambiente 0.55, direta 0.75); pós: none.

### PorDoSol

| Papel | Classe | Base | Sombra | Destaque |
|---|---|---|---|---|
| skyTop | Scenery | #3a3270 | #3a3270 | #3a3270 |
| skyBottom | Scenery | #f7a26b | #f7a26b | #f7a26b |
| fog | Scenery | #e9967a | #e9967a | #e9967a |
| ground | Floor | #7a5482 | #7a5482 | #7a5482 |
| track | Floor | #f6caa6 | #f6caa6 | #f6caa6 |
| trackStripe | Floor | #eeb790 | #eeb790 | #eeb790 |
| crowd | Unit | #fff1d6 | #fff1d6 | #fff1d6 |
| foliageDark | Scenery | #463a78 | #463a78 | #463a78 |
| foliageLight | Scenery | #5b4a8c | #5b4a8c | #5b4a8c |
| trunk | Scenery | #2f2650 | #2f2650 | #2f2650 |
| rock | Scenery | #a36b8f | #a36b8f | #a36b8f |
| uiAccent | Screen | #f26a5a | #f26a5a | #f26a5a |
| uiInk | Screen | #2d2448 | #2d2448 | #2d2448 |
| structureLight | Tower | #d9a6b0 | #d9a6b0 | #d9a6b0 |
| structureDark | Tower | #5b4170 | #5b4170 | #5b4170 |
| structureDetail | Tower | #f26a5a | #f26a5a | #f26a5a |
| defense | Enemy | #3a2d5c | #3a2d5c | #3a2d5c |

- [ ] Luz: sol (50, -30, 0) cor #ffffff intensidade 1; ambiente céu #ffffff chão #bbaacc (ambiente 0.55, direta 0.75); pós: none.

### Doce

| Papel | Classe | Base | Sombra | Destaque |
|---|---|---|---|---|
| skyTop | Scenery | #8fd3ff | #8fd3ff | #8fd3ff |
| skyBottom | Scenery | #eef9ff | #eef9ff | #eef9ff |
| fog | Scenery | #dff2ff | #dff2ff | #dff2ff |
| ground | Floor | #93d99a | #93d99a | #93d99a |
| track | Floor | #fffae6 | #fffae6 | #fffae6 |
| trackStripe | Floor | #ffedb0 | #ffedb0 | #ffedb0 |
| crowd | Unit | #ffcf3a | #ffcf3a | #ffcf3a |
| foliageDark | Scenery | #45b07c | #45b07c | #45b07c |
| foliageLight | Scenery | #6fcf8f | #6fcf8f | #6fcf8f |
| trunk | Scenery | #b07a55 | #b07a55 | #b07a55 |
| rock | Scenery | #f4b4c8 | #f4b4c8 | #f4b4c8 |
| uiAccent | Screen | #2fa7e6 | #2fa7e6 | #2fa7e6 |
| uiInk | Screen | #24435a | #24435a | #24435a |
| structureLight | Tower | #fff0f5 | #fff0f5 | #fff0f5 |
| structureDark | Tower | #e58fae | #e58fae | #e58fae |
| structureDetail | Tower | #2fa7e6 | #2fa7e6 | #2fa7e6 |
| defense | Enemy | #5a6fb0 | #5a6fb0 | #5a6fb0 |

- [ ] Luz: sol (50, -30, 0) cor #ffffff intensidade 1; ambiente céu #ffffff chão #bbaacc (ambiente 0.55, direta 0.75); pós: none.

### Aurora

| Papel | Classe | Base | Sombra | Destaque |
|---|---|---|---|---|
| skyTop | Scenery | #2c3e66 | #2c3e66 | #2c3e66 |
| skyBottom | Scenery | #9ad0c2 | #9ad0c2 | #9ad0c2 |
| fog | Scenery | #a9cfc4 | #a9cfc4 | #a9cfc4 |
| ground | Floor | #5e8f88 | #5e8f88 | #5e8f88 |
| track | Floor | #f2efe4 | #f2efe4 | #f2efe4 |
| trackStripe | Floor | #e3dcc8 | #e3dcc8 | #e3dcc8 |
| crowd | Unit | #c9f2a0 | #c9f2a0 | #c9f2a0 |
| foliageDark | Scenery | #2f6f73 | #2f6f73 | #2f6f73 |
| foliageLight | Scenery | #4f9a8f | #4f9a8f | #4f9a8f |
| trunk | Scenery | #3d3b52 | #3d3b52 | #3d3b52 |
| rock | Scenery | #c3b5e0 | #c3b5e0 | #c3b5e0 |
| uiAccent | Screen | #ff8a5c | #ff8a5c | #ff8a5c |
| uiInk | Screen | #22313f | #22313f | #22313f |
| structureLight | Tower | #dfe7f0 | #dfe7f0 | #dfe7f0 |
| structureDark | Tower | #4c5f86 | #4c5f86 | #4c5f86 |
| structureDetail | Tower | #ff8a5c | #ff8a5c | #ff8a5c |
| defense | Enemy | #2c3e66 | #2c3e66 | #2c3e66 |

- [ ] Luz: sol (50, -30, 0) cor #ffffff intensidade 1; ambiente céu #ffffff chão #bbaacc (ambiente 0.55, direta 0.75); pós: none.

### Cores iguais em todos os mundos

| Papel | Classe | Base | Sombra | Destaque |
|---|---|---|---|---|
| gateGood | Gate | #5ad1a0 | #5ad1a0 | #5ad1a0 |
| gateBad | Gate | #ff6b81 | #ff6b81 | #ff6b81 |

## Materiais

- [ ] Perfil `legacy-toon` (legado): Toon lit, 3-band ramp: structures, crowd, obstacles, props. Shaders: StudioKit/ToonLit.
- [ ] Perfil `legacy-translucent` (legado): URP Unlit, transparent: gate panels, shadows, danger strip, dome. Shaders: Universal Render Pipeline/Unlit.
- [ ] Perfil `legacy-text` (legado): TextMeshPro distance field: menu, HUD, gate and floating numbers. Shaders: TextMeshPro/.
- [ ] Perfil `legacy-ui-text` (legado): The built-in text shader of the dynamic font (UI.Text and TextMesh: health labels, a few HUD texts). Shaders: GUI/Text Shader.
- [ ] Perfil `legacy-sky` (legado): The sky gradient (reads the palette globals). Shaders: StudioKit/SkyGradientScreen.
- [ ] Perfil `legacy-track` (legado): The striped track. Shaders: StudioKit/StripedTrack.
- [ ] Perfil `legacy-blob-shadow` (legado): The instanced blob shadow of the crowd. Shaders: StudioKit/BlobShadowInstanced.
- [ ] Perfil `vinyl` (v3): v3: the character (edge light, soft specular, warm shade). Shader: K.v3a. Sem shader ainda.
- [ ] Perfil `matte-plastic` (v3): v3: scenery, floor, towers. Shader: K.v3a. Sem shader ainda.
- [ ] Perfil `gate-glow` (v3): v3: translucent gate with a lit edge, no depth write. Shader: K.v3a. Sem shader ainda.
- [ ] Perfil `enemy` (v3): v3: darkened vinyl with a threat-colored edge. Shader: K.v3a. Sem shader ainda.
- [ ] Perfil `ui-effect` (v3): v3: unlit UI and effects. Shader: K.v3a. Sem shader ainda.

- [ ] **Unit** usa só: CrowdToon (legacy-toon), CrowdShadow (legacy-blob-shadow).
- [ ] **Leader** usa só: LeaderCrown (legacy-toon).
- [ ] **Enemy** usa só: Iron (legacy-toon), IronWarm (legacy-toon), Soot (legacy-toon), Steel (legacy-toon), Wall (legacy-toon), ShieldCore (legacy-toon), ShieldDome (legacy-translucent), Danger (legacy-translucent).
- [ ] **Tower** usa só: Stone (legacy-toon), StoneDark (legacy-toon), StoneLight (legacy-toon), StoneWarm (legacy-toon), Wood (legacy-toon), Banner (legacy-toon), Terracotta (legacy-toon), ClayDark (legacy-toon), Moss (legacy-toon), Bone (legacy-toon), Health (legacy-toon), Glow (legacy-toon), GlowHot (legacy-toon), WoodDark (legacy-toon), Thatch (legacy-toon), Lantern (legacy-toon), Brick (legacy-toon), Frost (legacy-toon), Basalt (legacy-toon), Ice (legacy-toon).
- [ ] **Gate** usa só: GateGood (legacy-translucent), GateBad (legacy-translucent), GateGoodLit (legacy-translucent), GateBadLit (legacy-translucent), GateOff (legacy-translucent), GatePostToon (legacy-toon), Crate (legacy-toon), Baloo2 Gate Good (legacy-text), Baloo2 Gate Bad (legacy-text).
- [ ] **Obstacle** usa só: ObstacleToon (legacy-toon).
- [ ] **Floor** usa só: GroundToon (legacy-toon), Line (legacy-toon), StripedTrack (legacy-track), LaneMarker (legacy-translucent).
- [ ] **Scenery** usa só: PropsToon (legacy-toon), SkyGradient (legacy-sky).
- [ ] **Effect** usa só: Shadow (legacy-translucent), Baloo2 Float (legacy-text).
- [ ] **Screen** usa só: Baloo2 Counter (legacy-text), Baloo2 Ink (legacy-text), Baloo2 Ui (legacy-text), Baloo2-ExtraBold SDF (legacy-text), GameIcons (legacy-text), Baloo2-ExtraBold (legacy-ui-text).

## Efeitos (limite de ativos ao mesmo tempo)

- [ ] **FloatingNumber**: até 12 (numbers that float up from a gate (FloatingNumbers pool)).
- [ ] **CounterPop**: até 1 (the army counter pop in the HUD).
- [ ] **UnitDeath**: até 512 (ballistic deaths of the instanced swarms (assault); the obstacles' ring is smaller).
- [ ] **CameraShake**: até 1 (the run camera's shake on a bad gate).
- [ ] **ProjectileImpact**: até 32 (stone and landing shadow of the cannon (pool)).
- [ ] **TowerBreak**: até 4 (tower floors crumbling (blocks per body)).

## Sons (pico alvo ± tolerância, antes do mixer)

- [ ] `Coin`: role Coin; pico -1 dBFS ± 1.5 dB.
- [ ] `GateGood`: role GateGood; pico -1 dBFS ± 1.5 dB.
- [ ] `GateBad`: role GateBad; pico -1 dBFS ± 1.5 dB.
- [ ] `UnitLoss`: role UnitLoss; pico -1 dBFS ± 1.5 dB.
- [ ] `Finish`: role Finish; pico -1 dBFS ± 1.5 dB.
- [ ] `Victory`: role Victory; pico -1 dBFS ± 1.5 dB.
- [ ] `Defeat`: role Defeat; pico -1 dBFS ± 1.5 dB.
- [ ] `ButtonClick`: role ButtonClick; pico -1 dBFS ± 1.5 dB.
- [ ] `ButtonBack`: role ButtonBack; pico -1 dBFS ± 1.5 dB.
- [ ] `AdReward`: role AdReward; pico -1 dBFS ± 1.5 dB.
- [ ] `CueCannonFire`: cue CannonFire; pico -1 dBFS ± 1.5 dB.
- [ ] `CueTowerFall`: cue TowerFall; pico -1 dBFS ± 1.5 dB.
- [ ] `CueFloorBreak`: cue FloorBreak; pico -1 dBFS ± 1.5 dB.
- [ ] `CueWallBreak`: cue WallBreak; pico -1 dBFS ± 1.5 dB.
- [ ] `CueSweep`: cue Sweep; pico -1 dBFS ± 1.5 dB.
- [ ] `CueCrate`: cue Crate; pico -1 dBFS ± 1.5 dB.
- [ ] `CueBoss`: cue Boss; pico -1 dBFS ± 1.5 dB.
- [ ] `CueProjectileWarning`: cue ProjectileWarning; pico -1 dBFS ± 1.5 dB.
- [ ] `CueStoneImpact`: cue StoneImpact; pico -1 dBFS ± 1.5 dB.
- [ ] `CueTowerHit`: cue TowerHit; pico -1 dBFS ± 1.5 dB.
- [ ] `MenuLoop`: music: menus; pico -3 dBFS ± 1.5 dB.
- [ ] `RunLoop`: music: run and assault; pico -3 dBFS ± 1.5 dB.
