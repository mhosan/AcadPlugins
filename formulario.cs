using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
 

namespace pruebaAcadForm
{
    public class formulario
    {
        [CommandMethod("Demo")]
        public void Demo()
        {
            MainForm formulario = new MainForm();
            formulario.Show();
        }

    }
}
