using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.IO;
using Microsoft.Win32;
using Styx.Properties;
using Xceed.Wpf.Toolkit.PropertyGrid;
using UI = Styx.Properties.UserInterface; //alias to user interface settings

//using Xceed.Wpf.Toolkit;


namespace Styx
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{     
        
        /// <summary>
        /// Layout variables
        /// </summary>        

        //initial canvas coordinate system, later to be updated based on Epanet coordinates          
        private double xMax = 100.0; 
        private double yMin = 0.0; 
        private double yMax = 100.0;         
        private double xMin = 0.0;
        private double coordinatesMargin = 100;

        //store selected graphical objects
        //private List<Rectangle> rectangleHitList = new List<Rectangle>();
        public List<Rectangle> rectangleHitList = new List<Rectangle>();
        private List<Line> lineHitList = new List<Line>();
        public List<System.Windows.Shapes.Path> pathHitList = new List<System.Windows.Shapes.Path>();
        private List<Shape> tempHitList = new List<Shape>(); //to store overlayed elements

        ProgressWindow progressWindow; //window with progress bar
        public BackgroundWorker backWorker = new BackgroundWorker();

        private LoggerLocWindow loggerlocWindow; //window to place loggers
        public List<LoggerPaths> listOfAllLoggersPaths = new List<LoggerPaths>();//stores all paths for selected logger connections
        public PathsWindow pathsWindow; //window to select and highlight paths for selected logger connections

        private Point selectionSquareTopLeft; //staring point for selection box
        private bool isMultiSelection = false;
        private Rectangle selectionBox = new Rectangle(); //selection box


        /// <summary>
        /// Water network variables
        /// </summary>
        public WaterNetwork waterNetwork = new WaterNetwork();
        public WaterNetwork pathsNetwork = new WaterNetwork();///to store the selected paths of logger connection
        Epanet epanet; ///object to manipulate epanet files, simulate network and operate epanet tooltik
        Constants constants = new Constants();///to get predefined graphical attributes
       
        /// <summary>
        /// Engine variables
        /// </summary>
        public EFavorTest efavorTest; ///object containing all information and data from the field experiment (E-FAVOR)
        public LoggerConnections loggerConnections; ///object with all info related to loggers and their connections; include network 'skeletonization' methods
        private int _max_n_bursts; ///maximum number of bursts in the GA algorithm searching for bursts
        public string max_n_bursts ///string associated with textbox and linked to int _max_n_bursts
        {
            get
            {
                return (_max_n_bursts.ToString()); 
            }
            set
            {
                if (!int.TryParse(value, out _max_n_bursts))
                {
                    _max_n_bursts = 1; 
                    MessageBox.Show(value + " is not an integer value!");
                }
                if (_max_n_bursts < 1)
                {
                    _max_n_bursts = 1;
                    MessageBox.Show("Max number of bursts to search must be at least one!");
                }
            }            
        }
        
        public bool placingLoggersTool = false; ///true if logger placing tool is open and additional test should be run if node is clicked

        //public TestToDeleteLater testwindow = new TestToDeleteLater();
           

        public MainWindow()
		{
            //standard parameter of maximal burst
            max_n_bursts = "1";
			InitializeComponent();
            backWorker.WorkerReportsProgress = true;
            backWorker.WorkerSupportsCancellation = true;
            backWorker.DoWork += new DoWorkEventHandler(BackWorker_DoWork);
            backWorker.ProgressChanged += new ProgressChangedEventHandler(BackWorker_ProgressChanged);
            backWorker.RunWorkerCompleted +=new RunWorkerCompletedEventHandler(BackWorker_RunWorkerCompleted);
           // this.Closed += (sender, e) => this.Dispatcher.InvokeShutdown(); //alternative way to kill all treads
            //testwindow.Show();              
		}

        //======================================================================================
        #region         BACKGROUND WORKER METHODS
        //======================================================================================
        /// <summary>Do Work implementation for background worker to perform time consuming tasks. What the worker actually does is defined by argument e
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Defines what the worker does. Available options: "GenerateHigherLevelNeighbours" </param>
        private void BackWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgWorker = sender as BackgroundWorker;
            String whatToDo = e.Argument as String;
            if (whatToDo == "GenerateHigherLevelNeighbours")
            //if (e.Argument.GetType() == typeof(WaterNetwork)) //argument is water network so GenerateHigherLevelNeighbours needs to be called
            {
                e.Result = whatToDo;             
                int error = waterNetwork.GenerateHigherLevelNeighbours(AdvancedOptions.max_higher_level_neighbours, bgWorker);
                if (error < 0)
                    throw new Exception("Error while analysing higher level node neighbours!");
            }            
            else
                //throw new Exception("Don't know what to do with object of type: " + e.Argument.GetType().ToString() + " in Background Worker.");
                throw new Exception("Unknown option: " + whatToDo + " in Background Worker.");
        }

        private void BackWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressWindow.progressBar1.Value = e.ProgressPercentage; 
        }

        private void BackWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            String whatToDo = e.Result as String;
            if (whatToDo == "GenerateHigherLevelNeighbours")
            {
                //enable and disable relevant buttons
                button_LoggerLocTool.IsEnabled = true;
                button_LoggerLocTool.ToolTip = "Launch tool to plan placement of loggers";
                Button_loadEfavor.IsEnabled = true;
                estimateButton.IsEnabled = false;  //if was enabled earlier...
                button_LocalizeBurst.IsEnabled = false;
                inletSetSelectionComboBox.IsEnabled = false;
                mainWindow.IsEnabled = true;

                //close progress window
                progressWindow.allowClosing = true;
                progressWindow.Close(); 
            }            
            else
                throw new Exception("Unknown option: " + whatToDo + " in Background Worker.");
        }

        #endregion

        //======================================================================================================================================================
        #region                  LAYOUT METHODS
        //======================================================================================================================================================

        /// <summary>
        /// Shows message box to confirm closing and close Epanet toolkit if it is open
        /// </summary>
        /// <param name="e"></param>
        //protected override void OnClosing(CancelEventArgs e) 
        //{
            //base.OnClosing(e);

            //if (MessageBox.Show("Are you sure you want to close?", "Close software?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            //    e.Cancel = true;
            //else
            //{
            //    UserInterface.Default.Save(); //save user interface settings                           
               
            //    if (epanet != null)
            //    {
            //        int error = epanet.CloseENToolkit();
            //        if (error != 0)
            //            MessageBox.Show("Error while closing EPANET toolkit: " + error.ToString());
            //    }
            //}
        //}

        void CloseCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            
        }

        void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        void OpenCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void HelpExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://watersoftware.dmu.ac.uk/contact-us/"); 
            
        }

        private void HelpCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        void OpenExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                // Create an instance of the open file dialog box.
                OpenFileDialog openFileDialog1 = new OpenFileDialog();

                // Set filter options and filter index.
                //openFileDialog1.Filter = "Inp Files (.inp)|*.inp|All Files (*.*)|*.*";
                openFileDialog1.Filter = "Inp Files (.inp)|*.inp"; //open only inp files
                openFileDialog1.FilterIndex = 1;
                
                // Call the ShowDialog method to show the dialog box.
                bool? userClickedOK = openFileDialog1.ShowDialog();

                // Process input if the user clicked OK.
                if (userClickedOK == true)
                {
                    //close child windows
                    foreach (Window window in this.OwnedWindows)
                        window.Close(); 

                    waterNetwork = new WaterNetwork();

                    // Open the selected file to read.
                    System.IO.Stream fileStream = openFileDialog1.OpenFile();//
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(fileStream))
                    {
                        // Read from the file and write it the textbox.
                        //textBox1.Text += reader.ReadToEnd();
                    }
                    fileStream.Close();
                    

                    //Determine current path
                    Settings.Default.path = System.AppDomain.CurrentDomain.BaseDirectory; //Directory.GetCurrentDirectory();
                    Settings.Default.orginalInpFile = openFileDialog1.FileName;

                    //Create file names for EPAnet report file and results file
                    Settings.Default.epanetReportFile = Settings.Default.orginalInpFile.Substring(0, Settings.Default.orginalInpFile.IndexOf('.')) + ".rpt";
                    Settings.Default.epanetResultsFile = Settings.Default.orginalInpFile.Substring(0, Settings.Default.orginalInpFile.IndexOf('.')) + ".bin";

                    //Check that epanet.dll existis
                    Settings.Default.epanetDllLocation = Settings.Default.path + "\\" + "epanet2.dll";
                    if (!File.Exists(Settings.Default.epanetDllLocation))
                    {
                        throw new FileNotFoundException("Epanet2.dll not found.");
                    }

                    //NOT REQUIRED - FULL SIMULATION TO PRODUCE *.bin IS NOW DONE VIA DLL ENepanet
                    //Read path to epanet application(epanet2w.exe) and engine(epanet2d.exe) stored in epanetPaths.txt
                    /*if (!File.Exists(Settings.Default.path + "\\epanetPaths.txt"))//check that specified file exists 
                    {
                        throw new FileNotFoundException("Config file epanetPaths.txt not found.");
                    }
                    else
                    {
                        StreamReader epanetPathsFile = new StreamReader(Settings.Default.path + "\\epanetPaths.txt");//open file to read
                        Settings.Default.epanetEngine = epanetPathsFile.ReadLine();
                        Settings.Default.epanetApplication = epanetPathsFile.ReadLine();
                        epanetPathsFile.Close();
                    }*/

                    //get data from epanet
                    if ((epanet != null) && (epanet.en_toolkit_open)) //if toolkit was opened earlier close it
                        epanet.CloseENToolkit();
                    epanet = new Epanet(); //epanet is now member of MainWindow
                    int error = epanet.ReadEpanetDll_WindowApp(waterNetwork, false);
                    if (error < 0) throw new Exception("Error while loading Epanet network file.");
                    //analyze neighbours for each node
                    error = waterNetwork.GenerateNeighboursLists(AdvancedOptions.zero_flow_tolerance);
                    if (error < 0)
                        throw new Exception("Error while analysing node neighbours!");
                    progressWindow = new ProgressWindow("Analysing network structure, please wait...\n If the network is large this may take few minutes");
                    progressWindow.Owner = mainWindow;
                    progressWindow.Show();
                    backWorker.RunWorkerAsync("GenerateHigherLevelNeighbours"); //GenerateNeighboursLists must be called before GenerateHigherLevelNeighbours
                    //error = waterNetwork.GenerateHigherLevelNeighbours(AdvancedOptions.max_higher_level_neighbours, progressWindow);
                    mainWindow.IsEnabled = false;  //temporarily disable main window while GenerateHigherLevelNeighbours is run                                    

                    //set UI flags
                    waterNetwork.isPlotted = true;
                    mainNetworkVisibility.IsChecked = true;
                    checkbox_loggerNetworkVisibility.IsChecked = false;
                    nodesLabelsVisibility.IsEnabled = true;
                    linksLabelsVisibility.IsEnabled = true;
                    mainNetworkVisibility.IsEnabled = true;
                    Button_suspectedLoggers.IsEnabled = false;

                    //we've loaded new network so previous loggerConnections, efavorTest and pathsNetwork are irrelevant and should be removed
                    loggerConnections = null;
                    efavorTest = null;
                    pathsNetwork = null;

                    // draw network
                    //DrawWaterNetwork(waterNetwork, standardNodeSize, standardNodeColor, standardLinkThickness, standardLinkColor);
                    error = DrawWaterNetwork();                    
                    
                    ////enable and disable relevant buttons
                    //button_LoggerLocTool.IsEnabled = true;
                    //button_LoggerLocTool.ToolTip = "Launch tool to plan placement of loggers";
                    //Button_loadEfavor.IsEnabled = true;
                    //estimateButton.IsEnabled = false;  //if was enabled earlier...
                    //button_LocalizeBurst.IsEnabled = false;
                    //inletSetSelectionComboBox.IsEnabled = false;                                                           

                }
                else
                {

                }

                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);         
            }
        }
        /// <summary>
        /// Draws water network object
        /// </summary>
        /// <param name="waterNetwork">water network</param>
        /// <param name="nodeSize">Node size in pixels</param>
        /// <param name="nodeColor">Node color</param>
        /// <param name="linkThickness">Line thickenss</param>
        /// <param name="linkColor">Line color</param>
  /*      public void DrawWaterNetwork(WaterNetwork networkToDraw, double nodeSize, Brush nodeColor , double linkThickness, Brush linkColor)
        {
            try
            {

                if (networkToDraw.listOfNodes != null)
                {
                    //determine canvas size
                    xMax = waterNetwork.listOfNodes.Max(tmp => tmp.xcoord) + coordinatesMargin;
                    xMin = waterNetwork.listOfNodes.Min(tmp => tmp.xcoord) - coordinatesMargin;

                    yMax = waterNetwork.listOfNodes.Max(tmp => tmp.ycoord) + coordinatesMargin;
                    yMin = waterNetwork.listOfNodes.Min(tmp => tmp.ycoord) - coordinatesMargin;

                    //clear canvas
                    canvas1.Children.Clear();
                    canvas1.Background = Brushes.White;


                    if (networkToDraw.waterNetworkType == Constants.REAL_NETWORK)
                    {
                                             
                        //plot links
                        foreach (Link link in networkToDraw.listOfLinks)
                        {
                            Line connection = new Line();
                            Node nodeTo = new Node();
                            Node nodeFrom = new Node();
                            
                            //find nodes
                            nodeTo = networkToDraw.listOfNodes.ElementAt(link.nodeTo);
                            nodeFrom = networkToDraw.listOfNodes.ElementAt(link.nodeFrom);
                            
                            //assign link to object tag
                            connection.Tag = link;

                            //line attributes
                            connection.X1 = XNormalize((double)nodeFrom.xcoord) + nodeSize / 2; //added offset to centre of node
                            connection.Y1 = YNormalize((double)nodeFrom.ycoord) + nodeSize / 2;

                            connection.X2 = XNormalize((double)nodeTo.xcoord) + nodeSize / 2;
                            connection.Y2 = YNormalize((double)nodeTo.ycoord) + nodeSize / 2;

                            connection.Stroke = linkColor;
                            connection.StrokeThickness = linkThickness;
      
                            canvas1.Children.Add(connection);
                            connection.Uid = link.name;
                            link.graphicalObject = connection;

                            //plot link labels
                            if (linksLabelsVisibility.IsChecked == true)
                            {
                                TextBlock linkLabel = new TextBlock();
                                linkLabel.Text = link.name;
                                Canvas.SetLeft(linkLabel, (connection.X1 + connection.X2) / 2);
                                Canvas.SetTop(linkLabel, (connection.Y1 + connection.Y2) / 2);
                                canvas1.Children.Add(linkLabel);
                            }
                        }

                        //plot nodes
                        foreach (Node node in networkToDraw.listOfNodes)
                        {

                            //create rectangle object
                            Rectangle rect = new Rectangle();
                            rect.Fill = nodeColor;
                            rect.Height = nodeSize;
                            rect.Width = nodeSize;
                            rect.Tag = node;

                            //add rectange to canvas
                            Canvas.SetLeft(rect, XNormalize((double)node.xcoord));
                            Canvas.SetTop(rect, YNormalize((double)node.ycoord));
                            canvas1.Children.Add(rect);
                            rect.Uid = node.name;
                            node.graphicalObject = rect;

                            //plot node labels
                            if (nodesLabelsVisibility.IsChecked == true)
                            {
                                TextBlock nodeLabel = new TextBlock();
                                nodeLabel.Text = node.name;
                                Canvas.SetLeft(nodeLabel, XNormalize((double)node.xcoord));
                                Canvas.SetTop(nodeLabel, YNormalize((double)node.ycoord) - nodeSize * nodeLabelOffset);
                                canvas1.Children.Add(nodeLabel);
                            }

                        }
                    }
                    else if (networkToDraw.waterNetworkType == Constants.LOGGER_NETWORK)
                    {
                        //plot links
                        foreach (Link link in waterNetwork.listOfLinks)
                        {
                            Node nodeTo = new Node();
                            Node nodeFrom = new Node();


                            //find nodes
                            nodeTo = networkToDraw.listOfNodes.ElementAt(link.nodeTo);
                            nodeFrom = networkToDraw.listOfNodes.ElementAt(link.nodeFrom);

                            // 3 lines to draw a arrow line
                            Point toNode = new Point(XNormalize((double)nodeTo.xcoord) + nodeSize / 2, YNormalize((double)nodeTo.ycoord) + nodeSize / 2);
                            Point fromNode = new Point(XNormalize((double)nodeFrom.xcoord) + nodeSize / 2, YNormalize((double)nodeFrom.ycoord) + nodeSize / 2);
                            Shape arrowLine = DrawLinkArrow(fromNode, toNode, loggerLinkColor, loggerLinkThickness);

                            //assign link to object tag
                            arrowLine.Tag = link;
                            canvas1.Children.Add(arrowLine);
                            arrowLine.Uid = "LINK_LOG"+link.name;
                            link.graphicalObject = arrowLine;
                            

                            //plot link labels
                            if (linksLabelsVisibility.IsChecked == true)
                            {
                                TextBlock linkLabel = new TextBlock();
                                linkLabel.Text = link.name;
                                Canvas.SetLeft(linkLabel, ((XNormalize((double)nodeFrom.xcoord) + nodeSize / 2) + (XNormalize((double)nodeTo.xcoord) + nodeSize / 2)) / 2);
                                Canvas.SetTop(linkLabel, ((YNormalize((double)nodeFrom.ycoord) + nodeSize / 2) + (YNormalize((double)nodeTo.ycoord) + nodeSize / 2)) / 2);
                                canvas1.Children.Add(linkLabel);
                            }
                        }

                        //plot nodes
                        foreach (Node node in networkToDraw.listOfNodes)
                        {

                            //create rectangle object
                            Rectangle rect = new Rectangle();
                            rect.Fill = loggerNodeColor;
                            rect.Height = loggerNodeSize;
                            rect.Width = loggerNodeSize;
                            rect.Tag = node;

                            //add rectange to canvas
                            Canvas.SetLeft(rect, XNormalize((double)node.xcoord));
                            Canvas.SetTop(rect, YNormalize((double)node.ycoord));
                            canvas1.Children.Add(rect);
                            rect.Uid = "NODE_LOG"+node.name;
                            node.graphicalObject = rect;

                            //plot node labels
                            if (nodesLabelsVisibility.IsChecked == true)
                            {
                                TextBlock nodeLabel = new TextBlock();
                                nodeLabel.Text = node.name;
                                Canvas.SetLeft(nodeLabel, XNormalize((double)node.xcoord));
                                Canvas.SetTop(nodeLabel, YNormalize((double)node.ycoord) - nodeSize * nodeLabelOffset);
                                canvas1.Children.Add(nodeLabel);
                            }

                        }
                    }
                    else if (networkToDraw.waterNetworkType == Constants.PATH_NETWORK)
                    {
                        //plot links
                        foreach (Link link in waterNetwork.listOfLinks)
                        {
                            Node nodeTo = new Node();
                            Node nodeFrom = new Node();


                            //find nodes
                            nodeTo = networkToDraw.listOfNodes.ElementAt(link.nodeTo);
                            nodeFrom = networkToDraw.listOfNodes.ElementAt(link.nodeFrom);

                            // 3 lines to draw a arrow line
                            Point toNode = new Point(XNormalize((double)nodeTo.xcoord) + nodeSize / 2, YNormalize((double)nodeTo.ycoord) + nodeSize / 2);
                            Point fromNode = new Point(XNormalize((double)nodeFrom.xcoord) + nodeSize / 2, YNormalize((double)nodeFrom.ycoord) + nodeSize / 2);
                            Shape arrowLine = DrawLinkArrow(fromNode, toNode, pathLinkColor, pathLinkThickness);

                            //assign link to object tag
                            arrowLine.Tag = link;
                            canvas1.Children.Add(arrowLine);
                            arrowLine.Uid = "LINK_PATH" + link.name;
                            link.graphicalObject = arrowLine;


                            //plot link labels
                            if (linksLabelsVisibility.IsChecked == true)
                            {
                                TextBlock linkLabel = new TextBlock();
                                linkLabel.Text = link.name;
                                Canvas.SetLeft(linkLabel, ((XNormalize((double)nodeFrom.xcoord) + nodeSize / 2) + (XNormalize((double)nodeTo.xcoord) + nodeSize / 2)) / 2);
                                Canvas.SetTop(linkLabel, ((YNormalize((double)nodeFrom.ycoord) + nodeSize / 2) + (YNormalize((double)nodeTo.ycoord) + nodeSize / 2)) / 2);
                                canvas1.Children.Add(linkLabel);
                            }
                        }

                        //plot nodes
                        foreach (Node node in networkToDraw.listOfNodes)
                        {

                            //create rectangle object
                            Rectangle rect = new Rectangle();
                            rect.Fill = pathNodeColor;
                            rect.Height = pathNodeSize;
                            rect.Width = pathNodeSize;
                            rect.Tag = node;

                            //add rectange to canvas
                            Canvas.SetLeft(rect, XNormalize((double)node.xcoord));
                            Canvas.SetTop(rect, YNormalize((double)node.ycoord));
                            canvas1.Children.Add(rect);
                            rect.Uid = "NODE_PATH" + node.name;
                            node.graphicalObject = rect;

                            //plot node labels
                            if (nodesLabelsVisibility.IsChecked == true)
                            {
                                TextBlock nodeLabel = new TextBlock();
                                nodeLabel.Text = node.name;
                                Canvas.SetLeft(nodeLabel, XNormalize((double)node.xcoord));
                                Canvas.SetTop(nodeLabel, YNormalize((double)node.ycoord) - nodeSize * nodeLabelOffset);
                                canvas1.Children.Add(nodeLabel);
                            }
                            
                        }
                    }

                }
                else throw new Exception("Water network object has no nodes.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        */

        public int DrawWaterNetwork()
        {
            try
            {

                if (waterNetwork.listOfNodes.Count >0) //draw networks only when waternetwork object is loaded.
                {
                    //determine canvas size
                    xMax = waterNetwork.listOfNodes.Max(tmp => tmp.xcoord) + coordinatesMargin;
                    xMin = waterNetwork.listOfNodes.Min(tmp => tmp.xcoord) - coordinatesMargin;

                    yMax = waterNetwork.listOfNodes.Max(tmp => tmp.ycoord) + coordinatesMargin;
                    yMin = waterNetwork.listOfNodes.Min(tmp => tmp.ycoord) - coordinatesMargin;

                    //clear canvas
                    canvas1.Children.Clear();
                    canvas1.Background = Brushes.White;

                    //plot main water network
                    if (waterNetwork.isPlotted)
                    {
                        DrawWaterNetworkEngine(waterNetwork);
                    }
                    
                    //plot logger connection network
                    if ((loggerConnections != null) && (loggerConnections.logger_water_network != null))
                    {
                        if (loggerConnections.logger_water_network.isPlotted)
                        {
                            DrawWaterNetworkEngine(loggerConnections.logger_water_network);
                        }
                    }

                    //plot paths network
                    if (pathsNetwork != null)
                    {
                        if (pathsNetwork.isPlotted)
                        {
                            DrawWaterNetworkEngine(pathsNetwork);
                        }
                           
                    }
                }
                else throw new Exception("Main water network object has no nodes.");
                return 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return (-1);
            }
        }

        public void DrawWaterNetworkEngine(WaterNetwork networkToDraw)
        {
            try
            {
                if (networkToDraw.waterNetworkType == Constants.REAL_NETWORK)
                    {
                        //plot links
                        foreach (Link link in networkToDraw.listOfLinks)
                        {
                            Line connection = new Line();
                            Node nodeTo = new Node();
                            Node nodeFrom = new Node();

                            //find nodes
                            nodeTo = networkToDraw.listOfNodes.ElementAt(link.nodeTo);
                            nodeFrom = networkToDraw.listOfNodes.ElementAt(link.nodeFrom);

                            //assign link to object tag
                            connection.Tag = link;

                            //line attributes
                            connection.X1 = XNormalize((double)nodeFrom.xcoord) + nodeFrom.size / 2; //added offset to centre of node
                            connection.Y1 = YNormalize((double)nodeFrom.ycoord) + nodeFrom.size / 2;

                            connection.X2 = XNormalize((double)nodeTo.xcoord) + nodeTo.size / 2;
                            connection.Y2 = YNormalize((double)nodeTo.ycoord) + nodeTo.size / 2;

                            connection.Stroke = link.color;
                            connection.StrokeThickness = link.thickness;
                            connection.Uid = link.name;
                            canvas1.Children.Add(connection);
                          
                            link.graphicalObject = connection;


                            ////to preserve color of selected links
                            //if (lineHitList != null)
                            //{
                            //    foreach (Line tmpLine in lineHitList)
                            //    {
                            //        ChangeLinkApperance((Link)tmpLine.Tag,selectionLinkcolor , selectionLinkThickness);
                            //    }
                            //}

                            //plot link labels
                            if (linksLabelsVisibility.IsChecked == true)
                            {
                                TextBlock linkLabel = new TextBlock();
                                linkLabel.Text = link.name;
                                Canvas.SetLeft(linkLabel, (connection.X1 + connection.X2) / 2);
                                Canvas.SetTop(linkLabel, (connection.Y1 + connection.Y2) / 2);
                                canvas1.Children.Add(linkLabel);
                            }
                        }

                        //plot nodes
                        foreach (Node node in networkToDraw.listOfNodes)
                        {

                            //create rectangle object
                            Rectangle rect = new Rectangle();
                            rect.Fill = node.color;
                            rect.Height = node.size;
                            rect.Width = node.size;                              
                            rect.Tag = node;
                           
                            //add rectangle to canvas
                            Canvas.SetLeft(rect, XNormalize((double)node.xcoord));
                            Canvas.SetTop(rect, YNormalize((double)node.ycoord));
                            rect.Uid = node.name;
                            canvas1.Children.Add(rect);
                           
                            node.graphicalObject = rect;

                            

                            ////to preserve color of selected nodes
                            //if (rectangleHitList != null)
                            //{
                            //    foreach (Rectangle rectangle in rectangleHitList)
                            //    {
                            //       ChangeNodeApperance((Node)rectangle.Tag,selectionNodeColor,selectionNodeSize);
                            //    }
                            //}

                            //plot node labels
                            if (nodesLabelsVisibility.IsChecked == true)
                            {
                                TextBlock nodeLabel = new TextBlock();
                                nodeLabel.Text = node.name;
                                Canvas.SetLeft(nodeLabel, XNormalize((double)node.xcoord));
                                Canvas.SetTop(nodeLabel, YNormalize((double)node.ycoord) - node.size * UI.Default.NodeLabelOffset);
                                canvas1.Children.Add(nodeLabel);
                            }

                        }
                    }
                    else if (networkToDraw.waterNetworkType == Constants.LOGGER_NETWORK)
                    {
                        //plot links
                        foreach (Link link in networkToDraw.listOfLinks)
                        {
                            Node nodeTo = new Node();
                            Node nodeFrom = new Node();


                            //find nodes
                            nodeTo = networkToDraw.listOfNodes.ElementAt(link.nodeTo);
                            nodeFrom = networkToDraw.listOfNodes.ElementAt(link.nodeFrom);

                            // 3 lines to draw a arrow line
                            Point toNode = new Point(XNormalize((double)nodeTo.xcoord) + nodeTo.size / 2, YNormalize((double)nodeTo.ycoord) + nodeTo.size / 2);
                            Point fromNode = new Point(XNormalize((double)nodeFrom.xcoord) + nodeFrom.size / 2, YNormalize((double)nodeFrom.ycoord) + nodeFrom.size / 2);
                            Shape arrowLine = DrawLinkArrow(fromNode, toNode, link.color, link.thickness);

                            //assign link to object tag
                            arrowLine.Tag = link;
                            canvas1.Children.Add(arrowLine);
                            arrowLine.Uid = link.name;
                            link.graphicalObject = arrowLine;


                            //plot link labels
                            if (linksLabelsVisibility.IsChecked == true)
                            {
                                TextBlock linkLabel = new TextBlock();
                                linkLabel.Text = link.label;
                                Canvas.SetLeft(linkLabel, ((XNormalize((double)nodeFrom.xcoord) + nodeFrom.size / 2) + (XNormalize((double)nodeTo.xcoord) + nodeTo.size / 2)) / 2);
                                Canvas.SetTop(linkLabel, ((YNormalize((double)nodeFrom.ycoord) + nodeFrom.size / 2) + (YNormalize((double)nodeTo.ycoord) + nodeTo.size / 2)) / 2);
                                canvas1.Children.Add(linkLabel);
                            }
                        }

                        //plot nodes
                        foreach (Node node in networkToDraw.listOfNodes)
                        {

                            //create rectangle object
                            Rectangle rect = new Rectangle();
                            rect.Fill = node.color;
                            rect.Height = node.size;
                            rect.Width = node.size;
                            rect.Tag = node;

                            //add rectangle to canvas
                            Canvas.SetLeft(rect, XNormalize((double)node.xcoord));
                            Canvas.SetTop(rect, YNormalize((double)node.ycoord));
                            canvas1.Children.Add(rect);
                            rect.Uid = node.name;
                            node.graphicalObject = rect;

                            //plot node labels
                            if (nodesLabelsVisibility.IsChecked == true)
                            {
                                TextBlock nodeLabel = new TextBlock();
                                nodeLabel.Text = node.name;
                                Canvas.SetLeft(nodeLabel, XNormalize((double)node.xcoord));
                                Canvas.SetTop(nodeLabel, YNormalize((double)node.ycoord) - node.size * UI.Default.NodeLabelOffset);
                                canvas1.Children.Add(nodeLabel);
                            }

                        }
                    }
                    else if (networkToDraw.waterNetworkType == Constants.PATH_NETWORK)
                    {
                        //plot links
                        foreach (Link link in networkToDraw.listOfLinks)
                        {
                            Node nodeTo = new Node();
                            Node nodeFrom = new Node();


                            //find nodes
                            nodeTo = networkToDraw.listOfNodes.ElementAt(link.nodeTo);
                            nodeFrom = networkToDraw.listOfNodes.ElementAt(link.nodeFrom);

                            // 3 lines to draw a arrow line
                            Point toNode = new Point(XNormalize((double)nodeTo.xcoord) + nodeTo.size / 2, YNormalize((double)nodeTo.ycoord) + nodeTo.size / 2);
                            Point fromNode = new Point(XNormalize((double)nodeFrom.xcoord) + nodeFrom.size / 2, YNormalize((double)nodeFrom.ycoord) + nodeFrom.size / 2);
                            Shape arrowLine = DrawLinkArrow(fromNode, toNode, link.color, link.thickness);

                            //assign link to object tag
                            arrowLine.Tag = link;
                            canvas1.Children.Add(arrowLine);
                            arrowLine.Uid = "LINK_PATH" + link.name;
                            link.graphicalObject = arrowLine;


                            //plot link labels
                            if (linksLabelsVisibility.IsChecked == true && waterNetwork.isPlotted == false) //DP: 19/07/2013 "waterNetwork.isPlotted == false" added to remove Bug 39                               
                            {
                                TextBlock linkLabel = new TextBlock();
                                linkLabel.Text = link.name;
                                Canvas.SetLeft(linkLabel, ((XNormalize((double)nodeFrom.xcoord) + nodeFrom.size / 2) + (XNormalize((double)nodeTo.xcoord) + nodeTo.size / 2)) / 2);
                                Canvas.SetTop(linkLabel, ((YNormalize((double)nodeFrom.ycoord) + nodeFrom.size / 2) + (YNormalize((double)nodeTo.ycoord) + nodeTo.size / 2)) / 2);
                                canvas1.Children.Add(linkLabel);
                            }
                        }

                        //plot nodes
                        foreach (Node node in networkToDraw.listOfNodes)
                        {

                            //create rectangle object
                            Rectangle rect = new Rectangle();
                            rect.Fill = node.color;
                            rect.Height = node.size;
                            rect.Width = node.size;
                            rect.Tag = node;

                            //add rectange to canvas
                            Canvas.SetLeft(rect, XNormalize((double)node.xcoord));
                            Canvas.SetTop(rect, YNormalize((double)node.ycoord));
                            canvas1.Children.Add(rect);
                            rect.Uid = "NODE_PATH" + node.name;
                            node.graphicalObject = rect;

                            //plot node labels
                            if (nodesLabelsVisibility.IsChecked == true && waterNetwork.isPlotted == false) //DP: 19/07/2013 "waterNetwork.isPlotted == false" added to remove Bug 39 
                            {                             
                                    TextBlock nodeLabel = new TextBlock();
                                    nodeLabel.Text = node.name;
                                    Canvas.SetLeft(nodeLabel, XNormalize((double)node.xcoord));
                                    Canvas.SetTop(nodeLabel, YNormalize((double)node.ycoord) - node.size * UI.Default.NodeLabelOffset);
                                    canvas1.Children.Add(nodeLabel);
                                
                            }

                        }
                    }

                
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }        
        
		/// <summary>
        /// Toggle between docked and undocked states (Pane 1) 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        //public void pane1Pin_Click(object sender, RoutedEventArgs e) 
        //{
        //    if (pane1Button.Visibility == Visibility.Collapsed)
        //        UndockPane(1); 
        //    else
        //        DockPane(1); 
        //}


        ///// <summary>
        /////  Toggle between docked and undocked states (Pane 2)
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param> 
        //public void pane2Pin_Click(object sender, RoutedEventArgs e) 
        //{
        //    if (pane2Button.Visibility == Visibility.Collapsed)
        //        UndockPane(2); 
        //    else
        //        DockPane(2); 
        //}

        // Docks a pane, which hides the corresponding pane button 
        //public void DockPane(int paneNumber)
        //{
        //    if (paneNumber == 1) 
        //    {
        //        pane1Button.Visibility = Visibility.Collapsed; 
        //        pane1PinImage.Source = new BitmapImage(new Uri("pin.gif", UriKind.Relative));
        //        // Add the cloned column to layer 0: 
        //        layer0.ColumnDefinitions.Add(column1CloneForLayer0); 
        //        // Add the cloned column to layer 1, but only if pane 2 is docked: 
        //        if (pane2Button.Visibility == Visibility.Collapsed)
        //        layer1.ColumnDefinitions.Add(column2CloneForLayer1); 
        //    } 
        //    else if (paneNumber == 2) 
        //    {
        //        pane2Button.Visibility = Visibility.Collapsed; 
        //        pane2PinImage.Source = new BitmapImage(new Uri("pin.gif", UriKind.Relative));
        //        // Add the cloned column to layer 0: 
        //        layer0.ColumnDefinitions.Add(column2CloneForLayer0); 
        //        // Add the cloned column to layer 1, but only if pane 1 is docked: 
        //        if (pane1Button.Visibility == Visibility.Collapsed)
        //        layer1.ColumnDefinitions.Add(column2CloneForLayer1); 
        //    }
        //}

		// Undocks a pane, which reveals the corresponding pane button 
        //public void UndockPane(int paneNumber) 
        //{
        //    if (paneNumber == 1) 
        //    {
        //        layer1.Visibility = Visibility.Visible; 
        //        pane1Button.Visibility = Visibility.Visible; 
        //        pane1PinImage.Source = new BitmapImage
        //        (new Uri("pinHorizontal.gif", UriKind.Relative));
        //        // Remove the cloned columns from layers 0 and 1: 
        //        layer0.ColumnDefinitions.Remove(column1CloneForLayer0); 
        //        // This won’t always be present, but Remove silently ignores bad columns: 
        //        layer1.ColumnDefinitions.Remove(column2CloneForLayer1);
        //    } 
        //    else if (paneNumber == 2) 
        //    {
        //        layer2.Visibility = Visibility.Visible; 
        //        pane2Button.Visibility = Visibility.Visible;
        //        pane2PinImage.Source = new BitmapImage 
        //        (new Uri("pinHorizontal.gif", UriKind.Relative));
        //        // Remove the cloned columns from layers 0 and 1: 
        //        layer0.ColumnDefinitions.Remove(column2CloneForLayer0); 
        //        // This won’t always be present, but Remove silently ignores bad columns: 
        //        layer1.ColumnDefinitions.Remove(column2CloneForLayer1);
        //    }
        //}

        
        public HitTestResultBehavior HitTestCallbackGeometry(HitTestResult result)
        {
            // Retrieve the results of the hit test. 
            IntersectionDetail intersectionDetail =
               ((GeometryHitTestResult)result).IntersectionDetail;

            switch (intersectionDetail)
            {
                case IntersectionDetail.FullyContains:
                    if (result.VisualHit.GetType() == typeof(Rectangle) || result.VisualHit.GetType() == typeof(Line) || result.VisualHit.GetType() == typeof(System.Windows.Shapes.Path))
                    {
                        Shape shape = result.VisualHit as Shape;
                        tempHitList.Add(shape);
                    }
                    // Keep looking for hits 
                    return HitTestResultBehavior.Continue;

                case IntersectionDetail.Intersects:
                    if (result.VisualHit.GetType() == typeof(Rectangle) || result.VisualHit.GetType() == typeof(Line) || result.VisualHit.GetType() == typeof(System.Windows.Shapes.Path))
                    {
                        Shape shape = result.VisualHit as Shape;
                        tempHitList.Add(shape);
                    }
                    // Set the behavior to return visuals at all z-order levels: 
                    return HitTestResultBehavior.Continue;

                case IntersectionDetail.FullyInside:
                    if (result.VisualHit.GetType() == typeof(Rectangle) || result.VisualHit.GetType() == typeof(Line) || result.VisualHit.GetType() == typeof(System.Windows.Shapes.Path))
                    {
                        Shape shape = result.VisualHit as Shape;
                        tempHitList.Add(shape);
                    }
                    // Set the behavior to return visuals at all z-order levels: 
                    return HitTestResultBehavior.Continue;

                default:
                    return HitTestResultBehavior.Stop;
            }
        }

        public HitTestResultBehavior HitTestCallback(HitTestResult result)
        {

            if (result.VisualHit.GetType() == typeof(Rectangle) || result.VisualHit.GetType() == typeof(Line) || result.VisualHit.GetType() == typeof(System.Windows.Shapes.Path))
            {
                Shape shape = result.VisualHit as Shape;
                tempHitList.Add(shape);
            }
            // Keep looking for hits 
            return HitTestResultBehavior.Continue;
        }
        
        
        private void layer0_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void nodesLabelsVisibility_Unchecked(object sender, RoutedEventArgs e)
        {
            if (waterNetwork.listOfNodes.Count > 0)
            {
                int error = DrawWaterNetwork();
            }
        }

        private void nodesLabelsVisibility_Checked(object sender, RoutedEventArgs e)
        {
            if (waterNetwork.listOfNodes.Count > 0)
            {
                int error = DrawWaterNetwork();
            }
            else           
            {
                nodesLabelsVisibility.IsChecked = false;
            }
        }

        private void linksLabelsVisibility_Checked(object sender, RoutedEventArgs e)
        {
            if (waterNetwork.listOfNodes.Count > 0) { int error = DrawWaterNetwork(); }
        }

        private void linksLabelsVisibility_Unchecked(object sender, RoutedEventArgs e)
        {
            if (waterNetwork.listOfNodes.Count > 0) { int error = DrawWaterNetwork(); }
        }             
                        
        private void canvasGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
                    canvas1.Width = canvasGrid.ActualWidth
                    - chartBorder.Margin.Left
                    - chartBorder.Margin.Right
                    - chartBorder.BorderThickness.Left
                    - chartBorder.BorderThickness.Right;
                    canvas1.Height = canvasGrid.ActualHeight
                    - chartBorder.Margin.Top
                    - chartBorder.Margin.Bottom
                    - chartBorder.BorderThickness.Top
                    - chartBorder.BorderThickness.Bottom;

                    if (waterNetwork.listOfNodes.Count > 0)                    {
                       
                        int error = error = DrawWaterNetwork();
                    }
        }
        
        private void mainNetworkVisibility_Checked(object sender, RoutedEventArgs e)
        {
            if (waterNetwork.listOfNodes.Count > 0)
            {
                waterNetwork.isPlotted = true;
                int error = DrawWaterNetwork();
            }
        }

        private void mainNetworkVisibility_Unchecked(object sender, RoutedEventArgs e)
        {
            if (waterNetwork.listOfNodes.Count>0 )
            {
                waterNetwork.isPlotted = false;
                int error = DrawWaterNetwork();
            }
        }

        private void loggerNetworkVisibility_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (loggerConnections != null)
                {
                    //int error = loggerConnections.LoggerConn2WaterNet(efavorTest, "A", true);//2nd paremeter depends on combobox - important, 3rd param on checkbox - not important
                    int error;
                    if (loggerConnections.logger_water_network == null) //generate logger_water_network only if it doesn't exist
                    {
                        if (efavorTest != null) //if eFavor test data exists
                            error = loggerConnections.LoggerConn2WaterNet(efavorTest, inletSetSelectionComboBox.SelectedValue.ToString(), true);
                        else //if eFavor doesn't exist generate logger_water_network without it
                            error = loggerConnections.LoggerConn2WaterNet();
                    }
                    loggerConnections.logger_water_network.isPlotted = true;
                    error = DrawWaterNetwork();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in loggerNetworkVisibility_Checked: " + ex.Message);
            }           
        }

        private void loggerNetworkVisibility_Unchecked(object sender, RoutedEventArgs e)
        {
            if ((loggerConnections != null) && (loggerConnections.logger_water_network != null))
            {
                loggerConnections.logger_water_network.isPlotted = false;
                int error = DrawWaterNetwork();
            }
        }

        #region canvas1 events code
        private void canvas1_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(canvas1);

            //sent curent mouse position when over canvas1 to statusbar
            statusBarCoordinateX.Content = "X:" + mousePosition.X.ToString("f2");
            statusBarCoordinateY.Content = "Y:" + mousePosition.Y.ToString("f2");

            if (isMultiSelection)            
            {



                if (selectionSquareTopLeft.X < mousePosition.X)
            {
                Canvas.SetLeft(selectionBox, selectionSquareTopLeft.X);
                selectionBox.Width = mousePosition.X - selectionSquareTopLeft.X;
            }
            else
            {
                Canvas.SetLeft(selectionBox, mousePosition.X);
                selectionBox.Width = selectionSquareTopLeft.X - mousePosition.X;
            }

            if (selectionSquareTopLeft.Y < mousePosition.Y)
            {
                Canvas.SetTop(selectionBox, selectionSquareTopLeft.Y);
                selectionBox.Height = mousePosition.Y - selectionSquareTopLeft.Y;
            }
            else
            {
                Canvas.SetTop(selectionBox, mousePosition.Y);
                selectionBox.Height = selectionSquareTopLeft.Y - mousePosition.Y;
            }

            } 
        }
        private void canvas1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pointClicked = e.GetPosition(canvas1);
            tempHitList.Clear();

            VisualTreeHelper.HitTest(canvas1, null, new HitTestResultCallback(HitTestCallback), new PointHitTestParameters(pointClicked));//get all the elements under clicked point
        }
        private void canvas1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point mousePosition = e.GetPosition(canvas1);

            if (isMultiSelection)
            {

                tempHitList.Clear();

                RectangleGeometry geometry = new RectangleGeometry(new Rect(selectionSquareTopLeft, mousePosition));//define selection geometry

                HitTestResult hitResult = VisualTreeHelper.HitTest(canvas1, mousePosition);

                VisualTreeHelper.HitTest(canvas1, null, new HitTestResultCallback(HitTestCallbackGeometry), new GeometryHitTestParameters(geometry));

                isMultiSelection = false;
                canvas1.ReleaseMouseCapture();
                selectionBox.Visibility = Visibility.Collapsed;
                canvas1.Children.Remove(selectionBox);


                foreach (Shape s in tempHitList)
                {
                    Node nodeTemp = new Node();
                    Link linkTemp = new Link();

                    if (waterNetwork.listOfNodes.Count > 0)
                    {
                        nodeTemp = waterNetwork.listOfNodes.Find(tmp => tmp.name == s.Uid);
                        if (nodeTemp != null)
                        {
                            if (rectangleHitList.Any(tmp => ((Node)tmp.Tag) == nodeTemp)) //check if this node was already selected
                            {


                            }
                            else
                            {
                                if (placingLoggersTool) //logger placing tool is open so additional checks need to run before node can be selected
                                {
                                    //PS 22/07/2013: Allow loggers to be placed next to each other, but check if the node is in allowedLoggerLocations list if checkBox_AllowLoggersAnywhere is unchecked
                                    if ((loggerlocWindow.checkBox_AllowLoggersAnywhere.IsChecked == false) && (!loggerlocWindow.allowedLoggerLocations.Exists(tmp => tmp == nodeTemp)))
                                    {
                                        MessageBox.Show("Logger can't be placed at node " + nodeTemp.name + " as it is in the prohibited locations list. If you still want to place a logger here tick the \"Allow loggers anywhere\" checkbox. Refer to manual for further details.");
                                        return;
                                    }
                                    //if (Node.IsNeighbour(nodeTemp, rectangleHitList))
                                    //{
                                    //    MessageBox.Show("Loggers have to be separated by at least one non-logger node");
                                    //    return;
                                    //}
                                }
                                ChangeNodeApperance(nodeTemp,  ToBrush(UI.Default.SelectionNodeColor), UI.Default.SelectionNodeSize);
                                rectangleHitList.Add(nodeTemp.graphicalObject);

                                statusBarSelectedItem.Content = nodeTemp.name;
                                propertyGridForElement.SelectedObject = nodeTemp;

                                if (placingLoggersTool)
                                    loggerlocWindow.textBox_nAllocatedLog.Text = rectangleHitList.Count.ToString();
                            }

                        }
                        linkTemp = waterNetwork.listOfLinks.Find(tmp => tmp.name == s.Uid);
                        if ((linkTemp != null) && (!placingLoggersTool))
                        {
                            if (lineHitList.Any(tmp => ((Link)tmp.Tag) == linkTemp)) //check if this link was already selected
                            {

                            }
                            else
                            {
                                ChangeLinkApperance(linkTemp, ToBrush(UI.Default.SelectionLinkColor), UI.Default.SelectionLinkThickness);
                                lineHitList.Add((Line)linkTemp.graphicalObject);

                            }

                        }

                        if ((loggerConnections != null) && (loggerConnections.logger_water_network != null) && (!placingLoggersTool))
                        {
                            nodeTemp = null;
                            nodeTemp = loggerConnections.logger_water_network.listOfNodes.Find(tmp => tmp.name == s.Uid);
                            if (nodeTemp != null)
                            {
                                ChangeNodeApperance(nodeTemp, nodeTemp.color, UI.Default.LoggerSelectionNodeSize);
                                rectangleHitList.Add(nodeTemp.graphicalObject);

                                statusBarSelectedItem.Content = nodeTemp.name;
                                propertyGridForElement.SelectedObject = nodeTemp;
                            }
                            linkTemp = null;
                            linkTemp = loggerConnections.logger_water_network.listOfLinks.Find(tmp => tmp.name == s.Uid);
                            if (linkTemp != null && linkTemp.type == Constants.EN_LOG_CONN) //DP: 19//07/2013 Added "&& linkTemp.type==Constants.EN_LOG_CONN" to account for Bug 43
                            {
                                   var alreadyInList = pathHitList.Find(tmp=> tmp.Tag == linkTemp);// check whether logger conncection is already in pathHitList
                                   if (alreadyInList == null) //add if not
                                   {
                                       ChangeLinkApperance(linkTemp, linkTemp.color, linkTemp.thickness + UI.Default.SelectionLinkThickness);
                                       pathHitList.Add((System.Windows.Shapes.Path)linkTemp.graphicalObject);

                                       statusBarSelectedItem.Content = linkTemp.name;
                                   }
                            }
                        }
                    }

                }

            }
        }
        /// <summary>
        /// Zoom in/out using mouse wheel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas1_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {              
                if (waterNetwork.isPlotted || loggerConnections!=null) //DP: 19/07/2013 Added || loggerConnections!=null to account for Bug 41
                {
                    Point mousePosition = e.GetPosition(canvas1);
                    ScaleTransform scaleChange = new ScaleTransform();


                    scaleChange.CenterX = mousePosition.X;
                    scaleChange.CenterY = mousePosition.Y;

                    Point newMousePosition = new Point();

                    if (e.Delta > 0)
                    {
                        scaleChange.ScaleX = UI.Default.ZoomInStep;
                        scaleChange.ScaleY = UI.Default.ZoomInStep;

                        canvas1.Height = canvas1.Height * scaleChange.ScaleY;
                        canvas1.Width = canvas1.Width * scaleChange.ScaleX;
                        newMousePosition.X = mousePosition.X * UI.Default.ZoomInStep;
                        newMousePosition.Y = mousePosition.Y * UI.Default.ZoomInStep;
                    }

                    if (e.Delta < 0)
                    {
                        scaleChange.ScaleX = 1 / UI.Default.ZoomOutStep;
                        scaleChange.ScaleY = 1 / UI.Default.ZoomOutStep;

                        canvas1.Height = canvas1.Height * scaleChange.ScaleY;
                        canvas1.Width = canvas1.Width * scaleChange.ScaleX;

                        newMousePosition.X = mousePosition.X * 1 / UI.Default.ZoomOutStep;
                        newMousePosition.Y = mousePosition.Y * 1 / UI.Default.ZoomOutStep;
                    }


                    int error = DrawWaterNetwork(); //re-draw water network

                    //move Scrollviewer bars to centre at mouse position
                    scrollViewerCanvas1.ScrollToHorizontalOffset(scrollViewerCanvas1.ContentHorizontalOffset + newMousePosition.X - mousePosition.X);
                    scrollViewerCanvas1.ScrollToVerticalOffset(scrollViewerCanvas1.ContentVerticalOffset + newMousePosition.Y - mousePosition.Y);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

        }
        private void canvas1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            Point pointClicked = e.GetPosition(canvas1);

            // Define hit-testing area: 
            EllipseGeometry hitArea = new EllipseGeometry();
            hitArea = new EllipseGeometry(pointClicked, 10.0, 10.0);

            //hitList.Clear();

            if (singleSelection.IsChecked == true)
            {

                HitTestResult hitResult = VisualTreeHelper.HitTest(canvas1, pointClicked);

                VisualTreeHelper.HitTest(canvas1, null, new HitTestResultCallback(HitTestCallback), new PointHitTestParameters(pointClicked));
                //VisualTreeHelper.HitTest(canvas1, null, new HitTestResultCallback(HitTestCallback), new GeometryHitTestParameters(hitArea));


                if (hitResult != null)
                {
                    var hitElement = hitResult.VisualHit as UIElement;
                    statusBarSelectedItem.Content = hitElement.Uid;
                    if (hitResult.VisualHit.GetType() == typeof(Rectangle))
                    {

                        //check if element was already selected
                        Rectangle ifSelectedRectangle = new Rectangle();
                        ifSelectedRectangle = rectangleHitList.Find(temp => temp.Uid == hitElement.Uid);

                        //show selected node in propertygrid
                        propertyGridForElement.SelectedObject = (Node)((Rectangle)hitResult.VisualHit).Tag; //Properties.Settings.Default;//waterNetwork.listOfNodes[1];

                        if (ifSelectedRectangle != null)
                        {
                            if (((Node)ifSelectedRectangle.Tag).isLogger == true)
                            {
                                ChangeNodeApperance((Node)ifSelectedRectangle.Tag, ((Node)ifSelectedRectangle.Tag).color, UI.Default.LoggerNodeSize);
                            }
                            else
                            {
                                ChangeNodeApperance((Node)ifSelectedRectangle.Tag, ToBrush(UI.Default.StandardNodeColor), UI.Default.StandardNodeSize);
                            }


                            rectangleHitList.Remove(ifSelectedRectangle);

                            if (placingLoggersTool) //logger placing tool is open so additionally textBox string needs to update
                            {
                                loggerlocWindow.textBox_nAllocatedLog.Text = rectangleHitList.Count.ToString();
                            }
                        }
                        else
                        {
                            Node nodeTemp = ((Node)((Rectangle)hitResult.VisualHit).Tag);
                            if (nodeTemp.isPathNode) //if this is path node don't select it (just ignore it)
                                return;
                            if (placingLoggersTool) //logger placing tool is open so additional checks need to run before node can be selected
                            {
                                //check if this node actually belongs to logger_connections network and not to the actual network
                                if (nodeTemp.isLogger)
                                {
                                    return;
                                }
                                //PS 22/07/2013: Allow loggers to be placed next to each other, but check if the node is in allowedLoggerLocations list
                                if ((loggerlocWindow.checkBox_AllowLoggersAnywhere.IsChecked == false) && (!loggerlocWindow.allowedLoggerLocations.Exists(tmp => tmp == nodeTemp)))
                                {
                                    MessageBox.Show("Logger can't be placed at node " + nodeTemp.name + " as it is in the prohibited locations list. If you still want to place a logger here tick the \"Allow loggers anywhere\" checkbox. Refer to manual for further details.");
                                    return;
                                }
                                //check if any neighbour of this node is already selected
                                //if (Node.IsNeighbour((Node)((Rectangle)hitResult.VisualHit).Tag, rectangleHitList))
                                //{
                                //    MessageBox.Show("Loggers have to be separated by at least one non-logger node");
                                //    return;
                                //}

                            }
                            rectangleHitList.Add((Rectangle)hitResult.VisualHit);
                            if (placingLoggersTool)
                                loggerlocWindow.textBox_nAllocatedLog.Text = rectangleHitList.Count.ToString();
                            //mark all selected rectangles

                        }

                        foreach (Rectangle hitRectagle in rectangleHitList)
                        {
                            if (((Node)hitRectagle.Tag).isLogger == true)
                            {
                                ChangeNodeApperance((Node)hitRectagle.Tag, ((Node)hitRectagle.Tag).color, UI.Default.LoggerSelectionNodeSize);
                            }
                            else
                            {
                                ChangeNodeApperance((Node)hitRectagle.Tag, ToBrush(UI.Default.SelectionNodeColor), UI.Default.SelectionNodeSize);
                            }
                        }
                        //foreach (UIElement canvasElement in canvas1.Children)
                        //{
                        //    if (canvasElement.Uid == hitElement.Uid)
                        //    {
                        //        //MessageBox.Show(hitElement.Uid.ToString());
                        //    }
                        //}
                        //DependencyObject reec = hitResult.VisualHit;
                    }
                    else if (hitResult.VisualHit.GetType() == typeof(Line))
                    {
                        if (placingLoggersTool) //logger placing tool is open so links should not be selected
                            return;
                        //show selected link in propertygrid
                        propertyGridForElement.SelectedObject = (Link)((Line)hitResult.VisualHit).Tag;

                        //check if element was already selected
                        Line ifSelectedLine = new Line();
                        ifSelectedLine = lineHitList.Find(temp => temp.Uid == hitElement.Uid);

                        if (ifSelectedLine != null)
                        {
                            //ifSelectedLine.Stroke = Brushes.Black;
                            ChangeLinkApperance((Link)ifSelectedLine.Tag, ToBrush(UI.Default.StandardLinkColor), UI.Default.StandardLinkThickness);
                            lineHitList.Remove(ifSelectedLine);
                        }
                        else
                        {
                            lineHitList.Add((Line)hitResult.VisualHit);

                        }

                        foreach (Line hitLine in lineHitList)
                        {
                            //hitLine.Stroke = Brushes.Red;
                            ChangeLinkApperance((Link)hitLine.Tag, ToBrush(UI.Default.SelectionLinkColor), UI.Default.SelectionLinkThickness);
                        }
                        //foreach (UIElement canvasElement in canvas1.Children)
                        //{
                        //    if (canvasElement.Uid == hitElement.Uid)
                        //    {
                        //        //MessageBox.Show(hitElement.Uid.ToString());
                        //    }
                        //}

                    }
                    else if (hitResult.VisualHit.GetType() == typeof(System.Windows.Shapes.Path))
                    {
                        

                        //code to show selection of paths between loggers
                        
                        System.Windows.Shapes.Path ifSelectedPath = new System.Windows.Shapes.Path();
                        ifSelectedPath = pathHitList.Find(temp => temp.Uid == hitElement.Uid);

                        if (ifSelectedPath != null)
                        {
                            //ifSelectedLine.Stroke = Brushes.Black;
                            ChangeLinkApperance((Link)ifSelectedPath.Tag, ((Link)((System.Windows.Shapes.Path)hitResult.VisualHit).Tag).color, -UI.Default.SelectionLinkThickness);
                            pathHitList.Remove(ifSelectedPath);
                        }
                        else
                        {
                            if (((Link)((System.Windows.Shapes.Path)hitResult.VisualHit).Tag).type == Constants.EN_LOG_CONN) // DP: 19/07/2013 If not logger connection do not add to pathList. 
                            {
                                pathHitList.Add((System.Windows.Shapes.Path)hitResult.VisualHit);
                                ChangeLinkApperance((Link)((System.Windows.Shapes.Path)hitResult.VisualHit).Tag, ((Link)((System.Windows.Shapes.Path)hitResult.VisualHit).Tag).color, ((Link)((System.Windows.Shapes.Path)hitResult.VisualHit).Tag).thickness + UI.Default.SelectionLinkThickness);
                            }
                        }
                         

                    }



                }
            }
            else if (multipleSelection.IsChecked == true)
            {


                selectionBox.Visibility = Visibility.Collapsed;
                selectionBox.Stroke = Brushes.Gray;
                selectionBox.StrokeThickness = 1;
                selectionBox.StrokeDashArray.Add(2);
                selectionBox.StrokeDashArray.Add(1);

                // selectionBox.StrokeDashArray="2,1";


                isMultiSelection = true;
                selectionSquareTopLeft = pointClicked;
                canvas1.CaptureMouse();

                // Initial placement of the drag selection box.         
                Canvas.SetLeft(selectionBox, selectionSquareTopLeft.X);
                Canvas.SetTop(selectionBox, selectionSquareTopLeft.Y);
                selectionBox.Width = 0;
                selectionBox.Height = 0;
                canvas1.Children.Add(selectionBox);
                // Make the drag selection box visible.
                selectionBox.Visibility = Visibility.Visible;
            }

        }
        #endregion

        private void zoomOutButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (waterNetwork.isPlotted || loggerConnections != null)
            {

                //get centre of viewport of scrollviewer      
                var centerOfViewport = new Point(scrollViewerCanvas1.ViewportWidth / 2, scrollViewerCanvas1.ViewportHeight / 2);

                //relative centre to canvas
                var oldLocation = scrollViewerCanvas1.TranslatePoint(centerOfViewport, canvas1);

                //resize canvas
                canvas1.Height = canvas1.Height * 1 / UI.Default.ZoomOutStep; ;
                canvas1.Width = canvas1.Width * 1 / UI.Default.ZoomOutStep; ;

                //calculate new location
                Point newLocation = new Point(oldLocation.X * 1 / UI.Default.ZoomOutStep, oldLocation.Y * 1 / UI.Default.ZoomOutStep);
                var shift = newLocation - oldLocation;

                //draw network
                int error = DrawWaterNetwork();

                //move Scrollviewer bars to centre at desired position
                scrollViewerCanvas1.ScrollToHorizontalOffset(scrollViewerCanvas1.HorizontalOffset + shift.X);
                scrollViewerCanvas1.ScrollToVerticalOffset(scrollViewerCanvas1.VerticalOffset + shift.Y);
            }

        }

        private void zoomInButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (waterNetwork.isPlotted || loggerConnections != null)
            {
                //get centre of viewport of scrollviewer      
                var centerOfViewport = new Point(scrollViewerCanvas1.ViewportWidth / 2, scrollViewerCanvas1.ViewportHeight / 2);

                //relative centre to canvas
                var oldLocation = scrollViewerCanvas1.TranslatePoint(centerOfViewport, canvas1);

                //resize canvas
                canvas1.Height = canvas1.Height * UI.Default.ZoomInStep; ;
                canvas1.Width = canvas1.Width * UI.Default.ZoomInStep; ;

                //calculate new location
                Point newLocation = new Point(oldLocation.X * UI.Default.ZoomInStep, oldLocation.Y * UI.Default.ZoomInStep);
                var shift = newLocation - oldLocation;

                //draw network
                int error = DrawWaterNetwork();

                //move Scrollviewer bars to centre at desired position
                scrollViewerCanvas1.ScrollToHorizontalOffset(scrollViewerCanvas1.HorizontalOffset + shift.X);
                scrollViewerCanvas1.ScrollToVerticalOffset(scrollViewerCanvas1.VerticalOffset + shift.Y);
            }
        }        
      
        private void multipleSelection_Checked(object sender, RoutedEventArgs e)
        {
            singleSelection.IsChecked = false; //switch off single selection 
        }

        private void singleSelection_Checked(object sender, RoutedEventArgs e)
        {
            multipleSelection.IsChecked = false;//switch off multiple selection 
        }

        /// <summary>
        /// Unselect all elements
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void unselectAllContextMenu_Click(object sender, RoutedEventArgs e)
        {
            //back to standard color
            foreach (Rectangle rectangle in rectangleHitList)
            {

                if (((Node)rectangle.Tag).isLogger == true)
                {
                    ChangeNodeApperance((Node)rectangle.Tag, ((Node)rectangle.Tag).color, UI.Default.LoggerSelectionNodeSize);
                }
                else
                {
                    ChangeNodeApperance((Node)rectangle.Tag, ToBrush(UI.Default.StandardNodeColor), UI.Default.StandardNodeSize);
                }

                //if (((Node)ifSelectedRectangle.Tag).isLogger == true)
                //{
                //    ChangeNodeApperance((Node)ifSelectedRectangle.Tag, ((Node)ifSelectedRectangle.Tag).color, constants.loggerNodeSize);
                //}
                //else
                //{
                //    ChangeNodeApperance((Node)ifSelectedRectangle.Tag, constants.standardNodeColor, constants.standardNodeSize);
                //}


                //ChangeNodeApperance((Node)rectangle.Tag, constants.standardNodeColor, constants.standardNodeSize);
            }
            rectangleHitList.Clear();//clear list of selected rectangles
            if (placingLoggersTool)
                loggerlocWindow.textBox_nAllocatedLog.Text = "0";

            //back to standard color and thickness
            foreach (Line line in lineHitList)
            {
                ChangeLinkApperance((Link)line.Tag, ToBrush(UI.Default.StandardLinkColor), UI.Default.StandardLinkThickness);
            }
            lineHitList.Clear();//clear list of selected rectangles

            foreach (System.Windows.Shapes.Path path in pathHitList)
            {
                ChangeLinkApperance((Link)path.Tag, ((Link)path.Tag).color, ((Link)path.Tag).thickness - UI.Default.SelectionLinkThickness); // StandardLinkThickness
            }
            pathHitList.Clear();

            //int error = DrawWaterNetwork();

        }

        private void selectItemComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> ls = new List<string>();
            if (tempHitList.Count > 0)
            {
                foreach (Shape s in tempHitList)
                {
                    ls.Add(s.Uid);
                }
                selectItemComboBox.ItemsSource = ls;
            }
        }

        private void selectItemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Node nodeTemp = new Node();
            Link linkTemp = new Link();

            if (waterNetwork.listOfNodes.Count > 0)
            {
                nodeTemp = waterNetwork.listOfNodes.Find(tmp => tmp.name == (string)selectItemComboBox.SelectedItem);
                if (nodeTemp != null)
                {
                    if (rectangleHitList.Any(tmp => ((Node)tmp.Tag) == nodeTemp)) //check if this node was already selected
                        return;
                    //Rectangle ifSelectedRectangle = new Rectangle();
                    //ifSelectedRectangle = rectangleHitList.Find(temp => temp.Uid == hitElement.Uid);
                    if (placingLoggersTool) //logger placing tool is open so additional checks need to run before node can be selected
                    {
                        //PS 22/07/2013: Allow loggers to be placed next to each other, but check if the node is in allowedLoggerLocations list
                        if ((loggerlocWindow.checkBox_AllowLoggersAnywhere.IsChecked == false) && (!loggerlocWindow.allowedLoggerLocations.Exists(tmp => tmp == nodeTemp)))
                        {
                            MessageBox.Show("Logger can't be placed at node " + nodeTemp.name + " as it is in the prohibited locations list. If you still want to place a logger here tick the \"Allow loggers anywhere\" checkbox. Refer to manual for further details.");
                            return;
                        }
                        //if (Node.IsNeighbour(nodeTemp, rectangleHitList))
                        //{
                        //    MessageBox.Show("Loggers have to be separated by at least one non-logger node");
                        //    return;
                        //}
                    }
                    ChangeNodeApperance(nodeTemp,ToBrush(UI.Default.SelectionNodeColor), UI.Default.SelectionNodeSize);
                    rectangleHitList.Add(nodeTemp.graphicalObject);

                    statusBarSelectedItem.Content = nodeTemp.name;
                    propertyGridForElement.SelectedObject =nodeTemp;

                    if (placingLoggersTool)
                        loggerlocWindow.textBox_nAllocatedLog.Text = rectangleHitList.Count.ToString();
                }
                linkTemp = waterNetwork.listOfLinks.Find(tmp =>  tmp.name == (string)selectItemComboBox.SelectedItem);
                if ((linkTemp != null) && (!placingLoggersTool))
                {                    
                    if (lineHitList.Any(tmp => ((Link)tmp.Tag) == linkTemp)) //check if this link was already selected
                        return;
                    ChangeLinkApperance(linkTemp, ToBrush(UI.Default.SelectionLinkColor), UI.Default.SelectionLinkThickness);
                    lineHitList.Add((Line)linkTemp.graphicalObject);
                }

                if ((loggerConnections != null) && (loggerConnections.logger_water_network != null) && (!placingLoggersTool))
                {
                    nodeTemp = null;
                    nodeTemp = loggerConnections.logger_water_network.listOfNodes.Find(tmp => tmp.name == (string)selectItemComboBox.SelectedItem);
                    if (nodeTemp != null)
                    {
                        ChangeNodeApperance(nodeTemp, nodeTemp.color, UI.Default.LoggerSelectionNodeSize);
                        rectangleHitList.Add(nodeTemp.graphicalObject);

                        statusBarSelectedItem.Content = nodeTemp.name;
                        propertyGridForElement.SelectedObject = nodeTemp;
                    }
                    linkTemp = null;
                    linkTemp = loggerConnections.logger_water_network.listOfLinks.Find(tmp => tmp.name == (string)selectItemComboBox.SelectedItem);

                    if (linkTemp != null && linkTemp.type == Constants.EN_LOG_CONN) //DP: 19//07/2013 Added "&& linkTemp.type==Constants.EN_LOG_CONN" to account for Bug 43
                    {
                        var alreadyInList = pathHitList.Find(tmp=> tmp.Tag == linkTemp);// check whether logger conncection is already in pathHitList
                        if (alreadyInList == null) //add if not
                        {
                            ChangeLinkApperance(linkTemp, linkTemp.color, linkTemp.thickness + UI.Default.SelectionLinkThickness);
                            pathHitList.Add((System.Windows.Shapes.Path)linkTemp.graphicalObject);

                            statusBarSelectedItem.Content = linkTemp.name;
                        }
                    }
                }
            }

        }
        
        private void inletSetSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            loggerConnections.LoggerConn2WaterNet(efavorTest, inletSetSelectionComboBox.SelectedValue.ToString(), true);
            if (checkbox_loggerNetworkVisibility.IsChecked == true)
            {
                checkbox_loggerNetworkVisibility.IsChecked = false;
                checkbox_loggerNetworkVisibility.IsChecked = true;
            }
        }

        /// <summary>
        /// Prints contents of canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void printButton_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog dialog = new PrintDialog();
            try
            {
                if (dialog.ShowDialog() == true)
                {
                    dialog.PrintVisual(canvas1, "Network");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            } 
        }

        /// <summary>
        /// Select all the nodes from the network
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void selectAllNodesContextMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                rectangleHitList.Clear();//clear list of selected rectangles
                foreach (Node node in waterNetwork.listOfNodes)
                {
                    ChangeNodeApperance(node, ToBrush(UI.Default.SelectionNodeColor), UI.Default.SelectionNodeSize);
                    rectangleHitList.Add(node.graphicalObject);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }


        /// <summary>
        /// Opens a window to show logger connection's paths
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showPathsContextMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (pathsWindow != null) //path window already open so we need to close it
                    pathsWindow.Close();

                //List<List<Node>> tmpListPaths = loggerConnections.GetAllPathsBetweenLoggers("2", "3");
                //for (int i = 0; i < tmpListPaths.Count; i++)
                //{
                //    for (int j = i + 1; j < tmpListPaths.Count; j++)
                //    {
                //        List<Node> path1 = tmpListPaths[i];
                //        List<Node> path2 = tmpListPaths[j];
                        
                //        if (path1.Count != path2.Count) //node count different, move to next path
                //            continue;
                //        for (int k = 0; k < path1.Count; k++)
                //        {
                //            if (path1[k] != path2[k]) //different node so these 2 paths are not identical
                //                break;
                //            if (k == path1.Count - 1) //this is last element and so all nodes were identical
                //            {
                //                MessageBox.Show("Path " + i.ToString() + " identical to Path " + j.ToString());
                //            }                            
                //        }
                //    }
                //}

                //set UI flags
                pathsNetwork = new WaterNetwork();
                pathsNetwork.isPlotted = true;
                pathsNetwork.waterNetworkType = Constants.PATH_NETWORK;

                listOfAllLoggersPaths = ConvertToListofLoggerPaths();
                
                pathsWindow = new PathsWindow();
                pathsWindow.Show();
                pathsWindow.Owner = mainWindow;

                ObservableCollection<SubNode> nodes = new ObservableCollection<SubNode>();
                SubNode lastNode = null;

                //create tree of logger connections and paths
                foreach (LoggerPaths loggerLink in listOfAllLoggersPaths)
                {
                    SubNode node = new SubNode(loggerLink.loggerConnectionName);
                    foreach (WaterNetwork path in loggerLink.paths)
                    {
                        lastNode = new SubNode(path.title[0]);
                        node.Add(lastNode);
                    }
                    lastNode = node;
                    nodes.Add(node);
                }

                pathsWindow.multiSelectTreeView.treeView1.ItemsSource = nodes;
                pathsWindow.multiSelectTreeView.treeView1.SelectItem(lastNode);


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

        }

        /// <summary>
        /// Create a list of logger paths containing information about each logger connection and its paths
        /// </summary>
        /// <returns></returns>
        private List<LoggerPaths> ConvertToListofLoggerPaths()
        {

            List<LoggerPaths> listOfAllPaths = new List<LoggerPaths>();///List of all paths for selected logger connections
            List<List<Node>> listOfPathsNodes = new List<List<Node>>();///to get list of path's nodes for selected logger connection
            //PS: 19/07/2013 because of fucking EPANET link having nodeFrom/To as int (which we luckily don't have in our newer projects)
            //Link class was augmented with strings nodeFromName and nodeToName, otherwise indexing changes as multiple selection is made in PathsWindows, 
            //nodeFrom/To indexes for some links is no loger valid and the whole visualisation of paths turns into shit

            string startLoggerName;///start node of logger connection
            string endLoggerName;///end node of logger connection

            foreach (System.Windows.Shapes.Path loggerConnection in pathHitList)
            {

               

                    LoggerPaths loggerConnectionPaths = new LoggerPaths();

                    loggerConnectionPaths.loggerConnectionName = ((Link)(loggerConnection.Tag)).name;///store logger connection name

                    startLoggerName = loggerConnections.list_of_loggers[((Link)(loggerConnection.Tag)).nodeFrom].logger_id;
                    endLoggerName = loggerConnections.list_of_loggers[((Link)(loggerConnection.Tag)).nodeTo].logger_id;
                    listOfPathsNodes = loggerConnections.GetAllPathsBetweenLoggers(startLoggerName, endLoggerName);



                    foreach (List<Node> listOfNodes in listOfPathsNodes)
                    {
                        WaterNetwork path = new WaterNetwork(); ///each path is stored as water network object
                        path.listOfNodes = listOfNodes;

                        path.title = new string[3];
                        path.title[0] = loggerConnectionPaths.loggerConnectionName + ":Path:" + listOfPathsNodes.IndexOf(listOfNodes).ToString();///path name

                        List<Link> pathLinks = new List<Link>();

                        for (int i = 0; i < listOfNodes.Count - 1; i++)
                        {
                            //PS: 16/07/2013: instead of adding link from main waterNetwork create a new link for given nodes From and To; this is to have links with direction of arrows consistent with order of nodes given in listOfNodes; still, if the link was not found in the main waterNetwork it will not be created in path.listOfLinks (to prevent visualising non-existing links in case when the last part of path includes unsorted nodes which potentially may appear when high neighbourhood level is used when generating logger connections)
                            Link foundLink = waterNetwork.listOfLinks.Find(tmp => (tmp.nodeFrom == waterNetwork.listOfNodes.IndexOf(listOfNodes[i]) && tmp.nodeTo == waterNetwork.listOfNodes.IndexOf(listOfNodes[i + 1])) || (tmp.nodeFrom == waterNetwork.listOfNodes.IndexOf(listOfNodes[i + 1]) && tmp.nodeTo == waterNetwork.listOfNodes.IndexOf(listOfNodes[i])));
                            if (foundLink != null)
                            {
                                //pathLinks.Add(foundLink);
                                Link newPathLink = new Link(foundLink);
                                newPathLink.nodeFromName = listOfNodes[i].name;
                                newPathLink.nodeToName = listOfNodes[i + 1].name;

                               


                                //newPathLink.nodeFrom = i;
                                //newPathLink.nodeTo = i + 1;
                                pathLinks.Add(newPathLink);
                            }

                        }
                        path.listOfLinks = pathLinks;
                        loggerConnectionPaths.paths.Add(path);
                    }

                    listOfAllPaths.Add(loggerConnectionPaths);
                
            }

            return listOfAllPaths;
        }

        /// <summary>
        /// Opens options window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Options optionsWindow = new Options();
            optionsWindow.Owner = mainWindow;
            optionsWindow.ShowDialog();
        }


        /// <summary>
        /// Opens methodology description window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void instructionMenu_Click(object sender, RoutedEventArgs e)
        {
            Methodology methodologyWindow = new Methodology();
            methodologyWindow.ShowDialog();

        }

        /// <summary>
        /// Ensures that application is shutdown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainWindow_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown();
            //Application.Current.Shutdown();
        }

        /// <summary>
        /// Perfroms operations on mainWindow closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainWindow_Closing(object sender, CancelEventArgs e)
        {
            //base.OnClosing(e);

            if (MessageBox.Show("Are you sure you want to close?", "Close software?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                e.Cancel = true;
            else
            {
                UserInterface.Default.Save(); //save user interface settings                           

                if (epanet != null)
                {
                    int error = epanet.CloseENToolkit();
                    if (error != 0)
                        MessageBox.Show("Error while closing EPANET toolkit: " + error.ToString());
                }
            }
        }

        /// <summary>
        /// Controls Show Paths appearance from canvas context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas1_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (pathHitList.Count != 0 && checkbox_loggerNetworkVisibility.IsChecked == true) //Show Paths enabled only when pathitlist !=0 and logger connections are visible
            {
                showPathsContextMenu.IsEnabled = true;
                selectPathsNodesContextMenu.IsEnabled = true;
            }
            else
            {
                showPathsContextMenu.IsEnabled = false;
                selectPathsNodesContextMenu.IsEnabled = false;
            }
        }

        /// <summary>
        /// Zoom out water network to full extent 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fullExtentButton_Click(object sender, RoutedEventArgs e)
        {
            canvas1.Height = scrollViewerCanvas1.ViewportHeight;
            canvas1.Width = scrollViewerCanvas1.ViewportWidth;

            //redraw network
            int error = DrawWaterNetwork();
        }

        #region Additional supporting methods

        /// <summary>
        /// Change node graphical attributies 
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="nodeColor">Color</param>
        /// <param name="nodeSize">Size</param>
        public void ChangeNodeApperance(Node node, Brush nodeColor, double nodeSize)
        {
            node.graphicalObject.Fill = nodeColor;
            node.graphicalObject.Width = nodeSize;
            node.graphicalObject.Height = nodeSize;
            //node.graphicalObject.InvalidateVisual(); unnecessary ? 
            //save changed attributes
            node.color = nodeColor;
            node.size = nodeSize;
        }

        /// <summary>
        /// Change link graphical attributies 
        /// </summary>
        /// <param name="link">Link</param>
        /// <param name="linkColor">Color</param>
        /// <param name="linkThickness">Thickenss</param>
        public void ChangeLinkApperance(Link link, Brush linkColor, double linkThickness)
        {
            link.graphicalObject.Stroke = linkColor;
            //save changed attributes
            link.color = linkColor;

            if (linkThickness > 0)
            {
                link.graphicalObject.StrokeThickness = linkThickness;
                link.thickness = linkThickness;
            }
            else if (linkThickness < 0)
            {
                link.graphicalObject.StrokeThickness = link.graphicalObject.StrokeThickness + linkThickness;
                link.thickness = link.thickness + linkThickness;
            }
            //link.graphicalObject.InvalidateVisual();



        }

        /// <summary>
        /// Draws line with arrow in the middle
        /// </summary>
        /// <param name="p1">Start point</param>
        /// <param name="p2">End point</param>
        /// <param name="lineColor">Line color</param>
        /// <param name="lineThickenss">Line thickness</param>
        /// <returns>Arrow line</returns>
        private static Shape DrawLinkArrow(Point p1, Point p2, Brush lineColor, double lineThickenss)
        {
            GeometryGroup lineGroup = new GeometryGroup();
            double theta = Math.Atan2((p2.Y - p1.Y), (p2.X - p1.X)) * 180 / Math.PI;

            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            Point p = new Point(p1.X + ((p2.X - p1.X) / 1.9), p1.Y + ((p2.Y - p1.Y) / 1.9)); //1.9 offset for arrow localisation, set to 1 for arrow at destination node
            pathFigure.StartPoint = p;

            Point lpoint = new Point(p.X + 4, p.Y + 10); //4 and 10 are arrows size, width and height respectively 
            Point rpoint = new Point(p.X - 4, p.Y + 10);
            LineSegment seg1 = new LineSegment();
            seg1.Point = lpoint;
            pathFigure.Segments.Add(seg1);

            LineSegment seg2 = new LineSegment();
            seg2.Point = rpoint;
            pathFigure.Segments.Add(seg2);

            LineSegment seg3 = new LineSegment();
            seg3.Point = p;
            pathFigure.Segments.Add(seg3);

            pathGeometry.Figures.Add(pathFigure);
            RotateTransform transform = new RotateTransform();
            transform.Angle = theta + 90;
            transform.CenterX = p.X;
            transform.CenterY = p.Y;
            pathGeometry.Transform = transform;
            lineGroup.Children.Add(pathGeometry);

            LineGeometry connectorGeometry = new LineGeometry();
            connectorGeometry.StartPoint = p1;
            connectorGeometry.EndPoint = p2;
            lineGroup.Children.Add(connectorGeometry);
            System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
            path.Data = lineGroup;
            path.StrokeThickness = lineThickenss;
            path.Stroke = path.Fill = lineColor;

            return path;
        }

        /// <summary>
        /// Normalize object x coordinate in canvas
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private double XNormalize(double x)
        {
            double result = (x - xMin) * canvas1.Width / (xMax - xMin);
            return result;
        }
        /// <summary>
        /// Normalize object y coordinate in canvas
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        private double YNormalize(double y)
        {
            double result = canvas1.Height - (y - yMin) * canvas1.Height / (yMax - yMin);
            return result;
        }
        private TextBox createTexblock(string ID, double value)
        {
            TextBox t = new TextBox();
            t.Name = "burstTextBlock" + ID;
            t.Uid = "burstTextBlock" + ID;
            t.Text = value.ToString("f3");
            t.HorizontalAlignment = HorizontalAlignment.Left;
            t.VerticalAlignment = VerticalAlignment.Center;

            //b.Click += new EventHandler(Button_Click);
            //b.OnClientClick = "ButtonClick('" + b.ClientID + "')";
            return t;
        }
        private Label createLabel(string ID, int value)
        {
            Label l = new Label();
            l.Name = "burstLabel" + ID;
            l.Uid = "burstLabel" + ID;
            l.Content = value.ToString();
            l.HorizontalAlignment = HorizontalAlignment.Right;
            l.VerticalAlignment = VerticalAlignment.Center;
            //b.Click += new EventHandler(Button_Click);
            //b.OnClientClick = "ButtonClick('" + b.ClientID + "')";
            return l;

        }

        /// <summary>
        /// Converts Sytem.Drawing.Color type to Brush type
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public Brush ToBrush(System.Drawing.Color color)
        {
            return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        #endregion

        #endregion
        //======================================================================================================================================================
        #region                  ENGINE METHODS
        //======================================================================================================================================================


        private void Button_loadEfavor_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog file_dialog = new OpenFileDialog();
            //file_dialog.InitialDirectory = Settings.Default.path;
            file_dialog.Title = "Load E-Favor experiment data from Excel file";
            file_dialog.Filter = "Excel Files |*.xls";
            if (file_dialog.ShowDialog() != true)
                return;
            try
            {
                int error;
                
                //moved to run immediately after network is loaded:
                //error = waterNetwork.GenerateNeighboursLists(AdvancedOptions.zero_flow_tolerance);
                //if (error < 0)
                //    throw new Exception("Error while analysing node neighbours!");
                //error = waterNetwork.GenerateHigherLevelNeighbours(AdvancedOptions.max_higher_level_neighbours);
                //if (error < 0)
                //    throw new Exception("Error while analysing higher level node neighbours!");
                efavorTest = new EFavorTest();
                error = efavorTest.LoadEFavorData(file_dialog.FileName, waterNetwork);
                if (error < 0)
                    throw new Exception("Error while loading experiment data from Excel file: " + file_dialog.FileName);

                //check loggers proximity
                if ((AdvancedOptions.loggerNeighbourhoodLevel > 1) && (LoggerConnections.CheckLoggersProximity(efavorTest.list_of_loggers)))
                {
                    if (MessageBox.Show("There are some loggers next to each other and with head difference smaller than the tolerance parameter, but the logger neighbourhood parameter is greater than 1. It is recommended to change the parameters or the logger locations. Do you want to continue calculating logger connections (YES), or to stop and modify the parameters (NO)?",
                        "Continue?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    {
                        efavorTest = null;
                        return;
                    }
                }

                button_LoggerLocTool.IsEnabled = false; //Once E-Favor data was loaded logger location tool can't be used; load Epanet model again to enable this button
                button_LoggerLocTool.ToolTip = "Once E-Favor data was loaded logger location tool can't be used; load Epanet model again to enable this button.";

                //if path highlighting window is open close it as we're redefining all logger connections and paths
                if (mainWindow.pathsWindow != null)
                    mainWindow.pathsWindow.Close();
                //clear the list in case some logger connections were highlighed
                mainWindow.pathHitList.Clear();

                //Generate flow tree for the loaded waterNetwork and inlets given in experiment data file
                List<Node> inlet_nodes = efavorTest.list_of_inlet_loggers.ConvertAll<Node>(new Converter<Logger, Node>(Logger.GetLoggerNode));
                if (inlet_nodes.Count == 0)
                    throw new Exception("This network does not seem to have any PRV inlets!");
                FlowTree flow_tree = new FlowTree(waterNetwork, inlet_nodes);
                error = flow_tree.GenerateFlowTree();
                if (error < 0)
                    throw new Exception("Error while analysing network flow paths!");

                //Analyse logger connections
                loggerConnections = new LoggerConnections(efavorTest.list_of_loggers, flow_tree);
                error = loggerConnections.CalculatePathsBetweenLoggers(AdvancedOptions.head_diff_tolerance);
                if (error < 0)
                    throw new Exception("Error while skeletonizing the network!");
                
                //IMPORTANT activate combo box
                inletSetSelectionComboBox.IsEnabled = true;
                inletSetSelectionComboBox.ItemsSource = efavorTest.list_of_inlet_set_ids;
                inletSetSelectionComboBox.SelectedIndex = 0;

                checkbox_loggerNetworkVisibility.IsChecked = false;
                checkbox_loggerNetworkVisibility.IsChecked = true;
                Button_suspectedLoggers.IsEnabled = true;
                                
                //create textboxes etc to enter/estimate burst coefficients
                burstFlowEstimationGrid.Children.Clear();
                burstFlowEstimationGrid.ColumnDefinitions.Clear();
                burstFlowEstimationGrid.RowDefinitions.Clear();                    

                ColumnDefinition colDef1 = new ColumnDefinition();
                ColumnDefinition colDef2 = new ColumnDefinition();
                
                burstFlowEstimationGrid.ColumnDefinitions.Add(colDef1);
                burstFlowEstimationGrid.ColumnDefinitions.Add(colDef2);
                                
                for (int i = 0; i < efavorTest.total_no_of_pressure_steps + 1; i++)
                {
                    burstFlowEstimationGrid.RowDefinitions.Add(new RowDefinition());
                }
                                
                //create headers
                TextBlock column0Header = new TextBlock();
                column0Header.Text = "PRV Step No.";
                column0Header.Width = 50;
                column0Header.TextWrapping = TextWrapping.Wrap;
                column0Header.HorizontalAlignment = HorizontalAlignment.Right;
                column0Header.VerticalAlignment = VerticalAlignment.Center;
                column0Header.TextAlignment = TextAlignment.Right;

                Grid.SetColumn(column0Header, 0);
                Grid.SetRow(column0Header, 0);
                burstFlowEstimationGrid.Children.Add(column0Header);
                
                Label column1Header = new Label();
                column1Header.Content = "Burst flow";
                column1Header.HorizontalAlignment = HorizontalAlignment.Left;
                column1Header.VerticalAlignment = VerticalAlignment.Center;

                Grid.SetColumn(column1Header, 1);
                Grid.SetRow(column1Header, 0);
                burstFlowEstimationGrid.Children.Add(column1Header);
                
                for (int i = 0; i < efavorTest.total_no_of_pressure_steps; i++)
                {
                    double textBoxValue = 1.0;
                    
                    Label prvStepNo = new Label();
                    prvStepNo = createLabel(i.ToString(), i + 1);

                    TextBox burstFlow = new TextBox();
                    burstFlow = createTexblock(i.ToString(), textBoxValue);

                    Grid.SetColumn(prvStepNo, 0);
                    Grid.SetRow(prvStepNo, i + 1);

                    Grid.SetColumn(burstFlow, 1);
                    Grid.SetRow(burstFlow, i + 1);

                    burstFlowEstimationGrid.Children.Add(prvStepNo);
                    burstFlowEstimationGrid.Children.Add(burstFlow);
                }                   
                estimateButton.IsEnabled = true;
                button_LocalizeBurst.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }         
        }

        /// <summary>
        /// Recatngle object comparer by Tag fields.
        /// </summary>
        class CompareRectangleByTags : IEqualityComparer<Rectangle>
        {
            public bool Equals(Rectangle x, Rectangle y)
            {
                return x.Tag.Equals(y.Tag);
            }

            public int GetHashCode(Rectangle obj)
            {
                return obj.Tag.GetHashCode();
            }
        }


        private void button_LocalizeBurst_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BurstCoeffs burstCoefficients = new BurstCoeffs();
                burstCoefficients.est_burst_flow = new double[efavorTest.total_no_of_pressure_steps];

                int counter = 0;
                foreach (UIElement gridElement in burstFlowEstimationGrid.Children)
                {
                    if (gridElement.GetType() == typeof(TextBox))
                    {
                        //((TextBox)gridElement).Text = burstCoefficients.est_burst_flow[counter++].ToString("F3");
                        bool parse_ok;
                        double burst_flow;
                        parse_ok = double.TryParse(((TextBox)gridElement).Text, out burst_flow);
                        if (!parse_ok)
                            throw new Exception("Value of burst flow: " + ((TextBox)gridElement).Text + " is not recognized as number");
                        burstCoefficients.est_burst_flow[counter++] = burst_flow;
                        if (counter - 1 > burstCoefficients.est_burst_flow.Count())
                            throw new Exception("Length of burstCoefficients.est_burst_flow does not match number of textboxes in burstFlowEstimationGrid");                        
                    }
                }
                List<Node> listSuspectedNodes = new List<Node>();

                var rectangleHitListNoDups = rectangleHitList.Distinct(new CompareRectangleByTags()).ToList();//DP 27/07/13 Remove duplicates from rectanglehitlist                                               


                foreach (Rectangle rectangle in rectangleHitListNoDups)
                {
                    Node node = (Node)rectangle.Tag;
                    //make sure that this node belongs to main waterNetwork
                    if (waterNetwork.listOfNodes.Any(tmp => tmp == node))
                        listSuspectedNodes.Add(node);
                }
                if (listSuspectedNodes.Count == 0)
                    throw new Exception("Empty list of nodes to test for burst");

                //if _max_n_bursts == 1 then we do complete overview (test every node from listSuspectedNodes);
                //if _max_n_bursts > 1 then we run GA 
                if (_max_n_bursts == 1)
                {
                    double[][] chi2matrix;
                    BurstLocator burstLocator = new BurstLocator();
                    int ret_val = burstLocator.SimSeries_SingleBurst_Maxd2h(loggerConnections, efavorTest, epanet, listSuspectedNodes, burstCoefficients, true, inletSetSelectionComboBox.SelectedValue.ToString(), AdvancedOptions.zero_d2h_tolerance, out chi2matrix);
                    if (ret_val < 0)
                        throw new Exception("Error while simulating burst at the selected nodes");

                    List<double> chi2_aggregate = new List<double>();
                    for (int i = 0; i < chi2matrix.Length; i++)
                    {
                        chi2_aggregate.Add(chi2matrix[i].Sum());
                        if (double.IsNaN(chi2_aggregate[i]))
                            chi2_aggregate[i] = double.PositiveInfinity;
                    }
                    List<Tuple<string, double>> listview_items = new List<Tuple<string, double>>();
                    for (int i = 0; i < chi2_aggregate.Count; i++)

                    {
                        listview_items.Add(new Tuple<string, double>(listSuspectedNodes[i].name, chi2_aggregate[i]));
                    }

                    //var noDuplicates = listview_items.Distinct().ToList(); //DP 26/07/13 Added to eliminates duplicates in list of selected rectangles. DP 27/07/13 Removed because rectanglehitlist is already deduplicated.
                   
                    unselectAllContextMenu_Click(sender, e);
                    DrawWaterNetwork();

                    WindowTest chi2_window = new WindowTest(); //window with results as data grid

                    chi2_window.Show();
                    chi2_window.Owner = mainWindow;
                    chi2_window.dataGrid1.ItemsSource = listview_items;
                }
                else if (_max_n_bursts > 1)
                {
                    //TEST:
                    BurstLocator burstLocator = new BurstLocator();
                    int ret_val = burstLocator.TEMP_ManyBurstsSim(loggerConnections, efavorTest, epanet, listSuspectedNodes, burstCoefficients);
                    if (ret_val < 0)
                        throw new Exception("Error while simulating burst at the selected nodes");
                    MessageBox.Show("GA" + ret_val.ToString());                   
                }
                else
                    MessageBox.Show("Max number of bursts to search must be at least one!");
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
                
        
        private void estimateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int ret_val;
                BurstCoeffEstimator burstCoeffEst = new BurstCoeffEstimator();
                ret_val = burstCoeffEst.InitialiseCoeffs(efavorTest, 70);
                if (ret_val < 0)
                    throw new Exception("Error while initialising burst coefficients");
                ret_val = burstCoeffEst.EstimateBurstFromLoggers_2Term(epanet, waterNetwork, efavorTest, AdvancedOptions.lsqr_estimation_tolerance, AdvancedOptions.lsqr_estimation_iterations, AdvancedOptions.max_diff_from_min_chi2_percent, AdvancedOptions.max_no_min_chi2);
                if (ret_val < 0)
                    throw new Exception("Error while estimating burst coefficients");

                //update textbox values
                int counter = 0;
                foreach (UIElement gridElement in burstFlowEstimationGrid.Children)
                {
                    if (gridElement.GetType() == typeof(TextBox))
                    {
                        ((TextBox)gridElement).Text = burstCoeffEst.coeffs.est_burst_flow[counter++].ToString("F3");
                        if (counter - 1 > burstCoeffEst.coeffs.est_burst_flow.Count())
                            throw new Exception("Length of coeffs.est_burst_flow does not match number of textboxes in burstFlowEstimationGrid");
                        //MessageBox.Show(((TextBox)gridElement).Text);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
                        
        }


        private void button3_LoggerLocToolClick(object sender, RoutedEventArgs e)
        {
            try
            {

                singleSelection.IsChecked = true; //initiate selection mode
                singleSelection.IsEnabled = false; //disable selection buttons
                multipleSelection.IsEnabled = false;
                unselectAllContextMenu_Click(sender, e);
                
                //moved to run immediately after network is loaded:
                //int error = waterNetwork.GenerateNeighboursLists(AdvancedOptions.zero_flow_tolerance);
                //if (error < 0)
                //    throw new Exception("Error while analysing node neighbours!");
                //error = waterNetwork.GenerateHigherLevelNeighbours(AdvancedOptions.max_higher_level_neighbours);
                //if (error < 0)
                //    throw new Exception("Error while analysing higher level node neighbours!");
                
                List<Valve> listPRVs = waterNetwork.listOfValves.FindAll(tmp => tmp.link.type == Constants.EN_PRV);
                List<Node> inlet_nodes = new List<Node>();
                foreach (Valve prv in listPRVs)
                {
                    inlet_nodes.Add(waterNetwork.listOfNodes[prv.link.nodeTo]);
                }
                if (inlet_nodes.Count == 0)
                    throw new Exception("This network does not seem to have any PRV inlets!");

                loggerlocWindow = new LoggerLocWindow(inlet_nodes);
                loggerlocWindow.Owner = mainWindow;
                loggerlocWindow.Initialise();
                loggerlocWindow.Show();
                placingLoggersTool = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        #endregion


        private void button_TEMP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BurstCoeffs burstCoefficients = new BurstCoeffs();
                burstCoefficients.est_burst_flow = new double[efavorTest.total_no_of_pressure_steps];

                int counter = 0;
                foreach (UIElement gridElement in burstFlowEstimationGrid.Children)
                {
                    if (gridElement.GetType() == typeof(TextBox))
                    {                        
                        bool parse_ok;
                        double burst_flow;
                        parse_ok = double.TryParse(((TextBox)gridElement).Text, out burst_flow);
                        if (!parse_ok)
                            throw new Exception("Value of burst flow: " + ((TextBox)gridElement).Text + " is not recognized as number");
                        burstCoefficients.est_burst_flow[counter++] = burst_flow;
                        if (counter - 1 > burstCoefficients.est_burst_flow.Count())
                            throw new Exception("Length of burstCoefficients.est_burst_flow does not match number of textboxes in burstFlowEstimationGrid");
                    }
                }
                List<Node> listSuspectedNodes = new List<Node>();
                foreach (Rectangle rectangle in rectangleHitList)
                {
                    Node node = (Node)rectangle.Tag;
                    //make sure that this node belongs to main waterNetwork
                    if (waterNetwork.listOfNodes.Any(tmp => tmp == node))
                        listSuspectedNodes.Add(node);
                }
                if (listSuspectedNodes.Count == 0)
                    throw new Exception("Empty list of nodes to test for burst");


                BurstLocator burstLocator = new BurstLocator();
                int ret_val = burstLocator.TEMP_ManyBurstsSim(loggerConnections, efavorTest, epanet, listSuspectedNodes, burstCoefficients);
                if (ret_val < 0)
                    throw new Exception("Error while simulating burst at the selected nodes");
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void selectPathsNodesContextMenu_Click(object sender, RoutedEventArgs e)
        {
            string startLoggerName;///start node of logger connection
            string endLoggerName;///end node of logger connection
            try
            {
                List<Node> listSelectedNodes = new List<Node>();
                foreach (System.Windows.Shapes.Path currentPathHit in pathHitList)
                {
                    startLoggerName = loggerConnections.list_of_loggers[((Link)(currentPathHit.Tag)).nodeFrom].logger_id;
                    endLoggerName = loggerConnections.list_of_loggers[((Link)(currentPathHit.Tag)).nodeTo].logger_id;
                    listSelectedNodes.AddRange(loggerConnections.GetNodesFromPathsAndSideBranchesBetweenLoggers(startLoggerName, endLoggerName));
                }
                //highlight nodes and add to selected list
                foreach (Node currentNode in listSelectedNodes.Distinct())
                {
                    if (rectangleHitList.Exists(tmp => (Node)(tmp.Tag) == currentNode)) //already highlighted, skip it, else highlight it
                        continue;
                    ChangeNodeApperance(currentNode, ToBrush(UI.Default.SelectionNodeColor), UI.Default.SelectionNodeSize);
                    rectangleHitList.Add(currentNode.graphicalObject);                        
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception while selecting all paths nodes: " + ex.Message);
            }
        }

        private void Button_suspectedLoggers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Tuple<string, double, double>> loggersRanked = loggerConnections.GetLoggerListWithD2HFromInlet(efavorTest, inletSetSelectionComboBox.SelectedValue.ToString());
                WindowLoggerTotalD2H windowLoggerRanking = new WindowLoggerTotalD2H();
                windowLoggerRanking.Show();
                windowLoggerRanking.Owner = mainWindow;
                windowLoggerRanking.dataGrid1.ItemsSource = loggersRanked;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while calculating ranking of loggers: " + ex.Message);
            }

        }


 

	}
}
