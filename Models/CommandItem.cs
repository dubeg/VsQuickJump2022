using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickJump2022.Models;

public class CommandItem {
    public string Name { get; set; }
    public string Guid { get; set; }
    public int ID { get; set; }
    public int Index { get; set; }
    public List<string> Shortcuts { get; set; } = new();
};