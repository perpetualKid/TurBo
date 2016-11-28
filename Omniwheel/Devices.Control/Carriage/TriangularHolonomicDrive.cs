using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrickPi.Uwp.Motors;

namespace Devices.Control.Carriage
{

    public class TriangularHolonomicDrive
    {
        private Motor motorA;
        private Motor motorB;
        private Motor motorC;

        private const double degree2Radian = Math.PI / 180;
        private readonly double motorACos = Math.Cos(270 * degree2Radian);
        private readonly double motorASin = Math.Sin(270 * degree2Radian);
        private readonly double motorBCos = Math.Cos(30 * degree2Radian);
        private readonly double motorBSin = Math.Sin(30 * degree2Radian);
        private readonly double motorCCos = Math.Cos(150 * degree2Radian);
        private readonly double motorCSin = Math.Sin(150 * degree2Radian);

        public TriangularHolonomicDrive(Motor motorA, Motor motorB, Motor motorC)
        {
            this.motorA = motorA;
            this.motorB = motorB;
            this.motorC = motorC;
        }

        public void MoveRobot(double direction, int vLinear, int vAngular)
        {
            direction *= degree2Radian;
            double VwA = vLinear * (Math.Cos(direction) * motorACos - Math.Sin(direction) * motorASin) + vAngular;
            double VwB = vLinear * (Math.Cos(direction) * motorBCos - Math.Sin(direction) * motorBSin) + vAngular;
            double VwC = vLinear * (Math.Cos(direction) * motorCCos - Math.Sin(direction) * motorCSin) + vAngular;

            motorA.Velocity = (int)VwA;
            motorB.Velocity = (int)VwB;
            motorC.Velocity = (int)VwC;
        }

        public void Start()
        {
            MoveRobot(0, 0, 0);
            motorA.Enabled = true;
            motorB.Enabled = true;
            motorC.Enabled = true;
        }

        public void Stop()
        {
            motorA.Enabled = false;
            motorB.Enabled = false;
            motorC.Enabled = false;
            MoveRobot(0, 0, 0);
        }

        #region properties 
        public Motor MotorA { get { return this.motorA; } }

        public Motor MotorB { get { return this.motorB; } }

        public Motor MotorC { get { return this.motorC; } }
        #endregion

    }
}
