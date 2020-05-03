using UnityEngine;
using UnityEditor;

public class MenuItems
{
	[MenuItem("Tools/Clear PlayerPrefs")]
	private static void ClearPlayerPrefs() {
		PlayerPrefs.DeleteAll();
	}

    [MenuItem("Tools/Import Wad")]
    private static void CreateLevelData() {
        WadImporter.ImportWad();
	}
}
