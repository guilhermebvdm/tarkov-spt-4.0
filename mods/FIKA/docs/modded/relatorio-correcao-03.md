---
title: "Relatório de Implementação e Correção — FIKA (Partição 03: Sincronização de Bots & Spawns)"
date: 2026-09-02
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Implementação e Correção — FIKA (Partição 03: Sincronização de Bots & Spawns)

## 1. Resumo Executivo das Correções

Este relatório documenta a aplicação das correções técnicas cirúrgicas na **Partição 3 (`Sincronização de Bots & Spawns`)** do mod **FIKA**, implementadas em `mods/FIKA/modded/Fika-Plugin/`.

Todas as intervenções seguiram o princípio de **intervenção mínima / cirúrgica**, eliminando falhas de NRE no descarte de bots, garantindo resiliência em pools de inventário de bots e incorporando o destravamento nativo de armas montadas para bots de IA do **`TRL-Fixes`**, preservando 100% de integridade e compatibilidade com mods de IA externos (*SAIN*, *Questing Bots*, *Looting Bots*, *BigBrain*, *TRL-FIXES*).

| ID do Achado | Severidade | Arquivo / Linha Modificada | Ação / Correção Aplicada |
| :---: | :---: | :--- | :--- |
| `AUD-03-01` | 🔴 Crítico | [`FikaBot.cs:L360-385`](../../modded/Fika-Plugin/Fika.Core/Main/Players/FikaBot.cs#L360-L385) | Protegido o acesso a `Singleton<IFikaGame>.Instantiated` e null check na lista de bots no `FikaBot.OnDestroy()` contra NRE em saídas súbitas de raid. |
| `AUD-03-02` | 🟠 Alto | [`BotInventoryOperationHandlerPool.cs:L20-26`](../../modded/Fika-Plugin/Fika.Core/Networking/Pooling/BotInventoryOperationHandlerPool.cs#L20-L26) | Inserida guarda `Instance?.Dispose()` em `BotInventoryOperationHandlerPool.Clear()` para prevenir NRE caso a pool seja limpa antes da inicialização. |
| `AUD-03-03` | 🟡 Médio | [`BotStateManager.cs:L125-131`](../../modded/Fika-Plugin/Fika.Core/Main/Components/BotStateManager.cs#L125-L131) | Anulação explícita de referências a `_botsController`, `_controller` e `_server` no `BotStateManager.OnDestroy()` para evitar retenção na memória. |
| `TRL-Fixes #6` | 🛡️ Estabilidade | [`FikaPlayer.cs:L1315-1325`](../../modded/Fika-Plugin/Fika.Core/Main/Players/FikaPlayer.cs#L1315-L1325) | Inserido bypass `!IsAI && (...)` na trava de `WaitingForCallback` em `OperateStationaryWeapon`, permitindo que bots de IA operem armas fixas sem bloqueios de rede. |

---

## 2. Detalhamento do Código Modificado

### 2.1. Teardown Seguro em `FikaBot.OnDestroy`
```csharp
public override void OnDestroy()
{
#if DEBUG
    FikaGlobals.LogInfo("Destroying " + Profile.Info.Nickname);
#endif
    if (Singleton<FikaServer>.Instantiated)
    {
        if (Singleton<IFikaGame>.Instantiated)
        {
            var fikaGame = Singleton<IFikaGame>.Instance;
            if (fikaGame != null && fikaGame.GameController?.GameInstance?.Status == GameStatus.Started)
            {
                var server = Singleton<FikaServer>.Instance;
                if (server != null)
                {
                    BotStatePacket packet = new()
                    {
                        NetId = NetId,
                        Type = BotStatePacket.EStateType.DisposeBot
                    };

                    server.SendData(ref packet, DeliveryMethod.ReliableOrdered);
                    fikaGame.GameController.Bots?.Remove(ProfileId);
                }
            }
        }
    }
    if (CoopHandler.TryGetCoopHandler(out var coopHandler))
    {
        coopHandler.Players?.Remove(NetId);
    }
    base.OnDestroy();
}
```

### 2.2. Guarda em `BotInventoryOperationHandlerPool.Clear`
```csharp
public static void Clear()
{
    Instance?.Dispose();
    Instance = null;
}
```

### 2.3. Limpeza de Controladores em `BotStateManager.OnDestroy`
```csharp
private void OnDestroy()
{
    FikaBot.OnPlayerDeath -= OnPlayerDeath;
    FikaBot.OnPlayerDestroyed -= OnPlayerDeath;
    _bots?.Clear();
    _botsController = null;
    _controller = null;
    _server = null;
}
```

### 2.4. Destravamento de Armas Montadas para Bots (`TRL-Fixes #6`)
```csharp
public override void OperateStationaryWeapon(StationaryWeapon stationaryWeapon, StationaryPacketStruct.EStationaryCommand command)
{
    if (command is StationaryPacketStruct.EStationaryCommand.Occupy)
    {
        if (!IsAI && (WaitingForCallback || !HandsController.CanRemove()))
        {
            return;
        }
        if (FikaBackendUtils.IsClient)
        {
            _inventoryController.ExecuteStationaryOperation(stationaryWeapon, CheckIfStationarySucceeded);
        }
    }

    base.OperateStationaryWeapon(stationaryWeapon, command);
    ...
```

---

## 3. Validação de Compilação Isolada

- **Comando:** `dotnet build mods/FIKA/modded/Fika-Plugin/Fika.Core/Fika.Core.csproj -c Release`
- **Resultado:** `Compilação com êxito. 0 Aviso(s), 0 Erro(s).`
- **Binário Gerado:** `mods/FIKA/modded/Fika-Plugin/Fika.Core/bin/Release/netstandard2.1/Fika.Core.dll`
- **Isolamento:** Nenhum binário foi copiado para pastas fora de `mods/FIKA/modded/`.

---

## 4. Validação do Documento

```bash
bash .agents/hooks/validate-doc-header.sh mods/FIKA/docs/modded/relatorio-correcao-03.md
```
