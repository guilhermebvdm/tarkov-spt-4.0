using System.Collections.Generic;
using UnityEngine;

public class GClass644
{
	public int roundsCountTarget;

	public int roundsCountCurrent;

	public int shotsCountTarget;

	public int shotsCountCurrent;

	public float pauseBetweenShotsTime;

	public float pauseBetweenShotsTimeElapsed;

	public float pauseBetweenRoundsTime;

	public float pauseBetweenRoundsTimeElapsed;

	public float remainingTime;

	public bool pauseBetweenShotOn;

	public bool pauseBetweenRoundOn;

	public bool shellingNotifySended;

	public int shellingBrigadeID;

	public List<Vector3> ShellingPoints = new List<Vector3>();

	public List<Vector2> BoundsPoints = new List<Vector2>();

	public Dictionary<int, GClass642> PlayersInAlertZone = new Dictionary<int, GClass642>();

	public void InitRoundsPause(float time)
	{
		pauseBetweenRoundsTime = time;
		pauseBetweenRoundsTimeElapsed = 0f;
		shotsCountCurrent = 0;
		pauseBetweenRoundOn = true;
		shellingNotifySended = false;
		remainingTime = pauseBetweenRoundsTime;
		roundsCountCurrent++;
	}

	public void CalculateRoundsPause()
	{
		if (pauseBetweenRoundOn)
		{
			if (pauseBetweenRoundsTimeElapsed < pauseBetweenRoundsTime)
			{
				pauseBetweenRoundsTimeElapsed += Time.deltaTime;
				remainingTime = pauseBetweenRoundsTime - pauseBetweenRoundsTimeElapsed;
				return;
			}
			pauseBetweenRoundOn = false;
			remainingTime = 0f;
			shotsCountCurrent = 0;
			pauseBetweenRoundsTimeElapsed = 0f;
		}
	}

	public void InitShotPause(float time)
	{
		pauseBetweenShotsTime = time;
		pauseBetweenShotsTimeElapsed = 0f;
		pauseBetweenShotOn = true;
		shotsCountCurrent++;
	}

	public void CalculateShotsPause()
	{
		if (pauseBetweenShotOn)
		{
			if (pauseBetweenShotsTimeElapsed < pauseBetweenShotsTime)
			{
				pauseBetweenShotsTimeElapsed += Time.deltaTime;
			}
			else
			{
				pauseBetweenShotOn = false;
			}
		}
	}

	public void RefreshProcess()
	{
		roundsCountCurrent = 0;
		shotsCountCurrent = 0;
		pauseBetweenShotOn = false;
		pauseBetweenRoundOn = false;
		shellingNotifySended = false;
		pauseBetweenShotsTimeElapsed = 0f;
		pauseBetweenRoundsTimeElapsed = 0f;
		remainingTime = pauseBetweenRoundsTime;
		PlayersInAlertZone.Clear();
	}
}
