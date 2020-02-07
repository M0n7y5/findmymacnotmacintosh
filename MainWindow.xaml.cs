using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FindMyMACNotMacintosh
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new MainWindowViewModel();

            this.DevicesList.ItemsSource = ViewModel.Devices;

            this.WhenActivated(d => 
            {
                this.BindCommand(ViewModel,
                    vm => vm.StartScan,
                    v => v.btnStart)
                    .DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.AbortScan,
                    v => v.btnAbort)
                    .DisposeWith(d);

                this.OneWayBind(ViewModel,
                    vm => vm.ScanProgress,
                    v => v.ProgBar.Value)
                    .DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.Subnet,
                    v => v.SubnetBox.Text)
                    .DisposeWith(d);

                this.OneWayBind(ViewModel,
                    vm => vm.Devices.Count,
                    v => v.devCount.Content)
                    .DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.FilterText,
                    v => v.FilterBox.Text)
                    .DisposeWith(d);

                //this.OneWayBind(ViewModel,
                //    vm => vm.StartScan.IsExecuting,
                //    v => v.ProgBar.IsIndeterminate)
                //.DisposeWith(d);
                
            
            });
        }
    }
}
