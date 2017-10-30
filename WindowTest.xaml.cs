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
using System.Drawing.Printing;
using UI = Styx.Properties.UserInterface;


namespace Styx
{
    /// <summary>
    /// Interaction logic for WindowTest.xaml
    /// </summary>
    public partial class WindowTest : Window
    {
        Constants constants = new Constants();///to get predefined graphical attributes
        

        public WindowTest()
        {
            InitializeComponent();     

        }

 

        private void dataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
                       
            
            List<Tuple<string, double>> list_just_selected = new List<Tuple<string, double>>(e.AddedItems.Cast<Tuple<string, double>>());
            List<Tuple<string, double>> list_just_unselected = new List<Tuple<string, double>>(e.RemovedItems.Cast<Tuple<string, double>>());
            MainWindow mainWindow = (MainWindow)this.Owner;
                      
           

            //highlight just selected
            foreach (Tuple<string, double> tuple in list_just_selected)
            {
                Node node = mainWindow.waterNetwork.listOfNodes.Find(tmp => tmp.name == tuple.Item1);
                mainWindow.rectangleHitList.Add(node.graphicalObject);
                mainWindow.ChangeNodeApperance(node, mainWindow.ToBrush(UI.Default.SelectionNodeColor), UI.Default.SelectionNodeSize);
            }

            //unhighlight just selected
            foreach (Tuple<string, double> tuple in list_just_unselected)
            {
                Node node = mainWindow.waterNetwork.listOfNodes.Find(tmp => tmp.name == tuple.Item1);
                mainWindow.rectangleHitList.Remove(node.graphicalObject);
                mainWindow.ChangeNodeApperance(node, mainWindow.ToBrush(UI.Default.StandardNodeColor), UI.Default.StandardNodeSize);
            }
            int error = mainWindow.DrawWaterNetwork();
            
        }

        /// <summary>
        /// Prints content of datagrid1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
            //DP: 19/07/2013 - Button_Click was rewritten to account for Bug 20
            // Create the parent FlowDocument...
            FlowDocument flowDoc = new FlowDocument();

            // Create the Table...
            Table table1 = new Table();
            // ...and add it to the FlowDocument Blocks collection.
            flowDoc.Blocks.Add(table1);
            
            // Set some global formatting properties for the table.
            table1.CellSpacing = 10;
            table1.Background = Brushes.White;

            // Get datagrid elements to array
            var dataGridElements = dataGrid1.Items;
            Tuple<string, double>[] elementsArray = new Tuple<string, double>[dataGridElements.Count];          
            dataGridElements.CopyTo(elementsArray, 0);
            
           
            // Create 2 columns and add them to the table's Columns collection. 
            int numberOfColumns = 2;
            for (int x = 0; x < numberOfColumns; x++)
            {
                table1.Columns.Add(new TableColumn());
                // Set alternating background colors for the middle colums. 
                if (x % 2 == 0)
                    table1.Columns[x].Background = Brushes.Beige;
                else
                    table1.Columns[x].Background = Brushes.LightSteelBlue;
            }

            // Create and add an empty TableRowGroup to hold the table's Rows.
            table1.RowGroups.Add(new TableRowGroup());

            // Add the first (title) row.
            table1.RowGroups[0].Rows.Add(new TableRow());

            // Alias the current working row for easy reference.
            TableRow currentRow = table1.RowGroups[0].Rows[0];

            // Global formatting for the title row.
            currentRow.Background = Brushes.Silver;
            currentRow.FontSize = 40;
            currentRow.FontWeight = System.Windows.FontWeights.Bold;

            // Add the header row with content, 
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Burst Localisation"))));
            // and set the row to span all 2 columns.
            currentRow.Cells[0].ColumnSpan = 2;

            // Add the second (headers) row.
            table1.RowGroups[0].Rows.Add(new TableRow());
            currentRow = table1.RowGroups[0].Rows[1];

            // Global formatting for the header row.
            currentRow.FontSize = 18;
            currentRow.FontWeight = FontWeights.Bold;

            // Add cells with content to the second row.
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Node"))));
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Fit Index"))));
            
            for (int i =0; i< dataGridElements.Count; i++)
            {
                table1.RowGroups[0].Rows.Add(new TableRow());
                currentRow = table1.RowGroups[0].Rows[2+i];

                // Global formatting for the row.
                currentRow.FontSize = 12;
                currentRow.FontWeight = FontWeights.Normal;
                
                // Add cells with content to the third row.
                currentRow.Cells.Add(new TableCell(new Paragraph(new Run(elementsArray[i].Item1  ))));
                currentRow.Cells.Add(new TableCell(new Paragraph(new Run(elementsArray[i].Item2.ToString("F3")))));
            }

            // Create print dialog 
            PrintDialog dialog = new PrintDialog();             
            IDocumentPaginatorSource dps = flowDoc;           

            // Show dialog window
             var result = dialog.ShowDialog();
             if (result.HasValue && result.Value)
                {
                    dialog.PrintDocument(dps.DocumentPaginator, "Burst localisation");
                   
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            } 

        }

    }
}
