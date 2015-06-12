using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;

namespace ShowMeAnExampleOfAutomapperFromLinkedSource
{
    public static class DynamicExtensions {

        //Inspired from http://stackoverflow.com/questions/13651190/how-to-extend-an-existing-object-in-c-sharp-4-0-using-dynamics
        public static IDictionary<string, object> ToPropertyDictionary(this object value) {
            IDictionary<string, object> propertyDictionnary = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                propertyDictionnary.Add(property.Name, property.GetValue(value));

            return propertyDictionnary;
        }

        public static dynamic ToDynamic(this object value) {
            return value.ToPropertyDictionary() as ExpandoObject;
        }
    }
}