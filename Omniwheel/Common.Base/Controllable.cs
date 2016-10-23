using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Common.Base.Communication;

namespace Common.Base
{
    public abstract class Controllable
    {
        protected string componentName;

        protected static Dictionary<string, Controllable> components = new Dictionary<string, Controllable>();
        protected static List<CommunicationControllable> communicationComponents;

        static Controllable()
        {
            communicationComponents = new List<CommunicationControllable>();
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
            if (component is CommunicationControllable)
                communicationComponents.Add(component as CommunicationControllable);
            return component;
        }

        public string ComponentName { get { return this.componentName; } }

        public abstract Task ProcessCommand(MessageContainer data);

        public abstract Task ComponentHelp(MessageContainer data);

        public static string ResolveParameter(MessageContainer data, int index)
        {
            return data.Parameters.GetAtAsString(index) ?? string.Empty;
        }

        protected static async Task HandleInput(MessageContainer data)
        {
            string component = data.Target?.ToUpperInvariant();
            if (string.IsNullOrEmpty(component))
                throw new ArgumentNullException();
            if (components.ContainsKey(component))
            {
                try
                {
                    Controllable processor = components[component] as Controllable;
                    await processor.ProcessCommand(data);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(data.Target, ex.Message + "::" + ex.StackTrace);
                }
            } 
            else //handle locally
            {
                switch (component)
                {
                    case "HELP":
                        await ListHelp(data);
                        break;
                    case "HELLO":
                        await Hello(data);
                        break;
                    case "LIST":
                        await ListComponents(data);
                        break;
                    case "ECHO":
                        await Echo(data);
                        break;
                    case "EXIT":
                    case "CLOSE":
                        await CloseChannel(data);
                        break;
                    default:
                        Debug.WriteLine("{0} :: Nothing to do for '{1}'", component, data.Target);
                        break;
                }
            }
        }

        #region Text
        protected static async Task HandleOutput(MessageContainer data)
        {
            List<Task> sendTasks = new List<Task>();
            foreach (CommunicationControllable publisher in communicationComponents)
            {
                sendTasks.Add(publisher.Send(data));
            }
            await Task.WhenAll(sendTasks).ConfigureAwait(false);
        }


        public static async Task Echo(MessageContainer data)
        {
            await HandleOutput(data);
        }

        public static async Task Hello(MessageContainer data)
        {
            data.Responses.Add("HELLO. Great to see you here.");
            data.Responses.Add("Use 'HELP + CRLF' command to get help.");
            await HandleOutput(data);
        }

        public static async Task ListHelp(MessageContainer data)
        {
            data.Responses.Add("HELP : Shows this help screen.");
            data.Responses.Add("LIST : Lists the available modules.");
            data.Responses.Add("HELLO : Returns a simple greeting message. Useful to test communication channel.");
            data.Responses.Add("ECHO : Echos any text following the ECHO command.");
            data.Responses.Add("EXIT|CLOSE : Closes the currently used channel.");
            await HandleOutput(data);
        }

        public static async Task CloseChannel(MessageContainer data)
        {
            data.Responses.Add("BYE. Hope to see you soon again.");
            await HandleOutput(data);
            if (data.Origin is CommunicationControllable)
                await (data.Origin as CommunicationControllable).Close(data).ConfigureAwait(false);
        }

        public static async Task ListComponents(MessageContainer data)
        {
            foreach (KeyValuePair<string, Controllable> item in components)
            {
                data.Responses.Add(item.Key);
            }
            await HandleOutput(data);
        }

        #endregion

    }
}
