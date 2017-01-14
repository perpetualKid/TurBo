﻿using System;
using System.Threading.Tasks;
using BrickPi.Uwp.Motors;
using Devices.Controllable;
using Turbo.BrickPi.Components.Carriage;

namespace Turbo.BrickPi.Components.Lego
{
    public class DriveComponent : ControllableComponent
    {
        private TriangularHolonomicDrive holonomicDrive;

        public DriveComponent(string componentName, ControllableComponent parent, Motor motorA, Motor motorB, Motor motorC) : base(componentName, parent)
        {
            this.holonomicDrive = new TriangularHolonomicDrive(motorA, motorB, motorC);
        }

        protected override async Task ComponentHelp(MessageContainer data)
        {
            data.AddMultiPartValue("Help", "DRIVE HELP : Shows this help screen.");
            data.AddMultiPartValue("Help", "DRIVE STOP : Stops all motors.");
            data.AddMultiPartValue("Help", "DRIVE START : Starts all motors. Does not start driving.");
            data.AddMultiPartValue("Help", "DRIVE MOVE <Direction>:<Velocity>:<Rotation> : Drives  or turns at given velocity to given direction.");
            await HandleOutput(data).ConfigureAwait(false);
        }

        protected override async Task ProcessCommand(MessageContainer data)
        {
            switch (data.ResolveParameter(nameof(MessageContainer.FixedPropertyNames.Action), 1).ToUpperInvariant())
            {
                case "HELP":
                    await ComponentHelp(data);
                    break;
                case "STOP":
                    await DriveComponentStop(data);
                    break;
                case "START":
                    await DriveComponentStart(data);
                    break;
                case "MOVE":
                    await DriveComponentDrive(data);
                    break;
            }
        }

        private async Task DriveComponentStop(MessageContainer data)
        {
            holonomicDrive.Stop();
            await Task.CompletedTask;
        }
        private async Task DriveComponentStart(MessageContainer data)
        {
            holonomicDrive.Start();
            await Task.CompletedTask;
        }


        private async Task DriveComponentDrive(MessageContainer data)
        {
            double direction = Double.Parse(data.ResolveParameter("Direction", 0));
            int vLinear = int.Parse(data.ResolveParameter("Velocity", 1));
            int vAngular = int.Parse(data.ResolveParameter("Rotation", 2));

            holonomicDrive.MoveRobot(direction, vLinear, vAngular);
            await Task.CompletedTask;
        }

        #region public
        public Motor MotorA { get { return holonomicDrive.MotorA; } }

        public Motor MotorB { get { return holonomicDrive.MotorB; } }

        public Motor MotorC { get { return holonomicDrive.MotorC; } }
        #endregion
    }
}