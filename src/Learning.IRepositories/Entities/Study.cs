using System;
using System.Collections.Generic;
using System.Text;

namespace Learning.IRepositories.Entities
{
    public class Study
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime AddTime { get; set; }
    }
}
