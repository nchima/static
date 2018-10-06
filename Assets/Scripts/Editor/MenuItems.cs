using UnityEngine;
using UnityEditor;

public class MenuItems
{
	[MenuItem("Nerve Damage Tools/Clear PlayerPrefs")]
	private static void ClearPlayerPrefs() {
		PlayerPrefs.DeleteAll();
	}

//	[MenuItem("Nerve Damage Tools/Create Level Data")]
//	private static void () {
//		
//	}
}
