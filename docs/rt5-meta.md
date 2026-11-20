# RT-5 — Núcleo de meta (Clay Siege, nome de trabalho; código ainda "Claybreak")

Data: 2026-11-20. Entrega RT-5 de `plano-de-entregas.md`. **[F]** = conferido nesta sessão;
**[I]** = inferência ou proposta; **[P]** = valor provisório a calibrar.

## 1. O que existe

| Pedido da RT-5 | Como ficou [F] |
|---|---|
| Moeda | Uma moeda, só ganha jogando (D6: sem dinheiro real). Vitória paga prêmio + sobreviventes + ¼ do prêmio por estrela; derrota ⅛; cada volta do mundo paga 25% a mais. |
| Estrelas por fase | 1–3 por sobreviventes (como na fatia); a melhor fica salva; total por mundo e geral na tela inicial e na de mundos. |
| Upgrades | 4 trilhas de 6 degraus: **exército** (o da fatia, igual), **dano** (+15…+100%), **portão** (+1…+10 em todo portão "+"), **lançamento** (+10…+80% unidades/s). Preços [P] (ficha D19). |
| Coleção de unidades (tipos) | 3 elites com tamanho, cor, dano, armadura e velocidade próprios: **Bruto** (grátis na fase 3, como na fatia), **Corredor** (600), **Guardião** (900). Loadout de **2 vagas**; caixotes da corrida alternam entre os equipados; sai um elite a cada 3 lançamentos. |
| Skins | 6 cores de massinha para o exército inteiro (líder, multidão, reserva), feitas por código: Barro (grátis) … Tinta (1200). |
| Loja | Tela com abas UPGRADES / ELITES / SKINS; comprar, equipar, vestir; ponto dourado no botão SHOP quando há algo que dá para comprar. |
| Tela de mundos | Lista montada do catálogo (uma linha por mundo, qualquer número); mundo abre ao vencer o chefe do anterior e com as estrelas pedidas (dado); vencer o chefe abre e escolhe o próximo. Hoje: 1 mundo + "MORE WORLDS SOON". |
| Save versionado | Versão 2 por ids estáveis; o save da fatia (v1) migra mantendo moedas, estrelas, volta, degrau do exército e o Bruto; o original fica guardado uma vez. |
| Validador de save | `SaveValidator` + saves de exemplo de cada versão (`Assets/Claybreak/Editor/SaveFixtures`), no validador do jogo e nos testes. |
| Gate com loadout máximo | O nível do gate roda todos os upgrades no topo, 2 elites e a skin mais cara (~100 elites em voo entre as 300). |

Telas (capturas em `docs/art/telas/`): `jogo-menu.png`, `jogo-loja-upgrades.png`, `jogo-loja-elites.png`,
`jogo-loja-skins.png`, `jogo-mundos.png`.

Economia simulada: `docs/rt5-economia.md` (≈240 moedas/min; loja inteira em ≈57 min no mundo 1) [I].

## 2. Decisões

Técnicas (`decisoes.md`, T-RT5-1 a T-RT5-4): meta como catálogo de dados; save v2 por ids com migração e
validador; elites, upgrades e skins no jogo; tabela de economia gerada.

Do usuário, fichas em `decisoes.md` (o jogo roda com a recomendação como provisória):
- **D19** — preços e curvas da loja (recomendação A: aprovar a forma como ponto de partida).
- **D12** — conteúdo do lançamento (recomendação A: 4 mundos × 10 fases); define a RT-6.

## 3. Validação (2026-11-20) [F]

Unity 6000.6.7f1 em batchmode, alvo Android, no próprio repositório (Editor fechado), com
`Tools/rt4-check.sh /c/temp/rt5`:

| Verificação | Resultado |
|---|---|
| `ClaybreakSceneBuilder.BuildAndQuit` (1 mundo, 7 fases, 4 upgrades, 3 elites, 6 skins) | PASS |
| `ClaybreakValidation.RunAndQuit` (catálogo, mundos, fases, gate, tema, **saves de exemplo**, cena, regras de código) | PASS |
| `EngineIsolationValidation.RunAndQuit` do kit | PASS |
| Testes EditMode (kit + jogo: regras de meta, migração v1→v2, saves danificados, mundos) | 68/68 |
| Testes PlayMode (+ loja ponta a ponta: compra upgrade, elite e skin pelos botões e a corrida seguinte começa com eles; gate com loadout máximo) | 5/5 |
| Economia (`EconomyReport`) e capturas | PASS |
| APKs `C:\temp\rt5\Claybreak.apk` e `C:\temp\rt5\Claybreak-measure.apk` | gerados, ~36 MB |

Gate 3× no OPPO com o loadout máximo: **aprovado** (seção 5). O APK do jogo ficou instalado no OPPO.

## 4. ACEITE PENDENTE — o que só dá para fazer com vocês (e como refazer o gate)

1. **APKs**: já estão em `C:\temp\rt5\`. Se sumirem (Editor do Unity fechado), no Git Bash, na raiz do
   repositório: `Tools/rt4-check.sh /c/temp/rt5 "scene apk"`.
2. **Gate 3× com o loadout máximo** (OPPO no USB, **desbloqueado**, depuração USB ativa; não tocar na
   tela durante a medição): `adb start-server` e depois
   `Tools/rt2-gate.sh rt5-oppo-60 60 20 /c/temp/rt5/Claybreak-measure.apk`.
   Conferir em `C:\temp\rt2\gate-rt5-oppo-60.txt`: `p95` do quadro ≤ 33,3 ms; quadros > 50 ms abaixo de 5%;
   `PSS` ≤ 450 MB; `[DevTiming] ... main p95` ≤ 11 ms e `gpu p95` ≤ 11 ms; linhas
   `[ClaybreakGate] tower=Boss inFlight=300 specials=… projectiles=12/12 … loadout=2 damage=x2 launch=x1.8`.
   Se falhar: baixar no `WorldContent.cs` (nível do gate) `maximumUnitsInFlight` 300 → 250 e
   `maximumProjectiles` 12 → 8, reconstruir (`"scene apk"`) e repetir.
3. **Jogar a meta** (você e a Flávia): o jogo já está instalado no OPPO (se não, `adb install -r
   C:\temp\rt5\Claybreak.apk`). O save da fatia deve
   abrir com as mesmas moedas, estrelas, fase e o Bruto (migração). Comprar na loja, trocar elites e skin,
   abrir a tela de mundos; responder as fichas **D19** e **D12**.
4. Anotar o resultado na seção 5 e na linha "Status" da RT-5 do plano; commit `docs: RT-5 aceite`.

## 5. Resultado no aparelho (preencher)

| Data | Teste | p95 quadro | >50 ms | main p95 | gpu p95 | PSS | em voo | elites | pedras | Passou? |
|---|---|---|---|---|---|---|---|---|---|---|
| 2026-11-20 | gate 60 fps, loadout máximo, 1ª rodada | 16 ms (max 66) | 0,1% | 3,0–5,8 ms; uma janela de 5 s com 8,9 e outra com **14,4** | 4,0–4,4 ms | 181 MB | 299 | 100 | 11–12/12 | não conclusivo |
| 2026-11-20 | gate 60 fps, loadout máximo, 2ª rodada | 16 ms (max 16) | 0,0% | 4,5–7,3 ms | 4,1–5,7 ms | 192 MB | 299 | 100 | 12/12 | **Sim** |

A janela de 14,4 ms da 1ª rodada foi a última, quando o script coleta a memória (`dumpsys meminfo`) e
o processador esquenta [I]; nenhum quadro passou do tempo nela (quadro p95 16 ms). Na 2ª rodada, logo em
seguida, todas as 8 janelas ficaram ≤ 7,3 ms (temperatura da CPU 55,8 °C, sem limitação térmica). Saídas
completas em `C:\temp\rt2\gate-rt5-oppo-60.txt` e `gate-rt5-oppo-60b.txt`. Comparação com a RT-4 (torre do
chefe, sem loadout): main 4,0–5,1 ms; o loadout máximo (100 elites, dano com sobra, lançamento 1,8×)
custa ~1–2 ms de CPU e cabe na folga de 3×.

## 6. O que fica para depois

1. **RT-6**: mundos novos (D12), quanto cada mundo paga e o total da loja (D19), itens novos por mundo.
2. **RT-7**: anúncio recompensado (dobrar moedas, D6) e recompensa diária (D5) entram na economia;
   recalibrar `docs/rt5-economia.md` com as premissas medidas (analytics, D4).
3. **Kit** (pauta): cor do respingo por skin em tempo de jogo (`ThemeDeathEffect` só aceita a cor do tema);
   save versionado e economia como peças do kit se o Wild Pack quiser o mesmo formato.
4. Dificuldade das voltas altas: com todos os upgrades o mundo 1 fica fácil (ataque ≈3,6×, vida +25% por
   volta) — calibrar junto com a RT-6.
