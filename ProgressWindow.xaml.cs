using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Styx
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public bool allowClosing = false;

        public ProgressWindow(string message)
        {
            InitializeComponent();
            textBlock_message.Text = message;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!allowClosing)
                e.Cancel = true;
            else
                e.Cancel = false;
            //to close use progWindow.allowClosing = true; progWindow.Close();
        }
    }
}
