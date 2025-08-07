using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using QuickJump2022.Data;
using QuickJump2022.Options;
using QuickJump2022.Properties;

namespace QuickJump2022.Tools;

public static class Utilities
{
	public static Dictionary<string, Icon> FileTypeImages = new Dictionary<string, Icon>();

	public static Dictionary<Enums.EBindType, Icon> CodeImages = new Dictionary<Enums.EBindType, Icon>();

	public static Icon DefaultFileIcon = null;

	public static void PreloadCodeIcons()
	{
		Enums.EBindType[] array = (Enums.EBindType[])Enum.GetValues(typeof(Enums.EBindType));
		foreach (Enums.EBindType type in array)
		{
			if (!CodeImages.ContainsKey(type))
			{
				GetCodeIcon(type);
			}
		}
	}

	public static Icon GetMimeTypeIcon(string fileName)
	{
		FileInfo fileInfo = new FileInfo(fileName);
		try
		{
			_ = SystemIcons.WinLogo;
			if (!FileTypeImages.ContainsKey(fileInfo.Extension))
			{
				FileTypeImages.Add(fileInfo.Extension, Icon.ExtractAssociatedIcon(fileInfo.FullName));
			}
			return FileTypeImages[fileInfo.Extension];
		}
		catch (Exception)
		{
			if (DefaultFileIcon == null)
			{
				DefaultFileIcon = Icon.FromHandle(new Bitmap(Resource1.FileTransparent).GetHicon());
			}
			FileTypeImages.Add(fileInfo.Extension, DefaultFileIcon);
			return FileTypeImages[fileInfo.Extension];
		}
	}

	public static Icon GetCodeIcon(Enums.EBindType type)
	{
		if (CodeImages.ContainsKey(type))
		{
			return CodeImages[type];
		}
		GeneralOptionsPage options = QuickJumpData.Instance.GeneralOptions;
		Icon icon = type switch
		{
			Enums.EBindType.None => Icon.FromHandle(options.UseModernIcons ? Resource1.ModernGhost.GetHicon() : Resource1.Ghost.GetHicon()), 
			Enums.EBindType.Namespace => Icon.FromHandle(options.UseModernIcons ? Resource1.ModernNamespace.GetHicon() : Resource1.Namespace.GetHicon()), 
			Enums.EBindType.Class => Icon.FromHandle(options.UseModernIcons ? Resource1.ModernClass.GetHicon() : Resource1.Class.GetHicon()), 
			Enums.EBindType.Method => Icon.FromHandle(options.UseModernIcons ? Resource1.ModernMethod.GetHicon() : Resource1.Method.GetHicon()), 
			Enums.EBindType.Property => Icon.FromHandle(options.UseModernIcons ? Resource1.ModernProperty.GetHicon() : Resource1.Property.GetHicon()), 
			Enums.EBindType.Field => Icon.FromHandle(options.UseModernIcons ? Resource1.ModernField.GetHicon() : Resource1.Field.GetHicon()), 
			Enums.EBindType.Enum => Icon.FromHandle(options.UseModernIcons ? Resource1.ModernEnum.GetHicon() : Resource1.Enum.GetHicon()), 
			Enums.EBindType.Delegate => Icon.FromHandle(options.UseModernIcons ? Resource1.ModernDelegate.GetHicon() : Resource1.Delegate.GetHicon()), 
			Enums.EBindType.Event => Icon.FromHandle(options.UseModernIcons ? Resource1.ModernEvent.GetHicon() : Resource1.Event.GetHicon()), 
			Enums.EBindType.Interface => Icon.FromHandle(options.UseModernIcons ? Resource1.ModernInterface.GetHicon() : Resource1.Interface.GetHicon()), 
			Enums.EBindType.Struct => Icon.FromHandle(options.UseModernIcons ? Resource1.ModernStruct.GetHicon() : Resource1.Struct.GetHicon()), 
			_ => Icon.FromHandle(options.UseModernIcons ? Resource1.ModernGhost.GetHicon() : Resource1.Ghost.GetHicon()), 
		};
		if (icon == null)
		{
			return null;
		}
		CodeImages.Add(type, icon);
		return icon;
	}

	public static T TryGetProperty<T>(this ProjectItem projectItem, string property)
	{
		ThreadHelper.ThrowIfNotOnUIThread("TryGetProperty");
		try
		{
			return (T)projectItem.Properties.Item((object)property).Value;
		}
		catch (Exception)
		{
			return default(T);
		}
	}

	public static void DumpPropertyNames(this ProjectItem projectItem)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		ThreadHelper.ThrowIfNotOnUIThread("DumpPropertyNames");
		foreach (Property property2 in projectItem.Properties)
		{
			Property property = property2;
			string outputString = property.Name;
			if (property.Value != null)
			{
				outputString = outputString + ":" + property.Value.ToString();
			}
		}
	}

	public static bool Filter(string str, string srcStr)
	{
		if (srcStr.Length > 0)
		{
			return str.IndexOf(srcStr, StringComparison.OrdinalIgnoreCase) >= 0;
		}
		return true;
	}

	public static int Clamp(int value, int min, int max)
	{
		if (value < min)
		{
			return min;
		}
		if (value > max)
		{
			return max;
		}
		return value;
	}
}
