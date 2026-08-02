# 001 — Morte desligada com timer · Spec Técnica

**Mod:** TRL-PvpMode
**Spec funcional:** [001-morte-desligada-timer-01-spec.md](001-morte-desligada-timer-01-spec.md)
**Review:** [001-morte-desligada-timer-03-spec-tech-review-01.md](001-morte-desligada-timer-03-spec-tech-review-01.md) (rodada 01 aplicada)
**Criado:** 2026-08-01

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.
>
> **Este item também depende do código-fonte do Fika** (`references/fika-plugin/Fika.Core/`), que não é ofuscado — as referências a ele citam `arquivo.cs:linha` do fonte vendorizado.

## 1. Estratégia

O estado de caído **não é reimplementado**: é o do Fika, acionado pelo caminho nativo dele. A estratégia é
**destravar e ajustar** esse caminho em vez de construir um paralelo.

Fluxo nativo (confirmado no fonte):

```
ActiveHealthController.Kill(EDamageType)                    ← EFT/EFT.HealthSystem/ActiveHealthController.cs:3923
   └─ ClientHealthController_Kill_Patch.Prefix              ← Fika, Patches/Revival/ClientHealthController_Kill_Patch.cs:17
        se IsYourPlayer && ReviveEnabled && CanBeDowned && !CheckIfDamageShouldInstantKill():
            IsAlive = false; method_35(damageType); return false   ← bloqueia a morte
   └─ method_35 → ClientHealthController.SendNetworkSyncPacket(IsAlive=false)  ← Fika, ClientClasses/ClientHealthController.cs:83
        └─ TryProcessDownedState()                          ← :132
             └─ FikaPlayer.ToggleDowned(true)               ← Fika, Players/FikaPlayer.cs:595
                  trava movimento/eixos, guarda arma, DeathFade + FastBlur,
                  SetDamageCoeff(0), cria Bleedout
                  └─ envia DownedSyncPacket                 ← :680-682
                        ⇢ (pela REDE, no cliente de cada observador)
                          ObservedPlayer.ToggleDowned(true) ← Fika, Players/ObservedPlayer.cs:1283
                             └─ ReviveInteractable.Create() ← desliga o boneco + ragdoll
```

**Por que este caminho e não um próprio:** `ToggleDowned` já resolve trava sem prazo, visual, som, guarda de
arma e — o mais caro de replicar — **a sincronização** (`DownedSyncPacket`) e o desligamento do boneco nos
outros clientes. Reimplementar significaria criar pacote próprio (AP-11) e replicar `ReviveInteractable`.

**Alternativa descartada:** bloquear `Kill` por conta própria (o que o mod de referência faz). Bloqueia a
morte **antes** de qualquer aviso de rede sair, então nada é sincronizado — o defeito que originou este
trabalho.

**Ponto de atenção (AP-03):** `Kill` é `public void Kill(EDamageType)` declarado em `ActiveHealthController`
([:3923](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L3923)),
**não** virtual e **não** redeclarado em `ClientHealthController` (`GClass3010.cs:9` também não a
redeclara). Não há override a auditar — mas patchá-lo atinge todo mundo, humanos e bots, então todo
prefixo nosso filtra `IsYourPlayer` na primeira linha (AP-02).

**Pré-condição de ativação:** os patches de resgate do Fika só existem se o servidor mandar
`reviveConfig.enabled: true` ([FikaConfig.cs:908-913](../../../../references/fika-plugin/Fika.Core/FikaConfig.cs#L908-L913)).
Com a chave desligada nem `ClientHealthController_Kill_Patch` nem
`GetActionsClass_GetAvailableActions_Patch` são aplicados e o mod não tem em que se apoiar. O plugin
detecta isso no início da raid e avisa em vez de falhar em silêncio.

## 2. Pontos de patch

| Alvo | Tipo | Motivo |
|---|---|---|
| [`Fika ClientHealthController.CanBeRevivedByOtherPlayer()`](../../../../references/fika-plugin/Fika.Core/Main/ClientClasses/ClientHealthController.cs#L54) (private) | Postfix → `true` **ou** `false` | **O destravamento central e o portão de vidas, no mesmo ponto.** `CanBeDowned` (declarado em :18, condição em **:22**) exige `!_bledOut && (max==0 \|\| revives<max) && CanBeRevivedByOtherPlayer()`. Com vida disponível força `true` — só a última condição atrapalhava, e sem resgate por aliado ela é sem sentido, mas bloqueia jogar sozinho ou ser o último vivo. **Sem vida disponível força `false`**, e aí `CanBeDowned` é falso, o prefixo do Fika deixa o `Kill` original executar e a morte é normal. Patchar **este** método em vez de `CanBeDowned` preserva `_bledOut` — essencial contra laço infinito, porque `BleedOut()` (:125) faz `_bledOut = true; IsAlive = true; Kill()` e conta com `CanBeDowned == false` para a morte passar (AP-07) |
| [`Fika ClientHealthController.CheckIfDamageShouldInstantKill()`](../../../../references/fika-plugin/Fika.Core/Main/ClientClasses/ClientHealthController.cs#L68) (public) | Postfix | Duas funções: (a) move a opção de cabeça para o nosso F12; (b) **corrige o buraco do desgaste (R-02)** — retorna `true` para `Exhaustion \| Dehydration \| Stimulator`. Sem isso, o destravamento acima faz a morte por fome/sede/estimulante ser bloqueada pelo prefixo do Fika **e** recusada por `TryProcessDownedState` (:159-162), deixando o jogador `IsAlive=false`, sem estado de caído, sem evento de morte e sem fim de raid. Consultado nos **dois** pontos de decisão, então um postfix cobre ambos |
| [`Fika Bleedout.OnPlayerDeath(FikaPlayer)`](../../../../references/fika-plugin/Fika.Core/Main/Components/Bleedout.cs#L49) (private) | Prefix → `false` | Sem ele, a morte de **todos os companheiros** força `BleedOut()` no caído (:56-61). Com vidas próprias isso é errado: quem tem vida ainda decide sozinho |
| [`Fika ReviveInteractable.GetActions(GamePlayerOwner)`](../../../../references/fika-plugin/Fika.Core/Main/Components/ReviveInteractable.cs#L155) (método `public`, em tipo `internal sealed`) | Prefix → `__result = null` | Remove "levantar companheiro" do menu de interação. `null` já é retorno previsto pelo próprio método (:157-160) e o consumidor trata (`GamePlayerOwner.cs:883-889`, `?.InitSelected()`). **Mantém o ragdoll** — o corpo continua desligado e o fix de colisão do TRL-Fixes continua disparando em `RemoveRagdoll` |
| ~~`ActiveHealthController.Kill`~~ | — | **Não patchado.** A primeira versão previa um prefixo com `Priority.First` para o portão de vidas e para capturar o tipo de dano. Ambos foram resolvidos sem ele: o portão vira `__result = false` no postfix acima, e o tipo de dano é lido de `Player.LastDamageInfo` ([EFT/Player.cs:24360](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L24360), `protected` — alcançável por reflexão). Elimina a dependência de ordem entre patches, que era o ponto mais frágil do desenho |
| [`GameWorld.OnGameStarted()`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L2584) | Postfix | Início de raid: zera vidas e estado, valida a pré-condição do servidor, avisa se estiver desligada. **Guarda de contexto:** aborta se `MainPlayer` for nulo ou for `HideoutPlayer` |
| [`GameWorld.OnDestroy()`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L2111) | Postfix | Fim de raid — limpeza idempotente |

**Campo escrito diretamente (não patchado):** `ClientHealthController.<BleedoutTime>k__BackingField`, no
início de cada raid. A propriedade é auto-property com inicializador
([:26](../../../../references/fika-plugin/Fika.Core/Main/ClientClasses/ClientHealthController.cs#L26)); um
postfix no getter é candidato a ficar **inerte por inlining** do compilador (R-04). Escrever o campo de
apoio é imune a isso e alcança de uma vez `ShouldBleedOut` (:27), o prazo lido em `Bleedout.Init` (:30) e o
número exibido em `ShowUI` (:165).

**`BaseLocalGame.Stop` NÃO é patchado (R-03).** O tipo é genérico aberto
(`BaseLocalGame<TPlayerOwner>`, `EFT/BaseLocalGame-1.cs:31`), que o Harmony recusa; e `CoopGame` sobrescreve
`Stop` **sem chamar a base** ([CoopGame.cs:718](../../../../references/fika-plugin/Fika.Core/Main/GameMode/CoopGame.cs#L718)),
então um postfix na base nunca rodaria (AP-03). O repo já concluiu o mesmo no mod de stances
([RaidLifecyclePatches.cs:60-62](../../../stancesAndCameraPositionSPT4.0.11/modded/Patches/RaidLifecyclePatches.cs#L60-L62)):
`GameWorld.OnDestroy` cobre todos os caminhos de saída na prática, e `End()` é idempotente.

**Não patchados de propósito:** `CanBeDowned`, `TryProcessDownedState`, `FikaPlayer.ToggleDowned`,
`SendNetworkSyncPacket`. Todos são alcançados pelos pontos acima; mexer neles duplicaria decisão e criaria
divergência quando o Fika atualizar.

## 3. Novas propriedades F12 (BepInEx)

Seção única `Lives`, arquivo `com.trl.pvpmode.cfg`.

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `Lives` | `Enable Lives Mode` | bool | `true` | — | — | Liga o modo de vidas por raid. Desligado, volta a valer o resgate padrão do Fika (companheiro pode te levantar e o tempo vem do servidor). |
| `Lives` | `Lives Per Raid` | int | `1` | -1 a 10 | — | Quantas vezes você pode renascer por partida. `-1` = ilimitado. `0` = nenhuma (morre de primeira). |
| `Lives` | `Downed Timeout (s)` | float | `60` | 0 a 600 | — | Tempo para decidir renascer, em segundos. Ao zerar, a morte é definitiva. `0` = sem limite: você fica caído até decidir. O valor é lido no instante da queda — mudar durante a partida só vale na próxima. |
| `Lives` | `Headshot Kills Instantly` | bool | `false` | — | — | Tiro na cabeça encerra a partida na hora, ignorando as vidas restantes. |

> **Removido nesta rodada:** `Allow Finishing Downed` e `Grenade Kills Instantly` saíram do 001. A
> finalização do caído virou o item **005** (R-01). A opção de granada dependia da mesma fonte de dado
> indisponível (R-07) e acompanha o 005.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Plugin.cs` | MODIFICAR | `Settings.Init(Config)` e `new XPatch().Enable()` de cada patch no `Awake` |
| `modded/Settings.cs` | CRIAR | As quatro `ConfigEntry` da seção `Lives` |
| `modded/FikaBridge.cs` | CRIAR | Resolve por reflexão o tipo **internal** `Bleedout`/`ReviveInteractable` e os membros privados (`CanBeRevivedByOtherPlayer`, `OnPlayerDeath`, `_bledOut`, `<BleedoutTime>k__BackingField`). Resolvido uma vez em estático (SPT §3), com degradação graciosa e log por membro ausente |
| `modded/RaidState.cs` | CRIAR | Vidas restantes, pré-condição do servidor, último `EDamageType`, e `Begin()`/`End()` idempotentes |
| `modded/Patches/DownedGatePatches.cs` | CRIAR | `CanBeRevivedByOtherPlayer` + `CheckIfDamageShouldInstantKill` |
| `modded/Patches/KillGatePatch.cs` | CRIAR | Prefixo de `ActiveHealthController.Kill` (portão de vidas + captura do tipo de dano) |
| `modded/Patches/NoAllyRevivePatches.cs` | CRIAR | `ReviveInteractable.GetActions` + `Bleedout.OnPlayerDeath` |
| `modded/Patches/RaidLifecyclePatches.cs` | CRIAR | `GameWorld.OnGameStarted` + `GameWorld.OnDestroy` |
| `PROPRIEDADES.md` | CRIAR | Documentação das opções F12 |

**Infraestrutura de patch (R-14):** tudo é `SPT.Reflection.Patching.ModulePatch`, habilitado com
`new X().Enable()` no `Awake`. Não há instância Harmony própria nem `PatchAll`. A prioridade do prefixo de
`Kill` vem de `[HarmonyPriority(Priority.First)]` no próprio método do patch.

## 5. Stubs de código

```csharp
// modded/Patches/DownedGatePatches.cs
// O destravamento central: remove a exigência de "alguém vivo para me resgatar".
public class CanBeRevivedByOtherPlayerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // ref: fika-plugin/Fika.Core/Main/ClientClasses/ClientHealthController.cs:54
        return AccessTools.Method(typeof(ClientHealthController), "CanBeRevivedByOtherPlayer");
    }

    [PatchPostfix]
    private static void Postfix(ClientHealthController __instance, ref bool __result)
    {
        try
        {
            // Só o jogador local; o resto segue a regra do Fika (AP-02).
            if (__instance?.Player == null || !__instance.Player.IsYourPlayer) return;
            if (!Settings.ENABLED.Value) return;
            if (!RaidState.HasLifeAvailable) return;

            // _bledOut e a contagem de revives continuam sendo avaliados por
            // CanBeDowned (:22) — não os tocamos, senão BleedOut() vira laço (AP-07).
            __result = true;
        }
        catch (Exception ex) { Plugin.Log.LogError($"CanBeRevivedByOtherPlayer: {ex}"); }
    }
}

// Cabeça mata direto (config) + desgaste sempre mata direto (R-02).
public class InstantKillPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // ref: fika-plugin/Fika.Core/Main/ClientClasses/ClientHealthController.cs:68
        return AccessTools.Method(typeof(ClientHealthController), nameof(ClientHealthController.CheckIfDamageShouldInstantKill));
    }

    [PatchPostfix]
    private static void Postfix(ClientHealthController __instance, ref bool __result)
    {
        try
        {
            if (__instance?.Player == null || !__instance.Player.IsYourPlayer) return;
            if (!Settings.ENABLED.Value) return;
            if (__result) return; // o Fika já decidiu matar; não desfazemos

            // Fome / desidratação / estimulante são desgaste, não combate: matam direto.
            // Sem isto, o prefixo do Fika bloqueia a morte e TryProcessDownedState (:159)
            // recusa o caído — jogador fica IsAlive=false sem morrer e sem fim de raid.
            var dmg = RaidState.LastKillDamageType;
            if ((dmg & (EDamageType.Exhaustion | EDamageType.Dehydration | EDamageType.Stimulator)) != 0)
            {
                __result = true;
                return;
            }

            // LastDamagedBodyPart é campo público do Player (ref: EFT/Player.cs:24329).
            if (Settings.HEADSHOT_KILLS.Value && __instance.Player.LastDamagedBodyPart == EBodyPart.Head)
                __result = true;
        }
        catch (Exception ex) { Plugin.Log.LogError($"CheckIfDamageShouldInstantKill: {ex}"); }
    }
}
```

```csharp
// modded/Patches/KillGatePatch.cs
public class KillGatePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
        // ref: Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs:3923
        // Kill não é virtual e não é redeclarado em ClientHealthController: o patch
        // atinge humanos e bots, por isso o filtro IsYourPlayer é a primeira linha (AP-02/AP-03).
        => AccessTools.Method(typeof(ActiveHealthController), nameof(ActiveHealthController.Kill));

    [PatchPrefix, HarmonyPriority(Priority.First)]  // antes do prefixo do Fika
    private static void Prefix(ActiveHealthController __instance, EDamageType damageType)
    {
        try
        {
            if (__instance is not ClientHealthController chc) return;
            if (chc.Player == null || !chc.Player.IsYourPlayer) return;
            if (!Settings.ENABLED.Value) return;

            // Única fonte acessível do tipo de dano neste quadro (R-07).
            RaidState.LastKillDamageType = damageType;

            if (!RaidState.HasLifeAvailable)
            {
                // Marcar _bledOut faz CanBeDowned (:22) virar false, então o prefixo
                // do Fika (:20) deixa o Kill original executar = morte real. Mesmo
                // mecanismo do tempo esgotado — sem reentrância (AP-07).
                FikaBridge.SetBledOut(chc, true);
            }
        }
        catch (Exception ex) { Plugin.Log.LogError($"KillGate: {ex}"); }
    }
}
```

```csharp
// modded/Patches/NoAllyRevivePatches.cs
// Tira "levantar companheiro" do menu, sem tirar o ragdoll.
public class NoAllyRevivePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
        // ref: fika-plugin/Fika.Core/Main/Components/ReviveInteractable.cs:155
        // Tipo internal sealed => resolvido por nome (degrada se o Fika renomear).
        => AccessTools.Method(FikaBridge.ReviveInteractableType, "GetActions");

    // __result precisa do tipo REAL de retorno; `object` gera IL não verificável (R-06).
    [PatchPrefix]
    private static bool Prefix(ref ActionsReturnClass __result)
    {
        if (!Settings.ENABLED.Value) return true;
        __result = null;   // null já é retorno previsto do próprio método (:157)
        return false;
    }
}
```

## 6. Fluxo de dados

```
[1] dano fatal no jogador local
      ↓
[2] ActiveHealthController.Kill  ← nosso prefixo (Priority.First)
      ├─ guarda o EDamageType do quadro
      └─ sem vidas → marca _bledOut → segue e morre de verdade
      ↓
[3] prefixo do Fika: IsYourPlayer && ReviveEnabled && CanBeDowned && !CheckIfDamageShouldInstantKill()
      ├─ CanBeDowned → CanBeRevivedByOtherPlayer ← nosso postfixo (true quando há vida)
      ├─ CheckIfDamageShouldInstantKill ← nosso postfixo (desgaste sempre; cabeça se configurado)
      └─ passou → bloqueia a morte e sinaliza IsAlive=false
      ↓
[4] method_35 → SendNetworkSyncPacket → TryProcessDownedState → ToggleDowned(true)
      ├─ local: trava movimento e eixos, guarda arma, escurece a tela, dano zerado
      ├─ Bleedout: contagem = nosso BleedoutTime (0 = sem contagem)
      │     └─ OnPlayerDeath ← nosso prefixo bloqueia (companheiros mortos não me matam)
      └─ rede: DownedSyncPacket ⇢ ObservedPlayer.ToggleDowned ⇢ corpo desligado + ragdoll
      ↓
[5a] contagem zera → Bleedout.BleedOut() → _bledOut = true → Kill() → morte real → fim de raid normal
[5b] jogador escolhe renascer → item 002
```

## 7. Riscos e dependências

- **Dependência dura do Fika e da chave do servidor.** Sem `reviveConfig.enabled: true` os patches-base do
  Fika não existem. Mitigação: checagem no início da raid + aviso na tela; o mod não tenta suprir sozinho.
- **O mod precisa estar em TODOS os clientes (R-10).** O bloqueio de resgate roda no cliente de **quem
  olha**: um par sem o mod continua vendo "levantar companheiro" e consegue executar o resgate, devolvendo
  o jogador em pé sem debitar vida nenhuma. Mitigação neste item: documentar como pré-requisito. Blindagem
  do lado da vítima (recusar os pacotes de resgate) fica para o item 003, junto com o resto da rede.
- **Conflito com o PlayerLives.** Ele prefixa `ActiveHealthController.Kill` e retorna `false`, impedindo
  todo o resto de rodar. Os dois **não podem coexistir**. Mitigação: detectar o GUID
  `com.somtam.playerLives` carregado e avisar em alto e bom som.
- **Acoplamento a membros privados/internos do Fika.** Uma atualização pode renomeá-los. Mitigação: tudo
  em `FikaBridge` com log por membro ausente; o modo se autodesativa em vez de quebrar a raid.
- **Com tempo 0, o `Bleedout` fica curto-circuitado (R-05).** O `return` antecipado de `Update` acontece
  **antes** de `CheckForKeys()` (:77-86), então a tecla nativa de desistir para de funcionar, e o
  componente segue vivo com um gemido a cada 10s. Consequência para o **item 002**: a tecla de renascer
  **não pode** depender de `Bleedout.CheckForKeys` — precisa de leitura de input própria.
- **A plaquinha de vida remota mostra o tempo do servidor (R-04).** `FikaHealthBar` lê
  `ReviveConfig.BleedoutTime` cru (:619-624), fora do nosso alcance. Com tempo 0 os companheiros ainda
  veem uma contagem correndo. Aceito neste item: é informação cosmética na plaquinha de terceiro, e o
  desfecho real continua governado pelo nosso valor.
- **`Priority.First` no prefixo de `Kill`.** Se outro mod usar a mesma prioridade, a ordem é indefinida. O
  nosso prefixo não retorna `false` — só marca estado — então a pior consequência é a decisão de vidas ser
  avaliada depois, não uma raid travada.

### Corner cases do 01-spec — endereçamento (R-11)

| Corner case | Tratamento |
|---|---|
| Único humano / último vivo | **Resolvido** — é o objetivo do postfix de `CanBeRevivedByOtherPlayer` |
| Companheiros todos mortos | **Resolvido** — prefixo em `Bleedout.OnPlayerDeath` |
| Dois danos fatais no mesmo quadro | **Coberto por terceiro** — `TryProcessDownedState` sai cedo se já `Downed` (:144) e `ToggleDowned` recusa estado repetido (`FikaPlayer.cs:603-607`). Não é mérito desta spec; declarado para não parecer resolvido por desenho nosso |
| Fome / desidratação / estimulante | **Resolvido** — postfix de `CheckIfDamageShouldInstantKill` (R-02) |
| Esconderijo / menu | **Resolvido** — guarda `MainPlayer is not HideoutPlayer` no início de raid |
| Contagem zero | **Resolvido** — `ShouldBleedOut == false`, com a ressalva do R-05 |
| Anfitrião caído / anfitrião encerra a partida | **Aceito sem código** — o encerramento pelo anfitrião passa por `CoopGame.Stop`, que não interceptamos; o caído não segura nada. Validar in-game |
| Reconexão estando caído | **Delegado ao item 003** — restaurar `Downed` no rejoin é assunto de rede. Enquanto isso, comportamento indefinido: registrar no `PROPRIEDADES.md` como limitação conhecida |
| Alt-F4 estando caído | **Aceito com justificativa** — o perfil fica como em qualquer desconexão em partida; o servidor resolve pelo caminho normal. Nada nosso persiste (o estado é todo em memória, zerado no `Begin()`) |

## 8. Checklist de implementação

- [ ] `Settings.cs` com as quatro entradas e tooltips da §3
- [ ] `FikaBridge.cs` resolvendo tipos/membros internos com cache estático e degradação graciosa
- [ ] `RaidState.cs` com vidas, pré-condição, `LastKillDamageType` e `Begin`/`End` idempotentes
- [ ] `DownedGatePatches.cs` (2 patches)
- [ ] `KillGatePatch.cs` (portão de vidas + captura do tipo de dano)
- [ ] `NoAllyRevivePatches.cs` (2 patches)
- [ ] `RaidLifecyclePatches.cs` (2 patches, com guarda de esconderijo)
- [ ] Escrita do campo de apoio de `BleedoutTime` no início da raid
- [ ] Detecção do PlayerLives carregado + aviso
- [ ] Detecção de `reviveConfig.enabled == false` + aviso
- [ ] `PROPRIEDADES.md` do mod (incluindo as limitações R-04, R-05, R-15 e reconexão)
- [ ] Versão para `0.2.0` nos três lugares

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | ✅ | §2: `GameWorld.OnGameStarted` (start, com guarda de esconderijo) e `GameWorld.OnDestroy` (stop). `BaseLocalGame.Stop` **deliberadamente não** patchado — genérico aberto + override do Fika sem `base.Stop` (R-03), seguindo o precedente do mod de stances. `RaidState.End()` idempotente por flag |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | Todos os prefixos/postfixos de saúde filtram `IsYourPlayer` na primeira linha (§5). Crítico porque `Kill` é herdado e roda para todo bot |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | ✅ | `Kill` não é virtual e tem declaração única em `ActiveHealthController.cs:3923` (`GClass3010.cs:9` não redeclara) — nada a auditar. O caso real de override não-chamando-base (`CoopGame.Stop`) foi identificado e o patch correspondente **removido** (R-03). Tipos internos do Fika resolvidos por nome via `FikaBridge`, nunca por `GClassNNNN` |
| 4 | Mudança de estado via API canônica; side-effects mapeados — AP-04 | ✅ | O estado de caído é acionado pelo caminho nativo do Fika, não por escrita direta. Duas escritas diretas, ambas deliberadas e documentadas: `_bledOut` (é exatamente o que `BleedOut()` :125 faz) e o campo de apoio de `BleedoutTime` (alternativa a um getter que o compilador pode embutir — R-04) |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA | ✅ | `RaidState.Begin()` zera tudo no `OnGameStarted`; `End()` no `OnDestroy`. Nenhum estado vive em `static` fora do `RaidState`. Alt-F4 tratado no §7 |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | ✅ | §3 define faixa e **valor neutro explícito** dos casos ambíguos: `Lives = -1` (ilimitado), `Timeout = 0` (sem limite). Tooltip de `Enable Lives Mode` corrigido para descrever o que realmente acontece quando desligado (R-09) |
| 7 | Re-invocação de método patcheado tem reentry-guard — AP-07 | ✅ | O risco real é `BleedOut()` → `Kill()` (segunda entrada no método que prefixamos). Coberto por não tocar `_bledOut` no postfixo de destravamento e por **marcar** `_bledOut` antes de deixar a morte passar |
| 8 | Flags/caches validados contra o contexto atual após troca — AP-08 | ✅ | Nada cacheado por raid além de `RaidState`, resetado no start. `LastKillDamageType` é escrito e lido no mesmo quadro. `FikaBridge` cacheia só `MethodInfo`/`Type`, imutáveis no processo |
| 9 | Patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | `ActiveHealthController.Kill` em `:3923`; `ApplyDamage` guard em `:3723`; `SetDamageCoeff` em `:3673`; `GClass3010 : ActiveHealthController` em `GClass3010.cs:9`; `Player.LastDamagedBodyPart` em `EFT/Player.cs:24329`. **Corrigido nesta rodada:** a spec citava `EFT/BaseLocalGame.cs:1018`, arquivo que **não existe** (é `BaseLocalGame-1.cs`) — era AP-09 cometido, e o patch foi removido |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | O item não usa skill do EFT |
| 11 | Pacote FIKA próprio — AP-11 | N/A | Este item **não declara pacote**: a sincronização usa o `DownedSyncPacket` nativo do Fika. Pacote próprio só no item 003 |

## Histórico

| Data | Evento |
|---|---|
| 2026-08-01 | Spec técnica criada via `/create-technical-spec` |
| 2026-08-01 | Review adversarial rodada 01 aplicado — 3 🔴 + 8 🟡 + 4 🔵. Removidos o patch de `BaseLocalGame.Stop` e a opção de finalizar o caído (item 005); corrigido o buraco da morte por desgaste; `BleedoutTime` migrado de patch de getter para escrita do campo de apoio |
