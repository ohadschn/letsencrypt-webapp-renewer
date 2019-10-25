using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommandLine.Text;

[assembly: InternalsVisibleTo("OhadSoft.AzureLetsEncrypt.Renewal.WebJob.Tests")]

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("OhadSoft.AzureLetsEncrypt.Renewal.WebJob")]
[assembly: AssemblyDescription("Azure WebJob that renews Azure Web App SSL certificates using the LetsEncrypt API (based on letsencrypt-siteextension)")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("OhadSoft")]
[assembly: AssemblyProduct("OhadSoft.AzureLetsEncrypt.Renewal.WebJob")]
[assembly: AssemblyCopyright("Copyright ©  2017")]
[assembly: AssemblyTrademark("All Rights Reserved Ohad Schneider © 2017")]
[assembly: AssemblyLicense("Apache 2.0")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("b8c64de4-15ec-4935-8891-85245e97e48b")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("0.9.6.0")]
[assembly: AssemblyFileVersion("0.0.6.0")]
