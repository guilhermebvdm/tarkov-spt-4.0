# Spec Técnica: Stance para Recarga e Checagem

## 1. Contexto Técnico
O mod deve reagir a interações do jogador com a arma. No jogo base (Tarkov), ações como checar munição e recarregar são métodos da classe `EFT.Player` ou `Player.FirearmController`. Precisamos interceptar o início dessas ações para forçar a Stance para "Pronto Alto" e identificar quando terminam para restaurar a Stance original.

## 2. Pontos de Interceptação (Harmony Patches)

### 2.1 Início da Ação
Os métodos principais que indicam início de ações de manuseio da arma (encontrados em `Player.FirearmController` ou `EFT.Player`) são:
- `CheckAmmo()`
- `CheckChamber()`
- `ExamineWeapon()`
- Métodos de Recarga: `ReloadMag()`, `QuickReloadMag()`, `ReloadWithAmmo()`, `ReloadBarrels()`, `ReloadCylinderMagazine()`.

**Abordagem Sugerida:**
- Fazer um **Prefix Patch** nestes métodos.
- Lógica no Prefix:
  ```csharp
  if (StanceController.CurrentStance != EStance.HighReady) {
      StanceController.PreviousStance = StanceController.CurrentStance;
      StanceController.ChangeStance(EStance.HighReady);
      StanceController.IsRestoringStance = true;
  }
  ```

### 2.2 Fim da Ação
Existem algumas abordagens para detectar o final da ação:
1. **Callbacks Nativo:** Os métodos de `ReloadMag` recebem parâmetros de `Callback finishCallback`. Podemos interceptar a invocação desse callback ou criar um wrapper, mas é complexo lidar com os métodos que não recebem callbacks diretamente.
2. **Player.Update (State Machine):** Monitorar as propriedades `IsInImportantState`, `CheckAmmo`, `ReloadMagPacket`, etc., na struct de rede do jogador ou nas propriedades da arma.
3. **Player Animator:** Monitorar o hash do Animator do jogador ou de `ProceduralWeaponAnimation` para parâmetros como `PlayerAnimator.RELOAD_FLOAT_PARAM_HASH`. Quando o float zerar ou a animação correspondente terminar, sabemos que a ação acabou.
4. **CurrentOperation:** O `EFT.Player` possui a propriedade `CurrentOperation` que encapsula o estado atual do item em mãos.

**Abordagem Sugerida (Monitoramento Simples no Update):**
No componente `Update()` principal do mod que gerencia o jogador:
- Checar se `IsRestoringStance` é verdadeiro.
- Verificar se as ações ativas na arma (`player.ProceduralWeaponAnimation.IsReloading`, `player.MovementContext.IsInImportantState`, etc.) terminaram.
- Quando todas as flags voltarem a falso, restaurar: `StanceController.ChangeStance(StanceController.PreviousStance)`.

## 3. Configurações BepInEx (PluginConfig)
```csharp
public static ConfigEntry<bool> EnableActionStanceSwap;

// Inicialização:
EnableActionStanceSwap = Config.Bind("8. Action Stances", "Enable Action Stance Swap", true, "Muda automaticamente para Pronto Alto ao recarregar ou inspecionar a arma.");
```

## 4. Implementação
1. Criar `ActionStancePatches.cs` na pasta `modded/`.
2. Registrar os patches do `Prefix` para todos os métodos descritos (Reloads, Checks).
3. Adicionar lógica de verificação de término no script central de atualização de Stance do mod (`StanceController.Update()`).
4. Evitar conflitos caso a arma seja dropada ou a ação seja cancelada abruptamente.
