using System;
using JungleDegree.StudioKit.Swarm;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// Sets the hop of the kit's swarm renderers while playing (RT-6t.20: each crowd shape hops its own way, D31 A).
    /// The kit's <see cref="SwarmRenderer"/> and <see cref="LooseSwarmRenderer"/> read their serialized <c>motion</c> every
    /// frame but have no setter yet; until a kit delivery adds one (need registered in docs/plano-de-entregas.md, RT-6t.20),
    /// this overwrites only that serialized field with Unity's <see cref="JsonUtility.FromJsonOverwrite"/> (every other field
    /// is left alone). It runs on a shape change in the menu, never per frame. <c>SwarmMotionBridgeTests</c> fails if the kit
    /// renames the field, so the bridge can never silently stop working.
    /// </summary>
    public static class SwarmMotionBridge
    {
        // The JSON key must be the kit's serialized field name.
        [Serializable]
        private struct Payload
        {
            public SwarmMotionSettings motion;
        }

        public static void Apply(SwarmRenderer renderer, in SwarmMotionSettings motion) => Overwrite(renderer, motion);

        public static void Apply(LooseSwarmRenderer renderer, in SwarmMotionSettings motion) => Overwrite(renderer, motion);

        /// <summary>The hop the renderer draws with now (tests and the device log).</summary>
        public static SwarmMotionSettings Read(MonoBehaviour renderer)
        {
            return JsonUtility.FromJson<Payload>(JsonUtility.ToJson(renderer)).motion;
        }

        private static void Overwrite(MonoBehaviour renderer, in SwarmMotionSettings motion)
        {
            if (renderer != null)
            {
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(new Payload { motion = motion }), renderer);
            }
        }
    }
}
