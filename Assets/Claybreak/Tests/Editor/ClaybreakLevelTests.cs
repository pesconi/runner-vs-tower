using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Tests
{
    public sealed class ClaybreakLevelTests
    {
        private ClaybreakLevel level;

        [SetUp]
        public void SetUp()
        {
            level = ScriptableObject.CreateInstance<ClaybreakLevel>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(level);
        }

        [Test]
        public void DefaultLevelFitsAFullPool()
        {
            Assert.IsTrue(level.TryValidate(300, 300, out string error), error);
        }

        [Test]
        public void UnitsInFlightMustFitThePool()
        {
            Assert.IsFalse(level.TryValidate(300, 100, out string error));
            StringAssert.Contains("maximumUnitsInFlight", error);
        }

        // RT-6t.4: the army that starts the run must fit the crowd cap (the instanced crowd, decision D27).
        [Test]
        public void StartingArmyMustFitTheCrowdCap()
        {
            Assert.IsTrue(level.TryValidate(level.StartCount, 300, out string error), error);
            Assert.IsFalse(level.TryValidate(level.StartCount - 1, 300, out error));
            StringAssert.Contains("crowd cap", error);
        }

        [Test]
        public void AssaultGatesMustBeStreamOperations()
        {
            var so = new SerializedObject(level);
            SerializedProperty gate = so.FindProperty("assaultGates").GetArrayElementAtIndex(1);
            gate.FindPropertyRelative("operation").enumValueIndex = (int)GateOperation.Add;
            so.ApplyModifiedPropertiesWithoutUndo();
            Assert.IsFalse(level.TryValidate(300, 300, out string error));
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
            Assert.IsFalse(level.TryValidate(300, 300, out string error));
            StringAssert.Contains("runGates[0]", error);
        }

        [Test]
        public void DefensesAreOrderedAlongTheLane()
        {
            Assert.Less(level.AssaultGateZ, level.WallZ);
            Assert.Less(level.WallZ, level.TowerZ);
            Assert.Less(level.FiringLineZ, level.AssaultGateZ);
        }

        [Test]
        public void CratesKeepClearOfTheGates()
        {
            var so = new SerializedObject(level);
            SerializedProperty gates = so.FindProperty("runGates");
            gates.arraySize = 1;
            gates.GetArrayElementAtIndex(0).FindPropertyRelative("z").floatValue = 20f;
            gates.GetArrayElementAtIndex(0).FindPropertyRelative("left.value").intValue = 1;
            gates.GetArrayElementAtIndex(0).FindPropertyRelative("right.value").intValue = 1;
            SerializedProperty crates = so.FindProperty("crates");
            crates.arraySize = 1;
            crates.GetArrayElementAtIndex(0).FindPropertyRelative("z").floatValue = 21f;
            crates.GetArrayElementAtIndex(0).FindPropertyRelative("units").intValue = 2;
            so.ApplyModifiedPropertiesWithoutUndo();
            Assert.IsFalse(level.TryValidate(300, 300, out string error));
            StringAssert.Contains("crates[0]", error);
        }

        [Test]
        public void SweeperMustSitBetweenTheGateRowAndTheTower()
        {
            var so = new SerializedObject(level);
            so.FindProperty("sweeperEnabled").boolValue = true;
            so.FindProperty("sweeperDistance").floatValue = 5f;
            so.ApplyModifiedPropertiesWithoutUndo();
            Assert.IsFalse(level.TryValidate(300, 300, out string error));
            StringAssert.Contains("sweeper", error);
        }

        [Test]
        public void BossRuleNeedsFloors()
        {
            var so = new SerializedObject(level);
            so.FindProperty("bossRule").enumValueIndex = (int)BossRule.ShiftGates;
            so.FindProperty("floors").intValue = 1;
            so.ApplyModifiedPropertiesWithoutUndo();
            Assert.IsFalse(level.TryValidate(300, 300, out string error));
            StringAssert.Contains("boss rule", error);
        }

        [Test]
        public void ShieldReturnsNeedsTheShield()
        {
            var so = new SerializedObject(level);
            so.FindProperty("bossRule").enumValueIndex = (int)BossRule.ShieldReturns;
            so.FindProperty("floors").intValue = 4;
            so.ApplyModifiedPropertiesWithoutUndo();
            Assert.IsFalse(level.TryValidate(300, 300, out string error));
            StringAssert.Contains("shield", error);
        }

        [Test]
        public void ShieldGeneratorMustSitBetweenTheGateRowAndTheTower()
        {
            var so = new SerializedObject(level);
            so.FindProperty("shieldEnabled").boolValue = true;
            so.FindProperty("shieldDistance").floatValue = 60f;
            so.ApplyModifiedPropertiesWithoutUndo();
            Assert.IsFalse(level.TryValidate(300, 300, out string error));
            StringAssert.Contains("shield", error);
        }

        [Test]
        public void ABossIsALevelWithARule()
        {
            var spec = new LevelSpec { tower = TowerKind.Keep, floors = 3, bossRule = BossRule.TurnGate };
            level.EditorAssign(spec);
            Assert.IsTrue(level.IsBoss);
            Assert.IsTrue(level.TryValidate(300, 300, out string error), error);
            spec.bossRule = BossRule.None;
            level.EditorAssign(spec);
            Assert.IsFalse(level.IsBoss);
        }
    }
}
