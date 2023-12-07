using System.Windows.Controls;

namespace ImageHeshTools.ToolWindow.View
{
    /// <summary>
    /// ResultItemGrup.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ResultItemGrup : UserControl
    {
        private readonly ViewModel.ResultItemGrupVM _vm;

        public ResultItemGrup()
        {
            InitializeComponent();
            _vm = new();
            DataContext = _vm;
        }
    }
}
