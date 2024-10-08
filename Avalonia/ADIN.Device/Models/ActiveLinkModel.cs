using SciChart.Charting.Model.ChartSeries;
using System.Collections.ObjectModel;

namespace ADIN.Device.Models
{
    public class ActiveLinkModel
    {
        public ActiveLinkModel()
        {
            Annotations = new ObservableCollection<IAnnotationViewModel>();
        }

        public ObservableCollection<IAnnotationViewModel> Annotations { get; set; }
        public bool IsActiveLinkEnable { get; set; } = false;
        public bool IsLinkLengthVisible { get; set; }
        public bool IsMseBenchmarkVisible { get; set; }
        public string LinkLengthBenchMark { get; set; }
        public string MseBenchmark { get; set; }
    }
}