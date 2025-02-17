﻿using System;

namespace CW_ThoughtsOutLoud
{
	// Перечислимый тип, отвечающий за цвет узла
	public enum Colour 
	{
		Red, // Красный цвет
		Black // Чёрный цвет
	}

	// Класс узла дерева
	public class RBNode<TKey, TData> where TKey:IComparable 
	{
		// Цвет узла
		internal Colour color;
		// Ссылка на узел слева
		public RBNode<TKey, TData> left;
		// Ссылка на узел справа
		public RBNode<TKey, TData> right;
		// Ссылка на узел, являющийся родителем
		public RBNode<TKey, TData> parent;
		// Список значений узла
		public SingleLinkedList<TData> Data { get; } = new SingleLinkedList<TData>();
		// Уникальное значение узла - его ключ
		public TKey key;

		// Конструктор узла по ключу
		// Формальные параметры: ключ
		// Входные данные: пусто
		// Выходные данные: узел с заданным ключом
		public RBNode(TKey key)
		{
			this.key = key;
		}

		// Конструктор узла по ключу и значению
		// Формальные параметры: ключ, значение
		// Входные данные: пусто
		// Выходные данные: узел с заданным ключом и списком значений с переданным значением
		public RBNode(TKey key, TData data)
		{
			this.key = key;
			Data.PushBack(data);
		}

		// Конструктор узла по цвету
		// Формальные параметры: цвет узла color
		// Входные данные: пусто
		// Выходные данные: узел с заданным цветом
		public RBNode(Colour color)
		{
			this.color = color;
		}

		// Проверяет, меньше ли ключ первого узла, чем ключ второго узла
		// Формальные параметры: узел node2
		// Входные данные: 2 узла
		// Выходные данные: True или False
		public bool IsLess(RBNode<TKey, TData> node2) => key.CompareTo(node2.key) < 0;

		// Проверяет, больше ли ключ первого узла, чем ключ второго узла
		// Формальные параметры: узел node2
		// Входные данные: 2 узла
		// Выходные данные: True или False
		public bool IsMore(RBNode<TKey, TData> node2) => key.CompareTo(node2.key) > 0;

		// Проверяет, равны ли ключи первого и второго узлов
		// Формальные параметры: узел node2
		// Входные данные: 2 узла
		// Выходные данные: True или False
		public bool IsEqual(RBNode<TKey, TData> node2) => key.CompareTo(node2.key) == 0;
	}
	
	// Класс Красно-чёрного дерева, удаление min справа
	class RBTree<TKey, TData> where TKey : IComparable
	{
		// Узел-корень дерева
		private RBNode<TKey, TData> root;
		// Пустой узел-лист дерева
		private readonly RBNode<TKey, TData> nil;
		public RBNode<TKey, TData> Nil => nil;
		// Счётчик сравнений для операции поиска по диапазону
		public int ComparisonsNumber { get; private set; }
		// Список количества сравнений при поиске по диапазону
		public SingleLinkedList<int> ComparisonsList { get; private set; }

		// Конструктор дерева
		// Формальные параметры: пусто
		// Входные данные: узлы root и nil
		// Выходные данные: инициализация чёрного узла nil, root = nil
		public RBTree()
		{
			nil = new RBNode<TKey, TData>(Colour.Black);
			nil.parent = nil;
			nil.left = nil;
			nil.right = nil;
			nil.key = default;
			root = nil;
		}

		// Удаляет все элементы дерева, освобождая память
		// Формальные параметры: пусто
		// Входные данные: дерево
		// Выходные данные: root = nil
		public void Clear()
		{
			Console.WriteLine("Clearing the tree.");
			while (root != Nil)
				Delete(root);
			Info();
		}


		// Вращает дерево налево относительно узла
		// Формальные параметры: узел X
		// Входные данные: дерево
		// Выходные данные: дерево с изменёнными связями около X
		private void LeftRotate(RBNode<TKey, TData> X)
		{
			RBNode<TKey, TData> Y = X.right; // set Y
			X.right = Y.left; // turn Y's left subtree into X's right subtree

			if (Y.left != Nil)
				Y.left.parent = X;

			if (Y != Nil)
				Y.parent = X.parent; // link X's parent to Y

			if (X.parent == Nil)
				root = Y;
			else if (X == X.parent.left)
				X.parent.left = Y;
			else
				X.parent.right = Y;

			Y.left = X; // put X on Y's left

			if (X != Nil)
				X.parent = Y;
		}

		// Вращает дерево направо относительно узла
		// Формальные параметры: узел X
		// Входные данные: дерево
		// Выходные данные: дерево с изменёнными связями около X
		private void RightRotate(RBNode<TKey, TData> Y)
		{
			RBNode<TKey, TData> X = Y.left;
			Y.left = X.right;

			if (X.right != Nil)
				X.right.parent = Y;

			if (X != Nil)
				X.parent = Y.parent;

			if (Y.parent == Nil)
				root = X;
			else if (Y == Y.parent.right)
				Y.parent.right = X;
			else
				Y.parent.left = X;

			X.right = Y; // put Y on X's right

			if (Y != Nil)
				Y.parent = X;
		}

		// Нахождение узла с заданным ключом и значением в дереве
		// Формальные параметры: ключ key, значение data
		// Входные данные: дерево
		// Выходные данные: узел с заданным ключом и списком, содержащим значение
		public RBNode<TKey, TData> Find(TKey key, TData data)
		{
			bool isFound = false;
			RBNode<TKey, TData> temp = root;
			RBNode<TKey, TData> node = new RBNode<TKey, TData>(key);

			while (!isFound)
			{
				if (temp == Nil)
					break;
				if (node.IsLess(temp))
					temp = temp.left;
				else if (node.IsMore(temp))
					temp = temp.right;
				else if (temp.Data.Contains(data))
					isFound = true;
				else
					break;
			}

			if (isFound)
				return temp;
			else
				return Nil;
		}

		// Нахождение узла с заданным ключом в дереве
		// Формальные параметры: ключ key
		// Входные данные: дерево
		// Выходные данные: узел с заданным ключом
		public RBNode<TKey, TData> Find(TKey key)
		{
			bool isFound = false;
			RBNode<TKey, TData> temp = root;
			RBNode<TKey, TData> node = new RBNode<TKey, TData>(key);

			while (!isFound)
			{
				if (temp == Nil)
					break;
				if (node.IsLess(temp))
					temp = temp.left;
				else if (node.IsMore(temp))
					temp = temp.right;
				else
					isFound = true;
			}

			if (isFound)
				return temp;
			else
				return Nil;
		}

		// Нахождение списка узлов, ключ которых лежит в заданнам диапазоне
		// Формальные параметры: нижняя граница поиска ключ keyFrom, верхняя граница поиска ключ keyTo
		// Входные данные: дерево, счётчик сравнений, список сравнений
		// Выходные данные: список узлов, ключ которых лежит в заданнам диапазоне, счётчик сравнений, список сравнений
		public SingleLinkedList<RBNode<TKey, TData>> Search(TKey keyFrom, TKey keyTo)
		{
			SingleLinkedList<RBNode<TKey, TData>> result = new SingleLinkedList<RBNode<TKey, TData>>();
			ComparisonsList = new SingleLinkedList<int>();
			ComparisonsNumber = 0;

			if (keyFrom.CompareTo(keyTo) <= 0)
			{
				if (FindMaximum().key.CompareTo(keyFrom) >= 0 && FindMinimum().key.CompareTo(keyTo) <= 0)
				{
					PushLNR(root, keyFrom, keyTo, result);
				}
			}

			return result;
		}

		// Рекурсивное заполнение списка узлов, ключ которых лежит в заданном диапазоне,
		// основанное на симметричном обходе, начиная с переданного узла
		// Формальные параметры: нижняя граница поиска ключ keyFrom, верхняя граница поиска ключ keyTo,
		// список узлов result
		// Входные данные: дерево, счётчик сравнений, список сравнений
		// Выходные данные: список узлов, ключ которых лежит в заданнам диапазоне,
		// счётчик сравнений, список количества сравнений 
		private void PushLNR(RBNode<TKey, TData> current, TKey keyFrom, TKey keyTo, 
			SingleLinkedList<RBNode<TKey, TData>> result)
		{
			if (current != Nil)
			{
				PushLNR(current.left, keyFrom, keyTo, result);
				ComparisonsNumber++;
				if (current.key.CompareTo(keyFrom) >= 0 && current.key.CompareTo(keyTo) <= 0)
				{
					ComparisonsList.PushBack(ComparisonsNumber);
					result.PushBack(current);
				}
				PushLNR(current.right, keyFrom, keyTo, result);
			}
		}

		// Нахождение узла с минимальным ключом в дереве
		// Формальные параметры: пусто
		// Входные данные: дерево
		// Выходные данные: узел с минимальным ключом
		public RBNode<TKey, TData> FindMinimum()
		{
			RBNode<TKey, TData> node = FindMinimum(root);
			Console.WriteLine("Minimal node {0} in the tree was found.", node.key);
			return node;
		}

		// Нахождение узла с минимальным ключом в поддереве
		// Формальные параметры: узел-корень node поддерева
		// Входные данные: дерево
		// Выходные данные: узел с минимальным ключом в поддереве
		public RBNode<TKey, TData> FindMinimum(RBNode<TKey, TData> node)
		{
			RBNode<TKey, TData> temp = node;

			if (temp == Nil)
				return Nil;

			while (temp.left != Nil)
				temp = temp.left;

			return temp;
		}

		// Нахождение узла с максимальным ключом в дереве
		// Формальные параметры: пусто
		// Входные данные: дерево
		// Выходные данные: узел с максимальным ключом
		public RBNode<TKey, TData> FindMaximum()
		{
			RBNode<TKey, TData> node = FindMaximum(root);
			Console.WriteLine("Maximal node {0} in the tree was found.", node.key);
			return node;
		}

		// Нахождение узла с максимальным ключом в поддереве
		// Формальные параметры: узел-корень node поддерева
		// Входные данные: дерево
		// Выходные данные: узел с максимальным ключом в поддереве
		public RBNode<TKey, TData> FindMaximum(RBNode<TKey, TData> node)
		{
			RBNode<TKey, TData> temp = node;

			if (temp == Nil)
				return Nil;

			while (temp.right != Nil)
				temp = temp.right;

			return temp;
		}

		// Выводит значения полей узлов дерева на экран с учётом связей
		// Формальные параметры: пусто
		// Входные данные: дерево
		// Выходные данные: значения узлов дерева в порядке прямого обхода КЛП
		public string Info()
		{
			string result = string.Empty;

			if (root == Nil)
			{
				result += "Дерево пусто.\n";
			}
			else
			{
				result += Info(root);
			}

			return result;
		}

		// Выводит значения полей узлов поддерева на экран с учётом связей
		// Формальные параметры: узел-корень поддерева
		// Входные данные: дерево
		// Выходные данные: значения узлов поддерева в порядке прямого обхода КЛП
		private string Info(RBNode<TKey, TData> current)
		{
			string result = string.Empty;

			if (current != Nil)
			{
				result += $"Ключ: {current.key}\n";
				result += $"Цвет: {current.color}\n";
				result += $"Индексы:\n{current.Data.ElementsInfo()}\n";
				result += Info(current.left);
				result += Info(current.right);
			}

			return result;
		}

		// Выводит значения полей узлов дерева на экран с учётом связей
		// Формальные параметры: пусто
		// Входные данные: дерево
		// Выходные данные: значения узлов дерева по порядку
		public string InfoLikeTree()
		{
			string result = string.Empty;

			if (root == Nil)
			{
				result += "The tree is empty.\n";
			}
			else
			{
				result += InfoLikeTree(root, 0);
				result += "____________________________________\n";
			}

			return result;
		}

		// Выводит значения полей узлов поддерева на экран с учётом связей
		// Формальные параметры: узел-корень поддерева, число пробелов n
		// Входные данные: дерево
		// Выходные данные: значения узлов поддерева по порядку
		private string InfoLikeTree(RBNode<TKey, TData> current, int n)
		{
			string result = string.Empty;

			if (current != Nil)
			{
				result += InfoLikeTree(current.right, n + 1);

				for (int i = 0; i < n; i++)
					result += "	";
				result += $"{current.key} ";
				if (current.color == Colour.Black)
					result += $" (B, {current.Data.Info()})\n";
				else
					result += $" (R, {current.Data.Info()})\n";

				result += InfoLikeTree(current.left, n + 1);
			}

			return result;
		}

		// Выводит ключи узлов согласно прямому обходу дерева
		// Формальные параметры: пусто
		// Входные данные: дерево
		// Выходные данные: значения узлов дерева в порядке КЛП
		public void TraversalNLR()
		{
			Console.WriteLine("Pre-order NLR");
			TraversalNLR(root);
			if (root == Nil)
				Console.WriteLine("Empty tree.");
			Console.WriteLine();
		}

		// Выводит ключи узлов согласно прямому обходу поддерева
		// Формальные параметры: узел-корень current поддерева
		// Входные данные: дерево
		// Выходные данные: значения узлов поддерева в порядке КЛП
		private void TraversalNLR(RBNode<TKey, TData> current)
		{
			if (current != Nil)
			{
				Console.Write("{0} ", current.key);
				TraversalNLR(current.left);
				TraversalNLR(current.right);
			}
		}

		// Выводит ключи узлов согласно симметричному обходу дерева
		// Формальные параметры: пусто
		// Входные данные: дерево
		// Выходные данные: значения узлов дерева в порядке ЛКП
		public void TraversalLNR()
		{
			Console.WriteLine("In-order LNR");
			TraversalLNR(root);
			if (root == Nil)
				Console.WriteLine("Empty tree.");
			Console.WriteLine();
		}

		// Выводит ключи узлов согласно симметричному обходу поддерева
		// Формальные параметры: узел-корень current поддерева
		// Входные данные: дерево
		// Выходные данные: значения узлов поддерева в порядке ЛКП
		private void TraversalLNR(RBNode<TKey, TData> current)
		{
			if (current != Nil)
			{
				TraversalLNR(current.left);
				Console.Write("{0} ", current.key);
				TraversalLNR(current.right);
			}
		}

		// Выводит ключи узлов согласно обратному симметричному обходу дерева
		// Формальные параметры: пусто
		// Входные данные: дерево
		// Выходные данные: значения узлов дерева в порядке ПКЛ
		public void TraversalRNL()
		{
			Console.WriteLine("Reverse in-order RNL");
			TraversalRNL(root);
			if (root == Nil)
				Console.WriteLine("Empty tree.");
			Console.WriteLine();
		}

		// Выводит ключи узлов согласно обратному симметричному обходу поддерева
		// Формальные параметры: узел-корень current поддерева
		// Входные данные: дерево
		// Выходные данные: значения узлов поддерева в порядке ПКЛ
		private void TraversalRNL(RBNode<TKey, TData> current)
		{
			if (current != Nil)
			{
				TraversalRNL(current.right);
				Console.Write("{0} ", current.key);
				TraversalRNL(current.left);
			}
		}

		// Выводит ключи узлов согласно обратному обходу дерева
		// Формальные параметры: пусто
		// Входные данные: дерево
		// Выходные данные: значения узлов дерева в порядке ЛПК
		public void TraversalLRN()
		{
			Console.WriteLine("Post-order LRN");
			TraversalLRN(root);
			if (root == Nil)
				Console.WriteLine("Empty tree.");
			Console.WriteLine();
		}

		// Выводит ключи узлов согласно обратному обходу поддерева
		// Формальные параметры: узел-корень current поддерева
		// Входные данные: дерево
		// Выходные данные: значения узлов поддерева в порядке ЛПК
		private void TraversalLRN(RBNode<TKey, TData> current)
		{
			if (current != Nil)
			{
				TraversalLRN(current.left);
				TraversalLRN(current.right);
				Console.Write("{0} ", current.key);
			}
		}


		// Добавляет новый узел по ключу и значению в дерево по правилу вставки в бинарное дерево
		// Формальные параметры: ключ key, значение data
		// Входные данные: дерево
		// Выходные данные: дерево с новым узлом, удовлетворяющее свойствам КЧ дерева
		public void Insert(TKey key, TData data)
		{
			RBNode<TKey, TData> Z = new RBNode<TKey, TData>(key, data);
			RBNode<TKey, TData> Y = Nil;
			RBNode<TKey, TData> X = root;

			while (X != Nil)
			{
				Y = X;
				if (Z.IsEqual(X))
				{
					X.Data.PushBack(data);
					return;
				}
				else if (Z.IsLess(X))
					X = X.left;
				else
					X = X.right;
			}
			Z.parent = Y;

			if (Y == Nil)
				root = Z;
			else if (Z.IsLess(Y))
				Y.left = Z;
			else
				Y.right = Z;

			Z.left = Nil;
			Z.right = Nil;
			Z.color = Colour.Red; // colour the new node red

			InsertFixUp(Z); // call method to check for violations and fix
		}

		// Проверяет, нарушены ли свойства КЧ дерева после вставки узла, и исправляет нарушения
		// Формальные параметры: вставленный узел Z
		// Входные данные: дерево
		// Выходные данные: дерево, удовлетворяющее свойствам КЧ дерева
		private void InsertFixUp(RBNode<TKey, TData> Z)
		{
			while (Z != root && Z.parent.color == Colour.Red)
			{
				if (Z.parent == Z.parent.parent.left)
				{
					RBNode<TKey, TData> Y = Z.parent.parent.right;

					if (Y.color == Colour.Red) // Case 1: uncle is red
					{
						Z.parent.color = Colour.Black;
						Y.color = Colour.Black;
						Z.parent.parent.color = Colour.Red;
						Z = Z.parent.parent;
					}

					else // Case 2: uncle is black
					{
						if (Z == Z.parent.right)
						{
							Z = Z.parent;
							LeftRotate(Z);
						}
						// Case 3: recolour & rotate
						Z.parent.color = Colour.Black;
						Z.parent.parent.color = Colour.Red;
						RightRotate(Z.parent.parent);
					}
				}
				else
				{
					RBNode<TKey, TData> X = Z.parent.parent.left;

					if (X.color == Colour.Red) // Case 1
					{
						Z.parent.color = Colour.Black;
						X.color = Colour.Black;
						Z.parent.parent.color = Colour.Red;
						Z = Z.parent.parent;
					}

					else // Case 2
					{
						if (Z == Z.parent.left)
						{
							Z = Z.parent;
							RightRotate(Z);
						}
						// Case 3 
						Z.parent.color = Colour.Black;
						Z.parent.parent.color = Colour.Red;
						LeftRotate(Z.parent.parent);
					}
				}
			}

			root.color = Colour.Black; // re-colour the root black as necessary
		}

		// Меняет поддеревья местами
		// Формальные параметры: узел X, узел Y
		// Входные данные: дерево
		// Выходные данные: дерево с изменёнными связями
		private void Transplant(RBNode<TKey, TData> X, RBNode<TKey, TData> Y)
		{
			if (X.parent == Nil)
				root = Y;
			else if (X == X.parent.left)
				X.parent.left = Y;
			else
				X.parent.right = Y;

			Y.parent = X.parent;
		}

		// Удаляет при нахождении узел из дерева по правилу удаления в бинарном дереве
		// или удаляет лишь значение data из списка значений узла
		// и возвращает список значений узла
		// Формальные параметры: ключ key, значение data
		// Входные данные: дерево
		// Выходные данные: дерево, удовлетворяющее свойствам КЧ дерева
		public SingleLinkedList<TData> Delete(TKey key, TData data)
		{
			RBNode<TKey, TData> Z = Find(key, data);
			Delete(Z, data);
			return Z.Data;
		}

		// Удаляет при нахождении узел из дерева по правилу удаления в бинарном дереве
		// и возвращает список значений узла
		// Формальные параметры: ключ key
		// Входные данные: дерево
		// Выходные данные: дерево, удовлетворяющее свойствам КЧ дерева
		public SingleLinkedList<TData> Delete(TKey key)
		{
			RBNode<TKey, TData> Z = Find(key);
			Delete(Z);
			return Z.Data;
		}

		// Удаляет узел из дерева по правилу удаления в бинарном дереве или удаляет лишь индекс data
		// Формальные параметры: узел Z дерева, значение data
		// Входные данные: дерево
		// Выходные данные: дерево, удовлетворяющее свойствам КЧ дерева
		public void Delete(RBNode<TKey, TData> Z, TData data)
		{
			RBNode<TKey, TData> Y = Z;
			RBNode<TKey, TData> X = Nil;
			Colour SavedColor = Y.color;

			if (Z.Data.Count > 1)
			{
				Z.Data.Remove(data);
				return;
			}
			if (Z == Nil)
			{
				Console.WriteLine("Nothing to delete!");
				return;
			}
			if (Z.left == Nil)
			{
				X = Z.right;
				Transplant(Z, Z.right);
			}
			else if (Z.right == Nil)
			{
				X = Z.left;
				Transplant(Z, Z.left);
			}
			else
			{
				Y = FindMinimum(Z.right);
				Console.WriteLine("Minimum {0} was found.", Y.key);
				if (Y == Nil)
				{
					Console.WriteLine("Node does not have minimum.");
					return;
				}
				SavedColor = Y.color;
				X = Y.right;
				if (Y.parent == Z)
					X.parent = Y;
				else
				{
					Transplant(Y, Y.right);
					Y.right = Z.right;
					Y.right.parent = Y;
				}
				Transplant(Z, Y);

				Y.left = Z.left;
				Y.left.parent = Y;
				Y.color = Z.color;
			}

			if (SavedColor == Colour.Black)
				DeleteFixUp(X);
		}

		// Удаляет узел из дерева по правилу удаления в бинарном дереве
		// Формальные параметры: узел Z дерева
		// Входные данные: дерево
		// Выходные данные: дерево, удовлетворяющее свойствам КЧ дерева
		public void Delete(RBNode<TKey, TData> Z)
		{
			RBNode<TKey, TData> Y = Z;
			RBNode<TKey, TData> X = Nil;
			Colour SavedColor = Y.color;

			if (Z == Nil)
			{
				//Console.WriteLine("Nothing to delete!");
				return;
			}
			if (Z.left == Nil)
			{
				X = Z.right;
				Transplant(Z, Z.right);
			}
			else if (Z.right == Nil)
			{
				X = Z.left;
				Transplant(Z, Z.left);
			}
			else
			{
				Y = FindMinimum(Z.right);
				//Console.WriteLine("Minimum {0} was found.", Y.key);
				if (Y == Nil)
				{
					//Console.WriteLine("Node does not have minimum.");
					return;
				}
				SavedColor = Y.color;
				X = Y.right;
				if (Y.parent == Z)
					X.parent = Y;
				else
				{
					Transplant(Y, Y.right);
					Y.right = Z.right;
					Y.right.parent = Y;
				}
				Transplant(Z, Y);

				Y.left = Z.left;
				Y.left.parent = Y;
				Y.color = Z.color;
			}

			if (SavedColor == Colour.Black)
				DeleteFixUp(X);
		}

		// Проверяет, нарушены ли свойства КЧ дерева после удаления узла, и исправляет нарушения
		// Формальные параметры: текущий узел X
		// Входные данные: дерево
		// Выходные данные: дерево, удовлетворяющее свойствам КЧ дерева
		private void DeleteFixUp(RBNode<TKey, TData> X)
		{
			while (X != root && X.color == Colour.Black)
			{
				RBNode<TKey, TData> Y = X.parent;
				if (X == Y.left)
				{
					RBNode<TKey, TData> W = Y.right;

					if (W.color == Colour.Red)
					{
						W.color = Colour.Black; //case 1
						Y.color = Colour.Red;
						LeftRotate(Y);
						W = Y.right;
					}

					if (W.left.color == Colour.Black && W.right.color == Colour.Black)
					{
						W.color = Colour.Red; //case 2
						X = Y;
					}
					else
					{
						if (W.right.color == Colour.Black)
						{
							W.left.color = Colour.Black; //case 3
							W.color = Colour.Red;
							RightRotate(W);
							W = Y.right;
						}

						W.color = Y.color; //case 4
						Y.color = Colour.Black;
						W.right.color = Colour.Black;
						LeftRotate(Y);
						X = root;
					}
				}
				else //mirror code from above with "right" & "left" exchanged
				{
					RBNode<TKey, TData> W = Y.left;

					if (W.color == Colour.Red)
					{
						W.color = Colour.Black;
						Y.color = Colour.Red;
						RightRotate(Y);
						W = Y.left;
					}

					if (W.right.color == Colour.Black && W.left.color == Colour.Black)
					{
						W.color = Colour.Red;
						X = Y;
					}
					else
					{
						if (W.left.color == Colour.Black)
						{
							W.right.color = Colour.Black;
							W.color = Colour.Red;
							LeftRotate(W);
							W = Y.left;
						}

						W.color = Y.color;
						Y.color = Colour.Black;
						W.left.color = Colour.Black;
						RightRotate(Y);
						X = root;
					}
				}
			}

			X.color = Colour.Black;
		}

	}
}
