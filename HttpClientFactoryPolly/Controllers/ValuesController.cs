using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HttpClientFactoryPolly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private static int myCount = 0;
        public ValuesController(IHttpClientFactory httpClientFactory)
        {
            this._httpClientFactory = httpClientFactory;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
            var client = _httpClientFactory.CreateClient();
            var result = await client.GetStringAsync("https://www.microsoft.com/zh-cn/");
            return result;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
       "repos/aspnet/docs/pulls");

            var client = _httpClientFactory.CreateClient("github");

            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        [HttpGet("testtimeout")]
        public ActionResult<IEnumerable<string>> Timeout()
        {
            if (myCount < 3)//模拟超时
            {
                System.Threading.Thread.Sleep(3000);
            }
            myCount++;

            return new string[] { "value1", "value2" };
        }
        [HttpGet("timeout")]
        public async Task<ActionResult<string>> Gets() {
            var client = _httpClientFactory.CreateClient("test");

            var request = new HttpRequestMessage(HttpMethod.Get, "api/values/testtimeout");
            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            return content;
        }

    }
}
