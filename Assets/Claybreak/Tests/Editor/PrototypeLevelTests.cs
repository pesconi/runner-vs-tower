using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Tests
{
    public sealed class PrototypeLevelTests
    {
        private PrototypeLevel level;

        [SetUp]
        public void SetUp()
        {
            level = ScriptableObject.CreateInstance<PrototypeLevel>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(level);
        }

        [Test]
        public void DefaultLevelFitsAFullPool()
        {
            Assert.IsTrue(level.TryValidate(300, out string error), error);
        }

        [Test]
        public void UnitsInFlightMustFitThePool()
        {
            Assert.IsFalse(level.TryValidate(100, out string error));
            StringAssert.Contains("maximumUnitsInFlight", error);
        }

        [Test]
        public void AssaultGatesMustBeStreamOperations()
        {
            var so = new SerializedObject(level);
            SerializedProperty gate = so.FindProperty("assaultGates").GetArrayElementAtIndex(1);
            gate.FindPropertyRelative("operation").enumValueIndex = (int)GateOperation.Add;
            so.ApplyModifiedPropertiesWithoutUndo();
            Assert.IsFalse(level.TryValidate(300, out string error));
            StringAssert.Contains("assaultGates[1]", error);
        }

        [Test]
        public void RunGatesMustStayBeforeTheFiringLine()
        {
            var so = new SerializedObject(level);
            SerializedProperty gates = so.FindProperty("runGates");
            gates.arraySize = 1;
            gates.GetArrayElementAtIndex(0).FindPropertyRelative("z").floatValue = 200f;
            gates.GetArrayElementAtIndex(0).FindPropertyRelative("left.value").intValue = 1;
            gates.GetArrayElementAtIndex(0).FindPropertyRelative("right.value").intValue = 1;
            so.ApplyModifiedPropertiesWithoutUndo();
            Assert.IsFalse(level.TryValidate(300, out string error));
            StringAssert.Contains("runGates[0]", error);
        }

        [Test]
        public void DefensesAreOrderedAlongTheLane()
        {
            Assert.Less(level.AssaultGateZ, level.WallZ);
            Assert.Less(level.WallZ, level.TowerZ);
            Assert.Less(level.FiringLineZ, level.AssaultGateZ);
        }
    }
}
