using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickJump2022.Services;

/// <summary>
/// Retrieve a list of known commands.
/// </summary>
public class KnownCommandService {
    // --------------------
    // TODO:
    // Using Community.VisualStudio.Toolkit.KnownCommands && KnownMonikers,
    // I could hardcode a dictionary of kvp => (KnownCommand, (ImageMoniker, display name))
    // and return that.
    //
    // To build the list of known commands, I could try using AI.
    // --------------------
    // Eg. KnownCommands.Build_BatchBuild
    // --
    // public static class KnownCommands
    //   * public static CommandID Build_BatchBuild
}
