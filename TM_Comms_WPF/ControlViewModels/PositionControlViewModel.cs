using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
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
            get => new MotionScriptBuilder.MoveStep(MoveType, DataFormat, new MotionScriptBuilder.Position(Position), Velocity, Accel, Blend, "");
            set
            {
                int i = 0;
                foreach (ComboBoxItem cmb in MoveTypes)
                {
                    if ((MotionScriptBuilder.MoveTypes)cmb.Content == value.MoveType)
                    {
                        MoveTypesSelectedIndex = i;
                        break;
                    }
                    i++;
                }

                foreach (ComboBoxItem cmb in DataFormats)
                {
                    if ((MotionScriptBuilder.DataFormats)cmb.Content == value.DataFormat)
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

        public string MoveType => MoveTypes.Count > 0 ? ((MotionScriptBuilder.DataFormats)MoveTypes[MoveTypesSelectedIndex].Content).ToString() : "PTP";
        public string DataFormat => DataFormats.Count > 0 ? ((MotionScriptBuilder.DataFormats)DataFormats[DataFormatsSelectedIndex].Content).ToString() : "CPP";

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
            foreach (KeyValuePair<MotionScriptBuilder.MoveTypes, List<MotionScriptBuilder.DataFormats>> kv in MotionScriptBuilder.MoveTypes_DataFormats)
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

            foreach (MotionScriptBuilder.DataFormats df in MotionScriptBuilder.MoveTypes_DataFormats[(MotionScriptBuilder.MoveTypes)MoveTypes[MoveTypesSelectedIndex].Content])
            {
                ComboBoxItem cmb = new ComboBoxItem()
                {
                    Content = df
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
                {
                    if (double.TryParse(posVals[0], out double test))
                        PValue1 = Math.Round(test, 3).ToString("0.0##");
                    else
                        PValue1 = "0.0";
                }
                {
                    if (double.TryParse(posVals[1], out double test))
                        PValue2 = Math.Round(test, 3).ToString("0.0##");
                    else
                        PValue2 = "0.0";
                }
                {
                    if (double.TryParse(posVals[2], out double test))
                        PValue3 = Math.Round(test, 3).ToString("0.0##");
                    else
                        PValue3 = "0.0";
                }
                {
                    if (double.TryParse(posVals[3], out double test))
                        PValue4 = Math.Round(test, 3).ToString("0.0##");
                    else
                        PValue4 = "0.0";
                }
                {
                    if (double.TryParse(posVals[4], out double test))
                        PValue5 = Math.Round(test, 3).ToString("0.0##");
                    else
                        PValue5 = "0.0";
                }
                {
                    if (double.TryParse(posVals[5], out double test))
                        PValue6 = Math.Round(test, 3).ToString("0.0##");
                    else
                        PValue6 = "0.0";
                }
            }

        }

        private void SetPositionType()
        {
            if (DataFormats.Count == 0) return;

            char[] type = (((MotionScriptBuilder.DataFormats)DataFormats[DataFormatsSelectedIndex].Content).ToString()).ToCharArray();

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
