#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

public static class TextureColorUtility
{
    const int LOWERBOUND  = 10;
    const int UPPERBOUND = 255;
    [MenuItem("Tools/Textures/find best chroma key")]
    public static Color FindUnusedColorInSelectedFolder()
    {
        // Get selected folder
        string folderPath = GetSelectedFolderPath();
        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.LogError("No folder selected.");
            return Color.clear;
        }

        // Find all textures in folder
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });

        HashSet<Color32> seedColors = new HashSet<Color32>();

        foreach (string guid in guids){
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            if (tex == null)
                continue;

            // Make sure texture is readable
            Texture2D readableTex = GetReadableTexture(tex);

            foreach (Color32 c in readableTex.GetPixels32()){
                seedColors.Add(c);
            }
        }
        Debug.Log($"Collected {seedColors.Count} seed colors.");
        List<Color32> SortedSeeds = seedColors
            .OrderByDescending(c => c.g)
            .ThenBy(c => c.r)
            .ThenBy(c => c.b)
            .ToList();

        Color bestColor = Color.clear;
        float bestScore = -1;

        float MinMult = math.pow(UPPERBOUND-LOWERBOUND+1,1); 
        float MedMult = math.pow(UPPERBOUND-LOWERBOUND+1,2);
        float Maxmult = math.pow(UPPERBOUND-LOWERBOUND+1,3);
        for(int g = UPPERBOUND; g >= LOWERBOUND; g--){
            float LargeTracker = Maxmult - g * MedMult;
            EditorUtility.DisplayProgressBar(
            "Scanning colours",
            $"Processed {LargeTracker}/{Maxmult}, Best colour: {bestColor}, Score: {bestScore}",LargeTracker / Maxmult);
            for(int r = LOWERBOUND; r <= UPPERBOUND; r++){
                float SmallTracker = r * MinMult;
                for(int b = LOWERBOUND; b <= UPPERBOUND; b++){
                    float minDist = float.MaxValue;
                    foreach (var c in SortedSeeds){
                        float dr = r - c.r;
                        float dg = g - c.g;
                        float db = b - c.b;
                        float dist = dr * dr + dg * dg + db * db;

                        if (dist < minDist){
                            if(dist < bestScore)
                                break;
                            minDist = dist;
                        }
                    }

                    if (minDist >= bestScore){
                        bestScore = minDist;
                        bestColor = new Color32((byte)r,(byte)g,(byte)b,255);
                        Debug.Log($"New Best Found: {bestColor} : {minDist}");
                        EditorUtility.DisplayProgressBar("Scanning colours",
                        $"Processed {LargeTracker+SmallTracker+b}/{Maxmult} colours, Best colour: {bestColor}, Score: {bestScore}",(LargeTracker+SmallTracker+b) / Maxmult);
                    }
                }
            }
        }
        EditorUtility.ClearProgressBar();
        Debug.Log($"bestColor = {bestColor}, Score: {bestScore}");
        return bestColor;
    }

    private static string GetSelectedFolderPath()
    {
        Object obj = Selection.activeObject;
        if (obj == null)
            return null;

        string path = AssetDatabase.GetAssetPath(obj);

        if (System.IO.Directory.Exists(path))
            return path;

        return System.IO.Path.GetDirectoryName(path);
    }

    private static Texture2D GetReadableTexture(Texture2D tex)
    {
        RenderTexture rt = RenderTexture.GetTemporary(
            tex.width,
            tex.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear
        );

        Graphics.Blit(tex, rt);
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D readable = new Texture2D(tex.width, tex.height);
        readable.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readable.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        return readable;
    }
}
#endif