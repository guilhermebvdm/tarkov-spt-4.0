using UnityEngine;
using UnityEngine.UI;

public class BattleUINoiseLevel : MonoBehaviour
{
	public Image Image;

	public Sprite[] Sprites;

	public void SetNoiseLevel(int index)
	{
		if (index >= 0 && index < Sprites.Length)
		{
			Image.sprite = Sprites[index];
			return;
		}
		Debug.LogErrorFormat("SetNoiseLevel with invalid index {0}", index);
	}
}
