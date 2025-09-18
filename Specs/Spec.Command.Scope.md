# Spec: Commands

Currently, pressing TAB will switch to the next QuickJump type (file, symbol, commands (canonical names), commands (known cmds), commands (fast fetch)).

I don't find it useful to tab between types, but it might be useful to TAB through command types. 
For eg, it might be handy to sometimes search by canonical name, sometimes by friendly name,



- [x] Commands: TAB to change between canonical names, friendly names (fast fetch), or custom names (known commands).
    + For only for the command `ShowCommandSearchForm` in the VsCommandTable.
    + Create a new VsCommandTable command for searching by canonical name, which is already implemented by ShowCommandSearchForm.

- [x] When selecting a command, save the command name/text so that the next run will match exactly that command.
    + For eg. for a search string such as "build", the results returned may include "build: solution", "build: cancel".
        - If the user picks "build: cancel" by navigating the results using up/down arrows, the search string won't match exactly that result in the next run.
        - But that's what we want: being able to run exactly the last command.