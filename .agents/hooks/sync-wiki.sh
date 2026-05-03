#!/usr/bin/env bash
# Sincroniza wiki/ com o snapshot atual de github.com/sp-tarkov/wiki@main.
#
# Uso:
#   bash .agents/hooks/sync-wiki.sh
#
# O script substitui o conteúdo de wiki/ (preservando wiki/README.md) e
# atualiza o SHA do commit anotado em wiki/README.md.
#
# Conteúdo upstream é CC BY-NC-ND 4.0 — não modifique os arquivos sincronizados.

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
WIKI_DIR="$REPO_ROOT/wiki/spt"
TMP_DIR="$(mktemp -d)"
trap 'rm -rf "$TMP_DIR"' EXIT

UPSTREAM="sp-tarkov/wiki"
BRANCH="main"

echo "==> Buscando SHA atual de $UPSTREAM@$BRANCH"
SHA="$(curl -sfL "https://api.github.com/repos/$UPSTREAM/commits/$BRANCH" \
  | python -c 'import json,sys;print(json.load(sys.stdin)["sha"])')"
DATE="$(curl -sfL "https://api.github.com/repos/$UPSTREAM/commits/$BRANCH" \
  | python -c 'import json,sys;print(json.load(sys.stdin)["commit"]["author"]["date"][:10])')"
echo "    SHA:  $SHA"
echo "    Data: $DATE"

echo "==> Baixando tarball"
curl -sfL "https://github.com/$UPSTREAM/archive/$SHA.tar.gz" -o "$TMP_DIR/wiki.tar.gz"
tar -xzf "$TMP_DIR/wiki.tar.gz" -C "$TMP_DIR"

SRC="$TMP_DIR/wiki-$SHA"

echo "==> Substituindo wiki/ (preservando README.md)"
# Preserva o README local
cp "$WIKI_DIR/README.md" "$TMP_DIR/README.local.md"

# Limpa wiki/ (exceto README.md)
find "$WIKI_DIR" -mindepth 1 ! -name 'README.md' -exec rm -rf {} + 2>/dev/null || true

# Copia novo conteúdo
cp -r "$SRC/." "$WIKI_DIR/"

# Restaura README local (não mexer em README.md upstream — não existe na origem)
mv "$TMP_DIR/README.local.md" "$WIKI_DIR/README.md"

echo "==> Atualizando SHA e data em wiki/README.md"
# Atualiza linha do SHA
sed -i.bak -E "s|(\\*\\*Commit do snapshot:\\*\\* \`)[a-f0-9]+(\`.*)|\\1$SHA\\2|" "$WIKI_DIR/README.md"
sed -i.bak -E "s|(\\*\\*Data do snapshot:\\*\\* )[0-9-]+|\\1$DATE|" "$WIKI_DIR/README.md"
TODAY="$(date +%Y-%m-%d)"
sed -i.bak -E "s|(\\*\\*Importado em:\\*\\* )[0-9-]+|\\1$TODAY|" "$WIKI_DIR/README.md"
rm -f "$WIKI_DIR/README.md.bak"

echo "==> Pronto."
echo "    Revise: git diff wiki/spt/"
echo "    Commit: git add wiki/spt/ && git commit -m 'chore(wiki): sync spt snapshot from $UPSTREAM@${SHA:0:7}'"
