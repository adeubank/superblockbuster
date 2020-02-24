// Copyright 2017 Google Inc. All Rights Reserved.

/// Base type representing a GADU* pointer.
typedef const void *GADUMTypeRef;

/// Type representing a Unity Mediation Test client.
typedef const void *GADUMTypeMediationTestClientRef;

/// Type representing a Mediation Test client.
typedef const void *GADUMTypeMediationClientRef;

/// Callback for when a full screen view is about to be dismissed.
typedef void (*GADUMediationClientDidDismissScreenCallback)(
    GADUMTypeMediationTestClientRef *mediationClient);
