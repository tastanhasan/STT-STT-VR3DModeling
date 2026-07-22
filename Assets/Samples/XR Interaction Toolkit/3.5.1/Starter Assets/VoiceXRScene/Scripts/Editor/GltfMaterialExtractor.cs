#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GltfMaterialExtractor : EditorWindow
{
    [MenuItem("Tools/GLTF Materyallerini Düzenlenebilir Yap")]
    public static void ShowWindow()
    {
        GetWindow<GltfMaterialExtractor>("Materyal Çýkarýcý");
    }

    private void OnGUI()
    {
        GUILayout.Label("Seçili GLTF Modelinin Materyallerini Çýkar", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Sahnede GLTF ile içe aktarýlmýþ modeli seçin ve aþaðýdaki butona basýn. Ayný olan materyaller birleþtirilerek proje klasörüne kaydedilecektir.", MessageType.Info);

        if (GUILayout.Button("Seçili Objede Materyalleri Aktif Et ve Kaydet", GUILayout.Height(40)))
        {
            ExtractMaterialsFromSelected();
        }
    }

    private static void ExtractMaterialsFromSelected()
    {
        GameObject selectedObj = Selection.activeGameObject;
        if (selectedObj == null)
        {
            EditorUtility.DisplayDialog("Hata", "Lütfen sahnede bir GameObject seçin!", "Tamam");
            return;
        }

        MeshRenderer[] renderers = selectedObj.GetComponentsInChildren<MeshRenderer>(true);
        string folderPath = "Assets/ExtractedMaterials";

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "ExtractedMaterials");
        }

        // Çýkartýlan materyalleri takip etmek için Dictionary
        Dictionary<string, Material> extractedMaterials = new Dictionary<string, Material>();
        int extractedCount = 0;

        foreach (var rend in renderers)
        {
            Material[] mats = rend.sharedMaterials;
            bool modified = false;

            for (int i = 0; i < mats.Length; i++)
            {
                Material mat = mats[i];
                if (mat == null) continue;

                // Materyal gömülü mü kontrol et
                string path = AssetDatabase.GetAssetPath(mat);
                if (string.IsNullOrEmpty(path) || path.Contains(".gltf") || path.Contains(".glb"))
                {
                    string matName = mat.name;

                    // Eðer bu materyal (isim bazlý) daha önce çýkartýldýysa, referansýný kullan
                    if (extractedMaterials.ContainsKey(matName))
                    {
                        mats[i] = extractedMaterials[matName];
                        modified = true;
                    }
                    else
                    {
                        // Yeni materyal oluþtur (Ýsme gereksiz index eklemiyoruz)
                        string newMatPath = $"{folderPath}/{matName}.mat";
                        newMatPath = AssetDatabase.GenerateUniqueAssetPath(newMatPath); // Eðer ayný isimde baþka varsa sonuna 1,2 ekler

                        Material newMat = new Material(mat);
                        AssetDatabase.CreateAsset(newMat, newMatPath);

                        // Sözlüðe ekle ve modele ata
                        extractedMaterials.Add(matName, newMat);
                        mats[i] = newMat;
                        extractedCount++;
                        modified = true;
                    }
                }
            }

            // Eðer renderer üzerinde bir deðiþiklik yapýldýysa uygula
            if (modified)
            {
                rend.sharedMaterials = mats;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (extractedCount > 0)
        {
            EditorUtility.DisplayDialog("Baþarýlý", $"{extractedCount} adet tekil GLTF materyali baþarýyla çýkartýldý ve eþleþtirildi!", "Tamam");
        }
        else
        {
            EditorUtility.DisplayDialog("Bilgi", "Çýkartýlacak kilitli materyal bulunamadý.", "Tamam");
        }
    }
}
#endif