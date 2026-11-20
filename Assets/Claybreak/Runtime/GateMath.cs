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

        public string Label
        {
            get
            {
                switch (operation)
                {
                    case GateOperation.Add: return "+" + value;
                    case GateOperation.Subtract: return "-" + value;
                    case GateOperation.Multiply: return "x" + value;
                    default: return "÷" + value;
                }
            }
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
