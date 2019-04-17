using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CommonUtils
{
  public static class HttpRequestHelper
    {
        /// <summary>
        /// post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData">post数据</param>
        /// <returns></returns>
        public static string PostResponse(string url, string postData, out string statusCode)
        {
            //if (url.StartsWith("https"))
            //    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

            HttpContent httpContent = new StringContent(postData);

            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json")
            {
                CharSet = "utf-8"
            };

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Method", "Post");
            httpClient.DefaultRequestHeaders.Add("UserAgent",
              "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36");
            httpClient.DefaultRequestHeaders.Add("KeepAlive", "false");

            HttpResponseMessage response = httpClient.PostAsync(url, httpContent).Result;

            statusCode = response.StatusCode.ToString();
            //LogHelper.WriteLog("statusCode" + statusCode);

            //LogHelper.WriteLog("Result" + response.Content.ReadAsStringAsync().Result);
            while (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                return result;
            }
            return null;
        }
    }
}
