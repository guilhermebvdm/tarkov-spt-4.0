using UnityEngine;

public class GpuGraphTest : MonoBehaviour
{
	private int int_0;

	private Rect rect_0;

	public void Start()
	{
		int_0 = 0;
	}

	public void Update()
	{
		int_0++;
		float width = 0.5f * (float)Screen.width;
		float height = 0.2f * (float)Screen.height;
		float x = 0.2f * (float)Screen.width;
		float y = 0f * (float)Screen.height;
		rect_0 = new Rect(x, y, width, height);
		float num = Time.deltaTime * 1000f;
		if (GraphManager.Graph != null && num < 18f)
		{
			GraphManager.Graph.Plot("Test_WorldSpace", num, Color.green, new GraphManager.GStruct0(base.transform.position, base.transform.rotation, base.transform.localScale));
			GraphManager.Graph.Plot("Test_ScreenSpace", num, Color.green, rect_0);
		}
	}
}
