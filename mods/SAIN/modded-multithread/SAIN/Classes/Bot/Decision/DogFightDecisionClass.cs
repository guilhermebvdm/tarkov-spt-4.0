using SAIN.Components;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Decision;

public class DogFightDecisionClass : BotBase
{
    public bool DogFightActive
    {
        get { return _lastDogFightTarget != null; }
    }

    public DogFightDecisionClass(BotComponent bot)
        : base(bot)
    {
        CanEverTick = false;
    }

    public override void Init()
    {
        Bot.EnemyController.Events.OnEnemyRemoved += checkClear;
        base.Init();
    }

    public override void Dispose()
    {
        Bot.EnemyController.Events.OnEnemyRemoved -= checkClear;
        base.Dispose();
    }

    public bool CheckShallDogFight(EnemyList KnownEnemies, out Enemy result)
    {
        // ref: AUD-15-02 - Guarda para KnownEnemies nulo/vazio e null-safety em weaponManager.Reload
        if (KnownEnemies == null || KnownEnemies.Count == 0)
        {
            _lastDogFightTarget = null;
            result = null;
            return false;
        }

        BotWeaponManager weaponManager = BotOwner?.WeaponManager;
        if (weaponManager == null || !weaponManager.HaveBullets || weaponManager.Reload?.Reloading == true)
        {
            _lastDogFightTarget = null;
            result = null;
            return false;
        }
        var decision = Bot.Decision.CurrentCombatDecision;
        switch (decision)
        {
            case ECombatDecision.RushEnemy:
                result = null;
                return false;

            case ECombatDecision.Retreat:
            case ECombatDecision.SeekCover:
                if (decision == ECombatDecision.SeekCover && !Bot.Cover.SprintingToCover)
                {
                    break;
                }
                if (Bot.Decision.SelfActionDecisions.LowOnAmmo(0.3f))
                {
                    result = null;
                    return false;
                }
                break;

            default:
                break;
        }

        if (!KnownEnemies.Contains(_lastDogFightTarget))
        {
            _lastDogFightTarget = null;
        }

        if (_lastDogFightTarget != null)
        {
            if (ShallDogfightEnemy(_lastDogFightTarget))
            {
                result = _lastDogFightTarget;
                return true;
            }
            if (shallClearDogfightTarget(_lastDogFightTarget))
            {
                _lastDogFightTarget = null;
            }
        }

        if (_changeDFTargetTime < Time.time)
        {
            _changeDFTargetTime = Time.time + 0.5f;
            // ref: AUD-03-04 - Busca linear sem delegate/alocação e sem reordenar a lista compartilhada
            Enemy closestEnemy = null;
            float shortestPath = float.MaxValue;

            for (int i = 0; i < KnownEnemies.Count; i++)
            {
                Enemy enemy = KnownEnemies[i];
                if (enemy != null && ShallDogfightEnemy(enemy))
                {
                    float length = enemy.Path.PathLength;
                    if (length < shortestPath)
                    {
                        shortestPath = length;
                        closestEnemy = enemy;
                    }
                }
            }

            if (closestEnemy != null)
            {
                _lastDogFightTarget = closestEnemy;
                result = closestEnemy;
                return true;
            }
        }
        result = _lastDogFightTarget;
        return _lastDogFightTarget != null;
    }

    private bool ShallDogfightEnemy(Enemy enemy)
    {
        return enemy.EnemyKnown
            && enemy.LastKnownPosition != null
            && enemy.Path.PathLength <= Bot.Info.PersonalitySettings.General.DOGFIGHT_PATH_DIST_START
            && (
                (enemy.Seen && enemy.TimeSinceSeen < Bot.Info.PersonalitySettings.General.DOGFIGHT_TIMESINCESEEN_START)
                || enemy.Status.ShotMeRecently
            );
    }

    private bool shallClearDogfightTarget(Enemy enemy)
    {
        if (!enemy.EnemyKnown || enemy.LastKnownPosition == null)
        {
            return true;
        }
        float pathDist = enemy.Path.PathLength;
        var settings = Bot.Info.PersonalitySettings.General;
        if (pathDist > settings.DOGFIGHT_PATH_DIST_END)
        {
            return true;
        }
        return !enemy.IsVisible && enemy.TimeSinceSeen > settings.DOGFIGHT_TIMESINCESEEN_END;
    }

    private float _changeDFTargetTime;

    private Enemy _lastDogFightTarget;

    private void checkClear(string profileID, Enemy enemy)
    {
        if (_lastDogFightTarget != null && _lastDogFightTarget.EnemyProfileId == profileID)
        {
            _lastDogFightTarget = null;
        }
    }
}
