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
using VRage.Import;
using VRageMath;


namespace IngameScript
{
    public partial class Program : MyGridProgram
    {
        public class Ship
        {
            private readonly List<IMyGyro> myGyros = new List<IMyGyro>();
            private readonly Dictionary<Base6Directions.Direction,List<IMyThrust>> myThrusters = new Dictionary<Base6Directions.Direction,List<IMyThrust>>
            {
                {Base6Directions.Direction.Forward, new List<IMyThrust>()},
                {Base6Directions.Direction.Backward, new List<IMyThrust>()},
                {Base6Directions.Direction.Left, new List<IMyThrust>()},
                {Base6Directions.Direction.Right, new List<IMyThrust>()},
                {Base6Directions.Direction.Up, new List<IMyThrust>()},
                {Base6Directions.Direction.Down, new List<IMyThrust>()}
            };
            private IMyTerminalBlock myReference;
            readonly Boolean ControlGyros = true;
            private String _terminalTag;
            public String TerminalTag { 
                get { return _terminalTag; } 
                set { 
                    ControlEnabled(false);
                    _terminalTag = value; 
                    List<IMyRemoteControl> myRemoteControls = new List<IMyRemoteControl>();
                    MyGrid.GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(myRemoteControls, x => x.CustomName.Contains(_terminalTag));
                    MyGrid.GridTerminalSystem.GetBlocksOfType<IMyGyro>(myGyros, x => x.CustomName.Contains(_terminalTag));
                    switch (myRemoteControls.Count) {
                        case 1:
                            myReference = myRemoteControls[0];
                            break;
                        default:
                            _control = false;
                            throw new Exception("Needs exactly one remote control! Found: " + myRemoteControls.Count);
                    }
                    foreach (var thisDirection in Base6Directions.EnumDirections) {
                        MyGrid.GridTerminalSystem.GetBlocksOfType<IMyThrust>(
                            myThrusters[thisDirection], 
                                x => x.CustomName.Contains(_terminalTag)
                                && ThrustDirection(x) == thisDirection
//                              && Base6Directions.GetDirection(x.GridThrustDirection) == thisDirection
                        );
                        MyGrid.Echo("Direction: " + thisDirection.ToString() + " found: " + myThrusters[thisDirection].Count);
                        foreach (var myThruster in myThrusters[thisDirection])
                        {
                            MyGrid.Echo("Found thruster: " + myThruster.CustomName);
                            MyGrid.Echo("Thruster orientation: " + myThruster.Orientation.Forward);
                            myThruster.CustomData = thisDirection.ToString();
                        }
                    }
                    ControlEnabled(_control);
                }
            }

            private Base6Directions.Direction ThrustDirection(IMyThrust thruster)
            {
                return myReference.Orientation.TransformDirectionInverse(
                    Base6Directions.GetFlippedDirection(
                        thruster.Orientation.Forward
                    )
                );
            }

            readonly float RotationFactor = 1f;
            private readonly MyGridProgram MyGrid;
            private Quaternion homeQuaternion;
            private Vector3 homePosition;
            public void SetHome(){
                var homeConjugate = Quaternion.CreateFromRotationMatrix(myReference.WorldMatrix); 
                homeQuaternion = Quaternion.Conjugate(homeConjugate);
                homePosition = MyPosition;
            }
            public Vector3 MyPosition
            {
                get { return myReference.GetPosition(); }
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
            public Ship(MyGridProgram grid, String terminalTag)
            {
                MyGrid = grid;
                MyGrid.Echo("Ship initialized");
                homeQuaternion = Quaternion.Zero;
                TerminalTag = terminalTag;
            }
            private void ControlEnabled(Boolean control)
            {
                foreach (var myGyro in myGyros)
                {
                    myGyro.GyroOverride = control;
                }
            }
            private void UpdateGyro(IMyGyro myGyro, Vector3 angularMovement)
            {

                // Set in Radians per second, visualized in the control panel as RPM
                if (ControlGyros && Control ) {
                    myGyro.Pitch = angularMovement.X; 
                    myGyro.Yaw = angularMovement.Y; 
                    myGyro.Roll = angularMovement.Z;
                }
            }
            public void RunTick10()
            {
                TurnToCurrentTarget();
                GoToHomeLine();
            }
            private void GoToHomeLine()
            {
                if (Vector3.IsZero(homePosition)) { return; }

                var myDelta = homePosition - MyPosition;
                
                var upDelta = myDelta.Dot(homeQuaternion.Up);
                var rightDelta = myDelta.Dot(homeQuaternion.Right);
                MyGrid.Echo($"upDelta: {upDelta} rightDelta: {rightDelta}");

                foreach (var myThruster in myThrusters[Base6Directions.Direction.Up])
                {
                    myThruster.Enabled = upDelta > 0f;
//                    myThruster.ThrustOverridePercentage = Math.Abs(upDelta);
                }
                foreach (var myThruster in myThrusters[Base6Directions.Direction.Down])
                {
                    myThruster.Enabled = upDelta < 0f;
//                    myThruster.ThrustOverridePercentage = Math.Abs(upDelta);
                }
                foreach (var myThruster in myThrusters[Base6Directions.Direction.Right])
                {
//                    myThruster.Enabled = rightDelta > 0f;
//                    myThruster.ThrustOverridePercentage = Math.Abs(rightDelta);
                }
                foreach (var myThruster in myThrusters[Base6Directions.Direction.Left])
                {
//                    myThruster.Enabled = rightDelta < 0f;
//                    myThruster.ThrustOverridePercentage = Math.Abs(rightDelta);
                }

            }
            private void TurnToCurrentTarget()
            {
                if (Quaternion.IsZero(homeQuaternion)) { return; }

                var worldConjugate = Quaternion.CreateFromRotationMatrix(myReference.WorldMatrix); 
                var rotation = homeQuaternion * worldConjugate;

                if (rotation.W < 0f) {
                    rotation=Quaternion.Negate(rotation);
                }

                Quaternion refQuaternion, refConjugate;  
                myReference.Orientation.GetQuaternion(out refQuaternion);
                Quaternion.Conjugate(ref refQuaternion, out refConjugate);

                var shipRotation = refQuaternion * rotation * refConjugate;

                foreach (var myGyro in myGyros)
                {
                    Quaternion gyroQuaternion, gyroConjugate;  

                    myGyro.Orientation.GetQuaternion(out gyroQuaternion);
                    Quaternion.Conjugate(ref gyroQuaternion, out gyroConjugate);
                    var gyroRotation = gyroConjugate * shipRotation * gyroQuaternion;

                    float angle;
                    Vector3 gyroAxis;

                    gyroRotation.GetAxisAngle(out gyroAxis, out angle);
                    UpdateGyro(myGyro, angle*RotationFactor*gyroAxis);
                }
            }
        }


        private readonly Ship MyShip;
        public Program()
        {
            MyIniParseResult result;
            MyIni _ini = new MyIni();
            if (!_ini.TryParse(Me.CustomData, out result))
            {
                Echo($"Failed to parse custom data! {result}");
                return;
            }
            var terminalTag = _ini.Get("Settings", "TerminalTag").ToString("[tnm]");
            var targetX = _ini.Get("Settings", "TargetX").ToSingle(0f);
            var targetY = _ini.Get("Settings", "TargetY").ToSingle(0f);
            var targetZ = _ini.Get("Settings", "TargetZ").ToSingle(0f);
            var targetW = _ini.Get("Settings", "TargetW").ToSingle(1f);
            var targetQuaternion = new Quaternion(targetX, targetY, targetZ, targetW);

            MyShip = new Ship(this, terminalTag);
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
                case "home":
                    Echo("Setting home position");
                    MyShip.SetHome();
                    break;
                case "tag":
                    Echo("Setting terminal tag");
                    MyShip.TerminalTag = "[tnm]";
                    break;
            }
        }
    }
}