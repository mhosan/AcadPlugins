using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace pruebaAcadForm
{
    internal class Util
    {
        public async Task <List<string>> ListControles() {
            
            //await Task.Delay(1000); // Ejemplo de operación asincrónica simulada de 1 segundo
            List<string> listaControles = new List<string>();
          
            string url = "https://geo.arba.gov.ar/api-plano-control/controles";
            var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            
            // Verificar si la solicitud fue exitosa
            if (response.IsSuccessStatusCode)
            {
                // Leer el contenido de la respuesta 
                string respuestaCruda = await response.Content.ReadAsStringAsync();
                //Type elTipo = respuestaCruda.GetType();
                //MessageBox.Show(elTipo.Name);
                respuestaCruda = respuestaCruda.Substring(1);
                respuestaCruda = respuestaCruda.Substring(0, respuestaCruda.Length - 1);

                // Deserializar el JSON en un objeto C#
                //String[] objetoDeserializado = JsonConvert.DeserializeObject<String[]>(respuestaCruda);

                // Ahora puedes trabajar con el objetoDeserializado que contiene los datos del JSON response
                // Por ejemplo, imprimir alguna propiedad del objeto
                //Debug.WriteLine($"Propiedad del objeto deserializado: {objetoDeserializado.Propiedad}");
                //Debug.Write(objetoDeserializado);
                var delimiters = new char[] { ',', '[', ']', '{', '}' };
                var arrayExtraido = respuestaCruda.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                List<string> listExtraido = new List<string>(arrayExtraido);
                
                foreach (var elemento in listExtraido)
                {
                    if (elemento.Length > 1)
                    {
                        if (elemento.Contains("\"id\":"))
                        {
                            string elementoId = elemento.Substring(5);
                            listaControles.Add(elementoId);
                        }
                        
                    }
                }

            }
            else
            {
                // Si la solicitud no fue exitosa, manejar el error según sea necesario
                Debug.WriteLine($"Error en la solicitud: {response.StatusCode}");
            }
            return listaControles;
        }

    }

    // Clase que representa la estructura del JSON response
    public class Controles
    {
        public int id { get; }
        public string nombre { get; }
        public string tipo { get; }
        public string configFileName { get; }
        public string label { get; }
        public int value { get; }
        // Otras propiedades del objeto según la estructura del JSON response
    }
    
}
