# Checklist de estilo (gerado)

Gerado de `StyleGuide` por `Claybreak.Editor.StyleChecklist`. Não editar à mão: mude o guia em `StyleGuideContent` e rode `Tools/rt4-check.sh` (passo `scene`) ou `-executeMethod Claybreak.Editor.StyleChecklist.WriteAndQuit`.
Cada linha é uma regra que a captura deve cumprir; classe `legado` = ainda no visual anterior (o guia descreve o que o jogo tem hoje).

## Classes

- [ ] **Unit** (forma legado, superfície v3): teto atual 600 triângulos e 1 chamadas por objeto; meta v3 800–1200 triângulos, até 1 chamadas, reserva 450–550 triângulos, nenhuma aresta viva acima de 60° (chanfro).
- [ ] **Leader** (forma legado, superfície v3): teto atual 400 triângulos e 1 chamadas por objeto; meta v3 3000–5000 triângulos, até 1 chamadas, nenhuma aresta viva acima de 60° (chanfro).
- [ ] **Enemy** (forma legado, superfície v3): teto atual 2200 triângulos e 16 chamadas por objeto; meta v3 1500–3000 triângulos, até 1 chamadas, nenhuma aresta viva acima de 60° (chanfro).
- [ ] **Tower** (forma legado, superfície v3): teto atual 6000 triângulos e 70 chamadas por objeto; meta v3 6000–12000 triângulos, até 4 chamadas, nenhuma aresta viva acima de 60° (chanfro).
- [ ] **Boss** (forma legado, superfície v3): teto atual 13000 triângulos e 75 chamadas por objeto; meta v3 8000–15000 triângulos, até 4 chamadas, nenhuma aresta viva acima de 60° (chanfro).
- [ ] **Gate** (forma legado, superfície legado): teto atual 400 triângulos e 10 chamadas por objeto; meta v3 100–2000 triângulos, até 2 chamadas, nenhuma aresta viva acima de 60° (chanfro).
- [ ] **Obstacle** (forma legado, superfície v3): teto atual 700 triângulos e 1 chamadas por objeto; meta v3 100–2500 triângulos, até 1 chamadas, nenhuma aresta viva acima de 60° (chanfro).
- [ ] **Floor** (forma legado, superfície v3): teto atual 200 triângulos e 2 chamadas por objeto; meta v3 100–4000 triângulos, até 1 chamadas, nenhuma aresta viva acima de 60° (chanfro).
- [ ] **Scenery** (forma legado, superfície v3): teto atual 300 triângulos e 1 chamadas por objeto; meta v3 300–1500 triângulos, até 1 chamadas, nenhuma aresta viva acima de 60° (chanfro).
- [ ] **Effect** (forma legado, superfície legado): sem malha própria.
- [ ] **Sound** (forma v3, superfície legado): sem malha própria.
- [ ] **Screen** (forma legado, superfície legado): sem malha própria.

## Paletas (antes do pós; tolerância ΔE CIE76 ≤ 3 sobre os três níveis: base, sombra, destaque)

### Pastel

| Papel | Classe | Base | Sombra | Destaque |
|---|---|---|---|---|
| skyTop | Scenery | #233764 | #1d2e5c | #354c76 |
| skyBottom | Scenery | #3a5a79 | #314b6f | #587c8f |
| fog | Scenery | #3a5a79 | #314b6f | #587c8f |
| ground | Floor | #2f6f7a | #275c70 | #479991 |
| track | Floor | #e9e9ec | #c4c1d9 | #fbe5c7 |
| trackStripe | Floor | #dbdade | #b8b5cc | #fae2c6 |
| crowd | Unit | #ff2e9a | #d6268e | #fd2a77 |
| foliageDark | Scenery | #1f5d6b | #1a4d62 | #2f817f |
| foliageLight | Scenery | #3f8f97 | #35778b | #60c6b3 |
| trunk | Scenery | #3a3f66 | #31345e | #585779 |
| rock | Scenery | #c9c3e6 | #a9a2d4 | #f7dadc |
| uiAccent | Screen | #ff6f91 | #ff6f91 | #ff6f91 |
| uiInk | Screen | #3b2f4a | #3b2f4a | #3b2f4a |
| structureLight | Tower | #f2f4fa | #cbcbe6 | #fce7cb |
| structureDark | Tower | #3b4a7a | #323d70 | #5a6691 |
| structureDetail | Tower | #39e6ff | #30bfeb | #44f8ec |
| defense | Enemy | #0d5a55 | #0b4b4e | #147c65 |

- [ ] Clima: fábrica noturna, céu azul-marinho, pista clara neutra, multidão rosa-choque, brilho ciano, defesas petróleo-escuro
- [ ] Luz (vinil): sol (50, -30, 0), principal #ffe8c7 × 1.05 (direta 0.78); ambiente céu #a8c7ff chão #6c7da2 × 0.7; sombra #d6d4eb; contorno #92f1ff × 0.55; rampa 0.12; ombro 0.78; pico 1.1.
- [ ] Névoa na cor `fog` da paleta, 40–180 m.
- [ ] Pós: exposição 0, contraste 10, saturação 10, vinheta 0.22 (#0a0d24), bloom 0.4 acima de 1; sem tonemapping; UI fora do pós.

### PorDoSol

| Papel | Classe | Base | Sombra | Destaque |
|---|---|---|---|---|
| skyTop | Scenery | #2e2458 | #271e51 | #463268 |
| skyBottom | Scenery | #5a4a7e | #4c3d74 | #896695 |
| fog | Scenery | #5a4a7e | #4c3d74 | #896695 |
| ground | Floor | #2e4f5e | #274256 | #466d6f |
| track | Floor | #efebe7 | #c9c3d5 | #fce1be |
| trackStripe | Floor | #e2ddd8 | #beb7c7 | #fbdfbb |
| crowd | Unit | #f0600a | #ca5009 | #fc5c08 |
| foliageDark | Scenery | #2b2552 | #241f4b | #413361 |
| foliageLight | Scenery | #4a3f7a | #3e3470 | #705791 |
| trunk | Scenery | #241d40 | #1e183b | #37284c |
| rock | Scenery | #9b7fb8 | #8269a9 | #e2a8d1 |
| uiAccent | Screen | #f26a5a | #f26a5a | #f26a5a |
| uiInk | Screen | #2d2448 | #2d2448 | #2d2448 |
| structureLight | Tower | #f3eef8 | #ccc6e4 | #fce1c9 |
| structureDark | Tower | #4a3a78 | #3e306e | #70508e |
| structureDetail | Tower | #ffb347 | #d69541 | #fda237 |
| defense | Enemy | #2a52b8 | #2344a9 | #3f70d7 |

- [ ] Clima: fábrica noturna ao fim do turno, céu violeta-escuro, pista clara neutra, multidão laranja, brilho âmbar, defesas azul-cobalto
- [ ] Luz (vinil): sol (50, -30, 0), principal #ffe8c7 × 1.05 (direta 0.78); ambiente céu #a8c7ff chão #6b7298 × 0.7; sombra #d6d4eb; contorno #ffd59a × 0.55; rampa 0.12; ombro 0.78; pico 1.1.
- [ ] Névoa na cor `fog` da paleta, 40–180 m.
- [ ] Pós: exposição 0, contraste 10, saturação 10, vinheta 0.22 (#0a0d24), bloom 0.4 acima de 1; sem tonemapping; UI fora do pós.

### Doce

| Papel | Classe | Base | Sombra | Destaque |
|---|---|---|---|---|
| skyTop | Scenery | #173a5c | #133055 | #23506d |
| skyBottom | Scenery | #2f6a80 | #275876 | #479398 |
| fog | Scenery | #2f6a80 | #275876 | #479398 |
| ground | Floor | #2c7a6a | #256562 | #43a97e |
| track | Floor | #f0efe9 | #cac6d6 | #fce4bf |
| trackStripe | Floor | #e3e1d8 | #bfbbc7 | #fbe2ba |
| crowd | Unit | #a03cff | #8632eb | #c644f6 |
| foliageDark | Scenery | #1d5f55 | #184f4e | #2c8365 |
| foliageLight | Scenery | #3d9a7f | #338075 | #5cd395 |
| trunk | Scenery | #2f3b52 | #27314b | #475261 |
| rock | Scenery | #f2b8d0 | #cb99bf | #fcafa9 |
| uiAccent | Screen | #2fa7e6 | #2fa7e6 | #2fa7e6 |
| uiInk | Screen | #24435a | #24435a | #24435a |
| structureLight | Tower | #fbf7ea | #d3cdd7 | #fde2b8 |
| structureDark | Tower | #3a4f6e | #314265 | #586d82 |
| structureDetail | Tower | #5ff2c2 | #50c9b2 | #6cfaac |
| defense | Enemy | #b07a12 | #946511 | #ee9613 |

- [ ] Clima: fábrica noturna de doces, céu azul-petróleo, pista clara neutra, multidão violeta, brilho menta, defesas âmbar-escuro
- [ ] Luz (vinil): sol (50, -30, 0), principal #ffe8c7 × 1.05 (direta 0.78); ambiente céu #a8c7ff chão #6b819c × 0.7; sombra #d6d4eb; contorno #a7f8dd × 0.55; rampa 0.12; ombro 0.78; pico 1.1.
- [ ] Névoa na cor `fog` da paleta, 40–180 m.
- [ ] Pós: exposição 0, contraste 10, saturação 10, vinheta 0.22 (#0a0d24), bloom 0.4 acima de 1; sem tonemapping; UI fora do pós.

### Aurora

| Papel | Classe | Base | Sombra | Destaque |
|---|---|---|---|---|
| skyTop | Scenery | #152a52 | #12234b | #203a61 |
| skyBottom | Scenery | #2f5a7e | #274b74 | #477c95 |
| fog | Scenery | #2f5a7e | #274b74 | #477c95 |
| ground | Floor | #24586a | #1e4962 | #377a7e |
| track | Floor | #23273a | #1d2035 | #353645 |
| trackStripe | Floor | #2f3446 | #272b40 | #474853 |
| crowd | Unit | #9be21c | #82bc1a | #bbf81a |
| foliageDark | Scenery | #1e5a66 | #194b5e | #2e7c79 |
| foliageLight | Scenery | #2f7f86 | #27697b | #47b09f |
| trunk | Scenery | #2a2d44 | #23253f | #403e51 |
| rock | Scenery | #b8c8ef | #9ba6dc | #efedf3 |
| uiAccent | Screen | #ff8a5c | #ff8a5c | #ff8a5c |
| uiInk | Screen | #22313f | #22313f | #22313f |
| structureLight | Tower | #e8f1fb | #c3c8e7 | #fbeed4 |
| structureDark | Tower | #33406a | #2b3562 | #4d587e |
| structureDetail | Tower | #6af0ff | #59c7eb | #79fae3 |
| defense | Enemy | #7a2f9e | #662791 | #b941bb |

- [ ] Clima: fábrica noturna de gelo, céu azul-profundo, pista escura neutra sobre chão petróleo, multidão lima, brilho ciano-gelo, defesas roxas
- [ ] Luz (vinil): sol (50, -30, 0), principal #ffe8c7 × 1.05 (direta 0.78); ambiente céu #a8c7ff chão #68759c × 0.7; sombra #d6d4eb; contorno #adf7ff × 0.55; rampa 0.12; ombro 0.78; pico 1.1.
- [ ] Névoa na cor `fog` da paleta, 40–180 m.
- [ ] Pós: exposição 0, contraste 10, saturação 10, vinheta 0.22 (#0a0d24), bloom 0.4 acima de 1; sem tonemapping; UI fora do pós.

## Contraste (D36, CIELAB; paleta antes do pós e captura dos 4 mundos depois dele)

- [ ] A multidão tem o maior croma da tela: ≥ 5 acima de qualquer outra cor do mundo na paleta; ≥ 0 acima de pista, chão, cenário, inimigos, torre e céu na captura.
- [ ] Pista neutra (croma ≤ 15) e longe da multidão em valor: |ΔL*| ≥ 30 na paleta, ≥ 20 na captura.
- [ ] Inimigos na cor complementar: matiz a ≥ 115° da multidão na paleta, ≥ 90° na captura.
- [ ] Números dos portões legíveis: preenchimento × contorno ≥ 3:1 na captura.
- [ ] Toda skin a ΔE ≥ 20 da pista na captura (depois do pós).

### Cores iguais em todos os mundos

| Papel | Classe | Base | Sombra | Destaque |
|---|---|---|---|---|
| gateGood | Gate | #5ad1a0 | #5ad1a0 | #5ad1a0 |
| gateBad | Gate | #ff6b81 | #ff6b81 | #ff6b81 |

## Materiais

- [ ] Perfil `legacy-toon` (legado): Toon lit, 3-band ramp (before v3): gate panels' classes still legacy. Shaders: StudioKit/ToonLit.
- [ ] Perfil `legacy-translucent` (legado): URP Unlit, transparent: gate panels, effect shadows. Shaders: Universal Render Pipeline/Unlit.
- [ ] Perfil `legacy-text` (legado): TextMeshPro distance field: menu, HUD, gate and floating numbers. Shaders: TextMeshPro/.
- [ ] Perfil `legacy-ui-text` (legado): The built-in text shader of the dynamic font (UI.Text and TextMesh: health labels, a few HUD texts). Shaders: GUI/Text Shader.
- [ ] Perfil `legacy-sky` (legado): The sky gradient (reads the palette globals). Shaders: StudioKit/SkyGradientScreen.
- [ ] Perfil `legacy-track` (legado): The striped track. Shaders: StudioKit/StripedTrack.
- [ ] Perfil `legacy-blob-shadow` (legado): The instanced blob shadow of the crowd. Shaders: StudioKit/BlobShadowInstanced.
- [ ] Perfil `vinyl` (v3): v3: the character's glossy vinyl (soft ramp, warm key, highlight and rim, also on top). Shaders: StudioKit/ToonVinyl. Superfície: brilho 0.6 (tamanho 0.35, no topo 1), contorno 1 #ffffff, escurecer 0.
- [ ] Perfil `matte-plastic` (v3): v3: matte plastic for the floor, the rails, the scenery and the gate posts; no gloss on faces looking up. Shaders: StudioKit/ToonVinyl. Superfície: brilho 0.12 (tamanho 0.6, no topo 0), contorno 0.45 #ffffff, escurecer 0.
- [ ] Perfil `structure` (v3): v3: light matte plastic of the towers and bosses (the night factory's clear structures), a cool rim; no gloss on top. Shaders: StudioKit/ToonVinyl. Superfície: brilho 0.22 (tamanho 0.5, no topo 0), contorno 0.7 #ffffff, escurecer 0.
- [ ] Perfil `enemy` (v3): v3: darkened vinyl with a warm threat rim (defenses and obstacles). Shaders: StudioKit/ToonVinyl. Superfície: brilho 0.45 (tamanho 0.35, no topo 0.5), contorno 1.4 #ff6b57, escurecer 0.12.
- [ ] Perfil `sky` (v3): v3: the night sky gradient of the world (palette sky top/bottom). Shaders: StudioKit/SkyGradientScreen.
- [ ] Perfil `track` (v3): v3: the neutral striped track (unlit stripes, no gloss). Shaders: StudioKit/StripedTrack.
- [ ] Perfil `contact-shadow` (v3): v3: the contact shadow, a round blob under each figure (D36). Shaders: StudioKit/BlobShadowInstanced.
- [ ] Perfil `unlit-overlay` (v3): v3: unlit translucent overlays of the play area (lane marker, danger strip, shield dome). Shaders: Universal Render Pipeline/Unlit.
- [ ] Perfil `gate-glow` (v3): v3: translucent gate with a lit edge, no depth write (RT-6v3.6). Sem shader ainda.
- [ ] Perfil `ui-effect` (v3): v3: unlit UI and effects (RT-6v3.8/3.10). Sem shader ainda.

- [ ] **Unit** usa só: CrowdToon (vinyl), CrowdShadow (contact-shadow).
- [ ] **Leader** usa só: LeaderCrown (vinyl).
- [ ] **Enemy** usa só: Iron (enemy), IronWarm (enemy), Soot (enemy), Steel (enemy), Wall (enemy), ShieldCore (enemy), ShieldDome (unlit-overlay), Danger (unlit-overlay).
- [ ] **Tower** usa só: Stone (structure), StoneDark (structure), StoneLight (structure), StoneWarm (structure), Wood (structure), Banner (structure), Terracotta (structure), ClayDark (structure), Moss (structure), Bone (structure), Health (structure), Glow (structure), GlowHot (structure), WoodDark (structure), Thatch (structure), Lantern (structure), Brick (structure), Frost (structure), Basalt (structure), Ice (structure).
- [ ] **Gate** usa só: GateGood (legacy-translucent), GateBad (legacy-translucent), GateGoodLit (legacy-translucent), GateBadLit (legacy-translucent), GateOff (legacy-translucent), GatePostToon (matte-plastic), Crate (matte-plastic), Baloo2 Gate Good (legacy-text), Baloo2 Gate Bad (legacy-text).
- [ ] **Obstacle** usa só: ObstacleToon (enemy).
- [ ] **Floor** usa só: GroundToon (matte-plastic), Line (matte-plastic), StripedTrack (track), LaneMarker (unlit-overlay).
- [ ] **Scenery** usa só: PropsToon (matte-plastic), SkyGradient (sky).
- [ ] **Effect** usa só: Shadow (legacy-translucent), Baloo2 Float (legacy-text).
- [ ] **Screen** usa só: Baloo2 Counter (legacy-text), Baloo2 Ink (legacy-text), Baloo2 Ui (legacy-text), Baloo2-ExtraBold SDF (legacy-text), GameIcons (legacy-text), Baloo2-ExtraBold (legacy-ui-text).

## Efeitos (limite de ativos ao mesmo tempo)

- [ ] **FloatingNumber**: até 12 (numbers that float up from a gate (FloatingNumbers pool)).
- [ ] **CounterPop**: até 1 (the army counter pop in the HUD).
- [ ] **UnitDeath**: até 512 (ballistic deaths of the instanced swarms (assault); the obstacles' ring is smaller).
- [ ] **CameraShake**: até 1 (the run camera's shake on a bad gate).
- [ ] **ProjectileImpact**: até 32 (stone and landing shadow of the cannon (pool)).
- [ ] **TowerBreak**: até 4 (tower floors crumbling (blocks per body)).

## Rosto do Bibopzi (evento → expressão, RT-6v3.4b)

Atlas de 8 células; base = fica até outra base; reação aparece sobre base e reação de prioridade menor ou igual; piscar a cada 3.4 s no rosto de corrida.

| Evento | Expressão | Duração | Prioridade | Mãos abertas |
|---|---|---|---|---|
| Run | Neutral | base | 0 | 0 |
| Blink | Blink | 0.13 s | 1 | 0 |
| GoodGate | Happy | 0.9 s | 2 | 0.55 |
| BadGate | Scared | 0.8 s | 3 | 1 |
| ObstacleHit | Scared | 0.6 s | 3 | 1 |
| Assault | Effort | base | 1 | 0.25 |
| Launch | Ouch | base | 0 | 1 |
| Thrown | Ouch | base | 0 | 1 |
| Victory | Victory | base | 5 | 1 |
| Defeat | Defeat | base | 5 | 0 |

## Sons (pico alvo ± tolerância, antes do mixer)

- [ ] `Coin`: role Coin: purchase (short double bell); pico -1 dBFS ± 1.5 dB; volume -15 ± 5 dBFS; 0.05–1.5 s.
- [ ] `GateGood`: role GateGood: crowd takes a good gate (ratchet + first three notes of the motif on a short bell); pico -1 dBFS ± 1.5 dB; volume -15 ± 5 dBFS; 0.05–1.5 s.
- [ ] `GateBad`: role GateBad: bad gate or obstacle loss (coil spring loosening); pico -1 dBFS ± 1.5 dB; volume -15 ± 5 dBFS; 0.05–1.5 s.
- [ ] `UnitLoss`: role UnitLoss: a figure bursts into pieces and confetti (assault); pico -1 dBFS ± 1.5 dB; volume -15 ± 5 dBFS; 0.05–1.5 s.
- [ ] `Finish`: role Finish; pico -1 dBFS ± 1.5 dB; volume -15 ± 5 dBFS; 0.05–1.5 s.
- [ ] `Victory`: role Victory; pico -1 dBFS ± 1.5 dB; volume -15 ± 5 dBFS; 0.5–3 s.
- [ ] `Defeat`: role Defeat; pico -1 dBFS ± 1.5 dB; volume -15 ± 5 dBFS; 0.5–3 s.
- [ ] `ButtonClick`: role ButtonClick: button press (ratchet tick); pico -1 dBFS ± 1.5 dB; volume -15 ± 5 dBFS; 0.04–0.25 s.
- [ ] `ButtonBack`: role ButtonBack: back (lower tick); pico -1 dBFS ± 1.5 dB; volume -15 ± 5 dBFS; 0.04–0.25 s.
- [ ] `AdReward`: role AdReward; pico -1 dBFS ± 1.5 dB; volume -15 ± 5 dBFS; 0.05–1.5 s.
- [ ] `CueCannonFire`: cue CannonFire; pico -1 dBFS ± 1.5 dB; volume -15 ± 5 dBFS; 0.05–1.5 s.
- [ ] `CueTowerFall`: cue TowerFall; pico -1 dBFS ± 1.5 dB; volume -15 ± 5 dBFS; 0.5–3 s.
- [ ] `CueFloorBreak`: cue FloorBreak; pico -1 dBFS ± 1.5 dB; volume -15 ± 5 dBFS; 0.05–1.5 s.
- [ ] `CueWallBreak`: cue WallBreak; pico -1 dBFS ± 1.5 dB; volume -15 ± 5 dBFS; 0.05–1.5 s.
- [ ] `CueSweep`: cue Sweep; pico -1 dBFS ± 1.5 dB; volume -15 ± 5 dBFS; 0.05–1.5 s.
- [ ] `CueCrate`: cue Crate; pico -1 dBFS ± 1.5 dB; volume -15 ± 5 dBFS; 0.05–1.5 s.
- [ ] `CueBoss`: cue Boss; pico -1 dBFS ± 1.5 dB; volume -15 ± 5 dBFS; 0.5–3 s.
- [ ] `CueProjectileWarning`: cue ProjectileWarning; pico -1 dBFS ± 1.5 dB; volume -7 ± 3 dBFS; 0.05–1.5 s.
- [ ] `CueStoneImpact`: cue StoneImpact; pico -1 dBFS ± 1.5 dB; volume -15 ± 5 dBFS; 0.05–1.5 s.
- [ ] `CueTowerHit`: cue TowerHit; pico -1 dBFS ± 1.5 dB; volume -15 ± 5 dBFS; 0.05–1.5 s.
- [ ] `CueKeyWind`: cue KeyWind: the wind-up key turns as a level starts; pico -1 dBFS ± 1.5 dB; volume -15 ± 5 dBFS; 0.05–1.5 s.
- [ ] `CueNumberPop`: cue NumberPop: the number of a good gate jumps; pico -1 dBFS ± 1.5 dB; volume -15 ± 5 dBFS; 0.05–1.5 s.
- [ ] `MenuLoop`: music: menus (the motif in C major, marimba and bell); pico -3 dBFS ± 1.5 dB; volume -17 ± 3 dBFS; 10–25 s.
- [ ] `RunLoop`: music: run and assault, world 1 (C major, 124 BPM, bell lead); pico -3 dBFS ± 1.5 dB; volume -17 ± 3 dBFS; 10–25 s.
- [ ] `RunLoopW2`: music: run and assault, world 2 (A minor, 132 BPM, pluck lead); pico -3 dBFS ± 1.5 dB; volume -17 ± 3 dBFS; 10–25 s.
- [ ] `RunLoopW3`: music: run and assault, world 3 (F major, 112 BPM, swing, brass lead); pico -3 dBFS ± 1.5 dB; volume -17 ± 3 dBFS; 10–25 s.
- [ ] `RunLoopW4`: music: run and assault, world 4 (D minor, 140 BPM, square lead); pico -3 dBFS ± 1.5 dB; volume -17 ± 3 dBFS; 10–25 s.
