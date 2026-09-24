# 016 — elite-nao-nativo-clona-boss · Spec Técnica

**Mod:** TRL-DynamicSpawn
**Spec funcional:** [016-elite-nao-nativo-clona-boss-01-spec.md](016-elite-nao-nativo-clona-boss-01-spec.md)
**Criado:** 2026-09-23

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

## 1. Estratégia

Este item **não introduz um novo Harmony patch sobre o Assembly do EFT**. O bug vive inteiramente em código já pertencente ao mod — o bloco que monta os grupos de "elite não-nativo" dentro de `DynamicSpawnManager.ProcessWave`, em [DynamicSpawnManager.cs:690-774](../../modded/Client/Components/DynamicSpawnManager.cs#L690-L774) — e é consumido, sem alteração necessária, por `SpawnGroupBotsCoroutine` ([DynamicSpawnManager.cs:1163-1420](../../modded/Client/Components/DynamicSpawnManager.cs#L1163-L1420)).

A estratégia é:

1. **Separar os dois papéis que hoje compartilham a mesma variável `targetGroupSize`.** Rogues (`exUsec`), Raiders (`pmcBot`) e **Bloodhounds** (`arenaFighterEvent` — decisão do usuário nesta rodada: tratar igual a Rogue/Raiders) continuam podendo formar um "esquadrão" de N clones do mesmo `role` — comportamento correto pra eles, pois são grunts genéricos sem identidade única (ver [DynamicSpawnManager.cs:746](../../modded/Client/Components/DynamicSpawnManager.cs#L746), condição já existente hoje, só mal aproveitada, agora estendida a um terceiro role). Todo o resto (`eliteEntries` com nome de chefe próprio, incluindo Cultistas) passa a spawnar **sempre 1 unidade do próprio chefe** — nunca clone.
2. **Introduzir uma tabela estática somente-leitura `WildSpawnType → WildSpawnType[]`** mapeando cada chefe único ao(s) seu(s) tipo(s) de guarda nativo(s) do EFT. A tabela é extraída diretamente do enum `WildSpawnType` — [WildSpawnType.cs](../../../../references/eft-decompiled/Assembly-CSharp/EFT/WildSpawnType.cs), que já nomeia os pares boss/guarda lado a lado por ID (`followerSanitar=16`/`bossSanitar=17`, `followerTagilla=23` logo após `bossTagilla=22`, etc.) — não é invenção, é leitura direta do enum.
3. **Dobrar o caso especial hoje hardcoded do trio Knight/BigPipe/BirdEye** ([DynamicSpawnManager.cs:734-743](../../modded/Client/Components/DynamicSpawnManager.cs#L734-L743)) dentro dessa mesma tabela genérica — Knight deixa de ser um `if` especial e vira só mais uma entrada do mapa, eliminando duplicação de código sem mudar o resultado observável dele.
4. **Corrigir um segundo bug silencioso na mesma linha de causa raiz:** hoje `GetBossGroupSizeForMap(entry.info, mapName)` (o cap de squad **por mapa**, já exposto no painel via `maxGroupSizeByMap`) é calculado em [DynamicSpawnManager.cs:745](../../modded/Client/Components/DynamicSpawnManager.cs#L745) e imediatamente descartado pra qualquer chefe (a condição em `:746` sempre reescreve `targetGroupSize` de novo). O fix usa esse valor — já correto e já testado pra Rogues/Raiders — como teto do sorteio de guardas também.
5. **Semântica de `GroupChance` esclarecida (PA-01-02):** `GroupChance` decide **se** o grupo de guardas forma, não o tamanho dele. `GroupChance=0` nunca forma grupo (o original tratava `0` e `100` do mesmo jeito, por acidente — os dois pulavam o sorteio; corrigido aqui). `GroupChance>=100` sempre forma grupo, determinístico, no teto do mapa (fidelidade ao original). Entre 0 e 100, sorteia se forma e, em caso positivo, sorteia o tamanho entre 2 e o teto.

**Evidência de que a semântica pretendida já é "chefe + guardas" (não "clones"), vinda do próprio painel Web do mod** — os textos i18n do painel já descrevem o campo compartilhado exatamente como a correção propõe, o código C# é quem está desalinhado com o próprio texto que o mod já publica:
- [Index.razor:579](../../modded/Server/Web/Pages/Index.razor#L579) — `"É possível desativar guardas/seguidores ou ajustar spawns em grupo para Scavs e PMCs."` (distingue explicitamente "guardas/seguidores" de chefes vs. "spawns em grupo" de Scavs/PMCs).
- [Index.razor:921](../../modded/Server/Web/Pages/Index.razor#L921) — `disable_followers: "Desativar Seguidores / Guardas"`, `squad_size_label: "TAMANHO DO ESQUADRÃO"`.

**Alternativas descartadas:**
- *Estender `SpawnGroupBotsCoroutine`/`BotCreationDataClass.Create` pra aceitar múltiplos `WildSpawnType` numa única leva* — descartada porque `BotProfileDataClass` é desenhado pra **um** `WildSpawnType` por instância ([BotProfileDataClass.cs:66](../../../../references/eft-decompiled/Assembly-CSharp/BotProfileDataClass.cs#L66)) e `BotCreationDataClass.Create` recebe um `IGetProfileData` único ([BotCreationDataClass.cs:65](../../../../references/eft-decompiled/Assembly-CSharp/BotCreationDataClass.cs#L65)); forçar múltiplos roles numa chamada exigiria reescrever a criação de perfil pra algo que a API do EFT não foi feita pra fazer.
- *Usar o mecanismo de liderança `BotsGroup`/`Boss.OfferSelf` (linhas 1281-1406) pros guardas* — descartada porque esse mecanismo existe pra suprir IA de "grupo" em roles **genéricos** (exUsec/pmcBot) que não têm reconhecimento nativo de chefe. Guardas com `WildSpawnType` dedicado (`followerSanitar`, etc.) já têm a relação chefe-guarda embutida na IA nativa do EFT — o mesmo padrão que o trio Knight/BigPipe/BirdEye já usa hoje com sucesso, via 3 chamadas independentes de `SpawnGroupBotsCoroutine` com `GroupSize=1` na mesma zona.

## 2. Pontos de patch

Não há novo ponto de patch no Assembly do EFT (nenhum `[HarmonyPrefix]`/`[HarmonyPostfix]` novo). A tabela abaixo lista o ponto de edição no próprio mod e as APIs do EFT que o fix **reaproveita sem alterar** (já usadas hoje pelo caminho correto do Knight e pelo caminho de Rogues/Raiders):

| Alvo | Tipo | Motivo |
|---|---|---|
| [`DynamicSpawnManager.cs:690-774`](../../modded/Client/Components/DynamicSpawnManager.cs#L690-L774) (código do mod) | Reescrita de método próprio | Bloco exato onde `entry.role` é clonado em vez de usar o guarda correto — origem do bug relatado |
| [`WildSpawnType.cs:1-69`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/WildSpawnType.cs) (Assembly, referência) | Sem alteração — só leitura | Fonte dos pares boss/guarda usados na nova tabela `EliteFollowerMap` |
| [`BotProfileDataClass.cs:66`](../../../../references/eft-decompiled/Assembly-CSharp/BotProfileDataClass.cs#L66) (Assembly, referência) | Sem alteração — reutilizado | Confirma que 1 `WildSpawnType` por instância já é a única forma suportada — valida a estratégia de "spawns separados por role" |
| [`BotCreationDataClass.cs:65`](../../../../references/eft-decompiled/Assembly-CSharp/BotCreationDataClass.cs#L65) (Assembly, referência) | Sem alteração — reutilizado | `Create(IGetProfileData, IBotCreator, int count, GInterface22 token)` — assinatura que `SpawnGroupBotsCoroutine` já chama corretamente hoje |
| [`DynamicSpawnManager.cs:1163-1420`](../../modded/Client/Components/DynamicSpawnManager.cs#L1163-L1420) (`SpawnGroupBotsCoroutine`, mod) | Sem alteração | Já spawna corretamente 1 `role` por chamada (`GroupSize=1` = 1 unidade); é o motivo pelo qual o fix não precisa mexer aqui |

## 3. Novas propriedades F12 (BepInEx)

N/A — esta correção não adiciona nem altera nenhuma `ConfigEntry` do `Settings.cs` (F12 / BepInEx ConfigurationManager). Os campos reinterpretados (`groupChance`, `maxGroupSize`, `disableFollowers`, `maxGroupSizeByMap`) pertencem ao painel Web (`config.json` / `EliteLocationInfo`, [TRLConfig.cs:174-187](../../modded/Client/Models/TRLConfig.cs#L174-L187)), fora do escopo de `PROPRIEDADES.md`. O texto do painel já descreve a semântica nova (ver §1) — nenhuma string de UI precisa mudar.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Client/Helpers/EliteFollowerMap.cs` | CRIAR | Tabela estática `WildSpawnType → WildSpawnType[]` com os guardas dedicados de cada chefe único (inclui `sectantPriest → sectantWarrior`), mais `GetFollowers(WildSpawnType)` |
| `modded/Client/Components/DynamicSpawnManager.cs` | MODIFICAR | Reescreve o bloco `foreach (var entry in eliteEntries)` (linhas 690-774): separa caminho grunt-squad (Rogues/Raiders/**Bloodhounds**, inalterado no mecanismo) de caminho boss+guardas (novo, cobre Cultistas); remove o `if` especial do Knight (absorvido pela tabela); troca `entry.info.MaxGroupSize` por `GetBossGroupSizeForMap(...)` no cálculo de guardas |
| `modded/Client/Models/TRLConfig.cs` | MODIFICAR | Initializer de `Bloodhounds` (linha 156) ganha `MaxGroupSize = 4` e `MaxGroupSizeByMap` com todas as entradas em `4` (era 3), fallback em código caso `config.json` não exista |
| `modded/Server/config/config.json` | MODIFICAR | Bloco `arenaFighterEvent`: `maxGroupSize` e todas as entradas de `maxGroupSizeByMap` de `3` para `4` (config ativa do servidor) |
| `modded/Server/config/config.default.json` | MODIFICAR | Mesmo ajuste do arquivo acima, no template usado pelo botão "PADRÃO" / `/resetConfig` |

## 5. Stubs de código

```csharp
// modded/Client/Helpers/EliteFollowerMap.cs
using System;
using System.Collections.Generic;
using EFT;

namespace TRLDynamicSpawn.Helpers
{
    /// <summary>
    /// Mapeia cada chefe único não-nativo ao(s) seu(s) tipo(s) de guarda dedicado do EFT.
    /// Fonte: enum WildSpawnType.
    /// // ref: Assembly-CSharp/EFT/WildSpawnType.cs:1-69
    /// Chefes ausentes deste mapa (bossKilla, bossPartisan, gifter) não têm guarda dedicado por
    /// WildSpawnType no EFT e devem sempre nascer sozinhos. arenaFighterEvent (Bloodhounds) nunca
    /// consulta este mapa — vai pelo caminho de esquadrão genérico (ver isGruntSquad no chamador).
    /// ref: CR-016-01 — item 016 (elite-nao-nativo-clona-boss)
    /// </summary>
    public static class EliteFollowerMap
    {
        private static readonly WildSpawnType[] Empty = Array.Empty<WildSpawnType>();

        private static readonly Dictionary<WildSpawnType, WildSpawnType[]> Map = new()
        {
            // ref: Assembly-CSharp/EFT/WildSpawnType.cs:31 (bossKnight=26) / :32,33 (followerBigPipe=27, followerBirdEye=28)
            { WildSpawnType.bossKnight,   new[] { WildSpawnType.followerBigPipe, WildSpawnType.followerBirdEye } },
            // ref: Assembly-CSharp/EFT/WildSpawnType.cs:27,28 (bossTagilla=22 / followerTagilla=23)
            { WildSpawnType.bossTagilla,  new[] { WildSpawnType.followerTagilla } },
            // ref: Assembly-CSharp/EFT/WildSpawnType.cs:34,35 (bossZryachiy=29 / followerZryachiy=30)
            { WildSpawnType.bossZryachiy, new[] { WildSpawnType.followerZryachiy } },
            // ref: Assembly-CSharp/EFT/WildSpawnType.cs:16-20 (bossGluhar=11 / followerGluhar{Assault,Security,Scout,Snipe}=12..15)
            { WildSpawnType.bossGluhar,   new[] { WildSpawnType.followerGluharAssault, WildSpawnType.followerGluharSecurity, WildSpawnType.followerGluharScout, WildSpawnType.followerGluharSnipe } },
            // ref: Assembly-CSharp/EFT/WildSpawnType.cs:21,22 (followerSanitar=16 / bossSanitar=17)
            { WildSpawnType.bossSanitar,  new[] { WildSpawnType.followerSanitar } },
            // ref: Assembly-CSharp/EFT/WildSpawnType.cs:47-49 (bossKolontay=43 / followerKolontay{Assault,Security}=44,45)
            { WildSpawnType.bossKolontay, new[] { WildSpawnType.followerKolontayAssault, WildSpawnType.followerKolontaySecurity } },
            // ref: Assembly-CSharp/EFT/WildSpawnType.cs:8,10 (bossBully=3 / followerBully=5) — painel chama esse chefe de "Reshala"
            { WildSpawnType.bossBully,    new[] { WildSpawnType.followerBully } },
            // ref: Assembly-CSharp/EFT/WildSpawnType.cs:36,37,45,46 (bossBoar=32 / followerBoar=33 / followerBoarClose1=41 / followerBoarClose2=42) — painel chama esse chefe de "Kaban"
            { WildSpawnType.bossBoar,     new[] { WildSpawnType.followerBoar, WildSpawnType.followerBoarClose1, WildSpawnType.followerBoarClose2 } },
            // ref: Assembly-CSharp/EFT/WildSpawnType.cs:12,13 (bossKojaniy=7 / followerKojaniy=8) — painel chama esse chefe de "Shturman"
            { WildSpawnType.bossKojaniy,  new[] { WildSpawnType.followerKojaniy } },
            // ref: Assembly-CSharp/EFT/WildSpawnType.cs:25,26 (sectantWarrior=20 / sectantPriest=21) — decisão do usuário
            // nesta rodada: sectantWarrior conta como guarda dedicado do Cultist (padrão de nome difere de
            // "followerX", mas o par é a fonte real do evento de Cultistas do EFT).
            { WildSpawnType.sectantPriest, new[] { WildSpawnType.sectantWarrior } },
        };

        public static WildSpawnType[] GetFollowers(WildSpawnType bossRole)
        {
            return Map.TryGetValue(bossRole, out var followers) ? followers : Empty;
        }
    }
}
```

```csharp
// Trecho de modded/Client/Components/DynamicSpawnManager.cs — substitui integralmente
// o bloco atual em DynamicSpawnManager.cs:687-774 (do comentário "PROCESS NON-NATIVE
// ELITES / ROGUES / BOSSES" até o fechamento do `if (isFirstWave && ...)`).
// ref: CR-016-01 — item 016 (elite-nao-nativo-clona-boss)

// ======================================
// PROCESS NON-NATIVE ELITES / ROGUES / BOSSES
// ======================================
if (isFirstWave && !RaidInitialElitesSpawned && eliteConfig != null && !eliteConfig.DisableBosses)
{
    var eliteEntries = new (EliteLocationInfo info, WildSpawnType role, string bossName)[]
    {
        (eliteConfig.Rogues, WildSpawnType.exUsec, "exusec"),
        (eliteConfig.Raiders, WildSpawnType.pmcBot, "pmcbot"),
        (eliteConfig.Bloodhounds, WildSpawnType.arenaFighterEvent, "arenafighterevent"),
        (eliteConfig.Cultists, WildSpawnType.sectantPriest, "sectantpriest"),
        (eliteConfig.BossKnight, WildSpawnType.bossKnight, "bossknight"),
        (eliteConfig.BossTagilla, WildSpawnType.bossTagilla, "bosstagilla"),
        (eliteConfig.BossKilla, WildSpawnType.bossKilla, "bosskilla"),
        (eliteConfig.BossZryachiy, WildSpawnType.bossZryachiy, "bosszryachiy"),
        (eliteConfig.BossGluhar, WildSpawnType.bossGluhar, "bossgluhar"),
        (eliteConfig.BossSanitar, WildSpawnType.bossSanitar, "bosssanitar"),
        (eliteConfig.BossKolontay, WildSpawnType.bossKolontay, "bosskolontay"),
        (eliteConfig.BossReshala, WildSpawnType.bossBully, "bossbully"),
        (eliteConfig.BossKaban, WildSpawnType.bossBoar, "bossboar"),
        (eliteConfig.BossShturman, WildSpawnType.bossKojaniy, "bosskojaniy"),
        (eliteConfig.BossPartisan, WildSpawnType.bossPartisan, "bosspartisan"),
        (eliteConfig.BossGifter, WildSpawnType.gifter, "gifter")
    };

    foreach (var entry in eliteEntries)
    {
        if (entry.info == null || !entry.info.Enable) continue;

        // Regra Vanilla: Cultistas (sectantPriest) só nascem de noite (22:00 às 06:00)
        if (entry.role == WildSpawnType.sectantPriest && !IsNightTimeForCultists())
        {
            Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Skipping Cultists (sectantPriest) spawn on {mapName}: Raid hour is day time (Cultists only spawn between 22:00 and 06:00).");
            continue;
        }

        int spawnChance = GetBossChanceForMap(entry.info.SpawnChance, mapName);
        if (spawnChance <= 0) continue;

        // Se o mapa JÁ possui uma wave vanilla nativa para este chefe/elite (ex: Rogues no Lighthouse
        // ou Reshala no Customs), a wave vanilla é ajustada no AdjustVanillaBossWaves() e controlada
        // nativamente — caminho intocado por este fix.
        if (HasNativeVanillaWave(entry.bossName)) continue;

        // Se NÃO possui wave nativa no mapa, geramos o spawn dinamicamente.
        if (UnityEngine.Random.Range(1, 101) > spawnChance) continue;

        BotZone selectedZone = GetZoneFromConfig(entry.info, mapName, entry.role);
        bool isGruntSquad = entry.role == WildSpawnType.exUsec
            || entry.role == WildSpawnType.pmcBot
            || entry.role == WildSpawnType.arenaFighterEvent; // Bloodhounds — decisão do usuário: igual Rogue/Raiders

        if (isGruntSquad)
        {
            // Rogues/Raiders/Bloodhounds são esquadrões genéricos sem identidade única: "grupo" é
            // literalmente N clones do mesmo role. Mecanismo inalterado (ref: CR-016-01) — quando
            // squadSize > 1, SpawnGroupBotsCoroutine (:1256-1420, não tocado) JÁ designa o membro 0
            // como líder (Boss.IamBoss) e liga o resto como seguidores via BotsGroup/Boss.OfferSelf,
            // com sucessão de liderança se o líder morrer durante o escalonamento. Nenhum código novo
            // precisa ser escrito pra Bloodhounds ganhar líder — é o mesmo caminho que Rogues/Raiders
            // já usam hoje.
            int squadSize = GetBossGroupSizeForMap(entry.info, mapName);
            Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Non-Native Elite Invasion: Queueing squad of {squadSize}x {entry.role} ({entry.bossName}) on {mapName} in zone '{selectedZone?.NameZone ?? "Random"}' (Chance: {spawnChance}%)...");
            spawnList.Add(new Tuple<SpawnGroupData, BotZone>(new SpawnGroupData { Role = entry.role, Difficulty = BotDifficulty.normal, GroupSize = squadSize, Info = entry.info }, selectedZone));
            continue;
        }

        // Todo chefe único nasce como 1 unidade — nunca clone. groupChance/maxGroupSize/disableFollowers
        // passam a decidir SE e QUANTOS guardas dedicados dele acompanham (ref: CR-016-01).
        Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Non-Native Elite Invasion: Queueing {entry.bossName} (solo boss) on {mapName} in zone '{selectedZone?.NameZone ?? "Random"}' (Chance: {spawnChance}%)...");
        spawnList.Add(new Tuple<SpawnGroupData, BotZone>(new SpawnGroupData { Role = entry.role, Difficulty = BotDifficulty.normal, GroupSize = 1, Info = entry.info }, selectedZone));

        if (entry.info.DisableFollowers) continue;

        WildSpawnType[] followerRoles = EliteFollowerMap.GetFollowers(entry.role);
        if (followerRoles.Length == 0) continue;

        // GroupChance decide SE o grupo de guardas forma — não o tamanho dele (PA-01-02, confirmado
        // pelo usuário). GroupChance=0 nunca forma grupo; GroupChance>=100 sempre forma no teto do
        // mapa (determinístico, sem sorteio de tamanho); entre 0 e 100, sorteia se forma e, se sim,
        // sorteia o tamanho entre 2 e o teto.
        int totalSquadSize = 1;
        if (entry.info.GroupChance > 0)
        {
            // Cap por mapa (maxGroupSizeByMap) em vez do campo flat — antes descartado pra chefes (dead code).
            int maxSquad = Mathf.Max(2, GetBossGroupSizeForMap(entry.info, mapName));
            bool groupForms = entry.info.GroupChance >= 100 || UnityEngine.Random.Range(1, 101) <= entry.info.GroupChance;
            if (groupForms)
            {
                totalSquadSize = entry.info.GroupChance >= 100 ? maxSquad : UnityEngine.Random.Range(2, maxSquad + 1);
            }
        }

        int guardCount = totalSquadSize - 1;
        for (int gIdx = 0; gIdx < guardCount; gIdx++)
        {
            WildSpawnType guardRole = followerRoles[gIdx % followerRoles.Length]; // round-robin entre tipos de guarda
            Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Non-Native Elite Invasion: Queueing guard {gIdx + 1}/{guardCount} ({guardRole}) for {entry.bossName} on {mapName} in zone '{selectedZone?.NameZone ?? "Random"}'...");
            spawnList.Add(new Tuple<SpawnGroupData, BotZone>(new SpawnGroupData { Role = guardRole, Difficulty = BotDifficulty.normal, GroupSize = 1, Info = entry.info }, selectedZone));
        }
    }
}
```

## 6. Fluxo de dados

```
[A] SpawnHordeLoop dispara ProcessWave(isFirstWave=true) — 1ª onda da raid
      (DynamicSpawnManager.cs:550)
  → [B] foreach entry in eliteEntries (DynamicSpawnManager.cs:712)
      rola spawnChance do mapa (GetBossChanceForMap, :1926) →
      pula se HasNativeVanillaWave (:728, caminho vanilla já correto, intocado) →
      isGruntSquad? (exUsec/pmcBot/arenaFighterEvent)
        ├─ SIM → GetBossGroupSizeForMap(:1945) clones do mesmo role → spawnList (inalterado)
        └─ NÃO → 1x boss (GroupSize=1) + EliteFollowerMap.GetFollowers(role) (novo arquivo)
                  → rola groupChance/maxGroupSize (cap por mapa) → N guardas round-robin → spawnList
  → [C] loop de consumo de spawnList (DynamicSpawnManager.cs:1137)
      StartCoroutine(SpawnGroupBotsCoroutine(gData.Role, gData.Difficulty, gData.GroupSize=1, zone))
      — sem alteração, já correto pra GroupSize=1
  → [D] BotCreationDataClass.Create(profile, botCreator, count=1, botSpawner)
      (BotCreationDataClass.cs:65) → BotSpawner.TryToSpawnInZoneAndDelay (:1265)
      → 1 bot nasce com o WildSpawnType exato que [B] decidiu (boss OU guarda, nunca os dois com o mesmo role)
```

## 7. Riscos e dependências

- **Patches existentes em `modded/Patches/`:** nenhum outro patch toca `ProcessWave`/`eliteEntries`. `Patches.cs` e `SpawnGatePatches.cs` mexem em ondas vanilla/marksman (bloqueio de sniper — ver a conversa desta sessão sobre Ground Zero, item separado), sem sobreposição com este bloco.
- **Efeito colateral deliberado, mesma causa raiz:** troca `entry.info.MaxGroupSize` (campo flat) por `GetBossGroupSizeForMap(entry.info, mapName)` (cap por mapa via `maxGroupSizeByMap`) no cálculo do tamanho do esquadrão de guardas. Hoje esse cálculo é feito em `:745` e sempre descartado pra chefes (dead code) — o fix passa a usá-lo, o que já é o comportamento correto e testado pra Rogues/Raiders na mesma função. Registrar no as-build como parte do mesmo fix, não como item novo.
- **`sectantPriest` (Cultistas) — decidido:** entra no `EliteFollowerMap` com `sectantWarrior` como guarda. Efeito: hoje pode formar grupo de 2-3 padres clonados (mesmo bug do Sanitar); depois do fix, sempre 1 padre + 0-N `sectantWarrior` (nunca 2 padres).
- **`arenaFighterEvent` (Bloodhounds) — decidido:** não entra no `EliteFollowerMap` (não há `followerX` dedicado no enum pra esse role); em vez disso passa a ser tratado como esquadrão genérico (mesmo grupo de `isGruntSquad` que Rogues/Raiders). Efeito: deixa de ter chance de "sortear e falhar" (hoje 30% de virar grupo, 70% sozinho) — passa a spawnar sempre como esquadrão completo quando a chance de spawn do mapa é sorteada, do mesmo jeito que Raiders já se comporta hoje. Default de tamanho de esquadrão fixado em **4** (`config.json`/`config.default.json`/`TRLConfig.cs`, ver §4) — o caminho `isGruntSquad` usa `GetBossGroupSizeForMap` direto, sem sorteio de faixa; um valor fixo de 4 (meio-termo do "3 a 5" pedido) evita ter que construir um mecanismo de sorteio de tamanho novo só pra esse grupo (PA-01-02).
- **Compatibilidade com SAIN (quando instalado):** nenhuma API pública do SAIN é tocada. O SAIN lê o `WildSpawnType` de cada bot criado (o mod já checa `IsSainInstalled()` em `SpawnGroupBotsCoroutine`) — passar a usar o role de guarda correto deixa o SAIN mais alinhado com o que ele já espera nativamente para esse tipo de bot, não menos.
- **Ordem de inicialização:** nenhuma mudança. `EliteFollowerMap` é uma tabela estática somente-leitura sem estado por raid — não precisa de `Initialize()`/`Clear()` nem entra no `RaidLifecycle`.

## 8. Checklist de implementação

- [x] Criar `modded/Client/Helpers/EliteFollowerMap.cs` com a tabela de 10 pares boss→guarda(s) (inclui `sectantPriest`→`sectantWarrior`) e `GetFollowers(WildSpawnType)`.
- [x] Substituir o bloco `DynamicSpawnManager.cs:690-774` pelo stub da §5 — remove o `if` especial do Knight (:734-743), separa grunt-squad (Rogues/Raiders/Bloodhounds) de boss+guardas (inclui Cultistas).
- [x] Atualizar `TRLConfig.cs` (linha 156, initializer de `Bloodhounds`): `MaxGroupSize = 4` e `MaxGroupSizeByMap` com todas as entradas em `4`.
- [x] Atualizar `config.json` e `config.default.json`: bloco `arenaFighterEvent.maxGroupSize` e `arenaFighterEvent.maxGroupSizeByMap.*` de `3` para `4`.
- [x] Compilar (`/compile-mod TRL-DynamicSpawn`) e confirmar 0 erros/0 avisos novos. Client: 0 erros/0 avisos. Server: 0 erros/5 avisos (todos pré-existentes — nullability em `ModMetadata.cs`/`TRLRouters.cs`, não relacionados a este fix).
- [ ] Teste manual — Sanitar em Laboratory, `groupChance=100`, `maxGroupSize=3`, `disableFollowers=false`: confirmar 1x `bossSanitar` + sempre 2x `followerSanitar` (teto determinístico em 100%, sem sorteio), nunca 2x `bossSanitar`.
- [ ] Teste manual — mesmo cenário com `groupChance=0`: confirmar Sanitar sempre sozinho, sem guarda (fecha o gap do PA-01-02 — `0` deixa de se comportar como `100`).
- [ ] Teste manual — mesmo cenário com `disableFollowers=true`: confirmar Sanitar sempre sozinho.
- [ ] Teste manual — Rogues e Raiders com `groupChance`/`maxGroupSize` configurados: confirmar squad de clones idêntico ao comportamento pré-fix (sem regressão).
- [ ] Teste manual — Bloodhounds em Customs/Woods: confirmar esquadrão sempre de 4 `arenaFighterEvent` idênticos, com 1 assumindo liderança (`Boss.IamBoss`) automaticamente, sem chance de "sortear e falhar" (sempre esquadrão completo quando a chance de mapa acerta).
- [ ] Teste manual — Cultistas em raid noturna: confirmar 1x `sectantPriest` + 0-N `sectantWarrior` (nunca 2 padres); em raid diurna, continua sem spawnar (regra de horário inalterada).
- [ ] Teste manual — Knight em mapa não-nativo: confirmar trio Knight/BigPipe/BirdEye sai igual a antes (agora via caminho genérico).
- [x] Atualizar `016-elite-nao-nativo-clona-boss-05-asbuild.md` documentando a remoção do caso especial do Knight e as decisões finais sobre Cultistas/Bloodhounds.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | N/A | Não adiciona start/stop hook novo; reaproveita `RaidInitialElitesSpawned` já gerenciado por `RaidLifecycle` (não tocado). Ver §7. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | N/A | Bloco não reage a ação de player; já roda só no host via `DynamicSpawnManagerPatch.cs:25-29` (não tocado). |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | N/A | Nenhum método virtual/ofuscado do EFT é patcheado; edição é em método próprio do mod. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Reusa exatamente `BotProfileDataClass`/`BotCreationDataClass.Create`/`BotSpawner.TryToSpawnInZoneAndDelay` já usados hoje (§1, §5) — só reorganiza quais `WildSpawnType` entram em cada chamada. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | `EliteFollowerMap` é estática somente-leitura sem necessidade de `Clear()`; `RaidInitialElitesSpawned` continua resetado por `RaidLifecycle.OnRaidEnd` (não tocado). Ver §7. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | N/A | Nenhuma `ConfigEntry` nova. Campos de painel reinterpretados já são descritos como "guardas/seguidores"/"tamanho do esquadrão" no próprio `Index.razor:579,921` — código passa a bater com o texto já publicado. |
| 7 | Re-invocação de método patcheado tem reentry-guard — AP-07 | N/A | Sem recursão nem re-invocação de método patcheado. |
| 8 | Flags/caches de intercept validados contra o contexto atual — AP-08 | N/A | Sem cache de operação/arma/tela envolvido. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | `WildSpawnType` ([WildSpawnType.cs](../../../../references/eft-decompiled/Assembly-CSharp/EFT/WildSpawnType.cs)), `BotProfileDataClass` ([:66](../../../../references/eft-decompiled/Assembly-CSharp/BotProfileDataClass.cs#L66)) e `BotCreationDataClass.Create` ([:65](../../../../references/eft-decompiled/Assembly-CSharp/BotCreationDataClass.cs#L65)) conferidos linha a linha nesta sessão. |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Não usa skill do EFT como alavanca. |
| 11 | Pacote FIKA próprio — AP-11 | N/A | Mod não declara `INetSerializable` pra este fluxo; geração de elite é host-only, sem rede. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-23 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-23 | Decisões do usuário incorporadas: `sectantPriest`→`sectantWarrior` entra no `EliteFollowerMap`; `arenaFighterEvent` (Bloodhounds) tratado como esquadrão genérico (igual Rogue/Raiders, líder automático via `SpawnGroupBotsCoroutine` já existente) |
| 2026-09-23 | Review 01 resolvida: PA-01-01 (citações de linha corrigidas), PA-01-02 (semântica de `GroupChance` esclarecida — decide se o grupo forma, não o tamanho; `GroupChance=0` corrigido pra nunca formar grupo; Bloodhounds fixado em 4 membros em vez de sortear 3-5), PA-01-04 (diagrama §6 atualizado) — ver [016-...-03-spec-tech-review-01.md](016-elite-nao-nativo-clona-boss-03-spec-tech-review-01.md) |
