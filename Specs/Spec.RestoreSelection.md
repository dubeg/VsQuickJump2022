# Spec: Restore selection
For the command search type, we try to restore the selected command in the list of the dialog (SearchForm).
However, saving the selected command's text & restoring it to match exactly the last selected item turned out to be annoying, since it wasn't the user's text.

So, edit the restoring mechanishm to restore the user's text, but use the saved selected command text to restore the selection in the list.