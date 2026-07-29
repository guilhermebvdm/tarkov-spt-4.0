# Visceral Combat — Memória de Sessões

## Snapshot Delta
- **Versão:** 3.7.0 (SPT 4.0 / FIKA 2.2.6)
- **Estado:** Código C# extraído das DLLs originais e estruturado no repositório.
- **Pendências:** 🟢 Nenhuma pendência blocker registrada.

## Sessão 2026-07-28 — Code Review e Roadmap de Refatoração
- **Análise:** Realizado code-review minucioso identificando gargalos de FPS, vazamentos de RAM, corrotinas descontroladas, thread-safety bugs (`async void`) e 15+ propriedades fantasma no F12.
- **Entregável:** Criado o roadmap detalhado de refatoração em `docs/refactor-roadmap.md`.
- **Regra:** Todas as correções serão realizadas em `modded/` sem alterar a pasta `original/`.
