using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace Org.Dna.Aurora.UIFramework.Wpf.Timeline {


	public class TimelineControl : Control {

		private static readonly DateTime EmptyDate = new DateTime(2000, 01, 01);

		private TimelineItemsPresenter itemsPresenter;
		private ConnectionsPresenter connectionsPresenter;
		private ScrollViewer scrollViewer;

		private bool setCurrentTimePending = false;

		#region DP

		private static readonly DependencyPropertyKey ItemsPropertyKey =
			DependencyProperty.RegisterReadOnly("Items", typeof(IList<object>),
				typeof(TimelineControl), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty ItemsProperty =
			ItemsPropertyKey.DependencyProperty;


		public IList<object> Items {
			get { return (IList<object>)GetValue(ItemsProperty); }
			private set { SetValue(ItemsPropertyKey, value); }
		}



		public IEnumerable ItemsSource {
			get { return (IEnumerable)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ItemsSourceProperty =
				DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(TimelineControl), new FrameworkPropertyMetadata(null, ItemsSourceChanged));


		private static void ItemsSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
			TimelineControl self = (TimelineControl)o;
			self.ItemsSourceChanged(e);
		}

		private void ItemsSourceChanged(DependencyPropertyChangedEventArgs e) {
			if (e.NewValue != null) {
				IEnumerable items = (IEnumerable)e.NewValue;
				AddItemsInternal(items);

        INotifyCollectionChanged oldCollectionChanged = e.OldValue as INotifyCollectionChanged;
        if (oldCollectionChanged != null) {
          oldCollectionChanged.CollectionChanged -= ItemsSource_CollectionChanged;
        }

				INotifyCollectionChanged collectionChanged = e.NewValue as INotifyCollectionChanged;
				if (collectionChanged != null) {
					collectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(ItemsSource_CollectionChanged);
				}
			}
		}

		void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if (e.Action == NotifyCollectionChangedAction.Add) {
				AddItemInternal(e.NewItems[0]);
			}
      else if (e.Action == NotifyCollectionChangedAction.Remove) {
        RemoveItemsInternal(e.OldItems);
      }
		}

		private void AddItemsInternal(IEnumerable items) {
			foreach (object item in items) {
				AddItemInternal(item);
			}
		}

		private void AddItemInternal(object item) {
			Items.Add(item);
		}

    private void RemoveItemsInternal(IEnumerable items) {
      foreach (object item in items) {
        RemoveItemInternal(item);
      }
    }

    private void RemoveItemInternal(object item) {
      Items.Remove(item);
    }

		private static readonly DependencyPropertyKey ConnectionsPropertyKey =
			DependencyProperty.RegisterReadOnly("Connections", typeof(IList<object>),
			typeof(TimelineControl), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty ConnectionsProperty =
			ConnectionsPropertyKey.DependencyProperty;

		public IList<object> Connections {
			get { return (IList<object>)GetValue(ConnectionsProperty); }
			private set { SetValue(ConnectionsPropertyKey, value); }
		}



		public IEnumerable ConnectionsSource {
			get { return (IEnumerable)GetValue(ConnectionsSourceProperty); }
			set { SetValue(ConnectionsSourceProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ConnectionsSource.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ConnectionsSourceProperty =
				DependencyProperty.Register("ConnectionsSource", typeof(IEnumerable), typeof(TimelineControl), new FrameworkPropertyMetadata(null, ConnectionsSourceChanged));


		private static void ConnectionsSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
			TimelineControl self = (TimelineControl)o;
			self.ConnectionsSourceChanged(e);
		}

		private void ConnectionsSourceChanged(DependencyPropertyChangedEventArgs e) {
			if (e.NewValue != null) {
				IEnumerable items = (IEnumerable)e.NewValue;
				AddConnectionsInternal(items);

        INotifyCollectionChanged oldConnectionsSource = e.OldValue as INotifyCollectionChanged;
        if (oldConnectionsSource != null) {
          oldConnectionsSource.CollectionChanged -= ConnectionsSource_CollectionChanged;
        }

        INotifyCollectionChanged collectionChanged = e.NewValue as INotifyCollectionChanged;
        if (collectionChanged != null) {
          collectionChanged.CollectionChanged += ConnectionsSource_CollectionChanged;
        }
			}
		}

    void ConnectionsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
      if (e.Action == NotifyCollectionChangedAction.Add) {
        AddConnectionsInternal(e.NewItems);
      }
      else if (e.Action == NotifyCollectionChangedAction.Remove) {
        RemoveConnectionsInternal(e.OldItems);
      }
    }

		private void AddConnectionsInternal(IEnumerable items) {
			foreach (object item in items) {
				AddConnectionInternal(item);
			}
		}

		private void AddConnectionInternal(object item) {
			Connections.Add(item);
		}

    private void RemoveConnectionsInternal(IEnumerable items) {
      foreach (var item in items) {
        RemoveConnectionInternal(item);
      }
    }

    private void RemoveConnectionInternal(object item) {
      Connections.Remove(item);
    }


		public ItemsPanelTemplate ItemsPanel {
			get { return (ItemsPanelTemplate)GetValue(ItemsPanelProperty); }
			set { SetValue(ItemsPanelProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ItemsPanel.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ItemsPanelProperty =
				DependencyProperty.Register("ItemsPanel", typeof(ItemsPanelTemplate), typeof(TimelineControl),
				ItemsPanelMetadata());

		private static FrameworkPropertyMetadata ItemsPanelMetadata() {
			var defaultPanelTemplate = 
				new ItemsPanelTemplate(
					new FrameworkElementFactory(typeof(TimelineCompactPanel))
					);
			var md = new FrameworkPropertyMetadata(defaultPanelTemplate);
			return md;
		}



		public StyleSelector ItemContainerStyleSelector {
			get { return (StyleSelector)GetValue(ItemContainerStyleSelectorProperty); }
			set { SetValue(ItemContainerStyleSelectorProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ItemContainerStyleSelector.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ItemContainerStyleSelectorProperty =
				DependencyProperty.Register("ItemContainerStyleSelector", typeof(StyleSelector), typeof(TimelineControl), 
				new FrameworkPropertyMetadata(null));




		public StyleSelector ConnectionStyleSelector {
			get { return (StyleSelector)GetValue(ConnectionStyleSelectorProperty); }
			set { SetValue(ConnectionStyleSelectorProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ConnectionStyleSelector.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ConnectionStyleSelectorProperty =
				DependencyProperty.Register("ConnectionStyleSelector", typeof(StyleSelector), typeof(TimelineControl), 
				new FrameworkPropertyMetadata(null));


		
		public Nullable<DateTime> MaximumDate {
			get { return (Nullable<DateTime>)GetValue(MaximumDateProperty); }
			set { SetValue(MaximumDateProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MaximumDate.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MaximumDateProperty;




		public Nullable<DateTime> MinimumDate {
			get { return (Nullable<DateTime>)GetValue(MinimumDateProperty); }
			set { SetValue(MinimumDateProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MinimumDate.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MinimumDateProperty;





		public TimeSpan TickTimeSpan {
			get { return (TimeSpan)GetValue(TickTimeSpanProperty); }
			set { SetValue(TickTimeSpanProperty, value); }
		}

		// Using a DependencyProperty as the backing store for TickTimeSpan.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TickTimeSpanProperty;

		private static void TickTimeSpanChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
			TimelineControl self = (TimelineControl)o;
			self.TickTimeSpanChanged(e);
		}

		private void TickTimeSpanChanged(DependencyPropertyChangedEventArgs e) {
			setCurrentTimePending = true;
		}


		public DateTime CurrentTime {
			get { return (DateTime)GetValue(CurrentTimeProperty); }
			set { SetValue(CurrentTimeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for CurrentTime.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CurrentTimeProperty =
				DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(TimelineControl), new FrameworkPropertyMetadata(DateTime.MinValue, CurrentTimeChanged));

		private static void CurrentTimeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
			TimelineControl self = (TimelineControl)o;
			self.CurrentTimeChanged(e);
		}

		private bool ignoreCurrentChanged = false;

		private void CurrentTimeChanged(DependencyPropertyChangedEventArgs e) {
			if (ignoreCurrentChanged) return;

			GoToDate((DateTime)e.NewValue);
		}

		private static readonly DependencyPropertyKey MaximumTickTimeSpanPropertyKey =
			DependencyProperty.RegisterReadOnly("MaximumTickTimeSpan", typeof(TimeSpan),
			typeof(TimelineControl), new FrameworkPropertyMetadata(TimeSpan.FromDays(1)));

		public static readonly DependencyProperty MaximumTickTimeSpanProperty =
			MaximumTickTimeSpanPropertyKey.DependencyProperty;

		public TimeSpan MaximumTickTimeSpan {
			get { return (TimeSpan)GetValue(MaximumTickTimeSpanProperty); }
			private set { SetValue(MaximumTickTimeSpanPropertyKey, value); }
		}



    /// <summary>
    /// Get indication wether there are bounds to the timeline control or not.
    /// </summary>
    public bool IsNoBounds {
      get { return (bool)GetValue(IsNoBoundsProperty); }
      private set { SetValue(IsNoBoundsPropertyKey, value); }
    }

    // Using a DependencyProperty as the backing store for IsNoBounds.  This enables animation, styling, binding, etc...
    private static readonly DependencyPropertyKey IsNoBoundsPropertyKey =
      DependencyProperty.RegisterReadOnly("IsNoBounds", typeof(bool), typeof(TimelineControl), new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty IsNoBoundsProperty =
        IsNoBoundsPropertyKey.DependencyProperty;




    public object NoBounds {
      get { return (object)GetValue(NoBoundsProperty); }
      set { SetValue(NoBoundsProperty, value); }
    }

    // Using a DependencyProperty as the backing store for NoBoundsContent.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty NoBoundsProperty =
        DependencyProperty.Register("NoBounds", typeof(object), typeof(TimelineControl), new UIPropertyMetadata("No bounds are set"));


    public DataTemplate NoBoundsContentTemplate {
      get { return (DataTemplate)GetValue(NoBoundsContentTemplateProperty); }
      set { SetValue(NoBoundsContentTemplateProperty, value); }
    }

    // Using a DependencyProperty as the backing store for NoBoundsContentTemplate.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty NoBoundsContentTemplateProperty =
        DependencyProperty.Register("NoBoundsContentTemplate", typeof(DataTemplate), typeof(TimelineControl), new UIPropertyMetadata(null));



    public DataTemplateSelector NoBoundsContentTemplateSelector {
      get { return (DataTemplateSelector)GetValue(NoBoundsContentTemplateSelectorProperty); }
      set { SetValue(NoBoundsContentTemplateSelectorProperty, value); }
    }

    // Using a DependencyProperty as the backing store for NoBoundsContentTemplateSelector.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty NoBoundsContentTemplateSelectorProperty =
        DependencyProperty.Register("NoBoundsContentTemplateSelector", typeof(DataTemplateSelector), typeof(TimelineControl), new UIPropertyMetadata(null));



    public string NoBoundsStringFormat {
      get { return (string)GetValue(NoBoundsStringFormatProperty); }
      set { SetValue(NoBoundsStringFormatProperty, value); }
    }

    // Using a DependencyProperty as the backing store for NoBoundsStringFormat.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty NoBoundsStringFormatProperty =
        DependencyProperty.Register("NoBoundsStringFormat", typeof(string), typeof(TimelineControl), new UIPropertyMetadata(null));




		#endregion

		static TimelineControl() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TimelineControl), new FrameworkPropertyMetadata(typeof(TimelineControl)));

			MinimumDateProperty = Timeline.MinimumDateProperty.AddOwner(typeof(TimelineControl), new FrameworkPropertyMetadata(null, MinimumMaximumDateChanged));
      MaximumDateProperty = Timeline.MaximumDateProperty.AddOwner(typeof(TimelineControl), new FrameworkPropertyMetadata(null, MinimumMaximumDateChanged));
			TickTimeSpanProperty = Timeline.TickTimeSpanProperty.AddOwner(typeof(TimelineControl),
				new FrameworkPropertyMetadata(Timeline.TickTimeSpanDefaultValue, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, TickTimeSpanChanged));
		}

    private bool recalcMaxZoom = true;

    private static void MinimumMaximumDateChanged(object sender, DependencyPropertyChangedEventArgs e) {
      TimelineControl self = (TimelineControl)sender;
      bool newIsNoBound = (self.MinimumDate == null && self.MaximumDate == null);
      bool isNoBoundChanged = self.IsNoBounds != newIsNoBound;
      self.IsNoBounds = newIsNoBound;
      if (isNoBoundChanged) {
        self.TickTimeSpan = Timeline.TickTimeSpanDefaultValue;
      }
      self.recalcMaxZoom = true;
    }

		public TimelineControl() {
			Items = new ObservableCollection<object>();
			Connections = new ObservableCollection<object>();
		}

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();

			itemsPresenter = Template.FindName("PART_ItemsPresenter", this) as TimelineItemsPresenter;
			connectionsPresenter = Template.FindName("PART_ConnectionsPresenter", this) as ConnectionsPresenter;
			scrollViewer = Template.FindName("PART_ScrollViewer", this) as ScrollViewer;

			WireScrollViewer();
			WireConnectionsPresenter();
		}

		private void WireConnectionsPresenter() {
			if (connectionsPresenter != null) {
				connectionsPresenter.Timeline = this;
			}
		}

		private void WireScrollViewer() {
			if (scrollViewer != null) {
				scrollViewer.ScrollChanged += new ScrollChangedEventHandler(ScrollViewer_ScrollChanged);
			}
		}

		private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
			if (setCurrentTimePending) {
				setCurrentTimePending = false;
				GoToDate(CurrentTime);
			}
			else if (e.HorizontalChange != 0) {
				ignoreCurrentChanged = true;
				CurrentTime = Timeline.OffsetToDate(e.HorizontalOffset, this);
				ignoreCurrentChanged = false;
			}
		}

		public void GoToDate(DateTime date) {
			if (scrollViewer == null) return;

			double offset = Timeline.DateToOffset(date, this);
			if (offset >= 0) {
				scrollViewer.ScrollToHorizontalOffset(offset);
			}
		}

		internal TimelineItem ContainerFromItem(object item) {
			if (itemsPresenter == null) return null;

			return itemsPresenter.ContainerFromItem(item);
		}

		protected override Size ArrangeOverride(Size arrangeBounds) {
			Size actual = base.ArrangeOverride(arrangeBounds);

      if (recalcMaxZoom) {
        recalcMaxZoom = false;
				SetMaximumZoomFactor(actual);
			}

			return actual;
		}

		private void SetMaximumZoomFactor(Size actualSize) {
			if (MaximumDate.HasValue && MinimumDate.HasValue) {
				TimeSpan timeframe = MaximumDate.Value - MinimumDate.Value;
				double tickPerTimeSpan = timeframe.Ticks / actualSize.Width;

				MaximumTickTimeSpan = TimeSpan.FromTicks((long)(tickPerTimeSpan));
			}
      else {
        MaximumTickTimeSpan = TimeSpan.Zero;
      }
		}


		public void BringItemIntoView(object item) {
			TimelineItem container = ContainerFromItem(item);
			if (container != null) {
				Nullable<DateTime> start = TimelineCompactPanel.GetStartDate(container);
				if (start.HasValue) {
					GoToDate(start.Value);
				}
			}
		}

		public void BringIntoView(
			TimelineControlBringIntoViewMode mode, object dataItem) {

			TimelineItem container = ContainerFromItem(dataItem);
			if (container != null) {
				Nullable<DateTime> start = TimelineCompactPanel.GetStartDate(container);
				Nullable<DateTime> end = TimelineCompactPanel.GetEndDate(container);

				if (IsSetZoomToFit(mode) && start.HasValue && end.HasValue) {
					TimeSpan duration = end.Value - start.Value;
					double pixelPerTick = (ActualWidth / 2) / duration.Ticks;
					TimeSpan newTickTimeSpan = TimeSpan.FromTicks((long) (1D/pixelPerTick));

					if (newTickTimeSpan.TotalMinutes < 1) {
						newTickTimeSpan = TimeSpan.FromMinutes(1);
					}

					if (newTickTimeSpan < TickTimeSpan) {
						TickTimeSpan = newTickTimeSpan;
					}
					else {
						if (ActualWidth / 2 < duration.Ticks * Timeline.GetPixelsPerTick(this)) {
							TickTimeSpan = newTickTimeSpan;
						}
					}

					WpfUtility.WaitForPriority(DispatcherPriority.Background);
				}

				if (IsSetCurrentTime(mode)) {
					if (start.HasValue) CurrentTime = start.Value;
					else if (end.HasValue) CurrentTime = end.Value;
				}
			}
		}

		private static bool IsSetZoomToFit(TimelineControlBringIntoViewMode mode) {
			return ((mode & TimelineControlBringIntoViewMode.SetZoomToFit) != 0);
		}

		private static bool IsSetCurrentTime(TimelineControlBringIntoViewMode mode) {
			return ((mode & TimelineControlBringIntoViewMode.SetCurrentTime) != 0);
		}
	}

	[Flags]
	public enum TimelineControlBringIntoViewMode {
		SetCurrentTime = 1,
		SetZoomToFit = 2,

		FcosOnItem = SetCurrentTime | SetZoomToFit
	}
}
