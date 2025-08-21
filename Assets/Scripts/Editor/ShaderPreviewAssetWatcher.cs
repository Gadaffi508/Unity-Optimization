using UnityEditor;
using System;

public class ShaderPreviewAssetWatcher : AssetPostprocessor
{
    public static event Action<string[]> OnRelevantAssetsChanged;

    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        bool IsRelevant(string p) =>
            p.EndsWith(".shader", StringComparison.OrdinalIgnoreCase) ||
            p.EndsWith(".hlsl",   StringComparison.OrdinalIgnoreCase) ||
            p.EndsWith(".cginc",  StringComparison.OrdinalIgnoreCase) ||
            p.EndsWith(".mat",    StringComparison.OrdinalIgnoreCase);

        var any =
            Array.Exists(importedAssets, IsRelevant) ||
            Array.Exists(movedAssets,    IsRelevant) ||
            Array.Exists(movedFromAssetPaths, IsRelevant);

        if (any)
            OnRelevantAssetsChanged?.Invoke(importedAssets);
    }
}
