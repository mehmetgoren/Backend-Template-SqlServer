namespace Server.Models
{
    using System.Collections.Generic;

    public class ChartModelDataSet
    {
        public List<object> data { get; set; } = new List<object>();
        public List<string> backgroundColor { get; set; } = new List<string>();
        public List<string> hoverBackgroundColor { get; set; } = new List<string>();

        public string label { get; set; }
        public string borderColor { get; set; }
    }

    public class ChartModel
    {
        public List<string> labels { get; set; } = new List<string>();

        public List<ChartModelDataSet> datasets { get; set; } = new List<ChartModelDataSet>();
    }
}
