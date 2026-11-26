using JungleDegree.StudioKit.Swarm;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// The Bibopzi's face in play (RT-6v3.4b): the flow reports what happens (<see cref="Trigger"/>), the guide's table
    /// (<see cref="StyleGuide.Faces"/>) through a <see cref="FaceDirector"/> picks the face, and this writes it into the
    /// kit's swarms once per frame when it changes: the run crowd (its leader reads the same expression), the units in
    /// flight and the units the obstacles throw out. The other shapes have no face mask: the cell changes nothing on them,
    /// the hands do not exist, so the same calls are harmless.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class CrowdFace : MonoBehaviour
    {
        [SerializeField] private StyleGuide guide;
        [SerializeField] private SwarmRenderer crowd;
        [SerializeField] private LooseSwarmRenderer flyers;
        [SerializeField] private LooseSwarmRenderer obstacleDeaths;

        private FaceDirector director;
        private SwarmExpression shownCrowd;
        private SwarmExpression shownFlyers;
        private bool shown;

        /// <summary>The director (tests and the device log); null until enabled.</summary>
        public FaceDirector Director => director;

        public string Validate()
        {
            if (guide == null || crowd == null || flyers == null || obstacleDeaths == null)
            {
                return "CrowdFace needs the style guide, the crowd, the units in flight and the obstacle deaths.";
            }

            string error = guide.Faces?.Validate();
            return error == null ? null : "CrowdFace: " + error;
        }

        /// <summary>Starts a level: the run face, no reaction.</summary>
        public void ResetFace()
        {
            director?.Reset(Time.time);
            Apply();
        }

        public void Trigger(FaceEvent trigger)
        {
            if (director != null && director.Trigger(trigger, Time.time))
            {
                Apply();
            }
        }

        private void Awake()
        {
            string error = Validate();
            if (error != null)
            {
                Debug.LogError("[Face] " + error, this);
                enabled = false;
                return;
            }

            director = new FaceDirector(guide.Faces);
        }

        // Before the swarms read their expression (they schedule their jobs in Update).
        private void Update()
        {
            Apply();
        }

        private void Apply()
        {
            if (director == null)
            {
                return;
            }

            SwarmExpression crowdFace = director.Crowd(Time.time);
            SwarmExpression flying = director.Flyers();
            if (shown && Same(crowdFace, shownCrowd) && Same(flying, shownFlyers))
            {
                return;
            }

            crowd.SetExpression(crowdFace);
            flyers.Expression = flying;
            obstacleDeaths.Expression = crowdFace;
            shownCrowd = crowdFace;
            shownFlyers = flying;
            shown = true;
        }

        private static bool Same(in SwarmExpression a, in SwarmExpression b)
        {
            return a.Cell == b.Cell && a.HandsOpen == b.HandsOpen && a.DyingCell == b.DyingCell && a.DyingHandsOpen == b.DyingHandsOpen;
        }
    }
}
