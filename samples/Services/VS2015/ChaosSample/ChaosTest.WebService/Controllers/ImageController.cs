//-----------------------------------------------------------------------
// <copyright file="ImageController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ChaosTest.WebService.Controllers
{
    using System;
    using ChaosTest.WebService.Extensions;
    using System.Net.Http;
    using System.Web.Http;

    /// <summary>
    /// Controller that serves up JavaScript files from the Scripts directory that are included as embedded assembly resources.
    /// You can also use the FileSystem and StaticFile middleware for OWIN to render script files,
    /// or wait for ASP.NET vNext when the full MVC stack will be available for self-hosting.
    /// </summary>
    public class ImageController : ApiController
    {
        /// <summary>
        /// Renders javascript files in the Scripts directory.
        /// </summary>
        /// <returns>HttpResponseMessage</returns>
        [HttpGet]
        public HttpResponseMessage Get(string name)
        {
            return this.View("ChaosTest.WebService.Images." + name, "Image/jpeg");
        }
    }
}
