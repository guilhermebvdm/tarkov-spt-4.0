# 019 — Guard rails de config (anti-clobber + sync) · Spec funcional

**Mod:** CustomClasses · **Data:** 2026-06-10 · **Kickoff:** [00-kickoff](019-config-guard-rails-00-kickoff.md)

## Objetivo

O editor web (021+) escreve `.jsonc` no **install** (`D:/SPT/SPT/user/mods/CustomClasses/config/classes/`). Hoje dois fluxos destroem edições feitas lá/no repo: a cópia `config/` repo→install do `compile-mod.sh` (clobber em todo build) e a regeração dos `.jsonc` pelo `build-class-jsons.js`. Este item instala as guardas ANTES de existir qualquer save do editor.

## Critérios de aceite

1. **Anti-clobber (compile-mod):** com `config/classes/` divergente entre repo e install, o build **não sobrescreve** a config — aborta a cópia com lista dos arquivos divergentes (indicando o lado mais novo) e instrução (`--force-config` ou `/sync-classes`). DLL e demais artefatos continuam instalando. Arquivos que só existem no repo são copiados normalmente. Flag `--force-config` força a cópia.
2. **Comparação tolerante:** diferenças só de EOL (CRLF/LF) e trailing whitespace **não** contam como divergência.
3. **`/sync-classes` (script + skill):** copia install→repo (`config/classes/*.json[c]` + `config/*.jsonc` raiz) com preview de diff por arquivo; `--dry-run` e `--yes`; não-interativo sem `--yes` aborta com instrução; avisa sobre edições não commitadas no repo antes de sobrescrever.
4. **Freeze do gerador:** `build-class-jsons.js` sem `--force` **nunca altera** um `.jsonc` existente cujo conteúdo difira do que seria gerado (loga `skipped (frozen)`); resumo final `N written, N skipped (frozen), N unchanged`.
5. **wwwroot:** install de projeto server-csharp passa a copiar `wwwroot/` (clobber intencional — é código). Pré-requisito do item 020.

## Fora de escopo

Editor em si (021+); sync automático/watcher; merge de conteúdo (só cópia com confirmação).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-10 | Spec escrita retroativamente à implementação (agente da sessão 2026-06-09 interrompido por limite após implementar). |
