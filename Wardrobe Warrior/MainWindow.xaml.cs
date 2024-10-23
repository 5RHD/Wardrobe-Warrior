using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Collections.Generic;
using Vector = System.Windows.Vector;

namespace WW
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer gameTimer = new();
        private readonly Random random = new();
        private bool isJumping = false;
        private int jumpCount = 0;
        private double jumpVelocity = 0;
        private const double Gravity = 2.0;
        private int playerHealth = 20;
        private int bossHealth = 15;
        private double bossTargetX;
        private bool isLeftMouseDown = false;
        private readonly List<FrameworkElement> playerProjectiles = new();
        private readonly List<FrameworkElement> bossProjectiles = new();
        private DateTime lastPlayerShotTime = DateTime.Now;
        private DateTime lastBossShotTime = DateTime.Now;
        private DateTime lastBigProjectileTime = DateTime.Now;
        private Point mousePosition;
        private DateTime gameStartTime;
        private TimeSpan elapsedTime;
        private bool isPaused = false;
        private int shirtHealth = 3;
        private Rectangle ShirtHealthBar;
        private bool isShaking = false;//2 new
        private readonly DispatcherTimer shakeTimer = new();

        private double scaleX;
        private double scaleY;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CalculateScalingFactors();
            LoadImages();
            SetInitialPositions();
            GameCanvas.Visibility = Visibility.Collapsed;
            MenuGrid.Visibility = Visibility.Visible;
        }

        private void CalculateScalingFactors()
        {
            scaleX = ActualWidth / 1000;
            scaleY = ActualHeight / 600;
        }


        private void LoadImages()
        {
            BackgroundImage.Source = new BitmapImage(new Uri(@"C:\Users\irber\source\repos\WW\WW\assets\background.jpg", UriKind.Absolute));
            Boss.Source = new BitmapImage(new Uri(@"C:\Users\irber\source\repos\WW\WW\assets\Monster1.png.png", UriKind.Absolute));
            Player.Source = new BitmapImage(new Uri(@"C:\Users\irber\source\repos\WW\WW\assets\character_blond.png", UriKind.Absolute));
            Platform1.Source = new BitmapImage(new Uri(@"C:\Users\irber\source\repos\WW\WW\assets\platform1.png", UriKind.Absolute));
            Platform2.Source = new BitmapImage(new Uri(@"C:\Users\irber\source\repos\WW\WW\assets\platform1.png", UriKind.Absolute));
            Shirt.Source = new BitmapImage(new Uri(@"C:\Users\irber\source\repos\WW\WW\assets\shirt.png", UriKind.Absolute));

            Player.Width = 180 * scaleX;
            Player.Height = 180 * scaleY;
            Boss.Width = 425 * scaleX;
            Boss.Height = 425 * scaleY;
            Platform1.Width = 125 * scaleX;
            Platform1.Height = 30 * scaleY;
            Platform2.Width = 125 * scaleX;
            Platform2.Height = 30 * scaleY;
            Shirt.Width = 40 * scaleX;
            Shirt.Height = 40 * scaleY;

            StartButton.Width = 425 * scaleX;
            StartButton.Height = 350 * scaleY;
        }

        private void SetInitialPositions()
        {
            Canvas.SetLeft(Player, 50 * scaleX);
            Canvas.SetBottom(Player, 80 * scaleY);
            Canvas.SetRight(Boss, 275 * scaleX);
            Canvas.SetBottom(Boss, 75 * scaleY);
            Canvas.SetLeft(Platform1, 150 * scaleX);
            Canvas.SetBottom(Platform1, 200 * scaleY);
            Canvas.SetLeft(Platform2, 350 * scaleX);
            Canvas.SetBottom(Platform2, 300 * scaleY);
            Canvas.SetLeft(Shirt, Canvas.GetLeft(Platform1) + Platform1.Width / 2 - Shirt.Width / 2);
            Canvas.SetBottom(Shirt, Canvas.GetBottom(Platform1) + Platform1.Height);

            Canvas.SetLeft(ShirtText, Canvas.GetLeft(Shirt) - 10 * scaleX);
            Canvas.SetBottom(ShirtText, Canvas.GetBottom(Shirt) + Shirt.Height + 10);
            ShirtText.Text = "Raak het shirt drie keer om te verzamelen!";
            ShirtText.FontWeight = FontWeights.Bold;

            ShirtHealthBar = new Rectangle
            {
                Width = 15 * shirtHealth,
                Height = 5 * scaleY,
                Fill = Brushes.Red
            };
            Canvas.SetLeft(ShirtHealthBar, Canvas.GetLeft(Shirt));
            Canvas.SetBottom(ShirtHealthBar, Canvas.GetBottom(Shirt) - 10 * scaleY);
            GameCanvas.Children.Add(ShirtHealthBar);

            Canvas.SetBottom(StartButton, 100 * scaleY);
        }

        private void StartGame(object sender, RoutedEventArgs e)
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            gameTimer.Tick += GameLoop;
            gameTimer.Interval = TimeSpan.FromMilliseconds(16);
            gameTimer.Start();

            UpdateHealthBars();
            SetNewBossTarget();
            gameStartTime = DateTime.Now;

            MenuGrid.Visibility = Visibility.Collapsed;
            PauseGrid.Visibility = Visibility.Collapsed;
            GameCanvas.Visibility = Visibility.Visible;

            playerHealth = 10;
            bossHealth = 10;
            shirtHealth = 3;
            playerProjectiles.Clear();
            bossProjectiles.Clear();

            foreach (var child in GameCanvas.Children)
            {
                if (child is FrameworkElement element && (playerProjectiles.Contains(element) || bossProjectiles.Contains(element)))
                {
                    GameCanvas.Children.Remove(element);
                }
            }

            Shirt.Visibility = Visibility.Visible;
            ShirtText.Visibility = Visibility.Visible;
            ShirtHealthBar.Visibility = Visibility.Visible;
            UpdateShirtHealthBar();

            GameCanvas.Focus();
        }

        private void StartScreenShake()//new method
        {
            if (isShaking) return;

            isShaking = true;
            var originalLeft = Canvas.GetLeft(GameCanvas);
            var originalTop = Canvas.GetTop(GameCanvas);

            shakeTimer.Interval = TimeSpan.FromMilliseconds(50);
            int shakeCount = 0;

            shakeTimer.Tick += (s, e) =>
            {
                shakeCount++;
                if (shakeCount > 4)  // Stop after 4 shakes
                {
                    shakeTimer.Stop();
                    Canvas.SetLeft(GameCanvas, originalLeft);
                    Canvas.SetTop(GameCanvas, originalTop);
                    isShaking = false;
                    return;
                }

                // Simple left-right shake
                if (shakeCount % 2 == 0)
                    Canvas.SetLeft(GameCanvas, originalLeft - 5);
                else
                    Canvas.SetLeft(GameCanvas, originalLeft + 5);
            };

            shakeTimer.Start();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (isPaused) return;

            MovePlayer();
            MoveBoss();
            MoveProjectiles();
            CheckCollisions();

            if (isLeftMouseDown)
            {
                ShootPlayerProjectile();
            }

            ShootBossProjectile();
            DropBigProjectile();
            UpdateGameTimer();
        }

        private void MovePlayer()
        {
            if (Keyboard.IsKeyDown(Key.W) && jumpCount < 2)
            {
                isJumping = true;
                jumpVelocity = 30 * scaleY;
                jumpCount++;
            }

            if (isJumping)
            {
                jumpVelocity -= Gravity * scaleY;
                Canvas.SetBottom(Player, Math.Max(80 * scaleY, Canvas.GetBottom(Player) + jumpVelocity));

                if (Canvas.GetBottom(Player) <= 80 * scaleY)
                {
                    Canvas.SetBottom(Player, 80 * scaleY);
                    isJumping = false;
                    jumpCount = 0;
                }
            }

            if (Keyboard.IsKeyDown(Key.A))
            {
                Canvas.SetLeft(Player, Math.Max(0, Canvas.GetLeft(Player) - 7 * scaleX));
            }
            if (Keyboard.IsKeyDown(Key.D))
            {
                Canvas.SetLeft(Player, Math.Min(GameCanvas.ActualWidth - Player.Width, Canvas.GetLeft(Player) + 7 * scaleX));
            }
            Canvas.SetLeft(PlayerHealthBar, Canvas.GetLeft(Player));
            Canvas.SetBottom(PlayerHealthBar, Canvas.GetBottom(Player) + Player.Height + 10 * scaleY);
        }

        private void MoveBoss()
        {
            double currentX = GameCanvas.ActualWidth - Canvas.GetRight(Boss) - Boss.Width;

            double dx = (bossTargetX - currentX) * 0.05;

            currentX += dx;

            currentX = Math.Max(GameCanvas.ActualWidth * 2 / 3, Math.Min(GameCanvas.ActualWidth - Boss.Width, currentX));

            Canvas.SetRight(Boss, GameCanvas.ActualWidth - currentX - Boss.Width);

            Canvas.SetRight(BossHealthBar, Canvas.GetRight(Boss));
            Canvas.SetBottom(BossHealthBar, Canvas.GetBottom(Boss) + Boss.Height + 10 * scaleY);

            if (Math.Abs(currentX - bossTargetX) < 1)
            {
                SetNewBossTarget();
            }
        }

        private void SetNewBossTarget()
        {
            bossTargetX = random.NextDouble() * (GameCanvas.ActualWidth / 3) + GameCanvas.ActualWidth * 2 / 3;
        }

        private void MoveProjectiles()
        {
            MoveProjectileList(playerProjectiles);
            MoveProjectileList(bossProjectiles);
        }

        private void MoveProjectileList<T>(List<T> projectiles) where T : FrameworkElement
        {
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                T projectile = projectiles[i];
                Vector movement = (Vector)projectile.Tag;
                Canvas.SetLeft(projectile, Canvas.GetLeft(projectile) + movement.X);
                Canvas.SetTop(projectile, Canvas.GetTop(projectile) + movement.Y);

                if (Canvas.GetLeft(projectile) < 0 || Canvas.GetLeft(projectile) > GameCanvas.ActualWidth ||
                    Canvas.GetTop(projectile) < 0 || Canvas.GetTop(projectile) > GameCanvas.ActualHeight)
                {
                    GameCanvas.Children.Remove(projectile);
                    projectiles.RemoveAt(i);
                }
            }
        }

        private void CheckCollisions()
        {
            CheckProjectileCollisions(playerProjectiles, Boss, () =>
            {
                bossHealth--;
                UpdateHealthBars();
                if (bossHealth <= 0) EndGame(true);
            });

            CheckProjectileCollisions(bossProjectiles, Player, () =>
            {
                playerHealth--;
                UpdateHealthBars();
                if (playerHealth <= 0) EndGame(false);
            });

            CheckProjectileCollisions(playerProjectiles, Shirt, () =>
            {
                shirtHealth--;
                UpdateShirtHealthBar();
                if (shirtHealth <= 0)
                {
                    Shirt.Visibility = Visibility.Collapsed;
                    ShirtText.Visibility = Visibility.Collapsed;
                    ShirtHealthBar.Visibility = Visibility.Collapsed;
                }
            });

            CheckPlatformCollisions();
        }

        private void CheckProjectileCollisions<T>(List<T> projectiles, FrameworkElement target, Action onHit) where T : FrameworkElement
        {
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                if (CheckIntersection(projectiles[i], target, target == Boss ? 0.25 : (target == Player ? 0.25 : 1)))
                {
                    onHit();
                    GameCanvas.Children.Remove(projectiles[i]);
                    projectiles.RemoveAt(i);
                }
            }
        }

        private bool CheckIntersection(FrameworkElement element1, FrameworkElement element2, double scaleFactor = 1)
        {
            Rect bounds1 = element1.TransformToVisual(GameCanvas).TransformBounds(new Rect(0, 0, element1.ActualWidth, element1.ActualHeight * 1.25));
            Rect bounds2 = element2.TransformToVisual(GameCanvas).TransformBounds(new Rect(0, 0, element2.ActualWidth * scaleFactor, element2.ActualHeight * scaleFactor * 1.25));
            bounds2.X += element2.ActualWidth * (1 - scaleFactor) / 2;
            bounds2.Y += element2.ActualHeight * (1 - scaleFactor) / 2;
            return bounds1.IntersectsWith(bounds2);
        }

        private void CheckPlatformCollisions()
        {
            bool onPlatform = false;
            foreach (var child in GameCanvas.Children)
            {
                if (child is Image platform && (platform.Name == "Platform1" || platform.Name == "Platform2"))
                {
                    if (CheckIntersection(Player, platform))
                    {
                        double playerBottom = Canvas.GetBottom(Player);
                        double platformTop = Canvas.GetBottom(platform) + platform.Height;

                        if (playerBottom >= platformTop - 5 * scaleY && playerBottom <= platformTop + 5 * scaleY)
                        {
                            Canvas.SetBottom(Player, platformTop);
                            isJumping = false;
                            jumpCount = 0;
                            onPlatform = true;
                        }
                    }
                }
            }

            if (!onPlatform && !isJumping && Canvas.GetBottom(Player) > 80 * scaleY)
            {
                isJumping = true;
                jumpVelocity = 0;
            }
        }

        private void GameCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            mousePosition = e.GetPosition(GameCanvas);
        }

        private void GameCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isLeftMouseDown = true;
            ShootPlayerProjectile();
        }

        private void GameCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isLeftMouseDown = false;
        }

        private void ShootPlayerProjectile()
        {
            if ((DateTime.Now - lastPlayerShotTime).TotalMilliseconds < 400)
            {
                return;
            }

            FrameworkElement projectile = CreateProjectile(@"C:\Users\irber\source\repos\WW\WW\assets\attack1.png");
            Point playerCenter = new Point(Canvas.GetLeft(Player) + Player.Width / 2, GameCanvas.ActualHeight - Canvas.GetBottom(Player) - Player.Height / 2);
            Vector direction = Point.Subtract(mousePosition, playerCenter);
            direction.Normalize();
            direction *= 10 * scaleX;

            Canvas.SetLeft(projectile, playerCenter.X - projectile.Width / 2);
            Canvas.SetTop(projectile, playerCenter.Y - projectile.Height / 2);
            projectile.Tag = direction;

            GameCanvas.Children.Add(projectile);
            playerProjectiles.Add(projectile);
            //-----


            lastPlayerShotTime = DateTime.Now;
        }

        private void ShootBossProjectile()
        {
            if ((DateTime.Now - lastBossShotTime).TotalSeconds < 4)
            {
                return;
            }

            FrameworkElement projectile = CreateProjectile(@"C:\Users\irber\source\repos\WW\WW\assets\projectile_1.png");
            projectile.Width = 180 * scaleX;
            projectile.Height = 180 * scaleY;

            Point bossCenter = new Point(GameCanvas.ActualWidth - Canvas.GetRight(Boss) - Boss.Width / 2, GameCanvas.ActualHeight - Canvas.GetBottom(Boss) - Boss.Height / 2);
            Point playerCenter = new Point(Canvas.GetLeft(Player) + Player.Width / 3, GameCanvas.ActualHeight - Canvas.GetBottom(Player) - Player.Height / 3);
            Vector direction = Point.Subtract(playerCenter, bossCenter);
            direction.Normalize();
            direction *= 5 * scaleX;

            Canvas.SetLeft(projectile, bossCenter.X - projectile.Width / 2);
            Canvas.SetTop(projectile, bossCenter.Y - projectile.Height / 2);
            projectile.Tag = direction;

            GameCanvas.Children.Add(projectile);
            bossProjectiles.Add(projectile);

            lastBossShotTime = DateTime.Now;
        }

        private void DropBigProjectile()
        {
            if ((DateTime.Now - lastBigProjectileTime).TotalSeconds < 3)
            {
                return;
            }

            FrameworkElement projectile = CreateProjectile(@"C:\Users\irber\source\repos\WW\WW\assets\bigProjectile_1.png");
            projectile.Width = 90 * scaleX;
            projectile.Height = 90 * scaleY;

            double leftBound = 0;
            double rightBound = GameCanvas.ActualWidth * 2 / 3;
            double randomX = random.NextDouble() * (rightBound - leftBound) + leftBound;

            Canvas.SetLeft(projectile, randomX);
            Canvas.SetTop(projectile, 0);

            projectile.Tag = new Vector(0, 5 * scaleY);

            GameCanvas.Children.Add(projectile);
            bossProjectiles.Add(projectile);

            lastBigProjectileTime = DateTime.Now;
        }

        private FrameworkElement CreateProjectile(string imagePath)
        {
            return new Image
            {
                Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute)),
                Width = 60 * scaleX,
                Height = 60 * scaleY
            };
        }

        private void UpdateHealthBars()
        {
            PlayerHealthBar.Width = Math.Max(0, playerHealth * 6 * scaleX);
            BossHealthBar.Width = Math.Max(0, bossHealth * 25 * scaleX);
        }

        private void UpdateShirtHealthBar()
        {
            ShirtHealthBar.Width = Math.Max(0, shirtHealth * (50 * scaleX / 3));
        }

        private void UpdateGameTimer()
        {
            elapsedTime = DateTime.Now - gameStartTime;
            GameTimerText.Text = $"Time: {elapsedTime:mm\\:ss}";
        }

        private void EndGame(bool playerWon)
        {
            gameTimer.Stop();
            GameCanvas.Visibility = Visibility.Collapsed;
            MenuGrid.Visibility = Visibility.Visible;
            MessageBox.Show($"\nTime: {elapsedTime:mm\\:ss}\n{(playerWon ? "You Win!" : "Game Over!")}", "Game Finished", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GameCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.P)
            {
                TogglePause();
            }
        }

        private void TogglePause()
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                PauseGrid.Visibility = Visibility.Visible;
                gameTimer.Stop();
            }
            else
            {
                PauseGrid.Visibility = Visibility.Collapsed;
                gameTimer.Start();
                gameStartTime = gameStartTime.Add(DateTime.Now - gameStartTime.Add(elapsedTime));
            }
        }
    }
}
