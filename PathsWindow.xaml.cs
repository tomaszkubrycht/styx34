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
    /// Interaction logic for PathsWindow.xaml
    /// </summary>
    public partial class PathsWindow : Window
    {
        Constants constants = new Constants();///to get predefined graphical attributes
                                           
        public PathsWindow()
        {
            InitializeComponent();
            multiSelectTreeView.treeView1.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(selectionChanged); ///hook event when treeview items collection changed
            multiSelectTreeView.closeButton.Click += new RoutedEventHandler(closeButton_Click);///hook event to close button click
        }

        /// <summary>
        /// Handling event of close button click 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void closeButton_Click(object sender, RoutedEventArgs e)
        {
            pathsWindow.Close();
        }

    

        /// <summary>
        /// Handling event of TreeView item collection change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void selectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            MainWindow mainWindow = (MainWindow)this.Owner;

            try
            {
                mainWindow.pathsNetwork.listOfNodes.Clear();///clear pathNetwork lists
                mainWindow.pathsNetwork.listOfLinks.Clear();

                foreach (object item in multiSelectTreeView.treeView1.SelectedItems)
                {
                    string loggerConnectionName;
                    int pathIndex;


                    string[] split = ((SubNode)item).Name.Split(new Char[] { ':' });///rertrive logger connection name and path index of selected paths

                    if (split.Length == 3) /// to ensure that loggerConnection and pathindex can be retreived
                    {
                        loggerConnectionName = split[0];
                        pathIndex = Convert.ToInt32(split[2]);
                        
                        //find selected  loggerConnection
                        LoggerPaths loggerConnection = mainWindow.listOfAllLoggersPaths.Find(tmp => tmp.loggerConnectionName == loggerConnectionName);
                        if (loggerConnection != null)
                        {
                            //add path object to mainWindow pathNetwork object
                            foreach (Node node in loggerConnection.paths[pathIndex].listOfNodes)
                            {
                                //add the node to the list only if it doesn't exist in the list 
                                if (!mainWindow.pathsNetwork.listOfNodes.Exists(tmp => tmp == node))
                                {
                                    Node newNode = new Node(node);
                                    newNode.isPathNode = true;
                                    mainWindow.ChangeNodeApperance(newNode, mainWindow.ToBrush(UI.Default.PathNodeColor), UI.Default.PathNodeSize);
                                    mainWindow.pathsNetwork.listOfNodes.Add(newNode);
                                }
                            }

                            //PS: 16/07/2013: in MainWindow.xaml.cs instead of adding link from main waterNetwork create a new link for given nodes From and To; 
                            //below change is reflecting this change in MainWindow.xaml.cs
                            foreach (Link link in loggerConnection.paths[pathIndex].listOfLinks)
                            {
                                link.nodeFrom = mainWindow.pathsNetwork.listOfNodes.FindIndex(tmp => tmp.name == link.nodeFromName);
                                link.nodeTo = mainWindow.pathsNetwork.listOfNodes.FindIndex(tmp => tmp.name == link.nodeToName);
                                mainWindow.ChangeLinkApperance(link, mainWindow.ToBrush(UI.Default.PathLinkColor), UI.Default.PathLinkThickness);
                                mainWindow.pathsNetwork.listOfLinks.Add(link);
                                //Link newLink = new Link(link);

                                //int nodeFrom = mainWindow.pathsNetwork.listOfNodes.FindIndex(tmp => tmp.name == mainWindow.waterNetwork.listOfNodes.ElementAt(link.nodeFrom).name);
                                //int nodeTo = mainWindow.pathsNetwork.listOfNodes.FindIndex(tmp => tmp.name == mainWindow.waterNetwork.listOfNodes.ElementAt(link.nodeTo).name);

                                //newLink.nodeFrom = nodeFrom;
                                //newLink.nodeTo = nodeTo;
                                //mainWindow.ChangeLinkApperance(newLink, constants.pathLinkColor, constants.pathLinkThickness);
                                //mainWindow.pathsNetwork.listOfLinks.Add(newLink);
                            }

                        
                        }
                        else
                        {
                            throw new Exception("Selected logger connection not found.");
                        }
                    }
                }
                int error = mainWindow.DrawWaterNetwork();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            
        }
        /// <summary>
        /// To ensure that pathsNetwork object is empty and remove paths from mainWindow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)this.Owner;

            mainWindow.pathsNetwork = null;

            int error = mainWindow.DrawWaterNetwork();
        }



    }
}
