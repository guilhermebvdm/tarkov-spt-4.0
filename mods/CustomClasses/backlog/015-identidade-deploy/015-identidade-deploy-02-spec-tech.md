# 015 — Identidade da classe no nome do jogador · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** [015-identidade-deploy-01-spec.md](015-identidade-deploy-01-spec.md)
**Criado:** 2026-06-08

> Patch num **widget compartilhado** (`ChatSpecialIcon`) usado em deploy/character/online/chat. Só aplica ao **jogador local**. Reusa a base do 011/012 (`SkillMultipliers`, `ClassIconCache`, `ClassIdentityView.ApplyGradient`). Refs confirmadas via ilspycmd.

## 1. Estratégia

Postfix em `EFT.UI.ChatSpecialIcon.Show(EMemberCategory, string playerName, bool, int)` (o overload que faz o trabalho; `Show(GClass1410)` o chama). Quando `playerName` == nickname do perfil local **e** for classe do mod:
- `_icon.sprite` ← ícone da classe (`ClassIconCache.Get(IconFile)`), se houver;
- `_specialLabel` ← gradiente da cor da classe (`ApplyGradient`);
- `_specialLabel.text` ← `playerName + " [" + ClassName + "]"` (sufixo mesma-linha — seguro em listas).

Para identificar o local sem reflection frágil: a rota `/customclasses/skill-multipliers` passa a devolver o **`nickname`** do perfil; o client compara.

O **selo do 012** (menu-MO/Skills) é desligado por padrão (`ShowClassIdentity` default `false`), reversível no F12.

## 2. Pontos de patch / refs

| Símbolo | Fonte | Uso |
|---|---|---|
| `ChatSpecialIcon.Show(EMemberCategory, string, bool, int)` (campos `_icon`, `_specialLabel`) | Assembly (ilspycmd) | postfix p/ trocar ícone/cor/nome |
| `SkillMultipliersRouter` / `SkillMultipliersResponse` | `modded/Server/` | + `nickname` do perfil |
| `SaveServer.GetProfile(sessionId).ProfileInfo.Nickname` | (já usado p/ Edition) | nickname do perfil |
| `SkillMultipliers` (client) | `modded/Client/` | + `Nickname`; comparação |
| `ClassIconCache.Get` / `ClassIdentityView.ApplyGradient` | item 011/012 | sprite + gradiente |

`ChatSpecialIcon.Show(EMemberCategory, …)` tem 2 overloads → resolver por **param-count (4)**.

## 3. Novas propriedades F12

| Seção | Nome (EN) | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| `General` | `ShowClassOnPlayerName` | bool | `true` | Aplica ícone+nome da classe no nome do jogador (deploy/character/online). |
| `General` | `ShowClassIdentity` (já existe) | bool | **`false`** (mudança) | Selo separado da classe no menu e na tela de Skills (off por padrão; o nome do jogador já mostra via ShowClassOnPlayerName). |

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Server/SkillMultipliersResponse.cs` | MODIFICAR | + `nickname`. |
| `modded/Server/SkillMultipliersRouter.cs` | MODIFICAR | resolve `ProfileInfo.Nickname`. |
| `modded/Client/SkillMultipliers.cs` | MODIFICAR | + `Nickname`; `Reset()` zera. |
| `modded/Client/Patches/ChatSpecialIconPatch.cs` | CRIAR | postfix; só jogador local; ícone+gradiente+sufixo. |
| `modded/Client/Plugin.cs` | MODIFICAR | registra patch; `ShowClassOnPlayerName`; `ShowClassIdentity` default false. |
| `mods/CustomClasses/PROPRIEDADES.md` | MODIFICAR | novas/alteradas. |

## 5. Stubs

### SkillMultipliersResponse / Router / SkillMultipliers
```csharp
// Response: + [JsonPropertyName("nickname")] string? Nickname { get; init; }
// Router: Nickname = saveServer.GetProfile(sessionId)?.ProfileInfo?.Nickname,
// SkillMultipliers (client): public static string? Nickname { get; private set; }  + Payload.Nickname + Reset()
```

### ChatSpecialIconPatch.cs
```csharp
using System;
using System.Linq;
using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;
using UnityEngine.UI;

namespace CustomClasses.Client;

internal class ChatSpecialIconPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
        => AccessTools.GetDeclaredMethods(typeof(ChatSpecialIcon))
            .First(m => m.Name == nameof(ChatSpecialIcon.Show) && m.GetParameters().Length == 4);

    [PatchPostfix]
    private static void Postfix(ChatSpecialIcon __instance, string playerName,
        TextMeshProUGUI ____specialLabel, Image ____icon)
    {
        if (!Plugin.ShowClassOnPlayerName || string.IsNullOrEmpty(playerName)) return;
        try
        {
            SkillMultipliers.EnsureLoaded();
            if (SkillMultipliers.ClassName == null) return;                 // perfil vanilla
            if (!string.Equals(playerName, SkillMultipliers.Nickname, StringComparison.Ordinal)) return;  // só o jogador local

            if (____icon != null)
            {
                var sprite = ClassIconCache.Get(SkillMultipliers.IconFile);
                if (sprite != null) { ____icon.sprite = sprite; ____icon.preserveAspect = true; }
            }
            if (____specialLabel != null)
            {
                ClassIdentityView.ApplyGradient(____specialLabel, ClassIdentityView.ResolveColor(SkillMultipliers.NameColor, ____specialLabel.color));
                ____specialLabel.text = $"{playerName}  <size=80%>[{SkillMultipliers.ClassName}]</size>";   // sufixo mesma-linha
            }
        }
        catch (Exception ex) { Plugin.Log?.LogError($"[CustomClasses] chat special icon falhou: {ex.Message}"); }
    }
}
```

## 6. Riscos e dependências

- **Widget compartilhado:** `ChatSpecialIcon.Show` roda p/ vários jogadores (chat/online). Guard `playerName == Nickname` garante que **só o local** é alterado; demais ficam vanilla. Hot path leve (compara string e sai).
- **Reset por re-Show:** o vanilla seta `_specialLabel.text = playerName` e `.color` a cada Show; o postfix re-aplica → idempotente, sem acumular sufixo.
- **2ª linha no character:** adiada (sufixo mesma-linha é seguro em todos). Detecção de contexto (fontSize/rect) fica como refinamento.
- **Nickname na rota:** novo campo; client antigo ignora. Sem quebra.
- **012 default off:** reversível no F12 (`ShowClassIdentity`). Código mantido.
- **Prestige icon:** não mexer (`_iconPrestige`).

## 7. Checklist

- [ ] Server: `nickname` no response + router.
- [ ] Client: `SkillMultipliers.Nickname`.
- [ ] `ChatSpecialIconPatch` (só local; ícone+gradiente+sufixo).
- [ ] `Plugin`: registra patch + `ShowClassOnPlayerName` + `ShowClassIdentity` default false.
- [ ] `PROPRIEDADES.md`.
- [ ] `/compile-mod` 0 warn/err.
- [ ] Playtest: deploy/character/online mostram ícone+nome+[Classe] colorido p/ o local; outros jogadores intactos; menu-MO/Skills sem selo (a menos que religue no F12).

## Histórico

| Data | Evento |
|---|---|
| 2026-06-08 | Spec técnica via `/create-technical-spec` (ChatSpecialIcon postfix; nickname via rota; 012 default off). |
