using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shared = DeepLogic.Shared;

namespace BNet.Plugin.ConsoleModules
{
    public class BNetExpConsole : DeepLogic.Core.ExpConsole.ConsoleModule
    {
        public BNetExpConsole()
        {
            //Add an alias to the console
            base.Aliases.Add("battlenet");
        }

        public override string ConsoleModuleName
        {
            get
            {
                return "BNet";
            }
        }

        public override void ProcessCommand(List<string> args)
        {
            switch (args[0].ToLower())
            {
                default:
                    Console.WriteLine("-ExpCon: " + args[0] + ": command not found");
                    break;
            }
        }

        public override void GetDescription()
        {
            Console.WriteLine("BNet Plugin: Stub");
            Console.WriteLine("    Console Commands Not Implemented");
        }

        //Methods
    }
}
