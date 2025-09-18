# Spec: StatusBar

Add a status bar to the search dialog, to display the search type & scope.
Both the search type and scope will have an icon (using known monikers).
Place the search type (file, command, symbol) to the left-most, along with its icon. 
Place the scope to the right-most, with its icon as well.


For commands, the displayed info will be something like this:

- "Commands : Canonical Name"
- "Commands : Friendly Name"
- "Commands : Custom Name"

When we will implement File search scopes & Symbol search scopes, a file search will display either:

- "Files: Solution"
- "Files: Active Project"

A symbol search will display either:

- "Symbols: Solution"
- "Symbols: Active Project"
- "Symbols: Document"


