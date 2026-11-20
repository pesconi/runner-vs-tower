using System;
using JungleDegree.StudioKit.Core;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// The short run's math gates (RT-2: 3-5 pairs, from level data). The scene builder preallocates
    /// <see cref="MaximumPairs"/> pair visuals; the level places and labels the ones it uses at start.
    /// A pair is crossed when the leader passes its distance; the side is the half of the track the
    /// leader is on. No colliders: one distance check per frame (no physics queries per frame).
    /// </summary>
    public sealed class RunGateCourse : MonoBehaviour
    {
        public const int MaximumPairs = 5;

        [SerializeField] private Transform[] pairRoots = new Transform[0];
        [SerializeField] private TextMesh[] leftLabels = new TextMesh[0];
        [SerializeField] private TextMesh[] rightLabels = new TextMesh[0];
        [SerializeField] private Renderer[] leftPanels = new Renderer[0];
        [SerializeField] private Renderer[] rightPanels = new Renderer[0];
        [SerializeField] private Material goodMaterial;
        [SerializeField] private Material badMaterial;

        private PrototypeLevel level;
        private float trackCenterX;
        private int next;

        /// <summary>Raised when the leader crosses a gate: the gate taken.</summary>
        public event Action<GateSpec> GateCrossed;

        public int Capacity => pairRoots == null ? 0 : pairRoots.Length;

        public bool Configure(PrototypeLevel levelData, Renderer track)
        {
            int capacity = Capacity;
            if (levelData == null || track == null || goodMaterial == null || badMaterial == null
                || leftLabels.Length != capacity || rightLabels.Length != capacity
                || leftPanels.Length != capacity || rightPanels.Length != capacity
                || levelData.RunGates.Count > capacity)
            {
                Debug.LogError("RunGateCourse needs its preallocated pair visuals and room for every level pair.", this);
                return false;
            }

            level = levelData;
            trackCenterX = track.bounds.center.x;
            for (int i = 0; i < capacity; i++)
            {
                bool used = i < level.RunGates.Count;
                if (!used)
                {
                    continue;
                }

                RunGatePair pair = level.RunGates[i];
                Vector3 position = pairRoots[i].position;
                pairRoots[i].position = new Vector3(position.x, position.y, pair.z);
                Dress(leftLabels[i], leftPanels[i], pair.left);
                Dress(rightLabels[i], rightPanels[i], pair.right);
            }

            ResetCourse();
            return true;
        }

        public void ResetCourse()
        {
            next = 0;
            for (int i = 0; i < Capacity; i++)
            {
                pairRoots[i].gameObject.SetActive(level != null && i < level.RunGates.Count);
            }
        }

        /// <summary>Called by the flow during the run with the leader's position.</summary>
        public void Track(Vector3 leader)
        {
            while (level != null && next < level.RunGates.Count && leader.z >= level.RunGates[next].z)
            {
                RunGatePair pair = level.RunGates[next];
                GateSpec taken = leader.x < trackCenterX ? pair.left : pair.right;
                pairRoots[next].gameObject.SetActive(false);
                next++;
                GameAudio.Play(taken.IsGood ? GameSound.GateGood : GameSound.GateBad);
                GateCrossed?.Invoke(taken);
            }
        }

        private void Dress(TextMesh label, Renderer panel, GateSpec gate)
        {
            label.text = gate.Label;
            panel.sharedMaterial = gate.IsGood ? goodMaterial : badMaterial;
        }
    }
}
