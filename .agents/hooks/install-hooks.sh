#!/bin/bash
# Instala os git hooks do projeto (Windows-compatible — sem symlinks)

set -euo pipefail

REPO_ROOT=$(git rev-parse --show-toplevel)
GIT_HOOKS="$REPO_ROOT/.git/hooks"

# Cria wrapper para pre-commit (funciona no Windows via Git Bash / WSL)
cat > "$GIT_HOOKS/pre-commit" << 'EOF'
#!/bin/bash
exec bash "$(git rev-parse --show-toplevel)/.agents/hooks/pre-commit.sh"
EOF

chmod +x "$GIT_HOOKS/pre-commit"

echo "✅ Git hooks instalados:"
echo "   .git/hooks/pre-commit → .agents/hooks/pre-commit.sh"
