// Copyright (C) 2017 Google, Inc.
#import "GADUMObjectCache.h"
#import "GADUMTypes.h"
#import "GADUMediationTestClient.h"
#import "GADURequest.h"
#import "GADUTypes.h"
#import "GoogleMobileAdsMediationTestSuite+Internal.h"

/// Returns an NSString copying the characters from |bytes|, a C array of UTF8-encoded bytes.
/// Returns nil if |bytes| is NULL.
static NSString *GADUMStringFromUTF8String(const char *bytes) { return bytes ? @(bytes) : nil; }

/// Shows the Mediation Test Suite using the given application ID.
GADUMTypeMediationClientRef GADUShowMediationTestSuiteWithAppID(
    const char *appId, GADUMTypeMediationTestClientRef *unityClient) {
  [GoogleMobileAdsMediationTestSuite setUserAgentSuffix:@"unity"];
  NSString *applicationId = GADUMStringFromUTF8String(appId);
  GADUMediationTestClient *testClient = [[GADUMediationTestClient alloc] initWithAppId:applicationId
                                                                       mediationClient:unityClient];

  GADUMObjectCache *cache = [GADUMObjectCache sharedInstance];
  [cache addObject:testClient];

  [testClient showMediationTestSuite];
  return (__bridge GADUMTypeMediationClientRef)testClient;
}

/// Shows the Mediation Test Suite.
GADUMTypeMediationClientRef GADUShowMediationTestSuite(
    GADUMTypeMediationTestClientRef *unityClient) {
  [GoogleMobileAdsMediationTestSuite setUserAgentSuffix:@"unity"];
  GADUMediationTestClient *testClient =
      [[GADUMediationTestClient alloc] initWithMediationClient:unityClient];

  GADUMObjectCache *cache = [GADUMObjectCache sharedInstance];
  [cache addObject:testClient];

  [testClient showMediationTestSuite];
  return (__bridge GADUMTypeMediationClientRef)testClient;
}

void GADUMRelease(GADUMTypeRef ref) {
  if (ref) {
    GADUMObjectCache *cache = [GADUMObjectCache sharedInstance];
    [cache removeObject:(__bridge NSObject *)ref];
  }
}

/// Sets the mediation callback method to be invoked when test controller is dismissed
void GADUMSetMediationClientCallback(
    GADUMTypeMediationClientRef client,
    GADUMediationClientDidDismissScreenCallback screenDismissedCallback) {
  GADUMediationTestClient *testClient = (__bridge GADUMediationTestClient *)client;
  testClient.screenDismissedCallback = screenDismissedCallback;
}

/// Sets the ad request on the mediation test suite.
void GADUMSetAdRequest(GADUTypeRequestRef request) {
  GADURequest *internalRequest = (__bridge GADURequest *)request;
  [GoogleMobileAdsMediationTestSuite setAdRequest:[internalRequest request]];
}
