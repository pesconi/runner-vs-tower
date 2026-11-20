# RT-7 — Monetização, retenção e analytics (relato da entrega)

Decisões que valem: **D6 = B** (premiado + intersticial pronto e desligado, sem compra no app), **D5 = B**
(presente diário local), **D4 = A** (medição mínima consentida; ferramenta na ficha **D21**, pendente).
Registros técnicos: T-RT7-1 a T-RT7-4 em [`decisoes.md`](decisoes.md); no kit, T20 e a entrega K.7.

## O que entrou

| Peça | Onde | Como funciona |
|---|---|---|
| Consentimento UMP antes de qualquer anúncio | kit `MobileAdsGate` | atualiza o consentimento, mostra o formulário onde a lei exige, e só então inicia o SDK (uma vez por app); botão **PRIVACY** na tela inicial onde o UMP pede |
| Premiado: dobrar moedas | `ClaybreakFlow` (resultado) | botão verde "WATCH AD: x2 COINS +N ¢" abaixo do cartão, só com anúncio carregado e só se a fase pagou moedas; uma vez por resultado |
| Premiado: continuar uma vez por fase | `ClaybreakFlow` (assalto) | quando o exército cai no assalto com ≥ 10 unidades na linha: "WATCH AD +N" (metade do exército da linha) ou "NO THANKS"; a torre mantém o dano; as estrelas contam o exército original |
| Intersticial | kit `InterstitialAdService` + `InterstitialPacing` | só depois de vitória, ao tocar NEXT; nunca logo após um premiado; limites [P] 3 fases e 3 min antes do primeiro e entre dois; **desligado** em `MonetizationContent.InterstitialOn` (D6) |
| Presente diário | `DailyReward`, tela inicial | um por dia do calendário local; ciclo 50-75-100-125-150-200-300 moedas [P] que só anda quando se pega (faltar não zera) |
| Medição mínima | kit `PlayerMetrics`, `ClaybreakMetrics` | 4 eventos sem IDs (`game_session`, `level_start`, `level_end`, `rewarded_ad`); no release vão para lugar nenhum; nos builds de desenvolvimento e no APK de medição só para o log do aparelho (`adb logcat -s Unity \| grep Metrics`) |
| Unidades de anúncio | `Content/Data/AdUnits.asset` | vazio: todo build mostra anúncios **de teste** do Google; IDs reais na RT-9 (D7) |
| Validador de monetização | `MonetizationValidation` (também dentro de `ClaybreakValidation`) | D4/D5/D6, unidades só de teste, app ID de exemplo, cena, pacotes proibidos (billing, analytics), documentos |
| Política e Data safety | [`politica-de-privacidade.md`](politica-de-privacidade.md), [`data-safety.md`](data-safety.md) | só o que o código faz, com evidência |

Fora do escopo, de propósito: compra no app e "remover anúncios" (D6: sem IAP; o aceite "compra de teste" não se
aplica); "girar a recompensa do chefe" e "reforço no início" (propostas do GDD 7.2; dois usos bastam para medir
o aceite); missões (D5).

## Aceite

Ver a linha **Status** da RT-7 em [`plano-de-entregas.md`](plano-de-entregas.md).

## Como ver no celular (usuário)

1. Abrir **Clay Siege** no OPPO (APK normal instalado no fim da entrega).
2. Tela inicial: tocar **DAILY GIFT** (aparece uma vez por dia).
3. Vencer uma fase: no resultado, o botão verde **WATCH AD: x2 COINS** abre um anúncio de teste do Google
   (com a etiqueta "Test Ad"); ao fechar depois do prêmio, as moedas dobram.
4. Perder no assalto com exército grande: aparece **ARMY LOST** com **WATCH AD +N** e **NO THANKS**.
5. No Brasil o formulário de consentimento não aparece (a regra é da Europa/Reino Unido); o botão PRIVACY
   também não.
