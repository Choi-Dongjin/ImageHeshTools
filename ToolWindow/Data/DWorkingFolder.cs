using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ImageHeshTools.ToolWindow.Data
{
    public class DWorkingFolder : ICloneable
    {
        public string Name { get; set; } = string.Empty;

        public string FullPath { get; set; } = string.Empty;

        public bool Selected { get; set; } = false;

        public FileTypeENUM FileType { get; set; }

        public List<DWorkingFolder>? InnerData { get; set; }

        public JObject ToJson()
        {
            JArray jArray = new();
            if (InnerData != null && InnerData.Count > 0)
            {
                foreach (DWorkingFolder folder in InnerData)
                {
                    jArray.Add(folder.ToJson());
                }
            }

            return new JObject
            {
                { nameof(Name), Name },
                { nameof(FullPath), FullPath },
                { nameof(Selected), Selected },
                { nameof(FileType), FileType.ToString() },
                { nameof(InnerData), jArray }
            };
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public DWorkingFolder Clone()
        {
            List<DWorkingFolder> values = new();
            if (InnerData != null)
            {
                foreach (DWorkingFolder innerValue in InnerData)
                { values.Add(innerValue.Clone()); }
            }
            return new()
            {
                Name = Name,
                FullPath = FullPath,
                Selected = Selected,
                FileType = FileType,
                InnerData = values,
            };
        }
    }
}
