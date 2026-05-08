public class GClass2375 : GClass2372
{
	public readonly GClass2371 answer;

	public override GInterface235 SurveyAnswer => answer;

	public GClass2375(GClass2367 questionTemplate)
		: base(questionTemplate)
	{
		answer = new GClass2371();
	}

	public void SetAnswer(string value)
	{
		answer.value = value;
		method_0();
	}
}
