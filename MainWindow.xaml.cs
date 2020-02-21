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
using AdonisUI.Extensions;
using AdonisUI;

namespace FindMyMACNotMacintosh
{

    public class MainWindowBase : ReactiveWindow<MainWindowViewModel>
    { }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MainWindowBase
    {
        public Brush GreenBrush { get; private set; }
        public Brush AdonDefault { get; private set; }

        public MainWindow()
        {
            InitializeComponent();


            AdonDefault = (Brush)this.TryFindResource(AdonisUI.Brushes.AccentBrush);
            GreenBrush = (Brush)this.TryFindResource(AdonisUI.Brushes.SuccessBrush);

            ViewModel = new MainWindowViewModel();

            //this.DevicesList.ItemsSource = ViewModel.Devices.Items;

            this.WhenActivated(d => 
            {
                this.OneWayBind(ViewModel,
                    vm => vm.Devices,
                    v => v.DevicesList.ItemsSource)
                    .DisposeWith(d);

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
                    v => v.devCount.Text)
                    .DisposeWith(d);

                this.OneWayBind(ViewModel,
                    vm => vm.ElapsedTime,
                    v => v.elapsedTime.Text, 
                    value => 
                        (TimeSpan.FromMilliseconds(value).TotalSeconds * 10)
                        .ToString("00:00.00"))
                    .DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.FilterText,
                    v => v.FilterBox.Text)
                    .DisposeWith(d);



                this.WhenAnyValue(
                        x => x.ViewModel.ScanProgress, 
                        x => x == 100)
                    .Subscribe(ProgBarFinishedHandler);


                //this.OneWayBind(ViewModel,
                //    vm => vm.StartScan.IsExecuting,
                //    v => v.ProgBar.IsIndeterminate)
                //.DisposeWith(d);


            });
        }



        private void ProgBarFinishedHandler(bool x) {
            ProgressBarExtension.SetIsProgressAnimationEnabled(ProgBar, !x);
            ProgBar.Foreground = x ? GreenBrush : AdonDefault;

        }

        private void CopyIP_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }
}
