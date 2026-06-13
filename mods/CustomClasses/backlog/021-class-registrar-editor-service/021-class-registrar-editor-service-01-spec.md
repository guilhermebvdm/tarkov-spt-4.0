# 021 — ClassRegistrar (dry-run) + ClassEditorService · Spec funcional

**Mod:** CustomClasses · **Data:** 2026-06-10 · **Kickoff:** [00-kickoff](021-class-registrar-editor-service-00-kickoff.md)

## Objetivo

Extrair de `CustomClassesMod.RegisterClass` um pipeline de registro reutilizável em fases (validar/construir → commitar → remover), e criar o serviço de load/save dos `.jsonc` de classe. Boot e editor web (024+) passam a usar o MESMO pipeline — paridade de validação por construção.

## Critérios de aceite

1. **Boot inalterado:** mesmas mensagens de log, mesmas contagens (`Loaded N class(es), skipped M`), mesmo comportamento de skip por arquivo. O log `Registered '<name>' (base ..., skills usec=.../bear=..., items ..., hideout=..., outfit ..., skillMults=...) from '<file>'.` continua saindo idêntico.
2. **Dry-run puro:** `ClassRegistrar.ValidateAndBuild` roda TODAS as validações do loader (name vazio, colisão de edition, baseEdition inexistente, clone null — regras canônicas em [docs/class-schema.md](../../docs/class-schema.md) §5) e constrói o `sides` completo SEM tocar templates/registries. Classe inválida → diagnostics estruturados (`ClassDiagnostic` com Severity Error/Warning/Info + Code estável), templates/registries intactos.
3. **Replace controlado:** o check de colisão aceita `allowReplace` — o editor re-salva uma classe existente do próprio mod sem falso-positivo; colisão com edition vanilla/outro mod é SEMPRE erro; no boot `allowReplace=false`.
4. **Commit build-then-swap:** `Commit(plan)` atribui o `sides` completo numa única escrita de referência (`templates[name] = sides`) + atualiza os dois registries. `Remove(name)` tira a edition dos templates E dos registries (some do launcher para perfis novos), recusando editions que não são do mod.
5. **Registries enumeráveis:** `SkillMultiplierRegistry` e `ClassVisualRegistry` ganham `Remove(edition)` e `Editions` (enumeração), API atual intacta.
6. **Editor service:** `ClassEditorService` opera sobre `config/classes/` do mod instalado: `ListClassFiles` (varredura não-recursiva `*.json|*.jsonc`, parse + dry-run + flags enabled/registered), `Load`, `Save` (validar → erro bloqueia SEM escrever → backup rotativo `.bak1..bak3` → JSON indentado → hot-apply opcional → audit log) e `Delete` (backup → apagar → hot-remove opcional).
7. **Hot-apply:** save com hot-apply reflete em perfil novo sem reiniciar o servidor (CreateProfileService lê o dict vivo); `enabled:false`/delete com hot → `Remove`.
8. **Audit:** `config/classes/_audit.log` (extensão fora dos globs do loader) com timestamp UTC, arquivo, ação e resumo por save/delete.

## Decisões / limites aceitos

- **Comentários `.jsonc` são perdidos no save** (reserialização DTO→JSON); `.bak1` preserva o último estado manual.
- **Rename é responsabilidade do caller:** se o `name` salvo difere do que o arquivo continha, a edition antiga continua registrada até o caller chamar `Remove`.
- **Concorrência aceita:** dicts simples sem lock (server local, single-user); build-then-swap garante que leitores nunca veem estado meio-mutado.
- **Warnings dos builders** (loadout/hideout/outfit) seguem indo direto pro log do servidor, NÃO viram diagnostics neste item (builders não são refatorados).

## Fora de escopo

UI (024+), rotas HTTP, CatalogService/CostService (022), infra web (020), refator dos builders.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-10 | Spec escrita junto da implementação (item executado em paralelo a 020/022). |
