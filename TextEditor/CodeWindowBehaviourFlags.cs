using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickJump2022.TextEditor;

public enum CodeWindowBehaviourFlags : uint {
    CWB_DISABLEDROPDOWNBAR = 1U,
    CWB_DISABLESPLITTER = 2U,
    CWB_DISABLEDIFF = 4U,
    CWB_DISABLESCROLLBARS = 8U,
    CWB_DISABLESTATUSBAR = 16U,
    CWB_DISABLEALL = 31U,
    CWB_DEFAULT = 0U,
}
