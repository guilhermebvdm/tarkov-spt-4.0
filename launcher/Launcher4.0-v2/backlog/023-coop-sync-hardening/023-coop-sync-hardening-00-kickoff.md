# 023 — Coop-sync hardening (Fika PVE) · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Origem:** [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md) (gaps de coop) · **Severidade:** 🟡 · **Deps:** 007, 010, 006

> Brief de kickoff — insumo para `/create-spec`. Não é a spec. Contexto: servidor é Fika Coop PVE multiplayer (solo=host mascara bugs de cliente).

## Achados
- **Mirror-move quarentena plugin client-only** (`SyncRuleResolver.cs:32-35` + `SyncPlanner.cs:253-263`): o fallback marca `plugins`/`patchers` como `mirror-move-disabled`; qualquer arquivo sob `plugins` **fora do manifesto** vai p/ `plugins-disabled` no 1º sync. Se `Fika.Core.dll` não estiver no manifesto → **coop quebra** (recuperável mas silencioso).
- **Excluir conta do host durante sessão coop** (`ProfileView.axaml:162`): botão gated só no estado **local** (`!GameRunning && !IsUpdating`); remove `{id}.json` server-side no meio da raid dos clientes.
- **Auth headless dos clientes extras** depende de a authkey compartilhada ser reusável/`--unattended` (`TailscaleHelper.cs:16-17`); se single-use, só o 1º cliente entra headless.

## Critérios de aceite (seed)
- Plugins client-only conhecidos (Fika) **nunca** quarentenados (allowlist no fallback ou garantir no manifesto/`ignoredFiles`).
- Excluir conta considera sessão coop ativa (aviso/gate na medida do que o launcher consegue saber).
- Tipo de authkey documentado/validado (gate operacional).
