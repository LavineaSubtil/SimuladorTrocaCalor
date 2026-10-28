using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SimuladorTrocaCalor
{
    [System.ComponentModel.DesignerCategory("Code")]
    public class FormPrincipal : Form
    {
        private const double TemperaturaMaxima = 40000.0;
        private const double TemperaturaMaximaLegenda = 12000.0;
        private const double DtSegundos = 1.0;

        private readonly Simulador simulador = new Simulador();
        private readonly System.Windows.Forms.Timer timerSimulacao = new System.Windows.Forms.Timer();
        private readonly System.Windows.Forms.Timer timerClique = new System.Windows.Forms.Timer();
        private readonly ToolTip dica = new ToolTip();

        private MouseButtons botaoPressionado = MouseButtons.None;
        private Point posicaoMouse;
        private Corpo corpoSobMouse;
        private readonly List<Corpo> selecionados = new List<Corpo>();

        private int simulacaoIdAtual = 0;
        private bool salvarNoBancoAtivo = false;

        // Controles de interface
        private NumericUpDown numN;
        private ComboBox cmbMaterialCriacao;
        private NumericUpDown numLado;

        private RadioButton rbTemperatura;
        private RadioButton rbMaterial;
        private NumericUpDown numVariacao;
        private ComboBox cmbMaterialClique;
        private RadioButton rbSelecionar;

        private Label lblSelecao;
        private ComboBox cmbMaterialEdicao;
        private NumericUpDown numTempEdicao;

        private TrackBar trkVelocidade;
        private ComboBox cmbCriterio;
        private CheckBox chkParalelo;
        private CheckBox chkSalvarBanco;
        private Button btnIniciar;
        private Label lblStatus;

        private PainelDuploBuffer painelMatriz;
        private PainelDuploBuffer painelLegenda;

        public FormPrincipal()
        {
            Text = "Simulador de Troca de Calor — N2 (Computação Paralela + MySQL)";
            Size = new Size(1150, 860);
            MinimumSize = new Size(850, 650);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9f);

            MontarInterface();

            timerSimulacao.Interval = 50;
            timerSimulacao.Tick += TimerSimulacao_Tick;

            timerClique.Interval = 100;
            timerClique.Tick += TimerClique_Tick;

            AtualizarStatus();
        }

        private void MontarInterface()
        {
            painelMatriz = new PainelDuploBuffer { Dock = DockStyle.Fill, BackColor = Color.FromArgb(45, 45, 52) };
            painelMatriz.Paint += PainelMatriz_Paint;
            painelMatriz.MouseDown += PainelMatriz_MouseDown;
            painelMatriz.MouseMove += PainelMatriz_MouseMove;
            painelMatriz.MouseUp += PainelMatriz_MouseUp;
            painelMatriz.MouseLeave += (s, e) => { corpoSobMouse = null; painelMatriz.Invalidate(); };

            painelLegenda = new PainelDuploBuffer { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(45, 45, 52) };
            painelLegenda.Paint += PainelLegenda_Paint;

            Panel painelEsquerdo = new Panel { Dock = DockStyle.Left, Width = 275, AutoScroll = true };
            painelEsquerdo.Controls.Add(CriarGrupoCorpos());
            painelEsquerdo.Controls.Add(CriarGrupoClique());
            painelEsquerdo.Controls.Add(CriarGrupoEdicao());
            painelEsquerdo.Controls.Add(CriarGrupoSimulacao());

            lblStatus = new Label { Location = new Point(8, 735), Size = new Size(255, 60), ForeColor = Color.DimGray };
            painelEsquerdo.Controls.Add(lblStatus);

            Controls.Add(painelMatriz);
            Controls.Add(painelLegenda);
            Controls.Add(painelEsquerdo);
            painelMatriz.BringToFront();
        }

        private GroupBox CriarGrupoCorpos()
        {
            GroupBox gb = new GroupBox { Text = "Corpos", Location = new Point(8, 8), Size = new Size(255, 152) };

            gb.Controls.Add(NovoRotulo("Tamanho N:", 10, 27));
            numN = NovoNumero(155, 24, 1, 60, 10, 0, 1);
            gb.Controls.Add(numN);

            gb.Controls.Add(NovoRotulo("Material:", 10, 57));
            cmbMaterialCriacao = NovoComboMateriais(90, 54, 150);
            gb.Controls.Add(cmbMaterialCriacao);

            gb.Controls.Add(NovoRotulo("Lado (m):", 10, 87));
            numLado = NovoNumero(155, 84, 0.001m, 10m, 0.1m, 3, 0.01m);
            gb.Controls.Add(numLado);

            Button btnCriar = new Button { Text = "Criar corpos (0 K)", Location = new Point(10, 114), Size = new Size(230, 28) };
            btnCriar.Click += BtnCriar_Click;
            gb.Controls.Add(btnCriar);

            return gb;
        }

        private GroupBox CriarGrupoClique()
        {
            GroupBox gb = new GroupBox { Text = "Ao clicar no quadrado", Location = new Point(8, 168), Size = new Size(255, 185) };

            rbTemperatura = new RadioButton { Text = "Aquecer / esfriar", Location = new Point(10, 22), Width = 230, Checked = true };
            gb.Controls.Add(rbTemperatura);

            gb.Controls.Add(new Label
            {
                Text = "Esquerdo aquece · Direito esfria",
                Location = new Point(28, 45),
                AutoSize = true,
                ForeColor = Color.DimGray
            });

            gb.Controls.Add(NovoRotulo("Variação (K):", 28, 70));
            numVariacao = NovoNumero(155, 67, 1, 10000, 250, 0, 50);
            gb.Controls.Add(numVariacao);

            rbMaterial = new RadioButton { Text = "Trocar material", Location = new Point(10, 102), Width = 230 };
            gb.Controls.Add(rbMaterial);

            cmbMaterialClique = NovoComboMateriais(28, 124, 210);
            gb.Controls.Add(cmbMaterialClique);

            rbSelecionar = new RadioButton { Text = "Selecionar e editar (Ctrl = vários)", Location = new Point(10, 155), Width = 230 };
            gb.Controls.Add(rbSelecionar);

            return gb;
        }

        private GroupBox CriarGrupoEdicao()
        {
            GroupBox gb = new GroupBox { Text = "Editar selecionados", Location = new Point(8, 361), Size = new Size(255, 148) };

            lblSelecao = new Label { Location = new Point(10, 22), Size = new Size(235, 18), ForeColor = Color.DimGray };
            gb.Controls.Add(lblSelecao);

            gb.Controls.Add(NovoRotulo("Material:", 10, 50));
            cmbMaterialEdicao = NovoComboMateriais(90, 47, 150);
            gb.Controls.Add(cmbMaterialEdicao);

            gb.Controls.Add(NovoRotulo("Temperatura (K):", 10, 80));
            numTempEdicao = NovoNumero(155, 77, 0, (decimal)TemperaturaMaxima, 0, 1, 100);
            gb.Controls.Add(numTempEdicao);

            Button btnAplicar = new Button { Text = "Aplicar", Location = new Point(10, 110), Size = new Size(110, 28) };
            btnAplicar.Click += BtnAplicar_Click;
            gb.Controls.Add(btnAplicar);

            Button btnAplicarTodos = new Button { Text = "Aplicar a todos", Location = new Point(130, 110), Size = new Size(110, 28) };
            btnAplicarTodos.Click += BtnAplicarTodos_Click;
            gb.Controls.Add(btnAplicarTodos);

            return gb;
        }

        private GroupBox CriarGrupoSimulacao()
        {
            GroupBox gb = new GroupBox { Text = "Simulação (N2)", Location = new Point(8, 517), Size = new Size(255, 210) };

            gb.Controls.Add(NovoRotulo("Velocidade:", 10, 25));
            trkVelocidade = new TrackBar
            {
                Location = new Point(85, 20),
                Width = 160,
                Minimum = 1,
                Maximum = 200,
                Value = 20,
                TickStyle = TickStyle.None,
                AutoSize = false,
                Height = 25
            };
            gb.Controls.Add(trkVelocidade);

            gb.Controls.Add(NovoRotulo("Fluxo:", 10, 52));
            cmbCriterio = new ComboBox { Location = new Point(60, 49), Width = 185, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCriterio.Items.Add("Maior temperatura cede");
            cmbCriterio.Items.Add("Maior calor sensível cede");
            cmbCriterio.SelectedIndex = 0;
            cmbCriterio.SelectedIndexChanged += (s, e) =>
                simulador.Criterio = cmbCriterio.SelectedIndex == 0 ? CriterioSentido.Temperatura : CriterioSentido.CalorSensivel;
            gb.Controls.Add(cmbCriterio);

            chkParalelo = new CheckBox { Text = "Computação Paralela (N2)", Location = new Point(10, 80), Width = 230, Checked = true };
            chkParalelo.CheckedChanged += (s, e) => simulador.UsarParalelismo = chkParalelo.Checked;
            gb.Controls.Add(chkParalelo);

            chkSalvarBanco = new CheckBox { Text = "Gravar iterações no MySQL", Location = new Point(10, 105), Width = 230 };
            chkSalvarBanco.CheckedChanged += ChkSalvarBanco_CheckedChanged;
            gb.Controls.Add(chkSalvarBanco);

            btnIniciar = new Button { Text = "▶ Iniciar", Location = new Point(10, 135), Size = new Size(110, 28) };
            btnIniciar.Click += BtnIniciar_Click;
            gb.Controls.Add(btnIniciar);

            Button btnZerar = new Button { Text = "Zerar (0 K)", Location = new Point(130, 135), Size = new Size(110, 28) };
            btnZerar.Click += BtnZerar_Click;
            gb.Controls.Add(btnZerar);

            return gb;
        }

        private static Label NovoRotulo(string texto, int x, int y) => new Label { Text = texto, Location = new Point(x, y), AutoSize = true };

        private static NumericUpDown NovoNumero(int x, int y, decimal min, decimal max, decimal valor, int casas, decimal incremento)
        {
            return new NumericUpDown
            {
                Location = new Point(x, y),
                Width = 85,
                Minimum = min,
                Maximum = max,
                DecimalPlaces = casas,
                Increment = incremento,
                Value = valor
            };
        }

        private static ComboBox NovoComboMateriais(int x, int y, int largura)
        {
            ComboBox cmb = new ComboBox { Location = new Point(x, y), Width = largura, DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (Material m in Material.Catalogo) cmb.Items.Add(m);
            cmb.SelectedIndex = 0;
            return cmb;
        }

        private void ChkSalvarBanco_CheckedChanged(object sender, EventArgs e)
        {
            salvarNoBancoAtivo = chkSalvarBanco.Checked;
            if (salvarNoBancoAtivo && simulador.Corpos != null && simulacaoIdAtual == 0)
            {
                try
                {
                    simulacaoIdAtual = BancoDados.CriarSimulacao(simulador.N, (double)numLado.Value, simulador.Criterio.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao conectar com MySQL: " + ex.Message, "Erro BD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    chkSalvarBanco.Checked = false;
                }
            }
        }

        private void BtnCriar_Click(object sender, EventArgs e)
        {
            PararSimulacao();
            simulacaoIdAtual = 0;

            simulador.CriarMatriz(
                (int)numN.Value,
                (Material)cmbMaterialCriacao.SelectedItem,
                (double)numLado.Value,
                0.0);

            corpoSobMouse = null;
            selecionados.Clear();
            AtualizarTela();
        }

        private void BtnAplicar_Click(object sender, EventArgs e)
        {
            if (selecionados.Count == 0) return;
            foreach (Corpo c in selecionados) AplicarEdicao(c);
            AtualizarTela();
        }

        private void BtnAplicarTodos_Click(object sender, EventArgs e)
        {
            if (simulador.Corpos == null) return;
            foreach (Corpo c in simulador.Corpos) AplicarEdicao(c);
            AtualizarTela();
        }

        private void AplicarEdicao(Corpo c)
        {
            c.Material = (Material)cmbMaterialEdicao.SelectedItem;
            c.Temperatura = (double)numTempEdicao.Value;
        }

        private void BtnIniciar_Click(object sender, EventArgs e)
        {
            if (simulador.Corpos == null)
            {
                MessageBox.Show("Crie os corpos primeiro.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (timerSimulacao.Enabled)
                PararSimulacao();
            else
            {
                if (salvarNoBancoAtivo && simulacaoIdAtual == 0)
                {
                    try
                    {
                        simulacaoIdAtual = BancoDados.CriarSimulacao(simulador.N, (double)numLado.Value, simulador.Criterio.ToString());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erro MySQL: " + ex.Message, "Erro BD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        chkSalvarBanco.Checked = false;
                    }
                }
                timerSimulacao.Start();
                btnIniciar.Text = "❚❚ Pausar";
            }
        }

        private void BtnZerar_Click(object sender, EventArgs e)
        {
            PararSimulacao();
            simulador.ZerarTemperaturas();
            simulacaoIdAtual = 0;
            AtualizarTela();
        }

        private void PararSimulacao()
        {
            timerSimulacao.Stop();
            btnIniciar.Text = "▶ Iniciar";
        }

        private void TimerSimulacao_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < trkVelocidade.Value; i++)
            {
                simulador.Passo(DtSegundos);

                if (salvarNoBancoAtivo && simulacaoIdAtual > 0)
                {
                    try
                    {
                        BancoDados.SalvarIteracao(simulacaoIdAtual, simulador.NumeroPassos, simulador.TempoSimulado, simulador.Corpos);
                    }
                    catch (Exception ex)
                    {
                        PararSimulacao();
                        MessageBox.Show("Falha ao salvar no banco: " + ex.Message);
                        break;
                    }
                }
            }

            AtualizarTela();
        }

        private void PainelMatriz_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right) return;
            botaoPressionado = e.Button;
            posicaoMouse = e.Location;
            AplicarClique();
            if (rbTemperatura.Checked) timerClique.Start();
        }

        private void PainelMatriz_MouseMove(object sender, MouseEventArgs e)
        {
            posicaoMouse = e.Location;
            Corpo corpo = ObterCorpoNaPosicao(e.Location);
            if (corpo != corpoSobMouse)
            {
                corpoSobMouse = corpo;
                AtualizarDica();
                painelMatriz.Invalidate();
            }
            if (botaoPressionado == MouseButtons.Left && rbMaterial.Checked)
                AplicarClique();
        }

        private void PainelMatriz_MouseUp(object sender, MouseEventArgs e)
        {
            botaoPressionado = MouseButtons.None;
            timerClique.Stop();
        }

        private void TimerClique_Tick(object sender, EventArgs e)
        {
            if (botaoPressionado == MouseButtons.None) { timerClique.Stop(); return; }
            AplicarClique();
        }

        private void AplicarClique()
        {
            Corpo corpo = ObterCorpoNaPosicao(posicaoMouse);
            if (rbSelecionar.Checked) { Selecionar(corpo); return; }
            if (corpo == null) return;

            if (rbMaterial.Checked)
            {
                corpo.Material = (Material)cmbMaterialClique.SelectedItem;
            }
            else
            {
                double variacao = (double)numVariacao.Value;
                if (botaoPressionado == MouseButtons.Right) variacao = -variacao;
                corpo.Temperatura = Math.Clamp(corpo.Temperatura + variacao, 0.0, TemperaturaMaxima);
            }

            AtualizarDica();
            AtualizarTela();
        }

        private void Selecionar(Corpo corpo)
        {
            bool ctrl = (ModifierKeys & Keys.Control) == Keys.Control;
            if (!ctrl) selecionados.Clear();

            if (corpo != null)
            {
                if (ctrl && selecionados.Contains(corpo)) selecionados.Remove(corpo);
                else if (!selecionados.Contains(corpo)) selecionados.Add(corpo);
            }

            if (selecionados.Count == 1)
            {
                Corpo c = selecionados[0];
                cmbMaterialEdicao.SelectedItem = c.Material;
                numTempEdicao.Value = (decimal)Math.Clamp(c.Temperatura, 0.0, TemperaturaMaxima);
            }

            AtualizarTela();
        }

        private void AtualizarDica()
        {
            if (corpoSobMouse == null) { dica.SetToolTip(painelMatriz, null); return; }
            Corpo c = corpoSobMouse;
            dica.SetToolTip(painelMatriz, $"{c.Material.Nome} — {c.Temperatura:0.#} K\nMassa: {c.Massa:0.###} kg\nCalor: {c.CalorSensivel:N0} J");
        }

        private void CalcularGeometria(out float tamanhoCelula, out float x0, out float y0)
        {
            const float margem = 20f;
            int n = simulador.N;
            float larguraUtil = painelMatriz.ClientSize.Width - 2 * margem;
            float alturaUtil = painelMatriz.ClientSize.Height - 2 * margem;

            tamanhoCelula = Math.Max(1f, Math.Min(larguraUtil, alturaUtil) / n);
            x0 = (painelMatriz.ClientSize.Width - tamanhoCelula * n) / 2f;
            y0 = (painelMatriz.ClientSize.Height - tamanhoCelula * n) / 2f;
        }

        private Corpo ObterCorpoNaPosicao(Point ponto)
        {
            if (simulador.Corpos == null) return null;
            CalcularGeometria(out float tam, out float x0, out float y0);
            int coluna = (int)Math.Floor((ponto.X - x0) / tam);
            int linha = (int)Math.Floor((ponto.Y - y0) / tam);

            if (linha < 0 || coluna < 0 || linha >= simulador.N || coluna >= simulador.N) return null;
            return simulador.Corpos[linha, coluna];
        }

        private void PainelMatriz_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (simulador.Corpos == null)
            {
                TextRenderer.DrawText(g, "Escolha N, material e lado e clique em \"Criar corpos\".",
                    Font, painelMatriz.ClientRectangle, Color.Gainsboro,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            CalcularGeometria(out float tam, out float x0, out float y0);
            int n = simulador.N;
            bool escreverTexto = tam >= 36;

            using (Font fonte = new Font("Segoe UI", Math.Max(7f, tam / 7f), FontStyle.Bold))
            using (Font fonteMaterial = new Font("Segoe UI", Math.Max(6f, tam / 11f)))
            using (Pen borda = new Pen(Color.FromArgb(150, 150, 160), 1f))
            using (Pen destaque = new Pen(Color.DeepSkyBlue, 3f))
            using (Pen selecao = new Pen(Color.LimeGreen, 4f))
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        Corpo corpo = simulador.Corpos[i, j];
                        RectangleF r = new RectangleF(x0 + j * tam, y0 + i * tam, tam, tam);
                        Color cor = CorPorTemperatura.Obter(corpo.Temperatura);
                        using (SolidBrush pincel = new SolidBrush(cor)) g.FillRectangle(pincel, r);
                        g.DrawRectangle(borda, r.X, r.Y, r.Width, r.Height);

                        if (escreverTexto)
                        {
                            Color corTexto = CorPorTemperatura.CorDoTexto(cor);
                            Rectangle ri = Rectangle.Round(r);
                            Rectangle cima = new Rectangle(ri.X, ri.Y, ri.Width, ri.Height * 3 / 5);
                            Rectangle baixo = new Rectangle(ri.X + 2, ri.Y + ri.Height * 3 / 5, ri.Width - 4, ri.Height * 2 / 5);

                            TextRenderer.DrawText(g, corpo.Temperatura.ToString("0") + " K", fonte, cima, corTexto, TextFormatFlags.HorizontalCenter | TextFormatFlags.Bottom | TextFormatFlags.SingleLine);
                            TextRenderer.DrawText(g, corpo.Material.Nome, fonteMaterial, baixo, corTexto, TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
                        }
                    }
                }

                foreach (Corpo c in selecionados)
                    g.DrawRectangle(selecao, x0 + c.Coluna * tam + 2f, y0 + c.Linha * tam + 2f, tam - 4f, tam - 4f);

                if (corpoSobMouse != null)
                    g.DrawRectangle(destaque, x0 + corpoSobMouse.Coluna * tam + 1.5f, y0 + corpoSobMouse.Linha * tam + 1.5f, tam - 3f, tam - 3f);
            }
        }

        private void PainelLegenda_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            int margem = 35;
            int largura = painelLegenda.ClientSize.Width - 2 * margem;
            if (largura <= 1) return;

            Rectangle barra = new Rectangle(margem, 6, largura, 16);

            for (int x = 0; x < largura; x++)
            {
                double kelvin = TemperaturaMaximaLegenda * x / (largura - 1);
                using (Pen p = new Pen(CorPorTemperatura.Obter(kelvin)))
                    g.DrawLine(p, barra.X + x, barra.Top, barra.X + x, barra.Bottom);
            }
            g.DrawRectangle(Pens.Gray, barra);

            double[] marcas = { 0, 1000, 2000, 4000, 6000, 8000, 10000, 12000 };
            foreach (double k in marcas)
            {
                int x = barra.X + (int)((largura - 1) * k / TemperaturaMaximaLegenda);
                string texto = k.ToString("0") + " K";
                Size tamTexto = TextRenderer.MeasureText(texto, Font);
                TextRenderer.DrawText(g, texto, Font, new Point(x - tamTexto.Width / 2, barra.Bottom + 3), Color.Gainsboro);
            }
        }

        private void AtualizarTela()
        {
            AtualizarStatus();
            AtualizarTextoSelecao();
            painelMatriz.Invalidate();
        }

        private void AtualizarTextoSelecao()
        {
            if (selecionados.Count == 0) lblSelecao.Text = "Nenhum quadrado selecionado";
            else if (selecionados.Count == 1) lblSelecao.Text = $"Linha {selecionados[0].Linha + 1}, coluna {selecionados[0].Coluna + 1}";
            else lblSelecao.Text = $"{selecionados.Count} quadrados selecionados";
        }

        private void AtualizarStatus()
        {
            if (simulador.Corpos == null) { lblStatus.Text = ""; return; }
            double t = simulador.TempoSimulado;
            string tempo = t < 60 ? $"{t:0} s" : t < 3600 ? $"{t / 60:0.#} min" : $"{t / 3600:0.##} h";
            string modo = simulador.UsarParalelismo ? "Paralelo (N2)" : "Sequencial (N1)";
            lblStatus.Text = $"Modo: {modo}\nTempo simulado: {tempo}\nEnergia: {simulador.EnergiaTotal():N0} J\nÚltimo cálculo: {simulador.UltimoTempoExecucaoMs} ms";
        }
    }

    /// <summary>
    /// Painel customizado com DoubleBuffered para o desenho da tela não piscar.
    /// </summary>
    [System.ComponentModel.DesignerCategory("Code")]
    internal class PainelDuploBuffer : Panel
    {
        public PainelDuploBuffer()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }
    }
}