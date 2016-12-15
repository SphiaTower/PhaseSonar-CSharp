using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using JetBrains.Annotations;
using PhaseSonar.Analyzers;
using SpectroscopyVisualizer.Configs;
using SpectroscopyVisualizer.Consumers;

namespace SpectroscopyVisualizer {
    /// <summary>
    /// Interaction logic for ProcessStatistics.xaml
    /// </summary>
    public partial class StatsWindow : Window,INotifyPropertyChanged {
        private int _failuresCnt;
        private int _totalCnt;
        private int _totalDataAmount;
        private int _noPeaksFoundCnt;
        private int _noSliceValidCnt;
        private int _noFlatPhaseCnt;
        private double _time;
        private double _speed;
        private int _successCnt;
        private double _sucessRate;
        private int _periodCnt;

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
            this.SourceInitialized += Window1_SourceInitialized;
        }

        private void Window1_SourceInitialized(object sender, EventArgs e) {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(WndProc);
        }

        const int WM_SYSCOMMAND = 0x0112;
        const int SC_MOVE = 0xF010;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {

            switch (msg) {
                case WM_SYSCOMMAND:
                    int command = wParam.ToInt32() & 0xfff0;
                    if (command == SC_MOVE) {
                        handled = true;
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
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

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Update(IResult result) {
            int sizeInM = (int)(SamplingConfigurations.Get().RecordLength / 1e6);
            TotalCnt += 1;
            TotalDataAmount += sizeInM;
            Speed = TotalDataAmount / Time;
            if (result.IsSuccessful) {
                SuccessCnt += 1;
            } else {
                FailuresCnt += 1;
            }
            PeriodCnt += result.ValidPeriodCnt;
            SucessRate = SuccessCnt/ (double)TotalCnt;
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
