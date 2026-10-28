using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SimuladorTrocaCalor
{
    public enum CriterioSentido
    {
        Temperatura,
        CalorSensivel
    }

    /// <summary>
    /// Faz a simulação da troca de calor entre os corpos da matriz.
    /// Atualizado para a N2 com suporte a computação paralela (Parallel.For).
    /// </summary>
    public class Simulador
    {
        private const int MaxVizinhos = 4;

        public Corpo[,] Corpos { get; private set; }
        public int N => Corpos == null ? 0 : Corpos.GetLength(0);
        public double TempoSimulado { get; private set; }
        public int NumeroPassos { get; private set; }
        public CriterioSentido Criterio { get; set; } = CriterioSentido.Temperatura;

        // Opção para alternar entre computação paralela (N2) e sequencial (N1)
        public bool UsarParalelismo { get; set; } = true;

        // Armazena o tempo em milissegundos que levou para calcular o último passo
        public long UltimoTempoExecucaoMs { get; private set; }

        public void CriarMatriz(int n, Material material, double lado, double temperaturaInicial)
        {
            if (n <= 0) throw new ArgumentException("N deve ser maior que zero.");
            if (lado <= 0) throw new ArgumentException("O lado deve ser maior que zero.");

            Corpos = new Corpo[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    Corpos[i, j] = new Corpo(i, j, material, lado, temperaturaInicial);

            TempoSimulado = 0;
            NumeroPassos = 0;
        }

        public void ZerarTemperaturas()
        {
            if (Corpos == null) return;
            foreach (Corpo c in Corpos) c.Temperatura = 0;
            TempoSimulado = 0;
            NumeroPassos = 0;
        }

        public void Passo(double dt)
        {
            if (Corpos == null || dt <= 0) return;

            Stopwatch sw = Stopwatch.StartNew();
            int n = N;
            double[,] saldo = new double[n, n];

            if (UsarParalelismo)
            {
                // =================================================================
                // N2: COMPUTAÇÃO PARALELA (Utiliza múltiplos núcleos do processador)
                // =================================================================

                // 1. Trocas horizontais em paralelo
                Parallel.For(0, n, i =>
                {
                    for (int j = 0; j < n - 1; j++)
                    {
                        CalcularEAcumularTroca(Corpos[i, j], Corpos[i, j + 1], dt, saldo);
                    }
                });

                // 2. Trocas verticais em paralelo
                Parallel.For(0, n - 1, i =>
                {
                    for (int j = 0; j < n; j++)
                    {
                        CalcularEAcumularTroca(Corpos[i, j], Corpos[i + 1, j], dt, saldo);
                    }
                });

                // 3. Atualização das temperaturas em paralelo
                Parallel.For(0, n, i =>
                {
                    for (int j = 0; j < n; j++)
                    {
                        Corpo c = Corpos[i, j];
                        c.Temperatura += saldo[i, j] / c.CapacidadeTermica;
                        if (c.Temperatura < 0) c.Temperatura = 0;
                    }
                });
            }
            else
            {
                // =================================================================
                // N1: COMPUTAÇÃO SEQUENCIAL (Apenas 1 núcleo / 1 thread)
                // =================================================================
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (j + 1 < n) CalcularEAcumularTroca(Corpos[i, j], Corpos[i, j + 1], dt, saldo);
                        if (i + 1 < n) CalcularEAcumularTroca(Corpos[i, j], Corpos[i + 1, j], dt, saldo);
                    }
                }

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        Corpo c = Corpos[i, j];
                        c.Temperatura += saldo[i, j] / c.CapacidadeTermica;
                        if (c.Temperatura < 0) c.Temperatura = 0;
                    }
                }
            }

            sw.Stop();
            UltimoTempoExecucaoMs = sw.ElapsedMilliseconds;

            TempoSimulado += dt;
            NumeroPassos++;
        }

        private void CalcularEAcumularTroca(Corpo a, Corpo b, double dt, double[,] saldo)
        {
            double k = Math.Min(a.Material.CondutividadeK, b.Material.CondutividadeK);
            double ladoContato = Math.Min(a.Lado, b.Lado);
            double area = ladoContato * ladoContato;
            double deltaT = Math.Abs(a.Temperatura - b.Temperatura);

            if (deltaT == 0) return;

            double q = k * area * deltaT;
            double energia = q * dt;

            Corpo fonte, destino;
            double limite;

            if (Criterio == CriterioSentido.Temperatura)
            {
                fonte = a.Temperatura > b.Temperatura ? a : b;
                destino = fonte == a ? b : a;

                double cf = fonte.CapacidadeTermica;
                double cd = destino.CapacidadeTermica;
                limite = (fonte.Temperatura - destino.Temperatura) * cf * cd / (cf + cd);
            }
            else
            {
                if (a.CalorSensivel == b.CalorSensivel) return;
                fonte = a.CalorSensivel > b.CalorSensivel ? a : b;
                destino = fonte == a ? b : a;
                limite = (fonte.CalorSensivel - destino.CalorSensivel) / 2.0;
            }

            energia = Math.Min(energia, limite / MaxVizinhos);

            // O comando 'lock' impede conflitos de memória quando várias threads salvam no saldo ao mesmo tempo
            lock (saldo)
            {
                saldo[fonte.Linha, fonte.Coluna] -= energia;
                saldo[destino.Linha, destino.Coluna] += energia;
            }
        }

        public double EnergiaTotal()
        {
            double soma = 0;
            if (Corpos != null)
                foreach (Corpo c in Corpos) soma += c.CalorSensivel;
            return soma;
        }
    }
}