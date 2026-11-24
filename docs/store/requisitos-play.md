# Regras vigentes da Google Play para o Stampeep (pesquisa da RT-9a, 2026-11-24)

Pesquisa nas páginas oficiais do Google em **2026-11-24** (data do acesso; nenhuma das páginas mostra data de
atualização, só "©2026 Google"). Confiança: **[A]** = dito com essas palavras na página oficial; **[M]** = oficial
mas indireto ou deduzido por nós; **[B]** = só fonte secundária, conferir no Console. Não é parecer jurídico.

Conta: **organização** JUNGLE DEGREE, LDA, já verificada pelo Google e usada pelo Wild Pack (que já passou pelo
teste interno em 2026-11-21). A ficha D7 confirma se o Stampeep vai na mesma conta.

## 1. Resumo: números e datas

| Item | Regra hoje | O Stampeep | Fonte |
|---|---|---|---|
| Teste fechado antes da produção (12 testadores durante 14 dias seguidos) | só para contas **pessoais** criadas depois de 13/11/2023 [A]; contas de organização não são citadas, logo a regra não se aplica a elas [M] | **não se aplica** (conta de organização) — conferir se o Console mostra alguma exigência ao pedir produção | [14151465](https://support.google.com/googleplay/android-developer/answer/14151465) |
| Teste interno | até **100** testadores; disponível em minutos; pode não passar pela revisão normal [A] | faixa usada na RT-9b | [9845334](https://support.google.com/googleplay/android-developer/answer/9845334) |
| Teste fechado (se quisermos) | listas de e-mail até 2.000 pessoas, 50 listas por faixa; Google Groups sem limite [A] | opcional | [9845334](https://support.google.com/googleplay/android-developer/answer/9845334) |
| targetSdk de app novo e de atualização | **API 36** (Android 16) desde 31/08/2026; prorrogação até 01/11/2026 já acabou [A] | **36** (`aapt2` no APK da RT-6t.21; o Unity usa o maior SDK instalado, `AndroidTargetSdkVersion: 0`) — OK | [target-sdk](https://developer.android.com/google/play/requirements/target-sdk), [11926878](https://support.google.com/googleplay/android-developer/answer/11926878) |
| Páginas de 16 KB (apps com target Android 15+) | atualização sem suporte bloqueada a partir de **01/02/2027** [A] | **pronto**: todas as `.so` com alinhamento 16384 (`Tools/store/check_16kb.py` no AAB da RT-6t.21) | [page-sizes](https://developer.android.com/guide/practices/page-sizes) |
| 64 bits | toda ABI de 32 bits precisa da de 64 [A] | só `arm64-v8a` (IL2CPP) — OK | [64-bit](https://developer.android.com/google/play/requirements/64-bit) |
| Formato e assinatura | AAB obrigatório para app novo desde 08/2021; AAB exige Play App Signing [A]; chave de upload RSA ≥ 2048 bits [A]. Novidade: apps novos entram por padrão na assinatura "híbrida, pronta para quântica" (3 impressões digitais a registar em APIs de terceiros); dá para trocar a chave de assinatura antes do primeiro lançamento aberto ou de produção [A] | AAB pelo `ClaybreakBuild -claybreakAab`; chave de upload do usuário (passo 2 do passo a passo) | [9844279](https://support.google.com/googleplay/android-developer/answer/9844279), [7384423](https://support.google.com/googleplay/android-developer/answer/7384423) |
| Verificação de programador Android (apps fora da Play) | começa a 30/09/2026 em alguns países da Ásia; global em 2027; quem já está verificado no Play Console tem os apps da Play registados automaticamente [A/M] | nada a fazer além de conferir avisos no Console | [16561738](https://support.google.com/android-developer-console/answer/16561738) |

## 2. Ficha da loja

| Item | Limite | Fonte |
|---|---|---|
| Nome do app | 30 caracteres [A] | [9859152](https://support.google.com/googleplay/android-developer/answer/9859152) |
| Descrição curta | 80 caracteres [A] | idem |
| Descrição completa | 4.000 caracteres [A] | idem |
| Política de metadados (nome, ícone, nome do programador) | sem emoji; sem palavras TODAS EM MAIÚSCULAS fora da marca; nada de "#1", "App do ano", "Popular", prémios; nada de preço ou promoção ("10% off"); nada de "Editor's choice", "New"; sem superlativos sem prova ("best game ever"); depoimentos com autor [A]. Repetir palavras-chave pode suspender o app [A] | [9898842](https://support.google.com/googleplay/android-developer/answer/9898842) |
| Descrição curta (boas práticas) | evitar "Best", "#1", "New", "Free", "Download now"; sem emoji, sem maiúsculas para ênfase, sem pontuação repetida [A] | [9866151](https://support.google.com/googleplay/android-developer/answer/9866151) |

O texto em `ficha/en-US.md` passa nestas regras (`Tools/store/check_listing.py`). Os títulos de seção da descrição
longa estão em maiúsculas; a regra das maiúsculas fala do **nome, ícone e nome do programador**, não da descrição
longa [M]. Se o Console avisar, trocar para "Pick the right gate" etc.

## 3. Gráficos

| Item | Regra | Os nossos (`graficos/`) | Fonte |
|---|---|---|---|
| Ícone | 512×512, PNG 32 bits com alfa, até 1.024 KB; sem selos de ranking, preço ou categoria [A] | `icone-512.png` | [9866151](https://support.google.com/googleplay/android-developer/answer/9866151) |
| Gráfico de destaque | 1024×500, JPEG ou PNG 24 bits **sem alfa**; elementos importantes longe das bordas; evitar fundo branco, preto ou cinza-escuro puro, imagens de aparelho e selos de loja [A] | `destaque-1024x500.png` | idem |
| Capturas de telemóvel | 2 a 8; PNG ou JPEG até 8 MB; cada lado entre 320 e 3.840 px; lado maior ≤ 2,3× o menor [A] | 6 capturas 1080×1920 | idem |
| Capturas para jogos poderem ser promovidos | **pelo menos 3**, todas 9:16 (≥ 1080×1920) ou todas 16:9 [A] | 6 em 9:16, 1080×1920 — OK | idem |
| Conteúdo das capturas | o jogo real; sem palavras promocionais nem chamadas à ação; legenda no máximo 20% da imagem [A] | sem legendas; quadros do próprio jogo | idem |
| Tablet / Chromebook | especificação existe (≥ 4 capturas, lado 1.080–7.680 px), mas a página não diz que é obrigatório para jogo de telemóvel [M] | não feitas | idem |
| Vídeo | não obrigatório; recomendado para jogos (alguns espaços da Play só mostram jogos com vídeo) [A] | não feito (RT-11, sem custo) | idem |

## 4. Conteúdo do app (declarações)

- **Classificação (IARC):** obrigatória para app novo; sem ela o app pode ser removido [A]. Temas do questionário:
  violência, material sexual, casinos/jogos de azar, drogas, álcool, interação online e troca de conteúdo, partilha
  social, mostrar a localização a outros, compras digitais, símbolos nazis, instruções criminosas e "estilo pixelizado
  ou infantil", entre outros [A]. Descrições das classes: ESRB Everyone aceita "violência mínima de desenho animado,
  fantasia ou leve"; IARC 3+ aceita "alguma violência em contexto cómico ou de fantasia" [A]. O texto exato das
  perguntas não é público: conferir no Console. [9859655](https://support.google.com/googleplay/android-developer/answer/9859655),
  [tópico 6181287](https://support.google.com/googleplay/android-developer/topic/6181287)
- **Público-alvo e conteúdo:** quatro partes (idade, detalhes do app, anúncios, presença na loja). As faixas 13–15 e
  16–17 "podem ser consideradas crianças em alguns locais". Sem faixa abaixo de 13, a política de Famílias não se
  aplica, **mas** se a ficha parecer feita para crianças (animação juvenil, personagens novos) a Play pode recusar e
  pedir para mudar os gráficos ou declarar menores de 13 (e então só SDKs de anúncio certificados para Famílias). A
  revisão pode levar 7 dias ou mais [A]. [9867159](https://support.google.com/googleplay/android-developer/answer/9867159)
  — **ponto de atenção do Stampeep** (visual fofo, D25 A): ver `declaracoes.md` seção 3.
- **Acesso ao app ("Dados de início de sessão"):** só se alguma função exigir login; sem login, declarar que tudo
  está disponível sem acesso especial [A]. [9859455](https://support.google.com/googleplay/android-developer/answer/9859455)
- **Anúncios:** declaração própria; qualquer anúncio (premiado, intersticial, banner) = "Sim"; mentir é violação [A].
  Idem.
- **ID de publicidade:** apps com target Android 13+ precisam da permissão `com.google.android.gms.permission.AD_ID`
  (o SDK de anúncios junta-a sozinho) [A]
  ([6048248](https://support.google.com/googleplay/android-developer/answer/6048248)). O formulário pergunta se o app
  usa o ID de publicidade e para quê (publicidade ou marketing, análise, funcionalidade, prevenção de fraude);
  sem a declaração, envios com `AD_ID` são bloqueados [B: FlutterFlow, GoodBarber].
- **Segurança dos dados:** o SDK de anúncios declara IP (localização aproximada), interações com o produto,
  diagnóstico e IDs do dispositivo (ID de publicidade, app set ID), **recolhidos e partilhados**, para publicidade,
  análise e prevenção de fraude, cifrados em trânsito [A]
  ([AdMob](https://developers.google.com/admob/android/privacy/play-data-disclosure)). O Google Analytics for Firebase
  recolhe o ID da instância, o ID de publicidade (desligável — desligado aqui), IP mascarado para localização
  aproximada, eventos de compra e de ciclo de vida, para análise; a página não diz "partilhado" [A]
  ([Analytics 11582702](https://support.google.com/analytics/answer/11582702)). Respostas em `declaracoes.md`.

## 5. Conta de organização: o que aparece na loja

Número D-U-N-S obrigatório; e-mail e telefone de contacto e de programador verificados por código; nome e morada do
perfil de pagamentos iguais aos da D&B; **na Play aparecem publicamente:** nome legal, D-U-N-S, morada legal, e-mail
e telefone do programador; estatuto de **comerciante** obrigatório na UE/EEE e no Reino Unido [A].
[13628312](https://support.google.com/googleplay/android-developer/answer/13628312). Tudo isto já foi feito na conta
(Wild Pack, P1, 2026-11-19); vale para o Stampeep se a D7 ficar na mesma conta.

## 6. AdMob e Firebase

- **Mensagem RGPD ("Regulamentos europeus"):** Privacidade e mensagens > Regulamentos europeus > Criar; escolher o
  app, idiomas, opção "Não consentir", **segmentação** ("Países sujeitos ao RGPD (EEE, Reino Unido e Suíça)" **ou
  "Em todo o lado"**), nome, URL da política, publicar [A].
  [10113207](https://support.google.com/admob/answer/10113207). Existe também a mensagem para estados dos EUA
  [A] ([10860309](https://support.google.com/admob/answer/10860309)).
- **Restringir a chave de API do Firebase:** Cloud Console > APIs e serviços > Credenciais > a chave > Restrições de
  aplicação: "Apps Android" > Adicionar: nome do pacote + SHA-1 > Guardar; e Restrições de API (manter as que o
  Firebase usa, senão dá `API_KEY_SERVICE_BLOCKED`/403) [A].
  [api-keys](https://docs.cloud.google.com/docs/authentication/api-keys), [firebase api-keys](https://firebase.google.com/docs/projects/api-keys).
  Com Play App Signing, registar a SHA-1 da **chave de assinatura do app** guardada pelo Google (Play Console >
  Integridade do app > Assinatura), não só a de upload [A]; juntar a de upload (e a de debug) para testar APKs
  instalados à mão [M]. [7384423](https://support.google.com/googleplay/android-developer/answer/7384423)

## 7. O que isto muda no calendário

- **Nada atrasa por teste fechado:** com conta de organização não há os 14 dias com 12 testadores. A RT-9b vai do
  teste interno direto à preparação do soft launch (RT-10). Se o Console pedir teste fechado ao pedir produção,
  a RT-9b ganha 14 dias e 12 testadores (a lista sai da ficha D9/RT-10).
- **Publicar em produção já é o soft launch** (países escolhidos na D9): a revisão do público-alvo pode levar 7 dias
  ou mais; contar esses dias entre a RT-9b e a RT-10.
- **Sem mudança técnica pendente:** target 36, 16 KB e 64 bits já atendidos.

## 8. Lacunas (conferir no Console)

Texto exato do questionário IARC e do formulário do ID de publicidade; se a verificação de acesso a aparelho vale para
contas de organização; se o Console pede algum teste mínimo para contas de organização novas.
