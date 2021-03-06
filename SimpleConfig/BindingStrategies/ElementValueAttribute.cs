﻿using System;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using SimpleConfig.Helpers;

namespace SimpleConfig.BindingStrategies
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ElementValueAttribute : BaseBindingAttribute
    {
        public ElementValueAttribute() { }

        public ElementValueAttribute(string elementName)
        {
            ElementName = elementName;
        }

        public string ElementName { get; }

        public override IBindingStrategy MappingStrategy => new ElementValueMappingStrategy(ElementName);
    }

    public class ElementValueMappingStrategy : IBindingStrategy
    {
        public ElementValueMappingStrategy(){}

        public ElementValueMappingStrategy(string elementName)
        {
            ElementName = elementName;
        }

        public string ElementName { get; }


        public bool Map(object destinationObject, PropertyInfo destinationProperty, XmlElement element, XmlElement allConfig, ConfigMapper mapper)
        {
            var childElement = element.GetElementNamed(ElementName ?? destinationProperty.Name);

            if (childElement == null)
            {
                return false;
            }

            var elementValue = childElement.InnerText;

            var destinationPropertyType = destinationProperty.PropertyType;

            if (destinationPropertyType.IsEnum)
            {
                var value = Enum.Parse(destinationPropertyType, elementValue);
                destinationProperty.SetValue(destinationObject, value, null);
                return true;
            }

            if (destinationPropertyType.IsNullable())
            {
                if (elementValue == "")
                {
                    destinationProperty.SetValue(destinationObject, null, null);
                    return true;
                }

                destinationPropertyType = destinationPropertyType.GetGenericArguments()[0];
            }

            if (destinationPropertyType.IsA<IConvertible>())
            {
                var value = Convert.ChangeType(elementValue, destinationPropertyType);
                destinationProperty.SetValue(destinationObject, value, null);
                return true;
            }

            var converter = TypeDescriptor.GetConverter(destinationPropertyType);

            if (converter.CanConvertFrom(typeof (string)))
            {
                var value = converter.ConvertFromString(elementValue);
                destinationProperty.SetValue(destinationObject, value, null);
                return true;
            }

            return false;
        }
    }
}