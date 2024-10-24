using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace startScreen
{
    /// <summary>
    /// Interaction logic for Level1Win.xaml
    /// </summary>
    public partial class Level1Win : Window
    {
        private static readonly Random RandomGen = new Random();
        private DispatcherTimer confettiTimer;
       
        public Level1Win()
        {
            InitializeComponent();
            StartConfettiTimer();  // Start the timer when the window loads
            this.WindowState = WindowState.Maximized;

        }
        private void PreviousWindow_Click(object sender, RoutedEventArgs e)
        {
            MainWindow sW = new MainWindow();

            // Show the second window
            sW.Show();

            //close the current window
            this.Close();
        }
        private void StartConfettiTimer()
        {
            confettiTimer = new DispatcherTimer();
            confettiTimer.Interval = TimeSpan.FromSeconds(1.5);  // Set interval to 3 seconds
            confettiTimer.Tick += ConfettiTimer_Tick;          // Attach event handler
            confettiTimer.Start();                             // Start the timer
        }

        // Event handler for the timer's Tick event
        private void ConfettiTimer_Tick(object sender, EventArgs e)
        {
            // Generate multiple confetti particles every 3 seconds
            for (int i = 0; i < 30; i++)  // Adjust number of particles
            {
                CreateConfetti();
            }
        }

        // Function to create a single confetti particle and animate it
        private void CreateConfetti()
        {
            // Randomize confetti size, color, and start position
            double size = RandomGen.Next(10, 20);
            var confetti = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = new SolidColorBrush(GetRandomColor())
            };

            // Randomize start position (X axis)
            double startX = RandomGen.NextDouble() * ConfettiCanvas.ActualWidth;
            Canvas.SetLeft(confetti, startX);
            Canvas.SetTop(confetti, -size);  // Start above the visible area

            ConfettiCanvas.Children.Add(confetti);

            // Create a falling animation for the Y axis
            var animationY = new DoubleAnimation
            {
                From = -size,
                To = ConfettiCanvas.ActualHeight + size,
                Duration = TimeSpan.FromSeconds(RandomGen.Next(3, 6)),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            // Create a horizontal (X axis) sway animation
            var animationX = new DoubleAnimation
            {
                From = startX,
                To = startX + RandomGen.Next(-100, 100),
                Duration = animationY.Duration,
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            // Start the animations
            confetti.BeginAnimation(Canvas.TopProperty, animationY);
            confetti.BeginAnimation(Canvas.LeftProperty, animationX);

            // Optional: Remove confetti from canvas after it finishes falling
            animationY.Completed += (s, e) => ConfettiCanvas.Children.Remove(confetti);
        }

        // Function to get a random color for the confetti
        private Color GetRandomColor()
        {
            return Color.FromRgb((byte)RandomGen.Next(256),
                                 (byte)RandomGen.Next(256),
                                 (byte)RandomGen.Next(256));
        }
    }
}