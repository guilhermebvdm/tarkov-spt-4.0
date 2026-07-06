# 053 — Perks/Drawbacks UI (aba CLASS + render 3D) · Code Review 01

**Mod:** CustomClasses
**Spec funcional:** _(item iterativo — sem 01-spec formal)_
**Spec técnica:** _(item iterativo — sem 02-spec-tech formal)_
**Asbuild:** [053-perks-ui-tab-05-asbuild.md](053-perks-ui-tab-05-asbuild.md)
**Data:** 2026-07-01

> Análise crítica do código implementado nesta sessão (aba CLASS na tela de Skills + lista de cards + render 3D do personagem). Item desenvolvido de forma **iterativa** (fora do pipeline `spec → spec-tech → code-mod`), então esta review cobre o código-como-está. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 1 · 🟡 Médios: 7 · 🟢 Menores: 3 · ✅ Resolvidos: 11 · Total: 11
>
> **Todos endereçados em 2026-07-01** (DLL `CustomClasses-Client.dll` 100352 bytes). 9 corrigidos no código; CR-01-02 e CR-01-06 aceitos como **dívida consciente documentada** (side-effect inócuo na tela de Skills / boneco menu-only). Detalhe na seção **Resolução** ao fim.

Nada garante crash (tudo em `try/catch` com degradação para cards-only). O achado mais importante é **CR-01-01** (estado estático stale do render → boneco pode não recarregar na 2ª abertura / após cancelamento de load), que casa com o checklist §12 do `csharp-mod-best-practices` ("stale state across context switches", precedente stances 002 CR-01-02).

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟠 Forte | Estado estático do render não reseta ao reclonar / após cancelar load | ✅ Aplicado |
| CR-01-02 | D — Arquitetura | 🟡 Médio | `window.Show` chama `ItemUiContext.CloseAllWindows()` a cada clique em CLASS | ✅ Dívida documentada |
| CR-01-03 | B — Bug latente | 🟡 Médio | `RefreshPanel` recria todos os cards por ativação (Destroy deferido → flicker) | ✅ Aplicado |
| CR-01-04 | D — Arquitetura | 🟡 Médio | `FindObjectsOfTypeAll().FirstOrDefault()` sem filtro nem log da fonte | ✅ Aplicado |
| CR-01-05 | C — Gap/UX | 🟡 Médio | Cobertura de sprite: LMG/HMG/HeavyVests podem faltar → halo vazio | ✅ Aplicado |
| CR-01-06 | D — Arquitetura | 🟡 Médio | Render nunca recebe `.Close()` → loader/CTS não cancelados ao esconder | ✅ Dívida documentada |
| CR-01-07 | D — Arquitetura | 🟡 Médio | Injeção Harmony por nome `profile` vs `__0` posicional (robustez) | ✅ Aplicado |
| CR-01-08 | B — Bug latente | 🟡 Médio | Layout aninhado sem `ContentSizeFitter` → efeito longo (Bunker) pode clipar | ✅ Aplicado |
| CR-01-09 | E — Legibilidade | 🟢 Menor | Reflection `_progressSpinner` não cacheada em `static readonly` | ✅ Aplicado |
| CR-01-10 | E — Legibilidade | 🟢 Menor | Doc-comment da classe não menciona render 3D nem cards | ✅ Aplicado |
| CR-01-11 | B — Bug latente | 🟢 Menor | Ghost-clear destrói o spinner se a reflection de `_progressSpinner` falhar | ✅ Aplicado |

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

### CR-01-01 · B — Bug latente · 🟠 Forte

**Estado estático do render (`_window`/`_lastRenderedProfile`/`_ghostCleared`) não reseta ao reclonar nem revalida se o load completou**

**Local:** [`mods/CustomClasses/modded/Client/Patches/SkillsClassTabPatch.cs:358-402`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L358)

**Problema:** `ShowRender` decide recarregar só por identidade de profile:
```csharp
var needsLoad = !ReferenceEquals(_lastRenderedProfile, _profile);
if (!needsLoad) return;               // já carregado p/ este profile — só reativou
...
_window.Show(_profile, null, false);  // async — pode ser cancelado
_lastRenderedProfile = _profile;      // marcado como carregado IMEDIATAMENTE
```
Dois cenários quebram:
1. **Load cancelado (race):** `PlayerModelView.Show` é `async` e se auto-cancela quando o GameObject é escondido (token do `PoolManager`). Se o usuário clica CLASS e troca pra SKILLS antes do modelo carregar, o load é cancelado — mas `_lastRenderedProfile` já foi setado → nas próximas aberturas `needsLoad=false` → **nunca re-tenta → boneco fica vazio pra sempre**.
2. **Tela destruída e recriada:** se a `SkillsAndMasteringScreen` for destruída (não só escondida) e recriada, a idempotência (`:80`) reconstrói a aba e o `TryBuildRender` cria um `_window` novo — mas `_lastRenderedProfile` ainda `ReferenceEquals` o mesmo `profile` do menu → `needsLoad=false` → o clone novo **nunca** recebe `Show` → render vazio. Além disso `_ghostCleared` continua `true` → o modelo-fantasma do clone novo não é limpo.

**Por que importa:** é exatamente o fluxo que o usuário vai testar (abrir Skills → CLASS → fechar → reabrir). Falha silenciosa (sem erro no log), difícil de diagnosticar remotamente. Casa com o checklist §12 do `csharp-mod-best-practices` (precedente: stances 002 CR-01-02, snap no weapon novo após swap).

**Sugestão:**
- Em `TryBuildRender`, logo após criar o clone novo, **resetar** o estado: `_lastRenderedProfile = null; _ghostCleared = false;`.
- Em `ShowRender`, revalidar contra o load real do modelo, não só o profile:
  ```csharp
  var pmv = _window.GetComponentInChildren<PlayerModelView>(true);
  var needsLoad = !ReferenceEquals(_lastRenderedProfile, _profile) || pmv == null || !pmv.LoadingComplete;
  ```
  (`PlayerModelView.LoadingComplete` é público — `PlayerModelView.cs:30`.)

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-02 · D — Arquitetura · 🟡 Médio

**`window.Show` dispara `ItemUiContext.Instance.CloseAllWindows()` a cada ativação de CLASS**

**Local:** [`mods/CustomClasses/modded/Client/Patches/SkillsClassTabPatch.cs:395`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L395)

**Problema:** `InventoryPlayerModelWithStatsWindow.Show` (decompilado, `:126`) executa `ItemUiContext.Instance.CloseAllWindows()` no início. Como eu chamo o `Show` completo a cada clique em CLASS, todo clique fecha janelas/tooltips/context-menus abertos no menu — efeito colateral global disparado por navegar numa aba.

**Por que importa:** clicar numa aba não deveria mexer no estado de outras janelas do menu. Hoje é quase sempre inofensivo na tela de Skills, mas é acoplamento indevido e pode surpreender (ex.: fechar um tooltip que o usuário estava lendo).

**Sugestão:** aceitar como dívida conhecida (baixo risco), **ou** trocar o `_window.Show(...)` completo por um caminho estreito via reflection que chame só `method_3` (carrega o boneco) + `_playerLevelPanel.Set` + `_nicknameLabel`/`_experienceLabel` — evitando `CloseAllWindows`. Custo/benefício provavelmente não compensa agora; recomendo **documentar e aceitar**.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-03 · B — Bug latente · 🟡 Médio

**`RefreshPanel` destrói e recria todos os cards a cada ativação — Destroy é deferido → cards duplicados por 1 frame no `ForceRebuildLayoutImmediate`**

**Local:** [`mods/CustomClasses/modded/Client/Patches/SkillsClassTabPatch.cs:202-223`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L202)

**Problema:**
```csharp
for (var i = list.childCount - 1; i >= 0; i--)
    UnityEngine.Object.Destroy(list.GetChild(i).gameObject);   // deferido p/ fim do frame
...
foreach (var e in entries) BuildCard(list, e, font);           // adiciona já
...
LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)panel.transform);
```
`Destroy` só remove no fim do frame; os cards novos são adicionados imediatamente. No mesmo frame, o `List` tem **cards velhos (pendentes de destruição, ainda ativos) + novos** → o `ForceRebuildLayoutImmediate` roda com o dobro de filhos → layout calculado pra 2× e reposicionado no frame seguinte. Visível como flicker/pulo de 1 frame a cada abertura de CLASS. Também é reconstrução desnecessária (a classe raramente muda).

**Por que importa:** flicker perceptível em cada clique + trabalho redundante. Menor, mas fácil de eliminar.

**Sugestão:** reconstruir só quando necessário — guardar a classe já renderizada e pular se igual:
```csharp
if (list.childCount > 0 && _lastPanelClass == SkillMultipliers.ClassNameEn) { ShowRender(); return; }
_lastPanelClass = SkillMultipliers.ClassNameEn;
// ... limpa + rebuild
```
(ou usar `DestroyImmediate` — desaconselhado). Preferir o guard por classe.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-04 · D — Arquitetura · 🟡 Médio

**`FindObjectsOfTypeAll<...>().FirstOrDefault()` pode pegar o prefab-asset ou instância errada; sem filtro nem log da fonte**

**Local:** [`mods/CustomClasses/modded/Client/Patches/SkillsClassTabPatch.cs:322`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L322)

**Problema:** `Resources.FindObjectsOfTypeAll<InventoryPlayerModelWithStatsWindow>()` retorna **todas** as instâncias carregadas — incluindo o prefab-asset (não-instanciado em cena) e instâncias inativas — em ordem arbitrária. `FirstOrDefault()` pode clonar o asset ou uma instância nunca inicializada, o que muda o resultado do render (refs internas ok, mas estado editor-only / não-inicializado pode divergir da instância viva da aba OVERALL).

**Por que importa:** fonte do clone é não-determinística; se pegar a "errada", o boneco pode renderizar diferente ou não renderizar. E não há log de qual foi escolhida → debug remoto às cegas.

**Sugestão:** preferir uma instância de cena e logar a fonte:
```csharp
var all = Resources.FindObjectsOfTypeAll<InventoryPlayerModelWithStatsWindow>();
var template = all.FirstOrDefault(w => w.gameObject.scene.IsValid())   // instância de cena
             ?? all.FirstOrDefault();
Plugin.Log?.LogInfo($"[053-3d] template='{template?.name}' scene={template?.gameObject.scene.name ?? \"(asset)\"} de {all.Length}");
```

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-05 · C — Gap/UX · 🟡 Médio

**Cobertura de sprite incompleta: `LMG`/`HMG`/`HeavyVests` podem não existir em `SkillIdSprites` → card com halo vazio**

**Local:** [`mods/CustomClasses/modded/Client/PerksCatalog.cs:78-89`](../../modded/Client/PerksCatalog.cs#L78) (mapeamento) + [`SkillsClassTabPatch.cs:452-461`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L452) (fallback)

**Problema:** `IconSprite` faz `SkillIdSprites.GetValueOrDefault(id)`. Nem todo `ESkillId` tem sprite no dicionário (só os que aparecem na tela de Skills). Bunker→`LMG`, Bulwark→`HeavyVests` (e um eventual HMG) são candidatos a **não ter sprite** → `iimg.enabled = false` → card mostra só o halo tingido, sem ícone. Degradação graciosa, mas contraria o objetivo "cada perk com seu ícone".

**Por que importa:** justamente os perks-assinatura do Tanque (Bunker/Bulwark) podem sair sem ícone — o oposto do "premium".

**Sugestão:** validar cobertura in-game; para os que faltarem, fazer fallback pra um `ESkillId` garantido (ex.: `RecoilControl` p/ Bunker, `Vitality`/`Health` p/ Bulwark) ou um sprite genérico. Alternativa: logar 1× os `ESkillId` sem sprite pra saber quais curar.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-06 · D — Arquitetura · 🟡 Médio

**Render nunca recebe `.Close()` — `PlayerModelLoader`/CancellationTokenSource não são cancelados ao esconder a aba; boneco fica carregado em memória**

**Local:** [`mods/CustomClasses/modded/Client/Patches/SkillsClassTabPatch.cs:36-40`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L36) (TryHide) + [`:368`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L368) (só SetActive)

**Problema:** ao trocar de aba, `ClassTabController.TryHide` só faz `_panel.SetActive(false)`. O `_window` fica com o `PlayerBody` carregado (na layer WeaponPreview) e os `UI.AddDisposable(...)` registrados no `Show` nunca são disposed (nunca chamo `Close()`). O modelo 3D permanece em memória enquanto escondido.

**Por que importa:** menu-only, então não é leak de raid — mas mantém um dollbird + rig carregado desnecessariamente enquanto o usuário está em SKILLS/MASTERING. Contraria o checklist §2 do `spt-mod-best-practices` (identificar ponto de release de cada `new GameObject`/asset).

**Sugestão:** aceitável como dívida (menu, custo baixo), **ou** em `TryHide`/`ShowRender-hide` chamar `_window.GetComponentInChildren<PlayerModelView>()?.Close()` ao esconder e recarregar ao mostrar. Trade-off: recarregar tem custo async (spinner). Recomendo **aceitar** e revisitar se a memória do menu incomodar.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-07 · D — Arquitetura · 🟡 Médio

**Injeção Harmony do `profile` por nome vs `__0` posicional (robustez entre builds do EFT)**

**Local:** [`mods/CustomClasses/modded/Client/Patches/SkillsClassTabPatch.cs:68`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L68)

**Problema:** `Postfix(SkillsAndMasteringScreen __instance, Profile profile)` injeta o 1º parâmetro **por nome**. Confirmei que os nomes estão preservados no IL atual (`Show(Profile profile, InventoryController inventoryController, IHealthController healthController)`), então **funciona hoje**. Mas se um build futuro do EFT strippar nomes de parâmetro, o Harmony falha a resolver `profile` → a aplicação do patch inteiro pode lançar → aba CLASS some por completo.

**Por que importa:** o repo prefere injeção resiliente (`__instance`/`___field`/posicional). `__0` casa por posição e sobrevive a strip de nomes.

**Sugestão:** trocar `Profile profile` por `Profile __0` (e ajustar o uso `_profile = __0;`). Um caractere de custo, mais robusto.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-08 · B — Bug latente · 🟡 Médio

**Layout aninhado (col → card → list, todos LayoutGroup) sem `ContentSizeFitter` no card → efeito longo (Bunker) pode clipar**

**Local:** [`mods/CustomClasses/modded/Client/Patches/SkillsClassTabPatch.cs:404-507`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L404)

**Problema:** o card é `HorizontalLayoutGroup`; a coluna de texto é `VerticalLayoutGroup`; ambos sem `ContentSizeFitter`. A altura do card depende da altura do `Effect` (TMP com wrap), que depende da largura resolvida da coluna — propagação largura→altura por camadas aninhadas de LayoutGroup é frágil. O `ForceRebuildLayoutImmediate` (`:223`) mitiga, mas o efeito longo do **Bunker** ("heavy weapons (LMG/HMG/GL): −recoil, +ergo; GL no ergo penalty; no arm fatigue") pode quebrar em 2-3 linhas e ser clipado se a altura não for computada corretamente na primeira passada.

**Por que importa:** o card com o texto mais longo é o mais visível do Tanque; clipping = texto cortado.

**Sugestão:** validar in-game o card do Bunker. Se clipar, adicionar `ContentSizeFitter { verticalFit = PreferredSize }` no `card` (e opcionalmente encurtar o texto do Bunker no catálogo). Achado empírico — depende do screenshot.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-09 · E — Legibilidade · 🟢 Menor

**Reflection `_progressSpinner` não cacheada em `static readonly`**

**Local:** [`mods/CustomClasses/modded/Client/Patches/SkillsClassTabPatch.cs:383`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L383)

**Problema:** `AccessTools.Field(typeof(PlayerModelView), "_progressSpinner")` é resolvido inline. Roda só 1× (guardado por `_ghostCleared`), então o impacto de perf é nulo — mas contraria o checklist §4 (`csharp-mod-best-practices`: cachear todo `FieldInfo` em `static readonly`).

**Por que importa:** consistência com o padrão do repo; trivial.

**Sugestão:** extrair para `private static readonly FieldInfo? SpinnerField = AccessTools.Field(typeof(PlayerModelView), "_progressSpinner");`. Opcional.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-10 · E — Legibilidade · 🟢 Menor

**Doc-comment da classe `SkillsClassTabPatch` não menciona render 3D nem lista de cards**

**Local:** [`mods/CustomClasses/modded/Client/Patches/SkillsClassTabPatch.cs:43-49`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L43)

**Problema:** o `<summary>` descreve só a criação da aba/toggle-group. Agora a classe também clona o `InventoryPlayerModelWithStatsWindow` (boneco 3D) e monta uma lista de cards com ícones de skill — não documentado.

**Por que importa:** o "porquê" do clone do render (prefab serializado, impossível criar do zero) é não-óbvio e merece ficar no comentário pra próxima sessão.

**Sugestão:** ampliar o summary cobrindo: (1) lista de cards via `PerksCatalog`, (2) render 3D clonado do prefab serializado, (3) captura do `profile` do `Show`.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-11 · B — Bug latente · 🟢 Menor

**Ghost-clear destrói o `_progressSpinner` se a reflection do campo falhar**

**Local:** [`mods/CustomClasses/modded/Client/Patches/SkillsClassTabPatch.cs:383-391`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L383)

**Problema:** se `AccessTools.Field(..., "_progressSpinner")` retornar null (campo renomeado num build futuro), `spinnerGo` é null e o loop destrói **todos** os filhos do `PlayerModelView`, incluindo o spinner. Depois `_window.Show` → `PlayerModelView.Show` chama `_progressSpinner.Show()` num objeto destruído → NRE (engolido pelo `.HandleExceptions()` do `method_3`, `:258` no decompilado). O modelo ainda carrega, mas sem spinner.

**Por que importa:** baixo impacto (NRE silencioso, sem spinner), mas é um caminho de degradação não-intencional se o campo mudar.

**Sugestão:** se `SpinnerField`/`spinnerGo` for null, **pular** o ghost-clear inteiro (não destruir nada) em vez de destruir tudo. `if (spinnerGo == null) { /* log + skip */ }`.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## Resolução (2026-07-01)

Todos os 11 achados endereçados na mesma sessão (fora do `/apply-code-review` automatizado — item iterativo). DLL: `CustomClasses-Client.dll` **100352 bytes**. Compile 0 erros / 0 warnings.

| ID | Decisão | O que mudou |
| --- | --- | --- |
| CR-01-01 | ✅ Aplicado | `ShowRender` revalida por `PlayerModelView.LoadingComplete` (não só profile) → cobre load cancelado e clone recriado; `TryBuildRender` zera `_lastRenderedProfile`/`_ghostCleared` no clone novo. [`SkillsClassTabPatch.cs:332-343`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L332), [`:368-402`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L368) |
| CR-01-02 | ✅ Dívida documentada | Mantido o `Show` completo (boneco+nível+nick+stats, igual ACHIEVEMENTS). `CloseAllWindows()` é inócuo na tela de Skills. Comentário explícito em [`SkillsClassTabPatch.cs:395`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L395). |
| CR-01-03 | ✅ Aplicado | Guard `_lastPanelClass` — cards só reconstroem quando a classe muda; senão só `ShowRender()`. [`SkillsClassTabPatch.cs:194-206`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L194) |
| CR-01-04 | ✅ Aplicado | Prefere instância de cena (`scene.IsValid()`) e loga a fonte + nº de candidatos. [`SkillsClassTabPatch.cs:322-349`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L322) |
| CR-01-05 | ✅ Aplicado | `Entry.IconAlt` + fallback no `IconSprite`; Bulwark→(HeavyVests→Vitality), Bunker→(LMG→RecoilControl). [`PerksCatalog.cs`](../../modded/Client/PerksCatalog.cs) |
| CR-01-06 | ✅ Dívida documentada | Boneco fica carregado escondido (menu-only, custo desprezível). Comentário em [`SkillsClassTabPatch.cs:395`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L395). |
| CR-01-07 | ✅ Aplicado | `Postfix(... Profile __0)` — injeção posicional. [`SkillsClassTabPatch.cs:68`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L68) |
| CR-01-08 | ✅ Aplicado | `ContentSizeFitter (PreferredSize)` no card → efeito longo não clipa. [`SkillsClassTabPatch.cs`](../../modded/Client/Patches/SkillsClassTabPatch.cs) |
| CR-01-09 | ✅ Aplicado | `SpinnerField` cacheada em `static readonly FieldInfo`. [`SkillsClassTabPatch.cs:56`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L56) |
| CR-01-10 | ✅ Aplicado | Doc-comment da classe ampliada (cards + render 3D + captura do profile). [`SkillsClassTabPatch.cs:43-55`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L43) |
| CR-01-11 | ✅ Aplicado | Ghost-clear pula (não destrói nada) se o spinner não for resolvido, com warning. [`SkillsClassTabPatch.cs`](../../modded/Client/Patches/SkillsClassTabPatch.cs) |

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-01 | Code review 01 criada via `/code-review` (item iterativo, sem specs formais) |
| 2026-07-01 | Todos os 11 achados endereçados (9 corrigidos, 2 dívida documentada); DLL 100352 bytes, 0/0 |
