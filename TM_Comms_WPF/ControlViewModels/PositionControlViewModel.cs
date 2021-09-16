using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TM_Comms;

namespace TM_Comms_WPF.ControlViewModels
{
    public class PositionControlViewModel : INotifyPropertyChanged
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

        public delegate void DragDrop(object parent, bool drop);
        public event DragDrop DragDropEvent;
        public void DragDropStart(object parent)
        {
            DragDropEvent.Invoke(parent, false);
        }
        public void DragDropEnd(object parent)
        {
            DragDropEvent.Invoke(parent, true);
        }

        public Visibility Labels { get => labels; set { SetProperty(ref labels, value); } }
        private Visibility labels = Visibility.Visible;

        public Visibility DragDropTarget { get => dragDropTarget; set { SetProperty(ref dragDropTarget, value); } }
        private Visibility dragDropTarget = Visibility.Hidden;

        public ObservableCollection<ComboBoxItem> MoveTypes { get; } = new ObservableCollection<ComboBoxItem>();
        public int MoveTypesSelectedIndex { get => moveTypesSelectedIndex; set { SetProperty(ref moveTypesSelectedIndex, value); LoadDataFormats(); } }
        private int moveTypesSelectedIndex;

        public ObservableCollection<ComboBoxItem> DataFormats { get; } = new ObservableCollection<ComboBoxItem>();
        public int DataFormatsSelectedIndex { get => dataFormatsSelectedIndex; set { SetProperty(ref dataFormatsSelectedIndex, value); SetPositionType(); } }
        private int dataFormatsSelectedIndex;

        public MotionScriptBuilder.MoveStep MoveStep
        {
            get => new MotionScriptBuilder.MoveStep(MoveType, DataFormat, new MotionScriptBuilder.Position(Position), Velocity, Accel, Blend);
            set
            {
                int i = 0;
                foreach (ComboBoxItem cmb in MoveTypes)
                {
                    if ((string)cmb.Content == value.MoveType)
                    {
                        MoveTypesSelectedIndex = i;
                        break;
                    }
                    i++;
                }

                foreach (ComboBoxItem cmb in DataFormats)
                {
                    if ((string)cmb.Content == value.DataFormat)
                    {
                        DataFormatsSelectedIndex = i;
                        break;
                    }
                    i++;
                }

                Position = value.Position.ToCSV;
                Velocity = value.Velocity;
                Accel = value.Accel;
                Blend = value.Blend;
                Precision = value.Precision;
            }
        }

        public string MoveType => MoveTypes.Count > 0 ? (string)MoveTypes[MoveTypesSelectedIndex].Content : "PTP";
        public string DataFormat => DataFormats.Count > 0 ? (string)DataFormats[DataFormatsSelectedIndex].Content : "CPP";

        public string Position
        {
            get { return $"{PValue1},{PValue2},{PValue3},{PValue4},{PValue5},{PValue6}"; }
            set { ParsePosition(value); }
        }

        public string VelocityLabel { get => velocityLabel; set => SetProperty(ref velocityLabel, value); }
        private string velocityLabel;
        public string Velocity { get => velocity; set { SetProperty(ref velocity, value); } }
        private string velocity = "100";

        public string AccelLabel { get => accelLabel; set => SetProperty(ref accelLabel, value); }
        private string accelLabel;
        public string Accel { get => accel; set { SetProperty(ref accel, value); } }
        private string accel = "0";

        public string BlendLabel { get => blendLabel; set => SetProperty(ref blendLabel, value); }
        private string blendLabel;
        public string Blend { get => blend; set { SetProperty(ref blend, value); } }
        private string blend = "0";

        public bool Precision { get => precision; set { SetProperty(ref precision, value); } }
        private bool precision = false;

        public string PValue1Label { get => pValue1Label; set => SetProperty(ref pValue1Label, value); }
        private string pValue1Label;
        public string PValue2Label { get => pValue2Label; set => SetProperty(ref pValue2Label, value); }
        private string pValue2Label;
        public string PValue3Label { get => pValue3Label; set => SetProperty(ref pValue3Label, value); }
        private string pValue3Label;
        public string PValue4Label { get => pValue4Label; set => SetProperty(ref pValue4Label, value); }
        private string pValue4Label;
        public string PValue5Label { get => pValue5Label; set => SetProperty(ref pValue5Label, value); }
        private string pValue5Label;
        public string PValue6Label { get => pValue6Label; set => SetProperty(ref pValue6Label, value); }
        private string pValue6Label;

        public string PValue1 { get => pValue1; set => SetProperty(ref pValue1, value); }
        private string pValue1;
        public string PValue2 { get => pValue2; set => SetProperty(ref pValue2, value); }
        private string pValue2;
        public string PValue3 { get => pValue3; set => SetProperty(ref pValue3, value); }
        private string pValue3;
        public string PValue4 { get => pValue4; set => SetProperty(ref pValue4, value); }
        private string pValue4;
        public string PValue5 { get => pValue5; set => SetProperty(ref pValue5, value); }
        private string pValue5;
        public string PValue6 { get => pValue6; set => SetProperty(ref pValue6, value); }
        private string pValue6;

        public PositionControlViewModel()
        {
            SetupMoveTypes();
        }

        private void SetupMoveTypes()
        {
            foreach (KeyValuePair<string, List<string>> kv in MotionScriptBuilder.MoveTypes_DataFormats)
            {
                ComboBoxItem cmb = new ComboBoxItem()
                {
                    Content = kv.Key
                };
                MoveTypes.Add(cmb);
            }
            MoveTypesSelectedIndex = 0;
        }

        private void LoadDataFormats()
        {
            DataFormats.Clear();

            foreach (string s in MotionScriptBuilder.MoveTypes_DataFormats[(string)MoveTypes[MoveTypesSelectedIndex].Content])
            {
                ComboBoxItem cmb = new ComboBoxItem()
                {
                    Content = s
                };
                DataFormats.Add(cmb);
            }
            DataFormatsSelectedIndex = 0;
        }
        private void ParsePosition(string pos)
        {
            string[] posVals = pos.Split(',');
            if (posVals.Length == 6)
            {
                PValue1 = Regex.Match(posVals[0], @"-?\w*.\d{0,3}").Value;
                PValue2 = Regex.Match(posVals[1], @"-?\w*.\d{0,3}").Value;
                PValue3 = Regex.Match(posVals[2], @"-?\w*.\d{0,3}").Value;
                PValue4 = Regex.Match(posVals[3], @"-?\w*.\d{0,3}").Value;
                PValue5 = Regex.Match(posVals[4], @"-?\w*.\d{0,3}").Value;
                PValue6 = Regex.Match(posVals[5], @"-?\w*.\d{0,3}").Value;
            }

        }

        private void SetPositionType()
        {
            if (DataFormats.Count == 0) return;

            char[] type = ((string)DataFormats[DataFormatsSelectedIndex].Content).ToCharArray();

            if (type[0] == 'C')
            {
                if (PValue1Label != "X")
                    ClearValues();

                SetCartesian();
            }
            else
            {
                if (PValue1Label == "X")
                    ClearValues();

                SetJoint();
            }

            if (type[1] == 'P')
            {
                VelocityLabel = "V(%)";
                Velocity = "100";
            }
            else
            {
                VelocityLabel = "V(mm/sec)";
                Velocity = "10";
            }

            if (type[2] == 'P')
            {
                BlendLabel = "B(%)";
                Blend = "0";
            }
            else
            {
                BlendLabel = "B(rad:mm)";
                Blend = "3";
            }
        }

        private void SetCartesian()
        {
            PValue1Label = "X";
            PValue2Label = "Y";
            PValue3Label = "Z";
            PValue4Label = "Rx";
            PValue5Label = "Ry";
            PValue6Label = "Rz";
        }
        private void SetJoint()
        {
            PValue1Label = "J1";
            PValue2Label = "J2";
            PValue3Label = "J3";
            PValue4Label = "J4";
            PValue5Label = "J5";
            PValue6Label = "J6";
        }
        private void ClearValues()
        {
            PValue1 = "0";
            PValue2 = "0";
            PValue3 = "0";
            PValue4 = "0";
            PValue5 = "0";
            PValue6 = "0";
        }
    }
}
