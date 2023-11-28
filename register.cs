using Microsoft.Win32;
using System.Reflection;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System.Windows.Forms;

namespace pruebaAcadForm
{
    class Register
    {

        [CommandMethod("RegisterMyApp")]
        public void RegisterMyApp()
        {
            // Get the AutoCAD Applications key
            string sProdKey = HostApplicationServices.Current.MachineRegistryProductRootKey; //.RegistryProductRootKey; UserRegistryProductRootKey
            string sAppName = "MyApp";

            Autodesk.AutoCAD.Runtime.RegistryKey regAcadProdKey = Autodesk.AutoCAD.Runtime.Registry.CurrentUser.OpenSubKey(sProdKey);
            Autodesk.AutoCAD.Runtime.RegistryKey regAcadAppKey = regAcadProdKey.OpenSubKey("Applications", true);

            // Check to see if the "MyApp" key exists
            string[] subKeys = regAcadAppKey.GetSubKeyNames();
            foreach (string subKey in subKeys)
            {
                // If the application is already registered, exit
                if (subKey.Equals(sAppName))
                {
                    regAcadAppKey.Close();
                    MessageBox.Show("Ya está registrado el plugin");
                    return;
                }
            }

            // Get the location of this module
            string sAssemblyPath = Assembly.GetExecutingAssembly().Location;

            // Register the application
            Autodesk.AutoCAD.Runtime.RegistryKey regAppAddInKey = regAcadAppKey.CreateSubKey(sAppName);
            regAppAddInKey.SetValue("DESCRIPTION", sAppName, RegistryValueKind.String);
            regAppAddInKey.SetValue("LOADCTRLS", 14, RegistryValueKind.DWord);
            regAppAddInKey.SetValue("LOADER", sAssemblyPath, RegistryValueKind.String);
            regAppAddInKey.SetValue("MANAGED", 1, RegistryValueKind.DWord);
            MessageBox.Show("Plugin registrado OK!");
            regAcadAppKey.Close();
        }

        [CommandMethod("UnregisterMyApp")]
        public void UnregisterMyApp()
        {
            // Get the AutoCAD Applications key
            string sProdKey = HostApplicationServices.Current.MachineRegistryProductRootKey;
            string sAppName = "MyApp";

            Autodesk.AutoCAD.Runtime.RegistryKey regAcadProdKey = Autodesk.AutoCAD.Runtime.Registry.CurrentUser.OpenSubKey(sProdKey);
            Autodesk.AutoCAD.Runtime.RegistryKey regAcadAppKey = regAcadProdKey.OpenSubKey("Applications", true);

            // Check to see if the "MyApp" key exists
            bool appUnRegistered = false;
            string[] subKeys = regAcadAppKey.GetSubKeyNames();
            foreach (string subKey in subKeys)
            {
                // If the application is already registered, unregister
                if (subKey.Equals(sAppName))
                {
                    // Delete the key for the application
                    regAcadAppKey.DeleteSubKeyTree(sAppName);
                    regAcadAppKey.Close();
                    MessageBox.Show("Plugin desregistrado!");
                    appUnRegistered = true;
                    return;
                }
            }
            if (!appUnRegistered) 
            {
                MessageBox.Show("El Plugin no está registrado!");
            }

        }
    }
}