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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using BurstDetection.Properties;

namespace BurstDetection
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            int ret_val;
            double zero_flow_tolerance = 0.005; //value below which it is assumed that the flow is zero
            double head_diff_tolerance = 0.2; //if flow path contstructing algorithm arrives at logger's neighbour, it is assumed that it has arrinved at this logger ony if absolute head difference between the enighbour and the logger is smaller than this value
            WaterNetwork water_network = new WaterNetwork();
            //ret_val = Epanet.SimulateInEpanet();
            //ret_val = Epanet.ReadEpanetResults(water_network);
            IO io_object = new IO();
            ret_val = io_object.ReadEpanetDll_WindowApp(water_network, false);

            water_network.GenerateNeighboursLists(zero_flow_tolerance); //TEMP!!!
            EFavorTest efavor_test = new EFavorTest();
            OpenFileDialog file_dialog = new OpenFileDialog();
            file_dialog.InitialDirectory = Settings.Default.path;
            file_dialog.FileName = "EFavor_test_fake.xls";
            file_dialog.ShowDialog();
            efavor_test.LoadEFavorData(file_dialog.FileName, water_network);
            

            List<Valve> inlet_valves = efavor_test.list_of_inlet_flowmeters.ConvertAll<Valve>(new Converter<FlowMeter, Valve>(FlowMeter.GetValve));
            
            //TODO!!!update valve.setting based on FlowMeter.prv_settings, to do so ask efavor object for time_step_pressure_stepping and IO object for hydraulic step (using EN) (and other times if needed)
            inlet_valves[0].setting = new double[5] { 20, 30, 40, 50, 60 }; //!!!TEMP!!!
            //inlet_valves[0].setting = new double[6] { 30, 30, 30, 30, 30, 30 }; //!!!TEMP!!!
            io_object.SimulateUpdateHeadsFlows(water_network, inlet_valves, 1 * 60, 5 * 60, 60);
            //io_object.SimulateUpdateHeadsFlows(water_network, inlet_valves, 50, 340, 60);

            io_object.CloseENHydAnalysis();
            io_object.CloseENToolkit();
            //MessageBox.Show("Nodes: " + water_network.nNodes + " Pipes: " + water_network.nLinks);
                        
            List<Node> inlet_nodes = efavor_test.list_of_inlet_loggers.ConvertAll<Node>(new Converter<Logger, Node>(Logger.GetLoggerNode));
            FlowTree flow_tree = new FlowTree(water_network, inlet_nodes);
            flow_tree.GenerateFlowTree();

            LoggerConnections logger_connections = new LoggerConnections(efavor_test.list_of_loggers, flow_tree);
            logger_connections.CalculatePathsBetweenLoggers(head_diff_tolerance);
            logger_connections.LoggerConn2WaterNet(efavor_test, efavor_test.list_of_inlet_set_ids[0]);

            List<List<Node>> dupa = logger_connections.GetAllPathsBetweenLoggers("6", "5");
            return;
            
        }
    }
}
