using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Management.Automation.Runspaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;

namespace FoundationsEditor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
    public class VirtualNetwork
    {
        public string resourceGroup { get; set; }
        public string vnetName { get; set; }
        public string ipSegment { get; set; }
        public string location { get; set; }
    }
    public class IPSegment
    {
        public string ipSegment { get; set; }
        public int segment0 { get; set; }
        public int segment1 { get; set; }
        public int segment2 { get; set; }
        public int segment3 { get; set; }
        public string cidr { get; set; }
    }
    public class AzureLocation
    {
        public AzureLocation(string _displayName,string _location,string _environment)
        {
            displayName = _displayName;
            location = _location;
            environment = _environment;
        }
        public string displayName { get; set; }
        public string location { get; set; }
        public string environment { get; set; }
    }
    public class Subscription
    {
        public string subscriptionID { get; set; }
        public string environment { get; set; }
        public bool primaryOnly { get; set; }
        public bool autoIPRange { get; set; }
        public string fileName { get; set; }
        public string primaryLocation { get; set; }
        public string primaryResourceGroup { get; set; }
        public string primaryVnetName { get; set; }
        public string primaryIPSegment { get; set; }
        public int ipSeparation { get; set; }
        public string primaryCIDR { get; set; }
        public string secondaryLocation { get; set; }
        public string secondaryResourceGroup { get; set; }
        public string secondaryVnetName { get; set; }
        public string secondaryIPSegment { get; set; }
        public string secondaryCIDR { get; set; }
        public bool createGateway { get; set; }
        public bool createConnection { get; set; }
        public string localConnectionName { get; set; }
        public string localGatewayName { get; set; }
        public string localAddressSpace { get; set; }
        public string edgeIP { get; set; }
        public List<AzureSubnet> primarySubnets { get; set; }
        public List<AzureSubnet> secondarySubnets { get; set; }
    }
    public class AzureSubnet
    {
        public AzureSubnet(string _name, string _ipSegment)
        {
            name = _name;
            ipSegment = _ipSegment;
        }
        public string name { get; set; }
        public string ipSegment { get; set; }
    }
}
