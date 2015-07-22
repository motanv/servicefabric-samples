//-----------------------------------------------------------------------
// <copyright file="DefaultController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ChaosTest.WebService.Controllers
{
    using Common;
    using Extensions;
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;

    /// <summary>
    /// Default controller.
    /// </summary>
    public class DefaultController : ApiController
    {
        private readonly IAddressChangeNotifier addressResolver;

        public DefaultController(IAddressChangeNotifier addressResolver)
        {
            this.addressResolver = addressResolver;
        }

        [HttpGet]
        public HttpResponseMessage Index()
        {
            return this.View("ChaosTest.WebService.Views.Default.Index.html", "text/html");
        }

        [HttpPost]
        public Task<IHttpActionResult> Start()
        {
            return this.ServiceRequest(
                address =>
                {
                    HttpWebRequest request = WebRequest.CreateHttp(address + "/Start");
                    request.Method = "POST";
                    request.ContentLength = 0;
                    return request;
                },
                response => this.Ok());
        }

        [HttpPost]
        public Task<IHttpActionResult> Stop()
        {
            return this.ServiceRequest(
                address =>
                {
                    HttpWebRequest request = WebRequest.CreateHttp(address + "/Stop");
                    request.Method = "POST";
                    request.ContentLength = 0;
                    return request;
                },
                response => this.Ok());
        }

        [HttpGet]
        public Task<IHttpActionResult> Results()
        {
            return this.ServiceRequest(
                address =>
                {
                    HttpWebRequest request = WebRequest.CreateHttp(address + "/Results");
                    request.Method = "GET";
                    return request;
                },
                response =>
                {
                    Stream responseStream = response.GetResponseStream();
                    if (responseStream == null)
                    {
                        return this.Ok();
                    }

                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        return this.Ok(reader.ReadToEnd());
                    }
                });
        }

        private async Task<IHttpActionResult> ServiceRequest(Func<string, HttpWebRequest> createRequest, Func<HttpWebResponse, IHttpActionResult> createResponse)
        {
            for (int i = 0; i < Constants.ServiceRequestRetryCount; ++i)
            {
                string endpoint = await this.addressResolver.GetAddressAsync(CancellationToken.None);

                HttpWebRequest request = createRequest(endpoint);

                try
                {
                    using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                    {
                        if (createResponse != null)
                        {
                            return createResponse(response);
                        }
                    }
                }
                catch (WebException we)
                {
                    HttpWebResponse errorResponse = we.Response as HttpWebResponse;

                    if (we.Status == WebExceptionStatus.ProtocolError)
                    {
                        if (errorResponse != null)
                        {
                            int statusCode = (int) errorResponse.StatusCode;

                            if (statusCode == 404)
                            {
                                // this could either mean we requested an endpoint that does not exist in the service API (a user error)
                                // or the address that was resolved by fabric client is stale (transient runtime error) in which we should re-resolve.

                                continue;
                            }
                        }
                    }

                    if (we.Status == WebExceptionStatus.Timeout ||
                        we.Status == WebExceptionStatus.RequestCanceled ||
                        we.Status == WebExceptionStatus.ConnectionClosed ||
                        we.Status == WebExceptionStatus.ConnectFailure)
                    {
                        continue;
                    }

                    throw;
                }
                catch (HttpListenerException)
                {
                    ServiceEventSource.Current.Message("HttpListenerException inside ServiceRequest");
                }
            }

            return this.InternalServerError();
        }
    }
}
