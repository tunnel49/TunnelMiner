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
            private Quaternion rcQuaternion, gyroQuaternion, rcConjugate, gyroConjugate;
            public bool Control
            {
                get { return _control; }
                set
                {
                    _control = value;
                    ControlEnabled(value);
                }
            }
            private Quaternion _angularMovement;
            public Quaternion AngularMovement
            {
                get { return _angularMovement; }
                set
                {
                    _angularMovement = value;
                    UpdateGyro();
                }
            }
            
            public Ship(MyGridProgram grid)
            {
                MyGrid = grid;
                MyRemoteControl = grid.GridTerminalSystem.GetBlockWithName(myRemoteControlName) as IMyRemoteControl;
                MyGyro = grid.GridTerminalSystem.GetBlockWithName(myGyroName) as IMyGyro;
                MyRemoteControl.Orientation.GetQuaternion(out rcQuaternion);
                rcConjugate = Quaternion.Conjugate(rcQuaternion);
                MyGyro.Orientation.GetQuaternion(out gyroQuaternion);
                gyroConjugate = Quaternion.Conjugate(gyroQuaternion);
            }
            private void ControlEnabled(Boolean control)
            {
                MyGyro.GyroOverride = control;
            }
            private void UpdateGyro()
            {   
                Quaternion shipAngularMovement = rcConjugate * AngularMovement * rcQuaternion;
                Quaternion gyroAngularMovement = gyroQuaternion * shipAngularMovement * gyroConjugate;
                var gyroToRc= rcQuaternion * gyroConjugate;
                var rcToGyro= gyroQuaternion * rcConjugate;
                // gyroAngularMovement = rcToGyro * AngularMovement * gyroToRc;
                // get the pitch, roll, and yaw from the qyroAngularMovement
                MyGrid.Echo("Angular Movement: " + AngularMovement);
                MyGrid.Echo("Ship Angular Movement: " + shipAngularMovement);
                MyGrid.Echo("Gyro Angular Movement: " + gyroAngularMovement);
                MyGrid.Echo("Gyro to RC: " + gyroToRc);
                MyGrid.Echo("RC to Gyro: " + rcToGyro);
                MyGyro.Pitch = gyroAngularMovement.X;
                MyGyro.Yaw = gyroAngularMovement.Y;
                MyGyro.Roll = gyroAngularMovement.Z;
            }
            
        }

        private readonly Ship MyShip;
        public Program()
        {
            MyShip = new Ship(this); 
  
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
            MyShip.AngularMovement = new Quaternion(0f, 0f, 1f, 0f);
            // This method will be called every 10th tick (6 times a second).
            // This is a good place to put code that needs to run frequently
            // but doesn't need to run every tick.
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