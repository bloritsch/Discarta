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

namespace DHaven.DisCarta.Internals
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;

    /// <summary>
    ///     Internal class to separate all the scroll view logic from the actual
    ///     map logic.
    /// </summary>
    public abstract class ScrollablePanel : Panel, IScrollInfo
    {
        protected const double VisualPrecision = 1;
        protected Rect PanelExtent;
        protected Rect ViewPort;

        /// <summary>
        ///     Gets or sets the amount the view port moves for line up/down/left/right
        ///     movement.
        /// </summary>
        protected double LineLength { get; set; } = 96 / 2.54; // 1 cm in DPU

        /// <summary>
        ///     Override this to perform work if the view port chnages size, etc.
        /// </summary>
        protected virtual void OnViewPortChanged()
        {
            ScrollOwner?.InvalidateScrollInfo();
            InvalidateArrange();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (availableSize != ViewPort.Size)
            {
                ViewPort.Size = new Size(
                    double.IsInfinity(availableSize.Width) ? ActualWidth : availableSize.Width,
                    double.IsInfinity(availableSize.Height) ? ActualHeight : availableSize.Height);

                CanHorizontallyScroll = ExtentWidth > ViewportWidth;
                CanVerticallyScroll = ExtentHeight > ViewportHeight;

                // Enforce the horizontal and vertical placement
                SetHorizontalOffset(HorizontalOffset);
                SetVerticalOffset(VerticalOffset);
                OnViewPortChanged();
            }

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(PanelExtent.Size);
            }

            return PanelExtent.Size;
        }

        #region Implementation of IScrollInfo

        /// <summary>Scrolls up within content by one logical unit. </summary>
        public void LineUp()
        {
            SetVerticalOffset(VerticalOffset - LineLength);
        }

        /// <summary>Scrolls down within content by one logical unit. </summary>
        public void LineDown()
        {
            SetVerticalOffset(VerticalOffset + LineLength);
        }

        /// <summary>Scrolls left within content by one logical unit.</summary>
        public void LineLeft()
        {
            SetHorizontalOffset(HorizontalOffset - LineLength);
        }

        /// <summary>Scrolls right within content by one logical unit.</summary>
        public void LineRight()
        {
            SetHorizontalOffset(HorizontalOffset + LineLength);
        }

        /// <summary>Scrolls up within content by one page.</summary>
        public void PageUp()
        {
            SetVerticalOffset(VerticalOffset - ViewportHeight);
        }

        /// <summary>Scrolls down within content by one page.</summary>
        public void PageDown()
        {
            SetVerticalOffset(VerticalOffset + ViewportHeight);
        }

        /// <summary>Scrolls left within content by one page.</summary>
        public void PageLeft()
        {
            SetHorizontalOffset(HorizontalOffset - ViewportWidth);
        }

        /// <summary>Scrolls right within content by one page.</summary>
        public void PageRight()
        {
            SetHorizontalOffset(HorizontalOffset + ViewportWidth);
        }

        /// <summary>Scrolls up within content after a user clicks the wheel button on a mouse.</summary>
        public virtual void MouseWheelUp()
        {
            SetVerticalOffset(VerticalOffset - LineLength * 3);
        }

        /// <summary>Scrolls down within content after a user clicks the wheel button on a mouse.</summary>
        public virtual void MouseWheelDown()
        {
            SetVerticalOffset(VerticalOffset + LineLength * 3);
        }

        /// <summary>Scrolls left within content after a user clicks the wheel button on a mouse.</summary>
        public virtual void MouseWheelLeft()
        {
            SetHorizontalOffset(HorizontalOffset - LineLength * 3);
        }

        /// <summary>Scrolls right within content after a user clicks the wheel button on a mouse.</summary>
        public virtual void MouseWheelRight()
        {
            SetHorizontalOffset(HorizontalOffset + LineLength * 3);
        }

        /// <summary>Sets the amount of horizontal offset.</summary>
        /// <param name="offset">The degree to which content is horizontally offset from the containing viewport.</param>
        public void SetHorizontalOffset(double offset)
        {
            // Ensure that maxOffset is at least zero in case the ViewPort is wider than the PanelExtent
            var maxOffset = Math.Max(0, ExtentWidth - ViewportWidth);
            var adjustedOffset = offset.ClipToRange(0, maxOffset, VisualPrecision);

            if (!adjustedOffset.IsSameAs(HorizontalOffset, VisualPrecision))
            {
                ViewPort.X = adjustedOffset;
                OnViewPortChanged();
            }
        }

        /// <summary>Sets the amount of vertical offset.</summary>
        /// <param name="offset">The degree to which content is vertically offset from the containing viewport.</param>
        public void SetVerticalOffset(double offset)
        {
            // Ensure that maxOffset is at least zero in case the ViewPort is wider than the PanelExtent
            var maxOffset = Math.Max(0, ExtentHeight - ViewportHeight);
            var adjustedOffset = offset.ClipToRange(0, maxOffset, VisualPrecision);

            if (!adjustedOffset.IsSameAs(VerticalOffset, VisualPrecision))
            {
                ViewPort.Y = adjustedOffset;
                OnViewPortChanged();
            }
        }

        /// <summary>
        ///     Forces content to scroll until the coordinate space of a <see cref="T:System.Windows.Media.Visual" /> object
        ///     is visible.
        /// </summary>
        /// <returns>A <see cref="T:System.Windows.Rect" /> that is visible.</returns>
        /// <param name="visual">A <see cref="T:System.Windows.Media.Visual" /> that becomes visible.</param>
        /// <param name="rectangle">A bounding rectangle that identifies the coordinate space to make visible.</param>
        public abstract Rect MakeVisible(Visual visual, Rect rectangle);

        /// <summary>Gets or sets a value that indicates whether scrolling on the vertical axis is possible. </summary>
        /// <returns>true if scrolling is possible; otherwise, false. This property has no default value.</returns>
        public bool CanVerticallyScroll { get; set; }

        /// <summary>Gets or sets a value that indicates whether scrolling on the horizontal axis is possible.</summary>
        /// <returns>true if scrolling is possible; otherwise, false. This property has no default value.</returns>
        public bool CanHorizontallyScroll { get; set; }

        /// <summary>Gets the horizontal size of the extent.</summary>
        /// <returns>
        ///     A <see cref="T:System.Double" /> that represents, in device independent pixels, the horizontal size of the
        ///     extent. This property has no default value.
        /// </returns>
        public double ExtentWidth => PanelExtent.Width;

        /// <summary>Gets the vertical size of the extent.</summary>
        /// <returns>
        ///     A <see cref="T:System.Double" /> that represents, in device independent pixels, the vertical size of the
        ///     extent.This property has no default value.
        /// </returns>
        public double ExtentHeight => PanelExtent.Height;

        /// <summary>Gets the horizontal size of the viewport for this content.</summary>
        /// <returns>
        ///     A <see cref="T:System.Double" /> that represents, in device independent pixels, the horizontal size of the
        ///     viewport for this content. This property has no default value.
        /// </returns>
        public double ViewportWidth => ViewPort.Width;

        /// <summary>Gets the vertical size of the viewport for this content.</summary>
        /// <returns>
        ///     A <see cref="T:System.Double" /> that represents, in device independent pixels, the vertical size of the
        ///     viewport for this content. This property has no default value.
        /// </returns>
        public double ViewportHeight => ViewPort.Height;

        /// <summary>Gets the horizontal offset of the scrolled content.</summary>
        /// <returns>
        ///     A <see cref="T:System.Double" /> that represents, in device independent pixels, the horizontal offset. This
        ///     property has no default value.
        /// </returns>
        public double HorizontalOffset => ViewPort.X;

        /// <summary>Gets the vertical offset of the scrolled content.</summary>
        /// <returns>
        ///     A <see cref="T:System.Double" /> that represents, in device independent pixels, the vertical offset of the
        ///     scrolled content. Valid values are between zero and the
        ///     <see cref="P:System.Windows.Controls.Primitives.IScrollInfo.ExtentHeight" /> minus the
        ///     <see cref="P:System.Windows.Controls.Primitives.IScrollInfo.ViewportHeight" />. This property has no default value.
        /// </returns>
        public double VerticalOffset => ViewPort.Y;

        private ScrollViewer scrollOwner;

        /// <summary>Gets or sets a <see cref="T:System.Windows.Controls.ScrollViewer" /> element that controls scrolling behavior.</summary>
        /// <returns>
        ///     A <see cref="T:System.Windows.Controls.ScrollViewer" /> element that controls scrolling behavior. This
        ///     property has no default value.
        /// </returns>
        public ScrollViewer ScrollOwner
        {
            get { return scrollOwner; }
            set
            {
                if (ReferenceEquals(scrollOwner, value))
                {
                    return;
                }

                scrollOwner = value;

                if (scrollOwner == null)
                {
                    return;
                }

                // Hase to be done on the dispatcher or it doesn't update right away
                Action action = () =>
                {
                    // Ensure we have the scroll bar functionality, but the scrollbars are not visible.
                    scrollOwner.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    scrollOwner.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                };

                scrollOwner.Dispatcher.BeginInvoke(action);
            }
        }

        #endregion
    }
}