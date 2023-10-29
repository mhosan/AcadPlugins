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
//using System.Web.Extensions
//using Newtonsoft.Json;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System.Dynamic;
using System.Diagnostics;


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
            doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
        }

        /// <summary>
        /// Exportar a dxf
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            string docName = doc.Name;
            string nombreArchivo = "prototipo" + Path.GetFileName(docName);
            string directorio = Path.GetDirectoryName(docName);
            nuevoNombreArchivo = Path.ChangeExtension(nombreArchivo, ".dxf");
            lblStart.Text = $"EXPORTAR: {docName}";
            
            Database bd = HostApplicationServices.WorkingDatabase;
            nuevaRutaArchivo = Path.Combine(directorio, nuevoNombreArchivo);
            
            ///DwgVersion.AC1021 corresponde a Autocad 2007
            bd.DxfOut(nuevaRutaArchivo, 16, DwgVersion.AC1021);
            lblEnd.Text = $"RESULTADO: {nuevaRutaArchivo}";
        }
                
        
        /// <summary>
        /// Cerrar el formulario 
        /// </summary>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        /// <summary>
        /// Enviar request 
        /// </summary>
        private async void btnRequest_Click(object sender, EventArgs e)
        {
            string url = "https://geo.arba.gov.ar/api-plano-control/evaluaplano";
            var client = new HttpClient();
            List<int> selectControles = new List<int> { 501, 502, 503 };
            using (MultipartFormDataContent formData = new MultipartFormDataContent()) 
            {
                //var archivito = doc.Name;
                //byte[] archivoBytes = System.IO.File.ReadAllBytes(archivito);
                byte[] archivoBytes = System.IO.File.ReadAllBytes(nuevaRutaArchivo);
                ByteArrayContent archivoContent = new ByteArrayContent(archivoBytes);
                formData.Add(archivoContent, "file",nuevoNombreArchivo);
                foreach (var valor in selectControles) 
                {
                    formData.Add(new StringContent(valor.ToString()), "selectControles");
                }
                HttpResponseMessage response = await client.PostAsync(url, formData);
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
                int indiceInicio = textoRespuesta.IndexOf("('{\"resultado\"");
                int indiceFin = textoRespuesta.IndexOf(".replace(/");
                if (indiceInicio != -1 && indiceFin != -1)
                {
                    string extraido = textoRespuesta.Substring(indiceInicio, indiceFin - indiceInicio) + ")";
                    // Eliminar el primer y último carácter (comillas dobles)
                    if (extraido.Length > 1)
                    {
                        extraido = extraido.Substring(1);
                        extraido = extraido.Substring(0, extraido.Length - 1);
                    }
                    else 
                    {
                        // Si la cadena tiene longitud 1 o menos 
                        extraido = string.Empty;
                    }
                    //dynamic miObjeto = new ExpandoObject();
                    // Deserializar el string JSON a un objeto C#
                    //miObjeto = JsonConvert.DeserializeObject(extraido);
                    //var queEs = miObjeto.GetType();
                    var delimiters = new char[] { ',', '[', ']', '{', '}'};
                    var arrayExtraido = extraido.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    //foreach (var substring in arrayExtraido)
                    //{
                    //    Debug.WriteLine(substring);
                    //}
                    lboxResponse.Items.AddRange(arrayExtraido);
                }
                else 
                {
                    MessageBox.Show("no se encontró respuesta en el resonse...");
                }
            }
        }
    }
}
