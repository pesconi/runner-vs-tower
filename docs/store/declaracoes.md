# Declarações da Play para o Stampeep: respostas para copiar (RT-9a)

Respostas para o usuário preencher no Play Console (passo a passo em
[`passo-a-passo-console.md`](passo-a-passo-console.md)). **Nada foi enviado pela IA.** Cada resposta tem a evidência
do código; o que depende do texto exato do formulário (que o Google não publica) está marcado **[conferir no
Console]**. Regras e fontes: [`requisitos-play.md`](requisitos-play.md). Substitui o rascunho
[`../data-safety.md`](../data-safety.md) (RT-7/RT-8b), que fica como histórico do levantamento.

Versão do código: `Stage RT.6t.21` + RT-9a (só o endereço da política mudou). Se o código mudar (IDs reais do AdMob,
intersticial ligado, pedido próprio de consentimento da D23, outra medição), este documento, a política e a
seção "Segurança dos dados" mudam juntos.

## 0. O que o app coleta e envia (levantamento do código)

| Fato | Evidência |
|---|---|
| Dois SDKs falam com a rede: Google Mobile Ads (AdMob) com UMP, e Google Analytics for Firebase. Mais nada (sem servidor próprio, sem `UnityWebRequest`/`HttpClient`/`WWW` no jogo) | `Packages/manifest.json` (kit → `com.google.ads.mobile` 11.6.0); `Assets/Plugins/Android/FirebaseAnalytics.androidlib` (`firebase-analytics` 24.0.0); `MonetizationValidation` recusa billing, outros SDKs de analytics e outros produtos Firebase |
| Nenhum anúncio antes da resposta do UMP | kit `MobileAdsGate` (`CanRequestAds` antes de `MobileAds.Initialize` e de qualquer `Load`) |
| Anúncio premiado opcional (dobrar moedas, continuar uma vez); intersticial pronto e **desligado** | `ClaybreakFlow`; `MonetizationContent.InterstitialOn = false` (D6) |
| Anúncios com classificação PG, sem marca de público infantil nem de menor | kit `MobileAdsGate` (`MaxAdContentRating.PG`, `TagForChildDirectedTreatment.False`, `TagForUnderAgeOfConsent.False`) |
| Firebase: 4 eventos próprios sem ID (`game_session`, `level_start`, `level_end`, `rewarded_ad`) + eventos automáticos (`first_open`, `session_start`, `user_engagement`, `app_update`, `os_update`, `app_clear_data`, `app_remove`) com o ID da instância do app e a origem da instalação | kit `PlayerMetrics`, `FirebaseAnalyticsSink`; `ClaybreakMetrics` |
| Firebase só com consentimento (TCF propósito 1 + 8 ou 9, fornecedor Google); fora do EEE/Reino Unido/Suíça hoje não mede | kit `MeasurementConsent`; `ClaybreakMetrics.GameConsentGiven = false` (D23 = B) |
| Firebase sem ID de publicidade, sem ID do Android, sem sinais de anúncio, sem ecrãs automáticos | manifesto do módulo (`google_analytics_adid_collection_enabled=false`, `..._ssaid_...=false`, sinais de anúncio negados, `automatic_screen_reporting=false`) |
| Retirar o consentimento apaga os dados de análise da instalação | `FirebaseAnalyticsSink.SetCollectionEnabled(false)` → `resetAnalyticsData` |
| Save só local (PlayerPrefs): progresso, moedas, compras com moedas, skins, formas, primeiro dia, presente diário, som/música/vibração | `SaveData.cs` (save v5), `SaveStore`; kit `GameAudio`, `GameHaptics` |
| Sem login, conta, nome, e-mail, chat, conteúdo de outros jogadores | sem código de rede próprio; nenhum campo de texto no jogo |
| Sem compras com dinheiro | sem `com.unity.purchasing` nem permissão `BILLING` (validador) |
| Link externo único: a política de privacidade (abre o navegador) | `ClaybreakScreens` (botão da política em SETTINGS), endereço `MonetizationContent.PrivacyPolicyUrl` |
| Estatística de hardware do Unity e serviços Unity desligados | `ProjectConfiguration` (`submitAnalytics = 0`), `UnityConnectSettings.asset` |

Permissões do APK (`aapt2 dump permissions`, RT-8b; sem mudança desde então): `INTERNET`, `ACCESS_NETWORK_STATE`,
`com.google.android.gms.permission.AD_ID`, `ACCESS_ADSERVICES_AD_ID`, `ACCESS_ADSERVICES_ATTRIBUTION`,
`ACCESS_ADSERVICES_TOPICS`, `WAKE_LOCK`, `FOREGROUND_SERVICE`, `BIND_GET_INSTALL_REFERRER_SERVICE`, `VIBRATE` e a
interna `DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION`. Nenhuma de câmara, microfone, contactos, localização ou
armazenamento.

## 1. Segurança dos dados (Conteúdo do app > Segurança dos dados)

Base: o que o SDK de anúncios declara ("recolhe e partilha" IP/localização aproximada, interações, diagnóstico e IDs
do dispositivo, para publicidade, análise e prevenção de fraude, cifrado em trânsito —
[AdMob](https://developers.google.com/admob/android/privacy/play-data-disclosure)) e o que o Firebase Analytics
recolhe (ID da instância, IP mascarado para localização aproximada, eventos de uso, para análise —
[Analytics](https://support.google.com/analytics/answer/11582702)).

**Perguntas gerais**

| Pergunta | Resposta | Por quê |
|---|---|---|
| O app recolhe ou partilha algum dos tipos de dados exigidos? | **Sim** | AdMob e Firebase |
| Todos os dados recolhidos são cifrados em trânsito? | **Sim** | os dois SDKs usam HTTPS (página do AdMob: "encrypted in transit") |
| Fornece uma forma de os utilizadores pedirem a eliminação dos dados? | **Sim** — pelo e-mail da política e no jogo (botão PRIVACY retira o consentimento e apaga os dados de análise da instalação) **[conferir o texto]** | política, `resetAnalyticsData` |
| O app permite criar conta? | **Não** | sem login |

**Tipos de dados** (marque só estes; todo o resto = não recolhido)

| Categoria > tipo | Recolhido | Partilhado | Efémero? | Obrigatório ou opcional | Finalidades |
|---|---|---|---|---|---|
| Localização > **Localização aproximada** | Sim | Sim | Não | Obrigatório | Publicidade ou marketing; Prevenção de fraude, segurança e conformidade; Análise |
| Atividade no app > **Interações com o app** | Sim | Sim | Não | Obrigatório | Análise; Publicidade ou marketing; Prevenção de fraude, segurança e conformidade |
| Informações e desempenho do app > **Diagnóstico** | Sim | Sim | Não | Obrigatório | Análise; Prevenção de fraude, segurança e conformidade |
| Dispositivo ou outros IDs > **Dispositivo ou outros IDs** | Sim | Sim | Não | Obrigatório | Publicidade ou marketing; Análise; Prevenção de fraude, segurança e conformidade |

Notas:
- "Obrigatório" porque o SDK de anúncios envia estes dados ao pedir qualquer anúncio (o UMP decide só a
  personalização). O Firebase sozinho seria "opcional" (só com o "sim"), mas o tipo é o mesmo do AdMob, por isso
  vale o mais restritivo.
- "Partilhado" vem do AdMob (o Google como terceiro para anúncios). O Firebase age como prestador de serviço (não conta
  como partilha), mas os tipos já estão marcados como partilhados pelo AdMob.
- **Não recolhido:** informações pessoais (nome, e-mail, IDs de utilizador, morada, telefone), localização precisa,
  informações financeiras e de compras, saúde, mensagens, fotos, vídeos, áudio, ficheiros, calendário, contactos,
  histórico de navegação, pesquisas no app, apps instalados, outros conteúdos gerados pelo utilizador. Compras: o
  Firebase lista "eventos de compra", mas o jogo não tem compras com dinheiro (D6), por isso não há o que recolher.
- **Política de privacidade** (campo da seção): `https://jungledegree.com/stampeep/privacy/`

## 2. Anúncios (Conteúdo do app > Anúncios)

**Sim, o app contém anúncios.** Evidência: `ClaybreakFlow` (premiado), kit `MobileAdsGate`.

## 3. Público-alvo e conteúdo (13+, não destinado a crianças)

| Pergunta | Resposta | Evidência |
|---|---|---|
| Faixas etárias do público-alvo | **13–15, 16–17 e 18+** (nenhuma abaixo de 13) | D25 = A (2026-11-22: 13+ com a loja a compensar o visual) |
| O app pode atrair crianças sem querer? **[conferir no Console]** | Resposta honesta do usuário. **Risco real:** o visual é toon pastel, com bolinhas, pintinho, coelhinho, sapinho e pinguim com olhos (D25 A). A Play pode recusar a ficha se a achar infantil e pedir para mudar gráficos ou declarar menores de 13. O que já compensa: nome e textos de ação ("storm", "stampede", "traps"), nenhuma palavra infantil (`Tools/store/check_listing.py`), capturas com 300 corredores, obstáculos e assalto, ícone com a manada e a torre (não um bichinho em close), anúncios PG sem marca infantil. Se o Console perguntar e a resposta honesta for "sim", **parar e voltar a este documento** (Famílias exige SDKs certificados e outra configuração de anúncios) | `ficha/en-US.md`, `graficos/` |
| Direcionado a crianças / Famílias | **Não** | D25 |
| Anúncios | PG, sem tag infantil, personalização só com consentimento UMP | kit `MobileAdsGate` |

A revisão desta seção pode levar 7 dias ou mais ([9867159](https://support.google.com/googleplay/android-developer/answer/9867159)).

## 4. Classificação de conteúdo (questionário IARC)

Categoria: **Jogo**. E-mail de contacto: o da empresa (`<e-mail de contacto>` da política). O resultado é calculado
pelo questionário; espera-se uma classificação baixa (PEGI 7 ou 3 / ESRB Everyone ou Everyone 10+ / IARC 7+), por
causa da violência de fantasia leve **[conferir no Console]**. Público 13+ e classificação baixa não se contradizem:
um diz para quem o jogo é feito, o outro o que ele mostra.

| Tema | Resposta | Evidência / observação |
|---|---|---|
| Violência | **Sim, violência de fantasia/desenho, leve, contra personagens de fantasia (não humanos realistas)**: armadilhas (troncos com espinhos, serras, martelos, buracos, rolos de espinhos) e as defesas da torre (canhões que atiram pedras, escudos, braços que varrem) tiram corredores da multidão; os corredores saltam, giram, encolhem e somem | `ObstacleKind` (`ObstaclePlacement.cs`), `CannonDefense`, `SweeperDefense`, `ShieldDefense`; morte = `SwarmDying` (salto, giro e encolher em 1,1 s) |
| Sangue, ferimentos, partes do corpo | **Não** | nenhum efeito de sangue; a morte é encolher e sumir |
| Violência contra humanos realistas / armas reais | **Não** (canhões de fantasia atiram pedras contra bolinhas e bichos estilizados) | idem |
| Medo / horror | **Não** | paletas pastel, sem sustos |
| Conteúdo sexual, nudez | **Não** | |
| Linguagem imprópria / humor grosseiro | **Não** | textos curtos de ação ("TOWER DOWN!", "the tower holds") |
| Drogas, álcool, tabaco | **Não** | |
| Jogos de azar (reais ou simulados) | **Não**: portões somam/multiplicam pela escolha do jogador; sem aposta, sorte paga ou caixa de prémio | `GateMath`; presente diário fixo (`DailyReward`) |
| Símbolos de ódio, instruções criminosas | **Não** | |
| "Estilo pixelizado ou infantil" **[conferir a pergunta exata]** | Responder com honestidade: o estilo é cartoon toon pastel com personagens arredondados; não há conteúdo sexual nem violento associado a esse estilo | `docs/direcao-visual-crowd.md` |
| Interação entre utilizadores / troca de conteúdo / chat | **Não** | sem rede própria |
| Partilha a localização do utilizador com outros | **Não** | |
| Compras digitais | **Não** | D6 |
| Acesso irrestrito à internet / navegador | **Não** (só o link da política, que abre o navegador do sistema) | `ClaybreakScreens` |
| Anúncios | **Sim** (se perguntado) | |

## 5. Acesso ao app (Conteúdo do app > Acesso ao app / Dados de início de sessão)

**"Toda a funcionalidade está disponível sem acesso especial."** O jogo não tem login, conta, código de convite,
assinatura nem região bloqueada. Para revisar o assalto e a loja basta jogar a fase 1 (cerca de 1 minuto).

## 6. ID de publicidade (Conteúdo do app > ID de publicidade)

**Sim, o app usa o ID de publicidade.** Finalidades: **Publicidade ou marketing** e **Prevenção de fraude, segurança
e conformidade** (e **Análise**, se o formulário listar: o SDK de anúncios mede os anúncios). Quem o usa é só o SDK
de anúncios; o Firebase tem a recolha do ID de publicidade desligada. Manter a permissão `AD_ID` no manifesto (vem
do SDK de anúncios; sem ela o ID chega como zeros). **[conferir o texto do formulário]**

## 7. Outras declarações (todas "não" / não se aplica)

App governamental; recursos financeiros; saúde; notícias; VPN; rastreio de contactos/estado de saúde; apostas com
dinheiro real; contas de utilizador e eliminação de conta (sem login); permissões sensíveis (localização em segundo
plano, SMS, chamadas, todos os ficheiros, acessibilidade, serviço em primeiro plano declarado pelo app). Se o Console
pedir declaração de `FOREGROUND_SERVICE`: a permissão vem dos SDKs do Google, o jogo não inicia serviço em primeiro
plano **[conferir no Console]**.

## 8. Ficha, país e preço

- **Categoria:** Jogo > **Ação** (a corrida com portões e o assalto; "Casual" é a alternativa). Etiquetas sugeridas
  (até 5, se o Console oferecer): runner, multidão/crowd, estratégia leve, offline não (precisa de rede para anúncios).
- **Gratuito.** Contém anúncios: sim. Compras no app: não.
- **Países:** só os da ficha **D9** (Europa: EEE, Reino Unido, Suíça), enquanto valer a D23 = B
  (decisão técnica T-RT9a-3 em `../decisoes.md`).
- **Detalhes de contacto da ficha:** e-mail `<e-mail de contacto>`; site `https://jungledegree.com`; política
  `https://jungledegree.com/stampeep/privacy/`. Conta de organização: nome legal, D-U-N-S, morada e telefone aparecem
  na loja automaticamente.

## 9. Política de privacidade

- Texto final: [`../politica-de-privacidade.md`](../politica-de-privacidade.md); página pronta:
  [`site/stampeep/privacy/index.html`](site/stampeep/privacy/index.html) (gerada por
  `python -X utf8 Tools/store/make_privacy_html.py`).
- URL (no jogo e no Console): `https://jungledegree.com/stampeep/privacy/`.
- Antes de publicar: preencher os 5 campos `<...>` no Markdown, gerar o HTML de novo e conferir com
  `python -X utf8 Tools/store/make_privacy_html.py --check` (falha enquanto sobrar campo).
