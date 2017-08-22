﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Threax.AspNetCore.Halcyon.Client
{
    public class RawEndpointResult : IDisposable
    {
        public RawEndpointResult()
        {

        }

        public void Dispose()
        {
            Client?.Dispose();
            Request?.Dispose();
            FormData?.Dispose();
            Response?.Dispose();
        }

        public HttpClient Client { get; internal set; }

        public HttpRequestMessage Request { get; internal set; }

        public MultipartFormDataContent FormData { get; internal set; }

        public HttpResponseMessage Response { get; internal set; }
    }
}
