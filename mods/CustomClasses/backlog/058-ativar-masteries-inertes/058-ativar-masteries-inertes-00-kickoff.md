# 058 — Ativar masteries inertes ("skills fantasmas") · Kickoff

> **Data:** 2026-06-24<br>
> **Status:** ⚪ Backlog (a especificar)<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [class-design.md](../../docs/class-design.md)<br>

---

## Problema

5 maestrias de arma **aparecem na tela de Skills do EFT mas são INERTES** nesta build — `globals.json` `config.SkillsSettings` com array vazio `[]` (sem ação de XP, sem efeito). O jogador vê a skill, ela **nunca sobe e não buffa nada** → "fantasma".

**Confirmado na fonte** (`references/spt-source/.../database/globals.json`):

| Skill | weapClass correspondente | Linha | Estado |
|---|---|---|---|
| **SMG** | `smg` | 35559 | `[]` inerte |
| **LMG** | `machinegun` (LMG) | 35482 | `[]` inerte |
| **HMG** | `machinegun` (HMG) | 35376 | `[]` inerte |
| **Launcher** | `grenadeLauncher` (standalone) | 35483 | `[]` inerte |
| **AttachedLauncher** | underbarrel (GP-25/M203) | 35261 | `[]` inerte |

Funcionais (têm config `{…}`, NÃO mexer): Assault, DMR, Melee, Pistol, Revolver, Shotgun, Sniper.

## Objetivo

Deixar as 5 **funcionais de verdade** (progressão real que sobe na UI + persiste no perfil + buffa), não só o perk flat. Duas pernas:

1. **Ganho de XP** — disparar XP ao usar a arma da categoria (atirar com LMG/HMG/GL/underbarrel/SMG).
   - Investigar: o engine ganha XP a partir de `globals.SkillsSettings` populado (server-side) **OU** está hard-disabled e exige **patch client** concedendo XP no evento de tiro, gateando por `weapClass`?
2. **Efeito por nível** — buff progressivo (ex.: recuo −X%/nível, ergo +X%/nível) lendo o nível da skill.
   - Via globals (se o engine aplicar) **OU** patch client lendo `SkillManager` + multiplicando no ponto de uso (recuo `PWA.Shoot`, ergo `FirearmController.TotalErgonomics` — pontos já mapeados no 050).

## Decisões abertas (pro spec)

- **Relação com o perk Bunker (050):** o Bunker já dá bônus **flat** pra armas pesadas (recuo ×0.85 / ergo ×1.15). Este item é a abordagem por **skill** (progressão). Decidir: **coexistem** (skill p/ todos + Bunker extra pro Tanque) · skill **substitui** o Bunker · Bunker vira o **"elite bonus"** da skill (nível 51).
- **Escopo das 5:** ativar todas ou só as do interesse (o usuário destacou LMG/HMG/GL/underbarrel)? SMG entra junto (mesma categoria inerte).
- **Fórmula/curva:** alinhar com as maestrias funcionais (Assault/etc.) pra consistência (mesma magnitude de buff por nível).

## Restrições / notas

- **Coop-sync:** ganho de XP e efeitos (recuo/ergo) são **LOCAIS** (sua skill, sua arma) → seguros em coop, sem gap host-side. Ref memória `feedback_coop_multiplayer_sync`.
- ⚠️ **Risco-chave (perna 1):** `SkillsSettings []` pode ser **intencional do BSG** (mecânica removida na 0.16.x). Se o engine não consome mais o globals pra essas skills, repopular **não pega** → XP e/ou efeito exigem **patch client**. **A spec-tech DEVE confirmar no assembly** se `globals.SkillsSettings` é lido pra XP/efeito dessas skills ou se está desabilitado.
- Se tocar `globals.json` (server-side): coordenar com a sessão paralela do editor.

## Próximo passo

`/create-spec` deste item (spec funcional) → review → spec-tech (com o recon de "globals é consumido?") → review-tech → code-mod.
