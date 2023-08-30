using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opracowanie_heurystyk
{
    public class Machine
    {
        public int id;
        public double[,] change_matrix;
        public double[] operation_time;
        public Machine(int id, double[,] change_matrix, double[] operation_time)
        {
            this.id = id;
            this.change_matrix = change_matrix;
            this.operation_time = operation_time;
        }

    }

    public class Operation
    {
        public int id;
        public double time_after_previous;
        public double time_before_next;
        public List<List<double>> needed_sources; //row is time (last position as % of final time), column is source
        public List<List<double>> produced_products; //row is time (last position as % of final time), column is product
        public bool canPause;
        public double maxPauseTime;
        public int pauseCount; //-1 - infinity

        public Operation(List<List<double>> produced_products, int id, double time_after_previous, double time_before_next, List<List<double>> needed_sources, bool canPause, double maxPauseTime, int pauseCount)
        {
            this.produced_products = produced_products;
            this.id = id;
            this.time_after_previous = time_after_previous;
            this.time_before_next = time_before_next;
            this.needed_sources = needed_sources;
            this.canPause = canPause;
            this.maxPauseTime = maxPauseTime;
            this.pauseCount = pauseCount;
        }

    }
    public class Process
    {
        public int id;
        public List<Operation> operations;
        public int priority;

        public Process(int id, List<Operation> operations, int priority)
        {
            this.id = id;
            this.operations = operations;

            this.priority = priority;
        }
    }

    public struct OP
    {
        public OP(int p, int o, double time = 0)
        {
            this.P = p;
            this.O = o;  //o==-1 oznacza przerwę o zadanym czasie -2 oznacza przerwę na kalibrajcę maszyny w sumie to jedno
            this.Time = time; //dawać czas żeby było wiadome kiedy przerwy
        }
        public int O { get; }
        public int P { get; }

        public double Time { get; }

    }
}
