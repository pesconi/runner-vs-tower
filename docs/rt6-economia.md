# RT-6 — Economia simulada e fases (gerado por `EconomyReport`, não editar à mão)

Preços e curvas **[P]** do catálogo (`WorldContent.WriteCatalog`), jogados por um jogador simulado com as
regras reais (`Progression`). Premissas **[I]** (sem analytics ainda, D4):

- vitória em 55 s (corrida + assalto + resultado); derrota em 35 s;
- 80% de vitórias; 60 unidades na linha de tiro, 40% sobrevivem (2 estrelas);
- compra o mais barato que puder, upgrades antes de elites e skins; joga os 2 mundos em ordem (20 fases)
  e, depois do último, fica nas voltas dele (cada volta 25% mais forte e 25% mais rica).

Média: **277 moedas por minuto** de jogo.

| Mundo | Primeira passada termina em | Fases jogadas | Moedas ganhas até ali |
|---|---|---|---|
| world1 | 10,9 min | 13 | 1693 |
| world2 | 21,3 min | 25 | 4333 |

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
| 17 | ARMY 5 | 480 | 25 | 21,3 | 1 |
| 18 | LAUNCH SPEED 5 | 520 | 27 | 22,8 | 1 |
| 19 | DAMAGE 5 | 600 | 29 | 24,6 | 1 |
| 20 | GATE BOOST 5 | 650 | 32 | 27,0 | 1 |
| 21 | ARMY 6 | 700 | 35 | 29,8 | 1 |
| 22 | LAUNCH SPEED 6 | 750 | 38 | 32,2 | 2 |
| 23 | DAMAGE 6 | 900 | 40 | 34,0 | 2 |
| 24 | GATE BOOST 6 | 950 | 44 | 37,3 | 2 |
| 25 | SPRINTER | 600 | 47 | 39,8 | 2 |
| 26 | WARDEN | 900 | 49 | 41,6 | 2 |
| 27 | skin EMBER | 150 | 49 | 41,6 | 2 |
| 28 | skin MOSS | 300 | 50 | 42,5 | 3 |
| 29 | skin SLATE | 500 | 50 | 42,5 | 3 |
| 30 | skin BONE | 800 | 54 | 45,8 | 3 |
| 31 | skin INK | 1200 | 58 | 49,2 | 3 |

Total: **13380 moedas**, tudo comprado em **49 minutos** (58 fases).

## Sensibilidade

- Primeira passada do world1 (10 fases, sem comprar): **1663 moedas** = 12% da loja (13380).
- Primeira passada do world2 (10 fases, sem comprar): **2607 moedas** = 19% da loja (13380).
- Preços x0,75: tudo comprado em **41 minutos** (48 fases, volta 2).
- Preços x1,5: tudo comprado em **64 minutos** (75 fases, volta 5).

Vieses conhecidos: a taxa de vitória e os sobreviventes não mudam com os upgrades nem com as voltas mais
fortes, e a renda futura de anúncio recompensado e recompensa diária (RT-7) não entra.

## Fases (dados gerados; vida antes das voltas)

Potencial = exército no melhor caminho dos portões (teto 300) × melhor portão da fileira. Defesas: M muralha,
C canhão (intervalo, pedras), V varredor, E escudo (parte do dano que segura).

### WORLD 1 (world1) — cenário Marches, dano esperado x1, abre com 0 estrelas

| Fase | Torre | Andares | Vida | Início | Potencial | Defesas | Prêmio |
|---|---|---|---|---|---|---|---|
| LEVEL 1 | Keep | 1 | 110 | 10 | 160 | C3.0s/2 | 40 |
| LEVEL 2 | Keep | 1 | 250 | 10 | 480 | M60 C2.2s/4 | 50 |
| LEVEL 3 | Forge | 2 | 300 | 12 | 582 | C2.3s/4 V3.4s | 60 |
| LEVEL 4 | Keep | 2 | 320 | 12 | 522 | M70 C2.0s/4 | 70 |
| LEVEL 5 | Forge | 3 | 440 | 14 | 894 | C1.9s/5 V3.0s | 85 |
| LEVEL 6 | Keep | 3 | 470 | 15 | 735 | M90 C1.8s/5 V3.0s | 100 |
| LEVEL 7 | Keep | 3 | 480 | 14 | 825 | M90 C1.85s/5 V3.0s | 105 |
| LEVEL 8 | Keep | 3 | 530 | 15 | 900 | M100 C1.8s/5 V2.9s | 115 |
| LEVEL 9 | Forge | 3 | 550 | 15 | 900 | M100 C1.7s/6 V2.9s | 125 |
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
