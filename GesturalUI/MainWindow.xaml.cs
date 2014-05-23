using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;           
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;                      //For : OpenFileDialog / SaveFileDialog
using System.Windows.Ink;                   //For : InkCanvas
using System.Windows.Markup;                //For : XamlWriter
using System.Windows.Input.StylusPlugIns;   //For : DrawingAttributes
namespace GesturalUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Lists to save recognized shapes and touched shapes
        private List<Shape> shapesList = new List<Shape>();
        private List<Shape> shapesGroupTouched = new List<Shape>();
        //List to to save the current time in seconds and it is used to know the difference in sec between one finger consecutive touches
        private List<int> currentTimeSec = new List<int>();
        //List to to save the current time in minutes
        private List<int> currentTimeMin = new List<int>();
        public MainWindow()
        {
            InitializeComponent();
            //By Default
            Notes.EditingMode = InkCanvasEditingMode.Ink;
            //Fire MutliTouching
            canvas.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(OnManipulationDelta);
            
        }

        ///Scratch out feature using Gestures
        private void onGesture(object sender, InkCanvasGestureEventArgs e)
        {
            StrokeCollection hitStrokes = Notes.Strokes.HitTest(e.Strokes.GetBounds(), 10);
            Shape shapeToRemoved = null;
            //Check the intersection between the stroke box bound and the shape
            Polyline tempShape = new Polyline();
            double distance = 0;
            foreach (Shape shape in shapesList)
            {
                tempShape = (Polyline)shape;
                PointCollection shapePoints = tempShape.Points;
                foreach (Point shapePoint in shapePoints)
                {
                    double deltaX = e.Strokes.GetBounds().X - shapePoint.X;
                    double deltaY = e.Strokes.GetBounds().Y - shapePoint.Y;
                    distance = Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
                    //if the distance between them less than 100 the set the shape to bel deleted
                    if (distance < 100)
                    {
                        shapeToRemoved = shape;
                    }
                }
            }
            if (shapeToRemoved != null)
            {
                shapesList.Remove(shapeToRemoved);
                Notes.Children.Remove(shapeToRemoved);
            }
                
            Notes.Strokes.Remove(hitStrokes);
        }

        // Keep track of which touch points (fingers) are used for which TouchDevices
        Dictionary<TouchDevice, TouchPoint> touchFingers = new Dictionary<TouchDevice, TouchPoint>();
        //Finger is down (touch)
        protected override void OnTouchDown(TouchEventArgs e)
        {
            base.OnTouchDown(e);
            //Make sure that the inkCanvas mode is none so no inks will be drawn from finger
            Notes.EditingMode = InkCanvasEditingMode.None;
            //Capture this touch device
            canvas.CaptureTouch(e.TouchDevice);
            TouchPoint point = e.GetTouchPoint(canvas);
            touchFingers[e.TouchDevice] = point;
            //If you touched the screen then put a text inside the box telling it is a mulitouch mode
            textBox1.Text = "MultiTouch Mode";
            //Take all the points from the shapes that were recognized and convert them to points and check if the distance between the touch point and the shape is less than 90 threshold
            Shape touchedShape = null;
            Polyline tempShape = new Polyline();
            double distance = 0;
            foreach (Shape shape in shapesList) {
                tempShape = (Polyline)shape;
                PointCollection shapePoints = tempShape.Points;
                foreach (Point shapePoint in shapePoints) {
                    double deltaX = point.Position.X - shapePoint.X;
                    double deltaY = point.Position.Y - shapePoint.Y;
                    distance = Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
                    if (distance < 90)
                    {
                        touchedShape = shape;
                    }  
                }
            }

            //Get the current time in sec
            DateTime dt = DateTime.Now;
            int minute = dt.Minute;
            int seconds = dt.Second;
            //Add it to the list
            currentTimeSec.Add(seconds);
            currentTimeMin.Add(minute);
            //Check the last two timings and make it by default 10 (any number bigger than 1 sec)
            int diffTimeInSec = 10;
            if (currentTimeSec.Count > 1 && currentTimeMin.Count > 1)
            {
                if (currentTimeMin[currentTimeMin.Count - 1] - currentTimeMin[currentTimeMin.Count - 2] == 0)
                {
                    diffTimeInSec = currentTimeSec[currentTimeSec.Count - 1] - currentTimeSec[currentTimeSec.Count - 2];
                }
            }
            
            //One finger only can be used to select shapes
            if (touchFingers.Count == 1) {
                //If it is less than 1 sec between 1 finger touches and there are shapes in the touched shapes group
                if (diffTimeInSec < 1 && shapesGroupTouched.Count > 0) {
                    if (touchedShape != null && shapesGroupTouched.IndexOf(touchedShape) != -1) {
                        //Unselect the touched shape and remove it from the group    
                        touchedShape.Stroke = Brushes.DarkGreen;
                        touchedShape.StrokeThickness = 8;
                        shapesGroupTouched.Remove(touchedShape);
                    } else {
                        //Unselect all touched shapes
                        foreach (Shape shape in shapesGroupTouched) {
                            shape.Stroke = Brushes.DarkGreen;
                            shape.StrokeThickness = 8;
                        }
                        shapesGroupTouched.Clear();
                    }
                  // If shape is touched
                } else if (touchedShape != null) {
                    //If shape is not touched before then add it to the lest other wise do nothing
                    if (shapesGroupTouched.IndexOf(touchedShape) == -1) {
                        touchedShape.Stroke = Brushes.DarkBlue;
                        touchedShape.StrokeThickness = 8;
                        shapesGroupTouched.Add(touchedShape);
                    }
                }
            }
            
        }

       
        //Finger is up
        protected override void OnTouchUp(TouchEventArgs e)
        {
            base.OnTouchUp(e);

            // Release capture
            Notes.ReleaseTouchCapture(e.TouchDevice);
            //Clear touch fingers dictionary
            touchFingers.Clear();
            //Set the InkCanvas editing mode as Ink only
            Notes.EditingMode = InkCanvasEditingMode.Ink;
            //Display a message telling it is an Ink mode
            textBox1.Text = "Ink Mode";
        }


        //MultiTouch Functionalities
        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            // Make sure that you are Manipulating the touched shapes only
            if (shapesGroupTouched.Count > 0)
            {
                Polyline newTempShape = new Polyline();
                //Get the bounding box for the grouped selected shapes/or one shape
                double minX = double.MaxValue;
                double maxX = double.MinValue;
                double minY = double.MaxValue;
                double maxY = double.MinValue;
                foreach (Shape shape in shapesGroupTouched)
                {
                    newTempShape = (Polyline)shape;
                    PointCollection shapePoints = newTempShape.Points;

                    for (int i = 0; i < shapePoints.Count; i++)
                    {
                        Point p = shapePoints.ElementAt(i);
                        if (p.X < minX)
                        {
                            minX = p.X;
                        }
                        if (p.X > maxX)
                        {
                            maxX = p.X;
                        }
                        if (p.Y < minY)
                        {
                            minY = p.Y;
                        }
                        if (p.Y > maxY)
                        {
                            maxY = p.Y;
                        }
                    }

                }
                //Bounding left/right/width/hegiht
                double x = minX;
                double y = minY;
                double w = maxX - minX;
                double h = maxY - minY;

                TranslateTransform translation = new TranslateTransform();
                ScaleTransform scale = new ScaleTransform();
                RotateTransform rotation = new RotateTransform();
               //Set it to be rotated
                rotation.Angle += e.DeltaManipulation.Rotation;
                rotation.CenterX = x + w / 2;
                rotation.CenterY = y + h / 2;
                //Set it to be scaled
                scale.ScaleX *= e.DeltaManipulation.Scale.X;
                scale.ScaleY *= e.DeltaManipulation.Scale.Y;
                scale.CenterX = rotation.CenterX;
                scale.CenterY = rotation.CenterY;
                //Set it to be translated
                translation.X += e.DeltaManipulation.Translation.X;
                translation.Y += e.DeltaManipulation.Translation.Y;
                //Get the points from shapes/shape
                Polyline tempShape = new Polyline();
                foreach (Shape shape in shapesGroupTouched)
                {
                    //Do Manipulation on those points
                    PointCollection newPoints = new PointCollection();
                    tempShape = (Polyline)shape;
                    PointCollection shapePoints = tempShape.Points;

                    for (int i = 0; i < shapePoints.Count; i++)
                    {
                        Point p = shapePoints.ElementAt(i);
                        Point pp = new Point(p.X, p.Y);
                        pp = rotation.Transform(pp);
                        pp = scale.Transform(pp);
                        pp = translation.Transform(pp);
                        p.X = pp.X;
                        p.Y = pp.Y;
                        newPoints.Add(p);
                    }
                    //Transfer Shape to the new points
                    tempShape.Points = newPoints;
                }
            }
        }

        //Applying ShortStraw Alg. every time a stroke is drawn
        private void shortStraw(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
           
            List<StylusPoint> cornerPoints = shortStraw(e.Stroke);
            String shapeName = shapeRecognition(cornerPoints,e.Stroke);
            textBox1.Text = shapeName;
            
         }

        
        //Shape recognition process
        private String shapeRecognition(List<StylusPoint> cornerPoints,Stroke stroke)
        {
           
            StylusPoint p1 = new StylusPoint();
            StylusPoint p2 = new StylusPoint();
            double d;
            double a;
            List<double> distances = new List<double>();
            List<double> angles = new List<double>();
            //Calculate the distance between each two consecative corners
            for (int i = 0; i < (cornerPoints.Count - 1); i++)
            {
                p1 = cornerPoints[i];
                p2 = cornerPoints[i + 1];
                d = distance(p1, p2);
                a = angle(p1, p2);
                distances.Add(d);
                angles.Add(a);
            }
            String output;
            //Line
            if (angles.Count < 2)
            {
                output = "Line";
            }
            else
            {
                //Calculate the sum of all angles
                double sumAngle = 0;
                for (int i = 0; i < angles.Count; i++)
                {
                    if (angles[i] < 0)
                    {
                        sumAngle = sumAngle + (angles[i] * -1);
                    }
                    else
                    {
                        sumAngle = sumAngle + angles[i];
                    }
                }
                
                //Make all angles Positive
                for (int i = 0; i < angles.Count; i++) {
                    if (angles[i] < 0) {
                        angles[i] = angles[i] * -1;
                    }
                }
                //Make all distances Positive
                for (int i = 0; i < distances.Count; i++) {
                    if (distances[i] < 0) {
                        distances[i] = distances[i] * -1;
                    }
                }
                double diffDistance = 0;
                double diffDistance1 = 0;
                double diffDistance2 = 0;
                if (angles.Count == 4)
                {
                    //For square (make sure that two distances are equal and not negative)
                    diffDistance = distances[1] - distances[0];
                    if (diffDistance < 0)
                    {
                        diffDistance = diffDistance * -1;
                    }
                    //For rectangle and sqaure (make sure the two parallel line are equal in distances)
                    diffDistance1 = distances[2] - distances[0];
                    diffDistance2 = distances[3] - distances[1];
                    if (diffDistance1 < 0)
                    {
                        diffDistance1 = diffDistance1 * -1;
                    }
                    if (diffDistance2 < 0)
                    {
                        diffDistance2 = diffDistance2 * -1;
                    }

                }
                //For Triangle make sure that the sum of two lines between corners are less than the third one
                double sumDistance1 = 0;
                double sumDistance2 = 0;
                double sumDistance3 = 0;
                if (angles.Count == 3 || angles.Count == 4 || angles.Count == 5)
                {
                    sumDistance1 = distances[1] + distances[0];
                    sumDistance2 = distances[2] + distances[0];
                    sumDistance3 = distances[1] + distances[2];
                }
                //For cirlce make sure that the differnece between corners are almost euqal to zero (0 to 50)
                double circleDiffDistance = 0;
                if (angles.Count >= 6 && angles.Count <= 9)
                {
                    for (int i = 0; i < distances.Count;i++ )
                    {
                        circleDiffDistance = distances[i] - circleDiffDistance;
                        if ( circleDiffDistance < 0 )
                        {
                            circleDiffDistance = circleDiffDistance * -1;
                        }
                    }
                }
                //Rectangle or sqaure has one angle at least that is 90 (between 70 and 120) and two parallel lines are equal and number of angles are 4 and sum on angles are between 340 and 500
                if ((angles.Count == 4 || angles.Count == 5) && ((angles[0] >= 70 && angles[0] <= 120) || (angles[1] >= 70 && angles[1] <= 120) || (angles[3] >= 70 && angles[3] <= 120) || (angles[2] >= 70 && angles[2] <= 120)) && (sumAngle >= 350 && sumAngle <= 500))
                {
                    if ( diffDistance1 <= 5 && diffDistance2 <= 5 && diffDistance <= 5) {
                        //Square will have equal distances and 5 is threshold between 0 and 5)
                        output = "Square";
                    } else if ((diffDistance1 <= 50 || diffDistance2 <= 50))
                    {
                        output = "Rectangle";
                    } else {
                        //otherwise it is most probably a rectangle 
                        output = "Rectangle";
                    }
                }
                //Triangle has one angle at least that is equal or less than 90 and has three angles
                else if ((angles.Count == 3 || angles.Count == 4 || angles.Count == 5) && (angles[0] <= 90 || angles[1] <= 90 || angles[2] <= 90) && (distances[0] < sumDistance3 && distances[1] < sumDistance2 && distances[2] < sumDistance1))
                {
                    output = "Triangle";
                }
                //Arrow should have 4 to 7 corners and the first line should has distance bigger than the other three lines at least
                else if ((angles.Count >= 4 && angles.Count <= 7) && (distances[0] > (distances[1] + distances[2] + distances[3]))) {
                    output = "Arrow";
                }
                //cirlce has sum of angles are greater or equal than 600 and the difference between all lines in the cirlce are almsot equal to zero (0 to 100) 
                else if ((angles.Count >= 6 && angles.Count <= 9) && sumAngle >= 600 && circleDiffDistance <= 50)
                {
                     output = "Circle";
                }
                //ellipse has sum of angles between 500 and 600
                else if ((angles.Count >= 6 && angles.Count <= 9) && (sumAngle < 600 && sumAngle > 500))
                {
                    output = "Ellipse";
                }
               else {
                   output = "other";
               }
            }
            if (output != "other" && output != "Line")
            {
                Point[] strokePoints;
                if (output == "Circle" || output == "Ellipse" || output == "Arrow")
                {
                    strokePoints = (Point[])stroke.StylusPoints;
                }
                else
                {
                    strokePoints = new Point[cornerPoints.Count];
                    for (int i = 0; i < cornerPoints.Count; i++)
                    {
                        //Get the points
                        strokePoints[i] = (Point)cornerPoints[i];

                    }
                }
                //Convert the recognized strokes to shapes
                Polyline shape = new Polyline();
                //Get the points from the stroke
                PointCollection points = new PointCollection(strokePoints);
                //use the stroke points as Polyline points
                shape.Points = points;
                //Convert it to shape
                Shape shapeToAdd = shape;
                shapeToAdd.Stroke = Brushes.DarkGreen;
                shapeToAdd.StrokeThickness = 8;
                shapesList.Add(shapeToAdd);
                Notes.Children.Add(shapeToAdd);
                Notes.Strokes.Remove(stroke);
            }
            return output;
        }

        //Implement shortstraw Alg.
        private List<StylusPoint> shortStraw(Stroke stroke)
        {
            //Convert Strokes to Points
            StylusPoint[] points = strokeGetPoints(stroke);
            //Get resample spacing.
            double s = determineResamplePoint(points);
            //Get resample points.
            StylusPoint[] resampled = resamplePoints(points, s);
            //Get corners.
            int[] corners = getCorners(resampled);

            List<StylusPoint> cornerPoints = new List<StylusPoint>();
            for (int i = 0; i < corners.Length; i++)
            {
                cornerPoints.Add(resampled[corners[i]]);
            }
            return cornerPoints;
        } 

        // Get all points from a Stroke.
        private StylusPoint[] strokeGetPoints(Stroke oStroke)
        {
            int iRow = -1;
            StylusPointCollection colStylusPoints = oStroke.StylusPoints;
            StylusPoint [] AllPoints = new StylusPoint[colStylusPoints.Count];
            foreach (StylusPoint oPoint in colStylusPoints)
            {
                iRow += 1;
                AllPoints[iRow] = oPoint;
            }
            return AllPoints;
        }

        //Input: A series of points
        //Output: The interspacing distance for the resampled points
        private double determineResamplePoint(StylusPoint[] points)       
        {
            //Get boundingBox
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            for (int i = 0; i < points.Length;i++ )
            {
                StylusPoint p = points[i];
                if (p.X < minX)
                {
                    minX = p.X;
                }
                if (p.X > maxX)
                {
                    maxX = p.X;
                }
                if (p.Y < minY)
                {
                    minY = p.Y;
                }
                if (p.Y > maxY)
                {
                    maxY = p.Y;
                }

            }
            double x = minX;
            double y = minY;
            double w = maxX - minX;
            double h = maxY - minY;
            //topleft
            StylusPoint p1 = new StylusPoint();
            p1.X = x;
            p1.Y = y;
            //bottomRight
            StylusPoint p2 = new StylusPoint();
            p2.X = x+w;
            p2.Y = y+h;
            double diagonal = distance(p1,p2);
            double s = diagonal / 40.0;

            return s;
        }

        //Get the angle between two points
        private double angle(StylusPoint p1, StylusPoint p2)
        {
            double deltaX = p2.X - p1.X;
            double deltaY = p2.Y - p1.Y;
            return (Math.Atan2(deltaY,deltaX)*180)/Math.PI;
        }

        //Get the distance between tw points
        private double distance(StylusPoint p1, StylusPoint p2)
        {
            double deltaX = p2.X - p1.X;
            double deltaY = p2.Y - p1.Y;
            return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
        }

        //Input: A series of points and an interspacing distance
        //Output: The resampled points
        private StylusPoint[] resamplePoints(StylusPoint[] points, double s)
        {
            double D = 0;
            List<StylusPoint> resampledList = new List<StylusPoint>();
            resampledList.Add(points[0]);
            for (int i = 1; i < points.Length; i++)
            {
                StylusPoint p1 = points[i-1];
                StylusPoint p2 = points[i];
                double d = distance(p1,p2);
                if ((D + d) >= s)
                {
                    StylusPoint q = new StylusPoint();
                    q.X = p1.X + ((s - D) / d) * (p2.X - p1.X);
                    q.Y = p1.Y + ((s - D) / d) * (p2.Y - p1.Y);
                    resampledList.Add(q);
                    points[i] = q;
                    D = 0;
                }
                else
                {
                    D = D + d;
                }
            }
            StylusPoint[] resampled = resampledList.ToArray();
            return resampled;
        }

        //Input: A series of resampled points
        //Output: The resampled points that correspond to corners
        private int[] getCorners(StylusPoint[] points)
        {
            int W = 3;
            List<int> cornersList = new List<int>();
            cornersList.Add(0);
            List<double> strawsList = new List<double>();
            for (int j = W; j < (points.Length - W); j++)
            {
                strawsList.Add(distance(points[j-W],points[j+W]));
            }
            double t = getMedian(strawsList) * 0.95;

            for (int i = W; i < (points.Length - W); i++)
            {
                //Decrease the index by straw window (3) so the list will not be out of index
                if (strawsList[i-W] < t)
                {
                    //Decrease the index by straw window (3) so the list will not be out of index
                    double localMin = double.MaxValue;
                    int localMinIndex = i;
                    while (i < strawsList.Count && strawsList[i-W] < t)
                    {
                        if (strawsList[i-W] < localMin)
                        {
                            localMin = strawsList[i-W];
                            localMinIndex = i; 
                        }
                        i = i + 1;
                    }
                    cornersList.Add(localMinIndex);
                }
            }
            cornersList.Add(points.Length-1);

            cornersList = postProcessCorners(points, strawsList, cornersList);
            int[] corners = cornersList.ToArray();
            return corners;
        }

        //Get the Median
        private double getMedian(List<double> pNumbers)
        {
            int size = pNumbers.Count;
            int mid = size / 2;
            pNumbers.Sort();
            double median = (size % 2 != 0) ? pNumbers[mid] :
            (pNumbers[mid] + pNumbers[mid - 1]) / 2;
            return median;
        }

        //Input: A series of resampled points, an initial set of corners,and the straw distances for each point
        //Output: A set of corners post-processed with higher-level polyline rules
        private List<int> postProcessCorners(StylusPoint[] points, List<double> straws, List<int> corners)
        {
            bool cont = false;
            while (!cont)
            {
                cont = true;
                for (int i = 1; i < corners.Count; i++)
                {
                    int c1 = corners[i - 1];
                    int c2 = corners[i];
                    if (!isLine(points,c1,c2)) {
                        int newCorner = halfwayCorner(straws,c1,c2);
                        // This checking was not in the paper,but prevents adding undefined points
                        if (newCorner > c1 && newCorner < c2)
                        {
                            corners.Insert(i,newCorner);
                            cont = false;

                        }
                    }
                }
            }
            for (int i = 1; i < (corners.Count- 1); i++)
            {
                int c1 = corners[i - 1];
                int c2 = corners[i + 1];
                if (isLine(points, c1, c2))
                {
                    corners.RemoveAt(i);
                    i = i - 1;
                }
            }
           
            return corners;
        }
        
        //Input: A series of points and two indices, a and b
        //Output: A boolean for whether or not the stroke segment between points at a and b is a line
        private bool isLine(StylusPoint[] points, int a , int b)
        {
            double threshold = 0.95;
            double dis =distance(points[a],points[b]);
            double d = 0;
            for (int i = a; i < b; i++)
            {
                double newDis = distance(points[i], points[i+1]); ;
                d = d + newDis;
            }
            double pathDistance = d;
            if ((dis/pathDistance) > threshold)
            {
                return true;
            } else {
                return false;
            }
        }

        //Input: The straw distances for each point, two point indices a and b
        //Output: A possible corner between the points at a and b
        private int halfwayCorner(List<double> straws, int a, int b)
        {
            int quarter = (b - a) / 4;
            //Assgin local min to some really big number (INFINITY)
            double minValue = double.MaxValue;
            int minIndex = 0;
            
            for (int i = (a + quarter); i < (b - quarter); i++)
            {
                //Make sure that the straws list will not be out of index
                if ( i < straws.Count()) {
                    if (straws[i] < minValue)
                    {
                        minValue = straws[i];
                        minIndex = i;
                    }
                }
            }

            return minIndex;
        }

        // Give the selection the Pen color.
        private void cboPenColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Notes == null) return;
            //Check if it is a Pen or Highlighter
            if (Notes.DefaultDrawingAttributes.StylusTip == StylusTip.Rectangle) return;
            if (e.AddedItems.Count < 1) return;
            ComboBoxItem item = (ComboBoxItem)e.AddedItems[0];
            //The selected color
            string value = item.Content.ToString();
            switch (value)
            {
                case "Black":
                    Notes.DefaultDrawingAttributes.Color = Colors.Black;
                    break;
                case "Red":
                    Notes.DefaultDrawingAttributes.Color = Colors.Red;
                    break;
                case "Green":
                    Notes.DefaultDrawingAttributes.Color = Colors.Green;
                    break;
                case "Blue":
                    Notes.DefaultDrawingAttributes.Color = Colors.Blue;
                    break;
                case "Purple":
                    Notes.DefaultDrawingAttributes.Color = Colors.Purple;
                    break;
            }
        }

        // Give the selection the chosen Pen or Highlighter or Eraser or Selection Mode or Scribbling Gesture.
        private void cboEditing_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Notes == null) return;
            //Set Pen and Highlighter Colors to black each time the editing mode is changed
            cboPenColor.SelectedIndex = 0;
            cboHighlighterColor.SelectedIndex = 0;
            //By Default set the editing mode to Ink
            Notes.EditingMode = InkCanvasEditingMode.Ink;
            if (e.AddedItems.Count < 1) return;
            ComboBoxItem item = (ComboBoxItem)e.AddedItems[0];
            //The selected editing mode
            string value = item.Content.ToString();
            switch (value)
            {
                case "Pen":
                    // Set up the DrawingAttributes for the pen.
                    Notes.EditingMode = InkCanvasEditingMode.Ink;
                    DrawingAttributes penDA;
                    penDA = new DrawingAttributes();
                    penDA.FitToCurve = false;
                    Notes.DefaultDrawingAttributes = penDA;
                    textBox1.Text = "Ink Mode";
                    break;
                case "Highlighter":
                    // Set up the DrawingAttributes for the highlighter.
                    Notes.EditingMode = InkCanvasEditingMode.Ink;
                    DrawingAttributes highlighterDA;
                    highlighterDA = new DrawingAttributes();
                    highlighterDA.IsHighlighter = true;
                    highlighterDA.IgnorePressure = true;
                    highlighterDA.StylusTip = StylusTip.Rectangle;
                    highlighterDA.Height = 30;
                    highlighterDA.Width = 10;
                    Notes.DefaultDrawingAttributes = highlighterDA;
                    textBox1.Text = "Ink Mode";
                    break;
                case "Erase by Stroke":
                    Notes.EditingMode = InkCanvasEditingMode.EraseByStroke;
                    textBox1.Text = "Erase by Stroke Mode";
                    break;
                case "Erase by Point":
                    Notes.EditingMode = InkCanvasEditingMode.EraseByPoint;
                    textBox1.Text = "Erase by Point Mode";
                    break;
                case "Selection Mode":
                    Notes.EditingMode = InkCanvasEditingMode.Select;
                    textBox1.Text = "Selection Mode";
                    break;
                case "Scribbling Gesture":
                    Notes.EditingMode = InkCanvasEditingMode.GestureOnly;
                    //Enable the scratchout gesture and call the function for dleeting strokes and shapes
                    Notes.Gesture += new InkCanvasGestureEventHandler(onGesture);
                    Notes.SetEnabledGestures(new ApplicationGesture[] { ApplicationGesture.ScratchOut });
                    //Display a message telling it is Scribbling Gesture mode
                    textBox1.Text = "Scribbling Gesture Mode";
                    break;
            }
        }

        

        // Give the selection the chosen Highlighter Color.
        private void cboHighlighterColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Notes == null) return;
            //Check if it is a Pen or Highlighter
            if (Notes.DefaultDrawingAttributes.StylusTip != StylusTip.Rectangle) return;
            if (e.AddedItems.Count < 1) return;
            ComboBoxItem item = (ComboBoxItem)e.AddedItems[0];
            //The selected color
            string value = item.Content.ToString();
            switch (value)
            {
                case "Black":
                    Notes.DefaultDrawingAttributes.Color = Colors.Black;
                    break;
                case "Green":
                    Notes.DefaultDrawingAttributes.Color = Colors.Green;
                    break;
                case "Purple":
                    Notes.DefaultDrawingAttributes.Color = Colors.Purple;
                    break;
            }
        }

        // Load Notes (ISF files).
        private void mnuFileOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.Filter = "Ink Serialized Format (*.isf)|*.isf|" +
                         "All files (*.*)|*.*";

            if ((bool)dlg.ShowDialog(this))
            {
                this.Notes.Strokes.Clear();

                try
                {
                    using (FileStream file = new FileStream(dlg.FileName,
                                                FileMode.Open, FileAccess.Read))
                    {
                        if (!dlg.FileName.ToLower().EndsWith(".isf"))
                        {
                            MessageBox.Show("The requested file is not a Ink Serialized Format file\r\n\r\nplease retry", Title);
                        }
                        else
                        {
                            Notes.Strokes = new StrokeCollection(file);
                            file.Close();
                        }
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, Title);
                }
            }
        }


        // Save Notes as ISF file.
        private void mnuFileSave_Click(object sender, RoutedEventArgs e)
        {
            // Get the file name.
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = ".isf";
            dlg.Filter = "Ink Serialized Format (*.isf)|*.isf|" +
                         "All files (*.*)|*.*";

            if (dlg.ShowDialog() == true)
            {
                using (FileStream file = new FileStream(dlg.FileName,
                                            FileMode.Create, FileAccess.Write))
                    try
                    {
                        Notes.Strokes.Save(file); // Save.
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message,
                            "Error Saving File",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                    finally
                    {
                        file.Close();
                    }
            }
        }

        //Clear the window from all strokes and shapes and times
        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            Notes.Strokes.Clear();
            Notes.Children.Clear();
            shapesList.Clear();
            shapesGroupTouched.Clear();
            currentTimeSec.Clear();
        }

        //Close the application
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
