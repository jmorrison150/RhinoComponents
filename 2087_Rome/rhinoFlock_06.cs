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
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance {
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
    private void RunScript(Brep boundary, List<Curve> paths, List<double> variables, bool reset, ref object outPositions, ref object outTraces, ref object outMesh) {




        #region beginscript



        double initialVelocity = 3.400;

        if(reset == true) {
            agentCollection.Clear();
            positions.Clear();






            //make new points
            Random rnd = new Random();

            for(int i = 0; i < paths.Count; i++) {
                for(int j = 0; j < 250; j++) {

                    Point3d pt;
                    double t = rnd.NextDouble();
                    pt = paths[i].PointAtNormalizedLength(t);
                    pt = new Point3d(pt.X + ( rnd.NextDouble() * 2.0 ) - 1.000, pt.Y + rnd.NextDouble() * 2.0 - 1.000, 0);

                    double velX = rnd.NextDouble() * initialVelocity; //0.0 to 1.0
                    double velY = rnd.NextDouble() * initialVelocity;
                    double velZ = 0;

                    Agent agent = new Agent(pt, new Vector3d(velX, velY, velZ), variables);
                    agent.controlSurface = boundary;
                    agent.maxSpeed = rnd.NextDouble() * 0.450;
                    if(j % 3 == 0) {
                        agent.leader = true;
                        agent.shortPath = paths[i];
                    }
                    agentCollection.Add(agent);
                }
            }


        } else {

            positions.Clear();
            for(int i = 0; i < agentCollection.Count; i++) {
                Agent currentAgent = agentCollection[i];
                currentAgent.run(agentCollection);
                checkDestination(currentAgent, paths);
                positions.Add(currentAgent.location);

            }

            outPositions = positions;
        }
        #endregion




    }

    // <Custom additional code> 




    #region customCode
    List<Agent> agentCollection = new List<Agent>();
    List<Point3d> positions = new List<Point3d>();


    Mesh updateMeshCounter(Mesh meshCounter, Point3d location) {
        MeshPoint meshPt = meshCounter.ClosestMeshPoint(location, 0.0);
        MeshFace face = meshCounter.Faces[meshPt.FaceIndex];
        //Color colorBefore = meshCounter.ColorAt(meshPt);
        int colorA = meshCounter.VertexColors[face.A].ToArgb();
        int colorB = meshCounter.VertexColors[face.B].ToArgb();
        int colorC = meshCounter.VertexColors[face.C].ToArgb();
        colorA++;
        colorB++;
        colorC++;
        meshCounter.VertexColors[face.A] = Color.FromArgb(colorA);
        meshCounter.VertexColors[face.B] = Color.FromArgb(colorB);
        meshCounter.VertexColors[face.C] = Color.FromArgb(colorC);
        if(face.IsQuad) {
            int colorD = meshCounter.VertexColors[face.D].ToArgb();
            colorD++;
            meshCounter.VertexColors[face.D] = Color.FromArgb(colorD);
        }
        //Color colorAfter = meshCounter.ColorAt(meshPt);
        return meshCounter;
    }
    void checkDestination(Agent agent, List<Curve> shortPaths) {
        for(int i = 0; i < shortPaths.Count; i++) {

            double dist = agent.location.DistanceTo(shortPaths[i].PointAtEnd);

            if(dist < 2.000) {
                Random rnd = new Random();
                int index = (int) Math.Floor(rnd.NextDouble() * ( shortPaths.Count - 1 ));
                if(index < 0) { index = 0; }
                Point3d pt = shortPaths[index].PointAtStart;
                pt = new Point3d(pt.X + rnd.NextDouble() * 2.000, pt.Y + rnd.NextDouble() * 1.000, 0.0);
                agent.location = pt;
                break;
            }
        }
    }
    class Agent {

        //CONSTRUCTOR
        public Agent(Point3d _location, Vector3d _velocity, List<double> variables) {
            this.location = _location;
            this.velocity = _velocity;

            if(variables.Count >= 6) {
                this.maxForce = variables[0];
                this.maxSpeed = variables[1];
                this.sepDist = variables[2];
                this.cohDist = variables[3];
                this.aliDist = variables[4];
                this.boundaryDist = variables[5];
            }

        }

        //PROPERTIES
        public Point3d location;
        public Vector3d velocity;
        public Vector3d acceleration;


        public double maxForce = 0.537;
        public double maxSpeed = 0.395;
        public double sepDist = 0.989;
        public double cohDist = 0.529;
        public double aliDist = 2.780;
        public double boundaryDist = 1.2;
        public Brep controlSurface;
        public Curve shortPath;
        public bool leader = false;

        //METHODS
        public void run(List<Agent> agentCollection) {
            //acceleration is the thing that changes

            this.acceleration += seprate(agentCollection); //Vector3d
            this.acceleration += align(agentCollection);  //Vector3d
            this.acceleration += cohesion(agentCollection); //Vector3d

            checkBorder();
            update();
        }
        public Vector3d seprate(List<Agent> agentCollection) {

            double desiredseparation = sepDist;
            Vector3d steer = new Vector3d(0, 0, 0);
            int count = 0;
            for(int i = 0; i < agentCollection.Count; i++) {

                Agent other = agentCollection[i];
                double d = this.location.DistanceTo(other.location);


                if(( d > 0 ) && ( d < desiredseparation )) {

                    Vector3d diff = this.location - other.location;
                    diff.Unitize();
                    Vector3d direct = diff / d;
                    steer += direct;
                    count++;
                }
            }
            if(count > 0) steer = steer / (double) count;
            if(steer.Length > 0) {

                steer.Unitize();
                steer *= maxSpeed;
                steer = steer - velocity;
                if(steer.Length > maxForce) {
                    steer.Unitize();
                    steer *= maxForce;
                }
            }
            return steer;
        }
        public Vector3d align(List<Agent> agentCollection) {

            double neighbordist = aliDist;
            Vector3d steer = new Vector3d(0, 0, 0);
            int count = 0;
            for(int i = 0; i < agentCollection.Count; i++) {

                Agent other = agentCollection[i];
                double d = this.location.DistanceTo(other.location);
                if(( d > 0 ) && ( d < neighbordist )) {

                    steer = steer + other.velocity;
                    count++;
                }
            }
            if(count > 0) steer = steer / (double) count;
            if(steer.Length > 0) {

                steer.Unitize();
                steer *= maxSpeed;
                steer = Vector3d.Subtract(steer, velocity);
                if(steer.Length > maxForce) {

                    steer.Unitize();
                    steer *= maxForce;
                }
            }
            return steer;
        }
        public Vector3d cohesion(List<Agent> agentCollection) {

            double neighbordist = cohDist;
            Vector3d sum = new Vector3d(0, 0, 0);
            int count = 0;
            for(int i = 0; i < agentCollection.Count; i++) {

                Agent other = agentCollection[i];
                double d = this.location.DistanceTo(other.location);
                if(( d > 0 ) && ( d < neighbordist )) {

                    sum = sum + (Vector3d) other.location;
                    count++;
                }
            }
            if(count > 0) {

                sum = sum / (double) count;
                return steer(sum, false);
            }
            return sum;
        }
        public Vector3d steer(Vector3d target, bool slowdown) {

            Vector3d steer;
            Vector3d desired = target - (Vector3d) this.location;
            double d = desired.Length;

            if(d > 0) {

                desired.Unitize();
                if(( slowdown ) && ( d < 100 )) {
                    desired *= maxSpeed * ( d / 100 );

                } else {
                    desired *= maxSpeed;
                }
                steer = desired - velocity;
                if(steer.Length > maxForce) {
                    steer.Unitize();
                    steer *= maxForce;
                }
            } else {
                steer = new Vector3d(0, 0, 0);
            }
            return steer;
        }
        public void checkBorder() {

            Brep b = this.controlSurface;
            Point3d pt = b.ClosestPoint(this.location);

            Vector3d avoidWall = ( pt - this.location ) * this.boundaryDist;
            this.acceleration += avoidWall;
            this.location = pt;


        }
        public void update() {

            this.velocity += this.acceleration;
            if(this.velocity.Length > this.maxSpeed) {

                this.velocity.Unitize();
                this.velocity *= maxSpeed;
            }

            if(this.leader) {
                double t;
                Vector3d vec;
                this.shortPath.ClosestPoint(this.location, out t);
                Point3d pt = this.shortPath.PointAt(t);
                Point3d pt1 = this.shortPath.PointAt(t + this.maxSpeed);
                vec = pt1 - pt;
                //this.location = pt;
                this.velocity = vec;
            }
            this.location = this.location + this.velocity;
            this.acceleration *= 0.0;

        }

    }



    //The base methods, separate, align, cohesion, and seek, are
    //from Daniel Shiffman
    #endregion



    // </Custom additional code> 
}