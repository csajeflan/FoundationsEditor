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
            txtMessages.ForeColor = Color.Blue;
            txtMessages.BackColor = SystemColors.Control;
            txtMessages.Text = "Enter your Subscription ID or select LOAD to begin.";
            lblBuild.Text = "Build: " + buildNumber + Environment.NewLine + "06-MAR-2018";
            this.Text = "Azure Foundations Editor - " + buildNumber;
        }
        public static string buildNumber = "1.0.4.0";
        public static Subscription currentSubscription = new Subscription();
        List<AzureSubnet> subnets = new List<AzureSubnet>();
        List<AzureLocation> locations = new List<AzureLocation>();
        public static IPSegment primaryIP = new IPSegment();
        public static IPSegment secondaryIP = new IPSegment();
        public static bool complete = false;
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
        private string CreateARMTemplate(int _scr)
        {
            string cr = Environment.NewLine;
            string armTemplate = string.Empty;
            StringBuilder at = new StringBuilder();
            switch (_scr)
            {
                case 1:
                    at.Append("{" + cr);
                    at.Append("   \"$schema\": \"http://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#\"," + cr);
                    at.Append("   \"contentVersion\": \"1.0.0.0\"," + cr);
                    at.Append("   \"parameters\": { }," + cr);
                    at.Append("   \"variables\": {" + cr);
                    at.Append("      \"vnetID\": \"[resourceId('Microsoft.Network/virtualNetworks','" + currentSubscription.primaryVnetName + "')]\"," + cr);
                    at.Append("      \"gatewaySubnetRef\": \"[concat(variables('vnetID'),'/subnets/','GatewaySubnet')]\"" + cr);
                    at.Append("   }," + cr);
                    at.Append("   \"resources\": [" + cr);
                    at.Append("      {" + cr);
                    at.Append("         \"name\": \"" + currentSubscription.localGatewayName + "-" + currentSubscription.primaryLocation + "\"," + cr);
                    at.Append("         \"type\": \"Microsoft.Network/localNetworkGateways\"," + cr);
                    at.Append("         \"apiVersion\": \"2017-10-01\"," + cr);
                    at.Append("         \"location\": \"[resourceGroup().location]\"," + cr);
                    at.Append("         \"properties\": {" + cr);
                    at.Append("            \"localNetworkAddressSpace\": {" + cr);
                    at.Append("               \"addressPrefixes\": [" + cr);
                    at.Append("                  \"" + currentSubscription.localAddressSpace + "\"" + cr);
                    at.Append("               ]" + cr);
                    at.Append("            }," + cr);
                    at.Append("            \"gatewayIpAddress\": \"" + currentSubscription.edgeIP + "\"" + cr);
                    at.Append("         }" + cr);
                    at.Append("      }," + cr);
                    at.Append("      {" + cr);
                    at.Append("         \"name\": \"" + currentSubscription.primaryVnetName + "\"," + cr);
                    at.Append("         \"type\": \"Microsoft.Network/virtualNetworks\"," + cr);
                    at.Append("         \"apiVersion\": \"2017-10-01\"," + cr);
                    at.Append("         \"location\": \"[resourceGroup().location]\"," + cr);
                    at.Append("         \"properties\": {" + cr);
                    at.Append("            \"addressSpace\": {" + cr);
                    at.Append("               \"addressPrefixes\": [" + cr);
                    at.Append("                  \"" + currentSubscription.primaryIPSegment + "\"" + cr);
                    at.Append("               ]" + cr);
                    at.Append("            }," + cr);
                    at.Append("            \"subnets\": [" + cr);
                    for (int ctr = 0; ctr < 8; ctr++)
                    {
                        if (currentSubscription.primarySubnets[ctr].ipSegment != string.Empty)
                        {
                            at.Append("               {" + cr);
                            at.Append("                  \"name\": \"" + currentSubscription.primarySubnets[ctr].name + "\"," + cr);
                            at.Append("                  \"properties\": {" + cr);
                            at.Append("                     \"addressPrefix\": \"" + currentSubscription.primarySubnets[ctr].ipSegment + "\"" + cr);
                            at.Append("                  }" + cr);
                            if (ctr == 7) { at.Append("               }" + cr); }
                            else { at.Append("               }," + cr); }
                        }
                    }
                    at.Append("            ]" + cr);
                    at.Append("         }" + cr);
                    at.Append("      }," + cr);
                    at.Append("      {" + cr);
                    at.Append("         \"name\": \"" + currentSubscription.primaryVnetName + "-gw-ip" + "\"," + cr);
                    at.Append("         \"type\": \"Microsoft.Network/publicIPAddresses\"," + cr);
                    at.Append("         \"apiVersion\": \"2017-10-01\"," + cr);
                    at.Append("         \"location\": \"[resourceGroup().location]\"," + cr);
                    at.Append("         \"properties\": {" + cr);
                    at.Append("            \"publicIPAllocationMethod\": \"Dynamic\"" + cr);
                    at.Append("         }" + cr);
                    at.Append("      }," + cr);
                    at.Append("      {" + cr);
                    at.Append("         \"name\": \"" + currentSubscription.primaryVnetName + "-gw" + "\"," + cr);
                    at.Append("         \"type\": \"Microsoft.Network/virtualNetworkGateways\"," + cr);
                    at.Append("         \"apiVersion\": \"2017-10-01\"," + cr);
                    at.Append("         \"location\": \"[resourceGroup().location]\"," + cr);
                    at.Append("         \"dependsOn\": [" + cr);
                    at.Append("            \"[concat('Microsoft.Network/publicIpAddresses/','" + currentSubscription.primaryVnetName + "-gw-ip')]" + "\"," + cr);
                    at.Append("            \"[concat('Microsoft.Network/virtualNetworks/','" + currentSubscription.primaryVnetName + "')]" + "\"" + cr);
                    at.Append("         ]," + cr);
                    at.Append("         \"properties\": {" + cr);
                    at.Append("            \"ipConfigurations\": [" + cr);
                    at.Append("               {" + cr);
                    at.Append("                  \"properties\": {" + cr);
                    at.Append("                     \"privateIPAllocationMethod\": \"Dynamic\"," + cr);
                    at.Append("                     \"subnet\": {" + cr);
                    at.Append("                        \"id\": \"[variables('gatewaySubnetRef')]\"" + cr);
                    at.Append("                     }," + cr);
                    at.Append("                     \"publicIPAddress\": {" + cr);
                    at.Append("                        \"id\": \"[resourceId('Microsoft.Network/publicIPAddresses','" + currentSubscription.primaryVnetName + "-gw-ip')]\"" + cr);
                    at.Append("                     }" + cr);
                    at.Append("                  }," + cr);
                    at.Append("                  \"name\": \"vnetGatewayConfig\"" + cr);
                    at.Append("               }" + cr);
                    at.Append("            ]," + cr);
                    at.Append("            \"gatewayType\": \"Vpn\"," + cr);
                    at.Append("            \"vpnType\": \"RouteBased\"," + cr);
                    at.Append("            \"enableBgp\": false" + cr);
                    at.Append("         }" + cr);
                    at.Append("      }" + cr);
                    //at.Append("      {" + cr);
                    //at.Append("         \"name\": \"" + currentSubscription.localConnectionName + "-" + currentSubscription.primaryLocation + "\"," + cr);
                    //at.Append("         \"type\": \"Microsoft.Network/connections\"," + cr);
                    //at.Append("         \"apiVersion\": \"2017-10-01\"," + cr);
                    //at.Append("         \"location\": \"[resourceGroup().location]\"," + cr);
                    //at.Append("         \"properties\": {" + cr);
                    //at.Append("            \"virtualNetworkGateway1\": {" + cr);
                    //at.Append("               \"addressPrefixes\": [" + cr);
                    //at.Append("                  \"" + currentSubscription.localAddressSpace + "\"" + cr);
                    //at.Append("               ]" + cr);
                    //at.Append("            }," + cr);
                    //at.Append("            \"gatewayIpAddress\": \"" + currentSubscription.edgeIP + "\"" + cr);
                    //at.Append("         }" + cr);
                    //at.Append("      }," + cr);
                    at.Append("   ]" + cr);
                    at.Append("}" + cr);
                    break;
                case 2:
                    at.Append("{" + cr);
                    at.Append("   \"$schema\": \"http://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#\"," + cr);
                    at.Append("   \"contentVersion\": \"1.0.0.0\"," + cr);
                    at.Append("   \"parameters\": { }," + cr);
                    at.Append("   \"variables\": {" + cr);
                    at.Append("      \"vnetID\": \"[resourceId('Microsoft.Network/virtualNetworks','" + currentSubscription.secondaryVnetName + "')]\"," + cr);
                    at.Append("      \"gatewaySubnetRef\": \"[concat(variables('vnetID'),'/subnets/','GatewaySubnet')]\"" + cr);
                    at.Append("   }," + cr);
                    at.Append("   \"resources\": [" + cr);
                    at.Append("      {" + cr);
                    at.Append("         \"name\": \"" + currentSubscription.localGatewayName + "-" + currentSubscription.secondaryLocation + "\"," + cr);
                    at.Append("         \"type\": \"Microsoft.Network/localNetworkGateways\"," + cr);
                    at.Append("         \"apiVersion\": \"2017-10-01\"," + cr);
                    at.Append("         \"location\": \"[resourceGroup().location]\"," + cr);
                    at.Append("         \"properties\": {" + cr);
                    at.Append("            \"localNetworkAddressSpace\": {" + cr);
                    at.Append("               \"addressPrefixes\": [" + cr);
                    at.Append("                  \"" + currentSubscription.localAddressSpace + "\"" + cr);
                    at.Append("               ]" + cr);
                    at.Append("            }," + cr);
                    at.Append("            \"gatewayIpAddress\": \"" + currentSubscription.edgeIP + "\"" + cr);
                    at.Append("         }" + cr);
                    at.Append("      }," + cr);
                    at.Append("      {" + cr);
                    at.Append("         \"name\": \"" + currentSubscription.secondaryVnetName + "\"," + cr);
                    at.Append("         \"type\": \"Microsoft.Network/virtualNetworks\"," + cr);
                    at.Append("         \"apiVersion\": \"2017-10-01\"," + cr);
                    at.Append("         \"location\": \"[resourceGroup().location]\"," + cr);
                    at.Append("         \"properties\": {" + cr);
                    at.Append("            \"addressSpace\": {" + cr);
                    at.Append("               \"addressPrefixes\": [" + cr);
                    at.Append("                  \"" + currentSubscription.secondaryIPSegment + "\"" + cr);
                    at.Append("               ]" + cr);
                    at.Append("            }," + cr);
                    at.Append("            \"subnets\": [" + cr);
                    for (int ctr = 0; ctr < 8; ctr++)
                    {
                        if (currentSubscription.primarySubnets[ctr].ipSegment != string.Empty)
                        {
                            at.Append("               {" + cr);
                            at.Append("                  \"name\": \"" + currentSubscription.secondarySubnets[ctr].name + "\"," + cr);
                            at.Append("                  \"properties\": {" + cr);
                            at.Append("                     \"addressPrefix\": \"" + currentSubscription.secondarySubnets[ctr].ipSegment + "\"" + cr);
                            at.Append("                  }" + cr);
                            if (ctr == 7) { at.Append("               }" + cr); }
                            else { at.Append("               }," + cr); }
                        }
                    }
                    at.Append("            ]" + cr);
                    at.Append("         }" + cr);
                    at.Append("      }," + cr);
                    at.Append("      {" + cr);
                    at.Append("         \"name\": \"" + currentSubscription.secondaryVnetName + "-gw-ip" + "\"," + cr);
                    at.Append("         \"type\": \"Microsoft.Network/publicIPAddresses\"," + cr);
                    at.Append("         \"apiVersion\": \"2017-10-01\"," + cr);
                    at.Append("         \"location\": \"[resourceGroup().location]\"," + cr);
                    at.Append("         \"properties\": {" + cr);
                    at.Append("            \"publicIPAllocationMethod\": \"Dynamic\"" + cr);
                    at.Append("         }" + cr);
                    at.Append("      }," + cr);
                    at.Append("      {" + cr);
                    at.Append("         \"name\": \"" + currentSubscription.secondaryVnetName + "-gw" + "\"," + cr);
                    at.Append("         \"type\": \"Microsoft.Network/virtualNetworkGateways\"," + cr);
                    at.Append("         \"apiVersion\": \"2017-10-01\"," + cr);
                    at.Append("         \"location\": \"[resourceGroup().location]\"," + cr);
                    at.Append("         \"dependsOn\": [" + cr);
                    at.Append("            \"[concat('Microsoft.Network/publicIpAddresses/','" + currentSubscription.secondaryVnetName + "-gw-ip')]" + "\"," + cr);
                    at.Append("            \"[concat('Microsoft.Network/virtualNetworks/','" + currentSubscription.secondaryVnetName + "')]" + "\"" + cr);
                    at.Append("         ]," + cr);
                    at.Append("         \"properties\": {" + cr);
                    at.Append("            \"ipConfigurations\": [" + cr);
                    at.Append("               {" + cr);
                    at.Append("                  \"properties\": {" + cr);
                    at.Append("                     \"privateIPAllocationMethod\": \"Dynamic\"," + cr);
                    at.Append("                     \"subnet\": {" + cr);
                    at.Append("                        \"id\": \"[variables('gatewaySubnetRef')]\"" + cr);
                    at.Append("                     }," + cr);
                    at.Append("                     \"publicIPAddress\": {" + cr);
                    at.Append("                        \"id\": \"[resourceId('Microsoft.Network/publicIPAddresses','" + currentSubscription.secondaryVnetName + "-gw-ip')]\"" + cr);
                    at.Append("                     }" + cr);
                    at.Append("                  }," + cr);
                    at.Append("                  \"name\": \"vnetGatewayConfig\"" + cr);
                    at.Append("               }" + cr);
                    at.Append("            ]," + cr);
                    at.Append("            \"gatewayType\": \"Vpn\"," + cr);
                    at.Append("            \"vpnType\": \"RouteBased\"," + cr);
                    at.Append("            \"enableBgp\": false" + cr);
                    at.Append("         }" + cr);
                    at.Append("      }" + cr);
                    at.Append("   ]" + cr);
                    at.Append("}" + cr);
                    break;
                case 3:
                    break;
            }
            armTemplate = at.ToString();
            return armTemplate;
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
                    ps.Append("else {Write-Host 'SUCCESS'}" + cr);
                    //Create Primary Virtual Network
                    ps.Append("################################################" + cr);
                    ps.Append("# Create Primary Virtual Network" + cr);
                    ps.Append("################################################" + cr);
                    ps.Append("if($global:script_error -eq $false)" + cr);
                    ps.Append("{" + cr);
                    ps.Append("   Write-Host 'Creating Primary Virtual Network: ' -NoNewLine" + cr);
                    ps.Append("   $pvn=Get-AzureRmVirtualNetwork -Name " + currentSubscription.primaryVnetName + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + cr);
                    ps.Append("   if($pvn) {Write-Host 'WARNING: VNet already exists--skipping creation'}" + cr);
                    ps.Append("   else" + cr);
                    ps.Append("   {" + cr);
                    ps.Append("      New-AzureRmVirtualNetwork -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -Name " + currentSubscription.primaryVnetName + " -AddressPrefix " + currentSubscription.primaryIPSegment + " -Location " + currentSubscription.primaryLocation + " -ErrorAction Ignore | Out-Null" + cr);
                    ps.Append("      Write-Host 'Creating Primary Virtual Network: ' -NoNewLine" + cr);
                    ps.Append("      $pvn=Get-AzureRmVirtualNetwork -Name " + currentSubscription.primaryVnetName + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + cr);
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
                    ps.Append("   if($pvnExists) {Write-Host 'WARNING: Subnets already exist--skipping creation'}" + cr);
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
                        ps.Append("   $svn=Get-AzureRmVirtualNetwork -Name " + currentSubscription.secondaryVnetName + " -ResourceGroupName " + currentSubscription.secondaryResourceGroup + cr);
                        ps.Append("   if($svn)" + cr);
                        ps.Append("   {" + cr);
                        ps.Append("      Write-Host 'WARNING: VNet already exists--skipping creation'" + cr);
                        ps.Append("      $svnExists = $true" + cr);
                        ps.Append("   }" + cr);
                        ps.Append("   else" + cr);
                        ps.Append("   {" + cr);
                        ps.Append("      New-AzureRmVirtualNetwork -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -Name " + currentSubscription.secondaryVnetName + " -AddressPrefix " + currentSubscription.secondaryIPSegment + " -Location " + currentSubscription.secondaryLocation + " -ErrorAction Ignore | Out-Null" + cr);
                        ps.Append("      Write-Host 'Creating Secondary Virtual Network: ' -NoNewLine" + cr);
                        ps.Append("      $svn=Get-AzureRmVirtualNetwork -Name " + currentSubscription.secondaryVnetName + " -ResourceGroupName " + currentSubscription.secondaryResourceGroup + cr);
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
                        ps.Append("   if($svnExists) {Write-Host 'WARNING: Subnets already exist--skipping creation'}" + cr);
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
                    ps.Append("      New-AzureRmLocalNetworkGateway -Name " + currentSubscription.localGatewayName + "-" + currentSubscription.primaryLocation + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -Location " + currentSubscription.primaryLocation + " -GatewayIpAddress " + currentSubscription.edgeIP + " -AddressPrefix " + currentSubscription.localAddressSpace + " -ErrorAction Ignore | Out-Null" + cr);
                    ps.Append("      Write-Host 'Creating Primary Local Network Gateway: ' -NoNewLine" + cr);
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
                    ps.Append("   $pgwip=Get-AzureRmPublicIpAddress -Name " + currentSubscription.primaryVnetName + "-gw-ip -ResourceGroupName " + currentSubscription.primaryResourceGroup + cr);
                    ps.Append("   if($pgwip) {Write-Host 'WARNING: Public IP already exists--skipping creation'}" + cr);
                    ps.Append("   else" + cr);
                    ps.Append("   {" + cr);
                    ps.Append("      New-AzureRmPublicIpAddress -Name " + currentSubscription.primaryVnetName + "-gw-ip -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -Location " + currentSubscription.primaryLocation + " -AllocationMethod Dynamic -ErrorAction Ignore | Out-Null" + cr);
                    ps.Append("      Write-Host 'Requesting Primary Gateway IP Address: ' -NoNewLine" + cr);
                    ps.Append("      $pgwip=Get-AzureRmPublicIpAddress -Name " + currentSubscription.primaryVnetName + "-gw-ip -ResourceGroupName " + currentSubscription.primaryResourceGroup + cr);
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
                    ps.Append("   Write-Host 'NOTE: Azure Gateway Creation Takes Approximately 20 Minutes!'" + cr);
                    ps.Append("   Write-Host 'Creating Primary Azure Gateway: ' -NoNewLine" + cr);
                    ps.Append("   $pgw=Get-AzureRmPublicIpAddress -Name " + currentSubscription.primaryVnetName + "-gw-ip -ResourceGroupName " + currentSubscription.primaryResourceGroup + cr);
                    ps.Append("   if($pgwip) {Write-Host 'WARNING: Vnet gateway already exists--skipping creation'}" + cr);
                    ps.Append("   else" + cr);
                    ps.Append("   {" + cr);
                    ps.Append("      $psn=Get-AzureRmVirtualNetworkSubnetConfig -Name 'GatewaySubnet' -VirtualNetwork $pvn" + cr);
                    ps.Append("      $pgwipcfg=New-AzureRmVirtualNetworkGatewayIpConfig -Name pgwipconfig1 -SubnetId $psn.Id -PublicIpAddressId $pgwip.Id" + cr);
                    ps.Append("      $pgw=New-AzureRmVirtualNetworkGateway -Name " + currentSubscription.primaryVnetName + "-gw -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -Location " + currentSubscription.primaryLocation + " -IpConfigurations $pgwipcfg -GatewayType Vpn -VpnType RouteBased -GatewaySku VpnGw1 -ErrorAction Ignore" + cr);
                    ps.Append("      Write-Host 'Creating Primary Azure Gateway: ' -NoNewLine" + cr);
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
                        ps.Append("      New-AzureRmLocalNetworkGateway -Name " + currentSubscription.localGatewayName + "-" + currentSubscription.secondaryLocation + " -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -Location " + currentSubscription.secondaryLocation + " -GatewayIpAddress " + currentSubscription.edgeIP + " -AddressPrefix " + currentSubscription.localAddressSpace + " -ErrorAction Ignore | Out-Null" + cr);
                        ps.Append("      Write-Host 'Creating Secondary Local Network Gateway: ' -NoNewLine" + cr);
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
                        ps.Append("   $sgwip=Get-AzureRmPublicIpAddress -Name " + currentSubscription.secondaryVnetName + "-gw-ip -ResourceGroupName " + currentSubscription.secondaryResourceGroup + cr);
                        ps.Append("   if($sgwip) {Write-Host 'WARNING: Public IP address already exists--skipping creation'}" + cr);
                        ps.Append("   else" + cr);
                        ps.Append("   {" + cr);
                        ps.Append("      New-AzureRmPublicIpAddress -Name " + currentSubscription.secondaryVnetName + "-gw-ip -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -Location " + currentSubscription.secondaryLocation + " -AllocationMethod Dynamic -ErrorAction Ignore | Out-Null" + cr);
                        ps.Append("      Write-Host 'Requesting Secondary Gateway IP Address: ' -NoNewLine" + cr);
                        ps.Append("      $sgwip=Get-AzureRmPublicIpAddress -Name " + currentSubscription.secondaryVnetName + "-gw-ip -ResourceGroupName " + currentSubscription.secondaryResourceGroup + cr);
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
                        ps.Append("   Write-Host 'NOTE: Azure Gateway Creation Takes Approximately 20 Minutes!'" + cr);
                        ps.Append("   Write-Host 'Creating Secondary Azure Gateway: ' -NoNewLine" + cr);
                        ps.Append("   $sgw=Get-AzureRmPublicIpAddress -Name " + currentSubscription.secondaryVnetName + "-gw-ip -ResourceGroupName " + currentSubscription.secondaryResourceGroup + cr);
                        ps.Append("   if($sgw) {Write-Host 'WARNING: Vnet gateway already exists--skipping creation'}" + cr);
                        ps.Append("   else" + cr);
                        ps.Append("   {" + cr);
                        ps.Append("      $ssn=Get-AzureRmVirtualNetworkSubnetConfig -Name 'GatewaySubnet' -VirtualNetwork $svn" + cr);
                        ps.Append("      $sgwipcfg=New-AzureRmVirtualNetworkGatewayIpConfig -Name sgwipconfig1 -SubnetId $ssn.Id -PublicIpAddressId $sgwip.Id" + cr);
                        ps.Append("      $sgw=New-AzureRmVirtualNetworkGateway -Name " + currentSubscription.secondaryVnetName + "-gw -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -Location " + currentSubscription.secondaryLocation + " -IpConfigurations $sgwipcfg -GatewayType Vpn -VpnType RouteBased -GatewaySku VpnGw1 -ErrorAction Ignore" + cr);
                        ps.Append("      Write-Host 'Creating Secondary Azure Gateway: ' -NoNewLine" + cr);
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
                    ps.Append("else {Write-Host 'COMPLETE: Virtual Network(s) and Gateway(s) Created Sucessfully.' -ForegroundColor Green}" + cr);
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
                    ps.Append("$lgw=Get-AzureRmLocalNetworkGateway -Name " + currentSubscription.localGatewayName + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -ErrorAction Ignore" + cr);
                    ps.Append("$pgw=Get-AzureRmVirtualNetworkGateway -Name " + currentSubscription.primaryVnetName + "-gw -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -ErrorAction Ignore" + cr);
                    ps.Append("$sgw=Get-AzureRmVirtualNetworkGateway -Name " + currentSubscription.secondaryVnetName + "-gw -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -ErrorAction Ignore" + cr);
                    //Create Primary Gateway Connection to Local Gateway
                    ps.Append("####################################################" + cr);
                    ps.Append("# Create Primary Gateway Connection to Local Gateway" + cr);
                    ps.Append("####################################################" + cr);
                    ps.Append("if($global:script_error -eq $false)" + cr);
                    ps.Append("{" + cr);
                    ps.Append("   Write-Host 'Creating Primary Connection: ' -NoNewLine" + cr);
                    ps.Append("   $pcn=Get-AzureRmVirtualNetworkGatewayConnection -Name cn-local-" + currentSubscription.primaryLocation + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + cr);
                    ps.Append("   if($pcn) {Write-Host 'WARNING: Connection already exists--skipping creation'}" + cr);
                    ps.Append("   else" + cr);
                    ps.Append("   {" + cr);
                    string priSkey = CreateSharedKey();
                    ps.Append("      $pcn=New-AzureRmVirtualNetworkGatewayConnection -Name cn-local-" + currentSubscription.primaryLocation + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -Location " + currentSubscription.primaryLocation + " -VirtualNetworkGateway1 $pgw -LocalNetworkGateway2 $lgw -ConnectionType IPsec -RoutingWeight 10 -SharedKey '" + priSkey + "'" + cr);
                    ps.Append("      Write-Host 'Creating Primary Connection: ' -NoNewLine" + cr);
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
                        ps.Append("   $scn=Get-AzureRmVirtualNetworkGatewayConnection -Name cn-local-" + currentSubscription.primaryLocation + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + cr);
                        ps.Append("   if($scn) {Write-Host 'WARNING: Connection already exists--skipping creation'}" + cr);
                        ps.Append("   else" + cr);
                        ps.Append("   {" + cr);
                        string secSkey = CreateSharedKey();
                        ps.Append("      $scn=New-AzureRmVirtualNetworkGatewayConnection -Name cn-local-" + currentSubscription.primaryLocation + " -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -Location " + currentSubscription.secondaryLocation + " -VirtualNetworkGateway1 $pgw -LocalNetworkGateway2 $lgw -ConnectionType IPsec -RoutingWeight 10 -SharedKey '" + secSkey + "'" + cr);
                        ps.Append("      Write-Host 'Creating Secondary Connection: ' -NoNewLine" + cr);
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
                        ps.Append("   $pvcn=Get-AzureRmVirtualNetworkGatewayConnection -Name cn-local-" + currentSubscription.primaryLocation + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + cr);
                        ps.Append("   if($pvcn) {Write-Host 'WARNING: Connection already exists--skipping creation'}" + cr);
                        ps.Append("   else" + cr);
                        ps.Append("   {" + cr);
                        string v2vSkey = CreateSharedKey();
                        ps.Append("      $pvcn=New-AzureRmVirtualNetworkGatewayConnection -Name cn-" + currentSubscription.primaryVnetName + "-" + currentSubscription.secondaryVnetName + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + " -Location " + currentSubscription.primaryLocation + " -VirtualNetworkGateway1 $pgw -VirtualNetworkGateway2 $sgw -ConnectionType Vnet2Vnet -SharedKey '" + v2vSkey + "'" + cr);
                        ps.Append("      Write-Host 'Creating VNet-to-VNet Connection (Primary to Secondary): ' -NoNewLine" + cr);
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
                        ps.Append("   $svcn=Get-AzureRmVirtualNetworkGatewayConnection -Name cn-local-" + currentSubscription.primaryLocation + " -ResourceGroupName " + currentSubscription.primaryResourceGroup + cr);
                        ps.Append("   if($svcn) {Write-Host 'WARNING: Connection already exists--skipping creation'}" + cr);
                        ps.Append("   else" + cr);
                        ps.Append("   {" + cr);
                        ps.Append("      $svcn=New-AzureRmVirtualNetworkGatewayConnection -Name cn-" + currentSubscription.secondaryVnetName + "-" + currentSubscription.primaryVnetName + " -ResourceGroupName " + currentSubscription.secondaryResourceGroup + " -Location " + currentSubscription.secondaryLocation + " -VirtualNetworkGateway1 $sgw -VirtualNetworkGateway2 $pgw -ConnectionType Vnet2Vnet -SharedKey '" + v2vSkey + "'" + cr);
                        ps.Append("      Write-Host 'Creating VNet-to-VNet Connection (Secondary to Primary): ' -NoNewLine" + cr);
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
                using (var selectFileDiaglog = new SaveFileDialog())
                {
                    if (selectFileDiaglog.ShowDialog() == DialogResult.OK) { currentSubscription.fileName = selectFileDiaglog.FileName; }
                }
                try
                {
                    string saveJson = currentSubscription.fileName + ".json";
                    string jsonOut = JsonConvert.SerializeObject(currentSubscription);
                    File.WriteAllText(saveJson, jsonOut);
                    string psScript = CreatePowerShellScript(1);
                    string savePs = currentSubscription.fileName + "-vnets.ps1";
                    File.WriteAllText(savePs, psScript);
                    string psScript2 = CreatePowerShellScript(2);
                    string savePs2 = currentSubscription.fileName + "-connections.ps1";
                    File.WriteAllText(savePs2, psScript2);
                    string armTemplate = CreateARMTemplate(1);
                    string saveArm = currentSubscription.fileName + "-azuredeploy1.json";
                    File.WriteAllText(saveArm, armTemplate);
                    string armTemplateS = CreateARMTemplate(2);
                    string saveArmS = currentSubscription.fileName + "-azuredeploy2.json";
                    File.WriteAllText(saveArmS, armTemplateS);
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
                lblEdgeIP.Visible = true;
                lblLocalGWName.Visible = true;
                lblOPAddress.Visible = true;
                txtEdgeIP.Visible = true;
                txtLocalGWName.Visible = true;
                txtOPAddress.Visible = true;
                currentSubscription.createConnection = true;
            }
            else
            {
                lblEdgeIP.Visible = false;
                lblLocalGWName.Visible = false;
                lblOPAddress.Visible = false;
                txtEdgeIP.Visible = false;
                txtLocalGWName.Visible = false;
                txtOPAddress.Visible = false;
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
                    txtPriIP4.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".1.0/24";
                    txtPriIP5.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".2.0/23";
                    txtPriIP6.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".4.0/23";
                    txtPriIP7.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".6.0/23";
                    if (radBoth.Checked)
                    {
                        txtSecIP0.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + secondaryIP.segment2.ToString() + ".224/27";
                        txtSecIP1.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + secondaryIP.segment2.ToString() + ".0/25";
                        txtSecIP2.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + secondaryIP.segment2.ToString() + ".128/26";
                        txtSecIP3.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + secondaryIP.segment2.ToString() + ".192/27";
                        txtSecIP4.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + (secondaryIP.segment2 + 1).ToString() + ".0/24";
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
                    txtPriIP4.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".3.0/24";
                    txtPriIP5.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".4.0/22";
                    txtPriIP6.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".8.0/22";
                    txtPriIP7.Text = primaryIP.segment0.ToString() + "." + primaryIP.segment1.ToString() + ".12.0/22";
                    if (radBoth.Checked)
                    {
                        txtSecIP0.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + secondaryIP.segment2.ToString() + ".0/25";
                        txtSecIP1.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + (secondaryIP.segment2 + 1).ToString() + ".0/24";
                        txtSecIP2.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + (secondaryIP.segment2 + 2).ToString() + ".0/25";
                        txtSecIP3.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + (secondaryIP.segment2 + 2).ToString() + ".128/25";
                        txtSecIP4.Text = secondaryIP.segment0.ToString() + "." + secondaryIP.segment1.ToString() + "." + (secondaryIP.segment2 + 3).ToString() + ".0/24";
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
            int idx = cboPrimaryLocation.SelectedIndex;
            currentSubscription.primaryLocation = locations[idx].location;
        }

        private void cboSecondaryLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = cboSecondaryLocation.SelectedIndex;
            currentSubscription.secondaryLocation = locations[idx].location;
        }

        private void ckbCreateGateway_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbCreateGateway.Checked) { currentSubscription.createGateway = true; }
            else { currentSubscription.createGateway = false; }
        }

    }
}
