# Política de privacidade — Clay Siege (rascunho da RT-7)

Texto para o usuário revisar e publicar numa URL pública na RT-9 (o Play Console pede a URL; nada foi
publicado nem enviado). Escrito **só** com o que o código faz hoje; cada frase tem a evidência na tabela do
fim. Quando o código mudar (ferramenta de analytics da D21, IDs reais do AdMob na RT-9, intersticial ligado),
este texto e o [`data-safety.md`](data-safety.md) mudam juntos — o validador de monetização confere que os
dois existem e citam AdMob, UMP e analytics.

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

**Medição de uso (analytics).** Hoje o jogo **não** tem nenhuma ferramenta de analytics: nenhum evento de uso
sai do aparelho. Se uma ferramenta for adicionada no futuro, ela medirá só retorno de jogadores, início e fim
de fase e aceite do anúncio premiado, com o seu consentimento, e esta política será atualizada antes.

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

**Usage measurement (analytics).** The game currently has **no** analytics tool: no usage event leaves your
device. If one is added, it will only measure returning players, level start and end and rewarded ad
acceptance, with your consent, and this policy will be updated first.

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
| Só um SDK de rede: Google Mobile Ads + UMP | `Packages/manifest.json` (`com.google.ads.mobile` 11.6.0 via kit); validador recusa SDK de analytics e billing |
| Nenhum anúncio antes do UMP | kit `MobileAdsGate` (`Begin` → `ConsentInformation.Update` → formulário → `MobileAds.Initialize` só com `CanRequestAds`) |
| Botão PRIVACY onde exigido | `ClaybreakScreens.SetPrivacyVisible` ← `MobileAdsGate.PrivacyOptionsRequired` |
| Premiado opcional, ganho dito antes | `ClaybreakFlow` (`RefreshResultAd`, `TryOfferContinue`; botão "WATCH AD: x2 COINS +N", "WATCH AD +N" e "NO THANKS") |
| Intersticial | pronto, **desligado** (`MonetizationContent.InterstitialOn = false`, D6); a política não o cita porque não aparece; quando for ligado, a frase "O jogo mostra anúncios" já cobre, mas revisar |
| Sem analytics | `ClaybreakMetrics.Install`: build de release instala `NullMetricsSink` e não permite coleta; builds de desenvolvimento só escrevem no log do aparelho |
| Sem compras | sem `com.unity.purchasing` nem permissão `BILLING` (validador) |
| 13+, conteúdo PG, sem tag infantil | kit `MobileAdsGate` (`MaxAdContentRating.PG`, `TagForChildDirectedTreatment.False`, `TagForUnderAgeOfConsent.False`) |
| Estatística de hardware do Unity desligada | `ProjectConfiguration.DisableEngineHardwareStatistics` (`submitAnalytics = 0`); serviços Unity desligados em `ProjectSettings/UnityConnectSettings.asset` |
