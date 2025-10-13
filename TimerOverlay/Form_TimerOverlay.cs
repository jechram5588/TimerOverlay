using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace TimerOverlay
{
    public partial class Form_TimerOverlay : Form
    {
        private Timer timerRun;
        private Timer timerStop;

        private bool animationStop = true;
        private bool estaPausado = false;
        private bool estaEnCD = false;

        private int tiempoInicial = 10;
        private int segundosRestantes = 10;
        private int contadorInicial = 10;
        private int valorInicial = 10;
        private int alto = 10, ancho = 10,tamFuente=0;

        private ContextMenuStrip contextMenu;
        private ImageTextControl ctrolImgTxt;
        public Image imagenIcono = null;

        private static List<Form_TimerOverlay> instancias = new List<Form_TimerOverlay>();


        List<string> ArchivosSonido = new List<string> { };
        List<string> PerfilesApp = new List<string> { };
        private string sonidoAlerta = "";
        private string sonidoCD = "";

        private Point mouseOffset;
        private bool isDragging = false;

        private Keys teclaGlobal = Keys.None;

        enum TipoContador
        {
            Temporizador, Cronometro, ContadorIncremental, ContadorDecremental
        }
        private TipoContador tipoContador;

        enum TipoAnimacion
        {
            Ninguna, Mostrar, Parpadear, MostrarConCD, ParpadearConCD
        }
        private TipoAnimacion tipoAnimacion;

        #region Constructores
        public Form_TimerOverlay(TimerData data)
        {
            if (this.ctrolImgTxt == null)
                this.ctrolImgTxt = new ImageTextControl();

            this.tiempoInicial = data.Tiempo;
            this.segundosRestantes = tiempoInicial;
            this.ctrolImgTxt.Text = FormatearTiempo(segundosRestantes);
            this.sonidoAlerta = data.SonidoAlerta;
            this.sonidoCD = data.SonidoCD;
            this.alto = data.Alto;
            this.ancho = data.Alto;
            this.tamFuente = data.TamañoFuente;

            // modo de funcionamiento
            if (data.Funcion != null)
            {
                this.tipoContador = (TipoContador)Enum.Parse(typeof(TipoContador), data.Funcion);
                switch (tipoContador)
                {
                    case TipoContador.Temporizador:
                        this.segundosRestantes = tiempoInicial;
                        break;
                    case TipoContador.Cronometro:
                        this.segundosRestantes = 0;
                        break;
                    case TipoContador.ContadorIncremental:
                        this.contadorInicial = 0;
                        break;
                    case TipoContador.ContadorDecremental:
                        valorInicial = data.Contador;
                        contadorInicial = valorInicial;
                        break;
                    default:
                        break;
                }
            }
            else
                this.tipoContador = TipoContador.Temporizador;

            // modo de animacion
            if (data.Animacion != null)
            {
                this.tipoAnimacion = (TipoAnimacion)Enum.Parse(typeof(TipoAnimacion), data.Animacion);
            }
            else
                this.tipoAnimacion = TipoAnimacion.Ninguna;

            this.teclaGlobal = (Keys)Convert.ToInt32(data.Tecla);


            // Usamos negro para transparencia
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;

            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(data.X, data.Y);

            this.BackColor = Color.Black;
            this.TransparencyKey = Color.Black;

            // Movimiento con mouse
            this.MouseDown += Form_MouseDown;
            this.MouseMove += Form_MouseMove;
            this.MouseUp += Form_MouseUp;

            // Imagen de fondo
            ctrolImgTxt = new ImageTextControl
            {
                Location = new Point(0, 0),
                Size = new Size(ancho,alto),
                fuente = tamFuente
            };

            switch (tipoContador)
            {
                case TipoContador.Temporizador:
                case TipoContador.Cronometro:
                    ctrolImgTxt.Text = FormatearTiempo(tiempoInicial);
                    break;
                case TipoContador.ContadorIncremental:
                case TipoContador.ContadorDecremental:
                    FormateaContador();
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(data.ImagenBase64))
            {
                byte[] bytes = Convert.FromBase64String(data.ImagenBase64);
                using (var ms = new MemoryStream(bytes))
                {
                    imagenIcono = Image.FromStream(ms);
                }
            }
            else
                imagenIcono = Properties.Resources.iconoDefault;
            
            this.ctrolImgTxt.Image = imagenIcono;
            this.Controls.Add(ctrolImgTxt);

            // Para que el control también se pueda usar para mover la ventana
            ctrolImgTxt.MouseDown += Form_MouseDown;
            ctrolImgTxt.MouseMove += Form_MouseMove;
            ctrolImgTxt.MouseUp += Form_MouseUp;
            ctrolImgTxt.MouseClick += Control_MouseClick;

            // Timer
            timerRun = new Timer();
            timerRun.Interval = 1000;
            timerRun.Tick += timerRun_Tick;
            //timerRun.Start();

            timerStop = new Timer();
            timerStop.Interval = 500;
            timerStop.Tick += timerStop_Tick;

            // Menu Contextual
            contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(Menu_AgregarContador());
            contextMenu.Items.Add(Menu_CerrarCuadro());
            contextMenu.Items.Add(Menu_ReiniciarContador());
            contextMenu.Items.Add(Menu_PausarContador());

            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(Menu_CambiarImagen());
            contextMenu.Items.Add(Menu_AsignarSonido());
            contextMenu.Items.Add(Menu_ModificarTiempo());
            contextMenu.Items.Add(Menu_ModificarContador());
            contextMenu.Items.Add(Menu_CambiarModo());
            contextMenu.Items.Add(Menu_AsignarTecla());
            contextMenu.Items.Add(Menu_ModificarTamaños());
            contextMenu.Items.Add(Menu_CambiarAnimacion());

            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(Menu_ReiniciarTodosLosContadores());
            contextMenu.Items.Add(Menu_PausarTodosLosContadore());
            contextMenu.Items.Add(Menu_CerrarCuadros());

            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(Menu_CargarPerfil());
            contextMenu.Items.Add(Menu_ExportarPerfil());
            contextMenu.Items.Add(Menu_CerrarApp());

            GlobalKeyboardHook.OnKeyPressed += GlobalKeyboardHook_OnKeyPressed;
            GlobalKeyboardHook.Start();
        }
        #endregion

        #region EventosApp
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            instancias.Add(this);
            ObtenerArchivosSonido();
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            GlobalKeyboardHook.OnKeyPressed -= GlobalKeyboardHook_OnKeyPressed;
            instancias.Remove(this);
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            GlobalKeyboardHook.Stop();
        }
        private void timerRun_Tick(object sender, EventArgs e)
        {
            if (!estaPausado)
            {
                switch (tipoContador)
                {
                    case TipoContador.Temporizador:
                        segundosRestantes--;
                        if (segundosRestantes <= 0)
                        {
                            if (!timerStop.Enabled)
                            {
                                timerRun.Enabled =false;
                                timerStop.Enabled = true;
                                ReproduceSonido(sonidoAlerta);
                                ctrolImgTxt.Image = imagenIcono;
                            }
                        }
                        break;
                    case TipoContador.Cronometro:
                        segundosRestantes++;
                        if (segundosRestantes >= tiempoInicial)
                        {
                            if (!timerStop.Enabled)
                            {
                                timerRun.Enabled = false;
                                timerStop.Enabled = true;
                                ReproduceSonido(sonidoAlerta);
                                ctrolImgTxt.Image = imagenIcono;
                            }
                        }
                        break;
                    case TipoContador.ContadorIncremental:
                    case TipoContador.ContadorDecremental:
                        break;
                }
            }

           
                ctrolImgTxt.Text = FormatearTiempo(segundosRestantes);
           
        }
        private void timerStop_Tick(object sender, EventArgs e)
        {
            estaEnCD = false;
            if(tipoAnimacion == TipoAnimacion.ParpadearConCD || tipoAnimacion == TipoAnimacion.Parpadear)
            {
                ctrolImgTxt.Image = animationStop ?  null: imagenIcono;
                animationStop = !animationStop;
            }
            ctrolImgTxt.Text = "";
        }
        private void GlobalKeyboardHook_OnKeyPressed(Keys key)
        {
            if (key == teclaGlobal)
            {
                this.Invoke((MethodInvoker)(() =>
                {                    
                    switch (tipoContador)
                    {
                        case TipoContador.Temporizador:
                        case TipoContador.Cronometro:
                            if (estaEnCD)
                                ReproduceSonido(sonidoCD);
                            else
                                Reiniciar();
                            break;
                        case TipoContador.ContadorIncremental:
                            FormateaContador();
                            if (contadorInicial >= valorInicial)
                            {
                                Reiniciar();
                                ReproduceSonido(sonidoAlerta);
                            }
                            contadorInicial++;
                            break;
                        case TipoContador.ContadorDecremental:
                            contadorInicial--;
                            FormateaContador();
                            if (contadorInicial <= 0)
                            {
                                Reiniciar();
                                ReproduceSonido(sonidoAlerta);
                            }
                            break;
                        default:
                            break;
                    }
                }));
            }
        }      
        private void Control_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenu.Show(Cursor.Position);
            }
        }
        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                mouseOffset = e.Location;
            }
        }
        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point newLocation = this.Location;
                newLocation.X += e.X - mouseOffset.X;
                newLocation.Y += e.Y - mouseOffset.Y;
                this.Location = newLocation;
            }
        }
        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }
        private string ObtenerTextoTeclaGlobal(Keys keyCombo)
        {
            List<string> partes = new List<string>();

            if ((keyCombo & Keys.Control) == Keys.Control) partes.Add("Ctrl");
            if ((keyCombo & Keys.Alt) == Keys.Alt) partes.Add("Alt");
            if ((keyCombo & Keys.Shift) == Keys.Shift) partes.Add("Shift");

            // Extraer la tecla principal removiendo los modificadores
            Keys teclaPrincipal = keyCombo & ~Keys.Control & ~Keys.Alt & ~Keys.Shift;

            if (teclaPrincipal != Keys.None)
                partes.Add(teclaPrincipal.ToString());

            return string.Join(" + ", partes);
        }
        private string FormatearTiempo(int totalSegundos)
        {
            int minutos = totalSegundos / 60;
            int segundos = totalSegundos % 60;
            string text = $"{minutos}:{segundos:D2}";
            switch (tipoAnimacion)
            {
                case TipoAnimacion.Ninguna:
                case TipoAnimacion.MostrarConCD:
                case TipoAnimacion.ParpadearConCD:
                    return text;
                case TipoAnimacion.Parpadear:
                case TipoAnimacion.Mostrar:
                default:
                    text ="";
                    break;
            }
            return text;
        }
        private void FormateaContador()
        {
            ctrolImgTxt.Text = contadorInicial.ToString();
        }
        private void ReproduceSonido(string archivo)
        {
            if (archivo != null)
            {
                string path = Path.Combine(Application.StartupPath, "assets", archivo);
                if (File.Exists(path))
            {
                try
                {
                    var audioFile = new AudioFileReader(path);
                    var outputDevice = new WaveOutEvent();
                    outputDevice.Init(audioFile);
                    outputDevice.Play();

                    // Timer para detener 
                    var timer = new System.Windows.Forms.Timer();
                    timer.Interval = 1500; 
                    timer.Tick += (s, e) =>
                    {
                        timer.Stop();
                        outputDevice.Stop(); // Detiene la reproducción
                    };
                    timer.Start();

                    // Liberar recursos cuando termina
                    outputDevice.PlaybackStopped += (s, e) =>
                    {
                        outputDevice.Dispose();
                        audioFile.Dispose();
                        timer.Dispose();
                    };
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            }
        }
        private void Reiniciar()
        {
            estaPausado = false;
            timerStop.Enabled = false;
            ctrolImgTxt.Image = imagenIcono;
            estaEnCD = true;
            if (tipoAnimacion != TipoAnimacion.Ninguna)
                ctrolImgTxt.Image = null;

            switch (tipoContador)
            {
                case TipoContador.Temporizador:
                    segundosRestantes = tiempoInicial;
                    ctrolImgTxt.Text = FormatearTiempo(segundosRestantes);
                    timerRun.Enabled = true;
                    break;
                case TipoContador.Cronometro:
                    segundosRestantes = 0;
                    ctrolImgTxt.Text = FormatearTiempo(segundosRestantes);
                    timerRun.Enabled = true;
                    break;
                case TipoContador.ContadorIncremental:
                    contadorInicial = 0;
                    FormateaContador();
                    break;
                case TipoContador.ContadorDecremental:
                    contadorInicial = valorInicial;
                    FormateaContador();
                    break;
            }
        }
        private void Pausar()
        {
            estaPausado = true;
            timerRun.Enabled = false;
            timerStop.Enabled = false;
        }
        private void Reanudar()
        {
            estaPausado = false;
            timerRun.Enabled = true;
            timerStop.Enabled = false;
        }
        private static void ReiniciarTodos()
        {
            foreach (var t in instancias)
            {
                t.Reiniciar();
            }
        }
        private static void CerrarApp()
        {
            foreach (var f in new List<Form_TimerOverlay>(instancias))
            {
                f.Close();
            }
            Application.Exit();
        }
        private void CerrarTodos()
        {
            foreach (var f in new List<Form_TimerOverlay>(instancias))
            {
                f.Close();
            }
            AgregaCuadro();
        }
        private void CargarPerfil(string perfil)
        {
            var datos = TimerStorage.CargarPerfil(perfil);
            if (datos.Count > 1)
            {
                foreach (var f in new List<Form_TimerOverlay>(instancias))
                {
                    f.Close();
                }

                foreach (var d in datos)
                {
                    var frm = new Form_TimerOverlay(d);
                    frm.ShowInTaskbar = false;
                    frm.Show();
                }                
            }
            else
            {
                MessageBox.Show("Sin data");
                AgregaCuadro();
            }
            Properties.Settings.Default.UltimoPerfil = perfil;
            Properties.Settings.Default.Save();
        }
        private static void PausarTodos()
        {
            foreach (var f in new List<Form_TimerOverlay>(instancias))
            {
                f.Pausar();
            }
        }
        private static void ReanudarTodos()
        {
            foreach (var f in new List<Form_TimerOverlay>(instancias))
            {
                f.Reanudar();
            }
        }
        public static void ExportarTodos(string nombre)
        {
            var lista = new List<TimerData>();

            foreach (var timer in instancias)
            {
                using (var ms = new MemoryStream())
                {
                    timer.imagenIcono.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    var base64 = Convert.ToBase64String(ms.ToArray());

                    lista.Add(new TimerData
                    {
                        X = timer.Location.X,
                        Y = timer.Location.Y,
                        Tiempo = timer.tiempoInicial,
                        ImagenBase64 = base64,
                        SonidoAlerta = timer.sonidoAlerta,
                        Funcion = timer.tipoContador.ToString(),
                        Tecla = (int)timer.teclaGlobal,
                        Contador = timer.valorInicial,
                        Alto = timer.alto,
                        Ancho = timer.ancho,
                        TamañoFuente = timer.tamFuente,
                        Animacion = timer.tipoAnimacion.ToString(),
                        SonidoCD = timer.sonidoCD
                    });
                }
            }

            TimerStorage.Guardar(lista, nombre);
        }
        public void ObtenerArchivosSonido()
        {
            string[] fileNames = Directory.GetFiles(Application.StartupPath + @"\assets\")
                              .Select(Path.GetFileName)
                              .ToArray();
            ArchivosSonido = new List<string>(fileNames);
            ArchivosSonido.Insert(0, "Ninguno");
        }
        public void ObtenerPerfiles()
        {
            PerfilesApp.Clear();
            PerfilesApp = TimerStorage.ObtenerPerfiles();
        }
        public void AgregaCuadro()
        {
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
        }
        #endregion

        #region EventosMenu
        private ToolStripMenuItem Menu_AgregarContador()
        {
            ToolStripMenuItem agregarCuadro = new ToolStripMenuItem("Agregar")
            {
                Image = Properties.Resources.iconoAgregar
            };
            agregarCuadro.Click += (s, e) =>
            {
                AgregaCuadro();
            };
            return agregarCuadro;
        }
        private ToolStripMenuItem Menu_CerrarCuadro()
        {
            var cerrar = new ToolStripMenuItem("Cerrar")
            {
                Image = Properties.Resources.iconoCerrar
            };
            cerrar.Click += (s, e) => this.Close();
            return cerrar;
        }
        private ToolStripMenuItem Menu_CambiarImagen()
        {
            var cambiarImagen = new ToolStripMenuItem("Cambiar imagen")
            {
                Image = Properties.Resources.iconoImagen
            };
            cambiarImagen.Click += (s, e) =>
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp;*.gif";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            imagenIcono = Image.FromFile(ofd.FileName);
                            ctrolImgTxt.Image = imagenIcono;
                        }
                        catch
                        {
                            MessageBox.Show("No se pudo cargar la imagen.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            };
            return cambiarImagen;
        }
        private ToolStripMenuItem Menu_AsignarSonido()
        {
            var asignarSonido = new ToolStripMenuItem("Asignar sonido")
            {
                Image = Properties.Resources.iconoSonido
            };
            asignarSonido.Click += (s, e) =>
            {
                using (var inputForm = new Form()
                {
                    Size = new Size(235, 180),
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterParent,
                    Text = "Asignar sonido",
                    MaximizeBox = false,
                    MinimizeBox = false
                })
                {
                    List<string> ArchivosSonidoCD = new List<string>(ArchivosSonido);

                    var lbSonido = new Label()
                    {
                        Text = "Sonido",
                        Location = new Point(10, 15),
                        AutoSize = true
                    };

                    var cbSonido = new ComboBox()
                    {
                        DataSource = ArchivosSonido,
                        Text = sonidoAlerta,
                        Location = new Point(10,31),
                        Width = 200,
                        DropDownStyle = ComboBoxStyle.DropDownList
                    };

                    var lbSonidoCD = new Label()
                    {
                        Text = "Sonido CD",
                        Location = new Point(10,63),
                        AutoSize = true
                    };

                    var cbSonidoCD = new ComboBox()
                    {
                        DataSource = ArchivosSonidoCD,
                        Text = sonidoAlerta,
                        Location = new Point(10, 80),
                        Width = 200,
                        DropDownStyle = ComboBoxStyle.DropDownList
                    };

                    var btnPlay = new Button()
                    {
                        Text = "Play",
                        DialogResult = DialogResult.None,
                        Location = new Point(160, 5),
                        Width = 50
                    };

                    // Agregar evento para reproducir sonido
                    btnPlay.Click += (snd, evt) =>
                    {
                        string archivo = cbSonido.SelectedItem?.ToString();
                        if (!string.IsNullOrEmpty(archivo))
                        {
                            ReproduceSonido(archivo);
                        }
                    };

                    var btnPlayCD = new Button()
                    {
                        Text = "Play CD",
                        DialogResult = DialogResult.None,
                        Location = new Point(160, 55),
                        Width = 50
                    };

                    // Agregar evento para reproducir sonido
                    btnPlayCD.Click += (snd, evt) =>
                    {
                        string archivo = cbSonidoCD.SelectedItem?.ToString();
                        if (!string.IsNullOrEmpty(archivo))
                        {
                            ReproduceSonido(archivo);
                        }
                    };

                    var btnAceptar = new Button()
                    {
                        Text = "Aceptar",
                        DialogResult = DialogResult.OK,
                        Location = new Point(60, 106),
                        Width = 100,
                    };

                    inputForm.Controls.Add(lbSonido);
                    inputForm.Controls.Add(cbSonido);
                    inputForm.Controls.Add(btnPlay);

                    inputForm.Controls.Add(lbSonidoCD);
                    inputForm.Controls.Add(cbSonidoCD);
                    inputForm.Controls.Add(btnPlayCD);
                    inputForm.Controls.Add(btnPlay);


                    inputForm.Controls.Add(btnAceptar);
                    inputForm.AcceptButton = btnAceptar;

                    if (inputForm.ShowDialog(this) == DialogResult.OK)
                    {
                        sonidoAlerta = cbSonido.Text;
                        sonidoCD = cbSonidoCD.Text;
                    }
                }
            };
            return asignarSonido;
        }
        private ToolStripMenuItem Menu_ModificarTiempo()
        {
            var modificarTiempo = new ToolStripMenuItem("Modificar tiempo")
            {
                Image = Properties.Resources.iconoTiempo
            };

            modificarTiempo.Click += (s, e) =>
            {
                using (var inputForm = new Form()
                {
                    Size = new Size(250, 150),
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterParent,
                    Text = "Modificar tiempo",
                    MaximizeBox = false,
                    MinimizeBox = false
                })
                {
                    var lbMin = new Label() { Text = "Minutos", Location = new Point(10, 15), Width = 50 };
                    var lbSec = new Label() { Text = "Segundos", Location = new Point(120, 15), Width = 55 };
                    var nudMin = new NumericUpDown() { Minimum = 0, Maximum = 60, Value = tiempoInicial / 60, Location = new Point(10, 50), Width = 100 };
                    var nudSeg = new NumericUpDown() { Minimum = 0, Maximum = 59, Value = tiempoInicial % 60, Location = new Point(120, 50), Width = 100 };
                    var btnAceptar = new Button() { Text = "Aceptar", DialogResult = DialogResult.OK, Location = new Point(15, 80), Width = 200 };

                    inputForm.Controls.Add(lbMin);
                    inputForm.Controls.Add(lbSec);
                    inputForm.Controls.Add(nudMin);
                    inputForm.Controls.Add(nudSeg);
                    inputForm.Controls.Add(btnAceptar);
                    inputForm.AcceptButton = btnAceptar;

                    if (inputForm.ShowDialog(this) == DialogResult.OK)
                    {
                        tiempoInicial = (int)nudSeg.Value + 60 * (int)nudMin.Value;
                        segundosRestantes = tiempoInicial;
                        ctrolImgTxt.Text = FormatearTiempo(segundosRestantes);
                        timerStop.Enabled = false;
                        timerRun.Enabled = true;
                    }
                }
            };
            return modificarTiempo;
        }
        private ToolStripMenuItem Menu_ModificarContador()
        {
            var modificarContador = new ToolStripMenuItem("Modificar contador")
            {
                Image = Properties.Resources.iconoContador
            };

            modificarContador.Click += (s, e) =>
            {
                using (var inputForm = new Form()
                {
                    Size = new Size(150, 150),
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterParent,
                    Text = "Modificar contador",
                    MaximizeBox = false,
                    MinimizeBox = false
                })
                {
                    var lbMin = new Label() { Text = "Valor", Location = new Point(10, 15), Width = 50 };
                    var nudValor = new NumericUpDown() { Minimum = 0, Maximum = 1000, Value = valorInicial, Location = new Point(10, 50), Width = 100 };
                    var btnAceptar = new Button() { Text = "Aceptar", DialogResult = DialogResult.OK, Location = new Point(15, 80), Width = 100 };

                    inputForm.Controls.Add(lbMin);
                    inputForm.Controls.Add(nudValor);
                    inputForm.Controls.Add(btnAceptar);
                    inputForm.AcceptButton = btnAceptar;

                    if (inputForm.ShowDialog(this) == DialogResult.OK)
                    {
                        valorInicial = (int)nudValor.Value;
                        if (tipoContador == TipoContador.ContadorDecremental)
                            contadorInicial = valorInicial;
                        else
                            contadorInicial = 0;
                        FormateaContador();
                        timerStop.Enabled = false;
                        timerRun.Enabled = true; ;
                    }
                }
            };
            return modificarContador;
        }
        private ToolStripMenuItem Menu_ModificarTamaños()
        {
            var modificarTamaños = new ToolStripMenuItem("Modificar tamaños")
            {
                Image = Properties.Resources.iconoSize
            };

            modificarTamaños.Click += (s, e) =>
            {
                using (var inputForm = new Form()
                {
                    Size = new Size(250, 150),
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterParent,
                    Text = "Modificar tamaño",
                    MaximizeBox = false,
                    MinimizeBox = false
                })
                {
                    var lbAncho = new Label() { Text = "Ancho", Location = new Point(10, 15), Width = 50 };
                    var nudAncho = new NumericUpDown() { Minimum = 10, Maximum = 200, Value = alto, Location = new Point(10, 45), Width = 60 };

                    var lbAlto = new Label() { Text = "Alto", Location = new Point(80, 15), Width = 50 };
                    var nudAlto = new NumericUpDown() { Minimum = 10, Maximum = 200, Value = ancho, Location = new Point(80, 45), Width = 60 };

                    var lbFuente = new Label() { Text = "Texto", Location = new Point(150, 15), Width = 50 };
                    var nudFuente = new NumericUpDown() { Minimum = 1, Maximum = 200, Value = tamFuente, Location = new Point(150, 45), Width = 60 };
                    
                    var btnAceptar = new Button() { Text = "Aceptar", DialogResult = DialogResult.OK, Location = new Point(15, 80), Width = 200 };

                    inputForm.Controls.Add(lbAncho);
                    inputForm.Controls.Add(lbAlto);
                    inputForm.Controls.Add(nudAncho);
                    inputForm.Controls.Add(nudAlto);
                    inputForm.Controls.Add(lbFuente);
                    inputForm.Controls.Add(nudFuente);
                    inputForm.Controls.Add(btnAceptar);
                    inputForm.AcceptButton = btnAceptar;

                    if (inputForm.ShowDialog(this) == DialogResult.OK)
                    {
                        ancho = (int)nudAlto.Value; 
                        alto= (int)nudAncho.Value;
                        tamFuente = (int)nudFuente.Value;
                        ctrolImgTxt.Size = new Size(ancho, alto);
                        ctrolImgTxt.fuente = tamFuente;
                    }
                }
            };
            return modificarTamaños;
        }
        private ToolStripMenuItem Menu_CambiarModo()
        {
            var cambiarModo = new ToolStripMenuItem("Cambiar Modo");

            // Subopciones
            var modoTemporizador = new ToolStripMenuItem("Temporizador");
            var modoCronometro = new ToolStripMenuItem("Cronómetro");
            var modoIncremental = new ToolStripMenuItem("Contador Incremental");
            var modoDecremental = new ToolStripMenuItem("Contador Decremental");

            // Acción genérica para cambiar de modo
            void CambiarModo(TipoContador nuevoModo)
            {
                tipoContador = nuevoModo;
                ActualizarTextoModo(); // si quieres que el texto cambie con el modo actual
                Reiniciar();
            }

            // Eventos click de cada subopción
            modoTemporizador.Click += (s, e) => CambiarModo(TipoContador.Temporizador);
            modoTemporizador.Image = Properties.Resources.iconoTemporizador;

            modoCronometro.Click += (s, e) => CambiarModo(TipoContador.Cronometro);
            modoCronometro.Image = Properties.Resources.iconoCronometro;

            modoIncremental.Click += (s, e) => CambiarModo(TipoContador.ContadorIncremental);
            modoIncremental.Image = Properties.Resources.iconoChartUp;

            modoDecremental.Click += (s, e) => CambiarModo(TipoContador.ContadorDecremental);
            modoDecremental.Image = Properties.Resources.iconoChartDown;


            // Añadir subopciones al menú principal
            cambiarModo.DropDownItems.Add(modoTemporizador);
            cambiarModo.DropDownItems.Add(modoCronometro);
            cambiarModo.DropDownItems.Add(modoIncremental);
            cambiarModo.DropDownItems.Add(modoDecremental);

            // Actualizar texto/ícono si deseas reflejar el modo actual
            void ActualizarTextoModo()
            {
                switch (tipoContador)
                {
                    case TipoContador.Temporizador:
                        cambiarModo.Text = "Modo actual: Temporizador";
                        cambiarModo.Image = Properties.Resources.iconoTemporizador;
                        break;

                    case TipoContador.Cronometro:
                        cambiarModo.Text = "Modo actual: Cronómetro";
                        cambiarModo.Image = Properties.Resources.iconoCronometro;
                        break;

                    case TipoContador.ContadorIncremental:
                        cambiarModo.Text = "Modo actual: Incremental";
                        cambiarModo.Image = Properties.Resources.iconoChartUp;
                        break;

                    case TipoContador.ContadorDecremental:
                        cambiarModo.Text = "Modo actual: Decremental";
                        cambiarModo.Image = Properties.Resources.iconoChartDown;
                        break;
                }
            }

            ActualizarTextoModo();
            return cambiarModo;
        }
        private ToolStripMenuItem Menu_AsignarTecla()
        {
            var asignarTecla = new ToolStripMenuItem("Asignar tecla")
            {
                Image = Properties.Resources.iconoTecla // Asegúrate de tener este icono o usa null
            };
            asignarTecla.Click += (s, e) =>
            {
                using (var capturaForm = new Form()
                {
                    Text = "Presiona una tecla...",
                    Size = new Size(300, 100),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    KeyPreview = true
                })
                {
                    Label lbl = new Label()
                    {
                        Text = "Presiona una tecla para asignar...",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter
                    };

                    capturaForm.Controls.Add(lbl);

                    capturaForm.KeyUp += (s2, e2) =>
                    {
                        teclaGlobal = e2.KeyCode;
                        lbl.Text = $"Tecla asignada: {teclaGlobal}";
                        capturaForm.DialogResult = DialogResult.OK;
                    };
                    capturaForm.ShowDialog(this);
                    ActualizarTextoTeclaMenu();
                }
            };

            void ActualizarTextoTeclaMenu()
            {
                string textoTecla = teclaGlobal != Keys.None
                    ? ObtenerTextoTeclaGlobal(teclaGlobal)
                    : "ninguna";

                asignarTecla.Text = $"Asignar tecla ({textoTecla})";
            }
            ActualizarTextoTeclaMenu();
            return asignarTecla;
        }
        private ToolStripMenuItem Menu_ReiniciarContador()
        {
            var reiniciarEste = new ToolStripMenuItem("Reiniciar")
            {
                Image = Properties.Resources.iconoReiniciar
            };
            reiniciarEste.Click += (s, e) => Reiniciar();
            return reiniciarEste;
        }
        private ToolStripMenuItem Menu_PausarContador()
        {
            var pausarContador = new ToolStripMenuItem("Pausar")
            {
                Image = Properties.Resources.iconoPause
            };

            pausarContador.Click += (s, e) =>
            {
                if (!estaPausado)
                    Pausar();
                else
                    Reanudar();
                pausarContador.Text = estaPausado ? "Reanudar" : "Pausar";
                pausarContador.Image = estaPausado ? Properties.Resources.iconoPlay : Properties.Resources.iconoPause;
            };
            return pausarContador;
        }
        private ToolStripMenuItem Menu_ReiniciarTodosLosContadores()
        {
            var reiniciarTodos = new ToolStripMenuItem("Reiniciar todo")
            {
                Image = Properties.Resources.iconoReiniciarTodo
            };
            reiniciarTodos.Click += (s, e) => ReiniciarTodos();
            return reiniciarTodos;
        }
        private ToolStripMenuItem Menu_PausarTodosLosContadore()
        {
            var pausarTodos = new ToolStripMenuItem("Pausar todo")
            {
                Image = Properties.Resources.iconoPause
            };

            pausarTodos.Click += (s, e) =>
            {
                if (!estaPausado)
                    PausarTodos();
                else
                    ReanudarTodos();
                pausarTodos.Text = estaPausado ? "Reanudar todo" : "Pausar todo";
                pausarTodos.Image = !estaPausado ? Properties.Resources.iconoPause : Properties.Resources.iconoPlay;
            };
            return pausarTodos;
        }
        private ToolStripMenuItem Menu_ExportarPerfil()
        {
            var exportar = new ToolStripMenuItem("Exportar perfil")
            {
                Image = Properties.Resources.iconoExportar
            };

            exportar.Click += (s, e) =>
            {
                ObtenerPerfiles();
                // Crear ventana
                Form ventana = new Form()
                {
                    Text = "Exportar Perfil",
                    Size = new Size(300, 150),
                    StartPosition = FormStartPosition.CenterScreen,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                // Crear ComboBox (editable)
                ComboBox comboPerfiles = new ComboBox()
                {
                    DropDownStyle = ComboBoxStyle.DropDown, // Permite escribir
                    Location = new Point(20, 20),
                    Size = new Size(240, 25)
                };
                comboPerfiles.Items.AddRange(PerfilesApp.ToArray());

                comboPerfiles.SelectedItem = Properties.Settings.Default.UltimoPerfil;

                // Crear botón Exportar
                Button btnExportar = new Button()
                {
                    Text = "Exportar",
                    Location = new Point(100, 60),
                    Size = new Size(80, 30)
                };

                btnExportar.Click += (senderBtn, evt) =>
                {
                    string perfilIngresado = comboPerfiles.Text?.Trim();

                    if (string.IsNullOrWhiteSpace(perfilIngresado))
                    {
                        MessageBox.Show("Por favor, ingresa o selecciona un nombre de perfil.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Asegurar que termine en ".json"
                    if (!perfilIngresado.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        perfilIngresado += ".json";
                    }

                    // Aquí llamas a tu método de exportación
                    Form_TimerOverlay.ExportarTodos(perfilIngresado);

                    MessageBox.Show($"Perfil exportado como \"{perfilIngresado}\"", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ventana.Close();
                };

                // Agregar controles
                ventana.Controls.Add(comboPerfiles);
                ventana.Controls.Add(btnExportar);

                // Mostrar ventana
                ventana.ShowDialog();
            };

            return exportar;
        }
        private ToolStripMenuItem Menu_CargarPerfil()
        {
            var exportar = new ToolStripMenuItem("Cargar perfil")
            {
                Image = Properties.Resources.iconoLoad
            };

            exportar.Click += (s, e) =>
            {
                ObtenerPerfiles();

                // Crear ventana
                Form ventana = new Form()
                {
                    Text = "Cargar Perfil",
                    Size = new Size(300, 150),
                    StartPosition = FormStartPosition.CenterScreen,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                // Crear ComboBox (editable)
                ComboBox comboPerfiles = new ComboBox()
                {
                    DropDownStyle = ComboBoxStyle.DropDownList, // Permite escribir
                    Location = new Point(20, 20),
                    Size = new Size(240, 25)
                };
                comboPerfiles.Items.AddRange(PerfilesApp.ToArray());

                // Crear botón Exportar
                Button btnExportar = new Button()
                {
                    Text = "Cargar",
                    Location = new Point(100, 60),
                    Size = new Size(80, 30)
                };

                comboPerfiles.SelectedItem = Properties.Settings.Default.UltimoPerfil;

                btnExportar.Click += (senderBtn, evt) => {
                    if (comboPerfiles.SelectedItem != null)
                    {
                        string perfilSeleccionado = comboPerfiles.SelectedItem.ToString();
                        CargarPerfil(perfilSeleccionado); 
                        ventana.Close(); 
                    } 
                    else
                        MessageBox.Show("Selecciona un perfil.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                };

                // Agregar controles
                ventana.Controls.Add(comboPerfiles);
                ventana.Controls.Add(btnExportar);

                // Mostrar ventana
                ventana.ShowDialog();
            };

            return exportar;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Form_TimerOverlay
            // 
            this.ClientSize = new System.Drawing.Size(127, 93);
            this.Name = "Form_TimerOverlay";
            this.ResumeLayout(false);

        }

        private ToolStripMenuItem Menu_CerrarApp()
        {
            var cerrartodos = new ToolStripMenuItem("Salir")
            {
                Image = Properties.Resources.iconoCerrarTodo
            };
            cerrartodos.Click += (s, e) => CerrarApp();
            return cerrartodos;
        }
        private ToolStripMenuItem Menu_CerrarCuadros()
        {
            var cerrartodos = new ToolStripMenuItem("Cerrar todo")
            {
                Image = Properties.Resources.iconoClosed
            };
            cerrartodos.Click += (s, e) => CerrarTodos();
            return cerrartodos;
        }
        private ToolStripMenuItem Menu_CambiarAnimacion()
        {
            var cambiarAnimacion = new ToolStripMenuItem("Cambiar animación");

            // Subopciones
            var modoNinguna = new ToolStripMenuItem("Ninguna");
            var modoMostrar = new ToolStripMenuItem("Mostrar");
            var modoParpadear = new ToolStripMenuItem("Parpadear");
            var modoMostrarCD = new ToolStripMenuItem("Mostrar CD");
            var modoParpadearCD = new ToolStripMenuItem("Parpadear CD");

            // Acción genérica para cambiar de modo
            void CambiarAnimacion(TipoAnimacion nuevaAnimacion)
            {
                tipoAnimacion = nuevaAnimacion;
                ActualizarTextoModo(); // si quieres que el texto cambie con el modo actual
                Reiniciar();
            }

            // Eventos click de cada subopción
            modoNinguna.Click += (s, e) => CambiarAnimacion(TipoAnimacion.Ninguna);
            modoNinguna.Image = Properties.Resources.iconoNone;

            modoMostrar.Click += (s, e) => CambiarAnimacion(TipoAnimacion.Mostrar);
            modoMostrar.Image = Properties.Resources.iconoShow;

            modoMostrarCD.Click += (s, e) => CambiarAnimacion(TipoAnimacion.MostrarConCD);
            modoMostrarCD.Image = Properties.Resources.iconoShow;

            modoParpadear.Click += (s, e) => CambiarAnimacion(TipoAnimacion.Parpadear);
            modoParpadear.Image = Properties.Resources.iconoBlink;

            modoParpadearCD.Click += (s, e) => CambiarAnimacion(TipoAnimacion.ParpadearConCD);
            modoParpadearCD.Image = Properties.Resources.iconoBlink;


            // Añadir subopciones al menú principal
            cambiarAnimacion.DropDownItems.Add(modoNinguna);
            cambiarAnimacion.DropDownItems.Add(modoMostrar);
            cambiarAnimacion.DropDownItems.Add(modoMostrarCD);
            cambiarAnimacion.DropDownItems.Add(modoParpadear);
            cambiarAnimacion.DropDownItems.Add(modoParpadearCD);

            // Actualizar texto/ícono si deseas reflejar el modo actual
            void ActualizarTextoModo()
            {
                switch (tipoAnimacion)
                {
                    case TipoAnimacion.Ninguna:
                        cambiarAnimacion.Text = "Animación: Ninguna";
                        cambiarAnimacion.Image = Properties.Resources.iconoNone;
                        break;

                    case TipoAnimacion.Mostrar:
                        cambiarAnimacion.Text = "Animación: Mostrar";
                        cambiarAnimacion.Image = Properties.Resources.iconoShow;
                        break;
                    case TipoAnimacion.Parpadear:
                        cambiarAnimacion.Text = "Animación: Parpadear";
                        cambiarAnimacion.Image = Properties.Resources.iconoBlink;
                        break;
                    case TipoAnimacion.MostrarConCD:
                        cambiarAnimacion.Text = "Animación: Mostrar CD";
                        cambiarAnimacion.Image = Properties.Resources.iconoShow;
                        break;
                    case TipoAnimacion.ParpadearConCD:
                        cambiarAnimacion.Text = "Animación: Parpadear CD";
                        cambiarAnimacion.Image = Properties.Resources.iconoBlink;
                        break;
                }
            }

            ActualizarTextoModo();
            return cambiarAnimacion;
        }
        #endregion               
    }
}