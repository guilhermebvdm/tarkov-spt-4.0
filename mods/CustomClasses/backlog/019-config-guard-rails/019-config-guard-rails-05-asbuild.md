# 019 — Guard rails de config (anti-clobber + sync) · As-Built

**Mod:** CustomClasses · **Build:** 2026-06-10 · **Spec:** [01-spec](019-config-guard-rails-01-spec.md)

## Arquivos

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `.agents/scripts/compile-mod.sh` | (a) cópia de `config/` no install server-csharp agora é **guardada**: compara `config/classes/` install×repo com normalização (CRLF/trailing-ws); divergência → aborta a cópia de config com lista por arquivo + lado mais novo (mtime) + instruções; novo flag `--force-config` sobrescreve. Arquivos repo-only copiam sem bloquear. DLL/resto instala normalmente. (d) novo passo: copia `wwwroot/` repo→install (clobber intencional, `rm -rf` + cópia — órfãos removidos). |
| CRIADO | `mods/CustomClasses/scripts/sync-classes.sh` (118 linhas) | Sync INSTALL→REPO de `config/classes/*.json[c]` + `config/*.jsonc` raiz. Diff preview por arquivo, `--dry-run`, `--yes` (não-interativo sem `--yes` aborta), `--spt-path` (default `SPT_PATH`/`D:/SPT`, mesma convenção do compile-mod). Mesma normalização do guard. Avisa edições não-commitadas no repo antes de sobrescrever. |
| CRIADO | `.claude/commands/sync-classes.md` | Skill `/sync-classes`: roda o script e orienta commit em seguida. |
| MODIFICADO | `mods/CustomClasses/scripts/build-class-jsons.js` | **Freeze:** antes de escrever cada `.jsonc`, compara com o que seria gerado; divergente sem `--force` → `skipped (frozen)`, intacto. Resumo: `N written, N skipped (frozen), N unchanged`. Gerador rebaixado a bootstrap. |

## Verificação (2026-06-10)

- `bash -n` nos dois scripts: OK.
- **Freeze:** `node build-class-jsons.js` com os 11 `.jsonc` presentes (com edições manuais do usuário não commitadas) → `0 written, 0 skipped (frozen), 11 unchanged`; `git status` antes/depois **idêntico** (nenhum arquivo tocado).
- **`/sync-classes --dry-run`:** detecta divergências install×repo, lista os arquivos, avisa sobre edições não-commitadas no repo e termina sem copiar.
- **Anti-clobber:** exercido de verdade no primeiro `/compile-mod` do item 020 (install atual diverge do repo — o guard deve disparar). Resíduo de teste do agente (`_test019.jsonc` no repo) removido.

## Decisões

- Comparação por conteúdo normalizado (não mtime) para o gate; mtime usado só como dica de "lado mais novo" na mensagem.
- `wwwroot/` é tratado como código (clobber sempre); `config/` como dado (guard).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-10 | As-built. Implementação na sessão 2026-06-09 (agente interrompido por limite); validações + limpeza + spec/as-built concluídos em 2026-06-10. |
