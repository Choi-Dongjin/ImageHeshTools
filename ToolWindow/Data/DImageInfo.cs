using ImageAssist.SupportType;

namespace ImageHeshTools.ToolWindow.Data
{
    internal class DImageInfo : ImageAssist.DataFormat.DImageInfo
    {
        public DImageInfo() : base() { }

        public DImageInfo(string filePath, LType? lType, bool includeRowImage = false) : base(filePath, lType, includeRowImage) { }
    }
}
