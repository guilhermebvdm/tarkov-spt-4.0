using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;
using static EFT.Player;

namespace HandsAreNotBusy;

internal class HANB_Component : MonoBehaviour
{
    private Player _player;
    private bool _isFixing = false;

    protected void Awake()
    {
        _player = Singleton<GameWorld>.Instance?.MainPlayer;

        if (_player == null)
        {
            HANB_Plugin.HANB_Logger.LogError("Unable to find MainPlayer, destroying module.");
            Destroy(this);
            return;
        }

        if (!_player.IsYourPlayer)
        {
            HANB_Plugin.HANB_Logger.LogError("MainPlayer is not your player, destroying module");
            Destroy(this);
            return;
        }
    }

    protected void OnDisable()
    {
        _isFixing = false;
    }

    protected void Update()
    {
        if (!Singleton<GameWorld>.Instantiated)
        {
            return;
        }

        if (_player == null || _isFixing)
        {
            return;
        }

        if (HANB_Plugin.ResetKey.Value.IsDown())
        {
            FixHandsController(_player);
        }
    }

    private void FixHandsController(Player player)
    {
        // 0. Guarda contra acionamento enquanto escalando escadas (Climbable Ladders)
        if (player.gameObject.GetComponent("PlayerLadderController") != null)
        {
            HANB_Plugin.HANB_Logger.LogWarning("[HANB] Ignorando reset de mãos: jogador está escalando uma escada.");
            return;
        }

        InventoryController inventoryController = player.InventoryController;
        if (inventoryController != null)
        {
            _isFixing = true;
            // 1. Fechar inventário ANTES de qualquer troca de controller
            player.SetInventoryOpened(false);

            // 2. Sincroniza pedido de limpeza com o Host se conectado via FIKA
            HANB_FikaSync.SendResetRequestToServer(player);

            // 3. Drenar fila de operações travadas
            // GEventArgs1 = evento de operação de inventário enfileirada (EFT obfuscado; conceito: InventoryOperationEvent)
            // List_0 = fila interna de operações ativas do InventoryController
            int length = inventoryController.List_0.Count;
            if (length > 0)
            {
                GEventArgs1[] args = new GEventArgs1[length];
                inventoryController.List_0.CopyTo(args);
                foreach (GEventArgs1 queuedEvent in args)
                {
                    inventoryController.RemoveActiveEvent(queuedEvent);
                }
                HANB_Plugin.HANB_Logger.LogInfo($"Cleared {length} stuck inventory operations.");
            }

            // 4. Capturar controller atual e fazer teardown
            AbstractHandsController handsController = player.HandsController;

            // Se for o controller do LoadAmmoAnim, cancela graciosamente o driver antes do teardown
            if (handsController != null && handsController.GetType().Name == "LoadAmmoBundleController")
            {
                try
                {
                    var driverType = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                        .FirstOrDefault(t => t.FullName == "Manimal.LoadAmmoAnim.Patches.LoadAmmoAnimDriver")
                        ?? Type.GetType("Manimal.LoadAmmoAnim.Patches.LoadAmmoAnimDriver, LoadAmmoAnimClient");

                    var stopMethod = driverType?.GetMethod("StopAnimationInstantly", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    stopMethod?.Invoke(null, new object[] { player });
                    HANB_Plugin.HANB_Logger.LogInfo("[HANB] Interrompida animação ativa de LoadAmmoAnim com sucesso.");
                }
                catch (Exception ex)
                {
                    HANB_Plugin.HANB_Logger.LogWarning($"[HANB] Exceção contida ao parar LoadAmmoAnim: {ex.Message}");
                }
            }

            // CR-01-03: log de diagnóstico do tipo de controller para triagem de regressões
            HANB_Plugin.HANB_Logger.LogInfo(
                $"[HANB] Destruindo controller: {handsController?.GetType().Name ?? "null"} " +
                $"— desinscrição FirearmController: {handsController is FirearmController}");

            if (handsController is FirearmController currentFirearmController)
            {
                player.MovementContext.OnStateChanged -= currentFirearmController.method_17;
                player.Physical.OnSprintStateChangedEvent -= currentFirearmController.method_16;
                currentFirearmController.RemoveBallisticCalculator();
            }

            // Teardown canônico EFT — não chamar Destroy() Unity separadamente,
            // pois AbstractHandsController.Destroy() já gerencia o ciclo de vida interno.
            handsController?.Destroy();

            // 5. Spawnar controller vazio
            try
            {
                var emptyController = Player.EmptyHandsController.smethod_6<Player.EmptyHandsController>(player);
                player.SpawnController(emptyController);
            }
            catch (Exception ex)
            {
                HANB_Plugin.HANB_Logger.LogWarning($"[HANB] Exceção contida ao spawnar controlador vazio: {ex}");
            }

            // 6. Reequipar via corrotina com delay assíncrono para garantir
            // que o pacote de reset de inventário tenha sido processado no Host/Headless
            StartCoroutine(ReequipRoutine(player));
        }
        else
        {
            HANB_Plugin.HANB_Logger.LogError("[HANB] FixHandsController: player.InventoryController retornou null.");
        }
    }

    private System.Collections.IEnumerator ReequipRoutine(Player player)
    {
        // Se conectado ao FIKA como cliente, aguarda 150ms para que o pacote de limpeza chegue e seja processado no Host/Headless.
        // Em Singleplayer offline, 1 frame é suficiente para assentar o inventário local.
        if (HANB_FikaSync.IsFikaClient)
        {
            yield return new WaitForSeconds(0.15f);
        }
        else
        {
            yield return null;
        }

        if (player == null)
        {
            _isFixing = false;
            yield break;
        }

        try
        {
            player.ProcessStatus = EProcessStatus.None;
            if (player.LastEquippedWeaponOrKnifeItem != null)
            {
                player.TrySetLastEquippedWeapon();
            }
            else
            {
                player.SetFirstAvailableItem((result) => { });
            }
        }
        catch (Exception ex)
        {
            HANB_Plugin.HANB_Logger.LogWarning($"[HANB] Exceção ao reequipar item: {ex}");
        }

        // Aguarda 1 frame para assentamento do HandsController antes de sincronizar procedural animation
        yield return null;

        if (player != null && player.HandsController is FirearmController firearmController && firearmController.Weapon != null)
        {
            try
            {
                Traverse.Create(player.ProceduralWeaponAnimation)
                    .Field("_firearmAnimationData")
                    .SetValue(firearmController);
            }
            catch (Exception ex)
            {
                HANB_Plugin.HANB_Logger.LogWarning($"[HANB] Exceção ao sincronizar ProceduralWeaponAnimation: {ex}");
            }
        }

        _isFixing = false;
    }
}
