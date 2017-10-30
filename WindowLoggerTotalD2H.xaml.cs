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
using UI = Styx.Properties.UserInterface;

namespace Styx
{
    /// <summary>
    /// Interaction logic for WindowLoggerTotalD2H.xaml
    /// </summary>
    public partial class WindowLoggerTotalD2H : Window
    {
        public WindowLoggerTotalD2H()
        {
            InitializeComponent();
        }

        public Brush ToBrush(System.Drawing.Color color)
        {
            return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        /// <summary>!!!relies on specific coding of logger connection names!!!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<Tuple<string, double, double>> listJustSelected = new List<Tuple<string, double, double>>(e.AddedItems.Cast<Tuple<string, double, double>>());
            List<Tuple<string, double, double>> listJustUnselected = new List<Tuple<string, double, double>>(e.RemovedItems.Cast<Tuple<string, double, double>>());
            MainWindow mainWindow = (MainWindow)this.Owner;
            LoggerConnections loggerConnections = mainWindow.loggerConnections;
            int error;
            if (loggerConnections.logger_water_network == null)  //check if logger_water_network exists (it should at this stage!)
            {
                error = loggerConnections.LoggerConn2WaterNet(mainWindow.efavorTest, mainWindow.inletSetSelectionComboBox.SelectedValue.ToString(), true);
                loggerConnections.logger_water_network.isPlotted = true;
                error = mainWindow.DrawWaterNetwork();
            }

            //unselected:
            foreach (Tuple<string, double, double> currentTuple in listJustUnselected)
            {
                string loggerName = currentTuple.Item1;
                int loggerIndex = loggerConnections.list_of_loggers.FindIndex(tmp => tmp.logger_id == loggerName);
                if (loggerIndex < 0)
                {
                    MessageBox.Show("Can't find logger " + loggerName);
                    return;
                }
                //connections outgoing from this logger:
                for (int i = 0; i < loggerConnections.list_of_loggers.Count; i++)
                {
                    if (loggerConnections.logger_connection_matrix[loggerIndex, i] == true)
                    {
                        Link linkTemp = loggerConnections.logger_water_network.listOfLinks.Find(tmp => tmp.name == "L" + loggerName + "-" + loggerConnections.list_of_loggers[i].logger_id);
                        if (linkTemp == null)
                        {
                            MessageBox.Show("Can't find connection from logger " + loggerName + " to logger " + loggerConnections.list_of_loggers[i].logger_id);
                            return;
                        }
                        var alreadyInList = mainWindow.pathHitList.Find(tmp => tmp.Tag == linkTemp);// make sure that this logger connection is already in pathHitList
                        if (alreadyInList != null) //add if yes unselect it
                        {
                            mainWindow.ChangeLinkApperance(linkTemp, linkTemp.color, linkTemp.thickness - UI.Default.SelectionLinkThickness);
                            mainWindow.pathHitList.Remove(alreadyInList);
                            //MessageBox.Show(linkTemp.name + " unselected");
                        }
                    }
                }
                //connections incoming to this logger:
                for (int i = 0; i < loggerConnections.list_of_loggers.Count; i++)
                {
                    if (loggerConnections.logger_connection_matrix[i, loggerIndex] == true)
                    {
                        Link linkTemp = loggerConnections.logger_water_network.listOfLinks.Find(tmp => tmp.name == "L" + loggerConnections.list_of_loggers[i].logger_id + "-" + loggerName);
                        if (linkTemp == null)
                        {
                            MessageBox.Show("Can't find connection from logger " + loggerConnections.list_of_loggers[i].logger_id + " to logger " + loggerName);
                            return;
                        }
                        var alreadyInList = mainWindow.pathHitList.Find(tmp => tmp.Tag == linkTemp);// make sure that this logger connection is already in pathHitList
                        if (alreadyInList != null) //add if yes unselect it
                        {
                            mainWindow.ChangeLinkApperance(linkTemp, linkTemp.color, linkTemp.thickness - UI.Default.SelectionLinkThickness);
                            mainWindow.pathHitList.Remove(alreadyInList);
                            //MessageBox.Show(linkTemp.name + " unselected");
                        }
                    }
                }
            }

            //just selected:
            foreach (Tuple<string, double, double> currentTuple in listJustSelected)
            {                
                string loggerName = currentTuple.Item1;
                int loggerIndex = loggerConnections.list_of_loggers.FindIndex(tmp => tmp.logger_id == loggerName);
                if (loggerIndex < 0)
                {
                    MessageBox.Show("Can't find logger " + loggerName);
                    return;
                }
                //connections outgoing from this logger:
                for (int i = 0; i < loggerConnections.list_of_loggers.Count; i++)
                {
                    if (loggerConnections.logger_connection_matrix[loggerIndex, i] == true)
                    {
                        Link linkTemp = loggerConnections.logger_water_network.listOfLinks.Find(tmp => tmp.name == "L" + loggerName + "-" + loggerConnections.list_of_loggers[i].logger_id);
                        if (linkTemp == null)
                        {
                            MessageBox.Show("Can't find connection from logger " + loggerName + " to logger " + loggerConnections.list_of_loggers[i].logger_id);
                            return;
                        }
                        var alreadyInList = mainWindow.pathHitList.Find(tmp=> tmp.Tag == linkTemp);// check whether logger connection is already in pathHitList
                        if (alreadyInList == null) //add if not
                        {
                            mainWindow.ChangeLinkApperance(linkTemp, linkTemp.color, linkTemp.thickness + UI.Default.SelectionLinkThickness);
                            mainWindow.pathHitList.Add((System.Windows.Shapes.Path)linkTemp.graphicalObject);
                            //MessageBox.Show(linkTemp.name + " selected");
                        }                           
                    }
                }
                //connections incoming to this logger:
                for (int i = 0; i < loggerConnections.list_of_loggers.Count; i++)
                {
                    if (loggerConnections.logger_connection_matrix[i, loggerIndex] == true)
                    {
                        Link linkTemp = loggerConnections.logger_water_network.listOfLinks.Find(tmp => tmp.name == "L" + loggerConnections.list_of_loggers[i].logger_id + "-" + loggerName);
                        if (linkTemp == null)
                        {
                            MessageBox.Show("Can't find connection from logger " + loggerConnections.list_of_loggers[i].logger_id + " to logger " + loggerName);
                            return;
                        }
                        var alreadyInList = mainWindow.pathHitList.Find(tmp => tmp.Tag == linkTemp);// check whether logger connection is already in pathHitList
                        if (alreadyInList == null) //add if not
                        {
                            mainWindow.ChangeLinkApperance(linkTemp, linkTemp.color, linkTemp.thickness + UI.Default.SelectionLinkThickness);
                            mainWindow.pathHitList.Add((System.Windows.Shapes.Path)linkTemp.graphicalObject);
                            //MessageBox.Show(linkTemp.name + " selected");
                        }
                    }
                }
            }

        }
    }
}
