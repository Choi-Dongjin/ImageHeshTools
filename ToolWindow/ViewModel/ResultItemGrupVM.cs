using Components;
using ImageHeshTools.ToolWindow.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using DImageInfo = ImageHeshTools.ToolWindow.Data.DImageInfo;

namespace ImageHeshTools.ToolWindow.ViewModel
{
    internal class ResultItemGrupVM : ViewModelBase
    {
        private List<DImageInfo>? _images;

        public List<DImageInfo>? Images { get => _images; set => SetProperty(ref _images, value); }

        private string _name = "KongGuRE";

        public string Name { get => _name; set => SetProperty(ref _name, value); }


        public async Task<bool> Init(DResultGrup resultGrup)
        {
            if (resultGrup.InnerItem == null) { return false; }
            List<DImageInfo> loadingImage()
            {
                List<DImageInfo> values = new();
                foreach (DResultGrup imageInfos in resultGrup.InnerItem)
                {
                    values.Add(new(imageInfos.ItemName, ImageAssist.SupportType.LType.OpenCV, true));
                };
                return values;
            }
            Images = await Task.Run(loadingImage);
            return true;
        }

    }
}
