# 050 — Patches de signature (🔧) · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-20 · **Origem:** redesign 11→6, Fase 5 ([class-levers.md](../../docs/class-levers.md) §5/§6)
**Wave:** R-W1 · **Deps:** 047 (soft) — independente do 048

> Brief de kickoff — insumo para `/create-spec 050`. Não é a spec.

## Objetivo

Patches Harmony **per-player keyed na classe** (`Info.GameVersion`) para o que skill não cobre:

- **Médico de Combate** — cura de HP `×0.3` tempo, +50% HP, sem lock de movimento/arma; **cirurgia/restauração de membro destruído** (CMS/Surv12) `×0.5` tempo (distinto da cura de HP).
- **Fantasma** — Execução (melee `×20`), Passo Fantasma (ruído de todas as ações até −50%), MaxSpeed `×1.1`.
- **Tanque** — Couraça (dano recebido `×(1−[0.05→0.25])`), velocidade `×0.9`, −comida/bebida `×0.7`, GL mastery **via patch** + GL sem penalidade de ergo (o slot `AttachedLauncher` é inerte).
- **Caçador** — saque de pistola `×0.5`, ADS por arma (sniper/DMR `×0.85`, AR `×1.15`).
- **Fuzileiro** — resist. supressão (aim-punch `×0.5`), antitravamento (malfunction `×0.5`, fix `×2`).
- **Saqueador** — loot silencioso; **🌐 revelar valor ₽** (GLOBAL — todos veem; separar do gating de classe).

## Riscos / atenção

- Velocidade/inércia **compõem** com o stances (multiplicar) ✅. Os levers da **ZONA STANCES** (arm-ADS Caçador, heavy-weapon stamina Tanque) vão para o item **051**, não aqui.
- AttachedLauncher inerte → GL mastery do Tanque é patch puro, não skill 🎯.
- Revelar ₽ é global (não keyed por classe) — não confundir com lever de classe.

## Refs

- [../../docs/class-levers.md](../../docs/class-levers.md) §5/§6 · [../../docs/class-skill-catalog.md](../../docs/class-skill-catalog.md)
- Skills `spt-mod-best-practices`, `csharp-mod-best-practices`, `graph-code-navigation`

## DoD (resumo)

- Cada signature 🔧 observável in-game na classe certa; **sem efeito nas outras** (gating por `GameVersion` validado).
- Toda constante exposta no **F12** (`ConfigEntry`), runtime quando possível, senão nota de restart — decisão #8 ([tabela §6.4](../../docs/class-levers.md)).
