# Handoff — CustomClasses pós-unificação (gates pendentes + balance board)

> **Data:** 2026-07-10 (trabalho da sessão: 2026-07-03 → 07-06)<br>
> **De:** sessão longa 053+ (worktree wt-057, agora extinto)<br>
> **Para:** nova sessão de continuidade do CustomClasses<br>

## ⚡ Próxima ação mais importante

**Aguardar/consumir os resultados do gate humano** e, na sequência, **aplicar a Onda 0 do balance board** quando o
usuário marcar ✅ nas linhas B1–B19 de
[`mods/CustomClasses/backlog/balance-review-2026-07-05.md`](../mods/CustomClasses/backlog/balance-review-2026-07-05.md)
(§2 — painel de decisões). A Onda 0 (B1 · B17 · B2/B3 · B4) é quick-win sem risco e está totalmente especificada lá.
**Não aplicar nada do board sem o ✅ do usuário** — é análise aprovada como DOC, não como mudança.

## Estado do repositório

- **Tudo em `main`, tudo pushado** (`main` = `origin/main`). Unificação 2026-07-06: merges de
  `feat/trl-items-autodev` + `feat/053-perks-property-model`; worktree `tarkov-spt-4.0-wt-057` REMOVIDO.
  As 2 branches de feature ainda existem como refs locais 100% merged (P-13.4: deletar quando o usuário autorizar).
- **Instalado em `D:/SPT`**: DLLs client+server e configs sincronizados com o repo (build da árvore principal,
  2026-07-06). Server precisa de **restart** para o que ainda não foi testado.
- Trabalho em qualquer arquivo: direto na árvore principal `C:/Repos/spt/tarkov-spt-4.0` (não existe mais worktree).

## Fonte de verdade para contexto (ler nesta ordem)

1. [`mods/CustomClasses/memory/sessions.md`](../mods/CustomClasses/memory/sessions.md) — **Sessão 13** (última):
   decisões, lições e pendências P-13.x. Snapshot no topo está atualizado.
2. [`mods/CustomClasses/HANDOFF.md`](../mods/CustomClasses/HANDOFF.md) — pendências por item (4b=062, 6=057…).
3. [`mods/CustomClasses/backlog/mod-backlog.md`](../mods/CustomClasses/backlog/mod-backlog.md) — status por item
   (novos: 061 Quick Hands, 062 baseline v2, 063 rota do launcher — renumerado na unificação).

## Pendências vivas (IDs da memória)

| ID | O quê | Bloqueado por |
|---|---|---|
| **P-13.1** 🟡 | Gate consolidado in-game: 051 (estamina de braço Hunter/Tank) · 054 (perk gateado por "Stealth" morde) · 057 (deploy: brasão/cor por player + popover NO CURSOR; **coop 2+ como CLIENTE** é o teste crítico; lista inferior do FIKA intocada) · 058 (GP-25 XP ao vivo ~0.5/tiro; persiste; recuo com RealRecoil OFF; log "has no buffs") · 060 (footer WEAPON MASTERY vivo) · UI r5 (aba CLASS: crest escuro selecionado, label visível desselecionado; cores v2 sem drift) · **re-teste Peladão** (skin Tagilla+BEAR Vacation, SEM faca, SEM container) | usuário (in-game) |
| **P-13.2** 🟡 | Decisões B1–B19 + RN-03 (mastery por classe — considerar B13/B15, produto de multiplicadores) | usuário (✅/❌ no §2 do board) |
| **P-13.3** 🟢 | Weight Marker (056): usuário calibra X/Y no F12 → fixar default no `PerksConfig` | usuário (valores) |
| **P-13.4** 🟢 | Deletar branches locais merged | aprovação trivial |
| P-10.1 🔴 | Validação in-game dos ~21 efeitos do 050 (parcialmente coberta pelos gates acima) | usuário |
| P-10.2 🟡 | Deferrals: Combat Medic transpiler (=B12), Quick Hands (**agora item 061**, com anotação do bônus elite vanilla da Search), Iron Lungs sway | — |

## Fatos que a próxima sessão PRECISA saber (armadilhas já pagas)

- **Host do deploy:** `RaidReadyPlayerPanel` é código morto no SPT; o host real é `PartyPlayerItem`/`PartyInfoPanel`
  (patch: `modded/Client/Patches/PartyPlayerItemPatch.cs`). Antes de patch de UI, provar via decompile que o caller
  roda no modo SPT.
- **Lista inferior do FIKA no deploy é INTOCÁVEL** (regra do usuário). `ClassDetailLoadingPatch` existe mas está
  desregistrado.
- **`items.json` tem NODES** — validação de tpl deve filtrar `_type === 'Item'` (extrator já faz).
- **Som de classe é host-only vs bots** em coop (B14 do board tem o caminho de fix sem protocolo novo).
- **Recuo empilha por produto** (mastery×perks — Anexo C do board); mastery 51 anula o Shaky Hands nas 3 categorias.
- **Baseline v2 (062)**: políticas no extrator (`scripts/extract-from-profile.mjs`) — mags cheios, pinagem x/y só no
  stash, Alpha p/ todos, TUE no Saqueador, 300k normalizado, DSP excluído, `ItemSpec.Remove` (Peladão). Re-extração:
  `node scripts/extract-from-profile.mjs --profile <id> --class <classe>` com jogo/servidor fechados. Perfis-fonte
  mapeados na conversa de 2026-07-06 (Cacador `6a25db81…`, Medico `6a25cc1f…`, Furtivo `6a2ff55e…`, Saqueador
  `6a306c40…`, Fuzileiro `6a4a0695…`, Tanque `6a4ae66b…`).
- **Dependências de mod das classes:** Furtivo usa itens do `c11-tn-4`; Tanque usa belt do `WTT-PackNStrap`; skin do
  Peladão vem do `AllTheClothes`. Clientes do coop precisam dos plugins client correspondentes.
- **Launcher sync** pode reverter DLLs locais (Dev Mod off) — se um comportamento "sumir", conferir data das DLLs.

## Skills sugeridas para a próxima sessão

- `/g-autodev` — para executar a Onda 0/1 do balance (quando aprovada) ou o item 061 ponta a ponta.
- `/g-review-content` — revisar qualquer plano/spec antes de aplicar (padrão da casa; o board passou por 2 rodadas).
- `/code-review` + `spt-mod-best-practices` + `csharp-mod-best-practices` — em qualquer mudança de patch.
- `/update-mod-graph CustomClasses` — após mudanças de código (grafos regenerados por último em f52b6fa).
- `/update-memory CustomClasses` — ao fechar a sessão (Sessão 14; seguir `memory-curation`).
- `/compile-mod CustomClasses` — build+install (usar `--force-config` conscientemente; guard anti-clobber é amigo).

## Artefatos-chave (não duplicar conteúdo — ler no lugar)

- Balance board: `mods/CustomClasses/backlog/balance-review-2026-07-05.md` (v4 — B1–B19, ondas, anexos A/B/C)
- Code-review em lote: `mods/CustomClasses/backlog/code-review-2026-07-04-unreviewed-batch.md` (19 findings)
- 057 história completa: `mods/CustomClasses/backlog/057-class-identity-coop/` (specs + fixes 01→04)
- Baseline v2: plano `~/.claude/plans/fluffy-finding-stonebraker.md` + backlog 062 + `docs/class-schema.md`
  (políticas documentadas, campo `remove`, x/y/rotated)
- Commits de referência: `71998fe` (review batch) · `217957f` (re-extração) · `50bd261` (merge 053) · `f52b6fa` (grafos)
