using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using System.IO;

namespace Servus.Editor
{
    [InitializeOnLoad]
    public static class CreatePrototype
    {
        const string ScenePath = "Assets/Scenes/Prototype.unity";

        static CreatePrototype()
        {
            if (!Application.isBatchMode)
                EditorApplication.delayCall += () =>
                {
                    if (!File.Exists(ScenePath) && !EditorApplication.isPlayingOrWillChangePlaymode)
                        Generate();
                };
        }

        [MenuItem("Servus/Create prototype arena")]
        public static void Generate()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory("Assets/Materials");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.35f, .4f, .48f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(.06f, .08f, .12f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 25;
            RenderSettings.fogEndDistance = 70;
            var floor = Material("Floor", new Color(.17f, .2f, .25f));
            var wall = Material("Wall", new Color(.3f, .35f, .4f));
            var orange = Material("Orange", new Color(.95f, .3f, .08f));
            var cyan = Material("Cyan", new Color(.08f, .7f, .8f));
            Box("Floor", new Vector3(0, -.5f, 8), new Vector3(32, 1, 40), floor);
            Box("North wall", new Vector3(0, 3, 28), new Vector3(33, 6, 1), wall);
            Box("South wall", new Vector3(0, 3, -12), new Vector3(33, 6, 1), wall);
            Box("West wall", new Vector3(-16, 3, 8), new Vector3(1, 6, 40), wall);
            Box("East wall", new Vector3(16, 3, 8), new Vector3(1, 6, 40), wall);
            Box("Central cover", new Vector3(0, 1.25f, 10), new Vector3(5, 2.5f, 3), wall);
            for (int i = 0; i < 4; i++)
            {
                float height = .3f * (i + 1);
                Box("Step " + i, new Vector3(-9, height / 2f, 3 + i * 1.8f), new Vector3(4, height, 1.8f), orange);
                Box("Pillar " + i, new Vector3(10, 1.5f, i * 6), new Vector3(2, 3, 2), wall);
            }
            Box("Jump platform", new Vector3(-9, .9f, 13), new Vector3(4, 1.8f, 4), cyan);
            for (int i = 0; i < 7; i++)
                Box("Floor marker " + i, new Vector3(0, .015f, -5 + i * 4), new Vector3(.15f, .02f, 1.2f), orange);

            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.3f;
            sun.transform.rotation = Quaternion.Euler(50, -35, 0);
            sun.shadows = LightShadows.Soft;
            var player = new GameObject("Player");
            player.transform.position = new Vector3(0, .1f, -7);
            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = .35f;
            controller.center = new Vector3(0, .9f, 0);
            controller.stepOffset = .35f;
            controller.slopeLimit = 45;
            var camera = new GameObject("Player Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.SetParent(player.transform, false);
            camera.transform.localPosition = new Vector3(0, 1.65f, 0);
            camera.fieldOfView = 85;
            camera.nearClipPlane = .05f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = RenderSettings.fogColor;
            camera.gameObject.AddComponent<AudioListener>();
            player.AddComponent<FirstPersonMotor>().view = camera.transform;
            PlayerSettings.companyName = "servus-de-inferno";
            PlayerSettings.productName = "Servus";
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("SERVUS: Prototype scene generated successfully.");
        }

        static Material Material(string name, Color color)
        {
            string path = "Assets/Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.color = color;
            material.SetFloat("_Glossiness", .15f);
            EditorUtility.SetDirty(material);
            return material;
        }

        static void Box(string name, Vector3 position, Vector3 scale, Material material)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.position = position;
            box.transform.localScale = scale;
            box.GetComponent<Renderer>().sharedMaterial = material;
        }
    }
}
