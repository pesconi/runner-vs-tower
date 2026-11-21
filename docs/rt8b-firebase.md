# RT-8b — Firebase Analytics na configuração mínima (D21 = A)

## 1. O que só você faz (uns 10 minutos, no computador)

Você cria o projeto gratuito do Firebase com a **sua** conta Google. A sessão nunca entra na conta.

1. Abra **console.firebase.google.com** e entre com a conta Google do estúdio.
2. **Criar um projeto** (ou "Adicionar projeto"). Nome: `Clay Siege`. Não ative o Gemini nem planos pagos
   (o plano gratuito **Spark** basta).
3. Na etapa **Google Analytics**: deixe **ativado** (é ele que mede). Crie uma conta do Analytics chamada
   `Jungle Degree`, local **Brasil**. Nas caixas de **compartilhamento de dados** com o Google, **desmarque todas**
   as que forem opcionais. Aceite os termos (só você pode) e crie o projeto.
4. Na página do projeto, toque no ícone do **Android** ("Adicionar app"):
   - Nome do pacote: `com.jungledegree.runnervstower` (exatamente assim)
   - Apelido: `Clay Siege Android`
   - SHA-1: deixe vazio.
   - **Registrar app**.
5. **Baixar google-services.json** e salve exatamente em:

   `C:\projects\games\_privado\runner-vs-tower\google-services.json`

   (crie as pastas `_privado` e `runner-vs-tower`; essa pasta fica **fora** de todos os repositórios.)
   Pule os passos seguintes do assistente ("Adicionar o SDK", Gradle): a sessão faz isso.
6. **Não** ligue a vinculação com o AdMob, o BigQuery nem os "Sinais do Google" (vêm desligados).
7. Responda **"feito"**.

Por que fora do repositório: o arquivo não é um segredo forte (ele vai dentro do APK), mas identifica o
projeto Firebase do estúdio; por padrão fica fora do git, como a chave de upload. O build o procura nesse
caminho (ou no caminho da variável de ambiente `CLAYBREAK_GOOGLE_SERVICES_JSON`), e o `.gitignore` também
recusa `google-services.json` por garantia. Sem o arquivo o jogo funciona igual e não mede nada.

## 2. O que entrou

| Peça | Onde | Como funciona |
|---|---|---|
| SDK | `Assets/Plugins/Android/FirebaseAnalytics.androidlib` | módulo Android com **uma** dependência fixa (`com.google.firebase:firebase-analytics:24.0.0`, SDK nativo) e as flags mínimas no manifesto |
| Flags do manifesto (todas `false`) | mesmo módulo | `firebase_analytics_collection_enabled` (nada é coletado até o jogo ligar), `google_analytics_adid_collection_enabled` (sem ID de publicidade), `google_analytics_ssaid_collection_enabled` (sem ID do Android), `google_analytics_default_allow_ad_personalization_signals`, `..._default_allow_analytics_storage`, `..._ad_storage`, `..._ad_user_data`, `..._ad_personalization` (sem sinais de anúncio), `google_analytics_automatic_screen_reporting_enabled` (sem telas automáticas) |
| Configuração | `ClaybreakFirebase` (Editor) + kit `FirebaseAndroidConfig` | no build, lê o `google-services.json` de fora do repositório, confere cada campo e o pacote, e grava `values/firebase_config.xml` no projeto Gradle gerado; sem o arquivo o build sai igual e não mede nada |
| Adaptador | kit `FirebaseAnalyticsSink` | atrás da interface da RT-7 (`IMetricsSink`); fala com o SDK nativo por JNI; ao liberar a coleta manda o consentimento (análise sim; armazenamento de anúncio, dados de usuário para anúncio e personalização **não**, sempre) e liga a coleta; ao retirar, desliga |
| Regra de consentimento | kit `MeasurementConsent`, lida por `ClaybreakMetrics` ao fim de cada fluxo UMP (`MobileAdsGate.ConsentFlowFinished`) | Europa/Reino Unido (`IABTCF_gdprApplies=1`): só com o propósito TCF 1 (guardar no aparelho), o 8 ou o 9 (medição) e o Google (fornecedor 755) aceitos; fora (`=0`): liberado depois da atualização UMP desta abertura; sem resposta: só depois de uma atualização UMP bem-sucedida sem opções de privacidade exigidas. Retirar no PRIVACY desliga e apaga os dados de análise da instalação (`resetAnalyticsData`) |
| Evento de sessão | kit `PlayerMetrics` | `game_session` (o nome `session_start` é reservado do Firebase) espera na memória até o consentimento e sai uma vez; números vão como números |
| Validador | `MonetizationValidation` | uma dependência só, versão fixa, as 9 flags `false`, sem `collection_deactivated`, nenhum Firebase fora do módulo, nenhum `google-services.json` no repositório, `.gitignore` recusando o arquivo, política e Data safety citando Firebase |
| Prova no aparelho | `Tools/rt8b-firebase-check.sh` | manifesto do APK; instalação limpa; log do Firebase em modo verboso e DebugView; confere que nada é registrado nem enviado antes da linha `[Metrics] Firebase collection enabled` e que os eventos saem com os nomes aprovados |

Build de desenvolvimento e APK de medição: os eventos também vão para o log do aparelho (`adb logcat -s Unity | grep Metrics`).
No Editor só vão para o console.

## 3. Decisões técnicas (registro T-RT8b-1 em `decisoes.md`)

- **SDK nativo em vez do SDK Unity do Firebase.** Os pacotes Unity do Firebase não estão no OpenUPM nem no registro da
  Unity (conferido), trazem bibliotecas C++ por arquitetura e um segundo gerenciador de dependências que brigaria com o
  do AdMob. A medição só precisa de três chamadas; o módulo nativo é menor e não mexe no `mainTemplate.gradle`.
- **O kit não traz o SDK.** O adaptador e a regra de consentimento são genéricos e ficam no kit (0.4.0, entrega K.9);
  a dependência e as flags ficam no jogo, então o Wild Pack não muda de tamanho nem de comportamento.
- **ID de publicidade.** O Firebase **não** coleta o ID de publicidade (flag no manifesto, conferida no APK). A
  permissão `com.google.android.gms.permission.AD_ID` continua no APK porque o **AdMob** a usa (decidido na RT-7 e
  declarado na Data safety); tirá-la mudaria a receita dos anúncios (decisão de produto, fora da D21).
- **Escolha guardada.** O SDK lembra a última liberação entre aberturas: um "sim" anterior volta a valer na
  abertura seguinte até a regra ser lida de novo (como o consentimento guardado do UMP). Só a primeira abertura
  começa com tudo desligado; é ela que `Tools/rt8b-firebase-check.sh` mede (instalação limpa).
- **Mensagem do UMP para a Europa.** Sem resposta TCF, o jogo mede quando o UMP diz que nenhuma opção de privacidade é
  exigida. Isso depende de a mensagem GDPR estar **publicada** no AdMob (Privacidade e mensagens) — item da RT-9,
  que já é exigido para os anúncios.
- **Chave de API do Firebase.** Vai dentro do APK (normal no Firebase); na RT-9, restringir no Google Cloud ao pacote
  e à SHA-1 de assinatura (item do usuário).
- **Arquivo de configuração** em `C:\projects\games\_privado\runner-vs-tower\google-services.json` (ou na variável
  `CLAYBREAK_GOOGLE_SERVICES_JSON`); o build recusa o arquivo se ele estiver dentro de um repositório.

## 4. Provas

- **Nada é enviado antes da liberação:** testes EditMode do kit (`AnalyticsCollectionTests`: com a coleta fechada o
  SDK recebe "desligado" e nenhum evento; a sessão espera; retirar o consentimento desliga) e de regra
  (`MeasurementConsentTests`); no APK, `firebase_analytics_collection_enabled=false`. No aparelho:
  `Tools/rt8b-firebase-check.sh` (ver o status no plano).
- **AD_ID:** no manifesto final do APK, `google_analytics_adid_collection_enabled=false` e as demais 8 flags `false`
  (aapt2, `Tools/rt8b-firebase-check.sh`, parte sem aparelho: 10/10 PASS).
- **Nomes dos eventos:** lista fechada no kit (`game_session`, `level_start`, `level_end`, `rewarded_ad`, teste
  `EventNamesAreTheApprovedOnes` e `TheSessionEventKeepsItsApprovedName`); no aparelho, o log verboso do Firebase
  ("Logging event: origin=app,name=...") e o DebugView do console.

## 5. Peso

| Build | RT-8 (sem Firebase) | RT-8b (com Firebase) | Diferença |
|---|---|---|---|
| APK de release | 34,8 MB | 35,9 MB (37.628.223 bytes) | +1,1 MB |
| AAB de release | 34,6 MB | 35,6 MB (37.347.784 bytes) | +1,0 MB |
| APK de medição | — | 35,9 MB (37.654.819 bytes) | — |

Medição do peso no aparelho (quadro, CPU, GPU, memória): gate 3× abaixo.

## 6. Aceite no aparelho (pendente: OPPO desconectado na sessão de 2026-11-21)

Com o OPPO conectado e desbloqueado (Git Bash, na raiz do repositório):

1. Builds (se `C:	empt8b` tiver sido apagado): `Tools/rt4-check.sh /c/temp/rt8b "apk"`.
2. Prova do Firebase (instalação limpa, ~45 s; jogar uma fase durante a gravação):
   `adb start-server; Tools/rt8b-firebase-check.sh /c/temp/rt8b/Claybreak.apk 45`
   — espera PASS em "no Firebase event or upload before the game enabled collection" e "game_session reached
   Firebase", e os nomes no DebugView do console (Analytics > DebugView).
3. Gate 3× no chefe do mundo 4 (peso do SDK):
   `adb start-server; Tools/rt2-gate.sh rt8b-w4-oppo-60 60 20 /c/temp/rt8b/Claybreak-measure.apk 4`
   — limites: CPU p95 ≤ 11 ms, GPU p95 ≤ 11 ms, quadro p95 ≤ 33,3 ms, < 5% > 50 ms, PSS ≤ 450 MB
   (RT-8 sem Firebase: quadro p95 16 ms, CPU p95 ≤ 4,8 ms, GPU p95 ≤ 4,3 ms, PSS 228 MB).
4. Fechar o app de medição e deixar o normal: `adb shell am force-stop com.jungledegree.runnervstower; adb install -r /c/temp/rt8b/Claybreak.apk`.

