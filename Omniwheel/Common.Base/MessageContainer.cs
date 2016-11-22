using System;
using System.Collections.Generic;
using Windows.Data.Json;

namespace Common.Base
{
    public class MessageContainer
    {
        public enum FixedPropertyNames
        {
            Target,
            Parameters,
            Responses,
        }

        private JsonObject dataObject;
        private StringJsonCollection parameters; //shortcut reference to minimize lookups
        private StringJsonCollection responses; //shortcut reference to minimize lookups

        #region public properties
        public Guid SessionId { get; private set; }

        public string Target
        {
            get { return dataObject.GetNamedString(FixedPropertyNames.Target.ToString()); }
        }

        public Controllable Origin { get; private set; }

        public StringJsonCollection Parameters { get { return parameters; } }

        public StringJsonCollection Responses { get { return responses; } }

        public IJsonValue GetValueByName(string name)
        {
            return dataObject.GetNamedValue(name, JsonValue.CreateNullValue());
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
            dataObject.AddValue(FixedPropertyNames.Target.ToString(), data[0]);
            parameters = new StringJsonCollection(data);
            dataObject.Add(FixedPropertyNames.Parameters.ToString(), parameters.JsonArray);
            responses = new StringJsonCollection();
            dataObject.Add(FixedPropertyNames.Responses.ToString(), responses.JsonArray);
        }

        public MessageContainer(Guid sessionId, Controllable origin, JsonObject data) : this(sessionId, origin)
        {
            this.dataObject = data;
            //dataObject.AddValue(FixedPropertyNames.Target.ToString(), data[0]);
            if (!data.ContainsKey(FixedPropertyNames.Parameters.ToString()))
            {
                parameters = new StringJsonCollection();
                dataObject.Add(FixedPropertyNames.Parameters.ToString(), parameters.JsonArray);
            }
            if (!data.ContainsKey(FixedPropertyNames.Responses.ToString()))
            {
                responses = new StringJsonCollection();
                dataObject.Add(FixedPropertyNames.Responses.ToString(), responses.JsonArray);
            }
        }
        #endregion

        public JsonObject Data { get { return this.dataObject; } }

        public JsonObject GetJson()
        {
            return this.dataObject;
        }

        public IList<string> GetText()
        {
            List<string> result = new List<string>();
            foreach(string item in responses)
            {
                result.Add(item);
            }
            return result;
        }
    }
}
