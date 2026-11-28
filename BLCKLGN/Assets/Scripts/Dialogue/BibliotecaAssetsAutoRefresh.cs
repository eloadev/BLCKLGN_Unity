#if UNITY_EDITOR
using UnityEditor;

// =====================================================
// BibliotecaAssetsAutoRefresh — BLCKLGN (solo editor)
// Cada vez que se agrega, borra o mueve un archivo dentro de
// Assets/Art o Assets/Audio, vuelve a llenar la BibliotecaAssets.
// Así nadie tiene que acordarse de actualizarla a mano.
// =====================================================
public class BibliotecaAssetsAutoRefresh : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importados, string[] borrados, string[] movidos, string[] movidosDesde)
    {
        if (!Toca(importados) && !Toca(borrados) && !Toca(movidos) && !Toca(movidosDesde)) return;

        // Esperamos a que Unity termine de importar antes de llenar
        EditorApplication.delayCall += () =>
        {
            foreach (string guid in AssetDatabase.FindAssets("t:BibliotecaAssets"))
            {
                var biblioteca = AssetDatabase.LoadAssetAtPath<BibliotecaAssets>(AssetDatabase.GUIDToAssetPath(guid));
                if (biblioteca != null) biblioteca.Llenar();
            }
            AssetDatabase.SaveAssets();
        };
    }

    private static bool Toca(string[] rutas)
    {
        foreach (string ruta in rutas)
            if (ruta.StartsWith("Assets/Art/") || ruta.StartsWith("Assets/Audio/"))
                return true;
        return false;
    }
}
#endif
