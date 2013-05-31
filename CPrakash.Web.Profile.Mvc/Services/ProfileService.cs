using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CPrakash.Web.Profile.Mvc.Services
{
    /// <summary>
    /// ProfileService
    /// </summary>
    public class ProfileService : IProfileService
    {
        #region IProfileService Implementation

        /// <summary>
        /// SetPropertyValues
        /// </summary>
        /// <param name="userName">User Name</param>
        /// <param name="properties">Profile Properties</param>
        public void SetPropertyValues(string userName, Dictionary<string, object> properties)
        {
            XElement el = Serialize(properties);
            using (TextWriter textWriter = new StreamWriter(HttpContext.Current.Server.MapPath(string.Format("~/Files/{0}.xml", userName))))
            {
                el.Save(textWriter);
            }
        }

        /// <summary>
        /// GetPropertyValues
        /// </summary>
        /// <param name="userName">User Name</param>
        /// <returns></returns>
        public Dictionary<string, object> GetPropertyValues(string userName)
        {
            using (TextReader textReader = new StreamReader(HttpContext.Current.Server.MapPath(string.Format("~/Files/{0}.xml", userName))))
            {
                var el = XElement.Load(textReader, LoadOptions.None);
                return Deserialize(el);
            }
        }

        #endregion

        #region Deserialization

        private Dictionary<string, object> Deserialize(XElement pElement)
        {
            var objects = new Dictionary<string, object>();
            foreach(var xNode in pElement.Elements())
            {
                string name = xNode.Name.LocalName;
                object instance = GetObject(xNode);
                if (!objects.ContainsKey(name) && !string.IsNullOrWhiteSpace(name))
                    objects.Add(name, instance);
            }
            return objects;
        }

        private object DeserializeComplexType(XElement xNode, object parentInstance)
        {
            var props = parentInstance.GetType().GetProperties();
            foreach (var xChildNode in xNode.Elements())
            {
                string name = xChildNode.Name.LocalName;
                object instance = GetObject(xChildNode);
                var prop = props.Where(x => x.Name == name).FirstOrDefault();
                if (prop != null) prop.SetValue(parentInstance, instance, null);
            }
            return parentInstance;
        }

        private object GetObject(XElement xNode)
        {
            var runtimeType = this.GetObjectType(xNode);
            return GetObjectValue(runtimeType, xNode);
        }

        private Type GetObjectType(XElement xNode)
        {
            string type = "System.String";
            if (xNode.HasAttributes)
            {
                type = xNode.Attribute(XName.Get("type", string.Empty)).Value;
            }
            return Type.GetType(type);
        }

        private object GetObjectValue(Type runtimeType, XElement xNode)
        {
            object instance;
            if (runtimeType.IsSimpleType())
                return xNode.Value as object;

            instance = (object)Activator.CreateInstance(runtimeType);
            return DeserializeComplexType(xNode, instance);
        }

        #endregion

        #region Serialization

        private XElement Serialize(Dictionary<string, object> properties)
        {
            XElement el = new XElement("profile",
            properties.Select(kv => new XElement(kv.Key, 
                                                kv.Value.GetType().IsSimpleType() ? kv.Value as object : this.SerializeComplexType(kv.Value),
                                                new XAttribute("type", kv.Value.GetType().FullName))));

            return el;
        }

        private IEnumerable<XElement> SerializeComplexType(object childValue)
        {
            var type = childValue.GetType();
            var props = type.GetProperties();

            var elements = from prop in props
                           let name = XmlConvert.EncodeName(prop.Name)
                           let val = prop.PropertyType.IsArray ? "array" : prop.GetValue(childValue, null)
                           let value = prop.PropertyType.IsArray ? GetArrayElement(prop, (Array)prop.GetValue(childValue, null)) : (prop.PropertyType.IsSimpleType() ? new XElement(name, val, new XAttribute("type", val.GetType().FullName)) : new XElement(name, this.SerializeComplexType(val), new XAttribute("type", val.GetType().FullName)))
                           where value != null
                           select value;

            return elements;
        }

        /// <summary>
        /// Gets the array element.
        /// </summary>
        /// <param name="info">The property info.</param>
        /// <param name="input">The input object.</param>
        /// <returns>Returns an XElement with the array collection as child elements.</returns>
        internal XElement GetArrayElement(PropertyInfo info, Array input)
        {
            return GetArrayElement(info.Name, input);
        }

        /// <summary>
        /// Gets the array element.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="input">The input object.</param>
        /// <returns>Returns an XElement with the array collection as child elements.</returns>
        internal XElement GetArrayElement(string propertyName, Array input)
        {
            var name = XmlConvert.EncodeName(propertyName);

            XElement rootElement = new XElement(name);

            var arrayCount = input.GetLength(0);

            for (int i = 0; i < arrayCount; i++)
            {
                var val = input.GetValue(i);
                XElement childElement = val.GetType().IsSimpleType() ? new XElement(name + "Child", val) : new XElement(name, SerializeComplexType(val));

                rootElement.Add(childElement);
            }

            return rootElement;
        }

        #endregion
    }

    /// <summary>
    /// Extension methods for the dynamic object
    /// This is taken from - http://martinnormark.com/serialize-c-dynamic-and-anonymous-types-to-xml/
    /// </summary>
    public static class DynamicHelper
    {
        /// <summary>
        /// Defines the simple types that is directly writeable to XML.
        /// </summary>
        private static readonly Type[] _writeTypes = new[] { typeof(string), typeof(DateTime), typeof(Enum), typeof(decimal), typeof(Guid) };

        /// <summary>
        /// Determines whether [is simple type] [the specified type].
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>
        /// 	<c>true</c> if [is simple type] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSimpleType(this Type type)
        {
            return type.IsPrimitive || _writeTypes.Contains(type);
        }
    }
}