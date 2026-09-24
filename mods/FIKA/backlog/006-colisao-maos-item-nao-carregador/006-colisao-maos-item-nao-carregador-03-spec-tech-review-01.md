# 006 — Trava de mãos ao equipar arma/faca/granada após ação recente (item não-carregador) · Review Técnica 01

**Mod:** FIKA
**Spec técnica revisada:** [006-colisao-maos-item-nao-carregador-02-spec-tech.md](006-colisao-maos-item-nao-carregador-02-spec-tech.md)
**Data:** 2026-09-10

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

## Memória consultada

Snapshot de `mods/FIKA/memory/sessions.md` — 2026-09-08 (Sessão 4) · pendências P-3.1 (calibração `GraceWindowSeconds`), P-3.2 (patch órfão no UIFixes) e P-4.1 (validação in-game do item 004) — nenhuma bloqueante pra esta review, todas já contextualizadas na spec técnica. Docs técnicos re-conferidos: `docs/technical/spt-antipatterns.md` (AP-11) e `docs/technical/fika-packet-desync-prevention-plan.md` (guia canônico de rede).

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🔴 Bloqueador | Causa raiz do bug ContinuousLoadAmmo não confirmada — `SetEmptyHands` não usa o mecanismo `GEventArgs17` que a correção assume | ✅ Resolvido (hipótese rejeitada — causa real é `GEventArgs10`, spec reescrita) |
| PA-01-02 | C — Erro de Lógica | 🟡 Importante | Airbag central captura só `ParseException`, não toda exceção que pode escapar de `ReadAllPackets` | ✅ Resolvido |

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

### PA-01-01 · C — Erro de Lógica · 🔴 Bloqueador · ✅ Resolvido em 2026-09-11

**Causa raiz do bug ContinuousLoadAmmo não confirmada por leitura do Assembly — `SetEmptyHands` parece não usar o pipeline `GEventArgs17` que a correção assume**

**Problema:** A spec técnica (§2, §6 "Fix 1") herda do relatório de auditoria a cadeia causal: `SPT-ContinuousLoadAmmo` chama `Player.SetEmptyHands(...)` ao iniciar o carregamento fora do inventário → isso abriria um `Begin` (`GEventArgs17`) via `Player.TryRemoveFromHands` com `movedItem = weapon` → quando `Player.TrySetLastEquippedWeapon()` roda depois, colide com esse `Begin` residual. Tracei essa cadeia diretamente no Assembly nesta review e ela **não se sustenta**:

- [`Player.cs:31704`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L31704) — `SetEmptyHands` chama `Proceed(withNetwork: true, callback)`.
- [`Player.cs:31936`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L31936) — esse overload de `Proceed` cria um `new Process<EmptyHandsController, GInterface198>(this, controllerFactory, null).method_0(...)` — **não chama `TryRemoveFromHands` nem `TrySetInHands`**.
- [`Player.cs:31676-31688`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L31676-L31688) — `Process<>.Execute()` (linha 22514-22537 da classe `Process<TController,TResult>`) chama `Player_0.DestroyController()` pra remover o item das mãos antigo. `DestroyController()` faz `HandsController.Destroy()` + remove eventos `GEventArgs10` (um tipo de evento **diferente**) + `UnityEngine.Object.Destroy(HandsController)` — **em nenhum momento chama `RaiseInOutProcessEvents` nem constrói um `GEventArgs17`**.
- [`Player.cs:31800`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L31800) — `TrySetLastEquippedWeapon` chama `TryProceed(LastEquippedWeaponOrKnifeItem, ...)`, que por sua vez (linha 32100) chama `Proceed(weapon, callback)` — **o mesmo pipeline `Process<FirearmController,...>`**, não `TrySetInHands`/`method_33`.
- Ou seja: `Player.TryRemoveFromHands`/`Player.TrySetInHands` (os dois métodos que `InOutHandsProcessTimestampPatch` correlaciona, e que os itens 003/004 validaram in-game) parecem ser usados por um caminho **diferente** — drag-and-drop de item aninhado dentro da arma empunhada (`TraderControllerClass.InProcess`/`ObservedInventoryController.HandleInProcess`, item 003) — não pelo caminho de troca de controller completo (`Proceed`/`TryProceed`/`Process<>`) que `SetEmptyHands`/`TrySetLastEquippedWeapon` usam.
- Não fechei 100% a investigação: `Process<>.Execute()` chama `Player_0.DropCurrentController(callback, fastDrop, item)` **antes** de `DestroyController()` ([`Player.cs:31690`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L31690), que delega pra `HandsController.Drop(...)`) — não tracei o corpo de `Drop()` na controller concreta (`FirearmController`/`AbstractHandsController`), que **poderia** ser onde um `GEventArgs17` realmente é levantado por um caminho que eu não segui. Também não descartei que o `GEventArgs7` do `LoadMagazine` (ver Partition A do relatório de auditoria) colida por outro bloco de `CheckItemAction` que não o `inOutHandsProcess` — mas os campos de `GEventArgs7` (`_sourceAmmo`, `_magazine`, [`BaseInventoryController.cs:247`](../modded/Fika-Plugin/Fika.Core/Main/BaseClasses/BaseInventoryController.cs#L247)) não referenciam a arma, então esse bloco especificamente não parece ser a causa.

**Por que importa:** se a hipótese do relatório de auditoria estiver errada, a correção proposta em §5.1 da spec técnica (generalizar `IsSelfReferentialHandsTransition`) **não resolve o bug relatado pelo usuário** — corrige uma lacuna real e defensável (item ≠ carregador reequipando arma dentro da janela de graça, cenário confirmado pela Partição C pra cura→faca/granada/troca de arma), mas pode não tocar no caminho que o `ContinuousLoadAmmo` realmente usa. Implementar e fechar este item sem confirmar isso arrisca reportar "corrigido" pro usuário e o sintoma continuar aparecendo.

**Sugestão:** antes do `/code-mod`, resolver esta incerteza por um dos dois caminhos (o primeiro é mais rápido e decisivo):
1. **Validação empírica dirigida** — `ObservedInventoryController.CheckItemAction` já tem log `#if DEBUG` no bloco `inOutHandsProcess` (linhas 176-178: `"{item} failed inOutHandsProcess check..."`). Compilar `Fika.Core` em `Debug`, reproduzir o bug (carregar munição continuamente → tentar equipar arma) numa raid Headless real, e conferir: (a) se esse log aparece no momento da falha (confirma que é ESTE bloco, não outro `flag = true` do laço de `CheckItemAction`), e (b) qual o nome do item logado. Se o log **não** aparecer, a causa é outro bloco (`GEventArgs7`/`GEventArgs8`/`GEventArgs2`/`GEventArgs3`/`item == geventArgs2.Item` genérico) e a correção de §5.1 precisa mudar de alvo.
2. **Continuar o rastreio estático** — ler `AbstractHandsController.Drop()`/`FirearmController.Drop()` (chamado por `DropCurrentController`, `Player.cs:31690-31693`) pra confirmar ou descartar se é ali que um `GEventArgs17` é levantado nesse fluxo.
Enquanto isso não for resolvido, tratar a correção de §5.1 como "generalização defensável por si só" (fecha a lacuna cura→faca/granada/arma da Partição C) mas **não confirmada** como o fix do sintoma relatado — documentar essa distinção explicitamente na spec técnica e no as-build.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[x]` Caminho alternativo: usuário optou pela **opção 1 (validação empírica dirigida)** — compilar `Fika.Core` em modo `Debug`, reproduzir o bug do `ContinuousLoadAmmo` numa raid Headless real, e conferir se o log `#if DEBUG` de `ObservedInventoryController.cs:176-178` aparece no momento da falha (confirma o bloco `inOutHandsProcess`) ou não (indica outro bloco de `CheckItemAction`, exigindo reabrir esta spec técnica). **Item pausado até o usuário conseguir rodar esse teste em raid** — `/code-mod` não deve prosseguir enquanto este ponto estiver pendente.

**✅ Resolvido em 2026-09-11 — hipótese REJEITADA por evidência empírica direta.**

**Resolução:** Usuário compilou `Fika.Core` em Debug (build local em `mods/FIKA/builds/debug-item006/`, CRC32 replicado nos dois lados — cliente e Headless — pra passar da checagem `FikaBackendUtils.cs:205`), reproduziu o bug em raid Headless real municiando vários carregadores em sequência via `SPT-ContinuousLoadAmmo`, e capturou o log do Headless no momento exato da rejeição:

```
[Error : Fika.Core] [CheckItemAction]: item was same as GEventArgs2.Item
[Error : Fika.Core] [CheckItemAction]: Flag hit, gevent was GEventArgs10
```

O log `"failed inOutHandsProcess check"` (o que confirmaria a hipótese original) **não apareceu nenhuma vez**. O que disparou foi o bloco genérico `if (item == geventArgs2.Item)` ([`ObservedInventoryController.cs:182-188`](../../modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs#L182)) contra um evento `GEventArgs10` pendente em `List_0` — **mecanismo completamente diferente** de `GEventArgs17`/`inOutHandsProcess`, que nem é tocado por esse bloco.

Rastreamento do mecanismo real (`Player.cs`, lido nesta sessão):

- `GEventArgs10` é levantado por `Class1312.vmethod_0()`/`vmethod_1()` ([`Player.cs:22234-22243`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L22234)) — `RaiseEvent(new GEventArgs10(item, status, controller))`, não `RaiseInOutProcessEvents`.
- `Class1312` é criado e **executado imediatamente** (`Execute()` chamado dentro do próprio construtor) por `Player.method_138(Item item)` ([`Player.cs:32383-32393`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L32383)) — comentário do próprio jogo no fallback: `"Invalid BeginRemoveFromHands operation args"`.
- `FirearmController.Drop(...)` ([`Player.cs:13506-13524`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L13506)) chama `_player.method_138(Item)` **antes** de iniciar a animação de esconder a arma (`CurrentOperation.HideWeapon(...)`), e só chama `inventoryOperation.Confirm()` **dentro do callback de conclusão** dessa animação — ou seja, existe uma janela real, do tamanho da animação de "guardar arma", em que a arma tem um `GEventArgs10` `Begin` pendente em `List_0`.
- `SetEmptyHands(...)` (usado pelo `SPT-ContinuousLoadAmmo` pra tirar a arma da mão antes de carregar munição fora do inventário) passa pelo pipeline `Proceed`/`Process<>`/`DropCurrentController` → `HandsController.Drop(...)` → cai exatamente nesse `FirearmController.Drop`. Isso bate com a suspeita já levantada nesta review (linha 50 acima: "`DropCurrentController` chama `HandsController.Drop(...)`, que **poderia** ser onde um evento realmente é levantado por um caminho que eu não segui") — **confirmado**: é aqui, só que o evento é `GEventArgs10`, não `GEventArgs17`.

**Conclusão:** a correção planejada em §5.1 da spec técnica (generalizar `IsSelfReferentialHandsTransition`) **não teria corrigido o bug relatado** — ela só age dentro do bloco `lambda.inOutHandsProcess != null`, que este cenário nunca alcança (o `flag = true` já disparou antes, no bloco genérico `item == geventArgs2.Item`). A spec técnica precisa ser **reescrita** mirando o mecanismo real (`GEventArgs10`/`method_138`/`Class1312`/`FirearmController.Drop`), não ampliada. Achado promovido de "hipótese plausível" pra "causa raiz confirmada por reprodução + log direcionado".

---

### PA-01-02 · C — Erro de Lógica · 🟡 Importante · ✅ Resolvido em 2026-09-11

**Airbag central (`TryReadAllPackets`) captura só `ParseException` — não fecha "causa raiz 4" por completo, como o achado `AUD-01-02` e a spec técnica afirmam**

**Problema:** O stub em §5.2/§5.3 da spec técnica usa `catch (ParseException ex)`. Conferido em [`NetPacketProcessor.cs:135-155`](../modded/Fika-Plugin/Fika.Core/Networking/LiteNetLib/Utils/NetPacketProcessor.cs#L135-L155): `ReadAllPackets`/`ReadPacket` não têm try/catch interno nenhum — qualquer exceção que escape de **qualquer ponto** do processamento de um pacote (não só o hash desconhecido de `GetCallbackFromData`, mas também um `Deserialize` mal-comportado de terceiro usando `Get*` cru em vez de `TryGet*`, que lança em payload truncado — exatamente a "causa raiz 5" do próprio guia canônico citado na spec, `fika-packet-desync-prevention-plan.md` §2) propaga do mesmo jeito. `catch (ParseException ex)` só protege contra as causas 1-3 do guia (hash nunca registrado); não protege contra a causa 5 (assimetria Serialize/Deserialize de um pacote de terceiro mal implementado, ex.: os "não conformes" já catalogados no guia §6 — `Skills-Extended`, `TrueTrauma`, `Band-Aid`).

**Por que importa:** a spec técnica (checklist de conformidade, check 11) afirma que este item "implementa exatamente o requisito de airbag com throttle... fechando a causa raiz 4 que nenhum airbag por-mod cobre" — essa afirmação é mais forte do que o código proposto entrega. Se um mod de terceiro mal implementado lançar, por exemplo, `IndexOutOfRangeException` dentro do próprio `Deserialize` (payload truncado, `Get*` cru), o `catch (ParseException ex)` não captura, a exceção escapa do mesmo jeito, e o mesmo sintoma (fila do frame descartada) continua ocorrendo — só que agora com uma falsa sensação de que "o airbag central já cobre isso".

**Sugestão:** trocar `catch (ParseException ex)` por `catch (Exception ex)` em `TryReadAllPackets` (`FikaClient.cs` e `FikaServer.cs`, stub §5.2/§5.3), mantendo o mesmo throttle. Ajustar o comentário `// ref: AUD-01-02` pra deixar explícito que a captura é ampla de propósito (cobre tanto hash desconhecido quanto `Deserialize` malformado de terceiro), e atualizar a evidência do check 11 na §9 da spec técnica pra citar `catch (Exception ex)`, não `ParseException`.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Aplicado na reescrita da spec técnica (§5.4/5.5) — `TryReadAllPackets` agora usa `catch (Exception ex)` em vez de `catch (ParseException ex)`, com comentário explícito citando a causa raiz 5 do guia canônico.
