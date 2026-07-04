# 058 — Ativar masteries inertes (SMG/LMG/HMG/Launcher/AttachedLauncher)

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-07-03

## Visão geral

Tornar **funcionais** as 5 maestrias de arma que aparecem na tela de Skills mas são **inertes** (nunca sobem, não
buffam): **SMG, LMG, HMG, Launcher, AttachedLauncher**. Duas pernas: (1) **ganho de XP** ao acertar tiros com a arma da
categoria; (2) **efeito por nível** (menos recuo / mais ergo), como as maestrias funcionais. O [recon](058-ativar-masteries-inertes-recon.md)
provou que **repopular o `globals` não pega** (o engine não tem binding para elas) → a única via é **patch client**.

> **⚠️ Este item tem um gate INVERTIDO:** por causa das incógnitas da §"Validação prévia", parte do comportamento
> desejado **só pode ser especificada com precisão depois de validar in-game** (quais skills já sobem, se persistem).
> A implementação (`/code-mod`) **fica bloqueada** até essa validação — ver a seção própria abaixo.

## Comportamento atual

- As 5 skills aparecem na tela de Skills, mas: **não ganham XP** (o roteador arma→skill `SkillManager.method_1` nem
  registra HMG/Launcher/AttachedLauncher; SMG/LMG têm `case` mas config `[]`) e **não dão efeito** (o buff é chaveado por
  tipo de arma que essas skills não possuem). Fonte: recon §2–§3.
- O Tanque já tem o perk **Bunker** (flat: recuo ×0.85 / ergo ×1.15 com arma pesada) — mecânica separada, item 050.

## Comportamento desejado

Ao **acertar** tiros com a arma da categoria, a maestria correspondente **sobe** (barra na UI de Skills); com o nível,
a arma daquela categoria ganha **menos recuo / mais ergonomia**, proporcional ao nível — igual às maestrias funcionais.
O progresso **persiste** entre raids. **Coexiste** com o Bunker (decisão de escopo abaixo).

## Critérios de aceite

- [ ] **XP:** acertar tiro com arma de categoria SMG/LMG/HMG/Launcher/underbarrel → a skill correspondente ganha XP
      (barra sobe na tela de Skills).
- [ ] **Efeito:** com a skill em nível > 0, a arma da categoria tem **recuo reduzido** e **ergo aumentada**,
      proporcional ao nível (mesma ordem de grandeza das maestrias funcionais).
- [ ] **Persistência:** o progresso da skill **persiste** ao sair e reentrar na raid (é skill de perfil).
- [ ] **Coexistência com Bunker (decisão do escopo):** a skill sobe para **qualquer** classe; o Tanque **mantém** o
      Bunker flat **por cima** (aditivo). *(Assunção 2026-07-03 — ver "Fora de escopo / decisões".)*
- [ ] **Sem XP duplo:** se uma categoria já ganha XP no vanilla (SMG/LMG — a validar), o patch **não** credita XP de
      novo para ela (só as comprovadamente mortas recebem o XP do mod).
- [ ] **Fika/multiplayer:** cada player ganha XP e efeito da **própria** arma (locais); sem sync. `N/A` para host-side.
- [ ] **Estado entre raids:** o progresso é persistido pelo perfil (mesmo requisito da persistência acima).

## Validação prévia (GATE PRÉ-CÓDIGO — bloqueia o `/code-mod`)

O recon deixou 4 incógnitas que **só o jogo resolve** e que **mudam o design**. Rodar este protocolo in-game **antes**
de implementar (resultados alimentam a spec técnica):

- [x] **V1 — RODADO 2026-07-04** (perfil zerado, protocolo estendido da review 01): **SMG 2.46** e **LMG 2.76**
      SOBEM VANILLA → fora da Perna 1. **Launcher standalone (MSGL/FN40GL) 0.91** SOBE VANILLA → fora da Perna 1
      (a review PA-01-01 acertou: Launcher não estava morta). **Underbarrel (GP-25+VOG-25) = 0** → ÚNICA skill
      comprovadamente morta e alvo real da Perna 1. Consequência anti-XP-duplo: o mod NÃO credita XP em
      SMG/LMG/Launcher.
- [x] **V2 — CONFIRMADO 2026-07-04:** valores persistiram pós-extract (SMG 2.46 / LMG 2.76 / GL 0.91). Morte não
      testada (baixo risco — sistema nativo). A persistência do XP creditado PELO MOD (AttachedLauncher via
      SetCurrent) só é testável pós-implementação → item do checklist de validação do code-mod.
- [x] **V1d-extra (2026-07-04):** usuário NÃO acertou granada em cheio (só explosões próximas) e nada creditou →
      sela o design POR DISPARO na Perna 1 (detectar o tiro do underbarrel, não o acerto).
- [ ] **V3 — `SetCurrent` funciona?** (validado no code-mod) Confirmar que creditar XP numa skill inerte reflete na UI.
- [~] **V4 — PARCIAL 2026-07-04:** o disparo do underbarrel NÃO credita nada (nem Launcher) → o Item reportado no
      acerto da explosão da VOG não casa com nenhum weaponType (provável: a granada/munição). Detectar o DISPARO do
      underbarrel (FirearmController.UnderbarrelWeapon) é o caminho da Perna 1. **HMG segue indeterminada** (NSV é
      estacionária de mapa; não testada) → HMG entra como best-effort/deferível na spec técnica.

## Corner cases

- [ ] **XP duplo** (V1): categoria que já sobe no vanilla não deve receber XP do mod.
- [ ] **HMG vs LMG** (V4): ambas `weapClass=machinegun` — se indistinguíveis, unificar num buff só ou achar discriminante.
- [ ] **Underbarrel acoplado** (V4): detectar o modo de disparo do GP-25/M203 é a maior incógnita de gating.
- [ ] **Arma trocada rápido / disparo sem acerto:** o XP é por **acerto** (não por tiro no ar) — segue a semântica vanilla.
- [ ] **Coop:** XP/efeito locais; nenhum caminho depende do host.

## Fora de escopo / decisões

- [x] **Tocar `globals`/server** para XP ou efeito: **não pega** (recon) — descartado.
- [x] **Mastering** (`MasterSkillClass`, sistema separado que já funciona): fora — este item é sobre as **Weapon Skills**.
- [x] **Relação com o Bunker** — **assunção: (a) coexistir** (aditivo). Alternativas registradas para revisão do usuário:
      (b) substituir o Bunker pela skill; (c) Bunker vira o bônus de elite (nível 51) da skill. Decidir antes do `/code-mod`.

## Referências

- [058-ativar-masteries-inertes-recon.md](058-ativar-masteries-inertes-recon.md) — por que globals não pega + o caminho client.
- [058-ativar-masteries-inertes-00-kickoff.md](058-ativar-masteries-inertes-00-kickoff.md) — problema original.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-03 | Item criado via `/create-spec` (escopo 5 skills; coexistir Bunker; **gate de validação prévia** antes do code-mod) |
