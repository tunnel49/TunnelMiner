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
        private readonly IMyRemoteControl myRemoteControl;
        private readonly String myRemoteControlName = "myRemoteControl";

        public Program()
        {
            myRemoteControl = GridTerminalSystem.GetBlockWithName(myRemoteControlName) as IMyRemoteControl;
  
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

        public void Main(string argument, UpdateType updateSource)
        {
            switch (argument)
            {
                case "debug":
                    Echo("Hello World");
                    if (myRemoteControl == null) {
                        Echo("No remote control found");
                    } else {
                        Echo("Remote control found:");
                        Echo("  " + myRemoteControl.CustomName);
                        Echo("My Local Quaternion:");
                        Quaternion myQuaternion;
                        myRemoteControl.Orientation.GetQuaternion(out myQuaternion);
                        Echo("  " + myQuaternion );

                        Quaternion myWorldQuaternion = Quaternion.CreateFromRotationMatrix(myRemoteControl.WorldMatrix);
                        Echo("  " + myWorldQuaternion);
                    }
                    break;
            } 
    
        }
    }
}
