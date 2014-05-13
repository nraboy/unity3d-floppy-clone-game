using UnityEngine;
using System.Collections;

public class Title : MonoBehaviour {

	void Start () {
        guiText.fontSize = Mathf.RoundToInt(Camera.main.pixelHeight / 14f);
	}
	
}
