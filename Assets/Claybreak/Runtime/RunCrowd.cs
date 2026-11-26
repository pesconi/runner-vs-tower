using System;
using JungleDegree.StudioKit.Swarm;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// The army on the track (RT-6t.4): an instanced <see cref="SwarmRenderer"/> (no object per unit) led by a
    /// <see cref="CrowdLeader"/> (RT-6t.13). The leader runs forward at the course speed and a finger drag steers it across
    /// the track (the reference drag: target at the press plus the drag in screen widths, kept inside the track); the
    /// sunflower formation follows behind it (its center kept the disc's depth plus a clearance behind the leader, eased
    /// as the army grows or shrinks) and the camera rig follows the formation. The count is the army itself: the leader is
    /// not a unit, never dies at a gate and is never launched. The gates and crates read <see cref="Position"/> (the
    /// leader); the assault takes its units from the outer ring (<see cref="TryTakeOuterUnit"/>).
    /// Runs before the flow and the swarm each frame (the flow runs before the swarm too), so the swarm draws where the
    /// crowd is and how many it holds this frame. No allocation while playing.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class RunCrowd : MonoBehaviour
    {
        [SerializeField] private SwarmRenderer swarm;
        [SerializeField] private SwarmCameraRig cameraRig;
        [SerializeField] private CrowdLeader leader;
        [Tooltip("Gap between the front of the formation and the leader (m).")]
        [Min(0f)] [SerializeField] private float leaderClearance = 0.9f;
        [Tooltip("How fast the formation's distance behind the leader follows the army's size (1/s).")]
        [Min(0.1f)] [SerializeField] private float formationSharpness = 4f;
        [SerializeField] private SwarmDragSettings drag = SwarmDragSettings.Default;
        [Tooltip("Where the run starts along the track (m).")]
        [SerializeField] private float startZ;

        private bool steering;
        private bool dragging;
        private float pressX;
        private float targetAtPress;
        private float leaderZ;
        private float behind;

        /// <summary>Raised with the new army when it changes.</summary>
        public event Action<int> ArmyChanged;

        public SwarmRenderer Swarm => swarm;
        public SwarmCameraRig CameraRig => cameraRig;
        public CrowdLeader Leader => leader;

        /// <summary>Units in the army now.</summary>
        public int Army => swarm.Count;

        /// <summary>The largest army (the swarm capacity: the crowd cap of decision D27).</summary>
        public int MaximumArmy => swarm.Capacity;

        /// <summary>Forward speed along the track (m/s); 0 stands still.</summary>
        public float Speed { get; set; }

        /// <summary>True while a drag steers the crowd (run and assault).</summary>
        public bool Steering
        {
            get => steering;
            set
            {
                steering = value;
                dragging = false;
            }
        }

        /// <summary>
        /// The leader on the ground (RT-6t.13): across where the steering has it now, along where the run is. The gates,
        /// the crates, the firing line and the assault lane read it.
        /// </summary>
        public Vector3 Position => new Vector3(swarm.CenterX, swarm.transform.position.y, leaderZ);

        /// <summary>The formation center on the ground (behind the leader).</summary>
        public Vector3 FormationCenter
        {
            get
            {
                Vector3 origin = swarm.transform.position;
                return new Vector3(swarm.CenterX, origin.y, origin.z);
            }
        }

        /// <summary>How far the formation center stands behind the leader for <paramref name="army"/> units (settled).</summary>
        public float FormationBehind(int army)
        {
            SwarmFormationSettings formation = swarm.Formation;
            return formation.DepthStretch * SunflowerFormation.Radius(army, formation.Spacing) + leaderClearance;
        }

        /// <summary>Center of the track across (the swarm keeps its units inside it).</summary>
        public float TrackCenterX => swarm.transform.position.x;

        /// <summary>Half width of the track the crowd is kept in.</summary>
        public float TrackHalfWidth => swarm.LaneHalfWidth;

        /// <summary>Checks the wiring (the flow calls it once at start).</summary>
        public bool TryValidate(out string error)
        {
            if (swarm == null || cameraRig == null || leader == null)
            {
                error = "RunCrowd needs its swarm, camera rig and leader.";
                return false;
            }

            if (!swarm.isActiveAndEnabled)
            {
                error = "the crowd swarm did not start (see the log above).";
                return false;
            }

            if (!leader.isActiveAndEnabled)
            {
                error = "the crowd leader did not start (see the log above).";
                return false;
            }

            if (swarm.LaneHalfWidth <= drag.EdgeMargin)
            {
                error = $"the crowd lane ({swarm.LaneHalfWidth} m half width) must be wider than the drag margin ({drag.EdgeMargin} m).";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Back to the start of the track with <paramref name="army"/> units standing in their formation slots, centered,
        /// still and not steering; the camera jumps to its settled framing.
        /// </summary>
        public void ResetRun(int army)
        {
            Speed = 0f;
            Steering = false;
            PlaceAtStart();
            int next = Mathf.Clamp(army, 0, MaximumArmy);
            swarm.ResetCount(next);
            SnapFormation();
            ArmyChanged?.Invoke(next);
            cameraRig.Snap();
        }

        /// <summary>The same army back to the start of the track, running on (the device gate loops the run).</summary>
        public void RestartRun()
        {
            int army = Army;
            PlaceAtStart();
            swarm.ResetCount(army);
            SnapFormation();
            cameraRig.Snap();
        }

        /// <summary>
        /// Sets the army (clamped to 0..<see cref="MaximumArmy"/>); new units pop in their slots, removed ones are the outer
        /// ring and vanish or, with <see cref="SwarmRemoval.Die"/> (a bad gate), fly out in the ballistic death.
        /// </summary>
        public void SetArmy(int army, SwarmRemoval removal = SwarmRemoval.Vanish)
        {
            int next = Mathf.Clamp(army, 0, MaximumArmy);
            if (next == swarm.Count)
            {
                return;
            }

            swarm.SetCount(next, removal);
            ArmyChanged?.Invoke(next);
        }

        /// <summary>Takes the outermost unit out of the army; <paramref name="position"/> is where it stood. False when empty.</summary>
        public bool TryTakeOuterUnit(out Vector3 position)
        {
            int army = Army;
            if (army == 0)
            {
                position = default;
                return false;
            }

            position = swarm.UnitGroundPosition(army - 1);
            SetArmy(army - 1);
            return true;
        }

        /// <summary>Body color of every unit (the equipped skin).</summary>
        public void Wear(Color color)
        {
            swarm.SetTint(color);
        }

        /// <summary>The leader's crown color (the world's UI accent).</summary>
        public void CrownLeader(Color color)
        {
            leader.WearCrown(color);
        }

        /// <summary>
        /// The unit shape the crowd (and its leader) is drawn with (one of the generated crowd shapes), with its own hop
        /// (RT-6t.20) and the leader's crown seat.
        /// </summary>
        public void UseShape(CrowdShapeOption shape)
        {
            swarm.SetUnitMesh(shape.mesh);
            SwarmMotionBridge.Apply(swarm, shape.motion);
            leader.UseShape(shape.motion, shape.crownSeat, shape.leaderMesh);
        }

        /// <summary>Shakes the run camera (a bad gate).</summary>
        public void Shake()
        {
            cameraRig.Shake();
        }

        /// <summary>The camera rig follows the crowd again (from its settled framing at once).</summary>
        public void FollowWithCamera()
        {
            cameraRig.ReleaseShot();
        }

        /// <summary>
        /// The camera rig leaves the crowd for a fixed <paramref name="shot"/> of <paramref name="anchor"/> looking toward
        /// <paramref name="target"/> (the assault at the firing line, RT-6t.7), until <see cref="FollowWithCamera"/>.
        /// </summary>
        public void FrameWithCamera(SwarmCameraShot shot, Vector3 anchor, Vector3 target)
        {
            cameraRig.HoldShot(shot, anchor, target);
        }

        private void PlaceAtStart()
        {
            leaderZ = startZ;
            swarm.PlaceCenter(swarm.transform.position.x);
            dragging = false;
        }

        // The formation settled behind the leader for the army now (start, restart): no easing.
        private void SnapFormation()
        {
            behind = FormationBehind(Army);
            PlaceFormation();
            leader.Place(Position, true);
        }

        private void PlaceFormation()
        {
            Transform swarmTransform = swarm.transform;
            Vector3 origin = swarmTransform.position;
            swarmTransform.position = new Vector3(origin.x, origin.y, leaderZ - behind);
        }

        private void Update()
        {
            if (steering)
            {
                ReadDrag();
            }

            leaderZ += Speed * Time.deltaTime;
            // A multiplying gate grows the disc at once: the formation backs off three times faster than it closes up, so
            // the new outer ring does not pass the leader.
            float target = FormationBehind(Army);
            behind = SwarmCameraMath.Smooth(behind, target, target > behind ? formationSharpness * 3f : formationSharpness, Time.deltaTime);
            PlaceFormation();
        }

        // The leader is drawn where the steering has it this frame (the swarm moved its center in its Update).
        private void LateUpdate()
        {
            leader.Place(Position);
        }

        private void ReadDrag()
        {
            if (!PointerInput.TryGetHeld(out Vector2 pointer, out bool pressed))
            {
                dragging = false;
                return;
            }

            if (pressed || !dragging)
            {
                dragging = true;
                pressX = pointer.x;
                targetAtPress = swarm.TargetX;
            }

            swarm.SteerTo(SwarmSteering.DragTarget(targetAtPress, pressX, pointer.x, Screen.width, drag,
                TrackCenterX, TrackHalfWidth));
        }
    }
}
