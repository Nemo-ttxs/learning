using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Learning.Api.Controllers
{
    [Route("api/v{version:apiVersion}/test")]
    [ApiController]
    [ApiVersion("2.0")]
    public class TestController : ControllerBase
    {
        /// <summary>
        ///测试
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("learn")]
        public async Task<int> Learn()
        {
            var num = 0;
            var test = 5 / num;
            return test;
        }
    }
}
