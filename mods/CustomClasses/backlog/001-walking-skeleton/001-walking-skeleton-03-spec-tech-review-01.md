# 001 — Scaffold + 1 classe (walking skeleton) · Review Técnica 01

**Mod:** CustomClasses
**Spec técnica revisada:** [001-walking-skeleton-02-spec-tech.md](001-walking-skeleton-02-spec-tech.md)
**Data:** 2026-06-07

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 2 · 🟢 Menores: 4 · ✅ Resolvidos: 6 · Pendentes: 0 · Total: 6

**Verificações que passaram** (sem bloqueador): assinaturas batem com o `spt-source` — `IOnLoad.OnLoad()` retorna `Task` ([IOnLoad.cs:5](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/DI/IOnLoad.cs#L5)); `ICloner.Clone<T>(T? obj)` retorna `T?` ([ICloner.cs:5](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/Cloners/ICloner.cs#L5)); todas as linhas citadas na §2 conferidas. Estratégia (injeção em `GetProfileTemplates` no `PostDBModLoader+1`) é o caminho canônico que o launcher lê.

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | A — Gap | 🟡 | Fallback da edition base inconsistente (§7 vs stub) + confirmar chave | ✅ Resolvido |
| PA-01-02 | A — Gap | 🟡 | Versão do pacote `SPTarkov.*` + `SemanticVersioning` (pré-build) | ✅ Resolvido |
| PA-01-03 | C — Lógica | 🟢 | `cloner.Clone` retorna `T?` — deref sem guarda (nullable) | ✅ Resolvido |
| PA-01-04 | B — Edge | 🟢 | Launcher com server locale ≠ en mostra descrição em inglês | ✅ Resolvido |
| PA-01-05 | B — Edge | 🟢 | Confirmar v1/v2 do launcher + validação empírica (playtest) | ✅ Resolvido |
| PA-01-06 | A — Gap | 🟢 | Comportamento de remoção do mod não explicitado | ✅ Resolvido |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-01-01 · A — Gap · 🟡 Importante

**Fallback da edition base inconsistente (§7 promete, stub aborta) + chave não confirmada**

**Problema:** A §7 afirma "se ausente, fallback para a primeira chave disponível", mas o stub da §5 (`CustomClassesMod.cs`) apenas loga erro e retorna (`!templates.TryGetValue(BaseEditionKey, ...)` → `return Task.CompletedTask`), **sem** implementar fallback. Além disso, a chave `"Standard"` é uma suposição (doc em [ProfileHelper.cs:801](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/ProfileHelper.cs#L801) cita "Standard" só como exemplo) — não foi confirmada contra o DB do SPT 4.0.13 instalado.

**Por que importa:** Se a chave estiver errada/ausente, a edition **não é registrada** (falha silenciosa com log) e todos os critérios de aceite falham. A divergência texto-vs-código confunde o `/code-mod`.

**Sugestão:** (1) **Confirmar a chave exata** antes do `/code-mod` — listar `databaseService.GetProfileTemplates().Keys` num log temporário no primeiro boot, ou inspecionar o `SPT_Data` do install. (2) **Reconciliar:** trocar a §7 para "aborta com log claro (sem fallback)" — caminho mais simples e suficiente para o walking skeleton, já que "Standard" deve existir. (Alternativa: implementar de fato o fallback `templates.Keys.First()` no stub.)

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (confirmar chave + §7 = abortar sem fallback)
- `[ ]` Caminho alternativo: _________________

---

### PA-01-02 · A — Gap · 🟡 Importante

**Versão do pacote `SPTarkov.*` e disponibilidade de `SemanticVersioning` (pré-build)**

**Problema:** O `.csproj` da §5 fixa `SPTarkov.Server.Core/DI/Common` em `4.0.0` com um `TODO confirmar`. SkillDistribution usa `4.0.0`, Skills-Extended usa `4.0.2`. O `CustomClassesMetadata` usa `SemanticVersioning.Version`/`Range` sem PackageReference explícito (depende de virem transitivos via SPTarkov.*).

**Por que importa:** Versão incompatível com o SPT 4.0.13 instalado faz o `dotnet restore`/runtime falhar; se `SemanticVersioning` não vier transitivo, o `CustomClassesMetadata` não compila.

**Sugestão:** Pinar os pacotes na versão que casa com o SPT 4.0.13 (começar com `4.0.0` como SkillDistribution; se `restore`/load falhar, subir para `4.0.2`). Confirmar no primeiro build que `SemanticVersioning` resolve transitivamente; se não, adicionar `PackageReference` explícito. Resolver no `/code-mod` (é prerequisito de build, falha de forma clara — não é bug silencioso).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-01-03 · C — Lógica · 🟢 Menor

**`cloner.Clone` retorna `T?` — deref de `sides` sem guarda**

**Problema:** [`ICloner.Clone<T>(T? obj)`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/Cloners/ICloner.cs#L5) retorna `T?`. No stub, `var sides = cloner.Clone(baseSides);` seguido de `sides.DescriptionLocaleKey = ...` deref um `ProfileSides?` → aviso CS8602 sob `<Nullable>enable</Nullable>` (csharp-best-practices §4).

**Por que importa:** Em runtime `baseSides` é não-nulo (checado antes), mas o aviso é ruído e o código fica menos defensivo se `Clone` retornar null por outra razão.

**Sugestão:** Após o clone, adicionar `if (sides is null) { logger.Error(...); return Task.CompletedTask; }` (também fecha o flow de nullability), ou usar `!`. Resolver no `/code-mod`.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-01-04 · B — Edge · 🟢 Menor

**Server locale ≠ en mostra a descrição da classe em inglês (mistura no launcher)**

**Problema:** Como a descrição usa o texto inglês literal como `DescriptionLocaleKey` (resolvido pelo fallback `return value ?? key` em [ServerLocalisationService.cs:163](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/ServerLocalisationService.cs#L163)), se o server estiver configurado em pt as edições nativas aparecem em pt e a nossa em inglês.

**Por que importa:** Inconsistência visual no launcher quando o usuário roda o server em pt. É **esperado e aceito** no escopo do 001 (en-only), mas precisa ficar registrado para não ser tratado como bug depois.

**Sugestão:** Documentar explicitamente na §7 como limitação conhecida resolvida no item 008 (locale keys reais por idioma). Sem mudança de código no 001.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-01-05 · B — Edge · 🟢 Menor

**Confirmar v1/v2 do launcher + validação empírica obrigatória**

**Problema:** Há dois controllers de launcher; o 001 baseia-se no `LauncherController`. Já verifiquei que **ambos** leem `GetProfileTemplates()` ([LauncherController.cs:39](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/LauncherController.cs#L39), [LauncherV2Controller.cs:45](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/LauncherV2Controller.cs#L45)) — então a injeção surge independentemente da versão. Mas isso só foi verificado no código, não in-game.

**Por que importa:** O mecanismo central nunca foi exercido in-game neste repo via este caminho. Memória do projeto: escrita em arquivos do SPT exige validação no jogo, não só write+hash.

**Sugestão:** Manter o checklist §8 com playtest real (launcher + criar perfil + conferir skills). Não há mudança de spec; é um lembrete de que o aceite do 001 depende de playtest, não só de build OK.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-01-06 · A — Gap · 🟢 Menor

**Comportamento de remoção do mod não explicitado**

**Problema:** A spec funcional exige "remover o mod → launcher volta às nativas", mas a spec técnica não diz como isso acontece.

**Por que importa:** Sem registrar, parece um requisito não endereçado.

**Sugestão:** Adicionar 1 linha na §6/§7: a injeção é **em memória no `OnLoad`** (não persiste em disco); remover/desabilitar o DLL faz o `OnLoad` não rodar → a edition simplesmente não é adicionada no próximo boot. Sem cleanup necessário. Sem mudança de código.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

## Resolução (2026-06-07)

Todos os pontos resolvidos; spec técnica 02 atualizada conforme.

- **PA-01-01** ✅ Caminho alternativo (refinamento do usuário): base = `"Standard"` para todas as classes, **mas como variável por classe** — campo `BaseEdition` na definição da classe (no 001 hardcoded = "Standard"; vira campo do JSON no item 002). Se a chave base não existir → **abortar com log claro** (sem fallback). Confirmar a chave no 1º boot fica no checklist.
- **PA-01-02** ✅ Aceitar: pinar `SPTarkov.* 4.0.0` (subir p/ 4.0.2 se restore/load falhar); confirmar `SemanticVersioning` transitivo no 1º build.
- **PA-01-03** ✅ Aceitar: guarda `if (sides is null) { log; return; }` após o `Clone`.
- **PA-01-04** ✅ Aceitar: documentado na §7 como limitação conhecida (resolvida no item 008).
- **PA-01-05** ✅ Aceitar: v1/v2 ambos leem `GetProfileTemplates` (confirmado); aceite depende de playtest (checklist §8).
- **PA-01-06** ✅ Aceitar: §7 explicita injeção em memória no `OnLoad`; remover o DLL → edition some no próximo boot, sem cleanup.
