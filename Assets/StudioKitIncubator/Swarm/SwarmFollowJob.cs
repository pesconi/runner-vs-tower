using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>
    /// Moves every live unit one frame toward its slot in the sunflower formation (<see cref="SunflowerFormation"/>):
    /// across toward center + offset kept inside the lane, at the unit's own pace; along the run toward its offset.
    /// Positions are (world x, z relative to the swarm center). Also writes the across speed of the frame (m/s; 0 for a
    /// zero step), which leans the unit sideways.
    /// </summary>
    [BurstCompile(FloatMode = FloatMode.Fast)]
    public struct SwarmFollowJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float2> Offsets;
        public NativeArray<float2> Positions;
        [WriteOnly] public NativeArray<float> AcrossSpeeds;
        public float CenterX;
        public float LaneCenter;
        public float LaneHalfWidth;
        public float EdgeMargin;
        public float Follow;
        public float PaceSpread;
        public float DeltaTime;

        public void Execute(int index)
        {
            float2 offset = Offsets[index];
            float2 before = Positions[index];
            float slotX = SunflowerFormation.SlotX(CenterX, offset.x, LaneCenter, LaneHalfWidth, EdgeMargin);
            float2 after = SunflowerFormation.Follow(before, slotX, offset.y, Follow, SunflowerFormation.Pace(index, PaceSpread));
            Positions[index] = after;
            AcrossSpeeds[index] = DeltaTime > 0f ? (after.x - before.x) / DeltaTime : 0f;
        }
    }
}
