using EFT;
using SAIN.Components;
using UnityEngine;

namespace SAIN.SAINComponent.Classes;

public class SAINFriendlyFireClass : BotComponentClassBase
{
    public bool ClearShot
    {
        get { return FriendlyFireStatus != FriendlyFireStatus.FriendlyBlock; }
    }

    public FriendlyFireStatus FriendlyFireStatus { get; private set; }

    public SAINFriendlyFireClass(BotComponent sain)
        : base(sain)
    {
        TickRequirement = ESAINTickState.OnlyBotInCombat;
    }

    public override void ManualUpdate()
    {
        if (FriendlyFireStatus == FriendlyFireStatus.FriendlyBlock)
        {
            // ref: AUD-20-01 - Null-safety em BotOwner.ShootData
            BotOwner?.ShootData?.EndShoot();
        }
        base.ManualUpdate();
    }

    public bool UpdateFriendlyFireStatus(Vector3 target, Vector3 weaponFirePort, Vector3 weaponPointDirection, BotComponent bot)
    {
        FriendlyFireStatus = CheckFriendlyFireStatus(target, weaponFirePort, weaponPointDirection, bot);
        return FriendlyFireStatus != FriendlyFireStatus.FriendlyBlock;
    }

    public bool UpdateFriendlyFireStatus(float distance, Vector3 weaponFirePort, Vector3 weaponPointDirection, BotComponent bot)
    {
        FriendlyFireStatus = CheckFriendlyFireStatus(distance, weaponFirePort, weaponPointDirection, bot);
        return FriendlyFireStatus != FriendlyFireStatus.FriendlyBlock;
    }

    public static FriendlyFireStatus CheckFriendlyFireStatus(
        float distance,
        Vector3 weaponFirePort,
        Vector3 weaponPointDirection,
        BotComponent bot
    )
    {
        var members = bot.Squad?.Members;
        if (members == null || members.Count <= 1)
        {
            return FriendlyFireStatus.None;
        }
        return CheckFriendlyFire(weaponFirePort, distance, weaponPointDirection, bot);
    }

    public static FriendlyFireStatus CheckFriendlyFireStatus(
        Vector3 target,
        Vector3 weaponFirePort,
        Vector3 weaponPointDirection,
        BotComponent bot
    )
    {
        var members = bot.Squad?.Members;
        if (members == null || members.Count <= 1)
        {
            return FriendlyFireStatus.None;
        }
        return CheckFriendlyFire(weaponFirePort, (weaponFirePort - target).magnitude, weaponPointDirection, bot);
    }

    private static readonly RaycastHit[] _friendlyFireHitBuffer = new RaycastHit[16];

    public static FriendlyFireStatus CheckFriendlyFire(
        Vector3 weaponFirePort,
        float distance,
        Vector3 weaponPointDirection,
        BotComponent bot
    )
    {
        // ref: AUD-02-03 - SphereCastNonAlloc com buffer fixo estático para zero alocações de GC
        const float sphereCastRadius = 0.2f;
        int count = Physics.SphereCastNonAlloc(
            weaponFirePort,
            sphereCastRadius,
            weaponPointDirection,
            _friendlyFireHitBuffer,
            distance,
            LayerMaskClass.PlayerMask
        );

        if (count == 0)
        {
            return FriendlyFireStatus.None;
        }

        var gameWorld = GameWorldComponent.Instance?.GameWorld;
        if (gameWorld == null)
        {
            return FriendlyFireStatus.None;
        }

        for (int i = 0; i < count; i++)
        {
            var collider = _friendlyFireHitBuffer[i].collider;
            if (collider == null)
            {
                continue;
            }

            Player player = gameWorld.GetPlayerByCollider(collider);
            if (player == null)
            {
                continue;
            }

            if (player.ProfileId == bot.ProfileId)
            {
                continue;
            }

            // ref: AUD-20-01 - Null-safety em bot.EnemyController
            if (bot == null || bot.EnemyController?.IsPlayerAnEnemy(player.ProfileId) == false)
            {
                return FriendlyFireStatus.FriendlyBlock;
            }
        }
        return FriendlyFireStatus.Clear;
    }
}
