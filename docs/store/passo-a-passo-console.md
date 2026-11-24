# RT-9b — Passo a passo para publicar o Stampeep no teste interno

Para você fazer no computador, na ordem. Frases curtas; cada passo diz **o que copiar e de onde**. A IA não clica no
Play Console nem no AdMob e nunca vê senhas. Onde aparece **"Peça à IA"**, abra uma sessão e cole a frase.

Antes de começar, responda as fichas **D7** (conta), **D9** (países) e **D10** (idiomas) em
`docs/riscos-e-decisoes.md`. Este roteiro supõe as recomendações: mesma conta da empresa, inglês, países da Europa.

Tempo estimado: 2 a 3 horas no total, mais a espera das revisões do Google.

---

## Passo 1 — Publicar a política de privacidade (15 min)

1. Abra `docs/politica-de-privacidade.md`.
2. Procure por `<`. Troque os 10 campos pelos dados reais da empresa (5 em português, 5 em inglês):
   denominação legal, NIPC, morada, e-mail de contacto e a data de hoje.
   Dica: se forem os mesmos da política do Wild Pack, copie de lá (https://jungledegree.com/wild-pack/privacy/pt/).
3. **Peça à IA:** "Gere o HTML da política do Stampeep e confira que não sobrou campo."
   (Ela roda `python -X utf8 Tools/store/make_privacy_html.py --check`.)
4. Copie a pasta `docs/store/site/stampeep/` para o repositório do site (`jungledegree-site`), na raiz, ao lado de
   `wild-pack/`. Publique o site como fez com o Wild Pack.
5. Abra https://jungledegree.com/stampeep/privacy/ no celular. Tem de abrir a página, sem nenhum campo amarelo.

## Passo 2 — Criar a chave de upload (10 min, só uma vez)

A chave de upload assina o arquivo que você envia à Play. Ela fica **só no seu computador**, fora do repositório.

1. Abra o PowerShell e crie a pasta: `mkdir $HOME\keys`
2. Rode (uma linha só):

   ```powershell
   & "C:\Program Files\Unity\Hub\Editor\6000.6.7f1\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK\bin\keytool.exe" -genkeypair -v -keystore $HOME\keys\stampeep-upload.jks -alias upload -keyalg RSA -keysize 2048 -validity 10000
   ```

3. Ele pede uma senha (duas vezes) e o seu nome/empresa. Use uma senha forte.
4. Guarde a senha num gestor de senhas e faça uma cópia do arquivo `stampeep-upload.jks` (pen drive ou nuvem
   privada). Se perder a chave, é preciso pedir uma nova à Play (demora dias).

Não reutilize a chave do Wild Pack: cada jogo tem a sua.

## Passo 3 — Criar o app no Play Console (10 min)

1. Entre em https://play.google.com/console com a conta da empresa (D7).
2. **Criar app**. Preencha:
   - Nome do app: `Stampeep: Storm the Tower` (copie de `docs/store/ficha/en-US.md`, bloco "App name").
   - Idioma padrão: **English (United States) – en-US** (D10).
   - App ou jogo: **Jogo**. Gratuito ou pago: **Gratuito**.
   - Marque as declarações (políticas do programador, leis de exportação dos EUA).
3. Criar app.

## Passo 4 — Criar o app no AdMob (10 min)

1. Entre em https://admob.google.com com a conta **business** da empresa (a do Wild Pack; não a pessoal).
2. **Apps > Adicionar app > Android**. "Está publicado numa loja?" → **Não** (ainda não está). Nome: `Stampeep`.
3. Dentro do app: **Unidades de anúncio > Adicionar > Premiado**. Nome: `Stampeep rewarded`. Deixe o resto como está.
4. Anote os dois códigos que o AdMob mostra (são públicos, não são senha):
   - ID do app: `ca-app-pub-...~...`
   - ID da unidade premiada: `ca-app-pub-.../...`
5. Os dois códigos vão para o arquivo `C:\projects\games\_privado\runner-vs-tower\admob-ids.json`, **fora do
   repositório e sem commit** (D35 = A), com os campos `androidAppId` (o do app, com `~`) e `rewardedAdUnitId` (o da
   unidade, com `/`). **Peça à IA:** "RT-9b: os IDs do AdMob do Stampeep estão no admob-ids.json; valida e gera o
   APK." O build do jogo lê o arquivo, confere o formato e que os dois são da mesma conta, e falha com aviso se ele
   faltar; o desenvolvimento e o APK de medição usam os anúncios de teste do Google. O intersticial continua
   desligado (D6; não precisa de unidade agora). **Feito em 2026-11-24** (arquivo criado; build validado na RT-9b.1).

## Passo 4b — Firebase com o pacote novo (10 min)

Desde a D34 o pacote do jogo é `com.jungledegree.stampeep`. O arquivo do Firebase que está no computador foi feito
para o pacote antigo; com ele, o jogo funciona mas **não mede nada**. Faça antes do passo 5.

1. Entre em https://console.firebase.google.com e abra o projeto do jogo (ainda chamado "Clay Siege").
2. **Visão geral do projeto > Adicionar app > Android**.
   - Nome do pacote Android: `com.jungledegree.stampeep` (exatamente assim).
   - Apelido: `Stampeep`. SHA-1: deixe vazio. **Registar app**.
3. Baixe o `google-services.json` que ele oferece.
4. Ponha esse arquivo em `C:\projects\games\_privado\runner-vs-tower\`, **substituindo** o que já está lá (mesmo
   nome). Nunca dentro da pasta do projeto.
5. Os outros passos do assistente do Firebase (Gradle, SDK): pule com **Seguinte** até o fim. O jogo já tem tudo.
6. **Peça à IA:** "RT-9b: baixei o google-services.json do Stampeep; gera o APK e valida o Firebase no OPPO."
   (Ela roda `Tools/rt8b-firebase-check.sh`, que primeiro confere se o arquivo é do pacote novo.)

O app antigo (`com.jungledegree.runnervstower`) pode ficar no Firebase; não atrapalha.

## Passo 5 — Gerar o AAB assinado (20 min, quase tudo espera)

O AAB é o arquivo que vai para a Play. Faça depois de a IA terminar os passos 4.5 e 4b.6.

1. Feche o Unity se estiver aberto neste projeto.
2. No PowerShell, na pasta do projeto (`C:\projects\games\RunnerVsTower`), rode:

   ```powershell
   powershell -ExecutionPolicy Bypass -File Tools\store\build-signed-aab.ps1 -Keystore $HOME\keys\stampeep-upload.jks
   ```

3. Ele pede a senha da chave (não aparece na tela). Espere ~10 minutos.
4. No fim ele mostra o arquivo (`C:\temp\rt9b\Stampeep-0.1.0-1.aab`) e o certificado. Confira que **não** diz
   "Android Debug". Anote a linha **SHA1** (vai no passo 10).

## Passo 6 — Enviar ao teste interno (15 min)

1. Play Console > Stampeep > **Testar e lançar > Testes > Teste interno**.
2. Aba **Testadores > Criar lista de e-mails**. Nome: `Equipa`. Ponha o seu e-mail (e o da Flávia, se quiser).
   Guarde.
3. **Criar nova versão**. Na primeira vez o Console pergunta pela assinatura do app: aceite **a assinatura de
   apps do Google Play** (o Google guarda a chave final; você só usa a de upload). Se ele falar de "assinatura
   híbrida pronta para quântica", aceite o padrão.
4. Arraste o arquivo `.aab` do passo 5. O Console mostra o pacote: tem de ser `com.jungledegree.stampeep`. Este
   primeiro envio fixa o pacote para sempre; se aparecer outro, pare e **peça à IA**.
5. Nome da versão: deixe o automático (`1 (0.1.0)`). Notas da versão (en-US): `First internal test build.`
6. **Seguinte > Guardar e publicar**. Em poucos minutos fica disponível.
7. Na aba Testadores, copie o **link de participação**, abra-o no OPPO (com a mesma conta Google da lista),
   aceite e instale pela Play.

Antes de instalar pela Play, desinstale a versão de teste que está no OPPO (a assinatura é diferente; o progresso de
teste se perde — é só do teste).

## Passo 7 — Declarações do app (40 min)

Play Console > **Política > Conteúdo do app**. Para cada item, copie as respostas de `docs/store/declaracoes.md`:

| Item no Console | Seção de `declaracoes.md` |
|---|---|
| Política de privacidade | 9 (URL `https://jungledegree.com/stampeep/privacy/`) |
| Anúncios | 2 (**Sim**) |
| Acesso ao app | 5 (tudo disponível sem acesso especial) |
| Classificação de conteúdo | 4 (questionário IARC, categoria Jogo) |
| Público-alvo e conteúdo | 3 (13–15, 16–17, 18+; se perguntar se atrai crianças, leia a nota da seção 3 antes) |
| Segurança dos dados | 1 (4 tipos de dados, tabela) |
| ID de publicidade | 6 (**Sim**, publicidade e prevenção de fraude) |
| Apps governamentais, financeiros, saúde, notícias etc. | 7 (todos **Não**) |

Se uma pergunta não estiver no documento, não adivinhe: tire um print e **peça à IA**: "Pergunta nova no Console do
Stampeep: <texto>. O que responder?"

## Passo 8 — Ficha da loja (20 min)

Play Console > **Aumentar utilizadores > Presença na loja > Ficha principal da loja**:

1. Nome, descrição curta e descrição completa: copie os três blocos de `docs/store/ficha/en-US.md`.
2. Ícone: `docs/store/graficos/icone-512.png`.
3. Gráfico de destaque: `docs/store/graficos/destaque-1024x500.png`.
4. Capturas de ecrã do telemóvel: as 6 de `docs/store/graficos/capturas/` (01 a 06, nessa ordem).
5. Tablets: deixe vazio.
6. **Definições da loja:** categoria **Jogo > Ação**; e-mail de contacto o da empresa; site
   `https://jungledegree.com`.
7. Guardar.

## Passo 9 — Mensagem RGPD no AdMob (15 min)

Sem esta mensagem, na Europa o jogo não pede consentimento e não mostra anúncios nem mede nada.

1. AdMob > **Privacidade e mensagens > Regulamentos europeus > Criar mensagem**.
2. App: **Stampeep**. Idiomas: **English** e **Português (Portugal)** (pode juntar outros dos países da D9).
3. Opção **"Não consentir"**: ligada (é exigida para o RGPD).
4. **Segmentação: "Em todo o lado" (Everywhere)** — não só "países sujeitos ao RGPD". A empresa é portuguesa e
   aplica o RGPD a todos (decisão técnica T-RT9a-3).
5. URL da política: `https://jungledegree.com/stampeep/privacy/`.
6. Parceiros de publicidade: deixe a lista padrão do Google. Se a mensagem pedir "finalidades", deixe as padrão.
7. **Publicar**.
8. **Peça à IA:** "RT-9b: a mensagem RGPD do Stampeep está publicada; confere no OPPO." (Ela usa
   `Tools/rt8b-firebase-check.sh` com o celular ligado por cabo.)

## Passo 10 — Proteger a chave do Firebase (10 min)

A chave de API do Firebase vai dentro do app (é normal). Restringi-la impede que outros a usem noutro app.

1. Play Console > Stampeep > **Testar e lançar > Configuração > Integridade do app > Assinatura de apps**. Copie a
   **impressão digital SHA-1 do certificado da chave de assinatura do app** (a do Google).
2. Abra https://console.cloud.google.com, escolha o projeto do Firebase do jogo (ainda chamado "Clay Siege").
3. **APIs e serviços > Credenciais**. Clique na chave "Android key (auto created by Firebase)".
4. **Restrições de aplicação: Apps Android > Adicionar**, uma linha para cada SHA-1, todas com o pacote novo:
   - nome do pacote: `com.jungledegree.stampeep` (o pacote antigo `com.jungledegree.runnervstower` não serve mais;
     se ele já estiver na lista, pode apagar a linha dele)
   - SHA-1: a do passo 10.1. Adicione também a SHA-1 da chave de upload (passo 5.4).
5. **Restrições de API:** deixe as que já estão marcadas (o Firebase marcou as dele). Não tire nenhuma.
6. Guardar. Espere 5 minutos, abra o jogo instalado pela Play e **peça à IA** para conferir os eventos no
   DebugView (passo 9.8).
7. Opcional: no Firebase, renomeie o projeto e o app de "Clay Siege" para "Stampeep" (só o nome que você vê; não
   muda nada no jogo).

## Passo 11 — Conferir e avisar

1. No OPPO, instalado pela Play: abra o jogo, aceite ou recuse o consentimento, jogue uma fase, veja um anúncio
   premiado (pode demorar a aparecer nos primeiros dias: o AdMob revê o app).
2. Em SETTINGS, toque no link da política: deve abrir a página do passo 1.
3. Play Console > **Painel**: não pode haver aviso vermelho. Se houver, print e **peça à IA**.
4. Diga à IA: "RT-9b feita" — ela fecha a entrega e prepara a RT-10.

## Teste fechado: precisa?

Não para a sua conta. A regra dos 12 testadores durante 14 dias é só para contas **pessoais** criadas depois de
13/11/2023; a da Jungle Degree é de **organização** (`docs/store/requisitos-play.md`). Se, ao pedir produção, o
Console mostrar essa exigência mesmo assim, avise a IA: o calendário ganha 14 dias.
