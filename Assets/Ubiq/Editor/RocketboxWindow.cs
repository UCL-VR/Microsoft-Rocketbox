using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ubiq.Avatars.Rocketbox
{
    public class RocketboxWindow : EditorWindow
    {
        [MenuItem("Ubiq/Rocketbox Avatar Build Tools")]
        public static void ShowWindow()
        {
            RocketboxWindow wnd = GetWindow<RocketboxWindow>();
            wnd.titleContent = new GUIContent("Rocketbox Avatar Build Tools");
        }

        private List<RuntimePlatform> items = new List<RuntimePlatform>();

        private class BoundEnumField<T> : EnumField where T : System.Enum
        {
            public BoundEnumField(List<T> items, System.Enum defaultValue) : base(defaultValue)
            {
                this.items = items;
                this.RegisterValueChangedCallback(cb =>
                {
                    items[i] = (T)cb.newValue;
                });
            }

            private List<T> items;
            private int i;

            public void SetIndex(int i)
            {
                this.i = i;
                value = items[i];
            }
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            Label configLabel = new Label("Build Configs");
            root.Add(configLabel);

            Toggle facial = new Toggle();
            facial.label = "Facial";
            root.Add(facial);

            Toggle regular = new Toggle();
            regular.label = "Regular";
            root.Add(regular);

            regular.value = true;

            Button configButton = new Button();
            configButton.text = "Build Configs";
            root.Add(configButton);
            configButton.clicked += () =>
            {
                CreateConfigs(new ConfigOptions()
                {
                    facial = facial.value,
                    nonfacial = regular.value
                });

            };

            // VisualElements objects can contain other VisualElement following a tree hierarchy
            Label buildLabel = new Label("Build Bundles");
            root.Add(buildLabel);

            // (We can't use the EnumFlagsField because the BuildTarget enum values are not flags)
            Func<VisualElement> makeItem = () =>
            {
                var item = new BoundEnumField<RuntimePlatform>(items, RuntimePlatform.WindowsPlayer);
                return item;
            };
            Action<VisualElement, int> bindItem = (VisualElement e, int i) =>
            {
                var enumField = (e as BoundEnumField<RuntimePlatform>);
                enumField.SetIndex(i);
            };
            ListView view = new ListView(items, -1, makeItem, bindItem);
            view.showAddRemoveFooter = true;
            view.showBorder = true;
            view.reorderable = true;
            root.Add(view);

            Button button = new Button();
            button.text = "Build";
            root.Add(button);
            button.clicked += () =>
            {
                BuildBundles(items);
            };

            Label manifestLabel = new Label("Build Manifest");
            root.Add(manifestLabel);

            Button manifestButton = new Button();
            manifestButton.text = "Build Manifest";
            root.Add(manifestButton);
            manifestButton.clicked += () =>
            {
                BuildManifest();
            };
        }

        private struct ConfigOptions
        {
            public bool facial;
            public bool nonfacial;

            public bool ShouldBuildAsset(string assetPath)
            {
                if (!assetPath.StartsWith("Assets/Avatars"))
                {
                    return false;
                }

                if (!assetPath.EndsWith(".fbx"))
                {
                    return false;
                }

                if (assetPath.Contains("facial") && !facial)
                {
                    return false;
                }

                if (!assetPath.Contains("facial") && !nonfacial)
                {
                    return false;
                }

                return true;
            }
        }

        private void CreateConfigs(ConfigOptions filter)
        {
            foreach (var path in AssetDatabase.GetAllAssetPaths().Where(filter.ShouldBuildAsset))
            {
                RocketboxEditor.CreateSettingsAsset(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            }
        }

        private void BuildBundles(IEnumerable<RuntimePlatform> platform)
        {
            foreach(var buildTarget in platform)
            {
                BuildBundles(RocketboxHelper.RuntimePlatformToBuildTarget(buildTarget));
            }
        }

        private static void BuildBundles(BuildTarget target)
        {
            var folder = Application.dataPath + "/Build" + "/AssetBundles"+ "/" + target.ToString() + "/";
            System.IO.Directory.CreateDirectory(folder);
            BuildPipeline.BuildAssetBundles(folder, BuildAssetBundleOptions.None, target);
        }

        private static void BuildManifest()
        {
            RocketboxEditor.BuildManifest();
        }
    }
}