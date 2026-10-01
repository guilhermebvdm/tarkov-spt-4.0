# 002 — Drop de Arma na Morte + Capacete/Óculos no Desmembramento de Cabeça · Code Review 01

**Mod:** VisceralCombat
**Spec funcional:** [002-drop-arma-capacete-oculos-cabeca-01-spec.md](002-drop-arma-capacete-oculos-cabeca-01-spec.md)
**Spec técnica:** [002-drop-arma-capacete-oculos-cabeca-02-spec-tech.md](002-drop-arma-capacete-oculos-cabeca-02-spec-tech.md)
**Asbuild:** [002-drop-arma-capacete-oculos-cabeca-05-asbuild.md](002-drop-arma-capacete-oculos-cabeca-05-asbuild.md)
**Data:** 2026-09-09

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

**Memória consultada:** snapshot de 2026-08-24 (Sessão 2026-08-24) — nenhuma pendência aberta que afete este item. **Docs técnicos conferidos:** `spt-antipatterns.md`, `fika-packet-desync-prevention-plan.md` (código não introduz pacote novo, dívida pré-existente já documentada na spec técnica §7, fora de escopo deste diff).

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟡 Médio | `WeaponDropOnDeathPatch` pode descartar a arma silenciosamente se `InventoryController` não for `TraderControllerClass` | ✅ Aplicado em 2026-09-09 |
| CR-01-02 | E — Legibilidade | 🟢 Menor | Indentação mista (espaço/tab) no comentário de continuação da linha 53-54 | ✅ Aplicado em 2026-09-09 |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### CR-01-01 · B — Bug latente · 🟡 Médio · ✅ Aplicado em 2026-09-09

**`WeaponDropOnDeathPatch` retorna `false` incondicionalmente, mesmo quando o `ThrowItem` não chega a rodar**

**Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/WeaponDropOnDeathPatch.cs:44-54`](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/WeaponDropOnDeathPatch.cs#L44-L54)

**Problema:**
```csharp
if (!(FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer)) return true;

if (__instance.InventoryController is TraderControllerClass controller)
{
    controller.ThrowItem(item, false, null);
}

return false; // pula o fling cosmético do vanilla...
```
Se o gate de autoridade passar (host/singleplayer) mas `__instance.InventoryController` **não** for um `TraderControllerClass` (o `is` falha — por exemplo, se `_inventoryController` ainda for `null` num ponto de lifecycle inesperado, algo que `csharp-mod-best-practices` §4 pede pra tratar como possível para qualquer `Singleton`/controller do EFT), o método simplesmente pula o `if` e cai direto no `return false` da linha 53 — sem nunca ter chamado `ThrowItem`. O resultado: a arma não vira item solto (o objetivo da feature) **e** o fling cosmético do vanilla (`Player.cs:26853 AttachWeapon`) também é pulado (porque retornamos `false`). A arma simplesmente desaparece do raid, sem aviso.

**Por que importa:** É um cenário de baixa probabilidade (a cadeia `TraderControllerClass ← GClass3384 ← InventoryController ← PlayerInventoryController`, confirmada na spec técnica §0/§5.1, faz o `is TraderControllerClass` praticamente sempre verdadeiro para qualquer `Player` concreto), mas não impossível — e a consequência é perda permanente de item (economia do raid), sem log de aviso que ajude a diagnosticar caso aconteça. `DropHeadEquipment` (`KillPatch.cs:480`) não tem esse problema porque é `void` e faz `return` cedo antes de qualquer side-effect quando o mesmo cast falha — a mesma disciplina não foi aplicada aqui porque o Prefix precisa decidir `true`/`false` para o Harmony.

**Sugestão:** Só retornar `false` quando o `ThrowItem` de fato rodou; caso contrário, deixar o vanilla `DropItemDead` executar (mais seguro que perder o item):
```csharp
if (__instance.InventoryController is TraderControllerClass controller)
{
    controller.ThrowItem(item, false, null);
    return false; // pula o fling cosmético — a arma já foi removida do inventário
}

QuickLogger.Log(ELogType.Warn, $"[WeaponDropOnDeathPatch] InventoryController não é TraderControllerClass para {__instance.Profile?.Nickname} — mantendo comportamento vanilla.");
return true;
```

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/WeaponDropOnDeathPatch.cs:51-58` — `return false` movido para dentro do `if (controller...)`, e um `else` implícito (fallthrough) loga `ELogType.Warn` e retorna `true` (mantém vanilla) quando o cast falha. Rebuild confirmado (`dotnet build`, Release — Build succeeded).

---

### CR-01-02 · E — Legibilidade · 🟢 Menor · ✅ Aplicado em 2026-09-09

**Indentação mista no comentário de continuação**

**Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/WeaponDropOnDeathPatch.cs:53-54`](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/WeaponDropOnDeathPatch.cs#L53-L54)

**Problema:** A segunda linha do comentário (`// a arma já foi removida...`) usa espaços para alinhar sob o `//` da linha anterior, em vez de seguir a indentação por tab do resto do arquivo — inofensivo para o compilador, mas destoa do estilo do arquivo (e do restante do mod, que usa tabs consistentemente).

**Por que importa:** Puramente cosmético — não afeta comportamento, só a consistência visual do diff.

**Sugestão:** Realinhar a segunda linha do comentário com um tab, igual ao resto do arquivo, ou juntar em uma única linha de comentário.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada — comentário reescrito em uma única linha, indentação por tab consistente com o resto do arquivo (correção incorporada junto de CR-01-01, mesmo bloco de código).
**Aplicação:** `mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/WeaponDropOnDeathPatch.cs:54`.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-09 | Code review 01 criada via `/code-review` |
| 2026-09-09 | Aplicação automática de 2 achados via `/apply-code-review` — IDs aplicados: CR-01-01, CR-01-02 |
