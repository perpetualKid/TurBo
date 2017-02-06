using System;
using System.Threading.Tasks;
using BrickPi.Uwp.Motors;
using Devices.Components;
using Turbo.BrickPi.Components.Carriage;

namespace Turbo.BrickPi.Components.Lego
{
    public class DriveComponent : ComponentBase
    {
        private TriangularHolonomicDrive holonomicDrive;

        public DriveComponent(string componentName, ComponentBase parent, Motor motorA, Motor motorB, Motor motorC) : base(componentName, parent)
        {
            this.holonomicDrive = new TriangularHolonomicDrive(motorA, motorB, motorC);
        }

        [Action("Stop")]
        [Action("Disable")]
        [ActionHelp("Stops all motors")]
        private Task DriveComponentStop(MessageContainer data)
        {
            holonomicDrive.Stop();
            return Task.CompletedTask;
        }

        [Action("Start")]
        [Action("Enable")]
        [ActionHelp("Starts all motors")]
        private Task DriveComponentStart(MessageContainer data)
        {
            holonomicDrive.Start();
            return Task.CompletedTask;
        }

        [Action("Move")]
        [ActionParameter("Direction")]
        [ActionParameter("Velocity")]
        [ActionParameter("Rotation")]
        [ActionHelp("Drives or turns at given velocity to given direction.")]
        private Task DriveComponentDrive(MessageContainer data)
        {
            double direction = Double.Parse(data.ResolveParameter("Direction", 0));
            int vLinear = int.Parse(data.ResolveParameter("Velocity", 1));
            int vAngular = int.Parse(data.ResolveParameter("Rotation", 2));

            holonomicDrive.MoveRobot(direction, vLinear, vAngular);
            return Task.CompletedTask;
        }

        #region public
        public Motor MotorA { get { return holonomicDrive.MotorA; } }

        public Motor MotorB { get { return holonomicDrive.MotorB; } }

        public Motor MotorC { get { return holonomicDrive.MotorC; } }
        #endregion
    }
}
