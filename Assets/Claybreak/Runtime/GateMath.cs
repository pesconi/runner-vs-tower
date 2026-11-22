using System;
using System.Collections.Generic;
using UnityEngine;

namespace Claybreak
{
    /// <summary>What a math gate does to the units that pass through it.</summary>
    public enum GateOperation
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }

    /// <summary>One math gate: an operation and its operand (data, never a loose constant).</summary>
    [Serializable]
    public struct GateSpec
    {
        public GateOperation operation;
        [Min(1)] public int value;

        public GateSpec(GateOperation operation, int value)
        {
            this.operation = operation;
            this.value = value;
        }

        /// <summary>A gate the player wants: it never lowers the count (x1 and /1 are neutral, not good).</summary>
        public bool IsGood => operation == GateOperation.Add
            || (operation == GateOperation.Multiply && value > 1);

        /// <summary>Longest label: the operation sign and up to 10 digits.</summary>
        public const int MaximumLabelLength = 11;

        /// <summary>The label as text: "+10", "−10", "×2", "÷2" (true minus and times signs, Baloo 2 has them).</summary>
        public string Label
        {
            get
            {
                var buffer = new char[MaximumLabelLength];
                return new string(buffer, 0, WriteLabel(buffer));
            }
        }

        /// <summary>The operation sign of the label.</summary>
        public char Sign
        {
            get
            {
                switch (operation)
                {
                    case GateOperation.Add: return '+';
                    case GateOperation.Subtract: return '−';
                    case GateOperation.Multiply: return '×';
                    default: return '÷';
                }
            }
        }

        /// <summary>
        /// Writes the label into <paramref name="buffer"/> (at least <see cref="MaximumLabelLength"/> long) without
        /// allocating, for TextMeshPro's <c>SetCharArray</c>; returns its length.
        /// </summary>
        public int WriteLabel(char[] buffer)
        {
            if (buffer == null || buffer.Length < MaximumLabelLength)
            {
                throw new ArgumentException("The label buffer needs " + MaximumLabelLength + " characters.", nameof(buffer));
            }

            buffer[0] = Sign;
            // [Min(1)] is only an Inspector hint: a negative operand never writes garbage.
            int number = Math.Max(0, value);
            int digits = 1;
            for (int rest = number / 10; rest > 0; rest /= 10)
            {
                digits++;
            }

            int remaining = number;
            for (int i = digits; i >= 1; i--)
            {
                buffer[i] = (char)('0' + remaining % 10);
                remaining /= 10;
            }

            return digits + 1;
        }
    }

    /// <summary>Pure gate arithmetic, shared by the run gates and the assault gate row.</summary>
    public static class GateMath
    {
        /// <summary>Crowd size after a run gate, clamped to [0, maximum]. 0 means the crowd is gone.</summary>
        public static int Apply(GateSpec gate, int count, int maximum)
        {
            if (maximum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximum), "The crowd limit is never negative.");
            }

            return (int)Math.Max(0L, Math.Min(Promise(gate, count), maximum));
        }

        /// <summary>
        /// What the gate's label promises for <paramref name="count"/> units, before any limit (may pass the crowd
        /// limit; <see cref="Apply"/> is what the crowd really gets). Every operation is non-decreasing in the count,
        /// so the largest promise of a course is the gate applied to the largest army that can reach it.
        /// </summary>
        public static long Promise(GateSpec gate, int count)
        {
            if (count < 0 || gate.value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Counts are never negative and operands are at least 1.");
            }

            switch (gate.operation)
            {
                case GateOperation.Add:
                    return (long)count + gate.value;
                case GateOperation.Subtract:
                    return (long)count - gate.value;
                case GateOperation.Multiply:
                    return (long)count * gate.value;
                default:
                    return count / gate.value;
            }
        }

        /// <summary>
        /// The largest army any run gate of a course promises, over every path (left or right at each pair, every
        /// crate before it picked up), starting with <paramref name="start"/> units under the crowd limit
        /// <paramref name="maximum"/> (RT-6t.14). <paramref name="gateIndex"/> is the pair that promises it (-1 with
        /// no gates). A course is honest when this is at most <paramref name="maximum"/>: no label then shows a
        /// number the crowd cannot reach. Gates are taken in order of distance, crates before the pair past them.
        /// </summary>
        public static long HighestPromise(int start, IReadOnlyList<RunGatePair> gates, IReadOnlyList<CratePlacement> crates,
            int maximum, out int gateIndex)
        {
            gateIndex = -1;
            int army = Math.Max(0, Math.Min(start, maximum));
            long highest = army;
            var taken = new bool[crates == null ? 0 : crates.Count];
            for (int k = 0; gates != null && k < gates.Count; k++)
            {
                army = WithCratesBefore(army, gates[k].z, crates, taken, maximum);
                long promise = Math.Max(Promise(gates[k].left, army), Promise(gates[k].right, army));
                if (gateIndex < 0 || promise > highest)
                {
                    highest = Math.Max(highest, promise);
                    gateIndex = k;
                }

                army = (int)Math.Max(0L, Math.Min(promise, maximum));
            }

            return highest;
        }

        // The largest army after every crate not yet taken that sits before distance z (capped like the crowd).
        private static int WithCratesBefore(int army, float z, IReadOnlyList<CratePlacement> crates, bool[] taken, int maximum)
        {
            for (int i = 0; i < taken.Length; i++)
            {
                if (!taken[i] && crates[i].z < z)
                {
                    taken[i] = true;
                    army = (int)Math.Min((long)army + Math.Max(0, crates[i].units), maximum);
                }
            }

            return army;
        }

        /// <summary>
        /// Assault gates act on a stream of units, one at a time. Returns how many units leave the gate
        /// for the <paramref name="passed"/>-th unit to reach it (1-based): a xN gate turns each unit into
        /// N; a /N gate lets one unit in N through. Add and Subtract have no per-unit meaning and are
        /// rejected by the level validation.
        /// </summary>
        public static int UnitsOut(GateSpec gate, int passed)
        {
            if (passed < 1 || gate.value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(passed));
            }

            switch (gate.operation)
            {
                case GateOperation.Multiply:
                    return gate.value;
                case GateOperation.Divide:
                    return passed % gate.value == 0 ? 1 : 0;
                default:
                    throw new InvalidOperationException("Assault gates only multiply or divide.");
            }
        }

        public static bool IsStreamOperation(GateOperation operation)
        {
            return operation == GateOperation.Multiply || operation == GateOperation.Divide;
        }
    }
}
