//rectangleHitList
/*foreach (Node node in waterNetwork.listOfNodes.GetRange(10, 30))
{
	rectangleHitList.Add(node.graphicalObject);
	ChangeNodeApperance(node, constants.selectionNodeColor, constants.selectionNodeSize);
}
DrawWaterNetwork();*/

//!!!TODO!!! Copy original waterNetwork before any changes are made to it???


//List<List<Node>> dupa = loggerConnections.GetAllPathsBetweenLoggers("3", "6");


//burstCoeffEst.InitialiseCoeffs(30, 3, 0.6);
burstCoeffEst.InitialiseCoeffs(efavorTest, 20, 0);
//burstCoeffEst.InitialiseCoeffs(efavorTest, 60);
//burstCoeffEst.EstimateBurstFromLoggers_2Term(epanet, waterNetwork, efavorTest, 0.0006, 1000, 20, 3);

//double[] pressure = new double[3] { 41.44, 33.92, 27.33 }; //pressure at N75
//double[] pressure = new double[3] { 42.94, 35.08, 28.21 }; //pressure at nearest logger to N75
//double[] flow = new double[3] { 58.22, 54.67, 51.36 }; //prv flow
//burstCoeffEst.Estimate2TermModelCoeffs(pressure, flow, 0.0006, 500);



private void button3_Click(object sender, RoutedEventArgs e)
{
	//for testing
	

	/*List<Tuple<string, string>> dupa = new List<Tuple<string, string>>();
	Tuple<string, string> tuple = new Tuple<string, string>("chuj", "dupa");
	dupa.Add(tuple);
	dupa.Add(new Tuple<string, string>("chuj", "dupa777"));
	dupa.Add(new Tuple<string, string>("rzygi", "dupa666"));
	dupa.Add(new Tuple<string, string>("rzygi", "chuj"));
	dupa.Add(new Tuple<string, string>("chuj", "dupa777"));
	 
	WindowTemp chuj666 = new WindowTemp();
	chuj666.Owner = mainWindow;
	chuj666.Show();
	chuj666.listView1.ItemsSource = dupa;             
	   

	//myListView.Items.Add(new ListViewItem { Content = "This is an item added programmatically." });

	//SolidColorBrush myBrush = new SolidColorBrush(Colors.Red);
	/*SolidColorBrush myBrush = new SolidColorBrush();
	myBrush.Color = Color.FromRgb(250, 20, 20);
	
	ChangeLinkApperance(waterNetwork.listOfLinks[2], myBrush, 8);*/
	//ChangeLinkApperance(loggerConnections.logger_water_network.listOfLinks[1], Brushes.BlueViolet, 9);
	
}


private void button_LocalizeBurst_Click(object sender, RoutedEventArgs e)
        {
            //tutaj pisiorowac  
         
            List<Valve> inlet_valves = efavorTest.list_of_inlet_flowmeters.ConvertAll<Valve>(new Converter<FlowMeter, Valve>(FlowMeter.GetValve));
            //List<Valve> inlet_valves = waterNetwork.listOfValves;

            int ret_val = efavorTest.FlowmeterPrvSetpointToPrvSetting();
            if (ret_val < 0)
                throw new Exception("Error in FlowmeterPrvSetpointToPrvSetting, returned: " + ret_val.ToString());

            int sim_stop_time = efavorTest.first_measure_time + efavorTest.time_step_pressure_stepping * efavorTest.total_no_of_pressure_steps;
            epanet.SetSimulationStopTime(sim_stop_time);
            epanet.VerifyAndSetSimulationHydraulicTimeStep(efavorTest.time_step_pressure_stepping);
            epanet.SimulateUpdateHeadsFlows(waterNetwork, inlet_valves, efavorTest.first_measure_time, sim_stop_time, efavorTest.time_step_pressure_stepping);
            epanet.CloseENHydAnalysis();
            
            
            BurstCoeffEstimator burstCoeffEst = new BurstCoeffEstimator();
            burstCoeffEst.InitialiseCoeffs(efavorTest, 20, 0);
            burstCoeffEst.EstimateBurstFromLoggers_2Term(epanet, waterNetwork, efavorTest, 0.0006, 1000, 20, 3);

            List<Node> tmp_nodes = waterNetwork.listOfNodes;//.GetRange(40, 50);
            double[][] chi2matrix;
            BurstLocator burstLocator = new BurstLocator();
            //burstLocator.SimSeries_SingleBurst_Maxd2h(loggerConnections, efavorTest, epanet, tmp_nodes, burstCoeffEst.coeffs, true, "A", zero_d2h_tolerance, out chi2matrix);
            burstLocator.SimSeries_SingleBurst_Maxd2h(loggerConnections, efavorTest, epanet, tmp_nodes, burstCoeffEst.coeffs, true, inletSetSelectionComboBox.SelectedValue.ToString(), zero_d2h_tolerance, out chi2matrix);

            //
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
                listview_items.Add(new Tuple<string, double>(tmp_nodes[i].name, chi2_aggregate[i]));
            }

            /*List<double> chi2sorted = new List<double>(chi2_aggregate);
            chi2sorted.Sort();
            //string dupa = "CHUJ:\n";
            //List<Tuple<string, double>> listview_items = new List<Tuple<string, double>>();

            for (int i = 0; i < chi2matrix.Length; i++)
            {
                int index = chi2_aggregate.FindIndex(tmp => tmp == chi2sorted[i]);
                dupa += tmp_nodes[index].name + ": " + chi2sorted[i].ToString("f3") + "\n";
                listview_items.Add(new Tuple<string, double>(tmp_nodes[index].name, chi2sorted[i]));//.ToString("f3")));

            }*/
            //MessageBox.Show(dupa);

            unselectAllContextMenu_Click(sender, e);

            DrawWaterNetwork();
            WindowTest chi2_window = new WindowTest();
            chi2_window.Show();
            chi2_window.Owner = mainWindow;
            chi2_window.dataGrid1.ItemsSource = listview_items;
            
            //burstLocator.SimulateBurstAtNodes(waterNetwork, efavorTest, epanet, tmp_nodes, listBurstCoeff, true);
            
            /*List<Node> tmp_nodes = new List<Node>();
            tmp_nodes.Add(waterNetwork.listOfNodes.Find(tmp => tmp.name == "N3"));
            tmp_nodes.Add(waterNetwork.listOfNodes.Find(tmp => tmp.name == "N57"));
            List<double[]> tmp_dem_list = new List<double[]>();
            tmp_dem_list.Add(new double[3] { 30, 40, 50 });
            tmp_dem_list.Add(new double[3] { -10, -30, 10 });
            epanet.SimulateAdditionalDemandUpdateHeadsFlows(waterNetwork, inlet_valves, efavorTest.first_measure_time, sim_stop_time, efavorTest.time_step_pressure_stepping, tmp_nodes, tmp_dem_list);
            epanet.SimulateUpdateHeadsFlows(waterNetwork, inlet_valves, efavorTest.first_measure_time, sim_stop_time, efavorTest.time_step_pressure_stepping);
            */
            
            /*int chuj;
            epanet.AddFlatDemand(waterNetwork.listOfNodes.Find(tmp => tmp.name == "N78"), 20, out chuj);
            epanet.SimulateUpdateHeadsFlows(waterNetwork, inlet_valves, efavorTest.first_measure_time, sim_stop_time, efavorTest.time_step_pressure_stepping);
            epanet.listWorkPattern[chuj].ReplaceWorkWithOriginal();      
            //epanet.SimulateUpdateHeadsFlows(waterNetwork, inlet_valves, efavorTest.first_measure_time, efavorTest.first_measure_time+1*60, efavorTest.time_step_pressure_stepping);
            epanet.SimulateUpdateHeadsFlows(waterNetwork, inlet_valves, efavorTest.first_measure_time, sim_stop_time, efavorTest.time_step_pressure_stepping);
            epanet.AddFlatDemand(waterNetwork.listOfNodes.Find(tmp => tmp.name == "N164"), 20, out chuj);
            epanet.SimulateUpdateHeadsFlows(waterNetwork, inlet_valves, efavorTest.first_measure_time, sim_stop_time, efavorTest.time_step_pressure_stepping);
            epanet.listWorkPattern[chuj].ReplaceWorkWithOriginal();
            epanet.SimulateUpdateHeadsFlows(waterNetwork, inlet_valves, efavorTest.first_measure_time, sim_stop_time, efavorTest.time_step_pressure_stepping);
            */
            
            /*
             epanet.SetSimulationStopTime(5 * 60);
            epanet.SimulateUpdateHeadsFlows(waterNetwork, inlet_valves, 1 * 60, 5 * 60, 60);
            epanet.SetEmitterParameters(waterNetwork, waterNetwork.listOfNodes.Find(tmp => tmp.name == "N3"), 5, 0.6);
            epanet.SimulateUpdateHeadsFlows(waterNetwork, inlet_valves, 1 * 60, 5 * 60, 60);
            inlet_valves[0].setting = new double[8] { 45, 45, 45, 45, 45, 45, 45, 45 };            
            for (int i = 0; i < 10000; i++)
            {
                for (int j = 0; j < 8; j++)
                    inlet_valves[0].setting[j] += i / 50;
                epanet.SimulateUpdateHeadsFlows(waterNetwork, inlet_valves, 1 * 60, 5 * 60, 60);
            }
            MessageBox.Show("Juz!");
             */

            epanet.CloseENHydAnalysis();
            return;
        }