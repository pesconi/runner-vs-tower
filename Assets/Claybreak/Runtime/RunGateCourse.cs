using System;
using JungleDegree.StudioKit.Core;
using TMPro;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// The short run's math gates and special-unit crates (from level data). The scene builder
    /// preallocates <see cref="MaximumPairs"/> pair visuals and <see cref="ClaybreakLevel.MaximumCrates"/>
    /// crates; each level places and labels the ones it uses. RT-6t.13: the crowd's leader decides: a pair is crossed when
    /// the leader passes its distance and the side is the half of the track the leader is on; a crate is taken when the
    /// leader passes it on its side. No colliders: distance checks only (no physics queries per frame).
    /// RT-6t.9: each side is a translucent panel half the track wide (good / bad material) with a TextMeshPro label in
    /// Baloo 2 whose outline preset follows the side (dark green / wine); labels are written without allocating.
    /// RT-6t.13: a crossed pair stays up: the side taken lights (an opaquer panel of its color) and the other goes out (a
    /// faint grey panel, its label faded), so the crowd runs through the gate its leader chose.
    /// </summary>
    public sealed class RunGateCourse : MonoBehaviour
    {
        public const int MaximumPairs = 5;

        [SerializeField] private Transform[] pairRoots = new Transform[0];
        [SerializeField] private TMP_Text[] leftLabels = new TMP_Text[0];
        [SerializeField] private TMP_Text[] rightLabels = new TMP_Text[0];
        [SerializeField] private Renderer[] leftPanels = new Renderer[0];
        [SerializeField] private Renderer[] rightPanels = new Renderer[0];
        [SerializeField] private Transform[] crateRoots = new Transform[0];
        [SerializeField] private TMP_Text[] crateLabels = new TMP_Text[0];
        [SerializeField] private Material goodMaterial;
        [SerializeField] private Material badMaterial;
        [SerializeField] private Material goodLitMaterial;
        [SerializeField] private Material badLitMaterial;
        [SerializeField] private Material offMaterial;
        [Tooltip("Alpha of the label on the side not taken.")]
        [Range(0f, 1f)] [SerializeField] private float offLabelAlpha = 0.3f;
        [SerializeField] private Material goodLabelMaterial;
        [SerializeField] private Material badLabelMaterial;

        private readonly char[] labelBuffer = new char[GateSpec.MaximumLabelLength];

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
                || goodLitMaterial == null || badLitMaterial == null || offMaterial == null
                || goodLabelMaterial == null || badLabelMaterial == null
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
            }

            for (int i = 0; i < level.Crates.Count; i++)
            {
                CratePlacement crate = level.Crates[i];
                Vector3 position = crateRoots[i].position;
                crateRoots[i].position = new Vector3(trackCenterX + (crate.right ? crateOffsetX : -crateOffsetX),
                    position.y, crate.z);
                // Crates are specials, always good: "+n" with the good outline.
                crateLabels[i].SetCharArray(labelBuffer, 0, new GateSpec(GateOperation.Add, Mathf.Max(1, crate.units)).WriteLabel(labelBuffer));
                crateLabels[i].fontSharedMaterial = goodLabelMaterial;
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
                bool used = level != null && i < level.RunGates.Count;
                pairRoots[i].gameObject.SetActive(used);
                if (used)
                {
                    RunGatePair pair = level.RunGates[i];
                    Dress(leftLabels[i], leftPanels[i], pair.left);
                    Dress(rightLabels[i], rightPanels[i], pair.right);
                }
            }

            for (int i = 0; i < CrateCapacity; i++)
            {
                crateRoots[i].gameObject.SetActive(cratesShown && level != null && i < level.Crates.Count);
            }
        }

        /// <summary>Called by the flow during the run with the leader's position (<see cref="RunCrowd.Position"/>).</summary>
        public void Track(Vector3 leader)
        {
            while (level != null && next < level.RunGates.Count && leader.z >= level.RunGates[next].z)
            {
                RunGatePair pair = level.RunGates[next];
                bool right = leader.x >= trackCenterX;
                GateSpec taken = Boost(right ? pair.right : pair.left);
                Light(next, right, taken.IsGood);
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

        private void Dress(TMP_Text label, Renderer panel, GateSpec spec)
        {
            GateSpec gate = Boost(spec);
            label.SetCharArray(labelBuffer, 0, gate.WriteLabel(labelBuffer));
            label.fontSharedMaterial = gate.IsGood ? goodLabelMaterial : badLabelMaterial;
            label.alpha = 1f;
            panel.sharedMaterial = gate.IsGood ? goodMaterial : badMaterial;
        }

        // The side taken lights up in its color, the other goes out (grey panel, faded label).
        private void Light(int pair, bool right, bool good)
        {
            Renderer taken = right ? rightPanels[pair] : leftPanels[pair];
            Renderer other = right ? leftPanels[pair] : rightPanels[pair];
            taken.sharedMaterial = good ? goodLitMaterial : badLitMaterial;
            other.sharedMaterial = offMaterial;
            (right ? leftLabels[pair] : rightLabels[pair]).alpha = offLabelAlpha;
        }

        /// <summary>The panel of one side of a placed pair (tests and captures).</summary>
        public Renderer PanelOf(int pair, bool right)
        {
            return right ? rightPanels[pair] : leftPanels[pair];
        }

        /// <summary>The material a side shows once taken (lit) or passed by (off).</summary>
        public Material LitMaterial(bool good) => good ? goodLitMaterial : badLitMaterial;

        public Material OffMaterial => offMaterial;

        /// <summary>The label shown on one side of a placed pair (tests and captures).</summary>
        public TMP_Text LabelOf(int pair, bool right)
        {
            return right ? rightLabels[pair] : leftLabels[pair];
        }
    }
}
