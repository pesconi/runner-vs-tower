# RT-6 — Economia simulada e fases (gerado por `EconomyReport`, não editar à mão)

Preços e curvas **[P]** do catálogo (`WorldContent.WriteCatalog`), jogados por um jogador simulado com as
regras reais (`Progression`). Premissas **[I]** (sem analytics ainda, D4):

- vitória em 55 s (corrida + assalto + resultado); derrota em 35 s;
- 80% de vitórias; 60 unidades na linha de tiro, 40% sobrevivem (2 estrelas);
- compra o mais barato que puder, upgrades antes de elites e skins; joga os 4 mundos em ordem (40 fases)
  e, depois do último, fica nas voltas dele (cada volta 25% mais forte e 25% mais rica).

Média: **365 moedas por minuto** de jogo.

| Mundo | Primeira passada termina em | Fases jogadas | Moedas ganhas até ali |
|---|---|---|---|
| world1 | 10,9 min | 13 | 1693 |
| world2 | 21,3 min | 25 | 4333 |
| world3 | 32,2 min | 38 | 8110 |
| world4 | 42,5 min | 50 | 13186 |

| # | Compra | Preço | Fases jogadas | Minutos de jogo | Volta do mundo |
|---|---|---|---|---|---|
| 1 | ARMY 1 | 60 | 2 | 1,5 | 0 |
| 2 | LAUNCH SPEED 1 | 70 | 3 | 2,4 | 0 |
| 3 | DAMAGE 1 | 80 | 4 | 3,3 | 0 |
| 4 | GATE BOOST 1 | 100 | 5 | 4,3 | 0 |
| 5 | ARMY 2 | 120 | 5 | 4,3 | 0 |
| 6 | LAUNCH SPEED 2 | 130 | 7 | 5,8 | 0 |
| 7 | DAMAGE 2 | 150 | 8 | 6,7 | 0 |
| 8 | GATE BOOST 2 | 180 | 9 | 7,6 | 0 |
| 9 | ARMY 3 | 200 | 10 | 8,5 | 0 |
| 10 | LAUNCH SPEED 3 | 220 | 12 | 10,0 | 0 |
| 11 | DAMAGE 3 | 250 | 13 | 10,9 | 0 |
| 12 | GATE BOOST 3 | 300 | 14 | 11,8 | 0 |
| 13 | ARMY 4 | 320 | 17 | 14,3 | 0 |
| 14 | LAUNCH SPEED 4 | 350 | 19 | 16,1 | 0 |
| 15 | DAMAGE 4 | 400 | 20 | 17,0 | 0 |
| 16 | GATE BOOST 4 | 450 | 23 | 19,4 | 0 |
| 17 | ARMY 5 | 480 | 25 | 21,3 | 0 |
| 18 | LAUNCH SPEED 5 | 520 | 27 | 22,8 | 0 |
| 19 | DAMAGE 5 | 600 | 29 | 24,6 | 0 |
| 20 | GATE BOOST 5 | 650 | 32 | 27,0 | 0 |
| 21 | ARMY 6 | 700 | 34 | 28,8 | 0 |
| 22 | LAUNCH SPEED 6 | 750 | 37 | 31,3 | 0 |
| 23 | DAMAGE 6 | 900 | 38 | 32,2 | 0 |
| 24 | GATE BOOST 6 | 950 | 42 | 35,5 | 0 |
| 25 | SPRINTER | 600 | 43 | 36,4 | 0 |
| 26 | WARDEN | 900 | 45 | 38,3 | 0 |
| 27 | skin EMBER | 150 | 45 | 38,3 | 0 |
| 28 | skin MOSS | 300 | 47 | 39,8 | 0 |
| 29 | skin SLATE | 500 | 48 | 40,7 | 0 |
| 30 | skin BONE | 800 | 50 | 42,5 | 1 |
| 31 | skin INK | 1200 | 52 | 44,0 | 1 |
| 32 | skin MIRE | 1500 | 55 | 46,8 | 1 |
| 33 | skin CINDER | 2000 | 59 | 50,1 | 1 |
| 34 | skin FROST | 2500 | 63 | 53,4 | 2 |

Total: **19380 moedas**, tudo comprado em **53 minutos** (63 fases).

## Sensibilidade

- Primeira passada do world1 (10 fases, sem comprar): **1663 moedas** = 9% da loja (19380).
- Primeira passada do world2 (10 fases, sem comprar): **2607 moedas** = 13% da loja (19380).
- Primeira passada do world3 (10 fases, sem comprar): **3703 moedas** = 19% da loja (19380).
- Primeira passada do world4 (10 fases, sem comprar): **5008 moedas** = 26% da loja (19380).
- Preços x0,75: tudo comprado em **46 minutos** (54 fases, volta 1).
- Preços x1,5: tudo comprado em **67 minutos** (79 fases, volta 3).

Vieses conhecidos: a taxa de vitória e os sobreviventes não mudam com os upgrades nem com as voltas mais
fortes, e a renda futura de anúncio recompensado e recompensa diária (RT-7) não entra.

## Fases (dados gerados; vida antes das voltas)

Potencial = exército no melhor caminho dos portões (teto 300) × melhor portão da fileira. Defesas: M muralha,
C canhão (intervalo, pedras), V varredor, E escudo (parte do dano que segura).

### WORLD 1 (world1) — cenário Meadow, dano esperado x1, abre com 0 estrelas

| Fase | Torre | Andares | Vida | Início | Potencial | Defesas | Prêmio |
|---|---|---|---|---|---|---|---|
| LEVEL 1 | Keep | 1 | 110 | 10 | 160 | C3.0s/2 | 40 |
| LEVEL 2 | Keep | 1 | 250 | 10 | 480 | M60 C2.2s/4 | 50 |
| LEVEL 3 | World1Second | 2 | 300 | 12 | 582 | C2.3s/4 V3.4s | 60 |
| LEVEL 4 | Keep | 2 | 320 | 12 | 522 | M70 C2.0s/4 | 70 |
| LEVEL 5 | World1Second | 3 | 440 | 14 | 894 | C1.9s/5 V3.0s | 85 |
| LEVEL 6 | Keep | 3 | 470 | 15 | 735 | M90 C1.8s/5 V3.0s | 100 |
| LEVEL 7 | Keep | 3 | 480 | 14 | 825 | M90 C1.85s/5 V3.0s | 105 |
| LEVEL 8 | Keep | 3 | 530 | 15 | 900 | M100 C1.8s/5 V2.9s | 115 |
| LEVEL 9 | World1Second | 3 | 550 | 15 | 900 | M100 C1.7s/6 V2.9s | 125 |
| BOSS | Boss (ShiftGates) | 4 | 600 | 16 | 900 | M100 C1.6s/6 V2.8s | 200 |

### WORLD 2 (world2) — cenário Mire, dano esperado x1.3, abre com 10 estrelas

| Fase | Torre | Andares | Vida | Início | Potencial | Defesas | Prêmio |
|---|---|---|---|---|---|---|---|
| LEVEL 1 | World2Tower | 1 | 110 | 14 | 164 | M20 C2.2s/4 | 110 |
| LEVEL 2 | World2Tower | 1 | 220 | 15 | 320 | M40 C2.1s/4 | 115 |
| LEVEL 3 | World2Tower | 1 | 380 | 15 | 540 | M70 C2.05s/5 E50/45% | 125 |
| LEVEL 4 | Forge | 2 | 500 | 16 | 684 | C1.95s/5 V3.2s | 130 |
| LEVEL 5 | World2Tower | 2 | 540 | 17 | 729 | M100 C1.9s/5 V3.1s | 135 |
| LEVEL 6 | World2Tower | 2 | 550 | 17 | 726 | M100 C1.8s/6 V3.1s E80/50% | 145 |
| LEVEL 7 | World2Tower | 3 | 650 | 18 | 828 | C1.75s/6 V3.0s E90/55% | 150 |
| LEVEL 8 | Forge | 3 | 680 | 19 | 855 | M120 C1.65s/6 V2.9s E100/55% | 155 |
| LEVEL 9 | World2Tower | 3 | 720 | 19 | 888 | M130 C1.6s/7 V2.9s E100/60% | 165 |
| BOSS | World2Boss (ShieldReturns) | 4 | 800 | 20 | 900 | M140 C1.5s/7 V2.8s E110/60% | 350 |

### WORLD 3 (world3) — cenário Foundry, dano esperado x1.5, abre com 25 estrelas

| Fase | Torre | Andares | Vida | Início | Potencial | Defesas | Prêmio |
|---|---|---|---|---|---|---|---|
| LEVEL 1 | World3Tower | 1 | 180 | 18 | 228 | M30 C1.9s/5 V3.1s | 170 |
| LEVEL 2 | World3Tower | 1 | 250 | 19 | 304 | C1.85s/5 | 180 |
| LEVEL 3 | World3Tower | 1 | 330 | 19 | 387 | M60 C1.75s/6 | 185 |
| LEVEL 4 | World2Tower | 2 | 400 | 20 | 465 | C1.7s/6 E60/50% | 195 |
| LEVEL 5 | World3Tower | 2 | 490 | 21 | 546 | M90 C1.65s/6 | 200 |
| LEVEL 6 | World3Tower | 2 | 750 | 21 | 825 | M140 C1.55s/7 V2.8s E100/55% | 210 |
| LEVEL 7 | World3Tower | 3 | 840 | 22 | 900 | M150 C1.5s/7 | 215 |
| LEVEL 8 | World2Tower | 3 | 860 | 23 | 900 | M150 C1.45s/7 | 225 |
| LEVEL 9 | World3Tower | 3 | 870 | 23 | 900 | M160 C1.35s/8 V2.6s E120/60% | 230 |
| BOSS | World3Boss (Overheat) | 4 | 940 | 24 | 900 | M170 C1.3s/8 V2.5s E130/60% | 500 |

### WORLD 4 (world4) — cenário Peaks, dano esperado x1.75, abre com 45 estrelas

| Fase | Torre | Andares | Vida | Início | Potencial | Defesas | Prêmio |
|---|---|---|---|---|---|---|---|
| LEVEL 1 | World4Tower | 1 | 230 | 22 | 236 | M40 C1.7s/6 V2.9s | 240 |
| LEVEL 2 | World4Tower | 1 | 310 | 23 | 312 | M60 C1.65s/6 E40/50% | 250 |
| LEVEL 3 | World4Tower | 1 | 410 | 24 | 399 | C1.6s/7 E60/55% | 260 |
| LEVEL 4 | World3Tower | 2 | 630 | 25 | 600 | M110 C1.5s/7 V2.7s E90/55% | 265 |
| LEVEL 5 | World4Tower | 2 | 650 | 26 | 606 | M120 C1.45s/7 V2.6s E90/55% | 275 |
| LEVEL 6 | World4Tower | 2 | 750 | 26 | 684 | M140 C1.4s/8 V2.6s E100/60% | 285 |
| LEVEL 7 | World4Tower | 3 | 770 | 27 | 684 | M140 C1.35s/8 V2.5s E110/60% | 295 |
| LEVEL 8 | World3Tower | 3 | 840 | 28 | 738 | M150 C1.25s/8 V2.4s | 300 |
| LEVEL 9 | World4Tower | 3 | 890 | 29 | 762 | M160 C1.2s/9 V2.4s | 310 |
| BOSS | World4Boss (TurnGate) | 4 | 1130 | 30 | 900 | M200 C1.15s/9 V2.3s E160/65% | 700 |
