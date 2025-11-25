using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokazuha.Application.Configuration
{
    public class FileStorageSettings
    {
        public const string SectionName = "FileStorage";

        public string SharedFolderPath { get; set; } = string.Empty;
    }
}
