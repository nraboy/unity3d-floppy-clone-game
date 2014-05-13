using UnityEngine;
using System.Collections;

public class GameOver : MonoBehaviour {

	void Start () {
        guiText.fontSize = Mathf.RoundToInt(Camera.main.pixelHeight / 12f);
	}

}
