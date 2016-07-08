#region Copyright 2016 D-Haven.org

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

namespace DHaven.DisCarta
{
    using System.Windows;

    /// <summary>
    ///     Attached properties for shapes that will be on a map.
    /// </summary>
    public static class Geo
    {
        public static readonly DependencyProperty AreaProperty = DependencyProperty.RegisterAttached("Area",
            typeof(GeoArea), typeof(Geo),
            new FrameworkPropertyMetadata(GeoArea.Empty,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange
                | FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public static readonly DependencyProperty LocationProperty = DependencyProperty.RegisterAttached("Location",
            typeof(GeoPoint), typeof(Geo),
            new FrameworkPropertyMetadata(GeoPoint.Empty,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public static readonly DependencyProperty HotSpotXProperty = DependencyProperty.RegisterAttached("HotSpotX",
            typeof(HotSpot), typeof(Geo),
            new FrameworkPropertyMetadata(HotSpot.Center,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public static readonly DependencyProperty HotSpotYProperty = DependencyProperty.RegisterAttached("HotSpotY",
            typeof(HotSpot), typeof(Geo),
            new FrameworkPropertyMetadata(HotSpot.Center,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public static HotSpot GetHotSpotX(DependencyObject dependencyObject)
        {
            return (HotSpot) dependencyObject.GetValue(HotSpotXProperty);
        }

        public static void SetHotSpotX(DependencyObject dependencyObject, HotSpot value)
        {
            dependencyObject.SetValue(HotSpotXProperty, value);
        }

        public static HotSpot GetHotSpotY(DependencyObject dependencyObject)
        {
            return (HotSpot)dependencyObject.GetValue(HotSpotYProperty);
        }

        public static void SetHotSpotY(DependencyObject dependencyObject, HotSpot value)
        {
            dependencyObject.SetValue(HotSpotYProperty, value);
        }

        public static GeoArea GetArea(DependencyObject dependencyObject)
        {
            return (GeoArea) dependencyObject.GetValue(AreaProperty);
        }

        public static void SetArea(DependencyObject dependencyObject, GeoArea area)
        {
            dependencyObject.SetValue(AreaProperty, area);
        }

        public static GeoPoint GetLocation(DependencyObject dependencyObject)
        {
            return (GeoPoint) dependencyObject.GetValue(LocationProperty);
        }

        public static void SetLocation(DependencyObject dependencyObject, GeoPoint point)
        {
            dependencyObject.SetValue(LocationProperty, point);
        }
    }
}