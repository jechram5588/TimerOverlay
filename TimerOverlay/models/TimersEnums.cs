using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimerOverlay
{
    public class TimersEnums 
    { 
        public enum TipoFuncion
        {
            Temporizador, Cronometro, ContadorIncremental, ContadorDecremental
        }
        public enum TipoAnimacion
        {
            Ninguna, Mostrar, Parpadear, MostrarConCD, ParpadearConCD
        }
        public enum TipoAccion  {
            Reiniciar, Pausar
        }
    }
}
