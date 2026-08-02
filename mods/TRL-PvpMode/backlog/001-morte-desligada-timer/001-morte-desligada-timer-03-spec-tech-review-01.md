# 001 — Morte desligada com timer · Review da Spec Técnica (rodada 01)

**Mod:** TRL-PvpMode
**Spec técnica:** [001-morte-desligada-timer-02-spec-tech.md](001-morte-desligada-timer-02-spec-tech.md)
**Data:** 2026-08-01
**Método:** revisão adversarial por agente com contexto limpo, conferindo cada citação contra o fonte do Fika
e o dump do Assembly.

**Resultado:** 3 🔴 · 8 🟡 · 4 🔵 — todos com prova em código. Nenhum achado foi rejeitado.

---

## 🔴 Bloqueadores

### R-01 — "Finalizar o caído" não funciona pelo caminho previsto

**Onde:** §2 (prefixo de `Kill`, função "b") · §5 `KillGatePatch`

Ao entrar em caído, o Fika zera o coeficiente de dano
([FikaPlayer.cs:614](../../../../references/fika-plugin/Fika.Core/Main/Players/FikaPlayer.cs#L614)). Com ele
em zero, `ApplyDamage` sai antes de tocar em qualquer coisa
([ActiveHealthController.cs:3723](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L3723)
— `if (base.IsAlive && base.DamageCoeff > 0f)`), então **`Kill` nunca é chamado para quem já está caído** e
o portão nunca é avaliado. Bloqueio independente: `Kill` é `if (base.IsAlive) { … }`
([:3923](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L3923))
e `IsAlive` já é `false`. É por isso que `BleedOut()` precisa fazer `IsAlive = true` antes de chamar `Kill`
— com o comentário do próprio Fika: *"need to be alive to trigger Kill() again"*
([ClientHealthController.cs:125-130](../../../../references/fika-plugin/Fika.Core/Main/ClientClasses/ClientHealthController.cs#L125-L130)).
Agravante: nos outros clientes o corpo vai para a camada `Deadbody` e o componente do jogador é desligado
([ReviveInteractable.cs:80,100](../../../../references/fika-plugin/Fika.Core/Main/Components/ReviveInteractable.cs#L80))
— o tiro talvez nem gere evento de dano contra um jogador.

**Decisão do host:** ✅ **adiado para item próprio.** A opção sai do 001 (config e código). Criado o item
**005 — Finalizar o caído**. Motivo: entregar uma chave no F12 que talvez não faça nada é pior que não ter a
chave.

### R-02 — Morte por desgaste vira limbo: "morto" que anda e partida que não termina

**Onde:** §2/§6 — nada trata fome, desidratação e overdose de estimulante

O prefixo do Fika em `Kill` **não** filtra desgaste: olha só `CanBeDowned` e
`CheckIfDamageShouldInstantKill`
([ClientHealthController_Kill_Patch.cs:20-25](../../../../references/fika-plugin/Fika.Core/Main/Patches/Revival/ClientHealthController_Kill_Patch.cs#L20-L25)).
Hoje, jogando sozinho, `CanBeDowned` é `false` e a morte passa. **O destravamento central desta spec faz
esse caminho passar a ser tomado também por desgaste.** Aí o prefixo suprime o evento de morte, e logo
depois `TryProcessDownedState` recusa o estado de caído justamente por causa desses tipos
([ClientHealthController.cs:159-162](../../../../references/fika-plugin/Fika.Core/Main/ClientClasses/ClientHealthController.cs#L159-L162)).
Resultado: `IsAlive = false`, sem caído, **sem evento de morte** — logo sem tela de fim de raid
([CoopGame.cs:610](../../../../references/fika-plugin/Fika.Core/Main/GameMode/CoopGame.cs#L610)) — e sem
trava de movimento. **É literalmente o defeito do mod de referência que este item existe para corrigir.**

**Decisão:** ✅ **corrigido.** O postfix de `CheckIfDamageShouldInstantKill` passa a retornar `true` para
`Exhaustion | Dehydration | Stimulator`, fazendo o prefixo do Fika devolver `true` e a morte seguir o
caminho normal. Cobre os dois pontos de decisão de uma vez.

### R-03 — `BaseLocalGame.Stop` nunca dispara em partida Fika (e o arquivo citado não existe)

**Onde:** §2 ("fim de raid caminho 2") · §9 checks 1, 3 e 9 — todos marcados ✅ indevidamente

Três problemas empilhados: (a) o caminho citado não existe no dump — o arquivo é `EFT/BaseLocalGame-1.cs` e
o tipo é genérico aberto (`BaseLocalGame<TPlayerOwner>`), que o Harmony recusa patchar, de modo que
`GetTargetMethod` devolveria `null` e o plugin **estouraria na inicialização**; (b) mesmo fechando o
genérico, `Stop` é `virtual` e o Fika **sobrescreve sem chamar a base**
([CoopGame.cs:718](../../../../references/fika-plugin/Fika.Core/Main/GameMode/CoopGame.cs#L718), zero
ocorrências de `base.Stop`) — AP-03 puro; (c) **o repo já bateu nessa pedra** e removeu o patch
([stances RaidLifecyclePatches.cs:60-62](../../../stancesAndCameraPositionSPT4.0.11/modded/Patches/RaidLifecyclePatches.cs#L60-L62)).

**Decisão:** ✅ **corrigido seguindo o precedente do repo.** Fica só `GameWorld.OnDestroy` com `End()`
idempotente. O check 9 do §9 estava afirmando ter reconfirmado no dump um arquivo inexistente — isso é
AP-09 e foi corrigido.

---

## 🟡 Importantes

| ID | Achado | Decisão |
|---|---|---|
| **R-04** | `BleedoutTime` não governa tudo: a plaquinha de vida do caído vista pelos companheiros lê o config do servidor cru ([FikaHealthBar.cs:619-624](../../../../references/fika-plugin/Fika.Core/Main/Components/FikaHealthBar.cs#L619-L624)). Além disso, o getter de auto-property com inicializador é candidato a ser embutido pelo compilador, deixando o patch **inerte sem ninguém perceber** | ✅ Trocado: em vez de patchar o getter, escrever o campo de apoio (`<BleedoutTime>k__BackingField`) no início da raid — imune a inlining. A plaquinha remota mantém o valor do servidor: **limitação documentada** |
| **R-05** | Com tempo 0, o `return` antecipado de `Bleedout.Update` acontece **antes** de `CheckForKeys()` ([Bleedout.cs:77-86](../../../../references/fika-plugin/Fika.Core/Main/Components/Bleedout.cs#L77-L86)) — a tecla nativa morre junto. O componente segue vivo com um gemido a cada 10s indefinidamente | ✅ Registrado em §7 e no `PROPRIEDADES.md`. **O item 002 não pode depender de `Bleedout.CheckForKeys`** para a tecla de renascer |
| **R-06** | `ref object __result` num método que devolve `ActionsReturnClass` gera IL não verificável | ✅ Corrigido para `ref ActionsReturnClass __result`. (O retorno `null` em si é seguro: `GamePlayerOwner.cs:883-889` faz `?.InitSelected()`) |
| **R-07** | O postfix de `CheckIfDamageShouldInstantKill` não tem de onde ler o tipo de dano: `FikaPlayer.LatestDamageInfo` é `internal` e o campo do EFT é `protected` | ✅ Corrigido: o `EDamageType` vem do parâmetro que o **nosso próprio** prefixo de `Kill` já recebe, guardado para o mesmo quadro |
| **R-08** | Sem guarda de esconderijo no hook de início — `RaidState.Begin()` rodaria no hideout | ✅ Adicionada a guarda `MainPlayer is not HideoutPlayer` |
| **R-09** | Tooltip de `Enable Lives Mode` promete comportamento vanilla, que só existe com a chave do servidor desligada — AP-05 | ✅ **Decisão do host:** desligado = volta o resgate nativo do Fika. Tooltip reescrito para dizer isso |
| **R-10** | O bloqueio de resgate roda no cliente de **quem olha**: um par sem o mod continua vendo e conseguindo levantar o caído, furando a contagem de vidas | ✅ Declarado no §7 e no pré-requisito operacional: **o mod precisa estar em todos os clientes** |
| **R-11** | Corner cases do 01-spec sem solução técnica: reconexão estando caído, transição/extração, anfitrião caído, alt-F4 | ✅ Endereçados um a um no §7 (resolver / delegar / aceitar com justificativa). "Dois danos no mesmo quadro" está coberto — mas **pelo Fika**, não por decisão nossa; passou a ser declarado como coberto-por-terceiro |

## 🔵 Melhorias

| ID | Achado | Decisão |
|---|---|---|
| **R-12** | Citações imprecisas: `CanBeDowned` "(:18)" é a declaração, a condição está em `:22`; `GetActions` é `public` (o **tipo** é que é `internal`); o §1 desenha `ObservedPlayer.ToggleDowned` como filho direto de `FikaPlayer.ToggleDowned`, quando a ligação é pela rede | ✅ Corrigidas |
| **R-13** | Nome de arquivo inconsistente (`NoAllyReviveePatches` × `NoAllyRevivePatches`) | ✅ Padronizado |
| **R-14** | §5 mistura `ModulePatch` (SPT) com `[HarmonyPatch]` puro, e o `Plugin.cs` não tem instância Harmony | ✅ Padronizado em `ModulePatch`; a prioridade do prefixo de `Kill` passa a ser resolvida por `HarmonyPriority` no próprio método |
| **R-15** | `Bleedout.Init` fotografa prazo e `_shouldBleed` no instante da queda — mudar o tempo no F12 estando caído não tem efeito | ✅ Documentado no `PROPRIEDADES.md` |

---

## Histórico

| Data | Evento |
|---|---|
| 2026-08-01 | Review adversarial rodada 01 — 3 🔴, 8 🟡, 4 🔵; nenhum achado rejeitado |
| 2026-08-01 | Decisões do host: R-01 adiado para item 005; R-09 resolvido como "volta o resgate nativo" |
