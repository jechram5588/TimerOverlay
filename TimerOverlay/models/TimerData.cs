using System;
using System.Drawing;

namespace TimerOverlay
{
    [Serializable]
    public class TimerData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Tiempo { get; set; }
        public string ImagenBase64 { get; set; } // guardamos la imagen como texto base64
        public string SonidoAlerta { get; set; }
        public string Funcion { get; set; }
        public int Tecla { get; set; }
        public int Contador { get; set; }
        public int Alto { get; set; }
        public int Ancho { get; set; }
        public int TamañoFuente { get; set; }
        public string Animacion { get; set; }
        public string SonidoCD { get; set; }
    }
}
