using System;

namespace EFT;

public class LeftStanceController
{
	public enum ELeftStanceBodyActionType
	{
		None,
		DisableAnimLeftStance,
		SetToCacheValue
	}

	[NonSerialized]
	public Action<bool> SetLeftStanceInPlayerAnimator;

	[NonSerialized]
	public Action SetLeftStanceInOneFrameInPlayerAnimator;

	[NonSerialized]
	public Func<bool> CanSetAnimatorLeftStanceToCacheValue;

	[NonSerialized]
	public ELeftStanceBodyActionType LastBodyActionTypeRequest;

	[NonSerialized]
	public bool AllowBodyChangeAnim;

	[NonSerialized]
	public bool AllowBodyChangeAnimOnUnblockFromBodyAction;

	[NonSerialized]
	public bool BlockingLeftStanceOperationInProcess;

	[NonSerialized]
	public bool BlockChangeToLeftStanceFromBodyAction;

	[field: NonSerialized]
	public virtual bool LeftStance { get; set; }

	[field: NonSerialized]
	public virtual bool LastAnimValue { get; set; }

	public LeftStanceController(Action<bool> setLeftStanceInPlayerAnimator, Action setLeftStanceInOneFrameInPlayerAnimator, Func<bool> canSetAnimatorLeftStanceToCacheValue)
	{
		SetLeftStanceInPlayerAnimator = setLeftStanceInPlayerAnimator;
		CanSetAnimatorLeftStanceToCacheValue = canSetAnimatorLeftStanceToCacheValue;
		SetLeftStanceInOneFrameInPlayerAnimator = setLeftStanceInOneFrameInPlayerAnimator;
	}

	public void ToggleLeftStance()
	{
		LeftStance = !LeftStance;
		if (!BlockChangeToLeftStanceFromBodyAction && AllowBodyChangeAnim)
		{
			SetLeftStanceInPlayerAnimator(LeftStance);
			LastAnimValue = LeftStance;
		}
	}

	public void SetLeftStanceForce(bool value)
	{
		LeftStance = value;
		SetLeftStanceInPlayerAnimator(value);
	}

	public void DisableLeftStanceAnimFromBodyAction()
	{
		if (AllowBodyChangeAnim)
		{
			SetLeftStanceInPlayerAnimator(obj: false);
			LastAnimValue = false;
		}
		else
		{
			LastBodyActionTypeRequest = ELeftStanceBodyActionType.DisableAnimLeftStance;
		}
		BlockChangeAnimationFromBodyAction();
	}

	public void DisableLeftStanceAnimFromHandsAction()
	{
		SetLeftStanceInPlayerAnimator(obj: false);
		AllowBodyChangeAnim = false;
		AllowBodyChangeAnimOnUnblockFromBodyAction = false;
		LastAnimValue = false;
		BlockingLeftStanceOperationInProcess = true;
	}

	public void DisableLeftStanceAnimFromOpenInventory()
	{
		SetLeftStanceInPlayerAnimator(obj: false);
		AllowBodyChangeAnim = false;
		AllowBodyChangeAnimOnUnblockFromBodyAction = false;
		LastAnimValue = false;
	}

	public void UnblockChangeAnimationFromBodyAction()
	{
		BlockChangeToLeftStanceFromBodyAction = false;
		if (AllowBodyChangeAnimOnUnblockFromBodyAction)
		{
			AllowBodyChangeAnim = true;
		}
	}

	public void BlockChangeAnimationFromBodyAction()
	{
		BlockChangeToLeftStanceFromBodyAction = true;
	}

	public void SetAnimatorLeftStanceToCacheFromBodyAction(bool forceInOneFrame = false)
	{
		if (!CanSetAnimatorLeftStanceToCacheValue())
		{
			return;
		}
		if (AllowBodyChangeAnim)
		{
			if (LeftStance && forceInOneFrame)
			{
				SetLeftStanceInOneFrameInPlayerAnimator();
			}
			else
			{
				SetLeftStanceInPlayerAnimator(LeftStance);
			}
			LastAnimValue = LeftStance;
		}
		else
		{
			LastBodyActionTypeRequest = ELeftStanceBodyActionType.SetToCacheValue;
		}
		UnblockChangeAnimationFromBodyAction();
	}

	public void SetAnimatorLeftStanceToCacheFromCloseInventory()
	{
		if (!BlockingLeftStanceOperationInProcess)
		{
			SetAnimatorLeftStanceToCacheFromHandsAction();
		}
	}

	public void SetAnimatorLeftStanceToCacheFromHandsAction()
	{
		if (!CanSetAnimatorLeftStanceToCacheValue())
		{
			return;
		}
		if (LastBodyActionTypeRequest != ELeftStanceBodyActionType.None && LastBodyActionTypeRequest != ELeftStanceBodyActionType.SetToCacheValue)
		{
			if (LastBodyActionTypeRequest == ELeftStanceBodyActionType.DisableAnimLeftStance && !BlockChangeToLeftStanceFromBodyAction)
			{
				SetLeftStanceInPlayerAnimator(obj: false);
				LastAnimValue = false;
			}
		}
		else if (!BlockChangeToLeftStanceFromBodyAction)
		{
			SetLeftStanceInPlayerAnimator(LeftStance);
			LastAnimValue = LeftStance;
		}
		if (BlockChangeToLeftStanceFromBodyAction)
		{
			AllowBodyChangeAnimOnUnblockFromBodyAction = true;
			return;
		}
		AllowBodyChangeAnim = true;
		LastBodyActionTypeRequest = ELeftStanceBodyActionType.None;
	}

	public void Clear()
	{
		LeftStance = false;
		SetLeftStanceInPlayerAnimator = null;
		CanSetAnimatorLeftStanceToCacheValue = null;
	}
}
