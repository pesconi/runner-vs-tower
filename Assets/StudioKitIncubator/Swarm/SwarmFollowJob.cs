using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>
    /// Moves every live unit one frame toward its slot in the sunflower formation (<see cref="SunflowerFormation"/>):
    /// across toward center + offset kept inside the lane, at the unit's own pace; along the run toward its offset.
    /// Positions are (world x, z relative to the swarm center).
    /// </summary>
    [BurstCompile(FloatMode = FloatMode.Fast)]
    public struct SwarmFollowJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float2> Offsets;
        public NativeArray<float2> Positions;
        public float CenterX;
        public float LaneCenter;
        public float LaneHalfWidth;
        public float EdgeMargin;
        public float Follow;
        public float PaceSpread;

        public void Execute(int index)
        {
            float2 offset = Offsets[index];
            float slotX = SunflowerFormation.SlotX(CenterX, offset.x, LaneCenter, LaneHalfWidth, EdgeMargin);
            Positions[index] = SunflowerFormation.Follow(Positions[index], slotX, offset.y, Follow, SunflowerFormation.Pace(index, PaceSpread));
        }
    }
}
