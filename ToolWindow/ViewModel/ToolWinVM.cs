using Components;
using CSWPFUtile.CustomWindow;
using ImageAssist.DataFormat;
using ImageHeshTools.ToolWindow.Data;
using ImageHeshTools.ToolWindow.Model;
using ImageHeshTools.ToolWindow.Utiles;
using ImageHeshTools.ToolWindow.View;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ImageHeshTools.ToolWindow.ViewModel
{
    internal class ToolWinVM : ViewModelBase, IDisposable, ICustomSubWindow
    {
        #region IDisposable

        // To detect redundant calls
        protected bool _disposed = false;

        ~ToolWinVM() => Dispose(false);

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {


            }
            _disposed = true;
        }

        #endregion IDisposable

        #region ICustomSubwindow

        public System.Windows.Window? MainView { get; set; }

        public void CWClose(object? sender, CancelEventArgs? e)
        {
            if (sender is null || e is null)
                return;

            e.Cancel = true; // 창 닫기를 취소합니다.
            if (sender is System.Windows.Window window)
            {
                window.Hide();
                IsWindowVisible = false;
            }
        }

        private bool isWindowVisible;

        public bool IsWindowVisible
        {
            get { return isWindowVisible; }
            set
            {
                isWindowVisible = value;
                OnPropertyChanged(nameof(IsWindowVisible));
                UpdateWindowStateAndTaskbar();
            }
        }

        public WindowState WindowState { get; set; }
        public bool ShowInTaskbar { get; set; }

        public void UpdateWindowStateAndTaskbar()
        {
            WindowState = IsWindowVisible ? WindowState.Normal : WindowState.Minimized;
            ShowInTaskbar = IsWindowVisible;
            OnPropertyChanged(nameof(WindowState));
            OnPropertyChanged(nameof(ShowInTaskbar));
        }

        public void ShowWindow()
        {
            if (MainView == null)
            {
                MainView = new ToolWin()
                {
                    DataContext = this
                };
                MainView.Closing += CWClose;
            }
            MainView.Show();
            MainView.Activate();
            IsWindowVisible = true;
        }

        public void CloseWindow()
        {
            if (MainView == null)
                return;
            WindowDispose();
        }

        public void ShowDialog()
        {
            MainView ??= new ToolWin()
            {
                DataContext = this,
            };
            MainView.ShowDialog();
        }

        public void WindowDispose()
        {
            if (MainView is not null)
            {
                MainView.Closing -= CWClose;
                MainView.Close();
                MainView = null;
            }
        }

        public void ViewModelDispose()
        {
            Dispose();
        }

        #endregion ICustomSubwindow

        private ObservableCollection<DWorkingFolder> _workingFolders;

        public ObservableCollection<DWorkingFolder> WorkingFolders
        {
            get => _workingFolders;
            set => SetProperty(ref _workingFolders, value);
        }

        private bool _loadingWorkingFolders = false;

        public bool LoadingWorkingFolders
        {
            get => _loadingWorkingFolders;
            set => SetProperty(ref _loadingWorkingFolders, value);
        }

        private ObservableCollection<DResultGrup> _resultGrups;

        public ObservableCollection<DResultGrup> ResultGrups
        {
            get => _resultGrups;
            set => SetProperty(ref _resultGrups, value);
        }

        private ControlItem? _selectedItem;

        public ControlItem? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        private string _gpNumber = string.Empty;

        public string GPNumber { get => _gpNumber; set => SetProperty(ref _gpNumber, value); }

        private readonly string _workingFolderOptionFile;

        private HashingAssistOption? _hashingAssistOption;

        private Dictionary<Guid, ImageAssist.HashingAssist> _hashingAssists = new();

        public ToolWinVM()
        {
            _workingFolders = new();
            _resultGrups = new();
            _workingFolderOptionFile = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "..\\WorkingFolderOption.json");
            _ = LoadInfo();
        }

        public async void AddWorkingFolder()
        {
            if (LoadingWorkingFolders != false) { return; }

            string? file = OpenFileDialog.COpenFileDialog<string>(null, true, false);
            if (file == null) { return; }

            DWorkingFolder workingFolder = new()
            {
                FullPath = file,
                Selected = true,
                FileType = FileTypeENUM.Folder,
                Name = Path.GetFileName(file) == string.Empty ? file : Path.GetFileName(file),
            };

            LoadingWorkingFolders = true;
            await Task.Run(() => { SearchWorkingFolder(file, ref workingFolder); });
            LoadingWorkingFolders = false;
            WorkingFolders?.Add(workingFolder);
        }

        public async void WorkingFolderSave()
        {
            await SaveInfo();
        }

        public void WorkingFolderReset()
        {
            WorkingFolders = new();
        }

        public void SearchMethod()
        {
            HeshOptionVM vm = new(_hashingAssistOption);
            vm.ShowDialog();
            if (vm.Result != true) { return; }
            if (vm.HashingAssistOption != null) { _hashingAssistOption = vm.HashingAssistOption; }
            vm.Dispose();
        }

        public async void Searching()
        {
            if (LoadingWorkingFolders != false) { return; }
            if (_hashingAssistOption == null) { return; }

            LoadingWorkingFolders = true;
            ResultGrups = new();
            List<string> directorys = await GetWorkingDirectoryAsync();
            if (directorys.Count < 0) { return; }

            ImageAssist.HashingAssist hashingAssist = new(directorys, _hashingAssistOption);
            await hashingAssist.RunImageHash();
            Dictionary<string, List<string>>? analysisItems = await hashingAssist.RunAnalysis();
            if (analysisItems == null) { return; }
            GPNumber = string.Format("G number : {0}", analysisItems.Count);
            foreach (KeyValuePair<string, List<string>> analysisItem in analysisItems)
            {
                Guid guid = Guid.NewGuid();
                DResultGrup resultGrup = new()
                {
                    ID = guid,
                    ItemName = analysisItem.Key,
                    GrupType = ResultGrupType.Grup,
                    InnerItem = new()
                };

                foreach (string filePath in analysisItem.Value)
                {
                    DResultGrup item = new()
                    {
                        ID = Guid.NewGuid(),
                        ItemName = filePath,
                        GrupType = ResultGrupType.Item,
                    };
                    resultGrup.InnerItem.Add(item);
                }

                _hashingAssists.Add(guid, hashingAssist);
                ResultGrups.Add(resultGrup);
            }
            LoadingWorkingFolders = false;
        }

        public void ResultGrupsSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is not DResultGrup resultGrup)
            {
                return;
            }
            switch (resultGrup.GrupType)
            {
                case ResultGrupType.None:
                    break;
                case ResultGrupType.Item:
                    break;
                case ResultGrupType.Grup:
                    ResultItemSetGrup(resultGrup);
                    break;
            }
        }

        public async void ResultItemSetGrup(DResultGrup resultGrup)
        {
            SelectedItem = new(nameof(ResultItemGrup), typeof(ResultItemGrup));
            if (SelectedItem.Content is ResultItemGrup grup)
            {
                if (grup.DataContext is ResultItemGrupVM vm)
                {
                    await vm.Init(resultGrup);
                }
            }
        }


        private async Task<List<string>> GetWorkingDirectoryAsync()
        {
            List<DWorkingFolder> workingFolders = WorkingFolders.ToList();
            List<string> directorys = new();
            await Task.Run(() =>
            {
                foreach (DWorkingFolder d in workingFolders)
                {
                    GetWorkingDirectory(ref directorys, d);
                }
            });
            return directorys;
        }

        private void GetWorkingDirectory(ref List<string> directorys, DWorkingFolder workingFolder)
        {
            if (workingFolder.Selected == false) { return; }
            directorys.Add(workingFolder.FullPath);

            if (workingFolder.InnerData == null) { return; }

            foreach (DWorkingFolder innerData in workingFolder.InnerData)
            {
                GetWorkingDirectory(ref directorys, innerData);
            }
        }

        public void SearchWorkingFolder(string file, ref DWorkingFolder innerWorkingFolder)
        {
            if (!FileIOAssist.Extension.TrySubDirectories(file, out IReadOnlyCollection<string>? subFolder) || subFolder == null)
            {
                return;
            }
            foreach (string subFile in subFolder)
            {
                DWorkingFolder workingFolder = new()
                {
                    FullPath = subFile,
                    Selected = true,
                    FileType = FileTypeENUM.Folder,
                    Name = Path.GetFileName(subFile) == string.Empty ? subFile : Path.GetFileName(subFile),
                };
                innerWorkingFolder.InnerData ??= new();
                innerWorkingFolder.InnerData.Add(workingFolder);
                SearchWorkingFolder(subFile, ref workingFolder);
            }
        }



        private async Task LoadInfo()
        {
            if (!File.Exists(_workingFolderOptionFile)) { return; }

            if (LoadingWorkingFolders != false) { return; }
            LoadingWorkingFolders = true;
            await Task.Run(() =>
            {
                if (!JsonAssist.JsonAssist.TryGetJarrayConvert(_workingFolderOptionFile, out ICollection<DWorkingFolder>? workingFolder) || workingFolder == null)
                {
                    return;
                }

                ObservableCollection<DWorkingFolder> v = new ObservableCollection<DWorkingFolder>();
                foreach (var value in workingFolder)
                {
                    v.Add(value);
                }
                WorkingFolders = v;
            });
            LoadingWorkingFolders = false;
        }

        private async Task SaveInfo()
        {
            if (LoadingWorkingFolders != false) { return; }
            LoadingWorkingFolders = true;
            List<DWorkingFolder> workingFolders = new();
            if (WorkingFolders != null)
            {
                foreach (DWorkingFolder value in WorkingFolders)
                {
                    workingFolders.Add(value.Clone());
                }
            }
            await Task.Run(() =>
            {
                JArray saveInfo = new();
                if (WorkingFolders == null) { return; }
                foreach (var info in workingFolders)
                {
                    saveInfo.Add(info.ToJson());
                }
                JsonAssist.JsonAssist.PushJsonObject(_workingFolderOptionFile, saveInfo.ToString());
            });
            LoadingWorkingFolders = false;
        }
    }
}



