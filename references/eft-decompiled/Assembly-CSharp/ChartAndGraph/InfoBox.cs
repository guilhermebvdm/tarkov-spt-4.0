using UnityEngine;
using UnityEngine.UI;

namespace ChartAndGraph;

public class InfoBox : MonoBehaviour
{
	public GraphChartBase[] GraphChart;

	public Text infoText;

	public void method_0(GraphChartBase.GClass1667 args)
	{
		infoText.text = ((args.Magnitude < 0f) ? $"{args.Category} : {args.XString},{args.YString} Clicked" : $"{args.Category} : {args.XString},{args.YString} : Sample Size {args.Magnitude} Clicked");
	}

	public void method_1(GraphChartBase.GClass1667 args)
	{
		infoText.text = ((args.Magnitude < 0f) ? $"{args.Category} : {args.XString},{args.YString}" : $"{args.Category} : {args.XString},{args.YString} : Sample Size {args.Magnitude}");
	}

	public void method_2()
	{
		infoText.text = "";
	}

	public void HookChartEvents()
	{
		if (GraphChart == null)
		{
			return;
		}
		GraphChartBase[] graphChart = GraphChart;
		foreach (GraphChartBase graphChartBase in graphChart)
		{
			if (!(graphChartBase == null))
			{
				graphChartBase.PointClicked.AddListener(method_0);
				graphChartBase.PointHovered.AddListener(method_1);
				graphChartBase.NonHovered.AddListener(method_2);
			}
		}
	}

	public void Start()
	{
		HookChartEvents();
	}
}
