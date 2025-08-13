using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickJump2022.Tools;

public static class StringExtensions {
    public static bool IsNotIn(this string str, string[] strings, Func<string, string, bool> match = null) {
        match ??= (a, b) => a == b;
        foreach (string x in strings) {
            if (match(str, x)) {
                return false;
            }
        }
        return true;
    }
}
