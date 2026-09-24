# 008 — Fix 02 · `Player.OnControllerColliderHit` nunca é chamado pelo jogo — reescrito pra checagem periódica por proximidade

**Mod:** VisceralCombat
**Item raiz:** [008-atropelar-acorda-corpos-itens-01-spec.md](008-atropelar-acorda-corpos-itens-01-spec.md)
**Asbuild:** [008-atropelar-acorda-corpos-itens-05-asbuild.md](008-atropelar-acorda-corpos-itens-05-asbuild.md)
**Criado:** 2026-09-21
**Disparado por:** feedback in-raid do usuário — nenhuma reação de contato ocorria, nem em item nem em corpo, mesmo após o fix 01 (ForceMode)

## Contexto

Após o fix 01 (troca de `ForceMode.VelocityChange` por `ForceMode.Impulse`), o usuário reportou que "atropelar" continuava sem nenhum efeito — "meus pés não chutam objetos/itens". Diferente do item `007` (tiro em item, que já funcionava corretamente depois do fix 01), o item `008` continuava completamente inerte.

Investigação em 2 rounds:
1. Adicionado log condicional (só quando `hit.rigidbody != null`) — nenhuma linha apareceu em uma sessão de teste completa, mesmo com o usuário andando/parando sobre itens.
2. Build seguinte trocou pra log **incondicional** (antes de qualquer early-return, deduplicado por nome de collider) — **nenhuma linha apareceu, nem mesmo pro chão**, apesar do patch confirmado ativo no log (`Enabled patch PlayerContactPushPatch`) e da build correta carregada (`Loading [Visceral Combat 3.12.5]`).

## Causa raiz

`Player.OnControllerColliderHit` ([Player.cs:28849](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L28849)) é um método com a assinatura exata do callback "mágico" que a Unity invoca automaticamente **somente** quando o `GameObject` tem um componente `UnityEngine.CharacterController` real realizando `Move()`. Investigação em `CharacterControllerSpawner.cs` e `ApplicationConfigClass.cs` revelou que **nem o jogador local nem os bots usam esse componente nativo**:

```csharp
// ApplicationConfigClass.cs:76-96 — CharacterControllerSettings
public CharacterControllerSpawner.Mode ObservedPlayerMode = new CharacterControllerSpawner.Mode
{
    Type = CharacterControllerSpawner.ControllerType.Impostor, ...
};
public CharacterControllerSpawner.Mode ClientPlayerMode = new CharacterControllerSpawner.Mode
{
    Type = CharacterControllerSpawner.ControllerType.Simple, ...   // ← jogador local
};
public CharacterControllerSpawner.Mode BotPlayerMode = new CharacterControllerSpawner.Mode
{
    Type = CharacterControllerSpawner.ControllerType.Simple, ...   // ← bots
};
```

Só `ControllerType.Unity` cria um `UnityEngine.CharacterController` de verdade ([CharacterControllerSpawner.cs:169-184](../../../../references/eft-decompiled/Assembly-CSharp/CharacterControllerSpawner.cs#L169-L184)). `ClientPlayerMode`/`BotPlayerMode` usam `ControllerType.Simple`, que cria um `SimpleCharacterController` ([CharacterControllerSpawner.cs:203-215](../../../../references/eft-decompiled/Assembly-CSharp/CharacterControllerSpawner.cs#L203-L215)) — uma classe com colisão **100% customizada** (`Physics.OverlapSphereNonAlloc`/`Physics.CapsuleCast` internos, confirmado em `SimpleCharacterController.cs`), que nunca aciona o evento `OnControllerColliderHit` da Unity. Conclusão: `Player.OnControllerColliderHit` é, na prática, **código morto** neste jogo — nunca é chamado pelo motor, pra ninguém, em nenhuma circunstância normal. Isso **refuta a premissa central** da spec técnica original do item `008` (§1, "Novo Postfix Harmony em `Player.OnControllerColliderHit`"), que assumia esse callback seria confiável — não foi verificado contra o comportamento real do jogo antes do `/code-mod` original.

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/PlayerContactPushPatch.cs` | Reescrito por completo — deixou de ser um Harmony `ModulePatch` em `Player.OnControllerColliderHit` e virou uma classe estática com uma corrotina própria (`PeriodicCheckLoop`), iniciada 1x por raid. A cada `CheckIntervalSeconds` (0.2s, valor escolhido pelo usuário), roda `Physics.OverlapSphereNonAlloc` num raio pequeno (0.6m) em volta de `MainPlayer.Position`, aplicando o mesmo empurrão de antes (mass-scaled `ForceMode.Impulse`) em qualquer item/corpo encontrado — mesma técnica de detecção já usada e comprovada em `GrenadeItemsPatch.cs`, independente de qual sistema de movimento o jogo usa. |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/GameStartedPatch.cs` | Adicionado `PlayerContactPushPatch.StartPeriodicCheck();` junto ao `ClearContactPushCooldowns()` já existente — inicia a corrotina a cada raid nova. |
| `modded/VisceralCombat/VisceralCombat/VisceralEntry.cs` | Removido `((ModulePatch)new PlayerContactPushPatch()).Enable();` de `Awake()` — não é mais um `ModulePatch`. |

## Checklist de validação (obrigatório antes de marcar o fix como entregue)

- [x] Compila via `/compile-mod` sem erros (build 3.12.6)
- [ ] **In-raid:** encostar leve vs. correr sobre corpo/item já acomodado (sem tiro prévio) produz empurrão perceptível e proporcional — pendente de novo teste do usuário
- [ ] **Fika/multiplayer:** sem regressão com outros players — ou `N/A: <razão>`
- [ ] **raid1 → exit → raid2:** corrotina reinicia limpa a cada raid, sem acumular instâncias — confirmar visualmente que não há múltiplas corrotinas rodando (ex.: empurrões duplicados)
- [ ] **alt-F4 / morte / MIA:** teardown idempotente, sem exceção no LogOutput.log
- [ ] Memória do mod atualizada (`/update-memory`) com a lição do fix (evento `OnControllerColliderHit` não confiável neste jogo)

## Histórico

| Data | Evento |
|---|---|
| 2026-09-21 | Fix criado e aplicado — build 3.12.6 compilada, 0 erros. Aguardando validação em raid do usuário. |
