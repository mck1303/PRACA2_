using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opracowanie_heurystyk
{
    public class MachineCSV
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ProdStartCSV
    {
        public int Id { get; set; }
        public double Qnty { get; set; }
    }

    public class BasicCSV
    {
        public int Machines { get; set; }
        public int Operations { get; set; }
        public int Processes { get; set; }

        
    }

    public class MachineTimeCSV
    {
        public int Machine { get; set; }
        public int Operation { get; set; }
        public double Time { get; set; }


    }

    public class MachineChangeCSV
    {
        public int Machine { get; set; }
        public int Operation1 { get; set; }
        public int Operation2 { get; set; }
        public double Time { get; set; }


    }

    public class OperationCSV
    {
        public int Id { get; set; }
        public double TimeBefore { get; set; }
        public double TimeAfter { get; set; }
        public bool CanPause { get; set; }
        public double MaxPauseTime { get; set; }
        public int PauseCount { get; set;}


    }

    public class ProductionCSV
    {
        public int Operation { get; set; }
        public int Type { get; set; }
        public double Qnty { get; set; }
        public int Number { get; set; }
        public int Row { get; set; }

    }

    public class ProcessCSV
    {
        public int Id { get; set; }
        public int Priority { get; set; }
        public double MaxTime { get; set; }

    }

    public class OPCSV
    {
        public int Id { get; set; }
        public int Operation { get; set; }
    }

    public class b_resCSV
    {
        public int Iter { get; set; }

        public double Best_Result { get; set; }
    }
}
