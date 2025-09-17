# TODO: Files
Currently, pressing TAB cycling (search type) is only support for commands.
When the search type is Symbol, implement tab cycling between Current Document, Current Project, and Current Solution.
In the dialog's status bar, display the scope as "Document", "Project" or "Solution.

You'll have to implement the symbol search scope in SymbolService, to search for symbols in any of the scopes.

The default scope should be Current Document.

