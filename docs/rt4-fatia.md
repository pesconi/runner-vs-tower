# RT-4 — Fatia vertical (Clay Siege, nome de trabalho; código ainda "Claybreak")

Data: 2026-11-20. Entrega RT-4 de `plano-de-entregas.md`. **[F]** = conferido nesta sessão;
**[I]** = inferência ou proposta.

## 1. O que existe

Um mundo completo numa cena só (`Assets/Claybreak/Content/Claybreak.unity`), construído por código
(`ClaybreakSceneBuilder`), com tudo pré-alocado (regra 2.2.1). Reuso só pelo kit `studiokit` (pool,
multidão, líder, câmera, bonecos de massinha, som, vibração, peças de UI); nada copiado do Wild Pack.

| Pedido da RT-4 | Como ficou [F] |
|---|---|
| 5–8 fases | **7 fases** (`WorldContent.cs`): 6 comuns + chefe. Corridas de 15–19 s, 3–5 pares de portões. |
| 2–3 torres | **3 corpos**: bastião (fases 1, 2, 4, 6), forja (3, 5), cidadela do chefe (7). Torre com **andares** (1–4): cada andar que cai derruba um bloco de cima. |
| 3 defesas | **Muralha** (bloqueia uma faixa), **canhão** (pedra em arco, sombra avisa), **varredor** (poste de espetos que corre de um lado ao outro numa linha; acerta quem passa por baixo). |
| 1 chefe | Fase 7: 4 andares, as três defesas, e a regra do GDD 4.4: **a cada andar que cai, a fileira de portões gira uma faixa** (o portão bom muda de lugar). |
| Menu | Tela inicial: moedas, progresso do mundo (pontos por fase, chefe em vermelho), JOGAR, cartão do upgrade, cartão da unidade especial, chaves de som, música e vibração (preferências do kit). |
| HUD | Fase, exército, Brutos, moedas, barra de vida da torre no assalto, dica e faixa de aviso ("FLOOR DOWN!", "BOSS!"). |
| Resultado | Estrelas (por sobreviventes), moedas ganhas, o que destravou; **um toque** para seguir (NEXT/BOSS depois de vitória, AGAIN depois de derrota) e HOME. |
| Som e vibração | Papéis do kit (portões, perda, vitória, derrota, botões, música de menu e de corrida) + sons próprios: tiro de canhão, andar caindo, torre caindo, muralha, varredor, caixote, chefe. Todos sintetizados por código (sem licença a controlar). Vibração no golpe, andar, muralha, chefe e queda da torre, pela chave do kit. |
| Arte final de 1 torre e 1 chefe | Opções A/B/C de cada um em `docs/art/rt4/` — **aguardando a sua escolha (ficha D17)**. O jogo usa a recomendada (torre A, chefe A) como provisória. |
| 1 upgrade ponta a ponta | **Exército inicial**: 6 degraus (+5 … +46 unidades), comprado no menu com moedas, salvo, aplicado no início de toda fase. |
| 1 unidade especial ponta a ponta | **Bruto**: destrava ao vencer a fase 3 (aviso no resultado e no cartão do menu); aparece em caixotes na corrida (passar do lado do caixote); sai a cada 3º lançamento; maior, vermelho-escuro, 4× de dano, aguenta 1 golpe. |
| Save | JSON versionado no aparelho: fase atual, volta do mundo, moedas, degrau do upgrade, Bruto, estrelas, vitórias/derrotas. Save danificado começa do zero sem travar. |
| Sessão de 5 minutos [I] | 7 fases × ~35–50 s + menus ≈ 5–6 min na primeira passada; depois do chefe o mundo recomeça 25% mais forte por volta. **Confirmar jogando.** |

## 2. Decisões desta entrega

Técnicas, registradas em `decisoes.md` (T-RT4-1 a T-RT4-6): cena única com conteúdo como código; telas
escritas no jogo com as peças de UI do kit; sons próprios fora dos papéis do kit; regras do Bruto,
upgrade e save; nível do gate com a torre mais pesada; troca de nome só no que aparece.

Do usuário (respondidas em 2026-11-20, `decisoes.md`):
- **D17** — torre A (bastião de pedra clara) e chefe A (cidadela): é a arte final da fatia.
- **D18** — nome de trabalho **"Clay Siege"**. Checagem gratuita sem conflito no que abriu; INPI, USPTO,
  TMview e WIPO ficam para conferir à mão antes da RT-9. O app instalado se chama "Clay Siege" e o menu
  mostra "CLAY SIEGE". O ID do app e os nomes internos (`Claybreak`) não mudam (T-RT4-6).

Ajuste de plano: o aceite "os mesmos 5 + 2 novos" virou "o usuário e a Flávia jogam a fatia no OPPO e
respondem 'voltaria amanhã?'" (RT-3 dispensada, D13).

## 3. Pendência da RT-2: o teste "120 fps"

No gate da RT-2 o pedido de 120 fps rodou a ~62 fps. Conferido nesta sessão [F]:
- o OPPO suporta 120 Hz (modos 120/90/60 no `dumpsys display`; `peak_refresh_rate=120`);
- o jogo é feito para **60 fps**: o `FrameRateGovernor` do kit fixa 60 (com queda para 30 se não
  aguentar). O `--ei fps 120` só muda o alvo do Unity; a tela continua no modo que o sistema escolhe para o
  app, e o ColorOS costuma limitar jogos fora da sua lista a 60 Hz [I, não verificado no aparelho];
- a folga de 3× vale para 60 fps (CPU e GPU p95 ≤ 6,2 ms contra 11 ms), e esses tempos caberiam até no
  quadro de 8,3 ms de 120 Hz.

Conclusão: **não é defeito nem meta do produto** (60 fps é o alvo do estúdio). Fica registrado; se um dia
quisermos 120 Hz, é trabalho do kit (pedir o modo de tela ao Android) com o gate refeito.

## 4. ACEITE PENDENTE — o que só dá para fazer com o OPPO e com vocês

1. **Gerar os APKs** (Editor do Unity fechado), no Git Bash, na raiz do repositório:
   `Tools/rt4-check.sh /c/temp/rt4 apk` → `C:\temp\rt4\Claybreak.apk` (jogo) e
   `C:\temp\rt4\Claybreak-measure.apk` (medição). Se já existirem, pule.
2. **Gate 3× com a torre mais pesada** (OPPO ligado por USB, desbloqueado, depuração USB ativa; não
   toque na tela durante a medição): `Tools/rt2-gate.sh rt4-oppo-60 60 20 /c/temp/rt4/Claybreak-measure.apk`.
   O app abre sozinho no nível do gate (corpo do chefe, muralha, varredor, canhão com 12 pedras, 300 em
   voo). Conferir na saída (`C:\temp\rt2\gate-rt4-oppo-60.txt`): `p95` do quadro ≤ 33,3 ms; quadros >
   50 ms abaixo de 5%; `PSS` ≤ 450 MB; `[DevTiming] ... main p95` ≤ 11 ms e `gpu p95` ≤ 11 ms; linhas
   `[ClaybreakGate] tower=Boss inFlight=300 projectiles=12/12` (ou perto).
   Se falhar: baixar no `WorldContent.cs` (nível do gate) `maximumUnitsInFlight` 300 → 250 → 200 e
   `maximumProjectiles` 12 → 8, reconstruir (`Tools/rt4-check.sh /c/temp/rt4 "scene apk"`) e repetir.
3. **Jogar a fatia** (você e a Flávia): `adb install -r C:\temp\rt4\Claybreak.apk` e abrir "Clay Siege".
   Jogar do zero até o chefe (≈ 5 min). Cada um responde: **"voltaria amanhã?"** (sim/não e por quê).
   Durações por fase no log: `adb logcat -d -s Unity | findstr "WON LOST"`.
4. Antes da RT-9: conferir "CLAY SIEGE" à mão no INPI, TMview e USPTO (passo em `decisoes.md`, D18).
5. Anotar os resultados na seção 5 e na linha "Status" da RT-4 do plano; commit `docs: RT-4 aceite`.

## 5. Resultado no aparelho e dos jogadores (preencher)

| Data | Teste | p95 quadro | >50 ms | main p95 | gpu p95 | PSS | em voo | pedras | Passou? |
|---|---|---|---|---|---|---|---|---|---|
| | gate 60 fps, torre do chefe | | | | | | | | |

| Jogador | Chegou ao chefe? | Tempo | Voltaria amanhã? | Comentário |
|---|---|---|---|---|
| Usuário | | | | |
| Flávia | | | | |

## 6. Validação (sessão de 2026-11-20) [F]

Unity 6000.6.7f1 em batchmode, alvo Android, no próprio repositório (Editor fechado), com
`Tools/rt4-check.sh /c/temp/rt4`:

| Verificação | Resultado |
|---|---|
| `ClaybreakSceneBuilder.BuildAndQuit` (7 fases, pool 300, 32 pedras, 5 pares, 4 caixotes, 3 corpos de torre) | PASS |
| `ClaybreakValidation.RunAndQuit` (mundo, fases, nível do gate com a torre do chefe, tema, cena, HUD, telas, sons, regras de código) | PASS |
| `EngineIsolationValidation.RunAndQuit` do kit | PASS |
| Testes EditMode (kit + jogo: fases, portões, progressão, save) | 47/47 (numa rodada anterior, o teste de alocação do pool do kit falhou uma vez no conjunto e passou 2/2 sozinho: instável, anotado para o kit) |
| Testes PlayMode (menu abre na fase 1; corrida fica na pista até a linha de tiro; vencer salva, paga e "NEXT" abre a fase 2 em um toque; nível do gate chega a 300 em voo e 12/12 pedras com a torre do chefe) | 4/4 |
| Capturas de arte (`ArtDirectionCapture`) | PASS — `docs/art/rt4/` |
| APK do jogo `C:\temp\rt4\Claybreak.apk` e de medição `C:\temp\rt4\Claybreak-measure.apk` | gerados, ~34 MB cada |
| Revisão de segurança do save (entrada externa: JSON do aparelho) | leitura tolerante, versão checada, valores limitados (moedas com teto, sem estouro), sem rede |

O gate 3× **não** rodou: o OPPO apareceu no início da sessão com a tela bloqueada e depois saiu do USB
(`adb devices` vazio). Fica no aceite pendente (seção 4).

## 7. O que falta para a produção (lista do aceite)

1. **Arte final** no padrão da D17 também para a forja, defesas, caixote e cenário; ícone com o nome
   "Clay Siege" depois da checagem manual de marca.
2. **Balanço medido**: os números das 7 fases são estimativas [I]; ajustar com as partidas do aceite
   (duração por fase, onde perde) — RT-5.
3. **Conteúdo de produção** (D12): mais mundos e fases por dados (o formato `ClaybreakLevel`/`WorldContent`
   já serve), novos tipos de torre e o 4º tipo de defesa (escudo) — RT-6.
4. **Meta**: mais upgrades (dano, valor do portão, velocidade de lançamento), coleção de unidades com
   escolha de 1–2 no loadout, recompensa diária local (D5) — RT-5.
5. **Áudio definitivo** (CC0 ou próprio) no lugar dos sintetizados — RT-8.
6. **Fonte própria** (hoje é a fonte padrão do Unity) e tela de ajustes com link de privacidade — RT-7/RT-8.
7. **Anúncios** (recompensado + intersticial pronto e desligado, D6), consentimento (UMP) e analytics
   mínimo (D4) — RT-7.
8. **Kit** (pauta 5.1): tabela de "sons extras" no `ThemeAudio`; telas de menu/resultado; teste de alocação
   do `RunnerPool` instável no conjunto completo (passa sozinho) — entrega do `studiokit`.
9. Área segura do celular (recorte da câmera): o HUD usa margens fixas que cabem no OPPO; generalizar com
   a área segura do kit antes de testar em outros aparelhos.
