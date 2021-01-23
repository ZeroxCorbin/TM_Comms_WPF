using SimpleModbus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using TM_Comms_WPF.Commands;

namespace TM_Comms_WPF.ViewModels
{
    public class ModbusUserItemViewModel : INotifyPropertyChanged
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

        private SocketManagerNS.SocketManager Socket { get; }
        private SimpleModbusTCP ModbusTCP { get; }
        private TM_Comms.ModbusDictionary.MobusValue ModbusValue { get; set; }


        private string _value;
        private string type;

        public string Name { get; }
        public string Value { get => _value; set { _value = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value")); } }
        public string Type { get => type; set { SetProperty(ref type, value); ModbusValue.Type = (TM_Comms.ModbusDictionary.MobusValue.DataTypes)Enum.Parse(typeof(TM_Comms.ModbusDictionary.MobusValue.DataTypes), value); } }
        public string Addr { get; }
        public string Access { get; }

        public string[] TypeList { get => (string[])Enum.GetNames(typeof(TM_Comms.ModbusDictionary.MobusValue.DataTypes)); }

        public Visibility IsWritable { get; } = Visibility.Collapsed;
        public Visibility IsWritableBool { get; } = Visibility.Collapsed;

        private ICommand writeItemCommand;
        public ICommand WriteItemCommand
        {
            get
            {
                if (this.writeItemCommand == null)
                    this.writeItemCommand = new RelayCommand(WriteAction, c => CanConnect);
                return this.writeItemCommand;
            }
        }
        public bool CanConnect => true;

        private ICommand editItemCommand;
        public ICommand EditItemCommand
        {
            get
            {
                if (this.editItemCommand == null)
                    this.editItemCommand = new RelayCommand(EditAction, c => CanConnect);
                return this.editItemCommand;
            }
        }

        public ModbusUserItemViewModel Instance => this;

        public ModbusUserItemViewModel(string name, TM_Comms.ModbusDictionary.MobusValue modbusValue, SocketManagerNS.SocketManager socket)
        {

            Name = name;
            ModbusValue = modbusValue;
            Socket = socket;

            ModbusTCP = new SimpleModbusTCP(Socket);

            Type = ModbusValue.Type.ToString();
            Addr = $"{ModbusValue.Addr} / x{ModbusValue.Addr:X}";
            Access = ModbusValue.Access.ToString();

            IsWritable = ModbusValue.Access == TM_Comms.ModbusDictionary.MobusValue.AccessTypes.RW || ModbusValue.Access == TM_Comms.ModbusDictionary.MobusValue.AccessTypes.W ? Visibility.Visible : Visibility.Collapsed;
        }

        private void WriteAction(object parameter) => Write("true");
        public void Write(string value)
        {
            if (ModbusValue.Type == TM_Comms.ModbusDictionary.MobusValue.DataTypes.Bool)
                ModbusTCP.SetBool(ModbusValue.Addr, bool.Parse(value));
            if (ModbusValue.Type == TM_Comms.ModbusDictionary.MobusValue.DataTypes.Int16)
                ModbusTCP.SetInt16(ModbusValue.Addr, new Int16[] { Int16.Parse(value) });
            if (ModbusValue.Type == TM_Comms.ModbusDictionary.MobusValue.DataTypes.Int32)
                ModbusTCP.SetInt32(ModbusValue.Addr, new Int32[] { Int32.Parse(value) });
            if (ModbusValue.Type == TM_Comms.ModbusDictionary.MobusValue.DataTypes.Float)
                ModbusTCP.SetFloat(ModbusValue.Addr, new float[] { float.Parse(value) });
        }

        private void EditAction(object parameter) => Edit();
        public async void Edit()
        {
            //string res = await Application.Current.MainPage.DisplayPromptAsync($"{Name}", "New Value", initialValue: $"{Value}", keyboard: Keyboard.Numeric);

            //if (res != null)
            //    Write(res);
        }
        public string Read()
        {
            if (Socket.IsConnected)
            {
                if (ModbusValue.Type == TM_Comms.ModbusDictionary.MobusValue.DataTypes.Bool)
                    return Value = ModbusTCP.GetBool(ModbusValue.Addr).ToString();
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
