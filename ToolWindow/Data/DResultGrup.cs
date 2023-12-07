using System;
using System.Collections.ObjectModel;

namespace ImageHeshTools.ToolWindow.Data
{
    internal class DResultGrup
    {
        public string ItemName { get; set; } = string.Empty;

        public Guid ID { get; set; } = Guid.Empty;

        public ResultGrupType GrupType { get; set; }

        public ObservableCollection<DResultGrup>? InnerItem { get; set; }
    }
}
