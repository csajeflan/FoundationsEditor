using System;
using System.Deployment;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using Newtonsoft.Json;

namespace FoundationsEditor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CreateSubnets();
            CreateAzureLocations();
            defaultRules();
            txtMessages.ForeColor = Color.Blue;
            txtMessages.BackColor = SystemColors.Control;
            txtMessages.Text = "Enter your Subscription ID or select LOAD to begin.";
            lblBuild.Text = "Build: " + buildNumber + Environment.NewLine + "23-MAR-2018";
            this.Text = "Azure Foundations Editor - " + buildNumber;
        }
        public static string buildNumber = "1.0.2.0";
        public static Subscription currentSubscription = new Subscription();
        List<AzureSubnet> subnets = new List<AzureSubnet>();
        List<AzureLocation> locations = new List<AzureLocation>();
        public static IPSegment primaryIP = new IPSegment();
        public static IPSegment secondaryIP = new IPSegment();
        public static bool complete = false;
        public static string vVpnKey = string.Empty;
        public static bool IsGuid(string guidString)
        {
            bool isValid = false;
            if (!string.IsNullOrEmpty(guidString))
            {
                Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);
                if (isGuid.IsMatch(guidString)) { isValid = true; }
            }
            return isValid;
        }
        public static string defaultNSG = string.Empty;
        private string jLine(int _tab, string _name, string _value, string _end)
        {
            // Construct JSON String
            string cr = Environment.NewLine;
            string qt = "\"";
            string pad = string.Empty;
            string lineIn = pad.PadLeft(_tab * 3, ' ');
            if (_name == "OpenBrace") { lineIn = lineIn + "{" + cr; }
            else
            {
                if (_name == "CloseBrace") { lineIn = lineIn + "}" +_end + cr; }
                else
                {
                    if (_name == null) { lineIn = lineIn + _end + cr; }
                    else
                    {
                        if (_value == null) { lineIn = lineIn + qt + _name + qt + ": " + _end + cr; }
                        else { lineIn = lineIn + qt + _name + qt + ": " + qt + _value + qt + _end + cr; }
                    }
                }
            }
            return lineIn;
        }
        private void defaultRules()
        {
            string cr = Environment.NewLine;
            StringBuilder defRules = new StringBuilder();
            // Create Default NSG rules
            defRules.Append(jLine(4, "defaultSecurityRules", null, "["));
            // Allow VNET Inbound
            defRules.Append(jLine(5, "OpenBrace", null, ""));
            defRules.Append(jLine(6, "name", "AllowVnetInBound", ","));
            defRules.Append(jLine(6, "properties", null, "{"));
            defRules.Append(jLine(7, "description", "Allow inbound traffic from all VMs in VNET", ","));
            defRules.Append(jLine(7, "protocol", "*", ","));
            defRules.Append(jLine(7, "sourcePortRange", "*", ","));
            defRules.Append(jLine(7, "destinationPortRange", "*", ","));
            defRules.Append(jLine(7, "sourceAddressPrefix", "VirtualNetwork", ","));
            defRules.Append(jLine(7, "destinationAddressPrefix", "VirtualNetwork", ","));
            defRules.Append(jLine(7, "access", "Allow", ","));
            defRules.Append(jLine(7, "priority", null, "65000, "));
            defRules.Append(jLine(7, "direction", "Inbound", ","));
            defRules.Append(jLine(7, "sourcePortRanges", null, "[],"));
            defRules.Append(jLine(7, "destinationPortRanges", null, "[],"));
            defRules.Append(jLine(7, "sourceAddressPrefixes", null, "[],"));
            defRules.Append(jLine(7, "destinationAddressPrefixes", null, "[]"));
            defRules.Append(jLine(6, "CloseBrace", null, ""));
            defRules.Append(jLine(5, "CloseBrace", null, ","));
            // Allow Azure Load Balancer Inbound
            defRules.Append(jLine(5, "OpenBrace", null, ""));
            defRules.Append(jLine(6, "name", "AllowAzureLoadBalancerInBound", ","));
            defRules.Append(jLine(6, "properties", null, "{"));
            defRules.Append(jLine(7, "description", "Allow inbound traffic from azure load balancer", ","));
            defRules.Append(jLine(7, "protocol", "*", ","));
            defRules.Append(jLine(7, "sourcePortRange", "*", ","));
            defRules.Append(jLine(7, "destinationPortRange", "*", ","));
            defRules.Append(jLine(7, "sourceAddressPrefix", "AzureLoadBalancer", ","));
            defRules.Append(jLine(7, "destinationAddressPrefix", "*", ","));
            defRules.Append(jLine(7, "access", "Allow", ","));
            defRules.Append(jLine(7, "priority", null, "65001,"));
            defRules.Append(jLine(7, "direction", "Inbound", ","));
            defRules.Append(jLine(7, "sourcePortRanges", null, "[],"));
            defRules.Append(jLine(7, "destinationPortRanges", null, "[],"));
            defRules.Append(jLine(7, "sourceAddressPrefixes", null, "[],"));
            defRules.Append(jLine(7, "destinationAddressPrefixes", null, "[]"));
            defRules.Append(jLine(6, "CloseBrace", null, ""));
            defRules.Append(jLine(5, "CloseBrace", null, ","));
            // Deny All Inbound
            defRules.Append(jLine(5, "OpenBrace", null, ""));
            defRules.Append(jLine(6, "name", "DenyAllInBound", ","));
            defRules.Append(jLine(6, "properties", null, "{"));
            defRules.Append(jLine(7, "description", "Deny all inbound traffic", ","));
            defRules.Append(jLine(7, "protocol", "*", ","));
            defRules.Append(jLine(7, "sourcePortRange", "*", ","));
            defRules.Append(jLine(7, "destinationPortRange", "*", ","));
            defRules.Append(jLine(7, "sourceAddressPrefix", "*", ","));
            defRules.Append(jLine(7, "access", "Deny", ","));
            defRules.Append(jLine(7, "priority", null, "65500,"));
            defRules.Append(jLine(7, "direction", "Inbound", ","));
            defRules.Append(jLine(7, "sourcePortRanges", null, "[],"));
            defRules.Append(jLine(7, "destinationPortRanges", null, "[],"));
            defRules.Append(jLine(7, "sourceAddressPrefixes", null, "[],"));
            defRules.Append(jLine(7, "destinationAddressPrefixes", null, "[]"));
            defRules.Append(jLine(6, "CloseBrace", null, ""));
            defRules.Append(jLine(5, "CloseBrace", null, ","));
            // Allow VNET Outbound
            defRules.Append(jLine(5, "OpenBrace", null, ""));
            defRules.Append(jLine(6, "name", "AllowVnetOutBound", ","));
            defRules.Append(jLine(6, "properties", null, "{"));
            defRules.Append(jLine(7, "description", "Allow outbound traffic from all VMs to all VMs in VNET", ","));
            defRules.Append(jLine(7, "protocol", "*", ","));
            defRules.Append(jLine(7, "sourcePortRange", "*", ","));
            defRules.Append(jLine(7, "destinationPortRange", "*", ","));
            defRules.Append(jLine(7, "sourceAddressPrefix", "VirtualNetwork", ","));
            defRules.Append(jLine(7, "destinationAddressPrefix", "VirtualNetwork", ","));
            defRules.Append(jLine(7, "access", "Allow", ","));
            defRules.Append(jLine(7, "priority", null, "65000,"));
            defRules.Append(jLine(7, "direction", "Outbound", ","));
            defRules.Append(jLine(7, "sourcePortRanges", null, "[],"));
            defRules.Append(jLine(7, "destinationPortRanges", null, "[],"));
            defRules.Append(jLine(7, "sourceAddressPrefixes", null, "[],"));
            defRules.Append(jLine(7, "destinationAddressPrefixes", null, "[]"));
            defRules.Append(jLine(6, "CloseBrace", null, ""));
            defRules.Append(jLine(5, "CloseBrace", null, ","));
            // Allow Internet Outbound
            defRules.Append(jLine(5, "OpenBrace", null, ""));
            defRules.Append(jLine(6, "name", "AllowInternetOutBound", ","));
            defRules.Append(jLine(6, "properties", null, "{"));
            defRules.Append(jLine(7, "description", "Allow outbound traffic from all VMs to Internet", ","));
            defRules.Append(jLine(7, "protocol", "*", ","));
            defRules.Append(jLine(7, "sourcePortRange", "*", ","));
            defRules.Append(jLine(7, "destinationPortRange", "*", ","));
            defRules.Append(jLine(7, "sourceAddressPrefix", "*", ","));
            defRules.Append(jLine(7, "destinationAddressPrefix", "*", ","));
            defRules.Append(jLine(7, "access", "Allow", ","));
            defRules.Append(jLine(7, "priority", null, "65001,"));
            defRules.Append(jLine(7, "direction", "Outbound", ","));
            defRules.Append(jLine(7, "sourcePortRanges", null, "[],"));
            defRules.Append(jLine(7, "destinationPortRanges", null, "[],"));
            defRules.Append(jLine(7, "sourceAddressPrefixes", null, "[],"));
            defRules.Append(jLine(7, "destinationAddressPrefixes", null, "[]"));
            defRules.Append(jLine(6, "CloseBrace", null, ""));
            defRules.Append(jLine(5, "CloseBrace", null, ","));
            // Deny All Outbound
            defRules.Append(jLine(5, "OpenBrace", null, ""));
            defRules.Append(jLine(6, "name", "DenyAllOutBound", ","));
            defRules.Append(jLine(6, "properties", null, "{"));
            defRules.Append(jLine(7, "description", "Deny all outbound traffic", ","));
            defRules.Append(jLine(7, "protocol", "*", ","));
            defRules.Append(jLine(7, "sourcePortRange", "*", ","));
            defRules.Append(jLine(7, "destinationPortRange", "*", ","));
            defRules.Append(jLine(7, "sourceAddressPrefix", "*", ","));
            defRules.Append(jLine(7, "destinationAddressPrefix", "*", ","));
            defRules.Append(jLine(7, "access", "Deny", ","));
            defRules.Append(jLine(7, "priority", null, "65500,"));
            defRules.Append(jLine(7, "direction", "Outbound", ","));
            defRules.Append(jLine(7, "sourcePortRanges", null, "[],"));
            defRules.Append(jLine(7, "destinationPortRanges", null, "[],"));
            defRules.Append(jLine(7, "sourceAddressPrefixes", null, "[],"));
            defRules.Append(jLine(7, "destinationAddressPrefixes", null, "[]"));
            defRules.Append(jLine(6, "CloseBrace", null, ""));
            defRules.Append(jLine(5, "CloseBrace", null, ""));
            defRules.Append(jLine(4, null, null, "]"));
            //defRules.Append("            \"defaultSecurityRules\": [" + cr);
            //defRules.Append("               {" + cr);
            //defRules.Append("                  \"name\": \"AllowVnetInBound\"," + cr);
            //defRules.Append("                  \"properties\": {" + cr);
            //defRules.Append("                     \"description\": \"Allow inbound traffic from all VMs in VNET\"," + cr);
            //defRules.Append("                     \"protocol\": \"*\"," + cr);
            //defRules.Append("                     \"sourcePortRange\": \"*\"," + cr);
            //defRules.Append("                     \"destinationPortRange\": \"*\"," + cr);
            //defRules.Append("                     \"sourceAddressPrefix\": \"VirtualNetwork\"," + cr);
            //defRules.Append("                     \"destinationAddressPrefix\": \"VirtualNetwork\"," + cr);
            //defRules.Append("                     \"access\": \"Allow\"," + cr);
            //defRules.Append("                     \"priority\": 65000," + cr);
            //defRules.Append("                     \"direction\": \"Inbound\"," + cr);
            //defRules.Append("                     \"sourcePortRanges\": []," + cr);
            //defRules.Append("                     \"destinationPortRanges\": []," + cr);
            //defRules.Append("                     \"sourceAddressPrefixes\": []," + cr);
            //defRules.Append("                     \"destinationAddressPrefixes\": []" + cr);
            //defRules.Append("                  }" + cr);
            //defRules.Append("               }," + cr);
            //defRules.Append("               {" + cr);
            //defRules.Append("                  \"name\": \"AllowAzureLoadBalancerInBound\"," + cr);
            //defRules.Append("                  \"properties\": {" + cr);
            //defRules.Append("                     \"description\": \"Allow inbound traffic from azure load balancer\"," + cr);
            //defRules.Append("                     \"protocol\": \"*\"," + cr);
            //defRules.Append("                     \"sourcePortRange\": \"*\"," + cr);
            //defRules.Append("                     \"destinationPortRange\": \"*\"," + cr);
            //defRules.Append("                     \"sourceAddressPrefix\": \"AzureLoadBalancer\"," + cr);
            //defRules.Append("                     \"destinationAddressPrefix\": \"*\"," + cr);
            //defRules.Append("                     \"access\": \"Allow\"," + cr);
            //defRules.Append("                     \"priority\": 65001," + cr);
            //defRules.Append("                     \"direction\": \"Inbound\"," + cr);
            //defRules.Append("                     \"sourcePortRanges\": []," + cr);
            //defRules.Append("                     \"destinationPortRanges\": []," + cr);
            //defRules.Append("                     \"sourceAddressPrefixes\": []," + cr);
            //defRules.Append("                     \"destinationAddressPrefixes\": []" + cr);
            //defRules.Append("                  }" + cr);
            //defRules.Append("               }," + cr);
            //defRules.Append("               {" + cr);
            //defRules.Append("                  \"name\": \"DenyAllInBound\"," + cr);
            //defRules.Append("                  \"properties\": {" + cr);
            //defRules.Append("                     \"description\": \"Deny all inbound traffic\"," + cr);
            //defRules.Append("                     \"protocol\": \"*\"," + cr);
            //defRules.Append("                     \"sourcePortRange\": \"*\"," + cr);
            //defRules.Append("                     \"destinationPortRange\": \"*\"," + cr);
            //defRules.Append("                     \"sourceAddressPrefix\": \"*\"," + cr);
            //defRules.Append("                     \"destinationAddressPrefix\": \"*\"," + cr);
            //defRules.Append("                     \"access\": \"Deny\"," + cr);
            //defRules.Append("                     \"priority\": 65500," + cr);
            //defRules.Append("                     \"direction\": \"Inbound\"," + cr);
            //defRules.Append("                     \"sourcePortRanges\": []," + cr);
            //defRules.Append("                     \"destinationPortRanges\": []," + cr);
            //defRules.Append("                     \"sourceAddressPrefixes\": []," + cr);
            //defRules.Append("                     \"destinationAddressPrefixes\": []" + cr);
            //defRules.Append("                  }" + cr);
            //defRules.Append("               }," + cr);
            //defRules.Append("               {" + cr);
            //defRules.Append("                  \"name\": \"AllowVnetOutBound\"," + cr);
            //defRules.Append("                  \"properties\": {" + cr);
            //defRules.Append("                     \"description\": \"Allow outbound traffic from all VMs to all VMs in VNET\"," + cr);
            //defRules.Append("                     \"protocol\": \"*\"," + cr);
            //defRules.Append("                     \"sourcePortRange\": \"*\"," + cr);
            //defRules.Append("                     \"destinationPortRange\": \"*\"," + cr);
            //defRules.Append("                     \"sourceAddressPrefix\": \"VirtualNetwork\"," + cr);
            //defRules.Append("                     \"destinationAddressPrefix\": \"VirtualNetwork\"," + cr);
            //defRules.Append("                     \"access\": \"Allow\"," + cr);
            //defRules.Append("                     \"priority\": 65000," + cr);
            //defRules.Append("                     \"direction\": \"Outbound\"," + cr);
            //defRules.Append("                     \"sourcePortRanges\": []," + cr);
            //defRules.Append("                     \"destinationPortRanges\": []," + cr);
            //defRules.Append("                     \"sourceAddressPrefixes\": []," + cr);
            //defRules.Append("                     \"destinationAddressPrefixes\": []" + cr);
            //defRules.Append("                  }" + cr);
            //defRules.Append("               }," + cr);
            //defRules.Append("               {" + cr);
            //defRules.Append("                  \"name\": \"AllowInternetOutBound\"," + cr);
            //defRules.Append("                  \"properties\": {" + cr);
            //defRules.Append("                     \"description\": \"Allow outbound traffic from all VMs to Internet\"," + cr);
            //defRules.Append("                     \"protocol\": \"*\"," + cr);
            //defRules.Append("                     \"sourcePortRange\": \"*\"," + cr);
            //defRules.Append("                     \"destinationPortRange\": \"*\"," + cr);
            //defRules.Append("                     \"sourceAddressPrefix\": \"*\"," + cr);
            //defRules.Append("                     \"destinationAddressPrefix\": \"Internet\"," + cr);
            //defRules.Append("                     \"access\": \"Allow\"," + cr);
            //defRules.Append("                     \"priority\": 65001," + cr);
            //defRules.Append("                     \"direction\": \"Outbound\"," + cr);
            //defRules.Append("                     \"sourcePortRanges\": []," + cr);
            //defRules.Append("                     \"destinationPortRanges\": []," + cr);
            //defRules.Append("                     \"sourceAddressPrefixes\": []," + cr);
            //defRules.Append("                     \"destinationAddressPrefixes\": []" + cr);
            //defRules.Append("                  }" + cr);
            //defRules.Append("               }," + cr);
            //defRules.Append("               {" + cr);
            //defRules.Append("                  \"name\": \"DenyAllOutBound\"," + cr);
            //defRules.Append("                  \"properties\": {" + cr);
            //defRules.Append("                     \"description\": \"Deny all outbound traffic\"," + cr);
            //defRules.Append("                     \"protocol\": \"*\"," + cr);
            //defRules.Append("                     \"sourcePortRange\": \"*\"," + cr);
            //defRules.Append("                     \"destinationPortRange\": \"*\"," + cr);
            //defRules.Append("                     \"sourceAddressPrefix\": \"*\"," + cr);
            //defRules.Append("                     \"destinationAddressPrefix\": \"*\"," + cr);
            //defRules.Append("                     \"access\": \"Deny\"," + cr);
            //defRules.Append("                     \"priority\": 65500," + cr);
            //defRules.Append("                     \"direction\": \"Outbound\"," + cr);
            //defRules.Append("                     \"sourcePortRanges\": []," + cr);
            //defRules.Append("                     \"destinationPortRanges\": []," + cr);
            //defRules.Append("                     \"sourceAddressPrefixes\": []," + cr);
            //defRules.Append("                     \"destinationAddressPrefixes\": []" + cr);
            //defRules.Append("                  }" + cr);
            //defRules.Append("               }" + cr);
            //defRules.Append("            ]" + cr);
            defaultNSG = defRules.ToString();
        }
        private string CreateNSGRules(int _scr)
        {
            string cr = System.Environment.NewLine;
            StringBuilder nsg = new StringBuilder();
            switch (_scr)
            {
                case 1:     // DMZ rules
                    nsg.Append(jLine(4, "securityRules", null, "["));
                    // Port 80 HTTP
                    nsg.Append(jLine(5, "OpenBrace", null, ""));
                    nsg.Append(jLine(6, "name", "Port_80_HTTP", ","));
                    nsg.Append(jLine(6, "properties", null, "{"));
                    nsg.Append(jLine(7, "description", "Allow inbound internet HTTP traffic", ","));
                    nsg.Append(jLine(7, "protocol", "*", ","));
                    nsg.Append(jLine(7, "sourcePortRange", "*", ","));
                    nsg.Append(jLine(7, "destinationPortRange", "80", ","));
                    nsg.Append(jLine(7, "sourceAddressPrefix", "Internet", ","));
                    nsg.Append(jLine(7, "destinationAddressPrefix", "*", ","));
                    nsg.Append(jLine(7, "access", "Allow", ","));
                    nsg.Append(jLine(7, "priority", null, "100,"));
                    nsg.Append(jLine(7, "direction", "Inbound", ","));
                    nsg.Append(jLine(7, "sourcePortRanges", null, "[],"));
                    nsg.Append(jLine(7, "destinationPortRanges", null, "[],"));
                    nsg.Append(jLine(7, "sourceAddressPrefixes", null, "[],"));
                    nsg.Append(jLine(7, "destinationAddressPrefixes", null, "[]"));
                    nsg.Append(jLine(6, "CloseBrace", null, ""));
                    nsg.Append(jLine(5, "CloseBrace", null, ","));
                    // Port 443 HTTPS
                    nsg.Append(jLine(5, "OpenBrace", null, ""));
                    nsg.Append(jLine(6, "name", "Port_443_HTTPS", ","));
                    nsg.Append(jLine(6, "properties", null, "{"));
                    nsg.Append(jLine(7, "description", "Allow inbound internet HTTPS traffic", ","));
                    nsg.Append(jLine(7, "protocol", "*", ","));
                    nsg.Append(jLine(7, "sourcePortRange", "*", ","));
                    nsg.Append(jLine(7, "destinationPortRange", "443", ","));
                    nsg.Append(jLine(7, "sourceAddressPrefix", "Internet", ","));
                    nsg.Append(jLine(7, "destinationAddressPrefix", "*", ","));
                    nsg.Append(jLine(7, "access", "Allow", ","));
                    nsg.Append(jLine(7, "priority", null, "101,"));
                    nsg.Append(jLine(7, "direction", "Inbound", ","));
                    nsg.Append(jLine(7, "sourcePortRanges", null, "[],"));
                    nsg.Append(jLine(7, "destinationPortRanges", null, "[],"));
                    nsg.Append(jLine(7, "sourceAddressPrefixes", null, "[],"));
                    nsg.Append(jLine(7, "destinationAddressPrefixes", null, "[]"));
                    nsg.Append(jLine(6, "CloseBrace", null, ""));
                    nsg.Append(jLine(5, "CloseBrace", null, ","));
                    // Port 3389 RDP
                    nsg.Append(jLine(5, "OpenBrace", null, ""));
                    nsg.Append(jLine(6, "name", "Port_3389_RDP", ","));
                    nsg.Append(jLine(6, "properties", null, "{"));
                    nsg.Append(jLine(7, "description", "Allow inbound internet RDP traffic", ","));
                    nsg.Append(jLine(7, "protocol", "*", ","));
                    nsg.Append(jLine(7, "sourcePortRange", "*", ","));
                    nsg.Append(jLine(7, "destinationPortRange", "3389", ","));
                    nsg.Append(jLine(7, "sourceAddressPrefix", "Internet", ","));
                    nsg.Append(jLine(7, "destinationAddressPrefix", "*", ","));
                    nsg.Append(jLine(7, "access", "Allow", ","));
                    nsg.Append(jLine(7, "priority", null, "999,"));
                    nsg.Append(jLine(7, "direction", "Inbound", ","));
                    nsg.Append(jLine(7, "sourcePortRanges", null, "[],"));
                    nsg.Append(jLine(7, "destinationPortRanges", null, "[],"));
                    nsg.Append(jLine(7, "sourceAddressPrefixes", null, "[],"));
                    nsg.Append(jLine(7, "destinationAddressPrefixes", null, "[]"));
                    nsg.Append(jLine(6, "CloseBrace", null, ""));
                    nsg.Append(jLine(5, "CloseBrace", null, ","));
                    nsg.Append(jLine(4, null, null, "],"));
                    break;
                case 2:     // NVA rules
                    nsg.Append(jLine(4, "securityRules", null, "["));
                    nsg.Append(jLine(5, "OpenBrace", null, ""));
                    nsg.Append(jLine(6, "name", "Port_Any", ","));
                    nsg.Append(jLine(6, "properties", null, "{"));
                    nsg.Append(jLine(7, "description", "Allow inbound internet traffic", ","));
                    nsg.Append(jLine(7, "protocol", "*", ","));
                    nsg.Append(jLine(7, "sourcePortRange", "*", ","));
                    nsg.Append(jLine(7, "destinationPortRange", "*", ","));
                    nsg.Append(jLine(7, "sourceAddressPrefix", "Internet", ","));
                    nsg.Append(jLine(7, "destinationAddressPrefix", "*", ","));
                    nsg.Append(jLine(7, "access", "Allow", ","));
                    nsg.Append(jLine(7, "priority", null, "100,"));
                    nsg.Append(jLine(7, "direction", "Inbound", ","));
                    nsg.Append(jLine(7, "sourcePortRanges", null, "[],"));
                    nsg.Append(jLine(7, "destinationPortRanges", null, "[],"));
                    nsg.Append(jLine(7, "sourceAddressPrefixes", null, "[],"));
                    nsg.Append(jLine(7, "destinationAddressPrefixes", null, "[]"));
                    nsg.Append(jLine(6, "CloseBrace", null, ""));
                    nsg.Append(jLine(5, "CloseBrace", null, ","));
                    nsg.Append(jLine(4, null, null, "],"));
                    break;
                case 3:     // Default rules
                case 4:
                case 5:
                case 6:
                case 7:
                    nsg.Append(jLine(4, "securityRules", null, "[],"));
                    break;
            }
            if (_scr != 0) { nsg.Append(defaultNSG); }
            nsg.Append(jLine(3, "CloseBrace", null, ""));
            nsg.Append(jLine(2, "CloseBrace", null, ","));
            return nsg.ToString();
        }
        private string CreateNSG(int _scr)
        {
            string cr = Environment.NewLine;
            StringBuilder nsgTemplate = new StringBuilder();
            nsgTemplate.Append(jLine(0, "OpenBrace", null, ""));
            nsgTemplate.Append(jLine(1, "$schema", "https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#", ","));
            nsgTemplate.Append(jLine(1, "contentVersion", "1.0.0.0", ","));
            nsgTemplate.Append(jLine(1, "parameters", null, "{},"));
            nsgTemplate.Append(jLine(1, "variables", null, "{},"));
            nsgTemplate.Append(jLine(1, "resources", null, "["));
            switch (_scr)
            {
                case 1:     // Primary Location NSGs
                    for (int ctr = 1; ctr < 8; ctr++)
                    {
                        nsgTemplate.Append(jLine(2, "OpenBrace", null, ""));
                        nsgTemplate.Append(jLine(3, "type", "Microsoft.Network/networkSecurityGroups", ","));
                        nsgTemplate.Append(jLine(3, "name", "nsg-" + currentSubscription.primarySubnets[ctr].name + "-" + currentSubscription.primaryLocation, ","));
                        nsgTemplate.Append(jLine(3, "apiVersion", "2018-01-01", ","));
                        nsgTemplate.Append(jLine(3, "location", "[resourceGroup().location]", ","));
                        nsgTemplate.Append(jLine(3, "properties", null, "{"));
                        nsgTemplate.Append(CreateNSGRules(ctr));
                    }
                    nsgTemplate.Append(jLine(3, "OpenBrace", null, ""));
                    nsgTemplate.Append(jLine(4, "type", "Microsoft.Network/virtualNetworks", ","));
                    nsgTemplate.Append(jLine(4, "name", currentSubscription.primaryVnetName, ","));
                    nsgTemplate.Append(jLine(4, "location", "[resourceGroup().location]", ","));
                    nsgTemplate.Append(jLine(4, "apiVersion", "2018-01-01", ","));
                    nsgTemplate.Append(jLine(4, "properties", null, "{"));
                    nsgTemplate.Append(jLine(5, "addressSpace", null, "{"));
                    nsgTemplate.Append(jLine(6, "addressPrefixes", null, "["));
                    nsgTemplate.Append(jLine(7, null, null, "\"" + currentSubscription.primaryIPSegment + "\""));
                    nsgTemplate.Append(jLine(6, null, null, "]"));
                    nsgTemplate.Append(jLine(5, null, null, "},"));
                    nsgTemplate.Append(jLine(4, "subnets", null, "["));
                    for (int idx = 0; idx < 8; idx++)
                    {
                        nsgTemplate.Append(jLine(5, "OpenBrace", null, ""));
                        nsgTemplate.Append(jLine(6, "name", currentSubscription.primarySubnets[idx].name, ","));
                        nsgTemplate.Append(jLine(6, "properties", null, "{"));
                        if (idx > 0)
                        {
                            nsgTemplate.Append(jLine(7, "addressPrefix", currentSubscription.primarySubnets[idx].ipSegment, ","));
                            nsgTemplate.Append(jLine(7, "networkSecurityGroup", null, "{"));
                            nsgTemplate.Append(jLine(8, "id", "[resourceId('Microsoft.Network/networkSecurityGroups', 'nsg-" + currentSubscription.primarySubnets[idx].name + "-" + currentSubscription.primaryLocation+ "')]",""));
                            nsgTemplate.Append(jLine(7, "CloseBrace", null, ""));
                        }
                        else { nsgTemplate.Append(jLine(7, "addressPrefix", currentSubscription.primarySubnets[idx].ipSegment, "")); }
                        nsgTemplate.Append(jLine(6, "CloseBrace", null, ""));
                        if (idx == 7) { nsgTemplate.Append(jLine(5, "CloseBrace", null, "")); }
                        else { nsgTemplate.Append(jLine(5, "CloseBrace", null, ",")); }
                    }
                    nsgTemplate.Append(jLine(4, null, null, "]"));
                    nsgTemplate.Append(jLine(3, "CloseBrace", null, ","));
                    nsgTemplate.Append(jLine(3, "dependsOn", null, "["));
                    for (int idx = 1; idx < 7; idx++)
                    {
                        nsgTemplate.Append(jLine(4, null, null, "\"[resourceId('Microsoft.Network/networkSecurityGroups', 'nsg-" + currentSubscription.primarySubnets[idx].name + "-" + currentSubscription.primaryLocation + "')]\","));
                    }
                    nsgTemplate.Append(jLine(4, null, null, "\"[resourceId('Microsoft.Network/networkSecurityGroups', 'nsg-" + currentSubscription.primarySubnets[7].name + "-" + currentSubscription.primaryLocation + "')]\""));
                    nsgTemplate.Append(jLine(3, null, null, "]"));
                    nsgTemplate.Append(jLine(2, "CloseBrace", null, ""));
                    break;
                case 2:     // Secondary Location NSGs
                    for (int ctr = 1; ctr < 8; ctr++)
                    {
                        nsgTemplate.Append(jLine(2, "OpenBrace", null, ""));
                        nsgTemplate.Append(jLine(3, "type", "Microsoft.Network/networkSecurityGroups", ","));
                        nsgTemplate.Append(jLine(3, "name", "nsg-" + currentSubscription.secondarySubnets[ctr].name + "-" + currentSubscription.secondaryLocation, ","));
                        nsgTemplate.Append(jLine(3, "apiVersion", "2018-01-01", ","));
                        nsgTemplate.Append(jLine(3, "location", "[resourceGroup().location]", ","));
                        nsgTemplate.Append(jLine(3, "properties", null, "{"));
                        nsgTemplate.Append(CreateNSGRules(ctr));
                    }
                    nsgTemplate.Append(jLine(3, "OpenBrace", null, ""));
                    nsgTemplate.Append(jLine(4, "type", "Microsoft.Network/virtualNetworks", ","));
                    nsgTemplate.Append(jLine(4, "name", currentSubscription.secondaryVnetName, ","));
                    nsgTemplate.Append(jLine(4, "location", "[resourceGroup().location]", ","));
                    nsgTemplate.Append(jLine(4, "apiVersion", "2018-01-01", ","));
                    nsgTemplate.Append(jLine(4, "properties", null, "{"));
                    nsgTemplate.Append(jLine(5, "addressSpace", null, "{"));
                    nsgTemplate.Append(jLine(6, "addressPrefixes", null, "["));
                    nsgTemplate.Append(jLine(7, null, null, "\"" + currentSubscription.secondaryIPSegment + "\""));
                    nsgTemplate.Append(jLine(6, null, null, "]"));
                    nsgTemplate.Append(jLine(5, null, null, "},"));
                    nsgTemplate.Append(jLine(4, "subnets", null, "["));
                    for (int idx = 0; idx < 8; idx++)
                    {
                        nsgTemplate.Append(jLine(5, "OpenBrace", null, ""));
                        nsgTemplate.Append(jLine(6, "name", currentSubscription.secondarySubnets[idx].name, ","));
                        nsgTemplate.Append(jLine(6, "properties", null, "{"));
                        if (idx > 0)
                        {
                            nsgTemplate.Append(jLine(7, "addressPrefix", currentSubscription.secondarySubnets[idx].ipSegment, ","));
                            nsgTemplate.Append(jLine(7, "networkSecurityGroup", null, "{"));
                            nsgTemplate.Append(jLine(8, "id", "[resourceId('Microsoft.Network/networkSecurityGroups', 'nsg-" + currentSubscription.secondarySubnets[idx].name + "-" + currentSubscription.secondaryLocation + "')]", ""));
                            nsgTemplate.Append(jLine(7, "CloseBrace", null, ""));
                        }
                        else { nsgTemplate.Append(jLine(7, "addressPrefix", currentSubscription.secondarySubnets[idx].ipSegment, "")); }
                        nsgTemplate.Append(jLine(6, "CloseBrace", null, ""));
                        if (idx == 7) { nsgTemplate.Append(jLine(5, "CloseBrace", null, "")); }
                        else { nsgTemplate.Append(jLine(5, "CloseBrace", null, ",")); }
                    }
                    nsgTemplate.Append(jLine(4, null, null, "]"));
                    nsgTemplate.Append(jLine(3, "CloseBrace", null, ","));
                    nsgTemplate.Append(jLine(3, "dependsOn", null, "["));
                    for (int idx = 1; idx < 7; idx++)
                    {
                        nsgTemplate.Append(jLine(4, null, null, "\"[resourceId('Microsoft.Network/networkSecurityGroups', 'nsg-" + currentSubscription.secondarySubnets[idx].name + "-" + currentSubscription.secondaryLocation + "')]\","));
                    }
                    nsgTemplate.Append(jLine(4, null, null, "\"[resourceId('Microsoft.Network/networkSecurityGroups', 'nsg-" + currentSubscription.secondarySubnets[7].name + "-" + currentSubscription.secondaryLocation + "')]\""));
                    nsgTemplate.Append(jLine(3, null, null, "]"));
                    nsgTemplate.Append(jLine(2, "CloseBrace", null, ""));
                    break;
            }
            nsgTemplate.Append(jLine(1, null, null, "]"));
            nsgTemplate.Append(jLine(0, "CloseBrace", null, ""));
            return nsgTemplate.ToString();
        }
        private string CreateARMTemplate(int _scr)
        {
            string cr = Environment.NewLine;
            string armTemplate = string.Empty;
            StringBuilder at = new StringBuilder();
            switch (_scr)
            {
                case 1:     //Primary Location
                    at.Append(jLine(0, "OpenBrace", null, ""));
                    at.Append(jLine(1, "$schema", "http://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#", ","));
                    at.Append(jLine(1, "contentVersion", "1.0.0.0", ","));
                    at.Append(jLine(1, "parameters", null, "{ },"));
                    at.Append(jLine(1, "variables", null, "{"));
                    at.Append(jLine(2, "vnetId", "[resourceId('Microsoft.Network/virtualNetworks','" + currentSubscription.primaryVnetName + "')]", ","));
                    at.Append(jLine(2, "gatewaySubnetRef", "[concat(variables('vnetID'),'/subnets/','GatewaySubnet')]", ""));
                    at.Append(jLine(1, "CloseBrace", null, ","));
                    at.Append(jLine(1, "resources", null, "["));
                    // Create Virtual Network
                    at.Append(jLine(2, "OpenBrace", null, ""));
                    at.Append(jLine(3, "name", currentSubscription.primaryVnetName, ","));
                    at.Append(jLine(3, "type", "Microsoft.Network/virtualNetworks", ","));
                    at.Append(jLine(3, "apiVersion", "2018-01-01", ","));
                    at.Append(jLine(3, "location", "[resourceGroup().location]", ","));
                    at.Append(jLine(3, "properties", null, "{"));
                    at.Append(jLine(4, "addressSpace", null, "{"));
                    at.Append(jLine(5, "addressPrefixes", null, "["));
                    at.Append(jLine(6, null, null, "\"" + currentSubscription.primaryIPSegment + "\""));
                    at.Append(jLine(5, null, null, "]"));
                    at.Append(jLine(4, "CloseBrace", null, ","));
                    at.Append(jLine(4, "subnets", null, "["));
                    for (int ctr = 0; ctr < 8; ctr++)
                    {
                        if (currentSubscription.primarySubnets[ctr].ipSegment != string.Empty)
                        {
                            at.Append(jLine(5, "OpenBrace", null, ""));
                            at.Append(jLine(6, "name", currentSubscription.primarySubnets[ctr].name, ","));
                            at.Append(jLine(6, "properties", null, "{"));
                            at.Append(jLine(7, "addressPrefix", currentSubscription.primarySubnets[ctr].ipSegment, ""));
                            at.Append(jLine(6, "CloseBrace", null, ""));
                            if (ctr == 7) { at.Append(jLine(5, "CloseBrace", null, "")); }
                            else { at.Append(jLine(5, "CloseBrace", null, ",")); }
                        }
                    }
                    at.Append(jLine(4, null, null, "]"));
                    at.Append(jLine(3, "CloseBrace", null, ""));
                    at.Append(jLine(2, "CloseBrace", null, ","));
                    if (currentSubscription.createGateway)
                    {
                        // Create Local Gateway
                        at.Append(jLine(2, "OpenBrace", null, ""));
                        at.Append(jLine(3, "name", currentSubscription.localGatewayName + "-" + currentSubscription.primaryLocation, ","));
                        at.Append(jLine(3, "type", "Microsoft.Network/localNetworkGateways", ","));
                        at.Append(jLine(3, "apiVersion", "2018-01-01", ","));
                        at.Append(jLine(3, "location", "[resourceGroup().location]", ","));
                        at.Append(jLine(3, "properties", null, "{"));
                        at.Append(jLine(4, "localNetworkAddressSpace", null, "{"));
                        at.Append(jLine(5, "addressPrefixes", null, "["));
                        at.Append(jLine(6, null, currentSubscription.localAddressSpace, ""));
                        at.Append(jLine(5, null, null, "]"));
                        at.Append(jLine(4, "CloseBrace", null, ","));
                        at.Append(jLine(4, "gatewayIpAddress", currentSubscription.edgeIP, ""));
                        at.Append(jLine(3, "CloseBrace", null, ""));
                        at.Append(jLine(2, "CloseBrace", null, ","));
                        // Create Public IP Address
                        at.Append(jLine(2, "OpenBrace", null, ""));
                        at.Append(jLine(3, "name", currentSubscription.primaryVnetName + "-gw-ip", ","));
                        at.Append(jLine(3, "type", "Microsoft.Network/publicIPAddresses", ","));
                        at.Append(jLine(3, "apiVersion", "2018-01-01", ","));
                        at.Append(jLine(3, "location", "[resourceGroup().location]", ","));
                        at.Append(jLine(3, "properties", null, "{"));
                        at.Append(jLine(4, "publicIPAllocationMethod", "Dynamic", ""));
                        at.Append(jLine(3, "CloseBrace", null, ""));
                        at.Append(jLine(3, "CloseBrace", null, ","));
                        // Create Virtual Network Gateway
                        at.Append(jLine(3, "OpenBrace", null, ""));
                        at.Append(jLine(4, "name", currentSubscription.primaryVnetName + "-gw", ","));
                        at.Append(jLine(4, "type", "Microsoft.Network/virtualNetworkGateways", ","));
                        at.Append(jLine(4, "apiVersion", "2018-01-01", ","));
                        at.Append(jLine(4, "location", "[resourceGroup().location]", ","));
                        at.Append(jLine(4, "dependsOn", null, "["));
                        at.Append(jLine(5, null, null, "\"[concat('Microsoft.Network/publicIpAddresses/','" + currentSubscription.primaryVnetName + "-gw-ip')]" + "\","));
                        at.Append(jLine(5, null, null, "\"[concat('Microsoft.Network/virtualNetworks/','" + currentSubscription.primaryVnetName + "')]" + "\""));
                        at.Append(jLine(4, null, null, "],"));
                        at.Append(jLine(4, "properties", null, "{"));
                        at.Append(jLine(5, "ipConfigurations", null, "["));
                        at.Append(jLine(6, "OpenBrace", null, ""));
                        at.Append(jLine(7, "properties", null, "{"));
                        at.Append(jLine(8, "privateIPAllocationMethod", "Dynamic", ","));
                        at.Append(jLine(8, "subnet", null, "{"));
                        at.Append(jLine(9, "id", "[variables('gatewaySubnetRef')]", ""));
                        at.Append(jLine(8, "CloseBrace", null, ","));
                        at.Append(jLine(8, "publicIPAddress", null, "{"));
                        at.Append(jLine(9, "id", "[resourceId('Microsoft.Network/publicIPAddresses','" + currentSubscription.primaryVnetName + "-gw-ip')]", ""));
                        at.Append(jLine(8, "CloseBrace", null, ""));
                        at.Append(jLine(7, "CloseBrace", null, ","));
                        at.Append(jLine(7, "name", "vnetGatewayConfig", ""));
                        at.Append(jLine(6, "CloseBrace", null, ""));
                        at.Append(jLine(5, null, null, "],"));
                        at.Append(jLine(5, "gatewayType", "Vpn", ","));
                        at.Append(jLine(5, "vpnType", "RouteBased", ","));
                        at.Append(jLine(5, "enableBgp", null, "false,"));
                        at.Append(jLine(5, "sku", null, "{"));
                        at.Append(jLine(6, "name", "VpnGw1", ","));
                        at.Append(jLine(6, "tier", "VpnGw1", ""));
                        at.Append(jLine(5, "CloseBrace", null, ""));
                        at.Append(jLine(4, "CloseBrace", null, ""));
                        at.Append(jLine(3, "CloseBrace", null, ""));
                    }
                    else
                    {
                        at.Append(jLine(2, "CloseBrace", null, ""));
                    }
                    at.Append(jLine(1, null, null, "]"));
                    at.Append(jLine(0, "CloseBrace", null, ""));
                    break;
                case 2:     // Secondary Location
                    at.Append(jLine(0, "OpenBrace", null, ""));
                    at.Append(jLine(1, "$schema", "http://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#", ","));
                    at.Append(jLine(1, "contentVersion", "1.0.0.0", ","));
                    at.Append(jLine(1, "parameters", null, "{ },"));
                    at.Append(jLine(1, "variables", null, "{"));
                    at.Append(jLine(2, "vnetId", "[resourceId('Microsoft.Network/virtualNetworks','" + currentSubscription.secondaryVnetName + "')]", ","));
                    at.Append(jLine(2, "gatewaySubnetRef", "[concat(variables('vnetID'),'/subnets/','GatewaySubnet')]", ""));
                    at.Append(jLine(1, "CloseBrace", null, ","));
                    at.Append(jLine(1, "resources", null, "["));
                    // Create Virtual Network
                    at.Append(jLine(2, "OpenBrace", null, ""));
                    at.Append(jLine(3, "name", currentSubscription.secondaryVnetName, ","));
                    at.Append(jLine(3, "type", "Microsoft.Network/virtualNetworks", ","));
                    at.Append(jLine(3, "apiVersion", "2018-01-01", ","));
                    at.Append(jLine(3, "location", "[resourceGroup().location]", ","));
                    at.Append(jLine(3, "properties", null, "{"));
                    at.Append(jLine(4, "addressSpace", null, "{"));
                    at.Append(jLine(5, "addressPrefixes", null, "["));
                    at.Append(jLine(6, null, null, "\"" + currentSubscription.secondaryIPSegment + "\""));
                    at.Append(jLine(5, null, null, "]"));
                    at.Append(jLine(4, "CloseBrace", null, ","));
                    at.Append(jLine(4, "subnets", null, "["));
                    for (int ctr = 0; ctr < 8; ctr++)
                    {
                        if (currentSubscription.secondarySubnets[ctr].ipSegment != string.Empty)
                        {
                            at.Append(jLine(5, "OpenBrace", null, ""));
                            at.Append(jLine(6, "name", currentSubscription.secondarySubnets[ctr].name, ","));
                            at.Append(jLine(6, "properties", null, "{"));
                            at.Append(jLine(7, "addressPrefix", currentSubscription.secondarySubnets[ctr].ipSegment, ""));
                            at.Append(jLine(6, "CloseBrace", null, ""));
                            if (ctr == 7) { at.Append(jLine(5, "CloseBrace", null, "")); }
                            else { at.Append(jLine(5, "CloseBrace", null, ",")); }
                        }
                    }
                    at.Append(jLine(4, null, null, "]"));
                    at.Append(jLine(3, "CloseBrace", null, ""));
                    at.Append(jLine(2, "CloseBrace", null, ","));
                    if (currentSubscription.createGateway)
                    {
                        // Create Local Gateway
                        at.Append(jLine(2, "OpenBrace", null, ""));
                        at.Append(jLine(3, "name", currentSubscription.localGatewayName + "-" + currentSubscription.secondaryLocation, ","));
                        at.Append(jLine(3, "type", "Microsoft.Network/localNetworkGateways", ","));
                        at.Append(jLine(3, "apiVersion", "2018-01-01", ","));
                        at.Append(jLine(3, "location", "[resourceGroup().location]", ","));
                        at.Append(jLine(3, "properties", null, "{"));
                        at.Append(jLine(4, "localNetworkAddressSpace", null, "{"));
                        at.Append(jLine(5, "addressPrefixes", null, "["));
                        at.Append(jLine(6, null, currentSubscription.localAddressSpace, ""));
                        at.Append(jLine(5, null, null, "]"));
                        at.Append(jLine(4, "CloseBrace", null, ","));
                        at.Append(jLine(4, "gatewayIpAddress", currentSubscription.edgeIP, ""));
                        at.Append(jLine(3, "CloseBrace", null, ""));
                        at.Append(jLine(2, "CloseBrace", null, ","));
                        // Create Public IP Address
                        at.Append(jLine(2, "OpenBrace", null, ""));
                        at.Append(jLine(3, "name", currentSubscription.secondaryVnetName + "-gw-ip", ","));
                        at.Append(jLine(3, "type", "Microsoft.Network/publicIPAddresses", ","));
                        at.Append(jLine(3, "apiVersion", "2018-01-01", ","));
                        at.Append(jLine(3, "location", "[resourceGroup().location]", ","));
                        at.Append(jLine(3, "properties", null, "{"));
                        at.Append(jLine(4, "publicIPAllocationMethod", "Dynamic", ""));
                        at.Append(jLine(3, "CloseBrace", null, ""));
                        at.Append(jLine(3, "CloseBrace", null, ","));
                        // Create Virtual Network Gateway
                        at.Append(jLine(3, "OpenBrace", null, ""));
                        at.Append(jLine(4, "name", currentSubscription.secondaryVnetName + "-gw", ","));
                        at.Append(jLine(4, "type", "Microsoft.Network/virtualNetworkGateways", ","));
                        at.Append(jLine(4, "apiVersion", "2018-01-01", ","));
                        at.Append(jLine(4, "location", "[resourceGroup().location]", ","));
                        at.Append(jLine(4, "dependsOn", null, "["));
                        at.Append(jLine(5, null, null, "\"[concat('Microsoft.Network/publicIpAddresses/','" + currentSubscription.secondaryVnetName + "-gw-ip')]" + "\","));
                        at.Append(jLine(5, null, null, "\"[concat('Microsoft.Network/virtualNetworks/','" + currentSubscription.secondaryVnetName + "')]" + "\""));
                        at.Append(jLine(4, null, null, "],"));
                        at.Append(jLine(4, "properties", null, "{"));
                        at.Append(jLine(5, "ipConfigurations", null, "["));
                        at.Append(jLine(6, "OpenBrace", null, ""));
                        at.Append(jLine(7, "properties", null, "{"));
                        at.Append(jLine(8, "privateIPAllocationMethod", "Dynamic", ","));
                        at.Append(jLine(8, "subnet", null, "{"));
                        at.Append(jLine(9, "id", "[variables('gatewaySubnetRef')]", ""));
                        at.Append(jLine(8, "CloseBrace", null, ","));
                        at.Append(jLine(8, "publicIPAddress", null, "{"));
                        at.Append(jLine(9, "id", "[resourceId('Microsoft.Network/publicIPAddresses','" + currentSubscription.secondaryVnetName + "-gw-ip')]", ""));
                        at.Append(jLine(8, "CloseBrace", null, ""));
                        at.Append(jLine(7, "CloseBrace", null, ","));
                        at.Append(jLine(7, "name", "vnetGatewayConfig", ""));
                        at.Append(jLine(6, "CloseBrace", null, ""));
                        at.Append(jLine(5, null, null, "],"));
                        at.Append(jLine(5, "gatewayType", "Vpn", ","));
                        at.Append(jLine(5, "vpnType", "RouteBased", ","));
                        at.Append(jLine(5, "enableBgp", null, "false,"));
                        at.Append(jLine(5, "sku", null, "{"));
                        at.Append(jLine(6, "name", "VpnGw1", ","));
                        at.Append(jLine(6, "tier", "VpnGw1", ""));
                        at.Append(jLine(5, "CloseBrace", null, ""));
                        at.Append(jLine(4, "CloseBrace", null, ""));
                        at.Append(jLine(3, "CloseBrace", null, ""));
                    }
                    else
                    {
                        at.Append(jLine(2, "CloseBrace", null, ""));
                    }
                    at.Append(jLine(1, null, null, "]"));
                    at.Append(jLine(0, "CloseBrace", null, ""));
                    break;
            }
            armTemplate = at.ToString();
            return armTemplate;
        }
        private string CreateARMDeploymentScript()
        {
            string cr = Environment.NewLine;
            string psScript = string.Empty;
            StringBuilder armPS = new StringBuilder();
            armPS.Append("################################################" + cr);
            armPS.Append("# Clear Screen and Logon to Azure" + cr);
            armPS.Append("################################################" + cr);
            armPS.Append("Clear-Host" + cr);
            if (currentSubscription.environment == "AzureUSGovernment") { armPS.Append("Add-AzureRmAccount -Environment AzureUSGovernment" + cr); }
            else { armPS.Append("Add-AzureRmAccount" + cr); }
            armPS.Append("$global:script_error = $false" + cr);
            armPS.Append("Write-Host '*****************************************'" + cr);
            armPS.Append("Write-Host ' Azure Foundations Deployment Script'" + cr);
            armPS.Append("Write-Host '*****************************************'" + cr + "Write-Host" + cr);
            //Get Subscription
            armPS.Append("################################################" + cr);
            armPS.Append("# Select Desired Azure Subscription" + cr);
            armPS.Append("################################################" + cr);
            armPS.Append("Write-Host" + cr);
            armPS.Append("Read-Host '*** Press any key to continue ***' | Out-Null" + cr);
            armPS.Append("Write-Host 'Selecting Subscription: ' -NoNewLine" + cr);
            armPS.Append("$sub=Get-AzureRmSubscription -SubscriptionID " + currentSubscription.subscriptionID + " -ErrorAction Ignore" + cr);
            armPS.Append("if($sub)" + cr);
            armPS.Append("{" + cr);
            armPS.Append("   Select-AzureRmSubscription -SubscriptionObject $sub | Out-Null" + cr);
            armPS.Append("   Write-Host 'SUCCESS' -ForegroundColor Green" + cr);
            armPS.Append("}" + cr);
            armPS.Append("else" + cr);
            armPS.Append("{" + cr);
            armPS.Append("   Write-Host 'FAILED- Unable to select subscription' -ForegroundColor Red" + cr);
            armPS.Append("   $global:script_error = $true" + cr);
            armPS.Append("}" + cr);
            //Get or Create Primary Resource Group
            armPS.Append("###############################################" + cr);
            armPS.Append("# Get or Create Primary Location Resource Group" + cr);
            armPS.Append("###############################################" + cr);
            armPS.Append("if($global:script_error -eq $false)" + cr);
            armPS.Append("{" + cr);
            armPS.Append("   Write-Host 'Checking Primary Location Resouce Group: ' -NoNewLine" + cr);
            armPS.Append("   $prg=Get-AzureRmResourceGroup -Name " + currentSubscription.primaryResourceGroup + " -ErrorAction Ignore" + cr);
            armPS.Append("}" + cr);
            armPS.Append("if ($prg) {Write-Host 'WARNING: Resource Group Already Exists--Skipping Creation'}" + cr);
            armPS.Append("else" + cr);
            armPS.Append("{" + cr);
            armPS.Append("   Write-Host 'Resource Group Does Not Exist'" + cr);
            armPS.Append("   Write-Host 'Creating Primary Location Resouce Group: ' -NoNewLine" + cr);
            armPS.Append("   $prg=New-AzureRmResourceGroup -Name " + currentSubscription.primaryResourceGroup + " -Location " + currentSubscription.primaryLocation + cr);
            armPS.Append("   if ($prg) {Write-Host 'SUCCESS' -ForegroundColor Green}" + cr);
            armPS.Append("   else" + cr);
            armPS.Append("   {" + cr);
            armPS.Append("      Write-Host 'FAILED- Unable to create resource group' -ForegroundColor Red" + cr);
            armPS.Append("      $global:script_error = $true" + cr);
            armPS.Append("   }" + cr);
            armPS.Append("}" + cr);
            armPS.Append("########################################################" + cr);
            armPS.Append("# Deploy ARM Template to Primary Location Resource Group" + cr);
            armPS.Append("########################################################" + cr);
            armPS.Append("if($global:script_error -eq $false)" + cr);
            armPS.Append("{" + cr);
            armPS.Append("   Write-Host 'Deploying Primary Vnet'" + cr);
            armPS.Append("   Write-Host '==> NOTE: If a virtual network gateway is being deployed, it will take approximately 20 minutes to create <=='" + cr);
            armPS.Append("   Write-Host" + cr);
            armPS.Append("   New-AzureRmResourceGroupDeployment -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -TemplateFile \"" + currentSubscription.fileName + "-primaryvnet.json\" -DeploymentDebugLogLevel All -Mode Incremental -Verbose -ErrorVariable DeploymentError" + cr);
            armPS.Append("   if($DeploymentError)" + cr);
            armPS.Append("   {" + cr);
            armPS.Append("      Write-Host 'A error was encountered during the deployment--halting script' -ForegroundColor Red" + cr);
            armPS.Append("      $global:script_error = $true" + cr);
            armPS.Append("   }" + cr);
            armPS.Append("   else" + cr);
            armPS.Append("   {" + cr);
            armPS.Append("      Write-Host 'Primary Location Vnet Created Sucessfully' -ForegroundColor Green" + cr);
            armPS.Append("   }" + cr);
            armPS.Append("}" + cr);
            if (currentSubscription.primaryOnly == false)
            {
                armPS.Append("#################################################" + cr);
                armPS.Append("# Get or Create Secondary Location Resource Group" + cr);
                armPS.Append("#################################################" + cr);
                armPS.Append("if($global:script_error -eq $false)" + cr);
                armPS.Append("{" + cr);
                armPS.Append("   Write-Host 'Checking Secondary Location Resouce Group: ' -NoNewLine" + cr);
                armPS.Append("   $prg=Get-AzureRmResourceGroup -Name " + currentSubscription.secondaryResourceGroup + " -ErrorAction Ignore" + cr);
                armPS.Append("}" + cr);
                armPS.Append("if ($prg) {Write-Host 'WARNING: Resource Group Already Exists--Skipping Creation'}" + cr);
                armPS.Append("else" + cr);
                armPS.Append("{" + cr);
                armPS.Append("   Write-Host 'Resource Group Does Not Exist'" + cr);
                armPS.Append("   Write-Host 'Creating Secondary Location Resouce Group: ' -NoNewLine" + cr);
                armPS.Append("   $prg=New-AzureRmResourceGroup -Name " + currentSubscription.secondaryResourceGroup + " -Location " + currentSubscription.secondaryLocation + cr);
                armPS.Append("   if ($prg) {Write-Host 'SUCCESS' -ForegroundColor Green}" + cr);
                armPS.Append("   else" + cr);
                armPS.Append("   {" + cr);
                armPS.Append("      Write-Host 'FAILED- Unable to create resource group' -ForegroundColor Red" + cr);
                armPS.Append("      $global:script_error = $true" + cr);
                armPS.Append("   }" + cr);
                armPS.Append("}" + cr);
                armPS.Append("##########################################################" + cr);
                armPS.Append("# Deploy ARM Template to Secondary Location Resource Group" + cr);
                armPS.Append("##########################################################" + cr);
                armPS.Append("if($global:script_error -eq $false)" + cr);
                armPS.Append("{" + cr);
                armPS.Append("   Write-Host 'Deploying Secondary Vnet'" + cr);
                armPS.Append("   Write-Host '==> NOTE: If a virtual network gateway is being deployed, it will take approximately 20 minutes to create <=='" + cr);
                armPS.Append("   Write-Host" + cr);
                armPS.Append("   New-AzureRmResourceGroupDeployment -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -TemplateFile \"" + currentSubscription.fileName + "-secondaryvnet.json\" -DeploymentDebugLogLevel All -Mode Incremental -Verbose -ErrorVariable DeploymentError" + cr);
                armPS.Append("   if($DeploymentError)" + cr);
                armPS.Append("   {" + cr);
                armPS.Append("      Write-Host 'A error was encountered during the deployment--halting script' -ForegroundColor Red" + cr);
                armPS.Append("      $global:script_error = $true" + cr);
                armPS.Append("   }" + cr);
                armPS.Append("   else" + cr);
                armPS.Append("   {" + cr);
                armPS.Append("      Write-Host 'Secondary Location Vnet Created Sucessfully' -ForegroundColor Green" + cr);
                armPS.Append("   }" + cr);
                armPS.Append("}" + cr);
            }
            armPS.Append("############################################################" + cr);
            armPS.Append("# Deploy NSG ARM Template to Primary Location Resource Group" + cr);
            armPS.Append("############################################################" + cr);
            armPS.Append("if($global:script_error -eq $false)" + cr);
            armPS.Append("{" + cr);
            armPS.Append("   Write-Host 'Deploying NSGs to Primary Vnet'" + cr);
            armPS.Append("   Write-Host" + cr);
            armPS.Append("   New-AzureRmResourceGroupDeployment -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -TemplateFile \"" + currentSubscription.fileName + "-primarynsg.json\" -DeploymentDebugLogLevel All -Mode Incremental -Verbose -ErrorVariable DeploymentError" + cr);
            armPS.Append("   if($DeploymentError)" + cr);
            armPS.Append("   {" + cr);
            armPS.Append("      Write-Host 'A error was encountered during the deployment--halting script' -ForegroundColor Red" + cr);
            armPS.Append("      $global:script_error = $true" + cr);
            armPS.Append("   }" + cr);
            armPS.Append("   else" + cr);
            armPS.Append("   {" + cr);
            armPS.Append("      Write-Host 'Primary Location NSGs Created Sucessfully' -ForegroundColor Green" + cr);
            armPS.Append("   }" + cr);
            armPS.Append("}" + cr);
            if (currentSubscription.primaryOnly == false)
            {
                armPS.Append("##############################################################" + cr);
                armPS.Append("# Deploy NSG ARM Template to Secondary Location Resource Group" + cr);
                armPS.Append("##############################################################" + cr);
                armPS.Append("if($global:script_error -eq $false)" + cr);
                armPS.Append("{" + cr);
                armPS.Append("   Write-Host 'Deploying NSGs to Secondary Vnet'" + cr);
                armPS.Append("   Write-Host" + cr);
                armPS.Append("   New-AzureRmResourceGroupDeployment -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -TemplateFile \"" + currentSubscription.fileName + "-secondarynsg.json\" -DeploymentDebugLogLevel All -Mode Incremental -Verbose -ErrorVariable DeploymentError" + cr);
                armPS.Append("   if($DeploymentError)" + cr);
                armPS.Append("   {" + cr);
                armPS.Append("      Write-Host 'A error was encountered during the deployment--halting script' -ForegroundColor Red" + cr);
                armPS.Append("      $global:script_error = $true" + cr);
                armPS.Append("   }" + cr);
                armPS.Append("   else" + cr);
                armPS.Append("   {" + cr);
                armPS.Append("      Write-Host 'Secondary Location NSGs Created Sucessfully' -ForegroundColor Green" + cr);
                armPS.Append("   }" + cr);
                armPS.Append("}" + cr);
            }
            if (currentSubscription.createConnection)
            {
                armPS.Append("   Write-Host" + cr);
                armPS.Append("   Write-Host 'Creating VPN Connections'" + cr);
                armPS.Append("####################################################" + cr);
                armPS.Append("# Create Connections Between Locations and Local" + cr);
                armPS.Append("####################################################" + cr);
                //Set Gateway Variables
                armPS.Append("$plgw=Get-AzureRmLocalNetworkGateway -Name " + currentSubscription.localGatewayName + "-" + currentSubscription.primaryLocation + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                armPS.Append("$slgw=Get-AzureRmLocalNetworkGateway -Name " + currentSubscription.localGatewayName + "-" + currentSubscription.secondaryLocation + " -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                armPS.Append("$pgw=Get-AzureRmVirtualNetworkGateway -Name " + currentSubscription.primaryVnetName + "-gw -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                armPS.Append("$sgw=Get-AzureRmVirtualNetworkGateway -Name " + currentSubscription.secondaryVnetName + "-gw -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                //Create Primary Gateway Connection to Local Gateway
                armPS.Append("####################################################" + cr);
                armPS.Append("# Create Primary Gateway Connection to Local Gateway" + cr);
                armPS.Append("####################################################" + cr);
                armPS.Append("if($global:script_error -eq $false)" + cr);
                armPS.Append("{" + cr);
                armPS.Append("   Write-Host 'Creating Primary Connection: ' -NoNewLine" + cr);
                armPS.Append("   $pcn=Get-AzureRmVirtualNetworkGatewayConnection -Name cn-local-" + currentSubscription.primaryLocation + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                armPS.Append("   if($pcn) {Write-Host 'WARNING: Connection already exists--skipping creation'}" + cr);
                armPS.Append("   else" + cr);
                armPS.Append("   {" + cr);
                string priSkey = CreateSharedKey();
                armPS.Append("      $pcn=New-AzureRmVirtualNetworkGatewayConnection -Name cn-local-" + currentSubscription.primaryLocation + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -Location " + currentSubscription.primaryLocation + " -VirtualNetworkGateway1 $pgw -LocalNetworkGateway2 $plgw -ConnectionType IPsec -RoutingWeight 10 -SharedKey '" + priSkey + "' -WarningAction SilentlyContinue" + cr);
                armPS.Append("      if($pcn) {Write-Host 'SUCCESS' -ForegroundColor Green}" + cr);
                armPS.Append("      else" + cr);
                armPS.Append("      {" + cr);
                armPS.Append("         Write-Host 'FAILED- Unable to create connection' -ForegroundColor Red" + cr);
                armPS.Append("         $global:script_error = $true" + cr);
                armPS.Append("      }" + cr);
                armPS.Append("   }" + cr);
                armPS.Append("}" + cr);
                if (currentSubscription.primaryOnly == false)
                {
                    //Create Secondary Gateway Connection to Local Gateway
                    armPS.Append("######################################################" + cr);
                    armPS.Append("# Create Secondary Gateway Connection to Local Gateway" + cr);
                    armPS.Append("######################################################" + cr);
                    armPS.Append("if($global:script_error -eq $false)" + cr);
                    armPS.Append("{" + cr);
                    armPS.Append("   Write-Host 'Creating Secondary Connection: ' -NoNewLine" + cr);
                    armPS.Append("   $scn=Get-AzureRmVirtualNetworkGatewayConnection -Name cn-local-" + currentSubscription.secondaryLocation + " -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                    armPS.Append("   if($scn) {Write-Host 'WARNING: Connection already exists--skipping creation'}" + cr);
                    armPS.Append("   else" + cr);
                    armPS.Append("   {" + cr);
                    string secSkey = CreateSharedKey();
                    armPS.Append("      $scn=New-AzureRmVirtualNetworkGatewayConnection -Name cn-local-" + currentSubscription.secondaryLocation + " -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -Location " + currentSubscription.secondaryLocation + " -VirtualNetworkGateway1 $sgw -LocalNetworkGateway2 $slgw -ConnectionType IPsec -RoutingWeight 10 -SharedKey '" + secSkey + "' -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                    armPS.Append("      if($scn) {Write-Host 'SUCCESS' -ForegroundColor Green}" + cr);
                    armPS.Append("      else" + cr);
                    armPS.Append("      {" + cr);
                    armPS.Append("         Write-Host 'FAILED- Unable to create connection' -ForegroundColor Red" + cr);
                    armPS.Append("         $global:script_error = $true" + cr);
                    armPS.Append("      }" + cr);
                    armPS.Append("   }" + cr);
                    armPS.Append("}" + cr);
                    //Create VNet-to-VNet Connection (Primary to Secondary)
                    armPS.Append("#######################################################" + cr);
                    armPS.Append("# Create VNet-to-VNet Connection (Primary to Secondary)" + cr);
                    armPS.Append("#######################################################" + cr);
                    armPS.Append("if($global:script_error -eq $false)" + cr);
                    armPS.Append("{" + cr);
                    armPS.Append("   Write-Host 'Creating VNet-to-VNet Connection (Primary to Secondary): ' -NoNewLine" + cr);
                    armPS.Append("   $pvcn=Get-AzureRmVirtualNetworkGatewayConnection -Name cn-" + currentSubscription.primaryVnetName + "-" + currentSubscription.secondaryVnetName + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                    armPS.Append("   if($pvcn) {Write-Host 'WARNING: Connection already exists--skipping creation'}" + cr);
                    armPS.Append("   else" + cr);
                    armPS.Append("   {" + cr);
                    string v2vSkey = CreateSharedKey();
                    armPS.Append("      $pvcn=New-AzureRmVirtualNetworkGatewayConnection -Name cn-" + currentSubscription.primaryVnetName + "-" + currentSubscription.secondaryVnetName + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -Location " + currentSubscription.primaryLocation + " -VirtualNetworkGateway1 $pgw -VirtualNetworkGateway2 $sgw -ConnectionType Vnet2Vnet -SharedKey '" + v2vSkey + "' -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                    armPS.Append("      if($pvcn) {Write-Host 'SUCCESS' -ForegroundColor Green}" + cr);
                    armPS.Append("      else" + cr);
                    armPS.Append("      {" + cr);
                    armPS.Append("         Write-Host 'FAILED- Unable to create connection' -ForegroundColor Red" + cr);
                    armPS.Append("         $global:script_error = $true" + cr);
                    armPS.Append("      }" + cr);
                    armPS.Append("   }" + cr);
                    armPS.Append("}" + cr);
                    //Create VNet-to-VNet Connection (Secondary to Primary)
                    armPS.Append("#######################################################" + cr);
                    armPS.Append("# Create VNet-to-VNet Connection (Secondary to Primary)" + cr);
                    armPS.Append("#######################################################" + cr);
                    armPS.Append("if($global:script_error -eq $false)" + cr);
                    armPS.Append("{" + cr);
                    armPS.Append("   Write-Host 'Creating VNet-to-VNet Connection (Secondary to Primary): ' -NoNewLine" + cr);
                    armPS.Append("   $svcn=Get-AzureRmVirtualNetworkGatewayConnection -Name cn-" + currentSubscription.secondaryVnetName + "-" + currentSubscription.primaryVnetName + " -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                    armPS.Append("   if($svcn) {Write-Host 'WARNING: Connection already exists--skipping creation'}" + cr);
                    armPS.Append("   else" + cr);
                    armPS.Append("   {" + cr);
                    armPS.Append("      $svcn=New-AzureRmVirtualNetworkGatewayConnection -Name cn-" + currentSubscription.secondaryVnetName + "-" + currentSubscription.primaryVnetName + " -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -Location " + currentSubscription.secondaryLocation + " -VirtualNetworkGateway1 $sgw -VirtualNetworkGateway2 $pgw -ConnectionType Vnet2Vnet -SharedKey '" + v2vSkey + "' -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                    armPS.Append("      if($svcn) {Write-Host 'SUCCESS' -ForegroundColor Green}" + cr);
                    armPS.Append("      else" + cr);
                    armPS.Append("      {" + cr);
                    armPS.Append("         Write-Host 'FAILED- Unable to create connection' -ForegroundColor Red" + cr);
                    armPS.Append("         $global:script_error = $true" + cr);
                    armPS.Append("      }" + cr);
                    armPS.Append("   }" + cr);
                    armPS.Append("}" + cr);
                }
            }
            //Report Script Success or Failure
            armPS.Append("Write-Host" + cr);
            armPS.Append("if($global:script_error) {Write-Host 'ERROR: An Error Has Occurred. Script Halted.' -ForegroundColor Red}" + cr);
            armPS.Append("else" + cr);
            armPS.Append("{" + cr);
            armPS.Append("   Write-Host 'SUCCESS: Virtual Network Connections Created Sucessfully.' -ForegroundColor Green" + cr);
            armPS.Append("   Write-Host 'COMPLETE: All Foundations Resources Deployed.' -ForegroundColor Green" + cr);
            armPS.Append("}" + cr);
            psScript = armPS.ToString();
            return psScript;
        }
        private string CreatePowerShellScript(int _scr)
        {
            string cr = Environment.NewLine;
            string psScript = string.Empty;
            StringBuilder ps = new StringBuilder();
            switch (_scr)
            {
                case 1:     //Virtural Network
                    ps.Append("################################################" + cr);
                    ps.Append("# Clear Screen and Logon to Azure" + cr);
                    ps.Append("################################################" + cr);
                    ps.Append("Clear-Host" + cr);
                    if (currentSubscription.environment == "AzureUSGovernment") { ps.Append("Add-AzureRmAccount -Environment AzureUSGovernment" + cr); }
                    else { ps.Append("Add-AzureRmAccount" + cr); }
                    ps.Append("$global:script_error = $false" + cr);
                    ps.Append("Write-Host '*****************************************'" + cr);
                    ps.Append("Write-Host ' Azure Foundations Script 1: Create Vnet'" + cr);
                    ps.Append("Write-Host '*****************************************'" + cr + "Write-Host" + cr);
                    //Get Subscription
                    ps.Append("################################################" + cr);
                    ps.Append("# Select Desired Azure Subscription" + cr);
                    ps.Append("################################################" + cr);
                    ps.Append("Write-Host" + cr);
                    ps.Append("Read-Host '*** Press any key to continue ***' | Out-Null" + cr);
                    ps.Append("Write-Host 'Selecting Subscription: ' -NoNewLine" + cr);
                    ps.Append("$sub=Get-AzureRmSubscription -SubscriptionID " + currentSubscription.subscriptionID + " -ErrorAction Ignore" + cr);
                    ps.Append("if($sub)" + cr);
                    ps.Append("{" + cr);
                    ps.Append("   Select-AzureRmSubscription -SubscriptionObject $sub | Out-Null" + cr);
                    ps.Append("   Write-Host 'SUCCESS' -ForegroundColor Green" + cr);
                    ps.Append("}" + cr);
                    ps.Append("else" + cr);
                    ps.Append("{" + cr);
                    ps.Append("   Write-Host 'FAILED- Unable to select subscription' -ForegroundColor Red" + cr);
                    ps.Append("   $global:script_error = $true" + cr);
                    ps.Append("}" + cr);
                    //Get or Create Primary Resource Group
                    ps.Append("################################################" + cr);
                    ps.Append("# Get or Create Primary Resource Group" + cr);
                    ps.Append("################################################" + cr);
                    ps.Append("if($global:script_error -eq $false)" + cr);
                    ps.Append("{" + cr);
                    ps.Append("   Write-Host 'Checking Primary Resouce Group: ' -NoNewLine" + cr);
                    ps.Append("   $prg=Get-AzureRmResourceGroup -Name " + currentSubscription.primaryResourceGroup + " -ErrorAction Ignore" + cr);
                    ps.Append("}" + cr);
                    ps.Append("if ($prg) {Write-Host 'WARNING: Resource Group Already Exists--Skipping Creation'}" + cr);
                    ps.Append("else" + cr);
                    ps.Append("{" + cr);
                    ps.Append("   Write-Host 'Resource Group Does Not Exist'" + cr);
                    ps.Append("   Write-Host 'Creating Primary Resouce Group: ' -NoNewLine" + cr);
                    ps.Append("   $prg=New-AzureRmResourceGroup -Name " + currentSubscription.primaryResourceGroup + " -Location " + currentSubscription.primaryLocation + cr);
                    ps.Append("   if ($prg) {Write-Host 'SUCCESS' -ForegroundColor Green}" + cr);
                    ps.Append("   else" + cr);
                    ps.Append("   {" + cr);
                    ps.Append("      Write-Host 'FAILED- Unable to create resource group' -ForegroundColor Red" + cr);
                    ps.Append("      $global:script_error = $true" + cr);
                    ps.Append("   }" + cr);
                    ps.Append("}" + cr);
                    //Create Primary Virtual Network
                    ps.Append("################################################" + cr);
                    ps.Append("# Create Primary Virtual Network" + cr);
                    ps.Append("################################################" + cr);
                    ps.Append("if($global:script_error -eq $false)" + cr);
                    ps.Append("{" + cr);
                    ps.Append("   Write-Host 'Creating Primary Virtual Network: ' -NoNewLine" + cr);
                    ps.Append("   $pvn=Get-AzureRmVirtualNetwork -Name " + currentSubscription.primaryVnetName + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -ErrorAction Ignore" + cr);
                    ps.Append("   if($pvn) {Write-Host 'WARNING: VNet already exists--skipping creation'}" + cr);
                    ps.Append("   else" + cr);
                    ps.Append("   {" + cr);
                    ps.Append("      New-AzureRmVirtualNetwork -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -Name " + currentSubscription.primaryVnetName + " -AddressPrefix " + currentSubscription.primaryIPSegment + " -Location " + currentSubscription.primaryLocation + " -ErrorAction Ignore -WarningAction SilentlyContinue | Out-Null" + cr);
                    ps.Append("      $pvn=Get-AzureRmVirtualNetwork -Name " + currentSubscription.primaryVnetName + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -ErrorAction Ignore" + cr);
                    ps.Append("      if($pvn) {Write-Host 'SUCCESS' -ForegroundColor Green}" + cr);
                    ps.Append("      else" + cr);
                    ps.Append("      {" + cr);
                    ps.Append("         Write-Host 'FAILED- Unable to create the virtual network' -ForegroundColor Red" + cr);
                    ps.Append("         $global:script_error = $true" + cr);
                    ps.Append("      }" + cr);
                    ps.Append("   }" + cr);
                    ps.Append("}" + cr);
                    //Add Primary Subnets
                    ps.Append("################################################" + cr);
                    ps.Append("# Add Primary Subnets" + cr);
                    ps.Append("################################################" + cr);
                    ps.Append("if($global:script_error -eq $false)" + cr);
                    ps.Append("{" + cr);
                    ps.Append("   Write-Host 'Creating Subnets: ' -NoNewLine" + cr);
                    ps.Append("   $psn=Get-AzureRmVirtualNetworkSubnetConfig -VirtualNetwork $pvn -ErrorAction Ignore" + cr);
                    ps.Append("   if($psn) {Write-Host 'WARNING: Subnets already exist--skipping creation'}" + cr);
                    ps.Append("   else" + cr);
                    ps.Append("   {" + cr);
                    for (int ctr = 0; ctr < 8; ctr++)
                    {
                        if (currentSubscription.primarySubnets[ctr].ipSegment != string.Empty)
                        {
                            ps.Append("      Add-AzureRmVirtualNetworkSubnetConfig -Name '" + currentSubscription.primarySubnets[ctr].name + "' -AddressPrefix " + currentSubscription.primarySubnets[ctr].ipSegment + " -VirtualNetwork $pvn | Out-Null" + cr);
                        }
                    }
                    ps.Append("      Write-Host 'SUCCESS' -ForegroundColor Green" + cr);
                    ps.Append("      Write-Host 'Updating Virtual Network: ' -NoNewLine" + cr);
                    ps.Append("      $setPvn=Set-AzureRmVirtualNetwork -VirtualNetwork $pvn" + cr);
                    ps.Append("      Write-Host 'SUCCESS' -ForegroundColor Green" + cr);
                    ps.Append("   }" + cr);
                    ps.Append("}" + cr);
                    //Create Secondary Resources if Both Regions Selected for Deployment
                    if (currentSubscription.primaryOnly == false)
                    {
                        //Get or Create Secondary Resource Group
                        ps.Append("################################################" + cr);
                        ps.Append("# Get or Create Secondary Resource Group" + cr);
                        ps.Append("################################################" + cr);
                        ps.Append("if($global:script_error -eq $false)" + cr);
                        ps.Append("{" + cr);
                        ps.Append("   Write-Host 'Checking Secondary Resouce Group: ' -NoNewLine" + cr);
                        ps.Append("   $srg=Get-AzureRmResourceGroup -Name " + currentSubscription.secondaryResourceGroup + " -ErrorAction Ignore" + cr);
                        ps.Append("}" + cr);
                        ps.Append("if ($srg) {Write-Host 'WARNING: Resource group already exists--skipping creation'}" + cr);
                        ps.Append("else" + cr);
                        ps.Append("{" + cr);
                        ps.Append("   Write-Host 'Resource Group Does Not Exist'" + cr);
                        ps.Append("   Write-Host 'Creating Secondary Resouce Group: ' -NoNewLine" + cr);
                        ps.Append("   $srg=New-AzureRmResourceGroup -Name " + currentSubscription.secondaryResourceGroup + " -Location " + currentSubscription.secondaryLocation + cr);
                        ps.Append("   if ($srg) {Write-Host 'SUCCESS' -ForegroundColor Green}" + cr);
                        ps.Append("   else" + cr);
                        ps.Append("   {" + cr);
                        ps.Append("      Write-Host 'FAILED- Unable to create resource group' -ForegroundColor Red" + cr);
                        ps.Append("      $global:script_error = $true" + cr);
                        ps.Append("   }" + cr);
                        ps.Append("}" + cr);
                        //Create Secondary Virtual Network
                        ps.Append("################################################" + cr);
                        ps.Append("# Create Secondary Virtual Network" + cr);
                        ps.Append("################################################" + cr);
                        ps.Append("if($global:script_error -eq $false)" + cr);
                        ps.Append("{" + cr);
                        ps.Append("   Write-Host 'Creating Secondary Virtual Network: ' -NoNewLine" + cr);
                        ps.Append("   $svn=Get-AzureRmVirtualNetwork -Name " + currentSubscription.secondaryVnetName + " -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -ErrorAction Ignore" + cr);
                        ps.Append("   if($svn)" + cr);
                        ps.Append("   {" + cr);
                        ps.Append("      Write-Host 'WARNING: VNet already exists--skipping creation'" + cr);
                        ps.Append("      $svnExists = $true" + cr);
                        ps.Append("   }" + cr);
                        ps.Append("   else" + cr);
                        ps.Append("   {" + cr);
                        ps.Append("      New-AzureRmVirtualNetwork -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -Name " + currentSubscription.secondaryVnetName + " -AddressPrefix " + currentSubscription.secondaryIPSegment + " -Location " + currentSubscription.secondaryLocation + " -ErrorAction Ignore -WarningAction SilentlyContinue | Out-Null" + cr);
                        ps.Append("      $svn=Get-AzureRmVirtualNetwork -Name " + currentSubscription.secondaryVnetName + " -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -ErrorAction Ignore" + cr);
                        ps.Append("      if($svn) {Write-Host 'SUCCESS' -ForegroundColor Green}" + cr);
                        ps.Append("      else" + cr);
                        ps.Append("      {" + cr);
                        ps.Append("         Write-Host 'FAILED- Unable to create the virtual network' -ForegroundColor Red" + cr);
                        ps.Append("         $global:script_error = $true" + cr);
                        ps.Append("      }" + cr);
                        ps.Append("   }" + cr);
                        ps.Append("}" + cr);
                        //Add Secondary Subnets
                        ps.Append("################################################" + cr);
                        ps.Append("# Add Secondary Subnets" + cr);
                        ps.Append("################################################" + cr);
                        ps.Append("if($global:script_error -eq $false)" + cr);
                        ps.Append("{" + cr);
                        ps.Append("   Write-Host 'Creating Secondary Subnets: ' -NoNewLine" + cr);
                        ps.Append("   $ssn=Get-AzureRmVirtualNetworkSubnetConfig -VirtualNetwork $svn -ErrorAction Ignore" + cr);
                        ps.Append("   if($ssn) {Write-Host 'WARNING: Subnets already exist--skipping creation'}" + cr);
                        ps.Append("   else" + cr);
                        ps.Append("   {" + cr);
                        for (int ctr = 0; ctr < 8; ctr++)
                        {
                            if (currentSubscription.secondarySubnets[ctr].ipSegment != string.Empty)
                            {
                                ps.Append("      Add-AzureRmVirtualNetworkSubnetConfig -Name '" + currentSubscription.secondarySubnets[ctr].name + "' -AddressPrefix " + currentSubscription.secondarySubnets[ctr].ipSegment + " -VirtualNetwork $svn | Out-Null" + cr);
                            }
                        }
                        ps.Append("      Write-Host 'SUCCESS' -ForegroundColor Green" + cr);
                        ps.Append("      Write-Host 'Updating Virtual Network: ' -NoNewLine" + cr);
                        ps.Append("      $setSvn=Set-AzureRmVirtualNetwork -VirtualNetwork $svn" + cr);
                        ps.Append("      Write-Host 'SUCCESS' -ForegroundColor Green" + cr);
                        ps.Append("   }" + cr);
                        ps.Append("}" + cr);
                    }
                    //Create Local Gateway
                    ps.Append("################################################" + cr);
                    ps.Append("# Create Primary Local Gateway" + cr);
                    ps.Append("################################################" + cr);
                    ps.Append("if($global:script_error -eq $false)" + cr);
                    ps.Append("{" + cr);
                    ps.Append("   Write-Host 'Creating Primary Local Network Gateway: ' -NoNewLine" + cr);
                    ps.Append("   $lgw=Get-AzureRmLocalNetworkGateway -Name " + currentSubscription.localGatewayName + "-" + currentSubscription.primaryLocation + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -ErrorAction Ignore" + cr);
                    ps.Append("   if($lgw) {Write-Host 'WARNING: Local gateway already exists--skipping creation'}" + cr);
                    ps.Append("   else" + cr);
                    ps.Append("   {" + cr);
                    ps.Append("      New-AzureRmLocalNetworkGateway -Name " + currentSubscription.localGatewayName + "-" + currentSubscription.primaryLocation + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -Location " + currentSubscription.primaryLocation + " -GatewayIpAddress " + currentSubscription.edgeIP + " -AddressPrefix " + currentSubscription.localAddressSpace + " -ErrorAction Ignore -WarningAction SilentlyContinue | Out-Null" + cr);
                    ps.Append("      $lgw=Get-AzureRmLocalNetworkGateway -Name " + currentSubscription.localGatewayName + "-" + currentSubscription.primaryLocation + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -ErrorAction Ignore" + cr);
                    ps.Append("      if($lgw) {Write-Host 'SUCCESS' -ForegroundColor Green}" + cr);
                    ps.Append("      else" + cr);
                    ps.Append("      {" + cr);
                    ps.Append("         Write-Host 'FAILED- Unable to create local network gateway' -ForegroundColor Red" + cr);
                    ps.Append("         $global:script_error = $true" + cr);
                    ps.Append("      }" + cr);
                    ps.Append("   }" + cr);
                    ps.Append("}" + cr);
                    //Get a Public IP Address for Primary Gateway
                    ps.Append("################################################" + cr);
                    ps.Append("# Get a Public IP Address for Primary Gateway" + cr);
                    ps.Append("################################################" + cr);
                    ps.Append("if($global:script_error -eq $false)" + cr);
                    ps.Append("{" + cr);
                    ps.Append("   Write-Host 'Requesting Primary Gateway IP Address: ' -NoNewLine" + cr);
                    ps.Append("   $pgwip=Get-AzureRmPublicIpAddress -Name " + currentSubscription.primaryVnetName + "-gw-ip -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -ErrorAction Ignore" + cr);
                    ps.Append("   if($pgwip) {Write-Host 'WARNING: Public IP already exists--skipping creation'}" + cr);
                    ps.Append("   else" + cr);
                    ps.Append("   {" + cr);
                    ps.Append("      New-AzureRmPublicIpAddress -Name " + currentSubscription.primaryVnetName + "-gw-ip -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -Location " + currentSubscription.primaryLocation + " -AllocationMethod Dynamic -ErrorAction Ignore -WarningAction SilentlyContinue | Out-Null" + cr);
                    ps.Append("      $pgwip=Get-AzureRmPublicIpAddress -Name " + currentSubscription.primaryVnetName + "-gw-ip -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -ErrorAction Ignore" + cr);
                    ps.Append("      if($pgwip) {Write-Host 'SUCCESS' -ForegroundColor Green}" + cr);
                    ps.Append("      else" + cr);
                    ps.Append("      {" + cr);
                    ps.Append("         Write-Host 'FAILED- Unable to get a Public IP Address' -ForegroundColor Red" + cr);
                    ps.Append("         $global:script_error = $true" + cr);
                    ps.Append("      }" + cr);
                    ps.Append("   }" + cr);
                    ps.Append("}" + cr);
                    //Create Primary Azure Vnet Gateway
                    ps.Append("################################################" + cr);
                    ps.Append("# Create Primary Azure Vnet Gateway" + cr);
                    ps.Append("################################################" + cr);
                    ps.Append("if($global:script_error -eq $false)" + cr);
                    ps.Append("{" + cr);
                    ps.Append("   Write-Host '==> NOTE: Azure Gateway Creation Takes Approximately 20 Minutes! <=='" + cr);
                    ps.Append("   Write-Host 'Creating Primary Azure Gateway: ' -NoNewLine" + cr);
                    ps.Append("   $pgw=Get-AzureRmVirtualNetworkGateway -Name " + currentSubscription.primaryVnetName + "-gw -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -ErrorAction Ignore" + cr);
                    ps.Append("   if($pgw) {Write-Host 'WARNING: Vnet gateway already exists--skipping creation'}" + cr);
                    ps.Append("   else" + cr);
                    ps.Append("   {" + cr);
                    ps.Append("      $psn=Get-AzureRmVirtualNetworkSubnetConfig -Name 'GatewaySubnet' -VirtualNetwork $pvn" + cr);
                    ps.Append("      $pgwipcfg=New-AzureRmVirtualNetworkGatewayIpConfig -Name pgwipconfig1 -SubnetId $psn.Id -PublicIpAddressId $pgwip.Id" + cr);
                    ps.Append("      $pgw=New-AzureRmVirtualNetworkGateway -Name " + currentSubscription.primaryVnetName + "-gw -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -Location " + currentSubscription.primaryLocation + " -IpConfigurations $pgwipcfg -GatewayType Vpn -VpnType RouteBased -GatewaySku VpnGw1 -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                    ps.Append("      if($pgw) {Write-Host 'SUCCESS' -ForegroundColor Green}" + cr);
                    ps.Append("      else" + cr);
                    ps.Append("      {" + cr);
                    ps.Append("         Write-Host 'FAILED- Unable to Create VNet Gateway' -ForegroundColor Red" + cr);
                    ps.Append("         $global:script_error = $true" + cr);
                    ps.Append("      }" + cr);
                    ps.Append("   }" + cr);
                    ps.Append("}" + cr);
                    if (currentSubscription.primaryOnly == false)
                    {
                        //Create Local Gateway
                        ps.Append("################################################" + cr);
                        ps.Append("# Create Secondary Local Gateway" + cr);
                        ps.Append("################################################" + cr);
                        ps.Append("if($global:script_error -eq $false)" + cr);
                        ps.Append("{" + cr);
                        ps.Append("   Write-Host 'Creating Secondary Local Network Gateway: ' -NoNewLine" + cr);
                        ps.Append("   $lgw=Get-AzureRmLocalNetworkGateway -Name " + currentSubscription.localGatewayName + "-" + currentSubscription.secondaryLocation + " -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -ErrorAction Ignore" + cr);
                        ps.Append("   if($lgw) {Write-Host 'WARNING: Local gateway already exists--skipping creation'}" + cr);
                        ps.Append("   else" + cr);
                        ps.Append("   {" + cr);
                        ps.Append("      New-AzureRmLocalNetworkGateway -Name " + currentSubscription.localGatewayName + "-" + currentSubscription.secondaryLocation + " -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -Location " + currentSubscription.secondaryLocation + " -GatewayIpAddress " + currentSubscription.edgeIP + " -AddressPrefix " + currentSubscription.localAddressSpace + " -ErrorAction Ignore -WarningAction SilentlyContinue | Out-Null" + cr);
                        ps.Append("      $lgw=Get-AzureRmLocalNetworkGateway -Name " + currentSubscription.localGatewayName + "-" + currentSubscription.secondaryLocation + " -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -ErrorAction Ignore" + cr);
                        ps.Append("      if($lgw) {Write-Host 'SUCCESS' -ForegroundColor Green}" + cr);
                        ps.Append("      else" + cr);
                        ps.Append("      {" + cr);
                        ps.Append("         Write-Host 'FAILED- Unable to create local network gateway' -ForegroundColor Red" + cr);
                        ps.Append("         $global:script_error = $true" + cr);
                        ps.Append("      }" + cr);
                        ps.Append("   }" + cr);
                        ps.Append("}" + cr);
                        //Get a Public IP Address for Secondary Gateway
                        ps.Append("################################################" + cr);
                        ps.Append("# Get a Public IP Address for Secondary Gateway" + cr);
                        ps.Append("################################################" + cr);
                        ps.Append("if($global:script_error -eq $false)" + cr);
                        ps.Append("{" + cr);
                        ps.Append("   Write-Host 'Requesting Secondary Gateway IP Address: ' -NoNewLine" + cr);
                        ps.Append("   $sgwip=Get-AzureRmPublicIpAddress -Name " + currentSubscription.secondaryVnetName + "-gw-ip -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -ErrorAction Ignore" + cr);
                        ps.Append("   if($sgwip) {Write-Host 'WARNING: Public IP address already exists--skipping creation'}" + cr);
                        ps.Append("   else" + cr);
                        ps.Append("   {" + cr);
                        ps.Append("      New-AzureRmPublicIpAddress -Name " + currentSubscription.secondaryVnetName + "-gw-ip -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -Location " + currentSubscription.secondaryLocation + " -AllocationMethod Dynamic -ErrorAction Ignore -WarningAction SilentlyContinue | Out-Null" + cr);
                        ps.Append("      $sgwip=Get-AzureRmPublicIpAddress -Name " + currentSubscription.secondaryVnetName + "-gw-ip -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -ErrorAction Ignore" + cr);
                        ps.Append("      if($sgwip) {Write-Host 'SUCCESS' -ForegroundColor Green}" + cr);
                        ps.Append("      else" + cr);
                        ps.Append("      {" + cr);
                        ps.Append("         Write-Host 'FAILED- Unable to get a Public IP Address' -ForegroundColor Red" + cr);
                        ps.Append("         $global:script_error = $true" + cr);
                        ps.Append("      }" + cr);
                        ps.Append("   }" + cr);
                        ps.Append("}" + cr);
                        //Create Secondary Azure Vnet Gateway
                        ps.Append("################################################" + cr);
                        ps.Append("# Create Secondary Azure Vnet Gateway" + cr);
                        ps.Append("################################################" + cr);
                        ps.Append("if($global:script_error -eq $false)" + cr);
                        ps.Append("{" + cr);
                        ps.Append("   Write-Host '==> NOTE: Azure Gateway Creation Takes Approximately 20 Minutes! <=='" + cr);
                        ps.Append("   Write-Host 'Creating Secondary Azure Gateway: ' -NoNewLine" + cr);
                        ps.Append("   $sgw=Get-AzureRmVirtualNetworkGateway -Name " + currentSubscription.secondaryVnetName + "-gw -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -ErrorAction Ignore" + cr);
                        ps.Append("   if($sgw) {Write-Host 'WARNING: Vnet gateway already exists--skipping creation'}" + cr);
                        ps.Append("   else" + cr);
                        ps.Append("   {" + cr);
                        ps.Append("      $ssn=Get-AzureRmVirtualNetworkSubnetConfig -Name 'GatewaySubnet' -VirtualNetwork $svn" + cr);
                        ps.Append("      $sgwipcfg=New-AzureRmVirtualNetworkGatewayIpConfig -Name sgwipconfig1 -SubnetId $ssn.Id -PublicIpAddressId $sgwip.Id" + cr);
                        ps.Append("      $sgw=New-AzureRmVirtualNetworkGateway -Name " + currentSubscription.secondaryVnetName + "-gw -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -Location " + currentSubscription.secondaryLocation + " -IpConfigurations $sgwipcfg -GatewayType Vpn -VpnType RouteBased -GatewaySku VpnGw1 -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                        ps.Append("      if($sgw) {Write-Host 'SUCCESS' -ForegroundColor Green}" + cr);
                        ps.Append("      else" + cr);
                        ps.Append("      {" + cr);
                        ps.Append("         Write-Host 'FAILED- Unable to Create VNet Gateway' -ForegroundColor Red" + cr);
                        ps.Append("         $global:script_error = $true" + cr);
                        ps.Append("      }" + cr);
                        ps.Append("   }" + cr);
                        ps.Append("}" + cr);
                    }
                    //Report Script Success or Failure
                    ps.Append("Write-Host" + cr);
                    ps.Append("if($global:script_error) {Write-Host 'ERROR: An Error Has Occurred. Script Halted.' -ForegroundColor Red}" + cr);
                    ps.Append("else {Write-Host 'COMPLETE: Virtual Network(s) and Gateway(s) Created Successfully.' -ForegroundColor Green}" + cr);
                    break;
                case 2:     //Connections
                    ps.Append("################################################" + cr);
                    ps.Append("# Clear Screen and Logon to Azure" + cr);
                    ps.Append("################################################" + cr);
                    ps.Append("Clear-Host" + cr);
                    if (currentSubscription.environment == "AzureUSGovernment") { ps.Append("Add-AzureRmAccount -Environment AzureUSGovernment" + cr); }
                    else { ps.Append("Add-AzureRmAccount" + cr); }
                    ps.Append("$global:script_error = $false" + cr);
                    ps.Append("Write-Host '***********************************************'" + cr);
                    ps.Append("Write-Host ' Azure Foundations Script 2: Create Connections'" + cr);
                    ps.Append("Write-Host '***********************************************'" + cr + "Write-Host" + cr);
                    //Get Subscription
                    ps.Append("################################################" + cr);
                    ps.Append("# Select Desired Azure Subscription" + cr);
                    ps.Append("################################################" + cr);
                    ps.Append("Write-Host 'Selecting Subscription: ' -NoNewLine" + cr);
                    ps.Append("$sub=Get-AzureRmSubscription -SubscriptionID " + currentSubscription.subscriptionID + " -ErrorAction Ignore" + cr);
                    ps.Append("if($sub)" + cr);
                    ps.Append("{" + cr);
                    ps.Append("   Select-AzureRmSubscription -SubscriptionObject $sub | Out-Null" + cr);
                    ps.Append("   Write-Host 'SUCCESS' -ForegroundColor Green" + cr);
                    ps.Append("}" + cr);
                    ps.Append("else" + cr);
                    ps.Append("{" + cr);
                    ps.Append("   Write-Host 'FAILED- Unable to select subscription' -ForegroundColor Red" + cr);
                    ps.Append("   $global:script_error = $true" + cr);
                    ps.Append("}" + cr);
                    //Set Gateway Variables
                    ps.Append("$lgw=Get-AzureRmLocalNetworkGateway -Name " + currentSubscription.localGatewayName + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                    ps.Append("$pgw=Get-AzureRmVirtualNetworkGateway -Name " + currentSubscription.primaryVnetName + "-gw -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                    ps.Append("$sgw=Get-AzureRmVirtualNetworkGateway -Name " + currentSubscription.secondaryVnetName + "-gw -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                    //Create Primary Gateway Connection to Local Gateway
                    ps.Append("####################################################" + cr);
                    ps.Append("# Create Primary Gateway Connection to Local Gateway" + cr);
                    ps.Append("####################################################" + cr);
                    ps.Append("if($global:script_error -eq $false)" + cr);
                    ps.Append("{" + cr);
                    ps.Append("   Write-Host 'Creating Primary Connection: ' -NoNewLine" + cr);
                    ps.Append("   $pcn=Get-AzureRmVirtualNetworkGatewayConnection -Name cn-local-" + currentSubscription.primaryLocation + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                    ps.Append("   if($pcn) {Write-Host 'WARNING: Connection already exists--skipping creation'}" + cr);
                    ps.Append("   else" + cr);
                    ps.Append("   {" + cr);
                    string priSkey = CreateSharedKey();
                    ps.Append("      $pcn=New-AzureRmVirtualNetworkGatewayConnection -Name cn-local-" + currentSubscription.primaryLocation + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -Location " + currentSubscription.primaryLocation + " -VirtualNetworkGateway1 $pgw -LocalNetworkGateway2 $lgw -ConnectionType IPsec -RoutingWeight 10 -SharedKey '" + priSkey + "' -WarningAction SilentlyContinue" + cr);
                    ps.Append("      if($pcn) {Write-Host 'SUCCESS' -ForegroundColor Green}" + cr);
                    ps.Append("      else" + cr);
                    ps.Append("      {" + cr);
                    ps.Append("         Write-Host 'FAILED- Unable to create connection' -ForegroundColor Red" + cr);
                    ps.Append("         $global:script_error = $true" + cr);
                    ps.Append("      }" + cr);
                    ps.Append("   }" + cr);
                    ps.Append("}" + cr);
                    if (currentSubscription.primaryOnly == false)
                    {
                        //Create Secondary Gateway Connection to Local Gateway
                        ps.Append("######################################################" + cr);
                        ps.Append("# Create Secondary Gateway Connection to Local Gateway" + cr);
                        ps.Append("######################################################" + cr);
                        ps.Append("if($global:script_error -eq $false)" + cr);
                        ps.Append("{" + cr);
                        ps.Append("   Write-Host 'Creating Secondary Connection: ' -NoNewLine" + cr);
                        ps.Append("   $scn=Get-AzureRmVirtualNetworkGatewayConnection -Name cn-local-" + currentSubscription.primaryLocation + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                        ps.Append("   if($scn) {Write-Host 'WARNING: Connection already exists--skipping creation'}" + cr);
                        ps.Append("   else" + cr);
                        ps.Append("   {" + cr);
                        string secSkey = CreateSharedKey();
                        ps.Append("      $scn=New-AzureRmVirtualNetworkGatewayConnection -Name cn-local-" + currentSubscription.primaryLocation + " -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -Location " + currentSubscription.secondaryLocation + " -VirtualNetworkGateway1 $pgw -LocalNetworkGateway2 $lgw -ConnectionType IPsec -RoutingWeight 10 -SharedKey '" + secSkey + "' -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                        ps.Append("      if($scn) {Write-Host 'SUCCESS' -ForegroundColor Green}" + cr);
                        ps.Append("      else" + cr);
                        ps.Append("      {" + cr);
                        ps.Append("         Write-Host 'FAILED- Unable to create connection' -ForegroundColor Red" + cr);
                        ps.Append("         $global:script_error = $true" + cr);
                        ps.Append("      }" + cr);
                        ps.Append("   }" + cr);
                        ps.Append("}" + cr);
                        //Create VNet-to-VNet Connection (Primary to Secondary)
                        ps.Append("#######################################################" + cr);
                        ps.Append("# Create VNet-to-VNet Connection (Primary to Secondary)" + cr);
                        ps.Append("#######################################################" + cr);
                        ps.Append("if($global:script_error -eq $false)" + cr);
                        ps.Append("{" + cr);
                        ps.Append("   Write-Host 'Creating VNet-to-VNet Connection (Primary to Secondary): ' -NoNewLine" + cr);
                        ps.Append("   $pvcn=Get-AzureRmVirtualNetworkGatewayConnection -Name cn-local-" + currentSubscription.primaryLocation + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                        ps.Append("   if($pvcn) {Write-Host 'WARNING: Connection already exists--skipping creation'}" + cr);
                        ps.Append("   else" + cr);
                        ps.Append("   {" + cr);
                        string v2vSkey = CreateSharedKey();
                        ps.Append("      $pvcn=New-AzureRmVirtualNetworkGatewayConnection -Name cn-" + currentSubscription.primaryVnetName + "-" + currentSubscription.secondaryVnetName + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -Location " + currentSubscription.primaryLocation + " -VirtualNetworkGateway1 $pgw -VirtualNetworkGateway2 $sgw -ConnectionType Vnet2Vnet -SharedKey '" + v2vSkey + "' -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                        ps.Append("      if($pvcn) {Write-Host 'SUCCESS' -ForegroundColor Green}" + cr);
                        ps.Append("      else" + cr);
                        ps.Append("      {" + cr);
                        ps.Append("         Write-Host 'FAILED- Unable to create connection' -ForegroundColor Red" + cr);
                        ps.Append("         $global:script_error = $true" + cr);
                        ps.Append("      }" + cr);
                        ps.Append("   }" + cr);
                        ps.Append("}" + cr);
                        //Create VNet-to-VNet Connection (Secondary to Primary)
                        ps.Append("#######################################################" + cr);
                        ps.Append("# Create VNet-to-VNet Connection (Secondary to Primary)" + cr);
                        ps.Append("#######################################################" + cr);
                        ps.Append("if($global:script_error -eq $false)" + cr);
                        ps.Append("{" + cr);
                        ps.Append("   Write-Host 'Creating VNet-to-VNet Connection (Secondary to Primary): ' -NoNewLine" + cr);
                        ps.Append("   $svcn=Get-AzureRmVirtualNetworkGatewayConnection -Name cn-local-" + currentSubscription.primaryLocation + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                        ps.Append("   if($svcn) {Write-Host 'WARNING: Connection already exists--skipping creation'}" + cr);
                        ps.Append("   else" + cr);
                        ps.Append("   {" + cr);
                        ps.Append("      $svcn=New-AzureRmVirtualNetworkGatewayConnection -Name cn-" + currentSubscription.secondaryVnetName + "-" + currentSubscription.primaryVnetName + " -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -Location " + currentSubscription.secondaryLocation + " -VirtualNetworkGateway1 $sgw -VirtualNetworkGateway2 $pgw -ConnectionType Vnet2Vnet -SharedKey '" + v2vSkey + "' -ErrorAction Ignore -WarningAction SilentlyContinue" + cr);
                        ps.Append("      if($svcn) {Write-Host 'SUCCESS' -ForegroundColor Green}" + cr);
                        ps.Append("      else" + cr);
                        ps.Append("      {" + cr);
                        ps.Append("         Write-Host 'FAILED- Unable to create connection' -ForegroundColor Red" + cr);
                        ps.Append("         $global:script_error = $true" + cr);
                        ps.Append("      }" + cr);
                        ps.Append("   }" + cr);
                        ps.Append("}" + cr);
                        //Report Script Success or Failure
                        ps.Append("Write-Host" + cr);
                        ps.Append("if($global:script_error) {Write-Host 'ERROR: An Error Has Occurred. Script Halted.' -ForegroundColor Red}" + cr);
                        ps.Append("else {Write-Host 'COMPLETE: Virtual Network Connections Created Sucessfully.' -ForegroundColor Green}" + cr);
                    }
                    break;
                case 3:     //NSGs
                    break;
            }
            psScript = ps.ToString();
            return psScript;
        }
        private string ValidateFields()
        {
            string validateText = string.Empty;
            bool subOK = IsGuid(txtSubscriptionID.Text);
            if (subOK == false) { validateText = "Subscription ID is not Valid"; }
            if (validateText == string.Empty)
            {
                if (radMAC.Checked == false && radMAG.Checked == false) { validateText = "Azure Environment not Selected"; }
            }
            if (validateText == string.Empty)
            {
                if (ckbCreateConnection.Checked)
                {
                    if (txtLocalGWName.Text == string.Empty) { validateText = "Local Connection Name is blank"; }
                    if (validateText == string.Empty)
                    {
                        if (txtEdgeIP.Text == string.Empty) { validateText = "Edge IP Address is blank"; }
                    }
                    if (validateText == string.Empty)
                    {
                        if (txtOPAddress.Text == string.Empty) { validateText = "On-Premises IP Space is blank"; }
                    }
                }
            }
            if (validateText == string.Empty)
            {
                if (radBoth.Checked)
                {
                    if (cboPrimaryLocation.SelectedIndex == -1) { validateText = "Primary Region not Selected"; }
                    if (validateText == string.Empty)
                    {
                        if (cboSecondaryLocation.SelectedIndex == -1) { validateText = "Secondary Region not Selected"; }
                    }
                    if (validateText == string.Empty)
                    {
                        if (cboPrimaryLocation.SelectedIndex == cboSecondaryLocation.SelectedIndex) { validateText = "Primary and Secondary Regions Must Be Different"; }
                    }
                    if (validateText == string.Empty)
                    {
                        if (txtPrimaryRG.Text == string.Empty) { validateText = "Primary Resource Group is blank"; }
                    }
                    if (validateText == string.Empty)
                    {
                        if (txtSecondaryRG.Text == string.Empty) { validateText = "Secondary Resource Group is blank"; }
                    }
                    if (validateText == string.Empty)
                    {
                        if (txtPrimaryVnet.Text == string.Empty) { validateText = "Primary Vnet Name is blank"; }
                    }
                    if (validateText == string.Empty)
                    {
                        if (txtSecondaryVnet.Text == string.Empty) { validateText = "Secondary Vnet Name is blank"; }
                    }
                    if (validateText == string.Empty)
                    {
                        if (txtPrimaryIP.Text == string.Empty) { validateText = "Primary IP Segment is blank"; }
                    }
                    if (validateText == string.Empty)
                    {
                        if (txtSecondaryIP.Text == string.Empty) { validateText = "Secondary IP Segment is blank"; }
                    }
                }
                else
                {
                    if (cboPrimaryLocation.SelectedIndex == -1) { validateText = "Primary Region not Selected"; }
                    if (validateText == string.Empty)
                    {
                        if (txtPrimaryRG.Text == string.Empty) { validateText = "Primary Resource Group is blank"; }
                    }
                    if (validateText == string.Empty)
                    {
                        if (txtPrimaryVnet.Text == string.Empty) { validateText = "Primary Vnet Name is blank"; }
                    }
                    if (validateText == string.Empty)
                    {
                        if (txtPrimaryIP.Text == string.Empty) { validateText = "Primary IP Segment is blank"; }
                    }
                }
            }
            if (validateText == string.Empty)
            {
                complete = true;
                return "Validation Complete";
            }
            else { return validateText; }
        }
        private string CreateSharedKey()
        {
            using (var rijndael = System.Security.Cryptography.Rijndael.Create())
            {
                var key = Convert.ToBase64String(rijndael.Key);
                return key;
            }
        }
        private void btnLoad_Click(object sender, EventArgs e)
        {
            using (var selectFileDiaglog = new OpenFileDialog())
            {
                selectFileDiaglog.DefaultExt = ".json";
                selectFileDiaglog.Filter = "JSON files|*.json";
                if (selectFileDiaglog.ShowDialog() == DialogResult.OK)
                {
                    currentSubscription.fileName = selectFileDiaglog.FileName;
                }
            }
            using (StreamReader r = new StreamReader(currentSubscription.fileName))
            {
                string jsonIn = r.ReadToEnd();
                currentSubscription = JsonConvert.DeserializeObject<Subscription>(jsonIn);
            }
            txtSubscriptionID.Text = currentSubscription.subscriptionID;
            if (currentSubscription.environment == "AzureCloud") { radMAC.Checked = true; }
            else { radMAG.Checked = true; }
            if (currentSubscription.primaryOnly) { radPrimary.Checked = true; }
            else { radBoth.Checked = true; }
            if (currentSubscription.generatePS) { ckbPS.Checked = true; }
            else { ckbPS.Checked = false; }
            var priLoc = locations.Find(l => l.location == currentSubscription.primaryLocation);
            var secLoc = locations.Find(l => l.location == currentSubscription.secondaryLocation);
            cboPrimaryLocation.SelectedIndex = cboPrimaryLocation.FindStringExact(priLoc.displayName);
            cboSecondaryLocation.SelectedIndex = cboSecondaryLocation.FindStringExact(secLoc.displayName);
            txtPrimaryRG.Text = currentSubscription.primaryResourceGroup;
            txtSecondaryRG.Text = currentSubscription.secondaryResourceGroup;
            txtPrimaryVnet.Text = currentSubscription.primaryVnetName;
            txtSecondaryVnet.Text = currentSubscription.secondaryVnetName;
            txtPrimaryIP.Text = currentSubscription.primaryIPSegment;
            txtSecondaryIP.Text = currentSubscription.secondaryIPSegment;
            List<AzureSubnet> priSubs = new List<AzureSubnet>();
            List<AzureSubnet> secSubs = new List<AzureSubnet>();
            for (int ctr = 0; ctr < 8; ctr++)
            {
                AzureSubnet Psub = (AzureSubnet)currentSubscription.primarySubnets[ctr];
                AzureSubnet Ssub = (AzureSubnet)currentSubscription.secondarySubnets[ctr];
                priSubs.Add(Psub);
                secSubs.Add(Ssub);
            }
            //if(currentSubscription.autoIPRange == true) { ckbAutoIP.Checked = true; }
            txtPriIP0.Text = priSubs[0].ipSegment;
            txtPriIP1.Text = priSubs[1].ipSegment;
            txtPriIP2.Text = priSubs[2].ipSegment;
            txtPriIP3.Text = priSubs[3].ipSegment;
            txtPriIP4.Text = priSubs[4].ipSegment;
            txtPriIP5.Text = priSubs[5].ipSegment;
            txtPriIP6.Text = priSubs[6].ipSegment;
            txtPriIP7.Text = priSubs[7].ipSegment;
            txtSubnetName0.Text = priSubs[0].name;
            txtSubnetName1.Text = priSubs[1].name;
            txtSubnetName2.Text = priSubs[2].name;
            txtSubnetName3.Text = priSubs[3].name;
            txtSubnetName4.Text = priSubs[4].name;
            txtSubnetName5.Text = priSubs[5].name;
            txtSubnetName6.Text = priSubs[6].name;
            txtSubnetName7.Text = priSubs[7].name;
            txtSecIP0.Text = secSubs[0].ipSegment;
            txtSecIP1.Text = secSubs[1].ipSegment;
            txtSecIP2.Text = secSubs[2].ipSegment;
            txtSecIP3.Text = secSubs[3].ipSegment;
            txtSecIP4.Text = secSubs[4].ipSegment;
            txtSecIP5.Text = secSubs[5].ipSegment;
            txtSecIP6.Text = secSubs[6].ipSegment;
            txtSecIP7.Text = secSubs[7].ipSegment;
            if(currentSubscription.createGateway) { ckbCreateGateway.Checked = true; }
            else { ckbCreateGateway.Checked = false; }
            if (currentSubscription.createConnection) { ckbCreateConnection.Checked = true; }
            else { ckbCreateConnection.Checked = false; }
            txtLocalGWName.Text = currentSubscription.localGatewayName;
            txtEdgeIP.Text = currentSubscription.edgeIP;
            txtOPAddress.Text = currentSubscription.localAddressSpace;
            txtMessages.Text = "Sucessfully Loaded Existing File";
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            string validation = ValidateFields();
            if (complete)
            {
                txtMessages.ForeColor = Color.Blue;
                txtMessages.BackColor = SystemColors.Control;
                txtMessages.Text = "Validation Complete";
                currentSubscription.subscriptionID = txtSubscriptionID.Text;
                currentSubscription.edgeIP = txtEdgeIP.Text;
                currentSubscription.localGatewayName = txtLocalGWName.Text;
                currentSubscription.localAddressSpace = txtOPAddress.Text;
                currentSubscription.primaryResourceGroup = txtPrimaryRG.Text;
                currentSubscription.secondaryResourceGroup = txtSecondaryRG.Text;
                currentSubscription.primaryVnetName = txtPrimaryVnet.Text;
                currentSubscription.secondaryVnetName = txtSecondaryVnet.Text;
                currentSubscription.primaryIPSegment = txtPrimaryIP.Text;
                currentSubscription.secondaryIPSegment = txtSecondaryIP.Text;
                string[] priCidr = txtPrimaryIP.Text.Split('/');
                currentSubscription.primaryCIDR = "/" + priCidr[1];
                string[] secCidr = txtSecondaryIP.Text.Split('/');
                currentSubscription.secondaryCIDR = "/" + secCidr[1];
                List<AzureSubnet> priSubs = new List<AzureSubnet>();
                AzureSubnet priSub0 = new AzureSubnet(txtSubnetName0.Text, txtPriIP0.Text);
                priSubs.Add(priSub0);
                AzureSubnet priSub1 = new AzureSubnet(txtSubnetName1.Text, txtPriIP1.Text);
                priSubs.Add(priSub1);
                AzureSubnet priSub2 = new AzureSubnet(txtSubnetName2.Text, txtPriIP2.Text);
                priSubs.Add(priSub2);
                AzureSubnet priSub3 = new AzureSubnet(txtSubnetName3.Text, txtPriIP3.Text);
                priSubs.Add(priSub3);
                AzureSubnet priSub4 = new AzureSubnet(txtSubnetName4.Text, txtPriIP4.Text);
                priSubs.Add(priSub4);
                AzureSubnet priSub5 = new AzureSubnet(txtSubnetName5.Text, txtPriIP5.Text);
                priSubs.Add(priSub5);
                AzureSubnet priSub6 = new AzureSubnet(txtSubnetName6.Text, txtPriIP6.Text);
                priSubs.Add(priSub6);
                AzureSubnet priSub7 = new AzureSubnet(txtSubnetName7.Text, txtPriIP7.Text);
                priSubs.Add(priSub7);
                List<AzureSubnet> secSubs = new List<AzureSubnet>();
                AzureSubnet secSub0 = new AzureSubnet(txtSubnetName0.Text, txtSecIP0.Text);
                secSubs.Add(secSub0);
                AzureSubnet secSub1 = new AzureSubnet(txtSubnetName1.Text, txtSecIP1.Text);
                secSubs.Add(secSub1);
                AzureSubnet secSub2 = new AzureSubnet(txtSubnetName2.Text, txtSecIP2.Text);
                secSubs.Add(secSub2);
                AzureSubnet secSub3 = new AzureSubnet(txtSubnetName3.Text, txtSecIP3.Text);
                secSubs.Add(secSub3);
                AzureSubnet secSub4 = new AzureSubnet(txtSubnetName4.Text, txtSecIP4.Text);
                secSubs.Add(secSub4);
                AzureSubnet secSub5 = new AzureSubnet(txtSubnetName5.Text, txtSecIP5.Text);
                secSubs.Add(secSub5);
                AzureSubnet secSub6 = new AzureSubnet(txtSubnetName6.Text, txtSecIP6.Text);
                secSubs.Add(secSub6);
                AzureSubnet secSub7 = new AzureSubnet(txtSubnetName7.Text, txtSecIP7.Text);
                secSubs.Add(secSub7);
                currentSubscription.primarySubnets = priSubs;
                currentSubscription.secondarySubnets = secSubs;
                if (currentSubscription.primaryOnly == false) { vVpnKey = CreateSharedKey(); }
                using (var selectFileDialog = new SaveFileDialog())
                {
                    if (selectFileDialog.ShowDialog() == DialogResult.OK) { currentSubscription.fileName = selectFileDialog.FileName; }
                }
                try
                {
                    string saveJson = currentSubscription.fileName + "-foundationseditor.json";
                    string jsonOut = JsonConvert.SerializeObject(currentSubscription);
                    File.WriteAllText(saveJson, jsonOut);
                    if (currentSubscription.generatePS)
                    {
                        string psScript = CreatePowerShellScript(1);
                        string savePs = currentSubscription.fileName + "-vnets.ps1";
                        File.WriteAllText(savePs, psScript);
                        if (ckbCreateConnection.Checked)
                        {
                            string psScript2 = CreatePowerShellScript(2);
                            string savePs2 = currentSubscription.fileName + "-connections.ps1";
                            File.WriteAllText(savePs2, psScript2);
                        }
                    }
                    string primaryVnet = CreateARMTemplate(1);
                    string saveArm = currentSubscription.fileName + "-primaryvnet.json";
                    File.WriteAllText(saveArm, primaryVnet);
                    string primaryNSG = CreateNSG(1);
                    string savePrimaryNSG = currentSubscription.fileName + "-primarynsg.json";
                    File.WriteAllText(savePrimaryNSG, primaryNSG);
                    if (currentSubscription.primaryOnly == false)
                    {
                        string secondaryVnet = CreateARMTemplate(2);
                        string saveSecondaryVnet = currentSubscription.fileName + "-secondaryvnet.json";
                        File.WriteAllText(saveSecondaryVnet, secondaryVnet);
                        string secondaryNSG = CreateNSG(2);
                        string saveSecondaryNSG = currentSubscription.fileName + "-secondarynsg.json";
                        File.WriteAllText(saveSecondaryNSG, secondaryNSG);
                    }
                    string primaryARMPS = CreateARMDeploymentScript();
                    string savePrimaryARMPS = currentSubscription.fileName + "-deployARM.ps1";
                    File.WriteAllText(savePrimaryARMPS, primaryARMPS);
                    txtMessages.ForeColor = Color.Blue;
                    txtMessages.BackColor = SystemColors.Control;
                    txtMessages.Text = "Files Saved Sucessfully";
                }
                catch
                {
                    txtMessages.ForeColor = Color.Red;
                    txtMessages.BackColor = Color.Black;
                    txtMessages.Text = "Files Were Not Saved";
                }
            }
            else
            {
                txtMessages.ForeColor = Color.Red;
                txtMessages.BackColor = Color.Black;
                txtMessages.Text = validation;
            }

        }
        public bool ValidateIPv4(string ipString)
        {
            if (String.IsNullOrWhiteSpace(ipString)) { return false; }
            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4) { return false; }
            byte tempForParsing;
            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }
        private void parseCIDR(string _cidr)
        {
            string[] ipCidr = _cidr.Split('/');
            if (ValidateIPv4(ipCidr[0]))
            {
                primaryIP.ipSegment = ipCidr[0];
                primaryIP.cidr = "/" + ipCidr[1];
                if (primaryIP.cidr == "/20") { currentSubscription.ipSeparation = 32; }
                else { currentSubscription.ipSeparation = 16; }
                string[] segments = primaryIP.ipSegment.Split('.');
                primaryIP.segment0 = Convert.ToInt16(segments[0]);
                primaryIP.segment1 = Convert.ToInt16(segments[1]);
                primaryIP.segment2 = Convert.ToInt16(segments[2]);
                primaryIP.segment3 = Convert.ToInt16(segments[3]);
                if (radBoth.Checked)
                {
                    if (primaryIP.cidr == "/21")
                    {
                        secondaryIP.segment0 = primaryIP.segment0;
                        secondaryIP.segment1 = primaryIP.segment1;
                        secondaryIP.segment2 = primaryIP.segment2 + currentSubscription.ipSeparation;
                        secondaryIP.segment3 = primaryIP.segment3;
                        secondaryIP.ipSegment = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + secondaryIP.segment2.ToString() + "." + secondaryIP.segment3.ToString();
                        secondaryIP.cidr = "/21";
                        txtSecondaryIP.Text = secondaryIP.ipSegment + secondaryIP.cidr;
                        txtMessages.Text = "IP Automatic Ranging Enabled";
                    }
                    else if (primaryIP.cidr == "/20")
                    {
                        secondaryIP.segment0 = primaryIP.segment0;
                        secondaryIP.segment1 = primaryIP.segment1;
                        secondaryIP.segment2 = primaryIP.segment2 + currentSubscription.ipSeparation;
                        secondaryIP.segment3 = primaryIP.segment3;
                        secondaryIP.ipSegment = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + secondaryIP.segment2.ToString() + "." + secondaryIP.segment3.ToString();
                        secondaryIP.cidr = "/20";
                        txtSecondaryIP.Text = secondaryIP.ipSegment + secondaryIP.cidr;
                        txtMessages.Text = "IP Automatic Ranging Enabled";
                    }
                    else
                    {
                        txtMessages.ForeColor = Color.Blue;
                        txtMessages.Text = "IP Ranges Entered Manually if Not /21 or /20";
                        ckbAutoIP.Checked = false;
                    }
                }
            }
            else
            {
                txtMessages.ForeColor = Color.Red;
                txtMessages.Text = "Invalid IP Range Entered";
            }
        }
        private void CreateSubnets()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            using (StreamReader r = new StreamReader(currentDirectory + "\\subnets.json"))
            {
                string jsonIn = r.ReadToEnd();
                subnets = JsonConvert.DeserializeObject<List<AzureSubnet>>(jsonIn);
            }
            txtSubnetName0.Text = subnets[0].name;
            txtSubnetName1.Text = subnets[1].name;
            txtSubnetName2.Text = subnets[2].name;
            txtSubnetName3.Text = subnets[3].name;
            txtSubnetName4.Text = subnets[4].name;
            txtSubnetName5.Text = subnets[5].name;
            txtSubnetName6.Text = subnets[6].name;
            txtSubnetName7.Text = subnets[7].name;
            txtSubnetName0.ReadOnly = true;
        }
        private void CreateAzureLocations()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            using (StreamReader r = new StreamReader(currentDirectory + "\\locations.json"))
            {
                string jsonIn = r.ReadToEnd();
                locations = JsonConvert.DeserializeObject<List<AzureLocation>>(jsonIn);
            }
        }

        private void radMAC_CheckedChanged_1(object sender, EventArgs e)
        {
            txtMessages.ForeColor = Color.Blue;
            txtMessages.BackColor = SystemColors.Control;
            txtMessages.Text = "Microsoft Azure Commercial selected.";
            cboPrimaryLocation.Items.Clear();
            cboSecondaryLocation.Items.Clear();
            var locs = locations.Where(l => l.environment == "AzureCloud");
            foreach (var AzureLocation in locs)
            {
                cboPrimaryLocation.Items.Add(AzureLocation.displayName);
                cboSecondaryLocation.Items.Add(AzureLocation.displayName);
            }
            currentSubscription.environment = "AzureCloud";
        }

        private void radMAG_CheckedChanged(object sender, EventArgs e)
        {
            txtMessages.ForeColor = Color.Blue;
            txtMessages.BackColor = SystemColors.Control;
            txtMessages.Text = "Microsoft Azure Government selected.";
            cboPrimaryLocation.Items.Clear();
            cboSecondaryLocation.Items.Clear();
            var locs = locations.Where(l => l.environment == "AzureUSGovernment");
            foreach (var AzureLocation in locs)
            {
                cboPrimaryLocation.Items.Add(AzureLocation.displayName);
                cboSecondaryLocation.Items.Add(AzureLocation.displayName);
            }
            currentSubscription.environment = "AzureUSGovernment";
        }

        private void radPrimary_CheckedChanged(object sender, EventArgs e)
        {
            cboSecondaryLocation.Enabled = false;
            txtSecondaryRG.Enabled = false;
            txtSecondaryVnet.Enabled = false;
            txtSecondaryIP.Enabled = false;
            txtSecIP0.Enabled = false;
            txtSecIP1.Enabled = false;
            txtSecIP2.Enabled = false;
            txtSecIP3.Enabled = false;
            txtSecIP4.Enabled = false;
            txtSecIP5.Enabled = false;
            txtSecIP6.Enabled = false;
            txtSecIP7.Enabled = false;
            currentSubscription.primaryOnly = true;
        }

        private void radBoth_CheckedChanged(object sender, EventArgs e)
        {
            cboSecondaryLocation.Enabled = true;
            txtSecondaryRG.Enabled = true;
            txtSecondaryVnet.Enabled = true;
            txtSecondaryIP.Enabled = true;
            txtSecIP0.Enabled = true;
            txtSecIP1.Enabled = true;
            txtSecIP2.Enabled = true;
            txtSecIP3.Enabled = true;
            txtSecIP4.Enabled = true;
            txtSecIP5.Enabled = true;
            txtSecIP6.Enabled = true;
            txtSecIP7.Enabled = true;
            currentSubscription.primaryOnly = false;
        }

        private void ckbCreateConnection_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbCreateConnection.Checked)
            {
                currentSubscription.createConnection = true;
            }
            else
            {
                currentSubscription.createConnection = false;
            }
        }

        private void ckbAutoIP_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbAutoIP.Checked)
            {
                parseCIDR(txtPrimaryIP.Text);
                if(primaryIP.cidr == "/21")
                {
                    currentSubscription.autoIPRange = true;
                    txtPriIP0.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".0.224/27";
                    txtPriIP1.Text= primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".0.0/25";
                    txtPriIP2.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".0.128/26";
                    txtPriIP3.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".0.192/27";
                    txtPriIP4.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".1.0/25";
                    txtPriIP5.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".2.0/23";
                    txtPriIP6.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".4.0/23";
                    txtPriIP7.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".6.0/23";
                    if (radBoth.Checked)
                    {
                        txtSecIP0.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + secondaryIP.segment2.ToString() + ".224/27";
                        txtSecIP1.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + secondaryIP.segment2.ToString() + ".0/25";
                        txtSecIP2.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + secondaryIP.segment2.ToString() + ".128/26";
                        txtSecIP3.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + secondaryIP.segment2.ToString() + ".192/27";
                        txtSecIP4.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + (secondaryIP.segment2 + 1).ToString() + ".0/25";
                        txtSecIP5.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + (secondaryIP.segment2 + 2).ToString() + ".0/23";
                        txtSecIP6.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + (secondaryIP.segment2 + 4).ToString() + ".0/23";
                        txtSecIP7.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + (secondaryIP.segment2 + 6).ToString() + ".0/23";
                    }
                }
                else if (primaryIP.cidr == "/20")
                {
                    currentSubscription.autoIPRange = true;
                    txtPriIP0.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".0.0/25";
                    txtPriIP1.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".1.0/24";
                    txtPriIP2.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".2.0/25";
                    txtPriIP3.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".2.128/25";
                    txtPriIP4.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".3.0/25";
                    txtPriIP5.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".4.0/22";
                    txtPriIP6.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".8.0/22";
                    txtPriIP7.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".12.0/22";
                    if (radBoth.Checked)
                    {
                        txtSecIP0.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + secondaryIP.segment2.ToString() + ".0/25";
                        txtSecIP1.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + (secondaryIP.segment2 + 1).ToString() + ".0/24";
                        txtSecIP2.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + (secondaryIP.segment2 + 2).ToString() + ".0/25";
                        txtSecIP3.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + (secondaryIP.segment2 + 2).ToString() + ".128/25";
                        txtSecIP4.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + (secondaryIP.segment2 + 3).ToString() + ".0/25";
                        txtSecIP5.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + (secondaryIP.segment2 + 4).ToString() + ".0/22";
                        txtSecIP6.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + (secondaryIP.segment2 + 8).ToString() + ".0/22";
                        txtSecIP7.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + (secondaryIP.segment2 + 12).ToString() + ".0/22";
                    }
                }
                else
                {
                    currentSubscription.autoIPRange = false;
                    txtMessages.ForeColor = Color.Blue;
                    txtMessages.BackColor = SystemColors.Control;
                    txtMessages.Text = "IP Ranges Entered Manually if Not /21 or /20";
                    txtPriIP0.Text = string.Empty;
                    txtPriIP1.Text = string.Empty;
                    txtPriIP2.Text = string.Empty;
                    txtPriIP3.Text = string.Empty;
                    txtPriIP4.Text = string.Empty;
                    txtPriIP5.Text = string.Empty;
                    txtPriIP6.Text = string.Empty;
                    txtPriIP7.Text = string.Empty;
                    txtSecIP0.Text = string.Empty;
                    txtSecIP1.Text = string.Empty;
                    txtSecIP2.Text = string.Empty;
                    txtSecIP3.Text = string.Empty;
                    txtSecIP4.Text = string.Empty;
                    txtSecIP5.Text = string.Empty;
                    txtSecIP6.Text = string.Empty;
                    txtSecIP7.Text = string.Empty;
                    txtSecondaryIP.Text = string.Empty;
                    ckbAutoIP.Checked = false;
                }
            }
        }

        private void cboPrimaryLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            var loc = locations.FirstOrDefault(l => l.displayName == cboPrimaryLocation.Text);
            currentSubscription.primaryLocation = loc.location;
            var locPair = locations.FirstOrDefault(p => p.location == loc.geoPair);
            cboSecondaryLocation.SelectedIndex = cboSecondaryLocation.FindStringExact(locPair.displayName);
        }

        private void cboSecondaryLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            var loc = locations.FirstOrDefault(l => l.displayName == cboSecondaryLocation.Text);
            currentSubscription.secondaryLocation = loc.location;
        }

        private void ckbCreateGateway_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbCreateGateway.Checked)
            {
                lblEdgeIP.Visible = true;
                lblLocalGWName.Visible = true;
                lblOPAddress.Visible = true;
                txtEdgeIP.Visible = true;
                txtLocalGWName.Visible = true;
                txtOPAddress.Visible = true;
                currentSubscription.createGateway = true;
            }
            else
            {
                lblEdgeIP.Visible = false;
                lblLocalGWName.Visible = false;
                lblOPAddress.Visible = false;
                txtEdgeIP.Visible = false;
                txtLocalGWName.Visible = false;
                txtOPAddress.Visible = false;
                currentSubscription.createGateway = false;
            }
        }

        private void ckbPS_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbPS.Checked) { currentSubscription.generatePS = true; }
            else { currentSubscription.generatePS = false; }
        }

        private void picAzure_Click(object sender, EventArgs e)
        {
            if (ckbPS.Visible)
            {
                ckbPS.Visible = false;
                lblPS.Visible = false;
            }
            else
            {
                ckbPS.Visible = true;
                lblPS.Visible = true;
            }
        }
    }
}
