using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;

using Point = System.Windows.Point;
using System.Drawing.Imaging;




namespace WPF_KYJ_CustomControl.Controls
{

    public class ROICanvas : Canvas
    {
        public static readonly DependencyProperty ROIProperty = DependencyProperty.Register("ROI", typeof(BitmapImage), typeof(ROICanvas), new PropertyMetadata(null));

        public static readonly DependencyProperty BackgroundImageProperty = DependencyProperty.Register("BackgroundImage", typeof(BitmapImage), typeof(ROICanvas), new PropertyMetadata(null, OnBackgroundImageChanged));




        public BitmapImage ROI
        {
            get { return (BitmapImage)GetValue(ROIProperty); }
            set { SetValue(ROIProperty, value); }
        }

        public BitmapImage BackgroundImage
        {
            get { return (BitmapImage)GetValue(BackgroundImageProperty); }
            set { SetValue(BackgroundImageProperty, value); }
        }

        private static void OnBackgroundImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ROICanvas)d;
            var newImage = e.NewValue as BitmapImage;

            if (newImage != null)
            {
                var imageBrush = new ImageBrush(newImage);
                imageBrush.Stretch = Stretch.Fill;
                control.Background = imageBrush;
            }
            else
            {
                control.Background = null; // 기본 배경으로 설정
            }
        }



        private object lockObject = new object();

        private Rect _rect;

        private Point _startPoint;
        //private Point _currentPoint;
        private Point _endPoint;


        public ROICanvas()
        {
            this.MouseLeftButtonDown += ROICanvas_MouseLeftButtonDown;
            this.MouseMove += ROICanvas_MouseMove;
            this.MouseUp += ROICanvas_MouseUp;
        }


        private void ROICanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _rect = new Rect();
            _startPoint = e.GetPosition(this);

            e.Handled = true;
        }

        private void ROICanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_startPoint == null)
                return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _endPoint = e.GetPosition((IInputElement)e.Source);

                var width = Math.Abs(_endPoint.X - _startPoint.X);
                var height = Math.Abs(_endPoint.Y - _startPoint.Y);


                if (IsLeftUp())
                {
                    _rect = new Rect(new Point(_endPoint.X, _endPoint.Y), new Size(width, height));
                    //Console.WriteLine("Left Up");
                }
                else if (IsLeftDown())
                {
                    _rect = new Rect(new Point(_endPoint.X, _startPoint.Y), new Size(width, height));
                    //Console.WriteLine("Left Down");
                }
                else if (IsRightUp())
                {
                    _rect = new Rect(new Point(_startPoint.X, _endPoint.Y), new Size(width, height));
                    //Console.WriteLine("Right Up");
                }
                else if (IsRightDown())
                {
                    _rect = new Rect(new Point(_startPoint.X, _startPoint.Y), new Size(width, height));
                    //Console.WriteLine("Right Down");
                }

                InvalidateVisual();

                e.Handled = true;
            }
        }

        private void ROICanvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (BackgroundImage == null)
                return;


            var byteArr = BitmapImageToByteArray(BackgroundImage);
            var tempImage = ResizeImage(ByteArrayToImage(byteArr));

            BitmapImage srcImage = ImageToBitmapImage(tempImage);
            System.Drawing.Bitmap cropImage;

            System.Drawing.RectangleF srcRect = new System.Drawing.RectangleF((float)_rect.Left, (float)_rect.Top, (float)_rect.Width, (float)_rect.Height);
            System.Drawing.Rectangle destRect = new System.Drawing.Rectangle(new System.Drawing.Point(0, 0), new System.Drawing.Size(640, 640));

            cropImage = new System.Drawing.Bitmap(640, 640);

            using (var graphics = System.Drawing.Graphics.FromImage(cropImage))
            {
                graphics.DrawImage(tempImage, destRect, srcRect, System.Drawing.GraphicsUnit.Pixel);
            }

            ROI = BitmapToBitmapImage(cropImage);



        }




        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (BackgroundImage != null && _rect != Rect.Empty)
            {
                dc.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Blue, 2), _rect);
            }
        }








        private bool IsLeftUp()
        {
            return (_startPoint.X > _endPoint.X && _startPoint.Y > _endPoint.Y);
        }

        private bool IsLeftDown()
        {
            return (_startPoint.X > _endPoint.X && _startPoint.Y < _endPoint.Y);
        }

        private bool IsRightUp()
        {
            return (_startPoint.X < _endPoint.X && _startPoint.Y > _endPoint.Y);
        }

        private bool IsRightDown()
        {
            return (_startPoint.X < _endPoint.X && _startPoint.Y < _endPoint.Y);
        }



        private System.Drawing.Image ResizeImage(System.Drawing.Image image)
        {
            if (image == null)
                return image;

            var resultRect = new System.Drawing.Rectangle(0, 0, 640, 640);
            var resultImage = new System.Drawing.Bitmap(640, 640);

            using (var graphics = System.Drawing.Graphics.FromImage(resultImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);

                    graphics.DrawImage(image, resultRect);
                }
            }

            return resultImage;
        }

        private byte[] BitmapImageToByteArray(BitmapImage bitmapImage)
        {
            using (var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(stream);

                return stream.ToArray();
            }
        }

        private System.Drawing.Image ByteArrayToImage(byte[] b)
        {
            using (var ms = new MemoryStream(b))
            {
                System.Drawing.Image resultImage = System.Drawing.Image.FromStream(ms);

                return resultImage;
            }
        }
        private BitmapImage ImageToBitmapImage(System.Drawing.Image img)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(ms.ToArray());
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        private BitmapImage BitmapToBitmapImage(System.Drawing.Bitmap bmp)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                lock (lockObject)
                {
                    bmp.Save(memory, ImageFormat.Bmp);
                    memory.Position = 0;

                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = memory;
                    bitmapImage.EndInit();

                    return bitmapImage;
                }
            }
        }

    }
}
