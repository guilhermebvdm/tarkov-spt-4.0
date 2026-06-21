# 052 — Validação in-game das 6 classes · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-20 · **Origem:** redesign 11→6, Fase 6 ([class-overview.md](../../docs/class-overview.md) como checklist)
**Wave:** R-W3 · **Deps:** 047–051

> Brief de kickoff — insumo para `/create-spec 052`. Não é a spec.

## Objetivo

Validar o redesign completo **in-game** (não só write+hash — memória `feedback_spt_validation`):

- **Editor web:** 6 classes carregam, custos/matriz corretos, sem diagnostics.
- **Launcher:** criar perfil de cada classe.
- **Raid (FIKA):** skills sobem com os mults certos; signatures 🔧/🧪 observáveis; debuffs mordem em 🐇/🚶.

## Escopo / Riscos

- Build client é revertida pelo sync do launcher (Dev Mod off) → **subir build ao servidor** (memória `feedback_server_launcher_sync_builds`).
- REVIVE **não existe** (decisão #5) — Médico é cura rápida, não reanimação; não testar revive.
- Bug do Círculo de Cultistas (Saqueador/ShadowConnections) — confirmar comportamento real ([class-skill-catalog.md](../../docs/class-skill-catalog.md) §5.1).

## Refs

- [../../docs/class-overview.md](../../docs/class-overview.md) (checklist por classe) · [../../docs/class-levers.md](../../docs/class-levers.md) §5
- Skill `verify`

## DoD (resumo)

- As 6 classes jogáveis com identidade observável; achados registrados na memória do mod (`memory/sessions.md`).
- **F12 validado:** ajustar um parâmetro de cada camada (🔧 e 🧪) e ver o efeito mudar (runtime ou após restart, conforme o caso) — decisão #8.
