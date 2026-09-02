---
title: "Relatório de Auditoria Técnica de Código — SAIN (Parte 6: Presets, Serialização JSON e Editor F6)"
date: 2026-08-31
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — SAIN (Parte 6: Presets, Serialização JSON e Editor F6)

Auditoria estática e comportamental profunda focada no **Gerenciador de Presets (`SAINPresetClass`, `PresetHandler`)**, **Utilitários de Serialização JSON (`JsonUtility`)**, **Rastreador de Edições (`ConfigEditingTracker`)**, **Editor Gráfico In-Game F6 (`SAINEditor`, IMGUI)** e **Estruturas de Dados / Modelos**.

---

## 1. Resumo Executivo da Auditoria

| Severidade | Quantidade | Descrição |
|---|---|---|
| 🔴 **Crítico** | 0 | Falhas catastróficas imediatas |
| 🟠 **Alto** | 1 | Execução de lógica de estado (`ConfigEditingTracker.Update`) dentro de `OnGUI` (múltiplas passadas por frame de renderização) |
| 🟡 **Médio** | 2 | Risco de NRE em `ConfigEditingTracker.AddToStringBuilder` com valores nulos e supressão silenciosa de exceções de desserialização em `JsonUtility.LoadObject` |
| 🔵 **Baixo** | 1 | Hardcoded individual de personalidades em `ResetAllToDefaults` |
| 💡 **Otimização** | 2 | Migração de atualizações para `ManualUpdate()` e limpeza O(1) com `.Clear()` |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|---|---|---|---|
| `AUD-06-01` | 🟠 Alto | [`SAINEditor.cs:L105`](../modded/SAIN/Preset/Editor/SAINEditor.cs#L105) | IMGUI Lifecycle | `ConfigEditingTracker.Update()` dentro de `OnGUI` reseta flags de alteração prematuramente entre passadas de `Layout` e `Repaint`. |
| `AUD-06-02` | 🟡 Médio | [`ConfigEditingTracker.cs:L63`](../modded/SAIN/Plugin/ConfigEditingTracker.cs#L63) | NRE Defensivo | `value.GetType()` sem validação de nulo dispara exceção ao inspecionar propriedades anuladas. |
| `AUD-06-03` | 🟡 Médio | [`JsonUtility.cs:L152`](../modded/SAIN/Helpers/JsonUtility.cs#L152) | Error Handling | `catch (JsonSerializationException)` vazio mascara erros de sintaxe em presets JSON customizados. |
| `AUD-06-04` | 🔵 Baixo | [`PersonalityManagerClass.cs:L60-L68`](../modded/SAIN/Preset/Personalities/BasePersonality/PersonalityManagerClass.cs#L60-L68) | Fragilidade Estrutural | `ResetAllToDefaults` remove 8 enums manualmente em vez de usar `PersonalityDictionary.Clear()`. |

---

## 3. Detalhamento dos Achados

### AUD-06-01 · Lógica de Negócio Executada Durante Passadas IMGUI (`OnGUI`)
- **Severidade:** 🟠 Alto
- **Localização no Mod:** [`SAINEditor.cs:L105`](../modded/SAIN/Preset/Editor/SAINEditor.cs#L105)
- **Causa Raiz:** O método `MainWindowFunc` do IMGUI executa `ConfigEditingTracker.Update()` a cada ciclo de renderização gráfica. No Unity, o método `OnGUI()` é chamado várias vezes no mesmo frame (para eventos de `EventType.Layout`, `EventType.Repaint`, `EventType.MouseDrag`, etc.).
- **Impacto Técnico Real:** A propriedade `SettingChangedThisFrame` é resetada para `false` no primeiro evento do frame, fazendo com que renderizações subsequentes no mesmo quadro percam a notificação de que um valor foi modificado.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Mover `ConfigEditingTracker.Update()` para o método `ManualUpdate()`.
  - *Código Refatorado:*

```csharp
// Em SAINEditor.cs - ManualUpdate()
public static void ManualUpdate()
{
    ConfigEditingTracker.Update(); // Executado uma única vez por frame lógico

    if (DisplayingWindow)
    {
        CursorSettings.SetUnlockCursor(0, true);
        MouseFunctions.Update();
    }
    else
    {
        CheckKeys();
    }

    if ((SAINPlugin.OpenEditorConfigEntry.Value.IsDown() && !DisplayingWindow) || SAINPlugin.OpenEditorButton.Value)
    {
        if (SAINPlugin.OpenEditorButton.Value)
        {
            SAINPlugin.OpenEditorButton.BoxedValue = false;
            SAINPlugin.OpenEditorButton.Value = false;
        }
        ToggleGUI();
    }
}
```

- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 2)`

---

### AUD-06-02 · Falha por NRE em Propriedades Nulas no Rastreador de Edições
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`ConfigEditingTracker.cs:L63`](../modded/SAIN/Plugin/ConfigEditingTracker.cs#L63)
- **Causa Raiz:** `Type type = value.GetType();` é invocado diretamente para formatar a string de alterações pendentes. Se uma configuração aceitar valor nulo ou for resetada para `null`, o acesso a `.GetType()` lança `NullReferenceException`.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Adicionar proteção para valores nulos.
  - *Código Refatorado:*

```csharp
private static void AddToStringBuilder(string name, object value)
{
    if (value == null)
    {
        _stringBuilder.AppendLine($"{name}: null");
        _unsavedValues = _stringBuilder.ToString();
        return;
    }

    string _string;
    Type type = value.GetType();
    if (type == _float || type == _bool)
    {
        _string = $"{name}: {value}";
    }
    else
    {
        _string = $"{name}";
    }
    _stringBuilder.AppendLine(_string);
    _unsavedValues = _stringBuilder.ToString();
}
```

- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 3)`

---

### AUD-06-03 · Supressão Silenciosa de Falha em `JsonUtility.LoadObject`
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`JsonUtility.cs:L151-L155`](../modded/SAIN/Helpers/JsonUtility.cs#L151-L155)
- **Causa Raiz:** O bloco `catch (JsonSerializationException) { }` engole o erro sem notificar no console do BepInEx.
- **Impacto Técnico Real:** Se um usuário criar ou editar manualmente um arquivo de preset JSON com erros de sintaxe ou tipos incompatíveis, o mod falha silenciosamente e aplica valores padrão (`default`) sem dar feedback explicativo de onde está o erro.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Adicionar log de advertência no catch.
  - *Código Refatorado:*

```csharp
catch (JsonSerializationException ex)
{
    Logger.LogError($"Error deserializing JSON file [{fileName}] in folders [{string.Join("/", folders)}]: {ex.Message}");
}
```

- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 3)`

---

### AUD-06-04 · Fragilidade em `ResetAllToDefaults` por Remoção Manual
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`PersonalityManagerClass.cs:L60-L68`](../modded/SAIN/Preset/Personalities/BasePersonality/PersonalityManagerClass.cs#L60-L68)
- **Causa Raiz:** O método remove 8 itens por chave hardcoded.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Substituir por `PersonalityDictionary.Clear();`.
- **Decisão:**
  - `[x] Aceitar sugestão (Aplicado no commit Onda 4)`

---

## 4. Plano de Ação e Recomendações

1. **Ciclo de Vida IMGUI (AUD-06-01):** Migrar `ConfigEditingTracker.Update()` para `ManualUpdate()`.
2. **Robustez de Nulo em Configurações (AUD-06-02):** Proteger `AddToStringBuilder` contra `null`.
3. **Visibilidade de Erros JSON (AUD-06-03):** Registrar falhas de desserialização no log.
4. **Limpeza Elegante de Personalidades (AUD-06-04):** Usar `.Clear()`.
