---
title: Criar Mods para o SPT 4.0
date: 2026-07-26
status: 🟢 Vivo
authors: Guilherme + agente
---

# Como Criar Mods para o SPT 4.0

Guia de onboarding para desenvolvimento de mods **neste repositório**. O SPT 4.0 abandonou o Node.js/TypeScript no backend: hoje é **C#** dos dois lados — .NET 9 no servidor, .NET Framework 4.7.2 no cliente (a Unity do Tarkov).

> **Este doc cobre o "como fazer" no fluxo do repo.** Para o ciclo de desenvolvimento completo (do backlog à validação in-game), a fonte é [WORKFLOW.md](../../WORKFLOW.md). Para onde buscar evidência técnica, [.agents/resources.md](../../.agents/resources.md).

---

## 1. Entendendo a separação

Mesmo com a linguagem unificada, a responsabilidade continua dividida — e o repo mantém os dois lados fisicamente separados em `mods/<mod>/modded/Server/` e `mods/<mod>/modded/Client/`.

### A. Server mods (backend)

Regras do servidor, database (itens, traders, loot, quests), perfis, spawn de IA, flea market.

- **Runtime:** C# / .NET 9, class library carregada pelo servidor.
- **Instalação:** `<SPT>/SPT/user/mods/<NomeDoMod>/`
- **Não pode** referenciar `UnityEngine` — roda fora do processo do jogo.

> ⚠️ **Nunca editar `SPT_Data/database/` direto.** Qualquer atualização do SPT sobrescreve, a edição invalida o `checks.dat` de integridade e gera arquivos pesados difíceis de versionar. O caminho correto é um server mod que aplica os patches **em memória** no `postDBLoad` — sobrevive a updates, pesa quase nada e é diffável. Ver `AGENTS.md`.

### B. Client mods (in-raid)

Jogabilidade dentro da Unity: UI, mecânicas de vida, armas, câmera, interações, fluxo de raid.

- **Runtime:** C# / .NET Framework 4.7.2, plugin **BepInEx** com patches **Harmony** sobre o `Assembly-CSharp.dll`.
- **Instalação:** `<SPT>/BepInEx/plugins/<AssemblyName>/`
- **Não alcança** o servidor diretamente — a comunicação é por rota HTTP registrada por um server mod par.

---

## 2. Antes de escrever a primeira linha

O repo já resolve boa parte do setup. O que você precisa:

| Item | Como |
|---|---|
| **.NET SDK** | `dotnet --version`. Server mod exige .NET 9; o client compila com o mesmo SDK visando `net472`. |
| **Path do SPT** | `cp .spt-path.example .spt-path` e ajustar. **Nunca hardcode** um caminho de instalação — precedência: `$SPT_PATH` / `--spt-path` > `.spt-path` > default `D:/SPT`. |
| **Referências vendorizadas** | `node scripts/setup-references.js` — clona `spt-source` e os repos do FIKA (gitignored). |
| **Decompile do cliente EFT** | `bash scripts/decompile-eft.sh` (~70s → 8.683 tipos) + `bash scripts/update-graphs.sh eft-decompiled`. |

**IDE é indiferente.** O build do repo é por CLI (`/compile-mod` → `dotnet build`); Visual Studio, Rider ou VS Code funcionam, nenhum é pré-requisito.

---

## 3. Descobrir o que patchear — no repo, não no dnSpy

Este é o passo que mais muda em relação a tutoriais genéricos de modding. **Não abra o `Assembly-CSharp.dll` numa ferramenta externa por reflexo** — o repo já tem o assembly descompilado, indexado e grafado.

Ordem de busca:

1. **Sei o nome do tipo** → grafo de código (skill `graph-code-navigation`, MCP `graphify-eft`) para achar callers, callees e **todos os overrides** de um método virtual. Depois abra o `.cs` para provar a assinatura.
2. **Sei só o conceito** → `references/eft-decompiled/types-index.json` traz o alias 4.1 de cada tipo ofuscado (`GClass2348` = `EFT.LocalizationExtensions`), e os aliases também estão injetados no topo de cada `.cs`. Complemento: [`docs/files-from-4.1/consolidated-mappings.txt`](../files-from-4.1/consolidated-mappings.txt).
3. **Existe mesmo?** → **`types-index.json` é a resposta**, não um `grep` vazio. O dump é gitignored: numa máquina onde não foi gerado, os `.cs` não estão em disco e o grep não acha nada que existe.

> 🔴 **Regra do harness: "grafo aponta, leitura prova".** Todo achado do grafo se confirma abrindo `arquivo.cs:linha`. E todo ponto de patch vindo de recon é **candidato até reconfirmar** — pelo `.cs` e, definitivamente, pela compilação. Ver [AP-03 e AP-09](spt-antipatterns.md).

`ilspycmd -t <FQN>` segue legítimo em três casos: tipo marcado `// DECOMPILE-ERROR` (são 8), tipo fora do índice, ou dump ausente na máquina. Um hook (`remind-use-graph.sh`) intercepta o uso reflexo e lembra da ordem acima.

---

## 4. Client mod — esqueleto

Estrutura no repo: o trabalho acontece **sempre** em `mods/<mod>/modded/`; `mods/<mod>/original/` é o upstream intocado.

```csharp
using BepInEx;
using HarmonyLib;

namespace MeuMod
{
    [BepInPlugin("com.seu_nome.meu_mod", "Meu Mod", "1.0.0")]  // ← o 3º arg é o que o F12 exibe
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Awake roda UMA vez no boot do jogo — não há GameWorld, profile nem raid aqui.
            // É o único lugar para: registrar patches, Config.Bind, assinar eventos long-lived.
            new MeuPatch().Enable();
        }
    }
}
```

Três regras que evitam a maioria dos bugs, detalhadas na skill `spt-mod-best-practices`:

- **Lifecycle de raid.** Todo estado alocado durante a raid precisa ser liberado no fim dela — e o fim vem por extract, morte, MIA ou alt-F4, por caminhos diferentes. Hookar `GameWorld.OnDestroy` **e** `BaseLocalGame.Stop`, com teardown idempotente ([AP-01](spt-antipatterns.md)).
- **Filtro de player.** Métodos virtuais de `Player` rodam para **cada** player do mundo — bots e outros jogadores em raid Fika inclusive. Todo patch que reage a ação de player valida `IsYourPlayer` ou equivalente ([AP-02](spt-antipatterns.md)). Fika instalado é o cenário default deste repo.
- **API canônica.** Escrever um field interno pula os side-effects que o jogo dispara (HUD, som, animação, sync de rede). Procure o setter/command que o próprio EFT usa ([AP-04](spt-antipatterns.md)).

### Referências do jogo — resolvidas automaticamente

Você **não** adiciona as DLLs do jogo à mão. O `/compile-mod` popula `modded/References/` com até 19 DLLs (`BepInEx.dll`, `0Harmony.dll`, `Assembly-CSharp.dll`, `UnityEngine*.dll`, `SPT.Reflection.dll`, …) a partir do path em `.spt-path`.

> ⚠️ **Nunca copiar DLL do jogo manualmente para `References/`.** Se a pasta estiver vazia ou incompleta, a resposta é rodar `/compile-mod` — não caçar arquivo no diretório do jogo. Regra do `AGENTS.md`; um gate de pre-commit (`check-csproj-references.sh`) avisa sobre `<HintPath>` absoluto no `.csproj`.

---

## 5. Server mod — esqueleto

```csharp
// Metadata obrigatória: o servidor recusa o mod sem ela.
public record ModMetadata : AbstractModMetadata
{
    public override string Name => "MeuModServer";
    public override string Version => "1.0.0";
    public override string SptVersion => "4.0.0";
    // ...
}
```

- **`SemanticVersioning`** é dependência obrigatória para o servidor casar o mod com a versão do SPT: `<PackageReference Include="SemanticVersioning" Version="3.0.0" />`.
- **Rotas HTTP:** a partir do 4.0 não é preciso abrir porta secundária — classes com `[ApiController]` são hospedadas pelo ASP.NET do próprio servidor, na porta raiz.
- **Reusar, não reinventar:** `PresetHelper`, `ItemHelper`, `InventoryHelper` e `ICloner` já existem em `references/spt-source/`. Ao mexer com itens/inventário/hideout, [spt4-items-inventory-hideout.md](spt4-items-inventory-hideout.md) é a doc canônica.
- **Cuidado com cache de helper:** vários helpers do SPT (ex.: `PresetHelper`) estão vazios em `postDBLoad + 1` — usar os dicts crus do `DatabaseService`/`globals` nessa janela.

---

## 6. Build e instalação

Um comando, para os dois tipos:

```bash
/compile-mod <mod>              # build + instala no path do .spt-path
/compile-mod <mod> --flat       # client: DLL direto em plugins/, sem subfolder
/compile-mod <mod> --clean      # limpa builds/ antes (cache stale do MSBuild)
```

O script detecta o tipo pelo conteúdo de `modded/` (`.csproj` com BepInEx → client; `package.json` → server TS), resolve as referências, compila em `mods/<mod>/builds/` e instala no destino correto.

**Gate de versão.** Toda compilação precisa evoluir a semver — `z` para fix (default), `y` para feature, `x` para breaking. O script falha antes do build se a versão não mudou desde o último compile. A fonte canônica no client é o 3º argumento de `[BepInPlugin]`: é literalmente o que o painel F12 exibe.

> Casos fora do `/compile-mod` — o launcher e o server mod `TarkovRedLine.Server` (tipo `server-csharp`, ainda não suportado) — estão em [spt4-csharp-build.md](spt4-csharp-build.md).

**Compilar ≠ funcionar.** O critério de entrega neste repo é validação **in-game**: raid real, e no cenário Fika quando aplicável ([AP-06](spt-antipatterns.md)). Um gate de pre-commit bloqueia item marcado como entregue com a caixa de validação in-raid desmarcada.

---

## 7. Referências

**No repo (preferir sempre):**

| O quê | Onde |
|---|---|
| Ciclo de desenvolvimento completo | [WORKFLOW.md](../../WORKFLOW.md) |
| Hierarquia de evidência e mapa de fontes | [.agents/resources.md](../../.agents/resources.md) |
| Erros já cometidos aqui | [spt-antipatterns.md](spt-antipatterns.md) |
| Itens, inventário, hideout | [spt4-items-inventory-hideout.md](spt4-items-inventory-hideout.md) |
| Pacotes FIKA / coop | [fika-packet-desync-prevention-plan.md](fika-packet-desync-prevention-plan.md) |
| Wiki oficial (snapshot read-only) | [wiki/spt/](../../wiki/spt/) |

**Externas:**

- [dev.sp-tarkov.com](https://dev.sp-tarkov.com/) — templates oficiais de BepInEx e server mod C#.
- [github.com/sp-tarkov/server-mod-examples](https://github.com/sp-tarkov/server-mod-examples) — exemplos de server mod.
- [deepwiki.com/sp-tarkov/server-csharp](https://deepwiki.com/sp-tarkov/server-csharp/1-overview) — visão arquitetural do servidor, útil antes de mergulhar no código bruto.
- [docs.bepinex.dev](https://docs.bepinex.dev/) · [harmony.pardeike.net](https://harmony.pardeike.net/) — frameworks de client mod.
- SPT Discord, canal `#mods-development` — quando a doc falha.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-06 | Guilherme | chore(launcher): remove empty placeholder diff.txt |
| 2026-07-26 | Guilherme + agente | Reescrito e promovido a 🟢 Vivo. Removidas as instruções que contradiziam o harness: adicionar referências à mão do diretório do jogo e copiar DLL para `BepInEx/plugins/` (agora `/compile-mod` + `.spt-path`), VS2022 como pré-requisito (build é por CLI) e dnSpy como rotina de descoberta (agora decompile local + `types-index.json` + grafos, AP-09). Adicionados: lifecycle/filtro de player/API canônica com link para os APs, gate de versão, validação in-game como critério de entrega, e mapa de referências internas. |
| 2026-07-26 | Guilherme | docs(technical): arquiva legado TS 3.x e redige credenciais Supabase expostas |
