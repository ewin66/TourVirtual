#import <UIKit/UIKit.h>
#import "UnityAppController.h"
#import "UI/UnityView.h"

@interface MyAppController : UnityAppController
{
}
@end

@implementation MyAppController

-(BOOL)application:(UIApplication *)application openURL:(NSURL *)url sourceApplication:(NSString *)sourceApplication annotation:(id)annotation{
    // OJO que llegara el tema del SS.
    if( [[url scheme] isEqualToString:@"rmvt"] ){
        UnitySendMessage("Azure Services", "SetDeepLinking", [[url absoluteString] cStringUsingEncoding:[NSString defaultCStringEncoding]] );
        return YES;
    }
    return NO;
}
@end

IMPL_APP_CONTROLLER_SUBCLASS(MyAppController)