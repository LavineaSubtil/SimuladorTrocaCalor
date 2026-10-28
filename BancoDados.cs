using System;
using MySql.Data.MySqlClient;

namespace SimuladorTrocaCalor
{
	/// <summary>
	/// Classe responsável pela ligação com o MySQL e gravação dos dados da simulação.
	/// </summary>
	public class BancoDados
	{
		// Altere a senha em Pwd=root; caso tenha definido outra na instalação do MySQL
		private const string ConnectionString = "Server=localhost;Database=simulador_calor;Uid=root;Pwd=root;";

		/// <summary>
		/// Testa se a conexão com o MySQL está funcionando.
		/// </summary>
		public static bool TestarConexao()
		{
			try
			{
				using (var conn = new MySqlConnection(ConnectionString))
				{
					conn.Open();
					return true;
				}
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Registra o início de uma nova simulação e devolve o ID gerado.
		/// </summary>
		public static int CriarSimulacao(int n, double lado, string criterio)
		{
			using (var conn = new MySqlConnection(ConnectionString))
			{
				conn.Open();
				string sql = "INSERT INTO simulacao (data_criacao, tamanho_n, lado_cubo, criterio) VALUES (@data, @n, @lado, @criterio); SELECT LAST_INSERT_ID();";
				using (var cmd = new MySqlCommand(sql, conn))
				{
					cmd.Parameters.AddWithValue("@data", DateTime.Now);
					cmd.Parameters.AddWithValue("@n", n);
					cmd.Parameters.AddWithValue("@lado", lado);
					cmd.Parameters.AddWithValue("@criterio", criterio);
					return Convert.ToInt32(cmd.ExecuteScalar());
				}
			}
		}

		/// <summary>
		/// Grava o resultado de UMA iteração e o estado de todos os corpos no MySQL.
		/// </summary>
		public static void SalvarIteracao(int simulacaoId, int numeroPasso, double tempoSimulado, Corpo[,] matriz)
		{
			using (var conn = new MySqlConnection(ConnectionString))
			{
				conn.Open();

				using (var transacao = conn.BeginTransaction())
				{
					// 1. Inserir o registro da iteração
					string sqlIteracao = "INSERT INTO iteracao (simulacao_id, numero_passo, tempo_simulado) VALUES (@simId, @passo, @tempo); SELECT LAST_INSERT_ID();";
					int iteracaoId;
					using (var cmdIter = new MySqlCommand(sqlIteracao, conn, transacao))
					{
						cmdIter.Parameters.AddWithValue("@simId", simulacaoId);
						cmdIter.Parameters.AddWithValue("@passo", numeroPasso);
						cmdIter.Parameters.AddWithValue("@tempo", tempoSimulado);
						iteracaoId = Convert.ToInt32(cmdIter.ExecuteScalar());
					}

					// 2. Inserir o estado de cada corpo pertencente a esta iteração
					int n = matriz.GetLength(0);
					string sqlCorpo = "INSERT INTO corpo_estado (iteracao_id, linha, coluna, material_nome, temperatura) VALUES (@iterId, @linha, @coluna, @material, @temp);";

					for (int i = 0; i < n; i++)
					{
						for (int j = 0; j < n; j++)
						{
							Corpo c = matriz[i, j];
							using (var cmdCorpo = new MySqlCommand(sqlCorpo, conn, transacao))
							{
								cmdCorpo.Parameters.AddWithValue("@iterId", iteracaoId);
								cmdCorpo.Parameters.AddWithValue("@linha", c.Linha);
								cmdCorpo.Parameters.AddWithValue("@coluna", c.Coluna);
								cmdCorpo.Parameters.AddWithValue("@material", c.Material.Nome);
								cmdCorpo.Parameters.AddWithValue("@temp", c.Temperatura);
								cmdCorpo.ExecuteNonQuery();
							}
						}
					}

					transacao.Commit();
				}
			}
		}
	}
}