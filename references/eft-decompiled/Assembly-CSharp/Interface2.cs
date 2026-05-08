using System.Collections.Generic;
using System.Threading.Tasks;

public interface Interface2
{
	Task<GClass650> WaitResponse(string url, byte[] data, Dictionary<string, string> headers, int timeoutSeconds);
}
