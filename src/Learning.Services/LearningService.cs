using Autofac.AttributeExtensions.Attributes;
using Autofac.Features.AttributeFilters;
using Learning.Common.Const;
using Learning.Common.MQ;
using Learning.Common.Redis;
using Learning.IRepositories;
using Learning.IServices;
using Learning.Models.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Learning.Services
{
    [InstancePerLifetimeScope]
    public class LearningService : ILearningService
    {
        private readonly ILearningRepository _learningRepository;
        private readonly RedisHelper _redisHelper;
        private readonly RabbitMqHelper _rabbitMqHelper;

        public LearningService(ILearningRepository learningRepository, [KeyFilter(AutofacKeyedConst.BusinessRedis)]RedisHelper redisHelper, [KeyFilter(AutofacKeyedConst.BusinessMQ)]RabbitMqHelper rabbitMqHelper)
        {
            _learningRepository = learningRepository;
            _redisHelper = redisHelper;
            _rabbitMqHelper = rabbitMqHelper;
        }

        public async Task<List<StudyInfo>> TestMethod()
        {
            var cacheValue = await _redisHelper.StringGetAsync<List<StudyInfo>>("test");
            if (cacheValue?.Count > 0)
            {
                return cacheValue;
            }
            var list = await _learningRepository.GetStudyList();
            if (list?.Count > 0)
            {
                await _redisHelper.StringSetAsync("test",list,TimeSpan.FromMinutes(30));
            }
            return list?.Select(x => new StudyInfo 
            { 
                Id = x.Id,
                AddTime = x.AddTime,
                Title = x.Title,
                Content = x.Content }).ToList();
        }
    }
}
