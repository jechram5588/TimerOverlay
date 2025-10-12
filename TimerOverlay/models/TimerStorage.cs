using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace TimerOverlay
{
    public static class TimerStorage
    {
        private static readonly string Archivo = "Default.json";

        public static void Guardar(List<TimerData> timers, string nombre)
        {
            var json = JsonConvert.SerializeObject(timers, Formatting.Indented);
            File.WriteAllText(nombre, json);
        }

        public static List<TimerData> Cargar()
        {
            if (!File.Exists(Archivo))
                return new List<TimerData>();

            var json = File.ReadAllText(Archivo);
            return JsonConvert.DeserializeObject<List<TimerData>>(json);
        }
        public static List<TimerData> CargarPerfil(string ruta)
        {
            if (!File.Exists(ruta))
                return new List<TimerData>();

            var json = File.ReadAllText(ruta);
            return JsonConvert.DeserializeObject<List<TimerData>>(json);
        }

        public static List<string> ObtenerPerfiles(string ruta) {

            string[] fileNames = Directory.GetFiles(ruta)
                             .Select(Path.GetFileName).Where(x => x.Contains(".json"))
                             .ToArray();
            List<string> lista = new List<string>(fileNames);
            if(!lista.Contains(Archivo))
                lista.Insert(0, Archivo);
            return lista;
        }
    }
}
