using System;
using System.Collections.Generic;
using System.Linq;
using Library.API.Entities;
using Library.API.Models;

namespace Library.API.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private readonly Dictionary<string, PropertyMappingValue> _authorPropertyMapping = new 
            Dictionary<string, PropertyMappingValue> (StringComparer.OrdinalIgnoreCase)
        { 
            { "Id", new PropertyMappingValue (new List<string> () { "Id" }) }, 
            { "MainCategory", new PropertyMappingValue (new List<string> () { "MainCategory" }) }, 
            { "Age", new PropertyMappingValue (new List<string> () { "DateOfBirth" }, true) }, 
            { "Name", new PropertyMappingValue (new List<string> () { "FirstName", "LastName" }) }
        };
        private readonly IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping> ();
        public PropertyMappingService ()
        {
            _propertyMappings.Add (new PropertyMapping<AuthorDto, Author> (_authorPropertyMapping));
        }

        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination> ()
        {
            // get matching mapping
            var matchingMapping = _propertyMappings.OfType<PropertyMapping<TSource, TDestination>> ();

            if (matchingMapping.Count () == 1)
            {
                return matchingMapping.First ().MappingDictionary;
            }

            throw new Exception ($"Cannot find exact property mapping instance " +
                $"for <{typeof(TSource)},{typeof(TDestination)}");
        }

        public bool ValidMappingExistsFor<TSource, TDestination> (string fields)
        {
            throw new System.NotImplementedException ();
        }
    }
}
