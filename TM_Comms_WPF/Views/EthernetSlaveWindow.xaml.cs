using RingBuffer;
using SocketManagerNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Data;
using TM_Comms;

namespace TM_Comms_WPF.Views
{
    public partial class EthernetSlaveWindow : Window
    {
        public EthernetSlaveWindow(Window owner)
        {
  
            Owner = owner;

            InitializeComponent();

            SetBinding(Window.WidthProperty, new Binding("Width") { Source = this.DataContext, Mode = BindingMode.TwoWay });
            SetBinding(Window.HeightProperty, new Binding("Height") { Source = this.DataContext, Mode = BindingMode.TwoWay });
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox)sender).ScrollToEnd();
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if(((TextBox)sender).IsFocused)
                ((TextBox)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
    }
}
