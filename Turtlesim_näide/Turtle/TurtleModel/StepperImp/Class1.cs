using System;
using System.Threading.Tasks;
using RosSharp.RosBridgeClient;
using NModel.Conformance;
using NModel.Terms;
using System.Threading;
namespace StepperImp

{
    public class Stepper : NModel.Conformance.IStepper
    {

        public static float xposition { get; set; }
        public static bool isCollision = false;
      
        public static float yposition { get; set; }
        //Defineerime ja initsialiseerime kanalid mida kuulame ja millesse kuulutame
        static RosSocket rosSocket = new RosSocket("ws://127.0.0.1:9090");
        static int publish_vel = rosSocket.Advertize("/turtle1/cmd_vel", "geometry_msgs/Twist");
        static int subscribe_rosout = rosSocket.Subscribe("rosout", "rosgraph_msgs/Log", rosoutCallback);
        static int reset_pub = rosSocket.Advertize("reset", "std_srvs/Empty");
        public CompoundTerm DoAction(CompoundTerm action)
        {
            

            switch (action.Name)
            {
                case ("Tests"): return null;
                //Liigutame konna
                case ("MoveTurtle"):
                    GeometryTwist message = new GeometryTwist();
                    var speed = (float)action[0];
                    var angleX = (float)action[1];
                    if (speed <= angleX)
                    {
                        message.linear.x = speed;
                    }
                    else
                    {
                        message.linear.x = 7.0f;
                        message.angular.z = 7.0f;
                    }
                   
                    rosSocket.Publish(publish_vel, message);
                    isCollision = false;
                    return null;
                //Kontrollime ette antud aja pärast kas konn on jõudnud kohale
                case ("checkCollision"):
                    Console.WriteLine(isCollision);
                    var task = Task.Run(() => moveOrFail());
                    if (task.Wait(TimeSpan.FromSeconds(10)))
                        return task.Result;
                    else
                        throw new Exception("Timed out");
            

                default: throw new Exception("Unexpected action " + action);

            }
        }
        //Kontrollime et konn ei oleks seina liikunud
        static CompoundTerm moveOrFail()
        {
            Thread.Sleep(5000);
            if (isCollision==false)
            {
                Console.WriteLine("´Kas sein on ees  {0}", isCollision);
                return null;
            }
            else
            {


                return NModel.Terms.Action.Create("checkCollision",true);
            }
        }
        //Testi algoleku taastamine - tearDown
        void IStepper.Reset()
        {
           
        }
        //Adapteri konstrukto
        public static IStepper Create()
        {
            return new Stepper();
        }

        //Kuulame kas konn on liikunud seina
        private static void rosoutCallback(Message message)
        {

            RosGraphMsgsLog geo = (RosGraphMsgsLog)message;

            Console.WriteLine("Väljast2Message msg:");
            Console.WriteLine(geo.msg);
            if (geo.msg.Contains("Oh no! I hit the wall!"))
            {
                Console.WriteLine("Hit the wall");
                isCollision = true;
            }
            else
            {

                Console.WriteLine("Did not hit the wall");


              
            }
        }
    }
}
