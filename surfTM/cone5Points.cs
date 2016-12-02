using Rhino;
using Rhino.Geometry;

using Grasshopper;

using System;
using System.Drawing;
using System.Collections.Generic;




namespace gsd {
    class cone5pts {
        private void RunScript(List<Point3d> fivePts, ref object outStrings, ref object outPoints,
            ref object outCircles, ref object outEllipses, ref object outParabolas, ref object outHyperbolas) {




            #region beginScript
            List<string> updateStrings = new List<string>();
            List<Circle> updateCircles = new List<Circle>();
            List<Ellipse> updateEllipses = new List<Ellipse>();
            List<Point3d> updatePoints = new List<Point3d>();
            List<Curve> updateParabolas = new List<Curve>();
            List<Curve> updateHyperbolas = new List<Curve>();
            string equation;
            for (int i = 0; i < fivePts.Count; i += 5) {

                //set inputs
                Point3d[] points = new Point3d[5];
                points[0] = fivePts[i + 0];
                points[1] = fivePts[i + 1];
                points[2] = fivePts[i + 2];
                points[3] = fivePts[i + 3];
                points[4] = fivePts[i + 4];

                double x1 = points[0].X;
                double x2 = points[1].X;
                double x3 = points[2].X;
                double x4 = points[3].X;
                double x5 = points[4].X;

                double y1 = points[0].Y;
                double y2 = points[1].Y;
                double y3 = points[2].Y;
                double y4 = points[3].Y;
                double y5 = points[4].Y;


                //get into General Form
                //Ax^2 + Bxy + Cy^2 + Dx + Ey + F = 0

                //A
                double A =
                  x1 * y1 * mat16v(y2 * y2, x2, y2, 1, y3 * y3, x3, y3, 1, y4 * y4, x4, y4, 1, y5 * y5, x5, y5, 1)
                  - y1 * y1 * mat16v(x2 * y2, x2, y2, 1, x3 * y3, x3, y3, 1, x4 * y4, x4, y4, 1, x5 * y5, x5, y5, 1)
                  + x1 * mat16v(x2 * y2, y2 * y2, y2, 1, x3 * y3, y3 * y3, y3, 1, x4 * y4, y4 * y4, y4, 1, x5 * y5, y5 * y5, y5, 1)
                  - y1 * mat16v(x2 * y2, y2 * y2, x2, 1, x3 * y3, y3 * y3, x3, 1, x4 * y4, y4 * y4, x4, 1, x5 * y5, y5 * y5, x5, 1)
                  + mat16v(x2 * y2, y2 * y2, x2, y2, x3 * y3, y3 * y3, x3, y3, x4 * y4, y4 * y4, x4, y4, x5 * y5, y5 * y5, x5, y5);
                //B
                double B = -(
                  x1 * x1 * mat16v(y2 * y2, x2, y2, 1, y3 * y3, x3, y3, 1, y4 * y4, x4, y4, 1, y5 * y5, x5, y5, 1)
                  - y1 * y1 * mat16v(x2 * x2, x2, y2, 1, x3 * x3, x3, y3, 1, x4 * x4, x4, y4, 1, x5 * x5, x5, y5, 1)
                  + x1 * mat16v(x2 * x2, y2 * y2, y2, 1, x3 * x3, y3 * y3, y3, 1, x4 * x4, y4 * y4, y4, 1, x5 * x5, y5 * y5, y5, 1)
                  - y1 * mat16v(x2 * x2, y2 * y2, x2, 1, x3 * x3, y3 * y3, x3, 1, x4 * x4, y4 * y4, x4, 1, x5 * x5, y5 * y5, x5, 1)
                  + mat16v(x2 * x2, y2 * y2, x2, y2, x3 * x3, y3 * y3, x3, y3, x4 * x4, y4 * y4, x4, y4, x5 * x5, y5 * y5, x5, y5));
                //C
                double C =
                  x1 * x1 * mat16v(x2 * y2, x2, y2, 1, x3 * y3, x3, y3, 1, x4 * y4, x4, y4, 1, x5 * y5, x5, y5, 1)
                  - x1 * y1 * mat16v(x2 * x2, x2, y2, 1, x3 * x3, x3, y3, 1, x4 * x4, x4, y4, 1, x5 * x5, x5, y5, 1)
                  + x1 * mat16v(x2 * x2, x2 * y2, y2, 1, x3 * x3, x3 * y3, y3, 1, x4 * x4, x4 * y4, y4, 1, x5 * x5, x5 * y5, y5, 1)
                  - y1 * mat16v(x2 * x2, x2 * y2, x2, 1, x3 * x3, x3 * y3, x3, 1, x4 * x4, x4 * y4, x4, 1, x5 * x5, x5 * y5, x5, 1)
                  + mat16v(x2 * x2, x2 * y2, x2, y2, x3 * x3, x3 * y3, x3, y3, x4 * x4, x4 * y4, x4, y4, x5 * x5, x5 * y5, x5, y5);
                //D
                double D = -(
                  x1 * x1 * mat16v(x2 * y2, y2 * y2, y2, 1, x3 * y3, y3 * y3, y3, 1, x4 * y4, y4 * y4, y4, 1, x5 * y5, y5 * y5, y5, 1)
                  - x1 * y1 * mat16v(x2 * x2, y2 * y2, y2, 1, x3 * x3, y3 * y3, y3, 1, x4 * x4, y4 * y4, y4, 1, x5 * x5, y5 * y5, y5, 1)
                  + y1 * y1 * mat16v(x2 * x2, x2 * y2, y2, 1, x3 * x3, x3 * y3, y3, 1, x4 * x4, x4 * y4, y4, 1, x5 * x5, x5 * y5, y5, 1)
                  - y1 * mat16v(x2 * x2, x2 * y2, y2 * y2, 1, x3 * x3, x3 * y3, y3 * y3, 1, x4 * x4, x4 * y4, y4 * y4, 1, x5 * x5, x5 * y5, y5 * y5, 1)
                  + mat16v(x2 * x2, x2 * y2, y2 * y2, y2, x3 * x3, x3 * y3, y3 * y3, y3, x4 * x4, x4 * y4, y4 * y4, y4, x5 * x5, x5 * y5, y5 * y5, y5));
                //E
                double E =
                  x1 * x1 * mat16v(x2 * y2, y2 * y2, x2, 1, x3 * y3, y3 * y3, x3, 1, x4 * y4, y4 * y4, x4, 1, x5 * y5, y5 * y5, x5, 1)
                  - x1 * y1 * mat16v(x2 * x2, y2 * y2, x2, 1, x3 * x3, y3 * y3, x3, 1, x4 * x4, y4 * y4, x4, 1, x5 * x5, y5 * y5, x5, 1)
                  + y1 * y1 * mat16v(x2 * x2, x2 * y2, x2, 1, x3 * x3, x3 * y3, x3, 1, x4 * x4, x4 * y4, x4, 1, x5 * x5, x5 * y5, x5, 1)
                  - x1 * mat16v(x2 * x2, x2 * y2, y2 * y2, 1, x3 * x3, x3 * y3, y3 * y3, 1, x4 * x4, x4 * y4, y4 * y4, 1, x5 * x5, x5 * y5, y5 * y5, 1)
                  + mat16v(x2 * x2, x2 * y2, y2 * y2, x2, x3 * x3, x3 * y3, y3 * y3, x3, x4 * x4, x4 * y4, y4 * y4, x4, x5 * x5, x5 * y5, y5 * y5, x5);
                //F
                double F = -(x1 * x1
                  * mat16v(x2 * y2, y2 * y2, x2, y2, x3 * y3, y3 * y3, x3, y3, x4 * y4, y4 * y4, x4, y4, x5 * y5, y5 * y5, x5, y5) - x1 * y1
                  * mat16v(x2 * x2, y2 * y2, x2, y2, x3 * x3, y3 * y3, x3, y3, x4 * x4, y4 * y4, x4, y4, x5 * x5, y5 * y5, x5, y5) + y1 * y1
                  * mat16v(x2 * x2, x2 * y2, x2, y2, x3 * x3, x3 * y3, x3, y3, x4 * x4, x4 * y4, x4, y4, x5 * x5, x5 * y5, x5, y5) - x1
                  * mat16v(x2 * x2, x2 * y2, y2 * y2, y2, x3 * x3, x3 * y3, y3 * y3, y3, x4 * x4, x4 * y4, y4 * y4, y4, x5 * x5, x5 * y5, y5 * y5, y5) + y1
                  * mat16v(x2 * x2, x2 * y2, y2 * y2, x2, x3 * x3, x3 * y3, y3 * y3, x3, x4 * x4, x4 * y4, y4 * y4, x4, x5 * x5, x5 * y5, y5 * y5, x5));


                //round numbers to give some feedback
                equation = Math.Floor(A) + "x^2 + " + Math.Floor(B) + "xy + " + Math.Floor(C) + "y^2 + " + Math.Floor(D) + "x + " + Math.Floor(E) + "y + " + Math.Floor(F) + " = 0";



                //determine the type of conic
                //Ax^2 + Bxy + Cy^2 + Dx + Ey + F = 0
                string conic = "";
                double quadratic = B * B - (4 * A * C);

                if (quadratic > 0) { conic = "hyperbola"; }
                if (quadratic == 0) { conic = "parabola"; }
                if (quadratic < 0) { conic = "ellipse"; }
                double ac = Math.Abs(A - C);
                if (quadratic < 0 && ac < 0.001) { conic = "circle"; }

                //output feedback
                updateStrings.Add(equation);
                updateStrings.Add(conic);


                //draw curves
                //try { Plane plane = new Plane(points[0], points[1], points[2]); } catch { }
                BoundingBox bBox = new BoundingBox(points);
                double range = bBox.Max.X - bBox.Min.X;
                int steps = 20;
                double dx = range / steps;
                List<Point3d> graphPoints0;
                List<Point3d> graphPoints1;

                switch (conic) {
                    case "circle":
                        //needs Z
                        Point3d circleCenter = new Point3d((-D / (2 * A)), (-E / (2 * C)), points[0].Z);
                        double radius = Math.Sqrt((-F / A) + (D * D / (4 * A * A)) + (E * E / (4 * A * A)));
                        Circle c = new Circle(circleCenter, radius);
                        updateCircles.Add(c);
                        break;
                    case "ellipse":

                        //needs Z
                        double h = (-D / (2 * A));
                        double k = (-E / (2 * C));
                        double b = Math.Sqrt((-F / A + (E * E / (4 * A * C)) + (D * D / (4 * A * A))));
                        double a = Math.Sqrt((-F / C + (E * E / (4 * C * C)) + (D * D / (4 * A * C))));
                        Point3d center = new Point3d(h, k, points[0].Z);
                        Point3d vertex0 = new Point3d(h, (k + a), points[0].Z);
                        Point3d vertex1 = new Point3d(h + b, k, points[0].Z);
                        Ellipse e = new Ellipse(center, vertex0, vertex1);
                        updatePoints.Add(center);
                        updatePoints.Add(vertex0);
                        updatePoints.Add(vertex1);
                        updateEllipses.Add(e);

                        break;
                    case "parabola":
                        graphPoints0 = new List<Point3d>();
                        graphPoints1 = new List<Point3d>();
                        for (int j = 0; j < steps; ++j) {
                            double x = bBox.Min.X + (dx * j);
                            double y;

                            //trash
                            y = -1 * (Math.Sqrt((E * E) + (2 * x * B * E) - (4 * C * F) - (4 * x * C * D) - (4 * x * x * A * C) + (x * x * B * B) + E + (x * B)) / (2 * C));
                            Point3d pt0 = new Point3d(x, y, points[0].Z);
                            if (pt0.IsValid) {
                                graphPoints1.Add(pt0);
                            }
                            y = (Math.Sqrt((E * E) + (2 * x * B * E) - (4 * C * F) - (4 * x * C * D) - (4 * x * x * A * C) + (x * x * B * B) - E - (x * B)) / (2 * C));
                            Point3d pt1 = new Point3d(x, y, points[0].Z);
                            if (pt1.IsValid) {
                                graphPoints1.Add(pt1);
                            }
                        }

                        updatePoints.AddRange(graphPoints0);
                        updatePoints.AddRange(graphPoints1);

                        Curve parabola;
                        parabola = PolyCurve.CreateInterpolatedCurve(graphPoints0, 2);
                        updateParabolas.Add(parabola);
                        parabola = PolyCurve.CreateInterpolatedCurve(graphPoints1, 2);
                        updateParabolas.Add(parabola);



                        break;
                    case "hyperbola":
                        graphPoints0 = new List<Point3d>();
                        graphPoints1 = new List<Point3d>();
                        for (int j = 0; j < steps; ++j) {
                            double x = bBox.Min.X + (dx * j);
                            double y;

                            //trash
                            y = -1 * (Math.Sqrt((E * E) + (2 * x * B * E) - (4 * C * F) - (4 * x * C * D) - (4 * x * x * A * C) + (x * x * B * B) + E + (x * B)) / (2 * C));
                            Point3d pt0 = new Point3d(x, y, points[0].Z);
                            if (pt0.IsValid) {
                                graphPoints1.Add(pt0);
                            }
                            y = (Math.Sqrt((E * E) + (2 * x * B * E) - (4 * C * F) - (4 * x * C * D) - (4 * x * x * A * C) + (x * x * B * B) - E - (x * B)) / (2 * C));
                            Point3d pt1 = new Point3d(x, y, points[0].Z);
                            if (pt1.IsValid) {
                                graphPoints1.Add(pt1);
                            }
                        }

                        updatePoints.AddRange(graphPoints0);

                        updatePoints.AddRange(graphPoints1);

                        //Curve hyperbola;
                        //hyperbola = Curve.CreateInterpolatedCurve(graphPoints0, 2);
                        Polyline pl = new Polyline(graphPoints0);
                        updateHyperbolas.Add(pl.ToNurbsCurve());
                        //hyperbola = PolyCurve.CreateInterpolatedCurve(graphPoints0, 2);
                        //updateHyperbolas.Add(hyperbola);
                        //hyperbola = PolyCurve.CreateInterpolatedCurve(graphPoints1, 2);
                        //updateHyperbolas.Add(hyperbola);

                        break;
                    default:
                        break;
                }



            }
            outPoints = updatePoints;
            outCircles = updateCircles;
            outEllipses = updateEllipses;
            outParabolas = updateParabolas;
            outHyperbolas = updateHyperbolas;
            outStrings = updateStrings;
            #endregion






        }

        // <Custom additional code> 
        #region customCode
        double mat16v(double a, double b, double c, double d,
          double e, double f, double g, double h,
          double i, double j, double k, double l,
        double m, double n, double o, double p) {
            double mat16 = a * (f * k * p + g * l * n + h * j * o - h * k * n - g * j * p - f * o * l)
              - b * (e * k * p + g * l * m + h * i * o - h * k * m - g * i * p - e * o * l)
              + c * (e * j * p + f * l * m + h * i * n - h * j * m - f * i * p - e * n * l)
              - d * (e * j * o + f * k * m + g * i * n - g * j * m - f * i * o - e * n * k);
            return mat16;
        }
        double mat9v(double a, double b, double c,
          double d, double e, double f,
        double g, double h, double i) {
            double mat9 = a * e * i + b * f * g + c * d * h - a * f * h - b * d * i - c * e * g;
            return mat9;
        }
        #endregion


        // </Custom additional code> 
    }
}