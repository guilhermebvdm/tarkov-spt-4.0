using UnityEngine;

[ExecuteInEditMode]
public abstract class UpdateInEditorSystemComponent<T> : BaseSystemComponent<T> where T : BaseSystemComponent<T>, GInterface31
{
	public virtual void OnDestroy()
	{
	}

	public abstract void ManualUpdate(float deltaTime);

	public UpdateInEditorSystemComponent()
	{
	}
}
