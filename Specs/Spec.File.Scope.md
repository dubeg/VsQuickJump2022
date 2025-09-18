# Spec: Files
Currently, pressing TAB cycling (search type) is only support for commands.
When the search type is File, implement tab cycling between Solution and Current Project.

- Implement the file search scope in FileService, to search for files in either the whole solution or the active project.
- Update the search form to use the selected scope.
- Use TAB cycling to select the file search scope.
- Update the status bar to display the current scope.

The default scope should be Current Solution.

