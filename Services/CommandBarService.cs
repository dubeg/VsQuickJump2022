using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using EnvDTE;
using Microsoft.VisualStudio.CommandBars;
using QuickJump2022.Tools;

namespace QuickJump2022.Services;

public class CommandBarService(DTE Dte) {
    private List<CommandBarButtonInfo> _cachedCommands = new();
    
    public void PreloadCommands() {
        _cachedCommands = GetCommands();
    }
    
    public List<CommandBarButtonInfo> GetCachedCommands() {
        if (_cachedCommands.Count == 0) PreloadCommands();
        return _cachedCommands;
    }
    
    public void Execute(CommandBarButtonInfo commandBarButton) {
        try {
            var commandBars = (CommandBars)Dte.Application.CommandBars;
            var cmdBar = commandBars[commandBarButton.CommandBarName];
            if (cmdBar is null) return;
            
            foreach (CommandBarControl ctrl in cmdBar.Controls) {
                if (ctrl is CommandBarButton btn && btn.Caption == commandBarButton.Caption) {
                    btn.Execute();
                    Dte.StatusBar.Clear();
                    return;
                }
            }
            Dte.StatusBar.Text = $"The command bar button '{commandBarButton.Caption}' is not available";
        }
        catch (Exception ex) {
            Dte.StatusBar.Text = $"Error executing command bar button: {ex.Message}";
        }
    }
    
    private List<CommandBarButtonInfo> GetCommands() {
        var commandInfos = new List<CommandBarButtonInfo>();
        var commandBars = (CommandBars)Dte.Application.CommandBars;
        foreach (CommandBar cmdBar in commandBars) {
            if (cmdBar is null) continue;
            foreach (CommandBarControl ctrl in cmdBar.Controls) {
                if (!(ctrl is CommandBarButton btn)) continue;
                var caption = btn.Caption;
                var pic = btn.Picture;
                BitmapSource bitmap = null;
                if (pic is not null) {
                    var picType = pic.Type;
                    // StdPicture type
                    // 0: empty
                    // 1: Bitmap
                    // 2: metafile
                    // 3: icon => Icon.FromHandle()
                    // 4: enhanced metafile
                    bitmap = picType == 1 ? OlePictureConverter.ConvertStdPictureToBitmapSource(pic) : null;
                }
                commandInfos.Add(new CommandBarButtonInfo() {
                    Caption = caption,
                    CommandBarID = cmdBar.Id,
                    CommandBarName = cmdBar.Name,
                    BitmapSource = bitmap
                });
            }
        }
        return commandInfos;
    }
}

public class CommandBarButtonInfo {
    public string CommandBarName { get; set; }
    public int CommandBarID { get; set; }
    // --
    public string Caption { get; set; }
    public string CanonicalName { get; set; }
    public Guid CmdSet { get; set; }
    public int ID { get; set; }
    // --
    public BitmapSource BitmapSource { get; set; }
};
