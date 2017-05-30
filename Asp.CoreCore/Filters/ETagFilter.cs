using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace WebApiPattern.Asp.CoreCore.Filters
{
    /// <summary>
    /// Created an ETag filter that does 2 things.
    /// 
    /// Generate an ETag response header with an ETag
    /// Compares a request If-None-Match header with the ETag. If they are the same, return a Not Modified status code.
    /// </summary>
    public class ETagFilter : Attribute, IActionFilter
    {
        private readonly int[] _statusCodes;

        public ETagFilter(params int[] statusCodes)
        {
            _statusCodes = statusCodes;
            if (statusCodes.Length == 0) _statusCodes = new[] { 200 };
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.HttpContext.Request.Method != "GET") return;
            if (!_statusCodes.Contains(context.HttpContext.Response.StatusCode)) return;

            var eString = context.HttpContext.Request.Path.ToString() + JsonConvert.SerializeObject(context.Result);
            var etag = GenerateETag(Encoding.UTF8.GetBytes(eString));

            if (context.HttpContext.Request.Headers.Keys.Contains("If-None-Match") && context.HttpContext.Request.Headers["If-None-Match"].ToString() == etag)
            {
                context.Result = new StatusCodeResult(304);
            }
            context.HttpContext.Response.Headers.Add("ETag", new[] { etag });
        }
        
        private static string GenerateETag(byte[] data)
        {
            string ret;
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(data);
                var hex = BitConverter.ToString(hash);
                ret = hex.Replace("-", "");
            }
            return ret;
        }
        


    }
}