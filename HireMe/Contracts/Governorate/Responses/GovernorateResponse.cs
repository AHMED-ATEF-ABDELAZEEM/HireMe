using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HireMe.Contracts.Governorate.Responses
{
    public class GovernorateResponse
    {
        public int Id { get; set; }
        public string NameArabic { get; set; }
        public string NameEnglish { get; set; }
    }
}