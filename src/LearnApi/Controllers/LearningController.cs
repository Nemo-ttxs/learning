using Learning.Api.Extensions;
using Learning.IServices;
using Learning.Models.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Learning.Api.Controllers
{

    [Route("api/v{version:apiVersion}/learning")]
    [ApiVersion("1.0")]
    [ApiController]
    public class LearningController : ControllerBase
    {
        private readonly ILearningService _learningService;

        public LearningController(ILearningService learningService)
        {
            _learningService = learningService;
        }

        /// <summary>
        ///测试
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("get-study-list")]
        [TestFilter]
        public Task<List<StudyInfo>> GetStudyList()
        {
            var res = _learningService.TestMethod();
            return res;
        }
    }
}
