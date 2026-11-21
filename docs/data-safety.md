# Seção "Segurança dos dados" (Data safety) — rascunho da RT-7, analytics na RT-8b

Rascunho para o usuário preencher no Play Console na RT-9. **Nada foi enviado.** O que veio do código tem
evidência; o que depende do texto atual do formulário ou da documentação do Google está marcado
**[conferir no Console]**. Política correspondente: [`politica-de-privacidade.md`](politica-de-privacidade.md).

## 0. O que o app coleta e envia (levantamento do código, RT-7)

| Fato | Evidência |
|---|---|
| SDKs de rede: Google Mobile Ads (AdMob) com UMP, e Firebase Analytics (D21 = A, RT-8b) | `Packages/manifest.json` (dependência do kit `com.google.ads.mobile` 11.6.0); `Assets/Plugins/Android/FirebaseAnalytics.androidlib` (`firebase-analytics` 24.0.0); `MonetizationValidation` recusa billing, outros SDKs de analytics e outros produtos Firebase |
| Anúncios só depois do consentimento UMP | kit `MobileAdsGate` (`CanRequestAds` antes de `MobileAds.Initialize` e de qualquer `Load`) |
| Premiado opcional (dobrar moedas, continuar uma vez); intersticial pronto e **desligado** | `ClaybreakFlow`, `MonetizationContent.InterstitialOn = false` |
| Unidades de anúncio: só as de **teste** do Google até a RT-9 | `Assets/Claybreak/Content/Data/AdUnits.asset` vazio; `MonetizationValidation.ProductionUnitsAllowed = false` |
| Analytics mínimo: 4 eventos sem ID (`game_session`, `level_start`, `level_end`, `rewarded_ad`) + eventos automáticos do Firebase (`first_open`, `session_start`, `user_engagement`, `app_update`, `os_update`, `app_clear_data`, `app_remove`) com o ID da instância do app e a origem da instalação; o SDK do AdMob pode registrar eventos de anúncio **[conferir no DebugView]** | kit `PlayerMetrics`, `FirebaseAnalyticsSink`; `ClaybreakMetrics` |
| Analytics só com consentimento explícito, em todos os países (empresa portuguesa: RGPD para todos): na Europa/Reino Unido pelo "sim" do UMP; fora delas o jogo ainda não pergunta (ficha D23), então **não mede**; coleta desligada até o primeiro consentimento (a escolha fica guardada); retirar apaga os dados de análise da instalação | manifesto do módulo (`firebase_analytics_collection_enabled=false`); kit `MeasurementConsent` (TCF propósito 1, 8 ou 9, Google; senão só o consentimento próprio do jogo, hoje inexistente); `resetAnalyticsData` |
| Firebase sem ID de publicidade, sem ID do Android, sem sinais de anúncio | manifesto do módulo (`google_analytics_adid_collection_enabled=false`, `..._ssaid_...=false`, sinais de anúncio negados) |
| Save só local (PlayerPrefs): progresso, moedas, compras com moedas, skins, primeiro dia, presente diário | `SaveData.cs`; nunca enviado |
| Sem login, conta, servidor, compras com dinheiro | sem código de rede próprio; sem `com.unity.purchasing`/`BILLING` |
| Estatística de hardware do Unity e serviços Unity desligados | `ProjectConfiguration` (`submitAnalytics = 0`), `UnityConnectSettings.asset` (tudo `m_Enabled: 0`) |
| Público 13+, conteúdo PG, sem tag infantil | kit `MobileAdsGate` |

Permissões do APK (conferidas com `aapt2 dump permissions` no APK da RT-8b): `INTERNET`, `ACCESS_NETWORK_STATE`,
`com.google.android.gms.permission.AD_ID`, `ACCESS_ADSERVICES_AD_ID`, `ACCESS_ADSERVICES_ATTRIBUTION`,
`ACCESS_ADSERVICES_TOPICS`, `WAKE_LOCK`, `FOREGROUND_SERVICE` (SDK de anúncios e SDK de medição do Google; o
Firebase declara algumas delas também, mas não lê o ID de publicidade), `BIND_GET_INSTALL_REFERRER_SERVICE`
(Firebase: origem da instalação na Google Play), `VIBRATE` (kit) e a permissão interna
`DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION`; nenhuma de câmera, microfone, contatos, localização ou armazenamento.

## 1. Respostas propostas

| Pergunta | Resposta proposta | Por quê |
|---|---|---|
| O app coleta ou compartilha algum tipo de dado exigido? | **Sim** | SDK do AdMob e Firebase Analytics |
| Os dados são criptografados em trânsito? | **Sim** **[conferir]** | o SDK do Google usa HTTPS |
| Há forma de pedir exclusão dos dados? | **Sim** (contato da política, direitos do RGPD; o botão PRIVACY apaga os dados de análise da instalação; desinstalar troca o identificador; o save é local) **[conferir o texto]** | política; `resetAnalyticsData` |
| Informações pessoais (nome, e-mail, IDs de usuário) | **Não coletado** | o jogo não pede nada disso |
| Localização aproximada | **Coletada e compartilhada** (AdMob: publicidade, prevenção de fraude) **[conferir]**; Firebase: **coletada** para análise (país/região deduzidos do IP) **[conferir na página de divulgação do Firebase]** | IP do pedido de anúncio; IP da conexão do Firebase |
| Localização precisa | Não | sem permissão de localização |
| Atividade no app: interações com anúncios | **Coletada e compartilhada** (AdMob) | |
| Atividade no app: interações com o app (sessões, fases, aceite do premiado) | **Coletado** (Firebase), finalidade **análise**; **não compartilhado** (o Google trata como prestador de serviço) **[conferir]** | `PlayerMetrics`, eventos automáticos do Firebase |
| Informações e desempenho do app: falhas, diagnóstico | **Coletado e compartilhado** (AdMob) **[conferir]** | documentação do SDK |
| Dispositivo ou outros IDs: ID de publicidade | **Coletado e compartilhado** (AdMob): publicidade ou marketing, prevenção de fraude. O Firebase **não** o coleta | permissão `AD_ID` (do SDK de anúncios); `google_analytics_adid_collection_enabled=false` |
| Dispositivo ou outros IDs: ID da instância do app (Firebase) | **Coletado** (Firebase), finalidade **análise**; não compartilhado **[conferir]** | Firebase Analytics |
| Compras / informações financeiras | Não | D6: sem compra no app |
| Mensagens, fotos, áudio, arquivos, contatos, agenda, saúde | Não | sem permissões |
| Coleta obrigatória ou opcional? | Os dados de anúncio vão quando um anúncio é pedido; o consentimento UMP governa a personalização. Analytics: **opcional** (o usuário pode escolher: só com o "sim"; fora da Europa/Reino Unido hoje não há coleta) | `MobileAdsGate`, `MeasurementConsent` |
| Política de privacidade | URL pública a criar na RT-9 com o texto de `politica-de-privacidade.md` | |

**ID de publicidade** (Conteúdo do app > ID de publicidade): **Sim**, usado para publicidade/marketing e
prevenção de fraude; manter a permissão `AD_ID`.

**Anúncios** (Conteúdo do app > Anúncios): **Sim, o app contém anúncios.**

## 2. O que muda quando...

- **A RT-9 puser os IDs reais do AdMob:** as respostas acima continuam valendo (o SDK é o mesmo); trocar
  `ProductionUnitsAllowed` e o app ID juntos (o validador exige que a unidade pertença ao app ID).
- **O intersticial for ligado (decisão do usuário, D6):** nada novo no formulário (mesmo SDK, mesmos dados);
  revisar a frase de anúncios da política.
- **A D21 escolheu o Firebase Analytics (RT-8b, feito):** "Atividade no app" e "ID da instância do app" passaram
  a "Coletado" para análise; o ID de publicidade continua só do AdMob. Se um dia o Firebase for ligado a anúncios
  (vincular ao AdMob, sinais do Google, ID de publicidade), este documento e a política mudam antes.

## 3. Empresa e lei (2026-11-21)

A Jungle Degree é **portuguesa**: o RGPD vale para todo o tratamento, em qualquer país (art. 3.º, n.º 1), e a
autoridade é a **CNPD**. No Play Console, o endereço e os dados do programador devem ser os da empresa portuguesa;
a política cita o responsável, a base legal (consentimento), os direitos do titular e a CNPD.
