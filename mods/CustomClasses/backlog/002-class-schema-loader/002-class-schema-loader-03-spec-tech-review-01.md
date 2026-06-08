# 002 — Schema de classe + loader multi-classe · Review Técnica 01

**Mod:** CustomClasses
**Spec técnica revisada:** [002-class-schema-loader-02-spec-tech.md](002-class-schema-loader-02-spec-tech.md)
**Data:** 2026-06-07

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 0 · 🟡 1 · 🟢 4 · ✅ Resolvidos: 5 · Pendentes: 0 · Total: 5 (todos aceitos e dobrados na spec técnica)

**Verificado (sem bloqueador):** `JsonUtil.Deserialize<T>(string)` ([ModHelper.cs:28](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/ModHelper.cs#L28)); `FileUtil.GetFiles/DirectoryExists/ReadFile/GetFileNameAndExtension` ([FileUtil.cs:11/48/63/33](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/FileUtil.cs#L11)); `TimeUtil.GetTimeStamp()` retorna `long` = `CommonSkill.LastAccess` ([TimeUtil.cs:38](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/TimeUtil.cs#L38)); `ICloner.Clone` deep ([ICloner.cs:5](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Utils/Cloners/ICloner.cs#L5)). Todas as 4 escolhas de formato (pasta, `name`, `enabled`, defaults) coerentes.

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Lógica | 🟡 | `Enum.TryParse<SkillTypes>` aceita numérico/indefinido → skill fantasma | ✅ Resolvido |
| PA-01-02 | B — Edge | 🟢 | Log `applied / 2` engana se um lado for null | ✅ Resolvido |
| PA-01-03 | B — Edge | 🟢 | `GetFiles` não-recursivo ignora subpastas (documentar) | ✅ Resolvido |
| PA-01-04 | A — Gap | 🟢 | Posicionamento do patch no `compile-mod.sh` (escopo de `SERVER_DEST`) | ✅ Resolvido |
| PA-01-05 | A — Gap | 🟢 | Premissas a confirmar no 1º build (JSONC + defaults omitidos) | ✅ Resolvido |

## Categorias

- **A — Gaps de Especificação** · **B — Edge Cases** · **C — Erros de Lógica**

## Impacto

- 🔴 **Bloqueador** · 🟡 **Importante** · 🟢 **Menor**

---

## Pontos

### PA-01-01 · C — Lógica · 🟡 Importante

**`Enum.TryParse<SkillTypes>` aceita valores numéricos/indefinidos → skill fantasma**

**Problema:** no stub de `ApplySkills`, `Enum.TryParse<SkillTypes>(skillName, true, out var skill)` retorna **`true`** para strings numéricas ou inteiros não-definidos no enum (ex.: `"999"` → `(SkillTypes)999`, `"5"` → o membro de valor 5). Um nome de skill digitado errado mas numérico passaria como skill válida e seria aplicado.

**Por que importa:** o corner case "skill inválida → ignorar" da spec funcional não é garantido para entradas numéricas; criaria um `CommonSkill` com `Id` inexistente no jogo (efeito indefinido). É um cenário plausível com JSON editado à mão.

**Sugestão:** adicionar `&& Enum.IsDefined(typeof(SkillTypes), skill)` à condição (ou checar `Enum.IsDefined` antes de usar). Trocar para:
```csharp
if (!Enum.TryParse<SkillTypes>(skillName, ignoreCase: true, out var skill) || !Enum.IsDefined(typeof(SkillTypes), skill))
{
    logger.Warning($"[CustomClasses] '{def.Name}': unknown skill '{skillName}' — ignored.");
    continue;
}
```

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

### PA-01-02 · B — Edge · 🟢 Menor

**Log `applied / 2` engana quando um lado não tem skills**

**Problema:** o log de `RegisterClass` usa `{applied / 2}` (soma USEC+BEAR ÷ 2). Se um lado tiver `Character`/`Skills` null (retorna 0), a média mente (ex.: usec=2, bear=0 → log diz "1 skill/side").

**Por que importa:** observabilidade — só ruído no log; não afeta runtime. Para "SPT Zero to hero" os dois lados existem.

**Sugestão:** logar os dois separadamente, ex.: `(usec={usecApplied}, bear={bearApplied})`, capturando os retornos de cada `ApplySkills` em variáveis (como ficou no 001 após CR-01-02). Avisar (`Warning`) se um lado retornou 0 mas `def.Skills` tinha entradas.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

### PA-01-03 · B — Edge · 🟢 Menor

**`GetFiles(..., recursive: false, ...)` ignora subpastas**

**Problema:** o loader lê só o topo de `config/classes/`. Arquivos em subpastas (ex.: `config/classes/wip/foo.jsonc`) são silenciosamente ignorados.

**Por que importa:** baixo — é até desejável (uma "lixeira" `wip/` não carrega). Mas precisa estar documentado para não surpreender.

**Sugestão:** manter não-recursivo e documentar no `README`/no comentário do loader que só o topo de `config/classes/` é lido (subpastas = ignoradas, útil para rascunhos).

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

### PA-01-04 · A — Gap · 🟢 Menor

**Patch do `compile-mod.sh`: garantir escopo de `SERVER_DEST` e existência do SPT**

**Problema:** o snippet de cópia de `config/` deve ficar **dentro** do ramo `server` do loop csharp, **após** `install_own_dlls` (onde `SERVER_DEST` é definido) e **dentro** do guard `[[ -d "$SPT_PATH" ]]`. A spec mostra o snippet mas não fixa o ponto exato.

**Por que importa:** se colocado fora do escopo, `SERVER_DEST` fica indefinido (com `set -u`, erro) ou copia sem SPT instalado.

**Sugestão:** inserir logo após a linha `SERVER_DEST_SHOWN="$SERVER_DEST"; BUILT_SERVER=1`, usando `"$SERVER_DEST"`. Confirmar que roda só quando `-d "$SPT_PATH"` (já é o caso dentro do bloco de install).

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

### PA-01-05 · A — Gap · 🟢 Menor

**Premissas a confirmar no 1º build/boot (JSONC + defaults omitidos)**

**Problema:** dois pressupostos não verificados empiricamente: (1) `JsonUtil.Deserialize` tolera comentários `.jsonc` (provável — SkillDistribution usa `config.jsonc`); (2) System.Text.Json mantém o inicializador (`Enabled=true`, etc.) quando o campo é omitido no JSON.

**Por que importa:** se (1) falhar, o `exampleClass.jsonc` (com comentários) quebra o parse; se (2) falhar, `enabled` omitido viraria `false` (classe some).

**Sugestão:** adicionar ao checklist um teste explícito: um arquivo com comentários carrega; um arquivo **sem** `enabled` carrega como habilitado. Se (1) falhar, restringir exemplos a `.json` puro + doc à parte; se (2) falhar, tornar `enabled` `bool?` e tratar `null` como `true`.

**Decisão:** `[ ]` Pendente · `[x]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

## Resolução (2026-06-07)

Todos os 5 pontos **aceitos** e dobrados na spec técnica 02 (stubs + checklist + riscos):

- **PA-01-01** ✅ — `ApplySkills` agora exige `Enum.IsDefined(typeof(SkillTypes), skill)` além do `TryParse`.
- **PA-01-02** ✅ — `RegisterClass` captura `usecApplied`/`bearApplied` separados, loga os dois e avisa se um lado retornou 0 com skills configuradas.
- **PA-01-03** ✅ — comentário no loader + nota de que só o topo de `config/classes/` é lido (subpastas ignoradas).
- **PA-01-04** ✅ — patch do `compile-mod.sh` fixado logo após `BUILT_SERVER=1`, dentro do guard de `SPT_PATH`.
- **PA-01-05** ✅ — checklist ganhou testes: arquivo com comentários carrega; arquivo sem `enabled` carrega habilitado.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Review técnica 01 criada via `/review-technical-spec` |
| 2026-06-07 | Todos os 5 pontos aceitos e dobrados na spec técnica |
