using UnityEngine;
using System.Collections;

public class Scorea : MonoBehaviour {
	
	// Use this for initialization
	static public int score = 0;
	void Start()
	{
		score = 0;
	}
	static public void AddPoint()
	{
		score++;
	}
	// Update is called once per frame
	void FixedUpdate () {
		GetComponent<GUIText>().text = "SCORE: " + score;
		PlayerPrefs.SetInt ("Score", score);
	}
}
