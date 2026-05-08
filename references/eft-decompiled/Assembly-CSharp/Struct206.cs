using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

[BurstCompile(OptimizeFor = OptimizeFor.Performance, FloatMode = FloatMode.Fast)]
public struct Struct206 : IJob
{
	[NativeDisableUnsafePtrRestriction]
	public NativeArray<float> LowerCosts;

	[NativeDisableUnsafePtrRestriction]
	public NativeArray<float> LowerDistances;

	[NativeDisableParallelForRestriction]
	public NativeArray<float> BestCost;

	[NativeDisableParallelForRestriction]
	public NativeArray<float> BestDistance;

	public unsafe void Execute()
	{
		float num = float.MaxValue;
		void* unsafeBufferPointerWithoutChecks = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(LowerCosts);
		void* unsafeBufferPointerWithoutChecks2 = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(LowerDistances);
		UnsafeUtility.MemCpyReplicate(unsafeBufferPointerWithoutChecks, &num, 4, LowerCosts.Length);
		UnsafeUtility.MemCpyReplicate(unsafeBufferPointerWithoutChecks2, &num, 4, LowerDistances.Length);
		UnsafeUtility.MemCpyReplicate(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(BestCost), &num, 4, BestCost.Length);
		UnsafeUtility.MemCpyReplicate(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(BestDistance), &num, 4, BestDistance.Length);
	}
}
