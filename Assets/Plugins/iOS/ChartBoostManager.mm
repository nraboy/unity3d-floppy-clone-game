//
//  ChartBoostManager.m
//  CB
//
//  Created by Mike DeSaro on 12/20/11.
//

#import "ChartBoostManager.h"


void UnitySendMessage( const char * className, const char * methodName, const char * param );
void UnityPause( bool pause );


@implementation ChartBoostManager
@synthesize waitingForPause;

///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark NSObject

+ (ChartBoostManager*)sharedManager
{
	static ChartBoostManager *sharedSingleton;
	
	if( !sharedSingleton )
		sharedSingleton = [[ChartBoostManager alloc] init];
	
	return sharedSingleton;
}

- (id)init {
    self = [super init];
    
    if (self) {
        self.waitingForPause = NO;
    }
    
    return self;
}

- (void) dealloc {
    [super dealloc];
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Public

- (void)startChartBoostWithAppId:(NSString*)appId appSignature:(NSString*)appSignature
{
    Chartboost *cb = [Chartboost sharedChartboost];
    cb.appId = appId;
    cb.appSignature = appSignature;
    cb.delegate = self;
    
    [cb startSession];
}


- (void)cacheInterstitial:(NSString*)location
{
    if( location )
        [[Chartboost sharedChartboost] cacheInterstitial:location];
    else
        [[Chartboost sharedChartboost] cacheInterstitial];
}


- (void)showInterstitial:(NSString*)location
{
    if( location )
        [[Chartboost sharedChartboost] showInterstitial:location];
    else
        [[Chartboost sharedChartboost] showInterstitial];
}


- (void)cacheMoreApps
{
    [[Chartboost sharedChartboost] cacheMoreApps];
}


- (void)showMoreApps
{
    [[Chartboost sharedChartboost] showMoreApps];
}


- (void)pauseUnity
{
    if (self.waitingForPause)
        UnityPause( true );
    self.waitingForPause = NO;
}


///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark ChartboostDelegate

- (BOOL)shouldDisplayInterstitial:(NSString*)location
{
	UnitySendMessage( "ChartBoostManager", "didShowInterstitial", location.UTF8String );
    self.waitingForPause = YES;
    return YES;
}


// Called when an interstitial has failed to come back from the server
- (void)didFailToLoadInterstitial:(NSString*)location withError:(CBLoadError)error
{
	NSDictionary *data = [NSDictionary dictionaryWithObjectsAndKeys:
						  location ? location : [NSNull null], @"location",
						  [NSNumber numberWithInt: error], @"errorCode",
						  nil];
	NSData *jsonData = [NSJSONSerialization dataWithJSONObject:data options:0 error:NULL];
	NSString *json = [[[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding] autorelease];
	UnitySendMessage( "ChartBoostManager", "didFailToLoadInterstitial", json.UTF8String );
}


- (void)didCacheInterstitial:(NSString*)location
{
	UnitySendMessage( "ChartBoostManager", "didCacheInterstitial", location.UTF8String );
}


// Called when the user dismisses the interstitial
- (void)didDismissInterstitial:(NSString*)location
{
    UnityPause( false );
    self.waitingForPause = NO;
    UnitySendMessage( "ChartBoostManager", "didDismissInterstitial", location.UTF8String );
}


// Same as above, but only called when dismissed for a close
- (void)didCloseInterstitial:(NSString*)location
{
    UnityPause( false );
    self.waitingForPause = NO;
    UnitySendMessage( "ChartBoostManager", "didCloseInterstitial", location.UTF8String );
}


// Same as above, but only called when dismissed for a click
- (void)didClickInterstitial:(NSString*)location
{
    UnityPause( false );
    self.waitingForPause = NO;
    UnitySendMessage( "ChartBoostManager", "didClickInterstitial", location.UTF8String );
}


// Called when a more apps page has failed to come back from the server
- (void)didFailToLoadMoreApps:(CBLoadError)error
{
	NSDictionary *data = [NSDictionary dictionaryWithObjectsAndKeys:
						  [NSNumber numberWithInt: error], @"errorCode",
						  nil];
	NSData *jsonData = [NSJSONSerialization dataWithJSONObject:data options:0 error:NULL];
	NSString *json = [[[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding] autorelease];
	UnitySendMessage( "ChartBoostManager", "didFailToLoadMoreApps", json.UTF8String );
}


- (void)didCacheMoreApps
{
	UnitySendMessage( "ChartBoostManager", "didCacheMoreApps", "" );
}


- (BOOL)shouldDisplayMoreApps
{
	UnitySendMessage( "ChartBoostManager", "didShowMoreApps", "" );
    self.waitingForPause = YES;
    return YES;
}


- (void)didDismissMoreApps
{
    UnityPause( false );
    self.waitingForPause = NO;
    UnitySendMessage( "ChartBoostManager", "didDismissMoreApps", "" );
}


// Same as above, but only called when dismissed for a close
- (void)didCloseMoreApps
{
    UnityPause( false );
    self.waitingForPause = NO;
    UnitySendMessage( "ChartBoostManager", "didCloseMoreApps", "" );
}


// Same as above, but only called when dismissed for a click
- (void)didClickMoreApps
{
    UnityPause( false );
    self.waitingForPause = NO;
    UnitySendMessage( "ChartBoostManager", "didClickMoreApps", "" );
}

@end



#ifdef UNITY_4_2_0
@implementation UnityAppController(ChartBoostBugFix)
#else
@implementation AppController(ChartBoostBugFix)
#endif

- (UIWindow*)window
{
	return [UIApplication sharedApplication].keyWindow;
}

@end
