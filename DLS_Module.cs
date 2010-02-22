using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Shared = DeepLogic.Shared;

namespace BNet
{
    public class DLS_Module : Shared.Module
    {
        public static API.DLS LocalAPI;

        //Dependencies for load order
        public override List<Type> Dependencies
        {
            get { return null; }
        }

        public override string Header
        {
            get { return "BNet Server by Sagu, Ralek"; }
        }

        public override string Version
        {
            get {  return Assembly.GetCallingAssembly().GetName().Version.ToString() + String.Format(" API {0},{1} Build {2}", API.Version.Major, API.Version.Minor, API.Version.Build); }
        }

        //Unique module key
        public override string Key
        {
            get { return "BNet"; }
        }

        //CTOR
        public DLS_Module()
        {
            Plugin.Initializer _plugin = new Plugin.Initializer();
            base.Callback += _plugin.Start;
        }
    }
}
