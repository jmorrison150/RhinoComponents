          Curve c;
          double t;
      Vector3d vector3d1 = c.TangentAt(destination2);
      Vector3d vector3d2 = c.CurvatureAt(t);
      double length = vector3d2.Length;
      if (length > 1.490116119385E-08)
      {
        Vector3d vector3d3 = (vector3d2 / (length * length));
        Circle circle;
        circle = new Circle(point3d, vector3d1, (point3d +(vector3d3* 2.0)));
      }