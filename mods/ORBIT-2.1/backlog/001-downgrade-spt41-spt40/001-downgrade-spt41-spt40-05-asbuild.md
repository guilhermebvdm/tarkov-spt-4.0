# 001 — Downgrade SPT 4.1 → 4.0 · As-Built

**Mod:** ORBIT-2.1
**Spec funcional:** [001-downgrade-spt41-spt40-01-spec.md](001-downgrade-spt41-spt40-01-spec.md)
**Spec técnica:** [001-downgrade-spt41-spt40-02-spec-tech.md](001-downgrade-spt41-spt40-02-spec-tech.md)
**Última review técnica:** [001-downgrade-spt41-spt40-03-spec-tech-review-01.md](001-downgrade-spt41-spt40-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-30

> Documentação **pós-implementação**. Reflete o estado real do código. Quando o conteúdo aqui diverge da spec técnica, este documento ganha.

## Estado

| Parte | Compila contra o 4.0.13 | Verificado fora do jogo | Verificado em jogo |
|---|---|---|---|
| Cliente `ORBIT.dll` 2.1.0 | sim | 43 patches + 3 conjuntos manuais aplicam (alvo, parâmetros, transpilers); 8 leitores por reflexão resolvem; 17 nomes de camada cobertos | **não** |
| Addon `Orbit.Fika.dll` 1.1.0 | sim, contra Fika.Core 2.3.21 | 4 postfixes de porta ligam; campos de interação de porta encontrados | **não** |
| Servidor `Orbit.Server.dll` 2.1.0 | sim, `net9.0` | carrega no `SPT.Server` 4.0.13; 13 páginas e 3 rotas respondem; no navegador: navegação, caixa de confirmação, Save | **não** com o cliente do jogo |

Nada foi instalado em `D:\SPT`. O ORBIT 1.2.1 continua sendo o que o jogo carrega.

## Tamanho do port

52 arquivos em `modded/`, 471 linhas adicionadas e 183 removidas contra o upstream `8fd7e661`; 5 arquivos novos. `original/` intacto.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
|---|---|---|
| CRIADO | `modded/Directory.Build.props` | `SptRoot`, `FikaRef`, `SPT_DIR` vêm de `SPT_PATH` ou do `.spt-path`; nenhum caminho fixo nos projetos |
| CRIADO | `modded/Orbit/Compat/Spt40TypeAliases.cs` | 47 `global using` de nome 4.1 para tipo 4.0; compilado também pelo `Orbit.Fika` |
| CRIADO | `modded/Orbit/Compat/Spt40TypeNames.cs` | 17 camadas de IA: tipo 4.0 → nome 4.1, para comparações de texto |
| CRIADO | `modded/Orbit/Compat/Spt40Members.cs` | Nomes 4.0 de 5 métodos, 7 campos e do campo do dono do bot, para buscas por texto |
| CRIADO | `modded/Orbit.Server/Compat/Spt40ServerCompat.cs` | Namespace do logger, ponte `ShowMessageBoxAsync`, `Spt40Paths.AddonFolder` |
| MODIFICADO | `modded/Orbit/Orbit.csproj`, `Orbit.Fika/Orbit.Fika.csproj`, `Orbit.Server/Orbit.Server.csproj` | Sem caminho do autor; sem cópia para `C:\Games\SPT-4.1`; servidor em `net9.0`; addon compila o arquivo de apelidos |
| MODIFICADO | 30 arquivos em `modded/Orbit/` | Membros e genéricos renomeados no ponto de uso (tabelas §5.2 e §5.3 da spec técnica) |
| MODIFICADO | `modded/Orbit/Systems/NativeGhostPartisan.cs` | `Layer(bot)` devolve o nome 4.1 via `Spt40TypeNames.Of` |
| MODIFICADO | `modded/Orbit/Patches/NativeGhostBodyPatches.cs` | `Bind` usa `Spt40Members.OwnerField` e `Spt40Members.Method` |
| MODIFICADO | `modded/Orbit/Patches/NativePatrolDiagnosticPatch.cs` | Alvo `method_0`; parâmetros `___BotOwner_0` e `___Owner` |
| MODIFICADO | `modded/Orbit/Systems/NativeGhostLoot.cs`, `NativeGhostPatrol.cs`, `NativePatrolDiagnostics.cs` | Campos lidos por reflexão passam por `Spt40Members.Field` |
| MODIFICADO | `modded/Orbit.Fika/DoorStateReceiver.cs`, `DoorSyncBridge.cs` | `WorldInteractiveDataPacketStruct`, `GlobalEventHandlerClass`, `vmethod_0`/`vmethod_1` |
| MODIFICADO | `modded/Orbit.Server/ModMetadata.cs` | `AbstractModMetadata` + `IModWebMetadata`, `SptVersion "~4.0.0"` |
| MODIFICADO | `modded/Orbit.Server/Load/OrbitServerLoad.cs`, `Routers/ConfigRouter.cs`, `Routers/ZonesRouter.cs` | `OnLoad()`, `PreSptModLoader + 10`, ação de rota com 4 argumentos |
| MODIFICADO | `modded/Orbit.Server/Web/Shared/MainLayout.razor` | Carrega CSS, fonte e JS do MudBlazor |
| MODIFICADO | `modded/Orbit.Server/Presets/PresetArchive.cs`, `Web/Pages/Presets.razor`, `Web/Shared/PresetScopeDialog.razor` | Caminho de addon do 4.0 |
| CRIADO | `scripts/verify-port.sh`, `scripts/patch-dryrun/`, `scripts/check-type-name-literals.py` | Checkpoints 2 e 3 |
| CRIADO | `scripts/server-smoke-test.ps1` | Checkpoint 4 |
| CRIADO | `scripts/install-to-spt.sh` | Instala no SPT local e desfaz; move o ORBIT 1.2.1 para fora de `plugins` |

## Divergências em relação à spec técnica

| Tema | Spec técnica | Como ficou | Por quê |
|---|---|---|---|
| Categoria E | Só métodos e campo do dono | Também 7 campos lidos por reflexão (`Spt40Members.Field`) | Achados CR-01-04, CR-01-06, CR-01-09 |
| Categoria G | 4 pontos de contrato + arquivo de compatibilidade | Mais o layout (MudBlazor) e o caminho de addon | Achados CR-01-02 e CR-01-05 |
| Checkpoint 2 | Roda `Enable()` dos patches | O Harmony não consegue desviar métodos do jogo fora do processo do jogo; o programa intercepta `Harmony.Patch` e confere o que o Harmony conferiria | O runtime de desktop recusa compilar chamadas nativas da Unity |
| Checkpoint 4 | Conferir carga, páginas e rotas | Script + teste manual no navegador para a parte interativa | HTTP 200 não detectava o defeito CR-01-02 |

## O que cada verificação prova

| Verificação | Prova | Não prova |
|---|---|---|
| `/compile-mod ORBIT-2.1 --no-install` | Tipos e membros acessados por sintaxe C# existem no 4.0 com assinatura compatível | Nada sobre nomes em texto nem sobre comportamento |
| `scripts/verify-port.sh` parte 1 | Cada alvo de patch resolve no jogo instalado; parâmetros de prefixo/postfix/finalizer ligam pelas regras do HarmonyX 2.9; cada transpiler roda sobre o IL real sem lançar; leitores por reflexão em campos estáticos não são nulos; reflexão do SAIN resolve | Que o desvio é aplicado dentro do jogo; o efeito do patch |
| `scripts/verify-port.sh` parte 2 | Todo literal igual a um nome de tipo 4.1 tem linha no `Spt40TypeNames` | Nomes montados em tempo de execução |
| `scripts/server-smoke-test.ps1` | O servidor 4.0.13 carrega o mod; 13 páginas renderizam com os arquivos do MudBlazor; 3 rotas respondem; log sem erro | Cliques e Save (conferidos à mão no navegador em 2026-09-30) |
| Cinco defeitos introduzidos de propósito | Cada um fez a verificação correspondente falhar: linha de campo removida, método trocado por um inexistente, `___campo` errado, linha de camada removida, nome de parâmetro errado | — |

O programa de verificação altera duas coisas nas cópias que carrega, e só nelas: troca `Time.frameCount` por `0` dentro de `Orbit.Log` (chamada nativa da Unity) e marca como `sealed` 107 tipos delegate do `Assembly-CSharp` (o runtime de desktop recusa delegate não selado; o do jogo aceita). 53 tipos do jogo continuam sem carregar fora do jogo e são pulados.

## PA-NN-MM resolvidos durante o build

Ver [001-downgrade-spt41-spt40-03-spec-tech-review-01.md](001-downgrade-spt41-spt40-03-spec-tech-review-01.md).

## Mudanças posteriores

### Code review 01 — 2026-09-30

Aplicados: CR-01-01 a CR-01-09. Aceitos sem mudança: CR-01-10 a CR-01-14. Detalhe em [001-downgrade-spt41-spt40-04-code-review-01.md](001-downgrade-spt41-spt40-04-code-review-01.md).

## Pendências para validação em jogo

1. Instalar: `bash mods/ORBIT-2.1/scripts/install-to-spt.sh` (jogo e servidor fechados).
2. Subir o servidor e conferir no log `ORBIT Server ... carregado`.
3. Abrir o jogo e conferir em `BepInEx/LogOutput.log`: `ORBIT 2.1.0 fully loaded`, `body guards ready (32 entry points)`, `scripted medicine guard ready`, `scoped resume guard ready`, e nenhuma linha `failed to enable` ou `unavailable`.
4. Raid solo com Ghost Mode ligado: esquadrões com objetivo, saque, extração; bots distantes dormindo e acordando.
5. Raid Fika com host ou headless e um cliente, todos com `ORBIT.dll` e `Orbit.Fika.dll` iguais: portas abertas por bot e tiros de combate fantasma no cliente.
6. Raid 1 → saída → raid 2; morte; alt-F4.
7. Desfazer se necessário: `bash mods/ORBIT-2.1/scripts/install-to-spt.sh --rollback`.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-30 | Build inicial e code review 01 aplicada |
