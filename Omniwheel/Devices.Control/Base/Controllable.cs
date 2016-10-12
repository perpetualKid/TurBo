using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Communication;
using Devices.Control.Communication;
using Windows.Data.Json;
using Windows.Storage.Streams;

namespace Devices.Control.Base
{
    public abstract class Controllable
    {
        protected string componentName;

        protected static Dictionary<string, Controllable> components = new Dictionary<string, Controllable>();
        protected static List<CommunicationComponentBase> communicationComponents;

        static Controllable()
        {
            communicationComponents = new List<CommunicationComponentBase>();
        }

        public Controllable(string componentName)
        {
            this.componentName = componentName;
        }

        public async virtual Task InitializeDefaults()
        {
            await Task.FromResult(default(Task));
        }

        public static async Task<Controllable> RegisterComponent(Controllable component)
        {
            components.Add(component.componentName.ToUpperInvariant(), component);
            await component.InitializeDefaults();
            if (component is CommunicationComponentBase)
                communicationComponents.Add(component as CommunicationComponentBase);
            return component;
        }

        public string ComponentName { get { return this.componentName; } }

        public abstract Task ProcessCommand(Controllable sender, string[] commands);

        public abstract Task ComponentHelp(Controllable sender);

        public static string ResolveParameter(string[] parameterArray, int index)
        {
            if (null != parameterArray && parameterArray.Length > index && parameterArray[index] != null)
            {
                return parameterArray[index];
            }
            return string.Empty;
        }

        #region Text
        protected static async Task HandleInput(Controllable sender, StringMessageReceivedEventArgs stringMessage)
//        protected static async Task HandleInput(Controllable sender, string input)
        {
            string input = stringMessage.Message;
            string[] commands = input.Split(':', ' ');
            string component = ResolveParameter(commands, 0).ToUpperInvariant();

            if (components.Keys.Contains(component))
            {
                try
                {
                    Controllable processor = components[component] as Controllable;
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

        protected static async Task HandleOutput(Controllable sender, string text)
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

        protected static Task HandleOutput(Controllable sender, JsonObject json)
        {
            throw new NotImplementedException();
            //if (null != dataPortInstance)
            //    dataPortInstance.WriteLine(text);
        }

        protected static void HandleOutput(Controllable sender, byte[] data)
        {
            throw new NotImplementedException();
            //if (null != dataPortInstance)
            //    dataPortInstance.WriteLine(text);
        }

        protected static void HandleOutput(Controllable sender, IRandomAccessStream stream)
        {
            throw new NotImplementedException();
            //if (null != dataPortInstance)
            //    dataPortInstance.WriteLine(text);
        }
        protected static void HandleOutput(Controllable sender, IBuffer data)
        {
            throw new NotImplementedException();
            //if (null != dataPortInstance)
            //    dataPortInstance.WriteLine(text);
        }

        public static async Task Echo(Controllable sender, string input)
        {
            await HandleOutput(sender, input);
        }

        public static async Task Hello(Controllable sender)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("HELLO. Great to see you here.");
            builder.Append(Environment.NewLine);
            builder.Append("Use 'HELP + CRLF' command to get help.");
            builder.Append(Environment.NewLine);
            await HandleOutput(sender, builder.ToString());
        }

        public static async Task ListHelp(Controllable sender)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("HELP :  Shows this help screen.");
            builder.Append(Environment.NewLine);
            builder.Append("LIST : Lists the available modules.");
            builder.Append(Environment.NewLine);
            builder.Append("HELLO : Returns a simple greeting message. Useful to test communication channel.");
            builder.Append(Environment.NewLine);
            builder.Append("ECHO : Echos any text following the ECHO command.");
            builder.Append(Environment.NewLine);
            builder.Append("EXIT|CLOSE : Closes the currently used channel.");
            builder.Append(Environment.NewLine);
            await HandleOutput(sender, builder.ToString());
        }

        public static async Task CloseChannel(Controllable sender)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("BYE. Hope to see you soon again.");
            builder.Append(Environment.NewLine);
            await HandleOutput(sender, builder.ToString());
            if (sender is ChannelHolder)
                await (sender as ChannelHolder).Channel.Close().ConfigureAwait(false);

        }

        public static async Task ListComponents(Controllable sender)
        {
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, Controllable> item in components)
            {
                builder.Append(item.Key);
                builder.Append(Environment.NewLine);
            }
            await HandleOutput(sender, builder.ToString());
        }

        #endregion

        protected static async Task HandleInput(Controllable sender, JsonMessageReceivedEventArgs jsonMessage)
        {
            await Task.CompletedTask;
        }

    }
}
