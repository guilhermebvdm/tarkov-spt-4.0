# 002 — Gerenciador Ciclo Natural Estações · As-Built

**Mod:** TRL-WeatherSync
**Spec funcional:** [002-gerenciador-ciclo-natural-estacoes-01-spec.md](002-gerenciador-ciclo-natural-estacoes-01-spec.md)
**Spec técnica:** [002-gerenciador-ciclo-natural-estacoes-02-spec-tech.md](002-gerenciador-ciclo-natural-estacoes-02-spec-tech.md)
**Última review técnica:** [002-gerenciador-ciclo-natural-estacoes-03-spec-tech-review-01.md](002-gerenciador-ciclo-natural-estacoes-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-11

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MOVIDO | `modded/Plugin.cs`, `WeatherSyncSession.cs`, `TRL-WeatherSync.csproj`, `Networking/`, `Patches/`, `References/` → `modded/Client/` | Reestruturação pra caber a convenção `Client/`+`Server/` do repo agora que o mod tem os dois lados (precedente: Skills-Extended, CustomClasses). `HintPath` relativo do `.csproj` preservado — compila igual, verificado com `dotnet build` local. |
| CRIADO | `modded/Server/Server.csproj` | Projeto `net9.0`, `SPTarkov.Reflection` + `SPTarkov.Server.Core` v4.0.2. **Sem** target `CopyToServer`/`PostBuildEvent` (decisão permanente do usuário — nunca auto-instalar no jogo/servidor real). Inclui `<None Include="Config\**\*.*" CopyToOutputDirectory="PreserveNewest" />` pra `season-cycle.json` chegar no `bin/` local (path simplificado 2026-09-11, ver Mudanças posteriores). |
| CRIADO | `modded/Server/SeasonCycleModMetadata.cs` | `AbstractModMetadata` (`ModGuid`, `Name`, `Author`, `Version`, `SptVersion`, `License` + os 5 membros abstratos adicionais descobertos durante o build: `Contributors`/`Incompatibilities`/`ModDependencies`/`Url`/`IsBundleMod`). |
| CRIADO | `modded/Server/Models/SeasonCycleConfig.cs` | Records `SeasonCycleConfig` + `SeasonCycleEntry` — shape do `season-cycle.json`. |
| CRIADO | `modded/Server/SeasonCycleConfigController.cs` | `IOnLoad` — lê `Config/season-cycle.json` (ao lado do assembly instalado) uma vez no boot, com try/catch (`PA-01-01`). |
| CRIADO | `modded/Server/SeasonCycleUpdater.cs` | `IOnLoad` + `IOnUpdate` — calcula a sub-fase ativa por tempo real (`ResolveSeasonFromCycle`) e escreve em `WeatherConfig.OverrideSeason`/`WeatherPresetWeight` a cada ~5s; estação fixa não trava os pesos de clima (`PA-01-04`). |
| CRIADO | `modded/Server/Config/season-cycle.json` | Config padrão de fábrica: ciclo de 28 dias (AUTUMN → AUTUMN_LATE → WINTER → SPRING_EARLY → SPRING → SUMMER), pesos de clima inspirados na proporção ~2:1 do vanilla no Inverno. |
| MODIFICADO | `README.md` | Estrutura de pastas atualizada (`modded/Client/`/`modded/Server/`); nota sobre exigência de `season-cycle.json` idêntico entre peers FIKA + necessidade de reiniciar o SPT Server após editar. |
| MODIFICADO | `PROPRIEDADES.md` | Seção "Item 002" explicando a ausência de `ConfigEntry` novo (config é 100% `.json` server-side). |

## PA-NN-MM resolvidos durante o build

> Todos os pontos da review 01 já haviam sido resolvidos na própria spec técnica antes deste build (ver `03-spec-tech-review-01.md`) — nenhum ponto novo de review foi resolvido diretamente no código além do que a spec já continha. Lista de referência (já ✅/⏭️ na review):

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de Lógica · 🔴 | `try/catch` em `SeasonCycleConfigController.OnLoad()` e `SeasonCycleUpdater.Apply()` + null-guard em `Apply()` — evita que uma falha deste mod derrube o boot do servidor inteiro. |
| PA-01-02 | A — Gap · 🟡 | Caminho alternativo — sem hot-reload; documentado em `README.md` que editar `season-cycle.json` exige reiniciar o SPT Server. |
| PA-01-03 | B — Edge Case · 🟡 | Rejeitado — manipulação de relógio do sistema fora de escopo, decisão consciente do usuário. |
| PA-01-04 | A — Gap · 🟢 | Aceito com modificação — `Apply()` sobrescreve a entrada de peso de clima da estação fixa com os pesos da estação cíclica corrente a cada tick, então "estação fixa" trava só a estética, não a chance de sol/chuva/neve. |
| PA-01-05 | C — Erro de Lógica · 🟢 | `Enum.TryParse`/`Enum.Parse` com `ignoreCase: true` em todos os parses de nome vindos do `.json`. |

## Ajustes descobertos durante o build (além da spec técnica)

> Divergências entre o stub da spec técnica e o que realmente compilou — este documento é a fonte que ganha quando há diferença.

1. **`SeasonCycleModMetadata` — membros abstratos faltando.** O stub da spec técnica (§5) só sobrescrevia `ModGuid`/`Name`/`Author`/`Version`/`SptVersion`/`License`, assumindo (sem confirmar) que `Contributors`/`Incompatibilities`/`ModDependencies`/`Url`/`IsBundleMod` eram opcionais por serem nullable. Uma tentativa de `dotnet build` reprovou com `CS0534` nos 5 membros. Corrigido adicionando os 5 overrides com os mesmos defaults nulos/vazios usados em `mods/Skills-Extended/modded/Server/Metadata.cs`.
2. **`season-cycle.json` — chave `_comment` incompatível com o deserializer real.** Uma primeira versão do arquivo usava um campo JSON `"_comment"` pra documentação — `SPTarkov.Server.Core.Utils.JsonUtil` usa `UnmappedMemberHandling.Disallow` (confirmado lendo `JsonUtil.cs:24`), que rejeita qualquer chave não mapeada no record. Corrigido usando comentários `//` de linha no topo do arquivo — `JsonUtil.cs:20` confirma `ReadCommentHandling.Skip` (comentários `//` são permitidos, só chaves desconhecidas não).
3. **`Server.csproj` — `Resources/` não chegava no `bin/` local.** O stub da spec técnica não incluía nenhum `<None Include>` para os arquivos de config, porque a spec original (antes do risco 🔴 sobre `CopyToServer`) copiava a pasta `Resources/` pro destino via o mesmo MSBuild target que foi removido por instrução do usuário. Sem substituto, `Assembly.GetExecutingAssembly().Location`-relative lookup (`SeasonCycleConfigController.cs`) não acharia `season-cycle.json` nem em teste local. Corrigido com `<None Include="Resources\**\*.*" CopyToOutputDirectory="PreserveNewest" />` — copia só pro `bin/` local do projeto, nunca pro `SPT/user/mods/` real.

Todos os 3 ajustes foram validados via `dotnet build Server.csproj -c Release` (0 erros, 0 avisos) rodado nesta sessão — `season-cycle.json` confirmado presente em `bin/Release/net9.0/Resources/Configs/`.

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

### Rodada 01 (code-review 01, 2026-09-11)

| ID | Resultado | Arquivo(s) |
| --- | --- | --- |
| CR-01-01 | ✅ Aplicado | `SeasonCycleUpdater.cs:19` — `[Injectable(InjectionType.Singleton, null, OnLoadOrder.Database)]` (era sem prioridade). Corrige uma condição de corrida de boot real: `PostDbLoadService.PerformPostDbLoadActions()` (disparado por `GameCallbacks.OnLoad()`, prioridade `300000`) pré-gera e cacheia a previsão de clima uma vez no boot — sem prioridade explícita menor, a(s) primeira(s) raid(s) após cada restart do SPT Server podiam usar a estação real do calendário em vez da do ciclo do mod. |
| CR-01-02 | ✅ Aplicado | `SeasonCycleUpdater.cs` — novo método `ParseWeatherPresetWeights` (usa `Enum.TryParse` por entrada de preset, ignora e loga a entrada inválida em vez de abortar as 6 estações inteiras). Reusado nos dois pontos que antes duplicavam `Enum.Parse` dentro de `.ToDictionary()`. |
| CR-01-03 | ✅ Aplicado | `SeasonCycleUpdater.cs` — `WeatherConfig` (propriedade `=>` removida) virou variável local `weatherConfig` capturada uma vez no topo do `try` de `Apply()`. |

`dotnet build Server.csproj -c Release` validado após a rodada completa: 0 erros, 0 avisos.

### Fix manual (fora de code-review, 2026-09-11) — simplificação de path

Pedido direto do usuário: eliminar o segmento `Resources/` do path do config, tanto no source (`modded/Server/`) quanto no install real (`SPT/user/mods/TRL-WeatherSync/`). Primeiro pedido foi `Configs/` (plural); o próprio usuário corrigiu logo em seguida pra `Config/` (singular) — a tabela abaixo já reflete o resultado final, singular.

| Arquivo | Mudança |
| --- | --- |
| `SeasonCycleConfigController.cs` | `ResourcesDirectory` (apontava pra `<assembly>/Resources`) virou `AssemblyDirectory` (aponta direto pra pasta do assembly); path do config passou de `Path.Combine(ResourcesDirectory, "Configs", "season-cycle.json")` pra `Path.Combine(AssemblyDirectory, "Config", "season-cycle.json")`. |
| `Server.csproj` | `<None Include="Resources\**\*.*">` → `<None Include="Config\**\*.*">`. |
| `modded/Server/Resources/Configs/season-cycle.json` | Movido pra `modded/Server/Config/season-cycle.json` (pasta `Resources/` removida, ficou vazia). |
| `README.md`, `PROPRIEDADES.md` | Referências ao path atualizadas. |

Resultado: instalado, o mod fica `SPT/user/mods/TRL-WeatherSync/Config/season-cycle.json` em vez de `.../Resources/Configs/...`. `dotnet build Server.csproj -c Release` revalidado (build limpo, `rm -rf bin obj` antes, duas vezes — uma por versão de path testada): 0 erros, 0 avisos, `Config/season-cycle.json` confirmado em `bin/Release/net9.0/`.

## Pendências conhecidas (não resolvidas neste build)

- **`/compile-mod` na verdade JÁ suporta `server-csharp`** — descoberto rodando `/compile-mod TRL-WeatherSync` de verdade (2026-09-11), depois deste build. A afirmação anterior ("não suportado", vinda de `docs/technical/spt4-mod-creation.md:142`) estava desatualizada; corrigida na spec técnica §7. **Efeito colateral real:** o script instalou os dois `.dll` automaticamente em `E:/Tarkov Red Line` (não há flag `--no-install`) — violou a instrução permanente do usuário na primeira execução; usuário avisado, decidiu deixar como está dessa vez. Daqui pra frente, usar `--spt-path <inválido>` (registrado em `feedback_compile_mod_install.md`, memória pessoal) pra pular a instalação sem quebrar o build.
- **Nenhum teste in-game.** Nem o client (reestruturado) nem o server (novo) foram validados numa raid real ainda — só compilação confirmada (`dotnet build` manual + `/compile-mod` real, ambos 0 erros/0 avisos). Ver checklist §8/critérios de aceite da spec funcional antes de considerar o item pronto pra uso. **Atenção:** os `.dll` que já estão na instalação real do usuário (`E:/Tarkov Red Line`, escritos sem querer pelo `/compile-mod` antes do code-review) são a versão v1.0.0 **pré-fix** — nem os 3 achados do code-review 01 nem a simplificação de path (`Resources/Configs/` → `Config/`) estão lá. As versões corrigidas ficaram só em `mods/TRL-WeatherSync/builds/` (compiladas de propósito sem instalar, `--spt-path` inválido). Usuário precisa copiar manualmente antes de testar de verdade.
- **P-2.1 (memória do mod, item 001):** sincronizar o FIM de uma tempestade forçada continua sem solução — não relacionado a este item, mas segue aberta.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-11 | Build concluído via `/code-mod` — 6 arquivos criados no lado servidor, 6 arquivos movidos no lado client (reestruturação `Client/`+`Server/`), `README.md`/`PROPRIEDADES.md` atualizados. `dotnet build` manual validado nos dois lados (0 erros/0 avisos). Nenhuma instalação automática em pasta real de jogo/servidor, por instrução permanente do usuário. Testes in-game ainda pendentes. |
| 2026-09-11 | `/compile-mod TRL-WeatherSync` rodado (sem o cuidado de `--spt-path` inválido) — build real confirmado 0 erros/0 avisos nos dois lados (client v1.1.0→1.1.1, server v1.0.0 primeira medição), mas o script instalou os dois `.dll` automaticamente em `E:/Tarkov Red Line` (descoberta: `/compile-mod` já suporta `server-csharp`, doc do repo estava desatualizada). Usuário avisado via `AskUserQuestion`, decidiu deixar os arquivos instalados como estão dessa vez. Memória pessoal (`feedback_compile_mod_install.md`) e memória do mod atualizadas com a técnica correta pra próximas compilações. |
| 2026-09-11 | Aplicação de 3 achados de code-review 01 via `/apply-code-review` — CR-01-01 (bug real de ordem de boot vs. cache de previsão de clima do SPT), CR-01-02 (parse defensivo de preset de clima) e CR-01-03 (micro-otimização). Rodada 01 de code-review fechada, 3/3 aplicados, 0 bloqueadores restantes. `dotnet build` validado: 0 erros/0 avisos. |
| 2026-09-11 | `/compile-mod TRL-WeatherSync --spt-path <inválido> --allow-same-version` — desta vez sem tocar na instalação real (confirmado por timestamp). Client `1.1.1` (sem mudança), server `1.0.0 → 1.0.1`. Builds corrigidos disponíveis só em `mods/TRL-WeatherSync/builds/` — instalação real ainda com a versão pré-fix. |
| 2026-09-11 | Fix manual (pedido do usuário, fora do fluxo de code-review): path do config simplificado de `Resources/Configs/` pra `Config/` (usuário pediu `Configs/` primeiro, corrigiu pra singular em seguida). Server `1.0.1 → 1.0.2`. `/compile-mod TRL-WeatherSync --spt-path <inválido> --allow-same-version --clean` — build limpo (sem leftover de `Resources/Configs/` de builds anteriores), 0 erros/0 avisos, nada escrito na instalação real (confirmado). `builds/server/` agora reflete exatamente o estado final: `TRLWeatherSyncSeasonCycle.dll`/`.pdb`/`.deps.json` + `Config/season-cycle.json`. |
