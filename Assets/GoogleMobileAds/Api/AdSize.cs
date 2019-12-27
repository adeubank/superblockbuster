// Copyright (C) 2015 Google, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace GoogleMobileAds.Api
{
    internal enum Orientation
    {
        Current = 0,
        Landscape = 1,
        Portrait = 2
    }

    public class AdSize
    {
        public enum Type
        {
            Standard = 0,
            SmartBanner = 1,
            AnchoredAdaptive = 2
        }

        public static readonly AdSize Banner = new AdSize(320, 50);
        public static readonly AdSize MediumRectangle = new AdSize(300, 250);
        public static readonly AdSize IABBanner = new AdSize(468, 60);
        public static readonly AdSize Leaderboard = new AdSize(728, 90);
        public static readonly AdSize SmartBanner = new AdSize(0, 0, Type.SmartBanner);
        public static readonly int FullWidth = -1;

        public AdSize(int width, int height)
        {
            AdType = Type.Standard;
            Width = width;
            Height = height;
            Orientation = Orientation.Current;
        }

        private AdSize(int width, int height, Type type) : this(width, height)
        {
            AdType = type;
        }

        public int Width { get; }

        public int Height { get; }

        public Type AdType { get; }

        internal Orientation Orientation { get; private set; }

        private static AdSize CreateAnchoredAdaptiveAdSize(int width, Orientation orientation)
        {
            var adSize = new AdSize(width, 0, Type.AnchoredAdaptive);
            adSize.Orientation = orientation;
            return adSize;
        }

        public static AdSize GetLandscapeAnchoredAdaptiveBannerAdSizeWithWidth(int width)
        {
            return CreateAnchoredAdaptiveAdSize(width, Orientation.Landscape);
        }

        public static AdSize GetPortraitAnchoredAdaptiveBannerAdSizeWithWidth(int width)
        {
            return CreateAnchoredAdaptiveAdSize(width, Orientation.Portrait);
        }

        public static AdSize GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(int width)
        {
            return CreateAnchoredAdaptiveAdSize(width, Orientation.Current);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (AdSize) obj;
            return Width == other.Width && Height == other.Height
                                        && AdType == other.AdType && Orientation == other.Orientation;
        }

        public static bool operator ==(AdSize a, AdSize b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(AdSize a, AdSize b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            var hashBase = 71;
            var hashMultiplier = 11;

            var hash = hashBase;
            hash = (hash * hashMultiplier) ^ Width.GetHashCode();
            hash = (hash * hashMultiplier) ^ Height.GetHashCode();
            hash = (hash * hashMultiplier) ^ AdType.GetHashCode();
            hash = (hash * hashMultiplier) ^ Orientation.GetHashCode();
            return hash;
        }
    }
}