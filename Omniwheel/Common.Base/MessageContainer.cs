using System;
using System.Collections.Generic;
using Windows.Data.Json;
using System.Linq;

namespace Common.Base
{
    public class MessageContainer
    {
        public enum FixedPropertyNames
        {
            Target,
            Action,
            Parameters,
            Responses,
        }

        private JsonObject dataObject;
        private StringJsonCollection parameters; //shortcut reference to minimize lookups

        #region public properties
        public Guid SessionId { get; private set; }

        public string Target
        {
            get { return dataObject.GetNamedString(FixedPropertyNames.Target.ToString()); }
        }
        public string Action
        {
            get { return dataObject.GetNamedString(FixedPropertyNames.Action.ToString()); }
        }

        public Controllable Origin { get; private set; }

        public StringJsonCollection Parameters { get { return parameters; } }

        public JsonObject JsonData { get { return this.dataObject; } }

        #endregion

        #region public methods
        public IJsonValue GetValueByName(string name)
        {
            return dataObject.GetNamedValue(name, JsonValue.CreateNullValue());
        }

        public void AddValue(string name, object value)
        {
            dataObject.AddValue(name, value);
        }

        public void AddMultiPartValue(string name, object value)
        {
            dataObject.AddMultiPartValue(name, value);
        }

        #endregion

        #region .ctor
        private MessageContainer(Guid sessionId, Controllable origin)
        {
            this.SessionId = sessionId;
            this.Origin = origin;
        }

        public MessageContainer(Guid sessionId, Controllable origin, string[] data) : this(sessionId, origin)
        {
            this.dataObject = new JsonObject();
            data = ResolveParameters(data);
            parameters = new StringJsonCollection(data);
            dataObject.Add(FixedPropertyNames.Parameters.ToString(), parameters.JsonArray);
        }

        private string[] ResolveParameters(string[] data)
        {
            List<string> result = new List<string>();
            for(int i = 0; i<data.Length; i++)
            {
                string item = data[i];
                string[] names = item.Split('=');
                if (names.Length == 2) //name-value pair
                {
                    dataObject.AddValue(names[0], names[1]);
                    result.Add(names[1]);
                }
                else
                {
                    if (i == 0)
                        dataObject.AddValue(FixedPropertyNames.Target.ToString(), item);
                    else if (i == 1)
                        dataObject.AddValue(FixedPropertyNames.Action.ToString(), item);
                    else
                    {
                        result.Add(item);
                    }
                }
            }
            return result.ToArray();
        }

        public MessageContainer(Guid sessionId, Controllable origin, JsonObject data) : this(sessionId, origin)
        {
            this.dataObject = data;
            if (data.ContainsKey(FixedPropertyNames.Parameters.ToString()))
            {
                parameters = new StringJsonCollection(data.GetNamedArray(FixedPropertyNames.Parameters.ToString()));
            }
            else
            {
                parameters = new StringJsonCollection();
                dataObject.Add(FixedPropertyNames.Parameters.ToString(), parameters.JsonArray);
            }
        }
        #endregion

        public JsonObject GetJson()
        {
            return this.dataObject;
        }

        public IList<string> GetText()
        {
            List<string> result = new List<string>();
            foreach (var item in this.dataObject)
            {
                if (Enum.GetNames(typeof(FixedPropertyNames)).Contains(item.Key))
                    continue;
                result.Add(item.Value.GetValueString());
            }
            return result;
        }

        public void PushParameters()
        {
            if (this.dataObject.ContainsKey(FixedPropertyNames.Action.ToString()))
            {
                IJsonValue value = dataObject.GetNamedValue(FixedPropertyNames.Action.ToString());
                parameters.Insert(0, value);
            }
            if (this.dataObject.ContainsKey(FixedPropertyNames.Target.ToString()))
            {
                IJsonValue value = dataObject.GetNamedValue(FixedPropertyNames.Target.ToString());
                dataObject.SetNamedValue(FixedPropertyNames.Action.ToString(), value);
            }
        }
    }
}
