# Política de privacidade — Clay Siege (rascunho da RT-7, analytics e RGPD na RT-8b)

Texto para o usuário revisar e publicar numa URL pública na RT-9 (o Play Console pede a URL; nada foi
publicado nem enviado). Escrito **só** com o que o código faz hoje; cada frase tem a evidência na tabela do
fim. Quando o código mudar (IDs reais do AdMob na RT-9, intersticial ligado, pedido próprio de consentimento da
D23, outra medição), este texto e o [`data-safety.md`](data-safety.md) mudam juntos — o validador de monetização
confere que os dois existem e citam AdMob, UMP, analytics e Firebase.

**Base (2026-11-21):** a Jungle Degree é uma empresa **portuguesa** (responsável pelo tratamento estabelecido na
União Europeia). Pelo art. 3.º, n.º 1, do RGPD, o regulamento vale para todo o tratamento que ela faz, seja qual
for o país do jogador; a autoridade de controlo é a **CNPD** (Comissão Nacional de Proteção de Dados). A parte em
português está em **português de Portugal**. Não é parecer jurídico.

Campos a preencher pelo usuário antes de publicar: `<denominação legal da empresa>`, `<NIPC>`, `<morada>`,
`<e-mail de contacto>` e `<data de publicação>`.

---

## Português (Portugal)

**Clay Siege** é um jogo da Jungle Degree. Esta política explica o que acontece aos seus dados quando joga.

**Responsável pelo tratamento.** `<denominação legal da empresa>` ("Jungle Degree"), NIPC `<NIPC>`, com sede em
`<morada>`, Portugal. Contacto para assuntos de privacidade: `<e-mail de contacto>`.

**O que fica só no seu telemóvel.** O seu progresso (níveis, estrelas, moedas, compras feitas com moedas do jogo,
skins), as datas do primeiro dia de jogo e do último presente diário e as suas escolhas de som, música e vibração
ficam guardados apenas no telemóvel. Não temos servidor, não pedimos conta, nome, e-mail nem login, e nada disto
nos é enviado. Desinstalar o jogo apaga estes dados.

**Anúncios (Google AdMob).** O jogo mostra anúncios do Google AdMob. O anúncio premiado é sempre opcional: escolhe
vê-lo para ganhar o que o botão mostra antes (duplicar as moedas do nível ou continuar uma vez). Para mostrar
anúncios, o SDK da Google pode recolher e partilhar com a Google dados como o ID de publicidade do dispositivo, o
endereço IP (do qual se deduz uma localização aproximada), informações do dispositivo e da app, interações com os
anúncios e dados de diagnóstico, para publicidade, medição e prevenção de fraude. A Google trata estes dados nos
termos da sua política: https://policies.google.com/technologies/partner-sites

**Consentimento (UMP).** Antes de pedir qualquer anúncio, o jogo usa a Plataforma de Mensagens ao Utilizador da
Google (UMP). No Espaço Económico Europeu, no Reino Unido e na Suíça, ela pergunta se aceita anúncios personalizados e a
medição de utilização; nenhum anúncio é pedido antes da resposta. Pode mudar a escolha a qualquer momento no botão
**PRIVACY** do ecrã inicial. Também pode repor ou eliminar o ID de publicidade nas definições do Android.

**Medição de utilização (Google Analytics for Firebase).** Para saber se o jogo está bom (quantos jogadores voltam
no dia seguinte e na semana seguinte, onde os níveis ficam difíceis demais e quantas pessoas aceitam o anúncio
premiado), o jogo usa o Firebase Analytics, da Google, numa configuração mínima:
- **base legal:** o seu **consentimento** (art. 6.º, n.º 1, alínea a), do RGPD, e art. 5.º, n.º 3, da Diretiva
  ePrivacy, transposto pela Lei n.º 41/2004). Sem consentimento, nada é medido;
- **onde e como pedimos:** no Espaço Económico Europeu, no Reino Unido e na Suíça, na janela de consentimento (UMP): só
  medimos se aceitar o armazenamento de informações no dispositivo e uma finalidade de medição (desempenho de
  conteúdo ou estatísticas de público), com a Google como parceira. Nos outros países, o jogo ainda não pede este
  consentimento, por isso **não mede nada** nesses países;
- **o que é enviado (com consentimento):** o início de cada sessão de jogo (com quantos dias passaram desde a
  instalação), o início e o fim de cada nível (qual nível, se ganhou ou perdeu e quantos segundos durou) e se o
  anúncio premiado foi oferecido, aceite e concluído. O Firebase também regista sozinho a primeira abertura da app,
  o início e a duração das sessões e do tempo de utilização, atualizações da app e do sistema, e quando os dados da
  app são apagados ou a app é removida, juntamente com um identificador aleatório desta instalação (não é o seu
  nome, e-mail, conta nem o ID de publicidade), o modelo e a versão do sistema do dispositivo, o idioma, a versão
  do jogo, a origem da instalação indicada pela Google Play (por exemplo, a loja ou uma campanha) e o país ou
  região aproximados (deduzidos do endereço IP). O SDK de anúncios da Google também pode registar aí eventos dos
  anúncios mostrados (exibição, clique, recompensa);
- **o que não é enviado:** o ID de publicidade, o ID do Android, nome, e-mail, conta ou qualquer coisa que
  escreva. O Firebase não recebe sinais de publicidade do jogo nem dados para personalizar anúncios, e os ecrãs
  não são registados automaticamente;
- **quando:** na primeira vez, nada é recolhido antes de o jogo verificar o consentimento; depois, a escolha fica
  guardada no dispositivo até a mudar. Se retirar o consentimento no botão **PRIVACY**, a medição para e os dados
  de análise desta instalação são apagados (o identificador é substituído). Retirar o consentimento não afeta o
  tratamento feito antes;
- **quem trata e durante quanto tempo:** a Google trata estes dados por nossa conta, como subcontratante, nos
  termos do Firebase (https://firebase.google.com/support/privacy); os dados por jogador são guardados no máximo
  2 meses e depois só restam números agregados;
- **transferências para fora da UE:** a Google pode tratar os dados nos Estados Unidos e noutros países, com as
  garantias previstas no RGPD (Quadro de Privacidade de Dados UE-EUA e cláusulas contratuais-tipo da Comissão
  Europeia).

**Os seus direitos.** Nos termos do RGPD, tem direito de acesso, retificação, apagamento, limitação do tratamento,
portabilidade e oposição, e de retirar o consentimento a qualquer momento, sem afetar o tratamento anterior. Como
não pedimos conta nem nome, a forma mais direta de exercer estes direitos é no próprio jogo (botão **PRIVACY**) ou
apagando os dados da app/desinstalando; também pode escrever para `<e-mail de contacto>` (para identificarmos os
dados de análise, pode ser preciso que nos indique como encontrar a sua instalação; não pedimos mais do que isso).
Tem ainda o direito de apresentar reclamação à **CNPD — Comissão Nacional de Proteção de Dados** (www.cnpd.pt) ou à
autoridade de controlo do seu país.

**Compras.** O jogo não tem compras com dinheiro real.

**Crianças.** O jogo é feito para maiores de 13 anos e não se destina a crianças (em Portugal, 13 anos é a idade
a partir da qual o consentimento para serviços da sociedade da informação é válido, Lei n.º 58/2019). Os anúncios
são pedidos com classificação de conteúdo para público geral e sem marcação de público infantil.

**Alterações e contacto.** Avisaremos de alterações atualizando esta página e a data abaixo. Dúvidas:
`<e-mail de contacto>`.

Última atualização: `<data de publicação>`.

---

## English

**Clay Siege** is a game by Jungle Degree. This policy explains what happens to your data when you play.

**Controller.** `<company legal name>` ("Jungle Degree"), company number (NIPC) `<NIPC>`, registered at
`<address>`, Portugal. Privacy contact: `<contact e-mail>`.

**What stays on your device.** Your progress (levels, stars, coins, items bought with in-game coins, skins),
the dates of your first day of play and of your last daily gift, and your sound, music and vibration choices
are stored only on your device. We run no server, ask for no account, name, e-mail or login, and none of it
is sent to us. Uninstalling the game deletes this data.

**Ads (Google AdMob).** The game shows ads from Google AdMob. Rewarded ads are always optional: you choose to
watch one to get what the button shows first (double the level coins, or continue once). To serve ads,
Google's SDK may collect and share with Google data such as your device's advertising ID, IP address (from
which an approximate location is derived), device and app information, ad interactions and diagnostics, for
advertising, measurement and fraud prevention. See https://policies.google.com/technologies/partner-sites

**Consent (UMP).** Before requesting any ad, the game uses Google's User Messaging Platform (UMP). In the
European Economic Area, the UK and Switzerland it asks whether you accept personalised ads and usage measurement; no ad is
requested before you answer. You can change your choice at any time with the **PRIVACY** button on the home
screen. You can also reset or delete your advertising ID in Android settings.

**Usage measurement (Google Analytics for Firebase).** To learn whether the game works (how many players come
back the next day and the next week, where levels get too hard, and how many accept the rewarded ad), the game
uses Google's Firebase Analytics in a minimal setup:
- **legal basis:** your **consent** (GDPR Art. 6(1)(a) and Art. 5(3) of the ePrivacy Directive). Without consent
  nothing is measured;
- **where and how we ask:** in the European Economic Area, the UK and Switzerland, in the consent form (UMP): we only measure
  if you accept storing information on the device and a measurement purpose (content performance or audience
  statistics), with Google as a partner. In other countries the game does not ask for this consent yet, so it
  **measures nothing** there;
- **what is sent (with consent):** the start of each play session (with the number of days since install), the
  start and end of each level (which level, won or lost, and how many seconds it took) and whether the rewarded ad
  was offered, accepted and completed. Firebase also records on its own the app's first open, session start,
  length and engagement time, app and OS updates, and when the app's data is cleared or the app is removed,
  together with a random identifier of this app installation (not your name, e-mail, account or advertising ID),
  the device model and OS version, language, game version, the install source reported by Google Play (for
  example the store or a campaign) and approximate country or region (derived from the IP address). Google's ads
  SDK may also record events of the ads shown there (impression, click, reward);
- **what is not sent:** the advertising ID, the Android ID, your name, e-mail, account or anything you type.
  Firebase receives no advertising signals from the game and no data to personalise ads, and screens are not
  recorded automatically;
- **when:** the first time, nothing is collected before the game checks consent; after that, your choice is kept
  on the device until you change it. If you withdraw consent with the **PRIVACY** button, measurement stops and
  this installation's analytics data is erased (the identifier is replaced). Withdrawing does not affect earlier
  processing;
- **who processes it and for how long:** Google processes this data on our behalf, as a processor, under the
  Firebase terms (https://firebase.google.com/support/privacy); per-player data is kept for at most 2 months,
  after which only aggregated numbers remain;
- **transfers outside the EU:** Google may process the data in the United States and other countries, with the
  safeguards the GDPR provides (EU-US Data Privacy Framework and the European Commission's standard contractual
  clauses).

**Your rights.** Under the GDPR you have the right of access, rectification, erasure, restriction, portability
and objection, and to withdraw consent at any time without affecting earlier processing. As we ask for no account
or name, the most direct way to exercise them is in the game (the **PRIVACY** button) or by clearing the app's
data / uninstalling; you can also write to `<contact e-mail>` (to find the analytics data we may need you to tell
us how to identify your installation; we ask for nothing more). You may also lodge a complaint with the Portuguese
supervisory authority, **CNPD — Comissão Nacional de Proteção de Dados** (www.cnpd.pt), or with the authority of
your country.

**Purchases.** The game has no real-money purchases.

**Children.** The game is made for ages 13 and up and is not directed to children. Ads are requested with a
general-audience content rating and without child-directed tags.

**Changes and contact.** We will update this page and the date below. Questions: `<contact e-mail>`.

Last updated: `<publication date>`.

---

## Evidência no código (para quem revisa)

| Frase | Evidência |
|---|---|
| Progresso, moedas, skins, primeiro dia, último presente só no telemóvel | `Assets/Claybreak/Runtime/SaveData.cs` (`SaveData`, campos `firstDay`, `dailyLastDay`, `dailyClaims`), `SaveStore` grava em PlayerPrefs; nenhum código de rede no jogo |
| Som, música, vibração no telemóvel | kit `GameAudio`, `GameHaptics` (PlayerPrefs) |
| Sem servidor, conta, login | busca no código: sem `UnityWebRequest`, `HttpClient`, `WWW` em `Assets/Claybreak` |
| SDKs de rede: Google Mobile Ads + UMP, e Firebase Analytics | `Packages/manifest.json` (`com.google.ads.mobile` 11.6.0 via kit); `Assets/Plugins/Android/FirebaseAnalytics.androidlib` (`firebase-analytics` 24.0.0, única dependência); validador recusa billing, outros SDKs de analytics e outros produtos Firebase |
| Nenhum anúncio antes do UMP | kit `MobileAdsGate` (`Begin` → `ConsentInformation.Update` → formulário → `MobileAds.Initialize` só com `CanRequestAds`) |
| Botão PRIVACY onde exigido | `ClaybreakScreens.SetPrivacyVisible` ← `MobileAdsGate.PrivacyOptionsRequired` |
| Premiado opcional, ganho dito antes | `ClaybreakFlow` (`RefreshResultAd`, `TryOfferContinue`; botão "WATCH AD: x2 COINS +N", "WATCH AD +N" e "NO THANKS") |
| Intersticial | pronto, **desligado** (`MonetizationContent.InterstitialOn = false`, D6); a política não o cita porque não aparece; quando for ligado, a frase "O jogo mostra anúncios" já cobre, mas revisar |
| Eventos enviados e nomes | kit `PlayerMetrics` (lista fechada: `game_session`, `level_start`, `level_end`, `rewarded_ad`; parâmetros curtos, sem ID); `FirebaseAnalyticsSink` (nenhum `setUserId` nem propriedade de usuário) |
| Só com consentimento, em todos os países | `MeasurementConsent.Decide`: com `IABTCF_gdprApplies=1` exige o propósito TCF 1, o 8 ou o 9, e o Google (fornecedor 755) aceitos; em qualquer outro caso só com o consentimento próprio do jogo, que hoje não existe (`ClaybreakMetrics.GameConsentGiven = false`, ficha D23) |
| Nada antes do consentimento; escolha guardada | manifesto do módulo: `firebase_analytics_collection_enabled=false`; `ClaybreakMetrics` liga a coleta só com `MeasurementConsent` (kit), lido ao fim do fluxo UMP; o evento de sessão espera na memória; o SDK guarda a última escolha entre aberturas (`setAnalyticsCollectionEnabled`) |
| Retirar apaga | `FirebaseAnalyticsSink.SetCollectionEnabled(false)` depois de ligado: desliga, nega o consentimento e chama `resetAnalyticsData` |
| Eventos automáticos e origem da instalação | comportamento padrão do Firebase Analytics (`first_open`, `session_start`, `user_engagement`, `app_update`, `os_update`, `app_clear_data`, `app_remove`) e permissão `BIND_GET_INSTALL_REFERRER_SERVICE` no APK; eventos de anúncio do SDK do AdMob **[conferir no DebugView com um anúncio]** |
| Sem ID de publicidade, sem ID do Android, sem sinais de anúncio, sem ecrãs automáticos | manifesto do módulo: `google_analytics_adid_collection_enabled`, `google_analytics_ssaid_collection_enabled`, `google_analytics_default_allow_ad_personalization_signals`, `..._default_allow_ad_storage/ad_user_data/ad_personalization`, `google_analytics_automatic_screen_reporting_enabled` = false; `FirebaseAnalyticsSink.SetConsent` nega os três sinais de anúncio sempre |
| Retenção de 2 meses | configuração do projeto no Google Analytics (padrão de 2 meses) **[conferir no console: Administrador > Coleta e retenção de dados]** |
| Transferências (DPF, cláusulas-tipo) | termos de tratamento de dados do Firebase/Google **[conferir o texto atual nos termos do Firebase antes de publicar]** |
| Responsável português, RGPD, CNPD | fato informado pelo usuário em 2026-11-21 (empresa portuguesa); dados da empresa a preencher |
| Sem compras | sem `com.unity.purchasing` nem permissão `BILLING` (validador) |
| 13+, conteúdo PG, sem tag infantil | kit `MobileAdsGate` (`MaxAdContentRating.PG`, `TagForChildDirectedTreatment.False`, `TagForUnderAgeOfConsent.False`) |
| Estatística de hardware do Unity desligada | `ProjectConfiguration.DisableEngineHardwareStatistics` (`submitAnalytics = 0`); serviços Unity desligados em `ProjectSettings/UnityConnectSettings.asset` |
