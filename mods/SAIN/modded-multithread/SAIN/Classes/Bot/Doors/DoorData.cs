using EFT.Interactive;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Mover;

public struct DoorDataStruct(NavMeshDoorLink link)
{
    private const float DOORS_POSSIBLE_OPEN_DISTANCE_SQR = 3f * 3f;
    private const float DOORS_POSSIBLE_CLOSE_DISTANCE_SQR = 3f * 3f;
    private const float DOOR_INTERACTION_INTERVAL = 3.5f;
    private const float DOOR_OPEN_INTERVAL = 5f;
    private const float DOOR_CLOSE_INTERVAL = 5f;

    public readonly bool CanInteractByTime(float time)
    {
        if (time - LastInteractTime < DOOR_INTERACTION_INTERVAL)
        {
            return false;
        }
        if (time - LastOpenTime < DOOR_OPEN_INTERVAL)
        {
            return false;
        }
        if (time - LastCloseTime < DOOR_CLOSE_INTERVAL)
        {
            return false;
        }
        return true;
    }

    public void OnInteract(EDoorState desiredState)
    {
        LastInteractTime = Time.time;
        switch (desiredState)
        {
            case EDoorState.Open:
                LastOpenTime = Time.time;
                break;
            case EDoorState.Shut:
                LastCloseTime = Time.time;
                break;
        }
    }

    public readonly int Id
    {
        get { return Link.Id; }
    }

    public float LastInteractTime;
    public float LastOpenTime;
    public float LastCloseTime;
    public NavMeshDoorLink Link = link;
    public Door Door = link.Door;
    public float CurrentSqrMagnitude;
    public Vector3 Direction;
    public Vector3 DirectionNormal;

    public void ManualUpdate(Vector3 botPosition)
    {
        Direction = Link.MidClose - botPosition;
        //Direction = CenterPoint - from;
        DirectionNormal = Direction.normalized;
        CurrentSqrMagnitude = Direction.sqrMagnitude;
    }

    public readonly bool InRangeToInteract(Door door)
    {
        return door != null
            && door.DoorState switch
            {
                EDoorState.Open => CurrentSqrMagnitude < DOORS_POSSIBLE_CLOSE_DISTANCE_SQR,
                EDoorState.Shut => CurrentSqrMagnitude < DOORS_POSSIBLE_OPEN_DISTANCE_SQR,
                _ => false,
            };
    }
}
