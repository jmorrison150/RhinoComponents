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
public class Script_Instance : GH_ScriptInstance
{
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
    private void RunScript(string directory, bool reset, ref object outPoints, ref object outStrings, ref object edgePoints, ref object edgeData, ref object outLines, ref object lineColors, ref object selectedPerson, ref object selectedCommunity)
    {






        #region beginScript

        //setup
        //double distance = 7.0;
        bool moving = false;
        directoryName = directory;
        //if (reset || people == null) { resetter(); }


        //calculateNeighbors(distance);
        DateTime t0 = new DateTime(1, 1, 1);
        DateTime t1 = new DateTime(9999, 12, 31);
        //RebuildConnections(t0, t1);
        //makeEdges();


        //dynamics
        if (moving)
        {
            applyForces();
        }

        //output
        //mousePosition();
        //displayCommunity();
        //foreach (Edge e in allEdges) {
        //Print(e.springConstant.ToString());
        //}

        //output(out outPoints, out outStrings, out outLines, out lineColors, out selectedPerson, out selectedCommunity);
        //getEdgeData(out edgePoints, out edgeData);

        #endregion



    }

    // <Custom additional code> 








    #region customCode


    //declare
    string[] directories;
    string directoryName;
    List<string> checkDuplicates = new List<string>();
    Person[] people;
    Person selectedPerson;
    List<Point3d> community;
    Connection[][] allConnections; //contains emailList
    List<Edge> allEdges;
    List<Line> lines;
    List<Color> lineColors;
    public Random rn = new Random();



    //functions
    void resetter()
    {
        checkDuplicates.Clear();
        getPath();
        initializePeople();
        initializeConnections();
        makeConnections();
    }
    void getPath()
    {
        //string path = System.IO.Directory.GetCurrentDirectory();
        //grasshopper GetCurrentDirectory
        GH_Document ghDoc = owner.OnPingDocument();
        string ghDocString = ghDoc.FilePath;
        System.IO.DirectoryInfo dir = System.IO.Directory.GetParent(ghDocString);
        string path = dir.FullName;

        //path = path + "\\enron_mail_20110402\\maildir";
        path = path + directoryName;
        directories = System.IO.Directory.GetDirectories(path);
        people = new Person[directories.Length];
        //emailsFrom = new string[directories.Length];

    }
    void initializePeople()
    {
        List<string> emailsFrom = new List<string>();
        for (int i = 0; i < directories.Length; ++i)
        {
            //makes a new Person if they don't exist
            //stored as people[i]
            searchForAddress(i);
        }
    }
    void searchForAddress(int index)
    {
        //person[i]
        string[] sentDirectories = new string[3];
        sentDirectories[0] = "sent";
        sentDirectories[1] = "_sent_mail";
        sentDirectories[2] = "sent_items";


        for (int m = 0; m < sentDirectories.Length; ++m)
        {
            //get sent directories
            string[] sent = System.IO.Directory.GetDirectories(directories[index], sentDirectories[m]);
            for (int j = 0; j < sent.Length; ++j)
            {
                //get files
                string[] currentFiles = System.IO.Directory.GetFiles(sent[j]);
                for (int k = 0; k < currentFiles.Length; ++k)
                {
                    //put all lines in memory
                    string[] lines = System.IO.File.ReadAllLines(currentFiles[k]);
                    //find from
                    for (int l = 0; l < lines.Length; ++l)
                    {
                        bool testFrom = lines[l].StartsWith("From: ", System.StringComparison.CurrentCultureIgnoreCase);
                        if (testFrom)
                        {
                            //format string
                            string emailFrom = lines[l];
                            emailFrom = emailFrom.Remove(0, 6);
                            emailFrom = emailFrom.Trim();


                            people[index] = new Person(emailFrom, index, rn);
                            checkDuplicates.Add(emailFrom);
                            return;
                        }
                    }
                }
            }
        }
    }//end search for address
    void initializeConnections()
    {
        allConnections = new Connection[directories.Length][];
        for (int i = 0; i < allConnections.Length; ++i)
        {
            allConnections[i] = new Connection[directories.Length];
        }

        for (int i = 0; i < directories.Length; ++i)
        {
            for (int j = 0; j < directories.Length; ++j)
            {
                allConnections[i][j] = new Connection(people[i], people[j]);
            }
        }

    }
    void makeConnections()
    {
        for (int i = 0; i < people.Length; ++i)
        {

            //get address of personTo as a string
            List<string> addressesTo = getAddressesTo(i);

            //put the people in a list
            List<Person> peopleTo = new List<Person>();
            for (int k = 0; k < addressesTo.Count; ++k)
            {
                for (int j = 0; j < people.Length; ++j)
                {
                    if (addressesTo[k] == people[j].emailAddress)
                    {
                        peopleTo.Add(people[j]);
                    }

                }
            }

            int counter = 0;
            //make Emails
            //Print(peopleTo.Count.ToString());
            for (int j = 0; j < peopleTo.Count; ++j)
            {
                //if(addressesTo.Contains(peopleTo[j].){}
                Person personFrom = people[i];
                Person personTo = peopleTo[j];
                if (personTo != personFrom)
                {
                    Email currentEmail = new Email(personFrom, personTo);



                    //Print(personTo.emailAddress);
                    //Print(personFrom.emailAddress);
                    allConnections[personFrom.index][personTo.index].emailList.Add(currentEmail);

                    personFrom.emails.Add(currentEmail);
                    personTo.emails.Add(currentEmail);
                    //Print(i.ToString());
                    counter++;


                }
            } //Print(counter.ToString());
        }

    }
    List<string> getAddressesTo(int index)
    {
        List<string> addressesTo = new List<string>();
        Person personFrom = people[index];
        //person[i]
        string[] sentDirectories = new string[3];
        sentDirectories[0] = "sent";
        sentDirectories[1] = "_sent_mail";
        sentDirectories[2] = "sent_items";


        for (int m = 0; m < sentDirectories.Length; ++m)
        {
            //get sent directories
            string[] sent = System.IO.Directory.GetDirectories(directories[index], sentDirectories[m]);
            for (int j = 0; j < sent.Length; ++j)
            {
                //get files
                string[] currentFiles = System.IO.Directory.GetFiles(sent[j]);
                for (int k = 0; k < currentFiles.Length; ++k)
                {
                    //put all lines in memory
                    string[] lines = System.IO.File.ReadAllLines(currentFiles[k]);
                    //find from
                    for (int l = 0; l < lines.Length; ++l)
                    {
                        bool testTo = lines[l].StartsWith("To: ", System.StringComparison.CurrentCultureIgnoreCase);
                        if (testTo)
                        {
                            int nextLine = 1;
                            lines[l] = lines[l].Trim();
                            while (lines[l].EndsWith(","))
                            {
                                lines[l] = lines[l] + lines[l + nextLine];
                                nextLine++;
                            }
                            //format strings
                            string[] tos = lines[l].Split(',');
                            for (int n = 0; n < tos.Length; ++n)
                            {
                                string emailTo = tos[n];

                                if (emailTo.StartsWith("To: "))
                                {
                                    emailTo = emailTo.Remove(0, 4);
                                }

                                if (emailTo.Contains('<'))
                                {
                                    int startInt = emailTo.IndexOf("<");
                                    emailTo = emailTo.Remove(0, startInt + 1);
                                    emailTo = emailTo.Replace('>', ' ');

                                }
                                //                emailTo.Replace('<', ' ');

                                emailTo = emailTo.Trim();


                                //checks
                                bool fromEnron = emailTo.Contains("@enron.com");
                                if (fromEnron)
                                {
                                    if (checkDuplicates.Contains(emailTo) && emailTo != personFrom.emailAddress)
                                    {
                                        addressesTo.Add(emailTo);

                                    }
                                    else { }
                                }

                            }
                        }
                    }
                }
            }
        }
        return addressesTo;
    }
    //int[] addressToIndex(List<string> addressesTo) {
    //    int[] indexTo = new int[addressesTo.Count];
    //    for (int i = 0; i < addressesTo.Count; ++i) {
    //        for (int j = 0; j < people.Length; ++j) {
    //            if (addressesTo[i] == people[j].emailAddress) {
    //                indexTo[i] = j;
    //            }
    //        }
    //    }
    //    return indexTo;
    //}
    //Person[] indexToPerson(int[] indexTo) {
    //    Person[] peopleList = new Person[indexTo.Length];
    //    for (int i = 0; i < people.Length; ++i) {
    //        try {
    //            peopleList[i] = people[indexTo[i]];
    //        } catch { }
    //    }

    //    return peopleList;
    //}
    void makeEdges()
    {



        //make edges
        allEdges = new List<Edge>();
        for (int i = 0; i < allConnections.Length; ++i)
        {
            for (int j = i; j < allConnections[i].Length; ++j)
            {
                if (i != j)
                {
                    //Print(allConnections[i][j].ConnectionStrength.ToString());
                    //Print(allConnections[j][i].ConnectionStrength.ToString());
                    if (allConnections[i][j].ConnectionStrength > 0 || allConnections[j][i].ConnectionStrength > 0)
                    {

                        Edge currentEdge = new Edge(allConnections[i][j], allConnections[j][i]);
                        try
                        {
                            allEdges.Add(currentEdge);
                            allConnections[i][j].p0.edges.Add(currentEdge);
                            allConnections[i][j].p1.edges.Add(currentEdge);
                        }
                        catch
                        {
                            Print("i=" + i.ToString() + " makeEdges error " + people[i].emailAddress);
                        }
                    }
                }
            }
        }

        //edge.totalConnectionStrength
        foreach (Edge edge in allEdges)
        {
            try
            {
                edge.totalConnectionStrength = edge.c0.ConnectionStrength + edge.c1.ConnectionStrength;
                edge.connectionAsymmetry = Math.Abs(edge.c0.ConnectionStrength - edge.c1.ConnectionStrength);
            }
            catch { Print("edge.totalConnectionStrength error"); }
        }

        ////normalize edge.springConstant
        //double maxSpringForce = allEdges.Max(edge => edge.springConstant);
        //foreach (Edge e in allEdges) {
        //    e.springConstant = e.springConstant / maxSpringForce;
        //}



    }
    void calculateNeighbors(double distance)
    {
        foreach (Person p in people)
        {
            p.neighbors.Clear();
            foreach (Person otherPerson in people)
            {
                if (p != otherPerson)
                {
                    if (p.point.DistanceTo(otherPerson.point) < distance)
                    {
                        p.neighbors.Add(otherPerson.point);
                    }
                }
            }
        }
    }
    void RebuildConnections(DateTime t0, DateTime t1)
    {

        foreach (Connection[] cc in allConnections)
        {
            foreach (Connection connect in cc)
            {
                connect.ConnectionStrength = 0;
                foreach (Email em in connect.emailList)
                {
                    if (em.dateTime < t0 || em.dateTime > t1) continue;
                    connect.ConnectionStrength++;
                }
            }
        }

    }
    void output(out object outPoints, out object outStrings, out object outLines, out object outLineColors, out object outSelectedPerson, out object outSelectedCommunity)
    {

        //points
        List<Point3d> updatePoints = drawPoints();
        outPoints = updatePoints;

        //strings
        List<string> updateStrings = outputData();
        outStrings = updateStrings;

        //lines
        drawLines();
        outLines = lines;
        outLineColors = lineColors;

        //selected
        outSelectedPerson = selectedPerson.point;
        outSelectedCommunity = community;

    }
    List<Point3d> drawPoints()
    {
        List<Point3d> updatePoints = new List<Point3d>();
        for (int i = 0; i < people.Length; ++i)
        {
            try
            {
                updatePoints.Add(people[i].point);
            }
            catch { Print("drawPoints"); }
        }
        return updatePoints;
    }
    List<string> outputData()
    {
        List<string> updateStrings = new List<string>();
        for (int i = 0; i < people.Length; ++i)
        {
            try
            {
                string address = people[i].emailAddress;

                int startInt = address.IndexOf("@");
                address = address.Remove(startInt);

                updateStrings.Add(address);
            }
            catch { Print("outputData"); }
        }
        return updateStrings;
    }
    void drawLines()
    {
        lines = new List<Line>();
        lineColors = new List<Color>();

        Print(allEdges.Count.ToString());
        double maxStrength = allEdges.Max(edge => edge.totalConnectionStrength);
        for (int i = 0; i < allEdges.Count; ++i)
        {

            if (allEdges[i].totalConnectionStrength == 0) { continue; }



            int red = (int)(allEdges[i].totalConnectionStrength / maxStrength * 255);
            int green = 0;
            int blue = (int)((maxStrength - allEdges[i].totalConnectionStrength) / maxStrength * 255);
            System.Drawing.Color currentColor = Color.FromArgb(red, green, blue);

            Point3d point0 = allEdges[i].c0.p0.point;
            Point3d point1 = allEdges[i].c0.p1.point;
            Line currentLine = new Line(point0, point1);


            lines.Add(currentLine);
            lineColors.Add(currentColor);





        }

    }
    void getEdgeData(out object edgePoints, out object edgeData)
    {
        List<Point3d> updateEdgePoints = new List<Point3d>();
        List<string> updateEdgeData = new List<string>();

        for (int i = 0; i < allEdges.Count; ++i)
        {

            Point3d midPoint = (allEdges[i].c0.p0.point + allEdges[i].c0.p1.point) / 2;
            updateEdgePoints.Add(midPoint);
            string data = allEdges[i].totalConnectionStrength.ToString();
            updateEdgeData.Add("emails = " + data);
        }


        edgePoints = updateEdgePoints;
        edgeData = updateEdgeData;

    }
    void applyForces()
    {
        foreach (Person p in people)
        {
            p.seperate();
        }
        foreach (Edge e in allEdges)
        {
            e.applySpringForce();
        }
        foreach (Person p in people)
        {
            p.move();
        }


    }


    void mousePosition()
    {

        // Obtain the view's world to screen transformation
        Rhino.Geometry.Transform world_To_Screen = RhinoDocument.Views.ActiveView.ActiveViewport.GetTransform(
          Rhino.DocObjects.CoordinateSystem.World,
          Rhino.DocObjects.CoordinateSystem.Screen);
        Point3dList pts = new Point3dList(people.Length);
        for (int i = 0; i < people.Length; ++i)
        {
            pts.Add(people[i].point);
        }

        //get all the points into screen space
        pts.Transform(world_To_Screen);

        //get mouse position
        System.Drawing.Point mouseXY = System.Windows.Forms.Cursor.Position;
        System.Drawing.Point screenOffset = RhinoDocument.Views.ActiveView.ScreenRectangle.Location;
        Point3d mouseXYZ = new Point3d(mouseXY.X - screenOffset.X, mouseXY.Y - screenOffset.Y, 0.0);
        Print(mouseXYZ.ToString());

        //find nearest person
        int closestIndex = pts.ClosestIndex(mouseXYZ);
        selectedPerson = people[closestIndex];
    }
    void displayCommunity()
    {
        community = new List<Point3d>();
        List<Point3d> communityPoints = new List<Point3d>();
        for (int i = 0; i < selectedPerson.edges.Count; ++i)
        {
            Point3d p0 = selectedPerson.edges[i].c0.p0.point;
            if (!communityPoints.Contains(p0)) { communityPoints.Add(p0); }
            Point3d p1 = selectedPerson.edges[i].c0.p1.point;
            if (!communityPoints.Contains(p1)) { communityPoints.Add(p1); }
        }
        community = communityPoints;

    }
    public override void DrawViewportMeshes(IGH_PreviewArgs args)
    {     //Draw all lines
        for (int i = 0; i < lines.Count; ++i)
        {
            args.Display.DrawLine(lines[i], lineColors[i]);
        }
        args.Display.DrawPoint(selectedPerson.point, Rhino.Display.PointStyle.ActivePoint, 2, Color.FromArgb(0, 255, 0, 0));
        args.Display.DrawPoints(community, Rhino.Display.PointStyle.ControlPoint, 8, Color.FromArgb(0, 0, 0, 255));
    }

    //classes
    public class Person
    {

        //constructor
        public Person(string _emailAddress, int _index, Random rn)
        {
            this.emailAddress = _emailAddress;
            this.index = _index;


            //Random rnd = new Random(DateTime.Now.Millisecond);
            double randomX = (rn.NextDouble() * 100);
            double randomY = (rn.NextDouble() * 100);
            double randomZ = (rn.NextDouble() * 0.0);
            this.point = new Point3d(randomX, randomY, randomZ);

            this.force = Vector3d.Zero;
            this.influence = 1.0;
            this.anchor = false;

            this.emails = new List<Email>();
            this.neighbors = new List<Point3d>();
            this.edges = new List<Edge>();
            this.groups = new List<int>();



        }

        //properties
        public string emailAddress;
        public int index;
        public Point3d point;
        public Vector3d force;
        public double influence;
        public bool anchor;
        public Point3d anchorPoint;
        public Point3d centroid;
        public List<Point3d> neighbors;
        public List<Email> emails;
        public List<Edge> edges;
        public List<int> groups;

        //methods
        public void calculateCentroid()
        {
            this.centroid = new Point3d(
              neighbors.Average(point => point.X),
              neighbors.Average(point => point.Y),
              neighbors.Average(point => point.Z));
        }
        public void seperate()
        {
            if (this.neighbors.Count < 1)
            {
                this.force = Vector3d.Zero;
            }
            else
            {
                Vector3d direction = this.centroid - this.point;
                this.force -= direction * 0.01;
            }
        }
        public void move()
        {
            this.point = this.point + (this.force * 0.010);
        }

    }
    public class Email
    {

        //constructor
        public Email(Person _person0, Person _person1)
        {
            this.p0 = _person0;
            this.p1 = _person1;
            this.dateTime = new DateTime(2003, 01, 01);
        }

        //properties
        public Person p0;
        public Person p1;
        public Connection c;
        public DateTime dateTime;

        ////methods


    }
    public class Connection
    {
        //Constructor
        public Connection(Person _person0, Person _person1)
        {
            this.p0 = _person0;
            this.p1 = _person1;
            this.emailList = new List<Email>();
        }

        //Properties
        public Person p0;
        public Person p1;
        public List<Email> emailList;
        public Edge edge;
        public double ConnectionStrength;
    }
    public class Edge
    {
        public Edge(Connection _c0, Connection _c1)
        {
            this.c0 = _c0;
            this.c1 = _c1;
            this.springConstant = Math.Log10(this.c0.ConnectionStrength + this.c1.ConnectionStrength);
            this.springLength = 20.0;
        }
        public Connection c0;
        public Connection c1;

        public double totalConnectionStrength;
        public double connectionAsymmetry;
        public double springLength;
        public double springConstant;



        //public Connection getConnection(int whichOne) {
        //    if (whichOne == 0) {
        //        return this.c0;
        //    } else {
        //        return this.c1;
        //    }
        //}


        //methods
        public void applySpringForce()
        {
            Vector3d dv = this.c0.p1.point - this.c0.p0.point;
            double length = dv.Length;
            dv.Unitize();
            this.c0.p0.force = dv * (this.springConstant * (length - this.springLength) * 0.5);
            this.c0.p1.force = -dv * (this.springConstant * (length - this.springLength) * 0.5);
        }
    }





    #endregion



    // </Custom additional code> 
}
