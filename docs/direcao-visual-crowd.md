# Direção visual "crowd toon" — mapeamento técnico e plano

Data: 2026-11-21. **Status: aprovada pelo usuário em 2026-11-21** (D25, D27 e D26 sem resposta: valem os padrões das fichas).
Implementação em curso a partir da RT-6t.1 (resultado em `rt6t-crowd-toon.md`).
Substitui a direção v2 ("Brinquedo Brilhante", D24, RT-6v.1, kit K.10), descartada pelo usuário e revertida
em `fc1620e` (jogo) e `6db35b7` (kit).

Referência obrigatória: o protótipo three.js aprovado pelo usuário, guardado em
[`docs/reference/crowd-proto.html`](reference/crowd-proto.html) (abre em qualquer navegador; carrega three.js
0.160 e a fonte Baloo 2 pela rede). Toda entrega visual compara capturas do Unity com ele.

Convenção: **[F]** fato (código, protótipo ou documento lido em 2026-11-21); **[I]** inferência ou proposta
nossa; **[P]** parâmetro a calibrar.

---

## 0. Resumo em 10 linhas

1. Um **shader toon próprio** (URP, HLSL) com rampa de 3 faixas (43%/73%/100%, filtro point), luz direcional
   + ambiente de dois tons (céu/chão), neblina linear e cor por vértice. Sem sombras em tempo real, sem PBR.
2. A multidão deixa de ser GameObjects: **arrays nativos + job Burst** calculam posição e animação; a GPU
   desenha tudo em **1 chamada de desenho** (instâncias lidas de um buffer) **+ 1** para as sombras-bolha.
3. Personagens **Bolinha** e **Pintinho** são malhas únicas de 200–500 triângulos geradas por código (receitas
   no jogo, construtor de primitivas no kit), com cores por vértice e uma máscara que recebe a cor da paleta.
4. Formação em **espiral de girassol**, câmera que se afasta com a multidão, tremor no portão ruim.
5. **Paletas como ScriptableObject** (céu, neblina, chão, pista, faixa, multidão, árvore 2 tons, tronco,
   pedra, UI). Trocar de mundo = trocar o asset.
6. Cenário (árvores, pedras) **instanciado e reciclado**; pista com **faixas listradas** no shader.
7. Portões semitransparentes com texto **Baloo 2** (licença OFL, grátis) com contorno; número flutuante e
   "pop" do contador no HUD.
8. O motor genérico é escrito **já com cara de kit** (namespaces e assemblies do kit, sem citar o jogo), mas
   **incubado neste repositório** até funcionar no jogo de verdade; depois vai ao studiokit numa entrega só
   (uma aceitação do Wild Pack em vez de cinco). Formas, paletas, portões e textos ficam **no jogo**.
9. **Primeira entrega é um portão:** 1 personagem + shader + instancing + cena com 1000 unidades medida no
   OPPO. Só seguimos se passar no gate 3× a 60 fps.
10. Cinco decisões do usuário (seção 5): público/"fofura", teto da multidão, skins × formas, torres/assalto e o
    nome "Clay Siege". A 4ª paleta (o jogo tem 4 mundos) é proposta nossa, provisória, como foi a D20.

---

## 1. Mapeamento técnica a técnica (protótipo → Unity/URP)

Regras que valem para todas as linhas: o kit nunca nomeia jogo nem tema; mudar o kit afeta o Wild Pack e exige
a aceitação do studiokit (validadores e save do Wild Pack; gate 3× do Wild Pack só quando tocar o que ele usa);
nada é copiado do Wild Pack; **regra 2.2.1: nada de `Instantiate`/`Destroy` em tempo de jogo** — tudo é
alocado na carga da cena (arrays nativos `Persistent`, `GraphicsBuffer`, pools) e liberado só no
`OnDisable`/`OnDestroy` do dono.

A coluna "Kit" diz o **destino final**. Até a RT-6t.12, essas peças vivem em `Assets/StudioKitIncubator/`
neste repositório, com os namespaces e assemblies definitivos (`JungleDegree.StudioKit.Toon` e
`JungleDegree.StudioKit.Swarm`), asmdefs que **não podem** referenciar o jogo e o validador de isolamento do kit
apontado também para essa pasta. Na RT-6t.12 a pasta é movida para o pacote com os `.meta` (GUIDs iguais),
como assemblies novos e opcionais: o Wild Pack não referencia nada disso (decisão técnica T-RT6t-1, seção 6).

| # | Técnica do protótipo [F] | Abordagem escolhida em Unity/URP | Por quê | Kit | Jogo |
|---|---|---|---|---|---|
| 1 | **Toon de 3 faixas**: `MeshToonMaterial` com rampa 3×1 (110/185/255 → 43%/73%/100%) e `NearestFilter` | Shader HLSL próprio `StudioKit/ToonLit` (URP, `UniversalForward`): `NdotL` da luz principal → amostra uma rampa 3×1 `Texture2D` com `FilterMode.Point` e `TextureWrapMode.Clamp` (gerada em código e salva como asset). Níveis da rampa num asset `ToonLook` [P]. Variantes por `multi_compile` mínimas: instanciado por buffer (multidão/cenário) e normal (malhas soltas). | Shader Graph gera variantes demais e não lê buffer por instância de forma limpa; HLSL curto é previsível no celular e casa com o instancing do item 9. Rampa em textura (e não `step` no shader) deixa o artista trocar as faixas por dado. | shader, `ToonLook` (SO), gerador da rampa | valores do `ToonLook` do jogo |
| 2 | **Luz**: 1 directional suave + `HemisphereLight` forte (céu branco, chão #bbaacc), **sem sombras** | Uma `Light` directional na cena (só cor/direção; o shader lê `GetMainLight()`); o ambiente de dois tons é calculado no shader (`lerp(chão, céu, n.y*0.5+0.5)`) com cores e intensidade do `ToonLook` — não usa as sondas/SH do Unity. URP asset: `MainLightShadowsSupported = 0`, `AnyShadowsSupported = 0`, sem luzes adicionais. | O hemisfério do three é exatamente esse lerp; fazer no shader dá o mesmo resultado em qualquer cena, sem depender de bake de iluminação. Desligar sombras corta o shadow pass (CPU e GPU). | parte do `StudioKit/ToonLit` | `ToonLook` do jogo; URP asset do jogo |
| 3 | **Neblina** na cor do horizonte, linear, início ~22, fim ~75 | Neblina linear do próprio URP (`RenderSettings.fogMode = Linear`, `fogStartDistance`/`fogEndDistance` e cor vindos da paleta); shaders com `#pragma multi_compile_fog` e `MixFog`. | Já existe e é barata; um só lugar para todos os shaders do jogo. | aplicador de paleta escreve `RenderSettings` | valores [P] 22/75 por paleta |
| 4 | **Céu degradê** vertical de 2 cores (no three é um fundo **em espaço de tela**, topo → base) | Skybox com shader `StudioKit/SkyGradientScreen`: cor por `positionCS` normalizado em Y (espaço de tela, igual ao protótipo), duas cores da paleta. | Igual ao protótipo (não depende da inclinação da câmera); 1 desenho de tela cheia barato; sem textura. | shader | cores na paleta |
| 5 | **Cor por vértice, cada modelo UMA malha** | `Mesh` única por modelo com `Color32` por vértice; o canal alfa da cor marca as regiões "cor do corpo" (alfa 255 = recebe a cor da paleta/skin; 0 = cor fixa: olhos, bico, folhas). O shader faz `lerp(corDoVértice, _TintColor ou cor da instância, máscara)`. | Trocar paleta ou skin não reconstrói malha (o protótipo reconstrói); a mesma malha serve a todos os mundos e a elites com outra cor. | regra da máscara no shader | — |
| 6 | **Malhas procedurais** Bolinha e Pintinho (esferas, cone, cilindro; R = 0,22, frente −Z) | Construtor genérico `LowPolyMeshBuilder` no kit (esfera/elipsoide, cone, cilindro, cápsula, icosaedro; transformar, colorir, soldar, juntar; índices 16 bits). As **receitas** (Bolinha, Pintinho, árvore, pedra) são código do jogo num gerador de Editor que grava `.asset` de malha determinístico; validador confere 200–500 triângulos por forma e frente −Z (no Unity a frente é +Z: a receita gira 180° e o validador confere). Menos segmentos que o protótipo (a esfera 18×14 do three já passa de 500 tri) [P]. | Malha pronta na carga (zero custo em jogo), versionável e testável; o construtor é reutilizável por qualquer jogo sem nomear nada. | `LowPolyMeshBuilder` + testes | receitas, gerador, assets, validador de orçamento |
| 7 | **Animação procedural** sem esqueleto: quique `h=|sin(t·9,5+ph)|`, `y=0,2h`; squash `sy=0,8+0,32h`, `sx=sz=1/√sy`; inclinação lateral `clamp(−vx·0,06, ±0,5)` e −0,12 rad à frente; nascimento elástico 0,45 s com atraso 0–0,25 s; morte balística (gravidade 22, gira, encolhe em 1,1 s) | **CPU com Jobs + Burst** (`IJobParallelFor` sobre `NativeArray`s): o job do quadro calcula o estado (x, z, vx, fase, nascimento, morte) e escreve por instância uma transformação compacta 3×4 (48 bytes) + cor num `GraphicsBuffer`. Fórmulas **idênticas** às do protótipo, numa classe estática testável. **Plano B** (se a RT-6t.1 mostrar CPU apertada): mover só a parte cosmética (quique/squash/pop) para o vertex shader, mandando fase e tempo de nascimento no buffer. | Posição e morte são estado de jogo (portões, assalto e testes precisam delas na CPU); Burst faz 1000 unidades em bem menos de 0,2 ms [I, a medir; lote de 64+ por job, ou `IJob` único]; fórmulas na CPU são testáveis em EditMode e iguais ao protótipo. O vertex shader fica como reserva medida, não como aposta. | `Swarm`: simulação, jobs, fórmulas | parâmetros [P] num asset do jogo |
| 8 | **Blob shadow** (círculo #2b2233, opacidade 0,18, encolhe `1−0,35h` quando sobe) | Segunda chamada instanciada: disco de 12 lados no chão, shader `StudioKit/BlobShadowInstanced` (transparente, sem escrita de profundidade), **lendo o mesmo buffer** da multidão e calculando a escala a partir da altura. | +1 chamada de desenho no total, como pedido; zero dado extra por quadro. | shader + desenho | cor/opacidade na paleta [P] |
| 9 | **Instancing**: `InstancedMesh` com até 2600 instâncias, 1 chamada | `Graphics.RenderMeshIndirect` (ou `RenderMeshPrimitives`) com `RenderParams` e um `GraphicsBuffer` estruturado lido no vertex shader por `SV_InstanceID` → **1 chamada para qualquer contagem**. Capacidade fixa alocada na carga (ex.: 1024 vivos + 512 morrendo [P]); contagem por quadro no buffer de argumentos; `RenderParams.worldBounds` acompanha a multidão. **Reserva para aparelhos sem buffer estruturado no vertex shader** (muitas GPUs Mali em OpenGL ES): `Graphics.RenderMeshInstanced` com matrizes em blocos, variante própria do shader e um segundo array de matrizes para as sombras (calculado no mesmo job); 1000 unidades = 2–4 chamadas. A RT-6t.1 faz só o caminho indireto e registra API/GPU do OPPO; a reserva é a RT-6t.11, antes do soft launch. | `RenderMeshInstanced` divide em vários desenhos quando passa do limite do buffer constante (varia por GPU); o caminho indireto garante "1 chamada" mas exige buffer estruturado no vertex shader (Vulkan ok; OpenGL ES 3.1 depende do aparelho). O projeto usa Vulkan primeiro e GLES3 de reserva (Player Settings, automático) [F]; a RT-6t.1 registra a API e a GPU do OPPO. | `Swarm`: `SwarmRenderer` | — |
| 10 | **Sem GameObject/Rigidbody/Collider por unidade** | A unidade é um índice nos arrays. Portões: cruzamento por Z (já é assim na corrida). Assalto: acerto por distância na faixa (já é assim, R-D4) — passa a ler os arrays em vez de `Transform`s. Sem `Physics` por quadro. | Custo de CPU por unidade vira poucos ns; nada de `Update` por objeto. | `Swarm` | adaptação de `AssaultField` e da corrida |
| 11 | **Formação em espiral de girassol**: `offset_i = √(i+0,5)·0,25·(cos(i·2,39996), sin(i·2,39996)·1,35)`; lerp suave `k=1−e^(−7dt)` com variação de velocidade por unidade (0,8–1,2); X limitado à pista | Tabela de offsets pré-calculada na carga (capacidade inteira); o job segue `centroX + offset`, com o mesmo fator por unidade `(i·7 mod 10)/10`; clamp na largura da pista vinda do nível. | Igual ao protótipo; tabela fixa = zero trig por quadro na formação. | `Swarm`: `SunflowerFormation` | espaçamento/achatamento [P] |
| 12 | **Câmera que se afasta**: `camD → 1+√n/22` (suavizado `1−e^(−2dt)`), posição `(cx·0,35, 3,2+4,2camD, 5,5+5,5camD)`, olha para `(cx·0,35, 0, −5)`, FOV 50 | Componente de câmera do kit `SwarmCameraRig` com essas fórmulas e números num asset [P]; o jogo escolhe o perfil (corrida, assalto). Atenção: o protótipo foi feito em tela larga; em retrato o FOV vertical 50 corta as laterais — a RT-6t.3 ajusta o FOV/distância para a pista inteira caber em 9:16 e registra. | Mesmo comportamento, ajustável por dado; o perfil de assalto (R-D5) continua sendo do jogo. | `SwarmCameraRig` | perfis de câmera |
| 13 | **Shake** ~0,35 s no portão ruim (amplitude `shake·0,6`) | Mesmo rig: deslocamento aleatório decrescente; `UnityEngine.Random` (sem alocação). | Simples, igual ao protótipo. | no rig | quando disparar |
| 14 | **Cenário instanciado e reciclado**: 90 árvores + 40 pedras, voltam para o fundo ao passar da câmera | `InstancedProps` do kit: N instâncias por malha (árvore 2 tons + tronco; pedra), posições pré-sorteadas por semente, reciclagem por Z relativa à câmera, 1 chamada por malha com o mesmo `StudioKit/ToonLit` instanciado. | 2–3 chamadas para todo o cenário lateral, zero objetos. | `InstancedProps` | receitas de árvore/pedra, densidade por mundo |
| 15 | **Pista com faixas rolando** (textura 64×64 listrada, repetida 40×, offset animado) | Shader `StudioKit/StripedTrack`: listras **procedurais** pelo Z do mundo (`frac(z/período) < 0,5`), duas cores da paleta, toon + neblina. Se o jogo move a multidão para frente, as listras "rolam" sozinhas; se move o mundo, um offset global. Trilhos laterais com a cor da faixa. | Sem textura, sem `SetTextureOffset` por quadro, nítido em qualquer resolução. | shader | período/cores [P] |
| 16 | **Contador de FPS em debug** (fps + nº de bonecos a cada 0,5 s) | `DevFpsOverlay` do kit: rótulo uGUI só em build de desenvolvimento/medição (`Debug.isDebugBuild`), texto montado sem alocação (buffer de `char`); grava também uma linha no log a cada janela, que o script de gate lê. | O kit já tem `DevFrameTimingProbe` (CPU/GPU por janela); o overlay soma a leitura visual pedida. | `DevFpsOverlay` | ligar na cena de teste e no APK de medição |
| 17 | **Paletas como dado** (3 mundos: Pastel, Pôr do sol, Doce; o jogo tem 4) | `ToonPalette` (ScriptableObject do kit, campos genéricos: céu topo/base, neblina + início/fim, chão, pista, faixa, multidão, folhagem 1/2, tronco, pedra, destaque de UI, tinta de UI, sombra). Um `ToonPaletteApplier` escreve as cores como propriedades globais de shader (`Shader.SetGlobalColor`) e em `RenderSettings` — trocar de mundo é trocar o asset, sem reconstruir nada. Os assets das paletas (valores do usuário, seção 1.1) são do jogo. | Dado fora do código (padrão de qualidade); uma troca = poucas chamadas globais. | `ToonPalette`, `ToonPaletteApplier` | as 4 paletas (3 do usuário + 4ª provisória, seção 5.1) |
| 18 | **Portões semitransparentes**, metade da pista cada: verde #5ad1a0 (+N, ×2, ×3), vermelho #ff6b81 (−N, ÷2), opacidade 0,45; 3 postes brancos | Painel = cubo achatado com shader transparente sem escrita de profundidade, cor por dado; postes no `ToonLit`. Os portões continuam do jogo (T-RT2-2: a regra do portão é deste jogo). Pool de portões já existente, só troca de visual. | Os portões do Wild Pack não estão no kit e este jogo tem regra própria. | — | visual dos portões, cores |
| 19 | **Texto do portão**: grande, branco, contorno (verde-escuro #1f7a55 / vinho #a52a3a), fonte arredondada | **TextMeshPro** (vem no `com.unity.ugui` 2.x do projeto) com fonte **Baloo 2 ExtraBold** em SDF; contorno pelo material SDF (dois presets: bom/ruim); texto escrito sem alocação (`SetCharArray`). Licença **SIL Open Font License 1.1** (grátis, permite embutir em app comercial; exige incluir o texto da licença; não se vende a fonte sozinha) — registrar como a Roboto (`Assets/Claybreak/Content/Fonts/`). Conferir na entrega os glifos `÷`, `×` e acentos de pt. | Contorno nítido e barato em SDF; o `TextMesh` antigo não tem contorno e borra. | — | fonte, materiais, rótulos |
| 19b | **Fonte da UI** (o protótipo usa Baloo 2 no HUD e nos botões) | A UI atual usa `UnityEngine.UI.Text` com Roboto Black [F]: troca só o arquivo de fonte para Baloo 2 (TTF dinâmica), sem migrar telas para TMP. Contador da multidão e número flutuante em TMP (precisam de contorno). | Mudança mínima e segura nas telas; TMP só onde o contorno importa. | — | telas e HUD |
| 20 | **Número flutuante** ao passar (sobe ~110 px com bounce 0,4→1,25→1, some em 1 s; verde #2fbf86 / vermelho #ff4d6d, contorno branco) | Pool de 4 rótulos TMP na tela do HUD, animação por curva sem alocação (componente pequeno do jogo). | Igual ao protótipo, sem criar objetos. | — | HUD |
| 21 | **Pop do contador** (escala 1,18 por 0,15 s) | `HudPulse` do kit (já existe) ou o mesmo tween do item 20. | Reuso do que já existe. | (existente) | ligar no contador |
| 22 | **Antisserrilhado** (`antialias:true` no protótipo) | MSAA 4× no URP asset do jogo se couber no gate (hoje está desligado e com escala de render 0,8 [F]); a RT-6t.1 mede com MSAA 1×/4× e registra. | Bordas suaves fazem parte do visual "brinquedo"; em GPU de celular (tile) o MSAA costuma custar pouco [I]. | — | URP asset do jogo |

### 1.1 Paletas do usuário (valores exatos, para os assets)

| Campo | Pastel | Pôr do sol | Doce |
|---|---|---|---|
| Céu topo → base | #ffd9cf → #cfe8ff | #3a3270 → #f7a26b | #8fd3ff → #eef9ff |
| Neblina | #f3e3ea | #e9967a | #dff2ff |
| Chão | #bfe6cf | #7a5482 | #93d99a |
| Pista / faixa | #fff4ea / #ffe3d1 | #f6caa6 / #eeb790 | #fffae6 / #ffedb0 |
| Multidão | #ff8fab | #fff1d6 | #ffcf3a |
| Árvore (2 tons) | #7cc6a4 / #a8dcbf | #463a78 / #5b4a8c | #45b07c / #6fcf8f |
| Tronco | #c99076 | #2f2650 | #b07a55 |
| Pedra | #e7d3f0 | #a36b8f | #f4b4c8 |
| UI (destaque) / tinta | #ff6f91 / #3b2f4a | #f26a5a / #2d2448 | #2fa7e6 / #24435a |

Constantes do protótipo que não variam por paleta [F]: olhos #2b2233, folha #5cc98a, laranja (bico, topete)
#ff9f43, sombra #2b2233 a 0,18, portão bom #5ad1a0, ruim #ff6b81, contorno do texto #1f7a55 / #a52a3a,
número flutuante #2fbf86 / #ff4d6d. Neblina 22–75. Luz: hemisfério branco/#bbaacc ×1,4, direcional branca ×2,2
de (6, 12, 4) — o three r160 divide a irradiância por π; os números do `ToonLook` serão calibrados por captura
lado a lado com o protótipo, não copiados [I].

### 1.2 Orçamento de desempenho (alvo para a RT-6t.1) [I]

Gate 3× do estúdio [F]: CPU principal p95 ≤ 11 ms, GPU p95 ≤ 11 ms, < 5% dos quadros > 50 ms, PSS ≤ 450 MB.
Pedido do usuário: **1000+ unidades a 60 fps** → também quadro p95 ≤ 16,7 ms a 60 fps.

| Item | Orçamento | Base |
|---|---|---|
| Simulação + animação de 1000 vivos + 512 morrendo (Burst) | ≤ 0,5 ms CPU (main + workers) | inferência; medir |
| Envio do buffer (1536 × 64 B ≈ 96 KB/quadro) | ≤ 0,2 ms | inferência; medir |
| Multidão na GPU: 1000 × ≤ 500 tri = ≤ 0,5 mi tri/quadro | ≤ 4 ms GPU com o resto da cena | inferência; medir no OPPO |
| Chamadas de desenho da multidão | 1 + 1 (sombras) | pedido do usuário |
| Cenário | ≤ 3 chamadas instanciadas | item 14 |

Se a RT-6t.1 não passar: (1) animação cosmética no vertex shader; (2) menos triângulos (Bolinha 250); (3)
MSAA 1×; (4) escala de render 0,8 → 0,7; (5) se ainda não passar, **parar** e levar ao usuário uma ficha com o
maior número de unidades que passou. O gate nunca é afrouxado.

---

## 2. O que acontece com o que já existe

Estado atual lido em 2026-11-21 [F] (jogo em `7798e2f`/`fc1620e`, kit 0.4.0 em `6db35b7`):
- Cada unidade é um **GameObject do pool**: prefab `Generated/RunnerUnit.prefab` gerado pelo
  `ClaybreakSceneBuilder.CreatePool` (MeshRenderer com a massinha do kit, `RunnerUnit`, `CapsuleCollider`),
  300 instâncias pré-colocadas e inativas sob o `RunnerPool` do kit. Nenhum Rigidbody nem consulta de física;
  acertos já são por distância em arrays.
- **Corrida**: `RunnerCrowd` do kit (954 linhas; líder com `CharacterController`, seguidores com separação em 6
  iterações, poses por textura, inclinação). Contagem muda por `ClaybreakFlow.OnGateCrossed` → `GateMath.Apply`
  → `crowd.SetCount` (e caixotes, continuar, início). Perda: `player.Trip()` + `RunnerDeathEffects.Play`.
- **Assalto**: `AssaultField` (572 linhas) já move tudo num `Update` só sobre arrays paralelos, mas escreve
  `transform.position` por unidade e tinge elites (`UnitType`: brute, sprinter, warden; escala, cor, dano,
  armadura, velocidade) por `MaterialPropertyBlock`. Defesas (`CannonDefense`, `SweeperDefense`,
  `ShieldDefense`, `AssaultWall`, `AssaultTower`) consultam o campo por posição (`TryPickTarget`,
  `KillInRadius`, `Covers`, `Hit`) — não dependem de colisores.
- **Dependem de GameObject por unidade**: `RunnerCrowd`, `RunnerUnit`, `CameraFollow` (enquadra pelos
  colisores da multidão), `RunnerDeathEffects` (respingos e "saltadores" em pool), `RunnerLean`,
  `RunnerGroundShadow` (disco filho).
- **Render**: shader do kit `StudioKit/RunnerClay` na multidão; ~56 materiais do jogo em URP Simple Lit/Unlit
  gerados pelo `ArtPalette`; nenhum shader próprio do jogo, nenhum toon. URP móvel: escala 0,8, MSAA desligado,
  HDR ligado, sombras da luz principal suportadas (mas o sol está com `shadows = None`); sem skybox (cor de
  fundo = `WorldLook.sky`); neblina linear 50–130.
- **Mundo**: `WorldLook`/`SceneryStyle` (Marches, Mire, Foundry, Peaks) nos assets World1–4; um conjunto de
  cenário pré-alocado por mundo ligado/desligado por `WorldScenery`; pista = uma caixa fixa; cenário
  construído no Editor por `WorldSceneryArt`/`WorldContent`; torres por `TowerArt`.
- **Portões**: `RunGateCourse` (até 5 pares pré-alocados) e a fileira do assalto em `AssaultField`; rótulos
  `TextMesh` legado com Roboto Black; UI em `UnityEngine.UI.Text`. Não há número flutuante; o HUD tem
  `SetArmy` e um pop de faixa. `HudPulse` do kit não é usado.
- **Skins**: `CrowdSkin` (id, cor, preço) no catálogo = só a cor da massinha (`_BaseColor`).
- **Ferramentas e testes**: `ClaybreakSceneBuilder` (1226 linhas) constrói a cena inteira;
  `ClaybreakValidation` confere níveis contra a capacidade do pool; `ArtDirectionCapture` grava as capturas;
  PlayMode `DeviceGateLevelHolds300UnitsInFlightAndTheProjectileLimit` e afins; `rt2-gate.sh` mede o nível de
  gate (300 em voo).

| O que existe | Destino | Entrega |
|---|---|---|
| `RunnerPool`/`RunnerUnit`/`RunnerUnit.prefab` (300 GameObjects) no jogo | **removido do jogo** (o kit mantém, o Wild Pack usa) | 6t.4/6t.7, limpeza na 6t.12 |
| `RunnerCrowd` + `PlayerController` na corrida | **substituído** pelo `Swarm` (centro da multidão segue o arraste; sem `CharacterController`; caixotes e portões continuam por distância) | 6t.4 |
| `AssaultField` | **adaptado**: mesmos arrays e regras, sem `Transform`; elites por escala/cor de instância | 6t.7 |
| Defesas e torre | **sem mudança de regra**; leem posições do campo como hoje | 6t.7 |
| `CameraFollow` (kit) e `AssaultCamera` | `CameraFollow` **substituído** pelo `SwarmCameraRig` na corrida; `AssaultCamera` **adaptada** (perfil do rig) | 6t.3/6t.4/6t.7 |
| `RunnerDeathEffects` (respingo de massinha) | **substituído** pela morte balística do `Swarm` | 6t.5/6t.7 |
| `RunnerClay`, `RunnerGroundShadow`, `RunnerSkin`, poses | **sem uso no jogo** depois da 6t.12 (ficam no kit) | 6t.12 |
| Materiais Simple Lit do `ArtPalette` (cenário, torres) | **substituídos** pelo `ToonLit` (cenário na 6t.8; torres na 6t.10) | 6t.8, 6t.10 |
| `WorldLook`/`SceneryStyle`/`WorldScenery` | **adaptados**: o `WorldLook` passa a apontar para uma `ToonPalette`; cenário por `InstancedProps` | 6t.6, 6t.8 |
| Pista em caixa fixa | **substituída** pela pista listrada | 6t.8 |
| Neblina 50–130 e fundo em cor sólida | **substituídos** por neblina 22–75 [P] e céu degradê | 6t.6 |
| `TextMesh` + Roboto Black nos portões | **substituídos** por TextMeshPro + Baloo 2 com contorno (TMP já vem no `com.unity.ugui` 2.6 do projeto; importar os recursos essenciais do TMP) | 6t.9 |
| UI em `UI.Text` com Roboto | **adaptada**: só troca a fonte para Baloo 2 (sem migrar a UI para TMP); contador e número flutuante em TMP | 6t.9 |
| `CrowdSkin` (cor) | conforme **D26** (recomendado: continua cor, aplicada pela máscara; forma gravada no save) | 6t.6 |
| Nível de gate (300 em voo) e `rt2-gate.sh` | **ampliados**: teto da D27 na corrida + assalto mais pesado | 6t.1, 6t.4, 6t.12 |
| `ArtDirectionCapture` | **adaptado** para capturar a cena de teste e os mundos novos | 6t.2, 6t.12 |
| Validador: níveis × capacidade do pool | **adaptado**: níveis × capacidade do `Swarm`; nova regra de orçamento de malha | 6t.2, 6t.4 |
| Saves (v3) | **v4 com um campo novo** (forma escolhida), migração com valor padrão, na opção recomendada da D26 | 6t.6 |

---

## 3. Riscos

| Risco | Chance / impacto [I] | Mitigação |
|---|---|---|
| **R-T1 Desempenho no OPPO com 1000 unidades** (vértices na GPU, envio do buffer, transparências do portão e das sombras) | média / alto | É a primeira entrega e é portão; orçamento da seção 1.2; plano B medido (shader, menos tri, MSAA); nunca afrouxar o gate. |
| **R-T2 Buffer estruturado no vertex shader** indisponível em aparelhos GLES (comum em Mali) | baixa no OPPO (Vulkan) / média no público | Caminho de reserva `RenderMeshInstanced` em blocos (RT-6t.11), escolhido em tempo de execução por `SystemInfo.maxComputeBufferInputsVertex`; testado forçando GLES no OPPO. **Tratado na RT-6t.11 (2026-11-22):** em GLES o Mali do OPPO informa 0 buffers no vertex shader e a reserva desenha igual, com gate 3× aprovado. |
| **R-T3 Gate 3× numa cena maior que a medida até hoje** (o gate atual mede 300 em voo [F]) | média / alto | O nível do gate passa a ter 1000 vivos na corrida **e** o assalto mais pesado; `rt2-gate.sh` ganha o modo de 1000. |
| **R-T4 Fonte Baloo 2** | baixa / baixo | OFL 1.1 permite uso comercial e embutir; guardar `OFL.txt` ao lado da fonte; baixar do repositório oficial `github.com/google/fonts` (ofl/baloo2) com hash registrado; não renomear nem vender a fonte. |
| **R-T5 Compatibilidade com o Wild Pack** | baixa / médio | Incubação aqui até a 6t.12; no kit tudo é aditivo (assemblies novos); o kit passa a declarar `com.unity.burst`, `com.unity.collections` e `com.unity.mathematics` (hoje já vêm indiretos pelo URP [F: packages-lock]) — conferir que o Wild Pack resolve as mesmas versões; aceitação do studiokit **uma vez**, na 6t.12 (validadores 1–3, save 87/87, isolamento, anúncios offline 24/24, build Android do Wild Pack). |
| **R-T6 Visual "fofo" contra a direção adulta registrada** (GDD 2 e 9: 13+, "sem bichinhos fofos", "nada de olhos enormes"; D17: torres "sem rosto nem olhos"; lição da adultização do Wild Pack) e contra a diferenciação do R1 ("massinha adulta, não neon de Voodoo") | média / alto na loja (RT-9) | Decisão do usuário **D25**; variante sem olhos pronta como reserva barata (receita). |
| **R-T6b O protótipo não mostra o assalto** (metade do jogo: projéteis, muralha, escudo, efeitos, caixotes, loja, ícone) | alta / médio | Decisão **D28**; tudo o que não tem referência usa o `ToonLit` + paleta e vai em captura ao usuário. |
| **R-T6c Identidade "Clay" sem massinha** (nome Clay Siege, nomes de skins BARRO/BRASA/MUSGO, sons "de argila") | alta / médio antes da RT-9 | Decisão **D29**. |
| **R-T7 Saves e economia** (skins são cor da multidão [F: T-RT5-3]; paleta também define a cor; a forma escolhida precisa ser gravada) | média / médio | Decisão **D26**; save v4 só com um campo novo (forma), migração com valor padrão. |
| **R-T7b Contraste** (ex.: multidão #fff1d6 sobre a pista #f6caa6 no Pôr do sol; skins contra o verde/vermelho dos portões) | média / médio | Validador de contraste na 6t.6 só **avisa** (as cores do usuário são exatas); ajuste fino de luz/sombra-bolha; achados vão ao usuário em captura. |
| **R-T7c Sombras-bolha somadas** (1000 discos a 0,18 se sobrepõem e escurecem o chão no meio da multidão) | média / baixo | É o que o protótipo aprovado faz; se manchar, limitar com stencil ou opacidade menor — medir/capturar na 6t.5. |
| **R-T8 Retrato** (protótipo pensado em tela larga) | alta / baixo | Ajuste de FOV/distância por dado na RT-6t.3 e capturas 9:16. |
| **R-T9 Duas sessões escrevendo no mesmo repositório** (regra global: uma por repositório) | média / médio | Série **sequencial** por padrão; pares sem conflito de arquivos listados na seção 4 só se o usuário autorizar duas sessões aqui. |
| **R-T9b Trocar a multidão do jogo tarde demais** (o `RunnerCrowd` tem líder, caixotes, tropeço, continuar) | média / alto | Fatia fina na corrida já na **6t.4**, logo depois da formação e da câmera. |
| **R-T10 Prazo** | média / médio | 12 entregas pequenas [I] (~2–3 semanas de sessões, sequenciais); o soft launch (RT-9) espera esta série. |

---

## 4. Plano de entregas

Numeração **RT-6t.1 … RT-6t.12** (o "t" é de toon). Só a RT-6t.12 escreve no studiokit; ela é registrada lá
como **K.11** (o número K.10 foi usado pela direção revertida; não é reaproveitado). Seções completas, com o
prompt de cada sessão, em `docs/plano-de-entregas.md`.

Validação padrão (seção Maestro de `.claude/sessao.md`): `Tools/rt4-check.sh` (cena, validador do jogo,
isolamento do kit — também sobre `Assets/StudioKitIncubator/` a partir da 6t.1 —, EditMode, PlayMode, build
Android) e, quando a entrega toca desempenho, o gate 3× no OPPO com `Tools/rt2-gate.sh` (ou o modo novo da
6t.1), uma vez, no fim, no caso mais pesado. A 6t.12 roda também a aceitação do studiokit (Wild Pack:
validadores 1–3, save 87/87, isolamento, anúncios offline 24/24, build Android), **depois** do Unity deste
projeto (dois Unity em batchmode derrubam um deles [F: aprendizado RT-8]).

| Entrega | O quê | Depende de | Decisão que trava |
|---|---|---|---|
| **RT-6t.1** | **Portão técnico** (só desempenho): shader toon + rampa, Bolinha provisória no teto de 500 tri, simulação Burst com quique + squash, desenho indireto (1 + 1 chamadas), sombras-bolha simples, cena de teste com 1000 (2000 para registro), FPS em debug, gate 3× a 60 fps no OPPO | aprovação deste plano | — |
| RT-6t.2 | As 2 formas finais (Bolinha, Pintinho) por receita, máscara de cor, validador de orçamento, capturas lado a lado com o protótipo | 6t.1 | **D25** (olhos ou variante sem olhos) |
| RT-6t.3 | Formação girassol, arraste, câmera que se afasta, shake, ajuste a retrato 9:16 (cena de teste) | 6t.1 | — |
| RT-6t.4 | **Fatia fina na corrida do jogo**: o `Swarm` substitui `RunnerCrowd`/`PlayerController`/`RunnerPool` na corrida (contagem, portões, caixotes, tropeço, continuar), câmera da corrida, cor da skin atual como tinta; gate na corrida | 6t.2, 6t.3 | **D27** só se o teto subir (sem resposta: 300) |
| RT-6t.5 | Animações completas (inclinação, nascimento, morte balística com anel, sombra que encolhe) na cena de teste e na corrida | 6t.4 | — |
| RT-6t.6a | Paletas como dado (3 do usuário + 4ª provisória; campos de torre/defesa), céu degradê, neblina, aplicador; BARRO = cor do mundo | 6t.5 | — |
| RT-6t.6b | Formas e skins conforme D26 (menu, save v4) — dividida da 6t.6 em 2026-11-22 | 6t.6a | **D26** |
| RT-6t.7 | Multidão nova no **assalto** (índices em vez de `Transform`; defesas leem os arrays; elites por instância; mortes pelo anel); teto da D27 | 6t.6a | D27 (sem resposta: 300) |
| RT-6t.8 | **Cenário**: pista listrada, trilhos, árvores e pedras instanciadas e recicladas, chão, por mundo | 6t.6a | — |
| RT-6t.9 | **Portões e feedback**: painéis semitransparentes, Baloo 2 com contorno (corrida e assalto), número flutuante, pop do contador, shake no portão ruim; fonte do HUD e das telas | 6t.7 | — |
| RT-6t.10 | Torres, defesas, chefes, projéteis e efeitos do assalto no estilo novo | 6t.8 | **D28** (sem resposta: opção A) |
| RT-6t.11 | Caminho de reserva para aparelhos OpenGL ES sem buffer estruturado (`RenderMeshInstanced` em blocos) + teste forçando GLES no OPPO | 6t.7 | — |
| RT-6t.12 | **Levar o motor ao kit** (K.11, pacote 0.5.0) + limpeza do caminho antigo + capturas dos 4 mundos + gate 3× final | 6t.9, 6t.10, 6t.11 | — (D29 trava só a RT-9) |

**Paralelismo.** A regra global do usuário é **uma sessão escrevendo por repositório**, e toda a série escreve
neste repositório (até a 6t.12). Por isso o padrão é **sequencial**. Se o usuário autorizar duas sessões aqui, os
pares sem arquivos em comum são: **6t.2 ∥ 6t.3** (receitas de malha × formação/câmera; cada uma com sua parte da
cena de teste), **6t.8 ∥ 6t.9** (cenário × portões/HUD) e **6t.10 ∥ 6t.11** (arte do assalto × caminho de
desenho). O `ClaybreakSceneBuilder` é compartilhado: cada sessão mexe só no método da própria área e a última a
commitar regenera a cena depois do `git pull --ff-only`. 6t.7 e 6t.9 nunca juntas (as duas mexem na fileira de
portões do assalto em `AssaultField`).

### 4.1 Entregas em detalhe

**RT-6t.1 — Portão técnico: 1 personagem, toon, instancing, 1000 unidades no OPPO**
- Escopo (mínimo para medir de verdade): incubadora `Assets/StudioKitIncubator/` com os asmdefs
  `JungleDegree.StudioKit.Toon` (shader `StudioKit/ToonLit`: rampa 3×1 point, ambiente de dois tons, neblina, cor
  por vértice com máscara de tinta; `ToonLook`; gerador da rampa; `LowPolyMeshBuilder` com testes EditMode) e
  `JungleDegree.StudioKit.Swarm` (arrays nativos, job Burst com quique + squash em grade de girassol fixa,
  `SwarmRenderer` indireto, sombras-bolha como 2ª chamada, `DevFpsOverlay`); dependências Burst, Collections e
  Mathematics declaradas no manifest do jogo nas versões que o `packages-lock` já resolve. Jogo: Bolinha
  provisória **no teto de 500 tri**, cena `ToonLab` (Pastel, câmera fixa), modo de medição no APK
  (`--ez toonlab true --ei units N`), modo novo no script de gate, URP móvel sem sombras. Fora do escopo: formas
  finais, formação móvel, nascimento/morte, caminho GLES.
- Arquivos/áreas: `Assets/StudioKitIncubator/**` (novo), `Assets/Claybreak/Lab/**` (novo),
  `Assets/Claybreak/Editor/ClaybreakBuild.cs` (opção de medição), `Tools/rt2-gate.sh` (modo `toonlab`),
  `Assets/Settings/Mobile_RPAsset.asset`, `Packages/manifest.json`, `Assets/Editor/StudioKitIsolationTerms.txt`
  (se preciso) e o validador de isolamento apontado para a incubadora.
- Aceite: validação padrão; `ToonLab` roda no Editor; **no OPPO, 1000 unidades a 60 fps passando o gate 3×**
  (CPU principal p95 ≤ 11 ms, GPU p95 ≤ 11 ms, quadro p95 ≤ 16,7 ms, < 5% > 50 ms, PSS ≤ 450 MB), medido com MSAA
  1× e 4× e HDR desligado/ligado; 2000 só registro; chamadas da multidão = 1 + 1; API gráfica e GPU do OPPO
  registradas. **Se não passar**: plano B da seção 1.2 na ordem e nova medição; se ainda falhar, ficha ao
  usuário com o maior número que passou. **A série só continua se passar.**
- Exige o usuário: OPPO conectado e desbloqueado.

**RT-6t.2 — As duas formas**
- Escopo: receitas Bolinha (esfera achatada 0,9 em Y, 2 olhos, broto com caule e 2 folhas) e Pintinho (esfera,
  olhos, bico em cone laranja, topete de 2 esferinhas, rabinho em cone), R = 0,22, frente correta; máscara de
  tinta; variante "sem olhos" se a D25 pedir; assets gerados; regra no validador (200–500 tri, malha única,
  máscara); seletor de forma na `ToonLab`; capturas lado a lado com o protótipo em `docs/art/rt6t/`.
- Arquivos: `Assets/Claybreak/Editor/UnitShapes*.cs` (novo), `Assets/Claybreak/Content/Generated/Units/**`,
  `Assets/Claybreak/Lab/` (só o seletor), `ClaybreakValidation.cs` (regra nova), testes EditMode.
- Aceite: validação padrão; orçamento de triângulos; capturas conferidas por revisor independente; sem gate
  (dentro do teto da 6t.1).

**RT-6t.3 — Formação, controle e câmera**
- Escopo: `SunflowerFormation` (tabela pré-calculada), seguir `centroX + offset` com variação de velocidade por
  unidade e clamp na pista; `SwarmCameraRig` (afastamento `1+√n/22`, posição/alvo do protótipo, shake 0,35 s);
  arraste e painel de teste (+100, 1000, recomeçar) na `ToonLab`; FOV/distância para retrato 9:16 por dado.
- Arquivos: `Assets/StudioKitIncubator/Swarm/Formation*`, `.../Swarm/Camera*`, testes; `Assets/Claybreak/Lab/`
  (controles e perfil de câmera).
- Aceite: testes EditMode (offsets iguais ao protótipo, clamp); `ToonLab` 10 → 1000 com a câmera se afastando,
  em captura 9:16.

**RT-6t.4 — Fatia fina na corrida**
- Escopo: a corrida do jogo usa o `Swarm`: o centro segue o arraste (`PointerInput`), `GateMath` muda a contagem,
  caixotes e portões continuam por distância, tropeço/continuar/início ajustam a contagem; câmera da corrida pelo
  rig; cor da skin equipada vira a tinta (sem mudar save); forma Bolinha; validador "níveis × capacidade do
  `Swarm`"; nível de gate com a multidão máxima na corrida. Remoção simples (sem animação de morte ainda).
- Arquivos: `ClaybreakFlow.cs`, `RunGateCourse.cs` (só a ligação com a contagem), `ClaybreakLevel.cs`,
  `ClaybreakSceneBuilder.cs` (só a corrida), `ClaybreakValidation.cs`, testes PlayMode da corrida.
- Aceite: validação padrão completa; PlayMode da corrida verde (incluindo
  `RunKeepsTheCrowdOnTheTrackUntilTheFiringLine` adaptado); gate 3× na corrida no teto da D27.
- Exige o usuário: OPPO conectado.

**RT-6t.5 — Animações**
- Escopo: inclinação lateral `clamp(−vx·0,06, ±0,5)` e −0,12 rad à frente, nascimento elástico 0,45 s com atraso
  0–0,25 s, morte balística (empurrão radial + para cima, gravidade 22, giro, encolhe em 1,1 s) num anel de
  capacidade fixa, sombra que encolhe com a altura, ordem de remoção (de fora para dentro); na `ToonLab` e na
  corrida (portão ruim).
- Arquivos: `Assets/StudioKitIncubator/Swarm/Animation*`, `.../Swarm/Dying*`, shader da sombra, testes;
  `Assets/Claybreak/Lab/`; `ClaybreakFlow.cs` (só a chamada de morte).
- Aceite: testes das fórmulas contra valores calculados do protótipo; vídeo/capturas; **gate 3× no OPPO com
  1000 vivos + 512 morrendo**; captura do chão sob a multidão (R-T7c).

**RT-6t.6 — Paletas, céu, neblina, formas e skins**
- Escopo: `ToonPalette` (campos da seção 1.1 + torre clara/escura, detalhe de torre e defesa, para a 6t.10),
  `ToonPaletteApplier`, `StudioKit/SkyGradientScreen`; assets Pastel, Pôr do sol, Doce (valores exatos) e a 4ª
  provisória (seção 5.1); `WorldLook` aponta para a paleta; formas e skins conforme **D26** (seletor de forma
  no menu, save v4 com o campo "forma" e migração, cores novas das skins se a D26 pedir); validador: campos
  completos e **aviso** de contraste; capturas dos 4 mundos.
- Arquivos: `Assets/StudioKitIncubator/Toon/Palette*`, shader do céu; `Assets/Claybreak/Content/Data/Palettes/**`,
  `ClaybreakWorld.cs`, `WorldScenery.cs` (só paleta), `ClaybreakCatalog.cs`/`Catalog.asset` (skins),
  `SaveData.cs`, `SaveValidator.cs`, `Editor/SaveFixtures`, `ClaybreakScreens.cs` (seletor), testes de save.
- Aceite: validação padrão; teste de migração do save v3 → v4; capturas; revisão de segurança (mexe em save).
- Exige o usuário: D26.

**RT-6t.7 — Multidão nova no assalto**
- Escopo: `AssaultField` troca `RunnerUnit`/`Transform` por índices do `Swarm` (mesmas regras); defesas e torre
  leem os arrays; elites (brute, sprinter, warden) com escala e cor por instância; mortes pelo anel; limite em voo
  conforme **D27**; `AssaultCamera` como perfil do rig; nível de gate do assalto.
- Arquivos: `AssaultField.cs`, `CannonDefense.cs`, `SweeperDefense.cs`, `ShieldDefense.cs`, `AssaultWall.cs`,
  `AssaultTower.cs`, `AssaultCamera.cs`, `LevelSpec.cs`/`LevelGenerator.cs` (teto, se a D27 mudar),
  `ClaybreakSceneBuilder.cs` (só o assalto), testes PlayMode do assalto, `EconomyReport` (se o teto mudar).
- Aceite: validação padrão; PlayMode verde; gate 3× no chefe do mundo 4 com o assalto mais pesado.
- Exige o usuário: OPPO conectado.

**RT-6t.8 — Cenário**
- Escopo: `InstancedProps` (reciclagem por Z), `StudioKit/StripedTrack`; receitas de árvore (2 tons + tronco) e
  pedra, pista listrada e trilhos, chão, densidade por mundo; substitui o cenário Simple Lit dos 4 mundos.
- Arquivos: `Assets/StudioKitIncubator/Swarm/InstancedProps*`, shader da pista; `WorldScenery.cs`,
  `Editor/WorldSceneryArt.cs`, `Editor/WorldContent.cs` (só cenário), dados dos mundos,
  `ClaybreakSceneBuilder.cs` (só cenário/pista).
- Aceite: validação padrão; ≤ 3 chamadas para o cenário lateral; capturas dos 4 mundos; gate fica para a 6t.12.

**RT-6t.9 — Portões e feedback**
- Escopo: painéis semitransparentes verde #5ad1a0 / vermelho #ff6b81 (0,45), postes, rótulos TextMeshPro com
  Baloo 2 ExtraBold e contorno (corrida e fileira do assalto), número flutuante (pool de 4), pop do contador,
  shake no portão ruim; Baloo 2 no HUD e nas telas (troca de fonte do `UI.Text`); fonte baixada só de
  `github.com/google/fonts` (ofl/baloo2), com `OFL.txt` e hash; glifos `+ − × ÷` e acentos conferidos.
- Arquivos: `RunGateCourse.cs` (visual), `AssaultField.cs` (só `DressGates`), `NumberLabels.cs`,
  `ClaybreakHud.cs`, `ClaybreakScreens.cs` (fonte), `Assets/Claybreak/Content/Fonts/**`,
  `ClaybreakSceneBuilder.cs` (só portões/HUD), testes PlayMode de HUD.
- Aceite: validação padrão; capturas; licença registrada.

**RT-6t.10 — Arte do assalto**
- Escopo conforme **D28** (sem resposta: A — `ToonLit` com as cores de torre/defesa da paleta nas torres,
  defesas, chefes, projéteis, caixotes e efeitos atuais, sem remodelar e sem rostos).
- Arquivos: `Editor/TowerArt.cs`, `Editor/ArtPalette.cs`, materiais do jogo, dados dos mundos.
- Aceite: validação padrão; capturas dos 4 chefes; aprovação visual do usuário como aceite pendente.

**RT-6t.11 — Reserva para OpenGL ES**
- Escopo: caminho `RenderMeshInstanced` em blocos com variante de shader própria e matrizes de sombra no mesmo
  job; escolha em tempo de execução por `SystemInfo`; teste no OPPO forçando OpenGL ES 3.
- Arquivos: `Assets/StudioKitIncubator/Swarm/SwarmRenderer*`, shaders da multidão e da sombra, testes.
- Aceite: validação padrão; as duas rotas desenham igual (captura); gate 3× no OPPO em GLES com o teto da D27.

**RT-6t.12 — Levar ao kit e fechar**
- Escopo: mover `Assets/StudioKitIncubator/**` para o pacote (`Runtime/Toon`, `Runtime/Swarm`, `Shared/Shaders`,
  `Tests/Editor`) com os `.meta`, dependências no `package.json`, versão 0.5.0, K.11 no plano do studiokit;
  aceitação do studiokit; retirar do jogo o caminho antigo de massinha (prefab, pool, `RunnerSkin`/
  `RunnerDeathEffects` no jogo, materiais e assets sem uso, Roboto se sem uso); `ArtDirectionCapture` e
  capturas novas; nível do gate (corrida + assalto mais pesado + cenário); gate 3× final; atualizar GDD seção 9,
  `reuso-tecnico.md` 5.3 e `rt6t-crowd-toon.md`.
- Aceite: validação padrão completa; isolamento do kit PASS sem a incubadora; Wild Pack igual à linha de base;
  gate 3× aprovado; nenhum asset órfão; documentos coerentes.
- Exige o usuário: OPPO conectado; jogar os 4 mundos (aceite pendente).

---

## 5. Decisões que precisam do usuário

Cinco fichas. Ordem de envio (no máximo 3 por vez): **D25, D27, D26** primeiro; depois **D28** e **D29**.
Nenhuma trava a RT-6t.1. D25 trava a 6t.2; D27 só limita o teto (sem resposta, fica 300); D26 trava a 6t.6;
D28 tem padrão seguro (A) na 6t.10; D29 trava a RT-9.

### 5.1 Decidido pelo planejamento, sem ficha (o usuário pode trocar quando quiser)

- **4ª paleta (o jogo tem 4 mundos, o pedido tem 3).** Mundo 1 = Pastel, 2 = Pôr do sol, 3 = Doce, 4 = proposta
  "Aurora", provisória como foi a D20 (trocar é editar um arquivo): céu #2c3e66 → #9ad0c2, neblina #a9cfc4, chão
  #5e8f88, pista #f2efe4 / faixa #e3dcc8, multidão #c9f2a0, árvore #2f6f73 / #4f9a8f, tronco #3d3b52, pedra
  #c3b5e0, UI #ff8a5c / tinta #22313f. A multidão verde-limão evita repetir o rosa do Pastel. Os temas dos
  mundos (pântano, fundição, gelo) passam a ser só torre/chefe e regra; a cor vem da paleta (ver D28).
- **Fonte Baloo 2** (OFL 1.1, grátis, uso comercial permitido): técnica, T-RT6t-6.

### DECISÃO D25 — O visual novo é "fofo": para quem o jogo é declarado na Play?

O que você está aprovando: **adotar o visual do protótipo como está (bolinhas e pintinhos com olhos, cores
pastel) e continuar declarando na Play que o público-alvo é de 13 anos para cima (faixas 13–15, 16–17 e 18+), sem
as faixas infantis; responder "sim" se a Play perguntar se a ficha pode atrair crianças sem querer, para receber
a marca "Não é destinado a crianças"; e compensar na loja: ícone e capturas mostrando o desafio (a multidão
contra a torre, números grandes), sem rostos em destaque no ícone, textos sem palavras infantis. A regra antiga
do GDD "sem bichinhos fofos / nada de olhos" deixa de valer para a multidão; continua valendo para o ícone e
para as torres e chefes (sem rosto). Uma variante "sem olhos" das formas fica pronta como reserva.**

Por que agora: trava a RT-6t.2 (as formas finais, com ou sem olhos). Também define a ficha da loja na RT-9.

Contexto que você precisa saber:
- Fato (GDD seção 2 e decisão P3 sua, herdada do Wild Pack): o jogo foi planejado para 13+, "não direcionado a
  crianças", com "direção adulta": sem bichinhos fofos, nada de olhos enormes nem sorrisos no ícone. Na D17 as
  torres e chefes ficaram "sem rosto nem olhos". Isso veio da "adultização" do Wild Pack, feita porque a Play
  poderia achar que o app atrai crianças.
- Memória não verificada hoje (regras da Play, de cor): o desenvolvedor escolhe faixas de idade do público
  (até 5, 6–8, 9–12, 13–15, 16–17, 18+). Quem escolhe só 13+ responde se o app pode atrair crianças sem querer;
  pode receber a marca "Não é destinado a crianças". Se a Play entender que o app é **para** crianças, entram as
  regras de "Famílias": anúncios só de redes certificadas, sem anúncio personalizado para menores, mais cuidado
  com dados. A sessão da RT-9 confere isso na Central de Ajuda da Play antes de preencher.
- Fato (estado atual): anúncios do AdMob e medição Firebase só com consentimento (RT-7, RT-8b).
- Inferência nossa: bolinhas com olhos e um pintinho em pastel parecem infantis para muita gente. A Play julga
  o conjunto (ícone, capturas, textos, personagens), não um detalhe.
- Inferência nossa (segunda opinião): a diferenciação do jogo era "massinha adulta, não o neon colorido dos
  jogos da Voodoo"; bolinhas pastel aproximam o visual dos clones do gênero. Não temos dado que diga se isso
  ajuda ou atrapalha na loja.
- O código das entregas é o mesmo em todas as opções; muda a apresentação e, na C, um detalhe das formas.

Opções:
A) Adotar como está, público 13+ com a marca "não destinado a crianças", loja compensando (recomendada).
   + Fica o visual que você aprovou; nada muda em anúncios ou dados.
   - Riscos: a Play questionar o público na revisão ou depois (chance: média; impacto: alto — mudar anúncios,
     dados ou visual às pressas; depois de publicado, uma ação de política não se desfaz). Mitigação: ícone e
     capturas focados no desafio, checagem com a lista da adultização na RT-9, variante sem olhos pronta.
   Custo: nenhum agora. Dá para voltar atrás? sim antes de publicar; com custo depois.
B) Público misto (crianças e adultos), com uma tela neutra de idade na primeira abertura: menores recebem
   anúncios e medição no modo infantil, adultos no modo normal.
   + Declaração honesta para um visual fofo; adultos continuam como hoje.
   - Riscos: entra nas regras de Famílias para a parte infantil (chance: alta; impacto: médio) — redes de
     anúncio certificadas, política e Data safety revistas, menos receita nessa parte.
   Custo: ~1–2 entregas (tela de idade, anúncios, política). Volta atrás? com custo.
C) Adotar com ajustes "menos infantis": mesmas formas e cores, mas sem olhos (ou olhos bem pequenos); o
   Pintinho fica como segunda forma, não como padrão.
   + Reduz a leitura infantil mantendo o estilo.
   - Riscos: foge do protótipo que você aprovou; pode não bastar para a Play (chance: média; impacto: médio).
   Custo: pequeno (receita). Volta atrás? sim, fácil.
D) Adiar até a RT-9.
   + Decide com a ficha da loja na mão.
   - Custo de adiar: a RT-6t.2 faz as formas com olhos e a variante sem olhos; se a resposta mudar o público
     (B), o trabalho de anúncios cai em cima do lançamento de teste.

Recomendação: A — é o visual que você escolheu; o risco está na apresentação da loja, que controlamos, e a
variante sem olhos é uma reserva barata.
Segunda opinião (fable): concorda com A, desde que a ficha diga como a Play realmente pergunta (faixas e marca
"não destinado a crianças"), revogue de forma explícita a regra do GDD só para a multidão e mantenha a regra do
ícone; pediu a opção de público misto (B) e a variante sem olhos de reserva; apontou o risco de perder a
diferenciação "massinha adulta". Tudo incorporado. Regras da Play citadas de memória por nós e por ela.
Se você não responder: a RT-6t.1 segue; a 6t.2 faz as formas como no protótipo **e** a variante sem olhos, e
para antes de escolher qual vai para o jogo.

Responda: A, B, C ou D, ou "explica melhor".

---

### DECISÃO D27 — Quantos bonecos a multidão pode ter durante o jogo?

O que você está aprovando: **na corrida, a multidão pode chegar a 1000 bonecos na tela (o motor novo aguenta, se
passar no teste do celular); no ataque à torre, continuam saindo no máximo 300 bonecos, e quando a multidão
passa de 300 cada boneco lançado vale mais (por exemplo, 1000 na corrida = 300 lançados valendo 3,3 cada, e o
contador mostra o total). A vida das torres e as 40 fases são recalculadas pela tabela de economia para o
jogo continuar com a mesma dificuldade e o mesmo tempo de fase.**

Por que agora: define o teto que a RT-6t.4 (corrida) e a 6t.7 (assalto) usam. Sem resposta, o teto fica em
300, como hoje, e o motor novo só mostra 1000 na cena de teste.

Contexto que você precisa saber:
- Fato (código): hoje o exército tem teto de 300 (os portões não passam disso), e o ataque aguenta 300 bonecos
  em voo; o teste de desempenho no celular (gate 3×) mede exatamente esse caso.
- Fato (seu pedido): "1000+ unidades a 60 fps em Android intermediário".
- Fato (D12, D19, RT-6): 4 mundos × 10 fases, vida das torres e tempos ajustados para esse teto; a loja e as
  moedas por fase dependem do ritmo das fases.
- Inferência nossa: no ataque, cada boneco em voo conversa com canhão, muralha, escudo, varredor e torre; 1000
  em voo é o caso mais pesado do jogo e o mais arriscado no celular. Na corrida, 1000 é só desenho e formação
  (barato no motor novo). Lançar 1000 com a mesma velocidade triplicaria o tempo do ataque, a não ser que a
  velocidade de lançamento também suba.
- Inferência nossa: ver a multidão crescer até 1000 é o "momento de espetáculo" do gênero.

Opções:
A) 1000 na corrida; no ataque, até 300 em voo, cada um valendo mais (recomendada).
   + Espetáculo na corrida; ataque com o mesmo peso no celular e o mesmo tempo; dificuldade recalculada.
   - Riscos: o jogador perceber que "saíram menos do que eu tinha" (chance: média; impacto: baixo) — mitigação:
     contador do exército diminui pelo valor real e os bonecos especiais ficam maiores; reequilíbrio das fases
     (chance: média; impacto: médio — ajuste por dado, sem mexer na loja).
   Custo: parte das entregas 6t.4 e 6t.7. Volta atrás? sim, é dado.
B) 1000 na corrida e 1000 no ataque (lançamento mais rápido).
   + Mais fiel ao "1000 bonecos" em todo o jogo.
   - Riscos: o ataque vira o caso mais pesado de todos e pode não passar no gate (chance: média; impacto: alto
     — cortar de volta); fases e economia mudam mais (chance: alta; impacto: médio).
   Custo: + medição e ajuste; talvez +1 entrega. Volta atrás? sim.
C) Teto intermediário (ex.: 600) nas duas fases.
   + Meio-termo de espetáculo e risco.
   - Riscos: menos impacto visual que A na corrida; ajuste de fases do mesmo jeito.
   Custo: igual a A. Volta atrás? sim.
D) Manter 300 (como hoje).
   + Nada muda no equilíbrio nem na loja.
   - Riscos: o motor de 1000 fica sem uso no jogo; o visual perde o "momento de espetáculo" (chance: alta;
     impacto: médio).
   Custo: nenhum. Volta atrás? sim.

Recomendação: A — dá o espetáculo que você pediu onde ele é barato (corrida) e mantém o ataque, que é o
pesado, no limite já aprovado no celular.
Segunda opinião (fable): apontou que subir o teto mexe em portões, vida das torres, nas 40 fases e nos tempos
da D19, e por isso é decisão sua, não técnica; ficha criada por causa disso. Não opinou entre as opções.
Se você não responder: o teto fica em 300 em tudo (opção D); o motor já é feito para 1000 e a troca é dado.

Responda: A, B, C ou D, ou "explica melhor".

---

### DECISÃO D26 — Como ficam as skins da loja com as 2 formas e as cores por mundo

O que você está aprovando: **(1) as duas formas, Bolinha e Pintinho, ficam livres para o jogador escolher no
menu, sem custo, e a escolha fica gravada no save; (2) a skin grátis padrão (BARRO) passa a ser "cor do mundo":
a multidão usa a cor da paleta de cada mundo; (3) as outras 8 skins da loja continuam sendo uma cor que vale em
todos os mundos, com os mesmos preços, mas com cores e nomes novos que combinem com o estilo (em vez de cores de
argila); (4) elites continuam com tamanho e cor próprios.**

Por que agora: trava a RT-6t.6 (menu, save e loja). As entregas 6t.1–6t.5 seguem sem ela.

Contexto que você precisa saber:
- Fato (RT-5, RT-6, D19 = A): a loja tem 9 skins que são só uma cor da massinha: BARRO (grátis, padrão), BRASA,
  MUSGO e outras três da RT-5, e MIRE (pântano, 1.500), CINDER (fundição, 2.000) e FROST (gelo, 2.500), criadas
  nas cores dos mundos na RT-6; total da loja 19.380 moedas, com preços aprovados por você.
- Fato (seu pedido): cada mundo tem uma paleta que também define a cor da multidão (rosa no Pastel, creme no Pôr
  do sol, amarelo no Doce); 2 formas "selecionáveis".
- Fato: o jogo ainda não foi publicado; não há jogadores com compras reais. Só o save de teste muda.
- Inferência nossa: as cores atuais das skins são escuras/terrosas (direção adulta antiga) e destoam do pastel;
  MIRE/CINDER/FROST perdem o sentido quando as cores dos mundos mudam.
- O motor novo pinta só o corpo (olhos, bico e folhas não mudam), então qualquer opção é barata no código. O
  save ganha um campo novo (a forma), com migração automática.

Opções:
A) Formas livres no menu; BARRO vira "cor do mundo"; as outras 8 skins viram 8 cores novas no estilo, mesmos
   preços e mesma ordem (recomendada).
   + Economia aprovada intacta; o jogador vê a cor do mundo por padrão; quem compra tem uma cor sua em todo lugar.
   - Riscos: alguma cor de skin destoar de uma paleta ou confundir com o verde/vermelho dos portões (chance:
     média; impacto: baixo) — cores escolhidas contra as 4 paletas e conferidas em captura.
   Custo: incluído na 6t.6. Volta atrás? sim, fácil.
B) Igual a A, mas o Pintinho é uma recompensa: liberado ao terminar o mundo 1 (grátis, por progresso).
   + Um objetivo a mais sem mexer em preço.
   - Riscos: metade do que você pediu como "selecionável" fica escondida no começo (chance: alta; impacto: baixo).
   Custo: pequeno. Volta atrás? sim.
C) Skins deixam de ser cor: viram formas e enfeites (chapéus, broto colorido…); a cor vem sempre da paleta.
   + Visual sempre harmônico; loja mais "colecionável".
   - Riscos: precisa criar 8 itens novos e reprecificar (D19 reaberta) (chance: alta; impacto: médio no prazo).
   Custo: ~1 entrega a mais. Volta atrás? com custo (sem jogadores reais, o save não é problema).

Recomendação: A — mantém o que você já aprovou na loja e entrega as duas formas como pedido; C pode vir depois
do lançamento de teste, se os dados mostrarem que a loja precisa de mais atrativo.
Segunda opinião (fable): concorda com A, corrigindo a ficha: são 9 skins, não 6; MIRE/CINDER/FROST eram as
cores dos mundos; o save ganha o campo da forma; definir o que é "sem skin" (BARRO); sem jogadores reais, a C
custa menos; lembrar o contraste das cores com a pista e os portões; faltava a opção de forma por progresso (B).
Incorporado.
Se você não responder: as entregas 6t.1–6t.5 seguem; a 6t.6 para esperando.

Responda: A, B ou C, ou "explica melhor".

---

### DECISÃO D28 — Torres, defesas, chefes e o ataque no estilo novo

O que você está aprovando: **tudo o que aparece no ataque à torre e que o protótipo não mostra (torres, canhão,
muralha, escudo, varredor, chefes, projéteis, caixotes e efeitos) ganha o mesmo sombreado de 3 faixas e cores
novas de "torre" e "defesa" definidas em cada paleta, sem refazer os modelos e sem rostos. As capturas dos 4
mundos vão para você; se quiser modelos novos no estilo arredondado, isso vira uma entrega própria. Os temas dos
mundos (pântano, fundição, gelo) continuam nas formas das torres e chefes e nas regras; as cores passam a ser
as das paletas.**

Por que agora: trava a RT-6t.10 (com padrão seguro se você não responder). Seu pedido descreve multidão,
cenário e portões; o ataque, metade do jogo, não tem referência aprovada.

Contexto que você precisa saber:
- Fato (D17 = A, D20 = A): torre "bastião de pedra clara" e chefe "cidadela" no mundo 1; torres e chefes próprios
  nos mundos 2–4 (pântano, fundição, gelo), provisórios até o lançamento de teste; montados por código com
  formas simples; regra "sem rosto nem olhos".
- Inferência nossa: como as torres já são formas simples, o sombreado de 3 faixas e as cores da paleta devem
  combinar com o estilo; o maior choque seria manter o material antigo de massinha.
- Inferência nossa: remodelar no estilo "arredondado de brinquedo" daria mais unidade, ao custo de ~2 entregas.
- Fato: a paleta que você mandou não tem cores de torre; elas seriam propostas por nós e mostradas em captura.

Opções:
A) Pintar o que existe (sombreado + cores da paleta), sem rostos (recomendada).
   + Rápido (1 entrega); visual coerente; regras do jogo intactas.
   - Riscos: as formas retas das torres destoarem das bolinhas (chance: média; impacto: médio) — você vê as
     capturas e pode pedir C depois.
   Custo: 1 entrega. Volta atrás? sim.
B) Pintar tudo e remodelar só a torre do mundo 1 no estilo arredondado, como amostra para comparar.
   + Você compara as duas no mesmo mundo antes de decidir o resto.
   - Riscos: mundo 1 diferente dos outros até a decisão (chance: alta; impacto: baixo).
   Custo: ~1,5 entrega. Volta atrás? sim.
C) Remodelar tudo no estilo arredondado.
   + Unidade total de estilo.
   - Riscos: +2 entregas antes do lançamento de teste (chance: alta; impacto: médio no prazo).
   Custo: ~2–3 entregas. Volta atrás? sim.
D) Deixar como está (massinha antiga no ataque).
   + Zero trabalho.  - Riscos: dois estilos na mesma tela (chance: alta; impacto: alto na percepção).

Recomendação: A — coerência rápida e barata; B ou C depois, se as capturas pedirem.
Segunda opinião (fable): concorda com A; apontou que a paleta não tinha cores de torre (agora tem campos novos),
que o ataque inteiro não tem referência aprovada, a regra "sem rostos" e a opção de uma torre de amostra (B).
Incorporado.
Se você não responder: a 6t.10 aplica A (barato e reversível) e as capturas vão para você.

Responda: A, B, C ou D, ou "explica melhor".

---

### DECISÃO D29 — O nome "Clay Siege" sem massinha

O que você está aprovando: **antes da RT-9 (preparação do lançamento de teste), escolher o nome definitivo
entre "Clay Siege" e até 3 nomes novos que combinem com o visual de bolinhas, todos checados nas bases de marca
gratuitas (EUIPO, INPI Portugal, TMview); até lá, "Clay Siege" segue como nome de trabalho.**

Por que agora: não trava código. Trava a RT-9 (ícone, ficha da loja, domínio). Levantada agora porque o visual
novo tira a "massinha" que deu origem ao nome.

Contexto que você precisa saber:
- Fato (D1, D18): nome de trabalho "Claybreak", trocado para "Clay Siege" na RT-4 por conflito de marca; a
  checagem oficial à mão (INPI, EUIPO, TMview) ainda está pendente para antes da RT-9.
- Fato: o visual novo é toon/brinquedo, sem massinha; skins com nomes de argila (BARRO, BRASA, MUSGO) já serão
  renomeadas na D26; os sons foram feitos "com cara de argila" (D22 = A) e continuam aprovados.
- Inferência nossa: "Clay" num jogo sem massinha confunde quem vê o ícone; como a checagem de marca ainda não
  foi feita, trocar agora custa pouco.

Opções:
A) Manter "Clay Siege" (o "clay" vira só marca).
   + Nada muda; checagem já começou.
   - Riscos: nome e visual desencontrados na loja (chance: média; impacto: médio na conversão).
   Custo: zero. Volta atrás? sim, até publicar.
B) Gerar 3 nomes novos no estilo, checar marca de graça e você escolhe entre eles e "Clay Siege" antes da RT-9
   (recomendada).
   + Nome coerente com o visual, escolhido com a checagem feita.
   - Riscos: nenhum nome novo passar na checagem (chance: baixa; impacto: baixo — fica "Clay Siege").
   Custo: ~meia sessão de pesquisa + sua escolha. Volta atrás? sim, até publicar.
C) Adiar para depois do lançamento de teste.
   + Decide com dados.  - Riscos: trocar nome depois de publicado perde avaliações e busca (chance: alta;
   impacto: médio).

Recomendação: B — a checagem de marca já tinha de ser feita antes da RT-9; aproveitar para testar nomes que
combinem com o visual custa pouco.
Segunda opinião (fable): apontou que o nome, os nomes das skins e os sons "de argila" ficam órfãos sem massinha
e que isso precisa de ficha antes da RT-9; ficha criada por causa disso. Não opinou entre as opções.
Se você não responder: "Clay Siege" segue como nome de trabalho; a RT-9 não fecha a ficha da loja sem resposta.

Responda: A, B ou C, ou "explica melhor".


---

## 6. Decisões técnicas (tomadas pelo planejamento; registradas em `decisoes.md`)

- **T-RT6t-1 — Motor com cara de kit, incubado aqui, levado ao kit numa entrega só.** O usuário quer o padrão
  visual "para os jogos" e o studiokit como motor comum (visão de 2026-11-19); por isso shader toon, construtor
  de malhas, simulação instanciada, câmera, cenário reciclado, tipo de paleta e overlay de FPS são escritos com
  namespaces e asmdefs do kit, sem citar o jogo. Mas até a integração no jogo provar a API (6t.4–6t.11), ficam
  em `Assets/StudioKitIncubator/` neste repositório: assim cada entrega escreve num repositório só (regra
  global), não há cinco aceitações do Wild Pack para assemblies que ele não usa, e a API não congela antes do
  segundo uso real (princípio da K1). A RT-6t.12 move tudo com os `.meta`. O `RunnerCrowd`/`RunnerPool`/
  massinha do kit **não** mudam (o Wild Pack depende deles).
- **T-RT6t-2 — Simulação na CPU com Burst, desenho indireto.** Itens 7 e 9; vertex shader é o plano B medido.
- **T-RT6t-3 — Shader HLSL próprio, não Shader Graph.** Item 1.
- **T-RT6t-4 — Primeira entrega é um portão de desempenho** e mede no teto do orçamento (500 tri, 1000 vivos,
  sombras, MSAA 4×), para as seguintes não reabrirem o gate por surpresa.
- **T-RT6t-5 — Integração cedo**: a corrida do jogo recebe o `Swarm` na 6t.4, antes de paletas e cenário (o maior
  risco de jogabilidade é trocar o `RunnerCrowd`).
- **T-RT6t-6 — Baloo 2 (OFL 1.1) substitui a Roboto Black** nos portões, HUD e telas; TMP só onde há contorno.
- **T-RT6t-7 — Caminho GLES de reserva** antes do soft launch (6t.11), não no portão.
- **T-RT6t-8 — Numeração**: RT-6t.x aqui; K.11 no studiokit (K.10 foi a direção revertida).
- **T-RT6t-9 — Série sequencial** por padrão (regra global de uma sessão por repositório); pares sem conflito
  listados na seção 4 só com autorização do usuário.
