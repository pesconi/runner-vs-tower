# RT-6 — Economia simulada e fases (gerado por `EconomyReport`, não editar à mão)

Preços e curvas **[P]** do catálogo (`WorldContent.WriteCatalog`), jogados por um jogador simulado com as
regras reais (`Progression`). Premissas **[I]** (sem analytics ainda, D4):

- corrida = pista até a linha de tiro ÷ velocidade da fase (ritmo x1,3, RT-6t.15; média 13,0 s por fase jogada); depois dela, assalto + resultado em 39 s na vitória e 19 s na derrota;
- 80% de vitórias; 60 unidades na linha de tiro, 40% sobrevivem (2 estrelas);
- compra o mais barato que puder, upgrades antes de elites e skins; joga os 4 mundos em ordem (40 fases)
  e, depois do último, fica nas voltas dele (cada volta 25% mais forte e 25% mais rica).

Média: **388 moedas por minuto** de jogo.

| Mundo | Primeira passada termina em | Fases jogadas | Moedas ganhas até ali |
|---|---|---|---|
| world1 | 10,7 min | 13 | 1693 |
| world2 | 20,5 min | 25 | 4333 |
| world3 | 30,7 min | 38 | 8110 |
| world4 | 40,2 min | 50 | 13186 |

| # | Compra | Preço | Fases jogadas | Minutos de jogo | Volta do mundo |
|---|---|---|---|---|---|
| 1 | ARMY 1 | 60 | 2 | 1,4 | 0 |
| 2 | LAUNCH SPEED 1 | 70 | 3 | 2,3 | 0 |
| 3 | DAMAGE 1 | 80 | 4 | 3,1 | 0 |
| 4 | GATE BOOST 1 | 100 | 5 | 4,0 | 0 |
| 5 | ARMY 2 | 120 | 5 | 4,0 | 0 |
| 6 | LAUNCH SPEED 2 | 130 | 7 | 5,5 | 0 |
| 7 | DAMAGE 2 | 150 | 8 | 6,4 | 0 |
| 8 | GATE BOOST 2 | 180 | 9 | 7,4 | 0 |
| 9 | ARMY 3 | 200 | 10 | 8,3 | 0 |
| 10 | LAUNCH SPEED 3 | 220 | 12 | 9,8 | 0 |
| 11 | DAMAGE 3 | 250 | 13 | 10,7 | 0 |
| 12 | GATE BOOST 3 | 300 | 14 | 11,5 | 0 |
| 13 | ARMY 4 | 320 | 17 | 13,7 | 0 |
| 14 | LAUNCH SPEED 4 | 350 | 19 | 15,4 | 0 |
| 15 | DAMAGE 4 | 400 | 20 | 16,3 | 0 |
| 16 | GATE BOOST 4 | 450 | 23 | 18,7 | 0 |
| 17 | ARMY 5 | 480 | 25 | 20,5 | 0 |
| 18 | LAUNCH SPEED 5 | 520 | 27 | 21,8 | 0 |
| 19 | DAMAGE 5 | 600 | 29 | 23,5 | 0 |
| 20 | GATE BOOST 5 | 650 | 32 | 25,7 | 0 |
| 21 | ARMY 6 | 700 | 34 | 27,5 | 0 |
| 22 | LAUNCH SPEED 6 | 750 | 37 | 29,8 | 0 |
| 23 | DAMAGE 6 | 900 | 38 | 30,7 | 0 |
| 24 | GATE BOOST 6 | 950 | 42 | 33,6 | 0 |
| 25 | SPRINTER | 600 | 43 | 34,5 | 0 |
| 26 | WARDEN | 900 | 45 | 36,2 | 0 |
| 27 | skin CORAL | 150 | 45 | 36,2 | 0 |
| 28 | skin MANGO | 300 | 47 | 37,6 | 0 |
| 29 | skin SKY | 500 | 48 | 38,5 | 0 |
| 30 | skin LILAC | 800 | 50 | 40,2 | 1 |
| 31 | skin INDIGO | 1200 | 52 | 41,5 | 1 |
| 32 | skin LAGOON | 1500 | 55 | 44,0 | 1 |
| 33 | skin BERRY | 2000 | 59 | 47,1 | 1 |
| 34 | skin BUBBLEGUM | 2500 | 63 | 50,3 | 2 |

Total: **19380 moedas**, tudo comprado em **50 minutos** (63 fases).

## Sensibilidade

- Primeira passada do world1 (10 fases, sem comprar): **1663 moedas** = 9% da loja (19380).
- Primeira passada do world2 (10 fases, sem comprar): **2607 moedas** = 13% da loja (19380).
- Primeira passada do world3 (10 fases, sem comprar): **3703 moedas** = 19% da loja (19380).
- Primeira passada do world4 (10 fases, sem comprar): **5008 moedas** = 26% da loja (19380).
- Preços x0,75: tudo comprado em **43 minutos** (54 fases, volta 1).
- Preços x1,5: tudo comprado em **63 minutos** (79 fases, volta 3).

Vieses conhecidos: a taxa de vitória e os sobreviventes não mudam com os upgrades nem com as voltas mais
fortes, e a renda futura de anúncio recompensado e recompensa diária (RT-7) não entra.

## Fases (dados gerados; vida antes das voltas)

Potencial = exército no melhor caminho dos portões (teto 300) × melhor portão da fileira. Maior promessa = maior número que
um portão da corrida pode mostrar em qualquer caminho, com as caixas (nunca passa de 300, RT-6t.14). Defesas: M muralha,
C canhão (intervalo, pedras), V varredor, E escudo (parte do dano que segura). Obstáculos (RT-6t.19): T tronco, S serra,
Ma martelo, B buraco, R rolo, com o ciclo em segundos. Perda esperada = parte do exército do melhor caminho que os obstáculos
matam até a linha de tiro (estimativa `ObstacleLoss` [I]: quem desvia lê o obstáculo e acerta o momento em ±0,3 s; quem
não desvia corre reto pelo meio); a vida da torre é calibrada sobre o exército de quem desvia.

### WORLD 1 (world1) — paleta Pastel, 144 árvores e 64 pedras, dano esperado x1, abre com 0 estrelas

| Fase | Torre | Andares | Vida | Início | Corrida | Maior promessa | Potencial | Obstáculos | Perda esperada (desvia / reto) | Defesas | Prêmio |
|---|---|---|---|---|---|---|---|---|---|---|---|
| LEVEL 1 | Keep | 1 | 110 | 10 | 6,5 m/s, 12,0 s | 80 | 160 | — | — | C3.0s/2 | 40 |
| LEVEL 2 | Keep | 1 | 250 | 10 | 6,5 m/s, 14,8 s | 160 | 480 | — | — | M60 C2.2s/4 | 50 |
| LEVEL 3 | Forge | 2 | 270 | 12 | 7,25 m/s, 13,7 s | 194 | 582 | T4,15 | 10% / 27% | C2.3s/4 V3.4s | 60 |
| LEVEL 4 | Keep | 2 | 300 | 12 | 7,25 m/s, 14,5 s | 198 | 522 | T3,7 | 6% / 24% | M70 C2.0s/4 | 70 |
| LEVEL 5 | Forge | 3 | 420 | 14 | 7,25 m/s, 15,5 s | 297 | 852 | Ma3,1 | 1% / 19% | C1.9s/5 V3.0s | 85 |
| LEVEL 6 | Keep | 3 | 430 | 15 | 7,75 m/s, 15,8 s | 283 | 735 | T3,5 Ma2,75 | 8% / 40% | M80 C1.8s/5 V3.0s | 100 |
| LEVEL 7 | Keep | 3 | 440 | 14 | 7,25 m/s, 17,0 s | 299 | 819 | Ma2,7 T3,65 | 14% / 41% | C1.85s/5 V3.0s | 105 |
| LEVEL 8 | Keep | 3 | 470 | 15 | 7,75 m/s, 16,3 s | 298 | 840 | T3,55 Ma2,7 | 4% / 31% | M90 C1.8s/5 | 115 |
| LEVEL 9 | Forge | 3 | 480 | 15 | 7,75 m/s, 16,3 s | 300 | 780 | Ma2,65 T3,5 | 13% / 37% | M80 C1.7s/6 V2.9s | 125 |
| BOSS | Boss (ShiftGates) | 4 | 490 | 16 | 7,75 m/s, 16,3 s | 298 | 798 | T3,2 Ma2,4 | 8% / 36% | M90 C1.6s/6 V2.8s | 200 |

### WORLD 2 (world2) — paleta PorDoSol, 116 árvores e 80 pedras, dano esperado x1.3, abre com 10 estrelas

| Fase | Torre | Andares | Vida | Início | Corrida | Maior promessa | Potencial | Obstáculos | Perda esperada (desvia / reto) | Defesas | Prêmio |
|---|---|---|---|---|---|---|---|---|---|---|---|
| LEVEL 1 | World2Tower | 1 | 100 | 14 | 7,25 m/s, 10,7 s | 82 | 164 | T3,7 | 7% / 29% | M20 C2.2s/4 | 110 |
| LEVEL 2 | World2Tower | 1 | 190 | 15 | 7,25 m/s, 10,4 s | 156 | 280 | S4,1 | 4% / 26% | C2.1s/4 V3.3s | 115 |
| LEVEL 3 | World2Tower | 1 | 350 | 15 | 7,25 m/s, 10,4 s | 198 | 540 | S3,1 | 8% / 29% | M60 C2.05s/5 E50/45% | 125 |
| LEVEL 4 | Forge | 2 | 440 | 16 | 7,75 m/s, 12,7 s | 256 | 684 | T3,5 | 12% / 28% | C1.95s/5 V3.2s | 130 |
| LEVEL 5 | World2Tower | 2 | 450 | 17 | 7,75 m/s, 13,5 s | 224 | 618 | S3,0 Ma2,6 | 3% / 38% | M80 C1.9s/5 V3.1s E60/50% | 135 |
| LEVEL 6 | World2Tower | 2 | 460 | 17 | 7,75 m/s, 13,6 s | 275 | 726 | T3,4 S2,95 | 17% / 48% | M80 C1.8s/6 V3.1s E60/50% | 145 |
| LEVEL 7 | World2Tower | 3 | 550 | 18 | 7,75 m/s, 16,3 s | 296 | 828 | Ma2,5 T3,3 | 14% / 42% | C1.75s/6 V3.0s E80/55% | 150 |
| LEVEL 8 | Forge | 3 | 570 | 19 | 8,5 m/s, 15,0 s | 300 | 834 | S2,85 Ma2,45 | 7% / 39% | M100 C1.65s/6 V2.9s E80/55% | 155 |
| LEVEL 9 | World2Tower | 3 | 580 | 19 | 8,5 m/s, 15,1 s | 296 | 828 | T3,2 S2,8 | 16% / 44% | M100 C1.6s/7 V2.9s E80/60% | 165 |
| BOSS | World2Boss (ShieldReturns) | 4 | 590 | 20 | 8,5 m/s, 15,0 s | 300 | 750 | Ma2,4 S2,8 | 11% / 42% | M110 C1.5s/7 V2.8s E80/60% | 350 |

### WORLD 3 (world3) — paleta Doce, 173 árvores e 49 pedras, dano esperado x1.5, abre com 25 estrelas

| Fase | Torre | Andares | Vida | Início | Corrida | Maior promessa | Potencial | Obstáculos | Perda esperada (desvia / reto) | Defesas | Prêmio |
|---|---|---|---|---|---|---|---|---|---|---|---|
| LEVEL 1 | World3Tower | 1 | 140 | 18 | 7,75 m/s, 10,1 s | 98 | 184 | T3,5 | 8% / 28% | M30 C1.9s/5 V3.1s | 170 |
| LEVEL 2 | World3Tower | 1 | 200 | 19 | 7,75 m/s, 10,1 s | 141 | 258 | B | 8% / 75% | M40 C1.85s/5 V3.0s | 180 |
| LEVEL 3 | World3Tower | 1 | 310 | 19 | 7,75 m/s, 10,1 s | 129 | 369 | B | 2% / 75% | C1.75s/6 | 185 |
| LEVEL 4 | World2Tower | 2 | 350 | 20 | 8,5 m/s, 11,7 s | 179 | 417 | B | 2% / 70% | M60 C1.7s/6 V2.9s | 195 |
| LEVEL 5 | World3Tower | 2 | 430 | 21 | 8,5 m/s, 12,9 s | 188 | 531 | Ma2,45 T3,3 | 10% / 33% | C1.65s/6 | 200 |
| LEVEL 6 | World3Tower | 2 | 440 | 21 | 8,5 m/s, 12,6 s | 184 | 498 | S2,8 Ma2,4 | 4% / 33% | M80 C1.55s/7 V2.8s E60/55% | 210 |
| LEVEL 7 | World3Tower | 3 | 490 | 22 | 8,5 m/s, 15,1 s | 224 | 594 | B S2,75 | 11% / 84% | C1.5s/7 V2.7s E70/55% | 215 |
| LEVEL 8 | World2Tower | 3 | 620 | 23 | 9 m/s, 14,8 s | 299 | 816 | T3,1 B | 19% / 69% | M110 C1.45s/7 V2.6s | 225 |
| LEVEL 9 | World3Tower | 3 | 630 | 23 | 9 m/s, 14,7 s | 297 | 726 | Ma2,3 T3,05 | 12% / 39% | M110 C1.35s/8 E90/60% | 230 |
| BOSS | World3Boss (Overheat) | 4 | 650 | 24 | 9 m/s, 13,5 s | 297 | 816 | S2,65 B T3,05 | 24% / 75% | M120 C1.3s/8 V2.5s E90/60% | 500 |

### WORLD 4 (world4) — paleta Aurora, 98 árvores e 98 pedras, dano esperado x1.75, abre com 45 estrelas

| Fase | Torre | Andares | Vida | Início | Corrida | Maior promessa | Potencial | Obstáculos | Perda esperada (desvia / reto) | Defesas | Prêmio |
|---|---|---|---|---|---|---|---|---|---|---|---|
| LEVEL 1 | World4Tower | 1 | 210 | 22 | 8,5 m/s, 10,5 s | 124 | 236 | Ma2,5 T3,35 | 8% / 39% | M40 C1.7s/6 V2.9s | 240 |
| LEVEL 2 | World4Tower | 1 | 250 | 23 | 8,5 m/s, 10,4 s | 126 | 246 | R3,45 | 0% / 62% | M40 C1.65s/6 V2.8s E40/50% | 250 |
| LEVEL 3 | World4Tower | 1 | 410 | 24 | 8,5 m/s, 10,4 s | 140 | 399 | R2,6 | 0% / 56% | C1.6s/7 E60/55% | 260 |
| LEVEL 4 | World3Tower | 2 | 540 | 25 | 9 m/s, 11,4 s | 188 | 525 | R2,55 B | 1% / 82% | C1.5s/7 E80/55% | 265 |
| LEVEL 5 | World4Tower | 2 | 630 | 26 | 9 m/s, 11,4 s | 228 | 624 | T3,05 R2,45 | 6% / 66% | C1.45s/7 E90/55% | 275 |
| LEVEL 6 | World4Tower | 2 | 640 | 26 | 9 m/s, 11,2 s | 266 | 732 | Ma2,2 T2,95 S2,6 | 22% / 59% | M110 C1.4s/8 E90/60% | 285 |
| LEVEL 7 | World4Tower | 3 | 660 | 27 | 9 m/s, 13,4 s | 294 | 756 | S2,5 Ma2,15 B | 23% / 87% | C1.35s/8 E90/60% | 295 |
| LEVEL 8 | World3Tower | 3 | 760 | 28 | 9,75 m/s, 13,8 s | 279 | 777 | B S2,45 R2,25 | 14% / 75% | C1.25s/8 V2.4s | 300 |
| LEVEL 9 | World4Tower | 3 | 770 | 29 | 9,75 m/s, 14,0 s | 300 | 819 | R2,2 B T2,7 | 21% / 91% | M140 C1.2s/9 E110/65% | 310 |
| BOSS | World4Boss (TurnGate) | 4 | 830 | 30 | 9,75 m/s, 13,7 s | 296 | 810 | S2,4 R2,2 T2,7 | 19% / 62% | M150 C1.15s/9 V2.3s E120/65% | 700 |
