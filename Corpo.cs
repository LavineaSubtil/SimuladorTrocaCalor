namespace SimuladorTrocaCalor
{
    /// <summary>
    /// Um corpo cúbico da matriz N x N.
    /// Guarda apenas o que é "estado" do corpo (material, lado, temperatura);
    /// as grandezas derivadas (volume, massa, calor sensível...) são calculadas
    /// na hora, assim nunca ficam desatualizadas quando o material muda.
    /// </summary>
    public class Corpo
    {
        /// <summary>Linha do corpo na matriz (começa em 0).</summary>
        public int Linha { get; }

        /// <summary>Coluna do corpo na matriz (começa em 0).</summary>
        public int Coluna { get; }

        /// <summary>Material do corpo. Pode ser alterado depois da criação.</summary>
        public Material Material { get; set; }

        /// <summary>Tamanho da aresta do cubo, em metros.</summary>
        public double Lado { get; }

        /// <summary>Temperatura atual, em kelvin.</summary>
        public double Temperatura { get; set; }

        public Corpo(int linha, int coluna, Material material, double lado, double temperatura)
        {
            Linha = linha;
            Coluna = coluna;
            Material = material;
            Lado = lado;
            Temperatura = temperatura;
        }

        /// <summary>Volume do cubo: V = L³ (m³).</summary>
        public double Volume => Lado * Lado * Lado;

        /// <summary>Área de uma face do cubo: A = L² (m²). É por ela que o calor passa para o vizinho.</summary>
        public double AreaFace => Lado * Lado;

        /// <summary>Massa: m = ρ·V (kg).</summary>
        public double Massa => Material.Densidade * Volume;

        /// <summary>
        /// Capacidade térmica C = m·c (J/K).
        /// Diz quantos joules o corpo precisa receber para subir 1 K.
        /// </summary>
        public double CapacidadeTermica => Massa * Material.CalorEspecifico;

        /// <summary>
        /// Calor sensível conforme o enunciado: Q = m·c·ΔT, com ΔT medido
        /// de 0 K até a temperatura atual, ou seja, Q = m·c·T (em J).
        /// </summary>
        public double CalorSensivel => CapacidadeTermica * Temperatura;
    }
}
