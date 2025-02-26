using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ASPPR_1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void LogMatrix(double[,] matrix, StringBuilder log)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                string row = "";
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    row += matrix[i, j].ToString("F2", CultureInfo.InvariantCulture).Replace(".", ",") + " ";
                }
                log.AppendLine(row.Trim());
            }
        }

        Random random = new Random();

        private void ShowMatrix(double[,] matrix, DataGridView grid)
        {
            grid.Columns.Clear();
            grid.Rows.Clear();

            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            for (int i = 0; i < cols; i++)
                grid.Columns.Add("", "");

            grid.RowCount = rows;

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    grid[j, i].Value = matrix[i, j].ToString();
        }

        private void ShowMatrixF2(double[,] matrix, DataGridView grid)
        {
            grid.Columns.Clear();
            grid.Rows.Clear();

            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            for (int i = 0; i < cols; i++)
                grid.Columns.Add("", "");

            grid.RowCount = rows;

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    grid[j, i].Value = matrix[i, j].ToString("F2");
        }

        private double[,] Inverse(double[,] matrix, StringBuilder log = null)
        {
            int n = matrix.GetLength(0);
            double[,] aug = new double[n, 2 * n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    aug[i, j] = matrix[i, j];
                aug[i, i + n] = 1;
            }

            if (log != null)
            {
                log.AppendLine("Знаходження оберненої матриці:");
                log.AppendLine("Вхідна матриця:");
                LogMatrix(matrix, log);
                log.AppendLine("Протокол обчислення:");
            }

            for (int i = 0; i < n; i++)
            {
                double pivot = aug[i, i];

                if (log != null)
                {
                    log.AppendLine($"Крок #{i + 1}");
                    log.AppendLine($"Розв’язувальний елемент: A[{i + 1}, {i + 1}] = {pivot.ToString("F2", CultureInfo.InvariantCulture).Replace(".", ",")}");
                }

                for (int j = 0; j < 2 * n; j++)
                    aug[i, j] /= pivot;

                for (int k = 0; k < n; k++)
                {
                    if (k != i)
                    {
                        double factor = aug[k, i];
                        for (int j = 0; j < 2 * n; j++)
                            aug[k, j] -= factor * aug[i, j];
                    }
                }

                if (log != null)
                {
                    log.AppendLine("Матриця після виконання ЗЖВ:");
                    double[,] currentInv = new double[n, n];
                    for (int x = 0; x < n; x++)
                        for (int y = 0; y < n; y++)
                            currentInv[x, y] = aug[x, y + n];
                    LogMatrix(currentInv, log);
                }
            }

            double[,] inverse = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    inverse[i, j] = aug[i, j + n];

            return inverse;
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            ShowMatrixF2(Inverse(GetMatrixFromGrid(dataGridViewA)), dataGridViewResult);
        }

        private int Rank(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int rank = 0;

            for (int row = 0; row < rows; row++)
            {
                int pivotCol = -1;
                for (int j = rank; j < cols; j++)
                {
                    if (Math.Abs(matrix[row, j]) > 1e-10)
                    {
                        pivotCol = j;
                        break;
                    }
                }

                if (pivotCol == -1) continue;

                if (pivotCol != rank)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        double temp = matrix[i, rank];
                        matrix[i, rank] = matrix[i, pivotCol];
                        matrix[i, pivotCol] = temp;
                    }
                }

                double pivotVal = matrix[row, rank];
                for (int j = rank; j < cols; j++)
                    matrix[row, j] /= pivotVal;

                for (int i = 0; i < rows; i++)
                {
                    if (i != row && Math.Abs(matrix[i, rank]) > 1e-10)
                    {
                        double factor = matrix[i, rank];
                        for (int j = rank; j < cols; j++)
                            matrix[i, j] -= factor * matrix[row, j];
                    }
                }

                rank++;
            }

            return rank;
        }

        private void btnRank_Click(object sender, EventArgs e)
        {
            try
            {
                double[,] A = GetMatrixFromGrid(dataGridViewA);
                int rank = Rank(A);
                txtRank.Text = rank.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private double[,] Multiply(double[,] a, double[,] b)
        {
            int m = a.GetLength(0);
            int n = b.GetLength(1);
            int p = a.GetLength(1);
            double[,] result = new double[m, n];

            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    for (int k = 0; k < p; k++)
                        result[i, j] += a[i, k] * b[k, j];

            return result;
        }

        private double[,] Multiply(double[,] a, double[,] b, StringBuilder log)
        {
            int m = a.GetLength(0);
            int n = b.GetLength(1);
            int p = a.GetLength(1);
            double[,] result = new double[m, n];

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (log != null)
                    {
                        var sbTerms = new StringBuilder();
                        for (int k = 0; k < p; k++)
                        {
                            sbTerms.Append($"{a[i, k].ToString("F2", CultureInfo.InvariantCulture)}*{b[k, j].ToString("F2", CultureInfo.InvariantCulture)}");
                            if (k < p - 1) sbTerms.Append(" + ");
                        }
                        log.AppendLine($"X[{i + 1}] = {sbTerms}");
                    }

                    for (int k = 0; k < p; k++)
                        result[i, j] += a[i, k] * b[k, j];

                    if (log != null)
                        log.AppendLine($"Результат X[{i + 1}] = {result[i, j].ToString("F2", CultureInfo.InvariantCulture)}\n");
                }
            }
            return result;
        }

        private void SaveLog(StringBuilder log)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Текстові файли (*.txt)|*.txt";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, log.ToString());
            }
        }

        private double[,] SolveWithInverse(double[,] A, double[,] B)
        {
            double[,] invA = Inverse(A);
            return Multiply(invA, B);
        }

        private void btnSolve_Click(object sender, EventArgs e)
        {
            try
            {
                double[,] A = GetMatrixFromGrid(dataGridViewA);
                double[,] B = GetMatrixFromGrid(dataGridViewB);
                StringBuilder log = new StringBuilder();

                if (checkBoxLog.Checked)
                {
                    log.AppendLine("Обчислення СЛАУ 1-м методом (за допомогою оберненої матриці):");
                    double[,] invA = Inverse(A, log);

                    log.AppendLine("\nОбернена матриця:");
                    LogMatrix(invA, log);

                    log.AppendLine("\nВхідна матриця В:");
                    LogMatrix(B, log);

                    log.AppendLine("\nОбчислення розв’язків:");
                    double[,] X = Multiply(invA, B, log);

                    log.AppendLine("\nРезультат:");
                    LogMatrix(X, log);

                    SaveLog(log);
                }
                else
                {
                    double[,] X = SolveWithInverse(A, B);
                    ShowMatrixF2(X, dataGridViewX);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private double[,] GetMatrixFromGrid(DataGridView grid)
        {
            int rows = grid.RowCount;
            int cols = grid.ColumnCount;
            double[,] matrix = new double[rows, cols];

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    matrix[i, j] = Convert.ToDouble(grid[j, i].Value);

            return matrix;
        }

        double[,] matrixA = {
            {6, 2, 5},
            {-3, 4, -1},
            {1, 4, 3}

        };

        double[,] matrixB = {
            {1},
            {6},
            {6}
        };

        private void Form1_Load(object sender, EventArgs e)
        {
            ShowMatrix(matrixA, dataGridViewA);
            ShowMatrix(matrixB, dataGridViewB);
        }

        private void btnGenerateA_Click(object sender, EventArgs e)
        {
            GenerateRandomMatrixA(dataGridViewA, 3, 3);
        }

        private void btnGenerateB_Click(object sender, EventArgs e)
        {
            GenerateRandomMatrixB(dataGridViewB, 3, 1);
        }

        private void GenerateRandomMatrixA(DataGridView grid, int rows, int cols)
        {
            grid.Columns.Clear();
            dataGridViewResult.Columns.Clear();
            txtRank.Clear();
            dataGridViewX.Columns.Clear();
            grid.RowCount = rows;

            for (int i = 1; i < cols; i++)
                grid.Columns.Add("", "");

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    grid[j, i].Value = (random.Next(-10, 10)).ToString();
                }
            }
        }

        private void GenerateRandomMatrixB(DataGridView grid, int rows, int cols)
        {
            grid.Columns.Clear();
            dataGridViewX.Columns.Clear();
            grid.RowCount = rows;

            for (int i = 1; i < cols; i++)
                grid.Columns.Add("", "");

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    grid[j, i].Value = (random.Next(1, 10)).ToString();
                }
            }
        }

    }
}
