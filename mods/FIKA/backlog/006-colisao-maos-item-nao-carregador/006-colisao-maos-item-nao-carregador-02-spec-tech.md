# 006 — Trava de mãos ao equipar arma/faca/granada após ação recente (item não-carregador) · Spec Técnica

**Mod:** FIKA
**Spec funcional:** [006-colisao-maos-item-nao-carregador-01-spec.md](006-colisao-maos-item-nao-carregador-01-spec.md)
**Criado:** 2026-09-10

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT cita `arquivo.cs:linha`, reconfirmada por leitura direta nesta sessão (não herdada só do relatório de auditoria).

## ⚠️ Restrição crítica — superfície pública do FIKA

`mods/FIKA/modded/` é consumido como biblioteca por dezenas de mods **já presentes neste repositório e por qualquer mod externo que venha a ser instalado no futuro** — a lista de dependentes mapeada por `grep` neste repo (~20 mods) é só uma amostra do risco conhecido, não o universo de risco real. Nenhuma das três correções deste item altera nome, assinatura ou modificador de acesso de nenhum membro público. Duas delas (fix de `IsSelfReferentialHandsTransition` e o airbag de pacote) tocam código atrás de um método público **já existente e herdado do EFT** (`CheckItemAction`, `override` de `TraderControllerClass`) — não criam superfície nova, mas mudam **comportamento observável** desse método (uma operação antes rejeitada passa a ser aceita num caso específico). Ver §7 "Riscos e dependências" para o detalhamento desse risco residual não-relacionado a assinatura.

## 1. Estratégia

Três correções independentes, nenhuma delas usando Harmony (nenhum novo `ModulePatch`) — são edições diretas em código já pertencente ao nosso fork:

1. **`IsSelfReferentialHandsTransition`** (`ObservedInventoryController.cs:218-237`) — remover a restrição `item is not MagazineItemClass` da guarda de entrada, mantendo o bloqueio real (`item == weapon && movedItem == weapon`, saque genuíno da mesma arma). É uma mudança de **lógica de guarda**, não de patch — o método já existe, já é chamado por `CheckItemAction` (linha 174), só a condição interna muda.
2. **Airbag central de pacote desconhecido** — novo método privado `TryReadAllPackets` em `FikaClient.cs` e `FikaServer.cs` (espelhado, não compartilhado — essas duas classes não têm base comum além de `IFikaNetworkManager`, então duplicar ~10 linhas é mais simples e menos arriscado do que introduzir uma abstração nova só para isso). Descartada a alternativa de colocar o try/catch dentro do próprio `NetPacketProcessor.cs` (`Fika.Core.Networking.LiteNetLib.Utils`): esse arquivo é vendorizado (0 linhas de diff contra `references/fika-plugin/` e `original/`) e acoplar `FikaGlobals`/log específico do FIKA nele degradaria a rastreabilidade de upstream — o wrapper fica no lado FIKA (`FikaClient`/`FikaServer`), que já tem `_logger` próprio.
3. **Logging nos dois `catch (Exception) { }` vazios** de `ClientInventoryOperationHandler.cs` (linhas 63-65 e 114-116) — trocar por `catch (Exception ex) { FikaGlobals.LogError(...) }`, mesmo padrão já usado no resto do arquivo (linhas 53, 71, 80, 120).

## 2. Pontos de patch

Nenhum novo Harmony `ModulePatch`. Pontos de edição direta (não são "patches" no sentido Harmony, mas seguem o mesmo rigor de citação):

| Alvo (código do mod / EFT p/ contexto) | Tipo de mudança | Motivo |
|---|---|---|
| [`ObservedInventoryController.cs:220`](../modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs#L220) | Edição de guarda condicional | Remove restrição a `MagazineItemClass`, mantém bloqueio de saque real |
| [`FikaClient.cs:500`](../modded/Fika-Plugin/Fika.Core/Networking/FikaClient.cs#L500) | Substituição de chamada (`ReadAllPackets` → `TryReadAllPackets`) + novo método privado | Airbag central com throttle (AP-11) |
| [`FikaServer.cs:946`](../modded/Fika-Plugin/Fika.Core/Networking/FikaServer.cs#L946) | Idem, espelhado | Idem, lado servidor/Headless |
| [`ClientInventoryOperationHandler.cs:63-65`](../modded/Fika-Plugin/Fika.Core/Main/ClientClasses/ClientInventoryOperationHandler.cs#L63-L65) | `catch` vazio → log | Não mascarar falha silenciosa de `RaiseRefreshEvent` |
| [`ClientInventoryOperationHandler.cs:114-116`](../modded/Fika-Plugin/Fika.Core/Main/ClientClasses/ClientInventoryOperationHandler.cs#L114-L116) | `catch` vazio → log | Não mascarar falha silenciosa de `Operation.Dispose()` |

Contexto de referência no Assembly (para provar que o bloqueio residual continua correto — não são pontos de patch, são evidência):
- [`Player.cs:32223`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L32223) — `TryRemoveFromHands`: confirma que um saque real nunca abre `Begin`/`Succeed` por esse lado (desvia via `SetControllerInsteadRemovedOne`, `:32242`).
- [`Player.cs:32294`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L32294) — `TrySetInHands`: ramo B (`:32294-32337`) é o único que levanta `Begin` com `.Item = HandsController.Item` (o item que **já** ocupa as mãos) — confirma que `item == weapon && movedItem == weapon` só ocorre por reinserção da própria arma (Fold ou reentrada da mesma referência), nunca por um saque de arma diferente.

## 3. Novas propriedades F12 (BepInEx)

N/A — este item não introduz nenhuma `ConfigEntry`.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs` | MODIFICAR | `IsSelfReferentialHandsTransition` generalizada pro lado `item` (linha 220) |
| `modded/Fika-Plugin/Fika.Core/Networking/FikaClient.cs` | MODIFICAR | Novo método privado `TryReadAllPackets` + troca do call-site em `OnNetworkReceive` |
| `modded/Fika-Plugin/Fika.Core/Networking/FikaServer.cs` | MODIFICAR | Idem, espelhado |
| `modded/Fika-Plugin/Fika.Core/Main/ClientClasses/ClientInventoryOperationHandler.cs` | MODIFICAR | 2 blocos `catch` vazios ganham log |
| `modded/Fika-Plugin/Fika.Core/FikaPlugin.cs` | MODIFICAR | Bump SemVer (seguir convenção dos itens 001-005) |
| `mod.json` | MODIFICAR | Bump do componente `plugin` em paralelo |

## 5. Stubs de código

### 5.1 `IsSelfReferentialHandsTransition` — guarda generalizada

```csharp
// modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs
// (método já existe nesta classe — mostrando o corpo completo pós-mudança, linhas 218-237 hoje)

// ref: AUD-01-01 (docs/relatorio-auditoria-codigo-01.md) — remove a restrição a MagazineItemClass
// que o item 004 não tinha tocado. Continua tolerando só transições recentes (GraceWindowSeconds)
// do MESMO jogador na MESMA arma, e continua bloqueando o único caso que deve permanecer rejeitado:
// a própria arma reentrando nas mãos enquanto o Begin pendente também foi aberto por ela mesma
// (Fold de coronha, ou saque real concorrente — ver Player.cs:32294-32337 na spec técnica §2).
private bool IsSelfReferentialHandsTransition(Item item, GEventArgs17 inOutHandsProcess)
{
    if (inOutHandsProcess?.Item is not Weapon weapon)
    {
        return false;
    }

    if (!InOutHandsProcessTimestampPatch.TryGetPendingBegin(this, weapon, out var movedItem, out var elapsed))
    {
        return false;
    }

    // TODO confirmar (P-3.1, herdada do item 003): janela de graça calibrada por
    // instrumentação temporária antes de fechar o item.
    const float GraceWindowSeconds = 0.35f;

    if (movedItem == null || elapsed > GraceWindowSeconds)
    {
        return false;
    }

    // Único cenário que deve continuar bloqueado: a própria arma reentrando nas mãos
    // enquanto o Begin pendente também foi aberto por ela mesma (Fold, ou saque real
    // concorrente — ver Player.cs:32294-32337).
    if (item == weapon && movedItem == weapon)
    {
        return false;
    }

    return true;
}
```

### 5.2 Airbag central de pacote desconhecido (`FikaClient.cs`)

```csharp
// modded/Fika-Plugin/Fika.Core/Networking/FikaClient.cs
// Novo campo privado, junto dos outros campos de instância da classe:
private int _unknownPacketCount;

// Novo método privado:

// ref: AUD-01-02 (docs/relatorio-auditoria-codigo-01.md) + AP-11 (docs/technical/spt-antipatterns.md,
// "causa raiz 4" em docs/technical/fika-packet-desync-prevention-plan.md §2) — GetCallbackFromData
// (NetPacketProcessor.cs:83-91) lança ParseException sem barreira nenhuma até aqui quando o hash do
// pacote não está registrado (mod ausente no peer, ou registrado com atraso/versão diferente). Sem
// este catch, LiteNetManager.PollEvents (LiteNetManager.cs:1436-1441) descarta TODOS os eventos de
// rede já enfileirados no mesmo frame, de todos os peers — não só o pacote ruim. Throttle de log
// segue o padrão exigido pelo guia canônico (§4 regra 4): stack completo na 1ª ocorrência, resumo
// a cada N depois, para não inundar o console num mod desatualizado enviando em alta frequência.
private void TryReadAllPackets(NetDataReader reader, object userData)
{
    try
    {
        _packetProcessor.ReadAllPackets(reader, userData);
    }
    catch (ParseException ex)
    {
        _unknownPacketCount++;
        if (_unknownPacketCount == 1 || _unknownPacketCount % 50 == 0)
        {
            _logger.LogWarning($"[CLIENT] Dropping unknown/incompatible packet (#{_unknownPacketCount} so far): {ex.Message}");
        }
    }
}

// Call-site em OnNetworkReceive (FikaClient.cs:497-501), troca de uma linha:
public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
{
    switch (reader.GetEnum<EPacketType>())
    {
        case EPacketType.Serializable:
            TryReadAllPackets(reader, peer); // era: _packetProcessor.ReadAllPackets(reader, peer);
            break;
        // ... demais cases inalterados
    }
}
```

### 5.3 Espelho em `FikaServer.cs`

Mesma forma exata de `TryReadAllPackets`, campo `_unknownPacketCount` próprio (instância separada, sem estado compartilhado entre client e server), só troca a tag de log de `"[CLIENT]"` para `"[SERVER]"` (consistente com o padrão já usado em `_logger.LogError("[SERVER] error " + ...)`, `FikaServer.cs:793`). Call-site trocado em `FikaServer.cs:945-947` (dentro do mesmo `case EPacketType.Serializable:`).

### 5.4 Logging em `ClientInventoryOperationHandler.cs`

```csharp
// modded/Fika-Plugin/Fika.Core/Main/ClientClasses/ClientInventoryOperationHandler.cs
// Linhas 58-66 hoje (bloco TRL-Fixes #2, dentro de ReceiveStatusFromServer):
if (Operation is MoveOperationClass moveOp)
{
    try
    {
        moveOp.From?.Container?.ParentItem?.RaiseRefreshEvent(true, true);
        moveOp.To?.Container?.ParentItem?.RaiseRefreshEvent(true, true);
    }
    catch (Exception ex) // era: catch (Exception)
    {
        // ref: AUD-01-04 (docs/relatorio-auditoria-codigo-01.md)
        FikaGlobals.LogError($"{InventoryController?.ID} - RaiseRefreshEvent falhou após rejeição do servidor: {ex}");
    }
}

// Linhas 106-117 hoje (dentro de HandleResult):
if (Operation != null)
{
    try
    {
        Operation.Dispose();
    }
    catch (Exception ex) // era: catch (Exception)
    {
        // ref: AUD-01-04 (docs/relatorio-auditoria-codigo-01.md)
        FikaGlobals.LogError($"{InventoryController?.ID} - Operation.Dispose() falhou: {Operation?.Id} - {ex}");
    }
}
```

## 6. Fluxo de dados

**Fix 1 — tolerância de colisão de mãos:**
```
[A] Jogador equipa arma / saca faca / arremessa granada logo após outra transição de mãos recente
  → [B] Player.TrySetLastEquippedWeapon / HandsController.Execute → Item.CheckAction (Item.cs:657/664)
  → [C] owner.CheckItemAction → ObservedInventoryController.CheckItemAction (:70-209)
  → [D] IsSelfReferentialHandsTransition (:218-237) — agora tolera item != MagazineItemClass
  → [E] GClass1568 (sucesso) em vez de GClass1561 (rejeição "is currently being modified")
```

**Fix 2 — airbag de pacote desconhecido:**
```
[A] Peer remoto envia pacote de tipo X (registrado nele, ausente/desalinhado no receptor)
  → [B] FikaClient/FikaServer.OnNetworkReceive → TryReadAllPackets
  → [C] NetPacketProcessor.ReadAllPackets → GetCallbackFromData lança ParseException
  → [D] catch loga (com throttle) e retorna — não propaga
  → [E] LiteNetManager.PollEvents continua processando os demais eventos do mesmo lote/frame normalmente
```

## 7. Riscos e dependências

- **Patches existentes:** nenhum Harmony `ModulePatch` novo. `InOutHandsProcessTimestampPatch` (item 003) não é alterado por este item — só o consumidor (`IsSelfReferentialHandsTransition`) muda.
- **Mudança de comportamento observável em método público herdado (não é quebra de assinatura, mas é quebra de contrato comportamental potencial):** `CheckItemAction` é `public override` de `TraderControllerClass`/`Player.PlayerInventoryController` — já existia, sua assinatura não muda. Mas seu **comportamento** muda: uma operação que hoje é rejeitada (`GClass1561`) numa janela de ~0.35s após outra transição de mãos na mesma arma passa a ser aceita, desde que não seja a própria arma reentrando (`item == weapon && movedItem == weapon`). Isso é o efeito **desejado** para o cenário reportado, mas **qualquer mod externo (presente no repo ou não) que chame `CheckItemAction` diretamente numa `ObservedInventoryController` e dependa da rejeição estrita anterior** veria esse comportamento mudar. Nenhum mod deste repo foi encontrado fazendo isso (`grep` não achou chamador externo a `CheckItemAction` em `mods/*/modded/`), mas como não há como garantir isso para mods desconhecidos, **isso é sinalizado aqui para decisão humana explícita antes do `/code-mod`** — não é ⚠️ QUEBRA DE COMPATIBILIDADE POTENCIAL no sentido de assinatura, mas é uma mudança de contrato de comportamento que merece o mesmo nível de atenção.
- **Compatibilidade com mods relacionados:** `SPT-ContinuousLoadAmmo` (gatilho direto do bug relatado), `TRL-ImmersiveCombatMedicine` (gatilho do item 004), `UIFixes` (magazine swap, item 003) — nenhum precisa de mudança própria; todos se beneficiam da generalização sem qualquer ação de sua parte.
- **`TryReadAllPackets` não é API pública nova exposta a terceiros** — é `private` em `FikaClient`/`FikaServer`, não faz parte de `IFikaNetworkManager`. Mods externos que chamam `Singleton<IFikaNetworkManager>.Instance.SendData`/`RegisterPacket` (a API pública real de rede do FIKA) não são afetados.
- **Ordem de inicialização:** não aplicável — nenhuma das três correções introduz novo registro/`Awake`/evento de ciclo de vida.
- **Regressão de rede:** rodar `node scripts/check-packet-hashes.js` após a mudança é parte do checklist de implementação (não deve haver diferença, já que nenhum tipo `INetSerializable` novo é criado por este item — é só validação de não-regressão).

## 8. Checklist de implementação

- [ ] Editar `IsSelfReferentialHandsTransition` (`ObservedInventoryController.cs:220`) — remover a guarda `item is not MagazineItemClass`, manter e comentar o bloqueio `item == weapon && movedItem == weapon` (stub §5.1).
- [ ] Adicionar campo `_unknownPacketCount` e método `TryReadAllPackets` em `FikaClient.cs`; trocar o call-site em `OnNetworkReceive` (stub §5.2).
- [ ] Espelhar em `FikaServer.cs` com tag de log `"[SERVER]"` (stub §5.3).
- [ ] Adicionar log em `ClientInventoryOperationHandler.cs:63-65` (stub §5.4).
- [ ] Adicionar log em `ClientInventoryOperationHandler.cs:114-116` (stub §5.4).
- [ ] Rodar `node scripts/check-packet-hashes.js` — confirmar 0 colisões (não deve mudar, é validação de não-regressão).
- [ ] Bump de versão SemVer em `FikaPlugin.cs` + `mod.json` (seguir convenção dos itens 001-005).
- [ ] Compilar (`/compile-mod`, fora do ciclo de artefatos) e confirmar 0 erros/avisos nos módulos afetados (`Fika.Core`, `Fika.Headless` se aplicável).

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | N/A | Nenhuma das 3 correções introduz estado raid-scoped novo. `IsSelfReferentialHandsTransition` reaproveita o `ConditionalWeakTable` já existente do item 003 (`InOutHandsProcessTimestampPatch.cs:40`), que já é escopado por `TraderControllerClass`/raid. `_unknownPacketCount` é um contador de instância (vida do plugin, não do raid) — não precisa de teardown. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | `ObservedInventoryController` só existe para jogadores observados/Headless, nunca para `MainPlayer` local (§1 desta spec, `ObservedInventoryController.cs:16`). O airbag de pacote roda na camada de transporte, abaixo da distinção de jogador — não há ação de player pra filtrar. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | N/A | Nenhum novo Harmony patch em alvo virtual/ofuscado. |
| 4 | Mudança de estado via API canônica; side-effects mapeados — AP-04 | ✅ | Nenhuma das 3 correções escreve campo interno do EFT diretamente — `IsSelfReferentialHandsTransition` só ajusta uma condição booleana já existente; o airbag só envolve uma chamada já existente (`ReadAllPackets`) em try/catch; o logging só formata exceções já capturadas. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | Ver linha 1 — nenhum estado novo, nada a limpar entre raids. `_unknownPacketCount` sobrevive entre raids por design (é um contador cumulativo de diagnóstico, não estado de gameplay) — comportamento aceitável e documentado no critério de aceite "estado entre raids" da spec funcional. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry — AP-05 | N/A | Nenhuma `ConfigEntry` nova. |
| 7 | Reentry-guard em re-invocação de método patcheado — AP-07 | N/A | Nenhum método patcheado é re-invocado por este item. |
| 8 | Flags/caches de intercept validados contra contexto atual — AP-08 | N/A | Nenhum cache/flag de intercept novo introduzido. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | Todas as linhas citadas nesta spec (`ObservedInventoryController.cs`, `NetPacketProcessor.cs`, `FikaClient.cs`, `FikaServer.cs`, `ClientInventoryOperationHandler.cs`, `Player.cs`) foram lidas diretamente nesta sessão via `Read`/`Grep`, não herdadas só do relatório de auditoria — ver §2. |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Não aplicável, item não usa skill do EFT. |
| 11 | Pacote FIKA próprio: envelope, `TryGet*`, `Valid`, main thread, registro por instância, zero `UnregisterPacket`, airbag com throttle — AP-11 | ✅ | Este item não cria pacote `INetSerializable` novo (não se aplica envelope/`TryGet*`/`Valid`/registro), mas implementa exatamente o requisito de **"airbag com throttle"** do guia (§4 regra 4) no ponto mais central possível — antes de qualquer callback de pacote específico rodar, fechando a "causa raiz 4" (§2 do guia) que nenhum airbag por-mod cobre (hash nunca registrado não tem callback pra proteger). `node scripts/check-packet-hashes.js` faz parte do checklist de implementação (§8) para confirmar não-regressão. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-10 | Spec técnica criada via `/create-technical-spec` |
