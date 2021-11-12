using Autofac.AttributeExtensions.Attributes;
using Autofac.Features.AttributeFilters;
using Dapper;
using Learning.Common.Const;
using Learning.IRepositories;
using Learning.IRepositories.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Learning.Repositories
{
    [InstancePerLifetimeScope]
    public class LearningRepository : ILearningRepository
    {
        private readonly IDbConnection _dbConnection;

        public LearningRepository([KeyFilter(AutofacKeyedConst.LearningDb)] IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<Study>> GetStudyList()
        {
            var sql = @"select * from study";
            return (await _dbConnection.QueryAsync<Study>(sql))?.ToList();
        }
    }
}
