using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        private int m; // Ширина поля в клетках
        private int n; // Высота поля в клетках
        private int cellSize = 10; // Размер клетки
        private Random rand = new Random(); // Генератор случайных чисел
        private List<Human> people = new List<Human>(); // Список зелёных клеток (люди)
        private List<InfectedHuman> infectedHumans = new List<InfectedHuman>(); // Список красных клеток (инфицированные люди)
        private List<IncubationHuman> incubationHumans = new List<IncubationHuman>(); // Список оранжевых клеток (инкубационные люди)
        private List<RecoveredHuman> recoveryHumans = new List<RecoveredHuman>(); // Список синих клеток (выздоровевшие люди)
        private List<DeadHuman> deadHumans = new List<DeadHuman>(); // Список черных клеток (мертвые люди)
        private int mortalityRate; // Вероятность смерти
        private int infectionProbability; // Вероятность заражения

        public Form1()
        {
            InitializeComponent();
            timer2.Interval = 500; // Интервал в миллисекундах для таймера обновления
            timer2.Tick += Timer2_Tick;
        }

        private void Form1_Load(object sender, EventArgs e) { }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            mortalityRate = (int)numericUpDown2.Value; // Обновляем вероятность смерти
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            infectionProbability = (int)numericUpDown3.Value; // Обновляем вероятность заражения
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button2.Enabled = true;
            button1.Enabled = false;
            m = (int)nupm.Value; // Размер поля по X
            n = (int)nupn.Value; // Размер поля по Y
            people.Clear();
            infectedHumans.Clear();
            recoveryHumans.Clear();
            deadHumans.Clear();
            incubationHumans.Clear(); // Очищаем список инкубационных клеток

            // Остановка и удаление всех таймеров
            timer2.Stop();
            timer2.Dispose();

            // Создаём изображение поля
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Pen pen = new Pen(Color.Black);
                // Рисуем сетку клеток
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        g.DrawRectangle(pen, i * cellSize, j * cellSize, cellSize, cellSize);
                    }
                }
            }

            int totalCells = m * n;
            int humanCount = totalCells / 4; // Четверть клеток - зелёные (люди)
            int initialInfectionCount = (int)numericUpDown1.Value; // Количество красных клеток
            int incubationInfectionProbability = (int)numericUpDown4.Value; // Вероятность заражения от оранжевых клеток

            var allPositions = Enumerable.Range(0, totalCells).ToList();
            var infectionPositions = allPositions.OrderBy(x => rand.Next()).Take(initialInfectionCount).ToList();
            var humanPositions = allPositions.Except(infectionPositions).Take(humanCount).ToList();

            // Создаём красные (инфицированные) клетки
            foreach (var pos in infectionPositions)
            {
                int x = pos % m;
                int y = pos / m;
                infectedHumans.Add(new InfectedHuman(x, y, mortalityRate));
            }

            // Создаём зелёные (здоровые) клетки (люди)
            foreach (var pos in humanPositions)
            {
                int x = pos % m;
                int y = pos / m;
                people.Add(new Human(x, y, infectionProbability, incubationInfectionProbability));
            }

            pictureBox1.Image = bmp;
            timer2.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            // Остановка и удаление всех таймеров инкубационных и инфицированных клеток
            foreach (var inHuman in incubationHumans)
            {
                inHuman.timer.Stop();
                inHuman.timer.Dispose();
            }
            foreach (var inHuman in infectedHumans)
            {
                inHuman.timer.Stop();
                inHuman.timer.Dispose();
            }
            // Очистка изображения поля и списков клеток
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            people.Clear();
            infectedHumans.Clear();
            recoveryHumans.Clear();
            deadHumans.Clear();
            incubationHumans.Clear(); // Очищаем список инкубационных клеток

            // Остановка и удаление всех таймеров основного таймера
            timer2.Stop();
            timer2.Dispose();
        }

        // Метод, вызываемый по таймеру для обновления состояния клеток
        private void Timer2_Tick(object sender, EventArgs e)
        {
            // Движение красных клеток (инфицированные люди)
            foreach (var infection in infectedHumans.ToList())
            {
                infection.Move(m, n, rand, people, infectedHumans, incubationHumans, recoveryHumans, deadHumans);
            }

            // Движение зелёных клеток (здоровые люди)
            foreach (var human in people.ToList())
            {
                human.Move(m, n, rand, people, infectedHumans, incubationHumans, recoveryHumans, deadHumans);
            }

            // Движение оранжевых клеток (инкубационные люди)
            foreach (var incubation in incubationHumans.ToList())
            {
                incubation.Move(m, n, rand, people, infectedHumans, incubationHumans, recoveryHumans, deadHumans);
            }

            // Движение синих клеток (выздоровевшие люди)
            foreach (var recovery in recoveryHumans.ToList())
            {
                recovery.Move(m, n, rand, people, infectedHumans, incubationHumans, recoveryHumans, deadHumans);
            }

            // Проверка заражения между зелёными и инфицированными клетками
            CheckInfection();
            // Обновление изображения поля
            UpdatePictureBox();
        }

        // Метод для проверки заражения между зелёными и инфицированными клетками
        private void CheckInfection()
        {
            List<Human> newIncubations = new List<Human>();

            foreach (var human in people.ToList())
            {
                bool shouldBecomeIncubation = false;

                // Проверяем контакт с красными клетками (инфицированными людьми)
                foreach (var infection in infectedHumans)
                {
                    if (Math.Abs(human.X - infection.X) <= 1 && Math.Abs(human.Y - infection.Y) <= 1)
                    {
                        shouldBecomeIncubation = true;
                        // Если происходит заражение
                        if (rand.Next(1, 101) <= human.InfectionProbability)
                        {
                            newIncubations.Add(human);
                        }
                        break;
                    }
                }

                // Если не заразились от красных клеток, проверяем контакт с оранжевыми клетками (инкубационными людьми)
                if (!shouldBecomeIncubation)
                {
                    foreach (var incubation in incubationHumans)
                    {
                        if (Math.Abs(human.X - incubation.X) <= 1 && Math.Abs(human.Y - incubation.Y) <= 1)
                        {
                            shouldBecomeIncubation = true;
                            // Если происходит заражение
                            if (rand.Next(1, 101) <= human.IncubationInfectionProbability)
                            {
                                newIncubations.Add(human);
                            }
                            break;
                        }
                    }
                }
            }

            // Переводим заразившихся зелёных в оранжевые клетки (инкубационные люди)
            foreach (var human in newIncubations)
            {
                var incubationHuman = new IncubationHuman(human.X, human.Y);

                incubationHumans.Add(incubationHuman);
                people.Remove(human);

                // Устанавливаем таймер для перехода оранжевых клеток в красные (инфицированные люди)
                Timer incubationTimer = new Timer();
                incubationHuman.timer = incubationTimer;
                incubationTimer.Interval = 10000; // 10 секунд для перехода оранжевых клеток в красные
                incubationTimer.Tick += (s, ev) =>
                {
                    var infectedHuman = new InfectedHuman(incubationHuman.X, incubationHuman.Y, mortalityRate);
                    incubationHumans.Remove(incubationHuman);
                    infectedHumans.Add(infectedHuman);

                    StartInfectedHumanTimer(infectedHuman);
                    incubationTimer.Stop();
                    incubationTimer.Dispose();
                };
                incubationTimer.Start();
            }

            // Запускаем таймеры для красных клеток (инфицированных людей), если они ещё не запущены
            foreach (var infection in infectedHumans.ToList())
            {
                if (!infection.HasTimerStarted)
                {
                    StartInfectedHumanTimer(infection);
                }
            }
        }

        // Метод для запуска таймера для красных клеток (инфицированных людей)
        private void StartInfectedHumanTimer(InfectedHuman infectedHuman)
        {
            infectedHuman.HasTimerStarted = true;
            Timer infectedTimer = new Timer();
            infectedHuman.timer = infectedTimer;
            infectedTimer.Interval = 10000; // 10 секунд для выздоровления или смерти инфицированных людей
            infectedTimer.Tick += (s, ev) =>
            {
                if (rand.Next(1, 101) <= infectedHuman.ColorChangeProbability)
                {
                    var deadHuman = new DeadHuman(infectedHuman.X, infectedHuman.Y);
                    infectedHumans.Remove(infectedHuman);
                    deadHumans.Add(deadHuman);
                }
                else
                {
                    var recoveredHuman = new RecoveredHuman(infectedHuman.X, infectedHuman.Y);
                    infectedHumans.Remove(infectedHuman);
                    recoveryHumans.Add(recoveredHuman);
                }
                infectedTimer.Stop();
                infectedTimer.Dispose();
            };
            infectedTimer.Start();
        }

        // Метод для обновления изображения поля
        private void UpdatePictureBox()
        {
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Pen pen = new Pen(Color.Black);
                // Рисуем сетку клеток
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        g.DrawRectangle(pen, i * cellSize, j * cellSize, cellSize, cellSize);
                    }
                }

                // Рисуем зелёные клетки (люди)
                foreach (var human in people)
                {
                    g.FillRectangle(Brushes.Green, human.X * cellSize, human.Y * cellSize, cellSize, cellSize);
                }

                // Рисуем оранжевые клетки (инкубационные люди)
                foreach (var incubation in incubationHumans)
                {
                    g.FillRectangle(Brushes.Orange, incubation.X * cellSize, incubation.Y * cellSize, cellSize, cellSize);
                }

                // Рисуем красные клетки (инфицированные люди)
                foreach (var infection in infectedHumans)
                {
                    g.FillRectangle(Brushes.Red, infection.X * cellSize, infection.Y * cellSize, cellSize, cellSize);
                }

                // Рисуем синие клетки (выздоровевшие люди)
                foreach (var recovery in recoveryHumans)
                {
                    g.FillRectangle(Brushes.Blue, recovery.X * cellSize, recovery.Y * cellSize, cellSize, cellSize);
                }

                // Рисуем черные клетки (мертвые люди)
                foreach (var dead in deadHumans)
                {
                    g.FillRectangle(Brushes.Black, dead.X * cellSize, dead.Y * cellSize, cellSize, cellSize);
                }
            }

            pictureBox1.Image = bmp;
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            // Ничего не делаем, так как это значение не используется в текущей реализации
        }

        private void label5_Click(object sender, EventArgs e)
        {
            // Ничего не делаем, так как это событие не используется в текущей реализации
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Ничего не делаем, так как это событие не используется в текущей реализации
        }
    }

    // Класс, представляющий зелёные клетки (люди)
    public class Human
    {
        public int X { get; private set; } // Координата X на поле
        public int Y { get; private set; } // Координата Y на поле
        public int InfectionProbability { get; private set; } // Вероятность заражения от красных клеток
        public int IncubationInfectionProbability { get; private set; } // Вероятность заражения от оранжевых клеток

        public Human(int x, int y, int infectionProbability, int incubationInfectionProbability)
        {
            X = x;
            Y = y;
            InfectionProbability = infectionProbability;
            IncubationInfectionProbability = incubationInfectionProbability;
        }

        // Метод для движения зелёных клеток (людей)
        public void Move(int m, int n, Random rand, List<Human> people, List<InfectedHuman> infectedHumans, List<IncubationHuman> incubationHumans, List<RecoveredHuman> recoveryHumans, List<DeadHuman> deadHumans)
        {
            int newX = X + (rand.Next(3) - 1); // -1, 0 или 1
            int newY = Y + (rand.Next(3) - 1); // -1, 0 или 1

            // Обновляем координаты, если новые координаты в пределах поля
            if (newX >= 0 && newX < m && newY >= 0 && newY < n)
            {
                X = newX;
                Y = newY;
            }
        }
    }

    // Класс, представляющий красные клетки (инфицированные люди)
    public class InfectedHuman
    {
        public int X { get; private set; } // Координата X на поле
        public int Y { get; private set; } // Координата Y на поле
        public int ColorChangeProbability { get; private set; } // Вероятность изменения цвета
        public bool HasTimerStarted { get; set; } // Флаг, указывающий на запущен ли таймер для клетки

        public Timer timer; // Таймер для клетки

        public InfectedHuman(int x, int y, int mortalityRate)
        {
            X = x;
            Y = y;
            ColorChangeProbability = mortalityRate;
            HasTimerStarted = false;
        }

        // Метод для движения красных клеток (инфицированных людей)
        public void Move(int m, int n, Random rand, List<Human> people, List<InfectedHuman> infectedHumans, List<IncubationHuman> incubationHumans, List<RecoveredHuman> recoveryHumans, List<DeadHuman> deadHumans)
        {
            int newX = X + (rand.Next(3) - 1); // -1, 0 или 1
            int newY = Y + (rand.Next(3) - 1); // -1, 0 или 1

            // Обновляем координаты, если новые координаты в пределах поля
            if (newX >= 0 && newX < m && newY >= 0 && newY < n)
            {
                X = newX;
                Y = newY;
            }
        }
    }

    // Класс, представляющий оранжевые клетки (инкубационные люди)
    public class IncubationHuman
    {
        public int X { get; private set; } // Координата X на поле
        public int Y { get; private set; } // Координата Y на поле

        public Timer timer; // Таймер для клетки

        public IncubationHuman(int x, int y)
        {
            X = x;
            Y = y;
        }

        // Метод для движения оранжевых клеток (инкубационных людей)
        public void Move(int m, int n, Random rand, List<Human> people, List<InfectedHuman> infectedHumans, List<IncubationHuman> incubationHumans, List<RecoveredHuman> recoveryHumans, List<DeadHuman> deadHumans)
        {
            int newX = X + (rand.Next(3) - 1); // -1, 0 или 1
            int newY = Y + (rand.Next(3) - 1); // -1, 0 или 1

            // Обновляем координаты, если новые координаты в пределах поля
            if (newX >= 0 && newX < m && newY >= 0 && newY < n)
            {
                X = newX;
                Y = newY;
            }
        }
    }

    // Класс, представляющий синие клетки (выздоровевшие люди)
    public class RecoveredHuman
    {
        public int X { get; private set; } // Координата X на поле
        public int Y { get; private set; } // Координата Y на поле

        public RecoveredHuman(int x, int y)
        {
            X = x;
            Y = y;
        }

        // Метод для движения синих клеток (выздоровевших людей)
        public void Move(int m, int n, Random rand, List<Human> people, List<InfectedHuman> infectedHumans, List<IncubationHuman> incubationHumans, List<RecoveredHuman> recoveryHumans, List<DeadHuman> deadHumans)
        {
            int newX = X + (rand.Next(3) - 1); // -1, 0 или 1
            int newY = Y + (rand.Next(3) - 1); // -1, 0 или 1

            // Обновляем координаты, если новые координаты в пределах поля
            if (newX >= 0 && newX < m && newY >= 0 && newY < n)
            {
                X = newX;
                Y = newY;
            }
        }
    }

    // Класс, представляющий черные клетки (мертвые люди)
    public class DeadHuman
    {
        public int X { get; private set; } // Координата X на поле
        public int Y { get; private set; } // Координата Y на поле

        public DeadHuman(int x, int y)
        {
            X = x;
            Y = y;
        }

        // Метод для движения черных клеток (мертвых людей)
        public void Move(int m, int n, Random rand, List<Human> people, List<InfectedHuman> infectedHumans, List<IncubationHuman> incubationHumans, List<RecoveredHuman> recoveryHumans, List<DeadHuman> deadHumans)
        {
            // Мертвые люди не двигаются, поэтому этот метод оставляем пустым
        }
    }
}
