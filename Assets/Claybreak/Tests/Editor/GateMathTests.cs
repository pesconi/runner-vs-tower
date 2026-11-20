using System;
using NUnit.Framework;

namespace Claybreak.Tests
{
    public sealed class GateMathTests
    {
        [TestCase(GateOperation.Add, 10, 10, 20)]
        [TestCase(GateOperation.Subtract, 8, 10, 2)]
        [TestCase(GateOperation.Subtract, 15, 10, 0)]
        [TestCase(GateOperation.Multiply, 3, 10, 30)]
        [TestCase(GateOperation.Divide, 2, 11, 5)]
        public void RunGateChangesTheArmy(GateOperation operation, int value, int army, int expected)
        {
            Assert.AreEqual(expected, GateMath.Apply(new GateSpec(operation, value), army, 300));
        }

        [Test]
        public void RunGateNeverExceedsThePool()
        {
            Assert.AreEqual(300, GateMath.Apply(new GateSpec(GateOperation.Multiply, 3), 200, 300));
            Assert.AreEqual(300, GateMath.Apply(new GateSpec(GateOperation.Multiply, int.MaxValue), int.MaxValue, 300));
        }

        [Test]
        public void MultiplyGateTurnsEachUnitIntoN()
        {
            var gate = new GateSpec(GateOperation.Multiply, 3);
            for (int passed = 1; passed <= 5; passed++)
            {
                Assert.AreEqual(3, GateMath.UnitsOut(gate, passed));
            }
        }

        [Test]
        public void DivideGateLetsOneInNThrough()
        {
            var gate = new GateSpec(GateOperation.Divide, 2);
            int through = 0;
            for (int passed = 1; passed <= 10; passed++)
            {
                through += GateMath.UnitsOut(gate, passed);
            }

            Assert.AreEqual(5, through);
        }

        [Test]
        public void AssaultGatesRejectAddAndSubtract()
        {
            Assert.Throws<InvalidOperationException>(() => GateMath.UnitsOut(new GateSpec(GateOperation.Add, 2), 1));
            Assert.IsFalse(GateMath.IsStreamOperation(GateOperation.Subtract));
        }

        [Test]
        public void GoodGatesNeverLowerTheCount()
        {
            Assert.IsTrue(new GateSpec(GateOperation.Add, 1).IsGood);
            Assert.IsTrue(new GateSpec(GateOperation.Multiply, 2).IsGood);
            Assert.IsFalse(new GateSpec(GateOperation.Multiply, 1).IsGood);
            Assert.IsFalse(new GateSpec(GateOperation.Divide, 2).IsGood);
            Assert.AreEqual("x2", new GateSpec(GateOperation.Multiply, 2).Label);
        }
    }
}
