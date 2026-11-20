using System;
using JungleDegree.StudioKit.Core;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// The short run's math gates and special-unit crates (from level data). The scene builder
    /// preallocates <see cref="MaximumPairs"/> pair visuals and <see cref="ClaybreakLevel.MaximumCrates"/>
    /// crates; each level places and labels the ones it uses. A pair is crossed when the leader passes its
    /// distance; the side is the half of the track the leader is on. A crate is taken when the leader
    /// passes it on its side. No colliders: distance checks only (no physics queries per frame).
    /// </summary>
    public sealed class RunGateCourse : MonoBehaviour
    {
        public const int MaximumPairs = 5;

        [SerializeField] private Transform[] pairRoots = new Transform[0];
        [SerializeField] private TextMesh[] leftLabels = new TextMesh[0];
        [SerializeField] private TextMesh[] rightLabels = new TextMesh[0];
        [SerializeField] private Renderer[] leftPanels = new Renderer[0];
        [SerializeField] private Renderer[] rightPanels = new Renderer[0];
        [SerializeField] private Transform[] crateRoots = new Transform[0];
        [SerializeField] private TextMesh[] crateLabels = new TextMesh[0];
        [SerializeField] private Material goodMaterial;
        [SerializeField] private Material badMaterial;

        private ClaybreakLevel level;
        private float trackCenterX;
        private float crateOffsetX;
        private bool cratesShown;
        private int addBonus;
        private int next;
        private int nextCrate;

        /// <summary>Raised when the leader crosses a gate: the gate taken.</summary>
        public event Action<GateSpec> GateCrossed;

        /// <summary>Raised when the leader takes a crate: its index in the level and the special units it holds.</summary>
        public event Action<int, int> CratePicked;

        public int Capacity => pairRoots == null ? 0 : pairRoots.Length;
        public int CrateCapacity => crateRoots == null ? 0 : crateRoots.Length;

        /// <summary>Places the level's gates and crates; <paramref name="gateBonus"/> (upgrade) is added to every "+" gate.</summary>
        public bool Configure(ClaybreakLevel levelData, Renderer track, bool showCrates, int gateBonus)
        {
            int capacity = Capacity;
            if (levelData == null || track == null || goodMaterial == null || badMaterial == null
                || leftLabels.Length != capacity || rightLabels.Length != capacity
                || leftPanels.Length != capacity || rightPanels.Length != capacity
                || crateLabels.Length != CrateCapacity || levelData.RunGates.Count > capacity
                || levelData.Crates.Count > CrateCapacity)
            {
                Debug.LogError("RunGateCourse needs its preallocated pair and crate visuals, with room for every level item.", this);
                return false;
            }

            level = levelData;
            cratesShown = showCrates;
            addBonus = Mathf.Max(0, gateBonus);
            Bounds bounds = track.bounds;
            trackCenterX = bounds.center.x;
            crateOffsetX = bounds.size.x * 0.25f;
            for (int i = 0; i < level.RunGates.Count; i++)
            {
                RunGatePair pair = level.RunGates[i];
                Vector3 position = pairRoots[i].position;
                pairRoots[i].position = new Vector3(position.x, position.y, pair.z);
                Dress(leftLabels[i], leftPanels[i], pair.left);
                Dress(rightLabels[i], rightPanels[i], pair.right);
            }

            for (int i = 0; i < level.Crates.Count; i++)
            {
                CratePlacement crate = level.Crates[i];
                Vector3 position = crateRoots[i].position;
                crateRoots[i].position = new Vector3(trackCenterX + (crate.right ? crateOffsetX : -crateOffsetX),
                    position.y, crate.z);
                crateLabels[i].text = "+" + crate.units;
            }

            ResetCourse();
            return true;
        }

        public void ResetCourse()
        {
            next = 0;
            nextCrate = 0;
            for (int i = 0; i < Capacity; i++)
            {
                pairRoots[i].gameObject.SetActive(level != null && i < level.RunGates.Count);
            }

            for (int i = 0; i < CrateCapacity; i++)
            {
                crateRoots[i].gameObject.SetActive(cratesShown && level != null && i < level.Crates.Count);
            }
        }

        /// <summary>Called by the flow during the run with the leader's position.</summary>
        public void Track(Vector3 leader)
        {
            while (level != null && next < level.RunGates.Count && leader.z >= level.RunGates[next].z)
            {
                RunGatePair pair = level.RunGates[next];
                GateSpec taken = Boost(leader.x < trackCenterX ? pair.left : pair.right);
                pairRoots[next].gameObject.SetActive(false);
                next++;
                GameAudio.Play(taken.IsGood ? GameSound.GateGood : GameSound.GateBad);
                GateCrossed?.Invoke(taken);
            }

            while (cratesShown && level != null && nextCrate < level.Crates.Count && leader.z >= level.Crates[nextCrate].z)
            {
                CratePlacement crate = level.Crates[nextCrate];
                bool onRight = leader.x >= trackCenterX;
                if (onRight == crate.right)
                {
                    crateRoots[nextCrate].gameObject.SetActive(false);
                    ClaybreakSfx.Play(ClaybreakSfx.Cue.Crate);
                    CratePicked?.Invoke(nextCrate, crate.units);
                }

                nextCrate++;
            }
        }

        // The gate-value upgrade: "+" gates give more; the other operations stay as designed.
        private GateSpec Boost(GateSpec gate)
        {
            return gate.operation == GateOperation.Add ? new GateSpec(gate.operation, gate.value + addBonus) : gate;
        }

        private void Dress(TextMesh label, Renderer panel, GateSpec spec)
        {
            GateSpec gate = Boost(spec);
            label.text = gate.Label;
            panel.sharedMaterial = gate.IsGood ? goodMaterial : badMaterial;
        }
    }
}
