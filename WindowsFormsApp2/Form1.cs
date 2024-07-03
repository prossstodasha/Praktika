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

        public Form1()
        {
            InitializeComponent();
            timer2.Interval = 500; // интервал в миллисекундах
            timer2.Tick += Timer2_Tick;
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void nupm_ValueChanged(object sender, EventArgs e) { }
        private void nupn_ValueChanged(object sender, EventArgs e) { }
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            int initialInfectionCount = (int)numericUpDown1.Value;
            var allPositions = Enumerable.Range(0, m * n).ToList();
            var infectionPositions = allPositions.OrderBy(x => rand.Next()).Take(initialInfectionCount).ToList();

            infectedHumans.Clear();
            foreach (var pos in infectionPositions)
            {
                int x = pos % m;
                int y = pos / m;
                infectedHumans.Add(new InfectedHuman(x, y, mortalityRate));
            }

            UpdatePictureBox();
        }
        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e) { }

        private void button2_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            people.Clear();
            infectedHumans.Clear();
            recoveryHumans.Clear();
            deadHumans.Clear();
            incubationHumans.Clear(); // очищаем список инкубационных клеток
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m = (int)nupm.Value; // размер поля по X
            n = (int)nupn.Value; // размер поля по Y
            people.Clear();
            infectedHumans.Clear();
            recoveryHumans.Clear();
            deadHumans.Clear();
            incubationHumans.Clear(); // очищаем список инкубационных клеток

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
                people.Add(new Human(x, y));
            }

            pictureBox1.Image = bmp;
            timer2.Start();
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

            foreach (var dead in deadHumans.ToList())
            {
                dead.Move(m, n, rand, people, infectedHumans, incubationHumans, recoveryHumans, deadHumans);
            }

            CheckInfection();
            UpdatePictureBox();
        }

        private void CheckInfection()
        {
            List<Human> newIncubations = new List<Human>();

            foreach (var human in people.ToList())
            {
                bool shouldBecomeIncubation = false;
                foreach (var infection in infectedHumans)
                {
                    if (Math.Abs(human.X - infection.X) <= 1 && Math.Abs(human.Y - infection.Y) <= 1)
                    {
                        shouldBecomeIncubation = true;
                        break;
                    }
                }
                foreach (var incubation in incubationHumans)
                {
                    if (Math.Abs(human.X - incubation.X) <= 1 && Math.Abs(human.Y - incubation.Y) <= 1)
                    {
                        shouldBecomeIncubation = true;
                        break;
                    }
                }
                if (shouldBecomeIncubation)
                {
                    newIncubations.Add(human);
                }
            }

            foreach (var human in newIncubations)
            {
                var incubationHuman = new IncubationHuman(human.X, human.Y);
                incubationHumans.Add(incubationHuman);
                people.Remove(human);

                Timer incubationTimer = new Timer();
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

                foreach (var dead in deadHumans)
                {
                    g.FillRectangle(Brushes.Black, dead.X * cellSize, dead.Y * cellSize, cellSize, cellSize);
                }

                foreach (var recovery in recoveryHumans)
                {
                    g.FillRectangle(Brushes.Purple, recovery.X * cellSize, recovery.Y * cellSize, cellSize, cellSize);
                }

                foreach (var incubation in incubationHumans)
                {
                    g.FillRectangle(Brushes.Orange, incubation.X * cellSize, incubation.Y * cellSize, cellSize, cellSize);
                }

                foreach (var human in people)
                {
                    g.FillRectangle(Brushes.Green, human.X * cellSize, human.Y * cellSize, cellSize, cellSize);
                }

                foreach (var infection in infectedHumans)
                {
                    g.FillRectangle(Brushes.Red, infection.X * cellSize, infection.Y * cellSize, cellSize, cellSize);
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

            public virtual void Move(int m, int n, Random rand, List<Human> people, List<InfectedHuman> infectedHumans, List<IncubationHuman> incubationHumans, List<RecoveredHuman> recoveryHumans, List<DeadHuman> deadHumans)
            {
                int newX = X + rand.Next(-1, 2);
                int newY = Y + rand.Next(-1, 2);

                if (newX >= 0 && newX < m && newY >= 0 && newY < n)
                {
                    if (!people.Any(h => h.X == newX && h.Y == newY) &&
                        !infectedHumans.Any(ih => ih.X == newX && ih.Y == newY) &&
                        !incubationHumans.Any(i => i.X == newX && i.Y == newY) &&
                        !recoveryHumans.Any(r => r.X == newX && r.Y == newY) &&
                        !deadHumans.Any(d => d.X == newX && d.Y == newY))
                    {
                        X = newX;
                        Y = newY;
                    }
                }
            }
        }

        public class InfectedHuman : Human
        {
            public bool HasTimerStarted { get; set; }
            public int ColorChangeProbability { get; private set; }

            public InfectedHuman(int x, int y, int mortalityRate) : base(x, y)
            {
                HasTimerStarted = false;
                ColorChangeProbability = mortalityRate; // Вероятность изменения цвета на черный
            }

            public override void Move(int m, int n, Random rand, List<Human> people, List<InfectedHuman> infectedHumans, List<IncubationHuman> incubationHumans, List<RecoveredHuman> recoveryHumans, List<DeadHuman> deadHumans)
            {
                int newX = X + rand.Next(-1, 2);
                int newY = Y + rand.Next(-1, 2);

                if (newX >= 0 && newX < m && newY >= 0 && newY < n)
                {
                    if (!people.Any(h => h.X == newX && h.Y == newY) &&
                        !infectedHumans.Any(ih => ih.X == newX && ih.Y == newY) &&
                        !incubationHumans.Any(i => i.X == newX && i.Y == newY) &&
                        !recoveryHumans.Any(r => r.X == newX && r.Y == newY) &&
                        !deadHumans.Any(d => d.X == newX && d.Y == newY))
                    {
                        X = newX;
                        Y = newY;
                    }
                }
            }
        }

        public class IncubationHuman : Human
        {
            public IncubationHuman(int x, int y) : base(x, y) { }

            public override void Move(int m, int n, Random rand, List<Human> people, List<InfectedHuman> infectedHumans, List<IncubationHuman> incubationHumans, List<RecoveredHuman> recoveryHumans, List<DeadHuman> deadHumans)
            {
                int newX = X + rand.Next(-1, 2);
                int newY = Y + rand.Next(-1, 2);

                if (newX >= 0 && newX < m && newY >= 0 && newY < n)
                {
                    if (!people.Any(h => h.X == newX && h.Y == newY) &&
                        !infectedHumans.Any(ih => ih.X == newX && ih.Y == newY) &&
                        !incubationHumans.Any(i => i.X == newX && i.Y == newY) &&
                        !recoveryHumans.Any(r => r.X == newX && r.Y == newY) &&
                        !deadHumans.Any(d => d.X == newX && d.Y == newY))
                    {
                        X = newX;
                        Y = newY;
                    }
                }
            }
        }

        public class RecoveredHuman : Human
        {
            public RecoveredHuman(int x, int y) : base(x, y) { }

            public override void Move(int m, int n, Random rand, List<Human> people, List<InfectedHuman> infectedHumans, List<IncubationHuman> incubationHumans, List<RecoveredHuman> recoveryHumans, List<DeadHuman> deadHumans)
            {
                int newX = X + rand.Next(-1, 2);
                int newY = Y + rand.Next(-1, 2);

                if (newX >= 0 && newX < m && newY >= 0 && newY < n)
                {
                    if (!people.Any(h => h.X == newX && h.Y == newY) &&
                        !infectedHumans.Any(ih => ih.X == newX && ih.Y == newY) &&
                        !incubationHumans.Any(i => i.X == newX && i.Y == newY) &&
                        !recoveryHumans.Any(r => r.X == newX && r.Y == newY) &&
                        !deadHumans.Any(d => d.X == newX && d.Y == newY))
                    {
                        X = newX;
                        Y = newY;
                    }
                }
            }
        }

        public class DeadHuman : Human
        {
            public DeadHuman(int x, int y) : base(x, y) { }

            public override void Move(int m, int n, Random rand, List<Human> people, List<InfectedHuman> infectedHumans, List<IncubationHuman> incubationHumans, List<RecoveredHuman> recoveryHumans, List<DeadHuman> deadHumans)
            {
                // Мертвые люди не двигаются
            }
        }

        private void Form1_Load(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            mortalityRate = (int)numericUpDown2.Value; // Обновляем вероятность изменения цвета на черный
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
