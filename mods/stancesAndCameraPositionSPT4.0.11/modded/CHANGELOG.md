# Changelog

Changelog do fork (Tarkov Red Line). O histórico do mod original vai até a v1.1.4 e está em
[CHANGELOG_SIMPLIFIED.md](./CHANGELOG_SIMPLIFIED.md).

Versões mais recentes primeiro.

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
