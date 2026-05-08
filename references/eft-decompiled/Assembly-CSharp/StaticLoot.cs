using UnityEngine;

public class StaticLoot : MonoBehaviour, GInterface30
{
	[SerializeField]
	private string _id;

	public string Id => _id;
}
