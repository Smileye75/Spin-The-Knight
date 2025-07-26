using UnityEngine;
using UnityEditor;
using System.IO;
#if USING_URP
using UnityEditor.Rendering;
using UnityEditor.Rendering.Universal;
#endif
#if USING_HDRP
using UnityEditor.Rendering;
using UnityEditor.Rendering.HighDefinition;
#endif

namespace AnkleBreaker.Tools
{
    public class MaterialsConverterWindow : EditorWindow
    {
        private string _path;
        private int _conversionOption = 0;
        private string[] _options = new string[] { "Chosen Folder + Subfolders", "Only Chosen Folder" };
        private int _materialsCountTotal;
        private int _materialsCountConvertible;
        private DefaultAsset _targetFolderDefaultAsset;

        private bool _isFirstGUICall = true;
        private int _lastConversionOption;
        
        private RenderPipeline _detectedPipeline = RenderPipeline.Unknown;
        private Shader _urpShader;
        private Shader _hdrpShader;

        private enum RenderPipeline { Builtin, URP, HDRP, Unknown }

        [MenuItem("Tools/AnkleBreaker/Materials Converter (Resolve Pink Materials)")]
        public static void ShowWindow()
        {
            var window = GetWindow<MaterialsConverterWindow>("Materials Converter");
            window.minSize = new Vector2(320, 220);
            
            if (window._targetFolderDefaultAsset == null)
            {
                string defaultFolderPath = "Assets/AB_Stylized";
                window._targetFolderDefaultAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(defaultFolderPath);

                if (window._targetFolderDefaultAsset != null)
                    window._path = AssetDatabase.GetAssetPath(window._targetFolderDefaultAsset);
                else
                    Debug.LogWarning("Default folder 'Assets/AB_Stylized' not found. Please assign a folder manually.");
            }
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Materials Converter Window", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            if (_isFirstGUICall)
                UpdateMaterialsCount();
            
            DefaultAsset currentFolderDefaultAsset = (DefaultAsset)EditorGUILayout.ObjectField("Target Folder", _targetFolderDefaultAsset, typeof(DefaultAsset), false);

            if (currentFolderDefaultAsset != _targetFolderDefaultAsset)
            {
                _targetFolderDefaultAsset = currentFolderDefaultAsset;
                _path = AssetDatabase.GetAssetPath(_targetFolderDefaultAsset);
                EditorGUILayout.LabelField("Path:", _path);
                UpdateMaterialsCount();
            }
            
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);

            _conversionOption = EditorGUILayout.Popup("Conversion Scope", _conversionOption, _options);
            
            if (_lastConversionOption != _conversionOption)
            {
                UpdateMaterialsCount();
                _lastConversionOption = _conversionOption;
            }
            
            string convertLabel = _detectedPipeline switch
            {
                RenderPipeline.URP => "Convert to URP",
                RenderPipeline.HDRP => "Convert to HDRP",
                _ => "Unsupported Render Pipeline"
            };
            
            bool canConvert = _materialsCountConvertible > 0 &&
                              (_detectedPipeline == RenderPipeline.URP || _detectedPipeline == RenderPipeline.HDRP);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Materials in scope : {(canConvert ? _materialsCountConvertible : 0)} convertible materials / {_materialsCountTotal} materials found", EditorStyles.helpBox);
            if (GUILayout.Button("Refresh", GUILayout.Width(70)))
                UpdateMaterialsCount();
            EditorGUILayout.EndHorizontal();
            GUI.enabled = canConvert;
            if (GUILayout.Button(convertLabel, GUILayout.Height(30)))
            {
                if (_detectedPipeline == RenderPipeline.URP)
                    ConvertMaterialsToURP();
                else if (_detectedPipeline == RenderPipeline.HDRP)
                    ConvertMaterialsToHDRP();
                
                UpdateMaterialsCount();
            }
            GUI.enabled = true;
            
            if (_detectedPipeline == RenderPipeline.Unknown)
            {
                EditorGUILayout.HelpBox("Unsupported or unknown render pipeline. URP or HDRP must be active.", MessageType.Error);
            }
            else if (_detectedPipeline == RenderPipeline.Builtin)
            {
                EditorGUILayout.HelpBox("You are in Built-in pipeline. Conversion not needed.", MessageType.Warning);
            }
            else
            {
                string targetName = _detectedPipeline == RenderPipeline.URP ? "URP" : "HDRP";
                EditorGUILayout.HelpBox($"This will replace Built-in 'Standard' shaders with {targetName} shaders and attempt to preserve textures.", MessageType.Info);
            }

            _isFirstGUICall = false;
        }
        
        private void OnEnable()
        {
            InitRenderPipelineVariables();
        }

        void InitRenderPipelineVariables()
        {
            _urpShader = Shader.Find("Universal Render Pipeline/Lit");

            if (_urpShader != null)
            {
                _detectedPipeline = RenderPipeline.URP;
                return;
            }
            
            _hdrpShader = Shader.Find("HDRP/Lit");
            if (_hdrpShader != null)
            {
                _detectedPipeline = RenderPipeline.HDRP;
                return;
            }

            if (Shader.Find("Standard"))
            {
                _detectedPipeline = RenderPipeline.Builtin;
            }
            else
                _detectedPipeline = RenderPipeline.Unknown;
        }
        
        private void UpdateMaterialsCount()
        {
            _materialsCountTotal = 0;
            _materialsCountConvertible = 0;

            if (string.IsNullOrEmpty(_path) || !Directory.Exists(_path))
                return;

            var searchOption = _conversionOption == 0 ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            string[] materialFiles = Directory.GetFiles(_path, "*.mat", searchOption);
            _materialsCountTotal = materialFiles.Length;

            foreach (var matFile in materialFiles)
            {
                var assetPath = matFile.Replace(Application.dataPath, "").Replace("\\", "/");
                var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                if (material != null && material.shader != null && material.shader.name == "Standard")
                {
                    _materialsCountConvertible++;
                }
            }
        }

        private void ConvertMaterialsToURP()
        {
#if USING_URP
            string[] materialFiles = Directory.GetFiles(_path, "*.mat", 
                _conversionOption == 0 ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");

            if (urpShader == null)
            {
                Debug.LogError("URP Shader not found. Ensure URP is installed and configured.");
                return;
            }
            
            var upgrader = new StandardUpgrader("Standard");

            AssetDatabase.StartAssetEditing();

            foreach (string materialFile in materialFiles)
            {
                string relativePath = materialFile.Replace(Application.dataPath, "").Replace('\\', '/');
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(relativePath);
                if (mat == null || mat.shader.name != "Standard") 
                    continue;
                upgrader.Upgrade(mat, MaterialUpgrader.UpgradeFlags.None);
            }

            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
#endif
        }

        private void ConvertMaterialsToHDRP()
        {
#if USING_HDRP
            string[] materialFiles = Directory.GetFiles(_path, "*.mat",
                _conversionOption == 0 ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            Shader hdrpShader = Shader.Find("HDRP/Lit");

            if (hdrpShader == null)
            {
                Debug.LogError("HDRP Shader not found. Ensure HDRP is installed and configured.");
                return;
            }

            var upgrader = new CustomHDRPUpgrader();

            AssetDatabase.StartAssetEditing();

            foreach (string materialFile in materialFiles)
            {
                string relativePath = materialFile.Replace(Application.dataPath, "").Replace('\\', '/');
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(relativePath);
                if (mat == null || mat.shader.name != "Standard")
                    continue;

                upgrader.Upgrade(mat, MaterialUpgrader.UpgradeFlags.None);
                CustomHDRPUpgrader.ManualFinalize(mat);
            }

            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
#endif
        }
        
#if USING_HDRP
        public class CustomHDRPUpgrader : MaterialUpgrader
        {
            public CustomHDRPUpgrader()
            {
                RenameShader("Standard", "HDRP/Lit");

                RenameTexture("_MainTex", "_BaseColorMap");
                RenameTexture("_BumpMap", "_NormalMap");
                RenameTexture("_MetallicGlossMap", "_MaskMap");
                RenameTexture("_OcclusionMap", "_OcclusionMap");
                RenameTexture("_EmissionMap", "_EmissiveColorMap");

                RenameColor("_Color", "_BaseColor");
                RenameFloat("_Glossiness", "_Smoothness");
                RenameFloat("_Metallic", "_Metallic");
                RenameFloat("_OcclusionStrength", "_OcclusionStrength");
                RenameFloat("_BumpScale", "_NormalScale");

            }
            
            public static void ManualFinalize(Material mat)
            {
                if (mat.GetTexture("_NormalMap"))
                    mat.EnableKeyword("_NORMALMAP");

                if (mat.GetTexture("_EmissiveColorMap"))
                {
                    mat.EnableKeyword("_EMISSIVE_COLOR_MAP");
                    mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                    mat.SetColor("_EmissiveColor", Color.white);
                }

                if (mat.GetTexture("_MaskMap"))
                    mat.EnableKeyword("_MASKMAP");

                if (mat.GetTexture("_OcclusionMap"))
                    mat.EnableKeyword("_OCCLUSIONMAP");
            }
        }
#endif
    }
}