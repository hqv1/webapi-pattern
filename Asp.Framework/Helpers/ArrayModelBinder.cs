using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace WebApiPattern.Asp.Framework.Helpers
{
    /// <summary>
    /// Converts a query string value into a list of Ts.
    /// 
    /// For example if the user enters http://www.test.com/api/productcontrollers/(1,3) it'll 
    /// convert the (1,3) to a List of ints. 
    /// </summary>
    public class ArrayModelBinder : IModelBinder
    {
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            // Binder works only on enumerable types
            //if (!bindingContext.ModelMetadata.IsEnumerableType)
            //{
            //    return false;
            //}

            // Get the inputted value through the value provider
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).AttemptedValue;
            value = value.Replace("(", "").Replace(")", "");

            // If that value is null or whitespace, we return null
            if (string.IsNullOrWhiteSpace(value))
            {
                bindingContext.Model = null;
                return true;
            }

            // The value isn't null or whitespace, 
            // and the type of the model is enumerable. 
            // Get the enumerable's type, and a converter 
            var elementType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0];
            var converter = TypeDescriptor.GetConverter(elementType);

            // Convert each item in the value list to the enumerable type
            var values = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => converter.ConvertFromString(x.Trim()))
                .ToArray();

            // Create an array of that type, and set it as the Model value 
            var typedValues = Array.CreateInstance(elementType, values.Length);
            values.CopyTo(typedValues, 0);
            bindingContext.Model = typedValues;
            return true;
        }        
    }
}
