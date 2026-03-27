#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class VariantRemoval{
    [MenuItem("Tools/Prefabs/Remove ' Variant' in folder")]
    static void RenameFiles()
    {
        // Get selected folder
        Object selected = Selection.activeObject;

        if (selected == null){
            Debug.LogError("No folder selected!");
            return;
        }

        string folderPath = AssetDatabase.GetAssetPath(selected);

        if (!AssetDatabase.IsValidFolder(folderPath)){
            Debug.LogError("Selected object is not a folder!");
            return;
        }

        // Convert to absolute path
        string absolutePath = Path.GetFullPath(folderPath);

        string[] files = Directory.GetFiles(absolutePath);

        foreach (string file in files){
            if (file.EndsWith(".meta"))
                continue;

            string fileName = Path.GetFileName(file);

            if (!fileName.Contains(" Variant"))
                continue;

            string newFileName = fileName.Replace(" Variant", "");
            string newPath = Path.Combine(Path.GetDirectoryName(file), newFileName);

            File.Move(file, newPath);
            Debug.Log($"Renamed: {fileName} → {newFileName}");
        }

        AssetDatabase.Refresh();
    }
}
#endif