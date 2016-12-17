using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("RemoteTech-Transmitter")]
[assembly: AssemblyDescription("RemoteTech extended transmitter module")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("RemoteTechnologiesGroup")]
[assembly: AssemblyProduct("RemoteTech-Transmitter")]
[assembly: AssemblyCopyright("Copyright ©  2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("45de8446-3834-4f31-9025-5fc30a0617f7")]

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

// Don't include the patch revision in the AssemblyVersion - as this will break any dependent
// DLLs any time it changes.  Breaking on a minor revision is probably acceptable - it's
// unlikely that there wouldn't be other breaking changes on a minor version change.
[assembly: AssemblyVersion("1.0")]
[assembly: AssemblyFileVersion("1.0.0")]

// Use KSPAssembly to allow other DLLs to make this DLL a dependency in a
// non-hacky way in KSP.  Format is (AssemblyProduct, major, minor), and it
// does not appear to have a hard requirement to match the assembly version.
[assembly: KSPAssembly("RemoteTech-Transmitter", 1, 0)]
