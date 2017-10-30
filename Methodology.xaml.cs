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
using System.Reflection;
using System.IO;

namespace Styx
{
    /// <summary>
    /// Interaction logic for Instruction.xaml
    /// </summary>
    public partial class Methodology : Window
    {
        public Methodology()
        {
            InitializeComponent();
        }

        private void methodologyRTF_Loaded(object sender, RoutedEventArgs e)
        {
            TextRange textRange;
            //System.IO.FileStream fileStream;

            var resourceName = "Styx.Resources.BurstMethodology.rtf";
            Assembly assembly = Assembly.GetExecutingAssembly();

            Stream stream = assembly.GetManifestResourceStream(resourceName);
            StreamReader reader = new StreamReader(stream);

            textRange = new TextRange(methodologyRTF.Document.ContentStart, methodologyRTF.Document.ContentEnd);
            textRange.Load(stream, System.Windows.DataFormats.Rtf);


            //if (System.IO.File.Exists(Properties.Settings.Default.path + "rtftest.rtf"))
            //{
            //    textRange = new TextRange(methodologyRTF.Document.ContentStart, methodologyRTF.Document.ContentEnd);




            //    using (fileStream = new System.IO.FileStream(Properties.Settings.Default.path + "rtftest.rtf", System.IO.FileMode.OpenOrCreate))
            //    {
            //        textRange.Load(fileStream, System.Windows.DataFormats.Rtf);
            //    }
            //}

        }
    }
}
