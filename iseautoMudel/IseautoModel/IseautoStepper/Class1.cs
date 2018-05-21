using System;
using RosSharp.RosBridgeClient;
using NModel.Conformance;
using NModel.Terms;
using System.Threading;
using System.Numerics;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace IseautoStepper
{
    // public delegate void ResponseEventDelegate(string cmd, int id,bool status);
    // public delegate void ObserverDelegate(NModel.Terms.Action action);
    public class Stepper : IStepper
    {


        //https://github.com/CPFL/Autoware/blob/fad2d17234e77e393e263aac861b7a835f10903a/ros/src/computing/planning/mission/packages/lane_planner/README.md
        public static float xposition { get; set; }
        public static bool isCollision { get; set; }
        public static bool isWaypoinPassed { get; set; } = false;
        public static float yposition { get; set; }
        public delegate bool CarArrivedDelegate(bool x);
        public static lane nextLane { get; set; }
        //Kohale jõudmise konstant
        public static int konstant = 4;
        //Raadiuse määramine
        public static double radius = 3.2;
        public static  CancellationTokenSource tokenSource2 = new CancellationTokenSource();
        public static CancellationToken ct = tokenSource2.Token;
        public static waypoint currentPosition { get; set; }
        public static waypoint nextpublish_waypoint { get; set; }
        public static waypoint nextToReach { get; set; }
        //Andmetüüp järgmise teekonnapunkti jaoks
        public static waypoint nextWaypoint { get; set; }
        //Andmetüüp läbitud teekonnapunkti jaoks
        public static waypoint passedWaypoint { get; set; }
        public static List<waypoint> passedWpList = new List<waypoint>();
        static RosSocket rosSocket = new RosSocket("ws://127.0.0.1:9090");
        //
        static int publish_waypoint = rosSocket.Advertize("/final_waypoints", "autoware_msgs/lane");
        static int publish_laneArray = rosSocket.Advertize("/lane_waypoints_array", "autoware_msgs/LaneArray");
        static int subscribe_pose = rosSocket.Subscribe("/current_pose", "geometry_msgs/PoseStamped", currentposeCallback);
        //Hetkel välja kommenteeritud kuna testitav maailma ja autoware'i kaardid olid nihkes
        //static int subscribe_gazebo = rosSocket.Subscribe("/gazebo/link_states", "gazebo_msgs/LinkStates", gazeboLinkStatesCallback);
 
        static int arrivalTime = 0;
        public static bool isTimeNegative = false;
        public static lane laneToPub { get; set; }
        public static lane laneToPubNext { get; set; }
        public static List<waypoint> wpToReachList = new List<waypoint>();
        public static List<lane> lanelist = new List<lane>();
        public static int seqCounter = 0;
        public static  CarArrivedDelegate d = new CarArrivedDelegate(Result);
        public static CarArrivedDelegate someEvent=new CarArrivedDelegate((Result)=>wait.Set());
        public static ManualResetEvent wait =  new ManualResetEvent(false);
        public static float kmph2mps(float velocity_kmph)
        {

            return (velocity_kmph * 1000) / (60 * 60);
        }
        static bool Result(bool x)
        {
            wait.Set();
            return x;
        }
        public double mps2kmph(double velocity_mps)
        {

            return (velocity_mps * 60 * 60) / 1000;
        }
        //Arvutab kahe teekonnapunkti vahelise teekonna läbimiseks kuluva aja
        public static double calculateTravelingTimeToNextWaypoint(waypoint currentWp, waypoint nextWp, float speed)
        {
            double metpersec = 0.27777777777778;
            double power = 2.0;
            double cartX = Convert.ToDouble(nextWp.pose.pose.position.x - currentWp.pose.pose.position.x);
            double cartY = Convert.ToDouble(nextWp.pose.pose.position.y - currentWp.pose.pose.position.y);
            double cartZ = Convert.ToDouble(nextWp.pose.pose.position.z - currentWp.pose.pose.position.z);
            double xyzRes = Math.Pow(cartX, power) + Math.Pow(cartY, power) + Math.Pow(cartZ, power);
            double distanceInMeters = Math.Sqrt(xyzRes);

            double timeInSeconds = distanceInMeters / (metpersec * speed);

            double millieSeconds = timeInSeconds * 1000;

            return millieSeconds;
        }

        //Testi setup
        public Stepper()
        {
            currentPosition = new waypoint();
            passedWaypoint = new waypoint();
            nextWaypoint = new waypoint();
            nextToReach = new waypoint();

        }
        public static lane[] laneMsgDataSplit(float[] xs, float[] ys, float[] zs, float[] vels, float[] yaws, int seqCounter, int countOfWp)
        {

            lane lane1 = new lane();
            lane lane2 = new lane();
            waypoint[] wps1 = new waypoint[countOfWp / 2];
            waypoint[] wps2 = new waypoint[countOfWp / 2];
            lane1.header.frame_id = "/map";
            lane2.header.frame_id = "/map";
            for (int j = 0; j < countOfWp; j++)
            {


                waypoint wp = new waypoint();

                wp.pose.pose.position.x = xs[j];
                wp.pose.pose.position.y = ys[j];
                wp.pose.pose.position.z = zs[j];
                if (isTimeNegative)
                {
                    wp.twist.twist.linear.x = 0;
                }
                {
                    wp.twist.twist.linear.x =  kmph2mps(vels[j]);
                }

                wp.twist.twist.linear.y = 0f;
                wp.twist.twist.linear.z = 0f;
                Quaternion quaternionYaw = new Quaternion();
                GeometryQuaternion rot = new GeometryQuaternion();
                quaternionYaw = Quaternion.CreateFromYawPitchRoll(yaws[j], 0, 0);
                rot.w = quaternionYaw.W;
                rot.x = quaternionYaw.X;
                rot.y = quaternionYaw.Y;
                rot.z = quaternionYaw.Z;
                wp.pose.pose.orientation = rot;
                if (j < (countOfWp / 2))
                {
                    wps1[j] = wp;
                }
                else
                {
                    wps2[j - (countOfWp / 2)] = wp;
                }

            }

            lane1.header.seq = seqCounter;
            seqCounter = seqCounter + 1;
            lane2.header.seq = seqCounter;
            Console.WriteLine("Seq {0}", lane2.header.seq);


            lane1.waypoints = wps1;
            lane2.waypoints = wps2;
            lane[] laneArra = new lane[2];
            laneArra[0] = lane1;
            laneArra[1] = lane2;
            return laneArra;
        }

        //Teisendame .csv failist saadud teekonnapunktid lane andmetüübile sobilikule kujule
        public static lane laneMsgData(float[] xs, float[] ys, float[] zs, float[] vels, float[] yaws, int seqCounter, int countOfWp)
        {

            lane lane = new lane();
            waypoint[] wps = new waypoint[countOfWp];
            lane.header.frame_id = "/map";
            for (int j = 0; j < countOfWp; j++)
            {


                waypoint wp = new waypoint();

                wp.pose.pose.position.x = xs[j];
                wp.pose.pose.position.y = ys[j];
                wp.pose.pose.position.z = zs[j];


                wp.twist.twist.linear.x = kmph2mps(  vels[j]);

                wp.twist.twist.linear.y = 0f;
                wp.twist.twist.linear.z = 0f;
                Quaternion quaternionYaw = new Quaternion();
                GeometryQuaternion rot = new GeometryQuaternion();
                quaternionYaw = Quaternion.CreateFromYawPitchRoll(yaws[j], 0, 0);
                rot.w = quaternionYaw.W;
                rot.x = quaternionYaw.X;
                rot.y = quaternionYaw.Y;
                rot.z = quaternionYaw.Z;
                wp.pose.pose.orientation = rot;
                wps[j] = wp;
            }

            lane.header.seq = seqCounter;
            Console.WriteLine("Seq {0}", lane.header.seq);


            lane.waypoints = wps;

            return lane;
        }

        //Arvutame teekonna läbimiseks kuluva aja hetke positsiooni ja viimase antud teekonnapunkti vahel
        public static int calculateTimeTravelingBetweenWaypoints(lane laneIn, waypoint currenWaypoint)
        {
            double timeToTravel = 0;
            for (int i = 0; i < laneIn.waypoints.Length - 1; i++)
            {
                if (i == 0)
                {

                    timeToTravel = timeToTravel + calculateTravelingTimeToNextWaypoint(currenWaypoint, laneIn.waypoints[i], laneIn.waypoints[i].twist.twist.linear.x);

                }
                else
                {


                    timeToTravel = timeToTravel + calculateTravelingTimeToNextWaypoint(laneIn.waypoints[i], laneIn.waypoints[i + 1], laneIn.waypoints[i].twist.twist.linear.x);
                }

            }

            return (int)timeToTravel;
        }

        //Visualiseerib RViz'is publitseeritud teekonda
        public static void visualizeLane(lane laneViz)
        {
            LaneArray wpLaneArray2 = new LaneArray();

            lane[] laneArraViz2 = new lane[1];
            laneArraViz2[0] = laneViz;
            wpLaneArray2.lane = laneArraViz2;
            rosSocket.Publish(publish_laneArray, wpLaneArray2);
        }
        //Toimmingud
        public CompoundTerm DoAction(CompoundTerm action)
        {



            switch (action.Name)
            {
                case ("Tests"): return null;
                case ("StartPosition"):
                    Console.WriteLine("Väärtused ");
                    //Algpunkti publitseerimine
                    float[] xs1 = Array.ConvertAll(((string)action[0]).Split(','), float.Parse);
                    float[] ys1 = Array.ConvertAll(((string)action[1]).Split(','), float.Parse);
                    float[] zs1 = Array.ConvertAll(((string)action[2]).Split(','), float.Parse);
                    float[] vels1 = Array.ConvertAll(((string)action[3]).Split(','), float.Parse);
                    float[] yaws1 = Array.ConvertAll(((string)action[4]).Split(','), float.Parse);

                    lane laneToPub4 = laneMsgData(xs1, ys1, zs1, vels1, yaws1, seqCounter, (int)action[5]);
                    Console.WriteLine("Start");
                    seqCounter = seqCounter + 1;
                    rosSocket.Publish(publish_waypoint, laneToPub4);
                    
                    return null;

                case ("MoveCar"):
                    //Auto liigutamine määratud teekonnapunktide abil
                    float[] xs = Array.ConvertAll(((string)action[0]).Split(','), float.Parse);
                    float[] ys = Array.ConvertAll(((string)action[1]).Split(','), float.Parse);
                    float[] zs = Array.ConvertAll(((string)action[2]).Split(','), float.Parse);
                    float[] vels = Array.ConvertAll(((string)action[3]).Split(','), float.Parse);
                    float[] yaws = Array.ConvertAll(((string)action[4]).Split(','), float.Parse);
                    
                    lane lane = laneMsgData(xs, ys, zs, vels, yaws, seqCounter, (int)action[5]);
                    visualizeLane(lane);
                    int countOfWps = (int)action[5] - 1;



                    arrivalTime = calculateTimeTravelingBetweenWaypoints(lane, currentPosition);
                    nextWaypoint = lane.waypoints[countOfWps];
     
                     wpToReachList.Add(nextWaypoint);
                 
                     Console.WriteLine("Publish ");

                     rosSocket.Publish(publish_waypoint, lane);
           
                 

                    return null;

                case ("didReacheWaypoint_Start"):
                    //Teekonnapunkti jõudmise kontrollimine
                    if (arrivalTime <=0)
                    {
                        arrivalTime = 10000;
                    }
                    else
                    {
                        arrivalTime=arrivalTime* konstant;
                    }
                        Console.WriteLine("Wait for event");
                        Console.WriteLine("Arrival time {0} {1}", arrivalTime, nextWaypoint.pose.pose.position.x);
                        Console.WriteLine("Velocity {0} ", nextWaypoint.twist.twist.linear.x);
                        someEvent += new CarArrivedDelegate(Result);
                        bool result=wait.WaitOne(arrivalTime*konstant);
                        wait.Reset();
                        Console.WriteLine("Finished");

                    if (result)
                    {
                        return NModel.Terms.Action.Create("didReacheWaypoint_Finish");

                    }
                    else
                    {
                        return NModel.Terms.Action.Create("didNotReachWaypoint", nextWaypoint.pose.pose.position.x, nextWaypoint.pose.pose.position.y, nextWaypoint.pose.pose.position.z);



                    }

                case ("Goal"):
                    //Kogu teekond on läbitud
                    return null;

                default: throw new Exception("Unexpected action " + action);

            }
        }


        //Testi restartimine
        void IStepper.Reset()
        {
            Console.WriteLine("Reset test");
            //ExecuteCommand("cd ~/lauri-autoware/abi");
            lanelist.Clear();
            nextToReach = null;
            wpToReachList.Clear();
            ExecuteCommand("cd ~/lauri-autoware/abi; ./reset.sh");
        }
        //Protsessi avamine ja käivitamine terminalis
        public static void ExecuteCommand(string command)
        {
            Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "/bin/bash";
            proc.StartInfo.Arguments = "-c \" " + command + " \"";
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WorkingDirectory = "/home";
            proc.Start();

            while (!proc.StandardOutput.EndOfStream)
            {
                Console.WriteLine(proc.StandardOutput.ReadLine());
            }
        }
        public static IStepper Create()
        {

            return new Stepper();
        }
        //Kuulab infot auto hetke positsiooni kohta
        private static void currentposeCallback(Message message)
        {
            GeometryPoseStamped geo = (GeometryPoseStamped)message;
            currentPosition.pose.pose.position = geo.pose.position;
            currentPosition.pose.pose.orientation = geo.pose.orientation;
            if ((Math.Pow(currentPosition.pose.pose.position.x - nextWaypoint.pose.pose.position.x, 2) + Math.Pow(currentPosition.pose.pose.position.y - nextWaypoint.pose.pose.position.y, 2)) < (Math.Pow(radius, 2)))
            {

                if (wpToReachList.Count > 0)
                {
                    wpToReachList.RemoveAt(0);
                }

                passedWaypoint = nextWaypoint;

                //  Console.WriteLine("In radius, passed waypoint {0} ", passedWaypoint.pose.pose.position.x);
                Result(true);
                isWaypoinPassed = true;
        
           

            }
            else
            {
                isWaypoinPassed = false;
            
            }
        }

            private static void gazeboLinkStatesCallback(Message message)
        {

            LinkState geo = (LinkState)message;

            for (int i = 0; i < geo.name.Length; i++)
            {
                if (geo.name[i] == "catvehicle") 
                {

                    currentPosition.pose.pose.position = geo.pose[i].position;
                    currentPosition.pose.pose.orientation = geo.pose[i].orientation;
                    currentPosition.twist.twist.linear = geo.twist[i].linear;
                    currentPosition.twist.twist.angular = geo.twist[i].linear;
                   
                    double isInside = Math.Sqrt(Math.Pow(nextWaypoint.pose.pose.position.x-currentPosition.pose.pose.position.x+3.0, 2) + Math.Pow(nextWaypoint.pose.pose.position.y-currentPosition.pose.pose.position.y, 2) + Math.Pow(nextWaypoint.pose.pose.position.z-currentPosition.pose.pose.position.z, 2));


                    Console.WriteLine("Out of radius, not in next waypoint {0}, {1}, {2},{3}, {4}, {5}, {6}", currentPosition.pose.pose.position.x, currentPosition.pose.pose.position.y, currentPosition.pose.pose.position.z,currentPosition.pose.pose.orientation.x,currentPosition.pose.pose.orientation.y,currentPosition.pose.pose.orientation.z,currentPosition.pose.pose.orientation.w);



                    if ((Math.Pow(currentPosition.pose.pose.position.x-nextWaypoint.pose.pose.position.x, 2) + Math.Pow(currentPosition.pose.pose.position.y - nextWaypoint.pose.pose.position.y, 2)) < (Math.Pow(radius, 2)))
                    {

                        if (wpToReachList.Count > 0)
                        {
                            wpToReachList.RemoveAt(0);
                        }

                        passedWaypoint = nextWaypoint;

                      //  Console.WriteLine("In radius, passed waypoint {0} ", passedWaypoint.pose.pose.position.x);
                        Result(true);
                        isWaypoinPassed = true;
                        //tokenSource2.Cancel();
                        break;

                    }
                    else
                    {
                        isWaypoinPassed = false;
                        break;
                    }
                   
                }
            }

        }
    }
}
