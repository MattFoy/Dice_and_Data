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

        private Dictionary<string, Data.RollHistory> rollHistory;

        public CanvasDiceGraph(Canvas canvas)
        {
            this.canvas = canvas;
            canvas.Background = System.Windows.Media.Brushes.BlanchedAlmond;
            LoadHistory();
        }

        public void LoadHistory()
        {
            //TODO get this from Data.SQLiteDBWrapper function that loads history
            rollHistory = Data.SQLiteDBWrapper.getReference().GetRollHistory();

            if (rp != null && !rollHistory.ContainsKey(rp.ToString(false)))
            {
                rollHistory.Add(rp.ToString(false), new Data.RollHistory());
                Draw();
            }
        }

        public void UpdateGraphWithRoll(int value)
        {
            if (rp == null) { return; }
            else
            {
                value -= rp.Constant;
                Data.RollHistory hist = rollHistory[rp.ToString(false)];
                if (hist.ContainsKey(value))
                {
                    hist[value]++;
                }
                else
                {
                    hist.Add(value, 1);
                }
                Draw();
            }            
        }

        public double GetHistoricalP(int value)
        {
            Data.RollHistory hist = rollHistory[rp.ToString(false)];
            if (hist.ContainsKey(value))
            {
                return hist.p(value);
            }
            else
            {
                return 0;
            }
        }

        public void SetRollPattern(RollPattern rp)
        {
            this.rp = rp;
            if (!rollHistory.ContainsKey(this.rp.ToString(false)))
            {
                rollHistory.Add(this.rp.ToString(false), new Data.RollHistory());
            }
        }

        public void Draw()
        {
            if (Width < 50 || Height < 50) return;
            canvas.Children.Clear();

            //Set some relative variables for drawing.
            double axisBuffer = 20;
            double xPtr = axisBuffer;
            double yPtr = Height - axisBuffer;            

            // Draw horizontal axis:            
            canvas.Children.Add(new LineBuilder(Brushes.Black).startPoint(xPtr, yPtr).endPoint(xPtr + Width - (axisBuffer * 2), yPtr).Build());
            canvas.Children.Add(new LineBuilder(Brushes.Black).startPoint(xPtr, yPtr).endPoint(xPtr, axisBuffer).Build());

            if (rp == null) return;
            double historicalHighest = rollHistory[rp.ToString(false)].highestP();
            if (Double.IsNaN(historicalHighest)) { historicalHighest = 0.0; }
            double verticalScale = 0.8 * (Height - (axisBuffer * 2)) / Math.Max(historicalHighest, this.Highest);

            // Calculate the width of each value on the canvas
            double chunkWidth = (Width - (axisBuffer * 2)) / (this.Points+1);

            // Iterate through each roll value to draw elements.
            for (int i = rp.Min; i <= rp.Max; i++)
            {
                //Draw the historical roll data:
                BarBuilder bb = new BarBuilder(Brushes.ForestGreen)
                    .setBottomLeft(xPtr + (chunkWidth / 2), axisBuffer)
                    .setSize(chunkWidth, verticalScale * GetHistoricalP(i-rp.Constant));
                canvas.Children.Add(bb.Build());

                //Draw the probabiltiy line segment
                LineBuilder lb = new LineBuilder(Brushes.SteelBlue).startPoint(xPtr, yPtr);
                xPtr += chunkWidth;
                yPtr = Height - ((verticalScale * rp.p(i)) + axisBuffer);
                if (rp.ValidPTable)
                {
                    lb.endPoint(xPtr, yPtr);
                    if (i != rp.Min)
                    {
                        canvas.Children.Add(lb.Build());
                    }

                    //Draw a dot
                    Ellipse point = new Ellipse();
                    point.Width = 4;
                    point.Height = 4;
                    point.Fill = Brushes.Black;
                    Canvas.SetLeft(point, xPtr - (point.Width / 2));
                    Canvas.SetTop(point, yPtr - (point.Height / 2));
                    canvas.Children.Add(point);
                }

                //Draw a horizontal axis marker
                canvas.Children.Add(new LineBuilder(Brushes.Black).startPoint(xPtr, Height-axisBuffer+5).endPoint(xPtr, Height-axisBuffer-5).Build());

                //Draw a label for the axis marker
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

        private class BarBuilder
        {
            Rectangle rect;
            public BarBuilder(Brush color)
            {
                rect = new Rectangle();
                rect.Fill = color;
            }
            public BarBuilder setBottomLeft(double left, double bottom)
            {
                Canvas.SetLeft(rect, left);
                Canvas.SetBottom(rect, bottom);
                return this;
            }
            public BarBuilder setSize(double width, double height)
            {
                rect.Width = width;
                rect.Height = height;
                return this;
            }
            public Rectangle Build()
            {
                return rect;
            }
        }

    }
}
