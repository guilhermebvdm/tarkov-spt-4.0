# 018 — Correr agachado e rastejar rápido (crouch-run + high-crawl) · Spec Técnica

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional:** [018-rastejar-rapido-01-spec.md](018-rastejar-rapido-01-spec.md)
**Criado:** 2026-09-09

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

**Memória consultada:** snapshot de 2026-08-30 (Sessão 16) · pendências que afetam: **[P-11.1]** (velocidade que fica presa devagar — usada como corner case na spec funcional; a fórmula de `MaxSpeed` que essa pendência já mapeou é reconfirmada abaixo, linha idêntica) / nenhuma pendência bloqueia diretamente este item.
**Docs técnicos lidos (gatilho disparado):** `spt-antipatterns.md` (obrigatório sempre) — AP-01 a AP-09 avaliados na §9.
**Grafo:** `graphify-eft` (MCP) indisponível nesta sessão — fallback por Grep manual, com a MESMA disciplina de auditoria de overrides do AP-03 (ver §1 e §9 check 3).

## 1. Estratégia

Esta feature tem **duas mecânicas nativas de origem diferentes** para "segurar sprint numa postura baixa" — descobertas só investigando o Assembly, não presumidas:

### 1.1 Agachado — o jogo REALMENTE levanta o personagem

`RunStateClass.EnableSprint(bool enabled, bool isToggle)` ([RunStateClass.cs:401-420](../../../../references/eft-decompiled/Assembly-CSharp/RunStateClass.cs#L401-L420)) é o método que trata o pedido de sprint enquanto o jogador está em pé OU agachado (`RunStateClass` cobre as duas poses — "agachado" no EFT não é uma classe de estado própria, é só `PoseLevel < 1` dentro do mesmo estado de corrida/caminhada):

```csharp
public override void EnableSprint(bool enabled, bool isToggle = false)
{
    if (!MovementContext.CanSprint) return;
    if (MovementContext.MovementDirection.y > 0.1f)
    {
        MovementContext.EnableSprint(enabled);
        if (!MovementContext.IsSprintEnabled || !MovementContext.SetPoseLevel(1f))
        {
            MovementContext.EnableSprint(enable: false);
            MovementContext.ResetSpeedAfterSprint();
        }
    }
    else if (!isToggle) { Bool_1 = enabled; }
}
```

Ou seja: pedir sprint andando para frente (`MovementDirection.y > 0.1`) chama `SetPoseLevel(1f)` **incondicionalmente** — é isso que levanta o personagem, mesmo vindo de agachado. `MovementContext.CanSprint` ([MovementContext.cs:1240-1270](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1240-L1270)) só olha condições físicas (perna ferida, remédio) — nunca a pose atual.

### 1.2 Prone — o jogo NÃO levanta, mas também não acelera de verdade

`ProneMoveStateClass` **sobrescreve** `EnableSprint` com uma implementação totalmente diferente ([ProneMoveStateClass.cs:83-105](../../../../references/eft-decompiled/Assembly-CSharp/ProneMoveStateClass.cs#L83-L105)):

```csharp
public override void EnableSprint(bool enabled, bool isToggle = false)
{
    Bool_5 = enabled;   // só guarda a flag local — NUNCA chama MovementContext.EnableSprint nem SetPoseLevel
}

public override void ManualAnimatorMoveUpdate(float deltaTime)
{
    ...
    else if (Bool_5)
    {
        ChangeSpeed(EFTHardSettings.Instance.DELTA_SPEED);
        float num = Math.Min(MovementContext.MaxSpeed, MovementContext.StateSpeedLimit);
        if (MovementContext.CharacterMovementSpeed >= num) { Bool_5 = false; }
    }
}
```

**Achado importante (corrige a suposição da spec funcional):** segurar sprint deitado **não** levanta o jogador — mas também não é um "high-crawl" de verdade: só acelera a rampa de velocidade até o **mesmo teto** que o rastejo normal já usaria (`MovementContext.MaxSpeed`, que é **independente de pose** — ver §1.3). Na prática, hoje isso só faz o jogador chegar mais rápido à velocidade de rastejo já existente, não ir além dela — o que bate com a percepção do usuário de que "o prone continua lento" mesmo segurando sprint.

**Conclusão de design:** as duas posturas precisam de patches **diferentes**:
- **Agachado:** interceptar `RunStateClass.EnableSprint` para impedir o `SetPoseLevel(1f)` quando a feature estiver ativa (Prefix condicional que substitui o comportamento, não um Postfix — o `SetPoseLevel(1f)` já teria rodado antes de qualquer Postfix conseguir reagir).
- **Prone:** não precisa impedir nada (o jogo já não levanta) — precisa só de um **boost real de velocidade** acima do teto vanilla enquanto a tecla está segurada, via o mesmo mecanismo de `MaxSpeed` que o mod já usa em outra feature (§1.3).

### 1.3 Velocidade máxima é pose-independente — reaproveitar o patch que o mod já tem

`MovementContext.MaxSpeed` ([MovementContext.cs:910](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L910)):

```csharp
public float MaxSpeed => GClass2298.Evaluate(Singleton<BackendConfigSettingsClass>.Instance.WalkSpeed, (float)SkillManager.Strength.SummaryLevel / 60f);
```

Confirma a fórmula já mapeada na pendência [P-11.1] da memória do mod (skill de Força, fração `/60`) — **não depende de pose nem de estado** (agachado, prone e em pé usam o MESMO `MaxSpeed`; a diferença de velocidade entre posturas vem de outro lugar na cadeia de movimento, não de `MaxSpeed`). O mod **já tem um Postfix nessa property**: `MovementContextSpeedPatch` ([modded/Patches/MovementContextSpeedPatch.cs:8-23](../../modded/Patches/MovementContextSpeedPatch.cs#L8-L23)), que multiplica `__result` por `Plugin._WalkSpeedMultiplier.Value` **incondicionalmente** (afeta em pé, agachado e prone igualmente).

Este item precisa de um multiplicador **condicional** — só quando (a) a feature está habilitada no F12, (b) o jogador está na pose certa (agachado-não-prone, ou prone), e (c) a tecla de sprint está sendo segurada naquela pose. **Não reaproveitar o `_WalkSpeedMultiplier` existente** (ele já é usado para outra coisa, incondicional); criar um Postfix adicional na mesma property, com suas próprias condições — Harmony permite múltiplos Postfix na mesma property sem conflito, desde que cada um só leia/multiplique `__result` (nunca sobrescreva do zero).

### 1.4 Stamina de pernas — sistema diferente do item 012, mas com API canônica pública reaproveitável

`PlayerPhysicalClass.Sprint(bool target)` ([PlayerPhysicalClass.cs:1063-1074](../../../../references/eft-decompiled/Assembly-CSharp/PlayerPhysicalClass.cs#L1063-L1074)) mostra como o próprio jogo dispara o dreno de stamina de sprint:

```csharp
public override void Sprint(bool target)
{
    if (target != Sprinting && (flag = target && CanSprint) != Sprinting)
    {
        Consumptions[EConsumptionType.Sprint].SetActive(this, flag);
        if (flag) { Stamina.Consume(Gclass773_0); }
    }
}
```

`Consumptions` é um `Dictionary<EConsumptionType, GClass773>` fixo por tipo ([PlayerPhysicalClass.cs:406](../../../../references/eft-decompiled/Assembly-CSharp/PlayerPhysicalClass.cs#L406)) — não dá pra registrar um NOVO `EConsumptionType` (é um enum fechado do jogo). Mas o pool de stamina (`Stamina`, tipo `GClass774` — o MESMO tipo do `HandsStamina` do item 012) expõe `AddConsumption`/`RemoveConsumption` **públicos** ([GClass774.cs:281-297](../../../../references/eft-decompiled/Assembly-CSharp/GClass774.cs#L281-L297)) que aceitam **qualquer instância** de `GClass773`, não só as do dicionário fixo:

```csharp
public Action AddConsumption(PlayerPhysicalClass.GClass773 consumption)
{
    if (!Consumptions.Contains(consumption)) { Consumptions.Add(consumption); return delegate { RemoveConsumption(consumption); }; }
    return null;
}
```

E o `Process(dt)` que efetivamente drena stamina ([GClass774.cs:304-335](../../../../references/eft-decompiled/Assembly-CSharp/GClass774.cs#L304-L335)) soma o `Delta` de **todas** as consumptions ativas e só então multiplica pelo `Multiplier` da pool inteira (`Current -= num * dt * Multiplier`, linha 335) — ou seja, **qualquer consumption que o mod registre por essa API herda automaticamente o mesmo `Multiplier`** que a skill do personagem já aplica ao dreno nativo (não foi possível confirmar nesta investigação preliminar o ponto exato onde a skill escreve `Stamina.Multiplier` — ver TODO abaixo — mas a composição matemática do `Process()` GARANTE que, seja qual for esse fator, ele desconta a sobretaxa do mod da MESMA forma que desconta o sprint nativo, sem o mod precisar reimplementar nada). Isso resolve diretamente o critério de aceite "a sobretaxa continua respeitando a progressão de habilidades": **usar essa API em vez de subtrair `Stamina.Current` na mão** (o antipattern documentado em AP-04, já visto neste mod no item 001/PA-02-01).

`GClass773.Delta` é do tipo `GClass848<float>` — um wrapper "compute" trivial ([GClass848-1.cs:4-29](../../../../references/eft-decompiled/Assembly-CSharp/GClass848-1.cs#L4-L29)), construído com `new GClass848<float>(() => valor)`: um `Func<float>` avaliado a cada leitura. Isso permite que a sobretaxa leia o F12 ao vivo (`() => Plugin._CrouchRunStaminaSurcharge.Value`), sem precisar recriar o objeto quando o usuário muda o slider.

**TODO confirmar (não bloqueia a implementação, mas vale registrar ao validar in-game):** o ponto exato onde `Stamina.Multiplier` ([GClass774.cs:76-88](../../../../references/eft-decompiled/Assembly-CSharp/GClass774.cs#L76-L88), propriedade simples, sem lógica própria) é setado a partir da skill de Força/Endurance não foi localizado nesta investigação preliminar (buscas por `.Multiplier =` no Assembly não retornaram resultado — pode estar atrás de um nome ofuscado não capturado pelas strings buscadas). Isso **não bloqueia** o design (a composição do `Process()` já garante o comportamento desejado independente de onde o `Multiplier` é setado), mas fica como validação empírica: ao testar in-game com um personagem de skill alta vs. baixa, o dreno de pernas (nativo E a sobretaxa) devem variar juntos.

### 1.5 Ganho de velocidade gradual (pedido do usuário, 2026-09-09)

A spec funcional passou a exigir que o ganho de velocidade seja **progressivo**, não um salto instantâneo no frame em que a condição de boost vira verdadeira (a perda, ao soltar a tecla ou parar de mover, continua imediata — critério de aceite já existente, sem contradição). Em vez de multiplicar `MaxSpeed` direto pelo `ConfigEntry` do multiplicador-alvo, os dois Postfix de `MaxSpeed` (§5) mantêm um **progresso de rampa** `[0..1]` por postura, avançando com `UnityEngine.Time.deltaTime` em direção a `1` enquanto a condição de boost (feature ligada + pose certa + `SprintKeyHeld` + movimento real, mesma condição do PA-01-03) for verdadeira, e caindo pra `0` **instantaneamente** (não decai gradual) assim que deixar de valer — para não contradizer o critério "soltar a tecla é imediato". O multiplicador efetivo aplicado é `Mathf.Lerp(1f, alvoConfigurado, progresso)`.

Como `MaxSpeed` é lido possivelmente mais de uma vez por frame (não confirmado nesta investigação — `UpdateCharacterControllerSpeedLimit` é uma chamada, mas outros pontos do Assembly também podem ler a property no mesmo frame), o avanço do progresso só acontece uma vez por frame (`guard` por `Time.frameCount`), para o tempo de rampa não depender de quantas vezes a property é lida.

**Nota de arquitetura (ref: PA-02-02, review 02):** os efeitos colaterais que passaram a viver dentro deste Postfix (§1.6 liga/desliga animação, §1.7 muda `PoseLevel`) tecnicamente rodam dentro do Postfix de um property GETTER (`MaxSpeed`), que por convenção deveria ser leitura pura. O guard `Time.frameCount != _lastFrame` acima, pensado originalmente só pra não avançar a rampa mais de uma vez por frame, **também** acaba protegendo contra uma eventual reentrância (se `SetPoseLevel`/o Animator disparar algum evento que releia `MaxSpeed` no mesmo frame) — mas isso é incidental ao design, não uma garantia por contrato. Não há nenhum bug demonstrado nem motivo concreto pra refatorar agora; se o `/code-review` (depois do código pronto) achar sinal real de reentrância ou comportamento inconsistente, o caminho mais limpo é mover esses dois efeitos colaterais para um novo Postfix em `RunStateClass.ManualAnimatorMoveUpdate` (método de tick genuíno, já mapeado em §1.6) e deixar este Postfix só multiplicar `__result`.

### 1.6 Animação de sprint (baixar a arma / balançar os braços) — precisa ser forçada manualmente no crouch-run

**Pedido do usuário (2026-09-09):** o crouch-run deve acionar a mesma animação de "baixar a arma e balançar os braços" que o sprint em pé já usa. Investigando onde essa animação é acionada nativamente — `RunStateClass.ManualAnimatorMoveUpdate` ([RunStateClass.cs:245-256](../../../../references/eft-decompiled/Assembly-CSharp/RunStateClass.cs#L245-L256)):

```csharp
if (!(MovementContext.MovementDirection.y <= 0.1f))
{
    if (Bool_1) { MovementContext.EnableSprint(enable: true); Bool_1 = false; }
    if (MovementContext.IsSprintEnabled && MovementContext.PoseLevel > 0.9f && MovementContext.SmoothedCharacterMovementSpeed >= 1f)
    {
        MovementContext.PlayerAnimatorEnableSprint(enabled: true);
    }
}
```

**Achado confirmado:** o gatilho nativo da animação (`MovementContext.PlayerAnimatorEnableSprint(true)`, [MovementContext.cs:3832-3835](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L3832-L3835) — repassa direto pro Animator, `PlayerAnimator_1.EnableSprint(enabled)`) exige `PoseLevel > 0.9f` — ou seja, **só dispara perto de/em pé**. Como o crouch-run mantém `PoseLevel < 1` de propósito, essa condição nativa NUNCA vai ligar a animação sozinha — confirma exatamente a suspeita do usuário. E como o crouch-run nunca faz `CurrentState` transicionar para `SprintStateClass` (permanece em `RunStateClass` o tempo todo, pose baixa), o desligamento nativo da animação (`PlayerAnimatorEnableSprint(false)`, que só acontece em `SprintStateClass.Exit()`, [SprintStateClass.cs:37-52](../../../../references/eft-decompiled/Assembly-CSharp/SprintStateClass.cs#L37-L52)) também nunca roda para esse caminho — **o mod precisa ligar E desligar essa animação por conta própria**, não só uma das duas pontas.

**Onde acoplar:** no mesmo bloco "1x por frame" que já existe em `CrouchRunMaxSpeedPatch` (§1.5/§5) para a rampa de velocidade — chamar `__instance.PlayerAnimatorEnableSprint(wantsBoost)` só na transição (edge-detect com um novo campo estático `_animatorOn`), evitando chamar o Animator todo frame à toa. **Só para o crouch-run** — o prone-run **não** usa esse mecanismo: `ProneMoveStateClass.ManualAnimatorMoveUpdate` ([ProneMoveStateClass.cs:88-105](../../../../references/eft-decompiled/Assembly-CSharp/ProneMoveStateClass.cs#L88-L105)) nunca chama `PlayerAnimatorEnableSprint` — o rastejo tem sua própria animação de velocidade (via `ChangeSpeed`), sem esse flag discreto. Forçar o flag de "sprint em pé" durante o prone não teria efeito visual esperado (ou teria um efeito visual ERRADO, tentando tocar uma animação de corrida em pé enquanto deitado) — por isso o item 1.6 é exclusivo do crouch-run.

**Risco a validar in-game (não dá pra confirmar só lendo código):** como a animação de "baixar arma e balançar braços" foi desenhada pro personagem EM PÉ, ela pode ficar visualmente estranha combinada com a pose de agachado (a animação e a pose vêm de sistemas diferentes — animator flag vs. `PoseLevel`/altura do collider). Se ficar ruim visualmente, a alternativa é não forçar esse flag e aceitar que o crouch-run não tenha o balanço de braço — decisão de sensação de jogo, não técnica.

### 1.7 Piso de postura durante o crouch-run (pedido do usuário, 2026-09-09)

Correr num agachado MUITO baixo (`PoseLevel` perto de 0) deve subir a postura pra um mínimo confortável enquanto a aceleração estiver ativa, e voltar pro `PoseLevel` de antes ao parar. Isso é só sobre CROUCH — não se aplica ao prone: quando `IsInPronePose` é verdadeiro, `CalculatePoseLevelValues` ([MovementContext.cs:2176-2181](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L2176-L2181)) usa uma altura de collider **fixa** (`0.3f`), ignorando `PoseLevel` — ou seja, "postura muito baixa" não é um conceito visual que existe em prone da mesma forma.

**Mecanismo:** usar `MovementContext.SetPoseLevel(float, force: true)` ([MovementContext.cs:2139](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L2139)), a mesma API já usada em outros pontos do próprio Assembly para mudar `PoseLevel` fora do fluxo normal de input (`ChangePose`/`ProneMoveStateClass.Enter`, `force: true` para não ser bloqueado por `Physical.MaxPoseLevel` — ver linha 2155). Diferente do caso "impedir levantar" (§1.1, onde só baixar é sempre seguro — `CanStandAt` tem um atalho pra isso, [MovementContext.cs:3310-3313](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L3310-L3313)), **subir** a pose passa pela checagem real de colisão (`CanStandAt`, mesmo método) — então se não houver espaço nem pro nível intermediário, `SetPoseLevel` retorna `false` e a pose simplesmente não muda, com segurança herdada de graça (o jogo já trata esse caso).

**Fluxo, no mesmo bloco por-frame já usado pra rampa/animação (§1.5/§1.6):**
1. Na borda de subida de `wantsBoost` (false→true): capturar `_poseLevelBeforeRun = __instance.PoseLevel` (o valor ANTES de qualquer alteração).
2. Enquanto `wantsBoost` for true, todo frame: se `__instance.PoseLevel < <threshold configurável>`, chamar `SetPoseLevel(<alvo configurável>, force: true)`. Se já estiver no alvo ou acima, não chama nada (evita recalcular collider à toa).
3. Na borda de descida (true→false): chamar `SetPoseLevel(_poseLevelBeforeRun, force: false)` — restaura exatamente a postura de antes de começar a correr, mesmo que o jogador tenha ajustado a pose manualmente durante a corrida (comportamento pedido explicitamente pelo usuário: "volta pra PoseLevel que tava antes do sprint"). Chamar isso incondicionalmente é seguro mesmo se a pose nunca tiver mudado — `SetPoseLevel` já é um no-op quando o valor não muda (`MovementContext.cs:2145-2148`).

**ref: PA-02-01 — por que `force: false`, não `force: true`:** com `force: true`, `SmoothedPoseLevel` salta pro valor final na hora ([MovementContext.cs:2171](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L2171)) — a postura "teleporta" em vez de animar, bem mais perceptível do que qualquer corte de velocidade (é a altura do personagem/câmera mudando, não só um número). Com `force: false`, a transição sobe/desce suavemente na MESMA taxa que `Plugin.ApplyMovementSpeeds()` já configura via `EFTHardSettings.Instance.POSE_CHANGING_SPEED` ([modded/Plugin.cs:1431-1439](../../modded/Plugin.cs#L1431-L1439), item 005 — slider `Crouch Speed Multiplier`) — ganha suavidade configurável de graça, sem nenhuma config nova. O único efeito colateral do branch `!force` (disparar `Skills.PushUp.Complete`, linha 2155-2163) só ocorre perto de `_player.Physical.MaxPoseLevel` (quase em pé) — irrelevante pro alvo intermediário (`0.5` default). A checagem de colisão (`CanStandAt`) roda igual não importa o `force`, então a segurança contra teto baixo não muda.

### 1.8 Fix pós-validação in-game (2026-09-09) — supersede §1.5, muda §1.4 para o agachado

**Validação do usuário:** crouch-run funcionou; prone-run não mudava a velocidade mesmo com `Prone Run Speed Multiplier = 2.0`.

**Causa raiz confirmada:** `ProneRunObservePatch` (a versão original deste patch, agora removida) escutava `ProneMoveStateClass.EnableSprint` — mas esse método só existe (e só é chamado) enquanto o `CurrentManagedState` do jogador É `ProneMoveStateClass`, ou seja, enquanto ele já está se movendo em prone. Se a tecla de correr for pressionada com o jogador prone e **parado**, o estado ativo é `ProneIdleStateClass` ([ProneIdleStateClass.cs](../../../../references/eft-decompiled/Assembly-CSharp/ProneIdleStateClass.cs)), que **não sobrescreve `EnableSprint`** — herda de `IdleStateClass` sem nenhuma lógica de prone. A intenção de sprint nunca era capturada nesse caso, e — diferente do sprint em pé/agachado, que tem um mecanismo de "latch" nativo (`RunStateClass.Bool_1`, [RunStateClass.cs:247-251](../../../../references/eft-decompiled/Assembly-CSharp/RunStateClass.cs#L247-L251): se você segura sprint parado, o jogo lembra e ativa quando você anda) — **não existe latch equivalente para prone**. Resultado: a intenção se perdia e nunca era recuperada, mesmo depois de o jogador começar a andar.

**Onde o input de sprint REALMENTE entra, confirmado em [Class1728.cs:99-104](../../../../references/eft-decompiled/Assembly-CSharp/Class1728.cs#L99-L104)** (o roteador de `ECommand` do jogador):
```csharp
case ECommand.ToggleSprinting:
    Player_0.ToggleSprint();      // tecla pressionada — modo segurar OU alternar, não importa
    break;
case ECommand.EndSprinting:
    Player_0.EnableSprint(enable: false);  // tecla solta — só existe no modo segurar
    break;
```
`Player.ToggleSprint()` ([Player.cs:26008-26012](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L26008-L26012)) e `Player.EnableSprint(bool)` ([Player.cs:26000-26006](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L26000-L26006)) são os dois únicos pontos de entrada do INPUT de sprint, **independente de qual sub-estado de movimento esteja ativo**. `Player.ToggleSprint()` internamente faz `bool enable = !Physical.Sprinting` antes de delegar pro estado atual — mas **não dá pra reaproveitar essa leitura**: como visto acima, `Physical.Sprinting` nunca vira `true` durante prone (porque `ProneMoveStateClass.EnableSprint` nunca chama `Physical.Sprint()`), então esse flag ficaria sempre preso em "quer ligar".

**Fix aplicado:** `ProneRunPatch.cs` reescrito — `ProneRunObservePatch` removido, substituído por dois patches novos sobre os métodos de `Player`:
- `PlayerToggleSprintPatch` (Prefix em `Player.ToggleSprint()`): alterna um flag PRÓPRIO (`ProneRunSprintIntent.Held = !Held`), em vez de espelhar `Physical.Sprinting`.
- `PlayerEnableSprintPatch` (Postfix em `Player.EnableSprint(bool)`): força `Held = false` quando `enable == false` (tecla solta, modo segurar).

Isso funciona independente de o jogador estar parado ou andando no momento exato do input, e também herda de graça qualquer força-desligamento nativo de sprint (arame farpado, penalidade de velocidade — ambos chamam `Player.EnableSprint(false)` diretamente, [BarbedWire.cs:37](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/BarbedWire.cs#L37), [HandlerSpeedPenalty.cs:75](../../../../references/eft-decompiled/Assembly-CSharp/EFT.GameTriggers/HandlerSpeedPenalty.cs#L75)).

**Por que o crouch NÃO foi migrado pra esse mesmo mecanismo:** o crouch já funciona (validado in-game) porque `RunStateClass.EnableSprint` (o alvo do Prefix de `CrouchRunEnableSprintPatch`) é invocado igual não importa hold ou toggle — Harmony patcheia o MÉTODO, não o call-site. O risco NÃO testado é o mesmo tipo de bug do prone: se o jogador pressiona sprint agachado e PARADO (`CurrentManagedState` pode ser `IdleStateClass`, que também não teria overrides relevantes), a supressão do `SetPoseLevel(1f)` não dispara nesse instante — mas o `Bool_1`-latch nativo (que SÓ existe pra `RunStateClass`, não pra prone) resolve isso mais tarde chamando `MovementContext.EnableSprint(true)` DIRETAMENTE (não `RunStateClass.EnableSprint`), o que na prática não força `SetPoseLevel(1f)` mesmo sem nosso patch — mas também não passa pelo nosso Prefix, então `CrouchRunEnableSprintPatch.SprintKeyHeld` não seria setado, e o BOOST DE VELOCIDADE não ativaria nesse caso específico (crouch parado → sprint → andar). **Não corrigido nesta rodada** (o usuário não reportou esse caso como quebrado) — documentado aqui como corner case conhecido, mesmo padrão de fix disponível se precisar (ver PA-01-04 na review 01).

**Descoberta sobre stamina (decide a §1.4 para cada postura):** como `ProneMoveStateClass.EnableSprint` nunca chama `Physical.Sprint()`, e essa é a única via que ativa `Consumptions[EConsumptionType.Sprint]` ([PlayerPhysicalClass.cs:1063-1074](../../../../references/eft-decompiled/Assembly-CSharp/PlayerPhysicalClass.cs#L1063-L1074)), **o jogo não drena stamina nativamente ao correr em prone**. Já o crouch-run chama `movementContext.EnableSprint(enabled)` dentro do próprio Prefix (`CrouchRunEnableSprintPatch.cs`), então **o dreno nativo de sprint se aplica normalmente ao agachado** — confirmado pelo usuário in-game ("o consumo em 1.0 já é suficiente e muito bem aplicado"). Por isso: `_CrouchRunStaminaSurcharge` removida; `_ProneRunStaminaSurcharge` mantida (sem ela, o prone-run seria de graça).

**Rampa removida (pedido do usuário — "não fez sentido nenhum ele no mod"):** `_rampProgress`/`Mathf.Lerp` removidos dos dois `MaxSpeedPatch`. O multiplicador de velocidade agora aplica (e remove) instantaneamente junto com `wantsBoost`, igual à perda — sem assimetria entre ganho e perda.

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`RunStateClass.cs:401`](../../../../references/eft-decompiled/Assembly-CSharp/RunStateClass.cs#L401) (`EnableSprint`) | Prefix condicional (skip do original) | Impedir `SetPoseLevel(1f)` quando o crouch-run estiver ativo para este pedido de sprint; senão, deixar o original rodar normalmente. |
| [`Player.cs:26008`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L26008) (`ToggleSprint`) | Prefix (observação) | **Substitui o alvo original (`ProneMoveStateClass.EnableSprint`, fix 2026-09-09 — ver §1.8)**: alterna um flag próprio (`ProneRunSprintIntent.Held`) a cada tecla-pressionada de sprint, independente do sub-estado nativo ativo (parado/andando). |
| [`Player.cs:26000`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L26000) (`EnableSprint`) | Postfix (observação) | Força `Held = false` quando a tecla é solta (`enable == false`, modo segurar) — inclui força-desligamentos nativos (arame farpado, penalidade de velocidade). |
| [`MovementContext.cs:2139`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L2139) (`SetPoseLevel`) | Chamado diretamente pelo mod (não é Harmony patch) | Piso de postura do crouch-run (§1.7): sobe a pose se estiver muito baixa durante a corrida, restaura ao soltar. Reaproveita a mesma API pública já usada pelo próprio jogo em outros pontos. |
| [`MovementContext.cs:910`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L910) (`MaxSpeed`, getter) | Postfix condicional (novo — não reaproveitar `MovementContextSpeedPatch`) | Multiplicar `__result` só quando a postura-acelerada correta estiver ativa. No caso do crouch-run, o mesmo bloco também liga/desliga a animação de sprint (§1.6) via `PlayerAnimatorEnableSprint`, já que o gate nativo dessa animação exige `PoseLevel > 0.9f`. |
| Nenhum patch em `PlayerPhysicalClass`/`GClass774` | — | Consumo de stamina via API pública já exposta (`Stamina.AddConsumption`/`RemoveConsumption`) chamada diretamente do código do mod — não precisa de Harmony aqui. |

**Auditoria de overrides (AP-03):** `EnableSprint(bool, bool)` é `virtual`/`override` em pelo menos 3 classes confirmadas neste dump — `RunStateClass` (agachado/em pé), `SprintStateClass` (já sprintando, [SprintStateClass.cs:97-100](../../../../references/eft-decompiled/Assembly-CSharp/SprintStateClass.cs#L97-L100)) e `ProneMoveStateClass` (prone), além de `ProneIdleStateClass`/`IdleStateClass` que **não sobrescrevem** o método (herdam sem lógica de prone/sprint — a causa raiz do fix §1.8). Este item patcheia **deliberadamente `RunStateClass`** (crouch, para suprimir `SetPoseLevel(1f)` — precisa ser exatamente esse override, é onde a pose é forçada) e, para prone, **subiu um nível acima da hierarquia de estados**: `Player.ToggleSprint()`/`Player.EnableSprint(bool)` (§1.8), que são chamados pelo roteador de comandos ANTES de qualquer despacho por sub-estado — evita depender de qual override de `EnableSprint` (se algum) está ativo no momento. `SprintStateClass.EnableSprint` (linha 97-100) continua fora de escopo — só repassa para `MovementContext.EnableSprint(enabled && CanSprint)` e roda quando o jogador já está no estado Sprint (já em pé), sem pose baixa a proteger.

## 3. Novas propriedades F12 (BepInEx)

> Seção nova sugerida: `Crouch & Prone Sprint` (ou anexar a `Stance Transition & Kick` — decisão de organização, não técnica; ver `/review-mod-properties` se preferir revisitar depois).
>
> **Atualizado em 2026-09-09 (ver §1.8):** removidas `Crouch Run Stamina Surcharge` (o dreno nativo de sprint já se aplica sozinho ao agachado, confirmado in-game) e as duas `Speed Ramp-Up (s)` (sem rampa — ganho e perda de velocidade são ambos instantâneos, a pedido do usuário). `Prone Run Stamina Surcharge` foi **mantida** — confirmado no Assembly que o jogo não cobra stamina nativamente ao correr em prone.

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `Crouch & Prone Sprint` | `Enable Crouch Run` | bool | `false` | — | — | Quando ativado, segurar a tecla de correr agachado mantém o personagem agachado e aumenta a velocidade, em vez de levantá-lo. Desativado por padrão até validação in-game. |
| `Crouch & Prone Sprint` | `Crouch Run Speed Multiplier` | float | `1.25` | 1.0 – 2.0 <!-- review: a spec funcional pede que o teto NUNCA alcance a velocidade de sprint em pé; proposta abaixo (§7) é um clamp em runtime contra a velocidade de sprint atual, não travar a faixa do slider nesse valor — decisão pendente do usuário na spec funcional --> | — | Multiplicador de velocidade ao correr agachado (com Enable Crouch Run ativo). Sem sobretaxa de stamina própria — o dreno nativo de sprint já se aplica sozinho (validado in-game em 2026-09-09). |
| `Crouch & Prone Sprint` | `Crouch Run Pose Floor Threshold` | float | `0.3` | 0 – 1.0 | — | Se a postura estiver mais baixa que isto ao correr agachado, ela sobe para o valor de "Pose Floor Target" (evita correr agachado demais). Defina como 0 para desativar este ajuste. |
| `Crouch & Prone Sprint` | `Crouch Run Pose Floor Target` | float | `0.5` | 0 – 1.0 | — | Postura mínima mantida durante o correr agachado quando a postura atual está abaixo do "Pose Floor Threshold". Ao soltar a tecla, a postura volta exatamente para a que estava antes de começar a correr. |
| `Crouch & Prone Sprint` | `Enable Prone Run` | bool | `false` | — | — | Quando ativado, segurar a tecla de correr rastejando (prone) acelera o rastejo além do teto atual, sem alterar a postura. Desativado por padrão até validação in-game. |
| `Crouch & Prone Sprint` | `Prone Run Speed Multiplier` | float | `1.25` | 1.0 – 2.0 <!-- review: mesma decisão de clamp do Crouch Run --> | — | Multiplicador de velocidade ao rastejar com a tecla de correr segurada (com Enable Prone Run ativo). |
| `Crouch & Prone Sprint` | `Prone Run Stamina Surcharge` | float | `8.0` | 0 – 30 | — | Dreno extra de stamina (por segundo) ao rastejar acelerado. Mantida porque o rastejo acelerado NÃO aciona o dreno nativo de sprint (`ProneMoveStateClass.EnableSprint` nunca chama `Physical.Sprint()` — ver §1.8). |

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Plugin.cs` | MODIFICAR | 10 novos `ConfigEntry` (§3 — inclui os 2 tempos de rampa e os 2 de piso de postura, adicionados em 2026-09-09), nova seção F12 `Crouch & Prone Sprint`. |
| `modded/Patches/CrouchRunPatch.cs` | CRIAR | Prefix condicional em `RunStateClass.EnableSprint` (impede `SetPoseLevel(1f)`) + Postfix condicional em `MovementContext.MaxSpeed` (boost) + registro/remoção da sobretaxa de stamina via `Stamina.AddConsumption`. |
| `modded/Patches/ProneRunPatch.cs` | CRIAR | Postfix em `ProneMoveStateClass.EnableSprint` (cacheia a tecla segurada) + Postfix condicional em `MovementContext.MaxSpeed` (boost) + sobretaxa de stamina, mesmo padrão do arquivo acima. |
| `mods/stancesAndCameraPositionSPT4.0.11/PROPRIEDADES.md` | MODIFICAR | Nova seção "Crouch & Prone Sprint" com as 10 props. |

## 5. Stubs de código

```csharp
// modded/Patches/CrouchRunPatch.cs
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace CameraRotationMod.Patches
{
    // Impede o SetPoseLevel(1f) que RunStateClass.EnableSprint aplicaria ao pedir sprint agachado.
    // ref: Assembly-CSharp/RunStateClass.cs:401-420
    public class CrouchRunEnableSprintPatch : ModulePatch
    {
        // ref: PA-01-03 — só significa "o jogo está pedindo sprint agora" (evento down/up da tecla).
        // NUNCA usar sozinho para decidir boost/dreno — ver CrouchRunMaxSpeedPatch, que recomputa
        // o movimento real a cada frame. Sem essa separação, segurar a tecla de sprint PARADO (ex.:
        // o mesmo bind usado pra "prender a respiração" durante ADS) vazaria velocidade/stamina extra.
        public static bool SprintKeyHeld;

        // ref: PA-01-02 — chamado no reset de raid do mod (StanceManager.ResetState ou equivalente).
        public static void ResetState() => SprintKeyHeld = false;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(RunStateClass), nameof(RunStateClass.EnableSprint));
        }

        [PatchPrefix]
        private static bool Prefix(RunStateClass __instance, MovementContext ___MovementContext, bool enabled, bool isToggle)
        {
            try
            {
                // ref: PA-01-01 — só o jogador local. Bots/peers Fika passam direto pro vanilla.
                Player player = Traverse.Create(___MovementContext).Field<Player>("_player").Value;
                if (player == null || !player.IsYourPlayer) return true;

                bool featureOn = Plugin._EnableCrouchRun?.Value ?? false;
                if (!featureOn || !enabled) { SprintKeyHeld = false; return true; } // deixa o original rodar (comportamento vanilla)
                if (___MovementContext.PoseLevel >= 1f || ___MovementContext.IsInPronePose) { SprintKeyHeld = false; return true; } // já em pé / prone: fora de escopo
                if (___MovementContext.MovementDirection.y <= 0.1f) { return true; } // espelha a condição original (não é pedido de sprint pra frente)
                if (!___MovementContext.CanSprint) { SprintKeyHeld = false; return true; }

                // Réplica do lado "manter sprint" do original, SEM o SetPoseLevel(1f):
                ___MovementContext.EnableSprint(enabled);
                SprintKeyHeld = ___MovementContext.IsSprintEnabled;
                if (!SprintKeyHeld) { ___MovementContext.EnableSprint(enable: false); ___MovementContext.ResetSpeedAfterSprint(); }
                return false; // skip do original — pose não é tocada
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"[CrouchRun] Prefix falhou, caindo pro vanilla: {ex}");
                SprintKeyHeld = false;
                return true;
            }
        }
    }

    // Boost condicional de MaxSpeed — recalcula TODO frame se o crouch-run deve valer agora,
    // nunca confia só na flag de tecla (ver PA-01-03). Ganho gradual (ref: §1.5): o multiplicador
    // efetivo faz Lerp(1, alvo, progresso), progresso sobe com Time.deltaTime e cai a zero na
    // hora quando a condição para de valer (perda é imediata, ganho é progressivo). O mesmo bloco
    // também liga/desliga a animação de "baixar arma e balançar braços" do sprint (ref: §1.6) —
    // o gate nativo dela (RunStateClass.cs:252) exige PoseLevel > 0.9f, então nunca dispara sozinho
    // com o crouch-run mantendo a pose baixa.
    // ref: Assembly-CSharp/EFT/MovementContext.cs:910
    public class CrouchRunMaxSpeedPatch : ModulePatch
    {
        private const float MovementEpsilonSqr = 0.0001f;

        private static float _rampProgress; // 0..1
        private static bool _animatorSprintOn; // ref: §1.6 — edge-detect pra não chamar o Animator todo frame
        private static bool _poseFloorEngaged; // ref: §1.7 — se estamos com o piso de postura ativo agora
        private static float _poseLevelBeforeRun; // ref: §1.7 — pra restaurar exatamente ao soltar
        private static int _lastFrame = -1;

        // ref: PA-01-02 — reset de raid.
        public static void ResetState()
        {
            _rampProgress = 0f;
            _animatorSprintOn = false;
            _poseFloorEngaged = false;
            _lastFrame = -1;
        }

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(MovementContext), nameof(MovementContext.MaxSpeed));
        }

        [PatchPostfix]
        private static void Postfix(MovementContext __instance, ref float __result)
        {
            // ref: PA-01-01 — só o jogador local.
            Player player = Traverse.Create(__instance).Field<Player>("_player").Value;
            if (player == null || !player.IsYourPlayer) return;

            bool wantsBoost = CrouchRunEnableSprintPatch.SprintKeyHeld
                && __instance.PoseLevel < 1f
                && !__instance.IsInPronePose
                // ref: PA-01-03 — reconfirma movimento real AGORA, não o que era verdade quando a tecla foi apertada.
                && __instance.MovementDirection.sqrMagnitude > MovementEpsilonSqr;

            if (Time.frameCount != _lastFrame) // avança a rampa só 1x por frame, não por leitura da property
            {
                _lastFrame = Time.frameCount;
                if (wantsBoost)
                {
                    float rampSeconds = Mathf.Max(0.05f, Plugin._CrouchRunRampUpSeconds?.Value ?? 0.8f);
                    _rampProgress = Mathf.Min(1f, _rampProgress + Time.deltaTime / rampSeconds);
                }
                else
                {
                    _rampProgress = 0f; // perda é imediata (critério de aceite já existente da spec funcional)
                }

                // ref: §1.6 — só chama o Animator na transição, não em todo frame com o mesmo valor.
                if (wantsBoost != _animatorSprintOn)
                {
                    _animatorSprintOn = wantsBoost;
                    __instance.PlayerAnimatorEnableSprint(wantsBoost);
                }

                // ref: §1.7 — piso de postura: não deixa correr agachado DEMAIS, restaura ao soltar.
                if (wantsBoost && !_poseFloorEngaged)
                {
                    _poseFloorEngaged = true;
                    _poseLevelBeforeRun = __instance.PoseLevel; // captura ANTES de qualquer alteração
                }
                else if (!wantsBoost && _poseFloorEngaged)
                {
                    _poseFloorEngaged = false;
                    // ref: PA-02-01 — force:false = transição suave (herda POSE_CHANGING_SPEED / item 005),
                    // não um "pop" instantâneo. No-op se a pose já for igual.
                    __instance.SetPoseLevel(_poseLevelBeforeRun, force: false);
                }

                if (wantsBoost)
                {
                    float threshold = Plugin._CrouchRunPoseFloorThreshold?.Value ?? 0.3f;
                    if (threshold > 0f && __instance.PoseLevel < threshold)
                    {
                        float target = Mathf.Max(threshold, Plugin._CrouchRunPoseFloorTarget?.Value ?? 0.5f);
                        // ref: PA-02-01 — force:false (ver justificativa acima). Se não houver espaço,
                        // SetPoseLevel retorna false e nada muda (CanStandAt cuida disso, com qualquer force).
                        __instance.SetPoseLevel(target, force: false);
                    }
                }
            }

            if (_rampProgress <= 0f) return;
            float targetMultiplier = Plugin._CrouchRunSpeedMultiplier?.Value ?? 1f;
            __result *= Mathf.Lerp(1f, targetMultiplier, _rampProgress);
        }
    }
}
```

```csharp
// modded/Patches/ProneRunPatch.cs
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace CameraRotationMod.Patches
{
    // Só observa a tecla de sprint segurada em prone — o jogo já NÃO levanta o personagem aqui
    // (ver Estratégia §1.2). ref: Assembly-CSharp/ProneMoveStateClass.cs:83-86
    public class ProneRunObservePatch : ModulePatch
    {
        // ref: PA-01-03 — mesmo cuidado do CrouchRunEnableSprintPatch.SprintKeyHeld: só marca a
        // INTENÇÃO (tecla pressionada). O boost real é decidido a cada frame em ProneRunMaxSpeedPatch.
        public static bool SprintKeyHeld;

        // ref: PA-01-02 — chamado no reset de raid do mod.
        public static void ResetState() => SprintKeyHeld = false;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ProneMoveStateClass), nameof(ProneMoveStateClass.EnableSprint));
        }

        [PatchPostfix]
        private static void Postfix(ProneMoveStateClass __instance, MovementContext ___MovementContext, bool enabled)
        {
            // ref: PA-01-01 — só o jogador local.
            Player player = Traverse.Create(___MovementContext).Field<Player>("_player").Value;
            if (player == null || !player.IsYourPlayer) return;

            SprintKeyHeld = enabled;
        }
    }

    // Boost condicional de MaxSpeed — recalcula TODO frame (mesmo motivo do CrouchRunMaxSpeedPatch):
    // segurar a tecla de sprint parado (ex.: prender a respiração em prone durante ADS) nunca deve
    // aplicar boost nem, futuramente, a sobretaxa de stamina. Ganho gradual, perda imediata — mesmo
    // mecanismo de rampa do CrouchRunMaxSpeedPatch (ref: §1.5).
    public class ProneRunMaxSpeedPatch : ModulePatch
    {
        private const float MovementEpsilonSqr = 0.0001f;

        private static float _rampProgress; // 0..1
        private static int _lastFrame = -1;

        // ref: PA-01-02 — reset de raid.
        public static void ResetState() { _rampProgress = 0f; _lastFrame = -1; }

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(MovementContext), nameof(MovementContext.MaxSpeed));
        }

        [PatchPostfix]
        private static void Postfix(MovementContext __instance, ref float __result)
        {
            // ref: PA-01-01 — só o jogador local.
            Player player = Traverse.Create(__instance).Field<Player>("_player").Value;
            if (player == null || !player.IsYourPlayer) return;

            bool featureOn = Plugin._EnableProneRun?.Value ?? false;
            bool wantsBoost = featureOn
                && __instance.IsInPronePose
                && ProneRunObservePatch.SprintKeyHeld
                // ref: PA-01-03 — reconfirma movimento real AGORA, não o que era verdade quando a tecla foi apertada.
                && __instance.MovementDirection.sqrMagnitude > MovementEpsilonSqr;

            if (Time.frameCount != _lastFrame)
            {
                _lastFrame = Time.frameCount;
                if (wantsBoost)
                {
                    float rampSeconds = Mathf.Max(0.05f, Plugin._ProneRunRampUpSeconds?.Value ?? 0.8f);
                    _rampProgress = Mathf.Min(1f, _rampProgress + Time.deltaTime / rampSeconds);
                }
                else
                {
                    _rampProgress = 0f; // perda é imediata
                }
            }

            if (_rampProgress <= 0f) return;
            float targetMultiplier = Plugin._ProneRunSpeedMultiplier?.Value ?? 1f;
            __result *= Mathf.Lerp(1f, targetMultiplier, _rampProgress);
        }
    }
}
```

> **Sobretaxa de stamina (ambas as posturas):** registrar via `physical.Stamina.AddConsumption(customConsumption)` ([GClass774.cs:281](../../../../references/eft-decompiled/Assembly-CSharp/GClass774.cs#L281)) — mas **⚠️ ref: PA-01-03 — o gatilho de registrar/remover essa consumption tem que ser a MESMA condição recalculada por frame usada em `CrouchRunMaxSpeedPatch`/`ProneRunMaxSpeedPatch` (feature ligada + pose certa + `SprintKeyHeld` + `MovementDirection.sqrMagnitude` acima do épsilon), nunca só `SprintKeyHeld` sozinho** — senão a sobretaxa dreна stamina parado, exatamente o bug que o usuário descreveu (segurar a tecla de sprint parado pra prender a respiração). Guardar o `Action` de remoção retornado por `AddConsumption` para chamar assim que a condição completa deixar de valer (checar isso também todo frame, não só no `EnableSprint`). **Não** há stub completo aqui porque falta confirmar, em código real (não só leitura), o construtor completo de `PlayerPhysicalClass.GClass773` (campos `PrimaryTarget`/`Requires`/`StartThreshold` — os vistos em `Consumptions[EConsumptionType.Jump] = new GClass773(...) { ... }` em outros pontos do mesmo arquivo, não lidos por inteiro nesta investigação). **TODO confirmar no `/code-mod`:** ler o inicializador completo de pelo menos um `GClass773` existente (ex.: `EConsumptionType.Jump`, `PlayerPhysicalClass.cs:589`) para replicar os campos obrigatórios corretamente antes de escrever o stub final desta parte.

## 6. Fluxo de dados

```
[A] Jogador segura a tecla de correr nativa, agachado, andando para frente
        │
[B] RunStateClass.EnableSprint (RunStateClass.cs:401) seria chamado pelo input nativo
        │  ── Prefix do mod intercepta: replica EnableSprint(true) SEM SetPoseLevel(1f) ──
        │     (CrouchRunEnableSprintPatch.Prefix → SprintKeyHeld = true, só a INTENÇÃO)
        │
[C] MovementContext.MaxSpeed (MovementContext.cs:910) é lido every-frame por
    UpdateCharacterControllerSpeedLimit (MovementContext.cs:4173-4182)
        │  ── Postfix do mod recomputa wantsBoost = SprintKeyHeld + pose certa + movimento
        │     REAL nesse frame (ref: PA-01-03) — nunca confia só na intenção de [B] ──
        │     (CrouchRunMaxSpeedPatch.Postfix)
        │  ── Se wantsBoost mudou de estado (edge-detect), liga/desliga a animação de
        │     sprint (§1.6) via PlayerAnimatorEnableSprint ──
        │  ── Avança/zera o progresso de rampa [0..1] (§1.5): sobe com Time.deltaTime
        │     enquanto wantsBoost, cai a 0 na hora quando wantsBoost vira falso ──
        │  ── Multiplica __result por Lerp(1, alvoConfigurado, progresso) ──
        │
[D] SetCharacterMovementSpeed(RelativeSpeed * MaxSpeed) aplica a velocidade boostada
    (já suavizada pela rampa), mantendo a pose agachada (nunca tocada pelo Prefix acima)
        │
[E] Enquanto wantsBoost, o mod registra uma sobretaxa de stamina via Stamina.AddConsumption
    (GClass774.cs:281) — removida assim que wantsBoost (recomputado a cada frame) vira falso
```

Fluxo análogo para prone, trocando [B] por `Player.ToggleSprint()`/`Player.EnableSprint(bool)` (fix 2026-09-09, §1.8 — nível acima da hierarquia de sub-estados, nunca `ProneMoveStateClass.EnableSprint` diretamente), sem o passo de animação do §1.6 (prone não usa `PlayerAnimatorEnableSprint`), e a condição em [C] usando `IsInPronePose && ProneRunSprintIntent.Held` em vez da checagem de pose agachada.

## 7. Riscos e dependências

- **Decisão pendente da spec funcional (clamp do teto de velocidade):** a spec funcional (`<!-- review -->`, critério de aceite #3) pede que o teto nunca alcance o sprint em pé. Proposta desta spec técnica: em vez de travar a faixa do F12, aplicar um clamp EM RUNTIME dentro dos dois Postfix de `MaxSpeed` acima — ex.: `__result = Mathf.Min(__result, nativeSprintSpeedDeVoltaEmPe * 0.95f)` — precisa localizar a fonte de "velocidade de sprint em pé" acessível no mesmo frame (candidato: `MovementContext.SprintSpeed`, já usado por `MovementContextSprintSpeedPatch` existente no mod — **TODO confirmar** que dá pra ler esse valor de dentro do Postfix de `MaxSpeed` sem recursão). Isso é mais robusto que só limitar a faixa do slider, porque a velocidade de sprint em pé já é ela mesma configurável por outro slider do mod (`_SprintSpeedMultiplier`) — um teto travado no slider do crouch-run ficaria errado se o usuário mudar o multiplicador de sprint depois.
- **Ordem dos Postfix na mesma property:** `MovementContext.MaxSpeed` já tem um Postfix do mod (`MovementContextSpeedPatch`, `_WalkSpeedMultiplier`). Harmony permite múltiplos Postfix na mesma property; a ORDEM entre eles não deve importar aqui, porque todos só multiplicam `__result` (comutativo) — mas registrar os `SafeEnable(...)` dos dois novos patches na mesma vizinhança do `Plugin.cs:Awake()` do `MovementContextSpeedPatch` existente, para achar fácil no futuro.
- **`ResetCanUsePropState`/interações de interação:** fora de escopo desta spec — nenhuma referência encontrada de `EnableSprint` interagindo com looting/interação; não investigado a fundo, mas nenhum sinal de conflito nas leituras feitas.
- **Mount/ADS:** a spec funcional pede que a feature siga a mesma regra de outras transições do mod nesses estados. **TODO confirmar no `/code-mod`:** localizar como o mod hoje decide "está montado"/"está mirando" (`IsAiming`, `IsMountedState` — já usados pelos patches de rotação do item 021) e replicar a mesma guarda nos dois Prefixes/Postfixes deste item (não investigado agora — fora do escopo desta rodada de pesquisa, que focou no mecanismo de pose/velocidade/stamina).
- **Fika/multiplayer:** a mudança de velocidade acontece inteiramente dentro de `MovementContext`/`CharacterController`, que já é o caminho nativo de sincronização de movimento entre peers — não é uma sobreposição visual exclusiva do jogador local (diferente do sistema de rotação de arma do item 021, que É exclusivo do `IsYourPlayer`). Por isso, a expectativa (a confirmar in-game com outro jogador Fika) é que o peer observado veja a velocidade correta sem nenhum pacote novo do mod. **TODO confirmar em raid com 2 jogadores** antes de fechar o critério de aceite Fika da spec funcional.

## 8. Checklist de implementação

- [x] Adicionar os 10 `ConfigEntry` (§3) em `Plugin.cs`, nova seção `Crouch & Prone Sprint`.
- [x] Criar `CrouchRunEnableSprintPatch` (Prefix condicional em `RunStateClass.EnableSprint`) e `CrouchRunMaxSpeedPatch` (Postfix condicional em `MovementContext.MaxSpeed`).
- [x] ~~Criar `ProneRunObservePatch` (Postfix em `ProneMoveStateClass.EnableSprint`)~~ **substituído em 2026-09-09** por `PlayerToggleSprintPatch`/`PlayerEnableSprintPatch` (§1.8 — o alvo original não capturava a intenção com o jogador parado) + `ProneRunMaxSpeedPatch` (Postfix condicional em `MovementContext.MaxSpeed`, mantido).
- [x] ~~Sobretaxa de stamina no crouch-run~~ **removida em 2026-09-09** — confirmado in-game que o dreno nativo de sprint já se aplica sozinho ao agachado (o Prefix chama `movementContext.EnableSprint()`). Sobretaxa do **prone** mantida via `Stamina.AddConsumption`/ação de retorno (`PrimaryTarget = Base`, `Delta` lendo o F12 ao vivo) — necessária porque `ProneMoveStateClass.EnableSprint` nunca aciona o dreno nativo.
- [x] ~~Rampa de velocidade (ganho gradual)~~ **removida em 2026-09-09** a pedido do usuário — ganho e perda agora são ambos instantâneos nas duas posturas.
- [ ] **Ainda NÃO implementado** — clamp do teto de velocidade contra `MovementContext.SprintSpeed` (ver §7). Hoje o único limite é a faixa do slider do F12 (1.0–2.0), não um clamp em runtime.
- [ ] **Ainda NÃO implementado** — guarda de ADS/mount (ver §7, TODO original). Os patches deste item não checam `IsAiming`/`IsMountedState`.
- [ ] **Ainda NÃO implementado** — mesmo bug do prone (fix §1.8), classe de risco equivalente pro crouch: segurar sprint agachado PARADO e só depois andar pode não ativar o boost de velocidade (o latch nativo `Bool_1` resolve a stamina/pose sozinho, mas não passa pelo nosso Prefix). Não reportado como quebrado pelo usuário — só documentado, mesmo padrão de fix disponível (`Player.ToggleSprint`/`Player.EnableSprint`) se precisar.
- [x] Conectar os `ResetState()` (`CrouchRunEnableSprintPatch`, `CrouchRunMaxSpeedPatch`, `ProneRunSprintIntent`, `ProneRunMaxSpeedPatch`) ao hook de reset de raid já existente no mod — feito em `StanceManager.ResetState()`.
- [x] **Validar visualmente a animação de sprint forçada no crouch-run (§1.6):** **validado in-game pelo usuário (2026-09-09) — crouch-run funcionou.**
- [ ] **Validar o piso de postura (§1.7):** correr agachado bem baixo (abaixo do threshold) deve subir a postura durante a corrida e voltar exatamente pro nível de antes ao soltar — inclusive testando: (a) ajustar a postura manualmente DURANTE a corrida acelerada e confirmar que soltar restaura o nível de ANTES de começar (não o nível ajustado no meio); (b) correr num espaço com teto baixo que impede subir nem até o "Pose Floor Target" — confirmar que a corrida continua funcionando na postura mais baixa (sem erro), só sem o ajuste de postura.
- [ ] **Validar especificamente o cenário que motivou a review 01:** ficar parado mirando (ADS) e segurar a tecla de sprint (simulando "prender a respiração" com o mesmo bind) — confirmar que NÃO há boost de velocidade nem dreno de stamina extra enquanto `MovementDirection` estiver em zero, mesmo segurando a tecla por vários segundos.
- [ ] Validar o fluxo do PA-01-04 para o CROUCH (ver item novo acima — ainda não corrigido, só documentado).
- [x] **Re-validar prone-run depois do fix §1.8:** segurar sprint já em movimento, E segurar sprint parado e só depois andar (o cenário que estava quebrado) — pendente de nova validação do usuário.
- [x] Compilar — `dotnet build`, 0 erros/0 warnings (build inicial e após o fix de 2026-09-09).
- [ ] Validar in-game: prone-run após o fix, os dois com Stance customizada ativa, dreno de stamina do prone com skill alta vs. baixa, Fika com 2 jogadores, espaço de teto baixo (crouch-run não deve permitir "furar" a checagem nativa — como o Prefix só evita o `SetPoseLevel(1f)`, nunca CHAMA `SetPoseLevel` para baixo, o teto baixo nem chega a ser testado por este código — confirmar que isso é seguro mesmo assim, já que a pose não muda).

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | ✅ | `ResetState()` adicionado nas 4 classes de patch (`CrouchRunEnableSprintPatch`, `CrouchRunMaxSpeedPatch`, `ProneRunObservePatch`, `ProneRunMaxSpeedPatch` — as duas últimas adicionadas junto com a rampa de velocidade, §1.5) — falta só conectar ao hook de reset já existente no mod, item explícito no checklist §8. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | Guarda `IsYourPlayer` (via `Traverse.Create(...).Field<Player>("_player").Value`, mesmo padrão de `ApplyComplexRotationPatch.cs:207-209`) adicionada nos 4 Prefix/Postfix (ver stubs §5, ref PA-01-01). |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | ✅ | Ver §2 "Auditoria de overrides" — 3 overrides de `EnableSprint` enumerados, 2 patcheados deliberadamente, 1 (`SprintStateClass`) descartado com justificativa. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Stamina: usa `Stamina.AddConsumption`/`GClass773` — a mesma API pública que o jogo usa para o próprio dreno de sprint (§1.4), em vez de subtrair `Stamina.Current` na mão. Velocidade: usa o mesmo padrão de Postfix em `MaxSpeed` que o mod já usa (`MovementContextSpeedPatch`), não escreve campos privados. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | Mesma resolução do check 1 (ref PA-01-02). |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | 🟡 parcial | Defaults e semântica dos 6 `ConfigEntry` estão explícitos (§3). Falta fechar a decisão de clamp do teto (marcada `<!-- review -->`, herdada da spec funcional) antes de a faixa do F12 ser considerada final. |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` — AP-07 | N/A | O Prefix de `CrouchRunEnableSprintPatch` chama `MovementContext.EnableSprint` (método diferente de `RunStateClass.EnableSprint`, o alvo patcheado) — não há recursão no mesmo método. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca (arma/operação/tela) — AP-08 | ✅ | Resolvido via PA-01-03: `SprintKeyHeld` é só a intenção (evento down/up); a condição de boost/sobretaxa é **recalculada a cada frame** direto do `__instance.MovementDirection` dentro do Postfix de `MaxSpeed` — nunca confia num valor cacheado do momento em que a tecla foi apertada. Isso cobre o corner case "segurar sprint parado" (relatado pelo usuário) e o corner case "trocar de postura/parar de mover no meio da aceleração" da spec funcional, ambos pelo mesmo mecanismo. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump; "não existe" nunca de grep vazio — AP-09 | ✅ | Todas as refs desta spec foram lidas diretamente no dump local (`RunStateClass.cs`, `ProneMoveStateClass.cs`, `MovementContext.cs`, `PlayerPhysicalClass.cs`, `GClass774.cs`, `GClass848-1.cs`), incluindo `PlayerAnimatorEnableSprint` (`MovementContext.cs:3832-3835`) e seu gate nativo por pose (`RunStateClass.cs:252`). Onde a investigação não fechou (ponto exato do `Stamina.Multiplier`, construtor completo de `GClass773`, guarda de ADS/mount), marcado explicitamente como TODO — não inventado. |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Feature não usa skill do EFT como alavanca de gameplay (a skill de Força só é lida indiretamente via `MaxSpeed`/`Stamina.Multiplier`, não é o mod quem a aciona). |
| 11 | Pacote FIKA próprio — AP-11 | N/A | Mod não declara `INetSerializable`; a sincronização esperada é via o caminho nativo de movimento (ver §7). |

**Checks 1, 2, 5 e 8 resolvidos na review 01** (ver [018-rastejar-rapido-03-spec-tech-review-01.md](018-rastejar-rapido-03-spec-tech-review-01.md)) — guarda `IsYourPlayer`, reset de raid e recomputação por frame do movimento real já aplicados aos stubs acima. Resta: check 6 (decisão de clamp do teto, `<!-- review -->` pendente do usuário) e o ponto 🟡 PA-01-04 (confirmar in-game o fluxo "segura sprint parado, depois anda") antes de considerar zerado por completo — nenhum dos dois é 🔴, então **pode iniciar `/code-mod`** com esses dois como acompanhamento.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-09 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-09 | Review 01: 3 bloqueadores 🔴 resolvidos (AP-01, AP-02, e a separação `SprintKeyHeld` vs. condição de boost recalculada por frame, motivada pelo usuário) + 1 ponto 🟡 aberto (PA-01-04). Stubs §5 e checklist §8 atualizados. |
| 2026-09-09 | Pedido do usuário: ganho de velocidade gradual, não instantâneo (perda continua imediata). Adicionado mecanismo de rampa (§1.5), 2 novas `ConfigEntry` de tempo de rampa (§3), stubs de `CrouchRunMaxSpeedPatch`/`ProneRunMaxSpeedPatch` reescritos (§5), novo critério de aceite espelhado na spec funcional. |
| 2026-09-09 | Pedido do usuário: crouch-run deve acionar a mesma animação de "baixar arma / balançar braços" do sprint em pé. Confirmado que o gate nativo dessa animação (`RunStateClass.cs:252`) exige `PoseLevel > 0.9f` e nunca dispara sozinho com pose agachada — adicionada §1.6, e `CrouchRunMaxSpeedPatch` agora liga/desliga `PlayerAnimatorEnableSprint` via edge-detect no mesmo bloco por-frame da rampa. Exclusivo do crouch-run (prone não usa esse flag nativamente). |
| 2026-09-09 | Pedido do usuário: piso de postura durante o crouch-run — se a postura estiver muito baixa (abaixo de um threshold configurável, default 0.3), sobe pra um valor mínimo (default 0.5) enquanto corre, e restaura exatamente a postura de antes ao parar. Adicionada §1.7, 2 novas `ConfigEntry`, stub de `CrouchRunMaxSpeedPatch` ampliado com captura/restauração via `SetPoseLevel`. Exclusivo do crouch-run (prone tem altura de collider fixa, `PoseLevel` não se aplica da mesma forma). |
| 2026-09-09 | Review 02: 2 pontos resolvidos. PA-02-01 (🟠): trocado `force: true` por `force: false` no piso de postura (§1.7) — evita um "pop" visual e herda a suavização já configurável de `POSE_CHANGING_SPEED` (item 005). PA-02-02 (🟡): documentada (não refatorada) a fragilidade de ter side-effects no Postfix de uma property getter — decisão de deixar para o `/code-review` reavaliar com código real. |
| 2026-09-09 | Build concluído via `/code-mod`. Compilado com sucesso (0 erros/0 warnings). 2 itens do checklist ficaram deliberadamente fora desta rodada (clamp contra `SprintSpeed`, guarda de ADS/mount) — ver checklist §8 e `05-asbuild.md`. |
| 2026-09-09 | **Fix pós-validação (§1.8).** Usuário testou: crouch-run OK, prone-run sem efeito. Causa raiz: `ProneMoveStateClass.EnableSprint` só existe enquanto já em movimento; `ProneIdleStateClass` (prone parado) não sobrescreve `EnableSprint`, e não existe latch nativo pra prone (ao contrário do `Bool_1` de `RunStateClass`). Fix: nova fonte de sinal em `Player.ToggleSprint()`/`Player.EnableSprint(bool)` (nível acima da hierarquia de sub-estados). Removida sobretaxa de stamina do crouch (vanilla já cobra, confirmado) e a rampa de velocidade das duas posturas (a pedido do usuário). Documentado (não corrigido) o mesmo tipo de bug como risco latente pro crouch. |
