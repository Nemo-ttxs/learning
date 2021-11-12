using System;
using System.Collections.Generic;
using System.Text;

namespace Learning.Models.ResponseModel
{
    /// <summary>
    /// 学习信息
    /// </summary>
    public class StudyInfo
    {
        public int Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime AddTime { get; set; }
    }
}
