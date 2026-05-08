# 001 — Stamina e Velocidade por Postura · Review Técnica 01

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec técnica revisada:** [001-stamina-e-velocidade-technical-spec.md](001-stamina-e-velocidade-technical-spec.md)
**Spec funcional referência:** [001-stamina-e-velocidade-spec.md](001-stamina-e-velocidade-spec.md)
**Data:** 2026-05-07

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/build-item`.
>
> Skills aplicadas: `spt-mod-best-practices` + `csharp-mod-best-practices`. Checklists ao fim de cada skill foram a base mínima da revisão.

## Resumo

> 🔴 Bloqueadores: **5** · 🟡 Importantes: **7** · 🟢 Menores: **4** · ✅ Resolvidos: 0 · Total: **16**
>
> ⛔ **Status:** NÃO está pronto para `/build-item` — resolver os 5 bloqueadores na spec técnica antes de prosseguir. Após zerar 🔴, rodar `/review-technical-spec` novamente para validar e gerar `technical-review-02.md`.

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| [PA-01-01](#pa-01-01) | C — Lógica | 🔴 | Estratégia de Drain obsoleta vs. spec funcional (Opção B já decidida) | `[ ]` Pendente |
| [PA-01-02](#pa-01-02) | A — Gap | 🔴 | `Player.ESpeedLimit` cause não definida — código não compila | `[ ]` Pendente |
| [PA-01-03](#pa-01-03) | A — Gap | 🔴 | Sem hooks de raid lifecycle (`GameWorld.OnGameStarted`/`OnDestroy` e `BaseLocalGame.Stop`) | `[ ]` Pendente |
| [PA-01-04](#pa-01-04) | A — Gap | 🔴 | Postfixes não filtram por `MainPlayer` — afetam todos os `Player` no `gameWorld` | `[ ]` Pendente |
| [PA-01-05](#pa-01-05) | A — Gap | 🔴 | Hideout/menu guards faltando — feature roda fora de raid | `[ ]` Pendente |
| [PA-01-06](#pa-01-06) | A — Gap | 🟡 | Métodos auxiliares de `Plugin` referenciados nos stubs mas não definidos | `[ ]` Pendente |
| [PA-01-07](#pa-01-07) | A — Gap | 🟡 | `StanceStaminaState` sem método de reset — vaza estado entre raids | `[ ]` Pendente |
| [PA-01-08](#pa-01-08) | A — Gap | 🟡 | Postfixes sem `try/catch` + `ManualLogSource` — exception derruba o método patcheado | `[ ]` Pendente |
| [PA-01-09](#pa-01-09) | C — Lógica | 🟡 | Detecção de prone via `PoseLevel < 0.05f` é heurística mágica | `[ ]` Pendente |
| [PA-01-10](#pa-01-10) | C — Lógica | 🟡 | Default no stub do `Plugin.cs` (`None`) contradiz tabela de defaults | `[ ]` Pendente |
| [PA-01-11](#pa-01-11) | A — Gap | 🟡 | `SettingChanged` mencionado mas sem stub mostrando invalidação de cache | `[ ]` Pendente |
| [PA-01-12](#pa-01-12) | B — Edge | 🟡 | Recovery postfix usa `gameWorld.MainPlayer` cache em vez de comparar `__instance` direto | `[ ]` Pendente |
| [PA-01-13](#pa-01-13) | B — Edge | 🟢 | `AccessTools.Method(typeof(...), "method_10")` por nome literal é frágil | `[ ]` Pendente |
| [PA-01-14](#pa-01-14) | A — Gap | 🟢 | Itens "a confirmar" (`Float_5`, `Single_0`, nome ofuscado) deveriam ser resolvidos antes do build | `[ ]` Pendente |
| [PA-01-15](#pa-01-15) | B — Edge | 🟢 | Sem orientação para `HarmonyPriority` em conflito com mods de stamina | `[ ]` Pendente |
| [PA-01-16](#pa-01-16) | A — Gap | 🟢 | Logging discipline ausente (sem `ManualLogSource` por plugin) | `[ ]` Pendente |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-01-01 · C — Lógica · 🔴 Bloqueador {#pa-01-01}

**Estratégia de Drain está obsoleta — não bate com a Opção B já decidida na spec funcional**

**Problema:** A seção [§1 Estratégia](001-stamina-e-velocidade-technical-spec.md#1-estratégia) e o stub de [`StanceStaminaDrainPatch.cs`](001-stamina-e-velocidade-technical-spec.md#5-stubs-de-código) ainda descrevem **Harmony postfix em `PlayerPhysicalClass.method_10`** para o modo Drain. Mas a spec funcional foi explicitamente alterada para Opção B (tick manual em hipfire) — ver [001-stamina-e-velocidade-spec.md §"Stamina"](001-stamina-e-velocidade-spec.md):

> **Drain:** em **hipfire**, a stance ativa drena `HandsStamina` a uma taxa proporcional a `Intensity` (...). Implementado via tick manual no `StanceManager` chamando `HandsStamina.Consume(...)` enquanto a stance está ativa e o jogador não está em ADS. **Não aplica durante ADS** — o drain vanilla do EFT toma conta nesse estado.

Além disso, o histórico da própria spec técnica reconhece a pendência:

> **Pendente:** próximo `/review-technical-spec` deve validar esta versão considerando as mudanças (Drain agora é tick manual em hipfire, não postfix em `method_10`...)

**Por que importa:** se o build seguir o stub atual, vai (a) implementar uma estratégia rejeitada, (b) drenar **somente em ADS** (multiplicando o drain vanilla) — exatamente o oposto do que a spec funcional pede, e (c) não drenar nada em hipfire — falha total na entrega da feature de Stance 0.

**Decisão:** `[ ]` Pendente
<!-- A resolução exige reescrever §1 (estratégia), substituir o stub StanceStaminaDrainPatch.cs por um método Tick() em StanceManager que invoque HandsStamina.Consume(...), atualizar §6 fluxo de dados (Drain), e atualizar §3 tabela de pontos de patch removendo a linha de method_10 ou marcando-a "não usado nesta feature". -->

---

### PA-01-02 · A — Gap · 🔴 Bloqueador {#pa-01-02}

**`Player.ESpeedLimit` cause não foi definida — `AddStateSpeedLimit` exige um valor concreto**

**Problema:** §1 ("Redutor de velocidade"), §3 (Pontos de patch) e §7 (Riscos) listam `MovementContext.AddStateSpeedLimit(value, cause)` como API a usar, mas adiam a definição de `cause`:

> **`Player.ESpeedLimit` é fechada:** todas as causes existentes têm semântica do EFT. Reusar uma cause "neutra" (ex.: investigar `SurfaceNormal`) ou injetar nova entrada via reflection. **Decisão na implementação.**

A enum real está em [Player.cs:1584-1595](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L1584-L1595):

```csharp
public enum ESpeedLimit
{
    BarbedWire, HealthCondition, Aiming, Weight,
    SurfaceNormal, Swamp, Shot, Armor, Fall
}
```

Nenhum valor é semanticamente "stance". Reusar qualquer um colide com a semântica real do EFT (ex.: `SurfaceNormal` é redefinido pelo motor com base no terreno; nosso valor seria sobrescrito).

**Por que importa:** "decisão na implementação" não é aceitável numa spec técnica — sem `cause` definida, o stub não compila e `/build-item` precisaria adivinhar. Além disso, esta é uma decisão de **compatibilidade** (mods de stamina/velocidade dividem o mesmo enum) que merece estar na spec, não no commit.

**Caminhos viáveis a documentar:**
- (a) Reusar uma cause raramente ativa em raid (ex.: `BarbedWire`) com guard explícito de que stance e arame farpado são mutuamente exclusivos.
- (b) Cast de inteiro fora da enum (`(Player.ESpeedLimit)100`) — o `Dictionary<ESpeedLimit, float>` interno aceita qualquer int valido. Hacky mas isolado.
- (c) Reflection sobre o dicionário interno de `MovementContext` (`SpeedLimits`) para inserir/remover diretamente sem usar a enum.

**Decisão:** `[ ]` Pendente
<!-- Recomendação: opção (b) com constante única documentada — ex.: const int StanceSpeedLimitCause = 9001; (Player.ESpeedLimit)StanceSpeedLimitCause. Documentar no README como reservado pelo mod para evitar colisão com outros mods. -->

---

### PA-01-03 · A — Gap · 🔴 Bloqueador {#pa-01-03}

**Sem hooks de raid lifecycle — estado vaza entre raids**

**Problema:** A spec técnica não menciona patches em `GameWorld.OnGameStarted` ([GameWorld.cs:2584](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L2584)), `GameWorld.OnDestroy` ([GameWorld.cs:2111](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L2111)) ou `BaseLocalGame.Stop` ([BaseLocalGame.cs:1018](../../../../references/eft-decompiled/Assembly-CSharp/EFT/BaseLocalGame.cs#L1018)). Mas a spec funcional exige:

- AC: "Cleanup em todas as saídas de raid" — testar `Left` / `Killed` / `MissingInAction`.
- AC: "Ao terminar uma raid e iniciar outra, a velocidade base não fica reduzida sem stance ativa".
- Seção "Cleanup e ciclo de vida": "Início de raid: estado em cache zerado antes de aplicar novos valores".

Pelo skill `spt-mod-best-practices` §2: "Você **deve** hookar o raid-stop path. (...) Patch `GameWorld.OnDestroy` **e** `BaseLocalGame.Stop(...)`. Either may fire first depending on extract type. Make `RaidSession.End()` idempotent (...)".

**Por que importa:** sem esses hooks, (a) `StanceStaminaState` mantém estado da raid anterior na nova raid (estado estático persiste), (b) speed limits registrados em `MovementContext` da raid anterior não são limpos antes do próximo `MovementContext` ser criado — risco de leak de delegates/handlers se houver `+=` em qualquer ponto.

**Decisão:** `[ ]` Pendente
<!-- Adicionar §"Hooks de raid lifecycle" antes ou junto com §1 Estratégia, listando: (1) Postfix em GameWorld.OnGameStarted → StanceManager.OnRaidStart() — re-resolve cache da config, zera StanceStaminaState; (2) Postfix em GameWorld.OnDestroy E BaseLocalGame.Stop → StanceManager.OnRaidEnd() (idempotente, guard com bool _ended); (3) OnRaidEnd remove qualquer speed limit pendente, zera StanceStaminaState. Stubs concretos em §5. -->

---

### PA-01-04 · A — Gap · 🔴 Bloqueador {#pa-01-04}

**Postfixes não filtram por `MainPlayer` — alteram comportamento de bots e network players**

**Problema:** O stub de [`StanceStaminaRecoveryPatch.cs`](001-stamina-e-velocidade-technical-spec.md#5-stubs-de-código) faz:

```csharp
[PatchPostfix]
private static void Postfix(ref float __result)
{
    if (!StanceStaminaState.ShouldApplyStamina ||
        StanceStaminaState.Mode != EStanceStaminaMode.Recovery)
        return;
    var player = StanceManager.GetCachedGameWorld()?.MainPlayer;
    if (player?.ProceduralWeaponAnimation?.IsAiming == true)
        return;
    __result *= StanceStaminaState.Intensity;
}
```

Mas `PlayerPhysicalClass.GetHandsRestorationFunc` ([PlayerPhysicalClass.cs:1022](../../../../references/eft-decompiled/Assembly-CSharp/PlayerPhysicalClass.cs#L1022)) é `virtual` e roda **para cada `Player` no `gameWorld`** (incluindo bots — `BotOwner` chama esse método para sua própria stamina). O stub multiplica `__result` para qualquer instância chamadora, não só a do jogador local.

A spec funcional exige explicitamente:
- AC: "Apenas o `MainPlayer` afetado: num cenário com bots em volta, observação dos bots não mostra alteração de comportamento atribuível a este backlog".
- Corner case: "Postfix em `GetHandsRestorationFunc` rodando para múltiplos `Player` no `gameWorld` — precisa filtrar para aplicar Recovery somente quando `__instance` corresponde ao `MainPlayer`".
- Seção "Aplicação só ao jogador local".

**Por que importa:** sem filtro, a regen de mãos de **todos os bots PMC/Scav** é multiplicada pela `Intensity` do `MainPlayer`. Com `Intensity=2.0` (default Stance 1), bots se recuperam 2× mais rápido também — afeta balanceamento de combate de forma silenciosa. Mesmo problema vale para qualquer postfix futuro em `method_10`.

**Decisão:** `[ ]` Pendente
<!-- O postfix precisa receber `[HarmonyPostfix] private static void Postfix(PlayerPhysicalClass __instance, ref float __result)` e logo no início:
       var gw = Singleton<GameWorld>.Instance;
       if (gw == null || gw.MainPlayer == null || __instance.Player_0 != gw.MainPlayer) return;
     `__instance.Player_0` é o campo Player que o PlayerPhysicalClass referencia (ver PlayerPhysicalClass.cs:271 onde aparece como Player_0). Aplicar mesmo padrão se postfix em method_10 for retido para algum uso. -->

---

### PA-01-05 · A — Gap · 🔴 Bloqueador {#pa-01-05}

**Hideout/menu guards faltando — feature roda em qualquer contexto**

**Problema:** Nenhum dos stubs (`StanceStaminaDrainPatch`, `StanceStaminaRecoveryPatch`, `StanceManager.OnStanceChanged`) checa se há `gameWorld` ativo ou se o jogador está em hideout. Mas a spec funcional exige:

> **Hideout / menu:** feature **inerte** — sem drain, sem recovery, sem speed limit. Os offsets visuais das stances 1/2/3 (que já existem hoje no mod) continuam funcionando como antes.

> AC: "Hideout: entrar no hideout (com ou sem stance ativa setada antes) **não dispara drain, recovery ou speed limit**."

Pelo skill `spt-mod-best-practices` §2:

> **Hideout vs. raid vs. menu:** o mod pode estar ativo em três contextos. Sempre check qual antes de agir. Robust guard: `if (gameWorld.MainPlayer is HideoutPlayer) return;`.

**Por que importa:** `PlayerPhysicalClass.GetHandsRestorationFunc` continua sendo chamado no hideout (o personagem mantém stamina lá). Sem o guard, Recovery `Intensity=2.0` aceleraria regen no hideout — comportamento não pedido, e potencialmente confuso (stamina do jogador "voa" passivamente). Pior, drain via tick (após PA-01-01) consumiria stamina no hideout sem que o jogador esteja arriscando nada.

**Decisão:** `[ ]` Pendente
<!-- Em todos os patches e no tick do StanceManager, prefixar com:
       var gw = Singleton<GameWorld>.Instance;
       if (gw == null || gw.MainPlayer == null) return;          // sem raid
       if (gw.MainPlayer is HideoutPlayer) return;                // hideout — feature inerte
     Centralizar isso num helper `StanceStaminaState.IsActiveContext()` para evitar repetição e facilitar mudanças. -->

---

### PA-01-06 · A — Gap · 🟡 Importante {#pa-01-06}

**Métodos auxiliares de `Plugin` referenciados mas não definidos**

**Problema:** O stub de `StanceManager.OnStanceChanged` em §5 chama três métodos que não estão definidos em lugar algum da spec:

- `Plugin.GetSpeedLimitCauseFor(stance, out cause)`
- `Plugin.GetStaminaConfigFor(stance) → (Mode, Intensity)`
- `Plugin.GetApplyWhenProneFor(stance) → bool`

Esses são fundamentais para mapear "stance numerada" (0/1/2/3) → "valor da `ConfigEntry` correspondente". Sem definição, o build vai precisar inventá-los.

**Por que importa:** com 4 stances × 5 props = 20 `ConfigEntry`s, escrever um `switch (stance)` por chamada é ruidoso e propenso a erro de digitação. A spec deveria mostrar a estrutura recomendada (array indexado por stance? `Dictionary<int, StanceConfig>`?). Sem orientação, cada implementador resolve diferente.

**Decisão:** `[ ]` Pendente
<!-- Adicionar em §5 um stub do `Plugin.cs` com array/dicionário de `StanceConfig` indexável por número de stance. Ex.:
       private record StanceConfig(ConfigEntry<EStanceStaminaMode> Mode, ConfigEntry<float> Intensity, ConfigEntry<bool> ModifiesSpeed, ConfigEntry<int> SpeedMultiplier, ConfigEntry<bool> ApplyWhenProne);
       private static readonly Dictionary<int, StanceConfig> _stanceConfigs = new(); // populado em Awake
       public static StanceConfig GetStanceConfig(int stance) => _stanceConfigs[stance];
     Os getters `GetSpeedLimitCauseFor` etc. derivam disso. -->

---

### PA-01-07 · A — Gap · 🟡 Importante {#pa-01-07}

**`StanceStaminaState` sem método de reset — vaza estado entre raids**

**Problema:** A classe estática [`StanceStaminaState`](001-stamina-e-velocidade-technical-spec.md#5-stubs-de-código) tem 3 campos públicos mutáveis (`Mode`, `Intensity`, `IsSuspendedByProne`) mas nenhum `Reset()` ou `Initialize()`. Spec funcional exige:

> **Início de raid:** o estado das stances é re-resolvido a partir da config (...). Estado em cache (`StanceStaminaState`) é zerado antes de aplicar novos valores.

> Corner case: "Volta ao menu mid-raid (encerramento abrupto): se o jogo é encerrado abruptamente e o jogador volta ao menu sem passar pelos hooks de fim de raid, a próxima raid inicia limpa (estado estático é defensivamente zerado **no início** de cada raid, não só no fim)."

**Por que importa:** se o jogador morre com Stance 1 ativa (Recovery 2.0), a próxima raid começa com `StanceStaminaState.Mode = Recovery, Intensity = 2.0` sem que a stance esteja realmente ativa. Recovery seria aplicada incorretamente desde o spawn.

**Decisão:** `[ ]` Pendente
<!-- Adicionar à classe:
       public static void Reset()
       {
           Mode = EStanceStaminaMode.None;
           Intensity = 1f;
           IsSuspendedByProne = false;
       }
     Chamado tanto em OnRaidStart (defensivo) quanto em OnRaidEnd (limpeza). -->

---

### PA-01-08 · A — Gap · 🟡 Importante {#pa-01-08}

**Postfixes sem `try/catch` — exception derruba o método patcheado**

**Problema:** Os 2 stubs de patch não têm bloco `try/catch`. Pelo skill `csharp-mod-best-practices` §3:

> Wrap every Harmony patch body in `try/catch` and log via `BepInEx.Logging.ManualLogSource`. An unhandled exception inside a prefix can prevent the original method from running and brick the raid.

E spec funcional exige:

> AC: "Falha isolada: se um patch crashar (...), o mod continua carregado, outros patches seguem funcionando, e há entrada de `LogError` no console do BepInEx com stack trace."

**Por que importa:** se `StanceStaminaState.Intensity` for, por algum bug, NaN ou Infinity, o `__result *= Intensity` pode propagar para `HandsStamina.Consume` e quebrar a regen do EFT — para todos os Players. Postfixes que mexem em `__result` precisam ser defensivos.

**Decisão:** `[ ]` Pendente
<!-- Padrão para os 2 stubs (e qualquer outro):
       [PatchPostfix]
       private static void Postfix(...) {
           try {
               // lógica
           } catch (Exception ex) {
               Plugin.Logger.LogError($"[StanceStaminaXPatch] {ex}");
           }
       }
     Validar `Intensity` for finite (`float.IsFinite`) antes de aplicar. -->

---

### PA-01-09 · C — Lógica · 🟡 Importante {#pa-01-09}

**Detecção de prone via `PoseLevel < 0.05f` é heurística mágica e diverge do que a spec referencia**

**Problema:** §"Detecção de Prone" da spec técnica diz:

> `PlayerPhysicalClass.Epose_0` é o campo da `EPose` (...). Acesso via `Player.MovementContext.PoseLevel` (a confirmar — possivelmente `Player.IsInProne` mais direto).

E o stub de `StanceManager.IsPlayerProne`:

```csharp
return player.MovementContext.PoseLevel < 0.05f;
```

Dois problemas:
1. **`PoseLevel` é um `float` interpolado** ([MovementContext.cs:194](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L194) `PoseLevel_1 = 1f`) — usado para animação suave entre stand/crouch/prone. O `0.05f` é uma constante mágica sem justificativa documentada e pode falhar quando o EFT muda a curva de interpolação.
2. **`EPose` é discreto** — `enum { Prone, Sit, Stand }`. Se o sentido era "EPose == Prone", o teste correto é via `PlayerPhysicalClass.Epose_0` (campo) ou um getter público equivalente. A spec menciona `Player.IsInProne` "mais direto" mas não verifica se existe.

**Por que importa:** durante a animação de "deitar" (PoseLevel transitando de 1 → 0), há ~0.5s em que `PoseLevel < 0.05f` ainda é `false` mas o jogador está visivelmente deitando. O drain/recovery seguiria ativo durante essa transição, e os bordas do AC "ao entrar em prone com `Apply When Prone = false`, drain/recovery cessam" ficam ambíguas (cessam quando? início da animação ou fim?).

**Decisão:** `[ ]` Pendente
<!-- Confirmar via grep no Assembly se existe `Player.IsInProne` ou propriedade equivalente. Se não, padronizar para usar `Player.MovementContext.PoseLevel <= 0` (deitado) com comentário explicando que essa é a representação "pose-final-prone", aceitando que a transição de animação não conta como prone. Ou usar reflection sobre Epose_0 == EPose.Prone para precisão. Documentar a escolha na spec. -->

---

### PA-01-10 · C — Lógica · 🟡 Importante {#pa-01-10}

**Default no stub do `Plugin.cs` (`None`) contradiz a tabela de defaults (Drain/Recovery)**

**Problema:** O stub em §5 mostra:

```csharp
_Stance0StaminaMode = Config.Bind(
    Stance0Section,
    "Stance 0 Stamina Mode",
    EStanceStaminaMode.None,   // ← default = None
    new ConfigDescription(...)
);
```

Mas a §"Defaults por stance (instalação limpa)" da mesma spec técnica e a §"Defaults recomendados" da spec funcional dizem:

| Stance 0 | `Drain` | `0.50` | `true` | `90` | `false` |

E o AC: "Numa instalação limpa do mod (sem `BepInEx.cfg` prévio), os defaults persistidos batem **exatamente** com a tabela".

**Por que importa:** o build seguindo o stub literal vai persistir todos os modos como `None` na primeira execução, quebrando o AC. Como `BepInEx.Config.Bind` persiste o default na primeira leitura, depois disso o jogador precisaria editar manualmente — defaults da feature não viriam pré-configurados.

**Decisão:** `[ ]` Pendente
<!-- Atualizar o stub para refletir a tabela:
       _Stance0StaminaMode = Config.Bind(..., EStanceStaminaMode.Drain, ...);
       _Stance0StaminaIntensity = Config.Bind(..., 0.50f, ...);
       _Stance0ModifiesMovementSpeed = Config.Bind(..., true, ...);
       _Stance0MovementSpeedMultiplier = Config.Bind(..., 90, ...);
       _Stance0ApplyWhenProne = Config.Bind(..., false, ...);
     E mostrar pelo menos um exemplo análogo para Stance 1 (Recovery/2.00/100/...) e Stance 3 (Recovery/1.50/95/...). Stance 2 mantém None/1.00. -->

---

### PA-01-11 · A — Gap · 🟡 Importante {#pa-01-11}

**`SettingChanged` mencionado mas sem stub mostrando como invalidar cache**

**Problema:** §4 diz "subscribe em `SettingChanged` para invalidar cache" e o checklist §8 inclui "subscrever `SettingChanged` para invalidar cache da stance ativa", mas **não há stub mostrando o handler**. O AC funcional exige:

> "Mudar `Mode` ou `Intensity` no F12 com a stance já ativa atualiza o efeito sem precisar reiniciar a raid (efeito novo aplica em < 1 segundo)."

**Por que importa:** sem stub concreto, o implementador pode (a) esquecer de assinar todas as 20 entradas, (b) esquecer de reler `Mode/Intensity` da stance que **já estava ativa** quando a config mudou (não basta atualizar o cache; tem que aplicar imediato — incluindo registrar/remover speed limit conforme `Modifies Movement Speed` mudou).

**Decisão:** `[ ]` Pendente
<!-- Em §5, adicionar stub do handler:
       private static void OnConfigChanged(object sender, EventArgs e) {
           // re-resolve a config da stance ATIVA imediatamente
           int active = StanceManager.GetActiveStance();
           StanceManager.ApplyStanceConfig(active);  // re-cache + reaplica speed limit + reavalia prone
       }
     Em Awake, foreach das 20 entries: entry.SettingChanged += OnConfigChanged. -->

---

### PA-01-12 · B — Edge · 🟡 Importante {#pa-01-12}

**Recovery postfix usa `gameWorld.MainPlayer` cache em vez de comparar `__instance` direto — alocação implícita e indireção desnecessária**

**Problema:** O stub do `StanceStaminaRecoveryPatch.Postfix` faz:

```csharp
var player = StanceManager.GetCachedGameWorld()?.MainPlayer;
if (player?.ProceduralWeaponAnimation?.IsAiming == true)
    return;
```

Esse postfix roda em **todo frame que regen é avaliado, para todo `Player` no mundo**. Buscar `gameWorld → MainPlayer → ProceduralWeaponAnimation → IsAiming` por chamada é caminho longo, e a referência ao `gameWorld` cache pode estar nula em janelas de transição (dying frame, raid teardown).

Adicionalmente — e ligado ao PA-01-04 — o postfix nem usa `__instance`. O caminho correto é injetar `__instance` e comparar com `MainPlayer.Physical` direto.

**Por que importa:** este é um postfix em **hot path** (regen avaliada todo frame). Pelo skill `csharp-mod-best-practices` §1: "no allocations in hot paths". Mesmo que o `?.` não aloque, o número de derefs por frame × 30+ Players (jogador + bots) escala. Vale otimizar com check direto.

**Decisão:** `[ ]` Pendente
<!-- [HarmonyPostfix]
     private static void Postfix(PlayerPhysicalClass __instance, ref float __result) {
         var gw = Singleton<GameWorld>.Instance;
         if (gw?.MainPlayer == null || __instance.Player_0 != gw.MainPlayer) return;
         if (gw.MainPlayer.ProceduralWeaponAnimation?.IsAiming == true) return;
         if (StanceStaminaState.Mode != EStanceStaminaMode.Recovery || StanceStaminaState.IsSuspendedByProne) return;
         __result *= StanceStaminaState.Intensity;
     }
     Cache `MainPlayer` numa static field atualizada em OnRaidStart pode evitar a chamada `Singleton.Instance` por frame, mas é otimização secundária. -->

---

### PA-01-13 · B — Edge · 🟢 Menor {#pa-01-13}

**`AccessTools.Method(typeof(...), "method_10")` por nome literal é frágil entre versões do EFT**

**Problema:** O stub usa string literal `"method_10"` para resolver o método ofuscado. Pelo skill `spt-mod-best-practices` §1:

> Para alvos ofuscados (`GClass####`, `Class####`), resolva o `MethodBase` em um helper estático usando uma **assinatura/predicado estável** (return type + parameter list + name fragment). Não hardcode `GClassNNNN` — esses números mudam entre patches do EFT.

`method_N` é gerado pelo descompilador automaticamente; entre versões do EFT a numeração pode ser diferente. Para sobrevivência ao próximo patch, melhor resolver por assinatura.

**Por que importa:** se este patch sobreviver ao PA-01-01 (mantido para algum uso), ao subir para EFT 0.16.x+1 o nome pode ser `method_11` ou `method_9` e o `AccessTools.Method` retornaria null, quebrando o registro do patch silenciosamente (Harmony loga warning, jogo continua sem o patch).

**Decisão:** `[ ]` Pendente
<!-- Se PA-01-01 remover o postfix em method_10 totalmente, este PA fica sem objeto e pode ser fechado.
     Caso seja retido, resolver via:
       static readonly MethodBase Target = AccessTools.GetDeclaredMethods(typeof(PlayerPhysicalClass))
           .First(m => m.ReturnType == typeof(float)
                    && m.GetParameters().Length == 0
                    && m.Name.StartsWith("method_"));   // ainda fraco — adicionar um caller match
     Ideal: encontrar o caller (HandsStamina.Consume usa o consumption func) e resolver pelo callee único. -->

---

### PA-01-14 · A — Gap · 🟢 Menor {#pa-01-14}

**Itens "a confirmar" deveriam ser resolvidos antes do build**

**Problema:** A spec técnica tem 4 TODOs explícitos:

> - "Mathf.Sqrt(Float_5) // peso da arma (**a confirmar**)"
> - "Single_0 // normalização (**a confirmar**)"
> - §8 Checklist: "**Confirmar** nome ofuscado de `PlayerPhysicalClass.method_10`"
> - §8 Checklist: "**Confirmar** `EPose` API (campo público vs reflection)"

Pelo padrão definido no `/create-technical-spec`: "Não inventar nomes de classe ou método. Se não achar, registrar **TODO confirmar:** explicitamente." — registrar é OK, mas resolver isso é trabalho da spec, não do build.

**Por que importa:** especialmente o item "nome ofuscado de method_10" amarra-se ao PA-01-01. Se a estratégia é descartada, esses TODOs caem junto. Se algum ficar, devem ser resolvidos antes — `/build-item` segue a spec literalmente; itens "a confirmar" no checklist viram débito.

**Decisão:** `[ ]` Pendente
<!-- Após resolver PA-01-01: revisitar os "a confirmar" remanescentes e fechá-los na spec. Se Float_5 e Single_0 não forem mais relevantes (porque method_10 foi removido), simplesmente apagar a fórmula — vira referência informativa. -->

---

### PA-01-15 · B — Edge · 🟢 Menor {#pa-01-15}

**Sem orientação para `HarmonyPriority` em conflito com mods de stamina**

**Problema:** §7 Riscos diz:

> Compatibilidade com mods de stamina (ex.: `SPT-BetterArmStamina`): ambos pacheiam o mesmo método. Postfixes Harmony empilham — o resultado depende da ordem. Documentar no README que pode haver dupla modulação.

Mas não define ordem explícita via `[HarmonyPriority(Priority.Low/High)]` nem indica o critério (queremos rodar antes ou depois de outros multiplicadores externos?).

**Por que importa:** se `BetterArmStamina` aplicar postfix com `Priority.Normal` e nós também, a ordem é alfabética (do nome do plugin) ou pela ordem de registro — imprevisível. Composição de multiplicadores é commutativa (multiplicação), então o resultado matemático é o mesmo, mas se houver lógica condicional (early-return de outro mod), a ordem importa.

**Decisão:** `[ ]` Pendente
<!-- Definir [HarmonyPriority(Priority.Low)] nos postfixes do mod (rodam por último, multiplicação se aplica em cima do que outros mods já fizeram). Documentar essa escolha. -->

---

### PA-01-16 · A — Gap · 🟢 Menor {#pa-01-16}

**Logging discipline ausente — sem `ManualLogSource` por plugin**

**Problema:** Nenhum stub mostra inicialização de `ManualLogSource` nem usa `Plugin.Logger.LogX(...)`. Pelo skill `spt-mod-best-practices` §6 e `csharp-mod-best-practices` §8:

> Um `ManualLogSource` por plugin, nomeado pelo GUID do plugin. (...) `LogInfo` para eventos de lifecycle uma única vez. **Nunca** `LogInfo` por frame.

O mod já tem `Plugin.Logger` em [modded/Plugin.cs:12](../../modded/Plugin.cs) — basta usar.

**Por que importa:** sem logs, debug em raid (drain inesperado, speed limit grudado, prone não suspendendo) vai ser feito por inspeção visual. Adicionar uns poucos `LogInfo` em momentos de lifecycle (raid start/end, troca de stance) economiza horas de debug. E o AC "Falha isolada: há entrada de `LogError` no console" exige uso explícito do `Logger`.

**Decisão:** `[ ]` Pendente
<!-- Em cada handler/patch, fazer Plugin.Logger.LogDebug($"[StanceManager] OnStanceChanged {prev} → {next}, mode={StanceStaminaState.Mode}") gated por uma config `EnableDebugLogging`. Erros sempre logados como Error. -->

---

## Próximos passos

1. Resolver os 5 🔴 bloqueadores na spec técnica (editar [001-stamina-e-velocidade-technical-spec.md](001-stamina-e-velocidade-technical-spec.md) inline). Para PA-01-01, isso significa reescrever §1 Estratégia, §2 Pontos de patch, §5 Stubs de código (substituir `StanceStaminaDrainPatch` por método tick em `StanceManager`) e §6 Fluxo de dados.
2. Atualizar a spec técnica conforme cada decisão tomada nos comentários `<!--` -->` deste arquivo.
3. Rodar `/review-technical-spec` novamente — gera `technical-review-02.md`. Se `technical-review-02` validar que todos os pontos `PA-01-XX` foram corrigidos, ele referencia: `✅ PA-01-XX resolvido na spec — fechado.`
4. Quando `technical-review-NN` mais recente não tiver 🔴, executar `/build-item`.
