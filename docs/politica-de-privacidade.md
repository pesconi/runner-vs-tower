# Política de privacidade — Clay Siege (rascunho da RT-7, analytics na RT-8b)

Texto para o usuário revisar e publicar numa URL pública na RT-9 (o Play Console pede a URL; nada foi
publicado nem enviado). Escrito **só** com o que o código faz hoje; cada frase tem a evidência na tabela do
fim. Quando o código mudar (IDs reais do AdMob na RT-9, intersticial ligado, outra medição), este texto e o
[`data-safety.md`](data-safety.md) mudam juntos — o validador de monetização confere que os dois existem e citam
AdMob, UMP, analytics e Firebase. A medição (Firebase Analytics, D21 = A) entrou na RT-8b.

Campos a preencher pelo usuário antes de publicar: `<e-mail de contato>` e `<data de publicação>`.

---

## Português

**Clay Siege** é um jogo da Jungle Degree. Esta política explica o que acontece com os seus dados quando você
joga.

**O que fica só no seu aparelho.** O seu progresso (fases, estrelas, moedas, compras feitas com moedas do
jogo, skins), as datas do primeiro dia de jogo e do último presente diário, e as suas escolhas de som,
música e vibração ficam guardados apenas no aparelho. Nós não temos servidor, não pedimos conta, nome,
e-mail nem login, e nada disso é enviado a nós. Desinstalar o jogo apaga esses dados.

**Anúncios (Google AdMob).** O jogo mostra anúncios do Google AdMob. O anúncio premiado é sempre opcional:
você escolhe assistir para ganhar algo que o botão mostra antes (dobrar as moedas da fase ou continuar uma
vez). Para mostrar anúncios, o SDK do Google pode coletar e compartilhar com o Google dados como o ID de
publicidade do aparelho, endereço IP (do qual se deduz uma localização aproximada), informações do aparelho e
do app, interações com os anúncios e dados de diagnóstico, para publicidade, medição e prevenção de fraude.
Isso é regido pela política do Google: https://policies.google.com/technologies/partner-sites

**Consentimento (UMP).** Antes de pedir qualquer anúncio, o jogo usa a Plataforma de Mensagens ao Usuário do
Google (UMP). Onde a lei exige (por exemplo, no Espaço Econômico Europeu e no Reino Unido), ela pergunta se
você aceita anúncios personalizados; nenhum anúncio é pedido antes dessa resposta. Você pode mudar a escolha a
qualquer momento pelo botão **PRIVACY** na tela inicial, que aparece onde a regra se aplica. Você também pode
redefinir ou excluir o ID de publicidade nas configurações do Android.

**Medição de uso (Google Analytics for Firebase).** Para saber se o jogo está bom (quantos jogadores voltam no
dia seguinte e na semana seguinte, onde as fases ficam difíceis demais e quantas pessoas aceitam o anúncio
premiado), o jogo usa o Firebase Analytics, do Google, na configuração mínima:
- **o que é enviado:** o início de cada sessão de jogo (com quantos dias se passaram desde a instalação), o
  início e o fim de cada fase (qual fase, se venceu ou perdeu e quantos segundos durou) e se o anúncio premiado
  foi oferecido, aceito e concluído. O Firebase também registra sozinho o primeiro uso do app, o início e a
  duração das sessões e do tempo de uso, atualizações do app e do sistema, e quando os dados do app são apagados
  ou o app é removido, junto com um identificador aleatório da instalação do app (não é o seu nome, e-mail,
  conta, nem o ID de publicidade), o modelo e a versão do sistema do aparelho, o idioma, a versão do jogo, a
  origem da instalação informada pela Google Play (por exemplo, a loja ou uma campanha) e o país ou região
  aproximados (deduzidos do endereço IP). O SDK de anúncios do Google também pode registrar ali eventos dos
  anúncios mostrados (exibição, clique, recompensa);
- **o que não é enviado:** o ID de publicidade, o ID do Android, nome, e-mail, conta ou qualquer coisa que você
  escreva. O Firebase não recebe sinais de publicidade do jogo nem dados para personalizar anúncios, e as telas
  não são registradas automaticamente;
- **quando:** na primeira vez, nada é coletado antes de o jogo verificar o consentimento; depois, a escolha fica
  guardada no aparelho até mudar. No Espaço Econômico Europeu e no Reino Unido, só se você aceitar na janela de
  consentimento (UMP) o armazenamento de informações no aparelho e uma finalidade de medição (desempenho de
  conteúdo ou estatísticas de público), com o Google como parceiro; se recusar, nada é medido. Se mudar de ideia
  no botão **PRIVACY**, a medição para e os dados de análise desta instalação são apagados (o identificador é
  trocado). Nos demais países (incluindo o Brasil), a medição acontece com base no nosso interesse legítimo em
  melhorar o jogo (LGPD, art. 7º, IX), com os dados mínimos acima;
- **quem trata e por quanto tempo:** o Google trata esses dados para nós, como prestador de serviço, segundo os
  termos do Firebase (https://firebase.google.com/support/privacy); os dados por jogador ficam guardados por no
  máximo 2 meses e depois só restam números somados;
- **como parar:** desinstalar o jogo ou apagar os dados do app no Android apaga o identificador da instalação;
  uma instalação nova começa com outro identificador. Também pode escrever para o contato abaixo.

**Compras.** O jogo não tem compras com dinheiro real.

**Crianças.** O jogo é feito para maiores de 13 anos e não é direcionado a crianças. Os anúncios são pedidos
com classificação de conteúdo para público geral e sem marcação de público infantil.

**Mudanças e contato.** Avisaremos mudanças atualizando esta página e a data abaixo. Dúvidas:
`<e-mail de contato>`.

Última atualização: `<data de publicação>`.

---

## English

**Clay Siege** is a game by Jungle Degree. This policy explains what happens to your data when you play.

**What stays on your device.** Your progress (levels, stars, coins, items bought with in-game coins, skins),
the dates of your first day of play and of your last daily gift, and your sound, music and vibration choices
are stored only on your device. We run no server, ask for no account, name, e-mail or login, and none of it
is sent to us. Uninstalling the game deletes this data.

**Ads (Google AdMob).** The game shows ads from Google AdMob. Rewarded ads are always optional: you choose to
watch one to get what the button shows first (double the level coins, or continue once). To serve ads,
Google's SDK may collect and share with Google data such as your device's advertising ID, IP address (from
which an approximate location is derived), device and app information, ad interactions and diagnostics, for
advertising, measurement and fraud prevention. See https://policies.google.com/technologies/partner-sites

**Consent (UMP).** Before requesting any ad, the game uses Google's User Messaging Platform (UMP). Where the
law requires it (for example in the European Economic Area and the UK) it asks whether you accept
personalised ads; no ad is requested before you answer. You can change your choice at any time with the
**PRIVACY** button on the home screen, shown where this applies. You can also reset or delete your
advertising ID in Android settings.

**Usage measurement (Google Analytics for Firebase).** To learn whether the game works (how many players come
back the next day and the next week, where levels get too hard, and how many accept the rewarded ad), the game
uses Google's Firebase Analytics in a minimal setup:
- **what is sent:** the start of each play session (with the number of days since install), the start and end
  of each level (which level, won or lost, and how many seconds it took) and whether the rewarded ad was offered,
  accepted and completed. Firebase also records on its own the app's first open, session start, length and
  engagement time, app and OS updates, and when the app's data is cleared or the app is removed, together with a
  random identifier of this app installation (not your name, e-mail, account or advertising ID), the device model
  and OS version, language, game version, the install source reported by Google Play (for example the store or a
  campaign) and approximate country or region (derived from the IP address). Google's ads SDK may also record
  events of the ads shown there (impression, click, reward);
- **what is not sent:** the advertising ID, the Android ID, your name, e-mail, account or anything you type.
  Firebase receives no advertising signals from the game and no data to personalise ads, and screens are not
  recorded automatically;
- **when:** the first time, nothing is collected before the game checks consent; after that, your choice is kept
  on the device until you change it. In the European Economic Area and the UK, only if you accept in the consent
  form (UMP) storing information on the device and a measurement purpose (content performance or audience
  statistics), with Google as a partner; if you refuse, nothing is measured. If you change your mind with the
  **PRIVACY** button, measurement stops and this installation's analytics data is erased (the identifier is
  replaced). In other countries (including Brazil), measurement relies on our legitimate interest in improving
  the game, with the minimal data above;
- **who processes it and for how long:** Google processes this data for us, as a service provider, under the
  Firebase terms (https://firebase.google.com/support/privacy); per-player data is kept for at most 2 months,
  after which only aggregated numbers remain;
- **how to stop it:** uninstalling the game or clearing the app's data in Android deletes the installation
  identifier; a new installation starts with a different one. You can also write to the contact below.

**Purchases.** The game has no real-money purchases.

**Children.** The game is made for ages 13 and up and is not directed to children. Ads are requested with a
general-audience content rating and without child-directed tags.

**Changes and contact.** We will update this page and the date below. Questions: `<contact e-mail>`.

Last updated: `<publication date>`.

---

## Evidência no código (para quem revisa)

| Frase | Evidência |
|---|---|
| Progresso, moedas, skins, primeiro dia, último presente só no aparelho | `Assets/Claybreak/Runtime/SaveData.cs` (`SaveData`, campos `firstDay`, `dailyLastDay`, `dailyClaims`), `SaveStore` grava em PlayerPrefs; nenhum código de rede no jogo |
| Som, música, vibração no aparelho | kit `GameAudio`, `GameHaptics` (PlayerPrefs) |
| Sem servidor, conta, login | busca no código: sem `UnityWebRequest`, `HttpClient`, `WWW` em `Assets/Claybreak` |
| SDKs de rede: Google Mobile Ads + UMP, e Firebase Analytics | `Packages/manifest.json` (`com.google.ads.mobile` 11.6.0 via kit); `Assets/Plugins/Android/FirebaseAnalytics.androidlib` (`firebase-analytics` 24.0.0, única dependência); validador recusa billing, outros SDKs de analytics e outros produtos Firebase |
| Nenhum anúncio antes do UMP | kit `MobileAdsGate` (`Begin` → `ConsentInformation.Update` → formulário → `MobileAds.Initialize` só com `CanRequestAds`) |
| Botão PRIVACY onde exigido | `ClaybreakScreens.SetPrivacyVisible` ← `MobileAdsGate.PrivacyOptionsRequired` |
| Premiado opcional, ganho dito antes | `ClaybreakFlow` (`RefreshResultAd`, `TryOfferContinue`; botão "WATCH AD: x2 COINS +N", "WATCH AD +N" e "NO THANKS") |
| Intersticial | pronto, **desligado** (`MonetizationContent.InterstitialOn = false`, D6); a política não o cita porque não aparece; quando for ligado, a frase "O jogo mostra anúncios" já cobre, mas revisar |
| Eventos enviados e nomes | kit `PlayerMetrics` (lista fechada: `game_session`, `level_start`, `level_end`, `rewarded_ad`; parâmetros curtos, sem ID); `FirebaseAnalyticsSink` (nenhum `setUserId` nem propriedade de usuário) |
| Nada antes do consentimento; escolha guardada | manifesto do módulo: `firebase_analytics_collection_enabled=false`; `ClaybreakMetrics` liga a coleta só com `MeasurementConsent` (kit), lido ao fim do fluxo UMP; o evento de sessão espera na memória; o SDK guarda a última escolha entre aberturas (`setAnalyticsCollectionEnabled`) |
| Europa/Reino Unido só com o "sim" | `MeasurementConsent.Decide`: `IABTCF_gdprApplies=1` exige o propósito TCF 1, o 8 ou o 9, e o Google (fornecedor 755) aceitos |
| Retirar apaga | `FirebaseAnalyticsSink.SetCollectionEnabled(false)` depois de ligado: desliga, nega o consentimento e chama `resetAnalyticsData` |
| Eventos automáticos e origem da instalação | comportamento padrão do Firebase Analytics (`first_open`, `session_start`, `user_engagement`, `app_update`, `os_update`, `app_clear_data`, `app_remove`) e permissão `BIND_GET_INSTALL_REFERRER_SERVICE` no APK; eventos de anúncio do SDK do AdMob **[conferir no DebugView com um anúncio]** |
| Sem ID de publicidade, sem ID do Android, sem sinais de anúncio, sem telas automáticas | manifesto do módulo: `google_analytics_adid_collection_enabled`, `google_analytics_ssaid_collection_enabled`, `google_analytics_default_allow_ad_personalization_signals`, `..._default_allow_ad_storage/ad_user_data/ad_personalization`, `google_analytics_automatic_screen_reporting_enabled` = false; `FirebaseAnalyticsSink.SetConsent` nega os três sinais de anúncio sempre |
| Retenção de 2 meses | configuração do projeto no Google Analytics (padrão de 2 meses) **[conferir no console do Firebase/Analytics: Administrador > Coleta e retenção de dados]** |
| Sem compras | sem `com.unity.purchasing` nem permissão `BILLING` (validador) |
| 13+, conteúdo PG, sem tag infantil | kit `MobileAdsGate` (`MaxAdContentRating.PG`, `TagForChildDirectedTreatment.False`, `TagForUnderAgeOfConsent.False`) |
| Estatística de hardware do Unity desligada | `ProjectConfiguration.DisableEngineHardwareStatistics` (`submitAnalytics = 0`); serviços Unity desligados em `ProjectSettings/UnityConnectSettings.asset` |
