using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        private int m;
        private int n;
        private int cellSize = 10;
        private Random rand = new Random();
        private List<Human> humans = new List<Human>();
        private List<InfectedHuman> infectedHumans = new List<InfectedHuman>();
        private List<IncubationHuman> incubationHumans = new List<IncubationHuman>();
        private List<RecoveredHuman> recoveredHumans = new List<RecoveredHuman>();
        private List<DeadHuman> deadHumans = new List<DeadHuman>();

        public Form1()
        {
            InitializeComponent();

            // Инициализация таймера
            timer2.Interval = 500; // интервал в миллисекундах
            timer2.Tick += Timer2_Tick;
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void nupm_ValueChanged(object sender, EventArgs e)
        {

        }

        private void nupn_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            humans.Clear();
            infectedHumans.Clear();
            incubationHumans.Clear();
            recoveredHumans.Clear();
            deadHumans.Clear();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m = (int)nupm.Value; // размер поля по X
            n = (int)nupn.Value; // размер поля по Y

            humans.Clear();
            infectedHumans.Clear();
            incubationHumans.Clear();
            recoveredHumans.Clear();
            deadHumans.Clear();

            // Создаем изображение для отрисовки поля
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Pen pen = new Pen(Color.Black);
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        g.DrawRectangle(pen, i * cellSize, j * cellSize, cellSize, cellSize);
                    }
                }
            }

            // Определяем количество зараженных людей (красных клеток)
            int totalCells = m * n;
            int humanCount = totalCells / 4; // четверть клеток будут зелеными (люди)
            int infectedCount = (int)numericUpDown1.Value;

            var allPositions = Enumerable.Range(0, totalCells).ToList();
            var infectedPositions = allPositions.OrderBy(x => rand.Next()).Take(infectedCount).ToList();
            var humanPositions = allPositions.Except(infectedPositions).Take(humanCount).ToList();

            // Добавляем зараженных людей
            foreach (var pos in infectedPositions)
            {
                int x = pos % m;
                int y = pos / m;
                infectedHumans.Add(new InfectedHuman(x, y));
            }

            // Добавляем обычных людей
            foreach (var pos in humanPositions)
            {
                int x = pos % m;
                int y = pos / m;
                humans.Add(new Human(x, y));
            }

            pictureBox1.Image = bmp;

            // Запускаем таймер для автоматического перемещения клеток
            timer2.Start();
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            // Перемещение зараженных людей
            foreach (var infected in infectedHumans.ToList())
            {
                infected.Move(m, n, rand, humans, infectedHumans, incubationHumans, recoveredHumans, deadHumans);
            }

            // Перемещение обычных людей
            foreach (var human in humans.ToList())
            {
                human.Move(m, n, rand, humans, infectedHumans, incubationHumans, recoveredHumans, deadHumans);
            }

            // Перемещение инкубационных людей
            foreach (var incubation in incubationHumans.ToList())
            {
                incubation.Move(m, n, rand, humans, infectedHumans, incubationHumans, recoveredHumans, deadHumans);
            }

            // Перемещение выздоровевших людей
            foreach (var recovered in recoveredHumans.ToList())
            {
                recovered.Move(m, n, rand, humans, infectedHumans, incubationHumans, recoveredHumans, deadHumans);
            }

            // Перемещение мертвых людей
            foreach (var dead in deadHumans.ToList())
            {
                dead.Move(m, n, rand, humans, infectedHumans, incubationHumans, recoveredHumans, deadHumans);
            }

            // Обновление заражения и инкубации
            CheckInfection();

            // Обновление отображения на PictureBox
            UpdatePictureBox();
        }

        private void CheckInfection()
        {
            // Отслеживаем начальное количество каждого состояния
            int initialInfectedCount = infectedHumans.Count;
            int initialIncubationCount = incubationHumans.Count;
            int initialRecoveredCount = recoveredHumans.Count;
            int initialDeadCount = deadHumans.Count;

            // Обработка заражения от здоровых к инкубации
            foreach (var human in humans.ToList())
            {
                foreach (var infected in infectedHumans)
                {
                    if (Math.Abs(human.X - infected.X) <= 1 && Math.Abs(human.Y - infected.Y) <= 1)
                    {
                        // Создаем клетку инкубации и заменяем здоровую клетку на нее
                        var incubationHuman = new IncubationHuman(human.X, human.Y);
                        incubationHumans.Add(incubationHuman);
                        humans.Remove(human);

                        // Таймер для превращения инкубационной клетки в зараженную через 10 секунд
                        Timer incubationToInfectedTimer = new Timer();
                        incubationToInfectedTimer.Interval = 10000;
                        incubationToInfectedTimer.Tick += (s, ev) =>
                        {
                            var infectedHuman = new InfectedHuman(incubationHuman.X, incubationHuman.Y);
                            infectedHumans.Add(infectedHuman);
                            incubationHumans.Remove(incubationHuman);

                            // Проверяем, не превышено ли начальное количество окрашенных клеток
                            if (infectedHumans.Count > initialInfectedCount ||
                                incubationHumans.Count > initialIncubationCount)
                            {
                                // Возвращаем состояние назад
                                infectedHumans.Remove(infectedHuman);
                                incubationHumans.Add(incubationHuman);
                            }

                            incubationToInfectedTimer.Stop();
                            incubationToInfectedTimer.Dispose();
                        };
                        incubationToInfectedTimer.Start();

                        break; // Выходим из цикла, так как клетка уже превращена
                    }
                }
            }

            // Обработка перехода из зараженности в выздоровление или смерть
            foreach (var infected in infectedHumans.ToList())
            {
                // Таймер для перехода из зараженной клетки
                Timer infectedTimer = new Timer();
                infectedTimer.Interval = 10000;
                infectedTimer.Tick += (s, ev) =>
                {
                    Random rnd = new Random();
                    double chance = rnd.NextDouble();

                    if (chance <= 0.8)
                    {
                        var recoveredHuman = new RecoveredHuman(infected.X, infected.Y);
                        recoveredHumans.Add(recoveredHuman);
                    }
                    else
                    {
                        var deadHuman = new DeadHuman(infected.X, infected.Y);
                        deadHumans.Add(deadHuman);
                    }
                    infectedHumans.Remove(infected);

                    // Проверяем, не превышено ли начальное количество окрашенных клеток
                    if (recoveredHumans.Count > initialRecoveredCount ||
                        deadHumans.Count > initialDeadCount)
                    {
                        // Возвращаем состояние назад
                        if (recoveredHumans.Count > initialRecoveredCount)
                            recoveredHumans.RemoveAt(recoveredHumans.Count - 1);
                        if (deadHumans.Count > initialDeadCount)
                            deadHumans.RemoveAt(deadHumans.Count - 1);
                        infectedHumans.Add(infected); // Возвращаем состояние зараженности, если нужно
                    }

                    infectedTimer.Stop();
                    infectedTimer.Dispose();
                };
                infectedTimer.Start();
            }
        }



        private void UpdatePictureBox()
        {
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Pen pen = new Pen(Color.Black);
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        g.DrawRectangle(pen, i * cellSize, j * cellSize, cellSize, cellSize);
                    }
                }

                // Рисуем мертвых людей (черные клетки)
                foreach (var dead in deadHumans)
                {
                    g.FillRectangle(Brushes.Black, dead.X * cellSize, dead.Y * cellSize, cellSize, cellSize);
                }

                // Рисуем выздоровевших людей (фиолетовые клетки)
                foreach (var recovered in recoveredHumans)
                {
                    g.FillRectangle(Brushes.Purple, recovered.X * cellSize, recovered.Y * cellSize, cellSize, cellSize);
                }

                // Рисуем инкубационные клетки (оранжевые клетки)
                foreach (var incubation in incubationHumans)
                {
                    g.FillRectangle(Brushes.Orange, incubation.X * cellSize, incubation.Y * cellSize, cellSize, cellSize);
                }

                // Рисуем обычных людей (зеленые клетки)
                foreach (var human in humans)
                {
                    g.FillRectangle(Brushes.Green, human.X * cellSize, human.Y * cellSize, cellSize, cellSize);
                }

                // Рисуем зараженных людей (красные клетки)
                foreach (var infected in infectedHumans)
                {
                    g.FillRectangle(Brushes.Red, infected.X * cellSize, infected.Y * cellSize, cellSize, cellSize);
                }
            }

            pictureBox1.Image = bmp;
        }

        public class Human
        {
            public int X { get; set; }
            public int Y { get; set; }

            public Human(int x, int y)
            {
                X = x;
                Y = y;
            }

            public virtual void Move(int m, int n, Random rand, List<Human> humans, List<InfectedHuman> infectedHumans, List<IncubationHuman> incubationHumans, List<RecoveredHuman> recoveredHumans, List<DeadHuman> deadHumans)
            {
                // Перемещение на случайную соседнюю клетку
                int newX = X + rand.Next(-1, 2);
                int newY = Y + rand.Next(-1, 2);

                // Убедимся, что новые координаты находятся в пределах границ и клетка свободна
                if (newX >= 0 && newX < m && newY >= 0 && newY < n)
                {
                    if (!humans.Any(h => h.X == newX && h.Y == newY) && !infectedHumans.Any(ih => ih.X == newX && ih.Y == newY) && !incubationHumans.Any(i => i.X == newX && i.Y == newY) && !recoveredHumans.Any(r => r.X == newX && r.Y == newY) && !deadHumans.Any(d => d.X == newX && d.Y == newY))
                    {
                        X = newX;
                        Y = newY;
                    }
                }
            }
        }

        public class InfectedHuman : Human
        {
            public InfectedHuman(int x, int y) : base(x, y)
            {
            }

            public override void Move(int m, int n, Random rand, List<Human> humans, List<InfectedHuman> infectedHumans, List<IncubationHuman> incubationHumans, List<RecoveredHuman> recoveredHumans, List<DeadHuman> deadHumans)
            {
                // Перемещение на случайную соседнюю клетку
                int newX = X + rand.Next(-1, 2);
                int newY = Y + rand.Next(-1, 2);

                // Убедимся, что новые координаты находятся в пределах границ и клетка свободна
                if (newX >= 0 && newX < m && newY >= 0 && newY < n)
                {
                    if (!humans.Any(h => h.X == newX && h.Y == newY) && !infectedHumans.Any(ih => ih.X == newX && ih.Y == newY) && !incubationHumans.Any(i => i.X == newX && i.Y == newY) && !recoveredHumans.Any(r => r.X == newX && r.Y == newY) && !deadHumans.Any(d => d.X == newX && d.Y == newY))
                    {
                        X = newX;
                        Y = newY;
                    }
                }
            }
        }

        public class IncubationHuman : Human
        {
            public IncubationHuman(int x, int y) : base(x, y)
            {
            }

            public override void Move(int m, int n, Random rand, List<Human> humans, List<InfectedHuman> infectedHumans, List<IncubationHuman> incubationHumans, List<RecoveredHuman> recoveredHumans, List<DeadHuman> deadHumans)
            {
                // Перемещение на случайную соседнюю клетку
                int newX = X + rand.Next(-1, 2);
                int newY = Y + rand.Next(-1, 2);

                // Убедимся, что новые координаты находятся в пределах границ и клетка свободна
                if (newX >= 0 && newX < m && newY >= 0 && newY < n)
                {
                    if (!humans.Any(h => h.X == newX && h.Y == newY) && !infectedHumans.Any(ih => ih.X == newX && ih.Y == newY) && !incubationHumans.Any(i => i.X == newX && i.Y == newY) && !recoveredHumans.Any(r => r.X == newX && r.Y == newY) && !deadHumans.Any(d => d.X == newX && d.Y == newY))
                    {
                        X = newX;
                        Y = newY;
                    }
                }
            }
        }

        public class RecoveredHuman : Human
        {
            public RecoveredHuman(int x, int y) : base(x, y)
            {
            }

            public override void Move(int m, int n, Random rand, List<Human> humans, List<InfectedHuman> infectedHumans, List<IncubationHuman> incubationHumans, List<RecoveredHuman> recoveredHumans, List<DeadHuman> deadHumans)
            {
                // Перемещение на случайную соседнюю клетку
                int newX = X + rand.Next(-1, 2);
                int newY = Y + rand.Next(-1, 2);

                // Убедимся, что новые координаты находятся в пределах границ и клетка свободна
                if (newX >= 0 && newX < m && newY >= 0 && newY < n)
                {
                    if (!humans.Any(h => h.X == newX && h.Y == newY) && !infectedHumans.Any(ih => ih.X == newX && ih.Y == newY) && !incubationHumans.Any(i => i.X == newX && i.Y == newY) && !recoveredHumans.Any(r => r.X == newX && r.Y == newY) && !deadHumans.Any(d => d.X == newX && d.Y == newY))
                    {
                        X = newX;
                        Y = newY;
                    }
                }
            }
        }

        public class DeadHuman : Human
        {
            public DeadHuman(int x, int y) : base(x, y)
            {
            }

            public override void Move(int m, int n, Random rand, List<Human> humans, List<InfectedHuman> infectedHumans, List<IncubationHuman> incubationHumans, List<RecoveredHuman> recoveredHumans, List<DeadHuman> deadHumans)
            {
                // Мертвые люди не двигаются
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
