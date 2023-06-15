using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Opracowanie_heurystyk
{
    public class OPList
    {
        List<List<int>> operations_in_machines = new List<List<int>>();
        public List<List<OP>> NewRandomOPList(List<Process> pp, List<Machine> mm, List<Operation> oo, int global_OP_id)
        {

            Random rnd = new Random();
            List<List<OP>> sol = new List<List<OP>>();
            for (int i = 0; i < mm.Count; i++)
            {
                sol.Add(new List<OP> { new OP(-10, -10) });
            }
            for(int i = 0; i < oo.Count; i++)
            {
                this.operations_in_machines.Add(new List<int> { 0 });
            }
            for (int i = 0; i < mm.Count; i++)
            {
                for (int j = 0; j < mm[i].operation_time.Count(); j++)
                {
                    if (mm[i].operation_time[j] > 0)
                    {
                        this.operations_in_machines[j].Add(i);
                    }
                }
            }
            for (int i = 0; i < oo.Count; i++)
            {
                this.operations_in_machines[i].Remove(this.operations_in_machines[i][0]);
            }

            for (int i = 0; i < pp.Count; i++)
            {
                for (int j = 0; j < pp[i].operations.Count; j++)
                {
                    int k = rnd.Next(0, this.operations_in_machines[pp[i].operations[j].id].Count);
                    int mch = this.operations_in_machines[pp[i].operations[j].id][k];
                    sol[mch].Add(new OP(pp[i].id, pp[i].operations[j].id, mm[mch].operation_time[pp[i].operations[j].id]));
                    global_OP_id++;
                }
            }
            for (int i = 0; i < mm.Count; i++)
            {
                sol[i].Remove(sol[i][0]);
            }
            return sol;
        }

        public List<List<OP>> Shuffle(List<List<OP>> a, List<List<OP>> b, List<Machine> mm)
        {
            Random rnd= new Random();
            List<List<OP>> newBorn = new List<List<OP>>();
            List<OP> all_op = new List<OP>();
            List<OP> all_op_b = new List<OP>();

            for (int i = 0; i < a.Count; i++)
            {
                for(int j = 0; j < a[i].Count; j++)
                {
                    if(a[i][j].O!=-1 & a[i][j].O != -2)
                    {
                        all_op.Add(a[i][j]);
                    }
                        
                }
                
            }
            for (int i = 0; i < b.Count; i++)
            {
                for (int j = 0; j < b[i].Count; j++)
                {
                    if (b[i][j].O != -1 & b[i][j].O != -2)
                    {
                        all_op_b.Add(b[i][j]);
                    }

                }

            }

            for (int i = 0; i < mm.Count; i++)
            {
                newBorn.Add(new List<OP> { new OP(-1, -1, -1) });
                int x = rnd.Next(0, 2);
                if (x == 0)
                {
                    for (int j=0;j< a[i].Count; j++)
                    {
                        for(int k = 0; k < all_op.Count; k++)
                        {
                            if (a[i][j].O == all_op[k].O & a[i][j].P == all_op[k].P | a[i][j].O == -1 | a[i][j].O == -2)
                            {

                                newBorn[i].Add(a[i][j]);
                                if (a[i][j].O != -1 & a[i][j].O != -2)
                                {
                                    all_op.Remove(all_op[k]);
                                }


                            }
                        }
                    }
                }
                newBorn[i].Remove(newBorn[i][0]);
                if (x == 1)
                {
                    for (int j = 0; j < b[i].Count; j++)
                    {
                        for (int k = 0; k < all_op.Count; k++)
                        {
                            if (b[i][j].O == all_op[k].O & b[i][j].P == all_op[k].P | b[i][j].O==-1| b[i][j].O==-2)
                            {

                                newBorn[i].Add(b[i][j]);
                                if (b[i][j].O != -1 & b[i][j].O != -2)
                                {
                                    all_op.Remove(all_op[k]);
                                }
                                

                            }
                        }
                    }
                }
            }
            for(int i = 0; i < all_op.Count; i++)
            {
                for(int j = 0; j < a.Count; j++)
                {
                    for (int k = 0; k < a[j].Count; k++)
                    {
                        if (all_op[i].O == a[j][k].O & all_op[i].P == a[j][k].P)
                        {
                            int w = 0;
                            if (newBorn[j].Count <= k)
                            {
                                w= newBorn[j].Count;
                            }
                            else
                            {
                                w = k;
                            }
                            newBorn[j].Insert(w, all_op[i]);
                        }
                    }
                }
            }

            for (int i = 0; i < newBorn.Count; i++)
            {
                for (int j = newBorn[i].Count-1; j >0; j--)
                {
                    if (newBorn[i][j].O==-1 | newBorn[i][j].O == -2 & newBorn[i][j-1].O == -1 | newBorn[i][j-1].O == -2)
                    {
                        double newtime = newBorn[i][j].Time + newBorn[i][j - 1].Time;
                        OP newOP = new OP(newBorn[i][j].P, newBorn[i][j].O, newtime);
                        newBorn[i].Remove(newBorn[i][j]);
                        newBorn[i].Remove(newBorn[i][j-1]);
                        newBorn[i].Add(newOP);


                    }
                }
            }

            return newBorn;

            

        }

        public List<List<List<OP>>> Mutation(List<List<List<OP>>> OPs, int mut_percentage, double time_of_pause)
        {
            Random rnd = new Random();
            for (int i = 0; i < mut_percentage * OPs.Count / 100; i++)
            {
                int x = rnd.Next(0, OPs.Count);
                int y = rnd.Next(0, OPs[x].Count);
                int z = rnd.Next(0, OPs[x][y].Count);
                if (OPs[x][y].Count != 0)
                {
                    OPs[x][y].Insert(z, new OP(OPs[x][y][z].P, -2, rnd.NextDouble() * time_of_pause));
                }
                
            }
            return OPs;
        }
    }

    
    public class Algorithm //johnson po ustaleniu kolejności ale przed mutacjami, dawać do czasu czas przestrojenia i wykonania
    {
        public int mode; //0 - tylko genetyczny, 1 - genetyczny z początkowym johnsona, 2 - genetyczny z johnsona w każdej generacji
        public Algorithm(int mode)
        {
            this.mode = mode;
        }
        public List<List<OP>> Johnson(List<List<OP>> solution, int processes_num, List<Process> pp, List<Machine> mm)
        {
            if (processes_num != 1)
            {

                List<int> proces_order= new List<int>();
                List<double> m1 = new List<double>();
                List<double> m2 = new List<double>();
                List<double> m3 = new List<double>();
                List<double> m4 = new List<double>();
                for (int i = 0; i < pp.Count; i++)
                {
                    m1.Add(0);
                    m2.Add(0);
                    m3.Add(0);
                    m4.Add(0);
                }
                for (int i=0; i < solution.Count; i++)
                {
                    for (int j = 0; j < solution[i].Count; j++)
                    {
                        if (i == 0 & solution[i][j].O >= 0)
                        {
                            m1[solution[i][j].P] += solution[i][j].Time;
                        }
                        else if (i == solution.Count - 1 & solution[i][j].O >= 0)
                        {
                            m2[solution[i][j].P] += solution[i][j].Time;
                        }
                        else if (solution[i][j].O>=0)
                        {
                            m1[solution[i][j].P] += solution[i][j].Time;
                            m2[solution[i][j].P] += solution[i][j].Time;
                        }
                    }
                }
                
                for (int i = 0; i < m1.Count; i++)
                {
                    m3[i] = m1[i] - m2[i];
                }
                
                for (int i = 0; i < m3.Count; i++)
                {
                    int ij = 0;
                    if (m3[i] > 0)
                    {
                        m4[ij] = m1[i];
                        ij += 1;
                    }
                    
                }
                m4.Sort();
                if (m4.Sum() != 0)
                {
                    for (int i = 0; i < m4.Count; i++)
                    {
                        
                        int w = m1.IndexOf(m4[i]);
                        if (w != -1) {
                            proces_order.Add(w);
                            m1[w] = -1;
                            m2[w] = -1;
                        }
                        
                    }
                }
                

                for (int l = 0; l < m2.Count; l++)
                {
                    m4[l] = m2[l];
                }
                m4.Sort();
                int g = m4.Count-1;
                for (int i=0; i < g+1; i++)
                {
                    if (m4[g-i] != -1)
                    {
                        int w = m2.IndexOf(m4[g - i]);
                        proces_order.Add(w);
                        m1[w] = -1;
                        m2[w] = -1;
                    }
                    

                }
                if (proces_order.Count != processes_num)
                {
                    Console.WriteLine("Something went wrong with new johnson list");
                }
                List<List<OP>> new_sol = new List<List<OP>>();
                List<double> machine_times=new List<double>();
                for (int i = 0; i < mm.Count; i++)
                {
                    machine_times.Add(0);
                }
                
                for (int i=0; i < mm.Count; i++)
                {
                    List<OP> ww = new List<OP>();
                    ww.Add(new OP(-1, -1, -1));
                    new_sol.Add(ww);
                }
                for(int i = 0; i < proces_order.Count; i++)
                {
                    double last_proc=0;
                    for(int j = 0; j < pp[proces_order[i]].operations.Count; j++)
                    {
                        int m_indx = -1;
                        for (int k = 0; k < solution.Count; k++)
                        {
                            for (int l = 0; l < solution[k].Count; l++)
                            {
                                if (solution[k][l].P == proces_order[i] & solution[k][l].O == pp[proces_order[i]].operations[j].id)
                                {
                                    m_indx = k;
                                    break;
                                }
                            }
                        }
                        if (last_proc != 0)
                        {
                            if (new_sol[m_indx].Count == 1)
                            {
                                new_sol[m_indx].Add(new OP( proces_order[i],-2, last_proc));

                                new_sol[m_indx].Add(new OP(proces_order[i], pp[proces_order[i]].operations[j].id, mm[m_indx].operation_time[pp[proces_order[i]].operations[j].id]));

                                machine_times[m_indx] += mm[m_indx].operation_time[pp[proces_order[i]].operations[j].id];
                                last_proc += machine_times[m_indx];
                            }
                            else
                            {
                                int l_oper = new_sol[m_indx].Last().O;
                                new_sol[m_indx].Add(new OP(proces_order[i], -2, last_proc- machine_times[m_indx] + mm[m_indx].change_matrix[l_oper, pp[proces_order[i]].operations[j].id]));

                                new_sol[m_indx].Add(new OP(proces_order[i], pp[proces_order[i]].operations[j].id, mm[m_indx].operation_time[pp[proces_order[i]].operations[j].id]));

                                machine_times[m_indx] += mm[m_indx].operation_time[pp[proces_order[i]].operations[j].id] + last_proc - machine_times[m_indx] + mm[m_indx].change_matrix[l_oper, pp[proces_order[i]].operations[j].id];
                                last_proc += machine_times[m_indx];
                            }
                            
                        }
                        else
                        {
                            new_sol[m_indx].Add(new OP( proces_order[i], pp[proces_order[i]].operations[j].id, mm[m_indx].operation_time[pp[proces_order[i]].operations[j].id]));

                            machine_times[m_indx] += mm[m_indx].operation_time[pp[proces_order[i]].operations[j].id];
                            last_proc += machine_times[m_indx];
                        }

                    }
                }
                for(int i = 0; i < new_sol.Count; i++)
                {
                    new_sol[i].Remove(new_sol[i][0]);
                }

                return new_sol;

            }
            else
            {
                List<List<OP>> s = new List<List<OP>>();
                double t=0;
                for (int i = 0; i < pp[0].operations.Count; i++)
                {
                    int mach = -1;

                    for (int j = 0; j < solution.Count; j++) {
                        for (int k = 0; k < solution[j].Count; k++)
                        {
                            if (solution[j][k].O == pp[0].operations[i].id)
                            {
                                mach = j;
                            }
                        }
                    }
                    if (i == 0)
                    {
                        if (mach != -1)
                        {
                            s[mach].Add(new OP(0, pp[0].operations[i].id, mm[mach].operation_time[pp[0].operations[i].id]));
                            t = mm[mach].operation_time[pp[0].operations[i].id];

                        }
                        else
                        {
                            Console.WriteLine("Something went wrong in algorithm");
                        }
                            
                    }
                    else
                    {
                        if (s[mach].Count == 0)
                        {
                            s[mach].Add(new OP( 0, -2, t));
                            s[mach].Add(new OP(0, pp[0].operations[i].id, mm[mach].operation_time[pp[0].operations[i].id]));
                            t += mm[mach].operation_time[pp[0].operations[i].id];
                        }
                        else
                        {
                            double to_now_time=0;
                            int last_o=-3;
                            for (int j = 0; j < s[mach].Count; j++)
                            {
                                to_now_time += s[mach][j].Time;
                                if (s[mach][j].O != -1 | s[mach][j].O != -2)
                                {
                                    last_o = s[mach][j].O;
                                }
                            }
                            s[mach].Add(new OP( 0, -2, t - to_now_time + mm[mach].change_matrix[last_o,pp[0].operations[i].id]));
                            s[mach].Add(new OP( 0, pp[0].operations[i].id, mm[mach].operation_time[pp[0].operations[i].id]));
                            t += mm[mach].operation_time[pp[0].operations[i].id];
                        }
                        
                    }
                    
                }
                return s;

            }
            
            


        }

        

        public List<List<OP>> Run_Algorithm(double[] start_quant, List<Operation> oo, List<Machine> mm, List<Process> pp, int algorithm_mode,  int starting_quant, double a, double b, double c, double d, double e, double f, double g, double t,  int max_iterations = 1000, int population=100, int best_percentage=20, int mut_percentage=10, double time_of_pause=10.0)//algorithm mode: 1-only genetic, 2-johnson on start, 3- full johnson and genetic
        {
            int global_OP_id = 0;
            List<List<OP>> best_result = new List<List<OP>>();
            double best_value = -1;
            int best_objects = population * best_percentage / 100;
            List<List<List<OP>>> family_1=new List<List<List<OP>>>();
            List<double> family_1_values=new List<double>();
            List<double> f_val = new List<double>();
            Simulation sim = new Simulation(pp, oo, mm);//, start_quant);
            CostFunction cost = new CostFunction(a, b, c, d, t, e, f, g, start_quant);
            
            OPList OPL = new OPList();
            Random rnd = new Random();
            if (algorithm_mode == 1)
            {
                
                for (int i = 0; i < starting_quant; i++)
                {
                    family_1.Add(OPL.NewRandomOPList(pp, mm,oo, global_OP_id));
                }
                for(int i = 0; i < max_iterations; i++)
                {
                    family_1_values = new List<double>();
                    f_val = new List<double>();
                    Console.WriteLine("Iteration: {0}", i);
                    List<List<List<OP>>> family_2 = new List<List<List<OP>>>();
                    for (int j = 0; j < family_1.Count; j++)
                    {
                        List<List<List<double>>> cnt = sim.CountTime(family_1[j]);
                        family_1_values.Add(cost.CountCost(cnt[0], pp, oo, cnt[1], cnt[2]));

                        f_val.Add(family_1_values[j]);
                        if (best_value == -1)
                        {
                            best_value = family_1_values[j];
                        }
                        if (family_1_values[j] < best_value)
                        {
                            best_value = family_1_values[j];
                            best_result = family_1[j];
                        }
                    }
                    //f_val.Sort();
                    f_val.Reverse();
                    for (int j = 0; j < best_objects; j++)
                    {
                        int x = family_1_values.IndexOf(f_val[j]);
                        family_1_values[x] = 0;
                        family_2.Add(family_1[x]);
                    }
                    family_1 = family_2;
                    while (family_1.Count < population)
                    {
                        int x = rnd.Next(0, family_2.Count);
                        int y = rnd.Next(0, family_2.Count);
                        while (x == y)
                        {
                            y = rnd.Next(0, family_2.Count);
                        }
                        family_1.Add(OPL.Shuffle(family_2[x], family_2[y],mm));
                    }
                    family_1 = OPL.Mutation(family_1, mut_percentage, time_of_pause);
                    
                }

            }
            else if (algorithm_mode == 2)
            {

                for (int i = 0; i < starting_quant; i++)
                {
                    family_1.Add(Johnson(OPL.NewRandomOPList(pp, mm, oo, global_OP_id),pp.Count,pp,mm));
                }
                for (int i = 0; i < max_iterations; i++)
                {
                    family_1_values = new List<double>();
                    f_val = new List<double>();
                    Console.WriteLine("Iteration: {0}", i);
                    List<List<List<OP>>> family_2 = new List<List<List<OP>>>();
                    for (int j = 0; j < family_1.Count; j++)
                    {

                        List<List<List<double>>> cnt = sim.CountTime(family_1[j]);
                        family_1_values.Add(cost.CountCost(cnt[0], pp, oo, cnt[1], cnt[2]));
                        f_val.Add(family_1_values[j]);
                        if (best_value == -1)
                        {
                            best_value = family_1_values[j];
                        }
                        if (family_1_values[j] < best_value)
                        {
                            best_value = family_1_values[j];
                            best_result = family_1[j];
                        }
                    }
                    f_val.Sort();
                    f_val.Reverse();
                    for (int j = 0; j < best_objects; j++)
                    {
                        int x = family_1_values.IndexOf(f_val[j]);
                        family_1_values[x] = 0;
                        family_2.Add(family_1[x]);
                    }
                    family_1 = family_2;
                    while (family_1.Count < population)
                    {
                        int x = rnd.Next(0, family_2.Count);
                        int y = rnd.Next(0, family_2.Count);
                        while (x == y)
                        {
                            y = rnd.Next(0, family_2.Count);
                        }
                        family_1.Add(OPL.Shuffle(family_2[x], family_2[y], mm));
                    }
                    family_1 = OPL.Mutation(family_1, mut_percentage, time_of_pause);

                }
            }
            else if (algorithm_mode == 3)
            {
                
                for (int i = 0; i < starting_quant; i++)
                {
                    family_1.Add(OPL.NewRandomOPList(pp, mm, oo, global_OP_id));
                }
                
                for (int i = 0; i < max_iterations; i++)
                {
                    Console.WriteLine("Iteration: {0}", i);
                    family_1_values = new List<double>();
                    f_val = new List<double>();
                    for (int j = 0; j < family_1.Count; j++)
                    {
                        family_1[j] = Johnson(family_1[j], pp.Count, pp, mm);
                    }
                    List<List<List<OP>>> family_2 = new List<List<List<OP>>>();
                    for (int j = 0; j < family_1.Count; j++)
                    {

                        List<List<List<double>>> cnt = sim.CountTime(family_1[j]);
                        family_1_values.Add(cost.CountCost(cnt[0], pp, oo, cnt[1], cnt[2]));
                        f_val.Add(family_1_values[j]);
                        if (best_value == -1)
                        {
                            best_value = family_1_values[j];
                        }
                        if (family_1_values[j] < best_value)
                        {
                            best_value = family_1_values[j];
                            best_result = new List<List<OP>>();
                            for (int k= 0; k < family_1[j].Count; k++)
                            {
                                List<OP> p = new List<OP>();
                                for (int l=0; l < family_1[j][k].Count; l++)
                                {
                                    p.Add(family_1[j][k][l]);
                                }
                                best_result.Add(p);
                            }
                            
                        }
                    }
                    f_val.Sort();
                    f_val.Reverse();
                    for (int j = 0; j < best_objects; j++)
                    {
                        int x = family_1_values.IndexOf(f_val[j]);
                        family_1_values[x] = 0;
                        family_2.Add(family_1[x]);
                    }
                    family_1 = family_2;
                    while (family_1.Count < population)
                    {
                        int x = rnd.Next(0, family_2.Count);
                        int y = rnd.Next(0, family_2.Count);
                        while (x == y)
                        {
                            y = rnd.Next(0, family_2.Count);
                        }
                        family_1.Add(OPL.Shuffle(family_2[x], family_2[y], mm));
                    }
                    family_1 = OPL.Mutation(family_1, mut_percentage, time_of_pause);

                }
            }
            else
            {
                Console.WriteLine("Wrong mode of algorithm");
            }
            return best_result;
            
        }
    }
}
