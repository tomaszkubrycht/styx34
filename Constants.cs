using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using UI = Styx.Properties.UserInterface;

namespace Styx
{
    
    /// <summary>
    /// Class consists Constants used in this project.
    /// Constants are similar with Constants in EPAnet to
    /// use in function from epanet.dll library. 
    /// </summary>
    class Constants
    {
        /// <summary>
        /// Component codes countcode consist of the following Constants.
        /// Used in ENgetcount(int countcode, int[] arg2)
        /// </summary>        
        public const int EN_NODECOUNT = 0;      // Nodes  
        public const int EN_TANKCOUNT = 1;      // Reservoirs and tank nodes 
        public const int EN_LINKCOUNT = 2;      // Links  
        public const int EN_PATCOUNT = 3;       // Time patterns 
        public const int EN_CURVECOUNT = 4;     // Curves 
        public const int EN_CONTROLCOUNT = 5;   // Simple controls


        /// <summary>
        /// Node parameter codes paramcode consist of the following Constants.
        /// Used in ENgetnodevalue(int index, int paramcode, float[] value)
        /// </summary>     
        public const int EN_ELEVATION = 0;      // Elevation 
        public const int EN_BASEDEMAND = 1;     // Base demand 
        public const int EN_PATTERN = 2;        // Demand pattern index 
        public const int EN_EMITTER = 3;        // Emitter coeff. 
        public const int EN_INITQUAL = 4;       // Initial quality 
        public const int EN_SOURCEQUAL = 5;     // Source quality 
        public const int EN_SOURCEPAT = 6;      // Source pattern index 
        public const int EN_SOURCETYPE = 7;     // Source type (See note below) 
        public const int EN_TANKLEVEL = 8;      // Initial water level in tank 
        public const int EN_DEMAND = 9;         // Actual demand
        public const int EN_HEAD = 10;          // Hydraulic head 
        public const int EN_PRESSURE = 11;      // Pressure 

        //The following parameter codes apply only to storage tank nodes:
        public const int EN_INITVOLUME = 14;    // Initial water volume  
        public const int EN_TANKDIAM = 17;      // Tank diameter 
        public const int EN_MINVOLUME = 18;     // Minimum water volume 
        public const int EN_VOLCURVE = 19;      // Index of volume versus depth curve (0 if none assigned) 
        public const int EN_MINLEVEL = 20;      // Minimum water level 
        public const int EN_MAXLEVEL = 21;      // Maximum water level 


        /// <summary>
        /// Link type codes consist of the following Constants.
        /// Used in ENgetlinktype(int index, int[] typecode) 
        /// </summary>         
        public const int EN_CVPIPE = 0;         // Pipe with Check Valve 
        public const int EN_PIPE = 1;           // Pipe 
        public const int EN_PUMP = 2;           // Pump 
        public const int EN_PRV = 3;            // Pressure Reducing Valve 
        public const int EN_PSV = 4;            // Pressure Sustaining Valve 
        public const int EN_PBV = 5;            // Pressure Breaker Valve 
        public const int EN_FCV = 6;            // Flow Control Valve 
        public const int EN_TCV = 7;            // Throttle Control Valve 
        public const int EN_GPV = 8;            // General Purpose Valve

        public const int EN_VV = 9;             // Virtual Valve (for EPANET pipes with a closed status)
        public const int EN_LOG_CONN = 666;     // Virtual connection between loggers
        public const int EN_LOG_PATH = 777;     // Path for connection between loggers

        /// <summary>
        /// Node type codes consist of the following Constants.
        /// Used in ENgetnodetype( int index, int[] typecode )
        /// </summary>
        public const int EN_JUNCTION = 0;       // Junction node 
        public const int EN_RESERVOIR = 1;      // Reservoir node 
        public const int EN_TANK = 2;           // Tank node 


        /// <summary>
        /// Option type codes consist of the following Constants.
        /// Used in ENgetoption( int index, float[] typecode )
        /// </summary>
        public const int EN_TRIALS = 0;       // Maximum number of trials used to solve
        public const int EN_ACCURACY = 1;      // Convergence criterion
        public const int EN_TOLERANCE = 2;    // 
        public const int EN_EMITEXPON = 3;    // Emitter exponet
        public const int EN_DEMANDMULT = 4;     // Demand multiplier

 
        /// <summary>
        /// Link parameter codes consist of the following Constants.
        /// Used in ENgetlinkvalue(int index, int paramcode, float[] value)
        /// </summary>
        public const int EN_DIAMETER = 0;       // Diameter 
        public const int EN_LENGTH = 1;         // Length 
        public const int EN_ROUGHNESS = 2;      // Roughness coeff. 
        public const int EN_MINORLOSS = 3;      // Minor loss coeff. 
        public const int EN_INITSTATUS = 4;     // Initial link status (0 = closed, 1 = open) 
        public const int EN_INITSETTING = 5;    // Roughness for pipes, initial speed for pumps, initial setting for valves 
        public const int EN_KBULK = 6;          // Bulk reaction coeff. 
        public const int EN_KWALL = 7;          // Wall reaction coeff. 
        public const int EN_FLOW = 8;           // Flow rate 
        public const int EN_VELOCITY = 9;       // Flow velocity  
        public const int EN_HEADLOSS = 10;      // Head loss 
        public const int EN_STATUS = 11;        // Actual link status (0 = closed, 1 = open) 
        public const int EN_SETTING = 12;       // Roughness for pipes, actual speed for pumps, actual setting for valves 
        public const int EN_ENERGY = 13;        // Energy expended in kwatts 


        /// <summary>
        /// Time parameter codes
        /// used in ENgettimeparam(int paramcode, long[] timevalue);
        /// </summary>

        public const int EN_DURATION = 0;       // Simulation duration 
        public const int EN_HYDSTEP = 1;        // Hydraulic time step 
        public const int EN_QUALSTEP = 2;       // Water quality time step  
        public const int EN_PATTERNSTEP = 3;    // Time pattern time step 
        public const int EN_PATTERNSTART = 4;   // Time pattern start time 
        public const int EN_REPORTSTEP = 5;     // Reporting time step  
        public const int EN_REPORTSTART = 6;    // Report starting time  
        public const int EN_RULESTEP = 7;       // Time step for evaluating rule-based controls  
        public const int EN_STATISTIC = 8;      // Type of time series post-processing to use  


        /// <summary>
        /// Flags for water network object types
        /// </summary>
        public const int REAL_NETWORK = 1;
        public const int LOGGER_NETWORK = 2;
        public const int PATH_NETWORK = 3;

       
        /// <summary>
        /// Attributes of graphical objects
        /// </summary>
        public double standardNodeSize = 8;
        public Brush standardNodeColor = Brushes.Black;       
        public double standardLinkThickness = 1;
        public Brush standardLinkColor = Brushes.Black;

        public Brush loggerNodeColor = Brushes.Orange; //default colour
        public Brush loggerInletNodeColor = Brushes.Aqua; 
        public double loggerNodeSize = 10;
        public double loggerSelectionNodeSize = 12;
        public double loggerLinkThickness = 2;
        public Brush loggerLinkColor = Brushes.Orange; //default colour
        public double loggerMinLinkThickness = 0.6; ///logger connection thickness depends on absolute d2h
        public double loggerMaxLinkThickness = 9; ///logger connection thickness depends on absolute d2h
        public double loggerMinRedComponent = 50; ///red component of rgb to indicate largest d2h percentage for positive d2h
        public double loggerMaxRedComponent = 255; ///red component of rgb to indicate smallest d2h percentage for positive d2h 
        public double loggerMinBlueComponent = 100; ///blue component of rgb to indicate largest d2h percentage for negative d2h
        public double loggerMaxBlueComponent = 255; ///blue component of rgb to indicate smallest d2h percentage for negative d2h 
        public byte loggerStandardRedComponent = 20; ///amount of red if d2h percentage is negative and blue is operated
        public byte loggerStandardGreenComponent = 20;
        public byte loggerStandardBlueComponent = 20; ///amount of blue if d2h percentage is positive and red is operated

        public Brush pathNodeColor = Brushes.DeepPink;
        public double pathNodeSize = 10;
        public double pathLinkThickness = 2;
        public Brush pathLinkColor = Brushes.DeepPink;

        public Brush selectionNodeColor = Brushes.Red;
        public double selectionNodeSize = 11;
        public double selectionLinkThickness = 2;
        public Brush selectionLinkcolor = Brushes.Red;
        

        public double nodeLabelOffset = 1.5;
        //colors of the selection box
        public Brush selectionSquareBrush = Brushes.Transparent;
        public Pen selectionSquarePen = new Pen(Brushes.Black, 2);

        //main window colors
        public Color mainWindowBacgroundColor = Color.FromArgb(255, 156, 170,193);
    }

    /// <summary>
    /// Error class  
    /// </summary>
    class Error
    {
        public int errorIndex;
        public string errorMsg;

        /// <summary>
        /// Constructor
        /// </summary>
        public Error()
        {

        }


    }
    /// <summary>
    /// Contains list of EPAnet errors
    /// </summary>
    class EpanetError
    {
        public List<Error> listOfEpanetErrs = new List<Error>();//list of epanet errors

        /// <summary>
        /// Constructor
        /// </summary>
        public EpanetError()
        {
            Error error = new Error();
            error.errorIndex = 101; error.errorMsg = "Insufficient memory"; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 102; error.errorMsg = "No network data to process "; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 103; error.errorMsg = "Hydraulics solver not initialized "; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 104; error.errorMsg = "No hydraulic results available "; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 105; error.errorMsg = "Water quality solver not initialized "; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 106; error.errorMsg = "No results to report on"; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 110; error.errorMsg = "Cannot solve hydraulic equations"; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 120; error.errorMsg = "Cannot solve WQ transport equations "; listOfEpanetErrs.Add(error);

            error = new Error(); error.errorIndex = 200; error.errorMsg = "One or more errors in input file "; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 202; error.errorMsg = "Illegal numeric value in function call "; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 203; error.errorMsg = "Undefined node in function call"; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 204; error.errorMsg = "Undefined link in function call "; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 205; error.errorMsg = "Undefined time pattern in function call "; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 207; error.errorMsg = "Attempt made to control a check valve "; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 223; error.errorMsg = "Not enough nodes in network "; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 224; error.errorMsg = "No tanks or reservoirs in network "; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 240; error.errorMsg = "Undefined source in function call "; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 241; error.errorMsg = "Undefined control statement in function call "; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 250; error.errorMsg = "Function argument has invalid format "; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 251; error.errorMsg = "Illegal parameter code in function call "; listOfEpanetErrs.Add(error);

            error = new Error(); error.errorIndex = 301; error.errorMsg = "Identical file names "; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 302; error.errorMsg = "Cannot open input file "; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 303; error.errorMsg = "Cannot open report file "; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 304; error.errorMsg = "Cannot open binary output file "; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 305; error.errorMsg = "Cannot open hydraulics file "; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 306; error.errorMsg = "Invalid hydraulics file "; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 307; error.errorMsg = "Cannot read hydraulics file "; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 308; error.errorMsg = "Cannot save results to file "; listOfEpanetErrs.Add(error);
            error = new Error(); error.errorIndex = 309; error.errorMsg = "Cannot write report to file "; listOfEpanetErrs.Add(error);


        }


    }

    /// <summary>Contains advanced options used for different numerical methods; can be edited by the user 
    /// </summary>
    static class AdvancedOptions
    {
        public static double zero_flow_tolerance = UI.Default.ZeroFlowTolerance; ///value below which it is assumed that the flow is zero
        public static double head_diff_tolerance = UI.Default.HeadDifferenceTolerance; ///if flow path constructing algorithm arrives at logger's neighbour, it is assumed that it has arrived at this logger ony if absolute head difference between the neighbour and the logger is smaller than this value
        public static double zero_d2h_tolerance = UI.Default.ZeroD2HTtolerance; ///if abs of headloss change (d2h) is smaller than this value then it is considered zero
        public static double lsqr_estimation_tolerance = UI.Default.LeastSquareEstimationTolerance; ///burst coefficients estimation parameter: during estimation lsqr algorithm stops if normalized average error percentage is smaller than this tolerance
        public static int lsqr_estimation_iterations = UI.Default.LeastSquareEstimationIterations; ///burst coefficients estimation parameter: maximum number of iterations for the lsqr algorithm 
        public static double max_diff_from_min_chi2_percent = UI.Default.MaximumDifferenceFromMinChi2Percentage; ///burst coefficients estimation parameter: maximum difference (as percentage of the smallest chi2) of chosen set from the smallest chi2                                                 
        public static int max_no_min_chi2 = UI.Default.MaximumNumberOfMinChi2; ///burst coefficients estimation parameter: maximum number of sets of coefficients selected to calculate average coefficients
        public static int max_higher_level_neighbours = UI.Default.MaximumForHigherLevelNeighbours; ///maximum allowed level for higher level neighbours which are used for logger connections calculations
        public static int loggerNeighbourhoodLevel = UI.Default.LoggerNeighbourhoodLevel; ///when loggers connection is calculated, it is assumed we've arrived at a logger when we arrive at logger's neighbour; this variable determines level of neighbourhood: 0=only when we arrive at a logger itself, 1=when we arrive at a direct neighbour, 2=when we arrive at 2nd level neighbour (i.e. separated by 1 node)
    }


    
   
}
