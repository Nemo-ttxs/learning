using Learning.IRepositories.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Learning.IRepositories
{
    public interface ILearningRepository
    {
        Task<List<Study>> GetStudyList();
    }
}
