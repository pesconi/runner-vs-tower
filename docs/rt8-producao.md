# RT-8 — Produção: polimento, áudio e gate de release (relato da entrega)

Registros técnicos: T-RT8-1 e T-RT8-2 em [`decisoes.md`](decisoes.md); no kit, T21 e a entrega K.8 (pacote
0.3.0). Aceite do usuário pendente: ficha **D22** (ouvir e aprovar o som), no fim de `decisoes.md`.

## O que entrou

| Peça | Onde | Como funciona |
|---|---|---|
| Áudio definitivo (próprio) | `Tools/audio/clay_siege_audio.py` → `Content/Audio/*.wav`; licenças em [`audio-licenses.md`](audio-licenses.md) | 20 efeitos + 2 músicas em laço, gerados por síntese (sem amostras de terceiros); o validador recusa clipe fora da lista de licenças ou arquivo sem uso |
| Sons novos do assalto | `CannonDefense`, `AssaultTower` | **aviso de projétil** (assobio de queda 0,6 s antes da pedra cair), impacto da pedra, baque de cada golpe na torre (antes tocava a moeda) |
| Sons do jogo no kit | kit `ThemeAudio.ExtraSounds` + `GameAudio.PlayExtra` | os 10 sons próprios do jogo (canhão, queda da torre, andar, muro, varredor, caixote, chefe, aviso, impacto, golpe) viraram a tabela de "sons extras" do kit (pauta 5.1 da RT-4): mesmas vozes, botão de som e pausas dos papéis fixos; o componente de vozes próprio saiu |
| Vibração revisada | `ClaybreakAudio` (tabela) + kit `GameHaptics` | só nos momentos grandes (portões, caixote, muro/escudo, andar, queda da torre, chefe, derrota, recompensa); nunca por unidade perdida nem por pedra; cada vibração sem alocação de memória (JNI em cache) |
| Fonte própria | `Content/Fonts/Roboto-Black.ttf` (+ licença Apache 2.0 ao lado) | toda a interface e os números dos portões |
| Tela de ajustes | `ClaybreakScreens` (SETTINGS no canto superior esquerdo) | som, música, vibração, link da política de privacidade (dado `MonetizationContent.PrivacyPolicyUrl`, vazio até a RT-9: o botão avisa), opções de privacidade (só onde o UMP exige) e créditos |
| Área segura do celular | kit `SafeAreaFitter` | HUD e conteúdo dos menus dentro da área segura (recorte da câmera, cantos, barra de gestos); o véu escuro cobre a tela toda |
| Teste instável do pool | kit `RunnerPoolTests` | aceita a primeira de até 5 medições limpas (o Editor aloca às vezes nos próprios avisos de hierarquia) |
| Build de release | `ClaybreakBuild` (`-claybreakAab`), `Tools/rt4-check.sh` passo `aab` | AAB e APK como player de release; chave de debug sem `CLAYBREAK_KEYSTORE`; chave de upload só por variáveis de ambiente, fora do repositório, sem gravar nada no ProjectSettings |

## Resultados

| Verificação | Resultado |
|---|---|
| Cena, validador do jogo (inclui licenças de áudio e monetização), isolamento do kit | PASS |
| Testes EditMode (kit + jogo) | 124/124 (novos: tabela de sons extras, área segura, link https da política) |
| Testes PlayMode | 13/13 (novo: tela de ajustes) |
| Capturas | `docs/art/telas/` (nova: `jogo-ajustes.png`) |
| Wild Pack com o kit 0.3.0 | completude 90/0 (o mesmo aviso de antes), assets 117/0, isolamento PASS, `AudioValidation` 16 PASS, save 87/87 — igual à linha de base |
| Revisão de segurança (revisor independente, só leitura) | nada grave; 1 médio e 4 baixos corrigidos: a chave de upload agora só é aplicada depois de conferir as 4 variáveis (antes, uma faltando deixava o projeto com a chave ligada), recusa chave dentro de qualquer repositório git, APK de medição nunca usa a chave de upload, link da política só abre endereço https válido, vibração protegida contra exceção e entrada nula na tabela de sons |
| **AAB de release** | **34,6 MB** (36.241.034 bytes), versão 0.1.0 (1), chave de debug — `C:\temp\rt8\Claybreak.aab` |
| APK de release (anúncios de teste) | 34,8 MB (36.523.570 bytes) — `C:\temp\rt8\Claybreak.apk`, **instalado no OPPO** |
| APK de medição | 34,9 MB — `C:\temp\rt8\Claybreak-measure.apk` (fechado no celular depois do gate) |
| **Gate 3× (2026-11-21, OPPO CPH2791, 60 fps, build de release, som + música + vibração ligados, chefe do mundo 4, 299 em voo, 12/12 pedras)** | quadro p95 16 ms, 0,0% > 50 ms, 0 quadros perdidos; CPU principal p95 3,9–4,8 ms; GPU p95 4,0–4,3 ms; alocação 0 KB e **0 coletas** em todas as janelas; **PSS 228 MB** — **passou** (`C:\temp\rt2\gate-rt8-w4.txt`) |

## Pendências

- **Usuário (aceite da RT-8):** ouvir o som e responder a ficha D22 (roteiro de 5 minutos na própria ficha).
- **Usuário (RT-9): criar a chave de upload real** (keystore) fora do repositório — por exemplo
  `C:\Users\<você>\keys\clay-siege-upload.jks` com `keytool -genkeypair -v -keystore <arquivo> -alias upload
  -keyalg RSA -keysize 2048 -validity 10000` — e guardar senha e cópia num lugar seguro (perder a chave de
  upload exige pedir troca à Play). O build lê `CLAYBREAK_KEYSTORE`, `CLAYBREAK_KEYSTORE_PASS`,
  `CLAYBREAK_KEY_ALIAS` e `CLAYBREAK_KEY_PASS` do ambiente; nada disso entra no git.
- **RT-9:** publicar a política de privacidade num endereço https e pôr o endereço em
  `MonetizationContent.PrivacyPolicyUrl` (o validador recusa build de produção sem ele); versão do app
  (`bundleVersion`/`versionCode`) da primeira versão da loja.
- **Antes da RT-9 (sessão própria, D21 = A):** Firebase Analytics.
