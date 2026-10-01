# 007 — Impulso de Tiro Menos Dependente do Peso do Item · Spec Técnica

**Mod:** VisceralCombat
**Spec funcional:** [007-piso-impulso-tiro-itens-01-spec.md](007-piso-impulso-tiro-itens-01-spec.md)
**Criado:** 2026-09-21

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

## 1. Estratégia

Este item **não introduz nem modifica nenhum ponto de patch Harmony**. `BodiesImpulsePatch` já intercepta `BallisticsCalculator.Shoot(EftBulletClass)` (Postfix existente, [BodiesImpulsePatch.cs:16-26](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs#L16-L26)) e delega o cálculo de força pro método auxiliar `ProcessImpulse` (mesmo arquivo, [linhas 28-49](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs#L28-L49)) — ambos código do MOD, não do EFT. A mudança proposta é inteiramente uma edição de lógica dentro do ramo de item largado desse método já existente (linhas 40-49), sem tocar no alvo do patch em si.

**Causa raiz confirmada:** `physicalImpulse` é calculado como momento físico do projétil (massa × velocidade × 0.25, [BodiesImpulsePatch.cs:34-37](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs#L34-L37), usando `EftBulletClass.BulletMassGram` [EftBulletClass.cs:76](../../../../references/eft-decompiled/Assembly-CSharp/EftBulletClass.cs#L76) e `EftBulletClass.Speed` [EftBulletClass.cs:68](../../../../references/eft-decompiled/Assembly-CSharp/EftBulletClass.cs#L68)) e aplicado via `Rigidbody.AddForceAtPosition(..., ForceMode.Impulse)` ([BodiesImpulsePatch.cs:46](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs#L46)). `ForceMode.Impulse` é uma API padrão da Unity que converte o impulso em mudança de velocidade dividindo pela massa do `Rigidbody` alvo (`Δv = impulso / massa`) — e essa massa é o peso real do item cadastrado no jogo (`_rigidBody.mass = item_0.TotalWeight`, [LootItem.cs:334](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/LootItem.cs#L334)). Itens leves (~0.1kg) recebem `Δv` grande; itens pesados (~1.5-3.5kg) recebem `Δv` pequeno com o mesmo impulso.

**Estratégia escolhida:** aplicar um **teto e um piso na massa efetiva** usada só pra essa conversão impulso→velocidade — `effectiveMass = Mathf.Clamp(rigidbodyReal.mass, MinMassKg, MassCapKg)`. Itens com massa real **entre o piso e o teto** usam a massa real (comportamento idêntico ao de hoje, sem regressão); itens com massa real **acima do teto** são tratados, só pra este cálculo, como se pesassem `MassCapKg` — dando um piso de reação proporcional ao mesmo peso "leve" de referência, sem depender de quão pesado o item realmente é. O piso (`MinMassKg`, ref: PA-01-01) existe só como proteção defensiva pra itens com peso cadastrado zero/quase-zero (chave, documento, item de quest) — sem ele, a divisão manual por uma massa próxima de zero poderia gerar `Infinity`/`NaN` e quebrar a física daquele item especificamente; não afeta nenhum item com peso realista. Como a divisão por massa passa a ser feita manualmente (`physicalImpulse / effectiveMass`), a aplicação troca de `ForceMode.Impulse` (que dividiria de novo pela massa REAL) para `ForceMode.VelocityChange` (aplica a mudança de velocidade calculada diretamente, sem dividir de novo). **Limitação aceita (ref: PA-01-02):** essa correção equaliza só a componente LINEAR (deslocamento) da reação — o torque/rotação aplicado por `AddForceAtPosition` continua proporcional ao tensor de inércia real do item, não à `effectiveMass`; um item pesado desloca de forma comparável a um item leve, mas gira/tomba visivelmente menos, o que é esperado e não uma regressão.

**Alternativas descartadas:**
- *Escala não-linear/curva (ex.: raiz quadrada da massa)* — descartada por adicionar superfície de ajuste (expoente) sem ganho claro sobre um teto simples; um teto único já satisfaz os dois critérios de aceite (sem regressão em item leve, piso perceptível em item pesado) com uma única constante fácil de calibrar em jogo.
- *Remover completamente a dependência de massa (velocidade fixa pra todo item, independente do peso)* — descartada porque isso alteraria também o comportamento de itens leves (que hoje já reagem bem), violando o critério "sem regressão" da spec funcional; o teto preserva o comportamento atual pra qualquer item abaixo dele.

## 2. Pontos de patch

**Nenhum ponto de patch novo ou modificado.** O patch Harmony existente (`BallisticsCalculator.Shoot`, alvo de `BodiesImpulsePatch`) permanece inalterado — só a lógica interna do método auxiliar do mod (`ProcessImpulse`) muda.

| Referência (Assembly) | Uso |
|---|---|
| [`LootItem.cs:334`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/LootItem.cs#L334) | Confirma que a massa do `Rigidbody` do item é o peso real cadastrado (`item_0.TotalWeight`) — base da causa raiz. |
| [`EftBulletClass.cs:64`](../../../../references/eft-decompiled/Assembly-CSharp/EftBulletClass.cs#L64) (`Direction`), [`:68`](../../../../references/eft-decompiled/Assembly-CSharp/EftBulletClass.cs#L68) (`Speed`), [`:76`](../../../../references/eft-decompiled/Assembly-CSharp/EftBulletClass.cs#L76) (`BulletMassGram`) | Campos já lidos por `ProcessImpulse` hoje — sem mudança no que é lido, só como esses valores são convertidos em velocidade no final. |
| [`EftBulletClass.cs:226`](../../../../references/eft-decompiled/Assembly-CSharp/EftBulletClass.cs#L226) (`HitPoint`, virtual), [`:230`](../../../../references/eft-decompiled/Assembly-CSharp/EftBulletClass.cs#L230) (`HitCollider`, virtual) | Propriedades virtuais lidas via instância já resolvida (`shot`) — não são alvo de patch, então a natureza virtual delas não é uma preocupação AP-03 aqui (é despacho polimórfico normal, não interceptação). |

## 3. Novas propriedades F12 (BepInEx)

Nenhuma — reaproveita "Item Force Intensity" (`objectIntensity`, já existente, [VisceralEntry.cs:327](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat/VisceralEntry.cs#L327)) como único knob de ajuste do jogador, sem mudar seção, chave ou tooltip. `MassCapKg` fica como constante no código (decisão deliberada de simplicidade, ver §7).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs` | MODIFICAR | Bloco do item largado (linhas ~40-49 atuais) passa a calcular `effectiveMass`/`deltaV` com teto de massa, e troca `ForceMode.Impulse` por `ForceMode.VelocityChange`. Nenhuma outra parte do arquivo muda. |

## 5. Stubs de código

```csharp
// modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs
// Trecho substituído — bloco do item largado dentro de ProcessImpulse (linhas ~40-49 atuais).
// O restante do método (cálculo de physicalImpulse, ramo de corpo/ragdoll após o `return`) NÃO muda.

// Teto e piso de massa pra conversão impulso→velocidade: itens mais pesados que MassCapKg são
// tratados, só pra esta conta, como se pesassem MassCapKg — dá um piso de reação perceptível pra
// itens pesados (arma, capacete) sem alterar em nada a reação de itens já mais leves que o teto
// (máscara, óculos, fone continuam usando a massa real, comportamento idêntico ao de hoje).
// MinMassKg protege contra item com peso zero/quase-zero cadastrado (ex.: chave, documento,
// item de quest) — sem ele, a divisão manual abaixo poderia gerar Infinity/NaN e quebrar a
// física daquele item (ref: PA-01-01). Qualquer item com peso realista (> 0.05kg) não é afetado
// pelo piso, só pelo teto — mesmo comportamento de antes pra esses casos.
private const float MassCapKg = 0.5f; // ref: item 007 — calibrar em jogo se necessário
private const float MinMassKg = 0.05f; // ref: PA-01-01 — piso de segurança, não deve afetar item real

// Check if hitting dropped loot item
Rigidbody lootRb = hitCollider.attachedRigidbody ?? hitCollider.GetComponentInParent<Rigidbody>();
if (lootRb != null && lootRb.gameObject.GetComponent<ObservedLootItem>() != null)
{
	if (VisceralEntry.Instance != null && VisceralEntry.Instance.IsCategoryActive(VisceralEntry.Instance.ItemForce)) // ref: item 005
	{
		physicalImpulse *= VisceralEntry.Instance.objectIntensity.Value;

		// ref: item 007 — teto/piso de massa efetiva. lootRb.mass é o peso real do item
		// (LootItem.cs:334 — _rigidBody.mass = item_0.TotalWeight). Clamp() garante que itens
		// ACIMA do teto usem o teto, itens com peso ~zero usem o piso (PA-01-01), e qualquer
		// item com peso realista entre os dois use a massa real, sem mudança de comportamento.
		float effectiveMass = Mathf.Clamp(lootRb.mass, MinMassKg, MassCapKg);
		// Nota (PA-01-02): esta correção equaliza só a componente LINEAR (deslocamento) da
		// reação. O torque/rotação aplicado por AddForceAtPosition continua proporcional ao
		// tensor de inércia REAL do item (não a effectiveMass) — um item pesado desloca de
		// forma comparável a um item leve, mas gira/tomba visivelmente menos. Isso é esperado,
		// não é uma regressão nem um bug residual — ver §7.
		Vector3 deltaV = shot.Direction * (physicalImpulse / effectiveMass);

		// ForceMode.VelocityChange NÃO divide de novo pela massa real — já fizemos a divisão
		// manualmente acima com a massa "grampeada". Usar ForceMode.Impulse aqui cancelaria
		// o efeito do teto (voltaria a dividir pela massa real cheia).
		lootRb.AddForceAtPosition(deltaV, shot.HitPoint, ForceMode.VelocityChange);
	}
	return;
}
```

## 6. Fluxo de dados

```
[A] Bala atinge o collider de um item largado (EftBulletClass.HitCollider, EftBulletClass.cs:230)
  → [B] BodiesImpulsePatch.ProcessImpulse calcula physicalImpulse a partir de massa/velocidade
    do projétil (EftBulletClass.cs:76,68) — BodiesImpulsePatch.cs:34-37, inalterado
  → [C] Identifica que o collider pertence a um item largado (ObservedLootItem no mesmo
    GameObject) — BodiesImpulsePatch.cs:40-41, inalterado
  → [D] NOVO: em vez de aplicar physicalImpulse direto via ForceMode.Impulse (que dividiria
    pela massa real do item, LootItem.cs:334), calcula effectiveMass = Min(massa real,
    MassCapKg) e deltaV = direção × (physicalImpulse / effectiveMass)
  → [E] Aplica deltaV via AddForceAtPosition(..., ForceMode.VelocityChange) — Unity soma essa
    mudança de velocidade diretamente ao Rigidbody, sem dividir de novo pela massa
  → [F] Item reage com magnitude comparável independente do peso real (se estiver acima do
    teto) ou idêntica a hoje (se estiver abaixo do teto)
```

## 7. Riscos e dependências

- **Isolamento do ramo de corpo/ragdoll (preocupação levantada na spec funcional):** `physicalImpulse` é uma variável compartilhada, calculada uma vez ([BodiesImpulsePatch.cs:37](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs#L37)) e mutada in-place dentro do bloco do item (`physicalImpulse *= objectIntensity.Value`, linha 45) — mas o método retorna (`return;`, linha 48) logo em seguida. O ramo de corpo/ragdoll (código após esse `return`, usado só quando o collider atingido NÃO pertence a um item largado) é mutuamente exclusivo com o ramo de item pro mesmo tiro — a mutação nunca alcança o ramo de corpo na mesma execução. Confirmado por leitura completa do método atual. As novas variáveis (`effectiveMass`, `deltaV`) são inteiramente locais ao bloco do item, declaradas depois da mutação e usadas só antes do `return` — zero risco de regressão no ramo de corpo (força de tiro em ragdoll, calibrada separadamente por "Bullet Intensity").
- **`MassCapKg`/`MinMassKg` como constantes hardcoded, não `ConfigEntry`:** decisão deliberada de simplicidade (YAGNI) — "Item Force Intensity" já existente continua sendo o único knob de ajuste do jogador pra intensidade geral, igual hoje. Se os valores propostos (`0.5f`/`0.05f`) não se mostrarem adequados em teste de jogo, é um ajuste de uma linha, não uma mudança estrutural.
- <!-- ref: PA-01-01 --> **Divisão por massa efetiva sem piso mínimo (risco corrigido):** a divisão manual `physicalImpulse / effectiveMass` não tem a proteção interna que a Unity aplica normalmente em `ForceMode.Impulse`. Sem um piso mínimo, um item com peso cadastrado zero/quase-zero (chave, documento, item de quest — cenário plausível no Tarkov) geraria `effectiveMass` próximo de zero, produzindo `Infinity`/`NaN` em `deltaV` e corrompendo a física daquele `Rigidbody` (item podendo desaparecer, "explodir" pra posição inválida, ou travar fisicamente pro resto da raid). Resolvido adicionando `MinMassKg = 0.05f` ao `Clamp()` (§5) — protege contra esse caso sem afetar nenhum item de peso realista.
- <!-- ref: PA-01-02 --> **Torque/rotação continuam escalando com a massa real, não com `effectiveMass`:** `AddForceAtPosition` aplica torque proporcional ao deslocamento do ponto de impacto em relação ao centro de massa, convertido em velocidade angular via o tensor de inércia REAL do `Rigidbody` — isso não é afetado pela mudança de `ForceMode` nem pelo `effectiveMass` grampeado (que só entra no cálculo da componente linear, feito manualmente em script). Consequência esperada: um item pesado desloca de forma comparável a um item leve depois deste fix, mas gira/tomba visivelmente menos que um item leve atingido do mesmo jeito — isso não é uma regressão nem um bug residual, é uma limitação de escopo aceita (só a componente linear/deslocamento está coberta pelos critérios de aceite da spec funcional). Documentado aqui pra não ser confundido com "fix incompleto" numa validação futura.
- **Compatibilidade com item `006`** (Rigidbody preservado/"dormindo" em itens acomodados): a mudança só afeta COMO o impulso é convertido em velocidade, não SE o item ainda tem `Rigidbody` — funciona igual em item recém-largado ou já "dormindo" há muito tempo (mesmo `Rigidbody`, mesma API `AddForceAtPosition`, só trocando o `ForceMode` e pré-calculando a divisão). Nenhuma interação nova com `LootItemStopPhysicsPatch`/`LootItemKillCleanupPatch`.
- **Sem interação com força de granada:** `GrenadeItemsPatch` usa uma função de origem completamente separada (`Grenade.Explosion`, não `BallisticsCalculator.Shoot`) e não é tocado por este item.
- **Torque/rotação inalterados:** `AddForceAtPosition` também aplica torque proporcional ao deslocamento do ponto de impacto em relação ao centro de massa — esse comportamento não muda (mesma API, mesmo ponto de aplicação), só a magnitude linear calculada é diferente.
- **Ordem de inicialização:** nenhuma — não há registro de patch novo, `MassCapKg` é uma constante compilada, sem dependência de ordem de `Awake()`.

## 8. Checklist de implementação

- [x] Reconfirmar `LootItem.cs:334` e `EftBulletClass.cs:64,68,76,226,230` no momento do `/code-mod` (já lidos e confirmados nesta sessão via leitura direta, repetir antes de codar — AP-09).
- [x] Editar `BodiesImpulsePatch.cs` — bloco do item largado: adicionar constantes `MassCapKg`/`MinMassKg`, calcular `effectiveMass` via `Clamp` (não só `Min`)/`deltaV`, trocar `ForceMode.Impulse` por `ForceMode.VelocityChange`.
- [x] Confirmar por leitura que o ramo de corpo/ragdoll (código após o `return;`) permanece byte-a-byte inalterado.
- [x] Bump de versão (`VisceralEntry.cs` + `.csproj`).
- [x] Compilar, 0 erros.
- [ ] Validar em jogo: atirar num item pesado (arma ou capacete) largado, há mais de alguns segundos — reação perceptível, comparável em ordem de grandeza à de um item leve hoje.
- [ ] Validar em jogo: atirar num item leve (máscara, óculos, fone) — sem regressão nem exagero em relação ao comportamento atual.
- [ ] Validar regressão: força de granada em itens permanece idêntica (fora de escopo, código não tocado).
- [ ] Validar regressão: força de tiro em corpos/ragdoll permanece idêntica ("Bullet Intensity", ramo de código não tocado).
- [ ] Validar: item atingido por múltiplos tiros seguidos — cada tiro aplica reação de forma consistente.
- [ ] Com "Item Physics" desligado, comportamento permanece idêntico ao atual (branch já gateado por `IsCategoryActive`, inalterado).
- [ ] Validar, se possível, algum item de peso muito baixo (chave, documento) — confirma que o piso `MinMassKg` evita comportamento absurdo (PA-01-01).
- [ ] Confirmar visualmente: item pesado desloca perceptivelmente mesmo girando menos que um item leve no mesmo tiro — comportamento esperado, não é bug (PA-01-02).

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | N/A | Nenhum estado estático novo; `MassCapKg` é constante compile-time, sem nada a limpar entre raids. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | N/A | Já coberto pelo patch existente (`BodiesImpulsePatch`, não modificado por este item) — este item só ajusta a fórmula dentro de um bloco já existente, sem introduzir nova dependência de player/peer. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | N/A | Este item não introduz nem modifica nenhum ponto de patch Harmony — é uma edição de lógica interna do mod, dentro de um método já patcheado (ver §2). |
| 4 | Mudança de estado via API canônica; side-effects mapeados — AP-04 | ✅ | Troca de `ForceMode.Impulse` por `ForceMode.VelocityChange` é uso de uma API pública padrão da Unity (`Rigidbody.AddForceAtPosition`), mesmo método já usado hoje, só mudando o parâmetro de modo — nenhum side-effect novo (torque inalterado, ver §7). |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | N/A | Sem estado persistente — mesmo raciocínio do check 1. Ver critério "Estado entre raids" da spec funcional. |
| 6 | Semântica/defaults de ConfigEntry sem ambiguidade — AP-05 | N/A | Nenhum `ConfigEntry` novo (§3). |
| 7 | Reentrância: sem recursão infinita — AP-07 | N/A | Nenhuma invocação ao método patcheado a partir do próprio patch; edição é só lógica aritmética interna. |
| 8 | Flags/caches validados contra o contexto atual após troca — AP-08 | N/A | `effectiveMass`/`deltaV` são variáveis locais recalculadas a cada chamada, sem cache entre chamadas ou contexto que possa ficar stale. |
| 9 | Patch-point reconfirmado no `.cs` do dump, não só recon — AP-09 | ✅ | `LootItem.cs:334`, `EftBulletClass.cs:64,68,76,226,230` confirmados por leitura direta nesta sessão (Grep + Read, não recon de subagente). |
| 10 | Skill EFT como lever confirmada não-inerte — AP-10 | N/A | Este item não usa nenhuma skill do EFT como alavanca. |
| 11 | Pacote FIKA próprio conforme guia de prevenção de dessincronia — AP-11 | N/A | Nenhum `INetSerializable`/pacote de rede novo — comportamento é aplicação de física local por peer, mesmo padrão já existente e documentado no item `006` (cada peer aplica força na própria cópia observada do item, sem sincronização adicional). |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-21 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-21 | Revisão `03-spec-tech-review-01.md` — 2 achados (PA-01-01/02), ambos aceitos e aplicados: piso mínimo de massa (`MinMassKg`) adicionado ao `Clamp()` (§1, §5, §7); limitação de rotação/torque não corrigida documentada como comportamento esperado (§1, §5, §7, §8). |

**Status:** ✅ Pronta para `/code-mod`
