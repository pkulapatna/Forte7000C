using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forte7000E.Module.LotProcess
{
    public class LotData
    {
        public int Index { get; set; }
        public long LotNum { get; set; }
        public string LotStatus { get; set; }
        public bool Empty { get; set; }
        public DateTime OpenTD { get; set; }
        public DateTime CloseTD { get; set; }
        public bool CloseBySize { get; set; }
        public bool CloseByTime { get; set; }
        public int BaleCount { get; set; }
        public long Balelotnumber { get; set; }

        //Weight
        public double TotalNetWeight { get; set; }
        public double MinNetWeight { get; set; }
        public double MinNetWeightbale { get; set; }
        public double MaxNetWeight { get; set; }
        public int MaxNetWeightbale { get; set; }
        public double NetWeightsquare { get; set; }
        public double MeanNetWeight { get; set; }
        public double RangeNetWeight { get; set; }
        public double StdNetWeight { get; set; }
        public double TotalTareWeight { get; set; }
        public double TotalBoneDryWeight { get; set; }

        //Moisture
        public double TotalMoistureContent { get; set; }
        public double MinMoistureContent { get; set; }
        public int MinMoistureContentBale { get; set; }
        public double MaxMoistureContent { get; set; }
        public int MaxMoistureContentBale { get; set; }
        public double MoistureContentsquare { get; set; }
        public double RangeMoistureContent { get; set; }
        public double StdDevMoistureContent { get; set; }

        public double NextBaleNumber { get; set; }
        public double BaleAssigned { get; set; }
        public double ClosingSize { get; set; }

        public double Action { get; set; }
        public bool Sr { get; set; }
        public bool Finish { get; set; }
        public string OrderStr { get; set; }
        public int Uid { get; set; }
        public double SpareSngFld1 { get; set; }
        public double SpareSngFld2 { get; set; }
        public double SpareSngFld3 { get; set; }

        public string AsciiFld1 { get; set; }
        public string AsciiFld2 { get; set; }
        public string AsciiFld3 { get; set; }
        public string AsciiFld4 { get; set; }

        public string LotIdent { get; set; }
        public int QualityUID { get; set; }
        public string StockName { get; set; }
        public string FC_IdentString { get; set; }

        public int UnitCount { get; set; }
        public string MonthCode { get; set; }

        public LotData()
        {

        }

        public LotData(double minNetWeight)
        {
            MinNetWeight = minNetWeight;
        }
    }
}
