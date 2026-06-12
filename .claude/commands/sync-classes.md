# /sync-classes

Traz edições de config do CustomClasses feitas no **install** (`D:/SPT/SPT/user/mods/CustomClasses/config/` — editor web ou edição manual) de volta para o **repo** (`mods/CustomClasses/modded/Server/config/`). É o caminho oficial para commitar o que foi editado no install (item 019 — guard rails).

## O que fazer

1. **Preview** (sempre primeiro, não copia nada):
   ```bash
   bash mods/CustomClasses/scripts/sync-classes.sh --dry-run
   ```
2. Mostrar os diffs ao usuário. Se houver aviso de **mudanças não-commitadas** no config do repo, parar e resolver com o usuário antes (commit/stash) — sync sobrescreve.
3. **Aplicar** (após confirmação do usuário):
   ```bash
   bash mods/CustomClasses/scripts/sync-classes.sh --yes
   ```
4. **Commitar em seguida** os arquivos sincronizados (`git diff` → commit) — não deixar o sync solto na working tree.

## Flags

- `--dry-run` — só mostra os diffs; não copia.
- `--yes` — pula a confirmação (obrigatório em execução não-interativa; sem ele o script aborta com exit 2).
- `--spt-path <path>` — sobrescreve o path do SPT (default: env `SPT_PATH` ou `D:/SPT`, mesma convenção do `compile-mod.sh`).

## Notas

- Direção **install → repo** apenas. O caminho repo → install é o `/compile-mod` (que agora aborta a cópia de config se o install divergir — `--force-config` para sobrescrever).
- Cobre `config/classes/**/*.json[c]` e `config/*.jsonc` de nível raiz (ex.: `hidden-editions.jsonc`).
- Diferenças só de EOL (CRLF/LF) ou trailing whitespace **não** contam como divergência.
