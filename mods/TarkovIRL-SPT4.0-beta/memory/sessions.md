# TarkovIRL (SPT 4.0 Beta) — Memória de Sessões

## Snapshot Delta
- **Versão:** 4.0.0-beta (SPT 4.0)
- **Estado:** Desenvolvimento ativo de física de armas, Free Aim, Sway, Parallax e inércia.
- **Pendências:** 🟢 Nenhuma pendência blocker registrada.

---

## 2026-07-28 — Sessão 1: Inicialização da Governança Estrutural
- **Ação:** Criação de `mod.json`, `README.md`, `PROPRIEDADES.md` (mapeando 60+ opções F12 do `PrimeMover.cs`) e `memory/sessions.md`.

---

## 2026-07-29 02:40 (GMT-3) — Sessão 2: Diretriz de Isolamento de Rede para Inércia e Sway em 3ª Pessoa (Canal 3 / TIRL)

**Tema central:** Especificação da diretriz de rede no `ROADMAP.md` para eventual sincronização visual de inércia, sway e FreeAim da arma para a 3ª pessoa.

**Decisões-chave:**
- **Diretriz de Rede (ROADMAP.md):** Especificada no [ROADMAP.md](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TarkovIRL-SPT4.0-beta/ROADMAP.md) a transmissão de qualquer evento de oscilação visual de arma para outros jogadores no **Channel 3 Compartilhado TRL** (`Unreliable`) com a assinatura binária `TIRL` (`0x54 0x49 0x52 0x4C`).
- **Imunidade:** Garante que a movimentação visual de inércia ocorra sem interferir na movimentação ou inventário nativo do FIKA no `Channel 0`.

**Lições / hipóteses descartadas:**
- A hipotética necessidade de um canal exclusivo para o TarkovIRL foi descartada: o mod compartilha harmoniosamente o Canal 3 de dados e postura TRL através do Magic Header `TIRL`.

**Atividade cronológica:**
1. Análise da matemática de física de mãos e inércia visual em 3ª pessoa.
2. Criação do `ROADMAP.md` formalizando a arquitetura do Canal 3 Compartilhado TRL com Magic Header `TIRL`.
