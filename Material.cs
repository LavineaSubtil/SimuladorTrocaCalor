using System.Collections.Generic;

namespace SimuladorTrocaCalor
{
    /// <summary>
    /// Representa um material e suas propriedades térmicas, todas no SI.
    /// Cada corpo da matriz aponta para um Material; trocar o material de um
    /// corpo é só trocar essa referência (a massa é recalculada automaticamente).
    /// </summary>
    public class Material
    {
        /// <summary>Nome exibido na interface.</summary>
        public string Nome { get; }

        /// <summary>Condutividade térmica k, em W/(m·K). Usada na Lei de Fourier.</summary>
        public double CondutividadeK { get; }

        /// <summary>Calor específico c, em J/(kg·K). Usado em Q = m·c·ΔT.</summary>
        public double CalorEspecifico { get; }

        /// <summary>Densidade ρ, em kg/m³. Necessária para obter a massa: m = ρ·V.</summary>
        public double Densidade { get; }

        public Material(string nome, double condutividadeK, double calorEspecifico, double densidade)
        {
            Nome = nome;
            CondutividadeK = condutividadeK;
            CalorEspecifico = calorEspecifico;
            Densidade = densidade;
        }

        /// <summary>
        /// O ComboBox usa ToString() para mostrar o item, então devolvemos o nome.
        /// </summary>
        public override string ToString() => Nome;

        /// <summary>
        /// Catálogo de materiais disponíveis (o enunciado exige pelo menos 8).
        /// Valores típicos de tabelas de propriedades térmicas, próximos de 300 K.
        ///                         nome                 k (W/m·K)  c (J/kg·K)  ρ (kg/m³)
        /// </summary>
        public static readonly IReadOnlyList<Material> Catalogo = new List<Material>
        {
            new Material("Cobre",                  401.0,      385.0,      8960.0),
            new Material("Alumínio",               237.0,      897.0,      2700.0),
            new Material("Prata",                  429.0,      235.0,     10490.0),
            new Material("Ouro",                   318.0,      129.0,     19300.0),
            new Material("Ferro",                   80.0,      449.0,      7874.0),
            new Material("Aço inoxidável",          16.0,      500.0,      8000.0),
            new Material("Chumbo",                  35.0,      128.0,     11340.0),
            new Material("Granito",                  2.8,      790.0,      2700.0),
            new Material("Concreto",                 1.4,      880.0,      2300.0),
            new Material("Vidro",                    0.8,      840.0,      2500.0),
            new Material("Madeira (pinho)",          0.12,    1700.0,       500.0),
        };
    }
}
