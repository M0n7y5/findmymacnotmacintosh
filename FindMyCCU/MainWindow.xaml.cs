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
using AdonisUI.Controls;
using System.Globalization;
using System.Reactive.Linq;

namespace FindMyMACNotMacintosh
{

    public class AdonisReactiveWindow<TViewModel> : AdonisWindow, IViewFor<TViewModel> where TViewModel : class
    {
        /// <summary>
        /// The view model dependency property.
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                "ViewModel",
                typeof(TViewModel),
                typeof(ReactiveWindow<TViewModel>),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the binding root view model.
        /// </summary>
        public TViewModel BindingRoot => ViewModel;

        /// <inheritdoc/>
        public TViewModel ViewModel
        {
            get => (TViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        /// <inheritdoc/>
        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (TViewModel)value;
        }
    }


    public class MainWindowBase : AdonisReactiveWindow<MainWindowViewModel>
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

            this.cidr.ItemsSource = ViewModel.CIDR;
            this.ipbox.ItemsSource = ViewModel.Interfaces;

            this.cidr.SelectedIndex = 24;
            this.ipbox.SelectedIndex = 0;

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
                    vm => vm.SelectedIPIndex,
                    v => v.ipbox.SelectedIndex)
                    .DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.SelectedCIDR,
                    v => v.cidr.SelectedValue)
                    .DisposeWith(d);

                this.OneWayBind(ViewModel,
                    vm => vm.Devices.Count,
                    v => v.devCount.Text)
                    .DisposeWith(d);

                this.OneWayBind(ViewModel,
                    vm => vm.ElapsedTime,
                    v => v.elapsedTime.Text,
                    value => milisStringHandler(value))
                    .DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.FilterText,
                    v => v.FilterBox.Text)
                    .DisposeWith(d);

                this.WhenAnyValue(
                        x => x.ViewModel.ScanProgress,
                        x => x == 100)
                    .Subscribe(ProgBarFinishedHandler);

                this.WhenAnyObservable(
                    x => x.ViewModel.StartScan.IsExecuting)
                 .Select(x => !x)
                 .BindTo(this, x => x.ipbox.IsEnabled)
                 .DisposeWith(d);

                this.WhenAnyObservable(
                    x => x.ViewModel.StartScan.IsExecuting)
                 .Select(x => !x)
                 .BindTo(this, x => x.cidr.IsEnabled)
                 .DisposeWith(d);
            });
        }

        private string milisStringHandler(long value)
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(value * 10);

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:00}:{1:00}.{2:00}",
                ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
        }

        private void ProgBarFinishedHandler(bool x)
        {
            ProgressBarExtension.SetIsProgressAnimationEnabled(ProgBar, !x);
            ProgBar.Foreground = x ? GreenBrush : AdonDefault;

        }

        private void CopyIP_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }
}
