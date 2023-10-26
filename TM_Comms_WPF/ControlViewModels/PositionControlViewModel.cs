using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TM_Comms;

namespace TM_Comms_WPF.ControlViewModels
{
    public class PositionControlViewModel : Core.BaseViewModel
    {
        //public EthernetSlaveController EthernetSlaveController
        //{
        //    get => _EthernetSlaveController;
        //    set
        //    {
        //        _EthernetSlaveController = value;
        //        ViewRead = true;
        //        ReadPosition = new Core.RelayCommand(ReadPositionAction, c => true);
        //    }
        //}
        //private EthernetSlaveController _EthernetSlaveController;

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


        [JsonProperty]
        public bool ViewLabels { get => viewLabels; set { SetProperty(ref viewLabels, value); } }
        private bool viewLabels = true;
        [JsonProperty]
        public bool ViewDragDropTarget { get => viewDragDropTarget; set { SetProperty(ref viewDragDropTarget, value); } }
        private bool viewDragDropTarget = false;
        [JsonProperty]
        public bool ViewSimple { get => viewSimple; set { SetProperty(ref viewSimple, value); } }
        private bool viewSimple = false;
        public bool ViewRead { get => viewRead; set { SetProperty(ref viewRead, value); } }
        private bool viewRead = false;

        [JsonProperty]
        public bool IsReadOnly { get => isReadOnly; set { SetProperty(ref isReadOnly, value); } }
        private bool isReadOnly = false;
        [JsonProperty]
        public bool IsEnabled { get => isEnabled; set { SetProperty(ref isEnabled, value); } }
        private bool isEnabled = true;


        public ObservableCollection<MotionScriptBuilder.MoveTypes> MoveTypes { get; } = new ObservableCollection<MotionScriptBuilder.MoveTypes>();
        [JsonProperty]
        public int MoveTypesSelectedIndex { get => moveTypesSelectedIndex; set { SetProperty(ref moveTypesSelectedIndex, value); LoadDataFormats(); } }
        private int moveTypesSelectedIndex;

        public ObservableCollection<MotionScriptBuilder.DataFormats> DataFormats { get; } = new ObservableCollection<MotionScriptBuilder.DataFormats>();
        [JsonProperty]
        public int DataFormatsSelectedIndex { get => dataFormatsSelectedIndex; set { SetProperty(ref dataFormatsSelectedIndex, value); SetPositionType(); } }
        private int dataFormatsSelectedIndex;

        public MotionScriptBuilder.MoveStep MoveStep
        {
            get => new MotionScriptBuilder.MoveStep(MoveType, DataFormat, new MotionScriptBuilder.Position(Position), BaseName, Velocity, Accel, Blend);
            set
            {
                int i = 0;
                foreach (var cmb in MoveTypes)
                {
                    if (cmb == value.MoveType)
                    {
                        MoveTypesSelectedIndex = i;
                        break;
                    }
                    i++;
                }

                foreach (var cmb in DataFormats)
                {
                    if (cmb == value.DataFormat)
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
                BaseName = value.BaseName;
            }
        }

        public string MoveType => MoveTypes.Count > 0 ? MoveTypes[MoveTypesSelectedIndex].ToString() : "PTP";
        public string DataFormat => DataFormats.Count > 0 ? DataFormats[DataFormatsSelectedIndex].ToString() : "CPP";

        [JsonProperty]
        public string Position
        {
            get { return $"{PValue1},{PValue2},{PValue3},{PValue4},{PValue5},{PValue6}"; }
            set { ParsePosition(value); OnPropertyChanged("Position"); }
        }

        public string VelocityLabel { get => velocityLabel; set => SetProperty(ref velocityLabel, value); }
        private string velocityLabel;
        public int Velocity { get => velocity; set { SetProperty(ref velocity, value); } }
        private int velocity = 100;

        public string AccelLabel { get => accelLabel; set => SetProperty(ref accelLabel, value); }
        private string accelLabel;
        public int Accel { get => accel; set { SetProperty(ref accel, value); } }
        private int accel = 0;

        public string BlendLabel { get => blendLabel; set => SetProperty(ref blendLabel, value); }
        private string blendLabel;
        public int Blend { get => blend; set { SetProperty(ref blend, value); } }
        private int blend = 0;

        public bool Precision { get => precision; set { SetProperty(ref precision, value); } }
        private bool precision = false;

        [JsonProperty]
        public string BaseName { get => baseName; set { SetProperty(ref baseName, value); } }
        private string baseName = "";

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

        public ICommand ReadPosition { get; set; }

        public PositionControlViewModel()
        {
            ViewRead = false;

            SetupMoveTypes();

            //if (App.RobotController.EthernetSlaveController != null)
            //{
            //    ViewRead = true;
            //    ReadPosition = new Core.RelayCommand(ReadPositionAction, c => true);
            //}
        }

        //public void ReadPositionAction(object parameter)
        //{
        //    App.RobotController.EthernetSlaveController.EsStateEvent -= EthernetSlaveController_EsStateEvent;

        //    if (App.RobotController.EthernetSlaveController.IsConnected)
        //        App.RobotController.EthernetSlaveController.EsStateEvent += EthernetSlaveController_EsStateEvent;

        //}

        //private void EthernetSlaveController_EsStateEvent(EthernetSlaveController.EsStates state, string message, EthernetSlave ethernetSlave)
        //{
        //    if (state == EthernetSlaveController.EsStates.Normal)
        //    {
        //        App.RobotController.EthernetSlaveController.EsStateEvent -= EthernetSlaveController_EsStateEvent;

        //        if (DataFormatsSelectedIndex == 0)
        //        {
        //            BaseName = ethernetSlave.GetValue("Base_Name").Trim('\"');
        //            Position = ethernetSlave.GetValue("Coord_Base_Tool");
        //        }
        //        else
        //            Position = ethernetSlave.GetValue("Joint_Angle");
        //    }
        //}

        private void SetupMoveTypes()
        {
            foreach (KeyValuePair<MotionScriptBuilder.MoveTypes, List<MotionScriptBuilder.DataFormats>> kv in MotionScriptBuilder.MoveTypes_DataFormats)
            {
                //ComboBoxItem cmb = new ComboBoxItem()
                //{
                //    Content = kv.Key
                //};
                MoveTypes.Add(kv.Key);
            }
            MoveTypesSelectedIndex = 0;
        }

        private void LoadDataFormats()
        {
            DataFormats.Clear();

            foreach (MotionScriptBuilder.DataFormats df in MotionScriptBuilder.MoveTypes_DataFormats[MoveTypes[MoveTypesSelectedIndex]])
            {
                //ComboBoxItem cmb = new ComboBoxItem()
                //{
                //    Content = df
                //};
                DataFormats.Add(df);
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
                //PValue2 = double.Parse(posVals[1]).ToString();
                ////PValue1 = Regex.Match(posVals[0], @"-?\w*.\d{0,3}").Value;
                //PValue2 = Regex.Match(posVals[1], @"-?\w*.\d{0,3}").Value;
                //PValue3 = Regex.Match(posVals[2], @"-?\w*.\d{0,3}").Value;
                //PValue4 = Regex.Match(posVals[3], @"-?\w*.\d{0,3}").Value;
                //PValue5 = Regex.Match(posVals[4], @"-?\w*.\d{0,3}").Value;
                //PValue6 = Regex.Match(posVals[5], @"-?\w*.\d{0,3}").Value;
            }

        }

        private void SetPositionType()
        {
            if (DataFormats.Count == 0) return;

            char[] type = DataFormats[DataFormatsSelectedIndex].ToString().ToCharArray();

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
                Velocity = 100;
            }
            else
            {
                VelocityLabel = "V(mm/sec)";
                Velocity = 10;
            }

            if (type[2] == 'P')
            {
                BlendLabel = "B(%)";
                Blend = 0;
            }
            else
            {
                BlendLabel = "B(rad:mm)";
                Blend = 0;
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
