using System.Windows;

namespace ImageHeshTools.ToolWindow.View
{
    /// <summary>
    /// ToolWin.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ToolWin : Window
    {
        private readonly ViewModel.ToolWinVM _vm;

        public ToolWin()
        {
            InitializeComponent();
            _vm = new();
            this.DataContext = _vm;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }
    }
}
