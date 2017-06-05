using System;
using System.Collections.Generic;
using System.Linq;
using Hqv.CSharp.Common.Extensions;
using WebApiPattern.Domain.Entities;

namespace WebApiPattern.Asp.Shared.Ordering
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private readonly IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();

        private readonly Dictionary<string, PropertyMappingValue> _productMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {                
                {"Id", new PropertyMappingValue(new []{"ID"}) },
                {"Vendor", new PropertyMappingValue(new []{"Vendor"}) },
                {"Name", new PropertyMappingValue(new []{"Name"}) }
            };

        public PropertyMappingService()
        {
            _propertyMappings.Add(new PropertyMapping<ProductEntity, Data.Sqlite.Models.ProductModel>(_productMapping));
        }

        public IDictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            var matchMapping = _propertyMappings.OfType<PropertyMapping<TSource, TDestination>>().ToList();
            if (matchMapping.Count == 1)
            {
                return matchMapping.First().MappingDictionary;
            }

            throw new Exception($"Cannot find exact property mapping instance for <{typeof(TDestination)}>");
        }

        public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                return true;
            }

            // the string is separated by ",", so we split it.
            var fieldsAfterSplit = fields.Split(',');

            // run through the fields clauses
            foreach (var field in fieldsAfterSplit)
            {
                // trim
                var trimmedField = field.Trim();

                // remove everything after the first " " - if the fields 
                // are coming from an orderBy string, this part must be 
                // ignored
                var indexOfFirstSpace = trimmedField.IndexOf(" ", StringComparison.Ordinal);
                var propertyName = indexOfFirstSpace == -1 ?
                    trimmedField : trimmedField.Remove(indexOfFirstSpace);

                // find the matching property
                if (!propertyMapping.ContainsKey(propertyName))
                {
                    return false;
                }
            }
            return true;
        }
    }
}