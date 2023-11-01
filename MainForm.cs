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
        /// Exportar a dxf y despachar el request
        ///  private void button1_Click(object sender, EventArgs e)
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            Util RecuperarControles = new Util();
            Task <List<string>> misControles = RecuperarControles.ListControles();
            misControles.ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion)
                {
                    List<string> listaControles = t.Result;

                    foreach (var control in listaControles)
                    {
                        //MessageBox.Show(control);
                    }

                    //MessageBox.Show("Operación leer listado de controles completada.");

                    string docName = doc.Name;
                    string nombreArchivo = "prototipo" + Path.GetFileName(docName);
                    string directorio = Path.GetDirectoryName(docName);
                    nuevoNombreArchivo = Path.ChangeExtension(nombreArchivo, ".dxf");

                    Database bd = HostApplicationServices.WorkingDatabase;
                    nuevaRutaArchivo = Path.Combine(directorio, nuevoNombreArchivo);

                    ///DwgVersion.AC1021 corresponde a Autocad 2007
                    bd.DxfOut(nuevaRutaArchivo, 16, DwgVersion.AC1021);
                    _ = makeRequest();
                }
                else if (t.IsFaulted)
                {
                    MessageBox.Show("Error: " + t.Exception.InnerException.Message);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }


        /// <summary>
        /// Enviar request 
        /// </summary>
        private async Task makeRequest()
        {
            string url = "https://geo.arba.gov.ar/api-plano-control/evaluaplano";
            var client = new HttpClient();
            List<int> selectControles = new List<int> { 501, 502, 508 };
            using (MultipartFormDataContent formData = new MultipartFormDataContent()) 
            {
                byte[] archivoBytes = System.IO.File.ReadAllBytes(nuevaRutaArchivo);
                ByteArrayContent archivoContent = new ByteArrayContent(archivoBytes);
                formData.Add(archivoContent, "file", nuevoNombreArchivo);
                foreach (var valor in selectControles) 
                {
                    formData.Add(new StringContent(valor.ToString()), "selectControles");
                }
                HttpResponseMessage response = await client.PostAsync(url, formData);
                string textoRespuesta = await response.Content.ReadAsStringAsync();

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
                    // Convertir el array en una lista de strings
                    List<string> listExtraido = new List<string>(arrayExtraido);
                    foreach (var elemento in listExtraido)
                    {
                        if (elemento.Length > 1) {
                            lboxResponse.Items.Add(elemento);
                        }
                    }
                }
                else 
                {
                    MessageBox.Show("no se encontró respuesta en el response...");
                }
            }
        }


        /// <summary>
        /// Cerrar el formulario 
        /// </summary>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        
    }
}
