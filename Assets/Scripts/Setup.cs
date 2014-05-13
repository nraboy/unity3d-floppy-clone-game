using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Chartboost;

public class Setup : MonoBehaviour {

    void Awake() {
        useGUILayout = false;
        if(Application.platform == RuntimePlatform.IPhonePlayer) {
            Application.targetFrameRate = 60;
        }
        AudioListener.volume = PlayerPrefs.GetInt("volume", 50);
        FacebookManager.Initialize();
        if(PlayerPrefs.GetInt("use_game_services", 0) > 0 ? true : false) {
            SocialManager.Authenticate();
        }
    }

    void Start() {
        if(System.String.Compare(Application.loadedLevelName, "game_over") == 0) {
            if(ChartboostConfig.wasShown == false) {
                CBBinding.showInterstitial(null);
                CBBinding.cacheInterstitial(null);
                ChartboostConfig.wasShown = true;
            } else {
                if(Random.Range(0, 3) == 1) {
                    CBBinding.showInterstitial(null);
                    CBBinding.cacheInterstitial(null);
                }
            }
        }
    }
	
	void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            #if UNITY_ANDROID
            if(CBBinding.onBackPressed()) {
                return;
            } else {
                Application.Quit();
            }
            #else
                Application.Quit();
            #endif
        }
        if(Application.platform == RuntimePlatform.IPhonePlayer) {
            if(Time.frameCount % 30 == 0) {
                System.GC.Collect();
            }
        }
        if(Score.TotalScore >= 1000) {
            SocialManager.UnlockAchievement("CgkI-uuXy7ESEAIQBg");
        } else if(Score.TotalScore >= 100) {
            SocialManager.UnlockAchievement("CgkI-uuXy7ESEAIQBQ");
        } else if(Score.TotalScore >= 50) {
            SocialManager.UnlockAchievement("CgkI-uuXy7ESEAIQBA");
        } else if(Score.TotalScore >= 20) {
            SocialManager.UnlockAchievement("CgkI-uuXy7ESEAIQAw");
        }
	}

    void OnApplicationQuit() {
        #if UNITY_ANDROID
            CBBinding.destroy();
        #endif
    }

    void OnApplicationPause(bool paused) {
        #if UNITY_ANDROID
            CBBinding.pause(paused);
        #endif
    }

}
