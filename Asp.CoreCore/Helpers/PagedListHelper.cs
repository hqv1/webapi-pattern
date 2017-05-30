using System.Collections.Generic;
using Hqv.CSharp.Common.Extensions;
using Hqv.CSharp.Common.Web.Api;
using Microsoft.AspNetCore.Mvc;

namespace WebApiPattern.Asp.CoreCore.Helpers
{
    /// <summary>
    /// Helps create a string for X-PAGINATION header that contains various paging information
    /// </summary>
    public class PagedListHelper : IPagedListHelper
    {
        private enum ResourceUriType
        {
            PreviousPage,
            NextPage,
            Current
        }

        private readonly IUrlHelper _urlHelper;

        public PagedListHelper(IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper;
        }

        public string AddPaginationMetadataToResponseHeader<T>(PagedList<T> items, 
            ResourceParameters resourceParameters,
            object filterParameters,
            string routeName)
        {
            var filters = filterParameters.AsDictionary();
            
            var previousPageLink = items.HasPrevious
                ? CreateProductsResourceUri(resourceParameters, filters, ResourceUriType.PreviousPage, routeName)
                : null;
            var nextPageLink = items.HasNext
                ? CreateProductsResourceUri(resourceParameters, filters, ResourceUriType.NextPage, routeName)
                : null;

            var paginationMetadata = new
            {
                previousPageLink,
                nextPageLink,
                totalCount = items.TotalCount,
                pageSize = items.PageSize,
                currentPage = items.CurrentPage,
                totalPages = items.TotalPages
            };
            return Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata);
        }

        private string CreateProductsResourceUri(ResourceParameters resourceParameters, 
            IDictionary<string, object> filterParameters,
            ResourceUriType type, string routeName)
        {
            var filterParams = filterParameters ?? new Dictionary<string, object>();
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link(routeName, new Dictionary<string, object>(filterParams)
                    {
                        ["fields"] = resourceParameters.Fields,
                        ["orderBy"] = resourceParameters.OrderBy,
                        ["searchQuery"] = resourceParameters.SearchQuery,
                        ["pageNumber"] = resourceParameters.PageNumber - 1,
                        ["pageSize"] = resourceParameters.PageSize
                    });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link(routeName, new Dictionary<string, object>(filterParams)
                    {
                        ["fields"] = resourceParameters.Fields,
                        ["orderBy"] = resourceParameters.OrderBy,
                        ["searchQuery"] = resourceParameters.SearchQuery,
                        ["pageNumber"] = resourceParameters.PageNumber + 1,
                        ["pageSize"] = resourceParameters.PageSize
                    });
                case ResourceUriType.Current:
                default:
                    return _urlHelper.Link(routeName, new Dictionary<string, object>(filterParams)
                    {
                        ["fields"] = resourceParameters.Fields,
                        ["orderBy"] = resourceParameters.OrderBy,
                        ["searchQuery"] = resourceParameters.SearchQuery,
                        ["pageNumber"] = resourceParameters.PageNumber - 1,
                        ["pageSize"] = resourceParameters.PageSize
                    });
            }
        }
    }
}