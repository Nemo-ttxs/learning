using Learning.Models.ResponseModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Learning.IServices
{
    public interface ILearningService
    {
        Task<List<StudyInfo>> TestMethod();
    }
}
