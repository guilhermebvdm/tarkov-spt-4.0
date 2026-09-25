# 011 — remocao-otimizador-lunetas-smart-pip · Review Técnica 01

**Mod:** TRL-CoreSight  
**Spec técnica revisada:** [011-remocao-otimizador-lunetas-smart-pip-02-spec-tech.md](011-remocao-otimizador-lunetas-smart-pip-02-spec-tech.md)  
**Data:** 2026-09-20  

> Análise crítica preventiva da remoção do subsistema Smart PiP.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 1 · 🟢 Menores: 1 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | B — Edge Case | 🟡 Importante | Restauração de sombras originais quando `EnableInteriorShadowCulling` for desativado | ✅ Resolvido em 2026-09-20 |
| PA-01-02 | C — Consistência | 🟢 Menor | Limpeza de referências em `PROPRIEDADES.md` e numeração de seções | ✅ Resolvido em 2026-09-20 |

---

## Pontos

### PA-01-01 · B — Edge Case · 🟡 Importante

**Restauração de sombras originais quando `EnableInteriorShadowCulling` for desativado**

**Problema:** Em `InteriorOcclusionWatcher.cs`, a verificação inicial avaliava `(!ModConfig.EnableInteriorShadowCulling.Value && !ModConfig.EnableSmartPiP.Value)`. Ao remover o termo `!ModConfig.EnableSmartPiP.Value`, se o usuário desabilitar `EnableInteriorShadowCulling` no meio da raid enquanto estava dentro de um prédio, a distância de sombras ficaria congelada no valor interno reduzido se não houver restauração explícita para `_originalShadowDistance`.

**Por que importa:** Pode deixar as sombras em 25m para sempre se o jogador alternar o toggle no F12 enquanto estiver em local fechado.

**Sugestão:** Garantir que o bloco `if (!ModConfig.ModEnabled.Value || !ModConfig.EnableInteriorShadowCulling.Value)` restaure `QualitySettings.shadowDistance = _originalShadowDistance` imediatamente quando a flag estiver desligada.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** A verificação já inclui o bloco de restauração suave/imediata para `_originalShadowDistance` caso a diferença seja > 0.1f, mantendo o fallback seguro.

---

### PA-01-02 · C — Consistência · 🟢 Menor

**Limpeza de referências em `PROPRIEDADES.md` e numeração de seções**

**Problema:** A tabela de documentação de propriedades no repositório listava a seção `"7. Otimizador de Lunetas (Smart PiP)"`. Manter essa seção com o código removido induziria futuros desenvolvedores e usuários a procurarem por opções inexistentes.

**Por que importa:** Mantém a documentação técnica perfeitamente sincronizada com as opções reais expostas pelo BepInEx ConfigurationManager.

**Sugestão:** Deletar a seção 7 de `PROPRIEDADES.md` por completo durante a execução.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Seção 7 removida de `PROPRIEDADES.md`.
