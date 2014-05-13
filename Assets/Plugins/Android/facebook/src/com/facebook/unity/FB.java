package com.facebook.unity;

import java.math.BigDecimal;
import java.util.*;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;

import android.annotation.TargetApi;
import android.content.Intent;
import android.os.Build;
import android.text.TextUtils;

import android.app.Activity;
import android.os.Bundle;
import android.util.Log;
import android.util.Base64;
import android.content.pm.*;
import android.content.pm.PackageManager.NameNotFoundException;

import com.facebook.*;
import com.facebook.Session.Builder;
import com.facebook.Session.OpenRequest;
import com.facebook.Session.StatusCallback;
import com.facebook.internal.Utility;
import com.facebook.model.*;
import com.facebook.widget.WebDialog;
import com.facebook.widget.WebDialog.OnCompleteListener;
import com.unity3d.player.UnityPlayer;

public class FB {
	static final String TAG = "FBUnitySDK";
	// i.e. the game object that receives this message
	static final String FB_UNITY_OBJECT = "UnityFacebookSDKPlugin";
	private static Intent intent;
    private static AppEventsLogger appEventsLogger;

    private static String appID;

    private static Boolean frictionlessRequests = false;

    private static AppEventsLogger getAppEventsLogger() {
        if (appEventsLogger == null) {
            appEventsLogger = AppEventsLogger.newLogger(getUnityActivity().getApplicationContext());
        }
        return appEventsLogger;
    }

	private static boolean isLoggedIn() {
		return Session.getActiveSession() != null && Session.getActiveSession().isOpened();
	}


	static Activity getUnityActivity() {
		return UnityPlayer.currentActivity;
	}

	private static void initAndLogin(String params, final boolean show_login_dialog, final Activity activity) {
        String[] parts = null;
        UnityParams unity_params = UnityParams.parse(params, "couldn't parse login params: " + params);

        if (unity_params.hasString("appId")) {
            // override the app id from the android manifest from FB.Init() if it's there
            appID = unity_params.getString("appId");
        } else if (appID == null || appID.length() == 0) {
            // default: use the app id from the metadata
            appID = Utility.getMetadataApplicationId(activity);
        } // else there's an appID provided

        Session session;
        if (FB.isLoggedIn()) {
            session = Session.getActiveSession();

            // this shouldn't be an issue for most people: the app id in the session not matching the one provided
            // instead it can probably happen if a developer wants to switch app ids at run time.
            if (appID != session.getApplicationId()) {
                Log.w(TAG, "App Id in active session ("+ session.getApplicationId() +") doesn't match App Id passed in: " + appID);
                session = new Builder(activity).setApplicationId(appID).build();
            }
        } else {
            session = new Builder(activity).setApplicationId(appID).build();
        }

        final UnityMessage unityMessage = new UnityMessage((show_login_dialog) ? "OnLoginComplete" : "OnInitComplete");

        // add the key hash to the JSON dictionary
        unityMessage.put("key_hash", getKeyHash());

        // if we are init-ing, we can just return here if there is no active session
        if (!SessionState.CREATED_TOKEN_LOADED.equals(session.getState()) && !show_login_dialog) {
            unityMessage.send();
            return;
        }

		// parse and separate the permissions into read and publish permissions
        if (unity_params.hasString("scope")) {
            parts = unity_params.getString("scope").split(",");
        }
        List<String> publishPermissions = new ArrayList<String>();
        List<String> readPermissions = new ArrayList<String>();
        if(parts != null && parts.length > 0) {
            for(String s:parts) {
                if(s.length() == 0) {
                    continue;
                }
                if(Session.isPublishPermission(s)) {
                    publishPermissions.add(s);
                } else {
                    readPermissions.add((s));
                }
            }
        }

        boolean hasPublishPermissions = !publishPermissions.isEmpty();
        if (session != Session.getActiveSession()) {
            Session.setActiveSession(session);
        }

        // check to see if the readPermissions have been TOSed already
        // we don't need to show the readPermissions dialog if they have all been TOSed even though it's a mix
        // of permissions
        boolean showMixedPermissionsFlow = hasPublishPermissions && !session.getPermissions().containsAll(readPermissions);

        // if we're logging in and showing a mix of publish and read permission, we need to split up the dialogs
        // first just show the read permissions, then call initAndLogin() with just the publish permissions
        if (showMixedPermissionsFlow) {
            String publish_permissions = TextUtils.join(",", publishPermissions.toArray());
            unity_params.put("scope", publish_permissions);
            final String only_publish_params = unity_params.toString();

            Session.StatusCallback afterReadPermissionCallback = new Session.StatusCallback() {
                // callback when session changes state
                @Override
                public void call(Session session, SessionState state, Exception exception) {
                    if (session.getState().equals(SessionState.OPENING)){
                        return;
                    }
                    if (!session.isOpened() && state != SessionState.CLOSED_LOGIN_FAILED) {
                        unityMessage.sendError("Unknown error while opening session. Check logcat for details.");
                        return;
                    }
                    session.removeCallback(this); // without this, the callback will loop infinitely

                    // if someone cancels on the read permissions and we don't even have the most basic access_token
                    // for basic info, we shouldn't be asking for publish permissions.  It doesn't make sense
                    // and it simply won't work anyways.
                    if (session.getAccessToken() == null || session.getAccessToken().equals("")) {
                        unityMessage.putCancelled();
                        unityMessage.send();
                        activity.finish();
                        return;
                    }

                    initAndLogin(only_publish_params, show_login_dialog, activity);
                }
            };

            if (session.isOpened()) {
                session.requestNewReadPermissions(getNewPermissionsRequest(session, afterReadPermissionCallback, readPermissions, activity));
            } else {
                session.openForRead(getOpenRequest(afterReadPermissionCallback, readPermissions, activity));
            }

            return;
        }

        Session.StatusCallback finalCallback = new Session.StatusCallback() {
            // callback when session changes state
            @Override
            public void call(Session session, SessionState state, Exception exception) {
                if (session.getState().equals(SessionState.OPENING)){
                    return;
                }
            	//if we are logging in, we opened login activity to handle callbacks, we need to close it after we are done
                if (show_login_dialog) {
            	    activity.finish();
                }

                if (!session.isOpened() && state != SessionState.CLOSED_LOGIN_FAILED) {
                    unityMessage.sendError("Unknown error while opening session. Check logcat for details.");
                    return;
                }
                session.removeCallback(this);

                if (session.isOpened()) {
                    unityMessage.put("opened", true);
                } else if (state == SessionState.CLOSED_LOGIN_FAILED) {
                    unityMessage.putCancelled();
                }

                if (session.getAccessToken() == null || session.getAccessToken().equals("")) {
                    unityMessage.send();
                    return;
                }

                // there's a chance a subset of the permissions were allowed even if the login was cancelled
                // if the access token is there, try to get it anyways

                // add a callback to update the access token when it changes
                session.addCallback(new StatusCallback() {
                    @Override
                    public void call(Session session,
                                     SessionState state, Exception exception) {
                        if (session == null || session.getAccessToken() == null) {
                            return;
                        }
                        final UnityMessage unityMessage = new UnityMessage("OnAccessTokenUpdate");
                        unityMessage.put("access_token", session.getAccessToken());
                        unityMessage.put("expiration_timestamp", "" + session.getExpirationDate().getTime() / 1000);
                        unityMessage.send();
                    }
                });
                unityMessage.put("access_token", session.getAccessToken());
                unityMessage.put("expiration_timestamp", "" + session.getExpirationDate().getTime() / 1000);
                Request.newMeRequest(session, new Request.GraphUserCallback() {
                    @Override
                    public void onCompleted(GraphUser user, Response response) {
                        if (user != null) {
                            unityMessage.put("user_id", user.getId());
                        }
                        unityMessage.send();
                    }
                }).executeAsync();
            }
        };

        if (session.isOpened()) {
            Session.NewPermissionsRequest req = getNewPermissionsRequest(session, finalCallback, (hasPublishPermissions) ? publishPermissions : readPermissions, activity);
            if (hasPublishPermissions) {
                session.requestNewPublishPermissions(req);
            } else {
                session.requestNewReadPermissions(req);
            }
        } else {
            OpenRequest req = getOpenRequest(finalCallback, (hasPublishPermissions) ? publishPermissions : readPermissions, activity);
            if (hasPublishPermissions) {
                session.openForPublish(req);
            } else {
                session.openForRead(req);
            }
        }
	}

    private static OpenRequest getOpenRequest(StatusCallback callback, List<String> permissions, Activity activity) {
        OpenRequest req = new OpenRequest(activity);
        req.setCallback(callback);
        req.setPermissions(permissions);
        req.setDefaultAudience(SessionDefaultAudience.FRIENDS);

        return req;
    }

    private static Session.NewPermissionsRequest getNewPermissionsRequest(Session session, StatusCallback callback, List<String> permissions, Activity activity) {
        Session.NewPermissionsRequest req = new Session.NewPermissionsRequest(activity, permissions);
        req.setCallback(callback);
        // This should really be "req.setCallback(callback);"
        // Unfortunately the current underlying SDK won't add the callback when you do it that way
        // TODO: when upgrading to the latest see if this can be "req.setCallback(callback);"
        // if it still doesn't have it, file a bug!
        session.addCallback(callback);
        req.setDefaultAudience(SessionDefaultAudience.FRIENDS);
        return req;
    }

	@UnityCallable
	public static void Init(String params) {
	    UnityParams unity_params = UnityParams.parse(params, "couldn't parse init params: "+params);
	    if (unity_params.hasString("frictionlessRequests")) {
	        frictionlessRequests = Boolean.valueOf(unity_params.getString("frictionlessRequests"));
	    }
	    // tries to log the user in if they've already TOS'd the app
		initAndLogin(params, /*show_login_dialog=*/false, getUnityActivity());
	}

	/*
	 * Start login process using custom activity for process. Activity is closed after login is completed
	 */
	static void LoginUsingActivity(String params, Activity activity) {
		initAndLogin(params, /*show_login_dialog=*/true, activity);
	}

	@UnityCallable
	public static void Login(String params) {
		Intent intent = new Intent(getUnityActivity(), FBUnityLoginActivity.class);
	    intent.putExtra(FBUnityLoginActivity.LOGIN_PARAMS, params);
	    getUnityActivity().startActivity(intent);
	}

	@UnityCallable
	public static void Logout(String params) {
		Session.getActiveSession().closeAndClearTokenInformation();
		new UnityMessage("OnLogoutComplete").send();
	}

	@UnityCallable
    public static void AppRequest(String params_str) {
        Log.v(TAG, "sendRequestDialog(" + params_str + ")");
        final UnityMessage response = new UnityMessage("OnAppRequestsComplete");

        if (!isLoggedIn()) {
            response.sendNotLoggedInError();
            return;
        }

        UnityParams unity_params = UnityParams.parse(params_str);
        if (unity_params.hasString("callback_id")) {
            response.put("callback_id", unity_params.getString("callback_id"));
        }

        final Bundle params = unity_params.getStringParams();
        if (params.containsKey("callback_id")) {
            params.remove("callback_id");
        }

        if (frictionlessRequests) {
            params.putString("frictionless", "true");
        }

        getUnityActivity().runOnUiThread(new Runnable() {
            @Override
            public void run() {
                // TODO Auto-generated method stub
                WebDialog requestsDialog = (
                        new WebDialog.RequestsDialogBuilder(getUnityActivity(),
                                Session.getActiveSession(),
                                params))
                                .setOnCompleteListener(new OnCompleteListener() {

                                    @Override
                                    public void onComplete(Bundle values,
                                            FacebookException error) {

                                        if (error != null) {
                                            if(error.toString().equals("com.facebook.FacebookOperationCanceledException")) {
                                                response.putCancelled();
                                                response.send();
                                            } else {
                                                response.sendError(error.toString());
                                            }
                                        } else {
                                            if(values != null) {
                                                final String requestId = values.getString("request");
                                                if(requestId == null) {
                                                    response.putCancelled();
                                                }
                                            }

                                            for (String key : values.keySet()) {
                                                response.put(key, values.getString(key));
                                            }
                                            response.send();
                                        }

                                    }

                                })
                                .build();
                requestsDialog.show();

            }
        });
    }

	@UnityCallable
	public static void FeedRequest(String params_str) {
		Log.v(TAG, "FeedRequest(" + params_str + ")");
		final UnityMessage response = new UnityMessage("OnFeedRequestComplete");

		UnityParams unity_params = UnityParams.parse(params_str);
		if (unity_params.hasString("callback_id")){
            response.put("callback_id", unity_params.getString("callback_id"));
		}

		if (!isLoggedIn()) {
			response.sendNotLoggedInError();
			return;
		}

		final Bundle params = unity_params.getStringParams();
        if (params.containsKey("callback_id")) {
            params.remove("callback_id");
        }

		getUnityActivity().runOnUiThread(new Runnable() {

            @Override
            public void run() {
                WebDialog feedDialog = (
                        new WebDialog.FeedDialogBuilder(getUnityActivity(),
                                Session.getActiveSession(),
                                params))
                        .setOnCompleteListener(new OnCompleteListener() {

                            @Override
                            public void onComplete(Bundle values,
                                                   FacebookException error) {

                                // response
                                if (error == null) {
                                    final String postID = values.getString("post_id");
                                    if (postID != null) {
                                        response.putID(postID);
                                    } else {
                                        response.putCancelled();
                                    }
                                    response.send();
                                } else if (error instanceof FacebookOperationCanceledException) {
                                    // User clicked the "x" button
                                    response.putCancelled();
                                    response.send();
                                } else {
                                    // Generic, ex: network error
                                    response.sendError(error.toString());
                                }
                            }

                        })
                        .build();
                feedDialog.show();
            }
        });
	}

	@UnityCallable
	public static void PublishInstall(String params_str) {
		final UnityMessage unityMessage = new UnityMessage("OnPublishInstallComplete");
		final UnityParams unity_params = UnityParams.parse(params_str);
		if (unity_params.hasString("callback_id")) {
			unityMessage.put("callback_id", unity_params.getString("callback_id"));
		}
        AppEventsLogger.activateApp(getUnityActivity().getApplicationContext());
        unityMessage.send();
	}

    @UnityCallable
    public static void GetDeepLink(String params_str) {
        final UnityMessage unityMessage = new UnityMessage("OnGetDeepLinkComplete");
        if (intent != null && intent.getData() != null) {
            unityMessage.put("deep_link", intent.getData().toString());
        } else {
            unityMessage.put("deep_link", "");
        }
        unityMessage.send();
    }

    public static void SetIntent(Intent intent) {
        FB.intent = intent;
        GetDeepLink("");
    }

    public static void SetLimitEventUsage(String params) {
        Settings.setLimitEventAndDataUsage(getUnityActivity().getApplicationContext(), Boolean.valueOf(params));
    }

    @UnityCallable
    public static void AppEvents(String params) {
        Log.v(TAG, "AppEvents(" + params + ")");
        UnityParams unity_params = UnityParams.parse(params);

        Bundle parameters = new Bundle();
        if (unity_params.has("parameters")) {
            UnityParams unity_params_parameter = unity_params.getParamsObject("parameters");
            parameters = unity_params_parameter.getStringParams();
        }

        if (unity_params.has("logPurchase")) {
            FB.getAppEventsLogger().logPurchase(
                    new BigDecimal(unity_params.getDouble("logPurchase")),
                    Currency.getInstance(unity_params.getString("currency")),
                    parameters
            );
        } else if (unity_params.hasString("logEvent")) {
            FB.getAppEventsLogger().logEvent(
                    unity_params.getString("logEvent"),
                    unity_params.getDouble("valueToSum"),
                    parameters
            );
        } else {
            Log.e(TAG, "couldn't logPurchase or logEvent params: "+params);
        }
    }

    /**
     * This is to be called from Unity Login activity to call session callback after login activity completes
     * @param activity
     * @param requestCode
     * @param resultCode
     * @param data
     */
    public static void onActivityResult(Activity activity, int requestCode, int resultCode, Intent data) {
        Session.getActiveSession().onActivityResult(activity, requestCode, resultCode, data);
    }

    /**
     * Provides the key hash to solve the openSSL issue with Amazon
     * @return key hash
     */
    @TargetApi(Build.VERSION_CODES.FROYO)
    private static String getKeyHash() {
        try {
            PackageInfo info = getUnityActivity().getPackageManager().getPackageInfo(
                getUnityActivity().getPackageName(), PackageManager.GET_SIGNATURES);
            for (Signature signature : info.signatures){
                MessageDigest md = MessageDigest.getInstance("SHA");
                md.update(signature.toByteArray());
                String keyHash = Base64.encodeToString(md.digest(), Base64.DEFAULT);
                Log.d(TAG, "KeyHash: " + keyHash);
                return keyHash;
            }
        } catch (NameNotFoundException e) {
        } catch (NoSuchAlgorithmException e) {
        }
        return "";
    }
}
