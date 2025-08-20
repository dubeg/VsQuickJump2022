using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace QuickJump2022.Behaviors;

public static class ListViewBehaviors {
    public static readonly DependencyProperty MaxVisibleItemsProperty =
        DependencyProperty.RegisterAttached(
            "MaxVisibleItems",
            typeof(int),
            typeof(ListViewBehaviors),
            new PropertyMetadata(10, OnMaxVisibleItemsChanged)
    );

    public static void SetMaxVisibleItems(DependencyObject obj, int value) => obj.SetValue(MaxVisibleItemsProperty, value);
    public static int GetMaxVisibleItems(DependencyObject obj) => (int)obj.GetValue(MaxVisibleItemsProperty);

    private static void OnMaxVisibleItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is ListBox listView) {
            listView.Loaded += (s, args) => UpdateListViewHeight(listView);
            var dpd = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ListView));
            dpd.AddValueChanged(listView, (s, args) => UpdateListViewHeight(listView));
        }
    }

    private static void UpdateListViewHeight(ListBox listView) {
        if (listView.Items.Count == 0) return;
        var maxVisibleItems = GetMaxVisibleItems(listView);
        if (listView.Items.Count > maxVisibleItems) {
            var setters = listView.ItemContainerStyle.Setters.OfType<Setter>();
            var itemHeight = Convert.ToInt32(setters.FirstOrDefault(s => s.Property == FrameworkElement.HeightProperty)?.Value ?? 0);
            if (itemHeight > 0) {
                var visibleItems = Math.Min(listView.Items.Count, 20);
                var totalHeight = visibleItems * itemHeight;
                listView.MaxHeight = totalHeight + 2; // TODO: why 2?
            }
        }
    }
}
