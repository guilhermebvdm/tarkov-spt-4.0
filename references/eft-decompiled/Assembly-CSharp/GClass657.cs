public class GClass657
{
	public class GClass658
	{
		public string Code;

		public string Message;

		public override string ToString()
		{
			return "[code: " + Code + ", message: " + Message + "]";
		}
	}

	public GClass846 Result;

	public GClass658 Error;

	public string ID;

	public string Method;

	public string Status;

	public override string ToString()
	{
		return $"WSResponse:\n   Id: {ID},\n   Error: {Error}, \n Status: {Status}";
	}
}
