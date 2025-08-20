using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Imaging.Interop;

namespace QuickJump2022.Models;

public record class KnownCommandItem {
    public CommandID Command { get; set; }
    public string DisplayName { get; set; }
    public ImageMoniker Image { get; set; }
    public string Shortcut { get; set; } = "";

    public KnownCommandItem(
        CommandID command,
        string displayName,
        ImageMoniker image,
        string shortcut = ""
    ) {
        Command = command;
        DisplayName = displayName;
        Image = image;
        Shortcut = shortcut;
    }
};