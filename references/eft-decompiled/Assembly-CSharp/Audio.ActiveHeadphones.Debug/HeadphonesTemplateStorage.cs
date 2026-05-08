using System;
using UnityEngine;

namespace Audio.ActiveHeadphones.Debug;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable objects/Audio/Debug/HeadphonesTemplateStorage", fileName = "HeadphonesTemplateStorage")]
public class HeadphonesTemplateStorage : ScriptableObject
{
	[SerializeField]
	private EditorHeadphonesTemplates _editorHeadphonesTemplates = new EditorHeadphonesTemplates();

	public EditorHeadphonesTemplates Templates => _editorHeadphonesTemplates;
}
