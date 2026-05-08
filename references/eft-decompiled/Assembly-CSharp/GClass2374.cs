using System.Collections.Generic;

public class GClass2374 : GClass2372
{
	public readonly GClass2370 answer;

	public override GInterface235 SurveyAnswer => answer;

	public GClass2374(GClass2367 questionTemplate)
		: base(questionTemplate)
	{
		answer = new GClass2370();
	}

	public void SetAnswer(List<int> value)
	{
		answer.value = value;
		method_0();
	}
}
