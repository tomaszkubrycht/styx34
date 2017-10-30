using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Collections;

namespace Styx
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();   
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {

            
        }

        private void treeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }


    }
    public class SubNode : ObservableCollection<SubNode>
    {
        private string _name;

        public ObservableCollection<SubNode> Nodes
        {
            get
            {
                return this;
            }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }


        public SubNode(string name)
        {
            _name = name;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
