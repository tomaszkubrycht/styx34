using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;


namespace Styx
{
    public class LoggerPaths
    {
        public string loggerConnectionName { get; set; }/// name of logger connection
        public IList<WaterNetwork> paths = new List<WaterNetwork>();/// list of paths
        
        public IList<WaterNetwork> Items
        {
            get
            {
                IList<WaterNetwork> childNodes = new List<WaterNetwork>();
                foreach (var entry in this.paths)
                {
                    childNodes.Add(entry);
                }
                return childNodes;
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public LoggerPaths()
        {
        }
    }
}
