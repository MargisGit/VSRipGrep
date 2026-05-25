namespace VSRipGrep.Ui
{
    using VSRipGrep.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using VSRipGrep.Models;
    using System.ComponentModel;


    /// <summary>
    /// Interaction logic for ParametersToolWindowControl.
    /// </summary>
    public partial class ParametersToolWindowControl : UserControl, INotifyPropertyChanged
    {
        internal ParametersModel Parameters { get; } = new ParametersModel();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RipGrepToolWindowControl"/> class.
        /// </summary>
        public ParametersToolWindowControl()
        {
            this.InitializeComponent();
            this.DataContext = Parameters;

            var configuration = ToolWindowFactory.Package.GetDialogPage(typeof(Package.Configuration)) as Package.Configuration;
            if (configuration != null)
            {
                configuration.PropertyChanged += Configuration_PropertyChanged;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Configuration_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RipGrepExecutable")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsValidRipGrepExecutable"));
            }
        }

        public bool IsValidRipGrepExecutable
        {
            get
            {
                var configuration = ToolWindowFactory.Package.GetDialogPage(typeof(Package.Configuration)) as Package.Configuration;
                return configuration != null ? System.IO.File.Exists(configuration.RipGrepExecutable) : false;
            }
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && FindAll.IsEnabled)
            {
                FindAll_Click(sender, e);
            }
        }

        private void FindAll_Click(object sender, RoutedEventArgs e)
        {
            var resultsToolWindowControl = ToolWindowFactory.ShowResultsToolWindow()?.Content as ResultsToolWindowControl;
            if (resultsToolWindowControl != null)
            {
                var ripGrepTask = new RipGrepTask(Parameters.Clone());

                resultsToolWindowControl.ResultTask = ripGrepTask;
                ripGrepTask.Run();
            }
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var folderSelection = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (folderSelection.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Parameters.Path = folderSelection.SelectedPath;
                }
            }
        }

        private void RipGrepParameters_Loaded(object sender, RoutedEventArgs e)
        {
            var toolWindow = Window.GetWindow(this);
            if (toolWindow != null)
            {
                toolWindow.SizeToContent = SizeToContent.Height;
            }

            if (Parameters.Path == @"C:\")
            {
                var dte = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
                if (dte?.Solution?.FullName is string solution && !string.IsNullOrEmpty(solution))
                {
                    Parameters.Path = System.IO.Path.GetDirectoryName(solution);
                }
            }
        }

        private void Options_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var toolWindow = Window.GetWindow(this);
            if (toolWindow == null || !toolWindow.IsInitialized)
            {
                return;
            }

            if (e.PreviousSize.Height == 0)
            {
                return;
            }

            var height = toolWindow.Height + e.NewSize.Height - e.PreviousSize.Height;

            toolWindow.MinHeight = height;
            toolWindow.MaxHeight = height;
            toolWindow.Height = height;
        }

        private void RipGrepParameters_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var toolWindow = Window.GetWindow(this);
            if (toolWindow == null || !toolWindow.IsInitialized)
            {
                return;
            }

            toolWindow.MinHeight = toolWindow.Height;
            toolWindow.MaxHeight = toolWindow.Height;
        }
    }
}