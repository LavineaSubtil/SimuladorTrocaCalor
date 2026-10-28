-- Banco usado pelo Simulador de Troca de Calor (opcional).
-- Rode este script no MySQL antes de marcar "Gravar iterações no MySQL".

CREATE DATABASE IF NOT EXISTS simulador_calor;
USE simulador_calor;

CREATE TABLE IF NOT EXISTS simulacao (
    id            INT AUTO_INCREMENT PRIMARY KEY,
    data_criacao  DATETIME     NOT NULL,
    tamanho_n     INT          NOT NULL,
    lado_cubo     DOUBLE       NOT NULL,
    criterio      VARCHAR(30)  NOT NULL
);

CREATE TABLE IF NOT EXISTS iteracao (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    simulacao_id    INT    NOT NULL,
    numero_passo    INT    NOT NULL,
    tempo_simulado  DOUBLE NOT NULL,
    FOREIGN KEY (simulacao_id) REFERENCES simulacao(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS corpo_estado (
    id             INT AUTO_INCREMENT PRIMARY KEY,
    iteracao_id    INT          NOT NULL,
    linha          INT          NOT NULL,
    coluna         INT          NOT NULL,
    material_nome  VARCHAR(50)  NOT NULL,
    temperatura    DOUBLE       NOT NULL,
    FOREIGN KEY (iteracao_id) REFERENCES iteracao(id) ON DELETE CASCADE
);
