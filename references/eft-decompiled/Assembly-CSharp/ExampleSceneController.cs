using UnityEngine;

public class ExampleSceneController : MonoBehaviour
{
	public GameObject PlayerFPS;

	public GameObject FreeCam;

	public void Start()
	{
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			bool activeSelf = PlayerFPS.activeSelf;
			PlayerFPS.SetActive(!activeSelf);
			FreeCam.SetActive(activeSelf);
		}
	}
}
