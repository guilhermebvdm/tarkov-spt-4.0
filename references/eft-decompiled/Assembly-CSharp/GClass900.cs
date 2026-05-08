using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public abstract class GClass900
{
	[NonSerialized]
	public const int Int_0 = 44;

	public static bool Save(string filename, AudioClip clip)
	{
		if (!filename.ToLower().EndsWith(".wav"))
		{
			filename += ".wav";
		}
		string text = Path.Combine(Application.persistentDataPath, filename);
		Debug.Log(text);
		Directory.CreateDirectory(Path.GetDirectoryName(text));
		using (FileStream fileStream = smethod_0(text))
		{
			smethod_1(fileStream, clip);
			smethod_2(fileStream, clip);
		}
		return true;
	}

	public static AudioClip TrimSilence(AudioClip clip, float min)
	{
		float[] array = new float[clip.samples];
		clip.GetData(array, 0);
		return TrimSilence(new List<float>(array), min, clip.channels, clip.frequency);
	}

	public static AudioClip TrimSilence(List<float> samples, float min, int channels, int hz)
	{
		return TrimSilence(samples, min, channels, hz, _3D: false, stream: false);
	}

	public static AudioClip TrimSilence(List<float> samples, float min, int channels, int hz, bool _3D, bool stream)
	{
		int i;
		for (i = 0; i < samples.Count && !(Mathf.Abs(samples[i]) > min); i++)
		{
		}
		samples.RemoveRange(0, i);
		i = samples.Count - 1;
		while (i > 0 && !(Mathf.Abs(samples[i]) > min))
		{
			i--;
		}
		samples.RemoveRange(i, samples.Count - i);
		AudioClip audioClip = AudioClip.Create("TempClip", samples.Count, channels, hz, _3D, stream);
		audioClip.SetData(samples.ToArray(), 0);
		return audioClip;
	}

	public static FileStream smethod_0(string filepath)
	{
		FileStream fileStream = new FileStream(filepath, FileMode.Create);
		byte value = 0;
		for (int i = 0; i < 44; i++)
		{
			fileStream.WriteByte(value);
		}
		return fileStream;
	}

	public static void smethod_1(FileStream fileStream, AudioClip clip)
	{
		float[] array = new float[clip.samples];
		clip.GetData(array, 0);
		short[] array2 = new short[array.Length];
		byte[] array3 = new byte[array.Length * 2];
		int num = 32767;
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = (short)(array[i] * (float)num);
			BitConverter.GetBytes(array2[i]).CopyTo(array3, i * 2);
		}
		fileStream.Write(array3, 0, array3.Length);
	}

	public static void smethod_2(FileStream fileStream, AudioClip clip)
	{
		int frequency = clip.frequency;
		int channels = clip.channels;
		int samples = clip.samples;
		fileStream.Seek(0L, SeekOrigin.Begin);
		byte[] bytes = Encoding.UTF8.GetBytes("RIFF");
		fileStream.Write(bytes, 0, 4);
		byte[] bytes2 = BitConverter.GetBytes(fileStream.Length - 8L);
		fileStream.Write(bytes2, 0, 4);
		byte[] bytes3 = Encoding.UTF8.GetBytes("WAVE");
		fileStream.Write(bytes3, 0, 4);
		byte[] bytes4 = Encoding.UTF8.GetBytes("fmt ");
		fileStream.Write(bytes4, 0, 4);
		byte[] bytes5 = BitConverter.GetBytes(16);
		fileStream.Write(bytes5, 0, 4);
		byte[] bytes6 = BitConverter.GetBytes((ushort)1);
		fileStream.Write(bytes6, 0, 2);
		byte[] bytes7 = BitConverter.GetBytes(channels);
		fileStream.Write(bytes7, 0, 2);
		byte[] bytes8 = BitConverter.GetBytes(frequency);
		fileStream.Write(bytes8, 0, 4);
		byte[] bytes9 = BitConverter.GetBytes(frequency * channels * 2);
		fileStream.Write(bytes9, 0, 4);
		ushort value = (ushort)(channels * 2);
		fileStream.Write(BitConverter.GetBytes(value), 0, 2);
		byte[] bytes10 = BitConverter.GetBytes((ushort)16);
		fileStream.Write(bytes10, 0, 2);
		byte[] bytes11 = Encoding.UTF8.GetBytes("data");
		fileStream.Write(bytes11, 0, 4);
		byte[] bytes12 = BitConverter.GetBytes(samples * channels * 2);
		fileStream.Write(bytes12, 0, 4);
	}
}
