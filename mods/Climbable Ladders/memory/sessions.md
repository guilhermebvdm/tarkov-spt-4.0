# Memory — Climbable Ladders

Memória cronológica de sessões de desenvolvimento, auditoria e manutenção técnica do mod **Climbable Ladders** (SPT 4.0 / EFT 0.16.9 / Fika).

---

## Estado Atual (Snapshot)

- **Versão:** 1.0.5 (Client BepInEx `tarkin.ladders.bep.dll`, Fika Coop `tarkin.ladders.fika.dll`, Shared `tarkin.ladders.shared.dll`).
- **Arquitetura:** Interceptação da física do jogador via `Patch_Physical`, controle de estado via `PlayerLadderController`, rig procedural de mãos com FinalIK (`ProceduralGrip`), sincronização em rede Fika (`ObservedPlayerLadderController` / `FikaHandler`) e integração nativa com o sistema de Vaulting da BSG (`Patch_VaultingComponent`).
- **Documentação:** Modular completa em `docs/` (7 artigos técnicos + README + Relatório de Auditoria 01).

---

## 2026-08-30 (GMT-3) — Sessão 1: Documentação Modular e Auditoria Técnica

- **Documentação Modular (`/document-mod`)**:
  - Criada a suíte documental completa em `docs/`:
    - `01-visao-geral-e-arquitetura.md` — Visão geral da arquitetura de escadas escaláveis e pipeline do mod.
    - `02-controlador-de-jogador-e-maquina-de-estados.md` — `PlayerLadderController`, máquina de estados (Mounting, Climbing, Sliding, Dismounting) e custos de stamina/fadiga.
    - `03-cinematica-inversa-e-animacao-procedural.md` — Algoritmos de IK (FinalIK), posicionamento de pés e mãos em degraus (`ProceduralGrip`).
    - `04-infraestrutura-de-cenas-e-ferramentas-de-edicao.md` — Detecção de escadas por colisor/tag, ferramentas de debug e spawn de escadas em mapas.
    - `05-patches-harmony-e-integracao-com-eft.md` — Patches de física (`MovementContext`), transição de vaulting e bloqueio de disparo durante escalada.
    - `06-suporte-multiplayer-coop-fika.md` — Sincronização em rede Fika, replicação de estado de subida e animação de clones observados (`ObservedPlayerLadderController`).
    - `README.md` — Índice central dos artigos técnicos.
- **Auditoria de Código (`/audit-mod-code`)**:
  - Relatório registrado em `docs/relatorio-auditoria-codigo-01.md`.
  - Mapeadas melhorias de zero-allocation em hot paths e segurança de cancelamento de subida.
