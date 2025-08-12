using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace QuickJump2022.Behaviors;

public static class ListViewBehaviors {
    public static readonly DependencyProperty MaxVisibleItemsProperty =
        DependencyProperty.RegisterAttached(
            "MaxVisibleItems",
            typeof(int),
            typeof(ListViewBehaviors),
            new PropertyMetadata(10, OnMaxVisibleItemsChanged));

    public static void SetMaxVisibleItems(DependencyObject obj, int value) {
        obj.SetValue(MaxVisibleItemsProperty, value);
    }

    public static int GetMaxVisibleItems(DependencyObject obj) {
        return (int)obj.GetValue(MaxVisibleItemsProperty);
    }

    private static void OnMaxVisibleItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is ListBox listView) {
            listView.Loaded += (s, args) => UpdateListViewHeight(listView);
            listView.ItemContainerGenerator.StatusChanged += (s, args) => UpdateListViewHeight(listView);

            // Also listen to source collection changes
            var dpd = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ListView));
            dpd.AddValueChanged(listView, (s, args) => UpdateListViewHeight(listView));
        }
    }

    private static void UpdateListViewHeight(ListBox listView) {
        if (listView.Items.Count == 0) return;

        var maxVisibleItems = GetMaxVisibleItems(listView);
        var itemHeight = GetItemHeight(listView);

        if (itemHeight > 0) {
            var visibleItems = Math.Min(listView.Items.Count, maxVisibleItems);
            var totalHeight = visibleItems * itemHeight;
            listView.Height = totalHeight + 2; // TODO: why 2?
        }
    }

    private static double GetItemHeight(ListBox listView) {
        // Wait for containers to be generated
        if (listView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            return 0;

        var container = listView.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem;
        return container?.DesiredSize.Height ?? 0;
    }
}
