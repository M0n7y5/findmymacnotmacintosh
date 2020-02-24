using CsvHelper;
using DynamicData;
using DynamicData.Binding;
using FindMyMACNotMacintosh.Models;
using FindMyMACNotMacintosh.Utils;
using Nito.AsyncEx;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetTools;

namespace FindMyMACNotMacintosh
{
    public class MainWindowViewModel : ReactiveObject
    {
        [Reactive]
        public ObservableCollection<NetworkDevice> Devices { get; private set; } = new ObservableCollection<NetworkDevice>();

        [Reactive]
        private List<NetworkDevice> ScannedDevices { get; set; } = new List<NetworkDevice>(250);

        public List<string> Interfaces { get; }

        public List<int> CIDR { get; } = Enumerable.Range(0, 32).ToList();

        [Reactive]
        public int ScanProgress { get; private set; }

        [Reactive]
        public string Subnet { get; set; }

        [Reactive]
        public string FilterText { get; set; }

        [Reactive]
        public long ElapsedTime { get; private set; }

        [Reactive]
        public int SelectedIPIndex { get; set; }

        [Reactive]
        public int SelectedCIDR { get; set; } = 24;

        public ReactiveCommand<Unit, NetworkDevice> StartScan { get; }

        public ReactiveCommand<Unit, Unit> AbortScan { get; }

        // ----------------------------------------------------------------------
        //private CancellationTokenSource _cts;

        //private string _ip;

        //private uint _net;

        //private string[] _filter;

        private int _numberIPToScan;

        private int _finished;

        private List<NetworkInterface> _interfaces;

        private static readonly AsyncLazy<List<MACRecord>> _macRecords = new AsyncLazy<List<MACRecord>>(LoadMACRecords);

        private IDisposable _stopwatch;

        [Reactive]
        private bool canScan { get; set; }

        //Subject<int> _currentIndex;

        public MainWindowViewModel()
        {
            //_cts = new CancellationTokenSource();
            _macRecords.Start();
            Interfaces = new List<string>(25);
            _interfaces = NetworkInterface.GetAllNetworkInterfaces().ToList();

            _interfaces.ForEach(x =>
            {
                var ip = x.GetIPProperties()
                    .UnicastAddresses
                    .Where(u => u.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(i => i.Address).First().ToString();

                Interfaces.Add($"{ip} | {x.Name}");
            });

            this.WhenAnyValue(
                x => x.Subnet)
                .Subscribe(UpdateIPAndNet);

            // Command Init
            StartScan = ReactiveCommand.CreateFromObservable(
                () => StartScanHandler()
                    .ObserveOn(RxApp.TaskpoolScheduler)
                    .TakeUntil(AbortScan)
                    .Finally(() =>
                    {
                        _stopwatch.Dispose();
                    }),
                this.WhenAnyValue(x => x.SelectedIPIndex, (val) => val != -1));

            AbortScan = ReactiveCommand.Create(() => { }, StartScan.IsExecuting);

            StartScan.Subscribe(async x => await UpdateProgressAndDevices(x).ConfigureAwait(true));
            StartScan.ThrownExceptions.Subscribe(x => Console.WriteLine(x.Message));

            this.WhenAnyValue(x => x.FilterText).Subscribe(x => UpdateDevices());

        }

        private static async Task<List<MACRecord>> LoadMACRecords()
        {
            return await Task.Run(() =>
            {
                using (var text = new StringReader(Resources.oui))
                using (var csv = new CsvReader(text, CultureInfo.InvariantCulture))
                {
                    csv.Parser.Configuration.Delimiter = ",";
                    return csv.GetRecords<MACRecord>().ToList();
                }
            }).ConfigureAwait(true);
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

            //if (IPAddress.TryParse(ipSub[0], out IPAddress ip))
            //{
            //    _ip = ipSub[0];
            //}

            //if (uint.TryParse(ipSub[1], out uint val))
            //{
            //    _net = val;
            //}

            canScan = true;
        }


        void UpdateDevices()
        {
            var tmp = ScannedDevices
                .Where(x =>
                    string.IsNullOrEmpty(FilterText) ||
                    x.IP.Contains(FilterText, StringComparison.InvariantCultureIgnoreCase) ||
                    x.MAC.Contains(FilterText, StringComparison.InvariantCultureIgnoreCase) ||
                    (x.Vendor?.Contains(FilterText, StringComparison.InvariantCultureIgnoreCase) ?? false)
                    //FilterText.Contains("")
                    );

            Devices = new ObservableCollection<NetworkDevice>(tmp);
        }

        private void UpdateProggress()
        {
            ScanProgress = (int)Math.Round((((float)++_finished / _numberIPToScan) * 100));
        }

        async Task UpdateProgressAndDevices(NetworkDevice device)
        {
            if (device == null)
            {
                UpdateProggress();
                return;
            }

            var rec = await _macRecords;
            device.Vendor = rec
                .FirstOrDefault(
                    x => x.Assigment.Equals(
                        device.MAC
                            .Substring(0, 8)
                            .Replace(":", "", StringComparison.InvariantCultureIgnoreCase)
                        , StringComparison.InvariantCultureIgnoreCase))
                ?.OrganizationName;

            ScannedDevices.Add(device);
            UpdateDevices();
            UpdateProggress();
        }

        IObservable<NetworkDevice> StartScanHandler()
        {
            ScannedDevices.Clear();
            ScanProgress = 0;
            Devices.Clear();

            _stopwatch = Observable
                .Interval(TimeSpan.FromMilliseconds(1.0d), RxApp.MainThreadScheduler)
                .ObserveOnDispatcher()
                .SubscribeOnDispatcher()
                .Subscribe(x => ElapsedTime = x);

            return Observable.Create<NetworkDevice>((obs, cts) =>
            {
                return Task.Run(async () =>
                {
                    var listIps = new IPAddressRange(
                        _interfaces[SelectedIPIndex]
                            .GetIPProperties()
                            .UnicastAddresses
                            .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
                            .Select(x => x.Address)
                            .First(), SelectedCIDR)
                        .AsEnumerable();

                    _finished = 0;

                    _numberIPToScan = listIps.Count();

                    listIps.AsParallel()
                        .WithCancellation(cts)
                        .WithMergeOptions(ParallelMergeOptions.FullyBuffered)
                        .WithDegreeOfParallelism(255)
                        .ForAll(x =>
                        {
                            if (cts.IsCancellationRequested)
                                return;

                            string mac = "";

                            if (!NetworkInterop.TryGetMACAdrByIp(x, out mac))
                            {
                                if (obs is null)
                                    return;

                                obs.OnNext(null);
                                return;
                            }

                            if (obs is null)
                                return;

                            obs.OnNext(new NetworkDevice()
                            {
                                IP = x.ToString(),
                                MAC = mac.ToUpperInvariant()
                            });
                        });

                    listIps = null;
                }, cts);
            });
        }
    }
}
