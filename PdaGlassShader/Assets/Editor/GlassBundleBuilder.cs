using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
public static class GlassBundleBuilder
{
    public static void Build()
    {
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { GraphicsDeviceType.Direct3D11 });
        var builds = new[] { new AssetBundleBuild { assetBundleName = "zored-pda-glass", assetNames = new[] { "Assets/Shaders/PDARefraction.shader", "Assets/Shaders/PdaHandReflectionMask.shader" } } };
        var result = BuildPipeline.BuildAssetBundles("Build", builds, BuildAssetBundleOptions.StrictMode | BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);
        if (result == null) throw new System.Exception("AssetBundle build failed");
        Debug.Log("ZORED_GLASS_BUNDLE_BUILT");
    }
}
