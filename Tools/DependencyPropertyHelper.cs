using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QuickJump2022.Tools;

/// <summary>
/// Dependency Property Utilities
/// </summary>
public static class DP {
    public static DependencyProperty Register<TOwner, TProperty>(
        string name,
        TProperty defaultValue = default,
        PropertyChangedCallback propertyChanged = null
    ) {
        return DependencyProperty.Register(
            name,
            typeof(TProperty),
            typeof(TOwner),
            new PropertyMetadata(defaultValue, propertyChanged)
        );
    }
}