using System;
using MultiFlare;
using Unity.Collections;
using Unity.Jobs;

public class GClass1028 : GClass1027
{
	[NonSerialized]
	public FlareOverlapSettings FlareOverlapSettings_0;

	[NonSerialized]
	public GClass1030 Gclass1030_1;

	public override bool IsInstant => true;

	public GClass1028(ProFlareAtlas atlas, FlareOverlapSettings settings, GClass1030 lightStorage)
		: base(atlas, settings, lightStorage)
	{
		FlareOverlapSettings_0 = settings;
		Gclass1030_1 = lightStorage;
	}

	public override JobHandle ScheduleInternal(JobHandle dependsOn, NativeList<GStruct74> flares, NativeList<GStruct75> gpuData)
	{
		NativeArray<GStruct75> data = gpuData.AsDeferredJobArray();
		JobHandle jobHandle = IJobParallelForExtensions.Schedule(new GStruct79(Gclass1030_1, flares, FlareOverlapSettings_0, gpuData), flares.Length, 100, dependsOn);
		if (FlareOverlapSettings_0.IsNvEnabled)
		{
			return jobHandle;
		}
		return new GStruct78(data, FlareOverlapSettings_0).Schedule(jobHandle);
	}
}
