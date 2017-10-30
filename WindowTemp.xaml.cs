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
    /// Interaction logic for WindowTemp.xaml
    /// </summary>
    public partial class WindowTemp : Window
    {
        public WindowTemp()
        {
            InitializeComponent();
        
        }

        private void listView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<Tuple<string, string>> list_just_selected = new List<Tuple<string, string>>(e.AddedItems.Cast < Tuple<string, string>>());
            foreach (Tuple<string, string> tuple in list_just_selected)
                MessageBox.Show("just selected: " + tuple.Item1 + " & " + tuple.Item2);
            List<Tuple<string, string>> list_just_unselected = new List<Tuple<string, string>>(e.RemovedItems.Cast < Tuple<string, string>>());
            foreach (Tuple<string, string> tuple in list_just_unselected)
                MessageBox.Show("just unselected: " + tuple.Item1 + " & " + tuple.Item2);
           
        }

    }
}
