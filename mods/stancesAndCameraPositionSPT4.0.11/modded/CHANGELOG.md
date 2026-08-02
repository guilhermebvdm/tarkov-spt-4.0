# Changelog

Changelog do fork (Tarkov Red Line). O histórico do mod original vai até a v1.1.4 e está em
[CHANGELOG_SIMPLIFIED.md](./CHANGELOG_SIMPLIFIED.md).

Versões mais recentes primeiro.

---

## v2.14.0 (2026-08-01)

### Diagnóstico do "andar que trava devagar"

Nova opção `Debug Speed Limits` (seção `Debug (Advanced)`): mostra na tela **todas as causas de limite de
velocidade ativas**, qual delas está vencendo, e o limite resultante — mais uma linha no log **sempre que a
causa vencedora muda**, para o problema poder ser diagnosticado depois, sem depender de estar olhando a tela
na hora. Os valores são fatores normalizados (1 = sem limite), não m/s.

**Por que só um instrumento e não a correção:** a hipótese que estava registrada há semanas — "o teto é
calculado uma vez e não é recalculado quando a velocidade máxima muda, então agachar e levantar prende a
velocidade no valor do agachado" — **não se sustenta na leitura do jogo**:

- `MovementContext.MaxSpeed` **não depende da pose**. É função do backend e da skill Strength
  (`MovementContext.cs:910`), então agachar ou levantar não muda o valor de que o mod deriva o teto.
- O recálculo **não é preguiçoso**: `ProcessSpeedLimits` roda **todo frame** dentro de `ManualUpdate`
  (`MovementContext.cs:2499`), e o mod ainda re-aplica defensivamente por conta própria a cada tick.

O jogo escolhe o teto pegando o **menor** valor de um dicionário de causas (`BarbedWire`, `HealthCondition`,
`Aiming`, `Weight`, `SurfaceNormal`, `Swamp`, `Shot`, `Armor`, `Fall`, mais a do mod). O sintoma relatado —
anda devagar sem motivo, e mirar ou trocar de postura destrava — é compatível com **uma causa que fica
registrada quando deveria ter sido removida**, e mirar/desmirar força a remoção. Qual delas, só a captura
in-game responde: é para isso que serve esta tela.

---

## v2.13.0 (2026-08-01)

### A compressão de ADS-speed ficou visível e confiável

- **Nova opção `Debug ADS Speed`** (seção `Debug (Advanced)`): mostra na tela a velocidade de mira da arma em
  mãos, **nativa → comprimida**, com o tempo de mira em segundos. Sem ela a compressão era invisível — o efeito
  só aparecia na sensação, e não havia como saber se estava aplicando.
- ⚠️ **O `ADS Speed Pivot` default (1.5) está acima da faixa real das armas.** Derivando de `globals.Aiming`
  (pesos 0,6–9 kg e tempos 0,35–2,4 s), a velocidade interna vai de **~0,57 (LMG) a ~1,9 (pistola)**, com fuzis
  em ~1,0. Com o pivô em 1,5, a compressão **acelera as armas pesadas** em vez de segurar as leves, e uma
  compressão moderada parece não fazer nada. O centro real é **~1,0–1,1**. O default não foi alterado (mudá-lo
  agora mexeria na calibração de quem já ajustou) — use o overlay para calibrar.
- **Corrigido: a compressão podia acumular sobre si mesma.** A proteção contra recomprimir checava o campo
  `_firearmController`, mas o EFT decide se recalcula o valor nativo olhando `_firearmAnimationData.Weapon`.
  Na janela em que existe controlador sem dados de arma, o mod comprimia por cima do já comprimido. O guard
  agora é idêntico ao do jogo.

### F12

- **A seção `Action Stances` deixou de existir.** Sua única opção (`Enable Action Stance Swap`) passou para o
  **rodapé de `Stance Cycle & Hotkeys`** — é uma troca automática de postura, mora junto das outras formas de
  trocar de postura. ⚠️ **A opção volta ao padrão (ligada)** para quem atualizar: o BepInEx identifica cada
  ajuste pelo par (seção, chave), e a entrada antiga vira órfã. Coberto pela redistribuição do `.cfg` pelo
  launcher.
- **`ADS Kick Delay (In)` e `Tac Sprint Reset Delay` voltaram a ser exibidos em segundos.** Ambos são tempos,
  mas tinham faixa 0–1, e o ConfigurationManager renderiza toda faixa exatamente 0–1 como **porcentagem sem
  caixa de digitação** — apareciam como "15%" e "35%". Faixa alargada para 0–2: volta o valor em segundos, com
  caixa para digitar, e agora aceita atrasos acima de 1 s. Valores salvos continuam válidos.

### Documentação

- `PROPRIEDADES.md`: 7 títulos de seção traziam um sufixo `— Item NNN` que **não existe no F12** — quem
  buscasse pelo nome do documento não achava a seção no jogo. O código do item de backlog saiu do título e
  virou uma linha `> Origem: item NNN do backlog.` logo abaixo.

---

## v2.12.1 (2026-07-27)

### Correção — o painel de munição da câmara não aparecia

A entrega da v2.10.0 pedia o painel ao jogo por um caminho indireto (assinar o evento `Player.OnShowAmmoDetails`
por reflexão): o assinante era encontrado, nenhum erro acontecia, **e o painel simplesmente não desenhava** no
teste real (estande de tiro do esconderijo). Agora a chamada é direta na tela de combate
(`EftBattleUIScreen.ShowAmmoDetails` via `Singleton<CommonUI>`), abordagem que o RealismMod já usava — some a
dependência de **qual instância de jogador** o evento estava ligado. Corrigido junto o limite de contagem de
munição, para bater com a fórmula do painel nativo. Ref: code-review 02/03 do item 019.

> **Nota de histórico:** o `PickupAimingSafetyPatch` chegou a este mod em 2026-07-25 (vindo do `TRL-Fixes`,
> commit `19aa6499`) **sem release próprio** — não existiu uma v2.12.0; o número saltou de 2.11.0 para 2.12.1.
> O patch foi devolvido ao `TRL-Fixes` na v2.13.0, ver a seção "Escopo" daquela versão.

---

## v2.11.0 (2026-07-26)

### Endurecimento da sincronização de rede FIKA

⚠️ **Release lockstep:** o formato do pacote de stance mudou. Todos os jogadores **e o headless**
precisam atualizar juntos — um peer em 2.10.0 não entende o pacote novo.

- O corpo do pacote passou a ir dentro de um **envelope de comprimento**. O `NetPacketProcessor` do
  FIKA lê o datagrama em laço; um `Deserialize` que consumisse um número de bytes diferente do que
  o `Serialize` escreveu desalinhava o leitor e gerava `ParseException: Undefined packet in
  NetDataReader`. Como o `PollEvents` do LiteNetLib drena a fila de eventos **sem try/catch**, essa
  exceção descartava todos os eventos pendentes do frame — inclusive os de posição/movimento do
  FIKA e os dos outros mods. O envelope torna esse desalinhamento impossível.
- A leitura passou a usar as variantes `TryGet*` (não lançam) e o callback ganhou guard de
  `GameWorld.Instantiated` antes de criar o `ObservedStanceAnimator`.
- `StanceSyncPacket` (formato ≤2.10.0) segue registrado **só para recepção**, então 2.11.0 continua
  entendendo peers que ainda não subiram.
- Erros de rede passaram a logar com stack trace na primeira ocorrência de cada tipo e com throttle
  de 5 s depois, evitando flood no console.

---

## v2.10.0 (2026-07-19)

### Nova feature — UI ao checar a câmara (item 019)

Ao **checar a câmara** in-raid, agora aparece o **mesmo painel do check de carregador** mostrando se há bala e
**qual é** (o tipo). O vanilla não mostrava nada no HUD ao checar a câmara. Com bala → "Full" + nome da munição;
câmara vazia → "Empty" (útil junto do Manual Chambering, pra saber se precisa dar rack no ferrolho).

- Toggle F12: seção **Weapon Inspection** → `Show Chamber Ammo On Check` (default on).
- Reutiliza o evento nativo `Player.OnShowAmmoDetails` (mesma UI do check-carregador); só local (sem sync Fika).
- Code review adversarial: 0 🔴; achados menores aplicados (log one-shot, paridade do painel).

---

## v2.9.0 (2026-07-19)

### Defaults promovidos da config calibrada do servidor

Os valores de fábrica do mod passam a ser a **config calibrada in-game** do servidor — instalações limpas (ou
chaves ausentes no `.cfg`) já nascem com o tuning correto, sem precisar importar `.cfg`. Afeta **só quem não tem a
chave gravada**; `.cfg` existentes mantêm seus valores.

**Promovido (30 chaves):** poses das Stances 1/2 (Yaw, Roll, Up/Down), speed multipliers por stance (S0=80, S1=90,
S2/S3=100), stamina (S1=3, S2=4, ADS stand=0.1, hold-breath stand=0.6, prone=2, hold-breath prone=0.8),
inércia=3 / walk=0.9 / sprint=0.8, transition speed=0.8, kick=-0.025, overshoot damping=15, snap-on-fire por
stance (S2 on, S3 off), `Stance 2 ADS Waypoint=off` (Low Ready dispensa o waypoint), mouse-wheel cycle on,
hotkeys de teclado (Toggle, Stance 3) desligadas, volumes de respiração/batimento ≈ mudos (0.01), barra de
oxigênio off.

**Mantido no default histórico (não promovido, decisão de produto):**
- `Debug Transition Metrics = false` — é a régua de diagnóstico (F0); não deve nascer ligada para o player.
- `Mouse Wheel Modifier = LeftAlt` — evita conflito com o agachar (Ctrl) do EFT.

Detalhe técnico: `Stance 2 ADS Waypoint` default divergia por stance, então o tuple `_stanceDefaults` ganhou o
campo `AdsWaypoint` (S1/S3 = true, S2 = false).

---

## v2.8.2 (2026-07-19)

### Correção (apresentação no F12)

As opções `ADS Waypoint` / `...Time` de cada postura agora aparecem no **rodapé** da seção (abaixo de
`Snap to Stance 0 on Fire`), no mesmo lugar nas três — pedido do usuário: as posições ficam na ordem natural em
cima e os pares experimentais/calibráveis do waypoint no fim. (A v2.8.1 os tinha posto no topo; este layout é o
preferido.) Sem mudança de comportamento.

---

## v2.8.1 (2026-07-19)

### Correção (apresentação no F12)

As opções `Stance 2 ADS Waypoint` / `...Time` apareciam **intercaladas** com os sliders de posição da Low Ready
(empate de ordenação). Agora as opções de ADS Waypoint aparecem no **topo** da seção de cada postura, no mesmo
lugar nas três. Sem mudança de comportamento.

---

## v2.8.0 (2026-07-19)

### Novidade — compressão da velocidade de ADS (uniformizar leves × pesadas)

Armas muito leves miram rápido demais, pesadas devagar demais. Duas opções novas (seção `Stance Transition &
Kick`) puxam os dois extremos **em direção a um ponto central**, deixando a velocidade de mira mais uniforme entre
armas:

| Opção | O quê |
|---|---|
| `ADS Speed Compression (%)` | 0 = velocidade nativa do jogo (sem efeito). 100 = todas as armas na mesma velocidade (a do pivô). Valores no meio puxam leves e pesadas para o centro. |
| `ADS Speed Pivot` | A velocidade que fica **inalterada** (o centro da compressão). Maior = mais rápido. Calibre pela sensação. |

Funciona em **escala logarítmica** (natural para velocidade): com 50% e o pivô no centro, uma arma "2× mais
rápida que o pivô" vira "1,4× mais rápida", e uma "2× mais lenta" vira "1,4× mais lenta". Mexer nos sliders
reflete **na hora** (sem precisar re-sacar a arma). Só a velocidade de subida da mira — não afeta recuo, dano nem
nada de coop.

---

## v2.7.1 (2026-07-19)

### Ajustes

- **`ADS Waypoint` agora é por postura.** As opções `ADS Waypoint` e `ADS Waypoint Time (ms)` saíram da seção
  geral e viraram **uma por postura** (dentro de cada seção `Stance 1/2/3`) — cada postura liga/desliga e calibra
  o próprio tempo. (As posturas se comportam diferente ao mirar; a Low Ready costuma pedir um tempo diferente da
  High Ready.) As duas opções globais antigas foram removidas — as novas nascem ligadas, 120 ms.
- **`Stance N Movement Speed Multiplier` saiu de "Advanced"** — aparece agora sem precisar ligar o modo avançado.

---

## v2.7.0 (2026-07-18)

### Novidade — transição suave de High/Low Ready para a mira (fim do "loop" vertical)

Ao mirar a partir de uma postura (High Ready, Low Ready…), a arma fazia um **loop/salto vertical** antes de
assentar na mira — pior em armas leves. Causa: ao apertar mirar, a arma subia para a ótica **ao mesmo tempo** em
que saía da pose de postura, e os dois movimentos se somavam.

Agora, ao mirar de uma postura, a arma primeiro **assenta na posição neutra** e a mira fica **segurada por um
instante**; passado esse tempo, a mira sobe **limpa**, sem o loop. Ao sair da mira, você volta para a postura em
que estava — automático.

Duas opções novas no F12 (seção `Stance Transition & Kick`):

| Opção | O quê |
|---|---|
| `ADS Waypoint Via Stance 0` | Liga/desliga o recurso (padrão: ligado). Requer `Reset Positions When Aiming` ligado. |
| `ADS Waypoint Time (ms)` | **Calibrável** — quanto tempo (ms) a mira fica segurada antes de subir (padrão 120). Curto demais ainda dá loop; longo demais fica lento. |

Não muda nada da postura em si nem afeta atirar/stamina/mount/coop — só o momento da subida da mira.

---

## v2.6.0 (2026-07-17)

### Novidade (ferramenta de debug) — `Debug Transition Metrics`

Nova opção no F12 (seção `Debug (Advanced)`, desligada por padrão). Quando ligada, registra **uma linha no log por
transição de pose concluída** — rota, pico de movimento além do alvo (por eixo), oscilações e tempo até assentar.
É a régua para medir e atacar, com número, os problemas de transição para a mira (a arma "subir demais" antes de
assentar). **Custo praticamente zero quando desligada** — não muda nada no comportamento do mod.

Primeiro passo (F0) do item 017 (ataque cirúrgico ao overshoot de transição). Nenhuma mudança de jogabilidade
nesta versão.

---

## v2.5.0 (2026-07-14)

### Removido — a seção `Field of View` inteira (3 opções)

Apesar do nome, ela **não** mexia no campo de visão do jogo: era um FOV de **viewmodel** — mudava a perspectiva
**só dos braços e da arma**, deixando o mundo intacto. O resultado prático é ver os braços fora de escala,
esticados na tela.

Pior, ela deixava o jogo num estado do qual **não dava para sair pelo menu**: o valor ficava gravado nas
configurações do jogo, e **desligar a opção não desfazia**. Um travamento no meio do ajuste bastava para o
personagem ficar com os braços deformados permanentemente.

Sem valor real e com uma armadilha dessas, a funcionalidade foi **removida inteira** — as 3 opções e os dois
patches por trás delas. O jogo volta a limitar o FOV na faixa nativa (50–75).

> **Se o seu personagem ficou com os braços deformados:** atualize para esta versão e abra o jogo. Sem os
> patches, o limite nativo volta a valer e a visão normaliza sozinha. Se ainda estranhar, confira o FOV em
> *Configurações → Jogo*.

O menu F12 foi de 114 para **111 opções**, e de 20 para **19 seções**.

---

## v2.4.0 (2026-07-13)

### Novidade — a velocidade da mira agora é separada da velocidade das posturas

Até aqui, o `Stance Transition Speed` controlava **duas coisas ao mesmo tempo**: a rapidez com que você troca de
postura **e** a rapidez com que a arma sobe e desce ao mirar. Não dava para deixar a troca de postura lenta e a
mira ágil (ou o contrário).

Agora são duas opções independentes, na mesma seção `Stance Transition & Kick`:

| Opção | Controla |
|---|---|
| `Stance Transition Speed` | Trocar **entre posturas** (e voltar para a visão padrão) |
| `ADS Transition Speed` *(nova)* | **Levantar e baixar a mira** — vale tanto para entrar quanto para sair |

> ⚠️ **Se você tinha ajustado o `Stance Transition Speed`**, ele deixou de valer para a mira. A opção nova nasce
> em `1.0` (velocidade padrão). Para manter exatamente o comportamento que você tinha, coloque o
> `ADS Transition Speed` no **mesmo valor** que estiver no `Stance Transition Speed`.

Vale também para os outros jogadores no coop: cada um que você vê em partida usa as duas velocidades de forma
independente, como deve ser.

---

## v2.3.0 (2026-07-13)

### Corrigido — uma opção mal escrita derrubava o mod inteiro dentro da raid

Na v2.2.0 uma das opções foi renomeada para um nome que continha o caractere **`=`** — que é justamente o
separador usado no arquivo de configuração. O BepInEx recusa esse nome e **interrompe a inicialização do mod na
metade**. Como os patches do jogo já haviam sido ligados **antes** disso, eles continuavam rodando enquanto todas
as opções seguintes ficavam vazias — o resultado era uma **enxurrada de erros a cada quadro dentro da raid**
(corrigido na v2.2.1).

Esta versão ataca a **causa**, não o sintoma: a inicialização foi reordenada para **ler todas as opções primeiro e
só então ligar os patches**. Agora, se alguma opção estiver mal definida, o mod se **desliga sozinho** e registra
**um** erro claro no log — o jogo roda normalmente, sem o mod, em vez de ser inundado de mensagens.

### Interno

- Os dois patches que ainda eram ligados sem proteção (incluindo o principal, que aplica a rotação das posturas)
  passaram a ter o mesmo isolamento dos outros: se um alvo sumir num update do jogo, ele falha sozinho e avisa no
  log, sem derrubar o resto.
- O patch de FOV e a lista de multiplicadores de stamina ganharam proteção contra as mesmas classes de falha.
- O amortecimento (`Stance Overshoot Damping`) passou a valer também no caminho de rotação alternativo, onde antes
  era um valor fixo no código.

---

## v2.2.1 (2026-07-12)

Correção emergencial: veja acima. A opção `Stance Overshoot Damping` teve o nome corrigido para
`(Lower Means More Bounce)` — sem o `=` que quebrava a inicialização.

---

## v2.2.0 (2026-07-12)

### Corrigido — os eixos **Yaw** e **Roll** estavam trocados (todas as posturas e o ADS)

O que o menu chamava de **Yaw** (apontar para os lados) na verdade **tombava** a arma, e o que chamava de
**Roll** (tombar) na verdade **apontava** para os lados. Valia para as Stances 1, 2, 3 e para o ADS.

A causa: a rotação é aplicada nos **eixos locais da arma**, e não nos eixos do mundo. Nesse espaço, o eixo Y é
o **eixo do cano** — girar em torno dele **tomba** a arma (isso é *roll*), enquanto girar em torno do eixo
vertical é que **aponta** (isso é *yaw*). O código montava a rotação na ordem convencional do Unity
(pitch, yaw, roll), o que colocava cada um no eixo do outro. Corrigido na origem: agora o valor de *roll* vai
para o eixo que tomba e o de *yaw* para o eixo que aponta.

> ### ⚠️ Sobre a sua calibração das posturas
>
> **O mod NÃO migra o seu `.cfg` sozinho** — esta versão renomeia as chaves, então o BepInEx recria as opções
> com os **valores padrão** e a sua calibração de rotação/posição é **perdida**.
>
> **No servidor Tarkov Red Line isso não te afeta:** o arquivo de configuração já vem calibrado e é distribuído
> pelo launcher — você não precisa fazer nada.
>
> **Se você configurou as posturas por conta própria**, anote os valores antes de atualizar e reponha-os depois,
> **trocando Yaw por Roll**: o número que estava em `Yaw` agora vai no campo **`Roll`**, e o que estava em `Roll`
> vai no **`Yaw`**. (Era justamente essa a inversão — os dois campos faziam a coisa um do outro.) Pitch e as
> posições não mudam de lugar.

### Alterado — nomes das opções agora são só em inglês

As dicas entre parênteses existiam para tornar o eixo óbvio (os nomes técnicos `Pitch`/`Yaw`/`Roll` não são),
mas estavam em português. Foram **traduzidas**, não removidas:

| Antes | Agora |
|---|---|
| `Pitch (Cano Sobe/Desce)` | `Pitch (Muzzle Up/Down)` |
| `Yaw (Apontar Esq/Dir)` | `Yaw (Point Left/Right)` |
| `Roll (Tombar Arma)` | `Roll (Cant Weapon)` |
| `Up/Down (Coronha Sobe/Desce)` | `Up/Down (Stock Up/Down)` |
| `Sideways (Coronha Esq/Dir)` | `Sideways (Stock Left/Right)` |
| `Forward/Backward (Frente/Trás)` | `Forward/Backward` |
| `Stance Kick Intensity (Contra o Peito)` | `Stance Kick Intensity (Toward the Chest)` |
| `Stance Overshoot Damping (Menos gera Mais Quicada)` | `Stance Overshoot Damping (Lower Means More Bounce)` |

As **descrições (tooltips) seguem bilíngues** — inglês em cima, português abaixo.

---

## v2.1.0 (2026-07-12)

Faxina de opções que **não faziam nada** e o resgate de uma feature que havia quebrado sem ninguém notar.
O menu F12 foi de 120 para **113 opções** e de 21 para **20 seções**.

### Corrigido

- **O FOV expandido voltou a funcionar.** A opção `Enable Expanded FOV Range` estava inerte: o patch que
  remove o limite interno de 50–75 do jogo existia, mas **deixou de ser ativado** em algum momento — o
  código foi removido por acidente, junto com uma mudança que não tinha relação. O que sobrava só alargava
  o slider da tela, enquanto o jogo continuava limitando o valor de volta. O patch foi reativado.
- **As opções que definem quais posturas a tecla de troca percorre voltaram a aparecer no F12.**
  `Include Stance 0 in Cycle` e `Enable Stance 1/2/3 in Cycle` ficavam **escondidas** a menos que a roda do
  mouse estivesse em modo `Cycle` — mas elas sempre governaram também o ciclo da tecla (`V` por padrão).
  Quem usa a tecla não tinha como editá-las pela interface, apesar de elas estarem ativas.

### Removido (7 opções que não faziam efeito nenhum)

- **Seção `Default Hands/Arms Positions` inteira** (4 opções). Elas prometiam ajustar a posição das
  mãos/arma **fora** de postura, mas o código que as lia só rodava **dentro** de uma postura — nunca
  surtiam efeito. Para ajuste fora de postura, use a seção **`Camera Position`**.
- **`Stance 1/2/3 Apply When Prone`** (3 opções). Ao deitar, o mod já volta para a Stance 0 **antes** de
  consultar essa configuração — então só a da Stance 0 era lida. As três eram decorativas.
  **`Stance 0 Apply When Prone` continua e é a que realmente funciona:** é ela que decide se o limite de
  velocidade continua valendo quando você está deitado.

> As opções removidas ficam órfãs no seu `.cfg` e são simplesmente ignoradas — nada a fazer.

### Nota sobre a Stance 0

A seção `Stance 0 - Vanilla` **não é decorativa**, ao contrário do que o código sugeria: com os valores
padrão, ela aplica um **limite de 90% na velocidade de movimento sempre que você não está em nenhuma
postura** — ou seja, na maior parte da partida — e isso **se combina** com o `Walk Speed Multiplier`
(padrão 0,85). Se o personagem parece mais lento do que deveria, é aí que se mexe.

---

## v2.0.0 (2026-07-11)

> ### ⚠️ Leia antes de atualizar: suas configurações serão perdidas
>
> Esta versão renomeia as seções e as chaves do menu F12. O BepInEx casa cada opção salva pelo par
> (seção, chave) **literal**, então as opções antigas não são reconhecidas e **todas as configurações
> voltam ao padrão**. Não há migração automática do `.cfg`.
>
> **O que fazer:** nada, se você nunca mexeu no F12 — os valores padrão reproduzem exatamente o
> comportamento testado. Se você tinha uma calibração própria, anote-a antes de atualizar (ou guarde
> uma cópia do `BepInEx/config/com.shwng.fpscamerastances.cfg`) e refaça no F12 depois. Vale a pena
> reconfigurar do zero: **8 opções tinham os eixos Roll e Yaw trocados no rótulo**, ou seja, quem
> calibrou pelo nome estava mexendo no eixo errado.

### Novidades

- **Bloqueio do apoio de arma nas posturas** — apoiar a arma em superfícies (mount) agora só é
  possível na Stance 0 (vanilla), com a mira em ADS ou deitado. Nas Stances 1/2/3 o apoio é
  recusado, em vez de deixar a arma numa pose inconsistente. O bipé **não** é afetado.

### Correções

- **Sync das posturas no Fika: o braço agora acompanha a arma.** Para os outros jogadores, a postura
  era aplicada tarde demais no pipeline de animação (depois do IK), então só a arma se movia e o
  braço ficava parado. O offset passou a ser aplicado na janela pré-IK, e braço e arma se movem
  juntos.
  *Em partidas coop, recomenda-se que todos atualizem: o pacote de rede não mudou (jogadores em
  versões diferentes continuam se conectando normalmente), mas quem estiver na versão antiga vai
  continuar vendo os companheiros com a arma solta do braço.*
- **Eixos Roll e Yaw destrocados** em 8 opções de rotação (posturas e ADS): o rótulo dizia um eixo e
  o código aplicava o outro.
- **Rótulos legados das Stances 2 e 3** corrigidos no ciclo de posturas — os nomes ainda refletiam a
  ordem antiga, anterior à troca entre "Low Ready" e "Custom".

### Menu F12 reorganizado

- **23 propriedades mortas removidas** — não faziam nada: apareciam no menu, mas nenhuma delas era
  lida pelo código. O menu foi de 143 para **120 opções**, e de 23 para **21 seções**.
- **Seções renomeadas** para nomes descritivos em inglês (sem os prefixos numéricos antigos).
- **Todas as descrições agora são bilíngues** — inglês na primeira linha, português abaixo.

### Interno

- Os patches do Manual Chambering ganharam proteção contra exceções (uma falha ali não derruba mais o
  resto do mod).
- Logs de diagnóstico temporários removidos.
- Versão do assembly (`.csproj`) passa a acompanhar a versão do plugin — antes a DLL era compilada
  como `1.0.0.0` independentemente da versão anunciada no BepInEx.

---

## v1.3.1 e anteriores

Versões de desenvolvimento do fork, não distribuídas com changelog próprio. Acumulam, sobre o mod
original: stamina e velocidade por postura, ciclo linear de posturas e teclas dedicadas, snap para a
Stance 0 ao atirar, velocidade de agachar/inclinar, inércia e velocidade máxima, troca automática de
postura ao recarregar/checar arma, animação orgânica de transição (Wiggle), Manual Chambering, apoio
passivo de arma sobre o mount nativo, controlador central de stamina de braço e o sync visual das
posturas no Fika.
