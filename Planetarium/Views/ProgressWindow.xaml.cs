using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Planetarium.Views
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

        public ProgressWindow()
        {
            InitializeComponent();
        }
        
        public string Text 
        {
            set
            {
                Label.Content = value;
            }
            get
            {
                return Label.Content.ToString();
            }
        }

        private Progress<double> _Progress;
        public Progress<double> Progress
        {
            get { return _Progress; }
            set
            {                
                _Progress = value;
                ProgressBar.IsIndeterminate = _Progress == null;
                if (_Progress != null)
                {
                    _Progress.ProgressChanged += _Progress_ProgressChanged;
                }
            }
        }

        private CancellationTokenSource _CancellationTokenSource;
        public CancellationTokenSource CancellationTokenSource
        {
            get
            {
                return _CancellationTokenSource;
            }
            set
            {
                _CancellationTokenSource = value;
                Task.Run(WaitForCancel);
            }
        }

        private void WaitForCancel()
        {
            _CancellationTokenSource.Token.WaitHandle.WaitOne();
            Dispatcher.Invoke(Close);
        }

        private void _Progress_ProgressChanged(object sender, double progress)
        {
            ProgressBar.Value = progress;            
        }

        protected override void OnClosed(EventArgs e)
        {           
            if (_Progress != null)
            {
                _Progress.ProgressChanged -= _Progress_ProgressChanged;
            }
            base.OnClosed(e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancellationTokenSource?.Cancel();          
        }
    }
}
