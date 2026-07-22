#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GltfToUnityLitSafe : EditorWindow
{
    [MenuItem("Tools/Funixia - Seçili Nesneyi Hatasız Lit'e Çevir (Final)")]
    public static void ShowWindow()
    {
        GetWindow<GltfToUnityLitSafe>("Hatasız Çevirici Final");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.HelpBox("Sahnede modelinizi seçin. Bu araç, sharedMaterials kullanarak bellek sızıntısı yapmadan renkleri, dokuları korur ve Unity Lit'e çevirir.", MessageType.Info);

        GUILayout.Space(10);

        if (GUILayout.Button("Seçili Nesnenin Materyallerini Çevir", GUILayout.Height(40)))
        {
            ConvertSelectedModelOptimized();
        }
    }

    private static void ConvertSelectedModelOptimized()
    {
        GameObject selectedObj = Selection.activeGameObject;
        if (selectedObj == null)
        {
            EditorUtility.DisplayDialog("Uyarı", "Lütfen sahnede bir GameObject (Model) seçin!", "Tamam");
            return;
        }

        string folderPath = "Assets/Funixia_ConvertedMaterials";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Funixia_ConvertedMaterials");
        }

        Shader targetShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (targetShader == null)
        {
            EditorUtility.DisplayDialog("Hata", "Projede uygun bir Lit veya Standard shader bulunamadı!", "Tamam");
            return;
        }

        bool isURP = targetShader.name.Contains("Universal");
        string mainTexProp = isURP ? "_BaseMap" : "_MainTex";
        string colorProp = isURP ? "_BaseColor" : "_Color";

        Renderer[] renderers = selectedObj.GetComponentsInChildren<Renderer>(true);
        Dictionary<Material, Material> convertedCache = new Dictionary<Material, Material>();
        int successCount = 0;

        foreach (Renderer rend in renderers)
        {
            // Bellek sızıntısını ve uyarıları önlemek için sharedMaterials kullanıyoruz
            Material[] currentMats = rend.sharedMaterials;
            bool changed = false;

            for (int i = 0; i < currentMats.Length; i++)
            {
                Material oldMat = currentMats[i];
                if (oldMat == null || oldMat.shader == targetShader || oldMat.name.EndsWith("_Lit")) continue;

                if (!convertedCache.TryGetValue(oldMat, out Material newMat))
                {
                    newMat = new Material(targetShader);
                    string safeName = oldMat.name.Replace(":", "_").Replace("/", "_").Replace("\\", "_").Replace("*", "_");
                    newMat.name = safeName + "_Lit";

                    TransferPropertiesRobust(oldMat, newMat, mainTexProp, colorProp);

                    string path = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{newMat.name}.mat");
                    if (string.IsNullOrEmpty(path))
                    {
                        path = $"{folderPath}/Mat_{System.Guid.NewGuid().ToString().Substring(0, 5)}.mat";
                    }

                    AssetDatabase.CreateAsset(newMat, path);
                    convertedCache.Add(oldMat, newMat);
                    successCount++;
                }

                currentMats[i] = newMat;
                changed = true;
            }

            if (changed)
            {
                rend.sharedMaterials = currentMats;
                EditorUtility.SetDirty(rend);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("İşlem Başarılı", $"{successCount} adet materyal bellek sızıntısı olmadan renk ve dokularla Lit formatına çevrildi.", "Harika");
    }

    private static void TransferPropertiesRobust(Material source, Material destination, string mainTexProp, string colorProp)
    {
        // 1. Genişletilmiş Renk Taraması (Tüm olası GLTF renk property'leri)
        Color foundColor = Color.white;
        string[] allColorNames = { "baseColorFactor", "_baseColorFactor", "_BaseColor", "_Color", "color", "_ColorFactor", "diffuseFactor" };
        foreach (var cName in allColorNames)
        {
            try
            {
                if (source.HasProperty(cName))
                {
                    foundColor = source.GetColor(cName);
                    break;
                }
            }
            catch { }
        }
        destination.SetColor(colorProp, foundColor);

        // 2. Genişletilmiş Doku Taraması
        Texture foundTex = null;
        string[] allTexNames = { "baseColorTexture", "_baseColorTexture", "_BaseMap", "_MainTex", "diffuseTexture", "_diffuseTexture", "_BaseColorMap" };
        foreach (var tName in allTexNames)
        {
            try
            {
                if (source.HasProperty(tName))
                {
                    Texture t = source.GetTexture(tName);
                    if (t != null)
                    {
                        foundTex = t;
                        break;
                    }
                }
            }
            catch { }
        }

        if (foundTex != null)
        {
            destination.SetTexture(mainTexProp, foundTex);
        }

        // 3. Normal Map Taraması
        string[] normalNames = { "normalTexture", "_normalTexture", "_BumpMap", "_NormalMap" };
        foreach (var nName in normalNames)
        {
            try
            {
                if (source.HasProperty(nName))
                {
                    Texture nt = source.GetTexture(nName);
                    if (nt != null)
                    {
                        destination.SetTexture("_BumpMap", nt);
                        destination.EnableKeyword("_NORMALMAP");
                        break;
                    }
                }
            }
            catch { }
        }
    }
}
#endif