# 090 — Velocidade da animação de cura não replica pros outros jogadores (Fika) · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** [090-velocidade-cura-nao-replica-01-spec.md](090-velocidade-cura-nao-replica-01-spec.md)
**Criado:** 2026-09-08
**Depende de:** [`FIKA` item 005](../../../FIKA/backlog/005-hook-velocidade-cura-observada/005-hook-velocidade-cura-observada-01-spec.md) — hook genérico `ObservedMedsSpeedHook` no fork do Fika. Este item só entrega depois (ou junto) do 005.

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/) e, para o lado coop, [`mods/FIKA/modded/Fika-Plugin/Fika.Core/`](../../../../FIKA/modded/Fika-Plugin/Fika.Core/) (nosso fork, o que é realmente compilado/instalado — conferido idêntico ao vendorizado em `references/fika-plugin/` para os arquivos citados, diff vazio em 2026-09-08). Toda referência a código cita `arquivo.cs:linha`.

## 1. Estratégia

**Achado central:** não é preciso nenhum pacote de rede novo. O projeto já opera sob a premissa "F12 é distribuído IDÊNTICO para todos os jogadores" (documentada em [`ClassIdentities.cs:118-124`](../../../../mods/CustomClasses/modded/Client/ClassIdentities.cs#L118-L124), mesma lógica usada pelos perks de som dos itens 065/066). Qualquer cliente observador já pode calcular o MESMO fator de velocidade que o operador calcula, desde que saiba (a) a classe do operador — `ClassIdentities.ClassIdOf(EFT.Player)`, mapa nickname→classe já existente (057) — e (b) se o item em uso é cirurgia — `MedicTiming.IsSurgery(Item)`, [`ClassMedicPatches.cs:86-91`](../../../../mods/CustomClasses/modded/Client/Patches/ClassMedicPatches.cs#L86-L91), que não depende de nada exclusivo do dono (o item replica nativamente pelo Fika).

**O que falta:** um ponto de aplicação no lado do OBSERVADOR. O vanilla só ajusta a velocidade (`FirearmsAnimator.SetUseTimeMultiplier`) dentro de `Player.MedsController.ObservedMedsControllerClass.method_5()`, que **retorna cedo** quando `_player.ActiveHealthController == null` ([`Player.cs:19549-19551`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L19549-L19551)) — verdade em QUALQUER cliente que não seja o dono do personagem. Quem substitui essa lógica pra peers é o próprio FIKA: [`ObservedMedsController.cs`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/HandsControllers/ObservedMedsController.cs), cuja classe aninhada `ObservedMedsOperation` recalcula a velocidade usando só a skill Cirurgia nativa ([`ObservedMedsController.cs:182-183`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/HandsControllers/ObservedMedsController.cs#L182-L183)) — sem qualquer conhecimento do CustomClasses.

**Estratégia (Opção B — decisão do usuário, 2026-09-08):** como `mods/FIKA/modded` é um fork NOSSO (não read-only), o item irmão [`FIKA/005`](../../../FIKA/backlog/005-hook-velocidade-cura-observada/) expõe um hook genérico e público (`ObservedMedsSpeedHook.ExtraSpeedMultiplier`, um `Func<Player, Item, float>?` estático) direto no Fika — sem NENHUM conhecimento de CustomClasses ali. Este item (090) só precisa **assinar** esse hook. Nada de reflection em classe privada aninhada, nada de Harmony contra um alvo do Fika.

**Alternativa descartada (Opção A, mantida documentada em código — ver §5): reflection pura contra a classe privada `ObservedMedsOperation` do Fika, sem tocar no fork.** Foi a estratégia original desta spec antes da decisão do usuário. Continua sendo o caminho a seguir **se um dia este mod rodar contra um Fika vanilla/não-forkado** (o hook do item 005 só existe NESTE fork) — por isso a fórmula fica comentada no código como referência, não implementada.

## 2. Pontos de patch

| Alvo | Tipo | Motivo |
|---|---|---|
| `ObservedMedsSpeedHook.ExtraSpeedMultiplier` (`Fika.Core.Main.ObservedClasses.HandsControllers`, item FIKA/005) | Atribuição de delegate (não é Harmony) | CustomClasses assina esse `Func` uma vez no `Awake`, resolvendo o TIPO por reflection (`AccessTools.TypeByName`) — só o tipo, não membros privados — pra não precisar de referência de build ao `Fika.Core.dll`. |
| [`Player.cs:19542-19570`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L19542-L19570) — `MedsController.ObservedMedsControllerClass.method_5()` (referência, NÃO um patch novo) | — | Já coberto pelo `MedAnimSpeedPatch`/`MedUseTimePatch` existentes (item 072) para o caminho LOCAL — citado só pra deixar claro que local (vanilla) e peer-observado (Fika) são mecanismos DIFERENTES, sem sobreposição. |

## 3. Novas propriedades F12 (BepInEx)

Nenhuma. Reaproveita `Rapid Care`/`Swift Surgeon` (Enabled + valor) já existentes no F12 (`PerksConfig`, seção `2 · Combat Medic`).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Client/Patches/ClassMedicReplicationHook.cs` | CRIAR | Assina `ObservedMedsSpeedHook.ExtraSpeedMultiplier` (resolvido por reflection de TIPO) com o mesmo cálculo do 072/077; fórmula de fallback via reflection completa comentada (Opção A). |
| `modded/Client/Plugin.cs` | MODIFICAR | (a) Adicionar `[BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]` no topo da classe (PA-01-01 — garante que, SE o Fika estiver presente, seu assembly carrega e roda antes do `Awake()` do CustomClasses, tornando `AccessTools.TypeByName` determinístico); (b) chamar a assinatura do hook num `try/catch` (molde do `ExecutionSpeedCapPatch`), perto do bloco do item 072. |

## 5. Stubs de código

> `MedicTiming`, `ClassIdentities` e `EClassId` já existem no mod (`ClassMedicPatches.cs`, `ClassIdentities.cs`) — reaproveitados, não recriados.

```csharp
// modded/Client/Patches/ClassMedicReplicationHook.cs
using System;
using System.Reflection;
using EFT;
using EFT.InventoryLogic; // Item

namespace CustomClasses.Client;

/// <summary>
///     Item 090 — assina o hook genérico do FIKA (<c>ObservedMedsSpeedHook.ExtraSpeedMultiplier</c>, item
///     FIKA/005) pra replicar, nos clientes que OBSERVAM um Médico de Combate (auto-cura OU cura de aliado via
///     TRL-ImmersiveCombatMedicine), o mesmo fator de velocidade que os itens 072/077 já aplicam no cliente do
///     PRÓPRIO operador. Sem protocolo novo: a classe do operador já é resolvida pelo mapa nickname→classe
///     existente (<see cref="ClassIdentities.ClassIdOf"/>, item 057), e o VALOR do perk vem do F12 de quem
///     roda isto — mesma premissa "config idêntica p/ todos" já usada pelos itens 065/066.
/// </summary>
internal static class ClassMedicReplicationHook
{
    // Resolvido só o TIPO por reflection (não membros privados) — evita referência de build ao Fika.Core.dll,
    // então este arquivo compila e funciona igual em SP (Fika ausente: Resolve() vira no-op, fail-open).
    private static readonly Type? HookType =
        AccessTools.TypeByName("Fika.Core.Main.ObservedClasses.HandsControllers.ObservedMedsSpeedHook");

    private static readonly FieldInfo? ExtraSpeedMultiplierField =
        HookType != null ? AccessTools.Field(HookType, "ExtraSpeedMultiplier") : null;

    /// <summary>Chamado 1x no Awake do Plugin (dentro de try/catch, molde do ExecutionSpeedCapPatch).
    /// Depende de <c>[BepInDependency("com.fika.core", SoftDependency)]</c> em Plugin.cs (PA-01-01) —
    /// sem isso, a ordem de carregamento entre CustomClasses e Fika não é garantida, e este método pode
    /// nunca resolver o hook mesmo com o Fika instalado.</summary>
    internal static void Register()
    {
        if (ExtraSpeedMultiplierField == null)
        {
            // PA-01-02: aviso (não erro) — cenário esperado em SP (Fika ausente) OU um Fika/fork sem o
            // hook do item 005. Diagnosticável sem poluir o log com "erro" quando é só ausência normal.
            Plugin.Log?.LogWarning("[CustomClasses] (090) ObservedMedsSpeedHook não encontrado — Fika ausente ou fork sem o hook do item FIKA/005 (sem efeito, fail-open).");
            return;
        }

        Func<Player, Item, float> del = ResolveFactor;
        ExtraSpeedMultiplierField.SetValue(null, del);
        Plugin.Log?.LogInfo("[CustomClasses] (090) hook de velocidade de cura (Fika) assinado com sucesso.");   // PA-01-02
    }

    /// <summary>Assinatura EXATA exigida pelo hook (Fika.Core): Func&lt;Player, Item, float&gt;.
    /// Retorna 1f (sem efeito) se o dono não for Médico de Combate ou o perk estiver off.</summary>
    private static float ResolveFactor(Player fikaPlayer, Item item)
    {
        try
        {
            // ref: ClassIdentities.cs:143-151 — já barra IsAI (bot) e resolve local-vs-peer sozinho.
            if (ClassIdentities.ClassIdOf(fikaPlayer) != EClassId.CombatMedic)
            {
                return 1f;
            }

            var isSurgery = MedicTiming.IsSurgery(item);      // ref: ClassMedicPatches.cs:86-91
            var factor = MedicTiming.FactorFor(isSurgery);    // ref: ClassMedicPatches.cs:114-131 (F12 local)
            return factor > 0f ? 1f / factor : 1f;            // extra = inverso do fator (0.7 no efeito ⇒ ÷0.7 na anim)
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (090) ResolveFactor falhou: {ex.Message}");
            return 1f;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════════════════════
    // FALLBACK (Opção A, NÃO implementado) — fórmula de referência caso um dia seja preciso rodar
    // contra um Fika VANILLA/não-forkado (sem o hook do item FIKA/005). Reflection direta contra a
    // classe privada aninhada `ObservedMedsController.ObservedMedsOperation`:
    //
    //   var outerType = AccessTools.TypeByName(
    //       "Fika.Core.Main.ObservedClasses.HandsControllers.ObservedMedsController");
    //   var innerType = AccessTools.Inner(outerType, "ObservedMedsOperation");
    //   var observedControllerField = AccessTools.Field(innerType, "_observedMedsController"); // ObservedMedsController.cs:129
    //   var fikaPlayerField = AccessTools.Field(outerType, "_fikaPlayer");                      // ObservedMedsController.cs:16
    //
    //   // 2 Harmony Postfix (ModulePatch), targets resolvidos via innerType:
    //   //   AccessTools.Method(innerType, "ObservedStart", new[] { typeof(Action) })
    //   //   AccessTools.Method(innerType, "HealthController_EffectRemovedEvent", new[] { typeof(IEffect) })
    //   // Cada Postfix: ler _observedMedsController (do __instance) → _fikaPlayer (do controller) →
    //   // ClassIdOf/IsSurgery/FactorFor (igual acima) → (controller as Player.AbstractHandsController)
    //   //   ?.FirearmsAnimator?.SetUseTimeMultiplier(valor). No 2º patch, reconstruir a fórmula nativa
    //   // do Fika ANTES de aplicar o fator: `(1f + fikaPlayer.Skills.SurgerySpeed.Value/100f) / factor`
    //   // (ObservedMedsController.cs:182-183 — não há getter pro multiplicador já aplicado).
    //
    // Ver histórico desta spec (git) pela versão completa desses 2 stubs, escrita antes da decisão
    // do usuário de usar o hook do FIKA/005 em vez de reflection.
    // ═══════════════════════════════════════════════════════════════════════════════════════════
}
```

```csharp
// modded/Client/Plugin.cs — atributo de classe (PA-01-01). Adicionar ao lado dos já existentes:
[BepInPlugin("customclasses.mdj.client", "CustomClasses", "0.16.9")]
[BepInDependency("com.SPT.core", "4.0.0")]
[BepInDependency("me.sol.sain", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]   // (090) garante ordem de carga p/ o hook do FIKA/005
public class Plugin : BaseUnityPlugin
```

```csharp
// modded/Client/Plugin.cs — trecho a adicionar perto do bloco do item 072 (ver linha ~257-259 atual)
try
{
    ClassMedicReplicationHook.Register();   // (090) assina o hook genérico do FIKA (item 005) — auto-cura + cura de aliado
}
catch (System.Exception ex)
{
    Plugin.Log?.LogWarning($"[CustomClasses] (090) hook de replicação de velocidade (Fika) não registrado: {ex.Message}");
}
```

## 6. Fluxo de dados

```
[A] Médico usa item médico (auto-cura OU redirect do ICM p/ aliado)
      → [B] Player.cs:19542 method_5 roda no cliente do MÉDICO (ActiveHealthController != null)
            → [C] MedUseTimePatch/MedAnimSpeedPatch (072, já existente) aceleram EFEITO + animação — só neste cliente
      → [D] Fika replica a operação p/ TODO OUTRO cliente via ObservedMedsController (Fika.Core, fork)
            → [E] ObservedStart / HealthController_EffectRemovedEvent (ObservedMedsController.cs:133/154)
                  calculam a velocidade nativa (skill Cirurgia) e chamam ObservedMedsSpeedHook.ResolveExtra
                  (item FIKA/005) — hook genérico, sem saber que o CustomClasses existe
            → [F] NOVO (090): ClassMedicReplicationHook.ResolveFactor (assinado no hook) resolve a classe do
                  operador (ClassIdentities.ClassIdOf, 057) + o fator do F12 local (MedicTiming.FactorFor,
                  "config idêntica p/ todos") e devolve o MESMO fator de [C]
      → [G] Todo observador (aliado curado, 3º jogador) vê a MESMA velocidade que o médico vê em [C]
```

## 7. Riscos e dependências

- **Depende do item FIKA/005 estar entregue primeiro (ou junto).** Sem o hook, `HookType`/`ExtraSpeedMultiplierField` ficam `null` e `Register()` é um no-op silencioso — o mod não quebra, mas o item 090 simplesmente não faz efeito (mesmo estado de hoje). Sequenciar: `/code-mod` do FIKA/005 antes (ou no mesmo lote) do `/code-mod` deste item.
- **(PA-01-01, resolvido nesta spec) Ordem de carregamento BepInEx:** `HookType`/`ExtraSpeedMultiplierField` são `static readonly`, avaliados na primeira vez que `Register()` toca a classe — `AccessTools.TypeByName` só encontra o tipo do Fika se o assembly `Fika.Core.dll` já estiver carregado no AppDomain naquele instante. Sem uma dependência declarada, o BepInEx **não garante** essa ordem entre dois plugins sem relação — `CustomClasses/modded/Client/Plugin.cs:13-15` já usa `[BepInDependency("me.sol.sain", SoftDependency)]` exatamente pra esse tipo de garantia; adicionar o equivalente pro Fika (`com.fika.core`, confirmado em `FikaPlugin.cs:41`) fecha o risco de o item 090 nunca funcionar por pura sorte de ordem de carregamento, mesmo com tudo instalado corretamente.
- **Acoplamento com um fork específico:** o hook só existe em `mods/FIKA/modded` — se o usuário trocar por um Fika vanilla/outra distribuição, o item 090 volta a ficar inerte (fail-open, sem crash). A fórmula de fallback por reflection pura (Opção A) fica **comentada no código** (§5) exatamente pra esse cenário — implementar quando/se isso acontecer.
- **`last-write-wins` do lado do FIKA:** se outro mod também assinar `ExtraSpeedMultiplier`, um sobrescreve o outro (limitação documentada no item FIKA/005). Não há hoje nenhum outro mod conhecido que use esse hook.
- **Nuance NÃO replicada (aceita, fora de escopo):** o `MedAnimSpeedPatch` local (item 072/CR-F4) corrige um bug de fábrica da BSG na cirurgia (`S/100` em vez de `S`) e soma `+0.2f` perto de vida cheia. O recálculo nativo do Fika pra peers só usa `S/100` (sem o `+0.2f`) — este item não replica essas duas nuances vanilla pros observadores; a MAGNITUDE do perk (0.5×/0.7×) é o que importa pro relato original, e a nuance da skill nativa é escopo "vanilla puro", já descartado na spec funcional.
- **Dependência de `ClassIdentities` (057) e `MedicTiming` (072):** nenhuma mudança nesses dois — só consumo. Mapa 057 frio/indisponível → `ClassIdOf` degrada pra `EClassId.None` (mesma degradação silenciosa já aceita pelos itens 065/066).
- **Compatibilidade com ICM:** nenhuma mudança necessária no `TRL-ImmersiveCombatMedicine` — o hook do Fika dispara pra QUALQUER motivo do médico estar usando o item (self ou redirect do ICM), cobrindo os dois cenários da spec funcional automaticamente.
- **Ordem de inicialização:** `Register()` deve rodar no `Awake` (mesmo estágio dos outros `.Enable()`); não depende de `MedsOperationScopePatch`/`MedUseTimePatch`/`MedAnimSpeedPatch` (072) nem é dependido por eles.

## 8. Checklist de implementação

- [x] Confirmar que o item `FIKA/005` está implementado (código-fonte em `mods/FIKA/modded/`, code review 01 fechada). **Ainda não buildado** — `/compile-mod` fica pendente por decisão do usuário (só quer alterações na pasta do mod, não instalar no jogo agora).
- [x] Adicionar `[BepInDependency("com.fika.core", SoftDependency)]` em `Plugin.cs` (PA-01-01).
- [x] Criar `modded/Client/Patches/ClassMedicReplicationHook.cs` com `Register()`/`ResolveFactor()` + o comentário de fallback (Opção A).
- [x] Chamar `ClassMedicReplicationHook.Register()` num bloco `try/catch` em `Plugin.cs`, perto do bloco do item 072.
- [ ] Validar em build/execução SEM Fika instalado (SP): `Register()` não lança; log mostra o aviso de "hook não encontrado" (PA-01-02), não um erro.
- [ ] Validar com um `Fika.Core.dll` ANTERIOR ao item FIKA/005 instalado (sem o hook): mesmo resultado do item acima — aviso, não erro, sem quebrar o boot (PA-01-03).
- [ ] Validar in-game (2 clientes Fika): Médico com Rapid Care se autocura → 2º jogador observa animação no ritmo acelerado; log mostra "hook... assinado com sucesso" (PA-01-02) na inicialização.
- [ ] Validar in-game: Médico com Swift Surgeon cura um ALIADO humano via ICM → o aliado curado E um 3º observador veem o gesto acelerado.
- [ ] Validar in-game (regressão): Médico sem o perk (classe diferente / perk OFF) → observadores veem o ritmo padrão, igual a antes do 090.
- [ ] Conferir que a velocidade vista pelo PRÓPRIO médico (já corrigida pelo 072/077) não mudou — este item só toca a réplica de PEER via hook, nunca o caminho local.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes | ✅ | Nenhum estado novo por raid — `Register()` roda 1× no Awake (registra o delegate estático do lado do Fika); nada por raid a limpar. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | O hook do Fika só é chamado dentro de `ObservedMedsOperation` (peers, nunca o `MainPlayer` local); `ClassIdOf` (`ClassIdentities.cs:143-151`) barra `IsAI` (bots) antes de resolver classe. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | N/A | Não é Harmony patch — é atribuição de um delegate a um campo público (`ObservedMedsSpeedHook.ExtraSpeedMultiplier`, item FIKA/005). Sem virtual dispatch envolvido deste lado. |
| 4 | Mudança de estado via API canônica; side-effects mapeados — AP-04 | ✅ | O hook, do lado do Fika, chama `FirearmsAnimator.SetUseTimeMultiplier` — API pública já usada pelo vanilla/Fika (ver spec técnica do FIKA/005 §5). |
| 5 | Estado entre raids | ✅ | Mesma razão do check 1 — `ResolveFactor` é *stateless*, recalcula tudo a cada chamada. |
| 6 | Semântica/defaults de ConfigEntry sem ambiguidade — AP-05 | N/A | Nenhuma `ConfigEntry` nova (§3). |
| 7 | Reentrância — AP-07 | ✅ | `ResolveFactor` não invoca de volta nada do Fika nem se re-invoca. |
| 8 | Estado stale em troca de contexto — AP-08 | ✅ | Sem cache de identidade de operação — `ResolveFactor` recebe `Player`/`Item` frescos a cada chamada do hook. |
| 9 | Patch-point reconfirmado no `.cs` real — AP-09 | ✅ | `Player.cs:19542-19570`, `ClassMedicPatches.cs:86-131`, `ClassIdentities.cs:118-151` (lado CustomClasses) + `ObservedMedsController.cs:127-194` no fork (`mods/FIKA/modded/`, diff vazio vs. referência) — todos lidos e citados. |
| 10 | Skill EFT inerte — AP-10 | N/A | Reaproveita o perk de classe (F12) já entregue e validado pelo item 072. |
| 11 | Pacote FIKA próprio — AP-11 | N/A | Nenhum pacote novo — hook IN-PROCESS (cada cliente calcula localmente), sem tráfego de rede adicional. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-08 | Spec técnica criada via `/create-technical-spec` (Opção A: reflection pura no Fika, sem tocar o fork). |
| 2026-09-08 | **Reescrita (Opção B)** após o usuário confirmar que `mods/FIKA/modded` é o fork realmente usado e decidir expor um hook genérico lá (item `FIKA/005`) em vez de reflection contra classe privada aninhada. A fórmula da Opção A foi preservada como comentário no stub (§5), como fallback documentado caso o mod precise rodar contra um Fika não-forkado no futuro. |
| 2026-09-09 | Aplicados os 3 pontos da [review 01](090-velocidade-cura-nao-replica-03-spec-tech-review-01.md): `[BepInDependency("com.fika.core", SoftDependency)]` adicionado ao stub de `Plugin.cs` (PA-01-01, bloqueador); logs de sucesso/aviso em `Register()` (PA-01-02); checklist ganhou o cenário de Fika/fork sem o hook (PA-01-03). |
