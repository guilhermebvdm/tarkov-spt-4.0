using SAIN.Models.Structs;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.Classes.Search;

public enum EPathCalcFailReason
{
    None,
    NullDestination,
    NoTarget,
    NullPlace,
    TooClose,
    SampleStart,
    SampleEnd,
    CalcPath,
    LastCorner,
}

public class SearchPathFinder : BotSubClass<SAINSearchClass>
{
    public EnemyPlace TargetPlace { get; private set; }
    public bool SearchedTargetPosition
    {
        get { return TargetPlace == null || TargetPlace.HasArrivedPersonal || TargetPlace.HasArrivedSquad; }
    }

    public bool FinishedPeeking { get; set; }

    public SearchPathFinder(SAINSearchClass searchClass)
        : base(searchClass)
    {
        CanEverTick = false;
    }

    public bool HasPathToSearchTarget(Enemy enemy, out string failReason)
    {
        return CheckEnemyPath(enemy, out failReason);
    }

    public void UpdateSearchDestination(Enemy enemy)
    {
        if (!SearchedTargetPosition)
        {
            checkFinishedSearch(enemy);
        }

        if (_nextCheckPosTime < Time.time || SearchedTargetPosition || TargetPlace == null)
        {
            _nextCheckPosTime = Time.time + 4f;
            if (!CheckEnemyPath(enemy, out string failReason))
            {
                //Logger.LogDebug($"Failed to calc path during search for reason: [{failReason}]");
            }
        }
    }

    private void checkFinishedSearch(Enemy enemy)
    {
        if (SearchedTargetPosition)
        {
            return;
        }
        var lastKnown = enemy.KnownPlaces.LastKnownPlace;
        if (lastKnown == null)
        {
            Reset();
            return;
        }
        if (lastKnown.HasArrivedPersonal || lastKnown.HasArrivedSquad)
        {
            Reset();
            return;
        }
        EnemyPlace targetPlace = TargetPlace;
        if (targetPlace == null || targetPlace != lastKnown)
        {
            Reset();
            return;
        }

        var pathToEnemy = enemy.Path.PathToEnemy;
        if (pathToEnemy.corners.Length > 2)
        {
            return;
        }

        float destinationDistance = targetPlace.DistanceToBot;
        if (destinationDistance > 2f)
        {
            return;
        }
        enemy.KnownPlaces.SetPlaceAsSearched(targetPlace);
        Reset();
    }

    public void Reset()
    {
        TargetPlace = null;
        FinishedPeeking = false;
    }

    public bool CheckEnemyPath(Enemy enemy, out string failReason)
    {
        EnemyPlace lastKnownPlace = enemy.KnownPlaces.LastKnownPlace;
        if (lastKnownPlace == null)
        {
            failReason = "lastKnown null";
            return false;
        }
        NavMeshPath path = enemy.Path.PathToEnemy;
        if (path == null || path.status == NavMeshPathStatus.PathInvalid)
        {
            failReason = "path Invalid";
            return false;
        }
        int length = path.corners.Length;
        if (length < 2)
        {
            failReason = "path Invalid corner length";
            return false;
        }
        Vector3 destination = path.corners[length - 1];
        if ((destination - Bot.Position).sqrMagnitude <= 0.33f)
        {
            failReason = "tooClose";
            return false;
        }

        if (
            (destination - lastKnownPlace.Position).sqrMagnitude > 0.5f
            && Physics.SphereCast(destination + Vector3.up, 0.1f, lastKnownPlace.Position - destination, out RaycastHit hit, 1f)
        )
        {
            failReason = "path not complete";
            return false;
        }

        BaseClass.Reset();
        TargetPlace = lastKnownPlace;
        failReason = string.Empty;
        return true;
    }

    private float _nextCheckPosTime;
}
