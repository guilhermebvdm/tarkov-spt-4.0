# 024 — Viewer de classes — As-built

**Mod:** CustomClasses
**Data:** 2026-06-10
**Refs:** [01-spec](./024-class-viewer-01-spec.md) · [02-spec-tech](./024-class-viewer-02-spec-tech.md)

## Arquivos entregues

| Arquivo | Conteúdo |
|---|---|
| `modded/Server/Web/Pages/Classes.razor` | `@page "/customclasses/classes"` — lista via `ListClassFiles()` (021): ícone do wwwroot, nome na `nameColor`, displayName (en), chip de status (Invalid/Disabled/Registered/Not registered; Invalid com tooltip dos diagnostics), nº de skills, chip de custo de skills (budget 28–32), loadout ₽, arquivo. Linha clicável → detalhe (nome sem extensão na rota). Parse error renderiza a linha com erro. |
| `modded/Server/Web/Pages/ClassDetail.razor` | `@page "/customclasses/classes/{FileName}"` — resolve nome com/sem extensão contra `ListClassFiles()`; diagnostics no topo (MudAlert por severidade); painéis: General, Skills (breakdown 022 + total + warnings), XP multipliers (cores + badge SE + aviso `!SkillsExtendedInstalled`), Hideout, Outfit (nomes via `GetClothing`), Equipped (árvore recursiva), Stash (linhas `context=="stash"` + badge "⚠ no price"), Cost summary (dois totais + breakdown completo + warnings). |
| `modded/Server/Web/Shared/ClassViewItemSpec.razor` | componente recursivo de `ItemSpec` — preset resolvido (default/premium, mesmos resolvers internos do CatalogService que o CostService usa), chips premium/count/ammo, ícones loadedMag/chambered, mods (→ ClassViewModSpec) e contents (auto-recursão), cap de profundidade 12. |
| `modded/Server/Web/Shared/ClassViewModSpec.razor` | componente recursivo de `ModSpec` (slotId + nome resolvido). |
| `modded/Server/Web/Shared/NavMenu.razor` | EDITADO — link "Classes" agora aponta `/customclasses/classes` (era placeholder). |
| `modded/Server/Web/Pages/Home.razor` | EDITADO — card "Class editor" com botão pra lista, acima do smoke test do 020 (mantido). |

Não tocados (território dos itens 021/022/023): `ClassEditorService.cs`, `CostService.cs`, `CatalogService.cs`, `ClassRegistrar.cs`, builders, registries, `CustomClasses.Server.csproj`, demais componentes de `Web/Shared/`.

## Verificação de símbolos (sem build — exclusividade de build com o agente 023)

- `ClassEditorService.ListClassFiles()` → `List<ClassFileEntry>` (`FileName/Definition/Enabled/Registered/Diagnostics`) — ClassEditorService.cs:79.
- `CostService.ComputeSkillCost/ComputeLoadoutCost` + records (`SkillCostEntry.Skill/Level/Weight/Origin/Cost`, `LoadoutCostEntry.Tpl/Name/Context/Qty/UnitPrice/PriceSource/Subtotal/MissingPrice`) — CostService.cs.
- `CatalogService.GetItemName(MongoId, lang)`, `GetClothing(side)`, internos `ResolveDefaultPreset/ResolvePremiumPreset` (mesma assembly → acessíveis dos .razor) — CatalogService.cs:108,318,334,421.
- `ClassRegistrar.SkillsExtendedInstalled` — ClassRegistrar.cs:94; `SkillsExtendedCompat.Skills` — SkillsExtendedCompat.cs:15.
- `SkillWeights.BudgetMin/BudgetMax` — SkillWeights.cs:38-39.
- `DiagnosticSeverity`/`ClassDiagnostic.Code/Message` — ClassDiagnostic.cs.
- MudBlazor **8.13.0** confirmado em `obj/project.assets.json`; render `InteractiveServer` global confirmado em `references/spt-source/Libraries/SPTarkov.Server.Web/SPTWeb.cs:28,39` (eventos de UI funcionam).
- Estáticos: `wwwroot/icons/*.png` existem p/ as 12 classes; mount `/CustomClasses-Server/` (020).

## Pendências (orquestrador)

- [ ] **Build integrado** (`/compile-mod CustomClasses`) — não rodado aqui (exclusividade do agente 023 / build do orquestrador na junção da wave W2).
- [ ] **Smoke no browser**: 11+1 classes na lista, custos batendo com `scripts/check-skill-costs.mjs` (022), detalhe de uma classe com preset (Caçador) e da Peladão (vazia), classe inválida plantada à mão aparece/some.
- [ ] Conferir colisão de componentes com o merge do 023 em `Web/Shared/` (prefixo `ClassView*` deve evitar).
- [ ] DoD: validar tooltip de diagnostics na lista com classe inválida real.
