using JungleDegree.StudioKit.Toon;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace JungleDegree.StudioKit.Swarm
{
    /// <summary>
    /// Writes the drawn transform of every live unit for this frame: ground position, hop lift, squash with the
    /// volume kept and the forward lean (rotation about X, top toward +Z). Tint alpha carries the bounce (0..1)
    /// for the blob shadows.
    /// </summary>
    [BurstCompile(FloatMode = FloatMode.Fast)]
    public struct SwarmAnimateJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float2> Positions;
        [ReadOnly] public NativeArray<float> Phases;
        [WriteOnly] public NativeArray<ToonInstance> Instances;
        public SwarmMotionSettings Motion;
        public float3 Tint;
        public float Time;

        public void Execute(int index)
        {
            float bounce = SwarmMotion.Bounce(Time, Phases[index], Motion.Frequency);
            float sy = SwarmMotion.SquashY(bounce, Motion);
            float sxz = SwarmMotion.SquashXZ(sy);
            math.sincos(Motion.ForwardLean, out float sin, out float cos);
            float2 ground = Positions[index];
            // M = T · Rx(lean) · S(sxz, sy, sxz); rows of the 3×4 part, translation in w.
            Instances[index] = new ToonInstance
            {
                Row0 = new float4(sxz, 0f, 0f, ground.x),
                Row1 = new float4(0f, cos * sy, -sin * sxz, bounce * Motion.HopHeight),
                Row2 = new float4(0f, sin * sy, cos * sxz, ground.y),
                Tint = new float4(Tint, bounce),
            };
        }
    }
}
