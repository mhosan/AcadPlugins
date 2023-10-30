using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pruebaAcadForm
{
    internal class Util
    {
        public async Task <List<string>> ListControles() {
            
            await Task.Delay(1000); // Ejemplo de operación asincrónica simulada de 1 segundo

            List<string> listaControles = new List<string>
            {
                "control 1",
                "control 2",
                "control 3"
            };

            return listaControles;
        }

    }
    
}
