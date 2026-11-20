# Seção "Segurança dos dados" (Data safety) — rascunho da RT-7

Rascunho para o usuário preencher no Play Console na RT-9. **Nada foi enviado.** O que veio do código tem
evidência; o que depende do texto atual do formulário ou da documentação do Google está marcado
**[conferir no Console]**. Política correspondente: [`politica-de-privacidade.md`](politica-de-privacidade.md).

## 0. O que o app coleta e envia (levantamento do código, RT-7)

| Fato | Evidência |
|---|---|
| Único SDK de rede: Google Mobile Ads (AdMob) com UMP | `Packages/manifest.json` (dependência do kit `com.google.ads.mobile` 11.6.0); `MonetizationValidation` recusa pacotes de billing e de analytics |
| Anúncios só depois do consentimento UMP | kit `MobileAdsGate` (`CanRequestAds` antes de `MobileAds.Initialize` e de qualquer `Load`) |
| Premiado opcional (dobrar moedas, continuar uma vez); intersticial pronto e **desligado** | `ClaybreakFlow`, `MonetizationContent.InterstitialOn = false` |
| Unidades de anúncio: só as de **teste** do Google até a RT-9 | `Assets/Claybreak/Content/Data/AdUnits.asset` vazio; `MonetizationValidation.ProductionUnitsAllowed = false` |
| Sem analytics: nenhum evento sai do aparelho | `ClaybreakMetrics` instala `NullMetricsSink` no release (ficha D21 escolhe a ferramenta) |
| Save só local (PlayerPrefs): progresso, moedas, compras com moedas, skins, primeiro dia, presente diário | `SaveData.cs`; nunca enviado |
| Sem login, conta, servidor, compras com dinheiro | sem código de rede próprio; sem `com.unity.purchasing`/`BILLING` |
| Estatística de hardware do Unity e serviços Unity desligados | `ProjectConfiguration` (`submitAnalytics = 0`), `UnityConnectSettings.asset` (tudo `m_Enabled: 0`) |
| Público 13+, conteúdo PG, sem tag infantil | kit `MobileAdsGate` |

Permissões do APK: as mesmas que o SDK de anúncios traz (`INTERNET`, `ACCESS_NETWORK_STATE`, `AD_ID`,
`ACCESS_ADSERVICES_*`, `WAKE_LOCK`, `FOREGROUND_SERVICE`) e `VIBRATE` do kit **[conferir com
`aapt2 dump badging` no AAB da RT-9]**; nenhuma de câmera, microfone, contatos, localização ou armazenamento
(inferência a partir do código do jogo e do kit).

## 1. Respostas propostas

| Pergunta | Resposta proposta | Por quê |
|---|---|---|
| O app coleta ou compartilha algum tipo de dado exigido? | **Sim** | SDK do AdMob |
| Os dados são criptografados em trânsito? | **Sim** **[conferir]** | o SDK do Google usa HTTPS |
| Há forma de pedir exclusão dos dados? | **Não** (sem conta; o save é local e some ao desinstalar) **[conferir o texto]** | sem conta |
| Informações pessoais (nome, e-mail, IDs de usuário) | **Não coletado** | o jogo não pede nada disso |
| Localização aproximada | **Coletada e compartilhada** (AdMob: publicidade, prevenção de fraude) **[conferir]** | IP do pedido de anúncio |
| Localização precisa | Não | sem permissão de localização |
| Atividade no app: interações com anúncios | **Coletada e compartilhada** (AdMob) | |
| Atividade no app: outras ações (fases jogadas) | **Não coletado** enquanto a D21 não instalar uma ferramenta | `NullMetricsSink` |
| Informações e desempenho do app: falhas, diagnóstico | **Coletado e compartilhado** (AdMob) **[conferir]** | documentação do SDK |
| Dispositivo ou outros IDs: ID de publicidade | **Coletado e compartilhado** (AdMob): publicidade ou marketing, prevenção de fraude | permissão `AD_ID` |
| Compras / informações financeiras | Não | D6: sem compra no app |
| Mensagens, fotos, áudio, arquivos, contatos, agenda, saúde | Não | sem permissões |
| Coleta obrigatória ou opcional? | Os dados de anúncio vão quando um anúncio é pedido; o consentimento UMP governa a personalização | `MobileAdsGate` |
| Política de privacidade | URL pública a criar na RT-9 com o texto de `politica-de-privacidade.md` | |

**ID de publicidade** (Conteúdo do app > ID de publicidade): **Sim**, usado para publicidade/marketing e
prevenção de fraude; manter a permissão `AD_ID`.

**Anúncios** (Conteúdo do app > Anúncios): **Sim, o app contém anúncios.**

## 2. O que muda quando...

- **A RT-9 puser os IDs reais do AdMob:** as respostas acima continuam valendo (o SDK é o mesmo); trocar
  `ProductionUnitsAllowed` e o app ID juntos (o validador exige que a unidade pertença ao app ID).
- **O intersticial for ligado (decisão do usuário, D6):** nada novo no formulário (mesmo SDK, mesmos dados);
  revisar a frase de anúncios da política.
- **A D21 escolher uma ferramenta de analytics:** muda **este documento e a política**: "Atividade no app"
  passa a "Coletado" (e talvez "Compartilhado"), mais o que a ferramenta declarar (ex.: ID de instância do app,
  ID de publicidade no Firebase), e a coleta passa a depender de consentimento.
