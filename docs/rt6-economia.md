# RT-6 — Economia simulada e fases (gerado por `EconomyReport`, não editar à mão)

Preços e curvas **[P]** do catálogo (`WorldContent.WriteCatalog`), jogados por um jogador simulado com as
regras reais (`Progression`). Premissas **[I]** (sem analytics ainda, D4):

- vitória em 55 s (corrida + assalto + resultado); derrota em 35 s;
- 80% de vitórias; 60 unidades na linha de tiro, 40% sobrevivem (2 estrelas);
- compra o mais barato que puder, upgrades antes de elites e skins; joga os 1 mundos em ordem (10 fases)
  e, depois do último, fica nas voltas dele (cada volta 25% mais forte e 25% mais rica).

Média: **229 moedas por minuto** de jogo.

| Mundo | Primeira passada termina em | Fases jogadas | Moedas ganhas até ali |
|---|---|---|---|
| world1 | 10,9 min | 13 | 1693 |

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
| 11 | DAMAGE 3 | 250 | 13 | 10,9 | 1 |
| 12 | GATE BOOST 3 | 300 | 15 | 12,8 | 1 |
| 13 | ARMY 4 | 320 | 18 | 15,2 | 1 |
| 14 | LAUNCH SPEED 4 | 350 | 20 | 17,0 | 1 |
| 15 | DAMAGE 4 | 400 | 23 | 19,4 | 1 |
| 16 | GATE BOOST 4 | 450 | 25 | 21,3 | 2 |
| 17 | ARMY 5 | 480 | 28 | 23,7 | 2 |
| 18 | LAUNCH SPEED 5 | 520 | 32 | 27,0 | 2 |
| 19 | DAMAGE 5 | 600 | 34 | 28,8 | 2 |
| 20 | GATE BOOST 5 | 650 | 37 | 31,3 | 2 |
| 21 | ARMY 6 | 700 | 40 | 34,0 | 3 |
| 22 | LAUNCH SPEED 6 | 750 | 44 | 37,3 | 3 |
| 23 | DAMAGE 6 | 900 | 48 | 40,7 | 3 |
| 24 | GATE BOOST 6 | 950 | 51 | 43,1 | 4 |
| 25 | SPRINTER | 600 | 55 | 46,8 | 4 |
| 26 | WARDEN | 900 | 59 | 50,1 | 4 |
| 27 | skin EMBER | 150 | 59 | 50,1 | 4 |
| 28 | skin MOSS | 300 | 60 | 51,0 | 4 |
| 29 | skin SLATE | 500 | 62 | 52,5 | 4 |
| 30 | skin BONE | 800 | 64 | 54,3 | 5 |
| 31 | skin INK | 1200 | 70 | 59,5 | 5 |

Total: **13380 moedas**, tudo comprado em **60 minutos** (70 fases).

## Sensibilidade

- Primeira passada do world1 (10 fases, sem comprar): **1663 moedas** = 12% da loja (13380).
- Preços x0,75: tudo comprado em **49 minutos** (58 fases, volta 4).
- Preços x1,5: tudo comprado em **78 minutos** (92 fases, volta 7).

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
| LEVEL 7 | Keep | 3 | 520 | 14 | 900 | C1.85s/5 V3.0s | 105 |
| LEVEL 8 | Keep | 3 | 530 | 15 | 900 | M100 C1.8s/5 V2.9s | 115 |
| LEVEL 9 | Forge | 3 | 550 | 15 | 900 | M100 C1.7s/6 V2.9s | 125 |
| BOSS | Boss (ShiftGates) | 4 | 600 | 16 | 900 | M100 C1.6s/6 V2.8s | 200 |
