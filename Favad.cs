using OxyPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace Styx
{
    
    public class Favad
    {
       
        public double LoggerSumOfElevation;
        public double LoggerAverageElevation;
        public double FNodeSumOfElevation;
        public double FNodeAverageElevation;
       

        public List<Node> GenListWaterNetworkNode(WaterNetwork Fwaternetworknodelist)
        {
            List<Node> F_Node_list = new List<Node> { };
            F_Node_list = Fwaternetworknodelist.listOfNodes;
            foreach (var item in F_Node_list)
            {
                FNodeSumOfElevation = FNodeSumOfElevation + item.elevation;
            }
            FNodeAverageElevation = FNodeSumOfElevation / F_Node_list.Count();
            return (F_Node_list);
        }

        public List<Logger> GenListofLogers(EFavorTest DataEfavor)
        {
            List<Logger> F_list_of_logger = new List<Logger> { };
            F_list_of_logger = DataEfavor.list_of_loggers;
            foreach (var item in F_list_of_logger)
            {
                LoggerSumOfElevation = LoggerSumOfElevation + item.elevation;
            }
            LoggerAverageElevation = LoggerSumOfElevation / F_list_of_logger.Count();
            return (F_list_of_logger);
        }

        public Logger FFind_repr_logger(EFavorTest efavor,WaterNetwork waternetwork)
        {
            GenListWaterNetworkNode(waternetwork);
            Logger closest = GenListofLogers(efavor).OrderBy(item => Math.Abs(FNodeAverageElevation - item.elevation)).First();
            return closest;
        }

        
        
        public List<FlowMeter> FInlet(EFavorTest Fefavor)
        {
            List<FlowMeter> List_of_inlets_flow = Fefavor.list_of_inlet_flowmeters;

            return List_of_inlets_flow;
        }
        public class mesurement_pair
        {
            //public int index { get; set; }
            public int inlet_set { get; set; }
            public double minimum_nightflow { get; set; }
            public double pressure_nightflow { get; set; }
            
        }

        public double FNightflow(List<FlowMeter> FInlet)
        {
            List<mesurement_pair> favad_grouping_messurements = new List<mesurement_pair>();
            //to do prepari ng for the sets of inlet
            double min = 0;
            foreach (var item in FInlet)
            {
                min =item.measured_flow.Min();
            }

            
            //double nightflow =FInlet.Min(flow=>flow.measured_flow.);
            double nightflow = 0;
            return (nightflow);
        }
        public double Get_minDemand(WaterNetwork waternetwork)
        {
            List<double> demand_list = new List<double>();
            double suma = 0;
            var demands_counter = waternetwork.listOfNodes[0].demand.Count();
            var nodes_counter = waternetwork.listOfNodes.Count();
            for (int m = 0; m < demands_counter; m++)
            {
                for (int n = 0; n < nodes_counter; n++)
                {
                    suma = (suma +waternetwork.listOfNodes[n].demand[m]);
                }
                demand_list.Add(suma);
                suma = 0;
            }
            //MessageBox.Show("Minimal demand"+ demand_list.Min().ToString());
            return demand_list.Min();
        }
        
    }

    public class Favad1
    {
        public class favadresults
        {
            public double N1 { get; set; }
            public double AZNP1 { get; set; }
            public double Flow1 { get; set; }
            public double AZNP0 { get; set; }
            public double Flow0 { get; set; }
        }
        //public BurstCoeffs burst { get; set; }
        public List<comparative_results> lista = new List<comparative_results> { };
        //comparative_results results = new comparative_results();
        public List<favadresults> favadres = new List<favadresults>();
        public favadcoefficients favadcoeff = new favadcoefficients();
        //public favadresults favad_res = new favadresults();
        public double N1;
        public int GenFAVADOutput(WaterNetwork waternetwork_tk, EFavorTest efavor, BurstCoeffs burstCoeffs)
        {
            try
            {
                Favad favad2 = new Favad();
                double demand = favad2.Get_minDemand(waternetwork_tk);
                Logger representative = favad2.FFind_repr_logger(efavor, waternetwork_tk);
                List<FlowMeter> inlets = favad2.FInlet(efavor);
                List<pair_values_favad> records = new List<pair_values_favad>();
                for (int n = 0; n < representative.measured_pressure.Count(); n++)
                {
                    pair_values_favad pair = new pair_values_favad();
                    pair.flow = inlets[0].measured_flow[n];
                    pair.pressure = representative.measured_pressure[n];
                    records.Add(pair);
                }
                //calibration process
                var last = records.Last();
                var previous = records.AsEnumerable().Reverse().Skip(1).FirstOrDefault();
                double anzp0 = previous.pressure;
                double anzp1 = last.pressure;
                double L0 = previous.flow; ;
                double L1 = last.flow;

                N1 = (Math.Log(L1 / L0)) / (Math.Log(anzp1 / anzp0));
                var B = ((L0 - demand) / Math.Pow(anzp0, 0.5) - (L1 - demand) / Math.Pow(anzp1, 0.5)) / (anzp0 - anzp1);
                var A = ((L0 - demand) / Math.Pow(anzp0, 0.5)) - B * anzp0;
                favadcoeff.A = A;
                favadcoeff.B = B;
                var text = String.Format("N1 Coefficient: {0}", N1.ToString());
                //MessageBox.Show(text);
                LeakEstimation(N1, records, demand, waternetwork_tk, burstCoeffs, favadcoeff);
                return 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return -1;
                throw;
            }

        }
        public double Lx;
        public double Px;
        public double Px1;
        public double Lx1;
        public int LeakEstimation(double N1, List<pair_values_favad> para, double demand, WaterNetwork network, BurstCoeffs burstCoeffs, favadcoefficients favadcoeff)
        {
            try
            {
                //favad_res.N1 = N1;
                para.Reverse();
                for (int jx = 0; jx < para.Count() - 1; jx++)
                {
                    favadresults favad_res = new favadresults();
                    favad_res.N1 = N1;
                    favad_res.Flow0 = para[jx].flow - demand;
                    favad_res.AZNP0 = para[jx].pressure;
                    favad_res.AZNP1 = para[jx + 1].pressure;
                    favad_res.Flow1 = favad_res.Flow0 * Math.Pow((favad_res.AZNP1 / favad_res.AZNP0), favad_res.N1);
                    favadres.Add(favad_res);
                }
                var nodes = network.listOfNodes;
                var emmiter = nodes.Where(s => s.emmitterCoefficient != 0).First();
                var result_comparation=ComparativeRres(favadcoeff.A, favadcoeff.B, N1, emmiter.emmitterCoefficient, network.emitterExponent, burstCoeffs.est_burst_coeff, burstCoeffs.est_burst_exponent);

                czart czart1 = new czart(favadres, network, burstCoeffs, favadcoeff,result_comparation);
                czart1.Show();



                return 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return -1;
            }
        }
        public class pair_values_favad {
            public double pressure { get; set; }
            public double flow { get; set; }

        }
        public class difference
        {
            public double skoworcow { get; set;}
            public double favad { get; set; }
        }
        public class favadcoefficients
        {
            public double A { get; set; }
            public double B { get; set; }
        }
        public class comparative_results
        {
            public double pressure { get; set; }
            public double skoworcow_met { get; set; }
            public double FAVAD_met { get; set; }
            public double FAVAD_N1_met { get; set; }
            public double reallosses { get; set; }
        }
        
        public difference ComparativeRres(double A, double B,double N1, double coeff_real,double exponent_real, double skoworcow_coeff, double skoworcow_exponent)
        {

            for (int i = 1; i < 100; i++)
            {
                comparative_results results = new comparative_results();
                results.pressure = i;
                results.FAVAD_met = A * Math.Pow(i, 0.5) + B * Math.Pow(i, 1.5);
                results.skoworcow_met = skoworcow_coeff * Math.Pow(i, skoworcow_exponent);
                results.reallosses = coeff_real * Math.Pow(i, exponent_real);
                lista.Add(results);
            }
            double piotr_difference = 0;
            double favad_difference = 0;
            foreach (var item in lista)
            {
                piotr_difference = piotr_difference + (item.reallosses - item.skoworcow_met);
                favad_difference = favad_difference + (item.reallosses - item.FAVAD_met);
            }
            difference diff = new difference();
            diff.skoworcow = piotr_difference / lista.Count();
            diff.favad = favad_difference / lista.Count();

            using (var file = new StreamWriter(@"c:\tom\results.txt"))
            {
                lista.ForEach(v => file.WriteLine(v.pressure+";"+v.reallosses+";"+v.skoworcow_met+";"+v.FAVAD_met+";"));
            }
            return  diff;
        }
        
    }
}
