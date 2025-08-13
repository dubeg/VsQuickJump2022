using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickJump2022.Models;
public class FileItem {
    public string FileName { get; set; }
    public string FileExtension => System.IO.Path.GetExtension(FileName);
    public string FolderPath { get; set; }
    public string FullPath { get; set; }
    public string ProjectName { get; set; }
    public string ProjectPath { get; set; }
    public string ProjectRelativePath { get; set; }
    public string ProjectRelativeFolderPath { get; set; }
}
