using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;


namespace pruebaAcadForm
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        public Editor ed;
        public Document doc;
        public MainForm()
        {
            InitializeComponent();
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            //MessageBox.Show("hola mundo!");
            doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ed.WriteMessage("Comienza exportación...!!!");
            Database bd = HostApplicationServices.WorkingDatabase;
            bd.DxfOut("C:\\Users\\marcelo.hosan\\Desktop\\cadFolder\\pepeExportado.dxf", 16, DwgVersion.Current);
            ed.WriteMessage("Archivo exportado!!!");
        }
    }
}
