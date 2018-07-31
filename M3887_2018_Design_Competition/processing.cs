using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Collections;

using GH_IO;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance67 : GH_ScriptInstance {
    #region Utility functions
    /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
    /// <param name="text">String to print.</param>
    private void Print(string text) { /* Implementation hidden. */ }
    /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
    /// <param name="format">String format.</param>
    /// <param name="args">Formatting parameters.</param>
    private void Print(string format, params object[] args) { /* Implementation hidden. */ }
    /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
    /// <param name="obj">Object instance to parse.</param>
    private void Reflect(object obj) { /* Implementation hidden. */ }
    /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
    /// <param name="obj">Object instance to parse.</param>
    private void Reflect(object obj, string method_name) { /* Implementation hidden. */ }
    #endregion

    #region Members
    /// <summary>Gets the current Rhino document.</summary>
    private readonly RhinoDoc RhinoDocument;
    /// <summary>Gets the Grasshopper document that owns this script.</summary>
    private readonly GH_Document GrasshopperDocument;
    /// <summary>Gets the Grasshopper script component that owns this script.</summary>
    private readonly IGH_Component Component;
    /// <summary>
    /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
    /// Any subsequent call within the same solution will increment the Iteration count.
    /// </summary>
    private readonly int Iteration;
    #endregion

    /// <summary>
    /// This procedure contains the user code. Input parameters are provided as regular arguments,
    /// Output parameters as ref arguments. You don't have to assign output parameters,
    /// they will have a default value.
    /// </summary>
    private void RunScript(string imgFilePath, string imgFilePath2, bool reset, ref object A, ref object B) {




        #region runScript

        //setup image
        if (img == null) {
            //error check
            if (!System.IO.File.Exists(imgFilePath)) { throw new ArgumentException("File Does Not Exist"); }

            //load bitmaps
            img = new Bitmap(imgFilePath);
            img2 = new Bitmap(imgFilePath);
            maskImg = new Bitmap(imgFilePath2);



        }

        //how long to use flow
        if (frameCount % noiseStep == 0 || frameCount == 1) {
            double factor = random.Next(noiseScaleMin, noiseScaleMax);
            double scaler = Math.Pow(10, -1 * factor);
            noiseScale = scaler;

            noiseCount++;
            //noiseSeed(noiseCount);
            noiseMax = 1;
            noiseMin = 0;
        }

        Bitmap tempImg = img;
        Bitmap tempImg2 = img2;
        Bitmap tempImgN = img;
        Bitmap tempImg2N = img2;
        int origin = frameCount % 2;

        for (int i = 0; i < img.Width; i++) {
            for (int j = 0; j < img.Height; j++) {

                //add Perlin noise
                double noiseVal = noise((i + offsetX) * noiseScale, (j + offsetY) * noiseScale);
                noiseVal = map(noiseVal, noiseMin, noiseMax, 0, 3 * Math.PI);


                //aim
                Vector2d aim = new Vector2d(Math.Cos(noiseVal), Math.Sin(noiseVal));

                //interlace
                if (j % 2 == 0 && interlace) { aim *= -1; }

                aim.X = Math.Round(aim.X);
                aim.Y = Math.Round(aim.Y);
                aim.X *= xScale;
                aim.Y *= yScale;
                if (ortho) {
                    aim = orthoConstrain(aim);
                }
                if (interlace) {
                    aim.Y *= 2;
                    aim.X *= 2;
                }

                int index2X = (int)Math.Floor(i + aim.X);
                int index2Y = (int)Math.Floor(j + aim.Y);

                if (index2X < 0) { index2X = 0; }
                if (index2X > (tempImg.Width - 1)) { index2X = (tempImg.Width - 1); }
                if (index2Y < 0) { index2Y = 0; }
                if (index2Y > (tempImg.Height - 1)) { index2Y = (tempImg.Height - 1); }

                float th = -1;
                if (frameCount > framesBlack) {
                    th = 10 / 255;
                }
                if (frameCount > framesGrey) {
                    th = 200 / 255;
                }

                double brightness = maskImg.GetPixel(index2X, index2Y).GetBrightness();
                if (brightness > th) {
                    tempImgN.SetPixel(index2X, index2Y, tempImg.GetPixel(i, j));
                    tempImg2N.SetPixel(index2X, index2Y, tempImg.GetPixel(i, j));
                }
            }
        }

        //output mesh

        Interval dx = new Interval(0, img.Width);
        Interval dy = new Interval(0, img.Height);
        Mesh mesh = Mesh.CreateFromPlane(Plane.WorldXY, dx, dy, img.Width, img.Height);

        Color[] colours = new Color[mesh.Vertices.Count];
        for (int i = 0; i < mesh.Vertices.Count; i++) {
            Point3f vertex = mesh.Vertices[i];
            int x = (int)vertex.X;
            int y = (int)vertex.Y;
            colours[i] = tempImgN.GetPixel(x, y);
            mesh.Vertices.SetVertex(i, vertex.X, vertex.Y, -colours[i].B);
        }
        mesh.VertexColors.SetColors(colours);
        A = mesh;

        //A = tempImgN;
        //B = tempImg2N;



        #endregion



    }

    // <Custom additional code> 





    #region customCode



    /////////////////////////////////////////////////////
    ////   Copyright of Kinch and M. Casey Rehm      ////
    ////   always credit Kinc and M. Casey Rehm      ////
    ////   when utilizing this script for projects   ////
    ////   also credit all libraries used            ////                                             
    /////////////////////////////////////////////////////



  //  int totalFrames = 40; //the number of iterations this runs before exiting
    int framesBlack = 2; //the number of iterations before pixels on black part of mask are frozen
    int framesGrey = 5; //the number of iterations before pixels 
    float noiseStep = 20; //the length of time one flow field is used

    int noiseScaleMax = 3; //smaller number should make more noisewhile larger numbers should 
    int noiseScaleMin = 2; //make less keep these between 1-5 10^-n

    bool interlace = true; //alternates direction of field every other line if true
    bool ortho = true; //constrains to 90 degree motion if true

    int xScale = 2; //scales motion in x direction
    int yScale = 1; //scales motion in y direction



    /////////////////////////////////////////////////////////////////////////////////////////

    Bitmap img;
    Bitmap img2;
    Bitmap maskImg;

    double noiseScale;
    int noiseCount = 0;
    float noiseMax;
    float noiseMin;
    int offsetX = 6100;
    int offsetY = 3100;
    int frameCount = 0;
    Random random = new Random(0);

    Vector2d orthoConstrain(Vector2d vec) {
        Vector2d temp = vec;

        if (Math.Abs(vec.X) > Math.Abs(vec.Y)) {
            if (vec.X < 0) {
                temp.X = -1;
            } else {
                temp.X = 1;
            }

        } else {
            if (vec.Y < 0) {
                temp.Y = -1;
            } else {
                temp.Y = 1;
            }
        }


        return temp;

    }
    double noise(double x, double y) {
        Perlin perlin = new Perlin();
        double rndVal = Perlin.noise(x, y, 0);
        return rndVal;
    }
    double map(double number, double low1, double high1, double low2, double high2) {
        return low2 + (high2 - low2) * (number - low1) / (high1 - low1);
    }
    public class Perlin {

        public static double OctavePerlin(double x, double y, double z, int octaves, double persistence) {
            double total = 0;
            double frequency = 1;
            double amplitude = 1;
            for (int i = 0; i < octaves; i++) {
                total += noise(x * frequency, y * frequency, z * frequency) * amplitude;

                amplitude *= persistence;
                frequency *= 2;
            }

            return total;
        }

        private static readonly int[] permutation = { 151,160,137,91,90,15,					// Hash lookup table as defined by Ken Perlin.  This is a randomly
      131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,	// arranged array of all numbers from 0-255 inclusive.
      190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
      88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
      77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
      102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
      135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
      5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
      223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
      129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
      251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
      49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
      138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
      };

        private static readonly int[] p;                                                    // Doubled permutation to avoid overflow

        static Perlin() {
            p = new int[512];
            for (int x = 0; x < 512; x++) {
                p[x] = permutation[x % 256];
            }
        }

        public static double noise(double x, double y, double z) {
            int repeat = 0;
            if (repeat > 0) {                                   // If we have any repeat on, change the coordinates to their "local" repetitions
                x = x % repeat;
                y = y % repeat;
                z = z % repeat;
            }

            int xi = (int)x & 255;                              // Calculate the "unit cube" that the point asked will be located in
            int yi = (int)y & 255;                              // The left bound is ( |_x_|,|_y_|,|_z_| ) and the right bound is that
            int zi = (int)z & 255;                              // plus 1.  Next we calculate the location (from 0.0 to 1.0) in that cube.
            double xf = x - (int)x;                             // We also fade the location to smooth the result.
            double yf = y - (int)y;
            double zf = z - (int)z;
            double u = fade(xf);
            double v = fade(yf);
            double w = fade(zf);

            int a = p[xi] + yi;                             // This here is Perlin's hash function.  We take our x value (remember,
            int aa = p[a] + zi;                             // between 0 and 255) and get a random value (from our p[] array above) between
            int ab = p[a + 1] + zi;                             // 0 and 255.  We then add y to it and plug that into p[], and add z to that.
            int b = p[xi + 1] + yi;                             // Then, we get another random value by adding 1 to that and putting it into p[]
            int ba = p[b] + zi;                             // and add z to it.  We do the whole thing over again starting with x+1.  Later
            int bb = p[b + 1] + zi;                             // we plug aa, ab, ba, and bb back into p[] along with their +1's to get another set.
                                                                // in the end we have 8 values between 0 and 255 - one for each vertex on the unit cube.
                                                                // These are all interpolated together using u, v, and w below.

            double x1, x2, y1, y2;
            x1 = lerp(grad(p[aa], xf, yf, zf), // This is where the "magic" happens.  We calculate a new set of p[] values and use that to get
              grad(p[ba], xf - 1, yf, zf), // our final gradient values.  Then, we interpolate between those gradients with the u value to get
              u);                                       // 4 x-values.  Next, we interpolate between the 4 x-values with v to get 2 y-values.  Finally,
            x2 = lerp(grad(p[ab], xf, yf - 1, zf), // we interpolate between the y-values to get a z-value.
              grad(p[bb], xf - 1, yf - 1, zf),
              u);                                       // When calculating the p[] values, remember that above, p[a+1] expands to p[xi]+yi+1 -- so you are
            y1 = lerp(x1, x2, v);                               // essentially adding 1 to yi.  Likewise, p[ab+1] expands to p[p[xi]+yi+1]+zi+1] -- so you are adding
                                                                // to zi.  The other 3 parameters are your possible return values (see grad()), which are actually
            x1 = lerp(grad(p[aa + 1], xf, yf, zf - 1), // the vectors from the edges of the unit cube to the point in the unit cube itself.
              grad(p[ba + 1], xf - 1, yf, zf - 1),
              u);
            x2 = lerp(grad(p[ab + 1], xf, yf - 1, zf - 1),
              grad(p[bb + 1], xf - 1, yf - 1, zf - 1),
              u);
            y2 = lerp(x1, x2, v);

            return (lerp(y1, y2, w) + 1) / 2;                       // For convenience we bound it to 0 - 1 (theoretical min/max before is -1 - 1)
        }
        public static double grad(int hash, double x, double y, double z) {
            int h = hash & 15;                                  // Take the hashed value and take the first 4 bits of it (15 == 0b1111)
            double u = h < 8 /* 0b1000 */ ? x : y;              // If the most signifigant bit (MSB) of the hash is 0 then set u = x.  Otherwise y.

            double v;                                           // In Ken Perlin's original implementation this was another conditional operator (?:).  I
                                                                // expanded it for readability.

            if (h < 4 /* 0b0100 */)                             // If the first and second signifigant bits are 0 set v = y
                v = y;
            else if (h == 12 || h == 14 /* 0b1110*/)// If the first and second signifigant bits are 1 set v = x
                v = x;
            else                                                // If the first and second signifigant bits are not equal (0/1, 1/0) set v = z
                v = z;

            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v); // Use the last 2 bits to decide if u and v are positive or negative.  Then return their addition.
        }
        public static double fade(double t) {
            // Fade function as defined by Ken Perlin.  This eases coordinate values
            // so that they will "ease" towards integral values.  This ends up smoothing
            // the final output.
            return t * t * t * (t * (t * 6 - 15) + 10);         // 6t^5 - 15t^4 + 10t^3
        }
        public static double lerp(double a, double b, double x) {
            return a + x * (b - a);
        }
    }

    #endregion






    // </Custom additional code> 
}