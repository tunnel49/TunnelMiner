using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.Graphics.GUI;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;


namespace IngameScript
{
    public partial class Program : MyGridProgram
    {
        public class Ship
        {
            private readonly String myRemoteControlName = "myRemoteControl";
            private readonly String myGyroName = "myGyro";


            public float RotationFactor { get; set; }
            public IMyRemoteControl MyRemoteControl { get; }
            private readonly IMyGyro MyGyro;
            private readonly MyGridProgram MyGrid;

            private Quaternion _targetQuaternion;
            public Quaternion TargetQuaternion
            {
                get { return _targetQuaternion; }
                set
                {
                    _targetQuaternion = value;
                }
            }
            private bool _control;
            public bool Control
            {
                get { return _control; }
                set
                {
                    _control = value;
                    ControlEnabled(value);
                }
            }
            public Ship(MyGridProgram grid)
            {
                MyGrid = grid;
                RotationFactor = 1f;
                MyRemoteControl = MyGrid.GridTerminalSystem.GetBlockWithName(myRemoteControlName) as IMyRemoteControl;
                MyGyro = MyGrid.GridTerminalSystem.GetBlockWithName(myGyroName) as IMyGyro;
                MyGrid.Echo("Ship initialized");
                TargetQuaternion = Quaternion.Zero;
            }
            private void ControlEnabled(Boolean control)
            {
                MyGyro.GyroOverride = control;
            }
            private void UpdateGyro(Vector3 angularMovement)
            {
                // Set in Radians per second, visualized in the control panel as RPM
                MyGyro.Pitch = -angularMovement.X; 
                MyGyro.Yaw = -angularMovement.Y; 
                MyGyro.Roll = -angularMovement.Z;
            }
            public void RunTick10()
            {
                if (! Quaternion.IsZero(TargetQuaternion)) {
                    TurnToCurrentTarget();
                }
            }
            private void TurnToCurrentTarget()
            {
                var MyQuaternion = Quaternion.CreateFromRotationMatrix(MyGyro.WorldMatrix);
                var MyConjugate = Quaternion.Conjugate(MyQuaternion); 
                var MyWorldRotation = TargetQuaternion * MyConjugate;
                var MyLocalRotation = MyQuaternion * MyWorldRotation * MyConjugate;

                if (MyLocalRotation.W < 0f) {
                    MyLocalRotation = Quaternion.Negate(MyLocalRotation);
                }
                float angle;
                Vector3 axis;
                MyLocalRotation.GetAxisAngle(out axis, out angle);
                UpdateGyro(axis * angle * RotationFactor);
            }
        }

        private readonly Ship MyShip;
        public Program()
        {
            MyShip = new Ship(this)
            {
                TargetQuaternion = new Quaternion(1f, 0f, 0f, 0f)
            };
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        private void RunTick10()
        {
            // This method will be called every 10th tick (6 times a second).
            // This is a good place to put code that needs to run frequently
            // but doesn't need to run every tick.
            MyShip.RunTick10();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            switch (updateSource){
                case UpdateType.Update10:
                    RunTick10();
                    break;
                case UpdateType.Terminal:
                case UpdateType.Script:
                case UpdateType.Trigger:
                    RunCommand(argument);
                    break;
                default:
                    break;
            }
        }
        public void RunCommand(string argument)
        {
            switch (argument)
            {
                case "debug":
                    Echo("Hello World");
                    if (MyShip.MyRemoteControl == null) {
                        Echo("No remote control found");
                    } else {
                        Echo("Remote control found:");
                        Echo("  " + MyShip.MyRemoteControl.CustomName);
                        Echo("My Local Quaternion:");
                        Quaternion myQuaternion;
                        MyShip.MyRemoteControl.Orientation.GetQuaternion(out myQuaternion);
                        Echo("  " + myQuaternion );

                        Quaternion myWorldQuaternion = Quaternion.CreateFromRotationMatrix(MyShip.MyRemoteControl.WorldMatrix);
                        Echo("  " + myWorldQuaternion);
                    }
                    break;
                case "run":
                    Echo("Running");
                    Runtime.UpdateFrequency = UpdateFrequency.Update10;
                    MyShip.Control = true;

                    break;
                case "stop":
                    Echo("Stopping");
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                    MyShip.Control = false;
                    break;
            }
        }
    }
}