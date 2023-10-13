using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Shared.TaskResultTypes
{
    public class EditFile
    {
        public string FileName { get; set; }
        public string Content { get; set; }
        public bool CanEdit { get; set; }
    }
}
