using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TimerOverlay
{

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string perfil = Properties.Settings.Default.UltimoPerfil;

            List<TimerData> datos = TimerStorage.Cargar();
            if(perfil != "")
                datos = TimerStorage.CargarPerfil(perfil);
            else
                datos = TimerStorage.Cargar();

            if (datos.Count == 0)
            {
                // No hay datos guardados → iniciar con 1 cuadro por defecto
                var nuevoCuadro = new Form_TimerOverlay(new TimerData
                {
                    X = 100,
                    Y = 100,
                    Tiempo = 10,
                    Alto = 50,
                    Ancho = 50,
                    TamañoFuente = 12,
                    ImagenBase64 = ""
                });
                nuevoCuadro.StartPosition = FormStartPosition.CenterScreen;
                nuevoCuadro.ShowInTaskbar = false;
                nuevoCuadro.BackColor = Color.Black;
                nuevoCuadro.Show();
                Application.Run();
            }
            else
            {
                // Cargar todos los cuadros desde JSON
                foreach (var d in datos)
                {
                    var frm = new Form_TimerOverlay(d);
                    frm.ShowInTaskbar = false;
                    frm.Show();
                }
                Application.Run();
            }
        }
    }

}
