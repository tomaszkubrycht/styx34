using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Media;
using NumericalMethods;
using System.Windows.Shapes;
using System.ComponentModel;
using UI = Styx.Properties.UserInterface;


namespace Styx
{
    public class AnalysisMeasurementAndSimulation
    {
    }

    public static class ArrayProcessing
    {
        /// <summary>Method from stackoverflow.com
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static int[] FindAllIndexof<T>(this IEnumerable<T> values, T val)
        {
            return values.Select((b, i) => object.Equals(b, val) ? i : -1).Where(i => i != -1).ToArray();
        }

    }

    public partial class Node : IEquatable<Node>///additional fields in class Node required for analysis
    {        
        public bool isLogger { set; get; } ///true if this node is actually a logger node, used for display/selection
        public bool isPathNode { get; set; } ///true if this node is actually element of path between loggers
        /// <summary>list of all neighbour nodes of this node, needed to contruct flow-tree 
        /// </summary>
        public List<NodeNeighbour> list_of_neighbours;
        /// <summary>List of lists of neighbours of different levels; listsHigherLevelNeighbours[0]=list of direct neighbours, listsHigherLevelNeighbours[1]=list of nodes with 1 intermediate node to this node, 
        /// listsHigherLevelNeighbours[2]=list of nodes with 2 intermediate nodes to this node etc. 2nd element in Tuple is absolute average head difference from this node to the corresponding higher level neighbour
        /// </summary>
        public List<List<Tuple<Node,double>>> listsHigherLevelNeighbours;
        /// <summary>Finds all links that have their origin (nodeFrom) at this node
        /// </summary>
        /// <param name="water_network">WaterNetwork object to search links in</param>
        /// <returns>List of links or empty list if not found</returns>
        public List<Link> GetOutgoingLinks(WaterNetwork water_network) 
        {
            int node_index = water_network.listOfNodes.FindIndex(tmp => tmp == this);
            if (node_index >= 0) //node found
                return (water_network.listOfLinks.FindAll(tmp => tmp.nodeFrom == node_index));
            else
                return (new List<Link>()); //return empty list
        }
        /// <summary>Finds all links that have their end (nodeTo) at this node
        /// </summary>
        /// <param name="water_network">WaterNetwork object to search links in</param>
        /// <returns>List of links or empty list if not found</returns>
        public List<Link> GetIncomingLinks(WaterNetwork water_network)
        {
            int node_index = water_network.listOfNodes.FindIndex(tmp => tmp == this);
            if (node_index >= 0) //node found
                return (water_network.listOfLinks.FindAll(tmp => tmp.nodeTo == node_index));
            else
                return (new List<Link>()); //return empty list
        }
        /// <summary>Checks if this node is the same as other node 
        /// </summary>
        /// <param name="other_node">another node</param>
        /// <returns></returns>
        public bool Equals(Node other_node)
        {
            if (Object.ReferenceEquals(other_node, null))
                return(false);
            if (Object.ReferenceEquals(this, other_node))
                return(true);
            return(name.Equals(other_node.name));
        }

        /// <summary>Checks if node is direct neighbour of any node from rectangleList; to obtain nodes from rectangleList use (Node)rectangleList[i].Tag
        /// </summary>
        /// <param name="node"></param>
        /// <param name="rectangleList"></param>
        /// <returns></returns>
        public static bool IsNeighbour(Node node, List<Rectangle> rectangleList)
        {
            List<Node> neigh_nodes_list = node.list_of_neighbours.ConvertAll<Node>(new Converter<NodeNeighbour, Node>(NodeNeighbour.GetNode));
            foreach (Node neigh_node in neigh_nodes_list)
            {
                if (rectangleList.Any(tmp => ((Node)tmp.Tag) == neigh_node)) //if this neighbouring node belongs to rectangleList
                    return (true);
            }
            return (false);
        }

        /// <summary>Checks if node is direct neighbour of any node from loggerList
        /// </summary>
        /// <param name="node"></param>
        /// <param name="rectangleList"></param>
        /// <returns></returns>
        public static bool IsNeighbour(Node node, List<Logger> loggerList)
        {
            List<Node> neigh_nodes_list = node.list_of_neighbours.ConvertAll<Node>(new Converter<NodeNeighbour, Node>(NodeNeighbour.GetNode));
            foreach (Node neigh_node in neigh_nodes_list)
            {
                if (loggerList.Any(tmp => tmp.node == neigh_node)) //if this neighbouring node belongs to loggerList
                    return (true);
            }
            return (false);
        }
    }

    public partial class Link
    {
        public string label; 
    }

    public partial class WaterNetwork ///additional fields in class WaterNetwork required for analysis
    {
        /// <summary>Matrix to store results of FloydWarshall algorithm: shortest distance between all possible pairs of nodes in the network; distance is in terms of number of intermediate nodes (1 = direct connection, 2 = one intermedia node etc) 
        /// </summary>
        public double[][] distanceMatrix;
        
        /// <summary>For each node in this water network generate list of neighbours and their distances (inverse of flow)
        /// </summary>
        /// <param name="zero_flow_tolerance">value below which it is assumed that the flow is zero</param>
        /// <returns>0 if successful</returns>
        /// <returns>-1 if link flow not allocated</returns>
        public int GenerateNeighboursLists(double zero_flow_tolerance)
        {
            //should flow and head difference be taken only from the period when the EFAVOR experiment was carried out; PS 26/07/2013: no, because this method is called before efavor experiment data has been loaded
            foreach (Node node in listOfNodes) //generate neighbours for every node
            {
                //if (node.list_of_neighbours == null) //if neighbours list is empty, allocate it
                node.list_of_neighbours = new List<NodeNeighbour>(); //always allocate new neighbours list
                List<Link> outgoing_links = node.GetOutgoingLinks(this); 
                foreach (Link link in outgoing_links)
                {
                    if (link.flow == null) 
                    {
                        MessageBox.Show("Field flow does not exist for link " + link.name);
                        return (-1);
                    }
                    Node node_to = listOfNodes[link.nodeTo];
                    NodeNeighbour new_node_neighbour = new NodeNeighbour(node_to);
                    double avg_head_diff = node.head.Average() - node_to.head.Average();
                    new_node_neighbour.avg_head_diff = avg_head_diff;
                    double average_flow = link.flow.Average();                                      
                    if (average_flow > zero_flow_tolerance) //average flow in this pipe is in direction from -> to
                    {
                        new_node_neighbour.distance = 1 / average_flow;
                    } //don't need to assign infinity in else, distance is infinity by default through NodeNeighbour constructor
                    node.list_of_neighbours.Add(new_node_neighbour);
                }
                List<Link> incoming_links = node.GetIncomingLinks(this);
                foreach (Link link in incoming_links)
                {
                    if (link.flow == null)
                    {
                        MessageBox.Show("Field flow does not exist for link " + link.name);
                        return (-1);
                    }
                    Node node_from = listOfNodes[link.nodeFrom];
                    NodeNeighbour new_node_neighbour = new NodeNeighbour(node_from);
                    double avg_head_diff = node.head.Average() - node_from.head.Average();
                    new_node_neighbour.avg_head_diff = avg_head_diff;
                    double average_flow = link.flow.Average();
                    if (average_flow < -zero_flow_tolerance) //average flow in this pipe is in direction to -> from
                    {
                        new_node_neighbour.distance = 1 / Math.Abs(average_flow);
                    } //don't need to assign infinity in else, distance is infinity by default through NodeNeighbour constructor
                    node.list_of_neighbours.Add(new_node_neighbour);
                }
            }
            return 0;
        }

        /// <summary>For each node in this water network generate listsHigherLevelNeighbours, i.e. list of lists of neighbours of different levels
        /// </summary>
        /// <param name="maxLevel">Maximum neighbourhood level i.e. max number of nodes between; maxLevel >= 0</param>
        /// <returns>0 if successful</returns>
        public int GenerateHigherLevelNeighbours(int maxLevel, BackgroundWorker bgWorker)
        {            
            try
            {
                if (maxLevel < 0)
                    throw new Exception("Max neighbourhood level parameter must be >= zero");
                //DateTime startTime = DateTime.Now;                 
                FloydWarshall(out distanceMatrix, bgWorker);
                //TimeSpan duration = DateTime.Now - startTime;
                //MessageBox.Show("completed in " + duration.Minutes + "min " + duration.Seconds + "." + duration.Milliseconds + "s");
   
                for (int i = 0; i < listOfNodes.Count; i++)
                {
                    Node currentNode = listOfNodes[i];
                    currentNode.listsHigherLevelNeighbours = new List<List<Tuple<Node,double>>>(maxLevel + 1);
                    for (int currentLevel = 0; currentLevel < maxLevel + 1; currentLevel++) //from 0 and ending at +1 since in listsHigherLevelNeighbours level 0 means direct neighbour, and in distanceMatrix distance 1 means direct neighbour
                    {
                        int[] indices = distanceMatrix[i].Select(tmp => (int)tmp).FindAllIndexof(currentLevel + 1); //convert all doubles in the corresponding row of distanceMatrix to ints and find indices of elements equal to current neighbourhood level
                        if ((indices == null) || (indices.Length == 0)) //if no neighbours at this level, there will be no neighbours at higher levels
                            break;
                        List<Tuple<Node, double>> thisNodeNeighbours = new List<Tuple<Node, double>>();
                        foreach (int index in indices) //get all nodes whose indices were obtained above
                        {
                            double absAvgHeadDiff = Math.Abs(currentNode.head.Average() - listOfNodes[index].head.Average());
                            Tuple<Node, double> currentTuple = new Tuple<Node, double>(listOfNodes[index], absAvgHeadDiff);
                            thisNodeNeighbours.Add(currentTuple);
                        } //implementation below is slower than simple foreach (probably could be implemented better using LINQ)
                        //thisNodeNeighbours.AddRange(listOfNodes.FindAll(tmpNode => indices.Any(tmpIndex => listOfNodes.IndexOf(tmpNode) == tmpIndex))); //get all nodes whose indices were obtained above
                        currentNode.listsHigherLevelNeighbours.Add(thisNodeNeighbours); //add these nodes as current level neighbours of current node
                    }
                    //for (int currentLevel = 0; currentLevel < maxLevel + 1; currentLevel++) //from 0 and ending at +1 since in listsHigherLevelNeighbours level 0 means direct neighbour, and in distanceMatrix distance 1 means direct neighbour
                    //{
                    //    string chuj = "";
                    //    foreach (Node tmpnode in currentNode.listsHigherLevelNeighbours[currentLevel])
                    //    {
                    //        chuj += tmpnode.name + ", ";
                    //    }
                    //    MessageBox.Show(currentNode.name + ": (lvl " + currentLevel.ToString() + ") " + chuj);
                    //}
                }                

            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in GenerateHigherLevelNeighbours: " + ex.Message);
                return (-1);
            }
            return (0);
        }

        /// <summary>Implementation of FloydWarshall algorithm to calculate shortest distance between all possible pairs of nodes in the network; distance is in terms of number of intermediate nodes (1 = direct connection, 2 = one intermedia node etc) 
        /// </summary>
        /// <param name="distMatrix"></param>
        /// <returns>0 if successful</returns>
        public int FloydWarshall(out double[][] distMatrix, BackgroundWorker bgWorker)
        {
            int nNodes = listOfNodes.Count;
            distMatrix = new double[nNodes][];
            try
            {                                
                for (int i = 0; i < nNodes; i++)
                {
                    distMatrix[i] = new double[nNodes];
                }
                // Initialize distance matrix
                for (int i = 0; i < nNodes; i++)
                {
                    Node node1 = listOfNodes[i];
                    for (int j = 0; j < nNodes; j++)
                    {
                        if (i == j)
                            continue;
                        //Node node2 = listOfNodes[j]; //to save 1 operation...
                        NodeNeighbour nodeNeighbour = node1.list_of_neighbours.Find(tmp => tmp.node == listOfNodes[j]);
                        if (nodeNeighbour != null) //node2 is a neighbour of node1;
                        {
                            //D[i][j] = Math.Abs(nodeNeighbour.avg_head_diff); //distance is abs average head difference?
                            distMatrix[i][j] = 1;
                            distMatrix[j][i] = 1;
                        }
                        else
                        {
                            distMatrix[i][j] = double.PositiveInfinity;
                            distMatrix[j][i] = double.PositiveInfinity;
                        }
                    }
                }

                int progress;
                // Floyd-Warshall alghoritm
                for (int counter = 0; counter < 2; counter++) //we need two iterations
                {
                    //DateTime startTime = DateTime.Now;//time measure
                    for (int v = 0; v < nNodes; v++)
                    {
                        if (bgWorker.IsBusy)
                        {
                            progress = 100 * (v + nNodes * counter) / (nNodes * 2);
                            bgWorker.ReportProgress(progress);
                        }
                        for (int u = 0; u < nNodes; u++)
                        {
                            if (u == v)
                                continue;
                            for (int k = 0; k < nNodes; k++)
                            {
                                if (k == u || k == v)
                                    continue;
                                if (distMatrix[v][u] > distMatrix[v][k] + distMatrix[k][u])
                                {
                                    distMatrix[v][u] = distMatrix[v][k] + distMatrix[k][u];
                                }
                            }
                        }                        
                    }
                    //TimeSpan duration = DateTime.Now - startTime;
                    //duration = DateTime.Now - startTime;
                    //MessageBox.Show("completed in " + duration.Minutes + "min " + duration.Seconds + "." + duration.Milliseconds + "s");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in FloydWarshall: " + ex.Message);
                return (-1);
            }             
            return (0);
        }

        /// <summary>Generates list of nodes along shortest path/paths between 2 nodes. GenerateHigherLevelNeighbours must be called before calling this function. If there are paraller paths of the same length, nodes from both paths are returned with the order of ascending distance from nodeFrom
        /// </summary>
        /// <param name="nodeFrom"></param>
        /// <param name="nodeTo"></param>        
        /// <returns></returns>
        public List<Node> NodesBetween(Node nodeFrom, Node nodeTo)
        {
            List<Node> nodesList = new List<Node>();
            try
            {
                if (nodeFrom.listsHigherLevelNeighbours == null || nodeTo.listsHigherLevelNeighbours == null)
                    throw new Exception("GenerateHigherLevelNeighbours must be called before using this function");
                int indexFrom = listOfNodes.IndexOf(nodeFrom);
                int indexTo = listOfNodes.IndexOf(nodeTo);
                int distance = (int)distanceMatrix[indexFrom][indexTo];
                if (distance - 1 > nodeFrom.listsHigherLevelNeighbours.Count) //-1 since in distanceMatrix 1 means direct connection and listsHigherLevelNeighbours[1] means neighbours with 1 intermediate node
                    throw new Exception("Nodes are too far from each other"); //such high level neighbours were not calculated previously
                for (int i = 0; i < distance - 1; i++) //if distance is 1 (direct neighbours) or 0 (the same node) then this loop will simply not run and the returned list will be empty (count=0)
                {
                    List<Node> fromNeighbours = nodeFrom.listsHigherLevelNeighbours[i].Select(tmp => tmp.Item1).ToList();
                    List<Node> toNeighbours = nodeTo.listsHigherLevelNeighbours[distance - 2 - i].Select(tmp => tmp.Item1).ToList();
                    nodesList.AddRange(fromNeighbours.Intersect(toNeighbours));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in NodesBetween for nodes: " + nodeFrom.name + " and " + nodeTo.name + ". Exception: " + ex.Message);
            }
            return (nodesList);

        }

    }

    /// <summary>Required to store information about neighbours of each node
    /// </summary>
    public class NodeNeighbour
    {
        public Node node;
        public double distance; ///"distance" of a node to this neighbour, typically "distance"=inverse of flow between the node and the neighbour; default is infinity (if flow to this neighbour is not positive)
        public double avg_head_diff; ///average head difference between a node and this neighbour
        public NodeNeighbour(Node node, double distance)
        {
            this.node = node;
            this.distance = distance;
        }
        public NodeNeighbour(Node node)
        {
            this.node = node;
            this.distance = double.PositiveInfinity;
        }
        /// <summary>Retrieve node associated with a neighbour; used for converter of ConvertAll
        /// </summary>
        /// <returns></returns>
        public static Node GetNode(NodeNeighbour node_neighbour)
        {
            return (node_neighbour.node);
        }
                
    }

    /// <summary> Describes complete flow pattern (flow tree) of the network based on EPANET simulation. Roots of the tree are DMA inlets
    /// </summary>
    public class FlowTree
    {
        /// <summary>single junction of a complete flow tree 
        /// </summary>
        public class FlowTreeJunction
        {
            public Node this_node; ///node at this junction
            public List<Node> list_of_previous_nodes = new List<Node>(); ///neighbour nodes FROM which there is positive flow INCOMING TO this flow tree juntion
            public List<Node> list_of_next_nodes = new List<Node>(); ///neighbour nodes TO which there is positive flow OUTGOING FROM this flow tree juntion
            public FlowTreeJunction(Node node)
            {
                this_node = node;            
            }
        }
        public List<FlowTreeJunction> list_of_junctions = new List<FlowTreeJunction>(); ///list describing complete flow tree
        public WaterNetwork water_network; ///object to retrieve data on neighbours of nodes
        public List<Node> list_of_inlet_nodes; ///nodes that are roots of the flowtree
        //public double[][] distanceMatrixPositiveFlow; ///Matrix with shortest distance between all possible nodes in water_network, but if there is no positive flow from n1 to n2 then distance(n1->n2) is infinity
                                           
        /// <summary>Constructor 
        /// </summary>
        /// <param name="water_network">object to retrieve "neighbours of nodes" data from</param>
        /// <param name="list_of_inlet_nodes">Nodes that will be roots of the flowtree</param>
        public FlowTree(WaterNetwork water_network, List<Node> list_of_inlet_nodes)
        {            
            this.water_network = water_network;
            this.list_of_inlet_nodes = list_of_inlet_nodes;            
        }
        /// <summary>Retrieves FlowTreeJunction corresponding to Node
        /// </summary>
        /// <param name="node"></param>
        /// <returns>FlowTreeJunction objects for which this_node==node</returns>
        public FlowTreeJunction GetFlowTreeJunction(Node node)
        {
            return (list_of_junctions.Find(tmp => tmp.this_node.name == node.name));
        }
        /// <summary>Generates complete flow tree using this.list_of_inlet_nodes as tree roots and subsequently calls FloydWarshallWithFlowDirection to calculate shortest distance between all possible pairs of nodes in the network taking into account flow direction
        /// </summary>
        /// <param name="calculateDistanceMatrix">determines if FloydWarshallWithFlowDirection should be called to calculate distanceMatrixPositiveFlow for this FlowTree; default is true, use false to if distanceMatrixPositiveFlow is not needed and you want to save calculation time</param>
        /// <returns>0 if successful</returns>
        /// <returns>error code returned by AddTreeBranch</returns>
        public int GenerateFlowTree()
        {
            foreach (Node inlet_node in list_of_inlet_nodes)
            {
                FlowTreeJunction new_junction = new FlowTreeJunction(inlet_node);
                list_of_junctions.Add(new_junction); //add this inlet to the list_of_junctions
                int ret_val = AddTreeBranch(inlet_node); //start building flow tree at this inlet
                if (ret_val < 0)
                    return (ret_val);
            }
            return (0);
        }
        /// <summary>Builds flow tree starting at calling_node, calles itself recursively and returns when calling_node is terminal node (node at the end of branch) or when calling_node is already in the tree (has already been processed)
        /// </summary>
        /// <param name="calling_node">starting node if called from outside</param>
        /// <returns>0 if successful</returns>
        /// <returns>-1 if GetFlowTreeJunction failed for calling_node</returns>
        /// <returns>-2 if list of neighbours not allocated for calling_node</returns>
        public int AddTreeBranch(Node calling_node)
        {            
            //FlowTreeJunction calling_node_tree_junction = list_of_junctions.Find(tmp => tmp.this_node.name == calling_node.name);
            if (calling_node.list_of_neighbours == null)
            {
                MessageBox.Show("Eror: list of neighbours not allocated for node: " + calling_node.name);
                return (-2);
            }
            FlowTreeJunction calling_node_tree_junction = GetFlowTreeJunction(calling_node);
            if (calling_node_tree_junction == null)
            {
                MessageBox.Show("Eror: GetFlowTreeJunction failed for node: " + calling_node.name);
                return (-1);
            }
            else if (calling_node_tree_junction.list_of_next_nodes.Count > 0) //this node is already in the list_of_junctions and it's list_of_next_nodes is not empty, so this node has already been processed
                return (0);
            List<NodeNeighbour> neighbour_list = calling_node.list_of_neighbours.FindAll(tmp => tmp.distance != double.PositiveInfinity); //list of all neighbours to which "distance" is not infinity (i.e. there is positive flow from current_node to these neighbours)
            foreach (NodeNeighbour node_neighbour in neighbour_list)
            {
                //FlowTreeJunction neighbour_tree_junction = list_of_junctions.Find(tmp => tmp.this_node.name == node_neighbour.node.name);
                FlowTreeJunction neighbour_tree_junction = GetFlowTreeJunction(node_neighbour.node);
                if (neighbour_tree_junction == null) //check if FlowTreeJunction object was already created for this node_neighbour; if not, create it
                {
                    neighbour_tree_junction = new FlowTreeJunction(node_neighbour.node);
                    list_of_junctions.Add(neighbour_tree_junction);
                }
                calling_node_tree_junction.list_of_next_nodes.Add(node_neighbour.node);
                neighbour_tree_junction.list_of_previous_nodes.Add(calling_node);                
                int ret_val = AddTreeBranch(node_neighbour.node); ;
                if (ret_val < 0)
                    return (ret_val);
            }
            return (0);
        }
        ///// <summary>Implementation of FloydWarshall algorithm to calculate shortest distance between all possible pairs of nodes in the network taking into account flow direction; distance is in terms of number of intermediate nodes (1 = direct connection, 2 = one intermediate node etc), but if there is no positive flow from n1 to n2 then distance(n1->n2) is infinity
        ///// </summary>
        ///// <param name="bgWorker">Background worker to which progress should be reported</param>
        ///// <returns>0 if successful</returns>
        //int FloydWarshallWithFlowDirection()
        //{
        //    try
        //    {
        //        if (list_of_junctions.Count == 0)
        //            throw new Exception("Can't call FloydWarshallWithFlowDirection before calling GenerateFlowTree");
        //        int nNodes = water_network.listOfNodes.Count;
        //        distanceMatrixPositiveFlow = new double[nNodes][];
        //        for (int i = 0; i < nNodes; i++)
        //        {
        //            distanceMatrixPositiveFlow[i] = new double[nNodes];
        //        }
        //        // Initialize distance matrix
        //        for (int i = 0; i < nNodes; i++)
        //        {
        //            FlowTreeJunction node1Junction = GetFlowTreeJunction(water_network.listOfNodes[i]);
        //            for (int j = 0; j < nNodes; j++)
        //            {
        //                if (i == j)
        //                    continue;
        //                Node node2 = water_network.listOfNodes[j];
        //                if ((node1Junction != null) && (node1Junction.list_of_next_nodes.Exists(tmp => tmp == node2))) //there is positive flow from node1 to node2
        //                {
        //                    distanceMatrixPositiveFlow[i][j] = 1;
        //                    distanceMatrixPositiveFlow[j][i] = double.PositiveInfinity; //if there's positive flow n1->n2 then there's no positive flow n2->n1
        //                }
        //                else
        //                {
        //                    distanceMatrixPositiveFlow[i][j] = double.PositiveInfinity;
        //                }
        //            }
        //        }

        //        // Floyd-Warshall algorithm
        //        for (int counter = 0; counter < 2; counter++) //we need two iterations
        //        {
        //            for (int v = 0; v < nNodes; v++)
        //            {                        
        //                for (int u = 0; u < nNodes; u++)
        //                {
        //                    if (u == v)
        //                        continue;
        //                    for (int k = 0; k < nNodes; k++)
        //                    {
        //                        if (k == u || k == v)
        //                            continue;
        //                        if (distanceMatrixPositiveFlow[v][u] > distanceMatrixPositiveFlow[v][k] + distanceMatrixPositiveFlow[k][u])
        //                        {
        //                            distanceMatrixPositiveFlow[v][u] = distanceMatrixPositiveFlow[v][k] + distanceMatrixPositiveFlow[k][u];
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Exception in FloydWarshallWithFlowDirection: " + ex.Message);
        //        return (-1);
        //    } 
        //    return (0);
        //}

        ///// <summary>Use previously calculated distanceMatrixPositiveFlow to check if there is positive flow from nodeFrom to nodeTo
        ///// </summary>
        ///// <param name="nodeFrom"></param>
        ///// <param name="nodeTo"></param>
        ///// <param name="maxDistance">maximum number of nodes between from and two</param>
        ///// <returns></returns>
        //public bool IsPositiveFlowFromTo(Node nodeFrom, Node nodeTo, int maxDistance)
        //{
        //    if (distanceMatrixPositiveFlow == null)
        //        throw new Exception("distanceMatrixPositiveFlow has not been allocated");
        //    int indexFrom = water_network.listOfNodes.FindIndex(tmp => tmp == nodeFrom);
        //    int indexTo = water_network.listOfNodes.FindIndex(tmp => tmp == nodeTo);
        //    if (indexFrom < 0 || indexTo < 0)
        //        throw new Exception("Can't find declared nodes in the water network object during IsPositiveFlowFromTo");
        //    if (distanceMatrixPositiveFlow[indexFrom][indexTo] > maxDistance + 1) //distance 1 in distanceMatrixPositiveFlow means direct connection, but maxDistance==0 means we want direct connection, so we need maxDistance + 1 here
        //        return (false);
        //    else
        //        return (true);
        //}

        /// <summary>Use previously calculated flow tree to check if there is positive flow from nodeFrom to nodeTo
        /// </summary>
        /// <param name="nodeFrom"></param>
        /// <param name="nodeTo"></param>
        /// <param name="maxDistance">maximum number of nodes between from and to</param>
        /// <returns></returns>
        public bool IsPositiveFlowFromTo(Node nodeFrom, Node nodeTo, int maxDistance)
        {
            if (list_of_junctions.Count == 0)
                throw new Exception("Flowtree has not been generated");
            FlowTreeJunction fromJunction = GetFlowTreeJunction(nodeFrom);
            if (fromJunction.list_of_next_nodes.Exists(tmp => tmp == nodeTo))
                return (true);
            if (maxDistance > 0)
            {
                bool result = false;
                foreach (Node node in fromJunction.list_of_next_nodes)
                {
                    result = IsPositiveFlowFromTo(node, nodeTo, maxDistance - 1);
                    if (result)
                        return (true);
                }
            }
            return (false);
        }

        /// <summary>Get all nodes in all branches of this flow tree. Useful e.g. to determine which nodes in the water network are not a part of the tree
        /// </summary>
        /// <returns></returns>
        public List<Node> GetAllNodes()
        {
            List<Node> nodeList = new List<Node>(); //initially create empty list
            if (list_of_junctions.Count > 0)
                nodeList = list_of_junctions.Select(tmp => tmp.this_node).ToList();
            return (nodeList);
        }
    }

    /// <summary> all info related to loggers and their connections 
    /// </summary>
    public class LoggerConnections 
    {
        /// <summary>relations between 2 loggers: flow paths, delta2h, ...
        /// </summary>
        public class RelationsBetweenLoggers 
        {
            public List<List<Node>> list_of_paths_between_loggers = new List<List<Node>>();
        }

        double head_diff_tolerance; //if the flow path constructing algorithm arrives at logger's neighbour, it is assumed that it has arrinved at this logger only if absolute head difference between the neighbour and the logger is smaller than this value
        public List<Logger> list_of_loggers; 
        public RelationsBetweenLoggers[,] relations_between_loggers; ///each element of this matrix describes relations (flow paths, delta2h, ...) between corresponding loggers
        public bool[,] logger_connection_matrix; ///element [i,j] is true if there is connection between logger i and j
        public FlowTree flow_tree; ///flow tree obtained from EPANET simulation
        public WaterNetwork logger_water_network; ///used for visualisation; water network where links are logger connections (i.e. they are not physical pipes); link labels correspond to delta2h from EFavor (initially obtained for highest and lowest PRV setting); node names are as logger ids                                   
        
        Constants constants; ///to get pre-defined graphical attributes
        
        //MainWindow mw = new MainWindow();//to get ToBrush() class

        /// <summary>
        /// Converts Sytem.Drawing.Color type to Brush type
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private Brush ToBrush(System.Drawing.Color color)
        {
            return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        /// <summary>Constructor
        /// </summary>
        /// <param name="list_of_loggers">list of all loggers used in EFAVOR experiment</param>
        /// <param name="flow_tree">flow tree obtained from EPANET simulation</param>
        public LoggerConnections(List<Logger> list_of_loggers, FlowTree flow_tree)
        {
            this.list_of_loggers = list_of_loggers; 
            this.flow_tree = flow_tree;
            int no_of_loggers = list_of_loggers.Count;
            logger_connection_matrix = new bool[no_of_loggers, no_of_loggers];
            relations_between_loggers = new RelationsBetweenLoggers[no_of_loggers, no_of_loggers];            
        }
        
        /// <summary>Returns delta2h (change in headloss) between two nodes, taking headloss for largest pressure and headloss for lowest pressure, but only from measurement points corresponding to throttling of inlet PRVs from the specified inlet set
        /// </summary>
        /// <param name="logger_from"></param>
        /// <param name="logger_to"></param>
        /// <param name="efavor_test">efavor test object needed to retrieve info which measurement points correspond to which inlet set (PRVs belonging to the same inlet set were throttled at the same time) </param>
        /// <param name="inlet_set_id">inlet set id specifying which measurement points should be taken to calculate delta2h</param>
        /// <param name="delta2h_max">output: calculated delta2h (change in headloss)</param>
        /// <param name="delta_h">headloss for highest PRV setpoint</param>
        /// <returns>0 if successful</returns>
        /// <returns>-1 if specified inlet_set_id is invalid</returns>
        /// <returns>-20 if other exception</returns>
        public int GetDelta2hMax(Logger logger_from, Logger logger_to, EFavorTest efavor_test, string inlet_set_id, out double delta2h_max, out double delta_h)
        {
            delta2h_max = 0;
            delta_h = 0;
            try
            {
                if (!efavor_test.list_of_inlet_set_ids.Exists(tmp => tmp == inlet_set_id)) //check if specified inlet_set_id is valid
                {
                    MessageBox.Show("Invalid inlet set id: " + inlet_set_id);
                    return (-1);
                }
                int first_index = efavor_test.measurement_inlet_set_order.FindIndex(tmp => tmp == inlet_set_id); //beginning of measurement data segment to retrieve data from
                int last_index = efavor_test.measurement_inlet_set_order.FindLastIndex(tmp => tmp == inlet_set_id); //end of measurement data segment to retrieve data from
                double delta_p1 = logger_from.measured_pressure.GetRange(first_index, last_index - first_index + 1).Max() - logger_to.measured_pressure.GetRange(first_index, last_index - first_index + 1).Max();
                double delta_p2 = logger_from.measured_pressure.GetRange(first_index, last_index - first_index + 1).Min() - logger_to.measured_pressure.GetRange(first_index, last_index - first_index + 1).Min();
                double delta_elev = logger_from.elevation - logger_to.elevation;

                delta2h_max = (delta_p1 - delta_p2);
                delta_h = delta_p1 + delta_elev;
                return (0);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in GetDelta2hMax: " + ex.Message);
                return (-20);
            }
        }

        /// <summary>Returns delta2h (change in headloss) between two nodes RESULTING FROM SIMULATION, taking headloss for largest pressure and headloss for lowest pressure, but only from measurement points corresponding to throttling of inlet PRVs from the specified inlet set
        /// </summary>
        /// <param name="logger_from"></param>
        /// <param name="logger_to"></param>
        /// <param name="efavor_test">efavor test object needed to retrieve info which measurement points correspond to which inlet set (PRVs belonging to the same inlet set were throttled at the same time) </param>
        /// <param name="inlet_set_id">inlet set id specifying which measurement points should be taken to calculate delta2h</param>
        /// <param name="delta2h_max">output: calculated delta2h (change in headloss)</param>
        /// <param name="delta_h">headloss for highest PRV setpoint</param>
        /// <returns>0 if successful</returns>
        /// <returns>-1 if specified inlet_set_id is invalid</returns>
        /// <returns>-2 if hydraulic results do not exist</returns>
        /// <returns>-20 if other exception</returns>
        public int GetSimulatedDelta2hMax(Logger logger_from, Logger logger_to, EFavorTest efavor_test, string inlet_set_id, out double delta2h_max, out double delta_h)
        {
            delta2h_max = 0;
            delta_h = 0;
            try
            {
                if (!efavor_test.list_of_inlet_set_ids.Exists(tmp => tmp == inlet_set_id)) //check if specified inlet_set_id is valid
                {
                    MessageBox.Show("Invalid inlet set id: " + inlet_set_id);
                    return (-1);
                }
                if (logger_from.node.head == null) //check if hydraulic results exist
                {
                    MessageBox.Show("Error: Hydraulic results do not exist, run simulation first");
                    return (-2);
                }
                int first_index = efavor_test.measurement_inlet_set_order.FindIndex(tmp => tmp == inlet_set_id); //beginning of measurement data segment to retrieve data from
                int last_index = efavor_test.measurement_inlet_set_order.FindLastIndex(tmp => tmp == inlet_set_id); //end of measurement data segment to retrieve data from
                double delta_h1 = logger_from.node.head.ToList().GetRange(first_index, last_index - first_index + 1).Max() - logger_to.node.head.ToList().GetRange(first_index, last_index - first_index + 1).Max();
                double delta_h2 = logger_from.node.head.ToList().GetRange(first_index, last_index - first_index + 1).Min() - logger_to.node.head.ToList().GetRange(first_index, last_index - first_index + 1).Min();
                
                delta2h_max = (delta_h1 - delta_h2);
                delta_h = delta_h1;
                return (0);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in GetSimulatedDelta2hMax: " + ex.Message);
                return (-20);
            }
        }
       
        /// <summary>use flow patterns (flow_tree obtained from EPANET simulation) to calculate list_of_paths_matrix and logger_connection_matrix; equivalent to scilab's build_tree.sce
        /// </summary>
        /// <param name="head_diff_tolerance">if the flow path constructing algorithm arrives at logger's neighbour, it is assumed that it has arrived at this logger only if absolute head difference between the neighbour and the logger is smaller than this value</param>
        /// <returns>0 if successful</returns>
        /// <returns>-1 if unallocated node to logger</returns>
        /// <returns>-2 if can't find start of a path in list_of_loggers</returns>
        public int CalculatePathsBetweenLoggers(double head_diff_tolerance) 
        {
            //test...
            //Node tmpfrom = flow_tree.water_network.listOfNodes.Find(tmp => tmp.name == "N56");
            //Node tmpto = flow_tree.water_network.listOfNodes.Find(tmp => tmp.name == "N51");
            //MessageBox.Show(flow_tree.IsPositiveFlowFromTo(tmpfrom, tmpto, 4).ToString());
            //List<Node> chuj = flow_tree.water_network.NodesBetween(tmpfrom, tmpto);

            this.head_diff_tolerance = head_diff_tolerance;
            foreach (Logger logger in list_of_loggers)
            {
                if (logger.is_inlet) //skip inlets - can't go backwards (following flow) from inlets
                    continue;
                if (logger.node == null)
                {
                    MessageBox.Show("Error: Unallocated node to logger id " + logger.logger_id);
                    return (-1);
                }
                List<List<Node>> list_of_paths = new List<List<Node>>(); //list of all paths between 2 loggers
                int current_path_index = 0;
                int ret_val = FindPathsToLogger(list_of_paths, logger.node, current_path_index, AdvancedOptions.loggerNeighbourhoodLevel);
                foreach(List<Node> single_path in list_of_paths)
                {
                    int start_logger_index = list_of_loggers.FindIndex(tmp => tmp.node.name == single_path[0].name);
                    int end_logger_index = list_of_loggers.FindIndex(tmp => tmp == logger);
                    if ((start_logger_index < 0) || (end_logger_index < 0))
                    {
                        MessageBox.Show("Error: something wrong while retrieving paths ending at logger id " + logger.logger_id);
                        return (-2);
                    }
                    //19/07/2013: add connection L1->L2 only if L2->L1 doesn't already exist (FindPathsToLogger now improved to prevent such situation where both L1->L2 and L2->L1 seem to exist, this condition is just in case)
                    if (logger_connection_matrix[end_logger_index, start_logger_index] == false)
                    {
                        logger_connection_matrix[start_logger_index, end_logger_index] = true;
                        if (relations_between_loggers[start_logger_index, end_logger_index] == null)
                            relations_between_loggers[start_logger_index, end_logger_index] = new RelationsBetweenLoggers();
                        relations_between_loggers[start_logger_index, end_logger_index].list_of_paths_between_loggers.Add(single_path);
                    }
                    //temporary solution: if connection L2->L1 already exist, then don't add connection L1->L2 (not good enough!)
                    //if (logger_connection_matrix[end_logger_index, start_logger_index] == false)
                    //{
                    //    logger_connection_matrix[start_logger_index, end_logger_index] = true;
                    //    if (relations_between_loggers[start_logger_index, end_logger_index] == null)
                    //        relations_between_loggers[start_logger_index, end_logger_index] = new RelationsBetweenLoggers();
                    //    relations_between_loggers[start_logger_index, end_logger_index].list_of_paths_between_loggers.Add(single_path);
                    //}
                    //VERSION2:  if algorithm wants to create connection L1->L2 but L2->L1 already exists, then non of these 2 conenctions should exist!
                    //if (logger_connection_matrix[end_logger_index, start_logger_index] == true)
                    //{ //delete existing connection L2->L1 and do not add new connection L1->L2
                    //    logger_connection_matrix[end_logger_index, start_logger_index] = false;
                    //    relations_between_loggers[end_logger_index, start_logger_index] = null;                        
                    //}
                    //else //create new connection
                    //{
                    //    logger_connection_matrix[start_logger_index, end_logger_index] = true;
                    //    if (relations_between_loggers[start_logger_index, end_logger_index] == null)
                    //        relations_between_loggers[start_logger_index, end_logger_index] = new RelationsBetweenLoggers();
                    //    relations_between_loggers[start_logger_index, end_logger_index].list_of_paths_between_loggers.Add(single_path);
                    //}
                }
                //...TODO!!!: test comparing to Scilab, test for multi-inlet DMA
            }            
            return (0);
        }

        /// <summary>Goes backward (agains the flow, using flow_tree) from the current node until logger node (or its neighbour up to defined neighbourhood level and with sufficiently small head difference) is encountered and builds path (list of loggers) which is 
        /// part of list_of_paths; for each branching encountered in the flow_tree the current segment of the path is copied to a new path, and this method is run recursively for the new path
        /// </summary>
        /// <param name="list_of_paths">list of all paths between 2 loggers</param>
        /// <param name="current_node"></param>
        /// <param name="current_path_index">to identify which path stored in list_of_paths should be constructed by this method call, since it is called recursively</param>
        /// <param name="neighbourhoodLevel">0=stop only when we arrive at a logger itself, 1=when we arrive at a direct neighbour, 2=when we arrive at 2nd level neighbour (i.e. separated by 1 node) etc.</param>
        /// <returns>0 if successful</returns>
        /// <returns>-1 if exception while inserting node into list_of_paths</returns>
        int FindPathsToLogger(List<List<Node>> list_of_paths, Node current_node, int current_path_index, int neighbourhoodLevel)
        {
            int ret_val;
            if (list_of_paths.Count - 1 < current_path_index) //this path has not been started yet, so it needs to be allocated
                list_of_paths.Add(new List<Node>());
            try
            {
                list_of_paths[current_path_index].Insert(0, current_node); //insert current_node at the beginning of current path (and shift everything right)
            }
            catch (Exception exception)
            {
                MessageBox.Show("Exception: " + exception.ToString() + " while inserting node " + current_node.name + " into path");
                return (-1);
            }
            Node starting_node = list_of_paths[0].Last();
            if (current_node.name != starting_node.name) //check if current_node is the starting node when this method was initially called (i.e. starting logger); if not check if current_node is a logger or a neighbour of a logger
            {
                if (list_of_loggers.Exists(tmp => tmp.node == current_node)) //check if current_node is a logger, if yes finish building this path (return)
                    return (0);
                List<NodeNeighbour> neighbours_of_current_node = current_node.list_of_neighbours;
                List<Node> neighboursOfCurrentNode = new List<Node>();
                for (int i = 0; i < neighbourhoodLevel; i++) //go up to specified neighbourhoodLevel; if neighbourhoodLevel==0 then neighboursOfCurrentNode will be empty
                    //only get neighbours with sufficiently small head difference
                    neighboursOfCurrentNode.AddRange(current_node.listsHigherLevelNeighbours[i].Where(tmp => tmp.Item2 < head_diff_tolerance).Select(tmp => tmp.Item1));
                                
                List<Node> tmp_logger_nodes = list_of_loggers.ConvertAll<Node>(new Converter<Logger, Node>(Logger.GetLoggerNode));
                List<Node> neighbours_being_loggers = new List<Node>(tmp_logger_nodes.Intersect<Node>(neighboursOfCurrentNode)); //list of neighbours of current_node which are loggers
                List<Node> neighbours_being_loggers_except_starting = new List<Node>(neighbours_being_loggers.FindAll(tmp => tmp != starting_node)); //exclude starting logger
                if (neighbours_being_loggers_except_starting.Count > 0) //there are some loggers in the neighbourhood of current_node
                {
                    bool path_stops_here = false;
                    bool current_path_updated = false;
                    List<Node> path_copy = new List<Node>(list_of_paths[current_path_index]); //copy section of the current path to a new path before adding node neighbours_being_loggers[0]; it will be used to add the following nodes in the for loop below
                    Node firstLoggerNeighbourNode = neighbours_being_loggers_except_starting[0];
                    //get nodes along the shortest path between current_node and firstLoggerNeighbourNode, this will give all required nodes, but some may be actually parallel if there are parallel paths of the same length
                    List<Node> nodesBetweenCurrentAndLogger = flow_tree.water_network.NodesBetween(current_node, firstLoggerNeighbourNode);
                    nodesBetweenCurrentAndLogger.Add(firstLoggerNeighbourNode); //end the path with the logger
                    nodesBetweenCurrentAndLogger.Reverse(); //order needs to be reversed so the 1st element is the logger
                    //check if starting node is a neighbour of current node 
                    if (neighbours_being_loggers.Exists(tmp => tmp == starting_node))
                    {
                        //check if there is positive flow from current node to firstLoggerNeighbourNode, if yes the path should not stop at firstLoggerNeighbourNode, if no we stop the path here
                        if (!flow_tree.IsPositiveFlowFromTo(current_node, firstLoggerNeighbourNode, neighbourhoodLevel))
                        {
                            path_stops_here = true;
                            current_path_updated = true;                            
                            list_of_paths[current_path_index].InsertRange(0, nodesBetweenCurrentAndLogger);                            
                        }
                    }
                    else //starting node is NOT a neighbour of current node, we stop the path here
                    {
                        path_stops_here = true;
                        current_path_updated = true;                        
                        list_of_paths[current_path_index].InsertRange(0, nodesBetweenCurrentAndLogger);
                    }

                    for (int i = 1; i < neighbours_being_loggers_except_starting.Count; i++) //process other neighbours_being_loggers
                    {
                        Node followingLoggerNeighbourNode = neighbours_being_loggers_except_starting[i];
                        //get nodes along the shortest path between current_node and followingLoggerNeighbourNode, this will give all required nodes, but some may be actually parallel if there are parallel paths of the same length
                        nodesBetweenCurrentAndLogger = flow_tree.water_network.NodesBetween(current_node, followingLoggerNeighbourNode);
                        nodesBetweenCurrentAndLogger.Add(followingLoggerNeighbourNode); //end the path with the logger
                        nodesBetweenCurrentAndLogger.Reverse(); //order needs to be reversed so the 1st element is the logger
                        //check if starting node is a neighbour of current node 
                        if (neighbours_being_loggers.Exists(tmp => tmp == starting_node))
                        {
                            //check if there is positive flow from current node to followingLoggerNeighbourNode, if yes the path should not stop at followingLoggerNeighbourNode, if no we stop the path here
                            if (!flow_tree.IsPositiveFlowFromTo(current_node, followingLoggerNeighbourNode, neighbourhoodLevel))
                            {
                                //the path will stop here, but this function will return after this for loop is finished
                                path_stops_here = true;
                                if (current_path_updated) //this path was already finished with logger before or within this for loop, so we need to work on copy now
                                {
                                    list_of_paths.Add(new List<Node>(path_copy));//add the copied section of current path to list_of_paths
                                    list_of_paths.Last().InsertRange(0, nodesBetweenCurrentAndLogger); //add these nodes to the newly added path
                                }
                                else //this path was not already finished with logger -> add this node to the current path
                                {
                                    current_path_updated = true;                                    
                                    list_of_paths[current_path_index].InsertRange(0, nodesBetweenCurrentAndLogger);                                  
                                }
                            }
                        }
                        else //starting node is NOT a neighbour of current node, the path will stop here, but this function will return after this for loop is finished
                        {
                            path_stops_here = true;
                            if (current_path_updated) //this path was already finished with logger before or within this for loop, so we need to work on copy now
                            {
                                list_of_paths.Add(new List<Node>(path_copy));//add the copied section of current path to list_of_paths
                                list_of_paths.Last().InsertRange(0, nodesBetweenCurrentAndLogger); //add these nodes to the newly added path
                            }
                            else //this path was not already finished with logger -> add this node to the current path
                            {
                                current_path_updated = true;
                                list_of_paths[current_path_index].InsertRange(0, nodesBetweenCurrentAndLogger);
                            }
                        }
                    }
                    if (path_stops_here)
                        return (0);
                }               
            }
            //current_node is not a logger nor neighbour of a logger with sufficiently small head difference and sufficiently small distance, so we continue building the path
            //int no_of_previous_nodes = flow_tree.GetFlowTreeJunction(current_node).list_of_previous_nodes.Count;
            FlowTree.FlowTreeJunction tmp_flowtreejunction = flow_tree.GetFlowTreeJunction(current_node);
            int no_of_previous_nodes;
            if (tmp_flowtreejunction == null) //flowtreefunction does not exist for this node, so this node is not a part of the tree, hence we can't continue constructing path for this node
                return (0);
            else
                no_of_previous_nodes = tmp_flowtreejunction.list_of_previous_nodes.Count;
            for (int i = 1; i < no_of_previous_nodes; i++) //if no_of_previous_nodes>1 then there is branching at this node; create a new path
            {
                int no_of_paths = list_of_paths.Count; //number of paths already being constructed
                List<Node> path_copy = new List<Node>(list_of_paths[current_path_index]);//copy section of the current path to new path                
                list_of_paths.Add(path_copy);//add the copied section of current path to list_of_paths
                Node tmp_previous_node = flow_tree.GetFlowTreeJunction(current_node).list_of_previous_nodes[i];
                ret_val = FindPathsToLogger(list_of_paths, tmp_previous_node, no_of_paths, neighbourhoodLevel);//call FindPathsToLogger recursively with current_path_index=no_of_paths, so this call will continue constructing the path just created above (copied section of current path)
                if (ret_val < 0)
                    return (ret_val);
            }
            //now continue constructing the current path
            Node first_previous_node = flow_tree.GetFlowTreeJunction(current_node).list_of_previous_nodes[0];
            ret_val = FindPathsToLogger(list_of_paths, first_previous_node, current_path_index, neighbourhoodLevel);
            return (ret_val);
        }

        ///// <summary>Goes backward (agains the flow, using flow_tree) from the current node until logger node (or its direct neighbour with sufficiently small head difference) is encountered and builds path (list of loggers) which is 
        ///// part of list_of_paths; for each branching encountered in the flow_tree the current segment of the path is copied to a new path, and this method is run recursively for the new path
        ///// </summary>
        ///// <param name="list_of_paths">list of all paths between 2 loggers</param>
        ///// <param name="current_node"></param>
        ///// <param name="current_path_index">to identify which path stored in list_of_paths should be constructed by this method call, since it is called recursively</param>
        ///// <returns>0 if successful</returns>
        ///// <returns>-1 if exception while inserting node into list_of_paths</returns>
        //int FindPathsToLogger(List<List<Node>> list_of_paths, Node current_node, int current_path_index)
        //{
        //    int ret_val;
        //    if (list_of_paths.Count - 1 < current_path_index) //this path has not been started yet, so it needs to be allocated
        //        list_of_paths.Add(new List<Node>());
        //    try
        //    {
        //        list_of_paths[current_path_index].Insert(0, current_node); //insert current_node at the beginning of current path (and shift everything right)
        //    }
        //    catch (Exception exception)
        //    {
        //        MessageBox.Show("Exception: " + exception.ToString()+ " while inserting node " + current_node.name + " into path");
        //        return (-1);
        //    }
        //    Node starting_node = list_of_paths[0].Last();
        //    if (current_node.name != starting_node.name) //check if current_node is the starting node when this method was initially called (i.e. starting logger); if not check if current_node is a logger or a neighbour of a logger
        //    {
        //        if (list_of_loggers.Exists(tmp => tmp.node.name == current_node.name)) //check if current_node is a logger, if yes finish building this path (return)
        //            return (0);
        //        List<NodeNeighbour> neighbours_of_current_node = current_node.list_of_neighbours;                
        //        //v2:
        //        List<Node> tmp_neigh_nodes = neighbours_of_current_node.ConvertAll<Node>(new Converter<NodeNeighbour, Node>(NodeNeighbour.GetNode));
        //        List<Node> tmp_logger_nodes = list_of_loggers.ConvertAll<Node>(new Converter<Logger, Node>(Logger.GetLoggerNode));
        //        List<Node> neighbours_being_loggers = new List<Node>(tmp_logger_nodes.Intersect<Node>(tmp_neigh_nodes)); //list of neighbours of current_node which are loggers
        //        neighbours_being_loggers = neighbours_being_loggers.FindAll(tmp => tmp.name != starting_node.name);//exclude starting logger                
        //        if (neighbours_being_loggers.Count > 0)
        //        {
        //            bool path_stops_here = false;
        //            bool current_path_updated = false;
        //            List<Node> path_copy = new List<Node>(list_of_paths[current_path_index]); //copy section of the current path to a new path before adding node neighbours_being_loggers[0]; it will be used to add the following nodes in the for loop below
        //            NodeNeighbour node_neighbour = current_node.list_of_neighbours.Find(tmp => tmp.node.name == neighbours_being_loggers[0].name); //neighbour object corresponding to the current_node which is logger
        //            if (Math.Abs(node_neighbour.avg_head_diff) < head_diff_tolerance) //check if head difference is sufficiently small
        //            {
        //                //we've reached logger which is neighbour of the current_node with sufficiently small head difference so the path will stop here
        //                path_stops_here = true;
        //                current_path_updated = true;
        //                list_of_paths[current_path_index].Insert(0, neighbours_being_loggers[0]);
        //            }
        //            for (int i = 1; i < neighbours_being_loggers.Count; i++) //process other neighbours_being_loggers
        //            {
        //                Node neigh_log_node = neighbours_being_loggers[i];
        //                node_neighbour = current_node.list_of_neighbours.Find(tmp => tmp.node.name == neigh_log_node.name); //neighbour object corresponding to the current_node which is logger
        //                if (Math.Abs(node_neighbour.avg_head_diff) < head_diff_tolerance) //check if head difference is sufficiently small
        //                {
        //                    //we've reached logger which is neighbour of the current_node with sufficiently small head difference so the path will stop here, but this function will return after this for loop is finished
        //                    path_stops_here = true;
        //                    if (current_path_updated) //this path was already finished with logger before or within this for loop, so we need to work on copy now
        //                    {
        //                        list_of_paths.Add(new List<Node>(path_copy));//add the copied section of current path to list_of_paths
        //                        list_of_paths.Last().Insert(0, neigh_log_node);//add this node to the newly added path
        //                    }
        //                    else //this path was not already finished with logger, -> add this node to the current path
        //                    {
        //                        current_path_updated = true;
        //                        list_of_paths[current_path_index].Insert(0, neigh_log_node);
        //                    }
        //                }
        //            }
        //            if (path_stops_here)
        //                return (0);
        //        }

        //        //v1: only the first neighbour which is logger is added to path, if current_node has more such neighbours (which may occur if 2 loggers are separated by only 1 node), others are ignored
        //        /*foreach (NodeNeighbour neighbour in neighbours_of_current_node)
        //        {
        //            if (list_of_loggers.Exists(tmp => tmp.node.name == neighbour.node.name) && (neighbour.node.name != list_of_paths[0].Last().name)) //check if current_node is direct neighbour of any logger except for starting logger
        //            {
        //                //neighbour is a logger and is not starting logger;                         
        //                //insert this neighbour to the list_of_paths and return
        //                list_of_paths[current_path_index].Insert(0, neighbour.node);
        //                return (0);
        //            }
        //        }*/
        //    }
        //    //current_node is not a logger nor neighbour of a logger with sufficiently small head difference, so we continue building the path
        //    //int no_of_previous_nodes = flow_tree.GetFlowTreeJunction(current_node).list_of_previous_nodes.Count;
        //    FlowTree.FlowTreeJunction tmp_flowtreejunction = flow_tree.GetFlowTreeJunction(current_node);
        //    int no_of_previous_nodes;
        //    if (tmp_flowtreejunction == null) //flowtreefunction does nto exist for this node, so this node is not a part of the tree, hence we can't continue constructing path for this node
        //        return (0);
        //    else
        //        no_of_previous_nodes= tmp_flowtreejunction.list_of_previous_nodes.Count;
        //    for (int i = 1; i < no_of_previous_nodes; i++) //if no_of_previous_nodes>1 then there is branching at this node; create a new path
        //    {
        //        int no_of_paths = list_of_paths.Count; //number of paths already being constructed
        //        List<Node> path_copy = new List<Node>(list_of_paths[current_path_index]);//copy section of the current path to new path                
        //        list_of_paths.Add(path_copy);//add the copied section of current path to list_of_paths
        //        Node tmp_previous_node = flow_tree.GetFlowTreeJunction(current_node).list_of_previous_nodes[i];
        //        ret_val = FindPathsToLogger(list_of_paths, tmp_previous_node, no_of_paths);//call FindPathsToLogger recursively with current_path_index=no_of_paths, so this call will continue constructing the path just created above (copied section of current path)
        //        if (ret_val < 0)
        //            return (ret_val);
        //    }
        //    //now continue constructing the current path
        //    Node first_previous_node = flow_tree.GetFlowTreeJunction(current_node).list_of_previous_nodes[0];
        //    ret_val = FindPathsToLogger(list_of_paths, first_previous_node, current_path_index);
        //    return (ret_val);
        //}
        
        /// <summary>Uses relations_between_loggers to generate logger_water_network which will be used for visualisation
        /// </summary>
        /// <param name="efavor_test">efavor test object needed to retrieve info which measurement points correspond to which inlet set (PRVs belonging to the same inlet set were throttled at the same time) </param>
        /// <param name="inlet_set_id">inlet set id specifying which measurement points should be taken to calculate delta2h</param>
        /// <param name="d2h_as_percentage">if true, delta2h is normalised and saved to log-log link label as percentage alongside absolute d2h, otherwise only as absolute d2h</param>
        /// <returns>0 if successful</returns>
        /// <returns>negative error code of inner methods that failed</returns>
        /// <returns>-20 if other exception</returns>
        public int LoggerConn2WaterNet(EFavorTest efavor_test, string inlet_set_id, bool d2h_as_percentage)
        {
            try
            {
                int ret_val;
                logger_water_network = new WaterNetwork();
                logger_water_network.listOfNodes = new List<Node>();
                logger_water_network.listOfLinks = new List<Link>();
                constants = new Constants();
                //generate nodes from loggers
                foreach (Logger logger in list_of_loggers)
                {
                    Node new_node = new Node(0);
                    new_node.name = logger.logger_id; //node name is taken from logger id
                    new_node.xcoord = logger.node.xcoord;
                    new_node.ycoord = logger.node.ycoord;
                    new_node.isLogger = true;
                    //added size and colors
                    new_node.size = UI.Default.LoggerNodeSize;
                    if (efavor_test.list_of_inlet_loggers.Exists(tmp => tmp.logger_id == logger.logger_id)) //this is inlet logger
                        new_node.color = ToBrush(UI.Default.LoggerInletNodeColor);
                    else
                        new_node.color = ToBrush(UI.Default.LoggerNodeColor);
                    logger_water_network.listOfNodes.Add(new_node);
                }
                logger_water_network.nNodes = logger_water_network.listOfNodes.Count;

                //go through all connections to find min and max d2h and d2h_percentage for appropriate colour and width coding
                List<Tuple<int, int>> listConnections = GetLogConnFromToList();
                double max_d2h_positive = 0, max_d2h_percent_positive = 0, min_d2h_positive = 10000, min_d2h_percent_positive = 1000000;
                //max_...negative means the most negative, i.e. negative with largest Abs(value)
                double max_d2h_negative = 0, max_d2h_percent_negative = 0, min_d2h_negative = -10000, min_d2h_percent_negative = -1000000; 
                foreach (Tuple<int, int> logger_pair in listConnections)
                {
                    Logger logger_from = list_of_loggers[logger_pair.Item1];
                    Logger logger_to = list_of_loggers[logger_pair.Item2];
                    double tmp_d2h, tmp_dh, tmp_d2h_percent;
                    ret_val = GetDelta2hMax(logger_from, logger_to, efavor_test, inlet_set_id, out tmp_d2h, out tmp_dh);
                    if (ret_val < 0)
                        return (ret_val);
                    if (tmp_dh != 0)
                        tmp_d2h_percent = tmp_d2h / tmp_dh * 100;
                    else
                        tmp_d2h_percent = 0;

                    if (tmp_d2h > max_d2h_positive)
                        max_d2h_positive = tmp_d2h;
                    if ((tmp_d2h > 0) && (tmp_d2h < min_d2h_positive))
                        min_d2h_positive = tmp_d2h;
                    if (tmp_d2h < max_d2h_negative)
                        max_d2h_negative = tmp_d2h;
                    if ((tmp_d2h < 0) && (tmp_d2h > min_d2h_negative))
                        min_d2h_negative = tmp_d2h;

                    if (tmp_d2h_percent > max_d2h_percent_positive)
                        max_d2h_percent_positive = tmp_d2h_percent;
                    if ((tmp_d2h_percent > 0) && (tmp_d2h_percent < min_d2h_percent_positive))
                        min_d2h_percent_positive = tmp_d2h_percent;
                    if (tmp_d2h_percent < max_d2h_percent_negative)
                        max_d2h_percent_negative = tmp_d2h_percent;
                    if ((tmp_d2h_percent < 0) && (tmp_d2h_percent > min_d2h_percent_negative))
                        min_d2h_percent_negative = tmp_d2h_percent;                    
                }
                //factor to multiply abs(d2h) to obtain the value within required output range of line thickness
                double thick_factor = (UI.Default.LoggerMaxLinkThickness - UI.Default.LoggerMinLinkThickness) / (Math.Max(max_d2h_positive, Math.Abs(max_d2h_negative)) - Math.Min(min_d2h_positive, Math.Abs(min_d2h_negative)));
                //factor to multiply positive d2h to obtain the value within required output range of line colour
                double color_factor_positive = (UI.Default.LoggerMaxRedComponent - UI.Default.LoggerMinRedComponent) / (max_d2h_percent_positive - min_d2h_percent_positive);
                //factor to multiply negative d2h to obtain the value within required output range of line colour
                double color_factor_negative = (UI.Default.LoggerMaxBlueComponent - UI.Default.LoggerMinBlueComponent) / Math.Abs(max_d2h_percent_negative - min_d2h_percent_negative);

                for (int i = 0; i < list_of_loggers.Count; i++)
                {
                    for (int j = 0; j < list_of_loggers.Count; j++)
                    {
                        if (logger_connection_matrix[i, j])
                        {
                            Link new_link = new Link();
                            new_link.name = "L" + list_of_loggers[i].logger_id + "-" + list_of_loggers[j].logger_id;
                            new_link.type = Constants.EN_LOG_CONN;  //link type: logger connection
                            new_link.nodeFrom = logger_water_network.listOfNodes.FindIndex(tmp => tmp.name == list_of_loggers[i].logger_id); //node name in logger_water_network object is taken from logger id, hence such search
                            new_link.nodeTo = logger_water_network.listOfNodes.FindIndex(tmp => tmp.name == list_of_loggers[j].logger_id);
                            double delta2h_max; //headloss change
                            double delta_h; //headloss
                            ret_val = GetDelta2hMax(list_of_loggers[i], list_of_loggers[j], efavor_test, inlet_set_id, out delta2h_max, out delta_h);
                            if (ret_val < 0)
                                return (ret_val);
                            double delta2h_max_percent;
                            if (delta_h != 0)
                                delta2h_max_percent = delta2h_max / delta_h * 100;
                            else
                                delta2h_max_percent = 0;
                            if (d2h_as_percentage)
                                if (delta_h == 0)
                                    new_link.label = "0";
                                else
                                    new_link.label = delta2h_max.ToString("f2") + " (" + delta2h_max_percent.ToString("f1") + "%)";
                            else
                                new_link.label = delta2h_max.ToString("f2");

                            //added size and colors
                            SolidColorBrush brush = new SolidColorBrush();
                            if (delta2h_max >= 0) //manipulate red
                                brush.Color = Color.FromRgb((byte)(delta2h_max_percent * color_factor_positive + UI.Default.LoggerMinRedComponent), UI.Default.LoggerStandardGreenComponent, UI.Default.LoggerStandardBlueComponent);
                            else //manipulate blue
                                brush.Color = Color.FromRgb(UI.Default.LoggerStandardRedComponent, UI.Default.LoggerStandardGreenComponent, (byte)Math.Abs(delta2h_max_percent * color_factor_negative + UI.Default.LoggerMinBlueComponent));
                            //new_link.color = constants.loggerLinkColor;
                            new_link.color = brush;
                                                                                    
                            //new_link.thickness = constants.loggerLinkThickness;
                            new_link.thickness = Math.Abs(delta2h_max * thick_factor) + UI.Default.LoggerMinLinkThickness;

                            logger_water_network.listOfLinks.Add(new_link);
                        }
                    }
                }
                logger_water_network.nLinks = logger_water_network.listOfLinks.Count;

                //type of water network object
                logger_water_network.waterNetworkType = Constants.LOGGER_NETWORK;
                return (0);
            }//try
            catch (Exception ex)
            {
                MessageBox.Show("Error in LoggerConn2WaterNet: " + ex.Message);
                return (-20);
            }
        }

        /// <summary>Uses relations_between_loggers to generate logger_water_network which will be used for visualisation; for visualisation of connections only, not d2h
        /// </summary>
        /// <returns>0 if successful</returns>
        /// <returns>-20 if other exception</returns>
        public int LoggerConn2WaterNet()
        {
            try
            {
                logger_water_network = new WaterNetwork();
                logger_water_network.listOfNodes = new List<Node>();
                logger_water_network.listOfLinks = new List<Link>();
                constants = new Constants();
                //generate nodes from loggers
                foreach (Logger logger in list_of_loggers)
                {
                    Node new_node = new Node(0);
                    new_node.name = logger.logger_id; //node name is taken from logger id
                    new_node.xcoord = logger.node.xcoord;
                    new_node.ycoord = logger.node.ycoord;
                    new_node.isLogger = true;
                    //added size and colors
                    new_node.size = UI.Default.LoggerNodeSize;
                    if (logger.is_inlet) //this is inlet logger
                        new_node.color = ToBrush(UI.Default.LoggerInletNodeColor);
                    else
                        new_node.color = ToBrush(UI.Default.LoggerNodeColor);
                    logger_water_network.listOfNodes.Add(new_node);
                }
                logger_water_network.nNodes = logger_water_network.listOfNodes.Count;

                //generate links from connections
                for (int i = 0; i < list_of_loggers.Count; i++)
                {
                    for (int j = 0; j < list_of_loggers.Count; j++)
                    {
                        if (logger_connection_matrix[i, j])
                        {
                            Link new_link = new Link();
                            new_link.name = "L" + list_of_loggers[i].logger_id + "-" + list_of_loggers[j].logger_id;
                            new_link.type = Constants.EN_LOG_CONN;  //link type: logger connection
                            new_link.nodeFrom = logger_water_network.listOfNodes.FindIndex(tmp => tmp.name == list_of_loggers[i].logger_id); //node name in logger_water_network object is taken from logger id, hence such search
                            new_link.nodeTo = logger_water_network.listOfNodes.FindIndex(tmp => tmp.name == list_of_loggers[j].logger_id);
                            new_link.label = new_link.name;                                

                            //added size and colors
                            new_link.color = ToBrush(UI.Default.LoggerLinkColor);
                            new_link.thickness = UI.Default.LoggerLinkThickness;

                            logger_water_network.listOfLinks.Add(new_link);
                        }
                    }
                }
                logger_water_network.nLinks = logger_water_network.listOfLinks.Count;

                //type of water network object
                logger_water_network.waterNetworkType = Constants.LOGGER_NETWORK;
                return (0);
            }//try
            catch (Exception ex)
            {
                MessageBox.Show("Error in LoggerConn2WaterNet: " + ex.Message);
                return (-20);
            }
        }

        /// <summary> Returns list of all paths between two loggers identified by their logger ID
        /// </summary>
        /// <param name="logger_from_id"></param>
        /// <param name="logger_to_id"></param>
        /// <returns>list of all paths or null if no paths exist or if wrong logger id </returns>
        public List<List<Node>> GetAllPathsBetweenLoggers(string logger_from_id, string logger_to_id)
        {
            int logger_from = list_of_loggers.FindIndex(tmp => tmp.logger_id == logger_from_id);
            int logger_to = list_of_loggers.FindIndex(tmp => tmp.logger_id == logger_to_id);
            if ((logger_from < 0) || (logger_to < 0)) //wrong logger id
                return (null);
            if (logger_connection_matrix[logger_from, logger_to])
                return (relations_between_loggers[logger_from, logger_to].list_of_paths_between_loggers);
            else
                return (null);           
        }

        /// <summary>Calculates and returns number of connections between loggers indicated by logger_connection_matrix; if there are more than 1 path between Li and Lj this is still treated as 1 connection
        /// </summary>
        /// <returns>number of connections</returns>
        public int GetnLoggerConnections()
        {
            int nrows = logger_connection_matrix.GetLength(0);
            int ncolumns = logger_connection_matrix.GetLength(1);
            int counter = 0;
            for (int i = 0; i < nrows; i++)
            {
                for (int j = 0; j < ncolumns; j++)
                {
                    if (logger_connection_matrix[i,j])
                        counter++;
                }
            }
            return (counter);            
        }

        /// <summary> Calculates and returns list of logger indices (log_from, log_to) for all connections between loggers indicated by logger_connection_matrix
        /// </summary>
        /// <returns>List of Tuples; each Tuple is (log_from index, log_to index)</returns>
        public List<Tuple<int, int>> GetLogConnFromToList()
        {
            List<Tuple<int, int>> logConnList = new List<Tuple<int, int>>();
            int nrows = logger_connection_matrix.GetLength(0);
            int ncolumns = logger_connection_matrix.GetLength(1);
            for (int i = 0; i < nrows; i++)
            {
                for (int j = 0; j < ncolumns; j++)
                {
                    if (logger_connection_matrix[i, j])
                    {
                        Tuple<int, int> new_tuple = new Tuple<int, int>(i, j);
                        logConnList.Add(new_tuple);
                    }                        
                }
            }
            return (logConnList);
        }

        /// <summary>Check if there are any loggers next to each other (direct neighbours) with head difference smaller than AdvancedOptions.head_diff_tolerance
        /// </summary>
        /// <param name="listLoggers">list of loggers</param>
        /// <returns>True if there are loggers next to each other and with head difference smaller than AdvancedOptions.head_diff_tolerance</returns>
        public static bool CheckLoggersProximity(List<Logger> listLoggers)
        {
            List<Node> listLoggerNodes = listLoggers.ConvertAll<Node>(new Converter<Logger, Node>(Logger.GetLoggerNode));
            foreach (Node loggerNode in listLoggerNodes)
            {
                //get all direct neighbours of loggerNode with sufficiently small head difference
                List<Node> nodeNeighbours=loggerNode.listsHigherLevelNeighbours[0].Where(tmp => tmp.Item2 < AdvancedOptions.head_diff_tolerance).Select(tmp => tmp.Item1).ToList();
                if ((nodeNeighbours != null) && (nodeNeighbours.Intersect(listLoggerNodes).ToList().Count > 0)) //if intersetion of set of node neighbours and all loggers is not empty, then there are loggers next to each other and with head difference smaller than AdvancedOptions.head_diff_tolerance
                    return (true);                
            }
            return (false);
        }

        /// <summary>Returns a list of all nodes that form paths of all logger connections stored in relations_between_loggers
        /// </summary>
        /// <returns></returns>
        public List<Node> GetNodesFromAllPathsAllConnections()
        {
            List<Node> listNodes = new List<Node>();
            int nrows = logger_connection_matrix.GetLength(0);
            int ncolumns = logger_connection_matrix.GetLength(1);
            for (int i = 0; i < nrows; i++)
            {
                for (int j = 0; j < ncolumns; j++)
                {
                    if (logger_connection_matrix[i, j])
                    {
                        listNodes.AddRange(relations_between_loggers[i, j].list_of_paths_between_loggers.SelectMany(tmp => tmp));
                    }
                }
            }
            listNodes = listNodes.Distinct().ToList();
            return (listNodes);
        }

        /// <summary>Recursively adds to the list nodes in flowtrees outgoing from currentNode, but only of these which don't belong to any path between any loggers 
        /// </summary>
        /// <param name="branchNodeList">list of nodes to be constructed</param>
        /// <param name="currentNode"></param>
        /// <param name="allPathsAllLoggers">list of nodes in all paths of all loger connections</param>
        /// <returns></returns>
        public int AddBranchesNotInPaths(ref List<Node> branchNodeList, Node currentNode, List<Node> allPathsAllLoggers)
        {
            FlowTree.FlowTreeJunction currentNodeJunction = flow_tree.GetFlowTreeJunction(currentNode);
            if (currentNodeJunction == null)
                return (-1);
            foreach (Node nextNode in currentNodeJunction.list_of_next_nodes)
            {
                if (allPathsAllLoggers.Exists(tmp => tmp == nextNode)) //check if nextNode belongs to any path, if yes ignore it
                    continue;
                if (branchNodeList.Exists(tmp => tmp == nextNode)) //if nextNode is already in branchNodeList, then ignore it as it was already processed
                    continue;
                //if not, add nextNode to the list and run AddBranchesNotInPaths recursively
                branchNodeList.Add(nextNode);
                if (AddBranchesNotInPaths(ref branchNodeList, nextNode, allPathsAllLoggers) < 0)
                    return (-1);    
            }
            return (0);
        }

        /// <summary>Returns list of all nodes from paths between given 2 loggers, and also nodes in flowtree branches starting from the paths, but which do not belong to any other path stored in relations_between_loggers 
        /// </summary>
        /// <param name="logger_from_id"></param>
        /// <param name="logger_to_id"></param>
        /// <returns>list of nodes or null if no paths exist or if wrong logger id</returns>
        public List<Node> GetNodesFromPathsAndSideBranchesBetweenLoggers(string logger_from_id, string logger_to_id)
        {
            try
            {
                List<List<Node>> tmpList = GetAllPathsBetweenLoggers(logger_from_id, logger_to_id);
                if ((tmpList == null) || (tmpList.Count == 0))
                    return (null);
                List<Node> theseLoggersPathsNodes = tmpList.SelectMany(tmp => tmp).Distinct().ToList();
                List<Node> allPaths = GetNodesFromAllPathsAllConnections();
                List<Node> pathsAndBranches = new List<Node>();
                foreach (Node currentNode in theseLoggersPathsNodes)
                {
                    pathsAndBranches.Add(currentNode);
                    AddBranchesNotInPaths(ref pathsAndBranches, currentNode, allPaths);
                }
                return (pathsAndBranches.Distinct().ToList()); //distinct just in case some nodes were repeated
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while retrieving paths with side branches between loggers " + logger_from_id + " and " + logger_to_id + ". Exception: " + ex.Message);
                return (null);
            }


        }

        /// <summary>Calcula TotalD2H and TotalD2H_percentage from inlet for each logger. Implemented only for 1 inlet!
        /// </summary>
        /// <param name="efavor_test"></param>
        /// <param name="inlet_set_id"></param>
        /// <returns>List of Tuple(logger_id, totalD2H, totalD2H_percentage</returns>
        public List<Tuple<string, double, double>> GetLoggerListWithD2HFromInlet(EFavorTest efavor_test, string inlet_set_id)
        {
            List<Tuple<string, double, double>> tupleList = new List<Tuple<string, double, double>>();
            if (efavor_test.list_of_inlet_loggers.Count > 1)
                throw new Exception("This method is currenlty implemented only for 1-inlet networks. ");
            Logger inletLogger = efavor_test.list_of_inlet_loggers[0];            
            double totalD2H;
            double totalDH;
            foreach (Logger currentLogger in list_of_loggers)
            {
                if (currentLogger.is_inlet == true)
                {
                    totalD2H = 0;
                    totalDH = 0;
                }
                else
                {
                    GetDelta2hMax(inletLogger, currentLogger, efavor_test, inlet_set_id, out totalD2H, out totalDH);
                }
                Tuple<string, double, double> tmpTuple;
                if (totalDH == 0)
                    tmpTuple = new Tuple<string, double, double>(currentLogger.logger_id, 0, 0);
                else
                    tmpTuple = new Tuple<string, double, double>(currentLogger.logger_id, totalD2H, totalD2H / totalDH * 100);
                tupleList.Add(tmpTuple);
            }
            return(tupleList);
        }
    }

    /// <summary>Class to store burst coefficients 
        /// </summary>
    public class BurstCoeffs
        {
            public double est_demand; ///estimated demand
            public double est_burst_coeff; /// estimated burst coefficient
            public double est_burst_exponent; /// estimated burst exponent
            public double est_backleak_coeff; ///estimated background leakage coefficient                                          
            public double est_backleak_exponent; /// estimated background leakage exponent
            public double[] est_burst_flow; ///estimated burst flow for different pressures (est_burst_flow.length = number of flow/pressure data points)
            public double[] est_backleak_flow; ///estimated background leakage flow for different pressures (est_backleak_flow.length = number of flow/pressure data points)

            /// <summary>constructor to enter initial values for regression algorithm for 2 term inlet flow model 
            /// </summary>
            /// <param name="demand"></param>
            /// <param name="burst_coeff"></param>
            /// <param name="burst_exponent"></param>
            public BurstCoeffs(double demand, double burst_coeff, double burst_exponent)
            {
                est_demand = demand;
                est_burst_coeff = burst_coeff;
                est_burst_exponent = burst_exponent;
                est_backleak_coeff = 0;
                est_backleak_exponent = 0;
            }
            public BurstCoeffs()
            {
                est_demand = 0;
                est_burst_coeff = 0;
                est_burst_exponent = 0;
                est_backleak_coeff = 0;
                est_backleak_exponent = 0;
            }
            
            /// <summary>constructor which fills members of this by taking average of coefficients in the supplied list 
            /// </summary>
            /// <param name="list_burst_coeffs"></param>
            /// <returns>0 if successful</returns>
            public BurstCoeffs(List<BurstCoeffs> list_burst_coeffs)
            {
                if ((list_burst_coeffs == null) || (list_burst_coeffs.Count == 0)) //if list not allocated or empty
                {
                    MessageBox.Show("Error: Empty list supplied in BurstCoeffs");
                    return;
                }
                est_demand = 0;
                est_burst_coeff = 0;
                est_burst_exponent = 0;
                est_backleak_coeff = 0;
                est_backleak_exponent = 0;
                if (list_burst_coeffs.All(tmp => tmp.est_burst_flow != null) && list_burst_coeffs.All(tmp => tmp.est_burst_flow.Length == list_burst_coeffs[0].est_burst_flow.Length)) //check if est_burst_flow is allocated in the elements of supplied list and if lengths of est_burst_flow are equal in all elements
                    est_burst_flow = new double[list_burst_coeffs[0].est_burst_flow.Length];
                else
                    est_burst_flow = null;
                if (list_burst_coeffs.All(tmp => tmp.est_backleak_flow != null) && list_burst_coeffs.All(tmp => tmp.est_backleak_flow.Length == list_burst_coeffs[0].est_backleak_flow.Length)) //check if est_backleak_flow is allocated in the elements of supplied list and if lengths of est_backleak_flow are equal in all elements
                    est_backleak_flow = new double[list_burst_coeffs[0].est_backleak_flow.Length];
                else
                    est_backleak_flow = null;
                foreach (BurstCoeffs burst_coeff in list_burst_coeffs)
                {
                    est_demand += burst_coeff.est_demand;
                    est_burst_coeff += burst_coeff.est_burst_coeff;
                    est_burst_exponent += burst_coeff.est_burst_exponent;
                    est_backleak_coeff += burst_coeff.est_backleak_coeff;
                    est_backleak_exponent += burst_coeff.est_backleak_exponent;
                    if (est_burst_flow != null)
                        for (int i = 0; i < list_burst_coeffs[0].est_burst_flow.Length; i++)
                            est_burst_flow[i] += burst_coeff.est_burst_flow[i];
                    if (est_backleak_flow != null)
                        for (int i = 0; i < list_burst_coeffs[0].est_backleak_flow.Length; i++)
                            est_backleak_flow[i] += burst_coeff.est_backleak_flow[i];
                }
                est_demand /= list_burst_coeffs.Count;
                est_burst_coeff /= list_burst_coeffs.Count;
                est_burst_exponent /= list_burst_coeffs.Count;
                est_backleak_coeff /= list_burst_coeffs.Count;
                est_backleak_exponent /= list_burst_coeffs.Count;
                if (est_burst_flow != null)
                    for (int i = 0; i < list_burst_coeffs[0].est_burst_flow.Length; i++)
                        est_burst_flow[i] /= list_burst_coeffs.Count;
                if (est_backleak_flow != null)
                    for (int i = 0; i < list_burst_coeffs[0].est_backleak_flow.Length; i++)
                        est_backleak_flow[i] /= list_burst_coeffs.Count;
            }
            
            /// <summary>Retrieve estimated burst flow array; used for ConvertAll
            /// </summary>
            /// <param name="burst_coeff"></param>
            /// <returns></returns>
            public static double[] GetEstBurstFlow(BurstCoeffs burst_coeff)
            {
                return (burst_coeff.est_burst_flow);
            }
                  
        }

    /// <summary>Class to estimate burst coefficients and burst flow using EFavor experiment data    
    /// </summary>
    public class BurstCoeffEstimator
    {
        public BurstCoeffs coeffs; //all burst coefficients

        /// <summary>initial values for regression algorithm for 2 term inlet flow model are obtained guess of burst flow (to be provided by operator) and from Efavor test 
        /// </summary>
        /// <param name="efavorTest"></param>
        /// <param name="burst_flow">burst flow at normal PRVs setting (i.e. at measurement number index)</param>
        /// <param name="index">0-based index of measurement data corresponding to normal PRVs setting</param>
        /// <returns>0 if successful</returns>
        /// <returns>-1 if exception thrown</returns>
        public int InitialiseCoeffs(EFavorTest efavorTest, double burst_flow, int index)
        {
            try
            {
                double inflow = 0;                
                foreach (FlowMeter flowmeter in efavorTest.list_of_inlet_flowmeters) //flow is sum of all inflows
                {
                    inflow += flowmeter.measured_flow[index];
                }        
                double avg_pressure = 0;
                foreach (Logger logger in efavorTest.list_of_loggers)
                {
                    avg_pressure += logger.measured_pressure[index];
                }
                avg_pressure = avg_pressure / efavorTest.no_of_loggers;

                double alpha = 0.6; //for initial guess assume 0.6
                double demand = inflow - burst_flow;
                if (demand < 0)
                    throw new Exception("Initial guess for burst flow can't be larger than the DMA inflow!");
                double k = burst_flow / Math.Pow(avg_pressure, alpha);
                coeffs = new BurstCoeffs(demand, k, alpha);
                return (0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return (-1);
            }
        }

        /// <summary>initial values for regression algorithm for 2 term inlet flow model are obtained from Efavor test by assuming that burst flow is [leak_percentage] of minimum night flow 
        /// </summary>
        /// <param name="efavorTest"></param>
        /// <param name="leak_percentage">what percent of minimum night flow is leakage; specify as percentage (0-100)</param>
        /// <returns>0 if successful</returns>
        /// <returns>-1 if exception thrown</returns>
        public int InitialiseCoeffs(EFavorTest efavorTest, double leak_percentage)
        {
            try
            {
                int nDataPoints = efavorTest.total_no_of_pressure_steps;
                double[] flowData = new double[nDataPoints];
                for (int i = 0; i < nDataPoints; i++)
                {
                    foreach (FlowMeter flowmeter in efavorTest.list_of_inlet_flowmeters) //flow is sum of all inflows
                    {
                        flowData[i] += flowmeter.measured_flow[i];
                    }
                }

                double min_flow = flowData.Min();
                int min_flow_index = flowData.ToList().FindIndex(tmp => tmp == min_flow);
                double avg_pressure = 0;
                foreach (Logger logger in efavorTest.list_of_loggers)
                {
                    avg_pressure += logger.measured_pressure[min_flow_index];
                }
                avg_pressure = avg_pressure / efavorTest.no_of_loggers;

                double alpha = 0.6; //for initial guess assume 0.6
                double demand = (100 - leak_percentage) / 100 * min_flow; //for initial guess assume that (100-leak_percentage)% of minimum night flow is demand
                double k = (leak_percentage / 100) * min_flow / Math.Pow(avg_pressure, alpha);
                coeffs = new BurstCoeffs(demand, k, alpha);
                return (0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return (-1);
            }
        }

        /// <summary>initialise coefficients for regression algorithm for 2 term inlet flow model with directly specified value
        /// </summary>
        /// <param name="demand"></param>
        /// <param name="burst_coeff"></param>
        /// <param name="burst_exponent"></param>
        public int InitialiseCoeffs(double demand, double burst_coeff, double burst_exponent)
        {
            coeffs = new BurstCoeffs(demand, burst_coeff, burst_exponent);
            return (0);
        }

        /// <summary> Estimates 2 term inlet flow model coefficients; i.e. no background leakage: q=d+k*p^alpha
        /// </summary>
        /// <param name="pressure_data"></param>
        /// <param name="flow_data"></param>
        /// <param name="tolerance_percent">stop regression iterations when average normalized error (in %) of estimated model is smaller than tolerance </param>
        /// <param name="max_iter">maximum number of iterations of the regression algorithms</param>
        /// <returns>0 if successful</returns>
        /// <returns>-1 if inconsistent data length</returns>
        /// <returns>-2 if data set too short</returns>
        /// <returns>-3 if negative pressure</returns>
        /// <returns>-4 if coeffs not initialised</returns>
        /// <returns>-20 if other exception thrown</returns>
        public int Estimate2TermModelCoeffs(double[] pressure_data, double[] flow_data, double tolerance_percent, int max_iter)
        {
            try
            {
                if (pressure_data.Length != flow_data.Length)
                {
                    MessageBox.Show("Length of pressure and flow data are not equal");
                    return (-1);
                }
                int nDataPoints = pressure_data.Length;
                if (nDataPoints < 3)
                {
                    MessageBox.Show("Data set too short. At least 3 measurements are required to estimate 3 coefficients of inlet flow model");
                    return (-2);
                }
                if (pressure_data.Any(tmp => tmp < 0)) //check if all pressure is positive
                {
                    MessageBox.Show("Negative pressure data given to estimate burst");
                    return (-3);
                }
                if (coeffs == null)
                {
                    MessageBox.Show("Initial guess for burst coefficients not given");
                    return (-4);
                }

                double[,] all_data = new double[2, nDataPoints];
                for (int i = 0; i < nDataPoints; i++)
                {
                    all_data[0, i] = pressure_data[i];
                    all_data[1, i] = flow_data[i];
                }

                Parameter d = new Parameter(coeffs.est_demand);
                Parameter k = new Parameter(coeffs.est_burst_coeff);
                Parameter alpha = new Parameter(coeffs.est_burst_exponent);
                Parameter p = new Parameter();
                Parameter q = new Parameter();
                
                Func<double> regressionFunction = () => (d + k * Math.Pow(p, alpha));
                Parameter[] regressionParameters = new Parameter[] { d, k, alpha };
                Parameter[] observedParameters = new Parameter[] { p };
                LevenbergMarquardt regression_object = new LevenbergMarquardt(regressionFunction, regressionParameters, observedParameters, all_data);
                
                for (int i = 0; i < max_iter; i++)
                {
                    regression_object.Iterate();
                    double error = 0;
                    for (int j = 0; j < nDataPoints; j++)
                    {
                        error += Math.Abs(d.Value + k.Value * Math.Pow(pressure_data[j], alpha.Value) - flow_data[j]) / flow_data[j]; //relative error
                    }
                    error = error / nDataPoints * 100; //multiply by 100 to obtain percentage
                    if (error < tolerance_percent)
                        break;
                }

                coeffs.est_demand = d.Value;
                coeffs.est_burst_coeff = k.Value;
                coeffs.est_burst_exponent = alpha.Value;
                coeffs.est_burst_flow = new double[nDataPoints];
                for (int i = 0; i < nDataPoints; i++)
                {
                    coeffs.est_burst_flow[i] = flow_data[i] - coeffs.est_demand;
                }

            }//try
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return (-20);
            }
            return (0);
        }

        //separate function to EstimateBurstUsingChiSquareCriteria and series of simulations (taking pressure for different loggers, then simulating and comparing inlet flow - as in Scicoslab)
        /// <summary>Estimates 2 term inlet flow model coefficients (i.e. no background leakage: q=d+k*p^alpha), 
        /// using PRVs inflow and pressure at loggers; loggers are taken one by one and their pressure is used to estimate the burst coeffs
        /// then for each such coeffs set simulation is run with burst allocated to this logger (as emitter) and simulated inlet flow is compared to measured inlet flow
        /// finally few sets of coeffs are chosen for which flow error simulation-measurement was the smallest (in terms of chi^2) and the final set of coefficients is average from these chosen sets;
        /// Saves estimated parameters to this.coeffs
        /// !!!note: simulation of burst as emitter makes sense only if there are no other emitters in the model, since burst exponent is global in stupid epanet
        /// </summary>
        /// <param name="epanet">object to communicate with epanet toolkit and simulate the network</param>
        /// <param name="waterNetwork">water network object</param>
        /// <param name="efavorTest">object with efavor test data</param>
        /// <param name="tolerance_percent">stop regression iterations when average normalized error (in %) of estimated model is smaller than tolerance </param>
        /// <param name="max_iter">maximum number of iterations of the regression algorithms</param>
        /// <param name="max_diff_from_min_chi2_percent">maximum difference (in percent of the smallest chi2) of chosen set from the smallest chi2</param>
        /// <param name="max_no_min_chi2">maximum number of sets selected to calculate average coefficients</param>
        /// <returns>0 if successful</returns>
        /// <returns>-1 if functionality not yet implemented requested</returns>
        /// <returns>-2 if data set too short</returns>
        /// <returns>-20 if other exception thrown</returns>
        public int EstimateBurstFromLoggers_2Term(Epanet epanet, WaterNetwork waterNetwork, EFavorTest efavorTest, double tolerance_percent, int max_iter, double max_diff_from_min_chi2_percent, int max_no_min_chi2)
        {
            try            
            {
                int ret_val;
                List<Valve> inlet_valves = efavorTest.list_of_inlet_flowmeters.ConvertAll<Valve>(new Converter<FlowMeter, Valve>(FlowMeter.GetValve));
                ret_val = efavorTest.FlowmeterPrvSetpointToPrvSetting();
                if (ret_val < 0)
                    throw new Exception("Error in estimating burst coefficients from measurements, FlowmeterPrvSetpointToPrvSetting returned: " + ret_val.ToString());

                int sim_stop_time = efavorTest.first_measure_time + efavorTest.time_step_pressure_stepping * efavorTest.total_no_of_pressure_steps;
                epanet.SetSimulationStopTime(sim_stop_time);
                epanet.VerifyAndSetSimulationHydraulicTimeStep(efavorTest.time_step_pressure_stepping);

                if (coeffs == null) //if coefficients have not been initialised use efavor test data and assume burst is 70% of MNF
                {
                    InitialiseCoeffs(efavorTest, 70);
                    MessageBox.Show("Initial estimates for burst flow not provided, so it's assumed to be 70% of minimum night flow");
                }
                int nDataPoints = efavorTest.total_no_of_pressure_steps;
                if (nDataPoints < 3)
                {
                    MessageBox.Show("Data set too short. At least 3 measurements are required to estimate 3 coefficients of inlet flow model");
                    return (-2);
                }
                double[] flowData = new double[nDataPoints];
                for (int i = 0; i < nDataPoints; i++)
                {
                    foreach (FlowMeter flowmeter in efavorTest.list_of_inlet_flowmeters) //flow is sum of all inflows
                    {
                        flowData[i] += flowmeter.measured_flow[i];
                    }
                }

                int nLoggers = efavorTest.no_of_loggers;
                List<BurstCoeffs> coeffs_list = new List<BurstCoeffs>();
                double[] chi2_array = new double[nLoggers]; //chi square criterion calculated for each set of burst coefficients

                for (int i = 0; i < nLoggers; i++)
                {
                    coeffs_list.Add(new BurstCoeffs()); //always add new element to coeffs_list (even if continue is called later), so the indices in coeffs_list and in chi2_array match
                    double[] pressureData = efavorTest.list_of_loggers[i].measured_pressure.ToArray();
                    if (pressureData.Any(tmp => tmp < 0)) //can't use negative pressure, make chi^2 infinity for this logger and continue to the next logger
                    {
                        chi2_array[i] = double.PositiveInfinity;
                        continue;
                    }

                    ret_val = Estimate2TermModelCoeffs(pressureData, flowData, tolerance_percent, max_iter);
                    if (ret_val < 0) //if something went wrong make chi^2 infinity for this logger and continue to the next logger
                    {
                        chi2_array[i] = double.PositiveInfinity;
                        continue;
                    }
                    //store just calculated coefficients in the list
                    coeffs_list[i].est_demand = coeffs.est_demand;
                    coeffs_list[i].est_burst_flow = coeffs.est_burst_flow;
                    coeffs_list[i].est_burst_coeff = coeffs.est_burst_coeff;
                    coeffs_list[i].est_burst_exponent = coeffs.est_burst_exponent;

                    //set emitter, simulate network, set emitter back to original value
                    float originalEmitterCoeff = efavorTest.list_of_loggers[i].node.emmitterCoefficient;
                    float originalEmitterExp = waterNetwork.emitterExponent;
                    epanet.SetEmitterParameters(waterNetwork, efavorTest.list_of_loggers[i].node, coeffs.est_burst_coeff, coeffs.est_burst_exponent);
                    epanet.SimulateUpdateHeadsFlows(waterNetwork, inlet_valves, efavorTest.first_measure_time, sim_stop_time, efavorTest.time_step_pressure_stepping);
                    //revert to original emitter setting, 
                    epanet.SetEmitterParameters(waterNetwork, efavorTest.list_of_loggers[i].node, originalEmitterCoeff, originalEmitterExp);

                    double[] simulInflow = new double[nDataPoints];
                    double chi2_this_logger = 0;
                    for (int j = 0; j < nDataPoints; j++) 
                    {
                        foreach (FlowMeter flowmeter in efavorTest.list_of_inlet_flowmeters) //flow is sum of all inflows
                        {
                            simulInflow[j] += flowmeter.valve.link.flow[j];
                        }
                        chi2_this_logger += Math.Pow(simulInflow[j] - flowData[j], 2) / flowData[j];
                    }
                    chi2_array[i] = chi2_this_logger;
                    
                }//for (int i = 0; i < nLoggers; i++)
                double min_chi2 = chi2_array.Min();
                List<double> chi2_list_sorted = new List<double> (chi2_array.ToList());
                chi2_list_sorted.Sort();
                List<double> smallest_chi2 = chi2_list_sorted.FindAll(tmp => (tmp - min_chi2) / min_chi2 * 100 < max_diff_from_min_chi2_percent).Take(max_no_min_chi2).ToList(); //get elements fulfilling the criteria
                List<BurstCoeffs> best_coeffs_list = new List<BurstCoeffs>();
                foreach (double chi2 in smallest_chi2)
                {
                    int tmp_index = chi2_array.ToList().FindIndex(tmp => tmp == chi2);
                    best_coeffs_list.Add(coeffs_list[tmp_index]);
                }
                this.coeffs = new BurstCoeffs(best_coeffs_list);                
                /*int min_chi2_index = chi2_array.ToList().FindIndex(tmp => tmp == min_chi2);
                this.coeffs = coeffs_list[min_chi2_index];*/
                epanet.CloseENHydAnalysis();

            }//try
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return (-20);
            }
            return (0);
        }        
    }

    /// <summary>Class to find burst location by running series of simulations and  
    /// </summary>
    public class BurstLocator
    {
        /// <summary>Allocates burst to a list of node and simulates the network; results (to compare later against measurements) are in efavorTest.list_of_loggers[].node.head
        /// </summary>
        /// <param name="waterNetwork"></param>
        /// <param name="efavorTest"></param>
        /// <param name="epanet"></param>
        /// <param name="node"></param>
        /// <param name="burstCoeffs"></param>
        /// <param name="burst_as_demand">if true burst is allocated as additional demand, otherwise it is allocated as emitter</param>
        /// <returns>0 if successful</returns>
        /// <returns>-1 if burst data length and node list length inconsistent</returns>
        public int SimulateBurstAtNodes(WaterNetwork waterNetwork, EFavorTest efavorTest, Epanet epanet, List<Node> listBurstNodes, List<BurstCoeffs> listBurstCoeffs, bool burst_as_demand)
        {
            //!!!TODO: TEST FOR MULTIPLE BURSTS, TEST FOR BURST AS EMITTER
            if (listBurstCoeffs.Count != listBurstNodes.Count)
            {
                MessageBox.Show("Error: Lengths of node list to simulate burst and burst coefficients list are not equal.");
                return (-1);
            }
            List<Valve> inlet_valves = efavorTest.list_of_inlet_flowmeters.ConvertAll<Valve>(new Converter<FlowMeter, Valve>(FlowMeter.GetValve));
            int sim_stop_time = efavorTest.first_measure_time + efavorTest.time_step_pressure_stepping * efavorTest.total_no_of_pressure_steps;
            
            if (burst_as_demand) //simulate as demand
            {
                List<double[]> listDemands = listBurstCoeffs.ConvertAll<double[]>(new Converter<BurstCoeffs, double[]>(BurstCoeffs.GetEstBurstFlow));
                epanet.SimulateAdditionalDemandUpdateHeadsFlows(waterNetwork, inlet_valves, efavorTest.first_measure_time, sim_stop_time,
                    efavorTest.time_step_pressure_stepping, listBurstNodes, listDemands);
            }
            else //simulate as emitter
            {
                if (listBurstCoeffs.Count > 1)  //TODO!!!: move this message somewhere else if this is to be called in series of simulations
                    MessageBox.Show("Warning: Epanet supports only 1 global emitter exponent; exponent of the last burst coefficient set will be used.");
                for (int i = 0; i < listBurstNodes.Count; i++)
                {
                    epanet.SetEmitterParameters(waterNetwork, listBurstNodes[i], listBurstCoeffs[i].est_burst_coeff, listBurstCoeffs[i].est_burst_exponent);
                }
                epanet.SimulateUpdateHeadsFlows(waterNetwork, inlet_valves, efavorTest.first_measure_time, sim_stop_time, efavorTest.time_step_pressure_stepping);
            }
            return (0);
        }

        
        public int TEMP_ManyBurstsSim(LoggerConnections loggerConnection, EFavorTest efavorTest, Epanet epanet, List<Node> listSuspectedNodes, BurstCoeffs burstCoeffs)
        {
            //MessageBox.Show("TK breakpoint");
            WaterNetwork waterNetwork = loggerConnection.flow_tree.water_network; //retrieve WaterNetwork object from loggerConnection
            int ret_val = efavorTest.FlowmeterPrvSetpointToPrvSetting(); //make sure that PRV setpoints during simulation are as during the Efavor test
            if (ret_val < 0)
                return (-1);
            int sim_stop_time = efavorTest.first_measure_time + efavorTest.time_step_pressure_stepping * efavorTest.total_no_of_pressure_steps;
            epanet.SetSimulationStopTime(sim_stop_time);
            epanet.VerifyAndSetSimulationHydraulicTimeStep(efavorTest.time_step_pressure_stepping);

            int no_of_steps = burstCoeffs.est_burst_flow.Length;
            List<BurstCoeffs> listBurstCoeff = new List<BurstCoeffs>();
            for (int j = 0; j < listSuspectedNodes.Count; j++)
            {
                BurstCoeffs tmp_burst_coeff = new BurstCoeffs();
                tmp_burst_coeff.est_burst_flow = new double[no_of_steps];
                //distribute estimated burst flow evenly to all suspected nodes
                for (int i = 0; i < no_of_steps; i++)
                {
                    tmp_burst_coeff.est_burst_flow[i] = burstCoeffs.est_burst_flow[i] / listSuspectedNodes.Count;
                }
                listBurstCoeff.Add(tmp_burst_coeff);
            }
            ret_val = SimulateBurstAtNodes(waterNetwork, efavorTest, epanet, listSuspectedNodes, listBurstCoeff, true);
            if (ret_val < 0)
                return (-2);
            
            epanet.CloseENHydAnalysis();
            return (0);
        }
        //TODO!!! SimSeries_SingleBurst that will use all pressure steps to calculate d2h, not just min pressure and max pressure, so the output chi2  matrix will be 3D where 3rd dimension will correspond to d2h between different prv steps
        /// <summary>Performs series of simulations and by comparing to Efavor test results calculates chi2 matrix (taking headloss for largest pressure and headloss for lowest pressure, but only from measurement points corresponding to throttling of inlet PRVs from the specified inlet set)
        /// </summary>
        /// <param name="loggerConnection"></param>
        /// <param name="efavorTest"></param>
        /// <param name="epanet"></param>
        /// <param name="listSuspectedNodes"></param>
        /// <param name="burstCoeffs"></param>
        /// <param name="burst_as_demand">if true burst is allocated as additional demand, otherwise it is allocated as emitter</param>
        /// <param name="inlet_set_id">inlet set id specifying which measurement points should be taken to calculate delta2h</param>
        /// <param name="zero_d2h_tolerance">if abs of headloss change (d2h) is smaller than this value then it is considered zero</param>
        /// <param name="chi2matrix">resulting chi2 matrix as jagged array; each row (i) corresponds to different node, each column (j) corresponds to different logger connection, so during subsequent processing columns should be somehow aggregated</param>
        /// <returns>0 if successful</returns>
        /// <returns>-1 if error in FlowmeterPrvSetpointToPrvSetting</returns>
        /// <returns>-2 if error in SimulateBurstAtNodes</returns>
        /// <returns>-3 if error in obtaining delta2h_max</returns>
        /// <returns>-20 if other exception thrown</returns>
        public int SimSeries_SingleBurst_Maxd2h(LoggerConnections loggerConnection, EFavorTest efavorTest, Epanet epanet, List<Node> listSuspectedNodes, BurstCoeffs burstCoeffs, bool burst_as_demand, string inlet_set_id, double zero_d2h_tolerance, out double[][] chi2matrix)
        {
            //TODO!!! RUN THIS AS BACKGROUND WORKER!
            chi2matrix = null;
            try
            {
                int ret_val;
                List<BurstCoeffs> listBurstCoeff = new List<BurstCoeffs>();
                listBurstCoeff.Add(burstCoeffs);
                WaterNetwork waterNetwork = loggerConnection.flow_tree.water_network; //retrieve WaterNetwork object from loggerConnection
                
                ret_val = efavorTest.FlowmeterPrvSetpointToPrvSetting(); //make sure that PRV setpoints during simulation are as during the Efavor test
                if (ret_val < 0)
                    return (-1);
                int sim_stop_time = efavorTest.first_measure_time + efavorTest.time_step_pressure_stepping * efavorTest.total_no_of_pressure_steps;
                epanet.SetSimulationStopTime(sim_stop_time);
                epanet.VerifyAndSetSimulationHydraulicTimeStep(efavorTest.time_step_pressure_stepping);

                int nLoggerConnections = loggerConnection.GetnLoggerConnections();
                List<Tuple<int, int>> listLogToFrom = loggerConnection.GetLogConnFromToList();
                chi2matrix = new double[listSuspectedNodes.Count][];                
                for (int node_count = 0; node_count < listSuspectedNodes.Count; node_count++)
                {
                    chi2matrix[node_count] = new double[nLoggerConnections];
                    List<Node> tmp_listNodes = new List<Node>();
                    tmp_listNodes.Add(listSuspectedNodes[node_count]);
                    ret_val = SimulateBurstAtNodes(waterNetwork, efavorTest, epanet, tmp_listNodes, listBurstCoeff, burst_as_demand);
                    if (ret_val < 0)
                        return (-2);
                    for (int i = 0; i < nLoggerConnections; i++)
                    {
                        Logger logger_from = loggerConnection.list_of_loggers[listLogToFrom[i].Item1];
                        Logger logger_to = loggerConnection.list_of_loggers[listLogToFrom[i].Item2];
                        double d2h_measured, d2h_simulated, dh_measured, dh_simulated;
                        ret_val = loggerConnection.GetDelta2hMax(logger_from, logger_to, efavorTest, inlet_set_id, out d2h_measured, out dh_measured);
                        if (ret_val < 0)
                            return (-3);
                        ret_val = loggerConnection.GetSimulatedDelta2hMax(logger_from, logger_to, efavorTest, inlet_set_id, out d2h_simulated, out dh_simulated);
                        if (ret_val < 0)
                            return (-3);
                        double chi2 = 0;
                        if (Math.Abs(d2h_measured) > zero_d2h_tolerance)
                            chi2 = Math.Pow(d2h_simulated - d2h_measured, 2) / Math.Abs(d2h_measured); //Abs in case d2h_measured was negative, which is possible if flow changed direction (compared to model) due to burst
                        //!!!???should there be 'else' and some other way of calculating this chi2, or should it simply be zero???!!!
                        chi2matrix[node_count][i] = chi2;
                    }   
                }
                epanet.CloseENHydAnalysis();

                return (0);
            }//try
            catch (Exception ex)
            {
                MessageBox.Show("Error in SimSeriesSingleBurst: " + ex.Message);
                return (-20);
            }
        }
    }

}
