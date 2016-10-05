using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Communication;
using Common.Communication.Channels;
using Devices.Control.Communication;
using Windows.Data.Json;
using Windows.Storage.Streams;

namespace Devices.Control.Base
{
    public abstract class ControllableComponent
    {
        protected string componentName;

        protected static Dictionary<string, ControllableComponent> components = new Dictionary<string, ControllableComponent>();
        protected static List<CommunicationComponentBase> communicationComponents;

        static ControllableComponent()
        {
            communicationComponents = new List<CommunicationComponentBase>();
        }

        public ControllableComponent(string componentName)
        {
            this.componentName = componentName;
        }

        public async virtual Task InitializeDefaults()
        {
            await Task.FromResult(default(Task));
        }

        public static async Task RegisterComponent(ControllableComponent component)
        {
            components.Add(component.componentName.ToUpper(), component);
            await component.InitializeDefaults();
            if (component is CommunicationComponentBase)
                communicationComponents.Add(component as CommunicationComponentBase);
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
        protected static async Task HandleInput(ControllableComponent sender, string input)
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
                        await ListHelp(sender);
                        break;
                    case "LIST":
                        await ListComponents(sender);
                        break;
                    default:
                        Debug.WriteLine(sender.componentName, "Nothing to do on '{0}'", input);
                        break;
                }
            }

        }

        protected static async Task HandleOutput(ControllableComponent sender, string text)
        {
            List<Task> sendTasks = new List<Task>();
            foreach(CommunicationComponentBase publisher in communicationComponents)
            {
                sendTasks.Add(publisher.Send(sender, text));
            }
            await Task.WhenAll(sendTasks).ConfigureAwait(false);
        }

        protected static Task HandleOutput(ControllableComponent sender, JsonObject json)
        {
            throw new NotImplementedException();
            //if (null != dataPortInstance)
            //    dataPortInstance.WriteLine(text);
        }

        protected static void HandleOutput(ControllableComponent sender, byte[] data)
        {
            throw new NotImplementedException();
            //if (null != dataPortInstance)
            //    dataPortInstance.WriteLine(text);
        }

        protected static void HandleOutput(ControllableComponent sender, IRandomAccessStream stream)
        {
            throw new NotImplementedException();
            //if (null != dataPortInstance)
            //    dataPortInstance.WriteLine(text);
        }
        protected static void HandleOutput(ControllableComponent sender, IBuffer data)
        {
            throw new NotImplementedException();
            //if (null != dataPortInstance)
            //    dataPortInstance.WriteLine(text);
        }

        public static async Task ListHelp(ControllableComponent sender)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("HELP :  Shows this help screen");
            builder.Append(Environment.NewLine);
            builder.Append("LIST : Lists the available modules");
            builder.Append(Environment.NewLine);
            await HandleOutput(sender, builder.ToString());
        }

        public static async Task ListComponents(ControllableComponent sender)
        {
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, ControllableComponent> item in components)
            {
                builder.Append(item.Key);
                builder.Append(Environment.NewLine);
            }
            await HandleOutput(sender, builder.ToString());
        }

        #endregion

        protected static void HandleInput(ControllableComponent sender, JsonObject jsonObject)
        {
        }

    }
}
