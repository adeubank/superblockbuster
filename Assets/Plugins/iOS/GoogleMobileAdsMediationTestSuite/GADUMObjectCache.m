// Copyright 2017 Google Inc. All Rights Reserved.

#import "GADUMObjectCache.h"

@implementation GADUMObjectCache

+ (instancetype)sharedInstance {
  static GADUMObjectCache *sharedInstance;
  static dispatch_once_t onceToken;
  dispatch_once(&onceToken, ^{
    sharedInstance = [[self alloc] init];
  });
  return sharedInstance;
}

- (id)init {
  self = [super init];
  if (self) {
    _references = [[NSMutableDictionary alloc] init];
  }
  return self;
}

- (void)addObject:(NSObject *)object {
  [self.references setObject:object forKey:[object gadum_referenceKey]];
}

- (void)removeObject:(NSObject *)object {
  [self.references removeObjectForKey:[object gadum_referenceKey]];
}

@end

@implementation NSObject (GADUOwnershipAdditions)

- (NSString *)gadum_referenceKey {
  return [NSString stringWithFormat:@"%p", (void *)self];
}

@end
