using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using JetBrains.Annotations;
using PhaseSonar.Analyzers;
using SpectroscopyVisualizer.Configs;
using SpectroscopyVisualizer.Consumers;

namespace SpectroscopyVisualizer {
    /// <summary>
    ///     Interaction logic for ProcessStatistics.xaml
    /// </summary>
    public partial class StatsWindow : Window, INotifyPropertyChanged {
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MOVE = 0xF010;

        private readonly string[] _tips = {
            "When no peaks are found, try check AUTO-ADJUST PEAK AMP.",
            "Reducing the number of the POINTS DISPLAYED would smooth the graph update.",
            "Set QUEUE CAPACITY with a small number to view instant spectra.",
            "Setting QUEUE CAPACITY with a large number would reduce the waiting interval when the queue is full.",
            "Setting QUEUE CAPACITY with an over-large number would probably result in running out of memory.",
            "Increasing the THREADS would increase the number of process workers, thus accelerating the program.",
            "Increasing the THREADS over the max number of threads of the CPU would not accelerate the program.",
            "Click VIEW PULSES to check the temporal sequence from a single sampling record.",
            "Click HELP for more information about the program.",
            "Click the RIGHT BUTTON on the graph could undo a recent operation.",
            "Click the MIDDLE BUTTON on the graph could adjust the vertical scaling.",
            "To store the current parameter configurations, go CONFIGS->SAVE CONFIG.",
            "If most parts of the data are out of the current graphic scope, click MIDDLE BUTTON on the graph.",
            "Try shorten the linear PHASE RANGE when the count of EXCESSIVE PHASE LEAPS is high.",
            "Sampling would be unavailable if another program, or another instance of this program, is using the sampling device.",
            "Accumulating over too much data could result in the overflow of primitive types like DOUBLE and INT, and then the disappearence of the WHITE LINE.",
            "ADC might FAIL to sample data because of excessive temperature, or other internal exceptions",
            "Go TOOLS->MISCELLANEOUS OPTIONS to adjust parameters not shown on the panel."
        };

        private int _failuresCnt;
        private int _noFlatPhaseCnt;
        private int _noPeaksFoundCnt;
        private int _noSliceValidCnt;
        private int _periodCnt;
        private double _speed;
        private int _successCnt;
        private double _sucessRate;
        private double _time;
        private int _totalCnt;
        private int _totalDataAmount;

        public StatsWindow() {
            InitializeComponent();
            TbData.DataContext = this;
            TbFailures.DataContext = this;
            TbNoPeak.DataContext = this;
            TbNoPeriod.DataContext = this;
            TbSpeed.DataContext = this;
            TbRecords.DataContext = this;
            TbTime.DataContext = this;
            TbNoPhase.DataContext = this;
            TbSuccess.DataContext = this;
            TbSuccessRate.DataContext = this;
            TbValidPeriods.DataContext = this;
            SourceInitialized += Window1_SourceInitialized;

            var tipsCnt = _tips.Length;
            var random = new Random();
            TbTipsContent.Text = _tips[random.Next(tipsCnt)];
            var tickCnt = 0;

            var dispatcherTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(10)};
            dispatcherTimer.Tick += (sender, args) => {
                var index = tickCnt%tipsCnt;
                if (index == 0) {
                    Shuffle(_tips);
                }
                TbTipsContent.Text = _tips[index];
                tickCnt++;
            };
            dispatcherTimer.Start();
        }

        public int TotalCnt {
            get { return _totalCnt; }
            set {
                if (value == _totalCnt) return;
                _totalCnt = value;
                OnPropertyChanged();
            }
        }

        public int TotalDataAmount {
            get { return _totalDataAmount; }
            set {
                if (value == _totalDataAmount) return;
                _totalDataAmount = value;
                OnPropertyChanged();
            }
        }

        public double SucessRate {
            get { return _sucessRate; }
            set {
                if (value.Equals(_sucessRate)) return;
                _sucessRate = value;
                OnPropertyChanged();
            }
        }

        public int SuccessCnt {
            get { return _successCnt; }
            set {
                if (value == _successCnt) return;
                _successCnt = value;
                OnPropertyChanged();
            }
        }

        public int NoPeaksFoundCnt {
            get { return _noPeaksFoundCnt; }
            set {
                if (value == _noPeaksFoundCnt) return;
                _noPeaksFoundCnt = value;
                OnPropertyChanged();
            }
        }

        public int NoSliceValidCnt {
            get { return _noSliceValidCnt; }
            set {
                if (value == _noSliceValidCnt) return;
                _noSliceValidCnt = value;
                OnPropertyChanged();
            }
        }

        public int NoFlatPhaseCnt {
            get { return _noFlatPhaseCnt; }
            set {
                if (value == _noFlatPhaseCnt) return;
                _noFlatPhaseCnt = value;
                OnPropertyChanged();
            }
        }

        public int FailuresCnt {
            get { return _failuresCnt; }
            set {
                if (value == _failuresCnt) return;
                _failuresCnt = value;
                OnPropertyChanged();
            }
        }

        public double Time {
            get { return _time; }
            set {
                if (value.Equals(_time)) return;
                _time = value;
                OnPropertyChanged();
            }
        }

        public double Speed {
            get { return _speed; }
            set {
                if (value.Equals(_speed)) return;
                _speed = value;
                OnPropertyChanged();
            }
        }

        public int PeriodCnt {
            get { return _periodCnt; }
            set {
                if (value == _periodCnt) return;
                _periodCnt = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static void Shuffle<T>(T[] array) {
            var random = new Random();

            for (var index = 0; index < array.Length; index++) {
                var x = array[index];
                var swapIndex = random.Next(array.Length);
                var t = array[swapIndex];
                array[swapIndex] = x;
                array[index] = t;
            }
        }

        private void Window1_SourceInitialized(object sender, EventArgs e) {
            var helper = new WindowInteropHelper(this);
            var source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            switch (msg) {
                case WM_SYSCOMMAND:
                    var command = wParam.ToInt32() & 0xfff0;
                    if (command == SC_MOVE) {
                        handled = true;
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Update(IResult result) {
            var sizeInM = (int) (SamplingConfigurations.Get().RecordLength/1e6);
            TotalCnt += 1;
            TotalDataAmount += sizeInM;
            Speed = TotalDataAmount/Time;
            if (result.IsSuccessful) {
                SuccessCnt += 1;
            } else {
                FailuresCnt += 1;
            }
            PeriodCnt += result.ValidPeriodCnt;
            SucessRate = SuccessCnt/(double) TotalCnt;
            if (result.HasException) {
                switch (result.Exception) {
                    case ProcessException.NoPeakFound:
                        NoPeaksFoundCnt++;
                        break;
                    case ProcessException.NoSliceValid:
                        NoSliceValidCnt++;
                        break;
                    case ProcessException.NoFlatPhaseIntervalFound:
                        NoFlatPhaseCnt++;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void Reset() {
            TotalCnt = 0;
            TotalDataAmount = 0;
            FailuresCnt = 0;
            SuccessCnt = 0;
            SucessRate = 0;
            NoPeaksFoundCnt = 0;
            NoFlatPhaseCnt = 0;
            NoSliceValidCnt = 0;
            PeriodCnt = 0;
        }
    }
}