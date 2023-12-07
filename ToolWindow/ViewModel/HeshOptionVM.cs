
using Components;
using CSWPFUtile.CustomWindow;
using ImageAssist.DataFormat;
using ImageAssist.SupportType;
using ImageHeshTools.ToolWindow.View;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace ImageHeshTools.ToolWindow.ViewModel
{
    internal class HeshOptionVM : ViewModelBase, IDisposable, ICustomSubWindow
    {
        #region IDisposable

        // To detect redundant calls
        protected bool _disposed = false;

        ~HeshOptionVM() => Dispose(false);

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

        public Window? MainView { get; set; }

        public void CWClose(object? sender, CancelEventArgs? e)
        {
            if (sender is null || e is null)
                return;

            e.Cancel = true; // 창 닫기를 취소합니다.
            if (sender is Window window)
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
                MainView = new HeshOption()
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
            MainView ??= new HeshOption()
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

        #region Define

        /// <summary>
        /// 옵션 설정
        /// </summary>
        public ImageAssist.DataFormat.HashingAssistOption? HashingAssistOption { get; private set; }

        /// <summary>
        /// 지원 라이브러리
        /// </summary>
        public Array SupportLibrarys { get; } = Enum.GetValues(typeof(LType));

        /// <summary>
        /// 지원 해시 타입
        /// </summary>
        public Array SupportHashTypes { get; } = Enum.GetValues(typeof(ESupportHashType));

        /// <summary>
        /// 유사도 분석 방법
        /// </summary>
        public Array HashAnalysisTypes { get; } = Enum.GetValues(typeof(EHashAnalysisType));

        private LType _supportLibrary = LType.OpenCV;

        public LType SupportLibrary
        {
            get => _supportLibrary;
            set => SetProperty(ref _supportLibrary, value);
        }

        private ESupportHashType _supportHashType = ESupportHashType.DifferenceHash;

        public ESupportHashType SupportHashType
        {
            get => _supportHashType;
            set
            {
                SetProperty(ref _supportHashType, value);
                ChangeSupportHashType(value);
            }
        }

        private EHashAnalysisType _hashAnalysisType = EHashAnalysisType.HammingDistance;

        public EHashAnalysisType HashAnalysisType
        {
            get => _hashAnalysisType;
            set => SetProperty(ref _hashAnalysisType, value);
        }

        /// <summary>
        /// 해시 필터 사이즈
        /// </summary>
        private int _hashSizeWidth = 16;

        public int HashSizeWidth
        {
            get => _hashSizeWidth;
            set => SetProperty(ref _hashSizeWidth, value);
        }

        /// <summary>
        /// 해시 필터 사이즈
        /// </summary>
        private int _hashSizeHeight = 16;

        public int HashSizeHeight
        {
            get => _hashSizeHeight;
            set => SetProperty(ref _hashSizeHeight, value);
        }

        /// <summary>
        /// 허밍 거리 임계값
        /// </summary>
        private int _hammingDistanceThreshold = 1;

        public int HammingDistanceThreshold
        {
            get => _hammingDistanceThreshold;
            set => SetProperty(ref _hammingDistanceThreshold, value);
        }

        /// <summary>
        /// 허밍 거리 유사도
        /// </summary>
        private double _hammingSimilarityThreshold = 0.99D;

        public double HammingSimilarityThreshold
        {
            get => _hammingSimilarityThreshold;
            set => SetProperty(ref _hammingSimilarityThreshold, value);
        }

        /// <summary>
        /// Result
        /// </summary>
        public bool Result = false;

        /// <summary>
        /// Accecpt
        /// </summary>
        public ICommand AcceptCommand { get; }

        /// <summary>
        /// Cancel
        /// </summary>
        public ICommand CancelCommand { get; }

        #endregion End Define

        /// <summary>
        /// 초기화
        /// </summary>
        public HeshOptionVM()
        {
            AcceptCommand = new DelegateCommand(Accept);
            CancelCommand = new DelegateCommand(Cancel);
        }

        public HeshOptionVM(HashingAssistOption? hashingAssistOption)
        {
            AcceptCommand = new DelegateCommand(Accept);
            CancelCommand = new DelegateCommand(Cancel);
            if (hashingAssistOption != null)
            {
                SupportLibrary = hashingAssistOption.SupportLibrary;
                SupportHashType = hashingAssistOption.SupportHashType;
                HashAnalysisType = hashingAssistOption.HashAnalysisType;
                HashSizeWidth = hashingAssistOption.HashSize.Width;
                HashSizeHeight = hashingAssistOption.HashSize.Height;
                HammingDistanceThreshold = hashingAssistOption.HammingDistanceThreshold;
                HammingSimilarityThreshold = hashingAssistOption.HammingSimilarityThreshold;
            }
        }

        private void ChangeSupportHashType(ESupportHashType supportHash)
        {
            switch (supportHash)
            {
                case ESupportHashType.AverageHash:
                    HashSizeWidth = 16;
                    HashSizeHeight = 16;
                    break;
                case ESupportHashType.DifferenceHash:
                    HashSizeWidth = 16;
                    HashSizeHeight = 16;
                    break;
                case ESupportHashType.PerceptualHash:
                    HashSizeWidth = 32;
                    HashSizeHeight = 32;
                    break;
            }
        }

        private void Accept()
        {
            Result = true;
            HashingAssistOption = new()
            {
                HashSize = new System.Drawing.Size(HashSizeWidth, HashSizeHeight),
                HammingDistanceThreshold = HammingDistanceThreshold,
                HammingSimilarityThreshold = HammingSimilarityThreshold,
                SupportLibrary = SupportLibrary,
                SupportHashType = SupportHashType,
                HashAnalysisType = HashAnalysisType,
            };
            CloseWindow();
        }

        private void Cancel()
        {
            Result = false;
            HashingAssistOption = null;
            CloseWindow();
        }
    }
}
