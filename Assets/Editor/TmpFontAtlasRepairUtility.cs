using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public static class TmpFontAtlasRepairUtility
{
    private const string SessionRepairKey = "ArrowGame.TmpFontAtlasRepairUtility.Ran.v2";
    private const string OctinFontAssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/Octin College Rg SDF.asset";
    private const string OctinSourceFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/Octin College Rg.otf";
    private const string SportsFontAssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/Sports World-Regular SDF.asset";
    private const string SportsSourceFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/Sports World-Regular.otf";
    private const int DesiredOctinPointSize = 110;
    private const int DesiredSportsPointSize = 128;
    private const int DesiredAtlasPadding = 3;
    private const int DesiredAtlasSize = 2048;
    private const string DesiredShaderName = "TextMeshPro/Distance Field";

    [InitializeOnLoadMethod]
    private static void RepairFontsOnLoad()
    {
        EditorApplication.delayCall += MaybeRepairFontsOnLoad;
    }

    [MenuItem("Tools/Arrow Game/Repair TMP Font Atlases")]
    public static void RepairMenuFonts()
    {
        RepairFontAtlas(OctinFontAssetPath, OctinSourceFontPath, DesiredOctinPointSize, DesiredAtlasPadding, DesiredAtlasSize);
        RepairFontAtlas(SportsFontAssetPath, SportsSourceFontPath, DesiredSportsPointSize, DesiredAtlasPadding, DesiredAtlasSize);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        Debug.Log("TMP font atlas repair completed for Octin and Sports fonts.");
    }

    public static void RepairMenuFontsBatchMode()
    {
        RepairMenuFonts();
    }

    private static void MaybeRepairFontsOnLoad()
    {
        if (SessionState.GetBool(SessionRepairKey, false))
        {
            return;
        }

        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += MaybeRepairFontsOnLoad;
            return;
        }

        TMP_FontAsset octinFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(OctinFontAssetPath);
        TMP_FontAsset sportsFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SportsFontAssetPath);

        bool needsRepair = NeedsRepair(octinFontAsset, DesiredOctinPointSize) || NeedsRepair(sportsFontAsset, DesiredSportsPointSize);
        if (!needsRepair)
        {
            SessionState.SetBool(SessionRepairKey, true);
            return;
        }

        SessionState.SetBool(SessionRepairKey, true);
        RepairMenuFonts();
    }

    private static void RepairFontAtlas(string fontAssetPath, string sourceFontPath, int pointSize, int atlasPadding, int atlasSize)
    {
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);
        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(sourceFontPath);

        if (fontAsset == null)
        {
            throw new System.InvalidOperationException("TMP font asset not found: " + fontAssetPath);
        }

        if (sourceFont == null)
        {
            throw new System.InvalidOperationException("Source font not found: " + sourceFontPath);
        }

        SerializedObject serializedFontAsset = new SerializedObject(fontAsset);
        serializedFontAsset.FindProperty("m_SourceFontFile").objectReferenceValue = sourceFont;
        serializedFontAsset.FindProperty("m_SourceFontFileGUID").stringValue = AssetDatabase.AssetPathToGUID(sourceFontPath);
        serializedFontAsset.FindProperty("m_AtlasPopulationMode").intValue = (int)AtlasPopulationMode.Dynamic;
        serializedFontAsset.FindProperty("m_AtlasWidth").intValue = atlasSize;
        serializedFontAsset.FindProperty("m_AtlasHeight").intValue = atlasSize;
        serializedFontAsset.FindProperty("m_AtlasPadding").intValue = atlasPadding;
        serializedFontAsset.FindProperty("m_AtlasRenderMode").intValue = (int)GlyphRenderMode.SDFAA;

        SerializedProperty creationSettings = serializedFontAsset.FindProperty("m_CreationSettings");
        creationSettings.FindPropertyRelative("sourceFontFileGUID").stringValue = AssetDatabase.AssetPathToGUID(sourceFontPath);
        creationSettings.FindPropertyRelative("pointSize").intValue = pointSize;
        creationSettings.FindPropertyRelative("padding").intValue = atlasPadding;
        creationSettings.FindPropertyRelative("paddingMode").intValue = 0;
        creationSettings.FindPropertyRelative("atlasWidth").intValue = atlasSize;
        creationSettings.FindPropertyRelative("atlasHeight").intValue = atlasSize;
        creationSettings.FindPropertyRelative("renderMode").intValue = (int)GlyphRenderMode.SDFAA;

        serializedFontAsset.ApplyModifiedPropertiesWithoutUndo();

        fontAsset.ClearFontAssetData();

        string characterSet = BuildBasicCharacterSet();
        fontAsset.TryAddCharacters(characterSet, out string missingCharacters, false);

        Texture2D atlasTexture = fontAsset.atlasTexture;
        if (atlasTexture != null)
        {
            atlasTexture.filterMode = FilterMode.Bilinear;
            atlasTexture.wrapMode = TextureWrapMode.Clamp;
#if UNITY_2021_1_OR_NEWER
            atlasTexture.wrapModeU = TextureWrapMode.Clamp;
            atlasTexture.wrapModeV = TextureWrapMode.Clamp;
            atlasTexture.wrapModeW = TextureWrapMode.Clamp;
#endif
            EditorUtility.SetDirty(atlasTexture);
        }

        Material atlasMaterial = fontAsset.material;
        if (atlasMaterial != null)
        {
            Shader distanceFieldShader = Shader.Find(DesiredShaderName);
            if (distanceFieldShader != null && atlasMaterial.shader != distanceFieldShader)
            {
                atlasMaterial.shader = distanceFieldShader;
            }

            atlasMaterial.SetFloat("_GradientScale", atlasPadding + 1f);
            atlasMaterial.SetFloat("_Padding", atlasPadding);
            atlasMaterial.SetFloat("_TextureWidth", atlasSize);
            atlasMaterial.SetFloat("_TextureHeight", atlasSize);
            atlasMaterial.SetFloat("_FaceDilate", 0f);
            atlasMaterial.SetFloat("_OutlineWidth", 0f);
            atlasMaterial.SetFloat("_OutlineSoftness", 0f);
            atlasMaterial.SetFloat("_UnderlaySoftness", 0f);
            atlasMaterial.SetFloat("_UnderlayDilate", 0f);
            atlasMaterial.SetFloat("_GlowOffset", 0f);
            atlasMaterial.SetFloat("_GlowInner", 0f);
            atlasMaterial.SetFloat("_GlowOuter", 0f);
            atlasMaterial.SetFloat("_GlowPower", 0f);
            atlasMaterial.SetFloat("_ScaleRatioA", 1f);
            atlasMaterial.SetFloat("_ScaleRatioB", 1f);
            atlasMaterial.SetFloat("_ScaleRatioC", 1f);
            atlasMaterial.SetTexture("_FaceTex", null);
            atlasMaterial.SetColor("_FaceColor", Color.white);
            atlasMaterial.SetColor("_OutlineColor", Color.black);
            atlasMaterial.SetColor("_GlowColor", new Color(0f, 0f, 0f, 0f));
            atlasMaterial.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0f));
            EditorUtility.SetDirty(atlasMaterial);
        }

        if (!string.IsNullOrEmpty(missingCharacters))
        {
            Debug.LogWarning("Missing characters while rebuilding " + fontAsset.name + ": " + missingCharacters, fontAsset);
        }

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.ImportAsset(fontAssetPath, ImportAssetOptions.ForceUpdate);
    }

    private static string BuildBasicCharacterSet()
    {
        StringBuilder builder = new StringBuilder();

        for (char character = ' '; character <= '~'; character++)
        {
            builder.Append(character);
        }

        builder.Append('\u00A0');
        builder.Append('\u200B');
        builder.Append('\u2026');
        builder.Append('\u25A1');

        return builder.ToString();
    }

    private static bool NeedsRepair(TMP_FontAsset fontAsset, int desiredPointSize)
    {
        if (fontAsset == null)
        {
            return false;
        }

        Material atlasMaterial = fontAsset.material;
        float gradientScale = atlasMaterial != null && atlasMaterial.HasProperty("_GradientScale")
            ? atlasMaterial.GetFloat("_GradientScale")
            : 0f;
        string shaderName = atlasMaterial != null && atlasMaterial.shader != null
            ? atlasMaterial.shader.name
            : string.Empty;

        return fontAsset.faceInfo.pointSize != desiredPointSize
            || fontAsset.atlasPadding != DesiredAtlasPadding
            || fontAsset.atlasWidth != DesiredAtlasSize
            || fontAsset.atlasHeight != DesiredAtlasSize
            || Mathf.Abs(gradientScale - (DesiredAtlasPadding + 1f)) > 0.01f
            || shaderName != DesiredShaderName;
    }
}
