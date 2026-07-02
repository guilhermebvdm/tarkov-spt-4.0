# 053 — Perks/Drawbacks UI · Code Review 03 (layout/abas + polish premium)

**Mod:** CustomClasses
**Escopo:** 2 batches recentes em [`SkillsClassTabPatch.cs`](../../modded/Client/Patches/SkillsClassTabPatch.cs) — (1) layout/abas (CLASS default, título nas 2 versões, ícone da aba, seções, header com brasão, retry lazy do render 3D, fix do espaçamento) e (2) polish premium (hover, divisória, marca d'água, frame slot, chips de valor, fade-in) + nova ref `UnityEngine.UIModule` no csproj.
**Data:** 2026-07-01

> Foco: vazamento de estado/eventos, lifecycle de MonoBehaviour, correção do layout UGUI e reprodutibilidade do build. Complementa CR-01 (qualidade) e CR-02 (negócio).

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 1 · 🟡 Médios: 1 · 🟢 Menores: 4 · ✅ Resolvidos: 6 · Total: 6

Batch de UI, sem impacto em gameplay/coop. **Todos os 6 aplicados (2026-07-01).** Detalhe na seção **Resolução**.

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-03-02 | D — Arquitetura | 🟠 Forte | `UnityEngine.UIModule.dll` gitignored + fora do auto-copy do compile-mod → build quebra em checkout limpo | ✅ Aplicado |
| CR-03-01 | B — Bug latente | 🟡 Médio | Ícone do header + marca d'água viram **caixa branca** p/ classe sem sprite (Image sem sprite) | ✅ Aplicado |
| CR-03-03 | F — Melhoria | 🟢 Menor | `FadeIn.Update` roda todo frame após o fade terminar (não se auto-desliga) | ✅ Aplicado |
| CR-03-04 | E — Legibilidade | 🟢 Menor | `StyleClassTab` reaplica ícone/texto em toda seleção (redundante); `_loggedTabImages` nunca reseta | ✅ Aplicado |
| CR-03-05 | B — Bug latente | 🟢 Menor | Reabertura depende do `Show(null)` nativo restaurar `Tab_2` — robusto hoje, frágil a mudança do EFT | ✅ Documentado |
| CR-03-06 | E — Legibilidade | 🟢 Menor | Chips do `PillifyValues`: espaço duplo + quebra de linha no meio do chip (cosmético) | ✅ Aplicado |

## Categorias / Impacto

_(idênticas à Code Review 01)_

---

## Pontos

### CR-03-02 · D — Arquitetura · 🟠 Forte

**A referência nova `UnityEngine.UIModule.dll` é gitignored e não é auto-copiada pelo compile-mod → build quebra em checkout limpo / sessão paralela**

**Local:** [`mods/CustomClasses/modded/Client/CustomClasses.Client.csproj`](../../modded/Client/CustomClasses.Client.csproj) (nova `<Reference Include="UnityEngine.UIModule">`) + `.agents/scripts/compile-mod.sh` (lista de auto-copy)

**Problema:** o `CanvasGroup` (idéia 6, fade-in) vive em `UnityEngine.UIModule.dll`, que não estava referenciado. Adicionei a referência (HintPath `References/UnityEngine.UIModule.dll`) e **copiei a DLL manualmente**. Mas:
- `git check-ignore` confirma que `References/*.dll` é **gitignored** → a DLL **não commita**.
- O `compile-mod.sh` auto-copia várias `UnityEngine*` (CoreModule, IMGUIModule…) do install, mas **não** `UnityEngine.UIModule.dll`.

Logo, em qualquer máquina sem essa DLL local (checkout limpo, CI, **a sessão paralela do editor** citada na memória do mod), o MSBuild falha com "não encontrei UnityEngine.UIModule".

**Por que importa:** quebra o build pra qualquer outro contexto — exatamente o risco que a memória do mod alerta (coordenação multi-chat / `modded/`).

**Sugestão:** adicionar `UnityEngine.UIModule.dll` à lista de auto-copy do `compile-mod.sh` (ao lado de `UnityEngine.IMGUIModule.dll`), pra ser buscada do install como as outras:
```bash
"UnityEngine.UIModule.dll|$spt/EscapeFromTarkov_Data/Managed/UnityEngine.UIModule.dll"
```
Assim o build se auto-resolve sem depender de cópia manual nem de commit da DLL.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar: _________________

---

### CR-03-01 · B — Bug latente · 🟡 Médio

**Ícone do header e marca d'água renderizam uma caixa branca quando a classe não tem sprite**

**Local:** [`SkillsClassTabPatch.cs:368-372`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L368) (header icon) + [`:313-324`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L313) (watermark) + [`:224`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L224) / [`:234`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L234) (apply)

**Problema:** um `Image` **sem sprite** desenha um quad branco. O header icon e a watermark são criados sem sprite; o sprite só é setado no `RefreshPanel`:
- `ApplyClassIcon(headerIcon, ...)` faz `return` cedo se `GetTinted` devolver null — **sem tocar no Image** → fica quad branco.
- A watermark só recebe sprite `if (wmSprite != null)` — senão fica um quad branco 460×460 em alpha 0.05.

Para classe **vanilla** (ou `IconFile` ausente), aparecem **dois retângulos brancos** no painel.

**Por que importa:** artefato visual feio para qualquer classe sem ícone. As classes do mod têm ícone (ok), mas vanilla/edge não.

**Sugestão:** criar ambos os GameObjects **inativos** por padrão; ativar só quando o sprite for aplicado. `ApplyClassIcon` já faz `SetActive(true)` no sucesso (`ClassIdentityView.cs:100-103`), então basta `hicon.SetActive(false)` no BuildPanel. Para a watermark, `wm.SetActive(false)` no BuildPanel e `wm.SetActive(true)` no RefreshPanel só quando `wmSprite != null`.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar: _________________

---

### CR-03-03 · F — Melhoria · 🟢 Menor

**`FadeIn.Update` continua rodando todo frame depois do fade terminar**

**Local:** [`SkillsClassTabPatch.cs:815-826`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L815)

**Problema:** após `alpha >= 1f`, o `Update` só faz um early-return, mas continua sendo chamado a cada frame enquanto o painel estiver ativo. Custo é ínfimo, mas é trabalho ocioso perpétuo.

**Por que importa:** micro-desperdício; boa prática desligar o componente quando o trabalho acaba.

**Sugestão:** ao completar (`_cg.alpha >= 1f`), `enabled = false;` (o `OnEnable` reativa a lógica no próximo Show, mas `enabled=false` só para o `Update` — então usar um flag `_done` e resetar no `OnEnable`, ou `enabled=false` + reativar via SetActive do painel que dispara OnEnable). Simplest: `if (_cg.alpha >= 1f) { enabled = false; return; }` e em `OnEnable` fazer `enabled = true`.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar: _________________

---

### CR-03-04 · E — Legibilidade · 🟢 Menor

**`StyleClassTab` reaplica ícone+texto a cada `OnSelectionChanged` (redundante); `_loggedTabImages` nunca reseta**

**Local:** [`SkillsClassTabPatch.cs:133`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L133) (subscribe) + `StyleClassTab`

**Problema:** `StyleClassTab` roda 3× no setup (inicial, pós-seleção, e no handler de `OnSelectionChanged`), e depois a cada clique na aba. Reaplica o mesmo sprite/texto — idempotente, mas redundante. Foi feito de propósito (o selected vinha sem texto), então é aceitável — vale um comentário deixando claro que o custo é intencional. `_loggedTabImages` (diagnóstico) nunca reseta → num screen novo o log não re-dispara (ok, é só diagnóstico).

**Por que importa:** clareza; o reaplicar-sempre pode confundir quem lê depois.

**Sugestão:** manter (a robustez compensa), mas comentar que o re-apply cobre o reset-on-activate do selected. Opcional: guardar o último sprite aplicado e pular se igual.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar: _________________

---

### CR-03-05 · B — Bug latente · 🟢 Menor

**A reabertura da tela (screen pooled) depende do `gclass3808_0.Show(null)` nativo restaurar `Tab_2`**

**Local:** [`SkillsClassTabPatch.cs:80-83`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L80) (idempotência) · evidência: `SkillsAndMasteringScreen.Show` termina em `gclass3808_0.Show(null)` (Assembly decompilado)

**Problema:** o group é criado no `Awake` (1×) e meu Postfix o substitui pelo group de 3 tabs — persiste entre aberturas (pooled). Na reabertura, minha idempotência retorna cedo (a aba CLASS já existe) e **não** re-normaliza a seleção; quem seleciona é o `Show(null)` nativo → `SelectTab(Tab_2)` (restaura a **última** aba). Como `Show(null)` restaura `Tab_2` (não força SKILLS), não há double-select. **Mas** se um build futuro do EFT trocar `Show(null)` por `Show(_skillsTab)`, a reabertura voltaria a dar double-select (CLASS visual antigo + SKILLS forçado).

**Por que importa:** hoje funciona; é uma dependência implícita no comportamento do `Show(null)`.

**Sugestão:** aceitar como está (funciona) e **documentar** a dependência num comentário na idempotência. Se quiser blindar: mover a normalização de seleção (deselecionar as não-CLASS quando CLASS é `Tab_2`) pra rodar em todo Show, fora do guard de build.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar: _________________

---

### CR-03-06 · E — Legibilidade · 🟢 Menor

**Chips do `PillifyValues`: espaço duplo antes do chip e possível quebra de linha no meio**

**Local:** [`SkillsClassTabPatch.cs:767-773`](../../modded/Client/Patches/SkillsClassTabPatch.cs#L767)

**Problema:** o chip é `<mark=#ffffff14> <b>...</b> </mark>` com espaços internos de padding. Somados ao espaço original do texto ("damage: <space><mark><space>…"), pode dar espaço duplo visual; e um chip perto da borda pode quebrar linha no meio do `<mark>`. Cosmético.

**Por que importa:** polish; nada funcional.

**Sugestão:** usar ` ` (no-break space) dentro do mark em vez de espaço normal, e não deixar espaço antes do chip (o regex substitui só o token). Opcional.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar: _________________

---

## Resolução (2026-07-01)

DLL `CustomClasses-Client.dll` **105984 bytes** · compile 0/0.

| ID | Decisão | O que mudou |
| --- | --- | --- |
| CR-03-02 | ✅ Aplicado | `UnityEngine.UIModule.dll` adicionado ao auto-copy do [`.agents/scripts/compile-mod.sh`](../../../../.agents/scripts/compile-mod.sh) (ao lado do UI.dll) → build se auto-resolve sem depender de cópia manual / commit da DLL. |
| CR-03-01 | ✅ Aplicado | Header icon (`hicon.SetActive(false)`) e watermark (`wm.SetActive(false)`) nascem inativos; `ApplyClassIcon` reativa o header no sucesso e o RefreshPanel ativa a watermark só quando há brasão → fim dos quads brancos. [`SkillsClassTabPatch.cs`](../../modded/Client/Patches/SkillsClassTabPatch.cs) |
| CR-03-03 | ✅ Aplicado | `FadeIn.Update` faz `enabled=false` ao terminar; `Restart()` chamado pelo RefreshPanel re-dispara o fade em cada exibição. |
| CR-03-04 | ✅ Aplicado | `StyleClassTab` pula o sprite já aplicado (`if img.sprite == sprite continue`) + comentário explicando o re-apply intencional. |
| CR-03-05 | ✅ Documentado | Comentário na idempotência explicando a dependência do `Show(null)` nativo (restaura `Tab_2`) e o que revisitar se o EFT mudar. |
| CR-03-06 | ✅ Aplicado | Chips do `PillifyValues` com no-break space (padding sem espaço-duplo/quebra). |

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-01 | Code review 03 criada — 2 batches de UI (layout/abas + polish premium) |
| 2026-07-01 | CR-03-02 (build) e CR-03-01 (caixa branca) aplicados; 4× 🟢 deixados abertos (opcionais); DLL 105984 bytes |
| 2026-07-01 | CR-03-03/04/05/06 aplicados (todos os 6 fechados); DLL 105984 bytes, 0/0 |
