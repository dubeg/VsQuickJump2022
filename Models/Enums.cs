namespace QuickJump2022.Models;

public static class Enums {
    public enum SearchType {
        None,
        Files,
        Symbols,
        Commands,
        KnownCommands,
        FastFetchCommands,
        All,
    }

    public enum FileSearchScope {
        Solution,
        ActiveProject
    }

    public enum SymbolSearchScope {
        All,
        Type, // Includes Class, Struct, Interface, Enum, Delegate
        Method, // Includes Method, ctor, Operator
        Field, // Includes Field, Property, Event
    }

    public enum TokenType {
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
        Struct,
        Constructor,
        Indexer,
        Operator,
        Record,
        RecordStruct,
        Text,
        ParameterName
    }

[Flags]
    public enum ModifierType {
        None = 0,
        Static = 1,
        Const = 2,
        Public = 4,
        Private = 8,
        Protected = 0x10,
        Internal = 0x20,
        Abstract = 0x40,
        Virtual = 0x80,
        Override = 0x100,
        Sealed = 0x200,
        Async = 0x400,
        Readonly = 0x800,
        Partial = 0x1000,
        Extern = 0x2000,
        Record = 0x4000
    }

    public enum SortType {
        Weight = 0,
        WeightReverse = 1,
        Alphabetical = 10,
        AlphabeticalReverse = 11,
        LineNumber = 20,
        LineNumberReverse = 21,
        Fuzzy = 30,
        FuzzyReverse = 31
    }
}
