# 001 — Fix descarte de mochila ("ZZ" / DropBackpack) no Headless/Host · Spec Tech Review 01

**Mod:** FIKA  
**Spec Técnica:** [001-drop-backpack-sync-fix-02-spec-tech.md](001-drop-backpack-sync-fix-02-spec-tech.md)  
**Data:** 2026-09-03  

---

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 1 · 🟢 Menores: 1 · ✅ Resolvidos: 2 · Total: 2

---

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | D — Arquitetura | 🟡 Médio | Isolamento e necessidade de alteração em `Fika-Headless` | ✅ Resolvido |
| PA-01-02 | B — Bug latente | 🟢 Menor | Compatibilidade com descarte de armas de fogo em mãos | ✅ Resolvido |

---

## Pontos

### PA-01-01 · D — Arquitetura · 🟡 Médio

**Isolamento e necessidade de alteração em `Fika-Headless`**

- **Discussão:** O cliente Headless precisa de um patch próprio para descarte de mochila?
- **Análise:** O `Fika-Headless` é uma casca que inicializa a raid e depende diretamente de `Fika.Core.dll`. Como o subsistema de rede, entidades de jogador (`ObservedPlayer`) e execução de pacotes de inventário operam internamente no `Fika.Core`, aplicar a correção no `Fika.Core.dll` resolve o problema no Headless, no Host coop e em todos os clientes remotos. A única ação no Headless é atualizar a referência binária `References/Fika.Core.dll`.
- **Status:** ✅ Resolvido.

### PA-01-02 · B — Bug latente · 🟢 Menor

**Compatibilidade com descarte de armas de fogo em mãos**

- **Discussão:** O patch de prefix pode interferir quando o jogador descarta sua arma primária que está empunhada nas mãos?
- **Análise:** A condição `(__instance.HandsController == null || __instance.HandsController.Item != item)` garante que, caso o item seja a arma empunhada (`Item == item`), o Prefix retorna `true`, acionando o método nativo `SetControllerInsteadRemovedOne`, que já possui override em `ObservedPlayer.cs:1690` com `callback.Succeed()`. Portanto, não há risco de conflito.
- **Status:** ✅ Resolvido.
