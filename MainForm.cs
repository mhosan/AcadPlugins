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
using System.Text.RegularExpressions;

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
        public string nuevaRutaArchivo;
        public string nuevoNombreArchivo;
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
            nuevoNombreArchivo = Path.ChangeExtension(nombreArchivo, ".dxf");
            lblStart.Text = $"EXPORTAR: {docName}";
            Database bd = HostApplicationServices.WorkingDatabase;
            nuevaRutaArchivo = Path.Combine(directorio, nuevoNombreArchivo);
            bd.DxfOut(nuevaRutaArchivo, 16, DwgVersion.Current);
            lblEnd.Text = $"RESULTADO: {nuevaRutaArchivo}";
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void btnRequest_Click(object sender, EventArgs e)
        {
            string url = "https://geo.arba.gov.ar/api-plano-control/evaluaplano";
            var client = new HttpClient();
            List<int> selectControles = new List<int> { 501, 502, 503 };
            using (MultipartFormDataContent formData = new MultipartFormDataContent()) 
            {
                byte[] archivoBytes = System.IO.File.ReadAllBytes(nuevaRutaArchivo);
                ByteArrayContent archivoContent = new ByteArrayContent(archivoBytes);
                formData.Add(archivoContent, "file",nuevoNombreArchivo);
                foreach (var valor in selectControles) 
                {
                    formData.Add(new StringContent(valor.ToString()), "selectControles");
                }
                HttpResponseMessage response = await client.PostAsync(url, formData);
                lblResponseCode.Text = response.StatusCode.ToString();
                string textoRespuesta = await response.Content.ReadAsStringAsync();

                //string patron = @"var respuesta = JSON\.parse\('([^']+)'\)";
                //Match match = Regex.Match(textoRespuesta, patron);
                //if (match.Success)
                //{
                //    string subStringBuscado = match.Groups[4].Value;
                //    MessageBox.Show(subStringBuscado);
                //    lblResponseString.Text = subStringBuscado;
                //    //var controlList = JsonConvert.DeserializeObject<Control[]>(subStringBuscado);
                //    string[] substrings = subStringBuscado.Split(',');
                //    foreach (string substring in substrings)
                //        {
                //            MessageBox.Show(substring);
                //        }
                //}
                //else
                //{
                //    MessageBox.Show("No se encontró el patrón en la respuesta");
                //}
                int indiceInicio = textoRespuesta.IndexOf("var respuesta = ");
                int indiceFin = textoRespuesta.IndexOf("numero_requerimiento");
                if (indiceInicio != -1 && indiceFin != -1)
                {
                    string extraido = textoRespuesta.Substring(indiceInicio, indiceFin - indiceInicio);
                    MessageBox.Show(extraido);
                }
                else 
                {
                    MessageBox.Show("no se encontró...");
                }
            }
                
        }
    }
}
