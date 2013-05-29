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
    public class ProfileService : IProfileService
    {
        public Dictionary<string, object> GetPropertyValues(string userName)
        {
            Dictionary<string, object> objects = new Dictionary<string, object>();
            var stack = new Stack<object>();

            using (TextReader textReader = new StreamReader(HttpContext.Current.Server.MapPath(string.Format("~/Files/{0}.xml", userName))))
            {
                //var profileXml = textReader.ReadToEnd();
                var el = XElement.Load(textReader, LoadOptions.None);

                using (var xReader = el.CreateReader())
                {
                    xReader.MoveToContent();
                    string value = string.Empty;
                    string name = string.Empty;
                    object instance = string.Empty as object;
                    while (xReader.Read())
                    {
                        switch (xReader.NodeType)
                        {
                            case XmlNodeType.Element:
                                name = xReader.Name;
                                string type = "System.String";
                                string module = string.Empty;
                                if (xReader.HasAttributes)
                                {
                                    xReader.MoveToAttribute("type");
                                    type = xReader.Value;
                                    xReader.MoveToAttribute("module");
                                    module = xReader.Value;
                                }
                                var runtimeType = Type.GetType(type);
                                if (!runtimeType.IsSimpleType())
                                {
                                    instance = (object)Activator.CreateInstance(runtimeType);
                                    FillChildObject(xReader, instance);
                                }
                                
                                break;
                            case XmlNodeType.EndElement:
                                if (!objects.ContainsKey(name) && !string.IsNullOrWhiteSpace(name))
                                    objects.Add(name, instance);

                                name = module = type = value = string.Empty;
                                break;
                            case XmlNodeType.Text:
                                value = xReader.Value;
                                if (!string.IsNullOrWhiteSpace(value))
                                    instance = value;
                                break;
                            case XmlNodeType.Whitespace: break;
                            default: break;
                        }
                    }
                }
            }
            return objects;
        }

        private void FillChildObject(XmlReader xReader, object parentInstace)
        {
            string value = string.Empty;
            string name = string.Empty;
            object instance = string.Empty as object;
            var props = parentInstace.GetType().GetProperties();
            int index = 0;
            while (xReader.Read())
            {
                switch (xReader.NodeType)
                {
                    case XmlNodeType.Element:
                        name = xReader.Name;
                        string type = "System.String";
                        string module = string.Empty;
                        if (xReader.HasAttributes)
                        {
                            xReader.MoveToAttribute("type");
                            type = xReader.Value;
                            xReader.MoveToAttribute("module");
                            module = xReader.Value;
                        }
                        var runtimeType = Type.GetType(type);
                        if (!runtimeType.IsSimpleType())
                        {
                            instance = (object)Activator.CreateInstance(runtimeType);
                            FillChildObject(xReader, instance);
                        }

                        break;
                    case XmlNodeType.EndElement:
                        foreach(var prop in props)
                        {
                            if (prop.Name == name)
                            {
                                prop.SetValue(parentInstace, value, null);
                                break;
                            }
                        }
                        index++;
                        if (props.Count() == index)
                            return;
                        name = module = type = value = string.Empty;
                        break;
                    case XmlNodeType.Text:
                        value = xReader.Value;
                        if (!string.IsNullOrWhiteSpace(value))
                            instance = value;
                        break;
                    case XmlNodeType.Whitespace: break;
                    default: break;
                }
            }
        }

        public void SetPropertyValues(string userName, Dictionary<string, object> properties)
        {
            XElement el = GetXml(properties);
            TextWriter textWriter = new StreamWriter(HttpContext.Current.Server.MapPath(string.Format("~/Files/{0}.xml", userName)));
            el.Save(textWriter);
            textWriter.Close();
        }

        private XElement GetXml(Dictionary<string, object> properties)
        {
            XElement el = new XElement("profile",
            properties.Select(kv => new XElement(kv.Key, 
                                                kv.Value.GetType().IsSimpleType() ? kv.Value as object : this.GetChildXml(kv.Value),
                                                new XAttribute("type", kv.Value.GetType().FullName), new XAttribute("module", kv.Value.GetType().Assembly.FullName))));

            return el;
        }

        private IEnumerable<XElement> GetChildXml(object childValue)
        {
            var type = childValue.GetType();
            var props = type.GetProperties();

            var elements = from prop in props
                           let name = XmlConvert.EncodeName(prop.Name)
                           let val = prop.PropertyType.IsArray ? "array" : prop.GetValue(childValue, null)
                           let value = prop.PropertyType.IsArray ? DynamicHelper.GetArrayElement(prop, (Array)prop.GetValue(childValue, null)) : (prop.PropertyType.IsSimpleType() ? new XElement(name, val, new XAttribute("type", val.GetType().FullName), new XAttribute("module", val.GetType().Assembly.FullName)) : new XElement(name, this.GetChildXml(val), new XAttribute("type", val.GetType().FullName), new XAttribute("module", val.GetType().Assembly.FullName)))
                           where value != null
                           select value;

            return elements;
        }
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

        /// <summary>
        /// Converts the specified dynamic object to XML.
        /// </summary>
        /// <param name="dynamicObject">The dynamic object.</param>
        /// <returns>Returns an Xml representation of the dynamic object.</returns>
        public static XElement ConvertToXml(dynamic dynamicObject)
        {
            return ConvertToXml(dynamicObject, null);
        }

        /// <summary>
        /// Converts the specified dynamic object to XML.
        /// </summary>
        /// <param name="dynamicObject">The dynamic object.</param>
        /// /// <param name="element">The element name.</param>
        /// <returns>Returns an Xml representation of the dynamic object.</returns>
        public static XElement ConvertToXml(dynamic dynamicObject, string element)
        {
            if (String.IsNullOrWhiteSpace(element))
            {
                element = "object";
            }

            element = XmlConvert.EncodeName(element);
            var ret = new XElement(element);

            Dictionary<string, object> members = new Dictionary<string, object>(dynamicObject);

            var elements = from prop in members
                           let name = XmlConvert.EncodeName(prop.Key)
                           let val = prop.Value.GetType().IsArray ? "array" : prop.Value
                           let value = prop.Value.GetType().IsArray ? GetArrayElement(prop.Key, (Array)prop.Value) : (prop.Value.GetType().IsSimpleType() ? new XElement(name, val) : val.ToXml(name))
                           where value != null
                           select value;

            ret.Add(elements);

            return ret;
        }

        /// <summary>
        /// Generates an XML string from the dynamic object.
        /// </summary>
        /// <param name="dynamicObject">The dynamic object.</param>
        /// <returns>Returns an XML string.</returns>
        public static string ToXmlString(dynamic dynamicObject)
        {
            XElement xml = DynamicHelper.ConvertToXml(dynamicObject);

            return xml.ToString();
        }

        /// <summary>
        /// Converts an anonymous type to an XElement.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Returns the object as it's XML representation in an XElement.</returns>
        public static XElement ToXml(this object input)
        {
            return input.ToXml(null);
        }

        /// <summary>
        /// Converts an anonymous type to an XElement.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="element">The element name.</param>
        /// <returns>Returns the object as it's XML representation in an XElement.</returns>
        public static XElement ToXml(this object input, string element)
        {
            if (input == null)
            {
                return null;
            }

            if (String.IsNullOrWhiteSpace(element))
            {
                element = "object";
            }

            element = XmlConvert.EncodeName(element);
            var ret = new XElement(element);

            if (input != null)
            {
                var type = input.GetType();
                var props = type.GetProperties();

                var elements = from prop in props
                               let name = XmlConvert.EncodeName(prop.Name)
                               let val = prop.PropertyType.IsArray ? "array" : prop.GetValue(input, null)
                               let value = prop.PropertyType.IsArray ? GetArrayElement(prop, (Array)prop.GetValue(input, null)) : (prop.PropertyType.IsSimpleType() ? new XElement(name, val) : val.ToXml(name))
                               where value != null
                               select value;

                ret.Add(elements);
            }

            return ret;
        }

        /// <summary>
        /// Parses the specified XML string to a dynamic.
        /// </summary>
        /// <param name="xmlString">The XML string.</param>
        /// <returns>A dynamic object.</returns>
        public static dynamic ParseDynamic(this string xmlString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the array element.
        /// </summary>
        /// <param name="info">The property info.</param>
        /// <param name="input">The input object.</param>
        /// <returns>Returns an XElement with the array collection as child elements.</returns>
        internal static XElement GetArrayElement(PropertyInfo info, Array input)
        {
            return GetArrayElement(info.Name, input);
        }

        /// <summary>
        /// Gets the array element.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="input">The input object.</param>
        /// <returns>Returns an XElement with the array collection as child elements.</returns>
        internal static XElement GetArrayElement(string propertyName, Array input)
        {
            var name = XmlConvert.EncodeName(propertyName);

            XElement rootElement = new XElement(name);

            var arrayCount = input.GetLength(0);

            for (int i = 0; i < arrayCount; i++)
            {
                var val = input.GetValue(i);
                XElement childElement = val.GetType().IsSimpleType() ? new XElement(name + "Child", val) : val.ToXml();

                rootElement.Add(childElement);
            }

            return rootElement;
        }
    }

}