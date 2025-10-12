using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TimerOverlay
{
    public class ImageTextControl : Control
    {
        private Image _image;
        private string _text = "";
        public int fuente = 12;
        public string data { get; set; }
        public Image Image
        {
            get => _image;
            set { _image = value; Invalidate(); }
        }

        public override string Text
        {
            get => _text;
            set { _text = value; Invalidate(); }
        }
        
        public ImageTextControl()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint, true);
            this.BackColor = Color.Black;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            
            // Dibuja la imagen
            if (_image != null)
            {
                g.DrawImage(_image, new Rectangle(0, 0, this.Width, this.Height));
            }

            if (!string.IsNullOrEmpty(_text))
            {
                using (var sf = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                using (var font = new Font("Segoe UI", fuente, FontStyle.Bold))
                {
                    // Medimos el tamaño del texto
                    SizeF textSize = g.MeasureString(_text, font);

                    // Calculamos la posición del rectángulo para centrarlo
                    float rectX = (this.Width - textSize.Width) / 2 - 2; // Un poco de margen
                    float rectY = (this.Height - textSize.Height) / 2 - 1;
                    float rectWidth = textSize.Width;
                    float rectHeight = textSize.Height;

                    // Dibujar fondo negro semitransparente detrás del texto
                    // 150 = transparencia
                    using (var bgBrush = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
                    {
                        g.FillRectangle(bgBrush, rectX, rectY, rectWidth, rectHeight);
                    }

                    // Dibujar texto encima 
                    using (var textBrush = new SolidBrush(Color.White))
                    {
                        g.DrawString(_text, font, textBrush, ClientRectangle, sf);
                    }
                }
            }
        }

    }

}
