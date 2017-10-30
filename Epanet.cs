using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Styx.Properties;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices; //for DLLImport
using System.Windows;

namespace Styx
{
    public class Epanet
    {
        [DllImport("epanet2.dll")]
        static extern int ENsetnodevalue(int index, int paramcode, float value);
        [DllImport("epanet2.dll")]
        static extern int ENepanet(IntPtr inputfile, IntPtr reportfile, IntPtr binoutputfile, IntPtr nothing);
        [DllImport("epanet2.dll")]
        static extern int ENsetlinkvalue(int index, int paramcode, float value);
        [DllImport("epanet2.dll")]
        //static extern int ENgetnodeindex(char[] id, int[] index);
        static extern int ENgetnodeindex(IntPtr id, int[] index);
        [DllImport("epanet2.dll")]
        //static extern int ENgetlinkindex(char[] id, int[] index);
        static extern int ENgetlinkindex(IntPtr id, int[] index);
        [DllImport("epanet2.dll")]
        static extern int ENgettimeparam(int paramcode, long[] timevalue);
        //[DllImport("epanet2.dll", CallingConvention = CallingConvention.Cdecl)]
        [DllImport("epanet2.dll")]
        static extern int ENsettimeparam(int paramcode, Int32 timevalue);
        //static extern int ENsettimeparam(int paramcode, long timevalue);
        [DllImport("epanet2.dll")]
        static extern int ENsaveH();
        [DllImport("epanet2.dll")]
        static extern int ENnextH(ref long tstep);
        [DllImport("epanet2.dll")]
        static extern int ENrunH(ref long tstep);
        [DllImport("epanet2.dll")]
        static extern int ENinitH(int saveflag);
        [DllImport("epanet2.dll")]
        static extern int ENopenH();
        [DllImport("epanet2.dll")]
        static extern int ENcloseH();
        [DllImport("epanet2.dll")]
        static extern int ENopen(IntPtr input_file, IntPtr rep_file, IntPtr results_file);
        [DllImport("epanet2.dll")]
        static extern int ENgetcount(int countcode, int[] arg2);
        [DllImport("epanet2.dll")]
        static extern int ENgetpatternid(int index, byte[] id);
        [DllImport("epanet2.dll")]
        static extern int ENgetnodevalue(int index, int paramcode, float[] value);
        [DllImport("epanet2.dll")]
        static extern int ENgetnodeid(int index, byte[] id);
        [DllImport("epanet2.dll")]
        static extern int ENsaveinpfile(String fname);
        [DllImport("epanet2.dll")]
        //static extern int ENgetpatternindex(char[] id, int[] index); //this version with char[] will not work under .NET 4.0
        static extern int ENgetpatternindex(IntPtr id, int[] index);
        [DllImport("epanet2.dll")]
        static extern int ENaddpattern(IntPtr id); 
        [DllImport("epanet2.dll")]
        static extern int ENgetlinkid(int index, byte[] id);
        [DllImport("epanet2.dll")]
        static extern int ENgetlinktype(int index, int[] typecode);
        [DllImport("epanet2.dll")]
        static extern int ENgetnodetype(int index, int[] typecode);
        [DllImport("epanet2.dll")]
        static extern int ENgetlinkvalue(int index, int paramcode, float[] value);
        [DllImport("epanet2.dll")]
        static extern int ENgetpatternlen(int index, int[] len);
        [DllImport("epanet2.dll")]
        static extern int ENgetpatternvalue(int index, int period, float[] value);
        [DllImport("epanet2.dll")]
        static extern int ENsetpatternvalue(int index, int period, float value); 
        [DllImport("epanet2.dll")]
        static extern int ENgetoption(int index, float[] value);
        [DllImport("epanet2.dll")]
        static extern int ENsetoption(int optioncode, float value);
        [DllImport("epanet2.dll")]
        static extern int ENgetcontrol(int index, int[] ctype, int[] lindex, float[] setting, int[] nindex, float[] level);
        [DllImport("epanet2.dll")]
        static extern int ENclose();
        [DllImport("epanet2.dll")]
        static extern int ENsolveH();

        /// <summary>stores information about an additional pattern to be modified for burst simulation, since fucking Epanet dll 
        /// doesn't allow to allocate more than one demand pattern to a node (Demand Categories visible in Epanet app can't be accessed through dll)
        /// </summary>
        public class WorkPattern
        {
            public int index; ///1-based index of Epanet patterns corresponding to this work patter
            int replaced_pat_index; ///1-based index of pattern which was replaced by this pattern
            public Node node { get; private set; } ///node to which this pattern is assigned
            string pat_name; ///pattern name
            public bool original_base_dem_zero; ///true if original base demand was zero (i.e. no demand at all at this node)                             
            EpanetError EPAerr = new EpanetError();///access to list of EPAnet errors

            /// <summary>Constructor 
            /// </summary>
            /// <param name="name">name of a new working pattern</param>
            public WorkPattern(string name)
            {
                this.pat_name = name;
                replaced_pat_index = -1;
                original_base_dem_zero = false;

                int ret_val = 0; //error code
                IntPtr pat_name = (IntPtr)Marshal.StringToHGlobalAnsi(name);
                ret_val = ENaddpattern(pat_name);
                if (ret_val<0)
                    throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                int[] tmp_index = new int[1];
                ret_val = ENgetpatternindex(pat_name, tmp_index);
                if (ret_val < 0)
                    throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                index = tmp_index[0];                
                Marshal.FreeHGlobal(pat_name);
            }

            /// <summary>For given node, replaces original demand pattern with a work pattern
            /// </summary>
            /// <param name="node"></param>
            /// <returns>0 if successful</returns>
            /// <returns>-1 if exception</returns>
            /// <returns>-2 if original pattern has already been replaced</returns>
            public int ReplaceOriginalWithWork(Node node)
            {
                if (this.node != null)
                {
                    MessageBox.Show("Original pattern has already been replaced!");
                    return (-2); //this.node already allocated, so original pattern has already been replaced
                }
                try
                {
                    int[] node_index = new int[1];
                    IntPtr marsh_node_name = (IntPtr)Marshal.StringToHGlobalAnsi(node.name);
                    int ret_val = ENgetnodeindex(marsh_node_name, node_index);
                    if (ret_val < 0)
                        throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                    Marshal.FreeHGlobal(marsh_node_name);
                    float[] original_pat_index = new float[1];
                    ret_val = ENgetnodevalue(node_index[0], Constants.EN_PATTERN, original_pat_index); //get original pattern of this node
                    if (ret_val < 0)
                        throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                    ret_val = ENsetnodevalue(node_index[0], Constants.EN_PATTERN, (float)index); //replace the original pattern
                    if (ret_val < 0)
                        throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                    this.node = node;
                    replaced_pat_index = (int)original_pat_index[0];
                    return (0);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return (-1);
                }
            }                      

            /// <summary>For node stored in this.node, replaces work pattern with original demand pattern
            /// </summary>
            /// <returns>0 if successful</returns>
            public int ReplaceWorkWithOriginal()
            {
                try
                {
                    if (node == null)
                        throw new Exception("Node not allocated. Call ReplaceOriginalWithWork before calling ReplaceWorkWithOriginal");
                    int[] node_index = new int[1];
                    IntPtr marsh_node_name = (IntPtr)Marshal.StringToHGlobalAnsi(node.name);
                    int ret_val = ENgetnodeindex(marsh_node_name, node_index);
                    if (ret_val < 0)
                        throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                    Marshal.FreeHGlobal(marsh_node_name);
                    ret_val = ENsetnodevalue(node_index[0], Constants.EN_PATTERN, (float)replaced_pat_index);
                    if (ret_val < 0)
                        throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                    if (original_base_dem_zero) //if this node originally had base demand = 0
                    {
                        float tmp_float = 0;
                        ret_val = ENsetnodevalue(node_index[0], Constants.EN_BASEDEMAND, tmp_float);
                        original_base_dem_zero = false;
                        if (ret_val < 0)
                            throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                    }
                    replaced_pat_index = -1;
                    node = null;
                    return (0);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return (-1);
                }
            }
            
        }

        public bool en_toolkit_open { get; private set; }//= false; ///true if ENopen() successfully called and we can now call other EN_ functions
        bool en_hyd_anal_open = false; ///true if ENopenH() successfully called and we can now call step-by-step hydraulic analysis function, i.e. ENinitH, ENnextH, ENrunH
        //public WorkPattern workPattern; ///use this to avoid calling ENaddpatern each time we need to simulate different demand at some node
        public List<WorkPattern> listWorkPattern; ///use this to avoid calling ENaddpatern each time we need to simulate different demand at some node
        EpanetError EPAerr = new EpanetError(); ///access to list of EPAnet errors
        //[MarshalAs(UnmanagedType.I8)]        Int64 dupa; 

        /// <summary>
        /// Read water network data using epanet2.dll library
        /// </summary>
        /// <param name="waterNetwork">water network model</param>
        /// <returns>EPAnet error code</returns>
        public int ReadEpanetDll(WaterNetwork waterNetwork)
        {
            try
            {
                int error = 0; //error code
                EpanetError EPAerr = new EpanetError();//access to list of EPAnet errors

                string epanetInputFile = Settings.Default.orginalInpFile;
                string epanetResultsFile = Settings.Default.epanetResultsFile;
                string epanetReportFile = Settings.Default.epanetReportFile;

                // Marshal the path strings to unmanaged memory
                IntPtr inpPointer = (IntPtr)Marshal.StringToHGlobalAnsi(epanetInputFile);
                IntPtr rptPointer = (IntPtr)Marshal.StringToHGlobalAnsi(epanetReportFile);
                IntPtr binPointer = (IntPtr)Marshal.StringToHGlobalAnsi(epanetResultsFile);

                //open access to data via epanet2.dll
                error = ENopen(inpPointer, rptPointer, binPointer);

                if (error > 0)
                {
                    ENclose();
                    //free the unmanaged memory
                    Marshal.FreeHGlobal(inpPointer);
                    Marshal.FreeHGlobal(rptPointer);
                    Marshal.FreeHGlobal(binPointer);
                    throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg);
                }


                //get number of elements 
                int[] nPatterns = new int[1];   //numner of patterns
                int[] nCurves = new int[1];     //number of curves
                int[] nNodes = new int[1];     //number of nodes
                int[] nControls = new int[1];   //number of controls

                error = ENgetcount(Constants.EN_PATCOUNT, nPatterns); //get number of Time patterns 
                if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                error = ENgetcount(Constants.EN_CURVECOUNT, nCurves); //get number of curves
                if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                error = ENgetcount(Constants.EN_NODECOUNT, nNodes); //get number of nodes
                if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                error = ENgetcount(Constants.EN_CONTROLCOUNT, nControls); //get number of controls
                if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                if (nNodes[0] != waterNetwork.nNodes)//nodes number verification
                {
                    throw new Exception("Number of nodes in results file and inp file is diffrent.");
                }

                //int   i, NumNodes;
                //long  t, tstep;
                //float p;
                //char  id[32];
                //ENgetcount(EN_NODECOUNT, &NumNodes);
                //ENopenH();
                //ENinitH(0);
                //do {
                //  ENrunH(&t);
                //  for (i = 1; i <= NumNodes; i++) {
                //    ENgetnodevalue(i, EN_PRESSURE, &p);
                //    ENgetnodeid(i, id);
                //    writetofile(t, id, p);
                //  }
                //  ENnextH(&tstep);
                //} while (tstep > 0);
                //ENcloseH();  

                /*
                long[] tStep = new long[1];
                long[] simulationDuration = new long[1];
                
                error =  ENgettimeparam(Constants.EN_HYDSTEP, tStep); //get hydrualic time step
                if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }
                error = ENgettimeparam(Constants.EN_DURATION, simulationDuration); //get hydrualic time step
                if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }
                error = ENopenH();
                if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }
                error = ENinitH(0);
                if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                float[] temp = new float[simulationDuration[0]/tStep[0]];

                do
                {

                    for (int i = 1; i <= nNodes[0]; i++)
                    {
                       // ENgetnodevalue(i, Constants.EN_PRESSURE, &p);
                       // ENgetnodeid(i, id);
                        
                    }

                } while (tStep[0] > 0);
                error = ENcloseH();
                if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }
                */

                //update pattern label in list of nodes
                for (int i = 1; i <= waterNetwork.nNodes; i++)
                {
                    Node node_tmp = null;
                    float[] patindex = new float[1]; //pattern index
                    float[] basedemand = new float[1];
                    byte[] nodeid = new byte[32]; // node label
                    byte[] patternid = new byte[32]; // pattern label


                    error = ENgetnodevalue(i, Constants.EN_PATTERN, patindex);
                    if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                    if (patindex[0] > 0) //when index = 0 it means that no pattern has been assgined to node
                    {
                        error = ENgetpatternid((int)patindex[0], patternid);
                        if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                        error = ENgetnodeid(i, nodeid);
                        if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                        error = ENgetnodevalue(i, Constants.EN_BASEDEMAND, basedemand);
                        if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }


                        node_tmp = waterNetwork.listOfNodes.Find(tmp => tmp.name == ByteToString(nodeid));
                        if (node_tmp != null)
                        {
                            node_tmp.patternName = ByteToString(patternid);
                            node_tmp.demandFactor = basedemand[0];//update demand factor;
                        }
                    }
                }

                //update tank structure 
                for (int i = 1; i <= waterNetwork.nNodes; i++)
                {

                    int[] nodeType = new int[1];
                    float[] curveIndex = new float[1];
                    byte[] tankId = new byte[32];
                    float[] initLevel = new float[1];
                    float[] minLevel = new float[1];
                    float[] maxLevel = new float[1];
                    float[] diameter = new float[1];
                    float[] minVol = new float[1];

                    error = ENgetnodetype(i, nodeType); //get type of the node
                    if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                    if (nodeType[0] == Constants.EN_TANK)
                    {
                        Tank tankTemp = null;
                        error = ENgetnodeid(i, tankId); //get node id label
                        if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                        tankTemp = waterNetwork.listOfTanks.Find(tmp => tmp.node.name == ByteToString(tankId)); //serach for a tank
                        if (tankTemp != null)
                        {
                            //get tank parameters
                            error = ENgetnodevalue(i, Constants.EN_TANKLEVEL, initLevel);
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            error = ENgetnodevalue(i, Constants.EN_TANKDIAM, diameter);
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            error = ENgetnodevalue(i, Constants.EN_MINLEVEL, minLevel);
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            error = ENgetnodevalue(i, Constants.EN_MAXLEVEL, maxLevel);
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }


                            error = ENgetnodevalue(i, Constants.EN_MINVOLUME, minVol);
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            error = ENgetnodevalue(i, Constants.EN_VOLCURVE, curveIndex);
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            tankTemp.initLevel = initLevel[0];
                            tankTemp.minLevel = minLevel[0];
                            tankTemp.maxLevel = maxLevel[0];
                            tankTemp.diameter = diameter[0] / (float)(Math.Sqrt(4 / Math.PI));//bug in the code and when you get the tank diameter, you should divide the result by (sqrt(4/PI)).
                            tankTemp.minVolume = minVol[0];
                            tankTemp.volumeCurve = curveIndex[0].ToString();


                        }
                    }

                }

                /* update pipes structure*/
                int[] nlinks = new int[1]; //number of links

                error = ENgetcount(Constants.EN_LINKCOUNT, nlinks);//get number of links
                if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                if (nlinks[0] != waterNetwork.nLinks)//links number verification
                {
                    throw new Exception("Number of links in results file and inp file is diffrent.");
                }

                for (int i = 1; i <= nlinks[0]; i++)
                {
                    Link link = null;
                    int[] linktype = new int[1]; //pipe type
                    byte[] linkid = new byte[32]; // pipe label
                    float[] status = new float[1]; //pipe status
                    float[] minorloss = new float[1];// minorloss
                    float[] setting = new float[1]; // setting 

                    error = ENgetlinktype(i, linktype);//get pipe type
                    if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                    error = ENgetlinkid(i, linkid);//get link lablel
                    if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                    link = waterNetwork.listOfLinks.Find(tmp => tmp.name == ByteToString(linkid));//find link in list of links
                    if (link != null)
                    {
                        if (linktype[0] == Constants.EN_PIPE)
                        {
                            error = ENgetlinkvalue(i, Constants.EN_INITSTATUS, status);//get link status; open, closed, CV
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            switch ((int)status[0])
                            {
                                case 0:
                                    link.status = "Closed";
                                    //04-05-2011 lines added to retain pipes with closed status. 
                                    link.type = Constants.EN_VV;
                                    link.valve = new Valve(link);
                                    waterNetwork.listOfValves.Add(link.valve);

                                    break;
                                case 1:
                                    link.status = "Open";
                                    break;
                            }

                            error = ENgetlinkvalue(i, Constants.EN_MINORLOSS, minorloss);//get minor loss parameter
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }
                            link.minorLoss = minorloss[0];

                        }
                        else if (linktype[0] == Constants.EN_CVPIPE) /* update status field when pipe is with check valve */
                        {
                            error = ENgetlinkvalue(i, Constants.EN_INITSTATUS, status);
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            link.status = "CV";

                            error = ENgetlinkvalue(i, Constants.EN_MINORLOSS, minorloss);
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            link.minorLoss = minorloss[0];

                        }
                        else if (linktype[0] == Constants.EN_PUMP)/* update setting field for pumps */
                        {

                            error = ENgetlinkvalue(i, Constants.EN_INITSETTING, setting); //get initial setting
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }
                            link.setting = setting[0];

                            error = ENgetlinkvalue(i, Constants.EN_INITSTATUS, status);//get link status; open, closed, CV
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            switch ((int)status[0])
                            {
                                case 0:
                                    link.status = "Closed";
                                    break;
                                case 1:
                                    link.status = "Open";
                                    break;
                            }

                        }
                        else /* update valves */
                        {
                            error = ENgetlinkvalue(i, Constants.EN_INITSETTING, setting); //get initial setting
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            link.setting = setting[0];

                            error = ENgetlinkvalue(i, Constants.EN_MINORLOSS, minorloss);
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            link.minorLoss = minorloss[0];

                            error = ENgetlinkvalue(i, Constants.EN_INITSTATUS, status);//get link status; open, closed, CV
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            switch ((int)status[0])
                            {
                                case 0:
                                    link.status = "Closed";
                                    break;
                                case 1:
                                    link.status = "Open";
                                    break;
                            }


                        }

                    }

                }




                /* Update list of patterns read from inp file */
                for (int i = 1; i <= nPatterns[0]; i++)
                {
                    Pattern pattern = new Pattern();
                    byte[] patternid = new byte[32];
                    int[] patternlen = new int[1];
                    float[] value = new float[1];

                    error = ENgetpatternid(i, patternid); //get pattern id label
                    if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                    error = ENgetpatternlen(i, patternlen);//get pattern length
                    if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                    pattern.values = new double[patternlen[0]];
                    for (int j = 1; j <= patternlen[0]; j++)
                    {
                        error = ENgetpatternvalue(i, j, value);//get pattern's values
                        if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                        pattern.values[j - 1] = value[0];
                    }
                    pattern.name = ByteToString(patternid);
                    pattern.length = patternlen[0];
                    waterNetwork.listOfPatterns.Add(pattern);//update list of patterns
                }
                waterNetwork.nPatterns = waterNetwork.listOfPatterns.Count();//count numer of patterns

                /*get controls and retain links with associated controls*/
                for (int i = 1; i <= nControls[0]; i++)
                {
                    Controls control = new Controls();

                    Link link = null;
                    Valve valve = null;
                    byte[] linkid = new byte[32]; // pipe label

                    int[] controlType = new int[1];
                    int[] linkIndex = new int[i];
                    float[] setting = new float[1];
                    int[] nodeIndex = new int[1];
                    float[] value = new float[1];


                    error = ENgetcontrol(i, controlType, linkIndex, setting, nodeIndex, value);
                    if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                    control.ctype = controlType[0];
                    control.lindex = linkIndex[0];
                    control.setting = setting[0];
                    control.nindex = nodeIndex[0];
                    control.level = value[0];


                    waterNetwork.listOfControls.Add(control);

                    error = ENgetlinkid(linkIndex[0], linkid);//get link lablel
                    if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                    link = waterNetwork.listOfLinks.Find(tmp => tmp.name == ByteToString(linkid));//find link in list of links
                    if (link != null)
                    {
                        if (link.type == Constants.EN_PIPE)
                        {
                            valve = waterNetwork.listOfValves.Find(tmp => tmp.link.name == ByteToString(linkid));//check does virtual valve already exist
                            if (valve == null)
                            {
                                link.type = Constants.EN_VV;
                                link.valve = new Valve(link);
                                //waterNetwork.listOfLinks[i].valve = new Valve(waterNetwork.listOfLinks[i]);
                                //waterNetwork.listOfValves.Add(waterNetwork.listOfLinks[i].valve);
                                waterNetwork.listOfValves.Add(link.valve);
                            }
                        }
                    }
                }

                waterNetwork.nValves = waterNetwork.listOfValves.Count();
                waterNetwork.nPipes = waterNetwork.listOfLinks.Count() - waterNetwork.listOfValves.Count() - waterNetwork.listOfPumps.Count();

                //get emitter exponent
                float[] emitterExponent = new float[1];
                error = ENgetoption(Constants.EN_EMITEXPON, emitterExponent);
                if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }
                else
                {
                    waterNetwork.emitterExponent = emitterExponent[0];
                }


                //free the unmanaged string
                Marshal.FreeHGlobal(inpPointer);
                Marshal.FreeHGlobal(rptPointer);
                Marshal.FreeHGlobal(binPointer);

                error = ENclose();//close EPAnet dll
                if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }



                return error;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.Read();
                return (-1);
            }
        }

        /// <summary>
        /// Read water network data directly from inp file
        /// </summary>
        /// <param name="waterNetwork">water network model</param>
        /// <returns>error code</returns>
        public int ReadInpFile(WaterNetwork waterNetwork)
        {
            try
            {
                string epanetInputFile = Settings.Default.orginalInpFile;

                StreamReader inpFile = new StreamReader(epanetInputFile);//open inp file to read
                string line; //line to be read from inp file


                //Get curves
                long position = FindSection("[CURVES]", inpFile);

                string[] curveData = new string[3]; //contains curve's data: label, x and y values

                line = inpFile.ReadLine();//read line from file
                while (!line.StartsWith("["))//while not start of new section
                {
                    line = inpFile.ReadLine();//read line from file

                    while (!line.StartsWith(";") && !line.StartsWith("[") && line.Length != 0)
                    {
                        //split line and remove empty entries 
                        string[] split = line.Split(new Char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                        if (split[0].Trim() != "" && split[1].Trim() != "" && split[2].Trim() != "")
                        {
                            curveData[0] = split[0];//cvure name
                            curveData[1] += split[1] + ":";//x values seperated by ":"
                            curveData[2] += split[2] + ":";//y values seperated by ":"
                        }

                        line = inpFile.ReadLine();

                    }
                    if (curveData[0] != null)
                    {
                        AddCurve(curveData, waterNetwork);//add curve to water network object
                        curveData = new string[3];//reset curve data
                    }

                }
                waterNetwork.nCurves = waterNetwork.listOfCurves.Count();//count number of curves

                //associate curves to pumps 
                //NOTE: HEAD curves only
                position = FindSection("[PUMPS]", inpFile);//find pumps section

                line = inpFile.ReadLine();

                while (!line.StartsWith("["))//look for start of new section
                {
                    while (!line.StartsWith(";") && line.Length != 0) //skip empty lines and comments
                    {
                        string[] split = line.Split(new Char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < split.Length; i++)
                        {
                            if (split[i] == "HEAD")//look for "HEAD" string
                            {
                                string label = split[i + 1];//next element indicates curve name
                                Curve curve = waterNetwork.listOfCurves.Find(tmp => tmp.name == label);//verify
                                if (curve != null)
                                {
                                    Pump pump = waterNetwork.listOfPumps.Find(tmp => tmp.link.name == split[0]);
                                    if (pump != null)
                                    {
                                        pump.curveName = label;//update pump structure
                                        break;
                                    }
                                    else
                                    {
                                        throw new Exception("Cannot associate head curves to pumps. Curve or pump not found.");
                                    }

                                }
                                else
                                {
                                    throw new Exception("Cannot associate head curves to pumps. Curve or pump not found.");
                                }
                            }
                        }
                        line = inpFile.ReadLine();

                    }
                    line = inpFile.ReadLine();

                }

                //get node coordinates
                position = FindSection("[COORDINATES]", inpFile);//find coordinates section

                line = inpFile.ReadLine();
                while (!line.StartsWith("["))
                {
                    if (!line.StartsWith(";") && line.Length != 0)
                    {
                        string[] split = line.Split(new Char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        UpdateNodeCoord(split, waterNetwork);//update node coordinates fields

                    }
                    line = inpFile.ReadLine();

                }

                //get node tags
                position = FindSection("[TAGS]", inpFile);//find tags section

                line = inpFile.ReadLine();
                while (!line.StartsWith("["))
                {
                    if (!line.StartsWith(";") && line.Length != 0)
                    {
                        string[] split = line.Split(new Char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (split[0] == "NODE")
                        {
                            UpdateNodeTag(split, waterNetwork);//update node tag
                        }
                        if (split[0] == "LINK")
                        {
                            UpdateLink(split, waterNetwork);
                        }

                    }
                    line = inpFile.ReadLine();

                }

                //get node emitter coefficient
                position = FindSection("[EMITTERS]", inpFile);//find emitters section

                if (position != -1)
                {

                    line = inpFile.ReadLine();
                    while (!line.StartsWith("["))
                    {
                        if (!line.StartsWith(";") && line.Length != 0)
                        {
                            string[] split = line.Split(new Char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            UpdateNodeEmitter(split, waterNetwork);

                        }
                        line = inpFile.ReadLine();

                    }
                }
                else
                {
                    return (-1);
                }



                inpFile.Close();//close inp file
                return (0);
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                //Console.Read();
                MessageBox.Show(ex.Message);
                return (-1);
            }
        }
        /// <summary>
        /// The function converts array of bytes to string
        /// </summary>
        public string ByteToString(byte[] orginal)
        {
            char[] temp_char = new char[orginal.Length];
            string name;

            for (int i = 0; i < orginal.Length; i++)
            {
                temp_char[i] = Convert.ToChar(orginal[i]);
            }
            name = new string(temp_char);

            return name.Trim('\0');
        }

        /// <summary>
        /// Finds specified section in inp file
        /// </summary>
        /// <param name="section">Searched section</param>
        /// <param name="file">INP file</param>
        /// <returns>Position of section in file</returns>
        public long FindSection(string section, StreamReader file)
        {
            try
            {
                string line;
                bool found = false;
                file.BaseStream.Position = 1;//go to start position
                while (!file.EndOfStream)//find section in inp file
                {
                    line = file.ReadLine();
                    if (line == section)
                    {
                        found = true;
                        break;
                    }
                }
                if (found) //return position if found 
                {
                    return file.BaseStream.Position;
                }
                else //else trow exception
                {
                    throw new EndOfStreamException("Section " + section + " not found in inp file. Use export option from the Epanet2 software to save your INP file in a correct format.");                   

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return (-1);
            }

        }

        /// <summary>
        /// Adds curve to list of curves in water network object
        /// </summary>
        /// <param name="data">Curve data read from inp file</param>
        /// <param name="waterNetwork">Water network object</param>
        public void AddCurve(string[] data, WaterNetwork waterNetwork)
        {
            Curve curve = new Curve();
            int length = 0; //number of points in curve

            if (data[1] != null && data[2] != null)
            {
                //split string and remove ":"
                string name = data[0];
                string[] x = data[1].Split(new Char[] { ':' });
                string[] y = data[2].Split(new Char[] { ':' });

                if (x.Length != y.Length)
                {
                    throw new Exception("Curve X data length and curve Y data length is not equal.");
                }
                else
                {
                    length = x.Length - 1; //-1 due to last ":" in the string 
                    curve.x = new double[length];//allocate memory
                    curve.y = new double[length];


                    for (int i = 0; i < length; i++)//convert string to double
                    {
                        curve.x[i] = Convert.ToDouble(x[i]);
                        curve.y[i] = Convert.ToDouble(y[i]);
                    }
                    curve.name = name;
                    curve.length = length;
                    waterNetwork.listOfCurves.Add(curve);//add curve
                }
            }
            else
            {
                throw new Exception("Curves x and y data is empty.");
            }


        }

        /// <summary>
        /// Updates node coordinates fields
        /// </summary>
        /// <param name="data">Contains coordinates string data</param>
        /// <param name="waterNetwork">Water network object</param>
        public void UpdateNodeCoord(string[] data, WaterNetwork waterNetwork)
        {
            Node node = new Node();

            node = waterNetwork.listOfNodes.Find(tmp => tmp.name == data[0]);
            if (node != null)
            {
                node.xcoord = Convert.ToSingle(data[1]);
                node.ycoord = Convert.ToSingle(data[2]);
            }
            else
            {
                throw new Exception("Cannot update node's coordinations: Node not found");
            }

        }
        /// <summary>
        /// Updates node tag field.
        /// </summary>
        /// <param name="data">Contains tag string data</param>
        /// <param name="waterNetwork">Water network object</param>
        public void UpdateNodeTag(string[] data, WaterNetwork waterNetwork)
        {
            Node node = new Node();

            node = waterNetwork.listOfNodes.Find(tmp => tmp.name == data[1]);
            if (node != null)
            {
                node.tag = data[2];
            }
            else
            {
                throw new Exception("Cannot update node's tag: Node not found");
            }

        }

        /// <summary>
        /// Updates link tag field. Used to retain the specified pipes. e.g. used in rules
        /// </summary>
        /// <param name="data">Contains tag string data</param>
        /// <param name="waterNetwork">Water network object</param>
        public void UpdateLink(string[] data, WaterNetwork waterNetwork)//added 4-5-2011
        {
            Link link = new Link();
            Valve valve = new Valve();

            link = waterNetwork.listOfLinks.Find(tmp => tmp.name == data[1]);
            if (link != null)
            {
                valve = waterNetwork.listOfValves.Find(tmp => tmp.link.name == data[1]);
                if (valve == null)
                {
                    link.type = Constants.EN_VV;
                    link.valve = new Valve(link);
                    waterNetwork.listOfValves.Add(link.valve);
                }
            }
            else
            {
                throw new Exception("Cannot update links's tag: Link not found");
            }

        }

        /// <summary>
        /// Updates node emitterCoefficient field
        /// </summary>
        /// <param name="data">Contains read string data</param>
        /// <param name="waterNetwork">Water network object</param>
        public void UpdateNodeEmitter(string[] data, WaterNetwork waterNetwork)
        {
            Node node = new Node();

            node = waterNetwork.listOfNodes.Find(tmp => tmp.name == data[0]);
            if (node != null)
            {
                node.emmitterCoefficient = Convert.ToSingle(data[1]);
                RemoveLeakageFromDemand(node, waterNetwork);
            }
            else
            {
                throw new Exception("Cannot update node's emitter coefficient: Node not found");
            }
        }

        /// <summary>
        /// Removes from demand the outflow that resulted from leakage.
        /// This is needed for GAMS code generation, 
        /// since the leakage is pressure-dependent 
        /// and should not be included in demand.
        /// </summary>
        /// <param name="waterNetwork">Water Network object</param>
        public void RemoveLeakageFromDemand(Node node, WaterNetwork waterNetwork)
        {
            for (int i = 0; i < waterNetwork.nReportingPeriods; i++)
            {
                double leakFlow = 0;
                double pressure = node.head[i] - node.elevation;

                if (pressure < 0)//for networks with a negative pressure emitters represents inflow to network
                {
                    leakFlow = node.emmitterCoefficient * Math.Pow(Math.Abs(pressure), waterNetwork.emitterExponent);
                    node.demand[i] += leakFlow;
                }
                else
                {
                    leakFlow = node.emmitterCoefficient * Math.Pow(pressure, waterNetwork.emitterExponent);
                    node.demand[i] -= leakFlow;
                }
            }


        }

        /// <summary> Read water network data using epanet2.dll library; it should be run in Winform or WPF app, as error messages are displayed as messagebox
        /// </summary>
        /// <param name="waterNetwork">water network model</param>
        /// <param name="close_EN_dll">determines whether ENclose() is called at the end</param>
        /// <returns>EPAnet error code</returns>
        public int ReadEpanetDll_WindowApp(WaterNetwork waterNetwork, bool close_EN_dll)
        {
            try
            {
                int error = 0; //error code
                error = SimulateInEpanetDLL();
                if (error != 0)
                    throw new Exception("Error: Epanet.SimulateInEpanet returned " + error.ToString());
                error = ReadEpanetResults(waterNetwork, true);
                if (error != 0)
                    throw new Exception("Error: Epanet.ReadEpanetResults returned " + error.ToString());
                
                //string epanetInputFile = Settings.Default.path + "\\" + Settings.Default.orginalInpFile;
                //string epanetResultsFile = Settings.Default.path + "\\" + Settings.Default.epanetResultsFile;
                //string epanetReportFile = Settings.Default.path + "\\" + Settings.Default.epanetReportFile;

                string epanetInputFile = Settings.Default.orginalInpFile;
                string epanetResultsFile = Settings.Default.epanetResultsFile;
                string epanetReportFile = Settings.Default.epanetReportFile;

                // Marshal the path strings to unmanaged memory
                
                IntPtr inpPointer = (IntPtr)Marshal.StringToHGlobalAnsi(epanetInputFile);
                IntPtr rptPointer = (IntPtr)Marshal.StringToHGlobalAnsi(epanetReportFile);
                IntPtr binPointer = (IntPtr)Marshal.StringToHGlobalAnsi(epanetResultsFile);
                

                //open access to data via epanet2.dll
                error = ENopen(inpPointer, rptPointer, binPointer);                

                if (error > 0)
                {
                    ENclose();
                    //free the unmanaged memory
                    
                    Marshal.FreeHGlobal(inpPointer);
                    Marshal.FreeHGlobal(rptPointer);
                    Marshal.FreeHGlobal(binPointer);
                      
                    throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg);
                }
                en_toolkit_open = true;

                //get number of elements 
                int[] nPatterns = new int[1];   //numner of patterns
                int[] nCurves = new int[1];     //number of curves
                int[] nNodes = new int[1];     //number of nodes
                int[] nControls = new int[1];   //number of controls

                error = ENgetcount(Constants.EN_PATCOUNT, nPatterns); //get number of Time patterns 
                if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                error = ENgetcount(Constants.EN_CURVECOUNT, nCurves); //get number of curves
                if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                error = ENgetcount(Constants.EN_NODECOUNT, nNodes); //get number of nodes
                if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                error = ENgetcount(Constants.EN_CONTROLCOUNT, nControls); //get number of controls
                if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                if (nNodes[0] != waterNetwork.nNodes)//nodes number verification
                {
                    MessageBox.Show(nNodes[0].ToString() + " " + waterNetwork.nNodes.ToString());
                    throw new Exception("Number of nodes in results file and inp file is diffrent.");
                }

                //update pattern label in list of nodes
                for (int i = 1; i <= waterNetwork.nNodes; i++)
                {
                    Node node_tmp = null;
                    float[] patindex = new float[1]; //pattern index
                    float[] basedemand = new float[1];
                    byte[] nodeid = new byte[32]; // node label
                    byte[] patternid = new byte[32]; // pattern label

                    error = ENgetnodevalue(i, Constants.EN_PATTERN, patindex);
                    if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                    if (patindex[0] > 0) //when index = 0 it means that no pattern has been assgined to node
                    {
                        error = ENgetpatternid((int)patindex[0], patternid);
                        if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                        error = ENgetnodeid(i, nodeid);
                        if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                        error = ENgetnodevalue(i, Constants.EN_BASEDEMAND, basedemand);
                        if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }


                        node_tmp = waterNetwork.listOfNodes.Find(tmp => tmp.name == ByteToString(nodeid));
                        if (node_tmp != null)
                        {
                            node_tmp.patternName = ByteToString(patternid);
                            node_tmp.demandFactor = basedemand[0];//update demand factor;
                        }
                    }
                }

                //update tank structure 
                for (int i = 1; i <= waterNetwork.nNodes; i++)
                {
                    int[] nodeType = new int[1];
                    float[] curveIndex = new float[1];
                    byte[] tankId = new byte[32];
                    float[] initLevel = new float[1];
                    float[] minLevel = new float[1];
                    float[] maxLevel = new float[1];
                    float[] diameter = new float[1];
                    float[] minVol = new float[1];

                    error = ENgetnodetype(i, nodeType); //get type of the node
                    if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                    if (nodeType[0] == Constants.EN_TANK)
                    {
                        Tank tankTemp = null;
                        error = ENgetnodeid(i, tankId); //get node id label
                        if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                        tankTemp = waterNetwork.listOfTanks.Find(tmp => tmp.node.name == ByteToString(tankId)); //serach for a tank
                        if (tankTemp != null)
                        {
                            //get tank parameters
                            error = ENgetnodevalue(i, Constants.EN_TANKLEVEL, initLevel);
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            error = ENgetnodevalue(i, Constants.EN_TANKDIAM, diameter);
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            error = ENgetnodevalue(i, Constants.EN_MINLEVEL, minLevel);
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            error = ENgetnodevalue(i, Constants.EN_MAXLEVEL, maxLevel);
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }


                            error = ENgetnodevalue(i, Constants.EN_MINVOLUME, minVol);
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            error = ENgetnodevalue(i, Constants.EN_VOLCURVE, curveIndex);
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            tankTemp.initLevel = initLevel[0];
                            tankTemp.minLevel = minLevel[0];
                            tankTemp.maxLevel = maxLevel[0];
                            tankTemp.diameter = diameter[0] / (float)(Math.Sqrt(4 / Math.PI));//bug in the code and when you get the tank diameter, you should divide the result by (sqrt(4/PI)).
                            tankTemp.minVolume = minVol[0];
                            tankTemp.volumeCurve = curveIndex[0].ToString();
                        }
                    }
                }

                /* update pipes structure*/
                int[] nlinks = new int[1]; //number of links

                error = ENgetcount(Constants.EN_LINKCOUNT, nlinks);//get number of links
                if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                if (nlinks[0] != waterNetwork.nLinks)//links number verification
                {
                    throw new Exception("Number of links in results file and inp file is diffrent.");
                }
                for (int i = 1; i <= nlinks[0]; i++)
                {
                    Link link = null;
                    int[] linktype = new int[1]; //pipe type
                    byte[] linkid = new byte[32]; // pipe label
                    float[] status = new float[1]; //pipe status
                    float[] minorloss = new float[1];// minorloss
                    float[] setting = new float[1]; // setting 

                    error = ENgetlinktype(i, linktype);//get pipe type
                    if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                    error = ENgetlinkid(i, linkid);//get link lablel
                    if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                    link = waterNetwork.listOfLinks.Find(tmp => tmp.name == ByteToString(linkid));//find link in list of links
                    if (link != null)
                    {
                        if (linktype[0] == Constants.EN_PIPE)
                        {
                            error = ENgetlinkvalue(i, Constants.EN_INITSTATUS, status);//get link status; open, closed, CV
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            switch ((int)status[0])
                            {
                                case 0:
                                    link.status = "Closed";
                                    //04-05-2011 lines added to retain pipes with closed status. 
                                    link.type = Constants.EN_VV;
                                    link.valve = new Valve(link);
                                    waterNetwork.listOfValves.Add(link.valve);

                                    break;
                                case 1:
                                    link.status = "Open";
                                    break;
                            }

                            error = ENgetlinkvalue(i, Constants.EN_MINORLOSS, minorloss);//get minor loss parameter
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }
                            link.minorLoss = minorloss[0];

                        }
                        else if (linktype[0] == Constants.EN_CVPIPE) /* update status field when pipe is with check valve */
                        {
                            error = ENgetlinkvalue(i, Constants.EN_INITSTATUS, status);
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            link.status = "CV";

                            error = ENgetlinkvalue(i, Constants.EN_MINORLOSS, minorloss);
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            link.minorLoss = minorloss[0];
                        }
                        else if (linktype[0] == Constants.EN_PUMP)/* update setting field for pumps */
                        {

                            error = ENgetlinkvalue(i, Constants.EN_INITSETTING, setting); //get initial setting
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }
                            link.setting = setting[0];

                            error = ENgetlinkvalue(i, Constants.EN_INITSTATUS, status);//get link status; open, closed, CV
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            switch ((int)status[0])
                            {
                                case 0:
                                    link.status = "Closed";
                                    break;
                                case 1:
                                    link.status = "Open";
                                    break;
                            }
                        }
                        else /* update valves */
                        {
                            error = ENgetlinkvalue(i, Constants.EN_INITSETTING, setting); //get initial setting
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            link.setting = setting[0];

                            error = ENgetlinkvalue(i, Constants.EN_MINORLOSS, minorloss);
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            link.minorLoss = minorloss[0];

                            error = ENgetlinkvalue(i, Constants.EN_INITSTATUS, status);//get link status; open, closed, CV
                            if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                            switch ((int)status[0])
                            {
                                case 0:
                                    link.status = "Closed";
                                    break;
                                case 1:
                                    link.status = "Open";
                                    break;
                            }
                        }
                    }
                }

                /* Update list of patterns read from inp file */
                for (int i = 1; i <= nPatterns[0]; i++)
                {
                    Pattern pattern = new Pattern();
                    byte[] patternid = new byte[32];
                    int[] patternlen = new int[1];
                    float[] value = new float[1];

                    error = ENgetpatternid(i, patternid); //get pattern id label
                    if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                    error = ENgetpatternlen(i, patternlen);//get pattern length
                    if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                    pattern.values = new double[patternlen[0]];
                    for (int j = 1; j <= patternlen[0]; j++)
                    {
                        error = ENgetpatternvalue(i, j, value);//get pattern's values
                        if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                        pattern.values[j - 1] = value[0];
                    }
                    pattern.name = ByteToString(patternid);
                    pattern.length = patternlen[0];
                    waterNetwork.listOfPatterns.Add(pattern);//update list of patterns
                }
                waterNetwork.nPatterns = waterNetwork.listOfPatterns.Count();//count numer of patterns

                /*get controls and retain links with associated controls*/
                for (int i = 1; i <= nControls[0]; i++)
                {
                    Controls control = new Controls();

                    Link link = null;
                    Valve valve = null;
                    byte[] linkid = new byte[32]; // pipe label

                    int[] controlType = new int[1];
                    int[] linkIndex = new int[i];
                    float[] setting = new float[1];
                    int[] nodeIndex = new int[1];
                    float[] value = new float[1];

                    error = ENgetcontrol(i, controlType, linkIndex, setting, nodeIndex, value);
                    if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                    control.ctype = controlType[0];
                    control.lindex = linkIndex[0];
                    control.setting = setting[0];
                    control.nindex = nodeIndex[0];
                    control.level = value[0];

                    waterNetwork.listOfControls.Add(control);

                    error = ENgetlinkid(linkIndex[0], linkid);//get link lablel
                    if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }

                    link = waterNetwork.listOfLinks.Find(tmp => tmp.name == ByteToString(linkid));//find link in list of links
                    if (link != null)
                    {
                        if (link.type == Constants.EN_PIPE)
                        {
                            valve = waterNetwork.listOfValves.Find(tmp => tmp.link.name == ByteToString(linkid));//check does virtual valve already exist
                            if (valve == null)
                            {
                                link.type = Constants.EN_VV;
                                link.valve = new Valve(link);
                                //waterNetwork.listOfLinks[i].valve = new Valve(waterNetwork.listOfLinks[i]);
                                //waterNetwork.listOfValves.Add(waterNetwork.listOfLinks[i].valve);
                                waterNetwork.listOfValves.Add(link.valve);
                            }
                        }
                    }
                }

                waterNetwork.nValves = waterNetwork.listOfValves.Count();
                waterNetwork.nPipes = waterNetwork.listOfLinks.Count() - waterNetwork.listOfValves.Count() - waterNetwork.listOfPumps.Count();

                //get emitter exponent
                float[] emitterExponent = new float[1];
                error = ENgetoption(Constants.EN_EMITEXPON, emitterExponent);
                if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }
                else
                {
                    waterNetwork.emitterExponent = emitterExponent[0];
                }

                //free the unmanaged string
                
                Marshal.FreeHGlobal(inpPointer);
                Marshal.FreeHGlobal(rptPointer);
                Marshal.FreeHGlobal(binPointer);
                
                
                error = ReadInpFile(waterNetwork);
                if (error == -1) { throw new Exception("Error during reading INP file."); }

                //define type of water network object
                waterNetwork.waterNetworkType = Constants.REAL_NETWORK;
                

                if (close_EN_dll)
                {
                    error = CloseENToolkit();//close EPAnet dll
                    if (error != 0) { throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == error).errorMsg); }
                    return error;
                }
                else return (0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return (-1);
            }
        }//public int ReadEpanetDll

        /// <summary>initialises EN hydraulic analysis system 
        /// </summary>
        /// <returns>code returned by ENopenH</returns>
        public int InitENHydAnalysis()
        {
            int ret_val = ENopenH();
            if (ret_val != 0)
            {
                return (ret_val);
            }
            en_hyd_anal_open = true;
            return (0);
        }

        /// <summary>closes EN hydraulic analysis system  
        /// </summary>
        /// <returns></returns>
        public int CloseENHydAnalysis()
        {
            int ret_val = 0;
            if (en_hyd_anal_open)
                ret_val = ENcloseH();
            en_hyd_anal_open = false;
            return (ret_val);
        }

        /// <summary>Closes the EN toolkit
        /// </summary>
        /// <returns>value returned by ENclose</returns>
        public int CloseENToolkit()
        {
            if (en_hyd_anal_open)
                CloseENHydAnalysis();
            int ret_val = 0;
            if (en_toolkit_open)
                ret_val = ENclose();//close EPAnet dll
            en_toolkit_open = false;
            return (ret_val);
        }

        /// <summary>Sets emitter parameters: exponent for given water network object, and coefficient for given node
        /// </summary>
        /// <param name="waterNetwork"></param>
        /// <param name="node"></param>
        /// <param name="emitterCoeff"></param>
        /// <param name="emitterExponent"></param>
        /// <returns>0 if successful</returns>
        /// <returns>-1 if error encountered</returns>
        public int SetEmitterParameters(WaterNetwork waterNetwork, Node node, double emitterCoeff, double emitterExponent)
        {
            int ret_val;
            try
            {
                if (waterNetwork == null)
                    throw new Exception("Error: WaterNetwork object not allocated!");
                if (node == null)
                    throw new Exception("Error: Node object not allocated!");
                if (!en_toolkit_open)
                    throw new Exception("Error: Epanet toolkit has not been initialised!");
                ret_val = ENsetoption(Constants.EN_EMITEXPON, (float)emitterExponent);
                if (ret_val != 0)
                    throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                int[] node_index = new int[1]; //index to access nodes via ENgetnodevalue
                IntPtr marsh_node_name = (IntPtr)Marshal.StringToHGlobalAnsi(node.name);
                ret_val = ENgetnodeindex(marsh_node_name, node_index);
                Marshal.FreeHGlobal(marsh_node_name);
                if (ret_val != 0)
                    throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                ret_val = ENsetnodevalue(node_index[0], Constants.EN_EMITTER, (float)emitterCoeff);
                if (ret_val != 0)
                    throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return (-1);
            }
            return (0);
        }

        /// <summary>Adds new flat demand to a node by manipulating its demand pattern (Epanet toolkit doesn't allow to simply add new demand)
        /// </summary>
        /// <param name="waterNetwork"></param>
        /// <param name="node"></param>
        /// <param name="demand"></param>
        /// <param name="index_allocated_workpat">index of listWorkPattern to which this demand was allocated (-1 if error occured and demand was not allocated)</param>
        /// <returns></returns>
        public int AddFlatDemand(Node node, double demand, out int index_allocated_workpat)
        {
            index_allocated_workpat = -1;
            try
            {                
                int ret_val;
                if (listWorkPattern == null)
                    listWorkPattern = new List<WorkPattern>();
                WorkPattern workPattern = listWorkPattern.Find(tmp => tmp.node == null); //get 1st workPattern which has no node assigned, i.e. is available
                if (workPattern == null) //no available workPatterns, or list of workPatterns is empty, create new workpattern and add it to the list
                {
                    workPattern = new WorkPattern("chuj1ci2kurwa3w4dupe" + listWorkPattern.Count.ToString());
                    listWorkPattern.Add(workPattern);
                }
                //!!!TODO: consider what happens if EN toolkit is closed but epanet object is not destroyed, or vice-versa
                /*if (workPattern == null)
                    workPattern = new WorkPattern("chuj1ci2kurwa3w4dupe5Epanet");*/

                int[] node_index = new int[1];
                IntPtr marsh_node_name = (IntPtr)Marshal.StringToHGlobalAnsi(node.name);
                ret_val = ENgetnodeindex(marsh_node_name, node_index);
                if (ret_val < 0)
                    throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                Marshal.FreeHGlobal(marsh_node_name);                
                float[] base_demand = new float[1];
                ret_val = ENgetnodevalue(node_index[0], Constants.EN_BASEDEMAND, base_demand); //get base demand of this node
                if (ret_val < 0)
                    throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                float[] original_pat_index = new float[1];
                ret_val = ENgetnodevalue(node_index[0], Constants.EN_PATTERN, original_pat_index); //get original pattern of this node
                if (ret_val < 0)
                    throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                int[] pat_length = new int[1];
                ret_val = ENgetpatternlen((int)original_pat_index[0], pat_length); //get pattern length
                if (ret_val < 0)
                    throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);

                float[] pat_current_value = new float[1];
                double pat_offset;
                if (base_demand[0] != 0)
                {
                    pat_offset = demand / base_demand[0];
                    workPattern.original_base_dem_zero = false;
                }
                else //if base demand is 0, set it to 1
                {
                    workPattern.original_base_dem_zero = true;
                    pat_offset = demand;
                    float tmp_float = 1;
                    ret_val = ENsetnodevalue(node_index[0], Constants.EN_BASEDEMAND, tmp_float);
                    if (ret_val < 0)
                        throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                }
                for (int i = 0; i < pat_length[0]; i++) //add the required offset to each element of original pattern when filling new pattern
                {
                    ret_val = ENgetpatternvalue((int)original_pat_index[0], i + 1, pat_current_value); //EPAnet indices start at 1, hence i+1
                    if (ret_val < 0)
                        throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                    ret_val = ENsetpatternvalue(workPattern.index, i + 1, pat_current_value[0] + (float)pat_offset); //EPAnet indices start at 1, hence i+1
                    if (ret_val < 0)
                        throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                }
                ret_val = workPattern.ReplaceOriginalWithWork(node);
                if (ret_val == 0)
                    index_allocated_workpat = listWorkPattern.FindIndex(tmp => tmp == workPattern);
                return (ret_val);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return (-1);
            }

        }

        /// <summary>Simulates the model using ENToolkit (step-by-step) and updates flows in all links and heads in all nodes, while changing PRV setpoints according to their Valve.setting[i] field; 
        /// !!important: hydraulic results are retrieved only for times between sim_results_start and sim_stop_time; 
        /// !!important: clock start time in the epanet model has to be midnight
        /// ENToolkit should be already loaded using ENopen()
        /// </summary>
        /// <param name="water_network"></param>
        /// <param name="list_of_valves">list of valves which setting should be changed during simulation using their Valve.setting[i] field</param>
        /// <param name="sim_results_start">offset in minutes after midnight; indicates starting time when results are retrieved and PRV setpoints are changed</param>
        /// <param name="sim_stop_time">stop time of simulation in minutes after midnight</param>
        /// <param name="sim_time_step">time step (in minutes) to obtain hydraulic results and update PRV setpoints; should be N * EN_HYDSTEP/60 of the epanet model</param>
        /// <returns>0 if successful</returns>
        public int SimulateUpdateHeadsFlows(WaterNetwork water_network, List<Valve> list_of_valves, int sim_results_start, int sim_stop_time, int sim_time_step)
        {
            //TODO!!! Disable all rules/control associated with valves in list_of_valves, so nothing overwrites the PRV setpoints specified during the step-by-step simulation
            //ENsetcontrol, waterNetwork.listOfControls: To remove a control on a particular link, set the lindex parameter to 0. Values for the other parameters in the function will be ignored.
            int ret_val;
            try
            {
                if (water_network == null)
                    throw new Exception("Error: WaterNetwork object not allocated!");
                if ((water_network.listOfNodes == null) || (water_network.listOfLinks == null))
                    throw new Exception("Error: Nodes and/or links not allocated in WaterNetwork object!");
                if (!en_toolkit_open)
                    throw new Exception("Error: Epanet toolkit has not been initialised!");

                //long kupa = (sim_stop_time * 60);
                //dupa = (sim_stop_time * 60);
                //ret_val = ENsettimeparam(Constants.EN_DURATION, (sim_stop_time * 60)); //simulate only until sim_duration to speed up calculations
                //ret_val = ENsettimeparam(Constants.EN_DURATION, (long)(sim_stop_time * 60)); //simulate only until sim_duration to speed up calculations
                //if (ret_val != 0)
                    //throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                long[] time_step = new long[1];
                ret_val = ENgettimeparam(Constants.EN_HYDSTEP, time_step);
                if (ret_val != 0)
                    throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                if (time_step[0] / 60 > sim_time_step)
                    throw new Exception("Error: Specified time step to obtain hydraulic results is shorter than hydraulic time step in the epanet model. Specified time step: " + sim_time_step.ToString() + ", epanet model hydraulic time step: " + (time_step[0] / 60).ToString());
                int tmp_remainder;
                Math.DivRem(sim_time_step, (int)(time_step[0] / 60), out tmp_remainder);
                if (tmp_remainder != 0)
                    throw new Exception("Error: Specified time step to obtain hydraulic results is not a multiply of hydraulic time step in the epanet model. Specified time step: " + sim_time_step.ToString() + ", epanet model hydraulic time step: " + (time_step[0] / 60).ToString());
                water_network.nReportingPeriods = (int)Math.Round(((double)sim_stop_time - sim_results_start) / sim_time_step);
                //MessageBox.Show(water_network.nReportingPeriods.ToString());

                foreach (Node node in water_network.listOfNodes)
                {
                    node.head = new double[water_network.nReportingPeriods]; //allocate new head array, thus deleting previous results if any exist in this water_network object
                }
                foreach (Link link in water_network.listOfLinks)
                {
                    link.flow = new double[water_network.nReportingPeriods]; //allocate new flow array, thus deleting previous results if any exist in this water_network object
                }

                if (!en_hyd_anal_open) //if EN hydraulic analysis system not initialised, initialise it
                {
                    ret_val = InitENHydAnalysis();
                    if (ret_val != 0)
                        throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                }

                long elapsed_time = 0;
                long tstep = 0;
                int next_hydraulic_time = 0; //current time (in minutes from midnight) for which hydraulic results will be calculated when ENrunH is called; used to access corresponding PRV setpoint and head/flow result arrays
                int index; //index to access arrays associated with nodes, links and valves
                int[] link_index = new int[1]; //index to access links via ENsetlinkvalue and ENgetlinkvalue
                int[] node_index = new int[1]; //index to access nodes via ENgetnodevalue
                float[] link_flow = new float[1];
                float[] node_pressure = new float[1];
                ENinitH(0); //don't save hydraulic results file to speed up calculations
                do
                {
                    //index = (int)Math.Round(((double)next_hydraulic_time - sim_results_start) / 60);
                    index = (int)Math.Round(((double)next_hydraulic_time - sim_results_start) / sim_time_step);
                    if ((index >= 0) && (index < water_network.nReportingPeriods)) //update PRV setpoints only when elapsed simulation time has reached sim_results_start; otherwise PRV setpoint is kept as in the original epanet model
                        foreach (Valve prv in list_of_valves)
                        {
                            IntPtr marsh_prv_name = (IntPtr)Marshal.StringToHGlobalAnsi(prv.link.name);
                            //ret_val = ENgetlinkindex(prv.link.name.ToCharArray(), link_index);
                            ret_val = ENgetlinkindex(marsh_prv_name, link_index);
                            Marshal.FreeHGlobal(marsh_prv_name);
                            if (ret_val != 0)
                                throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                            ret_val = ENsetlinkvalue(link_index[0], Constants.EN_SETTING, (float)prv.setting[index]);
                            if (ret_val != 0)
                                throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                        }
                    ENrunH(ref elapsed_time); //Calculate hydraulic results for time elapsed_time. Returned elapsed_time is in seconds and is the time for which hydraulic result have already been simulated.
                    if ((index >= 0) && (index < water_network.nReportingPeriods)) //retrieve results only when elapsed simulation time has reached sim_results_start
                    {
                        //retrieve node pressures/heads
                        foreach (Node node in water_network.listOfNodes)
                        {
                            IntPtr marsh_node_name = (IntPtr)Marshal.StringToHGlobalAnsi(node.name);
                            ret_val = ENgetnodeindex(marsh_node_name, node_index);
                            //ret_val = ENgetnodeindex(node.name.ToCharArray(), node_index);
                            Marshal.FreeHGlobal(marsh_node_name);
                            if (ret_val != 0)
                                throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                            ret_val = ENgetnodevalue(node_index[0], Constants.EN_PRESSURE, node_pressure);
                            if (ret_val != 0)
                                throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                            node.head[index] = node.elevation + node_pressure[0];
                        }
                        //retrieve link flows
                        foreach (Link link in water_network.listOfLinks)
                        {
                            IntPtr marsh_link_name = (IntPtr)Marshal.StringToHGlobalAnsi(link.name);
                            //ret_val = ENgetlinkindex(link.name.ToCharArray(), link_index);
                            ret_val = ENgetlinkindex(marsh_link_name, link_index);
                            Marshal.FreeHGlobal(marsh_link_name);
                            if (ret_val != 0)
                                throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                            ret_val = ENgetlinkvalue(link_index[0], Constants.EN_FLOW, link_flow);
                            if (ret_val != 0)
                                throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                            link.flow[index] = link_flow[0];
                        }
                    }

                    ENnextH(ref tstep);
                    next_hydraulic_time += (int)tstep / 60;
                } while (tstep > 0);

                return (0);
            } //try
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return (-1);
            }
        }

        /// <summary>Simulates the model using ENToolkit (step-by-step) and updates flows in all links and heads in all nodes, while adding additional demands to nodes (e.g. to simulate burst) and changing PRV setpoints according to their Valve.setting[i] field; 
        /// !!important: hydraulic results are retrieved only for times between sim_results_start and sim_stop_time; 
        /// !!important: clock start time in the epanet model has to be midnight
        /// ENToolkit should be already loaded using ENopen()
        /// </summary>
        /// <param name="water_network"></param>
        /// <param name="list_of_valves">list of valves which setting should be changed during simulation using their Valve.setting[i] field</param>
        /// <param name="sim_results_start">offset in minutes after midnight; indicates starting time when results are retrieved and PRV setpoints are changed</param>
        /// <param name="sim_stop_time">stop time of simulation in minutes after midnight</param>
        /// <param name="sim_time_step">time step (in minutes) to obtain hydraulic results and update PRV setpoints; should be N * EN_HYDSTEP/60 of the epanet model</param>
        /// <param name="list_demands">list of additional demand changes, each list element is an array with demand changes and corresponds to a node in list_nodes_add_demand</param>
        /// <param name="list_nodes_add_demand">list of nodes at which additional demand occurs</param>
        /// <returns>0 if successful</returns>
        public int SimulateAdditionalDemandUpdateHeadsFlows(WaterNetwork water_network, List<Valve> list_of_valves, int sim_results_start, int sim_stop_time, int sim_time_step, List<Node> list_nodes_add_demand, List<double[]> list_demands)
        {
            //TODO!!! Disable all rules/control associated with valves in list_of_valves, so nothing overwrites the PRV setpoints specified during the step-by-step simulation
            //ENsetcontrol, waterNetwork.listOfControls: To remove a control on a particular link, set the lindex parameter to 0. Values for the other parameters in the function will be ignored.
            int ret_val;
            try
            {
                if (water_network == null)
                    throw new Exception("Error: WaterNetwork object not allocated!");
                if ((water_network.listOfNodes == null) || (water_network.listOfLinks == null))
                    throw new Exception("Error: Nodes and/or links not allocated in WaterNetwork object!");
                if (!en_toolkit_open)
                    throw new Exception("Error: Epanet toolkit has not been initialised!");

                //long kupa = (sim_stop_time * 60);
                //dupa = (sim_stop_time * 60);
                //ret_val = ENsettimeparam(Constants.EN_DURATION, (sim_stop_time * 60)); //simulate only until sim_duration to speed up calculations
                //ret_val = ENsettimeparam(Constants.EN_DURATION, (long)(sim_stop_time * 60)); //simulate only until sim_duration to speed up calculations
                //if (ret_val != 0)
                //throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                long[] time_step = new long[1];
                ret_val = ENgettimeparam(Constants.EN_HYDSTEP, time_step);
                if (ret_val != 0)
                    throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                if (time_step[0] / 60 > sim_time_step)
                    throw new Exception("Error: Specified time step to obtain hydraulic results is shorter than hydraulic time step in the epanet model. Specified time step: " + sim_time_step.ToString() + ", epanet model hydraulic time step: " + (time_step[0] / 60).ToString());
                int tmp_remainder;
                Math.DivRem(sim_time_step, (int)(time_step[0] / 60), out tmp_remainder);
                if (tmp_remainder != 0)
                    throw new Exception("Error: Specified time step to obtain hydraulic results is not a multiply of hydraulic time step in the epanet model. Specified time step: " + sim_time_step.ToString() + ", epanet model hydraulic time step: " + (time_step[0] / 60).ToString());
                water_network.nReportingPeriods = (int)Math.Round(((double)sim_stop_time - sim_results_start) / sim_time_step);
                //MessageBox.Show(water_network.nReportingPeriods.ToString());

                foreach (Node node in water_network.listOfNodes)
                {
                    node.head = new double[water_network.nReportingPeriods]; //allocate new head array, thus deleting previous results if any exist in this water_network object
                }
                foreach (Link link in water_network.listOfLinks)
                {
                    link.flow = new double[water_network.nReportingPeriods]; //allocate new flow array, thus deleting previous results if any exist in this water_network object
                }

                if (!en_hyd_anal_open) //if EN hydraulic analysis system not initialised, initialise it
                {
                    ret_val = InitENHydAnalysis();
                    if (ret_val != 0)
                        throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                }

                long elapsed_time = 0;
                long tstep = 0;
                int next_hydraulic_time = 0; //current time (in minutes from midnight) for which hydraulic results will be calculated when ENrunH is called; used to access corresponding PRV setpoint and head/flow result arrays
                int index; //index to access arrays associated with nodes, links and valves
                int[] link_index = new int[1]; //index to access links via ENsetlinkvalue and ENgetlinkvalue
                int[] node_index = new int[1]; //index to access nodes via ENgetnodevalue
                float[] link_flow = new float[1];
                float[] node_pressure = new float[1];
                ENinitH(0); //don't save hydraulic results file to speed up calculations
                do
                {
                    //index = (int)Math.Round(((double)next_hydraulic_time - sim_results_start) / 60);
                    index = (int)Math.Round(((double)next_hydraulic_time - sim_results_start) / sim_time_step);
                    if ((index >= 0) && (index < water_network.nReportingPeriods)) //update PRV setpoints only when elapsed simulation time has reached sim_results_start; otherwise PRV setpoint is kept as in the original epanet model
                    {
                        //Add additional demand
                        //List<int> list_demand_patterns_indices = new List<int>();
                        for (int i = 0; i < list_nodes_add_demand.Count; i++)
                        {                            
                            Node node = list_nodes_add_demand[i];
                            double add_demand = list_demands[i][index];
                            int tmp_pat_index;
                            ret_val = AddFlatDemand(node, add_demand, out tmp_pat_index);
                            if (ret_val < 0)
                                throw new Exception("Error in AddFlatDemand while simulating with additional demand");
                        }
                        //update PRV setpoints
                        foreach (Valve prv in list_of_valves)
                        {
                            IntPtr marsh_prv_name = (IntPtr)Marshal.StringToHGlobalAnsi(prv.link.name);
                            //ret_val = ENgetlinkindex(prv.link.name.ToCharArray(), link_index);
                            ret_val = ENgetlinkindex(marsh_prv_name, link_index);
                            Marshal.FreeHGlobal(marsh_prv_name);
                            if (ret_val != 0)
                                throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                            ret_val = ENsetlinkvalue(link_index[0], Constants.EN_SETTING, (float)prv.setting[index]);
                            if (ret_val != 0)
                                throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                        }
                    }
                    ENrunH(ref elapsed_time); //Calculate hydraulic results for time elapsed_time. Returned elapsed_time is in seconds and is the time for which hydraulic result have already been simulated.
                    if ((index >= 0) && (index < water_network.nReportingPeriods)) //retrieve results only when elapsed simulation time has reached sim_results_start
                    {
                        //remove additional demand before next simulation iteration
                        for (int i = 0; i < list_nodes_add_demand.Count; i++)
                        {
                            WorkPattern workPattern = listWorkPattern.Find(tmp => tmp.node == list_nodes_add_demand[i]);
                            if (workPattern != null)
                            {
                                ret_val = workPattern.ReplaceWorkWithOriginal();
                                if (ret_val < 0)
                                    throw new Exception("Error in ReplaceWorkWithOriginal while simulating with additional demand");
                            }
                            else
                            {
                                throw new Exception("Error: node with additional demand not found in listWorkPattern");
                            }
                        }
                        //retrieve node pressures/heads
                        foreach (Node node in water_network.listOfNodes)
                        {
                            IntPtr marsh_node_name = (IntPtr)Marshal.StringToHGlobalAnsi(node.name);
                            ret_val = ENgetnodeindex(marsh_node_name, node_index);
                            //ret_val = ENgetnodeindex(node.name.ToCharArray(), node_index);
                            Marshal.FreeHGlobal(marsh_node_name);
                            if (ret_val != 0)
                                throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                            ret_val = ENgetnodevalue(node_index[0], Constants.EN_PRESSURE, node_pressure);
                            if (ret_val != 0)
                                throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                            node.head[index] = node.elevation + node_pressure[0];
                        }
                        //retrieve link flows
                        foreach (Link link in water_network.listOfLinks)
                        {
                            IntPtr marsh_link_name = (IntPtr)Marshal.StringToHGlobalAnsi(link.name);
                            //ret_val = ENgetlinkindex(link.name.ToCharArray(), link_index);
                            ret_val = ENgetlinkindex(marsh_link_name, link_index);
                            Marshal.FreeHGlobal(marsh_link_name);
                            if (ret_val != 0)
                                throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                            ret_val = ENgetlinkvalue(link_index[0], Constants.EN_FLOW, link_flow);
                            if (ret_val != 0)
                                throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                            link.flow[index] = link_flow[0];
                        }
                    }

                    ENnextH(ref tstep);
                    next_hydraulic_time += (int)tstep / 60;
                } while (tstep > 0);

                return (0);
            } //try
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return (-1);
            }
        }

        /// <summary>Sets simulation duration using EN toolkit; should be called before InitENHydAnalysis() or after CloseENHydAnalysis()
        /// !!important: clock start time in the epanet model has to be midnight
        /// ENToolkit should be already loaded using ENopen() 
        /// </summary>
        /// <param name="sim_stop_time">stop time of simulation in minutes after midnight</param>
        /// <returns>0 if successful</returns>
        public int SetSimulationStopTime(int sim_stop_time)
        {
            int ret_val = 0;
            try
            {
                if (!en_toolkit_open)
                    throw new Exception("Error: Epanet toolkit has not been initialised!");
                if (en_hyd_anal_open) //if EN hydraulic analysis system is initialised ENsettimeparam can't be called, CloseENHydAnalysis() needs to be called first
                    throw new Exception("Error: Can't change simulation duration after EN hydraulic analysis system has been initialised");
                ret_val = ENsettimeparam(Constants.EN_DURATION, (sim_stop_time * 60)); //simulate only until sim_duration to speed up calculations
                //ret_val = ENsettimeparam(Constants.EN_DURATION, (long)(sim_stop_time * 60)); //FOR 64 bit DLL - simulate only until sim_duration to speed up calculations
                if (ret_val != 0)
                    throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return (-1);
            }
            return (0);
        }

        /// <summary> Compares specified time step to obtain results with hydraulic time step in the epanet model and adjusts epanet model time step if necessary
        /// </summary>
        /// <param name="sim_time_step">time step (in minutes) to obtain hydraulic results and update PRV setpoints</param>
        /// <returns>0 if successful</returns>
        public int VerifyAndSetSimulationHydraulicTimeStep(int sim_time_step)
        {
            int ret_val = 0;
            try
            {
                if (!en_toolkit_open)
                    throw new Exception("Error: Epanet toolkit has not been initialised!");
                if (en_hyd_anal_open) //if EN hydraulic analysis system is initialised ENsettimeparam can't be called, CloseENHydAnalysis() needs to be called first
                    throw new Exception("Error: Can't change simulation duration after EN hydraulic analysis system has been initialised");
                long model_time_step = GetSimulationHydraulicTimeStep();
                int tmp_remainder;
                Math.DivRem(sim_time_step, (int)(model_time_step / 60), out tmp_remainder);
                if ((model_time_step / 60 > sim_time_step) || (tmp_remainder != 0)) //Specified time step to obtain hydraulic results is shorter than hydraulic time step in the epanet model, or is not a multiply of hydraulic time step in the epanet model
                {
                    ret_val = ENsettimeparam(Constants.EN_HYDSTEP, sim_time_step * 60);
                    if (ret_val != 0)
                        throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return (-1);
            }
            return (0);
        }

        public long GetSimulationHydraulicTimeStep()
        {
            try
            {
                if (!en_toolkit_open)
                    throw new Exception("Error: Epanet toolkit has not been initialised!");
                long[] time_step = new long[1];
                int ret_val = ENgettimeparam(Constants.EN_HYDSTEP, time_step);
                if (ret_val != 0)
                    throw new Exception(EPAerr.listOfEpanetErrs.Find(tmp => tmp.errorIndex == ret_val).errorMsg);
                return (time_step[0]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return (-1);
            }
        }

        /// <summary>
        /// Simulates water network through epanet DLL and saves results to binary file;
        /// uses ENepanet so no need to use ENopen before this function is called
        /// </summary>
        /// <returns>Simulation exit code</returns>
        public int SimulateInEpanetDLL()
        {

            string epanetInputFile = Settings.Default.orginalInpFile;
            string epanetResultsFile = Settings.Default.epanetResultsFile;
            string epanetReportFile = Settings.Default.epanetReportFile;
            IntPtr inpPointer = (IntPtr)Marshal.StringToHGlobalAnsi(epanetInputFile);
            IntPtr rptPointer = (IntPtr)Marshal.StringToHGlobalAnsi(epanetReportFile);
            IntPtr binPointer = (IntPtr)Marshal.StringToHGlobalAnsi(epanetResultsFile);            
            
            int returnValue = ENepanet(inpPointer, rptPointer, binPointer, IntPtr.Zero);

            Marshal.FreeHGlobal(inpPointer);
            Marshal.FreeHGlobal(rptPointer);
            Marshal.FreeHGlobal(binPointer);                                   

            return (returnValue);

        }


        /// <summary>
        /// Calls EPAnet hydraulic simulator and simulates specifed water network
        /// </summary>
        /// <returns>Simulation exit code</returns>
        public int SimulateInEpanet()
        {

            string epanetInputFile = Settings.Default.orginalInpFile;
            string epanetResultsFile = Settings.Default.epanetResultsFile;
            string epanetReportFile = Settings.Default.epanetReportFile;
            string arguments = epanetInputFile + " " + epanetReportFile + " " + epanetResultsFile;

            Process epanetProcess = null;
            int returnValue = 0;
            try
            {

                epanetProcess = Process.Start(Settings.Default.epanetEngine, arguments);
                epanetProcess.WaitForExit();
                returnValue = epanetProcess.ExitCode;
            }
            finally
            {
                if (epanetProcess != null)
                {
                    epanetProcess.Close();
                }
            }

            return (returnValue);

        }

        /// <summary>
        /// Read EPAnet simulation results file
        /// </summary>
        /// <param name="waterNetwork">water network model</param>
        /// <param name="is_window_app">determines if error messages are displayed as message_boxes (if 1) or in console (if 0)</param>
        /// <returns>error code</returns>
        public int ReadEpanetResults(WaterNetwork waterNetwork, bool is_window_app)
        /*
         * The Epanet Toolkit uses an unformatted binary output file 
         * to store both hydraulic and water quality results at uniform reporting intervals. 
         * Data written to the file is either 4-byte integers, 4-byte floats, or fixed-size strings 
         * whose size is a multiple of 4 bytes. This allows the file to be divided conveniently 
         * into 4-byte records. The file consists of four sections of the following sizes in bytes: 

           Section              Size in Bytes  
         
           Prolog               884 + 36*Nnodes + 52*Nlinks + 8*Ntanks 
           Energy Use           28*Npumps + 4 
           Dynamic Results      (16*Nnodes + 32*Nlinks)*Nperiods 
           Epilog               28 
         
           where    Nnodes  =  number of nodes (junctions + reservoirs + tanks) 
                    Nlinks  =  number of links (pipes + pumps + valves) 
                    Ntanks  =  number of tanks and reservoirs 
                    Npumps  =  number of pumps 
                    Nperiods  =  number of reporting periods 
    
           and all of these counts are themselves written to the file's prolog or epilog sections. 
         */
        {
            try
            {
                //Open epanet bin file
                string epanetResultsFile = Settings.Default.epanetResultsFile;

                if (!File.Exists(epanetResultsFile))//check that results file exists
                {
                    throw new FileNotFoundException();
                }
                else
                {

                    Stream s = new FileStream(epanetResultsFile, FileMode.Open);
                    BinaryReader binReader = new BinaryReader(s);
                    binReader.BaseStream.Position = 0;

                    //Read the prolog section
                    int buffer = binReader.ReadInt32(); //start magic number
                    buffer = binReader.ReadInt32(); // Get version

                    //Get number of nodes. Number of nodes = Junctions + Reservoirs + Tanks
                    waterNetwork.nNodes = binReader.ReadInt32();

                    int nReservoirAndTanks = binReader.ReadInt32(); //get number of reservoirs and tanks
                    waterNetwork.nJunctions = waterNetwork.nNodes - nReservoirAndTanks; //calculate number of junctions

                    //Get number of links. Number of links = Pipes + Pumps + Valves
                    waterNetwork.nLinks = binReader.ReadInt32();

                    // Get number of pumps
                    waterNetwork.nPumps = binReader.ReadInt32();

                    //Get number of valves
                    waterNetwork.nValves = binReader.ReadInt32(); // Check valves will be included later;

                    buffer = binReader.ReadInt32(); //Omit water quality
                    buffer = binReader.ReadInt32(); //Omit index of node for source tracing

                    /* Get flow units 
                        % 0 = cubic feet/second		
                        % 1 = gallons/minute		
                        % 2 = million gallons/day		
                        % 3 = Imperial million gallons/day		
                        % 4 = acre-ft/day		
                        % 5 = liters/second		
                        % 6 = liters/minute		
                        % 7 = megaliters/day		
                        % 8 = cubic meters/hour		
                        % 9 = cubic meters/day
                     */
                    buffer = binReader.ReadInt32();
                    switch (buffer)
                    {
                        case 0:
                            waterNetwork.flowUnits = "cubic feet/second";
                            break;
                        case 1:
                            waterNetwork.flowUnits = "gallons/minute";
                            break;
                        case 2:
                            waterNetwork.flowUnits = "million gallons/day";
                            break;
                        case 3:
                            waterNetwork.flowUnits = "Imperial million gallons/day";
                            break;
                        case 4:
                            waterNetwork.flowUnits = "acre-ft/day";
                            break;
                        case 5:
                            waterNetwork.flowUnits = "liters/second";
                            break;
                        case 6:
                            waterNetwork.flowUnits = "liters/minute";
                            break;
                        case 7:
                            waterNetwork.flowUnits = "megaliters/day";
                            break;
                        case 8:
                            waterNetwork.flowUnits = "cubic meters/hour";
                            break;
                        case 9:
                            waterNetwork.flowUnits = "cubic meters/day";
                            break;
                        default:
                            waterNetwork.flowUnits = "Unknown flow unit";
                            break;
                    }

                    /* Get Pressure Units
                        % 0 = pounds/square inch		
                        % 1 = meters		
                        % 2 = kiloPascals	
                     */

                    buffer = binReader.ReadInt32();
                    switch (buffer)
                    {
                        case 0:
                            waterNetwork.pressureUnits = "pounds/square inch";
                            break;
                        case 1:
                            waterNetwork.pressureUnits = "meters";
                            break;
                        case 2:
                            waterNetwork.pressureUnits = "kiloPascals";
                            break;
                        default:
                            waterNetwork.pressureUnits = "Unknown pressure unit";
                            break;
                    }

                    buffer = binReader.ReadInt32(); //omit Time Statistics Flag

                    //Get Reporting Start Time (seconds)
                    waterNetwork.reportingStartTime = binReader.ReadInt32();

                    //Get Reporting Step Time (seconds) */
                    waterNetwork.reportingTimeStep = binReader.ReadInt32();

                    //Get Simulation Duration (seconds) 
                    waterNetwork.simulationDuration = binReader.ReadInt32();



                    //Read Problem Title    
                    int stringArrayLength = 80;
                    byte[] byteArray = new byte[stringArrayLength];
                    waterNetwork.title = new string[3];

                    /*  When reading from network streams, in some rare cases, the ReadChars 
                        method might read an extra character from the stream if 
                        the BinaryReader was constructed with Unicode encoding. 
                        If this occurs, you can use the ReadBytes method to read a fixed-length byte array,
                         and then pass that array to the ReadChars method.
                        */

                    for (int i = 0; i < 3; i++)
                    {
                        byteArray = binReader.ReadBytes(stringArrayLength);//read data title 
                        if (byteArray.Length != stringArrayLength)
                        {
                            throw new Exception("Error reading the data from bin file.");
                        }
                        waterNetwork.title[i] = Bytes2String(byteArray);
                    }




                    char[] inputFile = new char[260];
                    inputFile = binReader.ReadChars(260); //name of input file

                    char[] reportFile = new char[260];
                    reportFile = binReader.ReadChars(260); //name of report file

                    char[] chemicalUnits = new char[32];   //name of chemical units
                    chemicalUnits = binReader.ReadChars(32);

                    char[] chemicalonCentratonUnits = new char[32];   //name of chemical concentraton units
                    chemicalonCentratonUnits = binReader.ReadChars(32);

                    /* skip , 
                       name of chemical and chemical concentraton units.
                       This data are not used in Simplifiler.  */
                    //binReader.ReadChars(260 + 260 + 32 + 32);

                    //binReader.ReadChars(260 + 260 + 32 + 32); // it


                    //Get ID string for each node
                    char[] nodeName = new char[32];
                    for (int i = 0; i < waterNetwork.nNodes; i++)
                    {
                        nodeName = binReader.ReadChars(32);
                        Node node = new Node();
                        node.name = new string(nodeName).Trim('\0');//remove all NULL-characters at the end of the string which may be present in EPAnet names
                        node.type = 0; //initially assume this is connection node -> actual type is updated when info about reservoirs and tanks is read
                        waterNetwork.listOfNodes.Add(node);
                    }



                    //Get ID string for each link 
                    char[] linkName = new char[32];
                    for (int i = 0; i < waterNetwork.nLinks; i++)
                    {
                        linkName = binReader.ReadChars(32);
                        Link link = new Link();
                        link.name = new string(linkName).Trim('\0');//remove all NULL-characters at the end of the string which may be present in EPAnet names
                        waterNetwork.listOfLinks.Add(link);
                    }

                    // Get index of source node for all links 
                    for (int i = 0; i < waterNetwork.nLinks; i++)
                    {
                        waterNetwork.listOfLinks[i].nodeFrom = binReader.ReadInt32() - 1; //in EPAnet node numbering starts from 1 !
                    }

                    // Get index of destination for all links
                    for (int i = 0; i < waterNetwork.nLinks; i++)
                    {
                        waterNetwork.listOfLinks[i].nodeTo = binReader.ReadInt32() - 1; //in EPAnet node numbering starts from 1 !
                    }

                    /* Get type of each link 
                        % 0 = Pipe with CV		
                        % 1 = Pipe		
                        % 2 = Pump		
                        % 3 = PRV		
                        % 4 = PSV		
                        % 5 = PBV		
                        % 6 = FCV		
                        % 7 = TCV		
                        % 8 = GPV
                      
                      
                        % 9 = VV (Virtual Valve - used for pipes with the associated controls or pipe with initial status set to closed)
                     */
                    for (int i = 0; i < waterNetwork.nLinks; i++)
                    {
                        buffer = binReader.ReadInt32();
                        waterNetwork.listOfLinks[i].type = buffer;

                        /* NOTE: IN EPANET PIPE WITH CHECK VALVE (CV) DOES NOT COUNT AS VALVE */
                        if ((buffer >= Constants.EN_PRV && buffer <= Constants.EN_GPV) || buffer == Constants.EN_CVPIPE) //valves 
                        {
                            waterNetwork.listOfLinks[i].valve = new Valve(waterNetwork.listOfLinks[i]);
                            waterNetwork.listOfValves.Add(waterNetwork.listOfLinks[i].valve);
                        }
                        if (buffer == Constants.EN_PUMP) //pump
                        {
                            waterNetwork.listOfLinks[i].pump = new Pump(waterNetwork.listOfLinks[i]);
                            waterNetwork.listOfPumps.Add(waterNetwork.listOfLinks[i].pump);
                        }
                    }

                    //Update number of valves 
                    waterNetwork.nValves = waterNetwork.listOfValves.Count();

                    // Calculate no of pipes with check valve */
                    waterNetwork.nPipesCV = 0;
                    foreach (Valve valve in waterNetwork.listOfValves)
                    {
                        if (valve.link.type == Constants.EN_CVPIPE)
                        {
                            waterNetwork.nPipesCV++;
                        }

                    }

                    //Calculate number of pipes 
                    waterNetwork.nPipes = waterNetwork.nLinks - waterNetwork.nPumps - waterNetwork.nValves;


                    // Get node index of each tank or reservoir 
                    int[] tankReservoirIndex = new int[nReservoirAndTanks];
                    for (int i = 0; i < nReservoirAndTanks; i++)
                    {
                        tankReservoirIndex[i] = binReader.ReadInt32();
                    }



                    //Get Cross-Sectional Area of each tank, 0 denotes reservoir */
                    Single buffer_double = 0;
                    for (int i = 0; i < nReservoirAndTanks; i++)
                    {
                        buffer_double = binReader.ReadSingle();
                        if (buffer_double == 0) //this is reservoir
                        {
                            int tmp_index = tankReservoirIndex[i] - 1; //indices in EPAnet start from 1
                            waterNetwork.listOfNodes[tmp_index].type = 1;
                            waterNetwork.listOfNodes[tmp_index].reservoir = new Reservoir(waterNetwork.listOfNodes[tmp_index]);
                            waterNetwork.listOfReservoirs.Add(waterNetwork.listOfNodes[tmp_index].reservoir);
                        }
                        else //this is tank
                        {
                            int tmp_index = tankReservoirIndex[i] - 1; //indices in EPAnet start from 1
                            waterNetwork.listOfNodes[tmp_index].type = 2;
                            waterNetwork.listOfNodes[tmp_index].tank = new Tank(waterNetwork.listOfNodes[tmp_index]);

                            /* NOTE cross section area is converted from square feet to square meters  */
                            waterNetwork.listOfNodes[tmp_index].tank.crossSectionalArea = buffer_double * (double)0.0929;
                            waterNetwork.listOfTanks.Add(waterNetwork.listOfNodes[tmp_index].tank);
                        }
                    }

                    //Get elevation of each node 
                    for (int i = 0; i < waterNetwork.nNodes; i++)
                    {
                        waterNetwork.listOfNodes[i].elevation = binReader.ReadSingle();
                    }

                    // Get length of each link 
                    for (int i = 0; i < waterNetwork.nLinks; i++)
                    {
                        waterNetwork.listOfLinks[i].length = binReader.ReadSingle();
                    }

                    // Get diameter of each link 
                    for (int i = 0; i < waterNetwork.nLinks; i++)
                    {
                        waterNetwork.listOfLinks[i].diameter = binReader.ReadSingle();
                    }
                    /* The end of prolog section */





                    /* The epilog section */

                    //Position the file indicator to the beginning of epilog section 
                    binReader.BaseStream.Seek(-7 * 4, SeekOrigin.End);

                    /* Omit 
                       Average bulk reaction rate (mass/hr)
                       Average wall reaction rate (mass/hr)
                       Average tank reaction rate (mass/hr)
                       Average source inflow rate (mass/hr)
                 
                       The above data are not used in Simplifier
                    */
                    for (int i = 0; i < 4; i++)
                    {
                        buffer_double = binReader.ReadSingle();
                    }

                    // Get number of reporting periods
                    waterNetwork.nReportingPeriods = binReader.ReadInt32();

                    /* The end of epilog section*/




                    /* Memory allocation for arrays */

                    //Allocate memory for flow and head drop data in each link */
                    for (int i = 0; i < waterNetwork.nLinks; i++)
                    {
                        waterNetwork.listOfLinks[i].flow = new double[waterNetwork.nReportingPeriods];
                        waterNetwork.listOfLinks[i].headDrop = new double[waterNetwork.nReportingPeriods];
                    }

                    //Allocate memory for speed data of each pump */
                    for (int i = 0; i < waterNetwork.nPumps; i++)
                    {
                        waterNetwork.listOfPumps[i].speedPattern = new double[waterNetwork.nReportingPeriods];
                    }

                    // Allocate memory for valve setting data for each valve */
                    for (int i = 0; i < waterNetwork.nValves - waterNetwork.nPipesCV; i++)
                    {
                        waterNetwork.listOfValves[i].setting = new double[waterNetwork.nReportingPeriods];
                    }

                    /* Allocate memory for head data and demand data in each node */
                    for (int i = 0; i < waterNetwork.nNodes; i++)
                    {
                        waterNetwork.listOfNodes[i].head = new double[waterNetwork.nReportingPeriods];
                        waterNetwork.listOfNodes[i].demand = new double[waterNetwork.nReportingPeriods];
                    }

                    /* The dynamic results section */
                    /* Position the file indicator to the beginning of dynamic results section */
                    binReader.BaseStream.Seek(-((16 * waterNetwork.nNodes + 32 * waterNetwork.nLinks) * waterNetwork.nReportingPeriods + 28), SeekOrigin.End);

                    for (int i = 0; i < waterNetwork.nReportingPeriods; i++)
                    {

                        /* Get demands in all nodes */
                        for (int j = 0; j < waterNetwork.nNodes; j++)
                        {
                            waterNetwork.listOfNodes[j].demand[i] = binReader.ReadSingle();

                            /* NOTE for tanks and reservoirs these values are actually not demands, 
                            * but sum of inflows - sum of outflows, so these will be set to zero */
                            if (waterNetwork.listOfNodes[j].type != 0)
                            {
                                waterNetwork.listOfNodes[j].demand[i] = 0;
                            }

                            //Used to avoid NaN in demand matrix, but it is somehow decrease number of the pipes in simplified wds
                            /* else
                             {
                                 waterNetwork.listOfNodes[j].demand[i] = binReader.ReadSingle();
                             }
                
                             */


                        }


                        /* Get head in all nodes */
                        for (int j = 0; j < waterNetwork.nNodes; j++)
                        {
                            waterNetwork.listOfNodes[j].head[i] = binReader.ReadSingle();
                        }

                        //skip pressure and water quality at nodes
                        binReader.BaseStream.Seek(4 * (2 * waterNetwork.nNodes), SeekOrigin.Current);

                        /* Get flow and calcualte head drop in all links */
                        for (int j = 0; j < waterNetwork.nLinks; j++)
                        {

                            waterNetwork.listOfLinks[j].flow[i] = binReader.ReadSingle();
                            waterNetwork.listOfLinks[j].headDrop[i] = waterNetwork.listOfNodes.ElementAt(waterNetwork.listOfLinks[j].nodeFrom).head[i] - waterNetwork.listOfNodes.ElementAt(waterNetwork.listOfLinks[j].nodeTo).head[i];
                        }

                        //skip velocity, headloss, water quality, status for all links
                        binReader.BaseStream.Seek(4 * (4 * waterNetwork.nLinks), SeekOrigin.Current);




                        /* Get roughness for pipes */
                        for (int j = 0; j < waterNetwork.nPipes + waterNetwork.nPipesCV; j++)
                        {
                            waterNetwork.listOfLinks[j].setting = binReader.ReadSingle();
                        }

                        /* Get pump speed */
                        for (int j = 0; j < waterNetwork.nPumps; j++)
                        {
                            waterNetwork.listOfPumps[j].speedPattern[i] = binReader.ReadSingle();
                        }

                        /* Get valve setting */
                        for (int j = 0; j < waterNetwork.nValves - waterNetwork.nPipesCV; j++)
                        {
                            waterNetwork.listOfValves[j].setting[i] = binReader.ReadSingle();
                        }

                        //skip reaction rate and friction factor
                        binReader.BaseStream.Seek(4 * (2 * waterNetwork.nLinks), SeekOrigin.Current);
                    }

                    /* The end of dynamic results section */

                    //Close epanet bin file
                    binReader.Close();
                    s.Close();


                    //Count number of tanks and reservoirs
                    waterNetwork.nTanks = waterNetwork.listOfTanks.Count();
                    waterNetwork.nReservoirs = waterNetwork.listOfReservoirs.Count();



                    return (0);
                }

            }
            catch (Exception e)
            {
                if (is_window_app)
                    MessageBox.Show(e.Message);
                else
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("\nPress any key to terminate the program");
                    Console.ReadLine();
                    Environment.Exit(1);
                }
                return (-1);
            }
        }

        public static string Bytes2String(byte[] bytesArray)
        {
            char[] conversion = new char[bytesArray.Length];
            int index = 0;
            foreach (byte number in bytesArray)
            {
                conversion[index] = Convert.ToChar(number);
                index++;
            }
            string result = new string(conversion).Trim('\0');
            return result;
        }


    }

}
