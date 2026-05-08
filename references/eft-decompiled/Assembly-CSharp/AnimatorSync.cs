using UnityEngine;

public class AnimatorSync : ISyncAble
{
	[SerializeField]
	private Animator _animator;

	public int Int32_0 => 0;

	public void Start()
	{
		if (_animator == null)
		{
			Object.Destroy(this);
		}
	}

	public override void Serialize(GInterface131 writer)
	{
		writer.Write((_animator.IsInTransition(Int32_0) ? _animator.GetNextAnimatorStateInfo(Int32_0) : _animator.GetCurrentAnimatorStateInfo(Int32_0)).fullPathHash);
		method_1(writer);
	}

	public void method_1(GInterface131 writer)
	{
		byte b = (byte)_animator.parameters.Length;
		writer.Write(b);
		for (int i = 0; i < b; i++)
		{
			AnimatorControllerParameter animatorControllerParameter = _animator.parameters[i];
			if (animatorControllerParameter.type == AnimatorControllerParameterType.Int)
			{
				int integer = _animator.GetInteger(animatorControllerParameter.nameHash);
				writer.Write(integer);
			}
			else if (animatorControllerParameter.type == AnimatorControllerParameterType.Float)
			{
				float value = _animator.GetFloat(animatorControllerParameter.nameHash);
				writer.Write(value);
			}
			else if (animatorControllerParameter.type == AnimatorControllerParameterType.Bool)
			{
				bool value2 = _animator.GetBool(animatorControllerParameter.nameHash);
				writer.Write(value2);
			}
		}
	}

	public override void Deserialize(IDataReader readerStream)
	{
		int stateNameHash = readerStream.ReadInt32();
		method_2(readerStream);
		_animator.Play(stateNameHash, Int32_0, 1f);
	}

	public void method_2(IDataReader reader)
	{
		byte b = reader.ReadByte();
		if (b != _animator.parameters.Length)
		{
			Debug.LogError($"{GClass6.GetFullPath(base.transform)} wrong animator parameters count. Expect:{b} Actual {_animator.parameters.Length}");
			return;
		}
		for (int i = 0; i < b; i++)
		{
			AnimatorControllerParameter animatorControllerParameter = _animator.parameters[i];
			if (animatorControllerParameter.type == AnimatorControllerParameterType.Int)
			{
				int value = reader.ReadInt32();
				_animator.SetInteger(animatorControllerParameter.nameHash, value);
			}
			else if (animatorControllerParameter.type == AnimatorControllerParameterType.Float)
			{
				float value2 = reader.ReadFloat();
				_animator.SetFloat(animatorControllerParameter.nameHash, value2);
			}
			else if (animatorControllerParameter.type == AnimatorControllerParameterType.Bool)
			{
				bool value3 = reader.ReadBool();
				_animator.SetBool(animatorControllerParameter.nameHash, value3);
			}
		}
	}
}
