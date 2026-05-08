using System.IO;
using System.Text;
using ComponentAce.Compression.Libs.zlib;

public class GClass1456
{
	public static void CompressData(byte[] inData, out byte[] outData)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using ZOutputStream zOutputStream = new ZOutputStream(memoryStream, -1);
		using Stream input = new MemoryStream(inData);
		CopyStream(input, zOutputStream);
		zOutputStream.finish();
		outData = memoryStream.ToArray();
	}

	public static void DecompressData(byte[] inData, out byte[] outData)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using ZOutputStream zOutputStream = new ZOutputStream(memoryStream);
		using Stream input = new MemoryStream(inData);
		CopyStream(input, zOutputStream);
		zOutputStream.finish();
		outData = memoryStream.ToArray();
	}

	public static void CopyStream(Stream input, Stream output)
	{
		byte[] buffer = new byte[2000];
		int count;
		while ((count = input.Read(buffer, 0, 2000)) > 0)
		{
			output.Write(buffer, 0, count);
		}
		output.Flush();
	}

	public static string UnZipString(byte[] inData)
	{
		DecompressData(inData, out var outData);
		return Encoding.UTF8.GetString(outData);
	}
}
