# 005 — Multiplicadores de skill · Review Técnica 01

**Mod:** CustomClasses
**Spec técnica:** [005-skill-multipliers-02-spec-tech.md](005-skill-multipliers-02-spec-tech.md)
**Data:** 2026-06-07

> Hook de XP confirmado (`AbstractSkillClass.cs:100`). Os pontos abaixo são os **TODO confirmar** da spec + análise. IDs `PA-01-MM`.

## Resumo

> 🔴 1 · 🟡 3 · 🟢 1 · Total: 5 · ✅ **TODOS resolvidos** — PA-01-02/05 (Fatia 1a), PA-01-03/04 (Fatia 1b), **PA-01-01 (UI/Fatia 2: SkillPanel/SkillTooltip são tipos nomeados em EFT.UI — não ofuscados; dump parcial. Implementado).**

**Recomendação:** **fatiar** — Fatia 1 = server (registry+router) + client XP scaling (`OnTrigger`), que é determinística e testável sozinha; Fatia 2 = UI (`SkillPanel`/`SkillTooltip`), que depende de confirmar classes ofuscadas. Só PA-01-01 (UI) é bloqueador, e só da Fatia 2.

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | A — Gap | 🔴 | Classes de UI (`SkillPanel`/`SkillTooltip`) não confirmadas no decompilado | Pendente |
| PA-01-02 | A — Gap | 🟡 | APIs do server (StaticRouter/RouteAction/EmptyRequestData + Edition por sessionId) não verificadas | Pendente |
| PA-01-03 | C — Lógica | 🟡 | `ESkillId` (client) × nomes do JSON (`SkillTypes` server) podem divergir | Pendente |
| PA-01-04 | A — Gap | 🟡 | Hook de "config pronta" no client não definido (SkillDistribution usa hook ofuscado) | Pendente |
| PA-01-05 | C — Lógica | 🟢 | `InjectionType.Singleton` não confirmado na SPTarkov.DI | Pendente |

## Categorias

- **A — Gaps** · **B — Edge Cases** · **C — Erros de Lógica**

## Impacto

- 🔴 **Bloqueador** · 🟡 **Importante** · 🟢 **Menor**

---

### PA-01-01 · A — Gap · 🔴 Bloqueador (só da Fatia 2 / UI)

**Classes de UI `SkillPanel`/`SkillTooltip` não confirmadas no decompilado**

**Problema:** a spec (§2/§5) marca `SkillPanel.method_1()` e `SkillTooltip.Show()` como **TODO confirmar** — estão ofuscadas e o Explore não achou arquivo:linha. Sem o nome real da classe, do método, do **campo que guarda a skill** e do **elemento de texto** (pra anexar `+X%/−X%`), a UI não dá pra implementar.

**Por que importa:** a parte de UI (CA "mostrar na tela de Skills") fica impossível de codar com segurança; chutar quebra em runtime.

**Sugestão:** antes da Fatia 2, **ler o decompilado** localizando a classe da **linha de skill** e do **tooltip** (usar `mods/SkillDistribution/original/.../SkillPanelPatch.cs` e `SkillTooltipPatch.cs` como guia de QUAIS classes/métodos eles miram — `____effectivenessUp/Down`, `SkillTooltip.Show(SkillClass)`), e confirmar: tipo + método + campo da skill + onde injetar o texto. Reimplementar (não copiar). **Implementar a Fatia 1 (server+XP) primeiro** — não depende disto.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar (fatiar; confirmar UI antes da Fatia 2) · `[ ]` Caminho alternativo: _______

---

### PA-01-02 · A — Gap · 🟡 Importante

**APIs do server (StaticRouter/RouteAction/EmptyRequestData + Edition por sessionId) não verificadas**

**Problema:** o stub do `SkillMultipliersRouter` assume `StaticRouter`/`RouteAction<EmptyRequestData>` ([Router.cs:66/184]) e `saveServer.GetProfile(sessionId).ProfileInfo.Edition`, mas marca **TODO confirmar** o namespace de `EmptyRequestData`, a assinatura exata do `RouteAction<T>` e como obter a `Edition`.

**Por que importa:** o lado server não compila/responde se as assinaturas estiverem erradas.

**Sugestão:** ao codar a Fatia 1, **abrir** `references/spt-source/.../DI/Router.cs` (confirmar `StaticRouter` ctor + `RouteAction<T>`), achar **um StaticRouter existente** no spt-source como molde, e confirmar `Edition` em `SaveServer`/`ProfileHelper`/`CreateProfileService` (já vimos `account.ProfileInfo.Edition` no `CreateProfileService.cs:44`). Ajustar o stub.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

### PA-01-03 · C — Lógica · 🟡 Importante

**`ESkillId` (client) pode não bater com os nomes de skill do JSON (`SkillTypes` server)**

**Problema:** o JSON usa nomes parseados como `SkillTypes` (server, item 002). O client casa pelo `__instance.Id` (`ESkillId`, [AbstractSkillClass.cs:14](../../../../references/eft-decompiled/Assembly-CSharp/AbstractSkillClass.cs#L14)). Se os nomes dos dois enums divergirem em alguma skill, o multiplicador daquela skill **falha silenciosamente**.

**Por que importa:** multiplicador some sem erro óbvio para skills com nome divergente.

**Sugestão:** ao codar o client, **mapear por nome case-insensitive** (`Enum.TryParse<ESkillId>(skillName, true, ...)`) e **logar (Debug)** os nomes não-mapeados. Na Fatia 1, conferir uma amostra (Endurance, Strength, Vitality, Surgery, RecoilControl, Metabolism) entre `ESkillId` e `SkillTypes` p/ pegar divergências cedo.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

### PA-01-04 · A — Gap · 🟡 Importante

**Hook de "config pronta" no client não definido (evitar o hook ofuscado do SkillDistribution)**

**Problema:** a spec cita `ProfileReadyPatch` mas o SkillDistribution usa um hook **ofuscado** (`Class308.Class1596.method_0`) pra recarregar na seleção de perfil. A spec não fixa um hook estável. XP fora-de-raid (hideout/menu) exige a config **antes** da raid.

**Por que importa:** se a config só carregar em `OnGameStarted` (raid), o ganho no menu/hideout não escala; se depender de hook ofuscado, quebra entre patches do EFT.

**Sugestão:** usar um hook **estável**: buscar a config quando o `RequestHandler`/perfil estiver disponível — ex.: patch num ponto estável de carregamento do perfil/menu, **ou** Awake + fetch com retry quando a sessão existir, **ou** `GameWorld.OnGameStarted` (já confirmado :2584) como gatilho mínimo + re-fetch ao abrir a tela de Skills. Definir 1 caminho no `/code-mod` e documentar. Cache limpo na troca de perfil.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

### PA-01-05 · C — Lógica · 🟢 Menor

**`InjectionType.Singleton` não confirmado**

**Problema:** o stub do `SkillMultiplierRegistry` usa `[Injectable(InjectionType.Singleton)]`; não confirmei que esse enum/overload existe na `SPTarkov.DI`.

**Por que importa:** baixo — não compila se a sintaxe estiver errada, mas é trivial de ajustar.

**Sugestão:** confirmar o overload de `[Injectable]` p/ singleton no `references/spt-source` (DI Annotations). Se o default já for singleton, usar `[Injectable]` simples. O registry precisa ser a **mesma instância** vista pelo loader e pelo router.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

## Resolução (2026-06-07) — Fatia 1a (server)

- **PA-01-02** ✅ — StaticRouter/`RouteAction<EmptyRequestData>` (em `...Models.Eft.Common`) + `SaveServer.GetProfile(MongoId)→SptProfile.ProfileInfo.Edition` confirmados (SkillDistribution + spt-source). Router implementado.
- **PA-01-05** ✅ — `[Injectable]` default = **Scoped** (Injectable.cs:7) → registry usa `InjectionType.Singleton`.
- **PA-01-01** ⏭️ — UI ofuscada deferida p/ **Fatia 2** (não bloqueia server+XP).
- **PA-01-03** ⬜ — `ESkillId` × nomes: resolver no client (Fatia 1b) com `Enum.TryParse<ESkillId>(name, true)` + log dos não-mapeados.
- **PA-01-04** ⬜ — hook de config pronta: definir no client (Fatia 1b).

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Review técnica 01 criada via `/review-technical-spec` |
| 2026-06-07 | PA-01-02/05 resolvidos na Fatia 1a (server); 01→Fatia 2, 03/04→Fatia 1b |
