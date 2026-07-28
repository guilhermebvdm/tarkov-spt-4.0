namespace VisceralCombat.Ragdolls.Classes.RootMotion;

public class ShowLargeHeaderIf : ShowIfAttribute
{
	public string name;

	public string color = "white";

	public ShowLargeHeaderIf(string name, string propertyName, object propertyValue = null, object otherPropertyValue = null, bool indent = false, ShowIfMode mode = ShowIfMode.Hidden)
		: base(propertyName, propertyValue, otherPropertyValue, indent, mode)
	{
		this.name = name;
		color = "white";
	}

	public ShowLargeHeaderIf(string name, string color, string propertyName, object propertyValue = null, object otherPropertyValue = null, bool indent = false, ShowIfMode mode = ShowIfMode.Hidden)
		: base(propertyName, propertyValue, otherPropertyValue, indent, mode)
	{
		this.name = name;
		this.color = color;
	}
}
