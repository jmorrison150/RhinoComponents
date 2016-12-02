////intersection lines
//    private void RunScript(List<Surface> surfaces, object y, ref object A) {
//        List<Curve> cvs = new List<Curve>();
//        for (int i = 0; i < surfaces.Count; i++) {
//            for (int j = i+1; j < surfaces.Count; j++) {
//                //Rhino.Geometry.Intersect
//                Curve[] intersectionCurves;
//                Point3d[] intersectionPoints;
//                Rhino.Geometry.Intersect.Intersection.SurfaceSurface(surfaces[i], surfaces[j], 0.001, out intersectionCurves, out intersectionPoints);
//                cvs.AddRange(intersectionCurves);
//            }
//        }
//        A = cvs;
//    }





////angles 0 to 90
    //private void RunScript(List<Curve> x, object y, ref object A) {
    //    List<double> angles = new List<double>();
    //    double uMax = 90.0;
    //    double uMin = 0.0;
    //    int uCount = x.Count;
    //    double uStep = (uMax - uMin) / (uCount-1);
    //    double angle = uMin;

    //    for (int i = 0; i < x.Count; i++) {
    //        angles.Add(angle);
    //        angle += uStep;
    //    }
        
    //    A = angles;
    //}



















    //private void RunScript(Surface surface, double assym, double frequency, double amplitude, Line axis, ref object A, ref object B) {


    //    //set variables
    //    int numberOfCurves = 3;
    //    int resolution = 50;

    //    //get input surface
    //    NurbsSurface nSurface = surface.ToNurbsSurface();
    //    double range0 = ( nSurface.Domain(0).Max - nSurface.Domain(0).Min ) / (double) ( numberOfCurves );
    //    double range1 = ( nSurface.Domain(1).Max - nSurface.Domain(1).Min ) / (double) ( resolution - 1 );

    //    //empty lists
    //    Curve[] isoCurves0 = new Curve[numberOfCurves];
    //    Curve[] isoCurves1 = new Curve[numberOfCurves];
    //    Point2d[][] pts = new Point2d[numberOfCurves][];

    //    //add sin(x) in uv space
    //    for(int i = 0; i < pts.Length; i++) {
    //        pts[i] = new Point2d[resolution];
    //        double ptX = ( range0 * ( i ) ) + nSurface.Domain(0).Min;
    //        for(int j = 0; j < pts[i].Length; j++) {
    //            double ptY = ( range1 * j ) + nSurface.Domain(1).Min;
    //            double left = ptX + ( ( Math.Sin(j * frequency) ) * amplitude * assym ) + i * 0.02;
    //            pts[i][j] = new Point2d(left, ptY);

    //        }
    //        isoCurves0[i] = nSurface.InterpolatedCurveOnSurfaceUV(pts[i], 0.001);

    //        //subtract sin(x) in uv space
    //        for(int j = 0; j < pts[i].Length; j++) {
    //            double ptY = ( range1 * j ) + nSurface.Domain(1).Min;
    //            double right = ptX - ( ( Math.Sin(j * frequency) ) * amplitude );
    //            pts[i][j] = new Point2d(right, ptY);
    //        }
    //        isoCurves1[i] = nSurface.InterpolatedCurveOnSurfaceUV(pts[i], 0.001);

    //    }

    //    //output
    //    A = isoCurves0;
    //    B = isoCurves1;
    //}






















