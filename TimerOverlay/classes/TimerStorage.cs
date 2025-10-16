using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace TimerOverlay
{
    public static class TimerStorage
    {
        private static readonly string CarpetaPerfiles = "profiles";
        private static readonly string Archivo = "Default.json";
        private static string RutaDefault => Path.Combine(CarpetaPerfiles, Archivo);

        public static void Guardar(List<TimerData> timers, string nombre)
        {
            if (!Directory.Exists(CarpetaPerfiles))
                Directory.CreateDirectory(CarpetaPerfiles);

            var ruta = Path.Combine(CarpetaPerfiles, nombre);
            var json = JsonConvert.SerializeObject(timers, Formatting.Indented);
            File.WriteAllText(ruta, json);
        }

        public static List<TimerData> Cargar()
        {
            if (!File.Exists(RutaDefault))
                return new List<TimerData>();

            var json = File.ReadAllText(RutaDefault);
            return JsonConvert.DeserializeObject<List<TimerData>>(json);
        }

        public static List<TimerData> CargarPerfil(string nombreArchivo)
        {
            var ruta = Path.Combine(CarpetaPerfiles, nombreArchivo);

            if (!File.Exists(ruta))
                return new List<TimerData>();

            var json = File.ReadAllText(ruta);
            return JsonConvert.DeserializeObject<List<TimerData>>(json);
        }

        public static List<string> ObtenerPerfiles()
        {
            if (!Directory.Exists(CarpetaPerfiles))
                return new List<string> { Archivo };

            var archivos = Directory.GetFiles(CarpetaPerfiles, "*.json")
                                    .Select(Path.GetFileName)
                                    .ToList();

            if (!archivos.Contains(Archivo))
                archivos.Insert(0, Archivo);

            return archivos;
        }
    }
}
