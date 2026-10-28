using System;
using System.Drawing;

namespace SimuladorTrocaCalor
{
    /// <summary>
    /// Converte uma temperatura em kelvin numa cor, utilizando o padrão de cores
    /// do MATLAB ('Jet') distribuído suavemente na faixa de 0 K a 12000 K.
    /// </summary>
    public static class CorPorTemperatura
    {
        public const double TemperaturaMinimaFormula = 0.0;
        public const double TemperaturaMaximaFormula = 12000.0;

        public static Color Obter(double kelvin)
        {
            if (kelvin <= TemperaturaMinimaFormula)
                return CalcularPelaFormula(TemperaturaMinimaFormula);

            return CalcularPelaFormula(Math.Min(kelvin, TemperaturaMaximaFormula));
        }

        /// <summary>
        /// Espectro completo de cores do MATLAB ('Jet') mapeado entre 0 K e 12000 K:
        ///      0 K  = Azul Escuro
        ///  ~1440 K  = Azul / Ciano
        ///  ~6000 K  = Verde
        ///  ~7440 K  = Amarelo
        ///  12000 K  = Vermelho Escuro
        /// </summary>
        private static Color CalcularPelaFormula(double kelvin)
        {
            // Normaliza kelvin proporcionalmente no intervalo [0.0, 1.0]
            double v = (kelvin - TemperaturaMinimaFormula) / (TemperaturaMaximaFormula - TemperaturaMinimaFormula);

            // Equações do mapa de cores 'Jet' do MATLAB
            double r = LimitarEmUnidade(1.5 - Math.Abs(4.0 * v - 3.0));
            double g = LimitarEmUnidade(1.5 - Math.Abs(4.0 * v - 2.0));
            double b = LimitarEmUnidade(1.5 - Math.Abs(4.0 * v - 1.0));

            return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        /// <summary>Garante que o fator normalizado fique no intervalo [0.0, 1.0].</summary>
        private static double LimitarEmUnidade(double valor)
        {
            if (valor < 0.0) return 0.0;
            if (valor > 1.0) return 1.0;
            return valor;
        }

        /// <summary>
        /// Escolhe preto ou branco para o texto escrito sobre a cor de fundo,
        /// usando a luminância percebida (fórmula padrão 0.299R + 0.587G + 0.114B).
        /// </summary>
        public static Color CorDoTexto(Color fundo)
        {
            double luminancia = 0.299 * fundo.R + 0.587 * fundo.G + 0.114 * fundo.B;
            return luminancia > 140 ? Color.Black : Color.White;
        }
    }
}
