---
title: Portar um mod do SPT 4.1 para o SPT 4.0 (downgrade)
date: 2026-09-30
status: 🟢 Vivo
authors: Guilherme + agente
---

# Portar um mod do SPT 4.1 para o SPT 4.0 (downgrade)

Procedimento para pegar um mod escrito para o SPT 4.1 e fazê-lo rodar no SPT 4.0.13 / EFT 0.16.9.40087, que é a versão do servidor do grupo. A documentação oficial só descreve a subida (4.0 → 4.1); aqui ela é lida ao contrário. Extraído do port do ORBIT 2.1 ([mods/ORBIT-2.1/](../../mods/ORBIT-2.1/), item de backlog 001), onde cada passo abaixo foi executado.

**Gatilho de leitura:** a tarefa adiciona ou atualiza um mod cujo upstream declara `SptVersion ~4.1` ou usa nomes de classe do jogo sem `GClass`.

## 1. O que muda entre as versões

| Camada | SPT 4.0.13 | SPT 4.1 | Fonte |
|---|---|---|---|
| Build do jogo | EFT 0.16.9.40087 | EFT 0.16.9.5.40743 | `README.md` de `SP-Tushonka/modules` |
| `Assembly-CSharp` | remap antigo: tipos planos (`AmmoItemClass`, `GClass45`), membros derivados do tipo (`BotOwner_0`, `method_10`, `Bool_0`) | deofuscado: nome real de tipo, namespace e membro (`EFT.InventoryLogic.Ammo`, `_owner`, `CastFromPos`) | [Client_40_to_41.md](../../wiki/spt/modding/SPT_41_Modding/Client_40_to_41.md) |
| Servidor | .NET 9 | .NET 10 | `SPT.Server.runtimeconfig.json` |
| Metadados do mod | `record X : AbstractModMetadata` com `override` e `IsBundleMod` | `record X : IModMetadata` | [Server_40_to_41.md](../../wiki/spt/modding/SPT_41_Modding/Server_40_to_41.md) |
| Carga | `Task OnLoad()` | `Task OnLoadAsync(CancellationToken)` | idem |
| Ordem de carga | `PreSptModLoader`, `Database`, `PostDBModLoader`, `PostSptModLoader` | `Preload`, `Routers`, `PostLoad` (banco já carregado) | `references/spt-source/.../DI/OnLoadOrder.cs` |
| Rotas | ação com 4 argumentos | ação com 5 (`cancellationToken`) | `references/spt-source/.../DI/Router.cs` |
| Banco e configs | `DatabaseService`, `ConfigServer` | tabelas e configs injetáveis (`TemplateTable`, `InsuranceConfig`) | guia do servidor |
| Logger | `ISptLogger` em `SPTarkov.Server.Core.Models.Utils` | em `SPTarkov.Common.Models.Logging` | guia do servidor |
| Painel web | `IModWebMetadata` (marcador vazio); o documento do hospedeiro **não** carrega o MudBlazor; MudBlazor 8.13.0 | `IModBlazorMetadata` com cartão do mod | `references/spt-source/Libraries/SPTarkov.Server.Web/` |
| Pasta do servidor | `SPT/` | `SPT_Runtime/` | instalação |
| Arquivo `*.js`/`*.ts` na pasta do mod | o validador rejeita o mod | permitido para mods com página web | `ModValidator.cs` |
| Pacotes NuGet | `SPTarkov.*` | `SPTushonka.*` desde o 4.1.3 (assemblies e namespaces seguem `SPTarkov.*`) | [Server_413_Changes.md](../../wiki/spt/modding/SPT_41_Modding/Server_413_Changes.md) |

Como a build do jogo é outra, o port prova equivalência de **nomes e assinaturas**. Um método com o mesmo nome e a mesma assinatura pode ter corpo diferente; isso só aparece em jogo.

## 2. Fontes do de-para, em ordem de confiança

1. **O commit em que o autor do mod portou de 4.0 para 4.1**, se existir no histórico do upstream. Lido ao contrário (linhas `-` são 4.0, `+` são 4.1), dá tipos **e membros** já conferidos pelo autor. Procurar com `git log --grep='4\.1'` num clone completo; o `add-mod.sh` clona raso, então clonar à parte.
2. **Tabela oficial de tipos**: [Class_Name_Mappings.md](../../wiki/spt/modding/SPT_41_Modding/client/Class_Name_Mappings.md), idêntica a [consolidated-mappings.txt](../files-from-4.1/consolidated-mappings.txt). Esquerda = 4.0. Cobre só tipos.
3. **Decompile 4.0** (`references/eft-decompiled/`) para os membros que nenhuma tabela cobre. Padrões que se repetem: campo privado `_fooBar` do 4.1 é `FooBar` no 4.0 quando o remap antigo tinha nome, e `Tipo_N` (`BotOwner_0`, `Bool_0`) ou `tipo_N` minúsculo (campo privado) quando não tinha; método com nome real no 4.1 é `method_N` ou `vmethod_N`. Casar por tipo, assinatura, nome dos parâmetros e papel no corpo.
4. **Mesma dependência nas duas linhas** (Fika, SAIN): o fonte 4.0 e o 4.1 do mesmo arquivo dão pares de nomes de membro.

Não serve: gerar o assembly 4.1 com a ferramenta oficial de deofuscação. Os mapas dela são para a build 40743 e a numeração `GClassNNNN` muda entre builds.

## 3. Categorias de diferença e tratamento

O objetivo é manter o diff contra o upstream pequeno e deixar todo nome ofuscado em poucos arquivos.

| # | Diferença | O compilador acusa? | Tratamento |
|---|---|---|---|
| A | Nome de tipo não genérico | sim | `global using NomeNovo = TipoAntigo;` num arquivo `Compat/`. O fonte não muda. Cada projeto precisa compilar o arquivo (global using é por projeto) |
| B | Tipo genérico aberto; nome totalmente qualificado | sim | Edição no ponto de uso (apelido não cobre) |
| C | Membro acessado por sintaxe C# | sim | Edição no ponto de uso |
| D | Nome de tipo comparado como texto (`GetType().Name == "FollowerPatrolLayer"`) | **não** | Dicionário tipo 4.0 → nome 4.1 no ponto que lê o nome |
| E | Membro passado como texto a reflexão ou ao Harmony (`AccessTools.Field(t, "_owner")`) | **não** | Tabela nome 4.1 → nome 4.0 no ponto que faz a busca |
| F | Parâmetro de patch que injeta campo (`___campo`) ou casa parâmetro por nome | **não** | Renomear o parâmetro |
| G | API do servidor | sim | Arquivo de compatibilidade + contrato de metadados, carga e rotas |
| H | Página web | **não** (responde HTTP 200) | O layout do mod carrega CSS e JS do MudBlazor, como `BaseMudBlazorLayout.razor` do hospedeiro |
| I | Caminhos e nomes de pasta em texto (`SPT_Runtime/`, nome da pasta do mod) | **não** | Derivar da localização da DLL |
| J | Projeto | sim | `Directory.Build.props` resolvendo o `.spt-path`; remover alvos de cópia do autor; servidor em `net9.0` |

**As categorias D, E, F, H e I não geram erro de compilação.** No ORBIT 2.1, depois de zero erros de compilação, elas ainda deixavam o recurso principal da versão desligado e o painel sem funcionar.

Truques que economizam edição:

- `namespace Namespace.Que.So.Existe.No41 { }` vazio num arquivo de compatibilidade faz as diretivas `using` do upstream compilarem.
- Método de extensão com o nome novo encaminhando para o antigo (`ShowMessageBoxAsync` → `ShowMessageBox`).
- `LangVersion` `default` com o SDK atual aceita `global using` mesmo em `netstandard2.1`.

## 4. Procedimento

1. **Reconhecimento em cópia descartável.** Copiar `original/` para o scratchpad, compilar contra o SPT 4.0 passando o caminho por propriedade (`-p:SptRoot=...`) e apontando qualquer alvo de cópia do autor para uma pasta temporária. Rodar com `DOTNET_CLI_UI_LANGUAGE=en`: as mensagens em inglês têm formato estável para script.
2. **Laço guiado pelo compilador.** A cada rodada: tipo desconhecido com candidato único na tabela invertida vira apelido; membro desconhecido é editado **na posição exata do erro** (arquivo, linha, coluna), nunca por busca textual no arquivo inteiro. No ORBIT as rodadas deram 84, 350, 220, 82, 12 e 0 linhas de erro (a contagem sobe na segunda porque os erros de membro só aparecem depois que os tipos resolvem).
3. **Aplicar em `modded/` preservando o fim de linha** de cada arquivo. O `sed -i` do Git Bash remove o CR e faz todo o arquivo aparecer como alterado no `diff -r original modded`.
4. **Conferir cada apelido** contra a declaração da classe no decompile; para camada de IA, contra o literal devolvido por `Name()`.
5. **Varredura das categorias sem erro de compilação:** literais iguais a nome de tipo 4.1; toda chamada de reflexão com nome em texto, incluindo os helpers do próprio mod; parâmetros `___`; caminhos.
6. **Executar os patches fora do jogo** (seção 5).
7. **Servidor:** subir uma cópia da pasta `SPT/` sem `user/`, em outra porta, só com o mod; conferir carga, páginas e rotas; abrir o painel num navegador e clicar.
8. **Revisão por leitores independentes**, cada um com uma frente: nomes em texto, patches e equivalência de membros, servidor, e a própria spec. Se o mod declara pacote Fika (`INetSerializable`), conferir contra [fika-packet-desync-prevention-plan.md](fika-packet-desync-prevention-plan.md) §7: pacote herdado do upstream costuma estar fora do padrão.
9. **Defeito proposital:** reverter uma correção de cada tipo e confirmar que a verificação correspondente falha.
10. **Validação em jogo.** Nada acima a substitui.

## 5. Executar os patches fora do jogo

O Harmony não consegue desviar um método do jogo fora do processo do jogo: o runtime de desktop recusa compilar chamadas nativas da Unity (`ECall methods must be packaged into a system module`), em .NET Framework e em .NET 8+. O que funciona é carregar as DLLs reais, rodar o `Enable()` do próprio mod e interceptar `Harmony.Patch`. Implementação de referência: [mods/ORBIT-2.1/scripts/patch-dryrun/Program.cs](../../mods/ORBIT-2.1/scripts/patch-dryrun/Program.cs).

| Requisito | Motivo |
|---|---|
| Console em .NET Framework 4.8 | O HarmonyX 2.9 do BepInEx 5 não desvia métodos em .NET 8+, e o programa precisa aplicar um patch de verdade no próprio `Harmony.Patch` |
| `AssemblyResolve` apontando para `Managed/`, `BepInEx/core`, `plugins/spt` e as pastas dos plugins | As DLLs vêm da instalação; nada é copiado |
| Cópia do `Assembly-CSharp` com os tipos delegate marcados `sealed` (Mono.Cecil) | O runtime de desktop recusa delegate não selado; o Mono do jogo aceita |
| Finalizador em `RuntimeAssembly.GetTypes` devolvendo os tipos que carregaram | Alguns tipos do jogo não carregam fora do Mono e `GetTypes()` lançaria |
| Cópia do mod com as chamadas nativas da Unity do logger trocadas por constante | Sem isso todo `Log.Info` lança |
| Arquivo `.ini` ao lado da DLL com `AllowOptimize=0` | Impede o JIT de embutir métodos entre as cópias |
| Carregar as DLLs dos mods opcionais instalados antes de rodar os resolvedores | As buscas de tipo por nome só acham o mod se o assembly estiver carregado, como no jogo |
| Contar como falha todo aviso ou erro que o mod grave no próprio log | Um mod bem escrito desliga o recurso e avisa no log em vez de lançar; sem isso a execução "passa" com o recurso desligado |

O interceptador confere o que o Harmony conferiria ao montar o desvio: alvo não nulo, `__instance`, `__result`, `___campo`, parâmetros por nome (mesmas regras de `HarmonyManipulator.EmitCallParameter`), e roda cada transpiler sobre o IL real obtido com `PatchProcessor.GetOriginalInstructions`.

Prova: alvo resolve nesta instalação, parâmetros ligam, transpilers encontram o IL que esperam, leitores por reflexão não são nulos. Não prova: o efeito do patch em raid.

## 6. O que continua fora do alcance

- O lado 4.1 dos membros que só o decompile 4.0 sustenta: não há assembly 4.1 para comparar.
- Integrações com mods que não estão instalados na máquina.
- Outras instalações: SAIN, Fika e mods de facção do servidor de produção podem estar em versões diferentes. Rodar a verificação com `--spt-path` contra a instalação alvo.
- Comportamento. Compilar, resolver patch e responder HTTP 200 não são prova de comportamento ([AP-06](spt-antipatterns.md)).
- Patch que no 4.0 cai num método herdado. Um tipo que no 4.1 sobrescreve um método pode só herdá-lo no 4.0; o patch então resolve no método da classe base, junto de outro patch, e recebe instâncias de outro tipo. Dar a cada patch desses uma condição (`Applies`) e não ligá-lo quando o alvo não é declarado na classe esperada.

## Ver também

- [spt4-vs-spt41-gclass-deobfuscation.md](spt4-vs-spt41-gclass-deobfuscation.md) — a tabela de nomes e suas ressalvas
- [spt-antipatterns.md](spt-antipatterns.md) — AP-03, AP-06, AP-09
- [mods/ORBIT-2.1/backlog/001-downgrade-spt41-spt40/](../../mods/ORBIT-2.1/backlog/001-downgrade-spt41-spt40/) — spec técnica com as tabelas completas, code review e as-built

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-09-30 | Guilherme + agente | Criação, a partir do port do ORBIT 2.1 para o SPT 4.0.13 |
| 2026-09-30 | Guilherme | fix(orbit-2.1): make the web panel interactive on SPT 4.0 and fix name lookups found by review |
| 2026-09-30 | Guilherme | docs(orbit-2.1): as-built, consolidated code review, memory, install script and the 4.1->4.0 downgrade guide |
