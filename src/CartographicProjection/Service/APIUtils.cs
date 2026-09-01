using System;
using System.Net.Http;

namespace NORCE.Drilling.CartographicProjection.Service
{
    public static class APIUtils
    {
        // API parameters
        public static readonly string HostNameGeodeticDatum = Configuration.GeodeticDatumHostURL!;
        public static readonly string HostBasePathGeodeticDatum = "GeodeticDatum/api/";
        public static readonly HttpClient HttpClientGeodeticDatum = SetHttpClient(HostNameGeodeticDatum, HostBasePathGeodeticDatum);
        public static readonly ModelShared.Client ClientGeodeticDatum = new ModelShared.Client(HttpClientGeodeticDatum.BaseAddress!.ToString(), HttpClientGeodeticDatum);

        // API utility methods
        public static HttpClient SetHttpClient(string host, string microServiceUri)
        {
            HttpClient httpClient = new()
            {
                BaseAddress = new Uri(new Uri(host.TrimEnd('/') + "/"), microServiceUri)
            };
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }
    }
}