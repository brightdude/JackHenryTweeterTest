using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JHATest.Model
{
    public class TweetDTO
    {
        public string Text { get; set; }
        public List<string> Hashtags { get; set; }
    }
}
