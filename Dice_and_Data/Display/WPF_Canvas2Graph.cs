using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Dice_and_Data.Display
{    
    class CanvasDiceGraph
    {
        private Canvas canvas;
        private double Width { get { return canvas.ActualWidth; } set { } }
        private double Height { get { return canvas.ActualHeight; } set { } }

        private RollPattern rp;
        private int Points { get { return rp.Max - rp.Min + 1; } set { } }
        private double Highest { get { return rp.p((int)rp.Mean); } set { } }

        public CanvasDiceGraph(Canvas canvas)
        {
            this.canvas = canvas;
            canvas.Background = System.Windows.Media.Brushes.BlanchedAlmond;
        }

        public void SetRollPattern(RollPattern rp)
        {
            this.rp = rp;
        }

        public void Draw()
        {
            if (Width < 50 || Height < 50) return;

            //Set some relative variables for drawing.
            double axisBuffer = 20;
            double xPtr = axisBuffer;
            double yPtr = Height - axisBuffer;            

            // Draw horizontal axis:            
            canvas.Children.Add(new LineBuilder(Brushes.Black).startPoint(xPtr, yPtr).endPoint(xPtr + Width - (axisBuffer * 2), yPtr).Build());
            canvas.Children.Add(new LineBuilder(Brushes.Black).startPoint(xPtr, yPtr).endPoint(xPtr, axisBuffer).Build());

            if (rp == null) return;
            double verticalScale = 0.8 * (Height - (axisBuffer * 2)) / this.Highest;

            // Draw probability distribution
            double chunkWidth = (Width - (axisBuffer * 2)) / this.Points;

            for (int i = rp.Min; i <= rp.Max; i++)
            {
                LineBuilder lb = new LineBuilder(Brushes.SteelBlue).startPoint(xPtr, yPtr);
                xPtr += chunkWidth;
                yPtr = Height - ((verticalScale * rp.p(i)) + axisBuffer);
                lb.endPoint(xPtr, yPtr);
                if (i != rp.Min)
                {
                    canvas.Children.Add(lb.Build());
                }
                Ellipse point = new Ellipse();
                point.Width = 4;
                point.Height = 4;
                point.Fill = Brushes.Black;
                Canvas.SetLeft(point, xPtr - (point.Width/2));
                Canvas.SetTop(point, yPtr - (point.Height/2));
                canvas.Children.Add(point);
                canvas.Children.Add(new LineBuilder(Brushes.Black).startPoint(xPtr, Height-axisBuffer+5).endPoint(xPtr, Height-axisBuffer-5).Build());
                Label label = new Label();
                label.Content = i.ToString();
                label.Margin = new Thickness(xPtr - (10), Height - (axisBuffer), 0, 0);
                canvas.Children.Add(label);
            }

        }

        private class LineBuilder
        {
            Line line;

            public LineBuilder(Brush color)
            {
                line = new Line();
                line.Stroke = color;
                line.HorizontalAlignment = HorizontalAlignment.Left;
                line.VerticalAlignment = VerticalAlignment.Center;
                line.StrokeThickness = 1;
            }

            public LineBuilder startPoint(double x1, double y1)
            {
                line.X1 = x1;
                line.Y1 = y1;
                return this;
            }

            public LineBuilder endPoint(double x2, double y2)
            {
                line.X2 = x2;
                line.Y2 = y2;
                return this;
            }

            public Line Build()
            {
                return this.line;
            }

        }



    }
}
