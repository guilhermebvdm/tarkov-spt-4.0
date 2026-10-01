# TRL-ImmersiveOverlays — Memória de Sessões

## Snapshot Delta
- **Versão:** 1.0.0 (SPT 4.0)
- **Estado:** Controle de overlays de óculos e viseiras em funcionamento.
- **Pendências:** 🟢 Nenhuma pendência blocker registrada.

## Sessão 2026-07-28 — Inicialização de Governança
- **Ação:** Criação dos arquivos `mod.json`, `README.md`, `PROPRIEDADES.md` e `memory/sessions.md`.

## Sessão 2026-09-19 — Abertura do Item 001 (NullReferenceException na Saída de Raid)
- **Ação:** Identificado spam de `NullReferenceException` em `OverlayController.cs:110` ao extrair/encerrar raid devido a acesso a `PointOfView` com subsistemas de câmera já desmontados.
- **Backlog:** Criado item `001-prevencao-npe-extracao-raid` com spec funcional `001-prevencao-npe-extracao-raid-01-spec.md`.
