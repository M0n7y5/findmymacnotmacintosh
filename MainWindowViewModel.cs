using DynamicData;
using FindMyMACNotMacintosh.Models;
using FindMyMACNotMacintosh.Utils;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FindMyMACNotMacintosh
{
    public class MainWindowViewModel : ReactiveObject
    {
        [Reactive]
        public ObservableCollection<NetworkDevice> Devices { get; private set; }

        [Reactive]
        private ObservableCollection<NetworkDevice> ScannedDevices { get; set; }

        [Reactive]
        public int ScanProgress { get; private set; }

        [Reactive]
        public string Subnet { get; set; }

        public ReactiveCommand<Unit, ScanResultWithProgress> StartScan { get; }

        public ReactiveCommand<Unit, Unit> AbortScan { get; }

        [Reactive]
        public string FilterText { get; set; }

        // ----------------------------------------------------------------------
        private CancellationTokenSource _cts;

        private string _ip;

        private uint _net;

        private string[] _filter;

        [Reactive]
        private bool canScan { get; set; }

        //Subject<int> _currentIndex;

        public MainWindowViewModel()
        {
            _cts = new CancellationTokenSource();
            Devices = new ObservableCollection<NetworkDevice>();
            ScannedDevices = new ObservableCollection<NetworkDevice>();

            this.WhenAnyValue(
                x => x.Subnet)
                .Subscribe(UpdateIPAndNet);

            // Command Init
            StartScan = ReactiveCommand.CreateFromObservable<ScanResultWithProgress>(
                () => StartScanHandler()
                    .ObserveOn(RxApp.TaskpoolScheduler)
                    .TakeUntil(AbortScan), 
                this.WhenAnyValue(x => x.canScan));
            AbortScan = ReactiveCommand.Create(() => { }, StartScan.IsExecuting);

            StartScan.Subscribe(UpdateProgressAndDevices);
            StartScan.ThrownExceptions.Subscribe(x => Console.WriteLine(x.Message));

            this.WhenAnyValue(x => x.FilterText).Subscribe(x => UpdateDevices());
        }

        private void UpdateIPAndNet(string subnet)
        {
            if (subnet is null)
            {
                canScan = false;
                return;
            }

            var ipSub = Subnet.Split("/");

            if (ipSub.Length != 2)
            {
                canScan = false;
                return;
            }

            if (IPAddress.TryParse(ipSub[0], out IPAddress ip))
            {
                _ip = ipSub[0];
            }

            if (uint.TryParse(ipSub[1], out uint val))
            {
                _net = val;
            }

            canScan = true;
        }


        void UpdateDevices()
        {

            Devices.Clear();
            Devices.AddRange(
                ScannedDevices
                .Where(x => 
                    // If FilterText is empty add everything to list
                    string.IsNullOrWhiteSpace(FilterText) || 
                    // If FilterText is NOT empty add only filtered items
                    (
                    //x.IP.Contains(FilterText) && 
                    x.MAC.Contains(FilterText)))
                );
        }

        void UpdateProgressAndDevices(ScanResultWithProgress scan)
        {
            if (scan.Device != null)
                if (!string.IsNullOrEmpty(scan.Device.MAC))
                    ScannedDevices.Add(scan.Device);

            ScanProgress = scan.Progress;
            UpdateDevices();
        }

        IObservable<ScanResultWithProgress> StartScanHandler()
        {
            ScannedDevices.Clear();
            Devices.Clear();

            return Observable.Create<ScanResultWithProgress>((obs, cts) =>
            {
                return Task.Run(async () =>
                {

                    var listIps = IPCalc.GetListIpInNetwork(IPAddress.Parse(_ip), _net);
                    int finished = 0;

                    listIps.AsParallel()
                        .WithCancellation(cts)
                        .WithDegreeOfParallelism(20)                   
                        .ForAll(x =>
                       {
                           if (cts.IsCancellationRequested)
                               return;

                           string mac = "";
                           //var prog = (((float)item.index / listIps.Count) * 100);
                           try
                           {
                               mac = NetworkInterop.GetMACAdrByIp(x);

                           }
                           catch (Exception ex)
                           {
                               Interlocked.Increment(ref finished);

                               if (obs is null)
                                   return;

                               obs.OnNext(new ScanResultWithProgress()
                               {
                                   Device = null,
                                   Progress = Convert.ToInt32((((float)finished / listIps.Count) * 100))
                               });

                               return;
                           }

                           Interlocked.Increment(ref finished);
                           if (obs is null)
                               return;
                           obs.OnNext(new ScanResultWithProgress()
                           {
                               Device = new NetworkDevice()
                               {
                                   IP = x.ToString(),
                                   MAC = mac
                               },
                               Progress = Convert.ToInt32((((float)finished / listIps.Count) * 100))
                           });

                       });


                    //foreach (var item in listIps.Select((ip, index) => new { ip, index }))
                    //{
                    //    string mac = "";
                    //    var prog = (((float)item.index / listIps.Count) * 100);
                    //    try
                    //    {
                    //        mac = NetworkInterop.GetMACAdrByIp(item.ip);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        obs.OnNext(new ScanResultWithProgress()
                    //        {
                    //            Device = null,
                    //            Progress = Convert.ToInt32(prog)
                    //        });

                    //        continue;
                    //    }

                    //    obs.OnNext(new ScanResultWithProgress()
                    //    {
                    //        Device = new NetworkDevice()
                    //        {
                    //            IP = item.ip.ToString(),
                    //            MAC = mac
                    //        },
                    //        Progress = Convert.ToInt32(prog)
                    //    });

                    //    //await Task.Delay(500);
                    //}


                });
            });
        }
    }
}
