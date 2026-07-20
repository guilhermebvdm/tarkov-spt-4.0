# EFT Decompiled — Referência Interna

Código C# descompilado do cliente do Escape from Tarkov, usado como **referência de leitura** para identificar propriedades, métodos e estruturas internas do jogo ao desenvolver/ajustar mods.

## ⚠️ Aviso legal

- O conteúdo desta pasta é **propriedade intelectual da Battlestate Games (BSG)**.
- **Apenas para análise interna.** Não redistribuir, não publicar, não copiar em repositórios públicos.
- **Não compilar ou usar para criar reimplementações.** Uso restrito a inspeção de API para mods que rodam sobre o cliente original (BepInEx/Harmony patches).
- Este repositório deve permanecer **privado**.

## O que está aqui (e o que NÃO está no git)

| Caminho | No git? | O que é |
|---|---|---|
| `types-index.json` | ✅ **sim** (740 KB) | Índice de **todos** os 8.683 tipos top-level do assembly: FQN, status e alias 4.1 |
| `.provenance.json` | ✅ sim | De qual DLL/build este dump veio |
| `README.md` | ✅ sim | este arquivo |
| `Assembly-CSharp/` | ❌ **gitignored** | O dump em si — 8.683 arquivos `.cs`. Regenerável (ver Atualização) |

> **Por que o dump está fora do git:** são milhares de arquivos e dezenas de MB que inchariam o repositório
> permanentemente. O **índice**, que é leve, fica versionado — e é ele que responde *"esse tipo existe?"* mesmo
> numa máquina onde o dump não foi gerado.

## 🔎 Como procurar (a ordem importa)

1. **Sei o nome do tipo** → grafo (MCP `graphify-eft`: `query_graph`, `get_node`) → abrir o `.cs` para **provar** a
   assinatura em `arquivo.cs:linha`.
2. **Sei só o conceito** ("quem cuida de localização?") → **`types-index.json`** ou `grep` do alias no dump →
   obtenho o FQN (ex.: `GClass2348`) → grafo → `.cs`.
   ⚠️ O grafo indexa **AST**, então os aliases (que vivem em comentário/índice) **não** são nós do grafo —
   `query_graph "Localization"` não acha `GClass2348`. Por isso o passo pelo índice é necessário.
3. **`ilspycmd -t <FQN>`** → exceção, só quando: o tipo está marcado `// DECOMPILE-ERROR`, **ou** não consta do
   `types-index.json`, **ou** o dump não está nesta máquina.

**Nunca conclua "esse tipo não existe" a partir de um `grep` vazio** — confirme no `types-index.json`.

## Sobre os aliases do SPT 4.1

Tipos obfuscados (`GClass1234`) trazem no topo do arquivo um comentário com o nome conceitual, vindo do mapping
`docs/files-from-4.1/consolidated-mappings.txt`:

```csharp
// [SPT 4.1 alias — RÓTULO (mapping comunitário), não verificado] EFT.LocalizationExtensions
```

**4.763 dos 8.683 tipos (55%) têm alias.** Cobertura por família: `GClass`/`GStruct`/`GInterface` ~99%;
`Class<n>`/`Struct<n>` (aninhados) bem menor. Três ressalvas, porque o mapping é comunitário e não oficial:

- É **rótulo, não prova** — a assinatura ainda se confirma no `.cs`.
- Cobre **tipos, não membros** (`method_5`, `_player` seguem obfuscados).
- **Ausência de alias ≠ o tipo não existe** (ex.: `GClass898`/`GClass3008` são usados no repo e não estão no mapa).

## Origem

| Campo | Valor |
|---|---|
| Assembly | `D:/SPT/EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll` |
| SHA256 | `faef6f0b9f142f9d047495ec3dccfd5d6974ac048368dc7045955cf54b117982` |
| build-guid | `9c1bfdf078d74e26b2de13c18e539045` |
| EFT | 0.16.x · **SPT 4.0.13** |
| Gerado em | 2026-07-19 (`dumpVersion` 1) |

> A DLL é **patcheada pelo SPT** (existe `Assembly-CSharp.dll.spt-bak`, o original). O EFT não recebe mais updates
> compatíveis com o SPT (a próxima versão é a 1.0, com criptografia), mas **o SPT re-patcheia a DLL a cada update
> dele** — por isso a provenance registra `sptVersion`.

## Números deste dump

| Métrica | Valor |
|---|---|
| Tipos top-level | **8.683** (aninhados saem inline no arquivo do pai) |
| Descompilados OK | 8.675 |
| `// DECOMPILE-ERROR` | **8 (0,09%)** — `BackendAbstractClass`, `BackendDummyClass`, `ProfileEndpointFactoryAbstractClass`, `InteractionsHandlerClass`, `GClass3468`, `GClass3469`, `EFT.UI.ItemUiContext`, `EFT.UI.CharacteristicsPanel` |
| Pastas de namespace vazias | **0** (o dump anterior tinha **102**) |
| Tempo de geração | ~71 s |
| Grafo | 111.732 nós · 163.339 edges · 86,7 MB · ~142 s |

## Atualização

> ⚠️ **NÃO use `ilspycmd -p`.** O modo projeto **aborta no primeiro método indecompilável e descarta namespaces
> inteiros em silêncio** — foi exatamente assim que o dump anterior ficou com 102 pastas vazias
> (`EFT.Animations`, `EFT.HealthSystem`, `EFT.InventoryLogic`, `EFT.CameraControl`, `EFT.UI`…), o que levou tipos
> existentes a serem dados como inexistentes. O tipo que dispara o abort é o `BackendAbstractClass`.

```bash
bash scripts/decompile-eft.sh              # gera, valida e substitui
bash scripts/decompile-eft.sh --dry-run    # gera em temp e valida, sem substituir
bash scripts/update-graphs.sh eft-decompiled   # regenera o grafo (necessário para o MCP)
```

O runner (`scripts/decompile-eft.sh`) usa o harness em `.agents/tools/decompile-eft/`, que itera **tipo a tipo com
`try/catch`**: o que falha vira um stub visível e registrado no índice, em vez de derrubar o namespace. Ele gera em
diretório temporário e **só substitui o dump depois de validar** (contagem, tipos-canário, índice íntegro).

Ao bump do harness com a mesma DLL, use `DUMP_VERSION=2 bash scripts/decompile-eft.sh`.
