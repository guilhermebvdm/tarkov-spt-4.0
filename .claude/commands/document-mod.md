# /document-mod

Gera uma **documentação técnica e funcional completa, modular e estruturada** para qualquer mod do repositório, criando ou atualizando a pasta `mods/<NomeDoMod>/docs/` com artigos temáticos detalhados, diagramas conceituais em Mermaid, mapeamento de classes/métodos, tabelas de parâmetros e conformidade com as regras do workspace.

---

## Uso

```bash
/document-mod <mod-ou-caminho> [--scope <original|modded>] [--clean]
```

- `<mod-ou-caminho>` — Nome da pasta do mod em `mods/` (ex.: `ORBIT`, `VisceralCombat`, `TRL-ActionPOV`) ou caminho dentro de `mods/`.
- `--scope <original|modded>` (Opcional) — Define a pasta base de inspeção de código (padrão: `original/` para mods de terceiros/vendorizados para documentar a especificação base; `modded/` para mods próprios ou quando o foco for documentar as customizações ativas).
- `--clean` (Opcional) — Limpa a pasta `docs/` existente antes de gerar a nova documentação.

---

## Pré-condições

1. A pasta `mods/<NomeDoMod>/` deve existir no workspace.
2. Deve conter código-fonte inspecionável (`.cs` para BepInEx/client ou `.ts`/`.js`/`.json` para mods de servidor).
3. (Recomendado) Grafo de código gerado em `references/graphs/mods/<NomeDoMod>/` para acelerar a navegação de símbolos e dependências.

---

## Fluxo de Execução (Passo a Passo)

### 1. Resolução do Mod e Mapeamento do Código-Fonte
1. Identifique o mod alvo e resolva o caminho da pasta (`mods/<NomeDoMod>/`).
2. Varra recursivamente todos os arquivos de código (`.cs`, `.ts`, `.json`) na pasta de escopo (`original/` ou `modded/`).
3. Mapeie:
   - Pontos de entrada (`Plugin.cs`, `[BepInPlugin]`, classes herdando de `BaseUnityPlugin` ou `IOnLoad`).
   - Configurações F12 (`Config.Bind`, `PROPRIEDADES.md`).
   - Camadas de IA / BigBrain / SAIN (se aplicável).
   - Subsistemas, Managers, Services e Handlers.
   - Patches Harmony (`[HarmonyPatch]`, `ModulePatch`).
   - Modelos de dados, componentes e enums.

### 2. Decomposição Modular da Documentação
Agrupe a complexidade do mod em **4 a 8 temas lógicos e coesos**, criando documentos numerados sequencialmente. Exemplos de decomposição canônica:

- `01-visao-geral-e-arquitetura.md` — Arquitetura central, ciclo de vida de raid, modelo de dados/ECS e integração com BigBrain/SAIN/EFT.
- `02-sistema-de-objetivos-e-gameplay.md` (ou equivalente) — Mecânicas principais de jogabilidade, metas, missões, regras de negócio.
- `03-configuracoes-e-personalidades.md` — Configurações F12, perfis de comportamento, arquétipos de IA.
- `04-sistemas-de-movimentacao-e-navegacao.md` — Waypoints, NavMesh, campos de força, steering.
- `05-sistema-de-looting-e-itens.md` — Manipulação de inventário, contêineres, corpos, algoritmos de troca de equipamentos (*Gear Swap*).
- `06-sistema-de-extracao-e-eventos.md` — Rota de fuga, gatilhos de extração, emergências.
- `07-sistemas-auxiliares-portas-e-performance.md` — Física de portas, olhar (*LookSystem*), patches de ciclo de vida e otimizações de CPU.

### 3. Diretrizes Obrigatórias para Cada Documento Gerado

Todo arquivo `.md` dentro de `mods/<NomeDoMod>/docs/` (exceto `README.md`) deve seguir rigorosamente:

1. **Frontmatter Canônico Obrigatório no Topo:**
   ```yaml
   ---
   title: "Nome do Mod — Título do Subsistema"
   date: YYYY-MM-DD
   status: 🟢 Vivo
   authors: Antigravity
   ---
   ```
2. **Idioma:** Estritamente em **Português (Brasil)**.
3. **Diagramas Visuais em Mermaid:**
   - Pelo menos 1 a 2 diagramas conceituais por documento (fluxogramas de decisão, diagramas de sequência de eventos de raid, gráficos de máquinas de estado ou diagramas de classes).
4. **Tabelas Estruturadas:**
   - Tabelas comparativas de parâmetros, enums, limites numéricos, multiplicadores e comportamentos.
5. **Links Canônicos Relativos:**
   - Todo arquivo, classe ou método citado no texto deve possuir um link markdown funcional apontando para o arquivo no repositório, **sempre relativo ao documento** (nunca `file://` absoluto — quebra em outra máquina, AP-05). A partir de `mods/<Mod>/docs/`:
     `[NomeDoArquivo.cs](../original/<Caminho>/NomeDoArquivo.cs)` · `[GameWorld.cs:2584](../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L2584)`

### 4. Geração do `mods/<NomeDoMod>/docs/README.md` (Índice Central)
Crie um `README.md` na raiz da pasta `docs/` contendo:
- Apresentação executiva da documentação do mod.
- Tabela com o sumário de todos os documentos gerados, com links relativos e status.
- Relação estruturada de todos os arquivos de código-fonte mapeados do mod com links relativos.

### 5. Validação de Conformidade
Execute a validação manual de cabeçalhos em todos os arquivos markdown criados:
```bash
find mods/<NomeDoMod>/docs -name "*.md" ! -name "README.md" | while IFS= read -r f; do
  bash .agents/hooks/validate-doc-header.sh "$f"
done
```

### 6. Atualização do `README.md` Raiz do Mod
Verifique se `mods/<NomeDoMod>/README.md` possui uma seção apontando para a pasta `docs/` e para o catálogo [PROPRIEDADES.md](./PROPRIEDADES.md) (link relativo à raiz do mod).

---

## Saída Esperada ao Final do Comando

```text
✓ Documentação modular criada para <NomeDoMod>:
  Pasta: mods/<NomeDoMod>/docs/
  Índice: mods/<NomeDoMod>/docs/README.md
  Documentos gerados (N arquivos):
    - 01-visao-geral-e-arquitetura.md
    - 02-...
    - 03-...
  Validação de cabeçalhos: ✓ 100% aprovada via validate-doc-header.sh
```
