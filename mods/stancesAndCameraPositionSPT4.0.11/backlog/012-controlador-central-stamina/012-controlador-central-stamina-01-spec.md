# 012 — Controlador central de stamina de braço

**Mod:** stancesAndCameraPositionSPT4.0.11
**Status:** Backlog
**Criado:** 2026-06-21

## Visão geral

A stamina de **braço** (a que cansa ao mirar/segurar a respiração, separada da stamina de perna usada no sprint) hoje é disputada por várias regras do mod somadas ao comportamento nativo do jogo, gerando conflito. Este item cria um **controlador central** que é a **única autoridade** sobre a stamina de braço do jogador local: a cada quadro ele identifica **um único cenário** em que o personagem está e aplica **somente** a regra daquele cenário, ignorando o comportamento nativo. Cada cenário tem um **multiplicador configurável** no F12, reunidos num grupo único chamado **`Stamina Management`**. Um modo de **debug** mostra na tela e no log qual cenário está ativo.

## Comportamento atual

- O `06-fix-01` introduziu um coordenador que decide um "modo" de stamina por quadro, mas **cada regra o consulta de forma independente** e o comportamento **nativo continua rodando em paralelo**. Resultado observado in-game: a stamina **oscila** na Stance 0 (sobe e desce), mantém **comportamento residual** ao sair de mount passivo/ativo (como se a regra anterior continuasse), e o **ADS "cai no nativo"** em alguns cenários em vez de seguir a regra do mod.
- Os multiplicadores de stamina existentes ficam **espalhados** (um dentro de cada grupo de Stance); ADS, hold-breath e prone **não têm** multiplicador próprio.
- Não há forma de **ver, durante o jogo**, qual regra está controlando a stamina — o diagnóstico depende de inferir pelo comportamento.

## Comportamento desejado

- **Uma única regra ativa por quadro.** O controlador avalia o estado real e aplica exatamente um cenário; ao mudar de estado, o comportamento anterior é **substituído por completo** (nunca dois ao mesmo tempo, nunca resíduo).
- **Cobertura total dos cenários** (estado principal × modificador), cada um com seu multiplicador (`< 1` drena · `1` mantém/segura · `> 1` recupera):

  | Estado principal | Stance 0 / Hipfire | Stance 1/2/3 | ADS | Hold Breath |
  |---|---|---|---|---|
  | Stand up (sem mount) | por stance | por stance | próprio | próprio |
  | Prone (sem mount) | próprio | — | próprio | próprio |
  | Passive Mount | próprio | (só Stance 0 ou ADS) | próprio | próprio |
  | Active Mount | próprio | (só Stance 0 ou ADS) | próprio | próprio |

  Prioridade entre estados: **Active Mount > Passive Mount > Prone > Stand up**; entre modificadores: **Hold Breath > ADS > Stance/Hipfire**. Em **Prone**, a stance (1/2/3) é ignorada — usa-se o multiplicador de Prone (hipfire), ou o de Prone-ADS / Prone-Hold Breath conforme o modificador. Combinações são resolvidas pela prioridade (ex.: deitado com bipé montado conta como **Active Mount**, não Prone).
- **Stamina de braço controlada 100% pelo mod** nos cenários acima — o comportamento nativo é desprezado para o braço (a stamina de **perna** permanece nativa).
- **Configuração centralizada:** todos os multiplicadores num único grupo F12 `Stamina Management`, posicionado **acima** do grupo de Respiração (Hold Breath); dentro do grupo, ordem lógica (Stance 0→3, depois ADS, Hold Breath, Prone, Mounts). O grupo de Stance 3 deve aparecer **abaixo** do de Stance 2.
- **Debug opcional** (toggle no F12): texto na tela + linha no log a cada **troca** de cenário, no formato `STAMINA STATE: <cenário>` (ex.: `Passive Mount - ADS`).
- **Restrição de mount:** o mount passivo e o ativo só fazem sentido em **Stance 0 ou em ADS** — nas Stances 1/2/3 sem mira o passivo não deve ativar.
- **Toggle do passivo (CR-01-03):** um controle liga/desliga o controle de stamina do apoio passivo (off = o passivo ainda dá recoil/sway, mas não mexe na stamina — ela segue o cenário sem-mount). _(reaproveita `Passive Stamina Save`.)_

## Critérios de aceite

- [ ] Em cada cenário da tabela, a stamina de braço seguir **exclusivamente** o multiplicador configurado para aquele cenário (drena/mantém/recupera), sem o comportamento nativo somar ou contrariar.
- [ ] Em qualquer **transição** (ex.: Active → Passive → sem mount → ADS, ida e volta), o comportamento anterior cessar **imediatamente** e o novo assumir — sem oscilação na Stance 0 e sem precisar mexer no F12 para "destravar".
- [ ] Todos os multiplicadores residirem num **único grupo F12 `Stamina Management`**, posicionado **acima** do grupo "Respiração (Hold Breath)"; o grupo Stance 3 aparecer **abaixo** do Stance 2.
- [ ] Com o **debug** ligado, a tela e o log mostrarem o cenário ativo e ele **trocar na hora** a cada mudança de estado — confirmando que nunca há dois cenários simultâneos.
- [ ] O mount **passivo** não ativar em Stance 1/2/3 sem ADS (só Stance 0 ou ADS).
- [ ] A stamina de **perna** (sprint/corrida) permanecer **intacta** — o controle afeta apenas o braço.
- [ ] **Fika/multiplayer:** o controle e o debug aplicarem-se **somente ao jogador local** — nunca alterar a stamina de bots ou de outros players.
- [ ] **Estado entre raids:** raid1 → sair → raid2, e morte/MIA/alt-F4, sem estado preso nem exceção; o controlador reconfigurar-se corretamente e ceder ao nativo fora de raid (menu/hideout).

## Corner cases

- [ ] **Segurar a respiração fora de ADS** (se o jogo permitir): cair no multiplicador de Hold Breath do estado atual, sem ambiguidade com o de ADS.
- [ ] **Stamina de braço esgotada (chega a 0)** durante um cenário controlado: a stamina **não ficar presa em 0** — volta a recuperar assim que o cenário permitir (multiplicador `> 1`); o feedback de exaustão nativo (tremor de mira) deve continuar funcionando ou degradar sem efeito colateral.
- [ ] **Mãos vazias / item que não é arma de fogo** em mãos: o controlador **cede ao nativo** (sem controle do mod) — a stamina de braço só é gerida com arma de fogo equipada. _(assunção, registrar na entrega)_
- [ ] **Mudar um multiplicador no F12 durante o jogo:** o novo valor passar a valer **na hora**, sem precisar sair e voltar ao raid.
- [ ] **Troca rápida de stance / snap ao atirar:** o cenário acompanhar a stance real do momento, sem ficar uma regra atrasada.
- [ ] **Fora de raid (menu/hideout):** o controlador não interferir — a stamina segue o nativo.

## Fora de escopo

- [ ] **Bloquear o mount ATIVO (nativo) em Stance 1/2/3** — fica para um item futuro (exige interceptar a ação de montar do jogo). Aqui só o **passivo** é restringido.
- [ ] Stamina de **perna** e **oxigênio** — permanecem como estão.
- [ ] Recoil/sway do mount passivo — já entregues no item 011, não mudam aqui.

## Referências

- [011 — Mount passivo sobre o vanilla](../011-mount-passivo-vanilla/011-mount-passivo-vanilla-01-spec.md) (estados passivo/ativo que alimentam os cenários)
- [011 · 06-fix-01](../011-mount-passivo-vanilla/011-mount-passivo-vanilla-06-fix-01.md) (coordenador que este item evolui)

## Histórico

| Data | Evento |
|---|---|
| 2026-06-21 | Item criado via `/add-backlog-item` |
| 2026-06-21 | Revisão `/review-spec` — 3 gaps corrigidos (prone ignora stance; mãos vazias → cede ao nativo; "esgotada" tornado verificável) |
