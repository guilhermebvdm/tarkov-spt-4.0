# Handoff — Stances: backlog fechado (tudo validado in-game) + F12 reorganizado

> **Data:** 2026-07-11<br>
> **De:** sessão longa 2026-07-09 → 07-11 (itens 013/014/015 + revisão das propriedades)<br>
> **Para:** próxima sessão do mod `stancesAndCameraPositionSPT4.0.11`<br>

## ⚡ O estado em uma frase

**Não há nada pela metade.** Os 14 itens do backlog estão entregues **e validados in-game** pelo usuário
(2026-07-11) — o mod está funcional de ponta a ponta. Trabalho novo aqui **começa por um item de backlog novo**
(`/add-backlog-item`), não por retomar algo aberto.

## ⚠️ A única coisa que precisa de ação antes de um release

A reorganização das propriedades do F12 (2026-07-11) **renomeou seções e chaves** → o BepInEx casa a config salva
por `(seção, chave)` literal, então **a config do usuário reseta para o default** ao atualizar. Antes de distribuir:

1. **Incrementar a versão** do mod (`.csproj` + metadata, rebuild da DLL) e sufixar o zip com `-v X.Y.Z`.
2. **Avisar no changelog** que as configurações salvas serão perdidas (os defaults reproduzem o comportamento atual).
3. Abrir o F12 uma vez in-game e confirmar que as 21 seções / 120 opções aparecem com os nomes e tooltips certos.

## Estado do repositório

- **Branch: `main`.** Os 14 commits do stances desta sessão estão **todos na `main`**. (Durante o trabalho o checkout
  chegou a estar na `feat/trl-items-management-unify`, mas a sessão paralela mergeou essa branch de volta na `main` e
  trocou o checkout — a branch está **encerrada**: zero commits exclusivos, aparece em `git branch --merged main`.)
- ⚠️ **Checkout compartilhado com sessões paralelas.** **Antes de trabalhar, rodar `git status -sb`**: se a árvore
  estiver ocupada por outra sessão, criar worktree (`git worktree add ../tarkov-spt-4.0-wt-<item> main -b <branch>`)
  em vez de trocar a branch alheia. Commitar cedo — edição não commitada some se outra janela roda `pull`/`checkout`.
- **Tudo commitado, NADA pushado.** A `main` local está **à frente do remote** (inclui commits de sessões paralelas:
  launcher 2.2.1, CustomClasses). `git push` exige aprovação do usuário.
- **DLL instalada** em `D:/SPT/BepInEx/plugins/RealisticMobility/shwngFpsCameraStances4.dll` — hash `c83ed42`,
  contém tudo (014 fix-03 + 015 + revisão do F12). A cópia no repo (`modded/shwngFpsCameraStances4.dll`) é a mesma.
- **Grafo regenerado** e commitado (510 nós / 687 arestas).

## Fonte de verdade para contexto (ler nesta ordem)

1. [`mods/stancesAndCameraPositionSPT4.0.11/memory/sessions.md`](../mods/stancesAndCameraPositionSPT4.0.11/memory/sessions.md)
   — **snapshot no topo** + **Sessão 7** (última): decisões, lições e as 3 pendências restantes.
2. [`mods/stancesAndCameraPositionSPT4.0.11/backlog/mod-backlog.md`](../mods/stancesAndCameraPositionSPT4.0.11/backlog/mod-backlog.md)
   — status por item (todos 🟢; 004 🔴 cancelado, substituído pelo 011).
3. [`mods/stancesAndCameraPositionSPT4.0.11/PROPRIEDADES.md`](../mods/stancesAndCameraPositionSPT4.0.11/PROPRIEDADES.md)
   — as 120 opções do F12, regeneradas do código. O relatório da revisão está em `PROPRIEDADES-review-01.md`.

## Regras deste mod que economizam tempo

- **Editar somente `modded/`.** É o fork canônico desde 2026-07-09 (`modded-beta` foi promovido a `modded`, e o
  antigo virou `modded-bak`, que é backup). `original/` é intocável.
- **Build:** `dotnet build mods/stancesAndCameraPositionSPT4.0.11/modded/CameraRotationMod.csproj -c Release -o <tmp>`
  (o `.csproj` é self-contained — puxa o `Fika.Core` da raiz `references/`). O `/compile-mod` instala numa subpasta
  com o nome do assembly, então **a cópia da DLL para `plugins/RealisticMobility/` é manual**.
- **A DLL fica travada enquanto o EFT roda** — fechar o jogo antes de compilar.
- **Tooltip novo = bilíngue:** inglês na 1ª linha, linha em branco, português na 3ª (`"<EN>\n\n<pt>"`). O command
  `/review-mod-properties <mod>` audita isso (e propriedades mortas, eixos errados, seções mal nomeadas).
- **Renomear `(seção, chave)` de uma `Config.Bind` é breaking change** — sempre.

## Pendências vivas (IDs da memória, Sessão 7)

| ID | O quê | Tipo |
|---|---|---|
| **P-7.1** | Conferir o F12 in-game após a reorganização + **subir a versão** no release (a config salva reseta) | 🟡 débito |
| **P-7.2** | Dívida técnica adiada: unificar as molas (`SpringMath.SpringDamp`), matar a reflection por frame, `try/catch` nos ~19 patches restantes (só os 6 do Manual Chambering têm), auditar o reset de estado estático entre raids. **Mexe em código de câmera já validado — risco > valor sem bug real.** | 🟢 ideia |
| **P-7.3** | Dívida da revisão do F12: reordenar as seções (**inviável com segurança hoje** — os binds de uma mesma seção estão espalhados pelo `Awake`), rever onde ficam as opções de velocidade e se a seção da Stance 0 se justifica | 🟢 ideia |

## Armadilhas já pagas (não repetir)

- **Sync no Fika:** o offset da postura tem que ser aplicado na **janela pré-IK** (Postfix de
  `PlayerBones.ShiftWeaponRoot`, mexendo no `Weapon_Root_Anim`). Aplicar **depois** do IK move **só a arma** e o braço
  fica parado — foi exatamente o bug que custou duas rodadas de fix.
- **Antes de escrever lógica reativa, checar se outro item já fecha aquele estado.** O tick de "desmontar ao trocar de
  postura" (item 015) era **código morto**: o item 013 já força a Stance 0 enquanto a arma está montada. A spec técnica
  não pegou; o code-review pegou.
- **O nome/tooltip de uma propriedade é código.** 8 chaves tinham os eixos **Roll e Yaw trocados** em relação ao que o
  código aplica — quem calibrasse pelo nome mexeria no eixo errado. Revisar rótulo contra o método.
- **Contrato externo:** `CameraRotationMod.StaminaController.ExternalHandsDrainMult` é lido **por reflection** pelo mod
  **CustomClasses** (perks que alteram o dreno de braço). ⚠️ **Não renomear sem coordenar.**

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-11 | Guilherme | Criação — backlog do stances fechado (tudo validado in-game), F12 reorganizado, 3 pendências restantes |
