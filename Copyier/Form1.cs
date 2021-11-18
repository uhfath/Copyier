using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Copyier
{
	public partial class Form1 : Form
	{
		private string sourceFile;
		private string destinationDirectory;
		private int nestingLevel;

		private static string[] GetDirectories(string root, int nest)
		{
			var results = new List<string>();
			if (nest != 0)
			{
				var directories = Directory.EnumerateDirectories(root);
				foreach (var directory in directories)
				{
					var subDirectories = GetDirectories(directory, nest - 1);
					results.Add(directory);
					results.AddRange(subDirectories);
				}
			}

			return results.ToArray();
		}

		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			var result = openFileDialog1.ShowDialog();
			if (result == DialogResult.OK)
			{
				textBox1.Text = openFileDialog1.FileName;
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			var result = folderBrowserDialog1.ShowDialog();
			if (result == DialogResult.OK)
			{
				textBox2.Text = folderBrowserDialog1.SelectedPath;
			}
		}

		private void button3_Click(object sender, EventArgs e)
		{
			if (!backgroundWorker1.IsBusy)
			{
				if (!File.Exists(textBox1.Text))
				{
					MessageBox.Show("Некорректный путь к исходному файлу!", "Ошибка исходного файла", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				else
				{
					if (!Directory.Exists(textBox2.Text))
					{
						MessageBox.Show("Некорректный путь к папке назначения!", "Ошибка папки назначения", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
					else
					{
						button3.Text = "Отменить";

						sourceFile = textBox1.Text;
						destinationDirectory = textBox2.Text;
						nestingLevel = (int)numericUpDown1.Value;

						backgroundWorker1.RunWorkerAsync();
					}
				}
			}
			else
			{
				backgroundWorker1.CancelAsync();
			}
		}

		private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
			var worker = sender as BackgroundWorker;
			var directories = GetDirectories(destinationDirectory, nestingLevel);
			for (var i = 0; i < directories.Length && !worker.CancellationPending; i++)
			{
				var shouldCopy = true;
				var destination = Path.Combine(directories[i], Path.GetFileName(sourceFile));
				if (File.Exists(destination))
				{
					var result = MessageBox.Show($"Копируемый файл в папке уже существует. Перезаписать?{Environment.NewLine}{directories[i]}", "Ошибка копирования", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
					switch (result)
					{
						case DialogResult.Cancel:
							shouldCopy = false;
							worker.CancelAsync();
							break;

						case DialogResult.Yes:
							shouldCopy = true;
							break;

						case DialogResult.No:
							shouldCopy = false;
							break;
					}
				}

				if (shouldCopy)
				{
					File.Copy(sourceFile, destination, true);
				}

				worker.ReportProgress((int)((decimal)(i + 1) / (decimal)directories.Length * 100.0M));
			}
		}

		private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			progressBar1.Value = e.ProgressPercentage;
		}

		private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			MessageBox.Show("Копирование завершено", "Процесс завершён", MessageBoxButtons.OK, MessageBoxIcon.Information);
			button3.Text = "Начать";
			progressBar1.Value = 0;
		}
	}
}
