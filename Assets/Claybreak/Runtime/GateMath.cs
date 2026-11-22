using System;
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
            if (count < 0 || maximum < 0 || gate.value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Counts are never negative and operands are at least 1.");
            }

            long result;
            switch (gate.operation)
            {
                case GateOperation.Add:
                    result = (long)count + gate.value;
                    break;
                case GateOperation.Subtract:
                    result = (long)count - gate.value;
                    break;
                case GateOperation.Multiply:
                    result = (long)count * gate.value;
                    break;
                default:
                    result = count / gate.value;
                    break;
            }

            return (int)Math.Max(0L, Math.Min(result, maximum));
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
