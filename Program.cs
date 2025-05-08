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


        private readonly T49TunnelMiner MyShip;
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

            MyShip = new T49TunnelMiner(this, terminalTag);
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