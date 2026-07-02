using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VertexSiblings.IconStudio
{
    public static class RenderTextureExporter
    {
        private const string MenuRoot = "Tools/Icon Studio/";
        private const string PreferredIconCameraName = "IconCamera";

        private const string AutoSaveFolderRelativeToAssets = "IconStudio/Exports";
        private const string AutoSaveFileTimeFormat = "yyyy-MM-dd_HH-mm-ss";

        [MenuItem(MenuRoot + "Export Icon (Auto)")]
        public static void ExportIconAuto()
        {
            Camera cam = FindBestCameraForExport();
            if (!ValidateCamera(cam))
                return;

            if (!EnsureItemSpawnHasSingleModel())
                return;

            RenderTexture rt = cam.targetTexture;
            if (!ValidateRenderTexture(rt))
                return;

            string suggested = MakeFileNameSafe(rt.name) + ".png";
            string path = EditorUtility.SaveFilePanel("Save Icon PNG", Application.dataPath, suggested, "png");
            if (string.IsNullOrEmpty(path))
                return;

            ExportFromCamera_TrueTransparent(cam, rt, path);
            EditorUtility.RevealInFinder(path);
        }

        [MenuItem(MenuRoot + "Export Icon (Auto Save to Project)")]
        public static void ExportIconAutoSave()
        {
            Camera cam = FindBestCameraForExport();
            if (!ValidateCamera(cam))
                return;

            if (!EnsureItemSpawnHasSingleModel())
                return;

            RenderTexture rt = cam.targetTexture;
            if (!ValidateRenderTexture(rt))
                return;

            string folderAbs = EnsureAutoSaveFolderExists();
            if (string.IsNullOrEmpty(folderAbs))
            {
                EditorUtility.DisplayDialog("IconStudio", "Failed to create Exports folder.", "OK");
                return;
            }

            string safeName = MakeFileNameSafe(rt.name);
            string timestamp = DateTime.Now.ToString(AutoSaveFileTimeFormat);
            string fileName = $"{safeName}_{timestamp}.png";
            string fullPath = Path.Combine(folderAbs, fileName);

            ExportFromCamera_TrueTransparent(cam, rt, fullPath);

            // Import the exported PNG into Unity and prepare it for simple UI use.
            string assetPath = AbsolutePathToAssetPath(fullPath);
            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                ApplyBeginnerFriendlySpriteImportSettings(assetPath);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();

                SelectAndPingAsset(assetPath);
            }

            EditorUtility.RevealInFinder(fullPath);
        }

        [MenuItem(MenuRoot + "Center Model Under ItemSpawn")]
        public static void CenterModelUnderItemSpawn()
        {
            if (!TryGetSingleItemSpawnChild(out Transform child))
                return;

            Undo.RecordObject(child, "Center IconStudio Model Under ItemSpawn");

            child.localPosition = Vector3.zero;

            EditorUtility.SetDirty(child);

            EditorUtility.DisplayDialog(
                "IconStudio",
                "Model centered under ItemSpawn.\n\nYou can now adjust the model position manually for close-ups or portraits.",
                "OK"
            );
        }

        [MenuItem(MenuRoot + "Open Exports Folder")]
        public static void OpenExportsFolder()
        {
            string folderAbs = EnsureAutoSaveFolderExists();
            EditorUtility.RevealInFinder(folderAbs);
        }

        // =========================
        // ITEMSPAWN SAFETY
        // =========================

        private static bool EnsureItemSpawnHasSingleModel()
        {
            return TryGetSingleItemSpawnChild(out _);
        }

        private static bool TryGetSingleItemSpawnChild(out Transform child)
        {
            child = null;

            GameObject itemSpawn = GameObject.Find("ItemSpawn");

            if (itemSpawn == null)
            {
                EditorUtility.DisplayDialog("IconStudio", "ItemSpawn object not found in scene.", "OK");
                return false;
            }

            if (itemSpawn.transform.childCount == 0)
            {
                EditorUtility.DisplayDialog(
                    "IconStudio",
                    "No model found under ItemSpawn.\n\nDrag your model under ItemSpawn before exporting.",
                    "OK"
                );
                return false;
            }

            if (itemSpawn.transform.childCount > 1)
            {
                EditorUtility.DisplayDialog(
                    "IconStudio",
                    "Multiple objects found under ItemSpawn.\n\nPlease keep only one model under ItemSpawn.",
                    "OK"
                );
                return false;
            }

            child = itemSpawn.transform.GetChild(0);
            return true;
        }

        // =========================
        // CAMERA / EXPORT
        // =========================

        private static Camera FindBestCameraForExport()
        {
            if (Selection.activeGameObject != null)
            {
                Camera selectedCam = Selection.activeGameObject.GetComponent<Camera>();
                if (selectedCam != null)
                    return selectedCam;
            }

            Camera[] cams = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            if (cams == null || cams.Length == 0)
                return null;

            foreach (Camera cam in cams)
                if (cam != null && cam.name == PreferredIconCameraName)
                    return cam;

            foreach (Camera cam in cams)
                if (cam != null && cam.targetTexture != null)
                    return cam;

            return cams[0];
        }

        private static bool ValidateCamera(Camera cam)
        {
            if (cam == null)
            {
                EditorUtility.DisplayDialog("IconStudio", "No camera found. Please ensure your scene has an IconCamera.", "OK");
                return false;
            }

            if (cam.targetTexture == null)
            {
                EditorUtility.DisplayDialog("IconStudio", "IconCamera has no Target Texture assigned.", "OK");
                return false;
            }

            return true;
        }

        private static bool ValidateRenderTexture(RenderTexture rt)
        {
            if (rt == null)
                return false;

            if (rt.width <= 0 || rt.height <= 0)
            {
                EditorUtility.DisplayDialog("IconStudio", "RenderTexture size is invalid.", "OK");
                return false;
            }

            return true;
        }

        private static void ExportFromCamera_TrueTransparent(Camera cam, RenderTexture rt, string absolutePath)
        {
            CameraClearFlags prevClearFlags = cam.clearFlags;
            Color prevBg = cam.backgroundColor;
            Material prevSkybox = RenderSettings.skybox;
            RenderTexture prevTarget = cam.targetTexture;
            RenderTexture prevActive = RenderTexture.active;

            try
            {
                RenderSettings.skybox = null;

                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
                cam.targetTexture = rt;

                RenderTexture.active = rt;
                GL.Clear(true, true, new Color(0f, 0f, 0f, 0f));

                cam.Render();

                Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                tex.Apply(false, false);

                byte[] png = tex.EncodeToPNG();
                UnityEngine.Object.DestroyImmediate(tex);

                File.WriteAllBytes(absolutePath, png);
                Debug.Log("[IconStudio] Exported PNG: " + absolutePath);
            }
            finally
            {
                RenderTexture.active = prevActive;
                cam.targetTexture = prevTarget;
                cam.clearFlags = prevClearFlags;
                cam.backgroundColor = prevBg;
                RenderSettings.skybox = prevSkybox;
            }
        }

        // =========================
        // AUTO IMPORT AS SPRITE
        // =========================

        private static void ApplyBeginnerFriendlySpriteImportSettings(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;

            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;

            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Point;

            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 100f;

            EditorUtility.SetDirty(importer);
        }

        private static void SelectAndPingAsset(string assetPath)
        {
            UnityEngine.Object obj =
                AssetDatabase.LoadAssetAtPath<Sprite>(assetPath) as UnityEngine.Object
                ?? AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

            if (obj == null)
                return;

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }

        // =========================
        // PATHS / HELPERS
        // =========================

        private static string EnsureAutoSaveFolderExists()
        {
            string folderAbs = Path.Combine(Application.dataPath, AutoSaveFolderRelativeToAssets);
            if (!Directory.Exists(folderAbs))
                Directory.CreateDirectory(folderAbs);

            return folderAbs;
        }

        private static string AbsolutePathToAssetPath(string absolutePath)
        {
            string dataPath = Application.dataPath.Replace('\\', '/');
            string full = absolutePath.Replace('\\', '/');

            if (!full.StartsWith(dataPath, StringComparison.OrdinalIgnoreCase))
                return null;

            return "Assets" + full.Substring(dataPath.Length);
        }

        private static string MakeFileNameSafe(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "Icon";

            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');

            name = name.Trim().TrimEnd('.');
            return string.IsNullOrEmpty(name) ? "Icon" : name;
        }
    }
}