using System;
using JungleDegree.StudioKit.Swarm;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// The army on the track (RT-6t.4): an instanced <see cref="SwarmRenderer"/> (no object per unit) whose formation
    /// center is the old leader. It runs forward at the course speed, a finger drag steers the center across the track
    /// (the reference drag: target at the press plus the drag in screen widths, kept inside the track), and the camera
    /// rig follows it. The count is the army itself (there is no leader unit any more). The gates and crates read
    /// <see cref="Position"/>; the assault takes its units from the outer ring (<see cref="TryTakeOuterUnit"/>).
    /// Runs before the flow and the swarm each frame (the flow runs before the swarm too), so the swarm draws where the
    /// crowd is and how many it holds this frame. No allocation while playing.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class RunCrowd : MonoBehaviour
    {
        [SerializeField] private SwarmRenderer swarm;
        [SerializeField] private SwarmCameraRig cameraRig;
        [SerializeField] private SwarmDragSettings drag = SwarmDragSettings.Default;
        [Tooltip("Where the run starts along the track (m).")]
        [SerializeField] private float startZ;

        private bool steering;
        private bool dragging;
        private float pressX;
        private float targetAtPress;

        /// <summary>Raised with the new army when it changes.</summary>
        public event Action<int> ArmyChanged;

        public SwarmRenderer Swarm => swarm;
        public SwarmCameraRig CameraRig => cameraRig;

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

        /// <summary>The formation center on the ground: across where it is now, along where the crowd runs.</summary>
        public Vector3 Position
        {
            get
            {
                Vector3 origin = swarm.transform.position;
                return new Vector3(swarm.CenterX, origin.y, origin.z);
            }
        }

        /// <summary>Center of the track across (the swarm keeps its units inside it).</summary>
        public float TrackCenterX => swarm.transform.position.x;

        /// <summary>Half width of the track the crowd is kept in.</summary>
        public float TrackHalfWidth => swarm.LaneHalfWidth;

        /// <summary>Checks the wiring (the flow calls it once at start).</summary>
        public bool TryValidate(out string error)
        {
            if (swarm == null || cameraRig == null)
            {
                error = "RunCrowd needs its swarm and camera rig.";
                return false;
            }

            if (!swarm.isActiveAndEnabled)
            {
                error = "the crowd swarm did not start (see the log above).";
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
            ArmyChanged?.Invoke(next);
            cameraRig.Snap();
        }

        /// <summary>The same army back to the start of the track, running on (the device gate loops the run).</summary>
        public void RestartRun()
        {
            int army = Army;
            PlaceAtStart();
            swarm.ResetCount(army);
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

        /// <summary>The unit shape the crowd is drawn with (one of the generated crowd shapes).</summary>
        public void UseShape(Mesh shape)
        {
            swarm.SetUnitMesh(shape);
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
            Transform swarmTransform = swarm.transform;
            Vector3 origin = swarmTransform.position;
            swarmTransform.position = new Vector3(origin.x, origin.y, startZ);
            swarm.PlaceCenter(origin.x);
            dragging = false;
        }

        private void Update()
        {
            if (steering)
            {
                ReadDrag();
            }

            if (Speed != 0f)
            {
                Transform swarmTransform = swarm.transform;
                Vector3 origin = swarmTransform.position;
                swarmTransform.position = new Vector3(origin.x, origin.y, origin.z + Speed * Time.deltaTime);
            }
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
