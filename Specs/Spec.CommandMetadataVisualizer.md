# Command Metadata
- [ ] Add a section between statusBar & results (list) to show command metadata.
    + Image moniker
    + Command canonical name
    + Command GUID and ID.
    + Command binding(s) (with their scope)
        - Bindings are keyboard shortcuts.
        - Scopes are Global, Text Editor, etc.
    + Command location
        - Only FastFetch commands have a location. 
        - We'll show "N/A" for known commands.

- [ ] Add a vs command to enable display of command metadata.
    + The setting should be off by default, and only "persisted" for the current session.
    + So storing it in a static variable on QuickJumpPackage is enough.