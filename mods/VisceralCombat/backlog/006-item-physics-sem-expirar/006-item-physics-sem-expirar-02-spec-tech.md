# 006 — Item Physics Sem Expirar (Manter Rigidbody Adormecido em Vez de Destruir) · Spec Técnica

**Mod:** VisceralCombat
**Spec funcional:** [006-item-physics-sem-expirar-01-spec.md](006-item-physics-sem-expirar-01-spec.md)
**Criado:** 2026-09-20

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

## 1. Estratégia

`LootItem` (`EFT.Interactive`) roda uma corrotina (`method_4`, [LootItem.cs:446-457](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/LootItem.cs#L446-L457)) uma vez por quadro, checando `IsRigidbodyDone()` ([LootItem.cs:508-519](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/LootItem.cs#L508-L519)) — que retorna `true` assim que o `Rigidbody` "dorme" (`_rigidBody.IsSleeping()`, geralmente poucos segundos após o item parar de se mexer) ou passa de um teto de tempo (7.5s/15s no caso base; 15s/30s em `ObservedLootItem`, [ObservedLootItem.cs:30-41](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/ObservedLootItem.cs#L30-L41)). Quando `IsRigidbodyDone()` retorna `true`, `StopPhysics()` roda ([LootItem.cs:521-534](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/LootItem.cs#L521-L534)) e **destrói o componente `Rigidbody`** (`UnityEngine.Object.Destroy(_rigidBody)`) — depois disso, `BodiesImpulsePatch.ProcessImpulse`/`GrenadeItemsPatch.Postfix` (já existentes no mod) não encontram mais nenhum `Rigidbody` pra aplicar força (`hitCollider.attachedRigidbody` e `GetComponentInParent<Rigidbody>()` retornam `null`), então nada acontece visualmente.

**Estratégia escolhida:** dois Prefixes Harmony, ambos sem alvo virtual (métodos confirmados não-`virtual` no Assembly — sem risco AP-03):

1. **`LootItem.StopPhysics()` Prefix** — quando a categoria `ItemForce` está ativa (`IsCategoryActive`, item 005) e o item é do tipo já coberto pela feature (`ObservedLootItem`, mesmo critério usado em `BodiesImpulsePatch`/`GrenadeItemsPatch`/`PhysicalItemsPatch`), replica só a parte "parar a corrotina" do método original (via reflexão no campo privado `ienumerator_0`) e **pula** a parte que destrói o `Rigidbody`, retornando `false`. Sem a categoria ativa, `return true` — comportamento vanilla inalterado.
2. **`LootItem.Kill()` Prefix** — garante que, se um `Rigidbody` foi preservado pelo patch acima e o item está sendo removido do mundo (jogador lootou, ou outro motivo), o `Rigidbody` seja destruído **antes** do `GameObject` voltar pro pool (`AssetPoolObject.ReturnToPool`, [LootItem.cs:547](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/LootItem.cs#L547)) — evita que um objeto reciclado do pool carregue um `Rigidbody` órfão pro próximo item que usar esse mesmo `GameObject` (ver §7).

**Alternativa descartada:** bloquear `StopPhysics()` inteiro (não só a parte de destruição) — descartada porque isso também impediria `StopCoroutine`, deixando a corrotina de `method_4` rodando pra sempre por item (checagem a cada quadro, indefinidamente) — reintroduziria um custo sem teto no mapa inteiro, o problema que a spec funcional pede pra evitar.

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`LootItem.cs:521` `StopPhysics()`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/LootItem.cs#L521) | Prefix (`return false` condicional) | Pula a destruição do `Rigidbody` quando `ItemForce` está ativo, mas ainda para a corrotina de checagem (evita custo por quadro sem teto). |
| [`LootItem.cs:536` `Kill()`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/LootItem.cs#L536) | Prefix (`return true` sempre — só efeito colateral) | Garante limpeza do `Rigidbody` preservado antes do objeto voltar pro pool de reuso (`AssetPoolObject.ReturnToPool`, linha 547), evitando componente órfão em item reciclado. |

Campos privados/protegidos referenciados via reflexão (mesmo padrão já usado em `KillPatch._getInventoryController`, `modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs:226-230`):

| Campo | Declaração | Acesso |
|---|---|---|
| `ienumerator_0` | [`LootItem.cs:94`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/LootItem.cs#L94) — `private IEnumerator ienumerator_0` | `FieldInfo` cacheado estático, leitura + escrita |
| `_rigidBody` | [`LootItem.cs:56`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/LootItem.cs#L56) — `protected Rigidbody _rigidBody` | `FieldInfo` cacheado estático, leitura + escrita (só no patch de `Kill()`, pra destruir e zerar) |

Propriedade pública já existente, sem precisar de reflexão pra leitura (`public bool IsPhysicsOn => ienumerator_0 != null;`, [LootItem.cs:134](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/LootItem.cs#L134)) — útil como checagem auxiliar se necessário, mas não substitui o acesso de escrita ao campo (que só é possível via reflexão).

## 3. Novas propriedades F12 (BepInEx)

Nenhuma — reaproveita a propriedade "Item Physics" (`ItemForce`) já existente (`modded/VisceralCombat/VisceralCombat/VisceralEntry.cs:326`), sem mudar seção, chave ou tooltip.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LootItemPersistPhysicsPatch.cs` | CRIAR | Os dois Prefixes (`StopPhysics`/`Kill`) descritos acima. |
| `modded/VisceralCombat/VisceralCombat/VisceralEntry.cs` | MODIFICAR | Registrar o novo patch em `Awake()`, junto aos outros patches de `ItemForce` (`GrenadeItemsPatch`, `PhysicalItemsPatch`, ~linha 315-320). Bump de versão. |

## 5. Stubs de código

```csharp
// modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LootItemPersistPhysicsPatch.cs
using System;
using System.Collections;
using System.Reflection;
using EFT.Interactive;
using SPT.Reflection.Patching;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Patches;

/// <summary>
/// Com "Item Physics" (ItemForce) ativo, mantém o Rigidbody de um item largado vivo (só
/// "dormindo", sem simulação ativa) em vez de deixar o jogo destruí-lo quando o item acomoda —
/// ref: LootItem.StopPhysics (Assembly-CSharp/EFT.Interactive/LootItem.cs:521). Sem isso, tiro/
/// explosão num item parado há mais de alguns segundos não tinha em cima do que aplicar força
/// (BodiesImpulsePatch/GrenadeItemsPatch dependem de attachedRigidbody != null).
/// </summary>
public class LootItemStopPhysicsPatch : ModulePatch
{
	// ref: LootItem.cs:94 — private IEnumerator ienumerator_0 (corrotina de assentamento)
	private static readonly FieldInfo _ienumeratorField = typeof(LootItem).GetField("ienumerator_0", BindingFlags.Instance | BindingFlags.NonPublic);

	protected override MethodBase GetTargetMethod()
	{
		// ref: LootItem.cs:521 — não é virtual, alvo único, sem overrides pra auditar (AP-03 N/A)
		return typeof(LootItem).GetMethod("StopPhysics", BindingFlags.Instance | BindingFlags.Public);
	}

	[PatchPrefix]
	private static bool Prefix(LootItem __instance)
	{
		try
		{
			if (VisceralEntry.Instance == null || !VisceralEntry.Instance.IsCategoryActive(VisceralEntry.Instance.ItemForce)) return true; // ref: item 005
			if (__instance == null || __instance.gameObject.GetComponent<ObservedLootItem>() == null) return true; // mesmo critério de BodiesImpulsePatch/GrenadeItemsPatch/PhysicalItemsPatch

			// Replica só a parte de "parar a corrotina" do StopPhysics original — evita o custo
			// de checagem por quadro rodando pra sempre — mas NÃO destrói o Rigidbody (fica
			// "dormindo", Unity acorda ele sozinho quando uma força é aplicada).
			object coroutine = _ienumeratorField?.GetValue(__instance);
			if (coroutine is IEnumerator enumerator)
			{
				((MonoBehaviour)__instance).StopCoroutine(enumerator); // ref: PA-01-01 — cast único, mesmo padrão de VisceralShotProcessor.cs:28
				_ienumeratorField.SetValue(__instance, null);
			}
			return false; // pula o resto do método original (que destruiria o Rigidbody)
		}
		catch (Exception ex)
		{
			QuickLogger.Log(ELogType.Error, $"[LootItemStopPhysicsPatch] {ex}");
			return true; // em caso de erro, comportamento vanilla (seguro)
		}
	}
}

/// <summary>
/// Garante que um Rigidbody preservado por LootItemStopPhysicsPatch seja destruído antes do
/// item voltar pro pool de reuso (AssetPoolObject.ReturnToPool, LootItem.cs:547) — evita que um
/// GameObject reciclado carregue um Rigidbody órfão pro próximo item que o usar.
/// </summary>
public class LootItemKillCleanupPatch : ModulePatch
{
	// ref: LootItem.cs:56 — protected Rigidbody _rigidBody
	private static readonly FieldInfo _rigidBodyField = typeof(LootItem).GetField("_rigidBody", BindingFlags.Instance | BindingFlags.NonPublic);

	protected override MethodBase GetTargetMethod()
	{
		// ref: LootItem.cs:536 — override void Kill() (override de WorldInteractiveObject, não
		// mais sobrescrito por nenhuma classe abaixo de LootItem — ObservedLootItem não redeclara)
		return typeof(LootItem).GetMethod("Kill", BindingFlags.Instance | BindingFlags.Public);
	}

	[PatchPrefix]
	private static bool Prefix(LootItem __instance)
	{
		try
		{
			if (__instance == null) return true;
			object rb = _rigidBodyField?.GetValue(__instance);
			if (rb is Rigidbody rigidbody && rigidbody != null)
			{
				UnityEngine.Object.Destroy(rigidbody);
				_rigidBodyField.SetValue(__instance, null);
			}
		}
		catch (Exception ex)
		{
			QuickLogger.Log(ELogType.Error, $"[LootItemKillCleanupPatch] {ex}");
		}
		return true; // sempre deixa o Kill() original rodar — este patch só limpa, nunca substitui
	}
}
```

## 6. Fluxo de dados

```
[A] Item é largado (qualquer caminho de drop já existente no mod, ou loot vanilla) → ganha
    Rigidbody + corrotina de assentamento (LootItem.method_3, LootItem.cs:403-411)
  → [B] Corrotina roda 1x por quadro (LootItem.method_4, LootItem.cs:446-457), checando
    IsRigidbodyDone() (LootItem.cs:508-519)
  → [C] Quando o Rigidbody "dorme" ou passa do teto de tempo, IsRigidbodyDone() retorna true →
    StopPhysics() roda (LootItem.cs:521)
    → [C1] SEM "Item Physics" ativo: LootItemStopPhysicsPatch.Prefix retorna true → StopPhysics()
      vanilla roda → Rigidbody destruído (comportamento atual, inalterado)
    → [C2] COM "Item Physics" ativo: LootItemStopPhysicsPatch.Prefix para a corrotina (via
      reflexão em ienumerator_0) e retorna false → Rigidbody sobrevive, dormindo
  → [D] Tiro ou explosão atinge o item mais tarde (qualquer momento) → BodiesImpulsePatch.
    ProcessImpulse (modded/.../BodiesImpulsePatch.cs:40-49) ou GrenadeItemsPatch.Postfix
    (modded/.../GrenadeItemsPatch.cs) chamam AddForceAtPosition/AddExplosionForce no Rigidbody
    — Unity acorda automaticamente um Rigidbody dormindo ao receber força (comportamento nativo,
    sem código adicional necessário) → item se move visivelmente
  → [E] Item é removido do mundo (jogador loota, ou outro motivo) → Kill() roda (LootItem.cs:536)
    → LootItemKillCleanupPatch.Prefix destrói qualquer Rigidbody ainda vivo (preservado ou não)
      antes do GameObject voltar pro pool (AssetPoolObject.ReturnToPool, LootItem.cs:547)
```

## 7. Riscos e dependências

- **Pooling de `GameObject` (`AssetPoolObject.ReturnToPool`, [LootItem.cs:547](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/LootItem.cs#L547)):** `Kill()` **não chama `StopPhysics()`** em nenhum ponto do próprio método (confirmado lendo `Kill()`, `method_2()` e `method_9()` inteiros — nenhum toca `_rigidBody`/`ienumerator_0`). Isso significa que, mesmo em comportamento vanilla, se um jogador loota um item **antes** dele acomodar (Rigidbody ainda ativo), o objeto pode ir pro pool sem esse Rigidbody ter sido explicitamente destruído — não é um problema introduzido por este item, mas nosso patch torna a JANELA de "Rigidbody ainda vivo quando lootado" muito maior (de poucos segundos pra potencialmente a raid inteira), tornando esse caminho comum em vez de raro. Por isso o patch em `Kill()` (§1, ponto 2) existe especificamente pra fechar esse buraco — sem ele, este item teria um risco real de vazamento de componente em objetos reciclados do pool.
- **Fika/multiplayer (resolve o `<!-- review: -->` da spec funcional):** `BodiesImpulsePatch`/`GrenadeItemsPatch` (já existentes, não modificados por este item) **não têm gate `FikaBackendUtils.IsServer`** — cada peer aplica força localmente, de forma independente, quando testemunha um impacto na sua própria cópia do item (`ObservedLootItem`, uma instância de `GameObject` por peer representando o mesmo item lógico). Como este item patcheia o MÉTODO (`StopPhysics`/`Kill`), o Harmony intercepta a chamada em **qualquer** peer que a execute — ou seja, o comportamento "preservar o Rigidbody" passa a valer de forma uniforme em todo peer da sala, sem introduzir nenhuma assimetria nova entre host e clientes além da que já existe hoje (que é pré-existente a este item, não faz parte do escopo corrigir aqui). `ObservedLootItem.ApplyNetPacket` ([ObservedLootItem.cs:43-60](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/ObservedLootItem.cs#L43-L60)) também chama `StopPhysics()` quando recebe `packet.Done == true` pela rede — como o patch intercepta o MÉTODO (não um caminho de chamada específico), esse caminho também é coberto automaticamente, sem precisar de tratamento especial.
- **Patches existentes que dependem do resultado deste item:** `BodiesImpulsePatch.cs:40-49`, `GrenadeItemsPatch.cs` (força em item) — nenhuma mudança neles é necessária; eles já usam `AddForceAtPosition`/`AddExplosionForce`, que acordam Rigidbody dormindo automaticamente (comportamento nativo da Unity).
- **`PhysicalItemsPatch.cs:29`** (reatribui o item pra camada "Deadbody" durante o assentamento) continua rodando normalmente — não interfere neste item, já que opera antes de `StopPhysics()` ser chamado. <!-- ref: PA-01-03 --> `PhysicalItemsPatch.cs:29` intercepta `IsRigidbodyDone()` (decide *quando* a corrotina considera o item pronto), enquanto este item intercepta `StopPhysics()` (decide *o que acontece* quando isso acontece) — pontos diferentes da mesma cadeia, sem sobreposição ou conflito confirmado.
- **Ordem de inicialização:** registrar o novo patch em `VisceralEntry.Awake()` junto aos outros patches de `ItemForce` (após `PhysicalItemsPatch`, antes ou depois não importa — sem dependência de ordem entre eles).
- <!-- ref: PA-01-02 --> **Toggle desligado no meio da raid, pra itens já tratados:** desligar "Item Physics" no meio da raid não afeta itens cujo `StopPhysics()` já rodou com a categoria ativa — a corrotina daquele item específico já foi parada (`StopCoroutine`, sem nenhum mecanismo de re-checagem futura), então não há como retroativamente destruir o `Rigidbody` já preservado dele. Esses itens continuam com o Rigidbody preservado (dormindo, custo baixo) até o fim da raid, mesmo com o toggle desligado depois — só itens NOVOS, largados após a mudança, seguem o comportamento vanilla a partir daí. Isso já é o comportamento aceito no corner case correspondente da spec funcional (`006-item-physics-sem-expirar-01-spec.md`).

## 8. Checklist de implementação

- [x] Reconfirmar `LootItem.cs:94` (`ienumerator_0`), `:56` (`_rigidBody`), `:521-534` (`StopPhysics`), `:536-548` (`Kill`) no momento do `/code-mod` — já lidos e confirmados nesta sessão via investigação direta (não recon de subagente), mas repetir a leitura antes de codar (AP-09). Reconfirmado via Grep direto em `LootItem.cs` no início do `/code-mod`: linhas batem exatamente com a spec.
- [x] Criar `LootItemPersistPhysicsPatch.cs` com as duas classes (`LootItemStopPhysicsPatch`, `LootItemKillCleanupPatch`).
- [x] Registrar os dois patches em `VisceralEntry.Awake()`.
- [x] Bump de versão (`VisceralEntry.cs` + `.csproj`).
- [x] Compilar, 0 erros.
- [ ] Validar em jogo: atirar num item largado há mais de ~30s (bem depois de acomodar) → deve reagir com força, igual a um item recém-largado.
- [ ] Validar em jogo: granada perto de item largado há mais tempo → mesmo resultado.
- [ ] Validar regressão: com "Item Physics" desligado, comportamento idêntico ao atual (item para de reagir depois de acomodar).
- [ ] Validar: lootar um item enquanto ele ainda está "dormindo com física preservada" — item some do chão normalmente, sem erro/exceção no log.
- [ ] Validar: item atingido múltiplas vezes ao longo de um período longo (não só uma vez) — cada impacto aplica força corretamente.
- [ ] Validar em raid longa/com bastante combate: sem acúmulo perceptível de lag ao longo do tempo (checagem qualitativa, sem profiler formal disponível neste repo).

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | N/A | Este item não introduz nenhum estado estático próprio (só `FieldInfo` cacheados, reutilizáveis entre raids sem problema — mesmo padrão de `KillPatch._getInventoryController`). Todo o estado tocado (`_rigidBody`/`ienumerator_0`) é por-instância de `LootItem`, e todo `LootItem` de uma raid é destruído/reciclado no fim dela pelo próprio jogo — nada pra limpar do lado do mod. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | N/A | Os dois patches reagem ao ciclo de vida de um ITEM (não a uma ação de um player específico) — rodam identicamente pra qualquer item, de qualquer origem, em qualquer peer que execute o método localmente. Não há "ação de player" pra filtrar. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | ✅ | `LootItem.StopPhysics()` ([LootItem.cs:521](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/LootItem.cs#L521)) e `LootItem.Kill()` ([LootItem.cs:536](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/LootItem.cs#L536), `public override void Kill()` — override de uma classe-base fora do escopo de `LootItem`, mas não é ele mesmo redeclarado/sobrescrito em `ObservedLootItem.cs`, único subtipo relevante aqui, confirmado por leitura completa do arquivo) — nenhum dos dois é `virtual` em `LootItem` propriamente, e `ObservedLootItem` (o único subtipo que este item toca, mesmo critério das 3 patches já existentes) não redeclara nenhum dos dois. Alvo único, sem ambiguidade de dispatch. |
| 4 | Mudança de estado via API canônica; side-effects mapeados — AP-04 | ✅ | A escrita direta em `ienumerator_0`/`_rigidBody` via reflexão é uma escolha deliberada e documentada (não um bypass acidental): replica exatamente as mesmas 2-3 linhas que o próprio `StopPhysics()`/`Kill()` já fariam, só omitindo (`StopPhysics`) ou adicionando de propósito (`Kill`) a parte da destruição do Rigidbody. Nenhum side-effect do método original é pulado além do documentado em §1. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | N/A | Todo `LootItem` (e portanto todo `Rigidbody`/corrotina tocados por este item) pertence a uma raid específica — o próprio jogo destrói/recria o mundo inteiro entre raids, sem nenhum estado deste item sobrevivendo. Ver critério "Estado entre raids" da spec funcional. |
| 6 | Semântica/defaults de ConfigEntry sem ambiguidade — AP-05 | N/A | Nenhum `ConfigEntry` novo (§3) — reaproveita "Item Physics" já existente, sem mudar semântica/default/faixa dele. |
| 7 | Reentrância: sem recursão infinita — AP-07 | ✅ | Nenhum dos dois patches chama de volta o método que ele mesmo patcheia, nem invoca `MethodInfo.Invoke` sobre o alvo original. `StopCoroutine`/`Destroy` são chamadas a métodos completamente diferentes, sem caminho de retorno síncrono pro Prefix. |
| 8 | Flags/caches validados contra o contexto atual após troca — AP-08 | N/A | Nenhum cache de identidade de contexto (arma/operação/tela) é usado — os `FieldInfo` cacheados são metadados de reflexão genéricos (mesma declaração de campo pra qualquer instância de `LootItem`), não estado por-instância que possa ficar stale. |
| 9 | Patch-point reconfirmado no `.cs` do dump, não só recon — AP-09 | ✅ | Todos os pontos citados (`LootItem.cs:56,94,134,403-411,446-457,508-519,521-534,536-548`; `ObservedLootItem.cs:30-41,43-60`) foram lidos diretamente nesta sessão via ferramenta de leitura de arquivo, não vieram de recon de subagente. |
| 10 | Skill EFT como lever confirmada não-inerte — AP-10 | N/A | Este item não usa nenhuma skill do EFT como alavanca. |
| 11 | Pacote FIKA próprio conforme guia de prevenção de dessincronia — AP-11 | N/A | Este item não declara nenhum `INetSerializable`/pacote de rede novo — depende só da sincronização já existente do próprio jogo (`LootSyncStruct`/`ApplyNetPacket`, não modificados). |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-20 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-21 | Revisão `03-spec-tech-review-01.md` — 3 achados (PA-01-01/02/03), todos aceitos e aplicados: cast único no stub (§5), comportamento de toggle desligado no meio da raid documentado (§7), confirmação de não-conflito com `PhysicalItemsPatch` (§7). |

**Status:** ✅ Pronta para `/code-mod`
