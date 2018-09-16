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
using OxyPlot;
using OxyPlot.Series;
using System.Collections.Generic;
using OxyPlot.Axes;
using System.IO;


namespace Styx
{
    /// <summary>
    /// Interaction logic for czart.xaml
    /// </summary>
    public partial class czart : Window
    {
        public Favad1.favadresults Clasa { get; set; }
        List<Favad1.favadresults> rekord { get; set; }
        public Favad1 favad { get; set; }
        public WaterNetwork waterNetwork { get; set; }
        public BurstCoeffs bust { get; set; }
        //public List<Favad1.comparative_results> comparative { get; set; }
        public Favad1.favadcoefficients favadcoeff { get; set; }
        public Favad1.difference diff{get;set;}
        public czart(List<Favad1.favadresults> data,WaterNetwork waterNetwork,BurstCoeffs burst,Favad1.favadcoefficients favadcoeff, Favad1.difference diff)
        {

           
            this.InitializeComponent();
            //this.Model = CreateNormalDistributionModel();
            this.DataContext = this;
            this.rekord = data;
            this.waterNetwork = waterNetwork;
            this.bust = burst;
            this.diff = diff;
            this.Model = CreateNormalDistributionModel(data,waterNetwork,favadcoeff,burst);
            this.ShowResults(data,burst,diff);

        }
        
        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        public PlotModel Model { get; set; }
        public Grid grid { get; set; }



        private int ShowResults(List<Favad1.favadresults> data1, BurstCoeffs burst, Favad1.difference diff)
        {

            var hig = 40;
            data1.Reverse();
            ColumnDefinition colDef1 = new ColumnDefinition();
            ColumnDefinition colDef2 = new ColumnDefinition();
            ColumnDefinition colDef3 = new ColumnDefinition();
            ColumnDefinition colDef4 = new ColumnDefinition();
            ColumnDefinition colDef5 = new ColumnDefinition();
            ColumnDefinition colDef6 = new ColumnDefinition();
            FavadResultsGrid.ColumnDefinitions.Add(colDef1);
            FavadResultsGrid.ColumnDefinitions.Add(colDef2);
            FavadResultsGrid.ColumnDefinitions.Add(colDef3);
            FavadResultsGrid.ColumnDefinitions.Add(colDef4);
            FavadResultsGrid.ColumnDefinitions.Add(colDef5);
            FavadResultsGrid.ColumnDefinitions.Add(colDef6);
            for (int i = 0; i < data1.Count; i++)
            {
                FavadResultsGrid.RowDefinitions.Add(new RowDefinition());
            }
            //create headers
            TextBlock column0Header = new TextBlock();
            column0Header.Text = "PRV Step No.";
            column0Header.Height = hig;
            column0Header.Width = 150;
            column0Header.TextWrapping = TextWrapping.Wrap;
            column0Header.TextAlignment = TextAlignment.Left;
            column0Header.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            column0Header.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            Grid.SetColumn(column0Header, 0);
            Grid.SetRow(column0Header, 0);
            FavadResultsGrid.Children.Add(column0Header);

            TextBlock column1Header = new TextBlock();
            column1Header.Text = "Burst flow FAVAD N1 Update";
            column1Header.Height = hig;
            column1Header.Width = 150;
            column1Header.TextWrapping = TextWrapping.Wrap;
            column1Header.TextAlignment = TextAlignment.Left;
            column1Header.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            column1Header.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            Grid.SetColumn(column1Header, 1);
            Grid.SetRow(column1Header, 0);

            FavadResultsGrid.Children.Add(column1Header);

            TextBlock column2Header = new TextBlock();
            column2Header.Text = "AZNP";
            column2Header.Height = hig;
            column2Header.Width = 50;
            column2Header.TextWrapping = TextWrapping.Wrap;
            column2Header.TextAlignment = TextAlignment.Left;
            column2Header.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            column2Header.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            Grid.SetColumn(column2Header, 2);
            Grid.SetRow(column2Header, 0);

            FavadResultsGrid.Children.Add(column2Header);

            TextBlock column3Header = new TextBlock();
            column3Header.Text = "Flow from Burst Detectror";
            column3Header.Height = hig;
            column3Header.Width = 200;
            column0Header.TextWrapping = TextWrapping.Wrap;
            column3Header.TextAlignment = TextAlignment.Left;
            column3Header.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            column3Header.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            Grid.SetColumn(column3Header, 3);
            Grid.SetRow(column3Header, 0);

            FavadResultsGrid.Children.Add(column3Header);

            TextBlock column4Header = new TextBlock();
            column4Header.Text = "Mean difference Favad";
            column4Header.Height = hig;
            column4Header.Width = 150;
            column4Header.TextWrapping = TextWrapping.Wrap;
            column4Header.TextAlignment = TextAlignment.Left;
            column4Header.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            column4Header.VerticalAlignment = System.Windows.VerticalAlignment.Center;/09
            Grid.SetColumn(column4Header, 4);
            Grid.SetRow(column4Header, 0);
            FavadResultsGrid.Children.Add(column4Header);

            System.Windows.Controls.TextBox favad_diff = new System.Windows.Controls.TextBox();
            favad_diff = createTexblock1(diff.favad);
            Grid.SetColumn(favad_diff, 4);
            Grid.SetRow(favad_diff, 1);
            FavadResultsGrid.Children.Add(favad_diff);
            //test
            TextBlock column5Header = new TextBlock();
            column5Header.Text = "Mean difference Skoworcow";
            column5Header.Height = hig;
            column5Header.Width = 150;
            column5Header.TextWrapping = TextWrapping.Wrap;
            column5Header.TextAlignment = TextAlignment.Left;
            column5Header.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            column5Header.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            Grid.SetColumn(column5Header, 5);
            Grid.SetRow(column5Header, 0);
            FavadResultsGrid.Children.Add(column5Header);

            System.Windows.Controls.TextBox piotr_diff = new System.Windows.Controls.TextBox();
            piotr_diff = createTexblock1(diff.skoworcow);
            Grid.SetColumn(piotr_diff, 5);
            Grid.SetRow(piotr_diff, 1);
            FavadResultsGrid.Children.Add(piotr_diff);

            for (int i = 0; i < data1.Count(); i++)
            {

                System.Windows.Controls.Label prvStepNo = new System.Windows.Controls.Label();
                prvStepNo = createLabel(i.ToString(), i + 1);

                System.Windows.Controls.TextBox burstFlow = new System.Windows.Controls.TextBox();
                burstFlow = createTexblock1(data1[i].Flow1);
                //FavadResultsGrid.RowDefinitions[i].Height = new GridLength(20);
                System.Windows.Controls.TextBox AZNP1 = new System.Windows.Controls.TextBox();
                AZNP1 = createTexblock1(data1[i].AZNP1);

                System.Windows.Controls.TextBox burstdet = new System.Windows.Controls.TextBox();
                burstdet = createTexblock1(burst.est_burst_flow[i]);
                Grid.SetColumn(prvStepNo, 0);
                Grid.SetRow(prvStepNo, i + 1);

                Grid.SetColumn(burstFlow, 1);
                Grid.SetRow(burstFlow, i + 1);

                Grid.SetColumn(AZNP1, 2);
                Grid.SetRow(AZNP1, i + 1);

                Grid.SetColumn(burstdet, 3);
                Grid.SetRow(burstdet, i + 1);
                
                FavadResultsGrid.Children.Add(prvStepNo);
                FavadResultsGrid.Children.Add(burstFlow);
                FavadResultsGrid.Children.Add(AZNP1);
                FavadResultsGrid.Children.Add(burstdet);
            }
            return 0;
        }

        private System.Windows.Controls.TextBox createTexblock1(double value)
        {
            System.Windows.Controls.TextBox t = new System.Windows.Controls.TextBox();
            
            t.Text = value.ToString("f3");
            t.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            t.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            //b.Click += new EventHandler(Button_Click);
            //b.OnClientClick = "ButtonClick('" + b.ClientID + "')";
            return t;
        }
        private System.Windows.Controls.TextBox createTexblock(string ID, double value)
        {
            System.Windows.Controls.TextBox t = new System.Windows.Controls.TextBox();
            t.Name = "burstTextBlock" + ID;
            t.Uid = "burstTextBlock" + ID;
            t.Text = value.ToString("f3");
            t.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            t.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            //b.Click += new EventHandler(Button_Click);
            //b.OnClientClick = "ButtonClick('" + b.ClientID + "')";
            return t;
        }
        private System.Windows.Controls.Label createLabel(string ID, int value)
        {
            System.Windows.Controls.Label l = new System.Windows.Controls.Label();
            l.Name = "burstLabel" + ID;
            l.Uid = "burstLabel" + ID;
            l.Content = value.ToString();
             
            l.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            l.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            //b.Click += new EventHandler(Button_Click);
            //b.OnClientClick = "ButtonClick('" + b.ClientID + "')";
            return l;

        }
        /// <summary>
        /// Creates a model showing normal distributions.
        /// </summary>
        /// <returns>A PlotModel.</returns>
        public PlotModel model = new PlotModel();
        private PlotModel CreateNormalDistributionModel(List<Favad1.favadresults> dane, WaterNetwork waterNetwork,Favad1.favadcoefficients favadcoeff, BurstCoeffs burst)
        {
            model = new PlotModel { Title = "Burst estimation results" };
            //model.MouseDown += (s, e) => { MyModel_MouseDown(model, e); };
            Func<double, double> batFn1 = (x) => burst.est_burst_coeff * Math.Pow(x, burst.est_burst_exponent);
            Func<double, double> batFn2 = (x) => favadcoeff.A * Math.Pow(x, 0.5) + favadcoeff.B * Math.Pow(x, 1.5);

            var nodes = waterNetwork.listOfNodes;
            var emmiter = nodes.Where(s => s.emmitterCoefficient != 0).First();

            Func<double, double> bat1Fn3 = (x) => emmiter.emmitterCoefficient * Math.Pow(x, waterNetwork.emitterExponent);
            Func<double, double> BatFn4 = (x) => favadcoeff.A * Math.Pow(x, 0.5);
            Func<Double, double> batFn5 = (x) => dane[0].Flow1 * Math.Pow((x / dane[0].AZNP0), dane[0].N1);
            model.Series.Add(new FunctionSeries(batFn1, 0, 60, 0.01, "Piotr method"));
            model.Series.Add(new FunctionSeries(batFn2, 0, 60, 0.01, "Favad method"));
            model.Series.Add(new FunctionSeries(bat1Fn3, 0, 60, 0.01, "Real Emitter Parameters"));
            model.Series.Add(new FunctionSeries(BatFn4, 0, 60, 0.01, "Fixed part FAVAD method"));
            model.Series.Add(new FunctionSeries(batFn5, 0, 60, 0.01, "Fixed part FAVAD N1 update method"));

            model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, MaximumPadding = 0.1, MinimumPadding = 0.1, Title = "Pressure [m]" });
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, MaximumPadding = 0.1, MinimumPadding = 0.1, Title = "Burst Flow [m^3/h]" });
            return model;

        }

        private static void MyModel_MouseDown(PlotModel model, OxyMouseEventArgs e)
        {
            model.ZoomAllAxes(2);
            System.Windows.MessageBox.Show(e.Position.X.ToString());
            var plotModel = model as PlotModel;
        }
        private void openFile_Click(object sender, RoutedEventArgs e)
        {
            var appPath = System.AppDomain.CurrentDomain.BaseDirectory;
            System.Diagnostics.Process.Start(appPath + "F100.pdf");
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            
            dlg.DefaultExt = ".pdf"; //default file extension
            dlg.Filter = "pdf documents (.pdf)|*.pdf"; //filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            
                // Save document
                
            
            {
                using (var stream = File.Create(dlg.FileName))
                { 
                    var pdfExporter = new PdfExporter { Width = 600, Height = 400 };
                    pdfExporter.Export(model, stream);
                }
            }
            
        }
    }
}

