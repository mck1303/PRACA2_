using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opracowanie_heurystyk
{
    public class SaveResults
    {
        public void save_res(string name, List<Machine> all_machines, List<Process> all_processes, double[] start_quant, int alg_mode, List<List<OP>> res, double a, double c, double d, double e, double f, double g, double t, int start_pop, int max_iterations, int population, int best_percentage, int mut_percentage)
        {
            string paths = "C:\\Users\\Maciek\\Desktop\\PRACA2_\\WYNIKI\\";
            using (StreamWriter writer = new StreamWriter(paths + name + ".txt"))
            {
                writer.WriteLine("Wyniki testu o kryptonimie: {0}", name);
                writer.WriteLine("Maszyny:");
                for (int i = 0; i < all_machines.Count; i++)
                {
                    writer.WriteLine("Maszyna id={0}", all_machines[i].id);
                    writer.Write("Czas trwania kolejnych operacji:");
                    for (int j = 0; j < all_machines[i].operation_time.Count(); j++)
                    {
                        if (j != all_machines[i].operation_time.Count() - 1)
                        {
                            writer.Write(" {0}", all_machines[i].operation_time[j]);
                        }
                        else
                        {
                            writer.WriteLine(" {0}", all_machines[i].operation_time[j]);
                        }
                    }
                    writer.WriteLine("Czas trwania dostosowania maszyny z jednej operacji na drugą:");
                    for (int j = 0; j < all_machines[i].change_matrix.GetUpperBound(0) + 1; j++)
                    {
                        for (int k = 0; k < all_machines[i].change_matrix.GetUpperBound(1) + 1; k++)
                            if (k != all_machines[i].change_matrix.GetUpperBound(1))
                            {
                                writer.Write(" {0}", all_machines[i].change_matrix[j, k]);
                            }
                            else
                            {
                                writer.WriteLine(" {0}", all_machines[i].change_matrix[j, k]);
                            }
                    }

                }
                writer.WriteLine("");
                writer.WriteLine("Procesy:");
                for (int i = 0; i < all_processes.Count; i++)
                {
                    writer.WriteLine("Proces o id= {0}", all_processes[i].id);
                    writer.WriteLine("Priorytet: {0}", all_processes[i].priority);
                    writer.WriteLine("Operacje wchodzące w skład procesu:");
                    for (int j = 0; j < all_processes[i].operations.Count; j++)
                    {
                        writer.WriteLine("");
                        writer.WriteLine("Operacja o id={0}", all_processes[i].operations[j].id);
                        writer.WriteLine("Czas po którym musi się rozpocząć po zakończeniu poprzedniej: {0}", all_processes[i].operations[j].time_after_previous);
                        writer.WriteLine("Czas po którym musi się rozpocząć kolejna operacja: {0}", all_processes[i].operations[j].time_before_next);
                        writer.WriteLine("Czy można pauzować?: {0}", all_processes[i].operations[j].canPause);
                        if (all_processes[i].operations[j].canPause)
                        {
                            writer.WriteLine("Maksymalny czas pauzy: {0}", all_processes[i].operations[j].maxPauseTime);
                            writer.WriteLine("Maksymalna ilość pauz (-1 oznacza nieskończoność): {0}", all_processes[i].operations[j].pauseCount);
                        }
                        writer.WriteLine("Potrzebne materiały (ostatnia kolumna oznacza % postępu, w którym są potrzebne):");
                        for (int k = 0; k < all_processes[i].operations[j].needed_sources.Count; k++)
                        {
                            for (int l = 0; l < all_processes[i].operations[j].needed_sources[k].Count; l++)
                            {
                                if (l != all_processes[i].operations[j].needed_sources[k].Count - 1)
                                {
                                    writer.Write(" {0}", all_processes[i].operations[j].needed_sources[k][l]);
                                }
                                else
                                {
                                    writer.WriteLine(" {0}", all_processes[i].operations[j].needed_sources[k][l]);
                                }
                            }
                        }
                        writer.WriteLine("Produkowane materiały (ostatnia kolumna oznacza % postępu, w którym są produkowane):");
                        for (int k = 0; k < all_processes[i].operations[j].produced_products.Count; k++)
                        {
                            for (int l = 0; l < all_processes[i].operations[j].produced_products[k].Count; l++)
                            {
                                if (l != all_processes[i].operations[j].produced_products[k].Count - 1)
                                {
                                    writer.Write(" {0}", all_processes[i].operations[j].produced_products[k][l]);
                                }
                                else
                                {
                                    writer.WriteLine(" {0}", all_processes[i].operations[j].produced_products[k][l]);
                                }
                            }
                        }

                    }
                }
                writer.WriteLine("");
                writer.Write("Liczba produktów na początku procesu produkcyjnego:");
                for (int i = 0; i < start_quant.Length; i++)
                {
                    if (i != start_quant.Length - 1)
                    {
                        writer.Write(" {0}", start_quant[i]);
                    }
                    else
                    {
                        writer.WriteLine(" {0}", start_quant[i]);
                    }
                }
                writer.WriteLine("");
                writer.WriteLine("Ustawienia algorytmów:");
                if (alg_mode == 1)
                {
                    writer.WriteLine("Algorytm genetyczny");
                }
                else if (alg_mode == 2)
                {
                    writer.WriteLine("Algorytm genetyczny z początkowym algorytmem Johnsona");
                }
                else if (alg_mode == 3)
                {
                    writer.WriteLine("Algorytm genetyczny z algorytmem Johnsona");
                }
                writer.WriteLine("Populacja początkowa: {0}", start_pop);
                writer.WriteLine("Maksymalna populacja: {0}", max_iterations);
                writer.WriteLine("Procent najlepszych osobników do reprodukcji: {0}", population);
                writer.WriteLine("Procent mutowanych osobników: {0}", best_percentage);
                writer.WriteLine("Populacja początkowa: {0}", mut_percentage);
                writer.WriteLine("");
                writer.WriteLine("Wagi: a:{0},c:{1},d:{2},e:{3},f:{4},g:{5},t:{6}", a, c, d, e, f, g, t);
            }
        }
    }
}
