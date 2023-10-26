using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;

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
            string docName = doc.Name;
            string nombreArchivo = Path.GetFileName(docName);
            string directorio = Path.GetDirectoryName(docName);
            string nuevoNombreArchivo = Path.ChangeExtension(nombreArchivo, ".dxf");
            lblStart.Text = $"EXPORTAR: {docName}";
            Database bd = HostApplicationServices.WorkingDatabase;
            string nuevaRutaArchivo = Path.Combine(directorio, nuevoNombreArchivo);
            bd.DxfOut(nuevaRutaArchivo, 16, DwgVersion.Current);
            lblEnd.Text = $"RESULTADO: {nuevaRutaArchivo}";
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
