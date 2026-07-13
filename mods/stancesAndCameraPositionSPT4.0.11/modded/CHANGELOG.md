# Changelog

Changelog do fork (Tarkov Red Line). O histórico do mod original vai até a v1.1.4 e está em
[CHANGELOG_SIMPLIFIED.md](./CHANGELOG_SIMPLIFIED.md).

Versões mais recentes primeiro.

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

> **Sua calibração foi preservada.** Se você já tinha ajustado as posturas, os valores foram **migrados
> automaticamente** (o que estava em `Yaw` foi para `Roll` e vice-versa), de modo que **as poses continuam
> exatamente como estavam** — a diferença é que agora cada opção faz o que o nome promete. Um backup do
> arquivo antigo ficou como `com.shwng.fpscamerastances.cfg.bak-pre-v220`.

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
