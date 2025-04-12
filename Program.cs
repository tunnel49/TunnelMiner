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
            readonly Boolean CorrectW = true;
            readonly Boolean ControlGyros = true;
            readonly float RotationFactor = 1f;
            private readonly String myReferenceName = "myRemoteControl";
            private readonly String myGyroName = "myGyro";
            public Int16 pitchSign = 1;
            public Int16 yawSign = 1;
            public Int16 rollSign = 1;

            public IMyTerminalBlock MyReference { get; }
            private readonly List<IMyGyro> MyGyros;
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
                MyReference = MyGrid.GridTerminalSystem.GetBlockWithName(myReferenceName) as IMyRemoteControl;
                MyGyros = new List<IMyGyro>{
                    MyGrid.GridTerminalSystem.GetBlockWithName(myGyroName) as IMyGyro
                };
                MyGrid.Echo("Ship initialized");
                TargetQuaternion = Quaternion.Zero;
            }
            private void ControlEnabled(Boolean control)
            {
                foreach (var myGyro in MyGyros)
                {
                    myGyro.GyroOverride = control;
                }
            }
            private void UpdateGyro(IMyGyro myGyro, Vector3 angularMovement)
            {

                // Set in Radians per second, visualized in the control panel as RPM
                if (ControlGyros && Control ) {
                    myGyro.Pitch = pitchSign * angularMovement.X; 
                    myGyro.Yaw = yawSign * angularMovement.Y; 
                    myGyro.Roll = rollSign * angularMovement.Z;
                }
                MyGrid.Echo("Pitch:"+ -myGyro.Pitch );
                MyGrid.Echo("Yaw:"+ myGyro.Yaw );
                MyGrid.Echo("Roll:"+ myGyro.Roll );

            }
            public void RunTick10()
            {
                if (! Quaternion.IsZero(TargetQuaternion)) {
                    TurnToCurrentTarget();
                }
            }
            private void TurnToCurrentTarget()
            {
                var worldConjugate = Quaternion.CreateFromRotationMatrix(MyReference.WorldMatrix); 
                var rotation = TargetQuaternion * worldConjugate;

                MyGrid.Echo("Rotation X: " + rotation.X );
                MyGrid.Echo("Rotation Y: " + rotation.Y );
                MyGrid.Echo("Rotation Z: " + rotation.Z );
                MyGrid.Echo("Rotation W: " + rotation.W );

                Quaternion refQuaternion, refConjugate;  
                MyReference.Orientation.GetQuaternion(out refQuaternion);
                Quaternion.Conjugate(ref refQuaternion, out refConjugate);

                var shipRotation = refQuaternion * rotation * refConjugate;

                foreach (var myGyro in MyGyros)
                {
                    Quaternion gyroQuaternion, gyroConjugate;  

                    MyReference.Orientation.GetQuaternion(out gyroQuaternion);
                    Quaternion.Conjugate(ref gyroQuaternion, out gyroConjugate);
                    var newGyroRotation = gyroConjugate * shipRotation * gyroQuaternion;
                    var gyroRotation = rotation;

//                      var gyroRotation = referenceRotation;
//                    var gyroRotation = rotation;

                    if (gyroRotation.W < 0f && CorrectW) {
                        gyroRotation=Quaternion.Negate(gyroRotation);
                    }

                    float angle;
                    Vector3 gyroAxis;

                    gyroRotation.GetAxisAngle(out gyroAxis, out angle);
                    MyGrid.Echo("Gyro Angle: " + angle );

                    UpdateGyro(myGyro, angle*RotationFactor*gyroAxis);
                }
            }
        }


        private readonly Ship MyShip;
        public Program()
        {
            MyShip = new Ship(this);
            ReadIni();
        }
        public void ReadIni()
        {
            MyIniParseResult result;
            MyIni _ini = new MyIni();
            if (!_ini.TryParse(Me.CustomData, out result))
            {
                Echo($"Failed to parse custom data! {result}");
                return;
            }
            var pitchSign = _ini.Get("Settings", "PitchSign").ToInt16(1);
            var yawSign = _ini.Get("Settings", "YawSign").ToInt16(1);
            var rollSign = _ini.Get("Settings", "RollSign").ToInt16(1);
            var targetX = _ini.Get("Settings", "TargetX").ToSingle(0f);
            var targetY = _ini.Get("Settings", "TargetY").ToSingle(0f);
            var targetZ = _ini.Get("Settings", "TargetZ").ToSingle(0f);
            var targetW = _ini.Get("Settings", "TargetW").ToSingle(1f);
            var targetQuaternion = new Quaternion(targetX, targetY, targetZ, targetW);
            MyShip.pitchSign = pitchSign;
            MyShip.yawSign = yawSign;
            MyShip.rollSign = rollSign;
            MyShip.TargetQuaternion = Quaternion.Normalize(targetQuaternion);
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
                    if (MyShip.MyReference == null) {
                        Echo("No remote control found");
                    } else {
                        Echo("Remote control found:");
                        Echo("  " + MyShip.MyReference.CustomName);
                        Echo("My Local Quaternion:");
                        Quaternion myQuaternion;
                        MyShip.MyReference.Orientation.GetQuaternion(out myQuaternion);
                        Echo("  " + myQuaternion );

                        Quaternion myWorldQuaternion = Quaternion.CreateFromRotationMatrix(MyShip.MyReference.WorldMatrix);
                        Echo("  " + myWorldQuaternion);
                    }
                    Matrix myMatrix;
                    MyShip.MyReference.Orientation.GetMatrix(out myMatrix);
                    Echo("My Local Matrix:");
                    Echo("  " + myMatrix);
                    break;
                case "run":
                    Echo("Running");
                    Runtime.UpdateFrequency = UpdateFrequency.Update10;
                    MyShip.Control = true;

                    break;
                case "stop":
                    Echo("Stopping");
//                    Runtime.UpdateFrequency = UpdateFrequency.None;
                    MyShip.Control = false;
                    break;
                case "halt":
                    Echo("Stopping");
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                    MyShip.Control = false;
                    break;
                case "read":
                    Echo("Reading ini file");
                    ReadIni();
                    break;
            }
        }
    }
}