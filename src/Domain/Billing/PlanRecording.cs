using System;

namespace ZoomClient.Domain.Billing
{
    public class PlanRecording : ZObject
    {
        public string free_storage { get; set; }
        public string free_storage_usage { get; set; }
        public string plan_storage { get; set; }
        public string plan_storage_exceed { get; set; }
        public string plan_storage_usage { get; set; }
    }
}
