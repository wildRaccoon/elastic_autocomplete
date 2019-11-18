using System.Collections.Generic;

namespace data_generator
{
    public class UserData
    {
        public string firstName { get; set; }
        
        public string id { get; set; }

        public string lastName { get; set; }

        public string email { get; set; }
        
        public List<string> tags { get; set; }
    }
}