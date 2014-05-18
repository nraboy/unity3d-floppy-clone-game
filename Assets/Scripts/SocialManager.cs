using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;
using GooglePlayGames;
using System.Collections.Generic;

public class SocialManager {

    #if UNITY_ANDROID
        public static string leaderboardId = "CgkI-uuXy7ESEAIQAA";
    #else
        public static string leaderboardId = "CgkI_uuXy7ESEAIQAA";
    #endif
    public static Dictionary<string, bool> MyAchievements = new Dictionary<string, bool>();

    public static bool IsAuthenticated {
        get {
            return Social.Active.localUser.authenticated;
        }
    }

    public static void Authenticate() {
        if(IsAuthenticated) {
            Debug.Log("Is Already Authenticated");
            return;
        }
        if(Application.platform == RuntimePlatform.Android) {
            PlayGamesPlatform.Activate();
        }
        Social.localUser.Authenticate((bool success) => {
            if(success) {
                Debug.Log("Authentication Successful");
                PlayerPrefs.SetInt("use_game_services", 1);
                LoadAchievements();
            } else {
                Debug.Log("Failed To Authenticate");
                PlayerPrefs.SetInt("use_game_services", 0);
            }
        });
    }

    public static void UnlockAchievement(string id) {
        if(IsAuthenticated && !MyAchievements.ContainsKey(id)) {
            Social.ReportProgress(id, 100.0f, (bool success) => {});
            MyAchievements[id] = true;
        }
    }

    public static void PostToLeaderboard(int score) {
        if(IsAuthenticated) {
            Social.ReportScore((long)score, leaderboardId, (bool success) => {});
        }
    }

    public static void ShowLeaderboardUI() {
        if(IsAuthenticated) {
            Social.ShowLeaderboardUI();
        } else {
            AuthenticateAndShowLeaderboard();
        }
    }

    public static void AuthenticateAndShowLeaderboard() {
        if(IsAuthenticated) {
            return;
        }
        if(Application.platform == RuntimePlatform.Android) {
            PlayGamesPlatform.Activate();
        }
        Social.localUser.Authenticate((bool success) => {
            if(success) {
                PlayerPrefs.SetInt("use_game_services", 1);
                LoadAchievements();
                Social.ShowLeaderboardUI();
            } else {
                PlayerPrefs.SetInt("use_game_services", 0);
            }
        });
    }

    public static void ShowAchievementsUI() {
        if(IsAuthenticated) {
            Social.ShowAchievementsUI();
        }
    }

    public static void LoadAchievements() {
        if(IsAuthenticated) {
            Social.LoadAchievements((IAchievement[] achievements) => {
                Debug.Log ("Got " + achievements.Length + " achievement instances");
                foreach(IAchievement achievement in achievements) {
                    Debug.Log(achievement.id + " -> " + achievement.completed);
                    MyAchievements[achievement.id] = achievement.completed;
                }
            });
        }
    }
    
}