using EFT;
using EFT.Interactive;
using SAIN.Components;
using UnityEngine;

namespace SAIN.Layers.Extract;

public class SAINLootingBotsIntegration
{
    public SAINLootingBotsIntegration(BotOwner owner, BotComponent sain)
    {
        SAIN = sain;
        BotOwner = owner;
        randomizationFactor = Random.Range(0.75f, 1.25f);
    }

    public void Update()
    {
        UpdateLootingBotsInfo();
        CheckStatus();
    }

    private readonly BotOwner BotOwner;
    private readonly BotComponent SAIN;

    private void CheckStatus()
    {
        if (FullOnLoot || !CanExtractFromLootValue())
        {
            return;
        }

        Logger.LogInfo($"[{BotOwner.name}] Is Moving to Extract because because they are Full on loot. Net Loot Value: {NetLootValue}");
        FullOnLoot = true;
    }

    private bool CanExtractFromLootValue()
    {
        if (NetLootValue >= MinLootValException)
        {
            return true;
        }
        return FullInventory && NetLootValue >= GetMinNetLootValue();
    }

    private float GetMinNetLootValue()
    {
        if (SAIN.Info.Profile.IsPMC)
        {
            return MinLootValPMC * randomizationFactor;
        }
        if (SAIN.Info.Profile.IsScav)
        {
            return MinLootValSCAV * randomizationFactor;
        }
        return MinLootValOther * randomizationFactor;
    }

    private void UpdateLootingBotsInfo()
    {
        if (UpdateInfoTimer < Time.time)
        {
            UpdateInfoTimer = Time.time + 5f;
            NetLootValue = LootingBots.LootingBotsInterop.GetNetLootValue(BotOwner);
            if (NetLootValue != 0)
            {
                //Logger.LogWarning(NetLootValue);
            }
            FullInventory = LootingBots.LootingBotsInterop.CheckIfInventoryFull(BotOwner);
        }
    }

    private int GetItemPrice(LootItem item)
    {
        float price = LootingBots.LootingBotsInterop.GetItemPrice(item);
        return Mathf.RoundToInt(price);
    }

    public bool FullOnLoot { get; private set; }
    public float NetLootValue { get; private set; }
    public bool FullInventory { get; private set; }

    private float UpdateInfoTimer;

    private float randomizationFactor;
    private float MinLootValPMC => SAINPlugin.LoadedPreset.GlobalSettings.General.LootingBots.MinLootValPMC;
    private float MinLootValSCAV => SAINPlugin.LoadedPreset.GlobalSettings.General.LootingBots.MinLootValSCAV;
    private float MinLootValOther => SAINPlugin.LoadedPreset.GlobalSettings.General.LootingBots.MinLootValOther;
    private float MinLootValException => SAINPlugin.LoadedPreset.GlobalSettings.General.LootingBots.MinLootValException;
}
