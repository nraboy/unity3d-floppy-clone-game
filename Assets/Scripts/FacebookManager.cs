using UnityEngine;
using System.Collections;

public class FacebookManager {

    public static bool IsInitialized = false;
    public static string Permissions = "basic_info,publish_actions";

    public static void Initialize() {
        if(!IsInitialized) {
            FB.Init(() => { IsInitialized = true; }, (bool IsGameShown) => {
                if(!IsGameShown) {
                    Time.timeScale = 0;
                } else {
                    Time.timeScale = 1;
                }
            });
        }
    }

    public static bool IsAuthenticated {
        get {
            return FB.IsLoggedIn;
        }
    }

    public static void Authenticate() {
        if(IsAuthenticated) {
            Debug.Log("Facebook is already authenticated!");
            return;
        }
        FB.Login(Permissions, (FBResult result) => {
            if(result.Error != null) {
                Debug.Log("Facebook Login Error: " + result.Error);
            }
        });
    }

    public static void Invite(string inviteTitle, string inviteMessage) {
        if(IsAuthenticated) {
            FB.AppRequest(inviteMessage, null, "", null, null, "", inviteTitle, null);
        } else {
            AuthenticateAndInvite(inviteTitle, inviteMessage);
        }
    }

    public static void Share(string name, string caption, string description, string image, string url) {
        if(IsAuthenticated) {
            FB.Feed("", url, name, caption, description, image, "", "", "", "", null, null);
        } else {
            AuthenticateAndShare(name, caption, description, image, url);
        }
    }

    private static void AuthenticateAndInvite(string inviteTitle, string inviteMessage) {
        if(IsAuthenticated) {
            Debug.Log("Facebook is already authenticated!");
            return;
        }
        FB.Login(Permissions, (FBResult result) => {
            if(result.Error != null) {
                Debug.Log("Facebook Login Error: " + result.Error);
            } else if(IsAuthenticated) {
                Invite(inviteTitle, inviteMessage);
            }
        });
    }

    private static void AuthenticateAndShare(string name, string caption, string description, string image, string url) {
        if(IsAuthenticated) {
            Debug.Log("Facebook is already authenticated!");
            return;
        }
        FB.Login(Permissions, (FBResult result) => {
            if(result.Error != null) {
                Debug.Log("Facebook Login Error: " + result.Error);
            } else if(IsAuthenticated) {
                Share(name, caption, description, image, url);
            }
        });
    }

}
