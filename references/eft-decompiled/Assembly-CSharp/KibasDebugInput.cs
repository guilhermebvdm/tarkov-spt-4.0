using UnityEngine;

public class KibasDebugInput : MonoBehaviour
{
	public Animator targetAnimator;

	public float maxThingIndex;

	public float currentThingIndex;

	public bool doThing;

	private float float_0;

	public void Start()
	{
	}

	public void Update()
	{
		if ((double)float_0 < 0.2)
		{
			doThing = false;
		}
		float_0 += Time.deltaTime;
		if ((double)float_0 > 0.2 && Input.GetKeyDown(KeyCode.Mouse3))
		{
			doThing = true;
			float_0 = 0f;
		}
		if (Input.GetKeyDown(KeyCode.M))
		{
			currentThingIndex -= 1f;
		}
		if (Input.GetKeyDown(KeyCode.N))
		{
			currentThingIndex += 1f;
		}
		if (currentThingIndex > maxThingIndex)
		{
			currentThingIndex = 0f;
		}
		if (currentThingIndex < 0f)
		{
			currentThingIndex = maxThingIndex;
		}
		targetAnimator.SetBool("doThing", doThing);
		targetAnimator.SetFloat("currentThingIndex", currentThingIndex);
	}
}
