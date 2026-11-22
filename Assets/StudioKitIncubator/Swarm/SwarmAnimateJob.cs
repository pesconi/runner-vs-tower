using JungleDegree.StudioKit.Toon;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>
    /// Writes the drawn transform of every live unit for this frame: ground position (world x, z relative to
    /// <see cref="CenterZ"/>), hop lift, squash with the volume kept, the forward lean (top toward +Z), the sideways lean
    /// into its across speed and the elastic pop of its birth. Tint alpha carries the blob shadow scale (the shadow
    /// shrinks as the unit goes up and pops with it).
    /// </summary>
    [BurstCompile(FloatMode = FloatMode.Fast)]
    public struct SwarmAnimateJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float2> Positions;
        [ReadOnly] public NativeArray<float> AcrossSpeeds;
        [ReadOnly] public NativeArray<float> Phases;
        [ReadOnly] public NativeArray<float> BornAt;
        [WriteOnly] public NativeArray<ToonInstance> Instances;
        public SwarmMotionSettings Motion;
        public SwarmAnimationSettings Animation;
        public float3 Tint;
        public float Time;
        public float CenterZ;

        public void Execute(int index)
        {
            float bounce = SwarmMotion.Bounce(Time, Phases[index], Motion.Frequency);
            float sy = SwarmMotion.SquashY(bounce, Motion);
            float sxz = SwarmMotion.SquashXZ(sy);
            float birth = SwarmAnimation.BirthScale(Time, BornAt[index], Animation.BirthDuration);
            float tilt = SwarmAnimation.Tilt(AcrossSpeeds[index], Animation);
            float2 ground = Positions[index];
            var instance = new ToonInstance
            {
                Tint = new float4(Tint, SwarmAnimation.ShadowScale(bounce, birth, Animation)),
            };
            SwarmAnimation.Compose(ref instance, new float3(ground.x, bounce * Motion.HopHeight, CenterZ + ground.y),
                Motion.ForwardLean, tilt, sxz * birth, sy * birth);
            Instances[index] = instance;
        }
    }
}
