using MahApps.Metro.Controls.Dialogs;
using SimpleModbus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TM_Comms_WPF.Core;

namespace TM_Comms_WPF.WindowViewModels
{
    public class ModbusItemViewModel : Core.BaseViewModel
    { 

        private SimpleModbusTCP ModbusTCP { get; }
        private TM_Comms.ModbusDictionary.MobusValue ModbusValue { get; set; }

        public string[] TypeList { get => Enum.GetNames(typeof(TM_Comms.ModbusDictionary.MobusValue.DataTypes)); }

        private string _value;
        private string type;

        public string Name { get; }
        public string Value { get => _value; set { _value = value; OnPropertyChanged("Value"); } }
        public string Type { get => type; set { SetProperty(ref type, value); ModbusValue.Type = (TM_Comms.ModbusDictionary.MobusValue.DataTypes)Enum.Parse(typeof(TM_Comms.ModbusDictionary.MobusValue.DataTypes), value); } }
        public string Addr { get; }
        public string Access { get; }

        public Visibility ShowText => (Access == "W") ? Visibility.Collapsed : Visibility.Visible;
        public Visibility IsWritable { get; } = Visibility.Collapsed;

        public ICommand EditItemCommand { get; }

        public ModbusItemViewModel Instance => this;

        private IDialogCoordinator _DialogCoordinator;

        public ModbusItemViewModel(string name, TM_Comms.ModbusDictionary.MobusValue modbusValue, SimpleModbusTCP modbus, IDialogCoordinator diag)
        {

            _DialogCoordinator = DialogCoordinator.Instance;

            EditItemCommand = new RelayCommand(EditItemAction, c => true);

            Name = name;
            ModbusValue = modbusValue;

            ModbusTCP = modbus;

            Type = ModbusValue.Type.ToString();
            Addr = $"{ModbusValue.Addr} / x{ModbusValue.Addr:X}";
            Access = ModbusValue.Access.ToString();

            IsWritable = ModbusValue.Access == TM_Comms.ModbusDictionary.MobusValue.AccessTypes.RW || ModbusValue.Access == TM_Comms.ModbusDictionary.MobusValue.AccessTypes.W ? Visibility.Visible : Visibility.Collapsed;
        }


        private async Task<string> ShowInputDialog(string title, string message, string value)
        {
            MetroDialogSettings settings = new MetroDialogSettings()
            {
                DefaultText = value
            };

            string result = await _DialogCoordinator.ShowInputAsync(this, title, message, settings);

            if (string.IsNullOrEmpty(result))
                return null;

            return result;
        }

        private async Task<MessageDialogResult> ShowOkDialog(string title, string message)
        {
            return await _DialogCoordinator.ShowMessageAsync(this, title, message, MessageDialogStyle.Affirmative);
        }

        public async void EditItemAction(object parameter)
        {
            //Xamarin
            //string res = await Application.Current.MainPage.DisplayPromptAsync($"{Name}", "New Value", initialValue: $"{Value}", keyboard: Keyboard.Numeric);
            //if (res != null)
            //    Write(res);
            //DialogService.OkDialog(new DialogParameters() { Message = "Hello cruel world!" }, parameter as Window);


            string result;
            if (ModbusValue.Type == TM_Comms.ModbusDictionary.MobusValue.DataTypes.Coil)
            {
                if (!string.IsNullOrEmpty(result = await ShowInputDialog(Name, "Enter 'true' or 'false'.", this.Value)))
                {
                    if (bool.TryParse(result, out bool res))
                    {
                        if (!ModbusTCP.WriteSingleCoil(ModbusValue.Addr, res))
                            _ = ShowOkDialog("Modbus write error!", $"Could not write value: {res} to: {ModbusValue.Addr}");
                    }
                    else
                        _ = ShowOkDialog("Parse error!",$"Could not parse value: {result}");
                }
            }
            else if (ModbusValue.Type == TM_Comms.ModbusDictionary.MobusValue.DataTypes.Int16)
            {
                if (!string.IsNullOrEmpty(result = await ShowInputDialog(Name, $"Enter a value.", this.Value)))
                {
                    if (Int16.TryParse(result, out short res))
                    {
                        if (!ModbusTCP.SetInt16(ModbusValue.Addr, new Int16[] { res }))
                            _ = ShowOkDialog("Modbus write error!", $"Could not write value: {res} to: {ModbusValue.Addr}");
                    }
                    else
                        _ = ShowOkDialog("Parse error!", $"Could not parse value: {result}");
                }
            }
            else if (ModbusValue.Type == TM_Comms.ModbusDictionary.MobusValue.DataTypes.Int32)
            {
                if (!string.IsNullOrEmpty(result = await ShowInputDialog(Name, $"Enter a value.", this.Value)))
                {
                    if (Int32.TryParse(result, out int res))
                    {
                        if (!ModbusTCP.SetInt32(ModbusValue.Addr, new Int32[] { res }))
                            _ = ShowOkDialog("Modbus write error!", $"Could not write value: {res} to: {ModbusValue.Addr}");
                    }
                    else
                        _ = ShowOkDialog("Parse error!", $"Could not parse value: {result}");
                }
            }
            else if (ModbusValue.Type == TM_Comms.ModbusDictionary.MobusValue.DataTypes.Float)
            {
                if (!string.IsNullOrEmpty(result = await ShowInputDialog(Name, $"Enter a value.", this.Value)))
                {
                    if (float.TryParse(result, out float res))
                    {
                        if (!ModbusTCP.SetFloat(ModbusValue.Addr, new float[] { res }))
                            _ = ShowOkDialog("Modbus write error!", $"Could not write value: {res} to: {ModbusValue.Addr}");
                    }
                    else
                        _ = ShowOkDialog("Parse error!", $"Could not parse value: {result}");
                }
            }
            else if (ModbusValue.Type == TM_Comms.ModbusDictionary.MobusValue.DataTypes.String)
            {
                if (!string.IsNullOrEmpty(result = await ShowInputDialog(Name, $"Enter a value.", this.Value)))
                {

                    if (!ModbusTCP.SetString(ModbusValue.Addr, result))
                        _ = ShowOkDialog("Modbus write error!", $"Could not write value: {result} to: {ModbusValue.Addr}");
                }
            }


            //DialogResultData result = DialogService.EditValueDialog(param, parameter as Window);
        }

        public string Read()
        {
            if (ModbusTCP.Socket.IsConnected)
            {
                if(ModbusValue.Type == TM_Comms.ModbusDictionary.MobusValue.DataTypes.String)
                {
                    if(ModbusValue.Addr == 7308) // HMI Version
                        return Value = ModbusTCP.GetString(ModbusValue.Addr, 4);
                    else if(ModbusValue.Addr == 7701)
                        return Value = ModbusTCP.GetString(ModbusValue.Addr, 98);
                }
                if (ModbusValue.Type == TM_Comms.ModbusDictionary.MobusValue.DataTypes.Input)
                    return Value = ModbusTCP.ReadDiscreteInput(ModbusValue.Addr).ToString();
                if (ModbusValue.Type == TM_Comms.ModbusDictionary.MobusValue.DataTypes.Coil)
                    return Value = ModbusTCP.ReadCoils(ModbusValue.Addr).ToString();
                if (ModbusValue.Type == TM_Comms.ModbusDictionary.MobusValue.DataTypes.Int16)
                    return Value = ModbusTCP.GetInt16(ModbusValue.Addr).ToString();
                if (ModbusValue.Type == TM_Comms.ModbusDictionary.MobusValue.DataTypes.Int32)
                    return Value = ModbusTCP.GetInt32(ModbusValue.Addr).ToString();
                if (ModbusValue.Type == TM_Comms.ModbusDictionary.MobusValue.DataTypes.Float)
                    return Value = ModbusTCP.GetFloat(ModbusValue.Addr).ToString();
            }
            return "";
        }
    }
}
