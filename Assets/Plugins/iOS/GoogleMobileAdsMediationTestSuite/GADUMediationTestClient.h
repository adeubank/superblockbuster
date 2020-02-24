// Copyright 2017 Google Inc. All Rights Reserved.

#import <Foundation/Foundation.h>
#import <GoogleMobileAdsMediationTestSuite/GoogleMobileAdsMediationTestSuite.h>
#import "GADUMTypes.h"

// A client to interface with the native mediation test controller
@interface GADUMediationTestClient : NSObject

@property(nonatomic, assign) GADUMTypeMediationTestClientRef *mediationClient;
@property(nonatomic, assign) GADUMediationClientDidDismissScreenCallback screenDismissedCallback;

- (instancetype)initWithMediationClient:(GADUMTypeMediationTestClientRef *)mediationClient;

- (instancetype)initWithAppId:(NSString *)appId
              mediationClient:(GADUMTypeMediationTestClientRef *)mediationClient;

- (void)showMediationTestSuite;

@end
