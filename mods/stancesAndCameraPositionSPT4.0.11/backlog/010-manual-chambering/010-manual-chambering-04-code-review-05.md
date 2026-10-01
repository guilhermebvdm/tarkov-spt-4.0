# 010 — Manual Chambering & FIKA Chamber Sync · Code Review 05

**Mod:** stancesAndCameraPositionSPT4.0.11  
**Spec funcional:** [010-manual-chambering-01-spec.md](010-manual-chambering-01-spec.md)  
**Spec técnica:** [010-manual-chambering-02-spec-tech.md](010-manual-chambering-02-spec-tech.md)  
**Review anterior:** [010-manual-chambering-04-code-review-04.md](010-manual-chambering-04-code-review-04.md)  
**Data:** 2026-09-05  

> Análise crítica do código implementado em `modded-testchannel/` (v2.19.11) para separação entre a camada de UX local (manual chambering / racking do ferrolho) e sincronização autoritativa no Host (`ChamberStateSyncPacket`), viabilizando compatibilidade mista onde Host e Convidado podem ter a configuração ligada ou desligada independentemente. Referência estrita validada contra `mods/FIKA/modded/Fika-Plugin/Fika.Core` e Assembly EFT decompilado.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 1 · 🟡 Médios: 3 · 🟢 Menores: 1 · ✅ Resolvidos: 0 · Total: 5

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| **CR-05-01** | B — Bug latente | 🟠 Forte | Ausência de guardas defensivas (`HasChambers`, `Chambers.Length`, `ContainedItem` e `CylinderMagazineItemClass`) em `OnChamberStateSyncPacketReceived` | `[ ]` Pendente |
| **CR-05-02** | D — Arquitetura | 🟡 Médio | `SendChamberState` utiliza `broadcast: true` em pacote direcionado exclusivamente ao Host | `[ ]` Pendente |
| **CR-05-03** | B — Bug latente | 🟡 Médio | Host não atualiza `FirearmsAnimator.SetAmmoInChamber` ao aplicar `PopTo` autoritativo para jogador remoto | `[ ]` Pendente |
| **CR-05-04** | C — Gap vs. spec | 🟡 Médio | `InstallMagChamberPatch` ainda mantém guarda legada `IsFikaGuestClient()` que desativa manual chambering em drag-and-drop | `[ ]` Pendente |
| **CR-05-05** | E — Legibilidade | 🟢 Menor | Ausência de comentário ou tratamento para ramo `ChamberFilled == false` em `OnChamberStateSyncPacketReceived` | `[ ]` Pendente |

---

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### CR-05-01 · Cat B — Bug latente · 🟠 Forte

**Ausência de guardas defensivas (`HasChambers`, `Chambers.Length`, `ContainedItem` e `CylinderMagazineItemClass`) em `OnChamberStateSyncPacketReceived`**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Networking/FikaSyncManager.cs:245-253`](../../modded-testchannel/Networking/FikaSyncManager.cs#L245-L253)

**Problema:**
No manipulador de recepção de pacote no Host:
```csharp
if (packet.ChamberFilled && fc.Weapon.ChamberAmmoCount == 0)
{
    var mag = fc.Weapon.GetCurrentMagazine();
    if (mag != null && mag.Count > 0)
    {
        var result = mag.Cartridges.PopTo(
            targetPlayer.InventoryController,
            fc.Item.Chambers[0].CreateItemAddress());
```
O código acessa `fc.Item.Chambers[0]` diretamente sem verificar se a arma realmente possui câmaras (`fc.Weapon.HasChambers`), se a coleção não é nula/vazia (`fc.Item.Chambers?.Length > 0`) e se a câmara não possui item já alocado. Além disso, não checa se o carregador é do tipo tambor (`CylinderMagazineItemClass`), que no EFT/FIKA possui tratamento próprio de camoras e não aceita `PopTo` convencional. Também não checa se o jogador remoto ainda está vivo (`targetPlayer.HealthController.IsAlive`).

**Por que importa:**
Caso um pacote chegue para uma arma sem câmara convencional (como revólveres Chiappa Rhino / MP-412 ou lança-granadas), ou se o jogador for eliminado exatamente quando a câmara for processada, `fc.Item.Chambers[0]` causará `IndexOutOfRangeException` e pode corromper operações de inventário do cadáver.

**Sugestão:**
Aplicar as mesmas proteções defensivas adotadas internamente pelo FIKA (`ObservedFirearmController.cs:311, 720`):
```csharp
if (targetPlayer.HealthController != null && !targetPlayer.HealthController.IsAlive) return;
if (!fc.Weapon.HasChambers || fc.Item.Chambers == null || fc.Item.Chambers.Length == 0) return;
if (fc.Item.Chambers[0].ContainedItem != null) return;

if (packet.ChamberFilled && fc.Weapon.ChamberAmmoCount == 0)
{
    var mag = fc.Weapon.GetCurrentMagazine();
    if (mag != null && mag.Count > 0 && mag is not CylinderMagazineItemClass)
    {
        var result = mag.Cartridges.PopTo(
            targetPlayer.InventoryController,
            fc.Item.Chambers[0].CreateItemAddress());
        ...
    }
}
```

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-05-02 · Cat D — Arquitetura · 🟡 Médio

**`SendChamberState` utiliza `broadcast: true` em pacote direcionado exclusivamente ao Host**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Networking/FikaSyncManager.cs:154`](../../modded-testchannel/Networking/FikaSyncManager.cs#L154)

**Problema:**
```csharp
_lastRegisteredNetworkManager.SendData(ref packet, Fika.Core.Networking.LiteNetLib.DeliveryMethod.ReliableOrdered, true);
```
No FIKA (`FikaServer.cs:937-941`), quando o servidor recebe um pacote com a flag `broadcast: true`, ele retransmite o payload para todos os outros clientes conectados através de `_netServer.SendToAll(..., peer)`.
Contudo, o `ChamberStateSyncPacket` possui efeito estritamente autoritativo no Host (a recepção é guardada por `if (!FikaBackendUtils.IsServer) return;`). Convidados remotos descartam o pacote imediatamente.

**Por que importa:**
Em sessões cooperativas com múltiplos convidados (ex: Host + 3 convidados), sempre que um convidado puxar o ferrolho, o Host reencaminha o pacote a todos os outros clientes, gerando tráfego de rede e consumo de CPU desnecessários para os demais peers.

**Sugestão:**
Alterar o parâmetro `broadcast` de `true` para `false`:
```csharp
_lastRegisteredNetworkManager.SendData(ref packet, Fika.Core.Networking.LiteNetLib.DeliveryMethod.ReliableOrdered, false);
```

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-05-03 · Cat B — Bug latente · 🟡 Médio

**Host não atualiza `FirearmsAnimator.SetAmmoInChamber` ao aplicar `PopTo` autoritativo para jogador remoto**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Networking/FikaSyncManager.cs:254-258`](../../modded-testchannel/Networking/FikaSyncManager.cs#L254-L258)

**Problema:**
Após o Host executar o `PopTo` com sucesso no inventário do `targetPlayer`:
```csharp
if (result.Value != null)
    _logger?.LogInfo($"[TRL-StancesAndMobility] Host aplicou câmara para {targetPlayer.Profile?.Nickname ?? packet.ProfileId}.");
```
O animator do `FirearmController` correspondente ao jogador remoto no Host não recebe atualização do parâmetro `AmmoInChamber`. No FIKA (`ObservedFirearmController.cs:723`), toda manipulação de munição na câmara atualiza explicitamente `FirearmsAnimator.SetAmmoInChamber(weapon.ChamberAmmoCount)`.

**Por que importa:**
Sem atualizar o animator, o avatar observado do jogador na máquina do Host pode manter internamente `AmmoInChamber = 0`, impactando inspeções visuais ou estados de Mecanim até o próximo disparo ou troca de arma.

**Sugestão:**
Adicionar a chamada após o sucesso do `PopTo`:
```csharp
if (result.Value != null)
{
    fc.FirearmsAnimator?.SetAmmoInChamber(fc.Weapon.ChamberAmmoCount);
    _logger?.LogInfo($"[TRL-StancesAndMobility] Host aplicou câmara para {targetPlayer.Profile?.Nickname ?? packet.ProfileId}.");
}
```

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-05-04 · Cat C — Gap vs. spec · 🟡 Médio

**`InstallMagChamberPatch` ainda mantém guarda legada `IsFikaGuestClient()` que desativa manual chambering em drag-and-drop**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/ManualChamberingPatches.cs:736-741`](../../modded-testchannel/Patches/ManualChamberingPatches.cs#L736-L741)

**Problema:**
Na implementação anterior, existiam duas rotas de supressão de câmara: `ReloadExternalMagChamberPatch` (recarga de arma em mãos com tecla R) e `InstallMagChamberPatch` (inserção de magazine arrastando pelo inventário).
Enquanto `ReloadExternalMagChamberPatch` teve a guarda `IsFikaGuestClient()` removida para permitir que o convidado desfrute do manual chambering e sincronize via rede, `InstallMagChamberPatch` ainda possui:
```csharp
if (ManualChamberingPatches.IsFikaGuestClient())
{
    ManualChamberingState.CanLoadChamber = true;
    ManualChamberingState.BlockChambering = false;
    return true;
}
```

**Por que importa:**
Gera comportamento assimétrico para o convidado: se recarregar pela tecla R com câmara vazia, a câmara fica vazia e exige puxar o ferrolho (manual chambering funcional); se instalar o carregador arrastando no menu de inventário (drag-and-drop), a câmara é alimentada automaticamente no padrão vanilla.

**Sugestão:**
Remover a guarda legada em `InstallMagChamberPatch` para unificar a UX com o `ReloadExternalMagChamberPatch` (ou mantê-la documentada caso essa discrepância seja intencional).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-05-05 · Cat E — Legibilidade · 🟢 Menor

**Ausência de comentário ou tratamento para ramo `ChamberFilled == false` em `OnChamberStateSyncPacketReceived`**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Networking/FikaSyncManager.cs:245`](../../modded-testchannel/Networking/FikaSyncManager.cs#L245)

**Problema:**
A struct `ChamberStateSyncPacket` declara:
```csharp
public bool ChamberFilled; // true = bala entrou na câmara; false = câmara esvaziada
```
Entretanto, o método `OnChamberStateSyncPacketReceived` avalia apenas `if (packet.ChamberFilled && ...)`. O caso `packet.ChamberFilled == false` não possui ramo executável nem comentário justificando a omissão.

**Por que importa:**
No FIKA (`ObservedFirearmController.cs:394, 709`), o descarregamento da câmara e ejeção de cápsulas já são sincronizados nativamente por pacotes próprios de animação do EFT/FIKA. Futuros desenvolvedores podem interpretar a ausência de ramo `else if (!packet.ChamberFilled)` como um bug ou código incompleto.

**Sugestão:**
Inserir comentário explicativo detalhando que o esvaziamento de câmara é gerenciado nativamente pelo canal do jogo e não requer intervenção no pacote de estado:
```csharp
// Nota: ChamberFilled == false não requer ação aqui pois o esvaziamento de câmara
// (ejeção de cartucho/shell) já é replicado nativamente pelo EFT/FIKA via ObservedFirearmController.
```

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________
