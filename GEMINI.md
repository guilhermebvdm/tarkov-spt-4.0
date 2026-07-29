# Gemini — tarkov-spt-4.0

Leia [AGENTS.md](AGENTS.md) primeiro — ele é a fonte de verdade.

## Validação de docs (manual no Google AI Studio)

```bash
bash .agents/hooks/validate-doc-header.sh docs/caminho/arquivo.md
```

O git pre-commit hook (instalado via `bash .agents/hooks/install-hooks.sh`) cuida do histórico automaticamente.

## Regras de Compilação e Versionamento (Gemini)

Sempre seguir as instruções de versionamento do [.claude/commands/compile-mod.md](.claude/commands/compile-mod.md), com a seguinte exceção e diretrizes obrigatórias:

1. **Bump Obrigatório de Versão (SemVer):**
   - Antes de compilar qualquer mod (Client C# ou Server TS), **SEMPRE** incrementar a versão SemVer (`x.y.z`).
   - Manter a versão sincronizada no `Plugin.cs` (`[BepInPlugin("...", "...", "x.y.z")]`), no `.csproj` (`<Version>x.y.z</Version>`) e no `package.json` (`"version": "x.y.z"`).
   - Regra de incremento: `z` (patch) para correções/ajustes de desenvolvimento; `y` (minor) para novas features visíveis; `x` (major) para breaking changes.

2. **Isolamento de Build (Apenas na pasta do Mod):**
   - **NUNCA** compilar ou copiar automaticamente binários (`.dll`) para a pasta de instalação do jogo (`D:/SPT` ou `.spt-path`).
   - Manter todos os artefatos compilados **exclusivamente dentro da pasta do mod** (`mods/<mod>/builds/` ou `mods/<mod>/modded/bin/Release/`).
   - A proposta é trabalhar estritamente com controle de versionamento e histórico local dentro do repositório/workspace do mod.

