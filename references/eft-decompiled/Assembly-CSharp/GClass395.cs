using System;
using EFT;
using UnityEngine;

public class GClass395
{
	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public Vector3? Nullable_0;

	[NonSerialized]
	public float Float_0;

	public void Update(BotOwner _owner)
	{
		if (!_owner.Memory.GoalTarget.HavePlaceTarget() || !_owner.Memory.GoalTarget.GoalTarget.IsCome || Float_0 > Time.time)
		{
			return;
		}
		_owner.BotLight.TurnOn();
		if (_owner.Memory.LastEnemy != null && Time.time - _owner.Memory.LastEnemyTimeSeen < _owner.Settings.FileSettings.Cover.LOOK_LAST_ENEMY_POS_LOOKAROUND)
		{
			_owner.Steering.LookToPoint(_owner.Memory.LastEnemy.EnemyLastPosition);
			return;
		}
		if (Nullable_0.HasValue && Int_0 >= 0 && (double)Time.time - _owner.Memory.GoalTarget.CreatedTime > (double)_owner.Settings.FileSettings.Look.MIN_LOOK_AROUD_TIME)
		{
			_owner.Memory.GoalTarget.PointLookComplete(Int_0);
			Nullable_0 = null;
		}
		if (Nullable_0.HasValue)
		{
			return;
		}
		Int_0 = _owner.Memory.GoalTarget.GoalTarget.GetPointToLook(out Nullable_0);
		if (!Nullable_0.HasValue)
		{
			if ((double)Time.time - _owner.Memory.GoalTarget.CreatedTime > 15.0)
			{
				_owner.BotsGroup.PointChecked(_owner.Memory.GoalTarget.GoalTarget);
				_owner.Memory.GoalTarget.Clear();
			}
			Nullable_0 = null;
			Int_0 = -1;
			Float_0 = 0f;
		}
		else
		{
			_owner.Steering.LookToPoint(Nullable_0.Value, 30f);
			float num = _owner.Settings.FileSettings.Look.LOOK_AROUND_DELTA * (UnityEngine.Random.value + 0.5f);
			Float_0 = num * _owner.Settings.FileSettings.Look.LOOK_AROUND_DELTA + Time.time;
		}
	}
}
