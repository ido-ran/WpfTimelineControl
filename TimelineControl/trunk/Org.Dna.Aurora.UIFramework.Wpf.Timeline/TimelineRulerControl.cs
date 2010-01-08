﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;

namespace Org.Dna.Aurora.UIFramework.Wpf.Timeline {

	/// <summary>
	/// Timeline Ruler control display ruler of dates.
	/// </summary>
	class TimelineRulerControl : Control {

		static TimelineRulerControl() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TimelineRulerControl),
				new FrameworkPropertyMetadata(typeof(TimelineRulerControl)));

			MinimumDateProperty = Timeline.MinimumDateProperty.AddOwner(typeof(TimelineRulerControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure, TimeframeChanged));
			MaximumDateProperty = Timeline.MaximumDateProperty.AddOwner(typeof(TimelineRulerControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure, TimeframeChanged));
			TickTimeSpanProperty = Timeline.TickTimeSpanProperty.AddOwner(typeof(TimelineRulerControl), new FrameworkPropertyMetadata(Timeline.TickTimeSpanDefaultValue, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, TicksTimeSpanChanged));
		}

		public static readonly DependencyProperty MaximumDateProperty;
		public static readonly DependencyProperty MinimumDateProperty;
		public static readonly DependencyProperty TickTimeSpanProperty;

		public Nullable<DateTime> MaximumDate {
			get { return (Nullable<DateTime>)GetValue(MaximumDateProperty); }
			set { SetValue(MaximumDateProperty, value); }
		}

		public Nullable<DateTime> MinimumDate {
			get { return (Nullable<DateTime>)GetValue(MinimumDateProperty); }
			set { SetValue(MinimumDateProperty, value); }
		}

		public TimeSpan TickTimeSpan {
			get { return (TimeSpan)GetValue(TickTimeSpanProperty); }
			set { SetValue(TickTimeSpanProperty, value); }
		}



		public TimeSpan BlockTimeSpan {
			get { return (TimeSpan)GetValue(BlockTimeSpanProperty); }
			set { SetValue(BlockTimeSpanProperty, value); }
		}

		// Using a DependencyProperty as the backing store for BlockTimeSpan.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BlockTimeSpanProperty =
				DependencyProperty.Register("BlockTimeSpan", typeof(TimeSpan), typeof(TimelineRulerControl), new FrameworkPropertyMetadata(TimeSpan.FromDays(1)));


		private static readonly DependencyPropertyKey RulerBlocksPropertyKey =
			DependencyProperty.RegisterReadOnly("RulerBlocks", typeof(IList<RulerBlockItem>),
			typeof(TimelineRulerControl), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty RulerBlocksProperty =
			RulerBlocksPropertyKey.DependencyProperty;

		public IList<RulerBlockItem> RulerBlocks {
			get { return (IList<RulerBlockItem>)GetValue(RulerBlocksProperty); }
			private set { SetValue(RulerBlocksPropertyKey, value); }
		}

		private static readonly DependencyPropertyKey DpiPerBlockPropertyKey =
			DependencyProperty.RegisterReadOnly("DpiPerBlock", typeof(double),
			typeof(TimelineRulerControl), new FrameworkPropertyMetadata(1D));

		public static readonly DependencyProperty DpiPerBlockProperty =
			DpiPerBlockPropertyKey.DependencyProperty;

		public double DpiPerBlock {
			get { return (double)GetValue(DpiPerBlockProperty); }
			private set { SetValue(DpiPerBlockPropertyKey, value); }
		}

		public TimelineRulerControl() {

		}

		private bool blockUpdatePending = false;

		private static void TimeframeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
			TimelineRulerControl ruler = (TimelineRulerControl)o;
			ruler.blockUpdatePending = true;
			//ruler.UpdateRulerBlocks();
		}

		protected override Size MeasureOverride(Size constraint) {
			MeasureEffectiveBlockSpan();

			if (blockUpdatePending) {
				blockUpdatePending = false;
				UpdateRulerBlocks();
			}

			return base.MeasureOverride(constraint);
		}

		private ScrollViewer _headerSV;
		private ScrollViewer _mainSV;

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();

			ScrollViewer viewer = this._headerSV;
			this._headerSV = base.Parent as ScrollViewer;
			if (viewer != this._headerSV) {
				if (viewer != null) {
					viewer.ScrollChanged -= new ScrollChangedEventHandler(this.OnHeaderScrollChanged);
				}
				if (this._headerSV != null) {
					this._headerSV.ScrollChanged += new ScrollChangedEventHandler(this.OnHeaderScrollChanged);
				}
			}
			ScrollViewer viewer2 = this._mainSV;
			this._mainSV = base.TemplatedParent as ScrollViewer;
			if (viewer2 != this._mainSV) {
				if (viewer2 != null) {
					viewer2.ScrollChanged -= new ScrollChangedEventHandler(this.OnMasterScrollChanged);
				}
				if (this._mainSV != null) {
					this._mainSV.ScrollChanged += new ScrollChangedEventHandler(this.OnMasterScrollChanged);
				}
			}

		}

		private void OnHeaderScrollChanged(object sender, ScrollChangedEventArgs e) {
			if ((this._mainSV != null) && (this._headerSV == e.OriginalSource)) {
				this._mainSV.ScrollToHorizontalOffset(e.HorizontalOffset);
			}
		}

		private void OnMasterScrollChanged(object sender, ScrollChangedEventArgs e) {
			if ((this._headerSV != null) && (this._mainSV == e.OriginalSource)) {
				this._headerSV.ScrollToHorizontalOffset(e.HorizontalOffset);
			}
		} 


		private static void TicksTimeSpanChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
			TimelineRulerControl ruler = (TimelineRulerControl)o;
			ruler.updateDpiPerBlock = true;
		}

		private Nullable<TimeSpan> effectiveBlockSpan;
		private bool updateDpiPerBlock = false;

		private void MeasureEffectiveBlockSpan() {
			if (updateDpiPerBlock) {
				updateDpiPerBlock = false;
				effectiveBlockSpan = null;

				double pixelPerTick = 1D / TickTimeSpan.Ticks;
				DpiPerBlock = pixelPerTick * EffectiveBlockTimeSpan.Ticks;

				UpdateRulerBlocks();
			}
		}

		private TimeSpan EffectiveBlockTimeSpan {
			get {
				if (effectiveBlockSpan == null) {
					double pixelPerTick = 1D / TickTimeSpan.Ticks;
					long ticks = (long) (150 / pixelPerTick);
					effectiveBlockSpan = TimeSpan.FromTicks(ticks);
				//  double ratio = BlockTimeSpan.Ticks / TickTimeSpan.Ticks;
				//  if (ratio < 500) {
				//    effectiveBlockSpan = BlockTimeSpan;
				//  }
				//  else {
				//    effectiveBlockSpan = TimeSpan.FromTicks(TickTimeSpan.Ticks / 500);
				//  }
				}

				return effectiveBlockSpan.Value;
			}
		}

		private void UpdateRulerBlocks() {
			if (MinimumDate.HasValue && MaximumDate.HasValue) {
				TimeSpan timeframe = MaximumDate.Value - MinimumDate.Value;
				int totalBlocks = (int)Math.Ceiling((double) (timeframe.Ticks / EffectiveBlockTimeSpan.Ticks));
				totalBlocks++;

				List<RulerBlockItem> blocks = new List<RulerBlockItem>();

				TimeSpan spanFromStart = EffectiveBlockTimeSpan;
				DateTime prev = MinimumDate.Value;

				for (int blockIdx = 0; blockIdx < totalBlocks; blockIdx++) {
					DateTime current = MinimumDate.Value.Add(spanFromStart);

					RulerBlockItem block = new RulerBlockItem();
					block.Start = prev;
					block.Span = EffectiveBlockTimeSpan;
					//block.Text = prev.ToString();
					blocks.Add(block);

					prev = current;
					spanFromStart = spanFromStart.Add(EffectiveBlockTimeSpan);
				}

				RulerBlocks = blocks;
			}
		}

	}

	public class RulerBlockSizeConverter : IMultiValueConverter {

		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (values == null || values.Length < 2 ||
				values[0] == DependencyProperty.UnsetValue ||
				values[1] == DependencyProperty.UnsetValue) {
				return 0.0d;
			}

			TimeSpan tickTimeSpan = (TimeSpan)values[0];
			double pixelPerTick = 1D / tickTimeSpan.Ticks;
			TimeSpan span = (TimeSpan)values[1];

			double width = span.Ticks * pixelPerTick;

			return width;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}

	}
}
