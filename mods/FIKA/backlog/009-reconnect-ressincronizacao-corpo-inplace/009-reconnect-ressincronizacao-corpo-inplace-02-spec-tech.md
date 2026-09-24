# 009 — Correção Definitiva de Reconnect no FIKA (Ressincronização de Corpo In-Place) · Spec Técnica

**Mod:** FIKA  
**Spec funcional:** [009-reconnect-ressincronizacao-corpo-inplace-01-spec.md](009-reconnect-ressincronizacao-corpo-inplace-01-spec.md)  
**Criado:** 2026-09-13  

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

## 1. Estratégia

A correção adota a abordagem de **ressincronização autoritativa in-place**, descartando expressamente qualquer recriação ou destruição do `ObservedPlayer` na memória do Host (o que causaria perda de inventário coletado, integridade de coletes e estado de saúde). 

A estratégia divide-se em 4 pilares:
1. **Auto-recuperação Temporal (`PlayerSnapshotter`):** Mitigação do descarte contínuo de pacotes por reset de `Time.unscaledTimeAsDouble` após reinício do cliente, detectando quebras bruscas de relógio e limpando o buffer via `Clear()`.
2. **Re-ancoragem Física e Oclusão (`ObservedPlayer`):** Implementação de `ForceTeleport(Vector3 position, Vector2 rotation)` que encapsula o teleporte nativo do EFT (`Player.Teleport`), o alinhamento da raiz e dos ossos (`PlayerBones.BodyTransform`), a re-ancoragem da visibilidade do EFT (`LocalPlayerCullingHandlerClass.ApplyVisibleState`) e a sincronização do PhysX (`Physics.SyncTransforms`).
3. **Sincronização Contínua da Raiz sob Culling (`ObservedPlayer`):** Correção em `ManualStateUpdate` para assegurar que `Transform.position = CurrentPlayerState.Position` seja chamado mesmo quando `!_cullingHandler.IsVisible`, impedindo que a caixa de visibilidade do EFT se distancie da posição real do jogador enquanto ele se desloca sem ser observado diretamente.
4. **Broadcast Confiável de Re-ancoragem (`ClearSnapshotterPacket`):** Extensão do pacote de rede com `Vector3 Position` e `Vector2 Rotation` e retransmissão via broadcast pelo Host para todos os outros clientes da raid.

## 2. Pontos de patch e alvos de código

| Alvo | Tipo | Motivo |
|---|---|---|
| [`Player.cs:31308`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L31308) | Método EFT | Teletransporte nativo do EFT (`MovementContext.TransformPosition = position; method_14();`). |
| [`Player.cs:24637`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L24637) | Propriedade EFT | `Player.Position` afeta apenas `PlayerBones.BodyTransform.position`. |
| [`LocalPlayerCullingHandlerClass.cs:16`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/LocalPlayerCullingHandlerClass.cs#L16) | Método EFT | Forçar reavaliação de visibilidade da malha (`ApplyVisibleState`). |
| [`PlayerSnapshotter.cs:42`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/Snapshotting/PlayerSnapshotter.cs#L42) | Método FIKA | `AddSnapshot`: detectar salto temporal negativo e resetar buffer. |
| [`ObservedPlayer.cs:967`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L967) | Método FIKA | `ManualStateUpdate`: sincronizar `Transform.position` quando `!_cullingHandler.IsVisible`. |
| [`FikaServer.Callbacks.cs:191`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/FikaServer.Callbacks.cs#L191) | Callback FIKA | Acionar `ForceTeleport` e fazer broadcast para os demais clientes. |
| [`FikaClient.Callbacks.cs:39`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/FikaClient.Callbacks.cs#L39) | Callback FIKA | Acionar `ForceTeleport` no cliente ao receber o broadcast. |
| [`CoopGame.cs:425`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/GameMode/CoopGame.cs#L425) | Método FIKA | `Reconnect`: fornecer `Position` e `Rotation` ao `ClearSnapshotterPacket`. |

## 3. Novas propriedades F12 (BepInEx)

*N/A: Esta entrega trata de correção estrutural interna de protocolo de rede e ciclo de vida de entidades, sem introdução de parâmetros configuráveis pelo usuário.*

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `Networking/Snapshotting/PlayerSnapshotter.cs` | MODIFICAR | Reset defensivo do buffer em saltos de relógio negativos. |
| `Networking/Packets/Communication/ClearSnapshotterPacket.cs` | MODIFICAR | Adição de campos `Position` e `Rotation` com serialização. |
| `Main/Players/ObservedPlayer.cs` | MODIFICAR | Implementar `ForceTeleport` e sincronizar raiz sob oclusão. |
| `Networking/FikaServer.Callbacks.cs` | MODIFICAR | Teleporte no Host e broadcast de `ClearSnapshotterPacket`. |
| `Networking/FikaClient.Callbacks.cs` | MODIFICAR | Teleporte em clientes remotos ao receber broadcast. |
| `Main/GameMode/CoopGame.cs` | MODIFICAR | Preencher coordenadas reais no envio do pacote pós-reconnect. |
| `FikaPlugin.cs` | MODIFICAR | Bump de versão SemVer para `2.3.20`. |
| `Fika.Core.csproj` | MODIFICAR | Bump de versão SemVer para `2.3.20`. |

## 5. Stubs de código

```csharp
// mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/Snapshotting/PlayerSnapshotter.cs
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public void AddSnapshot(in T snapshot)
{
    if (_totalAdded > 0)
    {
        var newestIdx = (int)((_totalAdded - 1) & _mask);
        var newestTime = _buffer[newestIdx].RemoteTime;

        // sequence validation: if timestamp jumped backwards significantly (reconnect, client restart), reset buffer
        if (snapshot.RemoteTime < newestTime - 5.0d)
        {
            Clear();
        }
        else if (snapshot.RemoteTime <= newestTime)
        {
            // drop out-of-order or duplicate packets
            return;
        }
        else
        {
            var localDelta = snapshot.LocalTime - _lastLocalTime;
            var remoteDelta = snapshot.RemoteTime - _lastRemoteTime;
            _adaptiveJitterBuffer.Update(localDelta, remoteDelta);
        }
    }

    _buffer[_totalAdded & _mask] = snapshot;
    _totalAdded++;
    _timeSync.Update(snapshot.RemoteTime, snapshot.LocalTime);
    _lastLocalTime = snapshot.LocalTime;
    _lastRemoteTime = snapshot.RemoteTime;
}
```

```csharp
// mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Players/ObservedPlayer.cs
public void ForceTeleport(Vector3 position, Vector2 rotation)
{
    // ref: references/eft-decompiled/Assembly-CSharp/EFT/Player.cs:31308
    base.Teleport(position, true);
    Transform.position = position;
    PlayerBones.BodyTransform.position = position;

    CurrentPlayerState.Position = position;
    CurrentPlayerState.Rotation = rotation;
    Rotation = rotation;
    if (MovementContext != null)
    {
        MovementContext.Rotation = rotation;
        MovementContext.CachedRotation = rotation;
    }

    Snapshotter.Clear();

    Physics.SyncTransforms();

    if (_cullingHandler != null)
    {
        // ref: references/eft-decompiled/Assembly-CSharp/LocalPlayerCullingHandlerClass.cs:16
        _cullingHandler.ApplyVisibleState();
    }
}
```

## 6. Fluxo de dados

```
[1] Cliente reconecta e obtém perfil
         ↓
[2] CoopGame instancia LocalPlayer e posiciona em ReconnectPosition
         ↓
[3] CoopGame dispara ClearSnapshotterPacket (NetId, Position, Rotation) [ReliableOrdered]
         ↓
[4] Host (FikaServer) recebe pacote
         ↓
    ├─ Executa ForceTeleport no ObservedPlayer do Host
    └─ Faz broadcast de ClearSnapshotterPacket para todos os outros clientes
         ↓
[5] Outros Clientes (FikaClient) recebem pacote
         ↓
    └─ Executam ForceTeleport no ObservedPlayer correspondente
         ↓
[6] Novos pacotes de PlayerState chegam no Host e Clientes
         ↓
    └─ PlayerSnapshotter aceita os snapshots sem descarte (timestamp auto-recuperado)
         ↓
[7] Corpo perfeitamente visível e móvel para todos na nova coordenada
```

## 7. Riscos e dependências

- **Risco de sobrescrita de inventário:** Mitigado 100% ao não recriar o `ObservedPlayer`. O inventário original permanece autoritativo no Host.
- **Risco de concorrência com pacotes de movimento em trânsito:** O `Snapshotter.Clear()` dentro de `ForceTeleport` purga qualquer interpolação pendente do ponto antigo.
- **Ordem de pacotes de rede:** O `ClearSnapshotterPacket` usa `DeliveryMethod.ReliableOrdered`, garantindo entrega sequencial determinística antes da aceitação do fluxo contínuo de movimentação.

## 8. Checklist de implementação

- [x] Atualizar `PlayerSnapshotter.cs` com detecção de salto temporal negativo.
- [x] Expandir `ClearSnapshotterPacket.cs` com `Position` e `Rotation`.
- [x] Implementar `ForceTeleport` e sincronização de `Transform.position` em `ObservedPlayer.cs`.
- [x] Atualizar `FikaServer.Callbacks.cs` para invocar `ForceTeleport` e fazer broadcast para outros clientes.
- [x] Atualizar `FikaClient.Callbacks.cs` para invocar `ForceTeleport`.
- [x] Atualizar `CoopGame.cs` para enviar `Position` e `Rotation` no `ClearSnapshotterPacket`.
- [x] Bump SemVer para `2.3.20` em `FikaPlugin.cs` e `Fika.Core.csproj`.
- [x] Compilação Release limpa (0 erros).

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | ✅ | Limpeza é idempotente; encerramento de sessão segue o padrão de teardown de `CoopGame.Stop()`. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | Aplicação explícita em instâncias de `ObservedPlayer`, sem interferir no jogador local `FikaPlayer`. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | ✅ | `Player.Teleport` é virtual, `ObservedPlayer` invoca `base.Teleport(position, true)` diretamente. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Uso de `Player.Teleport`, `PlayerBones.BodyTransform.position`, `Physics.SyncTransforms` e `ApplyVisibleState`. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | O buffer de snapshots e a instância de `ObservedPlayer` são destruídos ao término da raid e recriados na próxima. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | N/A | Nenhuma `ConfigEntry` introduzida neste item. |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` — AP-07 | N/A | Não são utilizados hooks reentrantes; métodos são chamados explicitamente em callbacks de rede. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca — AP-08 | N/A | Sem caches estáticos por arma ou operação. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | Validado em `EFT/Player.cs:31308`, `EFT/Player.cs:24637` e `LocalPlayerCullingHandlerClass.cs:16`. |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Nenhuma skill do EFT utilizada como alavanca. |
| 11 | Pacote FIKA próprio: integridade, serialização e airbag — AP-11 | ✅ | `ClearSnapshotterPacket` implementa `INetSerializable`, serializa tipos não gerenciados com `PutUnmanaged` e é registrado no barramento oficial do FIKA. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-13 | Spec técnica criada e validada com Assembly-CSharp e Fika.Core. |
