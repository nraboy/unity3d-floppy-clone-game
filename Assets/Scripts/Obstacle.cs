using UnityEngine;
using System.Collections;

public class Obstacle : MonoBehaviour {

    public Vector3 velocity = new Vector3(-2.5f, 0);
    public AudioClip sound;
    private Transform cachedTransform;
    private bool hasEnteredTrigger = false;

    void Awake() {
        cachedTransform = transform;
    }
	
	void Update() {
        cachedTransform.Translate(velocity * Time.smoothDeltaTime);
        if(!isVisible()) {
            Deactivate();
        }
	}

    void OnEnable() {
        cachedTransform.position = new Vector3(11, Random.Range(-3.0f, 3.0f), 5);
    }

    void OnDisable() {
        cachedTransform.position = new Vector3(-9999, 0, 5);
        hasEnteredTrigger = false;
    }

    bool isVisible() {
        bool result = true;
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(cachedTransform.position);
        if(screenPosition.x < -100) {
            result = false;
        }
        return result;
    }

    void Deactivate() {
        ObjectPool.instance.PoolObject(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(hasEnteredTrigger == false && other != null && other.CompareTag("Player")) {
            Score.TotalScore += 1;
            AudioSource.PlayClipAtPoint(sound, new Vector3(0, 0, 0), 1.0f);
            hasEnteredTrigger = true;
        }
    }

}
