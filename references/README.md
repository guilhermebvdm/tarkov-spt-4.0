# references/

Material de referência **read-only** — não é código do projeto e não deve ser editado. O metadado canônico (upstream, commit pinado, LFS, licença) vive em [manifest.json](./manifest.json); **este README não o duplica**.

## Obter as referências (máquina nova)

As fontes gitignored são clonadas com um comando (as demais já vêm versionadas no repo):

```bash
node scripts/setup-references.js           # clona o que faltar
node scripts/setup-references.js --check    # confere presença
```

O script clona cada fonte conforme o `pin` do manifest, roda `git lfs pull` quando aplicável e remove o `.git` dos snapshots.

## Fontes

- **`eft-decompiled/`** — Assembly C# do cliente EFT descompilado (🥇 verdade do cliente). **Índice versionado; o dump em si é gitignored** (regenerável).

  > ## 📖 Como consultar — e por que um `grep` vazio NÃO prova ausência
  >
  > O dump é **completo** desde 2026-07-19: **8.683 tipos, 0 pastas de namespace vazias** (antes: 4.561 arquivos e **102 pastas vazias**, que faziam tipos existentes serem dados como inexistentes). `EFT.HealthSystem`, `EFT.Animations`, `EFT.InventoryLogic`, `EFT.CameraControl` e `EFT.UI` estão preenchidos.
  >
  > **Mas o dump não vem no git** (são milhares de arquivos) — só o `types-index.json`. Numa máquina onde ele não foi gerado, os `.cs` não existem em disco. Por isso:
  >
  > | `eft-decompiled/types-index.json` | `.cs` em disco | Significado | Ação |
  > |---|---|---|---|
  > | tem o tipo | presente | existe | ler o `.cs` (prova a assinatura) |
  > | tem o tipo | **ausente** | existe — **não gerado nesta máquina** | `bash scripts/decompile-eft.sh` |
  > | **não tem** | — | não existe no assembly | investigar / reportar |
  >
  > **Busca por conceito** (não sei o nome, sei o que faz): o `types-index.json` traz o **alias 4.1** de 4.763 tipos, e o dump traz o alias em comentário no topo de cada arquivo — `grep "Localization"` encontra `GClass2348` (= `EFT.LocalizationExtensions`). O grafo indexa AST e **não** contém os aliases, então essa busca passa pelo índice/grep, não pelo `query_graph`.
  >
  > **`ilspycmd -t <FQN>`** continua legítimo em 3 casos: tipo marcado `// DECOMPILE-ERROR` (são **8**, ex.: `BackendAbstractClass`), tipo **fora** do índice, ou dump ausente. FQN é obrigatório.
  >
  > ⚠️ **Nunca regenere com `ilspycmd -p`** — o modo projeto aborta no primeiro método indecompilável (`BackendAbstractClass.GetTemplates`) e descarta namespaces inteiros em silêncio; foi assim que os 102 buracos surgiram, e custou dois perks do CustomClasses declarados "impossíveis" sendo alcançáveis. Use `scripts/decompile-eft.sh` (itera tipo a tipo com try/catch).
- **`spt-source/`** — código-fonte do servidor SPT 4.0 (🥇 verdade do servidor: serviços, helpers, fórmulas, rotas). Gitignored (~856 MB), pinado no commit que corresponde ao SPT em `D:/SPT`. Ao atualizar o SPT, atualize o `pin` no manifest e re-rode o setup — senão a lógica diverge do runtime testado.
- **`fika-{server,plugin,headless}/`** — código do FIKA (coop): servidor C#, plugin cliente C# (contém `Fika.Core`) e cliente headless TS. Gitignored, pinados por commit.
- **`spt-bigbrain/`** — mod DrakiaXYZ-BigBrain, referência para injeção e controle de inteligência artificial/brains de bots. Gitignored, pinado por commit.
- **`SPT-Waypoints-1.8.2/`** — mod DrakiaXYZ-Waypoints, referência de navegação/waypoints de bots. Versionado.
- **`graphs/`** — grafos AST gerados (output, não fonte externa) — ver [graphs/README.md](./graphs/README.md).

> Ordem de citação de evidência (qual fonte vale primeiro) e a regra "grafo aponta, leitura do `arquivo.cs:linha` prova": [AGENTS.md](../AGENTS.md) e [WORKFLOW.md](../WORKFLOW.md).
