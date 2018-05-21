using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RosSharp.RosBridgeClient;
using NModel.Conformance;
using NModel.Terms;

namespace TurtleAdpImp

{
    public class Stepper:NModel.Conformance.IStepper
    {

        public static float xposition { get; set; }
        public static float yposition { get; set; }
        static RosSocket rosSocket = new RosSocket("ws://127.0.0.1:9090");
        static int publication_id = rosSocket.Advertize("/turtle1/cmd_vel", "geometry_msgs/Twist");
        static int publication2_id = rosSocket.Advertize("/turtle1/pose", "turtlesim/Pose");
        static int subscription_id = rosSocket.Subscribe("turtle1/pose", "turtlesim/Pose", subscriptionHandler);
        public CompoundTerm DoAction(CompoundTerm action)
        {



            switch (action.Name)
            {
                case ("Tests"): return null;
                case ("StartPosition"):
                    xposition = 0;
                    TurtlesimPose messageP = new TurtlesimPose();
                    messageP.x = 0;
                    messageP.y = 0;
                    messageP.theta = 0;
                    messageP.angular_velocity = 0;
                    messageP.linear_velocity = 0;
                    rosSocket.Publish(publication_id, messageP);


                    return null;
                case ("MoveTurtle"):
                    GeometryTwist message = new GeometryTwist();
                    float speed = (float)action[0];
                    message.linear.x = speed;
                    rosSocket.Publish(publication_id, message);
                    return null;
                case ("Goal_Start"):
                    if (xposition > 0)
                    {
                        return NModel.Terms.Action.Create("Goal_Finish");
                    }
                    else
                    {
                        return NModel.Terms.Action.Create("Goal_Failed");
                    }


                default: throw new Exception("Unexpected action " + action);

            }
        }
        void IStepper.Reset()
        {
            TurtlesimPose message = new TurtlesimPose();
            message.x = 0;
            message.y = 0;
            rosSocket.Publish(publication_id, message);
        }

        public static IStepper Create()
        {
            return new Stepper();
        }

        private static void subscriptionHandler(Message message)
        {


            TurtlesimPose geo = (TurtlesimPose)message;
            Console.WriteLine("Väljast2:");
            Console.WriteLine(geo.x);
            xposition = geo.x;
            yposition = geo.y;


        }

   
    }
}
