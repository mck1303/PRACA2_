using CsvHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Opracowanie_heurystyk
{
    public class CostFunction
    {

        public double a; //waga do zaburzonych procesów
        public double b; //waga do złego okna czasowego
        public double c; //waga do ilości pauz
        public double d; //waga do czasu pauz
        public double t; //waga do czasu produkcji
        public double e; //waga priorytetów
        public double f; //waga zachowania odstępów czasowych
        public double g; //waga do niezachowania surowców
        double[] prod_quant;


        public int Comp_prod(List<double> x, List<double> y)
        {
            if (x[0] < y[0])
            {
                return -1;
            }
            else if (x[0] > y[0])
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }

        public int Comp_S_T(List<double> x, List<double> y)
        {
            if (x[3] < y[3])
            {
                return -1;
            }
            else if (x[3] > y[3])
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }


        public CostFunction(double a, double c, double d, double t, double e, double f, double g, double[] prod_quant)
        {
            this.a = a;
            this.c = c;
            this.d = d;
            this.t = t;
            this.e = e;
            this.f = f;
            this.g = g;
            this.prod_quant = prod_quant;

        }

        public double CountCost(List<List<double>> s_times, List<Process> pp, List<Operation> oo, List<List<double>> production, List<List<double>> needs, int mode = 0, string name="Y")
        {
            int process_break = 0;
            int too_many_pauses = 0;
            double pauseTime = 0;
            double all_prod_time = 0;
            double time_window = 0;
            double too_big_window = 0;
            List<double> processes_f_time = new List<double>();
            double priority_fail = 0;
            int quant_of_lost_sources = 0;
            double[] prod_quant_ = new double[prod_quant.Length];
            for (int j = 0; j < prod_quant.Length; j++)
            {
                prod_quant_[j] = prod_quant[j];
            }

            for (int j = 0; j < pp.Count; j++)
            {

                int last_oper = -1;
                double last_oper_ft = 0;
                int pauseCount;


                for (int i = 0; i < pp[j].operations.Count; i++)
                {
                    int indx = pp[j].operations[i].id;
                    List<double> start_times = new List<double>();
                    List<double> finish_times = new List<double>();
                    for (int k = 0; k < s_times.Count; k++)
                    {
                        if (s_times[k][1] == j & s_times[k][2] == indx)
                        {
                            if (s_times[k][4] == 0)
                            {
                                start_times.Add(s_times[k][3]);

                            }
                            else if (s_times[k][4] == 9)
                            {
                                finish_times.Add(s_times[k][3]);
                            }
                            else
                            {
                                Console.WriteLine("Something went wrong with simulation write");
                            }



                        }
                    }

                    if (i != 0)
                    {
                        if (oo[indx].time_after_previous != 0 | oo[pp[j].operations[i - 1].id].time_before_next != 0)
                        {

                            if (oo[indx].time_after_previous != 0)
                            {
                                time_window = oo[indx].time_after_previous;
                            }
                            else
                            {
                                time_window = oo[pp[j].operations[i].id - 1].time_before_next;
                            }

                        }
                    }

                    start_times.Sort();
                    finish_times.Sort();
                    pauseCount = start_times.Count - 1;
                    if (pauseCount > oo[indx].pauseCount & pauseCount != 0)
                    {
                        too_many_pauses += oo[indx].pauseCount - pauseCount;
                    }
                    if (pauseCount > 1)
                    {
                        double s = start_times.Sum();
                        double f = finish_times.Sum();
                        double w_time = f - s;
                        double o_pause_time = finish_times.Last() - start_times[0] - w_time;
                        if (o_pause_time > oo[indx].maxPauseTime)
                        {
                            pauseTime += o_pause_time - oo[indx].maxPauseTime;
                        }
                    }

                    if (last_oper != -1)
                    {

                        if (last_oper_ft > start_times[0])
                        {
                            process_break++;
                        }
                    }
                    if (time_window < start_times[0] - last_oper_ft)
                    {
                        too_big_window = start_times[0] - last_oper_ft - time_window;
                    }


                    last_oper_ft = finish_times.Last();
                    last_oper = indx;

                }
                processes_f_time.Add(last_oper_ft);
                if (last_oper_ft > all_prod_time)
                {
                    all_prod_time = last_oper_ft;

                }
            }



            for (int i = 0; i < processes_f_time.Count; i++)
            {
                for (int j = i + 1; j < processes_f_time.Count; j++) //jesli sie psuje dac ifa przed petla
                {
                    if (processes_f_time[i] > processes_f_time[j] & pp[i].priority < pp[j].priority)
                    {
                        priority_fail += processes_f_time[i] - processes_f_time[j];
                    }
                }
            }




            List<List<double>> summary = new List<List<double>>();
            for (int i = 0; i < needs.Count; i++)
            {
                summary.Add(new List<double> { needs[i][0], needs[i][1], -needs[i][2] });
            }
            for (int i = 0; i < production.Count; i++)
            {
                summary.Add(production[i]);
            }


            summary.Sort(Comp_prod);

            for (int i = 0; i < summary.Count; i++)
            {
                prod_quant_[Convert.ToInt32(summary[i][1])] += summary[i][2];

                if (i != summary.Count - 1)
                {
                    if (prod_quant_.Min() < 0 & summary[i][0] != summary[i + 1][0])
                    {
                        quant_of_lost_sources += 1;
                    }
                }

            }


            double sum = 0;
            sum += process_break * a;
            sum += too_many_pauses * c;
            sum += pauseTime * d;
            sum += all_prod_time * t;
            sum += priority_fail * e;
            sum += too_big_window * f;
            sum += quant_of_lost_sources * g;
            if (sum == 0)
            {
                Console.WriteLine("Something is no yes");
            }
            if (mode != 0)
            {
                
                s_times.Sort(Comp_S_T);
                string paths = paths = "C:\\Users\\Maciek\\Desktop\\PRACA2_\\WYNIKI\\";
                using (StreamWriter writer = new StreamWriter(paths + name + "_b_res.txt"))
                {
                    writer.WriteLine("Wyniki rozwiązania testu o kryptonimie: {0}", name);
                    writer.WriteLine("Ilość zaburzeń procesów: {0}", process_break);
                    writer.WriteLine("Ilość przypadków zbyt wielu pauz: {0}", too_many_pauses);
                    writer.WriteLine("Czas przekaczający maksymalny czas pauz: {0}", pauseTime);
                    writer.WriteLine("Całkowity czas produkcji: {0}", all_prod_time);
                    writer.WriteLine("Różnica czasu ukończenia procesów z wyzszym priorytetem i niższym (nieuwzględnienie priorytetów): {0}", priority_fail);
                    writer.WriteLine("Czas wykraczający poza maksymalną długość okna czasowego: {0}", too_big_window);
                    writer.WriteLine("Ilość nieotrzymanych materiałów: {0}", quant_of_lost_sources);
                    writer.WriteLine("");
                    writer.WriteLine("Struktura najlepszej kombinacji:");
                    for (int i=0; i<pp.Count; i++)
                    {
                        writer.WriteLine("Proces {0}",i);
                        for (int j = 0; j < s_times.Count; j++)
                        {
                            if (s_times[j][1] == i)
                            {
                                if (s_times[j][4] == 0)
                                {
                                    writer.WriteLine("START O:{0} M:{1}, Czas:{2},", s_times[j][2], s_times[j][0], s_times[j][3]);
                                }
                                if (s_times[j][4] == 9)
                                {
                                    writer.WriteLine("STOP O:{0} M:{1}, Czas:{2},", s_times[j][2], s_times[j][0], s_times[j][3]);
                                }
                            }
                        }
                    }

                }

            }
            
            return sum;

        }
    }

    public class Simulation
    {
        public List<Process> pp;
        public List<Operation> oo;
        public List<Machine> mm;
        //public double[] start_quant;


        public List<double> machines_time;
        public List<List<double>> operations_time; //nr. maszyny, nr. procesu, nr. operacji, czas, typ wpisu (początek - 0, koniec - 9) 

        public Simulation(List<Process> pp, List<Operation> oo, List<Machine> mm)//, double[] start_quant)
        {
            this.mm = mm;
            this.oo = oo;
            this.pp = pp;
            //this.start_quant = start_quant;
            this.machines_time = new List<double>(new double[mm.Count]);

        }

        public List<List<List<double>>> CountTime(List<List<OP>> solution)
        {
            //Console.WriteLine(solution.Count);
            List<List<double>> needs = new List<List<double>>(); //lista: czas, surowce, ilość
            List<List<double>> production = new List<List<double>>(); //lista: czas, towar, ilość
            this.operations_time = new List<List<double>>();
            this.machines_time = new List<double>(new double[mm.Count]);
            for (int i = 0; i < solution.Count; i++)
            {
                int recording = 0;
                for (int j = 0; j < solution[i].Count; j++)
                {

                    if (solution[i][j].O == -1 | solution[i][j].O == -2)
                    {
                        this.machines_time[i] += solution[i][j].Time;

                    }
                    else
                    {
                        double last_o_delay;
                        if (j == 0)
                        {
                            last_o_delay = 0;
                        }
                        else
                        {
                            int wh = 0;
                            int l = j - 1;
                            int passing = 0;
                            while (wh == 0)
                            {
                                if (solution[i][l].O == -2 | solution[i][l].O == -1)
                                {
                                    if (l == 0)
                                    {
                                        passing = 1;
                                        wh = 1;
                                        recording = 1;
                                    }
                                    else
                                    {
                                        l -= 1;
                                    }


                                }
                                else { wh = 1; recording = 0; }
                            }
                            if (passing == 1)
                            {
                                last_o_delay = 0;
                            }
                            else
                            {
                                last_o_delay = mm[i].change_matrix[solution[i][l].O, solution[i][j].O];
                            }

                        }
                        if (recording != 1)
                        {
                            this.machines_time[i] += last_o_delay;
                        }

                        
                        this.operations_time.Add(new List<double> { i, solution[i][j].P, solution[i][j].O, this.machines_time[i], 0 });


                        for (int k = 0; k < this.oo[solution[i][j].O].produced_products.Count; k++)
                        {
                            for (int l = 0; l < this.oo[solution[i][j].O].produced_products[k].Count - 1; l++)
                            {
                                if (this.oo[solution[i][j].O].produced_products[k][l] != 0)
                                {

                                    production.Add(new List<double> { this.oo[solution[i][j].O].produced_products[k].Last() * this.mm[i].operation_time[this.oo[solution[i][j].O].id] + this.machines_time[i], l, this.oo[solution[i][j].O].produced_products[k][l] });
                                }

                            }



                        }

                        for (int k = 0; k < this.oo[solution[i][j].O].needed_sources.Count; k++)
                        {
                            for (int l = 0; l < this.oo[solution[i][j].O].needed_sources[k].Count - 1; l++)
                            {
                                if (this.oo[solution[i][j].O].needed_sources[k][l] != 0)
                                {

                                    needs.Add(new List<double> { this.oo[solution[i][j].O].needed_sources[k].Last() * this.mm[i].operation_time[this.oo[solution[i][j].O].id] + this.machines_time[i], l, this.oo[solution[i][j].O].needed_sources[k][l] });
                                }

                            }



                        }

                        this.machines_time[i] = this.machines_time[i] + solution[i][j].Time;
                        this.operations_time.Add(new List<double> { i, solution[i][j].P, solution[i][j].O, this.machines_time[i], 9 });
                    }
                }
            }

            return new List<List<List<double>>> { this.operations_time, needs, production };
        }


    }

    internal class Model
    {
        static void Main(string[] args)
        {

            int global_o_id = 0;
            int global_p_id = 0;
            string name;
            Console.WriteLine("Nazwij plik z wynikiami");
            name = Console.ReadLine();

            List<Operation> all_operations = new List<Operation>();
            List<Process> all_processes = new List<Process>();
            List<Machine> all_machines = new List<Machine>();
            int mode_of_creation = 1;//0-give info, 1-give CSV, 2-random

            if (mode_of_creation == 0)
            {
                Console.WriteLine("Podaj liczbę wszystkich produktów/surowców");
                int prod_num = Int32.Parse(Console.ReadLine());
                double[] products_quant = new double[prod_num];
                Console.WriteLine("Podaj liczbę procesów");
                int p = Int32.Parse(Console.ReadLine());
                for (int i = 0; i < p; i++)
                {

                    Console.WriteLine("Podajesz wartośći procesu o ID=" + global_p_id + ". Podaj liczbę operacji w procesie " + i);
                    int o = Int32.Parse(Console.ReadLine());
                    List<Operation> proc_oper = new List<Operation>();
                    Console.WriteLine("Podaj priorytet procesu (1-9, gdzie 1 to najwyższy priorytet)");
                    int prio = Int32.Parse(Console.ReadLine());
                    Console.WriteLine("Podaj maksymalny czas wykonania procesu. Wpisz -1 jeśli nie ma limitu");
                    int max_t = Int32.Parse(Console.ReadLine());


                    Console.WriteLine("Przechodzimy do skompletowania operacji tego procesu");
                    for (int j = 0; j < o; j++)
                    {
                        Console.WriteLine("Czy operacja znajduje się już w innym procesie? 0-nie, 1-tak");
                        int a = Int32.Parse(Console.ReadLine());
                        double bef;
                        double aft;
                        if (a == 1)
                        {
                            Console.WriteLine("Podaj id tej operacji");
                            int idd = Int32.Parse(Console.ReadLine());
                            proc_oper.Add(all_operations[idd]);
                        }

                        else
                        {
                            Console.WriteLine("ID nowej operacji wynosi " + global_o_id + ". Czy konieczne są ograniczenia czasowe? 0-Nie 1-Tak");
                            int time = Int32.Parse(Console.ReadLine());
                            if (time == 1)
                            {
                                Console.WriteLine("Ile czasu należy odczekać po poprzedniej operacji?");
                                bef = double.Parse(Console.ReadLine());
                                Console.WriteLine("Ile czasu należy odczekać przed kolejną operacją?");
                                aft = double.Parse(Console.ReadLine());
                            }
                            else
                            {
                                bef = -1;
                                aft = -1;
                            }
                            Console.WriteLine("Przechodzimy do produktów operacji");
                            int l = 1;
                            List<List<double>> sour = new List<List<double>>();
                            List<List<double>> prod = new List<List<double>>();
                            while (l == 1)
                            {

                                Console.WriteLine("Podaj id produktu operacji");
                                int prod_id = Int32.Parse(Console.ReadLine());
                                Console.WriteLine("Jeśli produkty są dostępne dopiero po ukończeniu operacji wpisz ich ilość. Inaczej wpisz -1");
                                int quant = Int32.Parse(Console.ReadLine());
                                if (quant != -1)
                                {
                                    List<double> row = new List<double>();
                                    for (int k = 0; k < prod_num + 1; k++)
                                    {
                                        row.Add(0);
                                    }
                                    row[prod_id] = quant;
                                    row[prod_num] = 100;

                                    prod.Add(row);
                                    Console.WriteLine("Czy to już koniec dodawania produktów? 0-Tak 1-Nie");
                                    l = Int32.Parse(Console.ReadLine());
                                }
                                else
                                {
                                    int w = 1;
                                    while (w == 1)
                                    {
                                        Console.WriteLine("Wpisz procent czasu całkowitego w którym zostanie wyprodukowana ilość towaru");
                                        int p_time = Int32.Parse(Console.ReadLine());
                                        Console.WriteLine("Wpisz ilość towaru wyprodukowanego od poprzedniego wpisu");
                                        int p_quant = Int32.Parse(Console.ReadLine());
                                        List<double> row = new List<double>();
                                        for (int k = 0; k < prod_num + 1; k++)
                                        {
                                            row.Add(0);
                                        }
                                        row[prod_id] = p_quant;
                                        row[prod_num] = p_time;
                                        prod.Add(row);
                                        Console.WriteLine("Czy to już koniec dla tego produktu? 0-Tak 1-Nie");
                                        w = Int32.Parse(Console.ReadLine());
                                    }
                                    Console.WriteLine("Czy to już koniec dodawania produktów? 0-Tak 1-Nie");
                                    l = Int32.Parse(Console.ReadLine());
                                }

                            }
                            Console.WriteLine("Przechodzimy do potrzebnych surowców");
                            l = 1;
                            while (l == 1)
                            {

                                Console.WriteLine("Podaj id surowca");
                                int sour_id = Int32.Parse(Console.ReadLine());
                                Console.WriteLine("Jeśli surowce są potrzebne przed rozpoczęciem operacji wpisz ich ilość. Inaczej wpisz -1");
                                int squant = Int32.Parse(Console.ReadLine());
                                if (squant != -1)
                                {

                                    List<double> row = new List<double>();
                                    for (int k = 0; k < prod_num + 1; k++)
                                    {
                                        row.Add(0);
                                    }
                                    row[sour_id] = squant;
                                    row[prod_num] = 100;
                                    sour.Add(row);
                                    Console.WriteLine("Czy to już koniec dodawania surowców? 0-Tak 1-Nie");
                                    l = Int32.Parse(Console.ReadLine());

                                }
                                else
                                {
                                    int w = 1;
                                    while (w == 1)
                                    {
                                        Console.WriteLine("Wpisz procent czasu całkowitego w którym ilość surowca jest potrzebna");
                                        int s_time = Int32.Parse(Console.ReadLine());
                                        Console.WriteLine("Wpisz ilość surowca potrzenego w danym czasie");
                                        int s_quant = Int32.Parse(Console.ReadLine());
                                        List<double> row = new List<double>();
                                        for (int k = 0; k < prod_num + 1; k++)
                                        {
                                            row.Add(0);
                                        }
                                        row[sour_id] = s_quant;
                                        row[prod_num] = s_time;
                                        sour.Add(row);
                                        Console.WriteLine("Czy to już koniec dla tego surowca? 0-Tak 1-Nie");
                                        w = Int32.Parse(Console.ReadLine());
                                    }
                                    Console.WriteLine("Czy to już koniec dodawania surowców? 0-Tak 1-Nie");
                                    l = Int32.Parse(Console.ReadLine());
                                }

                            }
                            Console.WriteLine("Czy operację tę można zatrzymać? 0-Tak 1-Nie");
                            int s = Int32.Parse(Console.ReadLine());
                            bool canPause;
                            double pauseTime;
                            int pauseCount;
                            if (s == 1)
                            {
                                canPause = false;
                                pauseTime = 0;
                                pauseCount = 0;

                            }
                            else
                            {
                                canPause = true;
                                Console.WriteLine("Na jaki czas można maksymanie zatrzymać operację?");
                                pauseTime = double.Parse(Console.ReadLine());
                                Console.WriteLine("Ile razy można wykonać pauzę? Wpisz -1 jeśli nie ma to znaczenia.");
                                pauseCount = Int32.Parse(Console.ReadLine());
                            }
                            all_operations.Add(new Operation(prod, global_o_id, aft, bef, sour, canPause, pauseTime, pauseCount));
                            proc_oper.Add(new Operation(prod, global_o_id, aft, bef, sour, canPause, pauseTime, pauseCount));
                            global_o_id++;

                        }

                    }
                    all_processes.Add(new Process(global_p_id, proc_oper, prio));
                    global_p_id++;
                }
                for (int i = 0; i < prod_num; i++)
                {
                    Console.WriteLine("Wpisz ilość początkową produktu o id=" + i);
                    products_quant[i] = Int32.Parse(Console.ReadLine());
                }
                Console.WriteLine("Wpisz ilość maszyn w symulacji");
                int m = Int32.Parse(Console.ReadLine());
                for (int i = 0; i < m; i++)
                {
                    List<Operation> ops = new List<Operation>();
                    double[] times = new double[global_o_id];
                    double[,] change = new double[global_o_id, global_o_id];
                    Console.WriteLine("Maszyna o id=" + i + ". Wpisz id operacji która może zostać na niej wykonana");
                    int l = 0;
                    l = Int32.Parse(Console.ReadLine());
                    while (l != -1)
                    {

                        ops.Add(all_operations[l]);
                        Console.WriteLine("Ile czasu trwa standardowo ta operacja na tej maszynie?");
                        double time = double.Parse(Console.ReadLine());
                        times[l] = time;
                        Console.WriteLine("Jeśli chcesz dodać jeszcze jakąś opercję wpisz jej id. Jeśli nie, wpisz -1");
                        l = Int32.Parse(Console.ReadLine());

                    }
                    Console.WriteLine("Czy konieczne są przeregulowania pomiędzy operacjami innego typu? 0-nie, 1-tak");
                    int n = Int32.Parse(Console.ReadLine());
                    while (n == 1)
                    {
                        Console.WriteLine("Wpisz id operacji poprzedzającej");
                        int b = Int32.Parse(Console.ReadLine());
                        Console.WriteLine("Wpisz id operacji następującej");
                        int a = Int32.Parse(Console.ReadLine());
                        if (a == b)
                        {
                            Console.WriteLine("Coś poszło nie tak.");
                        }
                        else
                        {
                            Console.WriteLine("Wpisz czas potrzebny na zmianę parametrów");
                            int t = Int32.Parse(Console.ReadLine());
                            change[b, a] = t;
                            Console.WriteLine("Czy trzeba dodać jeszcze jakiś czas? 0-nie, 1-tak");
                            n = Int32.Parse(Console.ReadLine());
                        }
                    }
                    all_machines.Add(new Machine(i, change, times));
                }
                int alg_mode = 3;
                double aa = 0.7;

                double c = 0.7;
                double d = 0.6;
                double e = 0.9;
                double f = 0.2;
                double g = 0.3;
                double tt = 0.3;
                int max_iterations = 1000;
                int population = 100;
                int best_percentage = 20;
                int mut_percentage = 10;
                double time_of_pause = 10.0;
                int start_pop = 100;
                Algorithm Al = new Algorithm(1);
                List<List<OP>> res = Al.Run_Algorithm(products_quant, all_operations, all_machines, all_processes, alg_mode, start_pop, aa, c, d, e, f, g, tt, max_iterations, population, best_percentage, mut_percentage, time_of_pause, name);
                SaveResults save = new SaveResults();
                save.save_res(name, all_machines, all_processes, products_quant, alg_mode, res, aa, c, d, e, f, g, tt, start_pop, max_iterations, population, best_percentage, mut_percentage);

            }

            if (mode_of_creation == 1)
            {
                double[] products_quant;
                int m;
                int p;
                int o;
                using (var reader = new StreamReader("C:\\Users\\Maciek\\Desktop\\PRACA2_\\CSV\\Basics.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<BasicCSV>().ToList();
                    m = records[0].Machines;
                    p = records[0].Processes;
                    o = records[0].Operations;
                }

                using (var reader = new StreamReader("C:\\Users\\Maciek\\Desktop\\PRACA2_\\CSV\\Products_at_start.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<ProdStartCSV>().ToList();
                    products_quant = new double[records.Count];
                    for (int i = 0; i < records.Count(); i++)
                    {
                        products_quant[records[i].Id] = records[i].Qnty;
                               
                    }
                }
                using (var reader = new StreamReader("C:\\Users\\Maciek\\Desktop\\PRACA2_\\CSV\\MachinesTimes.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records1 = csv.GetRecords<MachineTimeCSV>().ToList();
                    using (var reader2 = new StreamReader("C:\\Users\\Maciek\\Desktop\\PRACA2_\\CSV\\MachinesChange.csv"))
                    using (var csv2 = new CsvReader(reader2, CultureInfo.InvariantCulture))
                    {
                        var records2 = csv2.GetRecords<MachineChangeCSV>().ToList();
                        for (int i = 0; i < m; i++)
                        {
                            double[] times = new double[o];
                            double[,] change = new double[o, o];
                            for (int j = 0; j < records1.Count(); j++)
                            {
                                if (records1[j].Machine == i)
                                {
                                    times[records1[j].Operation] = records1[j].Time;
                                }
                            }
                            for (int j = 0; j < records2.Count(); j++)
                            {
                                if (records2[j].Machine == i)
                                {
                                    change[records2[j].Operation1, records2[j].Operation2] = records2[j].Time;
                                }
                            }
                            all_machines.Add(new Machine (i,change,times));

                        }
                        

                    }

                }

                using (var reader = new StreamReader("C:\\Users\\Maciek\\Desktop\\PRACA2_\\CSV\\Operation.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<OperationCSV>().ToList();
                    using (var reader1 = new StreamReader("C:\\Users\\Maciek\\Desktop\\PRACA2_\\CSV\\Production.csv"))
                    using (var csv1 = new CsvReader(reader1, CultureInfo.InvariantCulture))
                    {
                        var records1 = csv1.GetRecords<ProductionCSV>().ToList();
                        List<double> temp = new List<double>();
                        for (int i = 0; i < o; i++)
                        {
                            List<List<double>> prod = new List<List<double>>();
                            List<List<double>> sour = new List<List<double>>();
                            
                            for (int j = 0; j < records1.Count(); j++)
                            {
                                if (records1[j].Operation == i)
                                {
                                    
                                    while (records1[j].Row > prod.Count-1 & records1[j].Type == 0 | records1[j].Row > sour.Count-1 & records1[j].Type == 1)
                                    {
                                        temp = new List<double>();
                                        for (int l = 0; l <= products_quant.Count(); l++)
                                        {
                                            temp.Add(0);
                                        }
                                        if (records1[j].Type == 0)
                                        {
                                            prod.Add(temp);
                                        }
                                        else
                                        {
                                            sour.Add(temp);
                                        }
                                    }
                                    if (records1[j].Type == 0)
                                    {
                                        prod[records1[j].Row][records1[j].Number] = records1[j].Qnty;
                                    }
                                    else
                                    {
                                        sour[records1[j].Row][records1[j].Number] = records1[j].Qnty;
                                    }
                                    
                                }
                                
                                
                            }
                            all_operations.Add(new Operation(prod, i, records[i].TimeAfter, records[i].TimeBefore, sour, records[i].CanPause, records[i].MaxPauseTime, records[i].PauseCount));
                        }

                    }

                }


                using (var reader = new StreamReader("C:\\Users\\Maciek\\Desktop\\PRACA2_\\CSV\\Process.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<ProcessCSV>().ToList();
                    using (var reader1 = new StreamReader("C:\\Users\\Maciek\\Desktop\\PRACA2_\\CSV\\OP.csv"))
                    using (var csv1 = new CsvReader(reader1, CultureInfo.InvariantCulture))
                    {
                        var records1 = csv1.GetRecords<OPCSV>().ToList();
                        for (int i = 0; i < p; i++)
                        {
                            List<Operation> opera = new List<Operation>();
                            for (int j = 0; j < records1.Count(); j++)
                            {
                                if (records1[j].Id == i)
                                {
                                    opera.Add(all_operations[records1[j].Operation]);
                                }
                                
                            }
                            all_processes.Add(new Process(i, opera,  records[i].Priority));
                        }
                    }
                }

                int alg_mode = 1;
                double aa = 1000;

                double c = 1;
                double d = 1;
                double e = 1;
                double f = 1;
                double g = 1;
                double t = 1;
                int max_iterations = 100;
                int population = 10;
                int best_percentage = 2;
                int mut_percentage = 1;
                double time_of_pause = 3;
                int start_pop = 10;
                Algorithm Al = new Algorithm(1);
                List<List<OP>> res = Al.Run_Algorithm(products_quant, all_operations, all_machines, all_processes, alg_mode, start_pop, aa,  c, d, e, f, g, t, max_iterations, population, best_percentage, mut_percentage, time_of_pause, name);
                SaveResults save = new SaveResults();
                save.save_res(name, all_machines, all_processes, products_quant, alg_mode, res, aa,  c, d, e, f, g, t, start_pop, max_iterations, population, best_percentage, mut_percentage);

            }
            if (mode_of_creation == 2)
            {

                Random rnd = new Random();
                int p = rnd.Next(2, 5);
                int o = rnd.Next(p * 2, p * 3);
                int m = rnd.Next(1, o / 2);
                int mean_time = 10;
                int prod_num = rnd.Next(1, o);
                double[] products_quant = new double[prod_num];
                for (int i = 0; i < p; i++)
                {
                    List<Operation> proc_oper = new List<Operation>();
                    int prio = rnd.Next(1, 10);
                    int max_t = rnd.Next(600, 6000);
                    int l = o / 2;
                    if (m == 1) { l = o; }
                    for (int j = 0; j < l; j++)
                    {
                        int a;
                        if (global_o_id >= o)
                        {
                            a = 1;
                        }
                        else
                        {
                            a = 0;
                        }
                        double bef;
                        double aft;
                        if (a == 1)
                        {
                            int r = -1;
                            int wh = 1;
                            while (wh == 1)
                            {
                                r = rnd.Next(0, o);
                                if (proc_oper.Contains(all_operations[r]))
                                {
                                    continue;
                                }
                                else
                                {
                                    wh = 0;
                                }
                            }


                            proc_oper.Add(all_operations[r]);
                        }

                        else
                        {

                            int time = rnd.Next(0, 2);
                            if (time == 1)
                            {
                                bef = rnd.NextDouble() * mean_time;
                                aft = rnd.NextDouble() * mean_time;
                            }
                            else
                            {
                                bef = -1;
                                aft = -1;
                            }
                            List<List<double>> sour = new List<List<double>>();
                            List<List<double>> prod = new List<List<double>>();
                            int prod_id = rnd.Next(0, prod_num);
                            int quant = rnd.Next(1, 100);
                            List<double> row = new List<double>();
                            for (int k = 0; k < prod_num + 1; k++)
                            {
                                row.Add(0);
                            }
                            row[prod_id] = quant;
                            row[prod_num] = 100;
                            prod.Add(row);


                            int sour_id = rnd.Next(0, prod_num);
                            if (sour_id == prod_id)
                            {
                                if (sour_id != 0)
                                {
                                    sour_id = prod_id - 1;
                                }
                                else
                                {
                                    sour_id = 1;
                                }
                            }
                            int squant = rnd.Next(1, 100);
                            List<double> rows = new List<double>();
                            for (int k = 0; k < prod_num + 1; k++)
                            {
                                rows.Add(0);
                            }
                            rows[sour_id] = squant;
                            rows[prod_num] = 100;
                            sour.Add(rows);
                            int canPause = rnd.Next(0, 2);
                            bool pause;
                            int pauseCount;
                            double pauseTime;
                            if (canPause == 1)
                            {
                                pause = true;
                                pauseCount = rnd.Next(-1, 5);
                                pauseTime = rnd.NextDouble() * mean_time;
                            }
                            else
                            {
                                pause = false;
                                pauseCount = 0;
                                pauseTime = 0;
                            }

                            all_operations.Add(new Operation(prod, global_o_id, aft, bef, sour, pause, pauseTime, pauseCount));
                            proc_oper.Add(new Operation(prod, global_o_id, aft, bef, sour, pause, pauseTime, pauseCount));
                            global_o_id++;

                        }

                    }

                    all_processes.Add(new Process(global_p_id, proc_oper, prio));
                    global_p_id++;
                }
                if (all_operations.Count < o)
                {
                    Console.WriteLine("Too low quantity of operations");
                }
                for (int i = 0; i < prod_num; i++)
                {
                    products_quant[i] = rnd.Next(5, 100);
                }

                int ma = o / m;
                int n = 0;
                for (int i = 0; i < m; i++)
                {
                    int w = ma - 1;
                    if (w <= 0) { w = 1; };

                    List<Operation> ops = new List<Operation>();
                    double[] times = new double[o];
                    double[,] change = new double[o, o];


                    for (int j = n; j < o; j += m)
                    {
                        ops.Add(all_operations[j]);
                        double time = rnd.NextDouble() * mean_time;
                        times[j % o] = time;

                    }
                    n++;
                    for (int j = 0; j < ops.Count; j++)
                    {
                        for (int k = 0; k < ops.Count; k++)
                        {
                            int x = ops[j].id;
                            int y = ops[k].id;
                            change[x, y] = rnd.NextDouble() * mean_time;
                        }
                    }
                    all_machines.Add(new Machine(i, change, times));

                }

                int alg_mode = 1;//1-4
                double aa = 100;
                double c = 0.7;
                double d = 0.6;
                double e = 0.9;
                double f = 0.2;
                double g = 0.3;
                double t = 1;
                int max_iterations = 1000;
                int population = 100;
                int best_percentage = 10;
                int mut_percentage = 1;
                double time_of_pause = 10.0;
                int start_pop = 100;
                Algorithm Al = new Algorithm(1);
                List<List<OP>> res = Al.Run_Algorithm(products_quant, all_operations, all_machines, all_processes, alg_mode, start_pop, aa, c, d, e, f, g, t, max_iterations, population, best_percentage, mut_percentage, time_of_pause, name);
                SaveResults save = new SaveResults();
                save.save_res(name, all_machines, all_processes, products_quant, alg_mode, res, aa, c, d, e, f, g, t, start_pop, max_iterations, population, best_percentage, mut_percentage);
            }



            Console.ReadKey();
        }
    }
}

