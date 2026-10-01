# 008 — Acordar/Empurrar Corpos e Itens por Contato Físico (Atropelar) · Spec Técnica

**Mod:** VisceralCombat
**Spec funcional:** [008-atropelar-acorda-corpos-itens-01-spec.md](008-atropelar-acorda-corpos-itens-01-spec.md)
**Criado:** 2026-09-21

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

## 1. Estratégia

**Novo Postfix Harmony em `Player.OnControllerColliderHit(ControllerColliderHit hit)`** ([Player.cs:28849](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L28849)) — método público, **não-virtual**, com uma única declaração em todo o Assembly (confirmado via busca textual: só 3 arquivos citam esse nome — `Player.cs` [declaração], `MovementContext.cs` [lógica interna delegada, inalterada] e `GPUInstancer/FPController.cs` [asset de terceiros não relacionado à hierarquia de `Player`]) — sem risco AP-03. `Player.OnControllerColliderHit` delega pra `MovementContext.OnControllerColliderHit` ([MovementContext.cs:3740-3756](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L3740-L3756)), que só registra o toque pra detecção de chão/degrau — **nenhum código no jogo aplica força a partir desse callback hoje**.

**Causa raiz — corpos (confirmada, revisão da hipótese inicial desta sessão):** a hipótese conversacional inicial apontava pro sono nativo da Unity (`Muscle.cs:1141`, biblioteca PuppetMaster, mecanismo de blend de ragdoll ativo). Investigação mais profunda no código do PRÓPRIO mod encontrou o mecanismo real: `RagdollHelperClass.SleepCorpseWhenAtRest` (`modded/VisceralCombat/VisceralCombat.Ragdolls.Classes/RagdollHelperClass.cs:673-746`) espera o corpo assentar e então define **`rb.isKinematic = true`** pra cada osso do ragdoll ([RagdollHelperClass.cs:743](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Classes/RagdollHelperClass.cs#L743)) — um `Rigidbody` `isKinematic` é, por definição, imóvel por colisão/contato, **independente** de estar "dormindo" no sentido nativo da Unity. Isso bate exatamente com o teste do próprio usuário: corpo recém-morto (ainda não passou por `SleepCorpseWhenAtRest`, `isKinematic == false`) reage normalmente a contato via resolução de física padrão da Unity (um `Rigidbody` não-kinemático sempre recebe uma correção de sobreposição quando tocado por qualquer collider, mesmo sem nenhum código de "empurrão" explícito); corpo já acomodado (`isKinematic == true`) fica literalmente imóvel a qualquer contato até algo colocar `isKinematic = false` de novo — o que hoje só acontece via `RagdollHelperClass.WakeCorpse` ([RagdollHelperClass.cs:753-804](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Classes/RagdollHelperClass.cs#L753-L804)), chamado só a partir de `BodiesImpulsePatch.cs:62` (tiro) e `GrenadeDeadBodiesPatch.cs:39` (granada).

**Causa raiz — itens:** diferente de corpos, o `Rigidbody` de um item largado **nunca** é colocado em `isKinematic = true` — nem no vanilla, nem pelo item `006` (que só evita a DESTRUIÇÃO do `Rigidbody`, mantendo ele "dormindo" no sentido nativo da Unity, não kinemático). Isso significa que um item ASSENTADO ainda deveria, em teoria, reagir a um simples contato físico via resolução de sobreposição padrão da Unity — **mas só se a colisão entre a camada do item e a camada do jogador não estiver sendo ignorada**. Um item assentado é reatribuído pra camada "Deadbody" (`PhysicalItemsPatch.cs:29`, já existente) — a MESMA camada usada pelos ossos de corpo — e a colisão entre "Deadbody" e "Player"/"HitCollider" é controlada pela opção "Player Body Collision" (`BodyCollision`), aplicada uma vez no início da raid em `GameStartedPatch.cs:48-66` via `Physics.IgnoreLayerCollision`. Ou seja: **com "Player Body Collision" ligado, um item assentado plausivelmente já recebe uma reação passiva fraca de sobreposição hoje, mesmo sem este item** — mas essa reação (resolução de sobreposição da física) não é proporcional à velocidade do jogador nem visualmente satisfatória como "empurrão" (é só o suficiente pra desfazer a sobreposição, não um chute). Este item adiciona a reação PROPORCIONAL à velocidade que falta, tanto pra corpos (que hoje não reagem NADA depois de dormir) quanto pra itens (que podem já reagir fracamente, mas não de forma proporcional/satisfatória).

**Decisões do usuário aplicadas nesta spec:**
- Só o jogador humano dispara a reação — usando `Player.IsYourPlayer` ([Player.cs:25372](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L25372), `public bool IsYourPlayer { get; set; }`), verdadeiro só pra o personagem controlado localmente nesta máquina (funciona igual em host e em peer — cada um vê seu próprio `IsYourPlayer == true`, nenhum outro `Player` na cena, seja bot ou peer remoto observado, tem esse valor). Bots são excluídos por essa checagem, mesmo compartilhando o mesmo método `OnControllerColliderHit`. <!-- ref: PA-01-03 --> Confirmado via fonte Fika (não só convenção geral do EFT): [`FikaPlayer.cs:173`](../../../../references/fika-plugin/Fika.Core/Main/Players/FikaPlayer.cs#L173) (`player.IsYourPlayer = true;` — setado pro personagem controlado localmente, seja host ou peer) e [`ObservedPlayer.cs:217`](../../../../references/fika-plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L217) (`player.IsYourPlayer = false;` — setado explicitamente pra peers remotos observados no seu client).
- Reaproveita os toggles já existentes: reação em corpo condicionada a "Player Body Collision" (`BodyCollision`), reação em item condicionada a "Item Physics" (`ItemForce`) — ambos via `IsCategoryActive` (item 005, já compõe o toggle mestre).

**Alternativas descartadas:**
- *Reusar o mesmo teto/piso de massa do item `007` (`MassCapKg`/`MinMassKg`) pro empurrão de contato* — descartada. O item 007 resolve um problema específico de física REALISTA (momento de bala real vs. peso real do item); este item é uma mecânica nova, deliberadamente mais "de jogo" (um chute proporcional à velocidade de quem esbarra, não uma simulação de impacto). Usar uma fórmula independente de massa (`ForceMode.VelocityChange` direto, sem dividir pela massa do alvo) é mais simples e não tem nenhuma queixa equivalente de "item pesado não reage" a resolver aqui — reavaliar só se o teste em jogo mostrar necessidade.
- *Patch em `MovementContext.OnControllerColliderHit` em vez de `Player.OnControllerColliderHit`* — descartada. `Player.OnControllerColliderHit` já nos dá acesso direto à instância do `Player` (`__instance`, necessário pra `IsYourPlayer`/`Velocity`) sem precisar resolver o `Player` dono a partir do `MovementContext` (que não expõe uma referência pública de volta pro `Player` de forma tão direta); patchear na camada mais externa (`Player`) é mais simples e não perde nenhuma informação do `hit`.

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`Player.cs:28849` `OnControllerColliderHit(ControllerColliderHit hit)`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L28849) | Postfix | Dispara depois da lógica vanilla (detecção de chão, inalterada) — aplica a reação de contato quando o alvo é um corpo dormindo ou um item físico, só pro jogador humano local. |

Métodos/campos do mod (não-EFT) reaproveitados, não modificados:

| Referência (mod) | Uso |
|---|---|
| [`RagdollHelperClass.WakeCorpse(Collider, float)`](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Classes/RagdollHelperClass.cs#L753) | Reusa o mesmo mecanismo já usado por tiro/granada pra tirar o corpo do `isKinematic = true`, sem duplicar lógica. |
| [`GameStartedPatch.cs:48-66`](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/GameStartedPatch.cs#L48-L66) | Confirma que "Player Body Collision" já controla se corpo/item assentado (camada "Deadbody") colide fisicamente com o jogador — contexto, não modificado além de um `Clear()` novo (§4). |

## 3. Novas propriedades F12 (BepInEx)

Nenhuma `ConfigEntry` nova — reaproveita "Item Physics" (`ItemForce`, [VisceralEntry.cs:326](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralEntry.cs#L326)) e "Player Body Collision" (`BodyCollision`, [VisceralEntry.cs:299](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralEntry.cs#L299)). Ajuste cosmético de tooltip (mesma seção/chave, sem quebrar valor salvo do usuário — não é breaking change):

| Seção | Nome (EN) | Tooltip (pt-BR) atual | Tooltip (pt-BR) novo |
|---|---|---|---|
| `Ragdolls \| Ragdoll Phsyical Properties` | `Player Body Collision` | "Allows you to step on bodies. You can potentially get stuck on them once in awhile for brief moments. Turn this off if you do not like it." | Mesmo texto + `" Também controla se atropelar um corpo já acomodado o empurra (proporcional à sua velocidade)."` |
| `Physics \| Item Physical Properties` | `Item Physics` | "If you are getting too much lag turn this off. But most capable PC's should run this fine. (Besides on SoT)" | Mesmo texto + `" Também controla se atropelar um item largado o empurra (proporcional à sua velocidade)."` |

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/PlayerContactPushPatch.cs` | CRIAR | Postfix em `Player.OnControllerColliderHit` — detecta corpo/item tocado, aplica empurrão proporcional à velocidade do jogador humano local. |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/GameStartedPatch.cs` | MODIFICAR | Adicionar `PlayerContactPushPatch.ClearContactPushCooldowns();` junto aos outros `.Clear()` de início de raid (perto da linha 30-38). |
| `modded/VisceralCombat/VisceralCombat/VisceralEntry.cs` | MODIFICAR | Registrar o novo patch em `Awake()`; ajustar os 2 tooltips (§3); bump de versão. |

## 5. Stubs de código

```csharp
// modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/PlayerContactPushPatch.cs
using System;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using EFT.Interactive;
using SPT.Reflection.Patching;
using UnityEngine;
using VisceralCombat.Ragdolls.Classes;

namespace VisceralCombat.Ragdolls.Patches;

/// <summary>
/// Acorda/empurra corpos e itens físicos ao serem tocados pelo personagem do jogador humano local
/// ("atropelar"), sem depender de tiro/granada prévios — ref: Player.OnControllerColliderHit
/// (Assembly-CSharp/EFT/Player.cs:28849), que hoje só registra o toque pra detecção de chão
/// (MovementContext.cs:3740-3756), sem aplicar nenhuma força.
/// </summary>
public class PlayerContactPushPatch : ModulePatch
{
	private const float ContactPushCooldownSeconds = 0.35f; // evita reaplicar força a cada quadro em contato prolongado
	private const float MinSpeedForPush = 0.3f; // m/s — abaixo disso, considera "parado", sem empurrão
	private const float MaxSpeedForPush = 6f; // m/s — teto pra não deixar sprint gerar empurrão exagerado
	private const float PushIntensity = 0.5f; // ref: item 008 — calibrar em jogo se necessário
	private const float CorpseWakeDuration = 2.5f; // mesma duração default já usada por BodiesImpulsePatch.cs:62

	private static readonly Dictionary<Rigidbody, float> _lastPushTime = new Dictionary<Rigidbody, float>();

	// ref: item 006 — mesmo padrão de limpeza de estado raid-scoped (GameStartedPatch.Postfix)
	public static void ClearContactPushCooldowns()
	{
		_lastPushTime.Clear();
	}

	protected override MethodBase GetTargetMethod()
	{
		// ref: Player.cs:28849 — não-virtual, declaração única no Assembly, sem overrides (AP-03 N/A)
		return typeof(Player).GetMethod("OnControllerColliderHit", BindingFlags.Instance | BindingFlags.Public);
	}

	[PatchPostfix]
	private static void Postfix(Player __instance, ControllerColliderHit hit)
	{
		try
		{
			// Ordem por custo: checks mais baratos primeiro, eliminam a grande maioria das
			// chamadas (bots, toque em geometria estática sem Rigidbody) antes de qualquer
			// trabalho mais caro.
			if (__instance == null || !__instance.IsYourPlayer) return; // ref: Player.cs:25372 — decisão do usuário, só jogador humano local

			Rigidbody rb = hit.rigidbody;
			if (rb == null) return; // geometria estática (chão, parede) não tem Rigidbody — maioria dos toques

			if (_lastPushTime.TryGetValue(rb, out float lastTime) && Time.time - lastTime < ContactPushCooldownSeconds)
			{
				return; // contato prolongado no mesmo Rigidbody — evita custo/força sem teto por quadro
			}

			// ref: PA-01-02 — só o componente horizontal conta pra "atropelar". Sem isso, pular/cair
			// em cima de um corpo (ex.: descendo de um telhado) empurraria o alvo pra baixo/através
			// do chão em vez de lateralmente, já que Player.Velocity inclui a queda livre (Y).
			Vector3 horizontalVelocity = __instance.Velocity; // ref: Player.cs:24613
			horizontalVelocity.y = 0f;
			float speed = horizontalVelocity.magnitude;
			if (speed < MinSpeedForPush) return; // parado (ou só caindo, sem movimento horizontal) não empurra

			bool isLootItem = rb.gameObject.GetComponent<ObservedLootItem>() != null;
			bool isCorpseBone = false;
			Player corpseOwner = null;
			if (!isLootItem)
			{
				corpseOwner = hit.collider.GetComponentInParent<Player>();
				isCorpseBone = corpseOwner != null && corpseOwner.HealthController != null && !corpseOwner.HealthController.IsAlive;
			}

			if (!isLootItem && !isCorpseBone) return; // colisor irrelevante (ex.: outro player vivo, prop não coberto)

			// ref: PA-01-01 — se a parte tocada já foi desmembrada, WakeCorpse tem uma exclusão
			// interna (RagdollHelperClass.cs:775-783) que a trava em isKinematic=true E
			// detectCollisions=false — pior que "sem reação": desliga a colisão dela pro resto da
			// raid. Pular o empurrão inteiro nesse caso em vez de acionar essa exclusão de lado.
			if (isCorpseBone && RagdollHelperClass.ParentIsDismembered(rb.transform)) return; // ref: RagdollHelperClass.cs:508

			if (isLootItem)
			{
				if (VisceralEntry.Instance == null || !VisceralEntry.Instance.IsCategoryActive(VisceralEntry.Instance.ItemForce)) return; // ref: item 005
			}
			else
			{
				if (VisceralEntry.Instance == null || !VisceralEntry.Instance.IsCategoryActive(VisceralEntry.Instance.BodyCollision)) return; // ref: item 005
				RagdollHelperClass.WakeCorpse(hit.collider, CorpseWakeDuration); // ref: RagdollHelperClass.cs:753 — tira do isKinematic=true se necessário
			}

			float clampedSpeed = Mathf.Min(speed, MaxSpeedForPush);
			Vector3 deltaV = horizontalVelocity.normalized * (clampedSpeed * PushIntensity);
			rb.AddForceAtPosition(deltaV, hit.point, ForceMode.VelocityChange);

			_lastPushTime[rb] = Time.time;
		}
		catch (Exception ex)
		{
			QuickLogger.Log(ELogType.Error, $"[PlayerContactPushPatch] {ex}");
		}
	}
}
```

## 6. Fluxo de dados

```
[A] Jogador humano local move o personagem e a cápsula (CharacterController) toca um corpo ou
    item — Unity chama Player.OnControllerColliderHit (Player.cs:28849), que hoje só delega pra
    MovementContext.OnControllerColliderHit (detecção de chão, MovementContext.cs:3740-3756)
  → [B] NOVO: PlayerContactPushPatch.Postfix roda depois. Filtra: só __instance.IsYourPlayer
    (Player.cs:25372), só hit.rigidbody != null, respeita cooldown por Rigidbody, só acima de
    MinSpeedForPush
  → [C] Identifica o tipo de alvo: ObservedLootItem (mesmo padrão de BodiesImpulsePatch.cs:41) ou
    osso de corpo morto (GetComponentInParent<Player>() + HealthController.IsAlive == false)
  → [D] Se corpo: chama RagdollHelperClass.WakeCorpse(hit.collider, ...) (RagdollHelperClass.cs:753)
    — tira o osso de isKinematic=true (RagdollHelperClass.cs:743) se necessário, sem duplicar
    lógica já usada por tiro/granada
  → [E] Calcula deltaV = direção da velocidade HORIZONTAL do jogador (componente Y zerado, ref:
    PA-01-02) × (velocidade limitada × PushIntensity) — independente da massa do alvo (diferente
    do item 007, ver §1)
  → [F] Aplica via AddForceAtPosition(deltaV, hit.point, ForceMode.VelocityChange) — corpo/item
    reage com empurrão proporcional à velocidade de quem esbarrou, sem precisar de tiro prévio
```

## 7. Riscos e dependências

- **Custo do callback (`OnControllerColliderHit`):** esse callback é chamado pela Unity sempre que a cápsula do personagem toca QUALQUER collider, inclusive chão/parede a cada quadro em que há contato — o que é praticamente sempre (o personagem está sempre tocando o chão enquanto no ar não). A primeira checagem do Postfix (`hit.rigidbody == null`) já elimina a esmagadora maioria dessas chamadas (geometria estática não tem `Rigidbody`) com custo mínimo (um acesso de propriedade); `IsYourPlayer` elimina TODOS os bots antes disso ainda. O cooldown por `Rigidbody` (`_lastPushTime`) garante que mesmo contato prolongado com um corpo/item não gera reaplicação de força a cada quadro — resolve diretamente o corner case "contato constante/prolongado" da spec funcional.
- **Estado estático raid-scoped (`_lastPushTime`):** precisa ser limpo no início de cada raid — `ClearContactPushCooldowns()` chamado a partir de `GameStartedPatch.Postfix` (§4), mesmo padrão já usado pelas outras coleções estáticas do mod (`dismemberedPlayers.Clear()` etc., `GameStartedPatch.cs:30-38`). As chaves (`Rigidbody`) referenciam objetos que são destruídos/reciclados no fim da raid pelo próprio jogo — sem o `Clear()`, entradas órfãs se acumulariam entre raids (leak leve, não crítico, mas evitável).
- **Dependência do item `006` (persistência de física em itens):** este item só funciona bem em itens já assentados porque o `006` garante que o `Rigidbody` do item continua vivo (não destruído) depois de acomodar — sem o `006`, um item assentado há muito tempo não teria `Rigidbody` nenhum pra `hit.rigidbody` capturar, e este patch simplesmente não encontraria nada pra empurrar (mesma limitação que o `006` já resolveu pro caso de tiro).
- **Reação passiva pré-existente em itens via sobreposição de física (ver §1):** como o `Rigidbody` de um item nunca é `isKinematic`, é plausível que itens já recebam uma correção de sobreposição fraca e não-proporcional à velocidade hoje, quando "Player Body Collision" está ligado — este item não conflita com isso, só adiciona a reação proporcional que falta. Corpos não têm esse problema de sobreposição residual porque ficam genuinamente `isKinematic = true` (imóveis) até serem acordados.
- **Camada "Deadbody" compartilhada entre corpos e itens assentados (`PhysicalItemsPatch.cs:29`):** confirma que "Player Body Collision" (`GameStartedPatch.cs:48-66`) já é, tecnicamente, o toggle que decide se o jogador consegue fisicamente encostar em QUALQUER coisa na camada "Deadbody" (corpo OU item assentado) — mas este item usa toggles SEPARADOS por tipo de alvo (`BodyCollision` pra corpo, `ItemForce` pra item) conforme decisão explícita do usuário, então um jogador pode ter "Player Body Collision" ligado (colide com tudo na camada) mas "Item Physics" desligado (não recebe empurrão de item, só de corpo) — comportamento consistente com o pedido do usuário, ainda que a camada física de baixo nível não distinga os dois.
- **Limitação conhecida — mudança de "Player Body Collision" no meio da raid:** a matriz de colisão de camada (`Physics.IgnoreLayerCollision`) só é aplicada uma vez, no início da raid (`GameStartedPatch.Postfix`) — mudar o toggle no meio da raid não reativa/desativa retroativamente a colisão física em si (limitação pré-existente, não introduzida por este item). A checagem `IsCategoryActive(BodyCollision)` feita neste patch É reavaliada a cada contato (então reage a mudanças de toggle em tempo real pro EMPURRÃO), mas só terá contato pra reagir se a colisão de camada já estiver habilitada desde o início da raid.
- <!-- ref: PA-01-01 --> **Parte de corpo já desmembrada (resolvido com guarda explícita):** `WakeCorpse` já tem uma exclusão interna pra ossos desmembrados ([RagdollHelperClass.cs:775-783](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Classes/RagdollHelperClass.cs#L775-L783)) que trava o `Rigidbody` em `isKinematic = true` **e** `detectCollisions = false` — pior que "sem reação", desligaria a colisão da parte pro resto da raid se acionada de lado por este patch. Resolvido adicionando `if (isCorpseBone && RagdollHelperClass.ParentIsDismembered(rb.transform)) return;` (§5) antes de chamar `WakeCorpse` — pula o empurrão inteiro nesse caso, sem acionar a exclusão interna. **TODO confirmar em jogo:** se `GetComponentInParent<Player>()` ainda resolve pro `Player` original numa parte já destacada (photos podem reparentar pra um GameObject standalone) — se resolver, cai nesse novo early-return (sem reação, seguro); se não resolver, a parte simplesmente não é detectada como `isCorpseBone` (mesmo resultado prático: sem reação, mas por um caminho diferente). De qualquer forma, o cenário de risco (colisão desligada permanentemente) está eliminado.
- <!-- ref: PA-01-02 --> **Componente vertical de velocidade (resolvido):** `Player.Velocity`/`MovementContext.Velocity` ([MovementContext.cs:1110](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1110), `CharacterController.velocity`) inclui a queda livre (eixo Y) — sem filtrar isso, pousar sobre um corpo/item ao descer de um lugar alto (cenário plausível em mapas com verticalidade) empurraria o alvo majoritariamente pra baixo, não lateralmente. Resolvido zerando o componente Y antes de calcular direção/magnitude do empurrão (§5) — "atropelar" passa a significar só movimento horizontal, sem interferência de quedas/pulos.
- **Ordem de inicialização:** registrar `PlayerContactPushPatch` em `VisceralEntry.Awake()`, sem dependência de ordem com os outros patches (não compartilha estado com nenhum outro exceto o já mapeado acima).

## 8. Checklist de implementação

- [x] Reconfirmar `Player.cs:28849,25372,24613`, `MovementContext.cs:3740-3756`, `RagdollHelperClass.cs:743,753-804`, `GameStartedPatch.cs:48-66` no momento do `/code-mod` (já lidos e confirmados nesta sessão via investigação direta — repetir antes de codar, AP-09).
- [x] Criar `PlayerContactPushPatch.cs` com a classe e método descritos em §5.
- [x] Adicionar `PlayerContactPushPatch.ClearContactPushCooldowns();` em `GameStartedPatch.cs`, junto aos outros `.Clear()` de início de raid.
- [x] Registrar `PlayerContactPushPatch` em `VisceralEntry.Awake()`.
- [x] Ajustar os 2 tooltips (§3) em `VisceralEntry.cs` e atualizar `PROPRIEDADES.md` de acordo.
- [x] Bump de versão (`VisceralEntry.cs` + `.csproj`).
- [x] Compilar, 0 erros.
- [ ] Validar em jogo: encostar levemente (andando devagar) num corpo já acomodado, sem tiro prévio — deslocamento pequeno mas perceptível.
- [ ] Validar em jogo: correr por cima do mesmo corpo — empurrão nitidamente mais forte que o encostão leve.
- [ ] Validar em jogo: ficar parado encostado — sem força repetida/vibração.
- [ ] Validar em jogo: mesmo comportamento (leve/forte) num item físico assentado (arma, capacete).
- [ ] Validar regressão: bots atropelando corpos/itens não disparam a reação nova.
- [ ] Validar corner case (TODO confirmar acima): atropelar uma parte de corpo já desmembrada e largada separadamente — confirmar que não reage (esperado) e que não trava a colisão dela permanentemente (PA-01-01).
- [ ] Validar: pular/cair de um lugar alto sobre um corpo/item — empurrão deve ser lateral (baseado na velocidade horizontal no momento do contato), não pra baixo (PA-01-02).
- [ ] Validar: com "Player Body Collision"/"Item Physics" desligados, nenhuma reação de contato nova ocorre (comportamento idêntico ao atual).
- [ ] Validar em raid longa/com múltiplos corpos e itens: sem acúmulo perceptível de lag (checagem qualitativa).

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | ✅ | `_lastPushTime` (estado estático raid-scoped) é limpo via `ClearContactPushCooldowns()`, chamado em `GameStartedPatch.Postfix` (início de cada raid) — mesmo padrão das outras coleções estáticas do mod (§7). |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | `__instance.IsYourPlayer` (Player.cs:25372) filtra pro personagem controlado localmente nesta máquina — decisão explícita do usuário de excluir bots e peers remotos observados (§1, §5). Confirmado com fonte Fika (ref: PA-01-03): `FikaPlayer.cs:173` (setado `true` pro personagem local, host ou peer) e `ObservedPlayer.cs:217` (setado `false` pra peers remotos observados). |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | ✅ | `Player.OnControllerColliderHit` confirmado não-virtual, declaração única no Assembly (3 arquivos citam o nome, só 1 é a declaração real — ver §1). Sem overrides a auditar. |
| 4 | Mudança de estado via API canônica; side-effects mapeados — AP-04 | ✅ | `RagdollHelperClass.WakeCorpse` (mod, já existente e auditado) + `Rigidbody.AddForceAtPosition` (API pública padrão da Unity) — nenhum acesso via reflexão a campo privado neste item. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | `_lastPushTime` limpo a cada `OnGameStarted` (início de raid) — cobre início limpo em qualquer cenário de entrada em raid, incluindo após alt-F4/MIA da raid anterior (o jogo sempre recria o `GameWorld` do zero). |
| 6 | Semântica/defaults de ConfigEntry sem ambiguidade — AP-05 | N/A | Nenhum `ConfigEntry` novo — só ajuste de texto de tooltip em 2 entradas já existentes (§3), sem mudar seção/chave/default/faixa. |
| 7 | Reentrância: sem recursão infinita — AP-07 | ✅ | `RagdollHelperClass.WakeCorpse` não invoca `Player.OnControllerColliderHit` nem move o personagem — sem caminho de retorno síncrono pro Prefix/Postfix deste patch. |
| 8 | Flags/caches validados contra o contexto atual após troca — AP-08 | N/A | `_lastPushTime` é chaveado por referência de `Rigidbody` viva (não por identidade de arma/operação/tela); uma referência órfã de raid anterior nunca é reconsultada (raid recria o mundo do zero) e é limpa no início da próxima raid de qualquer forma — sem risco de reuso indevido de contexto. |
| 9 | Patch-point reconfirmado no `.cs` do dump, não só recon — AP-09 | ✅ | Todos os pontos citados (`Player.cs:28849,25372,24613`; `MovementContext.cs:3740-3756`) foram lidos diretamente nesta sessão via ferramenta de leitura de arquivo/busca, não vieram de recon de subagente. Pontos do mod (`RagdollHelperClass.cs`, `GameStartedPatch.cs`) também relidos diretamente. |
| 10 | Skill EFT como lever confirmada não-inerte — AP-10 | N/A | Este item não usa nenhuma skill do EFT como alavanca. |
| 11 | Pacote FIKA próprio conforme guia de prevenção de dessincronia — AP-11 | N/A | Nenhum `INetSerializable`/pacote de rede novo — reação de contato é aplicada localmente por cada peer sobre sua própria cópia observada do corpo/item, mesmo padrão já documentado no item `006` (sem sincronização adicional). |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-21 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-21 | Revisão `03-spec-tech-review-01.md` — 3 achados (PA-01-01/02/03), todos aceitos e aplicados: guarda contra parte desmembrada antes de `WakeCorpse` (§5, §7, §8); componente vertical de velocidade filtrado (§5, §6, §7, §8); citações Fika (`FikaPlayer.cs:173`, `ObservedPlayer.cs:217`) adicionadas a §1 e §9 check 2. |

**Status:** ✅ Pronta para `/code-mod`
