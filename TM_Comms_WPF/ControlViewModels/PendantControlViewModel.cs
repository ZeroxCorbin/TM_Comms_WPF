using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using TM_Comms;
using TM_Comms_WPF.Core;

namespace TM_Comms_WPF.ControlViewModels
{
    public class PendantControlViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Object.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public System.Windows.Visibility Border18Visible { get => App.Settings.Version >= TMflowVersions.V1_80_xxxx ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; }


        public delegate void ButtonEventDel();

        public event ButtonEventDel StopEvent;
        public event ButtonEventDel PlayPauseEvent;
        public event ButtonEventDel PlusEvent;
        public event ButtonEventDel MinusEvent;

        private void StopAction(object parameter) => StopEvent?.Invoke();
        private void PlayPauseAction(object parameter) => PlayPauseEvent.Invoke();
        private void PlusAction(object parameter) => PlusEvent.Invoke();
        private void MinusAction(object parameter) => MinusEvent.Invoke();


        private string errorDescription;
        private string errorCode;
        private string errorDate;
        public string ErrorDescription { get => errorDescription; set => SetProperty(ref errorDescription, value); }
        public string ErrorCode { get => errorCode; set => SetProperty(ref errorCode, value); }
        public string ErrorDate { get => errorDate; set => SetProperty(ref errorDate, value); }


        public Brush Good { get; } = new SolidColorBrush(Colors.Cyan);
        public Brush Bad { get; } = new SolidColorBrush(Colors.Red);
        public Brush Meh { get; } = new SolidColorBrush(Colors.Yellow);
        public Brush Disabled { get; } = new SolidColorBrush(Colors.White);
        public Brush GoodRadial { get; } = new RadialGradientBrush(Colors.Cyan, Colors.Transparent);
        public Brush BadRadial { get; } = new RadialGradientBrush(Colors.Red, Colors.Transparent);
        public Brush MehRadial { get; } = new RadialGradientBrush(Colors.Yellow, Colors.Transparent);
        public Brush Transparent { get; } = new SolidColorBrush(Colors.Transparent);

        private Brush power = new SolidColorBrush(Colors.Yellow);
        private Brush manual = new SolidColorBrush(Colors.White);
        private Brush auto = new SolidColorBrush(Colors.White);
        private Brush error = new SolidColorBrush(Colors.White);
        private Brush estop = new SolidColorBrush(Colors.White);
        private Brush getControl = new SolidColorBrush(Colors.White);
        private Brush autoActive = new SolidColorBrush(Colors.White);
        private Brush autoEnable = new SolidColorBrush(Colors.White);

        private Brush stop = new SolidColorBrush(Colors.Transparent);
        private Brush play = new SolidColorBrush(Colors.Transparent);

        public Brush Power { get => power; set => SetProperty(ref power, value); }
        public Brush Manual { get => manual; set => SetProperty(ref manual, value); }
        public Brush Auto { get => auto; set => SetProperty(ref auto, value); }
        public Brush Error { get => error; set => SetProperty(ref error, value); }
        public Brush Estop { get => estop; set => SetProperty(ref estop, value); }
        public Brush GetControl { get => getControl; set => SetProperty(ref getControl, value); }
        public Brush AutoActive { get => autoActive; set => SetProperty(ref autoActive, value); }
        public Brush AutoEnable { get => autoEnable; set => SetProperty(ref autoEnable, value); }

        public Brush Stop { get => stop; set => SetProperty(ref stop, value); }
        public Brush Play { get => play; set => SetProperty(ref play, value); }

        public ICommand StopCommand { get; }
        public ICommand PlayPauseCommand { get; }
        public ICommand PlusCommand { get; }
        public ICommand MinusCommand { get; }

        public PendantControlViewModel()
        {
            PlayPauseCommand = new RelayCommand(PlayPauseAction, c => true);
            PlusCommand = new RelayCommand(PlusAction, c => true);
            MinusCommand = new RelayCommand(MinusAction, c => true);
            StopCommand = new RelayCommand(StopAction, c => true);

        }

        public void UpdatePendant(EthernetSlave es)
        {
            Power = Good;

            if (es.GetValue("MA_Mode") == "1")
            {
                Auto = Good;
                Manual = Disabled;
            }
            else
            {
                Auto = Disabled;
                Manual = Good;
            }

            if (es.GetValue("ESTOP") == "true")
                Estop = Bad;
            else
                Estop = Disabled;

            if (App.Settings.Version > TMflowVersions.V1_80_xxxx)
            {
                if (es.GetValue("Get_Control") == "true")
                    GetControl = Good;
                else
                    GetControl = Bad;

                if (es.GetValue("Auto_Remote_Active") == "true")
                    AutoActive = Good;
                else
                    AutoActive = Bad;

                if (es.GetValue("Auto_Remote_Enable") == "true")
                    AutoEnable = Good;
                else
                    AutoEnable = Bad;
            }

            if (es.GetValue("Project_Run") == "true")
            {
                Play = GoodRadial;
                Stop = Transparent;
            }
            else if (es.GetValue("Project_Pause") == "true")
            {
                Play = MehRadial;
                Stop = Transparent;
            }
            else if (es.GetValue("Project_Edit") == "true")
            {
                Play = Transparent;
                Stop = MehRadial;
            }
            else
            {
                Play = Transparent;
                Stop = BadRadial;
            }

            if (es.GetValue("Robot_Error") == "true")
                Error = Bad;
            else
                Error = Disabled;


            if (es.GetValue("Robot_Error") == "true")
            {
                Error = Bad;

                uint code = uint.Parse(es.GetValue("Error_Code"));
                if (code != 0)
                {
                    string dat = $"{es.GetValue("Error_Time")}";
                    if (DateTime.TryParse(dat, out DateTime date))
                        ErrorDate = date.ToString();
                }
                else
                    ErrorDate = "";

                ErrorCode = code.ToString("X");

                if (ErrorCodes.Codes.TryGetValue(code, out string val))
                    ErrorDescription = val;
                else
                    ErrorDescription = "CAN NOT FIND ERROR IN TABLE.";
            }
            else
            {
                Error = Disabled;
                ErrorDate = "";
                ErrorCode = "";
                ErrorDescription = "";
            }
        }

        public void Reset()
        {
            Power = Meh;
            Manual = Disabled;
            Auto = Disabled;
            Estop = Disabled;
            GetControl = Disabled;
            AutoActive = Disabled;
            AutoEnable = Disabled;

            Stop = Transparent;
            Play = Transparent;

            Error = Disabled;
            ErrorCode = "";
            ErrorDate = "";
            ErrorDescription = "";
        }

    }
}
