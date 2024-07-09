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
        private List<Human> people = new List<Human>();
        private List<InfectedHuman> infectedHumans = new List<InfectedHuman>();
        private List<IncubationHuman> incubationHumans = new List<IncubationHuman>();
        private List<RecoveredHuman> recoveryHumans = new List<RecoveredHuman>();
        private List<DeadHuman> deadHumans = new List<DeadHuman>();
        private int mortalityRate; // Вероятность смерти
        private int infectionProbability; // Вероятность заражения

        public Form1()
        {
            InitializeComponent();
            timer2.Interval = 500; // интервал в миллисекундах
            timer2.Tick += Timer2_Tick;
        }

        private void Form1_Load(object sender, EventArgs e) { }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            mortalityRate = (int)numericUpDown2.Value; // Обновляем вероятность изменения цвета на черный
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            infectionProbability = (int)numericUpDown3.Value; // Обновляем вероятность заражения
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button2.Enabled = true;
            button1.Enabled = false;
            m = (int)nupm.Value; // размер поля по X
            n = (int)nupn.Value; // размер поля по Y
            people.Clear();
            infectedHumans.Clear();
            recoveryHumans.Clear();
            deadHumans.Clear();
            incubationHumans.Clear(); // очищаем список инкубационных клеток

            // Остановка и удаление всех таймеров
            timer2.Stop();
            timer2.Dispose();

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

            int totalCells = m * n;
            int humanCount = totalCells / 4; // четверть клеток зелёные (люди)
            int initialInfectionCount = (int)numericUpDown1.Value; // количество красных клеток
            int incubationInfectionProbability = (int)numericUpDown4.Value; // вероятность заражения от оранжевых клеток

            var allPositions = Enumerable.Range(0, totalCells).ToList();
            var infectionPositions = allPositions.OrderBy(x => rand.Next()).Take(initialInfectionCount).ToList();
            var humanPositions = allPositions.Except(infectionPositions).Take(humanCount).ToList();

            foreach (var pos in infectionPositions)
            {
                int x = pos % m;
                int y = pos / m;
                infectedHumans.Add(new InfectedHuman(x, y, mortalityRate));
            }

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
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            people.Clear();
            infectedHumans.Clear();
            recoveryHumans.Clear();
            deadHumans.Clear();
            incubationHumans.Clear(); // очищаем список инкубационных клеток

            // Остановка и удаление всех таймеров
            timer2.Stop();
            timer2.Dispose();

        }


        private void Timer2_Tick(object sender, EventArgs e)
        {
            foreach (var infection in infectedHumans.ToList())
            {
                infection.Move(m, n, rand, people, infectedHumans, incubationHumans, recoveryHumans, deadHumans);
            }

            foreach (var human in people.ToList())
            {
                human.Move(m, n, rand, people, infectedHumans, incubationHumans, recoveryHumans, deadHumans);
            }

            foreach (var incubation in incubationHumans.ToList())
            {
                incubation.Move(m, n, rand, people, infectedHumans, incubationHumans, recoveryHumans, deadHumans);
            }

            foreach (var recovery in recoveryHumans.ToList())
            {
                recovery.Move(m, n, rand, people, infectedHumans, incubationHumans, recoveryHumans, deadHumans);
            }

            // Dead humans do not move, so no need to iterate through deadHumans
            CheckInfection();
            UpdatePictureBox();
        }


        private void CheckInfection()
        {
            List<Human> newIncubations = new List<Human>();

            foreach (var human in people.ToList())
            {
                bool shouldBecomeIncubation = false;

                // Проверяем контакт с красными клетками
                foreach (var infection in infectedHumans)
                {
                    if (Math.Abs(human.X - infection.X) <= 1 && Math.Abs(human.Y - infection.Y) <= 1)
                    {
                        shouldBecomeIncubation = true;
                        if (rand.Next(1, 101) <= human.InfectionProbability)
                        {
                            newIncubations.Add(human);
                        }
                        break;
                    }
                }

                // Если уже должны стать инкубационной, проверяем контакт с оранжевыми клетками
                if (!shouldBecomeIncubation)
                {
                    foreach (var incubation in incubationHumans)
                    {
                        if (Math.Abs(human.X - incubation.X) <= 1 && Math.Abs(human.Y - incubation.Y) <= 1)
                        {
                            shouldBecomeIncubation = true;
                            if (rand.Next(1, 101) <= human.IncubationInfectionProbability)
                            {
                                newIncubations.Add(human);
                            }
                            break;
                        }
                    }
                }
            }

            foreach (var human in newIncubations)
            {
                var incubationHuman = new IncubationHuman(human.X, human.Y);
                
                incubationHumans.Add(incubationHuman);
                people.Remove(human);

                Timer incubationTimer = new Timer();
                incubationHuman.timer = incubationTimer;
                incubationTimer.Interval = 10000; // 10 seconds for orange cells to turn red
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

            foreach (var infection in infectedHumans.ToList())
            {
                if (!infection.HasTimerStarted)
                {
                    StartInfectedHumanTimer(infection);
                }
            }
        }


        private void StartInfectedHumanTimer(InfectedHuman infectedHuman)
        {
            infectedHuman.HasTimerStarted = true;
            Timer infectedTimer = new Timer();
            infectedHuman.timer = infectedTimer;
            infectedTimer.Interval = 10000; // 10 seconds for infected human to either recover or die
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

                foreach (var human in people)
                {
                    g.FillRectangle(Brushes.Green, human.X * cellSize, human.Y * cellSize, cellSize, cellSize);
                }

                foreach (var incubation in incubationHumans)
                {
                    g.FillRectangle(Brushes.Orange, incubation.X * cellSize, incubation.Y * cellSize, cellSize, cellSize);
                }

                foreach (var infection in infectedHumans)
                {
                    g.FillRectangle(Brushes.Red, infection.X * cellSize, infection.Y * cellSize, cellSize, cellSize);
                }

                foreach (var recovery in recoveryHumans)
                {
                    g.FillRectangle(Brushes.Blue, recovery.X * cellSize, recovery.Y * cellSize, cellSize, cellSize);
                }

                foreach (var dead in deadHumans)
                {
                    g.FillRectangle(Brushes.Black, dead.X * cellSize, dead.Y * cellSize, cellSize, cellSize);
                }
            }

            pictureBox1.Image = bmp;
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }

    public class Human
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int InfectionProbability { get; private set; } // Вероятность заражения от красных клеток
        public int IncubationInfectionProbability { get; private set; } // Вероятность заражения от оранжевых клеток

        public Human(int x, int y, int infectionProbability, int incubationInfectionProbability)
        {
            X = x;
            Y = y;
            InfectionProbability = infectionProbability;
            IncubationInfectionProbability = incubationInfectionProbability;
        }

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


    public class InfectedHuman
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int ColorChangeProbability { get; private set; } // Вероятность изменения цвета
        public bool HasTimerStarted { get; set; }

        public Timer timer;

        public InfectedHuman(int x, int y, int mortalityRate)
        {
            X = x;
            Y = y;
            ColorChangeProbability = mortalityRate;
            HasTimerStarted = false;
        }

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

    public class IncubationHuman
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public Timer timer;

        public IncubationHuman(int x, int y)
        {
            X = x;
            Y = y;
        }

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

    public class RecoveredHuman
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public RecoveredHuman(int x, int y)
        {
            X = x;
            Y = y;
        }

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

    public class DeadHuman
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public DeadHuman(int x, int y)
        {
            X = x;
            Y = y;
        }

        // Dead humans do not move, so the Move method does nothing
        public void Move(int m, int n, Random rand, List<Human> people, List<InfectedHuman> infectedHumans, List<IncubationHuman> incubationHumans, List<RecoveredHuman> recoveryHumans, List<DeadHuman> deadHumans)
        {
            // Dead humans do not move, so leave this method empty
        }
    }

}
