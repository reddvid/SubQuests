using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SubQuests.UWP
{
    [DataContract]
    public class History
    {
        [DataMember]
        public string q { get; set; }

        [DataMember]
        public string a { get; set; }
    }
}
