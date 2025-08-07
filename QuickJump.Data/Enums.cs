using System;

namespace QuickJump2022.Data;

public static class Enums
{
	public enum ESearchType
	{
		None,
		Files,
		Methods,
		All
	}

	public enum EBindType
	{
		None,
		Namespace,
		Class,
		Method,
		Property,
		Field,
		Enum,
		Delegate,
		Event,
		Interface,
		Struct
	}

	[Flags]
	public enum EAccessType
	{
		Sealed = 0,
		Static = 1,
		Const = 2,
		Public = 4,
		Private = 8,
		Protected = 0x10,
		Internal = 0x20
	}

	public enum SortType
	{
		Weight = 0,
		WeightReverse = 1,
		Alphabetical = 10,
		AlphabeticalReverse = 11,
		LineNumber = 20,
		LineNumberReverse = 21
	}
}
