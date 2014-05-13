using UnityEngine;
using System.Collections;

public class Score : MonoBehaviour {

    public static float distance = 0;
    public static int TotalScore = 0;
    public static bool IsLocked = false;
    private int previousScore;

	void Start() {
        guiText.fontSize = Mathf.RoundToInt(Camera.main.pixelHeight / 17f);
        previousScore = 0;
	}
	
	void Update() {
        if(IsLocked == false) {
            distance = Mathf.Round(Time.timeSinceLevelLoad);
            //TotalScore = (int) distance;
        }
	    if(TotalScore != previousScore) {
            guiText.text = "SCORE: " + TotalScore.ToString();
            previousScore = TotalScore;
        }
	}
}
