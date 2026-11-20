# Varredura de loja — RT-0 (gancho e nome)

Data da varredura: **2026-11-20**. Escopo: só pesquisa web gratuita (WebSearch e leitura
de páginas públicas, principalmente fichas da App Store, porque a ficha da Play não
abriu). Nada foi baixado, instalado ou jogado. Nenhuma conta criada.

Convenção: **[F]** fato com URL e data de acesso; **[I]** inferência minha; **não
verificado** = não consegui confirmar. **Limite importante:** li o *texto* das fichas;
**não vi capturas, vídeos nem ícones**, então "primeiros 30 s", "ciclo da fase" e "estilo
do ícone" estão quase todos **não verificados**. Isto não é busca oficial de marca.
Esta varredura **não decide nada**: D2 (gancho) e D1 (nome) voltam ao usuário.

Nota: o plano citava `docs/runner-vs-tower/varredura.md`; o caminho oficial é este
(`docs/varredura.md`), já alinhado no plano na RT-0. Depois desta varredura, D2 foi
decidida como **A** e D1 como nome de trabalho "Claybreak" (`decisoes.md`, 2026-11-20).

---

## 1. Quem já faz "correr/construir o exército e depois atacar"

| Jogo | O que a ficha diz [F] | Meta | A corrida define o exército? |
|---|---|---|---|
| **Mob Control** (Voodoo) | Mirar e atirar por portões multiplicadores para crescer a multidão; mandar Campeões furar a multidão inimiga e chegar à base; bases, cartas, Boss Levels, passe mensal ([App Store, 2026-11-20](https://apps.apple.com/us/app/id1562817072)) | Base inimiga | Não há "corrida": o jogador atira de um canhão. A multidão que passa pelos portões é a que ataca ([PocketGamer.biz, 2022-10-13, texto patrocinado pela Voodoo](https://www.pocketgamer.biz/comment-and-opinion/79857/mob-control-hybridising-a-hypercasual-hit/)). Composição: via Campeões/cartas, não via corrida [F, ficha]. |
| **Count Masters** (Freeplay) | Escolher portão, correr com a multidão pela cidade, enfrentar multidões rivais, vencer o "rei" e tomar o castelo ([App Store, 2026-11-20](https://apps.apple.com/us/app/count-masters-crowd-runner-3d/id1568245971)) | Castelo / chefe no fim da fase | Sim para **quantidade** (implícito na ficha). Composição: não vi nada. |
| **Join Clash** (Freeplay) | Juntar multidão correndo, desviar de serras/machados, coletar chaves, choque "parede contra parede", chefes, capturar castelos ([App Store, 2026-11-20](https://apps.apple.com/app/1499812410)) | Castelo no fim | Quantidade sim (implícito). Composição: não vi. |
| **Mob Run: Stickman Crowd Runner** | Portões x2 vs +50, soltar stickmen, armadilhas, choque com multidões, arco e flecha, upgrades Ataque/Armadura/Dinheiro, chefe gigante ([App Store, 2026-11-20](https://apps.apple.com/us/app/-/id6544786004)) | Multidões inimigas e chefe | Quantidade sim (a ficha diz que o maior exército vence). |
| **Castle Wipeout** | Corre, desvia, coleta companheiros da mesma cor para crescer a multidão, toma o castelo sob ataque; loja de armas/chapéus ([App Store via busca, 2026-11-20](https://apps.apple.com/us/app/-/id6526485576); a ficha deu 404 na leitura direta, só vi o resumo da busca) | Castelo | Quantidade sim. Composição: não vi. |
| **Town Rush** | Toque para gerar stickmen e atacar a torre inimiga; melhorar as torres ([App Store CA, 2026-11-20](https://apps.apple.com/ca/app/town-rush/id1567822996)) | **Torre** | Não há corrida; unidades nascem durante a batalha. |
| **Crowd Arena – Tower Defense** | Montar exército e atacar a torre inimiga, segundo resumo de busca ([Play](https://play.google.com/store/apps/details?id=com.anloft.crowdarena&hl=en_US), 2026-11-20; ficha não lida) | Torre | Não verificado. |
| **Gate Militia: Crowd Army Run** | Portões somam/multiplicam/reduzem; chefes no fim; 1.000+ instalações ([site](https://gatemilitia.optygate.com/), 2026-11-20) | Chefe | Quantidade sim. |

- **"Clash of Crowds":** não achei ficha na Play com esse nome exato. O que apareceu com
  esse nome é um jogo de "absorver rivais" sem portões nem torre ([APKCombo, 2026-11-20](https://apkcombo.com/clash-of-crowds/com.ServicesTechnomaniappsInc.ClashofCrowds/)).
  A referência do GDD provavelmente aponta para outro título. **Não verificado.**
- **Primeiros 30 s, ciclo de fase em detalhe:** **não verificado** para todos (sem
  capturas/vídeo). O que as fichas permitem dizer **[I]**: o padrão do gênero é
  "portões → choque/chefe no fim", com fase curta.
- **Torre vs castelo vs chefe [F]:** castelo + chefe (Count Masters, Join Clash, Castle
  Wipeout); base (Mob Control); torre (Town Rush, Crowd Arena). **Torre exata como meta de
  um jogo de corrida:** não achei.
- Retenção do Mob Control (D1 36,1%, D7 13,9%) já está no GDD (Udonis/AppMagic); não
  reverifiquei hoje.

## 2. Os ganchos do GDD seção 5

**Resumo [I]: não achei nenhum dos três exatamente como descrito. Ausência de
resultado numa busca de texto não prova que não exista.**

- **A (corrida real define quantidade *e composição*).** Quantidade: **já existe**
  (Count Masters, Join Clash, Mob Run, Castle Wipeout; ver tabela). Composição: Join
  Clash tem chaves na pista [F, ficha], mas não vi o que elas fazem; Castle Wipeout fala
  em coletar companheiros "da mesma cor" [F, busca]. Então "a corrida decide a
  composição" **não achei claramente**, mas há sinais próximos. Só a parte da composição
  seria distintiva; "corra e depois ataque" em si **não é**.
- **B (a torre manipula os portões no assalto).** Mob Control tem "portões móveis" e
  Boss Levels [F, ficha]; **não achei** torre que feche faixas ou troque portões. Evidência
  fraca de ausência (só texto de ficha). Protótipos de jam com "corrida + torre"
  existem, mas são outro formato ([Tower Defense of the Heart, itch.io](https://keithcomet.itch.io/tower-defense-of-the-heart)).
- **C (multidão vira escada).** **Existe em parte:** *Crowd Climb* (OHM Games) é
  descrito como correr, multiplicar e escalar muros empilhando unidades ([resumo de busca
  da App Store, 2026-11-20](https://apps.apple.com/us/app/id1560974689?l=en-us); a ficha
  deu 404 na leitura direta). Também *Stacky People*, *Stack Run!* e *Human Tower.io*
  usam empilhar gente. O que **não achei** é empilhar como forma de derrubar uma torre
  num assalto de multidão. O mecanismo, porém, **não é novo**.

## 3. Nomes e ícones

Lista de nomes do gênero a **evitar parecer** [F, fichas acima]: Mob Control, Mob Run,
Mob Attack, Count Masters, Count Master: Crowd Clash Run, Crowd Clash, Crowd Battle 3D,
Crowd Attack!, Crowd Climb, Crowd Arena, Crowd Elite 3D, Commander Force Crowd Control,
Clash of Crowd(s), Clash Crowd, Join Clash, Castle Wipeout, Castle Raid!, Castle
Conqueror!, Town Rush, Tower Rush (outro jogo, PvP), Gate Militia, War of Sticks, Stick
Clash.

Padrões de ícone/arte: **não verificado** (não vi imagens). Pelo texto, o gênero usa
**stickmen**/bonecos genéricos coloridos (azul vs vermelho) e "multidões" 3D; a ficha do
Mob Control cita "novo ícone" sem descrever. **[I]** O que evitar: stickman azul
genérico, números de portão em destaque neon, canhão azul com multidão. O GDD já decide
massinha/paleta quente, que foge disso; confirmar vendo as capturas da Play antes de
fechar o ícone.

Colisões de nome (busca web, 2026-11-20; **não é busca oficial de marca**):

| Nome | Resultado |
|---|---|
| **Runner vs Tower** | Nenhum app com o nome exato. Parecidos: *Runner Defense* (app iOS, [ficha](https://apps.apple.com/us/app/id1637389354)), *Towers Vs. Runners* (modo de comunidade, [workshop.codes](https://workshop.codes/CBF7W)), *Tower Runner*, *Runner Vs Turret* (itch.io). Baixa colisão, mas genérico e descritivo (SEO fraco). |
| **Wild Pack** | Nenhuma ficha na Play com o nome exato no resultado. Muitos jogos de lobos ocupam termos próximos ("Wolf Game: Wild Animal Wars", "Wolf Tales – Wild Animal Sim"; [exemplo](https://apps.apple.com/US/app/id1621026079)). Não verificado se a própria ficha do estúdio aparece na Play pelo nome. |
| **Claybreak** (sugestão) | Nenhum app; existe *Claybreak Range*, jogo de navegador de tiro ao prato ([Playgama](https://playgama.com/game/claybreak-range), 2026) e o histórico *Clay Jam*. Colisão baixa-média. |
| **Hordefall** (sugestão) | Nenhuma ficha encontrada; vários "Horde ..." próximos (Horde Halt, Heroes vs Hordes). Colisão baixa; "Horde" é termo comum. |
| **Toppler** (sugestão, alternativa "Tower Toppler") | Nenhuma ficha encontrada com "Siege Rush"/"Tower Toppler"; apareceu *Tower Rush* (Solar Forge, PvP), nome vizinho. Fraca verificação (uma busca). |

Os três nomes sugeridos são meus, sem os termos Mob/Count/Crowd Run, e **não foram
registrados nem checados em base de marcas**.

## 4. "Jogos que o testador vai citar como parecidos" [I, a partir do que apareceu]

1. **Mob Control** (o mais provável, por ser o líder e por "torre/base").
2. **Count Masters** (corrida com portões e castelo/chefe).
3. **Join Clash** (idem).
4. Wild Pack (do próprio estúdio, já previsto no critério do GDD).
5. Town Rush / Crowd Arena (se a torre for central).
6. Clash of Clans / Clash Royale (testadores leigos citam "Clash" por qualquer castelo).

## 5. Conclusão (máx. 10 linhas)

1. **[F]** "Corra com portões e depois enfrente um castelo/chefe" é **padrão do gênero**
   (Count Masters, Join Clash, Castle Wipeout, Mob Run); o Mob Control é o caso de "base".
2. **[F]** Não achei jogo que una corrida real e assalto a **torre** com vida e defesas.
   **[I]** Esse é o espaço livre; "corra e depois ataque" sozinho não distingue.
3. **A** isolada: quantidade já é comum; só a **composição vinda da corrida** seria
   distinta, e ela não foi confirmada nem refutada.
4. **B**: não achei igual; maior diferença sentida no assalto, mas risco de equilíbrio e CPU (GDD).
5. **C**: o empilhar já existe em corridas leves (Crowd Climb etc.); como assalto a torre
   não achei, mas custo técnico alto (GDD).
6. **[I] Recomendação:** manter **A + traço de B** como o GDD propõe, com a promessa de
   venda centrada em **torre com defesas** (e composição por corrida), não em "corrida".
7. **[I]** Distinção real é **moderada**; só o protótipo (pergunta "com que jogo se
   parece?") confirma. Limite desta varredura: sem capturas/vídeo/jogo real.
8. Decisão final de D2 e D1 é do usuário.
