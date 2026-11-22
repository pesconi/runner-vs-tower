using JungleDegree.StudioKit.Swarm;
using JungleDegree.StudioKit.Toon;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace JungleDegree.StudioKit.Tests
{
    public sealed class InstancedPropsTests
    {
        private GameObject owner;
        private Material material;
        private Mesh mesh;

        [SetUp]
        public void SetUp()
        {
            material = new Material(Shader.Find(ToonShaderIds.ToonLitShader));
            material.EnableKeyword(ToonShaderIds.InstancesKeyword);
            material.EnableKeyword(ToonShaderIds.SlotsKeyword);
            mesh = new LowPolyMeshBuilder().AddBox(Matrix4x4.identity, new Color32(255, 0, 0, 0)).ToMesh("prop");
            // Not enabled in edit mode (no OnEnable): only the data side is tested here, nothing is allocated.
            owner = new GameObject("props");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(owner);
            Object.DestroyImmediate(material);
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void SetLayerClampsToTheCapacityAndCountsOneDrawPerShownLayer()
        {
            InstancedProps props = Create(owner.transform, 10, 4);
            Assert.IsNull(props.Validate());
            Assert.AreEqual(0, props.DrawCalls);
            props.SetLayer(0, 99, Color.red, Color.green, Color.blue);
            Assert.AreEqual(10, props.Layer(0).Count, "clamped to the capacity");
            Assert.AreEqual(Color.green, props.Layer(0).Slot1);
            Assert.AreEqual(1, props.DrawCalls);
            props.SetLayer(1, 3, Color.white, Color.white, Color.white);
            Assert.AreEqual(2, props.DrawCalls);
            props.SetLayer(0, -5, Color.white, Color.white, Color.white);
            Assert.AreEqual(0, props.Layer(0).Count);
            Assert.AreEqual(1, props.DrawCalls);
            props.SetLayer(7, 1, Color.white, Color.white, Color.white);
            Assert.AreEqual(1, props.DrawCalls, "an unknown layer is ignored");
        }

        [Test]
        public void PropsStandWhereTheScatterPutsThemForTheAnchor()
        {
            var anchor = new GameObject("anchor").transform;
            anchor.SetParent(owner.transform);
            anchor.position = new Vector3(0f, 9f, 123f);
            InstancedProps props = Create(anchor, 6, 1);
            props.SetLayer(0, 6, Color.white, Color.white, Color.white);
            for (int i = 0; i < 6; i++)
            {
                float3 expected = PropScatter.Place(props.Scatter, 3, 0, i, 123f, new float2(0.5f, 2f)).Position;
                Assert.AreEqual(new Vector3(expected.x, expected.y, expected.z), props.PropPosition(0, i));
            }
        }

        [Test]
        public void ValidateRejectsAMaterialWithoutTheKeywords()
        {
            InstancedProps props = Create(owner.transform, 1, 1);
            material.DisableKeyword(ToonShaderIds.SlotsKeyword);
            StringAssert.Contains(ToonShaderIds.SlotsKeyword, props.Validate());
        }

        private InstancedProps Create(Transform anchor, int capacity0, int capacity1)
        {
            InstancedProps props = owner.AddComponent<InstancedProps>();
            var so = new SerializedObject(props);
            so.FindProperty("anchor").objectReferenceValue = anchor;
            so.FindProperty("material").objectReferenceValue = material;
            so.FindProperty("seed").uintValue = 3;
            SerializedProperty layers = so.FindProperty("layers");
            layers.arraySize = 2;
            for (int i = 0; i < 2; i++)
            {
                SerializedProperty layer = layers.GetArrayElementAtIndex(i);
                layer.FindPropertyRelative("Mesh").objectReferenceValue = mesh;
                layer.FindPropertyRelative("Capacity").intValue = i == 0 ? capacity0 : capacity1;
                layer.FindPropertyRelative("Count").intValue = 0;
                layer.FindPropertyRelative("Scale").vector2Value = new Vector2(0.5f, 2f);
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            return props;
        }
    }
}
