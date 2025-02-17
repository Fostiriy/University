﻿using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CW_ThoughtsOutLoud
{
	public partial class MainForm : Form
	{
		internal HashTable<string, string> dateNameBook = new HashTable<string, string>(8);
		internal HashTable<string, string> categoryColorBook = new HashTable<string, string>(8); // Изменить ХТ на свою Егору
		internal RBTree<double, DataGridViewRow> dateTree = new RBTree<double, DataGridViewRow>();
		internal string[] debugInfo = new string[4];
		internal DataGridView currentGrid;
		internal DataGridViewRow[] mainGridRows;

		internal AddMainRecordForm addMainRecordWindow = new AddMainRecordForm();
		private AddDateRecordForm addDateRecordWindow = new AddDateRecordForm();
		private AddCategoryRecordForm addCategoryRecordWindow = new AddCategoryRecordForm();
		private SearchDateRecordForm searchDateRecordWindow = new SearchDateRecordForm();
		private SearchMainRecordForm searchMainRecordWindow = new SearchMainRecordForm();
		DebugForm debugWindow;

		internal void ChangeDebugInfo(int index)
		{
			switch (index)
			{
				case 0:
					debugInfo[0] = "Хеш-таблица о времени записи.\nКлюч: (string) дата время.\nЗначение: (string) имя аудиозаписи.\n" +
						"Метод разрешения коллизий: открытая линейная адресация.\n" +
						$"Размер: {dateNameBook.Size}\n\n";
					debugInfo[0] += string.Join('\n', dateNameBook.Info());
					break;
				case 1:
					debugInfo[1] = "Хеш-таблица о категориях аудиозаписей.\nКлюч: (string) название категории.\nЗначение: (string) цвет категории.\n" +
						"Метод разрешения коллизий: метод цепочек.\n\n";
					break;
				case 2:
					debugInfo[2] = "Красно-чёрное двоичное дерево.\nПоиск: диапазон дат.\nКлюч: (double) дата время.\n\n";
					debugInfo[2] += string.Join('\n', dateTree.Info());
					break;
				case 3:
					debugInfo[3] = "АВЛ двоичное дерево.\nПоиск: категории в алфавитном порядке.\nКлюч: (string) название категории.\n\n";
					break;
				default: break;
			}
		}

		private void ClearAllBooksWithSaving()
		{
			DialogResult result = MessageBox.Show("Сохранить текущие данные в файл?", "Сохранение перед открытием нового файла",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					string filename = saveFileDialog.FileName;
					File.WriteAllText(filename, dateNameBook.InfoToFile());
					MessageBox.Show("Файл сохранён!", "Сохранение файла", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}

			mainGrid.Rows.Clear();
			dateNameGrid.Rows.Clear();
			categoryColorGrid.Rows.Clear();

			addMainRecordWindow.dateComboBox.Items.Clear();
			addMainRecordWindow.categoryComboBox.Items.Clear();

			dateTree.Clear();
			// дерево Егора
			dateNameBook.Clear();
			categoryColorBook.Clear(); // тоже Егора
			ChangeDebugInfo(0);
			ChangeDebugInfo(1);
			ChangeDebugInfo(2);
			ChangeDebugInfo(3);
		}

		private void ReadFile(string path)
		{
			FileInfo inputFile = new FileInfo(path);
			string line;

			using (StreamReader istream = inputFile.OpenText())
			{
				line = istream.ReadLine();
				if (line[0] == '|')
				{
					line = istream.ReadLine();
					while (line[0] != '|')
					{
						string[] info = line.Split('|');
						string data = info[0].Trim('\t', ' ');
						string key = info[1];

						addMainRecordWindow.dateComboBox.Items.Add(key);

						dateNameGrid.Rows.Add(data, key);
						dateNameBook.Insert(key, data);
						line = istream.ReadLine();
					}
					MessageBox.Show("Справочник \"Имя-дата-время\" был успешно заполнен!", "Уведомление о заполнении", 
						MessageBoxButtons.OK, MessageBoxIcon.Information);
					ChangeDebugInfo(0);

					line = istream.ReadLine();
					if (line[0] == '|')
					{
						line = istream.ReadLine();
						while (line[0] != '|')
						{
							string[] info = line.Split('|');
							string key = info[0].Trim('\t', ' ');
							string data = info[1].Trim('\t', ' ');

							addMainRecordWindow.categoryComboBox.Items.Add(key);

							categoryColorGrid.Rows.Add(key, data);
							categoryColorBook.Insert(key, data); // Справочник Егора
							line = istream.ReadLine();
						}
						MessageBox.Show("Справочник \"Категория-цвет\" был успешно заполнен!", "Уведомление о заполнении",
							MessageBoxButtons.OK, MessageBoxIcon.Information);
						ChangeDebugInfo(1);

						line = istream.ReadLine();
						if (line[0] == '|')
						{
							line = istream.ReadLine();
							while (line[0] != '|')
							{
								string[] info = line.Split('|');

								string data1 = info[0].Trim('\t', ' ');
								string key1 = info[1].Trim('\t', ' ');

								string key2 = info[2].Trim('\t', ' ');
								string data2 = info[3].Trim('\t', ' ');

								if (dateNameBook.Search(key1) != null && categoryColorBook.Search(key2) != null) // Егору поменять на свой справочник
								{
									var nodeFound = dateTree.Find(ConvertToTreeKey(key1));
									if (nodeFound != dateTree.Nil)
									{
										foreach (DataGridViewRow row in nodeFound.Data)
										{
											if (row.Cells[2].Value.ToString() == key2)
											{
												MessageBox.Show("Такая запись уже есть в справочнике!", "Добавление дубликата",
													MessageBoxButtons.OK, MessageBoxIcon.Error);
												mainGrid.Rows.Clear();
												dateTree.Clear();
												return;
											}
										}
									}
									int gridIndex = mainGrid.Rows.Add(data1, key1, key2, data2);
									dateTree.Insert(ConvertToTreeKey(key1), mainGrid.Rows[gridIndex]);
									// Дерево Егора
									
									line = istream.ReadLine();
								}
								else
								{
									MessageBox.Show("В файле нарушена целостность данных, справочник \"Общая информация\" не может быть заполнен!", 
										"Ошибка целостности данных", MessageBoxButtons.OK, MessageBoxIcon.Error);
									mainGrid.Rows.Clear();
									dateTree.Clear();
									// Дерево Егора
									return;
								}
							}
							MessageBox.Show("Справочник \"Общая информация\" был успешно заполнен!", "Уведомление о заполнении",
								MessageBoxButtons.OK, MessageBoxIcon.Information);
							ChangeDebugInfo(2);
							ChangeDebugInfo(3);
						}
					}
				}
			}
			ChangeDebugInfo(0);
		}

		internal double ConvertToTreeKey(string keyString)
		{
			string[] components = keyString.Split('.', ':', ' ');
			if (components[3].Length == 1)
				components[3] = "0" + components[3];
			string date = components[2] + components[1] + components[0];
			string time = components[3] + components[4] + components[5];

			return double.Parse(date + time);
		}


		public MainForm()
		{
			InitializeComponent();
			addMainRecordWindow.Owner = this;
			addDateRecordWindow.Owner = this;
			searchDateRecordWindow.Owner = this;
			searchMainRecordWindow.Owner = this;
		}

		private void OpenFileButton_Click(object sender, EventArgs e)
		{
			ClearAllBooksWithSaving();

			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				ReadFile(openFileDialog.FileName);
			}
		}

		private void SaveFileButton_Click(object sender, EventArgs e)
		{
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				string fileName = saveFileDialog.FileName;
				using (StreamWriter ostream = new StreamWriter(fileName))
				{
					ostream.WriteLine("|Справочник 1");
					ostream.Write(dateNameBook.InfoToFile());
					ostream.WriteLine("|Справочник 1");
					//ostream.WriteLine(categoryColorBook.InfoToFile());
					ostream.WriteLine("|Общий справочник");
					foreach (DataGridViewRow row in mainGrid.Rows)
					{
						ostream.WriteLine($"{row.Cells[0].Value.ToString(), -50}|" +
							$"{row.Cells[1].Value.ToString(), -14}|" +
							$"{row.Cells[2].Value.ToString(), -30}|" +
							$"{row.Cells[3].Value.ToString(), -30}");
					}
					ostream.WriteLine("|Общий справочник");
				}
				MessageBox.Show("Файл сохранён!", "Сохранение файла", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		private void AddRecordButton_Click(object sender, EventArgs e)
		{
			Form currentWindow = null;
			switch (booksTabControl.SelectedIndex)
			{
				case 0:
					currentGrid = mainGrid;
					currentWindow = addMainRecordWindow;
					break;
				case 1:
					currentGrid = dateNameGrid;
					currentWindow = addDateRecordWindow;
					break;
				case 2:
					currentGrid = categoryColorGrid;
					currentWindow = addCategoryRecordWindow;
					break;
				default: break;
			}
			currentWindow.ShowDialog();
		}

		private void ShowDebugButton_Click(object sender, EventArgs e)
		{
			if (showDebugButton.Text == "Показать окно отладки")
			{
				debugWindow = new DebugForm();
				debugWindow.Owner = this;
				debugWindow.StartPosition = FormStartPosition.Manual;
				debugWindow.Location = new Point(Location.X + Size.Width, Location.Y);
				debugWindow.Show();
				showDebugButton.Text = "Скрыть окно отладки";
			}
			else if (showDebugButton.Text == "Скрыть окно отладки")
			{
				debugWindow.Close();
			}
		}

		private void DeleteRecordButton_Click(object sender, EventArgs e)
		{
			DataGridViewRow currentRow = null;
			DialogResult result = DialogResult.Yes;
			double keyDate = 0;
			string keyCategory = string.Empty;

			switch (booksTabControl.SelectedIndex)
			{
				case 0:
					currentGrid = mainGrid;
					currentRow = currentGrid.CurrentRow;
					break;
				case 1:
					currentGrid = dateNameGrid;
					currentRow = currentGrid.CurrentRow;
					if (currentRow != null)
					{
						string date = currentRow.Cells[1].Value.ToString();
						keyDate = ConvertToTreeKey(date);
					}
					break;
				case 2:
					currentGrid = categoryColorGrid;
					currentRow = currentGrid.CurrentRow;

					// добавить обработку для keyCategory

					break;
				default: break;
			}
			if (currentRow != null && MessageBox.Show("Вы действительно хотите удалить выбранную запись?", "Подтверждение удаления",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				// Дописать случай с keyCategory != Empty
				if (keyDate != 0)
				{
					if (dateTree.Find(keyDate) != dateTree.Nil)
					{
						result = MessageBox.Show("В основном справочнике содержатся записи, связанные с записью, которую вы хотите удалить.\n" +
						"Всё равно удалить?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
						if (result == DialogResult.Yes)
						{
							SingleLinkedList<DataGridViewRow> deletedIndexes = dateTree.Delete(keyDate);
							ChangeDebugInfo(2);

							foreach (DataGridViewRow row in deletedIndexes)
							{
								mainGrid.Rows.RemoveAt(row.Index);
							}
						}
					}

					if (result == DialogResult.Yes)
					{
						string date = currentRow.Cells[1].Value.ToString();
						dateNameBook.Remove(date);
						addMainRecordWindow.dateComboBox.Items.Remove(date);
						ChangeDebugInfo(0);
					}
				}

				if (result == DialogResult.Yes)
				{
					currentGrid.Rows.RemoveAt(currentRow.Index);
				}

				if (currentGrid == mainGrid)
				{
					string date = currentRow.Cells[1].Value.ToString();
					keyDate = ConvertToTreeKey(date);
					dateTree.Delete(keyDate, currentRow);
					ChangeDebugInfo(2);

					// Удаление записи из дерева Егора
				}

			}
			else
			{
				MessageBox.Show("Необходимо выбрать строку для удаления в таблице!",
					"Не выбрана строка", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void SearchRecordButton_Click(object sender, EventArgs e)
		{
			Form currentWindow = null;
			switch (booksTabControl.SelectedIndex)
			{
				case 0:
					currentGrid = mainGrid;
					currentWindow = searchMainRecordWindow;
					break;
				case 1:
					currentGrid = dateNameGrid;
					currentWindow = searchDateRecordWindow;
					break;
				case 2:
					currentGrid = categoryColorGrid;
					currentWindow = addCategoryRecordWindow; // Создать что-то своё Егору
					break;
				default: break;
			}
			currentWindow.ShowDialog();
		}

		private void NewBookButton_Click(object sender, EventArgs e)
		{
			HashTable<string, string> dateHT = null;
			// Здесь ещё хеш-таблица с категориями
			DialogResult result = DialogResult.None;

			switch (booksTabControl.SelectedIndex)
			{
				case 0:
					currentGrid = mainGrid;
					result = MessageBox.Show("Все записи в выбранном справочнике будут удалены.\nПродолжить?",
						"Предупреждение об удалении данных",
						MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (result == DialogResult.Yes)
					{
						mainGrid.Rows.Clear();
						dateTree.Clear();
						ChangeDebugInfo(2);
						ChangeDebugInfo(3);

						// И дерево Егора тоже чистится
					}
					return;
				case 1:
					currentGrid = dateNameGrid;
					dateHT = dateNameBook;
					break;
				case 2:
					currentGrid = categoryColorGrid;
					break;
				default: break;
			}

			result = MessageBox.Show("Все записи в выбранном и основном справочниках будут удалены.\nПродолжить?",
				"Предупреждение об удалении данных",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (result == DialogResult.Yes)
			{
				mainGrid.Rows.Clear();
				dateTree.Clear();

				ChangeDebugInfo(2);
				ChangeDebugInfo(3);

				currentGrid.Rows.Clear();
				if (dateHT != null)
				{
					dateHT.Clear();
					addMainRecordWindow.dateComboBox.Items.Clear();
					ChangeDebugInfo(0);
				}

				// И дерево, и ХТ Егора тоже чистятся
				//addMainRecordWindow.categoryComboBox.Items.Clear();
				//ChangeDebugInfo(1);

			}
		}

		private void MainForm_LocationChanged(object sender, EventArgs e)
		{
			if (debugWindow != null)
				debugWindow.Location = new Point(Location.X + Size.Width, Location.Y);
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			debugInfo[0] = "Хеш-таблица о времени записи.\nКлюч: (string) дата время.\nЗначение: (string) имя аудиозаписи.\n" +
				"Метод разрешения коллизий: открытая линейная адресация.\n";
			debugInfo[1] = "Хеш-таблица о категориях аудиозаписей.\nКлюч: (string) название категории.\nЗначение: (string) цвет категории.\n" +
				"Метод разрешения коллизий: метод цепочек.\n";
			debugInfo[2] = "Красно-чёрное двоичное дерево.\nПоиск: диапазон дат.\nКлюч: (double) дата время.\n";
			debugInfo[3] = "АВЛ двоичное дерево.\nПоиск: категории в алфавитном порядке.\nКлюч: (string) название категории.\n";
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			DialogResult result = MessageBox.Show("Хотите выйти из программы?", "Завершение работы программы",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (result == DialogResult.Yes)
			{
				result = MessageBox.Show("Сохранить текущие данные в файл?", "Завершение работы программы",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
				{
					if (saveFileDialog.ShowDialog() == DialogResult.OK)
					{
						string filename = saveFileDialog.FileName;
						File.WriteAllText(filename, dateNameBook.InfoToFile());
						MessageBox.Show("Файл сохранён!", "Сохранение файла", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}
			}
			else
			{
				e.Cancel = true;
			}
		}

		private void ShowAllButton_Click(object sender, EventArgs e)
		{
			showAllButton.Enabled = !showAllButton.Enabled;
			bookWorkingBox.Enabled = !bookWorkingBox.Enabled;

			mainGrid.Rows.Clear();
			foreach (var row in mainGridRows)
			{
				mainGrid.Rows.Add(row);
			}
			ChangeDebugInfo(2);
		}
	}
}
