# 011 — Mount passivo sobre o mount vanilla (ativo)

**Mod:** stancesAndCameraPositionSPT4.0.11
**Status:** Backlog
**Criado:** 2026-06-21

## Visão geral

Reconstruir o sistema de apoio de arma (mount) sobre o **mount nativo do EFT**, substituindo o mount próprio do item 004 (descartado por suprimir o vanilla e nunca funcionar em 0.16). O **modo ativo** passa a ser 100% o vanilla: o jogador apoia a arma com a tecla de mount em superfícies como pedra, árvore, parede e peitoril. Sobre isso, adiciona-se um **modo passivo**: ao encostar a arma numa superfície apoiável **sem usar a tecla**, o jogador recebe um benefício leve de estabilidade (menos recuo, menos sway de respiração e economia de stamina de braço), **mais fraco que o ativo**, sinalizado por um ícone direcional no canto inferior direito.

## Comportamento atual

O mount próprio (item 004) foi removido nesta linha (`modded-beta`). Hoje o mount é **100% vanilla** — o mod não interfere no apoio de arma: a tecla nativa monta/desmonta normalmente, com os bônus nativos do EFT (ergonomia, redução de aim-drain). **Não existe** modo passivo: encostar a arma sem montar não dá nenhum benefício nem feedback visual.

## Comportamento desejado

- **Ativo:** permanece 100% vanilla — o mod não altera nem bloqueia o mount nativo.
- **Passivo:** quando a arma encosta numa superfície apoiável e o jogador **não** está montado no vanilla:
  - aplica benefício **leve** de estabilidade: redução de recuo, redução de **sway** de respiração (referência inicial: ~35%) e redução/pausa do drain de stamina de braço;
  - exibe um **ícone direcional** (esquerda / direita / baixo, conforme o lado do apoio) no canto inferior direito;
  - o benefício é **perceptivelmente menor** que o do mount ativo (vanilla montado); <!-- review: magnitudes exatas de recuo/stamina a definir na spec técnica (sway: referência ~35%) -->

  - ao montar de fato no vanilla, o passivo **cede** — sem somar benefícios;
  - controlável por **toggle no F12** (desligado = apenas vanilla, sem passivo).

## Critérios de aceite

- [ ] Com o mod ativo, o **mount vanilla funciona normalmente** — apoiar com a tecla em pedra/árvore/parede/peitoril monta a arma, com os bônus nativos do EFT inalterados (sem regressão).
- [ ] O mod **não bloqueia nem intercepta** o acionamento do mount nativo (lição do item 004, que suprimia o comando de mount e impedia qualquer mount — vanilla e próprio).
- [ ] Encostar a arma numa superfície apoiável **sem apertar a tecla** ativa o passivo: ao disparar/mirar encostado, o **recuo é menor**, o **sway (balanço da mira) é menor** (~35%+) e o **drain de stamina de braço é menor/pausado** comparado a disparar/mirar livre.
- [ ] O benefício do passivo é **menor que o do ativo**: medível disparando montado (vanilla) vs. apenas encostado.
- [ ] Um **ícone direcional** (left/right/down conforme o lado do apoio) aparece no **canto inferior direito** enquanto o passivo está ativo e **some** ao afastar a arma da superfície.
- [ ] Ao **montar no vanilla** enquanto o passivo estava ativo, o passivo **não soma** benefícios (cede ao ativo).
- [ ] Um **toggle no F12** liga/desliga o passivo; desligado, o comportamento é idêntico ao vanilla puro.
- [ ] **Fika/multiplayer:** o passivo (benefício + ícone) é **local ao próprio jogador** e aplica-se **somente ao seu jogador** — nunca a bots nem a outros players (os efeitos de recuo/sway/stamina precisam checar o jogador local antes de agir). O mount **ativo** continua sincronizado pelo próprio vanilla/Fika.
- [ ] **Estado entre raids:** o passivo **não vaza** entre raids — ao extrair/morrer/MIA ou voltar ao menu com a arma encostada, o ícone **não fica órfão** e o estado zera; na raid seguinte recomeça limpo.

## Corner cases

- [ ] **Encostar/afastar rápido:** alternar a arma para perto e longe da superfície repetidamente não deve causar flicker do ícone nem estado preso (benefício "grudado" sem superfície).
- [ ] **Transição passivo → ativo:** montar no vanilla com o passivo já ativo deve ser uma transição limpa (sem dupla aplicação de buff; ícone reflete o novo estado ou some).
- [ ] **Saída de raid com passivo ativo:** extrair/morrer/alt-F4 enquanto encostado — ícone some e estado reseta (sem heartbeat-órfão equivalente).
- [ ] **Troca de arma encostado:** trocar de arma enquanto encostado deve re-avaliar com a nova arma (ou cessar o passivo se o item em mãos não for arma de fogo).
- [ ] **Sprint / ADS encostado:** ao correr, o passivo cede (sem benefício durante sprint). Em **ADS** (mirando) encostado, o passivo **mantém** o benefício.
- [ ] **Divergência de detecção:** o passivo usa detecção própria (o EFT não expõe "superfície montável disponível" antes de montar) e **aproxima** as superfícies do vanilla — pode haver casos onde o passivo ativa numa superfície que o vanilla não aceita como montável, ou vice-versa. Aceitável; calibrar por tuning.
- [ ] **Item não-montável em mãos:** com pistola/arma não-apoiável, mãos vazias ou item que não é arma de fogo, o passivo **não ativa** (sem ícone, sem buff).
- [ ] **Bipé deployado:** o passivo **cede** ao mount/bipé nativo (não soma benefício).
- [ ] **Deitado (prone):** o passivo **não aplica nada** — mantém o comportamento vanilla (o EFT já concede o bônus de apoio em prone). O passivo cede em prone.
- [ ] **Plugin recarregado / entrar direto no shooting range:** o componente de detecção/UI deve existir e funcionar mesmo sem passar pelo menu (lição do mount antigo: MonoBehaviour criado no boot não rodava).

## Fora de escopo

- [ ] Reimplementar o **mount ativo** — permanece 100% vanilla.
- [ ] **Bipé** — tratado pelo mount nativo do EFT.
- [ ] **Sincronizar o efeito passivo para peers** (Fika) — fora de escopo; o passivo é feedback/benefício **local** (confirmado).

## Referências

- `mods/stancesAndCameraPositionSPT4.0.11/modded-beta/mount-diagnosis-plan.md` — diagnóstico comparativo (vanilla / RealismMod / impl. antiga) e descobertas técnicas.
- Item **004** (`004-apoiar-arma-superficie/`) — predecessor descartado (mount próprio que suprimia o vanilla).
- Plano de reconstrução: `~/.claude/plans/magical-imagining-meadow.md`.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-21 | Item criado via `/add-backlog-item` |
| 2026-06-21 | Spec funcional criada via `/create-spec` |
| 2026-06-21 | Decisões de escopo incorporadas (sway ~35%, ADS mantém o benefício, Fika local) |
| 2026-06-21 | Revisão `/review-spec` — +2 critérios (anti-supressão do vanilla; AP-02 só-local), +4 corner cases, 1 ponto a decidir (prone) |
| 2026-06-21 | Decisão: prone cede ao vanilla (passivo não aplica nada em prone) |
