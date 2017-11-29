using RMA.Rhino;
using RMA.OpenNURBS;
using RMA.Rhino.RhUtil;

using System;
using Rhino;

public class CirclePackingCommand : Rhino.Commands.Command
    
{

    public override System.Guid CommandUUID()
    {
        return new Guid("{8b364132-298c-4d6d-b6fb-9301c725c635}");
    }

    public override string EnglishCommandName()
    {
        return "CirclePacking";
    }

    protected static Int32 intCount = 100;

    protected static Int32 maxIterations = 10000;

    protected static double minRadius = 0.1;

    protected static double maxRadius = 1;

    protected static PackCircles.PackingAlgorithm bPackAlgorithm = PackCircles.PackingAlgorithm.FastPack;

    public override RMA.Rhino.IRhinoCommand.result RunCommand(RMA.Rhino.IRhinoCommandContext context)
    {
        On3dPoint ptBase;
        for (
        ; // TODO: Warning!!!! NULL EXPRESSION DETECTED...
        ;
        )
        {
            MRhinoGetPoint nGetOptions = new MRhinoGetPoint();
            nGetOptions.SetCommandPrompt("Center of fitting solution");
            nGetOptions.AddCommandOption(new MRhinoCommandOptionName("Count"), intCount);
            nGetOptions.AddCommandOption(new MRhinoCommandOptionName("MinRadius"), minRadius);
            nGetOptions.AddCommandOption(new MRhinoCommandOptionName("MaxRadius"), maxRadius);
            nGetOptions.AddCommandOption(new MRhinoCommandOptionName("IterationLimit"), maxIterations);
            switch (bPackAlgorithm)
            {
                case PackCircles.PackingAlgorithm.FastPack:
                    nGetOptions.AddCommandOption(new MRhinoCommandOptionName("Packing"), new MRhinoCommandOptionValue("Fast"));
                    break;
                case PackCircles.PackingAlgorithm.DoublePack:
                    nGetOptions.AddCommandOption(new MRhinoCommandOptionName("Packing"), new MRhinoCommandOptionValue("Double"));
                    break;
                case PackCircles.PackingAlgorithm.RandomPack:
                    nGetOptions.AddCommandOption(new MRhinoCommandOptionName("Packing"), new MRhinoCommandOptionValue("Random"));
                    break;
                case PackCircles.PackingAlgorithm.SimplePack:
                    nGetOptions.AddCommandOption(new MRhinoCommandOptionName("Packing"), new MRhinoCommandOptionValue("Simple"));
                    break;
            }
            nGetOptions.AcceptNumber(true);
            switch (nGetOptions.GetPoint)
            {
                case IRhinoGet.result.point:
                    ptBase = new On3dPoint(nGetOptions.Point);
                    break; //Warning!!! Review that break works as 'Exit Do' as it could be in a nested instruction like switch
                    break;
                case IRhinoGet.result.number:
                    Int32 newCount = Convert.ToInt32(nGetOptions.Number);
                    if ((newCount < 10))
                    {
                        RhUtil.RhinoApp.Print(("A minimum of 2 circles if required" + "\r\n"));
                    }
                    else
                    {
                        intCount = newCount;
                    }

                    break;
                case IRhinoGet.result.option:
                    switch (nGetOptions.Option.m_english_option_name)
                    {
                        case "Count":
                            RhUtil.RhinoGetInteger("Number of circles?", true, intCount, 2);
                            break;
                        case "MinRadius":
                            RhUtil.RhinoGetNumber("Minimum circle radius", true, false, minRadius, 0.001);
                            break;
                        case "MaxRadius":
                            RhUtil.RhinoGetNumber("Maximum circle radius", true, false, maxRadius, 0.001);
                            break;
                        case "Packing":
                            MRhinoGetOption nGetPack = new MRhinoGetOption();
                            nGetPack.SetCommandPrompt("Packing algorithm");
                            switch (bPackAlgorithm)
                            {
                                case PackCircles.PackingAlgorithm.FastPack:
                                    nGetPack.SetDefaultString("Fast");
                                    break;
                                case PackCircles.PackingAlgorithm.DoublePack:
                                    nGetPack.SetDefaultString("Double");
                                    break;
                                case PackCircles.PackingAlgorithm.RandomPack:
                                    nGetPack.SetDefaultString("Random");
                                    break;
                                case PackCircles.PackingAlgorithm.SimplePack:
                                    nGetPack.SetDefaultString("Simple");
                                    break;
                            }
                            nGetPack.AddCommandOption(new MRhinoCommandOptionName("Fast"));
                            nGetPack.AddCommandOption(new MRhinoCommandOptionName("Double"));
                            nGetPack.AddCommandOption(new MRhinoCommandOptionName("Random"));
                            nGetPack.AddCommandOption(new MRhinoCommandOptionName("Simple"));
                            nGetPack.AddCommandOption(new MRhinoCommandOptionName("Help"));
                            AskTheQuestion:
                            switch (nGetPack.GetOption)
                            {
                                case IRhinoGet.result.option:
                                    switch (nGetPack.Option.m_english_option_name)
                                    {
                                        case "Fast":
                                            bPackAlgorithm = PackCircles.PackingAlgorithm.FastPack;
                                            break;
                                        case "Double":
                                            bPackAlgorithm = PackCircles.PackingAlgorithm.DoublePack;
                                            break;
                                        case "Random":
                                            bPackAlgorithm = PackCircles.PackingAlgorithm.RandomPack;
                                            break;
                                        case "Simple":
                                            bPackAlgorithm = PackCircles.PackingAlgorithm.SimplePack;
                                            break;
                                        case "Help":
                                            string sHelp = "";
                                            ("Fast: fast packing prevents collisions by moving" + ("\r\n" + ("one circle away from all its intersectors." + ("\r\n" + ("After every collision iteration, all circles" + ("\r\n" + ("are moved towards the centre of the packing" + ("\r\n" + ("to reduce the amount of wasted space. Collision" + ("\r\n" + ("detection proceeds from the center outwards." + ("\r\n" + "\r\n"))))))))))));
                                            ("Double: similar to FastPacking, except that both" + ("\r\n" + ("circles are moved in case of a collision." + ("\r\n" + "\r\n"))));
                                            ("Random: similar to FastPacking, except that" + ("\r\n" + ("collision detection is randomized rather than" + ("\r\n" + ("sorted." + ("\r\n" + "\r\n"))))));
                                            ("Simple: similar to FastPacking, but without a" + ("\r\n" + "contraction pass after every collision iteration."));
                                            MsgBox(sHelp, (MsgBoxStyle.OkOnly | MsgBoxStyle.Information), "Packing algorithm description");
                                            goto AskTheQuestion;
                                            break;
                                    }
                                    break;
                            }
                            switch ("IterationLimit")
                            {
                                case RhUtil.RhinoGetInteger("Maximum number of allowed iterations", true, maxIterations, 100):
                                    break;
                            }
                            break;
                        default:
                            return IRhinoCommand.result.cancel;
                            break;
                    }
                    PackCircles allCircles = new PackCircles(ptBase, intCount, minRadius, maxRadius);
                    PackConduit fitConduit = new PackConduit(allCircles);
                    double sDamping = 0.1;
                    for (Int32 i = 1; (i <= maxIterations); i++)
                    {
                        RMA.Rhino.RhUtil.RhinoApp.SetCommandPrompt(string.Format("Performing circle packing iteration {0}...  (Press Shift+Ctrl to abort)", i));
                        Windows.Forms.Keys iKeys = Windows.Forms.Control.ModifierKeys;
                        if ((iKeys
                                    == (Windows.Forms.Keys.Control || Windows.Forms.Keys.Shift)))
                        {
                            RhUtil.RhinoApp.Print((string.Format("Circle fitting process aborted at iteration {0}...", i) + "\r\n"));
                            break;
                        }

                        if (!allCircles.Pack(bPackAlgorithm, sDamping))
                        {
                            RhUtil.RhinoApp.Print((string.Format("Circle fitting process completed at iteration {0}...", i) + "\r\n"));
                            break;
                        }

                        0.98;
                        RhUtil.RhinoApp.ActiveDoc.Regen();
                        RMA.Rhino.RhUtil.RhinoApp.Wait(1);
                    }

                    fitConduit.Disable();
                    fitConduit.Dispose();
                    fitConduit = null;
                    allCircles.Add();
                    RhUtil.RhinoApp.ActiveDoc.Regen();
                    return IRhinoCommand.result.success;
                    break;
            }
        }

    }
}