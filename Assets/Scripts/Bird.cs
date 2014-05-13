using UnityEngine;
using System.Collections;

public class Bird : MonoBehaviour {

    public Vector2 jumpForce = new Vector2(0, 250);
	
	void Update () {
        if (Input.GetKeyUp("space") || (Input.touchCount == 1 && Input.touches[0].phase == TouchPhase.Began)) {
            rigidbody2D.velocity = Vector2.zero;
            rigidbody2D.AddForce(jumpForce);
        }
        
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPosition.y > Screen.height || screenPosition.y < 0) {
            GameOver();
        }
	}

    void PlayExplosionSound() {
        //AudioSource.PlayClipAtPoint(explosion, new Vector3(0, 0, 0));
    }

    void GameOver() {
        SocialManager.UnlockAchievement("CgkI-uuXy7ESEAIQAg");
        Application.LoadLevel("game_over");
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(other != null && other.CompareTag("wall")) {
            GameOver();
        }
    }

}
