using System;

namespace ZoomClient.Domain.Billing
{
    public class PlanDetails : ZObject
    {
        public int hosts { get; set; }
        public string type { get; set; }
        public int usage { get; set; }
    }
}
