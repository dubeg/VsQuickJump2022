# README
Re-compiled [QuickJump](https://marketplace.visualstudio.com/items?itemName=ChristianSchubert.QuickJump), an extension made by Christian Schubert, to support Visual Studio 2022.

The extension has not been updated since 2019, as it seems that Christian Schubert disappeared from the online world.

## Commands
- `QuickJump.ShowMethodSearchForm`: search symbols in the active document.
- `QuickJump.ShowFileSearchForm`: search files in the active solution.
- `QuickJump.ShowAllSearchForm`: search files and symbols.

## Screenshots

### Search symbols
<img width="700" height="350" alt="Symbols" src="https://github.com/user-attachments/assets/fb145039-0b7e-4194-953f-6606a02e16e7" />


### Search files
<img width="700" height="329" alt="Files" src="https://github.com/user-attachments/assets/aa15de09-65aa-42e6-b98b-2f55b30a23ec" />


### Options
<img width="879" height="788" alt="Options" src="https://github.com/user-attachments/assets/ab03a342-d460-4965-9f67-95c0e01ab086" />



## Roadmap
- [ ] Implement fuzzy search when filtering files & symbols.
- [ ] Select file/symbol on click.
- [ ] Use Roslyn to parse documents.
  + It would probably improve symbol recognition.
