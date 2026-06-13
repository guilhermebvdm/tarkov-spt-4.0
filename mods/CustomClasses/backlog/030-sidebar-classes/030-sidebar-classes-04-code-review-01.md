# 030 — Sidebar persistente de classes — Code-review (01)

**Mod:** CustomClasses
**Criado:** 2026-06-12
**Refs:** [02-spec-tech](./030-sidebar-classes-02-spec-tech.md) · [03-spec-tech-review-01](./030-sidebar-classes-03-spec-tech-review-01.md) · [05-asbuild](./030-sidebar-classes-05-asbuild.md)

Território revisado (diff): `modded/Server/Web/Shared/NavMenu.razor`, `modded/Server/Web/Layouts/BaseLayout.razor`.

Política autônoma: APLICO só achados SEGUROS (bug null/crash, build-breaker, fuga de spec com fix inequívoco/local, leak/dispose, fio solto óbvio) marcados `// ref: CR-01-NN` + ✅ Aplicado. ADIO design/layout, fix ambíguo, ou que cruze território de outro item.

Categorias: 🐛 Bug · 🧱 Build-breaker · 📐 Fuga de spec · 💧 Leak/dispose · 🧵 Fio solto · 🎨 Design/layout · ⚡ Eficiência.

Resultado: **nenhum achado atingiu o bar de "aplicar"**. O código é auto-consistente, null-safe, faz `Dispose` correto e bate com os padrões já estabelecidos (`Classes.razor:121-146`, `ClassDetail.razor`, `ClassEdit.razor`). Assinaturas consumidas conferidas contra o código real (ver §Verificações). Todos os achados abaixo são ADIADOS (design/eficiência/cross-território) ou CONFIRMAÇÕES.

---

## Verificações de assinatura (todas conferem)

- `ClassEditorService.GetCachedEntries()` → `IReadOnlyList<ClassFileEntry>` (`ClassEditorService.cs:182`). ✓
- `ClassFileEntry(FileName, Definition?, Enabled, Registered, Diagnostics)` (`ClassEditorService.cs:21-26`) — `entry.Definition`, `entry.Diagnostics`, `entry.Enabled`, `entry.FileName` existem. ✓
- `CostService.ComputeSkillCost(ClassDefinition def)` → `SkillCostBreakdown` (`CostService.cs:106`); `SkillCostBreakdown.Total` (double) e `.WithinBudget` (bool) existem (`CostService.cs:27-28`). `.ToString("0", …)` sobre `double` ✓.
- `ClassDefinition.Name` / `.DisplayName` (LocalizedText) / `.NameColor` / `.IconFile` / `.Enabled` (`ClassDefinition.cs:14/21/41/37/25`); `LocalizedText.En` (`LocalizedText.cs:13`). ✓
- `DiagnosticSeverity.Error` (`ClassDiagnostic.cs:6`). ✓
- URL do ícone `/CustomClasses-Server/icons/{icon}` idêntica a `Classes.razor:137` / `ClassDetail.razor:29`. ✓
- Namespaces: `NavigationLock`, `LocationChangingContext`, `LocationChangedEventArgs`, `NavLinkMatch` ∈ `Microsoft.AspNetCore.Components.Routing` (em `Web/_imports.razor:2`); `IDialogService` ∈ `MudBlazor` (`_imports.razor:6`); `CascadingValue`/`LayoutComponentBase` auto-importados pelo Razor SDK. ✓

---

## 💧 Leak/dispose

### 🟢 CR-01 (confirmação) — `NavMenu` assina e desassina `LocationChanged` corretamente
`OnInitialized` faz `Nav.LocationChanged += OnLocationChanged`; `Dispose()` faz o `-=` e `@implements IDisposable` está declarado (linha 1). Sem leak de handler. Nenhuma ação.

### 🟢 CR-02 (confirmação) — `EditGuardState` é POCO por circuito, não DI singleton
`_guard` é `readonly` campo de instância do layout (1 por circuito) e o `CascadingValue` é `IsFixed="true"`. Não vaza estado "dirty" entre circuitos (resolve o risco do 02 §5). `Reset()` limpa flag + handlers. Nenhuma ação.

---

## 🐛 Bug / crash

### 🟢 CR-03 (confirmação) — todos os caminhos de null estão guardados
- `FallbackGlyph`: `Substring(0,1)` é protegido por `IsNullOrEmpty(row.Name)` antes (linha 299-300). Sem `ArgumentOutOfRange`.
- `OnBeforeNavAsync`: `SaveAsync is null || await SaveAsync.Invoke()` e `Discard?.Invoke()` — null-safe quando o form (025/026) ainda não fez wire-up (`IsDirty` fica `false`, guard é no-op). Comportamento "safe default" documentado no header do BaseLayout.
- `LoadRows`: `def is null` tratado (cost null → total 0 → status nunca `OverBudget` por engano). `def?.IconFile is { Length: > 0 }` evita URL com nome vazio.

Nenhum achado de crash. Nenhuma ação.

---

## 📐 Fuga de spec

### 🟢 CR-04 (confirmação) — implementação bate com 02 §1a/§2/§3/§4
Custo/status computados UMA vez por navegação em `LoadRows()` (não por render) — resolve 🔴-2 do 03-review. `DetectView` faz parse defensivo da URL (split `?`/`#`, `Trim('/')`, checa `parts.Length`) — resolve 🔴-4. `NavigateToClass` preserva vista (edit→edit só se `HasDefinition`, senão cai no detail) — bate com 01-spec. Nenhuma ação.

---

## ⚡ Eficiência (ADIADO — não atinge o bar)

### CR-05 — `FilteredRows()` aloca `List` novo a cada render quando há filtro
`FilteredRows()` é chamado no corpo do `MudNavMenu` (linha 47), logo roda a cada render do componente. Com filtro ativo, `.Where(...).ToList()` aloca uma lista nova por render (não por keystroke). Impacto: micro-alocação numa lista de classes pequena; sem correção (não é bug nem leak). Possível melhora: memoizar `visible` por (`_filter`, `_rows`) ou materializar só quando o filtro muda. **ADIADO** (eficiência, ganho marginal, decisão de design).

### CR-06 — `@bind-Value:after="StateHasChanged"` é redundante com o re-render do `@bind`
O `@bind-Value` já dispara re-render ao atualizar `_filter`; o `:after="StateHasChanged"` é redundante (não causa duplo-render observável porque o Blazor coalesce, mas é ruído). Inofensivo. **ADIADO** (cosmético/design).

---

## 🧵 Fio solto / cross-território (ADIADO)

### CR-07 — `SidebarRow.FileName` e `.HasError` não são lidos no render
`FileName` e `HasError` são projetados no record mas o render usa `BareName` (navegação) e `Status` (que já encapsula `hasError`). Campos "a mais" no view-model; não é dead code perigoso (record imutável, custo zero), pode servir a tooltip/diagnóstico futuro. Removê-los é mudança de design do view-model. **ADIADO** (fio solto cosmético, não obrigatório).

### CR-08 — Guard depende de `ClassEdit.razor` (025/026) para ter sinal de "dirty"
`EditGuardState.IsDirty` nunca é setado pelos arquivos do território 030; o wire-up (`[CascadingParameter] EditGuardState`, set do flag, `SaveAsync`/`Discard`/`Reset`) vive em `ClassEdit.razor`, fora do território. Já documentado como dependência cross-item no 02 §5 e no 03-review 🔴-1. Sem esse toque, o guard é no-op (navegação nunca bloqueada) — safe default, mas o critério de aceite "navegar com mudanças pendentes sempre pergunta" só fecha quando o 025/026 fizer o wire-up. **ADIADO** (cruza território de outro item — NÃO toco). Sinalizado em `deferred`.

### CR-09 — `EditGuardState` exposto como tipo aninhado público em `BaseLayout`
O contrato cross-item exige que `ClassEdit.razor` consuma `[CascadingParameter] BaseLayout.EditGuardState`. Acoplar o tipo do estado dentro do componente de layout é uma decisão de arquitetura (vs. extrair para um POCO de namespace próprio, ex. `CustomClasses.Web.EditGuardState`, mais fácil de consumir/testar). Funciona como está. **ADIADO** (design; e mover o tipo afetaria o consumidor em 025/026 — fora do território).

---

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-12 | Guilherme | Code-review 01 do item 030 (NavMenu + BaseLayout). Nenhum achado seguro p/ aplicar; 5 adiados (CR-05..09). |
