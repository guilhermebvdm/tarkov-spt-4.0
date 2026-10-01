# 014 — Invasor de raid não vira inimigo dos bots (corrida com registro de jogador vivo) · Spec Técnica

**Mod:** FIKA
**Spec funcional:** [014-invasor-nao-vira-inimigo-bots-01-spec.md](014-invasor-nao-vira-inimigo-bots-01-spec.md)
**Criado:** 2026-09-24

> Fonte primária: `references/eft-decompiled/Assembly-CSharp/` pra confirmar o mecanismo vanilla da corrida, e código do próprio FIKA (`mods/FIKA/modded-V2/`) pra localizar o ponto de patch — este item não patcheia o Assembly do EFT (o alvo real, `BotsGroup.AddEnemy`, é vanilla profundo e usado em todo o jogo; patchá-lo é fora de escopo e mais arriscado). A correção é uma **guarda defensiva antes da chamada existente**, dentro do código já nosso do FIKA.

## 1. Estratégia

Sem patch Harmony novo — **modificação direta** de `CoopHandler.AddClientToBotEnemies` (`mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/Main/Components/CoopHandler.cs:534-572`), a coroutine que já existe e já é responsável por registrar um jogador recém-chegado como inimigo possível de cada grupo de bots.

**Causa raiz confirmada no Assembly (vanilla, não é bug nosso nem do SAIN):**

- `BotsGroup.AddEnemy(IPlayer person, EBotEnemyCause cause)` ([`BotsGroup.cs:634-715`](../../../../references/eft-decompiled/Assembly-CSharp/BotsGroup.cs#L634-L715)) é o método que cada grupo de bots usa pra tentar registrar um novo inimigo. Em [`BotsGroup.cs:683-687`](../../../../references/eft-decompiled/Assembly-CSharp/BotsGroup.cs#L683-L687):
  ```csharp
  Player alivePlayerByProfileID = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(person.ProfileId);
  if (alivePlayerByProfileID == null)
  {
      return false;
  }
  ```
  Se `GetAlivePlayerByProfileID` retornar `null` nesse instante, o método **retorna `false` silenciosamente** — sem lançar exceção, sem logar nada, sem disparar `OnEnemyAdd` (linha 703, nunca alcançada), e **sem nenhum mecanismo de nova tentativa depois**. É uma falha permanente pra aquele grupo especificamente.
- `GameWorld.GetAlivePlayerByProfileID(string profileID)` ([`GameWorld.cs:1238-1245`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L1238-L1245)) consulta o dicionário `allAlivePlayersByID` ([`GameWorld.cs:554`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L554)).
- Esse dicionário só é populado dentro de `GameWorld.RegisterPlayer(IPlayer iPlayer)` ([`GameWorld.cs:2260-2278`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L2260-L2278), atribuição em `:2273`) — ou seja, existe uma janela de tempo entre "o jogador foi instanciado/spawnado" e "o jogador foi de fato registrado como vivo no `GameWorld`", e se o registro nos grupos de bots (disparado pelo FIKA) correr **antes** dessa janela fechar, a falha é garantida e silenciosa.

**Caminho do FIKA que dispara o registro (o que já existe, sem checar essa pré-condição):**

- `CoopHandler.SpawnPlayer` ([`CoopHandler.cs:346-368`](../../modded-V2/Fika-Plugin/Fika.Core/Main/Components/CoopHandler.cs#L346-L368) — só no host, `FikaBackendUtils.IsServer`) chama `StartCoroutine(AddClientToBotEnemies(botController, otherPlayer))` (linha 357) logo depois de `otherPlayer` ser criado por `SpawnObservedPlayer` (linha 338).
- `AddClientToBotEnemies` ([`CoopHandler.cs:534-572`](../../modded-V2/Fika-Plugin/Fika.Core/Main/Components/CoopHandler.cs#L534-L572)) espera a raid estar rodando e o `BotSpawner` existir (linhas 538-546 — ambos já satisfeitos numa invasão de raid em andamento, então passam direto sem `yield`), e então chama `botController.AddActivePLayer(playerToAdd)` (linha 551) **sem checar em nenhum momento se o jogador já está registrado como vivo no `GameWorld`** — essa é exatamente a pré-condição que falta.
- `BotsController.AddActivePLayer(Player player)` ([`BotsController.cs:680-686`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/BotsController.cs#L680-L686)) repassa pra `BotSpawner_1.AddPlayer(player)` (linha 684), que internamente itera todos os `BotsGroup` já existentes chamando `AddEnemy`/`AddNeutral` (confirmado por investigação nesta sessão, sem link de código próprio pois `BotSpawner`/`GClass575` ficam fora do escopo direto deste patch).
- A verificação que o FIKA já faz depois ([`CoopHandler.cs:553-572`](../../modded-V2/Fika-Plugin/Fika.Core/Main/Components/CoopHandler.cs#L553-L572)) confere se o jogador está na lista geral do `BotSpawner` (`GetPlayer(i) == playerToAdd`) — **não** confere se cada `BotsGroup` individualmente conseguiu adicioná-lo como inimigo (`BotsGroup.Enemies`, um dicionário interno diferente). Por isso a falha em `BotsGroup.AddEnemy` nunca aparece nesse log, nem como sucesso nem como erro.

**Correção:** adicionar uma nova espera, **antes** da chamada a `AddActivePLayer`, checando `Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(playerToAdd.ProfileId) != null` — bloqueado até confirmar, com um teto de tentativas (a spec funcional exige limite, não espera indefinida). Isso resolve a corrida na origem, sem precisar patchear `BotsGroup.AddEnemy` (vanilla, usado em todo o jogo, alto risco).

**Alternativas descartadas:**
- Patchear `BotsGroup.AddEnemy` diretamente (Harmony) pra adicionar retry lá dentro — descartado: é vanilla profundo, chamado em todo cenário de IA do jogo (não só invasão de raid FIKA), risco de efeito colateral muito maior que o ganho; a guarda no lado FIKA já resolve o caso concreto sem tocar em comportamento vanilla nenhum.
- Verificar sucesso inspecionando `BotsGroup.Enemies` diretamente via reflexão após a chamada (verificação pós-fato em vez de pré-condição) — descartado: exigiria reflexão em campo provavelmente privado de uma classe vanilla, mais frágil que simplesmente esperar a pré-condição já conhecida (`GetAlivePlayerByProfileID`) ficar satisfeita antes de chamar.

## 2. Pontos de patch

| Alvo | Tipo | Motivo |
|---|---|---|
| [`CoopHandler.cs:534-572`](../../modded-V2/Fika-Plugin/Fika.Core/Main/Components/CoopHandler.cs#L534-L572) (`AddClientToBotEnemies`) | Modificação direta | Adiciona espera com teto por `GetAlivePlayerByProfileID != null` antes de `AddActivePLayer`. |

Referências ao Assembly do EFT usadas pra confirmar a causa raiz (não são pontos de patch): `BotsGroup.cs:634-715`, `GameWorld.cs:1238-1245`, `GameWorld.cs:2260-2278`, `BotsController.cs:122`, `:680-686` (ver §1).

## 3. Novas propriedades F12 (BepInEx)

N/A — correção de bug, sem trade-off de UX que justifique um toggle novo. O teto de tentativas/tempo fica como constante interna, não exposto no F12.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `Fika-Plugin/Fika.Core/Main/Components/CoopHandler.cs` | MODIFICAR | `AddClientToBotEnemies` ganha espera com teto por `GetAlivePlayerByProfileID`, e loga explicitamente (sem gate de DEBUG) se o teto for atingido sem sucesso. |
| `Fika-Plugin/Fika.Core/FikaPlugin.cs` | MODIFICAR | Bump de versão (patch — fix de bug). |
| `Fika-Plugin/Fika.Core/Fika.Core.csproj` | MODIFICAR | `<Version>` acompanha o bump acima. |

## 5. Stubs de código

```csharp
// Main/Components/CoopHandler.cs — trecho de AddClientToBotEnemies (corpo já existente, só a nova espera é inserida)
private IEnumerator AddClientToBotEnemies(BotsController botController, LocalPlayer playerToAdd)
{
    var coopGame = LocalGameInstance;
    _logger.LogInfo($"AddClientToBotEnemies: {playerToAdd.Profile.GetCorrectedNickname()}");
    while (coopGame.GameController.GameInstance.Status != GameStatus.Running && !botController.IsEnable)
    {
        yield return null;
    }

    while (botController.BotSpawner == null)
    {
        yield return null;
    }

    // ref: Assembly-CSharp/BotsGroup.cs:683-687 — BotsGroup.AddEnemy retorna false
    // silenciosamente (sem log, sem evento, sem retry) se GetAlivePlayerByProfileID
    // ainda não reconhece este jogador como vivo no instante exato da chamada.
    // Confirmado em Assembly-CSharp/EFT/GameWorld.cs:1238-1245 (consulta) e
    // :2260-2278 (só populado dentro de RegisterPlayer). Esperar essa condição
    // ANTES de chamar AddActivePLayer fecha a corrida na origem (backlog 014).
    //
    // Teto em SEGUNDOS REAIS (Time.time), não em contagem de frames — um host
    // headless não renderiza nada, então seu framerate é imprevisível (pode
    // rodar muito acima ou abaixo de 60fps conforme carga); contar frames daria
    // um teto de tempo real sem garantia nenhuma (review 01, PA-01-01).
    const float AliveCheckTimeoutSeconds = 5f;
    var aliveCheckDeadline = Time.time + AliveCheckTimeoutSeconds;
    var gameWorld = Singleton<GameWorld>.Instance;
    while (gameWorld.GetAlivePlayerByProfileID(playerToAdd.ProfileId) == null)
    {
        if (Time.time >= aliveCheckDeadline)
        {
            // Log incondicional (sem #if DEBUG) -- a falha original era 100% silenciosa,
            // este é justamente o rastro que a spec funcional exige mesmo se esgotar.
            _logger.LogError($"AddClientToBotEnemies: {playerToAdd.Profile.GetCorrectedNickname()} nunca foi reconhecido como vivo no GameWorld após {AliveCheckTimeoutSeconds}s -- abortando registro nos bots.");
            yield break;
        }
        yield return null;
    }

#if DEBUG
    _logger.LogInfo($"Adding Client {playerToAdd.Profile.GetCorrectedNickname()} to enemy list");
#endif
    botController.AddActivePLayer(playerToAdd);

    var found = false;

    for (var i = 0; i < botController.BotSpawner.PlayersCount; i++)
    {
        if (botController.BotSpawner.GetPlayer(i) == playerToAdd)
        {
            found = true;
            break;
        }
    }

    if (found)
    {
#if DEBUG
        _logger.LogInfo($"Verified that {playerToAdd.Profile.GetCorrectedNickname()} was added to the enemy list.");
#endif
        yield break;
    }

    _logger.LogError($"Failed to add {playerToAdd.Profile.GetCorrectedNickname()} to the enemy list.");
}
```

## 6. Fluxo de dados

```
Cenário A (jogador entra no início normal da raid -- sem regressão):
[Host] SpawnPlayer -> SpawnObservedPlayer cria o jogador -> RegisterPlayer (GameWorld.cs:2260-2278) já
  roda antes/junto -> AddClientToBotEnemies: GetAlivePlayerByProfileID já não-nulo de cara
  -> 0 iterações extras de espera -> AddActivePLayer roda exatamente como hoje. Sem mudança perceptível.

Cenário B (invasão de raid em andamento -- bug relatado, corrigido):
[Host] SpawnPlayer -> SpawnObservedPlayer -> AddClientToBotEnemies dispara
  -> GetAlivePlayerByProfileID ainda retorna null (corrida) -> nova espera (yield return null)
  -> alguns frames depois, GameWorld.RegisterPlayer roda -> GetAlivePlayerByProfileID passa a
  retornar o jogador -> AddActivePLayer roda -> BotsGroup.AddEnemy (BotsGroup.cs:683-687)
  encontra o jogador vivo -> registro bem-sucedido em todo grupo -> bots percebem o invasor.

Cenário C (falha real, não é só corrida -- ex.: algo mais está quebrado):
[Host] espera até o teto (MaxAliveCheckAttempts) sem GetAlivePlayerByProfileID nunca resolver
  -> log de erro incondicional, visível sem precisar de build DEBUG -> investigação futura
  tem rastro, ao contrário do silêncio total de hoje.
```

## 7. Riscos e dependências

- **Coroutine e fim de raid:** `AddClientToBotEnemies` já é iniciada via `StartCoroutine` diretamente em `CoopHandler` ([`CoopHandler.cs:357`](../../modded-V2/Fika-Plugin/Fika.Core/Main/Components/CoopHandler.cs#L357)), sem `MonoBehaviour` alvo explícito diferente — ou seja, já roda sob o ciclo de vida do próprio `CoopHandler`. Unity para automaticamente todas as coroutines de um `MonoBehaviour` quando ele é destruído/desabilitado; como `CoopHandler` é raid-scoped, uma tentativa pendente (incluindo a nova espera adicionada aqui) já é descartada corretamente no fim de raid **sem precisar de código adicional** — resolve o corner case "raid termina com tentativa de registro ainda pendente" da spec funcional por construção do próprio Unity, não por uma correção nova.
- **`BotsGroup.AddEnemy` tem outros motivos legítimos de rejeição** (`USE_ADD_TO_ENEMY_VALIDATION`/`VALID_REASONS_TO_ADD_ENEMY`, `REACT_ADD_DRUNK_ENEMY`, lógica de boss em `BotsGroup.cs:664-681`) que **não são a corrida corrigida aqui** — este item não tenta contornar essas rejeições, só a específica de "jogador ainda não vivo no instante exato". Se um bot legitimamente rejeitar o jogador por outro motivo do jogo, esse comportamento permanece inalterado.
- **Item 010 (Join in Progress):** cenário onde essa corrida fica exposta com mais probabilidade (mais passos assíncronos até o jogador estar pronto). Nenhuma mudança de código no item 010; só o cenário de teste principal.
- **`fika-packet-desync-prevention-plan.md`:** não aplicável diretamente — esta correção não envolve serialização de pacote nem `_dataWriter` compartilhado, só uma espera local antes de uma chamada síncrona já existente.
- **Diagnóstico ainda não confirmado em log real (marcado na spec funcional):** o mecanismo da corrida foi confirmado por leitura direta do Assembly, mas a spec funcional já registra que falta confirmação por instrumentação em raid real. Recomendo manter o log de erro incondicional (§5) mesmo após validar, já que ele documenta qualquer falha real futura (inclusive as legítimas do parágrafo anterior, que também não tinham rastro antes).

## 8. Checklist de implementação

- [x] Adicionar a nova espera com teto em `AddClientToBotEnemies` (`CoopHandler.cs`), logo após o guard existente de `BotSpawner == null` e antes de `AddActivePLayer`, conforme stub §5.
- [ ] Confirmar que o cenário de entrada normal (não-invasão) não introduz nenhuma espera perceptível (0 iterações, já que `GetAlivePlayerByProfileID` já deve estar resolvido nesse fluxo).
- [x] Bump de versão (patch) em `FikaPlugin.cs` e `Fika.Core.csproj` — 2.4.5 → 2.4.6.
- [ ] Build Release (0 erros) isolado no fork `modded-V2` — pendente `/compile-mod`.
- [ ] Validar em raid real: invadir uma raid em andamento via headless (cenário original do bug) e confirmar que os bots já ativos passam a reagir ao jogador invasor.
- [ ] Validar em raid real: entrada normal no início da raid continua funcionando sem regressão perceptível de timing.
- [ ] Validar em raid real: vários jogadores invadindo quase ao mesmo tempo (corner case da spec funcional) — cada um registrado corretamente.
- [ ] Se possível, forçar/simular o cenário de esgotamento do teto (ex.: reduzir `AliveCheckTimeoutSeconds` temporariamente em teste) pra confirmar que o log de erro incondicional aparece como esperado.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes (`GameWorld.OnDestroy` + `BaseLocalGame.Stop`) — AP-01 | N/A | Não introduz hook de lifecycle novo; a coroutine já existente continua sob o ciclo de vida do `CoopHandler` (ver §7, Unity para coroutines automaticamente na destruição do MonoBehaviour). |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | Roda exclusivamente no host (`FikaBackendUtils.IsServer`, guard já existente em `CoopHandler.cs:346`, não modificado); a nova espera se aplica a qualquer jogador (humano) que entra na raid, igual ao comportamento já existente — não distingue host/convidado além do que já era distinguido. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | N/A | Não há patch sobre método virtual/ofuscado do EFT; a mudança é uma espera adicionada dentro de uma coroutine já existente do próprio FIKA, chamando um método público (`GetAlivePlayerByProfileID`) direto, sem dispatch virtual. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Usa a API pública e já leitura-somente `GameWorld.GetAlivePlayerByProfileID` ([`GameWorld.cs:1238`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L1238)) só como condição de espera — não muta nenhum estado do EFT; a mutação real (`AddActivePLayer`) já existia e não muda. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | Ver §7 — a espera nova vive dentro da mesma coroutine raid-scoped já existente; para automaticamente com a destruição do `CoopHandler` no fim de raid, sem estado adicional a descartar manualmente. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | N/A | Nenhuma `ConfigEntry` nova (§3). |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` (sem recursão infinita) — AP-07 | N/A | Não é patch Harmony; não há re-invocação do método alvo. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca (arma/operação/tela) — AP-08 | N/A | Não há cache/flag de intercept; a condição de espera (`GetAlivePlayerByProfileID`) é sempre lida ao vivo do `GameWorld`, nunca cacheada. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump (não só no recon); "não existe" conferido no `types-index.json`, nunca num grep vazio — AP-09 | ✅ | Todas as refs (`BotsGroup.cs:634-715`, `GameWorld.cs:1238-1245`, `:2260-2278`, `BotsController.cs:122`, `:680-686`, `CoopHandler.cs:534-572`) foram lidas diretamente nesta sessão, com trechos de código citados literalmente em §1. |
| 10 | Skill EFT usada como lever confirmada não-inerte (`SkillsSettings` ≠ `[]`) — AP-10 | N/A | Não usa skill do EFT como mecanismo. |
| 11 | Pacote FIKA próprio: envelope + `TryGet*` + `Valid` + campos resetados + envio só main thread + registro por instância + zero `UnregisterPacket` + airbag com throttle — AP-11 | N/A | Não introduz nem modifica pacote de rede nenhum — a correção é inteiramente local (host), sem serialização envolvida. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-24 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-24 | Revisão `/review-technical-spec` 01 (PA-01-01, 🟡) — teto da nova espera trocado de contagem de frames pra segundos reais (`Time.time`), já que um host headless não tem framerate previsível. §5 e §8 atualizados |
