using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeepLogic;
using Shared = DeepLogic.Shared;

namespace BNet.Plugin
{
    public class Initializer
    {
        public Shared.DBObjects.User PluginUser = new Shared.DBObjects.User();

        public void Start()
        {
            //Initialize WCF net.pipe API
            DLS_Module.LocalAPI = new API.DLS(new TimeSpan(0, 5, 0));
            DLS_Module.LocalAPI.SetServer("localhost", "net.pipe");

            //Register our Console Plugin (DEPRECIATION WARNING - Types will be picked up through reflection in DLS build 1040)
            DeepLogic.Core.ErrorHandler.Log("BNet", "Notice", "Registering Console Module");
            DeepLogic.Core.ExpConsole.C_Driver.Plugins.Add(typeof(ConsoleModules.BNetExpConsole));
            DeepLogic.Core.ExpConsole.C_Driver.Plugins.Add(typeof(ConsoleModules.BitStreamExpConsole));

            //Start networking
            NetHandler.AuxServer = new DeepLogic.Net.AuxSocketServer(1119);
            NetHandler.AuxServer.ReceivedDataHandler += NetHandler.ProcessPacket;
            DeepLogic.Core.ErrorHandler.Log("BNet", "Notice", "Registered data handler with Aux Server");
            NetHandler.AuxServer.Start();
        }

        public void InitializePluginUser()
        {
            if (DeletePluginUsers())
                DeepLogic.Core.ErrorHandler.Log("BNet", "Notice", "Cleared stale IPC user accounts");
            CreatePluginUser();
        }

        public bool CreatePluginUser()
        {
            Guid Token = PluginUser.NewToken;
            string Password = Guid.NewGuid().ToString();
            PluginUser.Initialize();
            PluginUser.AccessType = "IPC";
            PluginUser.Name = "TEMP_MODULE:BNet";
            PluginUser.FirstName = "Module Communication";
            PluginUser.Permissions = Shared.Security.Permissions.Flags.GROUP_IPC;
            PluginUser.Password = Password;
            App.Server.db4oDatabase.UserCom.AddUser(PluginUser);
            KeyValuePair<Shared.Events.EventCode, Shared.AuthObject> kvp = DLS_Module.LocalAPI.AuthService.Login(PluginUser.Name, Password);
            PluginUser = kvp.Value.UserObject;
            return (kvp.Key == Shared.Events.EventCode.AUTH_LOGIN_OK);
        }

        public bool DeletePluginUsers()
        {
            PluginUser = new Shared.DBObjects.User();
            PluginUser.AccessType = "IPC";
            PluginUser.HasPermission(Shared.Security.Permissions.Flags.GROUP_IPC);
            PluginUser.Name = "TEMP_MODULE:BNet";
            List<Shared.DBObjects.User> Result = App.Server.db4oDatabase.UserCom.LookupUserObject(PluginUser);
            if (Result.Count > 0)
            {
                bool Deleted = false;
                foreach (Shared.DBObjects.User User in Result)
                {
                    //Being lazy and calling usercom directly since we won't have a session set up.  Assembly must be in the same appdomain
                        // Deleted |= (API.UserManagerService.DeleteUser(User) == Shared.Events.EventCode.USER_DELETE_SUCCESS);
                    App.Server.db4oDatabase.UserCom.DeleteUser(User);
                    Deleted = true;
                }
                return Deleted;
            }
            else
                return false;
        }

        ~Initializer()
        {
            try
            {
                DeletePluginUsers();
            }
            catch { }
        }
    }
}
