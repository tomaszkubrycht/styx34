using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;
using Styx.Properties;


namespace Styx
{

        
        /// <summary>
        /// Library for water distribution network and hydraulic elements 
        /// </summary>
        class WDS
        {
            
        }

        /// <summary>
        /// Store ingormation about water distribution network
        /// </summary>
        public partial class WaterNetwork
        {
            /// <summary>
            /// Lists of elements in water network
            /// </summary>
            public List<Link> listOfLinks = new List<Link>();
            public List<Pump> listOfPumps = new List<Pump>();
            public List<Valve> listOfValves = new List<Valve>();
            public List<Node> listOfNodes = new List<Node>();
            public List<Tank> listOfTanks = new List<Tank>();
            public List<Reservoir> listOfReservoirs = new List<Reservoir>();
            public List<Pattern> listOfPatterns = new List<Pattern>();
            public List<Curve> listOfCurves = new List<Curve>();
            public List<Controls> listOfControls = new List<Controls>();

            /// <summary>
            /// Number of elements in water network
            /// </summary>
            public int nNodes;          /// Numbder of : Junctions + Reservoirs + Tanks
            public int nLinks;          /// Numbder of : Pipes + Pumps + Valves
            public int nPumps;          /// Numbder of : Pumps
            public int nValves;         /// Numbder of : Valves
            public int nPipes;          /// Numbder of : Pipes
            public int nJunctions;      /// Numbder of : Junctions
            public int nReservoirs;     /// Numbder of : Reservoirs 
            public int nTanks;          /// Numbder of : Tanks
            public int nPipesCV;        /// Numbder of : Pipes with check valve.In Epanet classified as pipes.
            public int nPatterns;       /// Numbder of : Patterns
            public int nCurves;         /// Numbder of : Curves

            public int nReportingPeriods;   /// Number of Reporting Periods
            public int reportingStartTime;  ///
            public int reportingTimeStep;   /// 
            public int simulationDuration;  ///

            public float emitterExponent;   /// Emitter exponent

            /// <summary>
            /// Units
            /// </summary>
            public string flowUnits;        /// Flow units
            public string pressureUnits;    /// Pressure units
            /* updated 24/5/2012 
             * conversion fields added */
            public double flowConversion;   /// Flow conversion to cubic meter per second
            public double lengthConversion; /// Length conversion to meters
            public double presssureConversion; /// Pressure conversion to meters
                                               /// 
            //water network object type
            public int waterNetworkType;        ///1 - real network, 2 - logger network, 3 - path network
            public Boolean isPlotted;       ///check whether network was aread plotted    


            /// <summary>
            /// Water network title read from inp file 
            /// </summary>
            public string[] title;

            /// <summary>
            /// Constructor
            /// </summary>
            public WaterNetwork()
            {
                isPlotted = false;
            }

            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="waterNetworkObject">Water network object</param>
            public WaterNetwork(WaterNetwork waterNetworkObject)
            {
                nNodes = waterNetworkObject.nNodes;
                nLinks = waterNetworkObject.nLinks;
                nPumps = waterNetworkObject.nPumps;
                nValves = waterNetworkObject.nValves;
                nPipes = waterNetworkObject.nPipes;
                nPipesCV = waterNetworkObject.nPipesCV;
                nJunctions = waterNetworkObject.nJunctions;
                nReservoirs = waterNetworkObject.nReservoirs;
                nTanks = waterNetworkObject.nTanks;
                nPatterns = waterNetworkObject.nPatterns;
                nCurves = waterNetworkObject.nCurves;

                nReportingPeriods = waterNetworkObject.nReportingPeriods;
                reportingStartTime = waterNetworkObject.reportingStartTime;
                reportingTimeStep = waterNetworkObject.reportingTimeStep;
                simulationDuration = waterNetworkObject.simulationDuration;

                emitterExponent = waterNetworkObject.emitterExponent;

                flowUnits = waterNetworkObject.flowUnits;
                pressureUnits = waterNetworkObject.pressureUnits;
                flowConversion = waterNetworkObject.flowConversion;
                lengthConversion = waterNetworkObject.lengthConversion;
                presssureConversion = waterNetworkObject.presssureConversion;
                waterNetworkType = waterNetworkObject.waterNetworkType;
                title = waterNetworkObject.title;
                isPlotted = waterNetworkObject.isPlotted;

                foreach (Link link in waterNetworkObject.listOfLinks)
                {
                    listOfLinks.Add(link);
                }

                foreach (Node node in waterNetworkObject.listOfNodes)
                {
                    listOfNodes.Add(node);
                }

                foreach (Pump pump in waterNetworkObject.listOfPumps)
                {
                    listOfPumps.Add(pump);
                }

                foreach (Valve valve in waterNetworkObject.listOfValves)
                {
                    listOfValves.Add(valve);
                }

                foreach (Reservoir reservoir in waterNetworkObject.listOfReservoirs)
                {
                    listOfReservoirs.Add(reservoir);
                }

                foreach (Tank tank in waterNetworkObject.listOfTanks)
                {
                    listOfTanks.Add(tank);
                }
                foreach (Pattern pattern in waterNetworkObject.listOfPatterns)
                {
                    listOfPatterns.Add(pattern);
                }
                foreach (Curve curve in waterNetworkObject.listOfCurves)
                {
                    listOfCurves.Add(curve);
                }

                foreach (Controls control in waterNetworkObject.listOfControls)
                {
                    listOfControls.Add(control);
                }
            }

        }
        /// <summary>
        /// Store information about link
        /// </summary>
        public partial class Link
        {
            public int nodeTo { get; set; }          ///destination node
            public int nodeFrom { get; set; }        ///source node
            public int type { get; set; }           ///0 pipe with CV, 1 pipe, 2 pump, 3-8 different types of valve
            public double length { get; set; }
            public double diameter { get; set; }
            public float setting { get; set; }       /// H-W C-value
            public double[] flow;
            public double[] headDrop;
            public string name { get; set; }
            public string status { get; set; }
            public double minorLoss { get; set; }
            public Pump pump;
            public Valve valve;
            public Shape graphicalObject; /// graphical link representation
            public double thickness { get; set; }   /// graphical link representation thickenss
            public Brush color { get; set; }        /// graphical link representation color
                        
            public string nodeToName; ///not linked or automatically updated! use this fields only for logger connection path visualisation
            public string nodeFromName; ///not linked or automatically updated! use this fields only for logger connection path visualisation

            Constants constant = new Constants();   

            /// <summary>
            /// Constructor
            /// </summary>
            public Link()
            {
                nodeFrom = 0; nodeTo = 0; type = -1;
                length = 0; diameter = 0; setting = 0; minorLoss = 0;
                status = null; pump = null; valve = null;

                thickness = UserInterface.Default.StandardLinkThickness;
                color = new SolidColorBrush(Color.FromArgb(UserInterface.Default.StandardLinkColor.A, UserInterface.Default.StandardLinkColor.R, UserInterface.Default.StandardLinkColor.G, UserInterface.Default.StandardLinkColor.B));
  

            }

            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="link"></param>
            public Link(Link link)
            {
                length = link.length;
                diameter = link.diameter;
                flow = link.flow;
                name = link.name;
                nodeFrom = link.nodeFrom;
                nodeTo = link.nodeTo;
                pump = link.pump;
                setting = link.setting;
                type = link.type;
                valve = link.valve;
                headDrop = link.headDrop;
                minorLoss = link.minorLoss;
                status = link.status;
                graphicalObject = link.graphicalObject;
                thickness = link.thickness;
                color = link.color;
            }

        }
        /// <summary>
        /// Store information about pump
        /// </summary>
        public class Pump
        {
            public double[] speedPattern;
            public string curveName;        ///HEAD curve name
            public Link link;

            /// <summary>
            /// Constructors
            /// </summary>
            public Pump()
            {
                link = null;

            }
            public Pump(Link parentLink)
            {
                link = parentLink;

            }
        }
        /// <summary>
        /// Store information about valve
        /// </summary>
        public class Valve
        {
            public double[] setting;
            public Link link;

            /// <summary>
            /// Constructors
            /// </summary>
            public Valve()
            {
                link = null;
            }
            public Valve(Link parentLink)
            {
                link = parentLink;
            }
        }


        /// <summary>
        /// Store information about node
        /// </summary>
        public partial class Node
        {
     
            public double elevation {get; set;}
            public double lowerConstraint{get; set;}  ///Min and max of head constraint
            public double upperConstraint{get; set;}
            public double[] head;            
            public double[] demand;        ///Total demand at given time step, i.e. result of all patterns multiplied by corresponding factors
            public float demandFactor{get; set;}      ///Demand pattern multiplier
            public string patternName{get; set;}      ///Pattern ID label 
            public string name{get; set;}             ///Node label
            public int type{get; set;}                ///Node type : 0 junction, 1 reservoir, 2 tank
            public Reservoir reservoir;
            public Tank tank;
            public bool toSimplification{get; set;}   ///True - remove, False - remain    
            public float xcoord{get; set;}            ///Coordinates
            public float ycoord{get; set;}            ///Coordinates
            public string tag{get; set;}             ///Tag, used to indicate nodes to retain 
            public float emmitterCoefficient {get; set;}///Emitter coefficient                                

            /* updated 24-5-2012
             * minimumServicePressure field added
             */
            public double minimumServicePressure {get; set;} ///Minimum service pressure
            /* updated 19-6-2012
            *  field to store graphical representation 
            */
            public Rectangle graphicalObject; ///graphical node represnetation
            public double size {get; set;}      ///size of graphical represnetation
            public Brush color { get; set; }  ///

            Constants constant = new Constants();   

            /// <summary>
            /// Constructors
            /// </summary>
            public Node()
            {
                type = -1;
                toSimplification = true;   ///intially all nodes are proposed to remove

                size = UserInterface.Default.StandardNodeSize;
                color = new SolidColorBrush(Color.FromArgb(UserInterface.Default.StandardNodeColor.A, UserInterface.Default.StandardNodeColor.R, UserInterface.Default.StandardNodeColor.G, UserInterface.Default.StandardNodeColor.B));
                isLogger = false;
                isPathNode = false;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="nodeType"> 0 junction, 1 reservoir, 2 tank</param>
            public Node(int nodeType)
            {
                if (nodeType == 1)
                {
                    reservoir = new Reservoir(this); tank = null;
                }
                if (nodeType == 2)
                {
                    reservoir = null; tank = new Tank(this);
                }
            }


            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="node"></param>
            public Node(Node node)
            {


                elevation = node.elevation;
                lowerConstraint = node.lowerConstraint;
                upperConstraint = node.upperConstraint;
                head = node.head;
                demand = node.demand;
                demandFactor = node.demandFactor;
                patternName = node.patternName;
                name = node.name;
                type = node.type;
                reservoir = node.reservoir;
                tank = node.tank;
                toSimplification = node.toSimplification;
                xcoord = node.xcoord;
                ycoord = node.ycoord;
                tag = node.tag;
                emmitterCoefficient = node.emmitterCoefficient;
                minimumServicePressure = node.minimumServicePressure;
                graphicalObject = node.graphicalObject;
                size = node.size;
                color = node.color;

            }
        }
        /// <summary>
        /// Store information about reservoir - FHS (fixed head reservoir)
        /// </summary>
        public class Reservoir
        {
            public Node node;
            public int patternIndex;
            public Reservoir(Node parentNode)
            {
                node = parentNode;
            }
        }

        /// <summary>
        /// Store information about tank - VHS (variable head reservoir)
        /// </summary>
        public class Tank
        {
            public Node node;
            public double crossSectionalArea;
            public float initLevel;
            public float minLevel;
            public float maxLevel;
            public float diameter;
            public float minVolume;
            public string volumeCurve;

            public Tank(Node parentNode)
            {
                node = parentNode;
            }
        }

        /// <summary>
        /// Store information about pattern
        /// </summary>
        public class Pattern
        {
            public string name;     ///pattern label
            public int length;      ///pattern length
            public double[] values;
        }

        /// <summary>
        /// Store information about curve
        /// </summary>
        public class Curve
        {
            public string name;
            public string type;     ///curve types in Epanet : PUMP , VOLUME, HEADLOSS, EFFICIENCY
            public double[] x;
            public double[] y;
            public int length;
        }

        /// <summary>
        /// Store information about simple controls
        /// </summary>
        public class Controls
        {
            public int ctype;       ///control type code
            public int lindex;      ///index of link being controlled 
            public double setting;  ///value of the control setting    
            public int nindex;      ///index of controlling node
            public double level;    ///value of controlling water level or pressure for level controls or of time of control action (in seconds) for time-based controls 
        }


    
}
