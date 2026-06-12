# 021 — ClassRegistrar (dry-run) + ClassEditorService · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-09 · **Origem:** plano aprovado do editor web de classes (`~/.claude/plans/`, sessão 2026-06-09; renumerado 020→021)
**Wave:** W1 (paralelo a 019/022 desde o início) · **Deps:** 018 (soft — pode ler `ClassDefinition.cs` direto)

> Brief de kickoff — insumo para `/create-spec 021`. Não é a spec.

## Objetivo

O coração do editor: um pipeline de registro **reutilizável e sem efeito colateral na validação**, e o serviço de load/save dos `.jsonc`. Editor e boot usam o MESMO pipeline → paridade de validação por construção.

## Escopo

- **`ClassRegistrar`** extraído de `CustomClassesMod.RegisterClass` (hoje mistura validação e commit):
  - `Validate/Build(def)` → `(sides?, List<Diagnostic>)` — **puro** (dry-run real: nada de mutar templates/registries; diagnósticos estruturados em vez de só logs).
  - `Commit(name, sides, def)` / `Remove(name)` — muta `templates` + `SkillMultiplierRegistry` + `ClassVisualRegistry`. **Adicionar `Remove` e enumeração aos dois registries** (hoje não existem).
  - `OnLoad` passa a usar o pipeline — **comportamento do boot idêntico** (mesmos logs/contagens).
- **`ClassEditorService`:** lista/lê/escreve os `.jsonc` da pasta do próprio mod no install (via `ModHelper.GetAbsolutePathToModFolder`), com backup `.bak` rotativo + audit log; save = validar → backup → escrever → hot-apply.
- **Hot-apply:** viável — `CreateProfileService` lê o dict de templates a cada criação de perfil; commit **atômico build-then-swap** (montar `sides` completo antes de atribuir a entry). Não dá pra interceptar leitores do SPT — risco residual de concorrência **aceito** (server local, single-user). `enabled:false`/delete → `Remove`.
- **Round-trip perde comentários** dos `.jsonc` (reserialização) — decisão documentada; `.bak` preserva o último estado manual.
- **Driver de teste** (não existe UI ainda): definir na spec — página smoke do 020, rota temporária ou chamada manual documentada.

## Riscos / atenção

- Regressão de boot é o risco nº 1 — refactor deve ser comportamento-preservante (comparar log de boot antes/depois).
- Locks/concorrência: não prometer mais que build-then-swap (ver plano).

## Refs

- [modded/Server/CustomClassesMod.cs](../../modded/Server/CustomClassesMod.cs) — `RegisterClass`/`ApplySkills` a extrair
- [modded/Server/SkillMultiplierRegistry.cs](../../modded/Server/SkillMultiplierRegistry.cs), [ClassVisualRegistry.cs](../../modded/Server/ClassVisualRegistry.cs) — ganham Remove/enumeração
- `references/spt-source` — `CreateProfileService.CreateProfile` / `ProfileHelper.GetProfileTemplateForSide` (leitura viva do dict)
- Doc do 018 (`docs/class-schema.md`) — regras de validação

## DoD (resumo)

- Boot inalterado (11 classes, mesmos logs).
- Dry-run de classe inválida retorna diagnósticos sem tocar templates/registries.
- Save + hot-apply reflete em perfil novo **sem reiniciar**; remoção tira a edition do launcher.
