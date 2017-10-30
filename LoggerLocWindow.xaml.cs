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
using System.IO;
using Microsoft.Win32;
using GemBox.Spreadsheet; //Maximum number of rows per sheet is 150. Maximum number of sheets per workbook is 5.
using System.ComponentModel;
using UI = Styx.Properties.UserInterface;

namespace Styx
{
    
    /// <summary>
    /// Interaction logic for LoggerLocWindow.xaml
    /// </summary>
    public partial class LoggerLocWindow : Window
    {
        List<Node> inletNodes;
        Constants constants = new Constants();///to get predefined graphical attributes
        public List<Node> allowedLoggerLocations; ///list of nodes where loggers can be placed                                              
        private double _nNeighboursWeight; ///weight defining importance of number of neighbours (nodes), when loggers are automatically allocated
        private double _flowWeight; ///weight defining importance of average flow through connected pipes, when loggers are automatically allocated
        private double _spreadWeight; ///weight defining importance of spread of loggers, when loggers are automatically allocated; when new candidate hydrant (node) is evaluated spread is calculated as minimum distance (in terms of delta_h) to any existing logger 
        private int _desiredNLoggers; /// number of loggers to be allocated by automatic allocation algorithm 
        public string nNeighboursWeight
        {
            get
            {
                return (_nNeighboursWeight.ToString());
            }
            set
            {
                if (!double.TryParse(value, out _nNeighboursWeight))
                {
                    _nNeighboursWeight = 1;
                    MessageBox.Show(value + " is not a numerical value!");
                }
            }
        }
        public string flowWeight
        {
            get
            {
                return (_flowWeight.ToString());
            }
            set
            {
                if (!double.TryParse(value, out _flowWeight))
                {
                    _flowWeight = 1;
                    MessageBox.Show(value + " is not a numerical value!");
                }
            }
        }
        public string spreadWeight 
        {
            get
            {
                return (_spreadWeight.ToString());
            }
            set
            {
                if (!double.TryParse(value, out _spreadWeight))
                {
                    _spreadWeight = 1;
                    MessageBox.Show(value + " is not a numerical value!");
                }
            }
        }
        public string desiredNLoggers
        {
            get
            {
                return (_desiredNLoggers.ToString());
            }
            set
            {
                if (!int.TryParse(value, out _desiredNLoggers))
                {
                    _desiredNLoggers = 20;
                    MessageBox.Show(value + " is not an integer value!");
                }
            }
        }
        public string loggerNeighbourhoodLevel
        {
            get
            {
                return (AdvancedOptions.loggerNeighbourhoodLevel.ToString());
            }
            set
            {
                if (!int.TryParse(value, out AdvancedOptions.loggerNeighbourhoodLevel))
                {
                    AdvancedOptions.loggerNeighbourhoodLevel = UI.Default.LoggerNeighbourhoodLevel;
                    MessageBox.Show(value + " is not an integer value!");
                }
                if ((AdvancedOptions.loggerNeighbourhoodLevel > AdvancedOptions.max_higher_level_neighbours) || (AdvancedOptions.loggerNeighbourhoodLevel < 0))
                {
                    AdvancedOptions.loggerNeighbourhoodLevel = UI.Default.LoggerNeighbourhoodLevel;
                    MessageBox.Show("Logger neighbourhood level must be between 0 and " + AdvancedOptions.max_higher_level_neighbours.ToString() + ". Allowed maximum can be changed in advanced options but it is not recommended. ");
                }
            }
        }
        public string headDiffTolerance
        {
            get
            {
                return (AdvancedOptions.head_diff_tolerance.ToString());
            }
            set
            {
                if (!double.TryParse(value, out AdvancedOptions.head_diff_tolerance))
                {
                    AdvancedOptions.head_diff_tolerance = UI.Default.HeadDifferenceTolerance;
                    MessageBox.Show(value + " is not an integer value!");
                }
                if (AdvancedOptions.head_diff_tolerance < 0)
                {
                    AdvancedOptions.head_diff_tolerance = UI.Default.HeadDifferenceTolerance;
                    MessageBox.Show("Head difference tolerance must be greater than zero.");
                }

            }
        }
            
        public LoggerLocWindow(List<Node> inlet_nodes)
        {
            this.inletNodes = inlet_nodes;
            nNeighboursWeight = "1";
            flowWeight = "1";
            spreadWeight = "1";
            desiredNLoggers = "20";
            //don't need to set loggerNeighbourhoodLevel and headDiffTolerance, they will be read automatically from AdvancedOptions
            InitializeComponent();            
            //this.DataContext = new LoggerLocParameters() { nNeighboursWeight = "666" };
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)this.Owner;
            mainWindow.unselectAllContextMenu_Click(sender, new RoutedEventArgs());
            mainWindow.singleSelection.IsEnabled = true;
            mainWindow.multipleSelection.IsEnabled = true;
            mainWindow.singleSelection.IsChecked = false;
            mainWindow.button_LoggerLocTool.IsEnabled = true;
            mainWindow.Button_loadEfavor.IsEnabled = true;
            mainWindow.placingLoggersTool = false;
        }

        private void button_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>Initialises pre-selected nodes, use after this.Owner has been set 
        /// </summary>
        public void Initialise()
        {
            try
            {
                MainWindow mainWindow = (MainWindow)this.Owner;
                mainWindow.button_LoggerLocTool.IsEnabled = false;
                mainWindow.Button_loadEfavor.IsEnabled = false;
                mainWindow.checkbox_loggerNetworkVisibility.IsChecked = false;
                allowedLoggerLocations = new List<Node>(mainWindow.waterNetwork.listOfNodes); //initially assume all nodes can be used as logger locations
                if ((inletNodes != null) && (inletNodes.Count > 0))
                {
                    foreach (Node node in inletNodes)
                    {
                        mainWindow.rectangleHitList.Add(node.graphicalObject);
                        mainWindow.ChangeNodeApperance(node, mainWindow.ToBrush(UI.Default.SelectionNodeColor), UI.Default.SelectionNodeSize);
                    }
                    MessageBox.Show("Loggers were automatically allocated to the outlets of PRVs");
                    textBox_nAllocatedLog.Text = mainWindow.rectangleHitList.Count.ToString();
                }
                DefaultProhibitedLocations();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while initialising logger location tool window: " + ex.Message);
                return;
            }
        }

        /// <summary>Defines allowedLoggerLocations by taking all water network nodes and eliminating all disconnected nodes and all nodes upstream from PRV outlets; analysis is done using flowtree
        /// </summary>
        private void DefaultProhibitedLocations()
        {
            MainWindow mainWindow = (MainWindow)this.Owner;
            //Generate flow tree for the loaded waterNetwork and inlets specified above                
            FlowTree flowTree = new FlowTree(mainWindow.waterNetwork, inletNodes);
            int ret_val = flowTree.GenerateFlowTree(); 
            if (ret_val < 0)
                throw new Exception("Error while analysing network flow paths!");
            allowedLoggerLocations = flowTree.GetAllNodes(); //loggers can be placed anywhere in the flowtree; this eliminates all disconnected nodes and all nodes upstream from PRV outlets

        }

        private void button_calcLogConnections_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow mainWindow = (MainWindow)this.Owner;                
                List<Node> local_inlet_nodes = new List<Node>();
                //check which selected nodes are inlet by comparing to inlet_nodes
                if ((inletNodes != null) && (inletNodes.Count > 0))
                {
                    foreach (Node node in inletNodes)
                    {
                        if (!mainWindow.rectangleHitList.Any(tmp => ((Node)tmp.Tag) == node)) //check if this node is in selected nodes list
                        { //no
                            if (MessageBox.Show("Node " + node.name + " is at the outlet of PRV but has not been selected as logger. Do you want to add this node to list of inlet loggers",
                                "Inlet logger missed?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            { //user wants to add this node to list of inlet loggers 
                                mainWindow.rectangleHitList.Add(node.graphicalObject);
                                mainWindow.ChangeNodeApperance(node,  mainWindow.ToBrush(UI.Default.SelectionNodeColor), UI.Default.SelectionNodeSize);
                                local_inlet_nodes.Add(node);
                                textBox_nAllocatedLog.Text = mainWindow.rectangleHitList.Count.ToString();
                            }
                        }
                        else //yes
                            local_inlet_nodes.Add(node);
                    }
                }
                else
                    throw new Exception("List of inlet nodes unexpectedly empty!");                

                if (local_inlet_nodes.Count == 0)                
                    throw new Exception("No loggers at zone inlets have been defined!");
                
                //Generate loggers from selected nodes
                List<Logger> listLoggers = new List<Logger>();
                for (int i = 0; i < mainWindow.rectangleHitList.Count; i++)
                {
                    Node node = (Node)mainWindow.rectangleHitList[i].Tag;
                    bool isInlet = local_inlet_nodes.Any(tmp => tmp == node);
                    Logger new_logger = new Logger((i + 1).ToString(), node, isInlet);
                    new_logger.elevation = node.elevation;
                    listLoggers.Add(new_logger);
                }                

                //check loggers proximity
                if ((AdvancedOptions.loggerNeighbourhoodLevel > 1) && (LoggerConnections.CheckLoggersProximity(listLoggers)))
                {
                    if (MessageBox.Show("There are some loggers next to each other and with head difference smaller than the tolerance parameter, but the logger neighbourhood parameter is greater than 1. It is recommended to change the parameters or the logger locations. Do you want to continue calculating logger connections (YES), or to stop and modify the parameters (NO)?",
                        "Continue?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        return;                    
                }

                //if path highlighting window is open close it as we're redefining all logger connections and paths
                if (mainWindow.pathsWindow != null)
                    mainWindow.pathsWindow.Close();
                //clear the list in case some logger connections were highlighed
                mainWindow.pathHitList.Clear();

                //Generate flow tree for the loaded waterNetwork and inlets specified above                
                FlowTree flowTree = new FlowTree(mainWindow.waterNetwork, local_inlet_nodes);
                int ret_val = flowTree.GenerateFlowTree();
                if (ret_val < 0)
                    throw new Exception("Error while analysing network flow paths!");

                //Calculate and display logger connections
                mainWindow.loggerConnections = new LoggerConnections(listLoggers, flowTree);
                ret_val = mainWindow.loggerConnections.CalculatePathsBetweenLoggers(AdvancedOptions.head_diff_tolerance);
                if (ret_val < 0)
                    throw new Exception("Error while skeletonizing the network!");
                mainWindow.loggerConnections.LoggerConn2WaterNet();
                mainWindow.checkbox_loggerNetworkVisibility.IsChecked = false; //uncheck and check to refresh display
                mainWindow.checkbox_loggerNetworkVisibility.IsChecked = true;
                button_saveLogLocation.IsEnabled = true;
                button_RemoveLoggers.IsEnabled = true;

                //List<Node> chuj = mainWindow.loggerConnections.GetNodesFromPathsAndSideBranchesBetweenLoggers("5", "9");
                //List<Node> allPaths = mainWindow.loggerConnections.GetNodesFromAllPathsAllConnections();
                //List<Node> tmpbranch = new List<Node>();
                //Node thisNode = mainWindow.waterNetwork.listOfNodes.Find(tmp => tmp.name == "N15");
                //mainWindow.loggerConnections.AddBranchesNotInPaths(ref tmpbranch, thisNode, allPaths);
                
                //tmpbranch = new List<Node>();
                //thisNode = mainWindow.waterNetwork.listOfNodes.Find(tmp => tmp.name == "N63");
                //mainWindow.loggerConnections.AddBranchesNotInPaths(ref tmpbranch, thisNode, allPaths);


            }            
            catch (Exception ex)
            {
                MessageBox.Show("Error while calculating connections in logger location tool: " + ex.Message);
                return;
            }

        }

        private void button_saveLogLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow mainWindow = (MainWindow)this.Owner;
                ExcelFile excelFile = new ExcelFile();
                ExcelWorksheet workSheet = excelFile.Worksheets.Add("loggers");
                //create headers
                workSheet.Cells["A1"].Value = "Logger ID";
                workSheet.Cells["B1"].Value = "Nearest Node ID (EPANET Junction ID)";
                workSheet.Cells["C1"].Value = "Logger Elevation (use -1000 if the same as node elevation) [m]";
                workSheet.Cells["D1"].Value = "Is inlet node? (true/false)";
                //save data
                for (int i = 0; i < mainWindow.loggerConnections.list_of_loggers.Count; i++)
                {
                    Logger logger = mainWindow.loggerConnections.list_of_loggers[i];
                    workSheet.Cells[i + 1, 0].Value = logger.logger_id;
                    workSheet.Cells[i + 1, 1].Value = logger.node.name;
                    workSheet.Cells[i + 1, 2].Value = -1000;
                    workSheet.Cells[i + 1, 3].Value = logger.is_inlet.ToString();
                }
                ExcelWorksheet notesSheet = excelFile.Worksheets.Add("notes");
                notesSheet.Cells["A1"].Value = "Logger neighbourhood parameter = " + AdvancedOptions.loggerNeighbourhoodLevel.ToString();
                notesSheet.Cells["A2"].Value = "Head difference tolerance parameter [m] = " + AdvancedOptions.head_diff_tolerance.ToString();
                SaveFileDialog file_dialog = new SaveFileDialog();                
                file_dialog.Title = "Save selected loggers to Excel file";
                file_dialog.Filter = "Excel Files |*.xls";
                if (file_dialog.ShowDialog() != true)
                    return;
                excelFile.SaveXls(file_dialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while saving loggers in logger location tool: " + ex.Message);
                return;
            }

        }

        private void button_suggestLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {                
                //copy global allowedLoggerLocations to local allowedLoggerLocations
                List<Node> localAllowedLoggerLocations = new List<Node>(allowedLoggerLocations);
                MainWindow mainWindow = (MainWindow)this.Owner;
                if (mainWindow.waterNetwork.distanceMatrix == null)
                {
                    throw new Exception("Distance matrix not calculated. Please re-load the water network model.");
                }
                double[][] distanceMatrix = mainWindow.waterNetwork.distanceMatrix;
                List<Node> listSelectedNodes = new List<Node>();
                
                //add currently highlighted nodes to selected nodes and remove from allowedLoggerLocations
                for (int i = 0; i < mainWindow.rectangleHitList.Count; i++)
                {
                    Node node = (Node)mainWindow.rectangleHitList[i].Tag;
                    listSelectedNodes.Add(node);
                    localAllowedLoggerLocations.Remove(node);
                }
                //don't need to add inlets, should be already highlighted. PS 22/07/2013: not necessarily, inlets could have been removed manually
                if ((inletNodes != null) && (inletNodes.Count > 0))
                {
                    //add all inlet nodes to selected nodes and remove from allowedLoggerLocations
                    foreach (Node node in inletNodes)
                    {
                        if (!listSelectedNodes.Exists(tmp => tmp == node)) //if this inlet node has not already been selected
                        {
                            listSelectedNodes.Add(node);
                            localAllowedLoggerLocations.Remove(node);
                            mainWindow.rectangleHitList.Add(node.graphicalObject);
                            mainWindow.ChangeNodeApperance(node, constants.selectionNodeColor, constants.selectionNodeSize);
                        }
                    }
                    textBox_nAllocatedLog.Text = mainWindow.rectangleHitList.Count.ToString();
                }
                for (int i = listSelectedNodes.Count; i < _desiredNLoggers; i++)
                {
                    List<Tuple<Node, int, double, double>> listPriorityComponents = new List<Tuple<Node, int, double, double>>();
                    foreach (Node node in localAllowedLoggerLocations)
                    {
                        //number of neighbours component of priority
                        int nNeighbours = node.list_of_neighbours.Count;

                        //flow component of priority
                        List<Link> attachedLinks = new List<Link>();
                        attachedLinks.AddRange(node.GetIncomingLinks(mainWindow.waterNetwork));
                        attachedLinks.AddRange(node.GetOutgoingLinks(mainWindow.waterNetwork));
                        double totalFlow = 0;
                        foreach (Link link in attachedLinks)
                        {
                            totalFlow += Math.Abs(link.flow.Average());
                        }

                        //distance component of priority: v1 - as average head difference
                        //double thisNodeAvgHead = node.head.Average();
                        //double minDistance = double.PositiveInfinity;
                        //foreach (Node selectedNode in listSelectedNodes)
                        //{
                        //    double absHeadDifference = Math.Abs(selectedNode.head.Average() - thisNodeAvgHead);
                        //    if (absHeadDifference < minDistance)
                        //        minDistance = absHeadDifference;
                        //}

                        //distance component of priority: v2 - as number of intermediate nodes (1 = direct connection)
                        int index1 = mainWindow.waterNetwork.listOfNodes.FindIndex(tmp => tmp == node);
                        if (index1 < 0)
                            throw new Exception("Node " + node.name + " unexpectedly does not exist in the network!");
                        double minDistance = (i == 0) ? 1 : double.PositiveInfinity; //during 1st iteration distance is 1 as there are no loggers allocated yet
                        foreach (Node selectedNode in listSelectedNodes)
                        {
                            int index2 = mainWindow.waterNetwork.listOfNodes.FindIndex(tmp => tmp == selectedNode);
                            if (index2 < 0)
                                throw new Exception("Node " + selectedNode.name + " unexpectedly does not exist in the network!");
                            if (distanceMatrix[index1][index2] < minDistance)
                                minDistance = distanceMatrix[index1][index2];
                        }

                        //new tuple with all priority components
                        Tuple<Node, int, double, double> tmpTuple = new Tuple<Node, int, double, double>(node, nNeighbours, totalFlow, minDistance);
                        listPriorityComponents.Add(tmpTuple);                        
                    }

                    //normalize all components and calculate overall priority
                    List<Tuple<Node, double>> listNodePriority = new List<Tuple<Node, double>>();
                    int maxNeighbours = listPriorityComponents.Max(tmp => tmp.Item2);
                    double maxTotalFlow = listPriorityComponents.Max(tmp => tmp.Item3);
                    double maxMinDistance = listPriorityComponents.Max(tmp => tmp.Item4);                    
                    foreach (Tuple<Node, int, double, double> currentTuple in listPriorityComponents)
                    {
                        double overallPriority = currentTuple.Item2 / maxNeighbours * _nNeighboursWeight +
                                               currentTuple.Item3 / maxTotalFlow * _flowWeight +
                                               currentTuple.Item4 / maxMinDistance * _spreadWeight;
                        listNodePriority.Add(new Tuple<Node, double>(currentTuple.Item1, overallPriority));
                    }

                    //sort by overall priority and choose the node with highest overall priority 
                    List<Tuple<Node, double>> sortedListNodePriority = listNodePriority.OrderByDescending(tmp => tmp.Item2).ToList();
                    Node topNode = sortedListNodePriority[0].Item1;
                    listSelectedNodes.Add(topNode);
                    
                    //remove this node and it's neighbours from allowedLoggerLocations
                    localAllowedLoggerLocations.Remove(topNode);                    
                    foreach (NodeNeighbour nodeNeighbour in topNode.list_of_neighbours)
                    {
                        localAllowedLoggerLocations.Remove(nodeNeighbour.node);
                    }                    

                    //highlight newly added node
                    mainWindow.rectangleHitList.Add(topNode.graphicalObject);
                    mainWindow.ChangeNodeApperance(topNode, mainWindow.ToBrush(UI.Default.SelectionNodeColor), UI.Default.SelectionNodeSize);
                    textBox_nAllocatedLog.Text = mainWindow.rectangleHitList.Count.ToString();
                    //Tuple<Node, int, double, double> kupa = listPriorityComponents.Find(tmp => tmp.Item1 == topNode);
                    //MessageBox.Show("Total: "+ sortedListNodePriority[0].Item2.ToString() + "\nNeigh: " + kupa.Item2.ToString() + 
                    //    "\nFlow: " + kupa.Item3.ToString() + "\nSpread: " + kupa.Item4.ToString());

                } //for (int i = 0; i < _desiredNLoggers; i++)
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while calculating logger locations: " + ex.Message);
                return;
            }
        }

        /// <summary>Load locations from Excel file where loggers can't be placed. Takes existing allowedLoggerLocations as starting point, but if it hasn't been allocated take all waterNetwork nodes as starting point.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_loadProhibited_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "Load locations from Excel file where loggers can't be placed";
            fileDialog.Filter = "Excel Files |*.xls";
            if (fileDialog.ShowDialog() != true)
                return;
            try
            {
                MainWindow mainWindow = (MainWindow)this.Owner;
                if (allowedLoggerLocations == null) //if for some reason allowedLoggerLocations has not been allocated, take all waterNetwork nodes
                    allowedLoggerLocations = new List<Node>(mainWindow.waterNetwork.listOfNodes);
                ExcelFile excelFile = new ExcelFile();
                excelFile.LoadXls(fileDialog.FileName);
                ExcelWorksheet sheet = excelFile.Worksheets[0]; //worksheet name not important, but it has to be the 1st worksheet
                foreach (ExcelRow nodeNameRow in sheet.Rows)
                {
                    string nodeName = nodeNameRow.Cells[0].Value.ToString();
                    Node nodeToRemove = mainWindow.waterNetwork.listOfNodes.Find(tmp => tmp.name == nodeName);
                    if (nodeToRemove == null) //such node does not exist in the network
                        MessageBox.Show("Can't find node: " + nodeName + " in the water network. Entry ignored.");
                    else
                        allowedLoggerLocations.Remove(nodeToRemove);                    
                }
                
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while loading prohibited logger locations: " + ex.Message);
                return;
            }
        }

        private void button_RemoveLoggers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow mainWindow = (MainWindow)this.Owner;
                //if path highlighting window open close it as we're removing all logger connections and paths
                if (mainWindow.pathsWindow != null)
                    mainWindow.pathsWindow.Close();
                //unselect all highlighted elements, including nodes and logger connections
                mainWindow.unselectAllContextMenu_Click(sender, e);
                mainWindow.checkbox_loggerNetworkVisibility.IsChecked = false;
                mainWindow.loggerConnections = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while removing loggers: " + ex.Message);
                return;
            }
        }


    }
}
