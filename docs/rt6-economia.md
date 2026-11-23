# RT-6 — Economia simulada e fases (gerado por `EconomyReport`, não editar à mão)

Preços e curvas **[P]** do catálogo (`WorldContent.WriteCatalog`), jogados por um jogador simulado com as
regras reais (`Progression`). Premissas **[I]** (sem analytics ainda, D4):

- corrida = pista até a linha de tiro ÷ velocidade da fase (ritmo x1,3, RT-6t.15; média 11,7 s por fase jogada); depois dela, assalto + resultado em 39 s na vitória e 19 s na derrota;
- 80% de vitórias; 60 unidades na linha de tiro, 40% sobrevivem (2 estrelas);
- compra o mais barato que puder, upgrades antes de elites e skins; joga os 4 mundos em ordem (40 fases)
  e, depois do último, fica nas voltas dele (cada volta 25% mais forte e 25% mais rica).

Média: **399 moedas por minuto** de jogo.

| Mundo | Primeira passada termina em | Fases jogadas | Moedas ganhas até ali |
|---|---|---|---|
| world1 | 10,5 min | 13 | 1693 |
| world2 | 20,1 min | 25 | 4333 |
| world3 | 30,0 min | 38 | 8110 |
| world4 | 39,2 min | 50 | 13186 |

| # | Compra | Preço | Fases jogadas | Minutos de jogo | Volta do mundo |
|---|---|---|---|---|---|
| 1 | ARMY 1 | 60 | 2 | 1,4 | 0 |
| 2 | LAUNCH SPEED 1 | 70 | 3 | 2,3 | 0 |
| 3 | DAMAGE 1 | 80 | 4 | 3,1 | 0 |
| 4 | GATE BOOST 1 | 100 | 5 | 4,0 | 0 |
| 5 | ARMY 2 | 120 | 5 | 4,0 | 0 |
| 6 | LAUNCH SPEED 2 | 130 | 7 | 5,5 | 0 |
| 7 | DAMAGE 2 | 150 | 8 | 6,3 | 0 |
| 8 | GATE BOOST 2 | 180 | 9 | 7,3 | 0 |
| 9 | ARMY 3 | 200 | 10 | 8,2 | 0 |
| 10 | LAUNCH SPEED 3 | 220 | 12 | 9,6 | 0 |
| 11 | DAMAGE 3 | 250 | 13 | 10,5 | 0 |
| 12 | GATE BOOST 3 | 300 | 14 | 11,3 | 0 |
| 13 | ARMY 4 | 320 | 17 | 13,4 | 0 |
| 14 | LAUNCH SPEED 4 | 350 | 19 | 15,2 | 0 |
| 15 | DAMAGE 4 | 400 | 20 | 16,0 | 0 |
| 16 | GATE BOOST 4 | 450 | 23 | 18,3 | 0 |
| 17 | ARMY 5 | 480 | 25 | 20,1 | 0 |
| 18 | LAUNCH SPEED 5 | 520 | 27 | 21,4 | 0 |
| 19 | DAMAGE 5 | 600 | 29 | 23,0 | 0 |
| 20 | GATE BOOST 5 | 650 | 32 | 25,2 | 0 |
| 21 | ARMY 6 | 700 | 34 | 26,9 | 0 |
| 22 | LAUNCH SPEED 6 | 750 | 37 | 29,1 | 0 |
| 23 | DAMAGE 6 | 900 | 38 | 30,0 | 0 |
| 24 | GATE BOOST 6 | 950 | 42 | 32,8 | 0 |
| 25 | SPRINTER | 600 | 43 | 33,6 | 0 |
| 26 | WARDEN | 900 | 45 | 35,3 | 0 |
| 27 | skin CORAL | 150 | 45 | 35,3 | 0 |
| 28 | skin MANGO | 300 | 47 | 36,7 | 0 |
| 29 | skin SKY | 500 | 48 | 37,5 | 0 |
| 30 | skin LILAC | 800 | 50 | 39,2 | 1 |
| 31 | skin INDIGO | 1200 | 52 | 40,5 | 1 |
| 32 | skin LAGOON | 1500 | 55 | 42,9 | 1 |
| 33 | skin BERRY | 2000 | 59 | 45,9 | 1 |
| 34 | skin BUBBLEGUM | 2500 | 63 | 48,9 | 2 |

Total: **19380 moedas**, tudo comprado em **49 minutos** (63 fases).

## Sensibilidade

- Primeira passada do world1 (10 fases, sem comprar): **1663 moedas** = 9% da loja (19380).
- Primeira passada do world2 (10 fases, sem comprar): **2607 moedas** = 13% da loja (19380).
- Primeira passada do world3 (10 fases, sem comprar): **3703 moedas** = 19% da loja (19380).
- Primeira passada do world4 (10 fases, sem comprar): **5008 moedas** = 26% da loja (19380).
- Preços x0,75: tudo comprado em **42 minutos** (54 fases, volta 1).
- Preços x1,5: tudo comprado em **61 minutos** (79 fases, volta 3).

Vieses conhecidos: a taxa de vitória e os sobreviventes não mudam com os upgrades nem com as voltas mais
fortes, e a renda futura de anúncio recompensado e recompensa diária (RT-7) não entra.

## Fases (dados gerados; vida antes das voltas)

Potencial = exército no melhor caminho dos portões (teto 300) × melhor portão da fileira. Maior promessa = maior número que
um portão da corrida pode mostrar em qualquer caminho, com as caixas (nunca passa de 300, RT-6t.14). Defesas: M muralha,
C canhão (intervalo, pedras), V varredor, E escudo (parte do dano que segura).

### WORLD 1 (world1) — paleta Pastel, 144 árvores e 64 pedras, dano esperado x1, abre com 0 estrelas

| Fase | Torre | Andares | Vida | Início | Corrida | Maior promessa | Potencial | Defesas | Prêmio |
|---|---|---|---|---|---|---|---|---|---|
| LEVEL 1 | Keep | 1 | 110 | 10 | 6,5 m/s, 12,0 s | 80 | 160 | C3.0s/2 | 40 |
| LEVEL 2 | Keep | 1 | 250 | 10 | 6,5 m/s, 14,8 s | 160 | 480 | M60 C2.2s/4 | 50 |
| LEVEL 3 | Forge | 2 | 300 | 12 | 7,25 m/s, 13,5 s | 194 | 582 | C2.3s/4 V3.4s | 60 |
| LEVEL 4 | Keep | 2 | 320 | 12 | 7,25 m/s, 13,8 s | 198 | 522 | M70 C2.0s/4 | 70 |
| LEVEL 5 | Forge | 3 | 420 | 14 | 7,25 m/s, 14,3 s | 297 | 852 | C1.9s/5 V3.0s | 85 |
| LEVEL 6 | Keep | 3 | 470 | 15 | 7,75 m/s, 13,9 s | 283 | 735 | M90 C1.8s/5 V3.0s | 100 |
| LEVEL 7 | Keep | 3 | 480 | 14 | 7,25 m/s, 15,7 s | 300 | 762 | M90 C1.85s/5 V3.0s | 105 |
| LEVEL 8 | Keep | 3 | 490 | 15 | 7,75 m/s, 14,7 s | 300 | 801 | M90 C1.8s/5 V2.9s | 115 |
| LEVEL 9 | Forge | 3 | 500 | 15 | 7,75 m/s, 14,7 s | 300 | 780 | M90 C1.7s/6 V2.9s | 125 |
| BOSS | Boss (ShiftGates) | 4 | 530 | 16 | 7,75 m/s, 14,5 s | 298 | 798 | M100 C1.6s/6 V2.8s | 200 |

### WORLD 2 (world2) — paleta PorDoSol, 116 árvores e 80 pedras, dano esperado x1.3, abre com 10 estrelas

| Fase | Torre | Andares | Vida | Início | Corrida | Maior promessa | Potencial | Defesas | Prêmio |
|---|---|---|---|---|---|---|---|---|---|
| LEVEL 1 | World2Tower | 1 | 110 | 14 | 7,25 m/s, 10,2 s | 82 | 164 | M20 C2.2s/4 | 110 |
| LEVEL 2 | World2Tower | 1 | 220 | 15 | 7,25 m/s, 10,2 s | 166 | 320 | M40 C2.1s/4 | 115 |
| LEVEL 3 | World2Tower | 1 | 380 | 15 | 7,25 m/s, 10,2 s | 198 | 540 | M70 C2.05s/5 E50/45% | 125 |
| LEVEL 4 | Forge | 2 | 500 | 16 | 7,75 m/s, 12,1 s | 256 | 684 | C1.95s/5 V3.2s | 130 |
| LEVEL 5 | World2Tower | 2 | 540 | 17 | 7,75 m/s, 12,1 s | 257 | 729 | M100 C1.9s/5 V3.1s | 135 |
| LEVEL 6 | World2Tower | 2 | 550 | 17 | 7,75 m/s, 12,1 s | 275 | 726 | M100 C1.8s/6 V3.1s E80/50% | 145 |
| LEVEL 7 | World2Tower | 3 | 630 | 18 | 7,75 m/s, 14,7 s | 296 | 828 | C1.75s/6 V3.0s E90/55% | 150 |
| LEVEL 8 | Forge | 3 | 640 | 19 | 8,5 m/s, 13,4 s | 300 | 834 | M120 C1.65s/6 V2.9s E90/55% | 155 |
| LEVEL 9 | World2Tower | 3 | 650 | 19 | 8,5 m/s, 13,4 s | 297 | 849 | M110 C1.6s/7 V2.9s E90/60% | 165 |
| BOSS | World2Boss (ShieldReturns) | 4 | 660 | 20 | 8,5 m/s, 13,4 s | 300 | 750 | M120 C1.5s/7 V2.8s E90/60% | 350 |

### WORLD 3 (world3) — paleta Doce, 173 árvores e 49 pedras, dano esperado x1.5, abre com 25 estrelas

| Fase | Torre | Andares | Vida | Início | Corrida | Maior promessa | Potencial | Defesas | Prêmio |
|---|---|---|---|---|---|---|---|---|---|
| LEVEL 1 | World3Tower | 1 | 180 | 18 | 7,75 m/s, 9,5 s | 120 | 228 | M30 C1.9s/5 V3.1s | 170 |
| LEVEL 2 | World3Tower | 1 | 250 | 19 | 7,75 m/s, 9,5 s | 168 | 304 | C1.85s/5 | 180 |
| LEVEL 3 | World3Tower | 1 | 330 | 19 | 7,75 m/s, 9,5 s | 141 | 387 | M60 C1.75s/6 | 185 |
| LEVEL 4 | World2Tower | 2 | 400 | 20 | 8,5 m/s, 11,1 s | 180 | 465 | C1.7s/6 E60/50% | 195 |
| LEVEL 5 | World3Tower | 2 | 490 | 21 | 8,5 m/s, 11,1 s | 202 | 546 | M90 C1.65s/6 | 200 |
| LEVEL 6 | World3Tower | 2 | 720 | 21 | 8,5 m/s, 11,1 s | 300 | 789 | M130 C1.55s/7 V2.8s E100/55% | 210 |
| LEVEL 7 | World3Tower | 3 | 740 | 22 | 8,5 m/s, 13,4 s | 298 | 795 | M130 C1.5s/7 | 215 |
| LEVEL 8 | World2Tower | 3 | 810 | 23 | 9 m/s, 12,7 s | 300 | 855 | M150 C1.45s/7 | 225 |
| LEVEL 9 | World3Tower | 3 | 820 | 23 | 9 m/s, 12,7 s | 299 | 834 | M150 C1.35s/8 V2.6s E110/60% | 230 |
| BOSS | World3Boss (Overheat) | 4 | 850 | 24 | 9 m/s, 12,7 s | 299 | 813 | M150 C1.3s/8 V2.5s E120/60% | 500 |

### WORLD 4 (world4) — paleta Aurora, 98 árvores e 98 pedras, dano esperado x1.75, abre com 45 estrelas

| Fase | Torre | Andares | Vida | Início | Corrida | Maior promessa | Potencial | Defesas | Prêmio |
|---|---|---|---|---|---|---|---|---|---|
| LEVEL 1 | World4Tower | 1 | 230 | 22 | 8,5 m/s, 8,7 s | 124 | 236 | M40 C1.7s/6 V2.9s | 240 |
| LEVEL 2 | World4Tower | 1 | 310 | 23 | 8,5 m/s, 8,7 s | 160 | 312 | M60 C1.65s/6 E40/50% | 250 |
| LEVEL 3 | World4Tower | 1 | 410 | 24 | 8,5 m/s, 8,7 s | 140 | 399 | C1.6s/7 E60/55% | 260 |
| LEVEL 4 | World3Tower | 2 | 630 | 25 | 9 m/s, 10,4 s | 212 | 600 | M110 C1.5s/7 V2.7s E90/55% | 265 |
| LEVEL 5 | World4Tower | 2 | 650 | 26 | 9 m/s, 10,4 s | 244 | 606 | M120 C1.45s/7 V2.6s E90/55% | 275 |
| LEVEL 6 | World4Tower | 2 | 750 | 26 | 9 m/s, 10,4 s | 261 | 684 | M140 C1.4s/8 V2.6s E100/60% | 285 |
| LEVEL 7 | World4Tower | 3 | 770 | 27 | 9 m/s, 12,7 s | 249 | 684 | M140 C1.35s/8 V2.5s E110/60% | 295 |
| LEVEL 8 | World3Tower | 3 | 840 | 28 | 9,75 m/s, 11,7 s | 266 | 738 | M150 C1.25s/8 V2.4s | 300 |
| LEVEL 9 | World4Tower | 3 | 890 | 29 | 9,75 m/s, 11,7 s | 265 | 762 | M160 C1.2s/9 V2.4s | 310 |
| BOSS | World4Boss (TurnGate) | 4 | 1070 | 30 | 9,75 m/s, 11,7 s | 300 | 846 | M190 C1.15s/9 V2.3s E150/65% | 700 |
