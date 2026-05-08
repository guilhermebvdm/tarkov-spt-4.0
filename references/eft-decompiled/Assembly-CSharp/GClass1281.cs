using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

public abstract class GClass1281
{
	public enum StoragePropertyId
	{
		StorageDeviceProperty,
		StorageAdapterProperty,
		StorageDeviceIdProperty,
		StorageDeviceUniqueIdProperty,
		StorageDeviceWriteCacheProperty,
		StorageMiniportProperty,
		StorageAccessAlignmentProperty,
		StorageDeviceSeekPenaltyProperty,
		StorageDeviceTrimProperty,
		StorageDeviceWriteAggregationProperty,
		StorageDeviceDeviceTelemetryProperty,
		StorageDeviceLBProvisioningProperty,
		StorageDevicePowerProperty,
		StorageDeviceCopyOffloadProperty,
		StorageDeviceResiliencyProperty,
		StorageDeviceMediumProductType,
		StorageAdapterRpmbProperty,
		StorageAdapterCryptoProperty,
		StorageDeviceIoCapabilityProperty,
		StorageAdapterProtocolSpecificProperty,
		StorageDeviceProtocolSpecificProperty,
		StorageAdapterTemperatureProperty,
		StorageDeviceTemperatureProperty,
		StorageAdapterPhysicalTopologyProperty,
		StorageDevicePhysicalTopologyProperty,
		StorageDeviceAttributesProperty,
		StorageDeviceManagementStatus,
		StorageAdapterSerialNumberProperty,
		StorageDeviceLocationProperty,
		StorageDeviceNumaProperty,
		StorageDeviceZonedDeviceProperty,
		StorageDeviceUnsafeShutdownCount
	}

	public enum StorageQueryType
	{
		PropertyStandardQuery,
		PropertyExistsQuery,
		PropertyMaskQuery,
		PropertyQueryMaxDefined
	}

	public struct Struct249
	{
		public int Version;

		public int Size;

		public bool IncursSeekPenalty;
	}

	public struct Struct250
	{
		public StoragePropertyId PropertyId;

		public StorageQueryType QueryType;

		public byte AdditionalParameters;
	}

	[NonSerialized]
	public const int Int_0 = 2954240;

	[NonSerialized]
	public const uint Uint_0 = 1u;

	[NonSerialized]
	public const uint Uint_1 = 2u;

	[NonSerialized]
	public const uint Uint_2 = 3u;

	[NonSerialized]
	public const uint Uint_3 = 128u;

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public unsafe static extern bool DeviceIoControl([In] SafeFileHandle hDevice, [In] int dwIoControlCode, [In] void* lpInBuffer, [In] int nInBufferSize, [Out] void* lpOutBuffer, [In] int nOutBufferSize, out int lpBytesReturned, [In] IntPtr lpOverlapped);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern SafeFileHandle CreateFileW([MarshalAs(UnmanagedType.LPWStr)] string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

	public unsafe static bool IsSsd(string driveLetter)
	{
		string text = "\\\\.\\" + driveLetter + ":";
		SafeFileHandle safeFileHandle = CreateFileW(text, 0u, 3u, IntPtr.Zero, 3u, 128u, IntPtr.Zero);
		if (safeFileHandle.IsClosed | safeFileHandle.IsInvalid)
		{
			throw new IOException("Error opening drive " + text);
		}
		Struct250 structure = new Struct250
		{
			PropertyId = StoragePropertyId.StorageDeviceSeekPenaltyProperty,
			QueryType = StorageQueryType.PropertyStandardQuery
		};
		Struct249 structure2 = default(Struct249);
		if (!DeviceIoControl(safeFileHandle, 2954240, &structure, Marshal.SizeOf(structure), &structure2, Marshal.SizeOf(structure2), out var _, IntPtr.Zero))
		{
			throw new IOException("Could not determine whether the disk is " + text + " SSD");
		}
		return !structure2.IncursSeekPenalty;
	}
}
