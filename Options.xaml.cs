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
using Styx.Properties;

namespace Styx
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();
        
            PropertyGridOptions.SelectedObject = UserInterface.Default;                    
        }

        /// <summary>
        /// Reset settings to defaults
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetUISettings_Click(object sender, RoutedEventArgs e)
        {
                
            UserInterface.Default.Reset();
            PropertyGridOptions.Refresh();//refresh property grid
        }

        /// <summary>
        /// Converts SolidColorBrush to Drawing.Color
        /// </summary>
        /// <param name="oBrush"></param>
        /// <returns></returns>
        private System.Drawing.Color GetColor(System.Windows.Media.SolidColorBrush oBrush)
        {
            return System.Drawing.Color.FromArgb(oBrush.Color.A,
                                              oBrush.Color.R,
                                              oBrush.Color.G,
                                              oBrush.Color.B);
        }

        /// <summary>
        /// Perfroms data updates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optionsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            MainWindow mw = (MainWindow)this.Owner;

            try
            {
                //update advance options
                AdvancedOptions.head_diff_tolerance = UserInterface.Default.HeadDifferenceTolerance;
                AdvancedOptions.loggerNeighbourhoodLevel = UserInterface.Default.LoggerNeighbourhoodLevel;
                AdvancedOptions.lsqr_estimation_iterations = UserInterface.Default.LeastSquareEstimationIterations;
                AdvancedOptions.lsqr_estimation_tolerance = UserInterface.Default.LeastSquareEstimationTolerance;
                AdvancedOptions.max_diff_from_min_chi2_percent = UserInterface.Default.MaximumDifferenceFromMinChi2Percentage;
                AdvancedOptions.max_higher_level_neighbours = UserInterface.Default.MaximumForHigherLevelNeighbours;
                AdvancedOptions.max_no_min_chi2 = UserInterface.Default.MaximumNumberOfMinChi2;
                AdvancedOptions.zero_d2h_tolerance = UserInterface.Default.ZeroD2HTtolerance;
                AdvancedOptions.zero_flow_tolerance = UserInterface.Default.ZeroFlowTolerance;


                //update colors and size water network objects
                if (mw.waterNetwork != null)
                {
                    foreach (Node n in mw.waterNetwork.listOfNodes)
                    {
                        n.size = UserInterface.Default.StandardNodeSize;
                        n.color = mw.ToBrush(UserInterface.Default.StandardNodeColor);
                    }
                    foreach (Link l in mw.waterNetwork.listOfLinks)
                    {
                        l.color = mw.ToBrush(UserInterface.Default.StandardLinkColor);
                        l.thickness = UserInterface.Default.StandardLinkThickness;
                    }
                }

                if (mw.efavorTest == null)
                {
                    if ((mw.loggerConnections != null) && (mw.loggerConnections.logger_water_network != null))
                    {
                        foreach (Node n in mw.loggerConnections.logger_water_network.listOfNodes)
                        {
                            n.size = UserInterface.Default.LoggerNodeSize;
                            n.color = mw.ToBrush(UserInterface.Default.LoggerNodeColor);
                        }
                        foreach (Link l in mw.loggerConnections.logger_water_network.listOfLinks)
                        {
                            l.color = mw.ToBrush(UserInterface.Default.LoggerLinkColor);
                            l.thickness = UserInterface.Default.LoggerLinkThickness;
                        }
                    }
                }

                if (mw.pathsNetwork != null)
                {
                    foreach (Node n in mw.pathsNetwork.listOfNodes)
                    {
                        n.size = UserInterface.Default.PathNodeSize;
                        n.color = mw.ToBrush(UserInterface.Default.PathNodeColor);
                    }
                    foreach (Link l in mw.pathsNetwork.listOfLinks)
                    {
                        l.color = mw.ToBrush(UserInterface.Default.PathLinkColor);
                        l.thickness = UserInterface.Default.PathLinkThickness;
                    }

                }

                //unselect all elements 
                mw.unselectAllContextMenu_Click(null, null);

                //redraw network with new colors
                if (mw.waterNetwork.listOfNodes.Count>0)
                {
                    int error = mw.DrawWaterNetwork();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when closing options window."+ ex.Message);
                return;
            }
        }


 
    }
}
