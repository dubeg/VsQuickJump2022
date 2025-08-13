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
    public List<CommandBarButtonInfo> GetCommands() {
        var commandInfos = new List<CommandBarButtonInfo>();
        var commandBars = (CommandBars)Dte.Application.CommandBars;
        foreach (CommandBar cmdBar in commandBars) {
            if (cmdBar is null) continue;
            foreach (CommandBarControl ctrl in cmdBar.Controls) {
                if (!(ctrl is CommandBarButton btn)) continue;
                var caption = btn.Caption;
                var pic = btn.Picture;
                var picType = pic.Type;
                // StdPicture type
                // 0: empty
                // 1: Bitmap
                // 2: metafile
                // 3: icon => Icon.FromHandle()
                // 4: enhanced metafile
                var bitmap = picType == 1 ? OlePictureConverter.ConvertStdPictureToBitmapSource(pic) : null;
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
