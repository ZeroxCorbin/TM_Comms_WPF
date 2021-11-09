using SimpleModbus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using TM_Comms_WPF.Commands;
using TM_Comms_WPF.Dialogs.DialogService;

namespace TM_Comms_WPF.WindowViewModels
{
    public class ModbusItemViewModel : BaseViewModel
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

        public ModbusItemViewModel(string name, TM_Comms.ModbusDictionary.MobusValue modbusValue, SimpleModbusTCP modbus)
        {
            EditItemCommand = new RelayCommand(EditItemAction, c => true);

            Name = name;
            ModbusValue = modbusValue;

            ModbusTCP = modbus;

            Type = ModbusValue.Type.ToString();
            Addr = $"{ModbusValue.Addr} / x{ModbusValue.Addr:X}";
            Access = ModbusValue.Access.ToString();

            IsWritable = ModbusValue.Access == TM_Comms.ModbusDictionary.MobusValue.AccessTypes.RW || ModbusValue.Access == TM_Comms.ModbusDictionary.MobusValue.AccessTypes.W ? Visibility.Visible : Visibility.Collapsed;
        }

        public void EditItemAction(object parameter)
        {
            //Xamarin
            //string res = await Application.Current.MainPage.DisplayPromptAsync($"{Name}", "New Value", initialValue: $"{Value}", keyboard: Keyboard.Numeric);
            //if (res != null)
            //    Write(res);
            //DialogService.OkDialog(new DialogParameters() { Message = "Hello cruel world!" }, parameter as Window);

            DialogResultData result;
            if (ModbusValue.Type == TM_Comms.ModbusDictionary.MobusValue.DataTypes.Coil)
            {
                if ((result = DialogService.EditValueDialog(new DialogParameters() { Title = "Bool", Value = this.Value }, parameter as Window)).Result == DialogResult.Ok)
                {
                    if (bool.TryParse(result.Value, out bool res))
                    {
                        if (!ModbusTCP.WriteSingleCoil(ModbusValue.Addr, res))
                            DialogService.OkDialog(new DialogParameters() { Title = "Modbus write error!", Message = $"Could not write value: {res} to: {ModbusValue.Addr}" }, parameter as Window);
                    }
                    else
                        DialogService.OkDialog(new DialogParameters() { Title = "Parse error!", Message = $"Could not parse value: {result.Value}" }, parameter as Window);
                }
            }
            else if (ModbusValue.Type == TM_Comms.ModbusDictionary.MobusValue.DataTypes.Int16)
            {
                if ((result = DialogService.EditValueDialog(new DialogParameters() { Title = "Int16", Value = this.Value }, parameter as Window)).Result == DialogResult.Ok)
                {
                    if (Int16.TryParse(result.Value, out short res))
                    {
                        if (!ModbusTCP.SetInt16(ModbusValue.Addr, new Int16[] { res }))
                            DialogService.OkDialog(new DialogParameters() { Title = "Modbus write error!", Message = $"Could not write value: {res} to: {ModbusValue.Addr}" }, parameter as Window);
                    }
                    else
                        DialogService.OkDialog(new DialogParameters() { Title = "Parse error!", Message = $"Could not parse value: {result.Value}" }, parameter as Window);
                }
            }
            else if (ModbusValue.Type == TM_Comms.ModbusDictionary.MobusValue.DataTypes.Int32)
            {
                if ((result = DialogService.EditValueDialog(new DialogParameters() { Title = "Int32", Value = this.Value }, parameter as Window)).Result == DialogResult.Ok)
                {
                    if (Int32.TryParse(result.Value, out int res))
                    {
                        if (!ModbusTCP.SetInt32(ModbusValue.Addr, new Int32[] { res }))
                            DialogService.OkDialog(new DialogParameters() { Title = "Modbus write error!", Message = $"Could not write value: {res} to: {ModbusValue.Addr}" }, parameter as Window);
                    }
                    else
                        DialogService.OkDialog(new DialogParameters() { Title = "Parse error!", Message = $"Could not parse value: {result.Value}" }, parameter as Window);
                }
            }
            else if (ModbusValue.Type == TM_Comms.ModbusDictionary.MobusValue.DataTypes.Float)
            {
                if ((result = DialogService.EditValueDialog(new DialogParameters() { Title = "Float", Value = this.Value }, parameter as Window)).Result == DialogResult.Ok)
                {
                    if (float.TryParse(result.Value, out float res))
                    {
                        if (!ModbusTCP.SetFloat(ModbusValue.Addr, new float[] { res }))
                            DialogService.OkDialog(new DialogParameters() { Title = "Modbus write error!", Message = $"Could not write value: {res} to: {ModbusValue.Addr}" }, parameter as Window);
                    }
                    else
                        DialogService.OkDialog(new DialogParameters() { Title = "Parse error!", Message = $"Could not parse value: {result.Value}" }, parameter as Window);
                }
            }
            else if (ModbusValue.Type == TM_Comms.ModbusDictionary.MobusValue.DataTypes.String)
            {
                if ((result = DialogService.EditValueDialog(new DialogParameters() { Title = "String", Value = this.Value }, parameter as Window)).Result == DialogResult.Ok)
                {
                    
                    if (!ModbusTCP.SetString(ModbusValue.Addr, result.Value))
                        DialogService.OkDialog(new DialogParameters() { Title = "Modbus write error!", Message = $"Could not write value: {result.Value} to: {ModbusValue.Addr}" }, parameter as Window);
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
