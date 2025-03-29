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
            public IMyRemoteControl MyRemoteControl { get; }
            private readonly IMyGyro MyGyro;
            private readonly MyGridProgram MyGrid;
            private readonly String myRemoteControlName = "myRemoteControl";
            private readonly String myGyroName = "myGyro";
            private bool _control;
            public Quaternion MyQuaternion
            {
                get { return Quaternion.CreateFromRotationMatrix(MyGyro.WorldMatrix); } 
            }
            public Quaternion MyConjugate
            {
                get { return Quaternion.Conjugate(MyQuaternion); } 
            }
            public bool Control
            {
                get { return _control; }
                set
                {
                    _control = value;
                    ControlEnabled(value);
                }
            }
            public Vector3 AngularMovement //Radians per second
            {
                set
                {
                    UpdateGyro(value);
                }
            }
            
            public Ship(MyGridProgram grid)
            {
                MyGrid = grid;
                MyRemoteControl = grid.GridTerminalSystem.GetBlockWithName(myRemoteControlName) as IMyRemoteControl;
                MyGyro = grid.GridTerminalSystem.GetBlockWithName(myGyroName) as IMyGyro;
            }
            private void ControlEnabled(Boolean control)
            {
                MyGyro.GyroOverride = control;
            }
            private void UpdateGyro(Vector3 angularMovement)
            {
                // Set in Radians per second, visualized in the control panel as RPM
                MyGyro.Pitch = angularMovement.X;
                MyGyro.Yaw = angularMovement.Y;
                MyGyro.Roll = angularMovement.Z;
            }
        }

        private readonly Ship MyShip;
        private readonly Quaternion MyTargetQuaternion;
        public Program()
        {
            MyShip = new Ship(this); 
            MyTargetQuaternion = new Quaternion(0f, 0f, 0f, 1f);
  
            // It's recommended to set Runtime.UpdateFrequency 
            // here, which will allow your script to run itself without a 
            // timer block.

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
            // MyShip.AngularMovement = new Quaternion(0f, 0f, 1f, 0f);
            // This method will be called every 10th tick (6 times a second).
            // This is a good place to put code that needs to run frequently
            // but doesn't need to run every tick.
            var MyDelta = MyTargetQuaternion * MyShip.MyConjugate ;
            Vector3 axis;
            float angle;
            MyDelta.GetAxisAngle(out axis, out angle);
            MyShip.AngularMovement = angle * axis;
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