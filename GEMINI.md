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

## Regras de Backlog (Gemini)

**Antes de escrever qualquer código** em `mods/<mod>/modded*/` (`modded/`, `modded-V2/` etc.), abra ou identifique o item correspondente em `mods/<mod>/backlog/NNN-slug/` e siga o ciclo completo descrito em [WORKFLOW.md](WORKFLOW.md) — spec funcional → spec técnica → review técnica → código → code-review → as-built. Isso vale mesmo quando o pedido do usuário for só "investiga" ou "diagnostica": um documento de diagnóstico não é licença para já implementar a correção sem passar pelo ciclo.

1. **Nunca implemente antes de existir o item de backlog.** Se o trabalho começou como diagnóstico solto em `docs/`, formalize-o como item numerado assim que decidir codificar — não deixe código novo em `modded*/` sem um `NNN-slug/` correspondente.
2. **Revisão técnica e code-review devem ser críticos de verdade**, mesmo quando você mesmo os escreve: aponte riscos e trade-offs explicitamente, não apenas confirme que está tudo certo.
3. **IDs de achados de auditoria (`AUD-NN-NN`) são únicos por relatório** — nunca reutilize um ID que já existe em `docs/relatorio-auditoria-codigo-*.md` apontando para outro achado. Se precisar referenciar um achado novo, use o ID do item de backlog que você abriu.
4. **Todo item que mira um fork alternativo do mod (ex.: `modded-V2/` em vez de `modded/`) deve declarar isso explicitamente** no cabeçalho do artefato (`Target:`/`Fork:`).
5. Um hook de pre-commit (`.agents/hooks/check-backlog-traceability.sh`) avisa (não bloqueia) quando código de mod é commitado sem nenhum arquivo de backlog staged junto — leve o aviso a sério antes de ignorá-lo.

