using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using GemBox.Spreadsheet; //Maximum number of rows per sheet is 150. Maximum number of sheets per workbook is 5.



namespace Styx
{
    public class EFavorTest
    {
        /*
        /// <summary>Stores info which measurement number (i.e. which element of measured_flow in FlowMeter and which element of measured_pressure in Logger) correspond to which inlet set
        /// </summary>
        public class MeasurementAndInletSet
        {
            public string inlet_set;
            public int index;
            MeasurementAndInletSet(string inlet_set, int index)
            {
                this.inlet_set = inlet_set;
                this.index = index;
            }
        }*/
        //time steps of measurements probably not required - it is easier to pre-process the data in excel and just use 1 measurement sample (1 pressure sample for each logger and 1 flow sample for each flowmeter) for each pressure step
        //public double time_step_measurements; ///time step of measurements in minutes - how often we sample pressure and flow
        public int first_measure_time; /// time (in minutes after midnight) corresponding to the first measurement in the excel spreadsheet
        public int time_step_pressure_stepping; /// time step of pressure stepping in minutes - how often PRV setpoint is changed
        public int total_no_of_pressure_steps; ///total number of PRV steps from all inlet sets; e.g. set of inlets A has 4 steps and set of inlets B has 3 steps, so total_no_of_pressure_steps=7;
        public int no_of_loggers; ///number of pressure loggers 
        public int no_of_inlets; ///number of inlets to the investigated DMA
        public List<Logger> list_of_inlet_loggers = new List<Logger>(); ///pressure loggers at inlets
        public List<Logger> list_of_loggers = new List<Logger>(); ///all pressure loggers, including loggers at inlets
        public List<FlowMeter> list_of_inlet_flowmeters = new List<FlowMeter>(); ///list of all inlet flowmeters (and hence valves), length = no_of_inlets
        public List<string> list_of_inlet_set_ids = new List<string>(); ///inlets belonging to the same set are throttled at the same time.                                                                                 
        public List<string> measurement_inlet_set_order = new List<string>(); ///Each item is inlet set id; order of items in this list determines which measurement number (i.e. which element of measured_flow in FlowMeter and which element of measured_pressure in Logger) correspond to which inlet set
        //list_of_throttled_inlets field is not needed if one EFavorTest allows throttling of inlets one-by-one 
        //public List<FlowMeter> list_of_throttled_inlets = new List<FlowMeter>(); ///list of inlet flowmeters (and hence valves) that are throttled at the same time in this experiment
        //prv_settings will be a property of each Flowmeter object
        //public List<double[]> prv_settings = new List<double[]>(); ///no of elements in list = no_of_inlets, length of each list element (array) = no_of_pressure_steps; with field like this we have flexibility in how prvs are manipulated for multi-inlet DMA (one-by-one, all-at-once, etc.); we can still use this field, even if we assume that in each EFavorTest object only 1 prv is throttled
        
        /// <summary>load experiment data from Excel file
        /// </summary>
        /// <param name="file_name">xls file to be loaded</param>
        /// <param name="water_network">water_network object to use for allocating loggers to nodes and flowmeters to valves</param>
        /// <returns>0 if successful</returns>
        /// <returns>-1 if wrong logger node name</returns>
        /// <returns>-2 if wrong flowmeter valve name</returns>
        /// <returns>-3 if error in PRV setpoints</returns>
        /// <returns>-4 if unknown logger ID used in pressure_measurements sheet</returns>
        /// <returns>-5 if unknown inlet set in pressure_measurements sheet</returns>
        /// <returns>-6 if error while reading pressure in pressure_measurements sheet</returns>
        /// <returns>-7 if unknown flowmeter ID used in flow_measurements sheet</returns>
        /// <returns>-8 if unknown inlet set in flow_measurements sheet</returns>
        /// <returns>-9 if order on inlet sets different in flow measurement sheet and pressure measurement sheet</returns>
        /// <returns>-10 if error while reading flow in flow_measurements sheet</returns>
        /// <returns>-11 if VerifyEFavorSetup() returned error in data</returns>
        /// <returns>-12 if error in parsing loggers sheet</returns>
        /// <returns>-13 if error in reading times sheet</returns>
        /// <returns>-20 if other exception thrown</returns>
        public int LoadEFavorData(string file_name, WaterNetwork water_network) 
        {
             //!!!TODO: test for multi-inlet DMA
            bool parse_ok;
            try
            {
                ExcelFile efavor_data = new ExcelFile();
                efavor_data.LoadXls(file_name);
                ExcelWorksheet efavor_loggers_sheet = efavor_data.Worksheets["loggers"];
                foreach (ExcelRow logger_row in efavor_loggers_sheet.Rows)
                {
                    string logger_id = logger_row.Cells[0].Value.ToString();
                    string node_id = logger_row.Cells[1].Value.ToString();
                    if (logger_id.StartsWith("Logger ID")) //header row 
                        continue;
                    double logger_elevation;
                    parse_ok = double.TryParse(logger_row.Cells[2].Value.ToString(), out logger_elevation);
                    if (!parse_ok)
                    {
                        MessageBox.Show("Unexpected string: " + logger_row.Cells[2].Value.ToString() + ", when parsing logger elevation data, expected number");
                        return (-12);
                    }
                    bool is_inlet;
                    parse_ok = bool.TryParse(logger_row.Cells[3].Value.ToString(), out is_inlet);
                    if (!parse_ok)
                    {
                        MessageBox.Show("Unexpected string: " + logger_row.Cells[3].Value.ToString() + ", when parsing logger data, expected true/false");
                        return (-12);
                    }
                    Node logger_node = water_network.listOfNodes.Find(tmp => tmp.name == node_id);
                    if (logger_node == null)
                    {
                        MessageBox.Show("Can't find node " + node_id + " when processing logger ID " + logger_id);
                        return (-1);
                    }
                    Logger new_logger = new Logger(logger_id, logger_node, is_inlet);
                    if (logger_elevation <= -1000)
                        new_logger.elevation = logger_node.elevation;
                    else
                        new_logger.elevation = logger_elevation;
                    list_of_loggers.Add(new_logger);
                    if (is_inlet)
                        list_of_inlet_loggers.Add(new_logger);
                }
                no_of_loggers = list_of_loggers.Count;
                //MessageBox.Show(efavor_loggers_sheet.Rows.Count.ToString()); //used number of rows

                ExcelWorksheet efavor_flowmeters_sheet = efavor_data.Worksheets["inlets"];
                foreach (ExcelRow flowmeter_row in efavor_flowmeters_sheet.Rows)
                {
                    string flowmeter_id = flowmeter_row.Cells[0].Value.ToString();
                    string valve_id = flowmeter_row.Cells[1].Value.ToString();
                    string set_of_inlets_id = flowmeter_row.Cells[2].Value.ToString(); //Inlets belonging to the same set are throttled at the same time.
                    if (flowmeter_id.StartsWith("Flowmeter ID")) //header row - !!! check if all headings are as expected
                        continue;
                    Valve flowmeter_valve = water_network.listOfValves.Find(tmp => tmp.link.name == valve_id);
                    if (flowmeter_valve == null)
                    {
                        MessageBox.Show("Can't find valve " + valve_id + " when processing flowmeter ID " + flowmeter_id);
                        return (-2);
                    }
                    FlowMeter new_flowmeter = new FlowMeter(flowmeter_id, flowmeter_valve, set_of_inlets_id);
                    for (int i = 3; i < flowmeter_row.AllocatedCells.Count; i++) //first 3 columns are flowmeter id, valve id and set
                    {
                        double prv_setting;
                        parse_ok = double.TryParse(flowmeter_row.Cells[i].Value.ToString(), out prv_setting);
                        if (parse_ok)
                            new_flowmeter.prv_settings.Add(prv_setting);
                        else
                        {
                            MessageBox.Show("Error while reading PRV setpoints for flowmeter ID " + flowmeter_id);
                            return (-3); //perhaps this could be just ignored instead of return???
                        }
                    }
                    list_of_inlet_flowmeters.Add(new_flowmeter);
                    bool already_defined = list_of_inlet_set_ids.Exists(tmp => tmp == set_of_inlets_id); //check if this set_of_inlets has already been added to the list of sets
                    if (!already_defined)
                        list_of_inlet_set_ids.Add(set_of_inlets_id);
                }
                no_of_inlets = list_of_inlet_flowmeters.Count;
                if (no_of_inlets == 0)
                    throw new Exception("This experiment data does not seem to have any PRV inlets!");

                ExcelWorksheet efavor_pressure_measure_sheet = efavor_data.Worksheets["pressure_measurements"];
                List<int> logger_index_order = new List<int>(); //order of loggers in the pressure_measurements sheet
                for (int i = 1; i < efavor_pressure_measure_sheet.Rows[0].AllocatedCells.Count; i++) //read header row, get order of loggers; use AllocatedCells.Count instead of no_of_loggers to avoid error in case pressure_measurements sheet did not have enough columns, correctness of all measurement data is checked later on anyway
                {
                    string logger_id = efavor_pressure_measure_sheet.Cells[0, i].Value.ToString();
                    int tmp_index = list_of_loggers.FindIndex(tmp => tmp.logger_id == logger_id);
                    if (tmp_index < 0)
                    {
                        MessageBox.Show("Unknown logger ID " + logger_id + " in pressure_measurements sheet");
                        return (-4);
                    }
                    else
                        logger_index_order.Add(tmp_index);
                }
                if (logger_index_order.Count != list_of_loggers.Count)
                {
                    MessageBox.Show("Number of declared loggers does not match number of loggers in sheet \"pressure_measurements\"");
                    return (-6);
                }
                for (int i = 1; i < efavor_pressure_measure_sheet.Rows.Count; i++)
                {
                    ExcelRow current_row = efavor_pressure_measure_sheet.Rows[i];
                    string inlet_set = current_row.Cells[0].Value.ToString();
                    if (!list_of_inlet_set_ids.Exists(tmp => tmp == inlet_set))
                    {
                        MessageBox.Show("Unknown inlet set " + inlet_set + " when processing pressure_measurements sheet");
                        return (-5);
                    }
                    measurement_inlet_set_order.Add(inlet_set);
                    for (int j = 1; j < no_of_loggers + 1; j++)
                    {
                        double pressure;
                        parse_ok = double.TryParse(current_row.Cells[j].Value.ToString(), out pressure);
                        if (parse_ok)
                            list_of_loggers[logger_index_order[j - 1]].measured_pressure.Add(pressure);
                        else
                        {
                            MessageBox.Show("Error while reading pressure measurement for logger ID " + list_of_loggers[logger_index_order[j-1]].logger_id);
                            return (-6);
                        }
                    }
                }

                ExcelWorksheet efavor_flow_measure_sheet = efavor_data.Worksheets["flow_measurements"];
                List<int> flowmeter_index_order = new List<int>(); //order of flowmeters in the flow_measurements sheet
                if (efavor_flow_measure_sheet.Rows[0].AllocatedCells.Count - 1 != no_of_inlets)
                {
                    MessageBox.Show("Missing data in flow measurement sheet. Declared flowmeters: " + no_of_inlets.ToString() + ". Flow measurement datasets: " + (efavor_flow_measure_sheet.Rows[0].AllocatedCells.Count - 1).ToString());
                    return (-10);
                }
                for (int i = 1; i < efavor_flow_measure_sheet.Rows[0].AllocatedCells.Count; i++) //read header row, get order of flowmeters; use AllocatedCells.Count instead of no_of_inlets to avoid error in case flow_measurements sheet did not have enough columns, correctness of all measurement data is checked later on anyway
                {
                    string flowmeter_id = efavor_flow_measure_sheet.Cells[0, i].Value.ToString();
                    int tmp_index = list_of_inlet_flowmeters.FindIndex(tmp => tmp.flowmeter_id == flowmeter_id);
                    if (tmp_index < 0)
                    {
                        MessageBox.Show("Unknown flowmeter ID " + flowmeter_id + " in flow_measurements sheet");
                        return (-7);
                    }
                    else
                        flowmeter_index_order.Add(tmp_index);
                }
                total_no_of_pressure_steps = 0;
                for (int i = 1; i < efavor_flow_measure_sheet.Rows.Count; i++)
                {
                    total_no_of_pressure_steps++;
                    ExcelRow current_row = efavor_flow_measure_sheet.Rows[i];
                    string inlet_set = current_row.Cells[0].Value.ToString();
                    if (!list_of_inlet_set_ids.Exists(tmp => tmp == inlet_set))
                    {
                        MessageBox.Show("Unknown inlet set " + inlet_set + " when processing flow_measurements sheet");
                        return (-8);
                    }
                    if (measurement_inlet_set_order[i - 1] != inlet_set)
                    {
                        MessageBox.Show("Order on inlet sets different in flow measurement sheet and pressure measurement sheet");
                        return (-9);
                    }
                    for (int j = 1; j < no_of_inlets + 1; j++)
                    {
                        double flow;
                        parse_ok = double.TryParse(current_row.Cells[j].Value.ToString(), out flow);
                        if (parse_ok)
                            list_of_inlet_flowmeters[flowmeter_index_order[j - 1]].measured_flow.Add(flow);
                        else
                        {
                            MessageBox.Show("Error while reading flow measurement for flowmeter ID " + list_of_inlet_flowmeters[flowmeter_index_order[j - 1]].flowmeter_id);
                            return (-10);
                        }
                    }
                }

                //load experiment time variables
                ExcelWorksheet efavor_times_sheet = efavor_data.Worksheets["times"];
                int tmp_time;
                parse_ok = int.TryParse(efavor_times_sheet.Rows[0].Cells[1].Value.ToString(), out tmp_time);
                if (parse_ok)
                    first_measure_time = tmp_time;
                else
                {
                    MessageBox.Show("Error while reading start time. Value read: " + efavor_times_sheet.Rows[0].Cells[1].Value.ToString());
                    return (-13);
                }
                parse_ok = int.TryParse(efavor_times_sheet.Rows[1].Cells[1].Value.ToString(), out tmp_time);
                if (parse_ok)
                    time_step_pressure_stepping = tmp_time;
                else
                {
                    MessageBox.Show("Error while reading measurement time step. Value read: " + efavor_times_sheet.Rows[1].Cells[1].Value.ToString());
                    return (-13);
                }
                if ((first_measure_time <= 0) || (time_step_pressure_stepping <= 0))
                {
                    MessageBox.Show("Times cannot be negative!");
                    return (-13);
                }
                
            }//try
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return (-20);
            }
            //Verify that the experiment data makes sense            
            if (VerifyEFavorSetup() < 0)
                return (-11);
            return (0);
        }

        /// <summary>Check if the data describing EFavor experiment is consistent
        /// </summary>
        /// <returns>0 if everything ok with data</returns>
        public int VerifyEFavorSetup()
        {
            //TODO!!!
            //check if inlets belonging to the same set have the same number of different PRV setpoints
            //DONE: check if number of pressure steps in measurements data (pressure and flow) matches number of pressure steps defined for inlets 
            //DONE: check if there are pressure measurements for all loggers
            //DONE: check if loggers are separated from each other by at least 2 non-logger nodes - PS 23/07/2013: not needed, loggers can also be next to each other
            if (!InletValvesArePRVs())
                return (-1);
            if (measurement_inlet_set_order.Count != total_no_of_pressure_steps)
            {
                MessageBox.Show("Number of steps in pressure measurements sheet does not match number of steps in flow measurement sheet");
                return (-1);
            }
            foreach (FlowMeter flowmeter in list_of_inlet_flowmeters)
            {
                if (flowmeter.prv_settings.Count != total_no_of_pressure_steps)
                {
                    MessageBox.Show("Number of PRV setpoints declared in sheet \"inlet\" for Flowmeter ID: " + flowmeter.flowmeter_id + " does not match number of steps in flow measurement sheet");
                    return (-1);
                }
            }
            foreach (Logger logger in list_of_loggers)
            {
                if ((logger.measured_pressure == null) || (logger.measured_pressure.Count != total_no_of_pressure_steps))
                {
                    MessageBox.Show("Number of pressure measurements for logger ID: " + logger.logger_id + " does not match number of PRV setpoints");
                    return (-1);
                }
                //PS 24/07/2013: Allow loggers next to each other
                //if (Node.IsNeighbour(logger.node, list_of_loggers))
                //{
                //    MessageBox.Show("Loggers have to separated by at least one non-logger node; logger ID: " + logger.logger_id + " is a direct neighbour of another logger");
                //    return (-1);
                //}
            }

            return (0);
        }

        /// <summary>Check if all items in list_of_inlet_flowmeters are PRVs
        /// </summary>
        /// <returns>true if all inlets are PRVs</returns>
        public bool InletValvesArePRVs()
        {
            foreach (FlowMeter flow_meter in list_of_inlet_flowmeters)
            {
                if (flow_meter.valve.link.type != Constants.EN_PRV)
                {
                    MessageBox.Show("Inlet " + flow_meter.flowmeter_id + " is not a PRV!");
                    return (false);
                }
            }
            return true;
        }

        /// <summary>For each inlet flowmeter object in list_of_inlet_flowmeters copy its prv_settings, to their corresponding valve.setting[i] field
        /// </summary>
        /// <returns>0 if successful</returns>
        public int FlowmeterPrvSetpointToPrvSetting()
        {
            try
            {
                foreach (FlowMeter flowmeter in list_of_inlet_flowmeters)
                {
                    flowmeter.valve.setting = new double[total_no_of_pressure_steps];
                    for (int i = 0; i < total_no_of_pressure_steps; i++)
                    {
                        flowmeter.valve.setting[i] = flowmeter.prv_settings[i];
                        //MessageBox.Show("tk breakpoint in efavor test"+flowmeter.prv_settings[i].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in FlowmeterPrvSetpointToPrvSetting: " + ex.Message);
                return (-1);
            }
            return (0);
        }

        /*
        /// <summary> returns pressure at given logger for given pressure step, pressure_step_no=0 means first section of data, i.e. normal PRV setting
        /// </summary>
        /// <param name="logger_id"></param>
        /// <param name="pressure_step_no">zero-based index</param>
        /// <returns>logger pressure if successful, -1000000 if error</returns>
        public double GetLoggerPressure(string logger_id, int pressure_step_no) 
        {
            Logger tmp_logger = list_of_loggers.Find(tmp => tmp.logger_id == logger_id);
            if ((tmp_logger != null) && (pressure_step_no <= tmp_logger.measured_pressure.Count - 1))
                return (tmp_logger.measured_pressure[pressure_step_no]);
            else
                MessageBox.Show("Wrong logger id or pressure step number too large");
            return (-1000000);
        }

        /// <summary> sets pressure at given logger for given pressure step
        /// </summary>
        /// <param name="logger_id"></param>
        /// <param name="pressure_step_no">zero-based index</param>
        /// <param name="pressure"></param>
        /// <returns>0 if successful, -1 if error</returns>
        public int SetLoggerPressure(string logger_id, int pressure_step_no, double pressure) 
        {
            Logger tmp_logger = list_of_loggers.Find(tmp => tmp.logger_id == logger_id);
            if ((tmp_logger != null) && (pressure_step_no <= tmp_logger.measured_pressure.Count - 1))
                tmp_logger.measured_pressure[pressure_step_no] = pressure;
            else
            {
                MessageBox.Show("Wrong logger id or pressure step number too large");
                return (-1);
            }
            return (0);
        }

        /// <summary> returns flow at given flowmeter for given pressure step, pressure_step_no=0 means first section of data, i.e. normal PRV setting
        /// </summary>
        /// <param name="flowmeter_id"></param>
        /// <param name="pressure_step_no"></param>
        /// <returns></returns>
        public double GetInletFlow(string flowmeter_id, int pressure_step_no)
        {
            //...
            return (0);
        }
        /// <summary>sets flow at given flowmeter for given pressure step        
        /// </summary>
        /// <param name="flowmeter_id"></param>
        /// <param name="pressure_step_no"></param>
        /// <param name="flow"></param>
        /// <returns>0 if successful</returns>
        public int SetInletFlow(string flowmeter_id, int pressure_step_no, double flow)
        {
            //...
            return (0);
        }
        */
    }
    /// <summary>Information about inlet flowmeter installed on valve (typically PRV)
    /// </summary>
    public class FlowMeter
    {
        public Valve valve; ///valve of EPANET model on which this flowmeter is installed, typically it is PRV
        public string flowmeter_id;
        public string set_of_inlets; ///Inlets belonging to the same set  are throttled at the same time
        public List<double> measured_flow = new List<double>(); ///separate field for measured data, since FlowMeter.Valve.flow is used to store EPANET data; length = EFavorTest.no_of_pressure_steps
        public List<double> prv_settings = new List<double>();

        public FlowMeter(string id, Valve valve, string set_of_inlets)
        {
            flowmeter_id = id;
            this.valve = valve;
            this.set_of_inlets = set_of_inlets;
        }

        /// <summary>Retrieve valve associated with a flowmeter; used for converter of ConvertAll
        /// </summary>
        /// <param name="flowmeter"></param>
        /// <returns></returns>
        public static Valve GetValve(FlowMeter flowmeter)
        {
            return (flowmeter.valve);
        }
    }
    /// <summary>information about pressure logger 
    /// </summary>
    public class Logger
    {
        public Node node;
        public string logger_id;
        public bool is_inlet; ///true if this logger measures inlet pressure to the DMA (i.e. prv outlet pressure)
        public double elevation; ///measurements can be taken at different elevation than node elevation (e.g. hydrant 1 meter above the node); if not provided assume it is equal to node elevation
        public List<double> measured_pressure = new List<double>(); ///separate field for measured data, since Logger.Node.head is used to store EPANET data; length = EFavorTest.no_of_pressure_steps
        
        public Logger(string id, Node node, bool is_inlet)
        {
            logger_id = id;
            this.node = node;
            this.is_inlet = is_inlet;
        }

        /// <summary>Retrieve node associated with a logger; used for converter of ConvertAll
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static Node GetLoggerNode(Logger logger)
        {
            return (logger.node);
        }
    }
}
