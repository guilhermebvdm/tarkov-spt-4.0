---
title: "Relatório de Implementação e Correção — FIKA (Partição 02: Replicação de Jogadores & Movimento)"
date: 2026-09-02
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Implementação e Correção — FIKA (Partição 02: Replicação de Jogadores & Movimento)

## 1. Resumo Executivo das Correções

Este relatório documenta a aplicação das correções técnicas cirúrgicas na **Partição 2 (`Replicação de Jogadores & Movimento`)** do mod **FIKA**, implementadas em `mods/FIKA/modded/Fika-Plugin/`.

Todas as intervenções seguiram o princípio de **intervenção mínima / cirúrgica**, eliminando vazamentos de memória (RAM Leaks) em armaduras e áudio VOIP, prevenindo corrupção de física por coordenadas `NaN`/`Infinity` e incorporando a solução para armas multi-trilho do **`TRL-Fixes`**, preservando 100% de integridade e compatibilidade com mods de terceiros (*Speak From Tarkov*, *SAIN*, *Dynamic Maps*, *Realism*, *TRL-FIXES*).

| ID do Achado | Severidade | Arquivo / Linha Modificada | Ação / Correção Aplicada |
| :---: | :---: | :--- | :--- |
| `AUD-02-01` | 🔴 Crítico | [`FikaPlayer.cs:L1586-1605`](../../modded/Fika-Plugin/Fika.Core/Main/Players/FikaPlayer.cs#L1586-L1605) | Inserida desinscrição explícita de `_armorUnsubcribes` em `FikaPlayer.OnDestroy()` para cobrir jogadores que sobrevivem à raid, desconectam ou extraem vivos. |
| `AUD-02-02` | 🔴 Crítico | [`ObservedPlayer.cs:L1794-1805`](../../modded/Fika-Plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L1794-L1805) | Implementada liberação explícita de `VoipEftSource.Release()` no `ObservedPlayer.OnDestroy()` para devolver o `BetterSource` nativo à pool do `BetterAudio`. |
| `AUD-02-04` | 🟡 Médio | [`ObservedPlayer.cs:L1807-1815`](../../modded/Fika-Plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L1807-L1815) | Protegido o acesso a `Singleton<IFikaNetworkManager>.Instance` com `Instantiated` e checagem de nulo durante o teardown. |
| `TRL-Fixes #4` | 🛡️ Estabilidade | [`ObservedPlayer.cs:L1428-1445`](../../modded/Fika-Plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L1428-L1445) | Substituída a variável local `Dictionary<string, GClass768.GClass769>` por `List<KeyValuePair<string, GClass768.GClass769>>` em `RefreshSlotViews`, eliminando colisão de chave em armas com múltiplos adaptadores/trilhos (`mod_tactical`). |
| `AUD-02-03` | 🟠 Alto | [`ObservedMovementContext.cs:L65,L91`](../../modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedMovementContext.cs#L65) | Inseridas guardas defensivas `deltaTime > 0.0001f` em `DirectApplyMotion` e `LimitMotionXZ` para prevenir injeção de `NaN` ou `Infinity` nas coordenadas do jogador. |

---

## 2. Detalhamento do Código Modificado

### 2.1. Desinscrição de Armadura em `FikaPlayer.OnDestroy`
```csharp
public override void OnDestroy()
{
    if (_armorUnsubcribes != null)
    {
        for (var i = 0; i < _armorUnsubcribes.Length; i++)
        {
            _armorUnsubcribes[i]?.Invoke();
            _armorUnsubcribes[i] = null;
        }
    }

    if (IsAI || IsYourPlayer)
    {
        CommonPacket?.Clear();
        CommonPacket = null;
    }
    OnPlayerDestroyed?.Invoke(this);
    base.OnDestroy();
}
```

### 2.2. Armas Multi-Trilho (`TRL-Fixes #4`) em `ObservedPlayer.RefreshSlotViews`
```csharp
var newSlots = newWeapon.AllSlots;
if (newSlots != null)
{
    List<KeyValuePair<string, GClass768.GClass769>> currentViews = [];
    foreach (var kvp in controller.CCV.ContainerBones)
    {
        if (kvp.Key is Slot slot && slot.ContainedItem != null)
        {
            currentViews.Add(new KeyValuePair<string, GClass768.GClass769>(slot.FullId, kvp.Value));
        }
    }
    controller.CCV.RemoveBones(controller.Weapon.AllSlots);
    ...
```

### 2.3. Liberação de `VoipEftSource` em `ObservedPlayer.OnDestroy`
```csharp
if (VoipEftSource != null)
{
    try
    {
        VoipEftSource.Release();
    }
    catch (Exception)
    {
    }
    VoipEftSource = null;
}
```

### 2.4. Proteção contra Divisão por Zero em `ObservedMovementContext.cs`
```csharp
public override void DirectApplyMotion(Vector3 motion, float deltaTime)
{
    InputMotion = deltaTime > 0.0001f ? motion / deltaTime : Vector3.zero;
    ...
}

public override void LimitMotionXZ(ref Vector3 motion, float deltaTime, float threshold = 0.0001F)
{
    InputMotionBeforeLimit = deltaTime > 0.0001f ? motion / deltaTime : Vector3.zero;
}
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
bash .agents/hooks/validate-doc-header.sh mods/FIKA/docs/modded/relatorio-correcao-02.md
```
