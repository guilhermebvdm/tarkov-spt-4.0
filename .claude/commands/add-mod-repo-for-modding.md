# /add-mod-repo-for-modding

Adiciona um novo mod ao repositório clonando da URL fornecida e criando a estrutura padrão.

## Uso

```
/add-mod-repo-for-modding <git-url> [--name <ModName>] [--forge <forge-url>]
```

## O que fazer

1. Repasse os argumentos para o script:
   ```
   bash .agents/scripts/add-mod.sh $ARGUMENTS
   ```
2. Se o script falhar, mostre o erro ao usuário e pare. Não tente recuperar manualmente.
3. **Gerar `PROPRIEDADES.md` (somente para mods BepInEx)** — após sucesso do script:
   - Verifique se há chamadas `Config.Bind(...)` em arquivos `.cs` dentro de `mods/<Nome>/original/` (ex.: `Plugin.cs`). Use Grep com pattern `Config\.Bind\(` e glob `*.cs`.
   - Se NÃO houver: pule este passo (mod server-side ou sem F12) e siga para o passo 4.
   - Se houver: leia os arquivos relevantes e crie `mods/<Nome>/PROPRIEDADES.md` listando **todas** as propriedades expostas no F12 (BepInEx ConfigurationManager), agrupadas pela `section` (1º argumento do `Config.Bind`), na ordem de exibição (atributo `Order` decrescente — maior `Order` aparece primeiro).
   - Para cada propriedade, registre em uma linha de tabela: nome em inglês, tradução pt-BR, tipo (`bool`/`float`/`int`/`KeyCode`/etc.), valor padrão, faixa (`AcceptableValueRange`, se houver), e a coluna **Tooltip (pt-BR)** com a tradução fiel do `ConfigDescription` (texto do tooltip que aparece ao passar o mouse).
   - Marque com **(Avançado)** as entradas que tenham `IsAdvanced = true` em `ConfigurationManagerAttributes` — adicione coluna "Avançado" na tabela ou anote no cabeçalho da seção quando todas forem avançadas.
   - Inclua no topo: nome do plugin (`BepInPlugin`), versão, link `[original/Plugin.cs](original/Plugin.cs)` (ou arquivo equivalente), e nota de que itens **(Avançado)** só aparecem com "Advanced settings" ligado no F12.
4. **Gerar o grafo inicial do mod** — rodar `/update-mod-graph <Nome>` (o escopo `mods/<Nome>/modded` é auto-descoberto pelo glob do `scripts/update-graphs.sh` — sem registro manual). Adicionar ao `README.md` do mod uma seção curta "Mapa de código" apontando para `references/graphs/mods/<Nome>/GRAPH_REPORT.md` e o comando de regeneração. Se o graphify não estiver instalado, **pular com aviso** (instruções em `references/graphs/README.md`).

5. Em sucesso, confirme ao usuário:
   - nome do mod e pasta criada
   - SHA do upstream capturado
   - se `PROPRIEDADES.md` foi gerado (ou que foi pulado por não ser BepInEx)
   - se o grafo de código foi gerado (ou pulado por graphify ausente)
   - lembrete: editar `mod.json` (preencher `spt_version`) e `README.md`

## O que o script faz

- Infere o nome do mod a partir da URL se `--name` não vier (basename do repo, sem `.git`)
- Recusa sobrescrever se `mods/<Nome>/` já existir
- Clona com `--depth=1` em diretório temporário
- Captura `HEAD` SHA, branch, licença (heurística) e versão (de `package.json` ou `*.csproj`)
- **Remove `.git/` do clone** — nenhuma pasta de mod fica vinculada ao git original
- Move clone → `mods/<Nome>/original/`
- Copia `original/` → `modded/` (idênticos no momento zero)
- Cria `assets/ backlog/ builds/ scripts/` vazios
- Renderiza `mod.json` e `README.md` a partir de templates em `.agents/templates/`

## Regras

- `mods/*/original/` é referência intocada — não modifique
- Modificações vão em `mods/*/modded/`
- Ver diff: `diff -r mods/<Nome>/original/ mods/<Nome>/modded/`
