//
//  ChartBoostManager.h
//  CB
//
//  Created by Mike DeSaro on 12/20/11.
//


#import <Foundation/Foundation.h>
#import "ChartBoost.h"

#ifdef UNITY_4_2_0
  #import "UnityAppController.h"
#else
  #import "AppController.h"
#endif



@interface ChartBoostManager : NSObject <ChartboostDelegate>

@property (nonatomic) BOOL waitingForPause;


+ (ChartBoostManager*)sharedManager;


- (void)startChartBoostWithAppId:(NSString*)appId appSignature:(NSString*)appSignature;

- (void)cacheInterstitial:(NSString*)location;

- (void)showInterstitial:(NSString*)location;

- (void)cacheMoreApps;

- (void)showMoreApps;

- (void)pauseUnity;

@end



#ifdef UNITY_4_2_0
@interface UnityAppController(ChartBoostBugFix)
#else
@interface AppController(ChartBoostBugFix)
#endif

- (UIWindow*)window;

@end