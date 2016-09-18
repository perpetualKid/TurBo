using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Windows.Data.Json;

namespace Devices.Control.Base
{
    public abstract class ControllableComponent
    {
        protected string componentName;

        protected static Dictionary<string, ControllableComponent> components = new Dictionary<string, ControllableComponent>();


        public ControllableComponent(string componentName)
        {
            this.componentName = componentName;
        }

        public string ComponentName { get { return this.componentName; } }

        public abstract void ProcessCommand(ControllableComponent sender, string[] commands);

        public abstract void ComponentHelp();

        public static string ResolveParameter(string[] parameterArray, int index)
        {
            if (null != parameterArray && parameterArray.Length > index && parameterArray[index] != null)
            {
                return parameterArray[index].ToUpper();
            }
            return null;
        }

        #region Text
        protected static void HandleInput(ControllableComponent sender, string input)
        {
            string[] commands = input.Split(':');
            string component = ResolveParameter(commands, 0);

            if (components.Keys.Contains(component))
            {
                try
                {
                    ControllableComponent processor = components[component] as ControllableComponent;
                    processor.ProcessCommand(sender, commands);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(component, ex.Message + "::" + ex.StackTrace);
                }
            }
            else
            {
                switch (component)
                {
                    case "HELP":
                        ListHelp(sender);
                        break;
                    case "LIST":
                        ListComponents(sender);
                        break;
                    default:
                        Debug.WriteLine(sender.componentName, "Nothing to do on '{0}'", input);
                        break;
                }
            }

        }

        protected static void HandleOutput(ControllableComponent sender, string text)
        {
            //if (null != dataPortInstance)
            //    dataPortInstance.WriteLine(text);
        }

        public static void ListHelp(ControllableComponent sender)
        {
            Debug.WriteLine(sender.componentName, "Listing help options.");
            HandleOutput(sender, "HELP :  Shows this help screen");
            HandleOutput(sender, "LIST :  Lists the available modules");
        }

        public static void ListComponents(ControllableComponent sender)
        {
            Debug.WriteLine(sender.componentName, "Listing available components.");
            foreach (KeyValuePair<string, ControllableComponent> item in components)
            {
                HandleOutput(sender, "Component " + item.Key);
            }
        }

        #endregion

        protected static void HandleInput(ControllableComponent sender, JObject jsonObject)
        {
        }
    }
}
