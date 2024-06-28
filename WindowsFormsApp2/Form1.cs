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
        private Timer infectedTimer; // объявление таймера

        public Form1()
        {
            InitializeComponent();
            // Инициализация таймера для зараженных людей
            infectedTimer = new Timer();
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

        private void Form1_Load(object sender, EventArgs e)
        {
            // Инициализация значений для поля
            nupm.Value = 25; // примерное значение, можете изменить
            nupn.Value = 25; // примерное значение, можете изменить
            numericUpDown1.Value = 1; // примерное значение зараженных, можете изменить

            // Установка размеров PictureBox на основе значений m и n
            pictureBox1.Width = (int)nupm.Value * cellSize;
            pictureBox1.Height = (int)nupn.Value * cellSize;

            // Создание изображения и отрисовка начального состояния поля
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Pen pen = new Pen(Color.Black);
                for (int i = 0; i < nupm.Value; i++)
                {
                    for (int j = 0; j < nupn.Value; j++)
                    {
                        g.DrawRectangle(pen, i * cellSize, j * cellSize, cellSize, cellSize);
                    }
                }
            }
            pictureBox1.Image = bmp;
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
            // Проверка и изменение состояний клеток
            foreach (var human in humans.ToList())
            {
                // Проверяем соседство с красными клетками
                foreach (var infected in infectedHumans.ToList())
                {
                    if (Math.Abs(human.X - infected.X) <= 1 && Math.Abs(human.Y - infected.Y) <= 1)
                    {
                        // Заменяем зеленую клетку на оранжевую
                        var incubationHuman = new IncubationHuman(human.X, human.Y);
                        incubationHumans.Add(incubationHuman);
                        humans.Remove(human);

                        // Запускаем таймер для изменения цвета через 10000 мс
                        Timer incubationTimer = new Timer();
                        incubationTimer.Interval = 10000;
                        incubationTimer.Tick += (s, ev) =>
                        {
                            // Заменяем оранжевую клетку на красную
                            var redHuman = new InfectedHuman(incubationHuman.X, incubationHuman.Y);
                            incubationHumans.Remove(incubationHuman);
                            infectedHumans.Add(redHuman);
                            incubationTimer.Stop();
                            incubationTimer.Dispose();
                        };
                        incubationTimer.Start();

                        break; // Выходим из цикла, так как клетка уже изменена на оранжевую
                    }
                }

                // Проверяем соседство с оранжевыми клетками
                foreach (var orange in incubationHumans.ToList())
                {
                    if (Math.Abs(human.X - orange.X) <= 1 && Math.Abs(human.Y - orange.Y) <= 1)
                    {
                        // Заменяем зеленую клетку на оранжевую
                        var incubationHuman = new IncubationHuman(human.X, human.Y);
                        incubationHumans.Add(incubationHuman);
                        humans.Remove(human);

                        // Запускаем таймер для изменения цвета через 10000 мс
                        Timer incubationTimer = new Timer();
                        incubationTimer.Interval = 10000;
                        incubationTimer.Tick += (s, ev) =>
                        {
                            // Заменяем оранжевую клетку на красную
                            var redHuman = new InfectedHuman(incubationHuman.X, incubationHuman.Y);
                            incubationHumans.Remove(incubationHuman);
                            infectedHumans.Add(redHuman);
                            incubationTimer.Stop();
                            incubationTimer.Dispose();
                        };
                        incubationTimer.Start();

                        break; // Выходим из цикла, так как клетка уже изменена на оранжевую
                    }
                }
            }

            // Замена состояния зараженных клеток через 10000 мс
            foreach (var infected in infectedHumans.ToList())
            {
                // Запускаем таймер для изменения цвета через 10000 мс
                Timer infectedTimer = new Timer();
                infectedTimer.Interval = 10000;
                infectedTimer.Tick += (s, ev) =>
                {
                    // Заменяем оранжевую клетку на красную
                    var redHuman = new DeadHuman(infected.X, infected.Y);
                    infectedHumans.Remove(infected);
                    deadHumans.Add(redHuman);
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

                // Отрисовка зеленых клеток
                foreach (var human in humans)
                {
                    g.FillRectangle(Brushes.Green, human.X * cellSize, human.Y * cellSize, cellSize, cellSize);
                }

                // Отрисовка красных клеток
                foreach (var infected in infectedHumans)
                {
                    g.FillRectangle(Brushes.Red, infected.X * cellSize, infected.Y * cellSize, cellSize, cellSize);
                }

                // Отрисовка оранжевых клеток
                foreach (var incubation in incubationHumans)
                {
                    g.FillRectangle(Brushes.Orange, incubation.X * cellSize, incubation.Y * cellSize, cellSize, cellSize);
                }

                // Отрисовка черных клеток
                foreach (var dead in deadHumans)
                {
                    g.FillRectangle(Brushes.Black, dead.X * cellSize, dead.Y * cellSize, cellSize, cellSize);
                }

                // Отрисовка фиолетовых клеток
                foreach (var recovered in recoveredHumans)
                {
                    g.FillRectangle(Brushes.Purple, recovered.X * cellSize, recovered.Y * cellSize, cellSize, cellSize);
                }

                // Рисуем сетку клеток
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        g.DrawRectangle(pen, i * cellSize, j * cellSize, cellSize, cellSize);
                    }
                }
            }

            pictureBox1.Image = bmp;
        }
    }

    // Классы для клеток людей
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
            // Логика перемещения для обычных людей
            int direction = rand.Next(4);
            switch (direction)
            {
                case 0: // вверх
                    if (Y > 0) Y--;
                    break;
                case 1: // вниз
                    if (Y < n - 1) Y++;
                    break;
                case 2: // влево
                    if (X > 0) X--;
                    break;
                case 3: // вправо
                    if (X < m - 1) X++;
                    break;
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
            // Логика перемещения для зараженных людей
            int direction = rand.Next(4);
            switch (direction)
            {
                case 0: // вверх
                    if (Y > 0) Y--;
                    break;
                case 1: // вниз
                    if (Y < n - 1) Y++;
                    break;
                case 2: // влево
                    if (X > 0) X--;
                    break;
                case 3: // вправо
                    if (X < m - 1) X++;
                    break;
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
            // Логика перемещения для инкубационных людей
            int direction = rand.Next(4);
            switch (direction)
            {
                case 0: // вверх
                    if (Y > 0) Y--;
                    break;
                case 1: // вниз
                    if (Y < n - 1) Y++;
                    break;
                case 2: // влево
                    if (X > 0) X--;
                    break;
                case 3: // вправо
                    if (X < m - 1) X++;
                    break;
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
            // Логика перемещения для выздоровевших людей
            int direction = rand.Next(4);
            switch (direction)
            {
                case 0: // вверх
                    if (Y > 0) Y--;
                    break;
                case 1: // вниз
                    if (Y < n - 1) Y++;
                    break;
                case 2: // влево
                    if (X > 0) X--;
                    break;
                case 3: // вправо
                    if (X < m - 1) X++;
                    break;
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
            // Логика перемещения для мертвых людей (мертвые люди не перемещаются)
        }
    }
}
