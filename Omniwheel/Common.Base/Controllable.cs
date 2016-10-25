using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Common.Base.Categories;

namespace Common.Base
{
    public abstract class Controllable
    {
        protected string componentName;

        protected static Dictionary<string, Controllable> components = new Dictionary<string, Controllable>();
        protected static List<CommunicationControllable> communicationComponents;

        #region static
        static Controllable()
        {
            communicationComponents = new List<CommunicationControllable>();
        }

        public static async Task<Controllable> RegisterComponent(Controllable component)
        {
            components.Add(component.componentName.ToUpperInvariant(), component);
            await component.InitializeDefaults().ConfigureAwait(false);
            if (component is CommunicationControllable)
                communicationComponents.Add(component as CommunicationControllable);
            return component;
        }

        public static Controllable GetByName(string name)
        {
            name = name?.ToUpperInvariant();
            if (components.ContainsKey(name))
                return components[name] as Controllable;
            return null;
        }

        protected static string ResolveParameter(MessageContainer data, int index)
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
                        await ListHelp(data).ConfigureAwait(false);
                        break;
                    case "HELLO":
                        await ControllableHello(data).ConfigureAwait(false);
                        break;
                    case "LIST":
                        await ControllableListComponents(data).ConfigureAwait(false);
                        break;
                    case "ECHO":
                        await ControllableEcho(data).ConfigureAwait(false);
                        break;
                    case "BYE":
                    case "EXIT":
                    case "CLOSE":
                        await ControllableCloseChannel(data).ConfigureAwait(false);
                        break;
                    default:
                        Debug.WriteLine("{0} :: Nothing to do for '{1}'", component, data.Target);
                        break;
                }
            }
        }

        protected static async Task HandleOutput(MessageContainer data)
        {
            List<Task> sendTasks = new List<Task>();
            foreach (CommunicationControllable publisher in communicationComponents)
            {
                sendTasks.Add(publisher.Send(data));
            }
            await Task.WhenAll(sendTasks).ConfigureAwait(false);
        }

        private static async Task ControllableEcho(MessageContainer data)
        {
            data.Responses.Add(data.Parameters.ToList());
            await HandleOutput(data).ConfigureAwait(false);
        }

        private static async Task ControllableHello(MessageContainer data)
        {
            data.Responses.Add("HELLO. Great to see you here.");
            data.Responses.Add("Use 'HELP + CRLF' command to get help.");
            await HandleOutput(data).ConfigureAwait(false);
        }

        public static async Task ListHelp(MessageContainer data)
        {
            data.Responses.Add("HELP : Shows this help screen.");
            data.Responses.Add("LIST : Lists the available modules.");
            data.Responses.Add("HELLO : Returns a simple greeting message. Useful to test communication channel.");
            data.Responses.Add("ECHO : Echos any text following the ECHO command.");
            data.Responses.Add("EXIT|CLOSE : Closes the currently used channel.");
            await HandleOutput(data).ConfigureAwait(false);
        }

        private static async Task ControllableCloseChannel(MessageContainer data)
        {
            data.Responses.Add("BYE. Hope to see you soon again.");
            await HandleOutput(data);
            if (data.Origin is CommunicationControllable)
                await CloseChannel(data.Origin as CommunicationControllable, data.SessionId).ConfigureAwait(false);
        }

        private static async Task ControllableListComponents(MessageContainer data)
        {
            data.Responses.Add(await ListComponents().ConfigureAwait(false));
            await HandleOutput(data).ConfigureAwait(false);
        }

        public static async Task CloseChannel(CommunicationControllable channel, Guid sessionId)
        {
            await channel.CloseChannel(sessionId).ConfigureAwait(false);
        }

        public static async Task<IList<string>> ListComponents()
        {
            return await Task.Run(() => components.Keys.ToList()).ConfigureAwait(false);
        }
        #endregion

            #region base instance
        public Controllable(string componentName)
        {
            this.componentName = componentName;
        }

        protected async virtual Task InitializeDefaults()
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }

        public string ComponentName { get { return this.componentName; } }

        protected abstract Task ProcessCommand(MessageContainer data);

        protected abstract Task ComponentHelp(MessageContainer data);
        #endregion


    }
}
