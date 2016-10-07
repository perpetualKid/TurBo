using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public abstract Task ProcessCommand(ControllableComponent sender, string[] commands);

        public abstract Task ComponentHelp(ControllableComponent sender);

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
                    await processor.ProcessCommand(sender, commands);
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
                    case "HELLO":
                        await Hello(sender);
                        break;
                    case "LIST":
                        await ListComponents(sender);
                        break;
                    case "ECHO":
                        await Echo(sender, input);
                        break;
                    case "EXIT":
                    case "CLOSE":
                        await CloseChannel(sender);
                        break;
                    default:
                        Debug.WriteLine("{0} :: Nothing to do on '{1}'", sender.componentName, input);
                        break;
                }
            }

        }

        protected static async Task HandleOutput(ControllableComponent sender, string text)
        {
            List<Task> sendTasks = new List<Task>();
            if (sender is ChannelHolder)
                await (sender as ChannelHolder).Channel.Send(text).ConfigureAwait(false);
            else
            {
                foreach (CommunicationComponentBase publisher in communicationComponents)
                {
                    sendTasks.Add(publisher.Send(sender, text));
                }
                await Task.WhenAll(sendTasks).ConfigureAwait(false);
            }
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

        public static async Task Echo(ControllableComponent sender, string input)
        {
            await HandleOutput(sender, input);
        }

        public static async Task Hello(ControllableComponent sender)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("HELLO. Great to see you here.");
            builder.Append(Environment.NewLine);
            builder.Append("Use 'HELP + CRLF' command to get help");
            builder.Append(Environment.NewLine);
            await HandleOutput(sender, builder.ToString());
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

        public static async Task CloseChannel(ControllableComponent sender)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("BYE. Hope to see you soon again.");
            builder.Append(Environment.NewLine);
            await HandleOutput(sender, builder.ToString());
            if (sender is ChannelHolder)
                await (sender as ChannelHolder).Channel.Close().ConfigureAwait(false);

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
