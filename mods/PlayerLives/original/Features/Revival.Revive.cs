using EFT;
using EFT.HealthSystem;
using EFT.Communications;
using System;
using System.Linq;
using UnityEngine;
using PlayerLives.Helpers;

namespace PlayerLives.Features
{
    // Manual revive, give up, healing, and the revive-item lookup/consume logic.
    internal partial class RevivalFeatures
    {
        public static bool TryPerformManualRevival(Player player)
        {
            if (player == null) return false;

            string playerId = player.ProfileId;

            // Clear critical BEFORE standing so the per-frame downed tick can't
            // re-slam prone this same frame after we stand the player up.
            SetPlayerCriticalState(player, false);
            _lastRevivalTimesByPlayer[playerId] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (Settings.REQUIRE_STIM.Value != "None")
                ConsumeReviveItem(player);

            HealPlayer(player);

            StartInvulnerability(player);

            CancelDownedMovementBlock(player);

            StandPlayerUp(player);

            player.Say(EPhraseTrigger.OnMutter, false, 2f, ETagStatus.Combat, 100, true);

            // Show successful revival notification
            NotificationManagerClass.DisplayMessageNotification(
                $"Invulnerability started - {Settings.REVIVAL_DURATION.Value}s! {Plugin.CurrentLives} lives left.",
                ENotificationDurationType.Long,
                ENotificationIconType.Default,
                Color.green);

            Plugin.LogSource.LogInfo($"Manual revival performed for player {playerId}");
            return true;
        }

        public static bool GiveUp(Player player)
        {
            if (player == null) return false;

            string playerId = player.ProfileId;

            // Set alive first before applying effects
            SetPlayerCriticalState(player, false);
            _lastRevivalTimesByPlayer[playerId] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            Plugin.GaveUp = true;
            player.ActiveHealthController.IsAlive = true;
            RestoreDamageCoeff(player);

            // Kill player by applying fatal damage to head
            if (player.ActiveHealthController != null)
            {
                player
                .ActiveHealthController
                .ApplyDamage(EBodyPart.Head, player.ActiveHealthController.GetBodyPartHealth(EBodyPart.Head).Maximum + 100f, new DamageInfoStruct { DamageType = GetLastLethalDamageType(playerId) });
            }

            // Show successful revival notification
            NotificationManagerClass.DisplayMessageNotification(
                $"You gave up!",
                ENotificationDurationType.Long,
                ENotificationIconType.Default,
                Color.red);

            Plugin.LogSource.LogInfo($"Player gave up {playerId}");
            return true;
        }

        public static void HealPlayer(Player player)
        {
            try
            {
                ActiveHealthController healthController = player.ActiveHealthController;
                if (healthController == null)
                {
                    Plugin.LogSource.LogError("Could not get ActiveHealthController");
                    return;
                }

                foreach (EBodyPart bodyPart in Enum.GetValues(typeof(EBodyPart)))
                {
                    // EBodyPart.Common is not a real body part and has no dictionary entry.
                    if (!healthController.Dictionary_0.TryGetValue(bodyPart, out var bodyPartState))
                        continue;
                    if (bodyPartState.IsDestroyed)
                    {
                        // Remove bleed from destroyed body parts
                        healthController.method_17(bodyPart);

                        if (Settings.RESTORE_DESTROYED_BODY_PARTS.Value)
                        {
                            // from healthController.FullRestoreBodyPart(bodyPart);
                            // take health down to 25%
                            bodyPartState.IsDestroyed = false;
                            float healingPercent = bodyPartState.Health.Maximum * (Settings.RESTORE_DESTROYED_BODY_PARTS_HEALING.Value / 100f);
                            bodyPartState.Health = new HealthValue(
                                healingPercent,
                                bodyPartState.Health.Maximum
                            );
                            healthController.method_43(bodyPart, 0f, new DamageInfoStruct { DamageType = EDamageType.Undefined });
                            healthController.method_35(EDamageType.Undefined);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"Error applying revival effects: {ex.Message}");
            }
        }

        // Stims ordered cheapest -> most expensive. Used when REQUIRE_STIM is "Any".
        private static readonly string[] StimsCheapestFirst = new[]
        {
            "Adrenaline", "Trimadol", "P22", "AHF1M", "Antidote", "PNB", "MULE",
            "Zagustin", "Obdolbos", "Obdolbos2", "Meldonin", "Perfotoran", "eTGchange",
            "L1", "2A2bTG", "SJ1TGLabs", "SJ6TGLabs", "SJ12_TGLabs", "3bTG", "Propital",
        };

        // Returns the template id of the revive item the player should use, or "" if none.
        // For "Any", picks the cheapest stim currently in inventory.
        private static string ResolveReviveItemId(Player player)
        {
            if (Settings.REQUIRE_STIM.Value == "Any")
            {
                var inRaidItems = player.Inventory.GetPlayerItems(EFT.InventoryLogic.EPlayerItems.Equipment);
                foreach (var stimName in StimsCheapestFirst)
                {
                    var id = FindItemId(stimName);
                    if (id == "") continue;
                    if (inRaidItems.Any(item => item.TemplateId == id))
                        return id;
                }
                return "";
            }

            return FindItemId(Settings.REQUIRE_STIM.Value);
        }

        public static bool hasReviveItem(Player player)
        {
            var reviveItemId = ResolveReviveItemId(player);

            if (reviveItemId == "") return false;

            var inRaidItems = player.Inventory.GetPlayerItems(EFT.InventoryLogic.EPlayerItems.Equipment);

            return inRaidItems.Any(item => item.TemplateId == reviveItemId);
        }

        private static void ConsumeReviveItem(Player player)
        {
            try
            {
                var reviveItemId = ResolveReviveItemId(player);

                if (reviveItemId == "") return;

                var inRaidItems = player.Inventory.GetPlayerItems(EFT.InventoryLogic.EPlayerItems.Equipment);
                var reviveItem = inRaidItems.FirstOrDefault(item => item.TemplateId == reviveItemId);

                if (reviveItem != null)
                {
                    Plugin.LogSource.LogInfo($"Consuming revive item: {FindItemName(reviveItemId)} ({reviveItemId})");

                    GStruct154<GClass3408> gStruct = InteractionsHandlerClass.Discard(reviveItem, player.InventoryController, true);
                    if (gStruct.Failed)
                    {
                        Plugin.LogSource.LogError($"Error consuming item: {gStruct.Error}");
                    }
                    else
                    {
                        player.InventoryController.vmethod_1(
                            new RemoveOperationClass(player.InventoryController.method_12(), player.InventoryController, gStruct.Value),
                            null
                            );

                        Plugin.LogSource.LogInfo($"You have {CountReviveItemsInRaid(player, reviveItemId)} revive items left");
                    }
                }


            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"Error consuming item: {ex.Message}");
            }
        }

        public static int CountReviveItemsInRaid(Player player, string reviveItemId)
        {
            // Initialize a counter for items matching the reviveItemId
            int count = 0;

            // Retrieve all items in raid from the player's inventory
            var inRaidItems = player.Inventory.GetPlayerItems(EFT.InventoryLogic.EPlayerItems.Equipment);

            // Iterate over the items to count matching items
            foreach (var item in inRaidItems)
            {
                if (item.TemplateId == reviveItemId)
                {
                    count++;
                }
            }

            return count;
        }

        public static string FindItemId(string stimName)
        {
            return stimName switch
            {
                "Adrenaline" => "5c10c8fd86f7743d7d706df3",
                "Propital" => "5c0e530286f7747fa1419862",
                "SJ1TGLabs" => "5c0e531286f7747fa54205c2",
                "SJ6TGLabs" => "5c0e531d86f7747fa23f4d42",
                "Zagustin" => "5c0e533786f7747fa23f4d47",
                "eTGchange" => "5c0e534186f7747fa1419867",
                "2A2bTG" => "66507eabf5ddb0818b085b68",
                "3bTG" => "5ed515c8d380ab312177c0fa",
                "AHF1M" => "5ed515f6915ec335206e4152",
                "Antidote" => "5fca138c2a7b221b2852a5c6",
                "L1" => "5ed515e03a40a50460332579",
                "MULE" => "5ed51652f6c34d2cc26336a1",
                "Meldonin" => "5ed5160a87bb8443d10680b5",
                "Obdolbos" => "5ed5166ad380ab312177c100",
                "Obdolbos2" => "637b60c3b7afa97bfc3d7001",
                "P22" => "5ed515ece452db0eb56fc028",
                "PNB" => "637b6179104668754b72f8f5",
                "Perfotoran" => "637b6251104668754b72f8f9",
                "SJ12_TGLabs" => "637b612fb7afa97bfc3d7005",
                "Trimadol" => "637b620db7afa97bfc3d7009",
                _ => "",
            };

        }

        // Reverse of FindItemId: template id -> friendly stim name, or the id if unknown.
        public static string FindItemName(string templateId)
        {
            foreach (var stimName in StimsCheapestFirst)
            {
                if (FindItemId(stimName) == templateId)
                    return stimName;
            }
            return templateId;
        }
    }
}
