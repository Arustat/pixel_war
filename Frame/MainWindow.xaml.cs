﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Frame
{
    public partial class MainWindow : Window
    {
        private ScaleTransform _scaleTransform = new ScaleTransform(1, 1); // Pour gérer le zoom
        private Brush _currentColor = Brushes.White; // La couleur actuelle des pixels
        private string _pseudo; // Nouveau champ pour le pseudo
        
        //Eviter conflit souris et click (jpp)
        private bool zoommouse = false;

        public MainWindow(string pseudo)
        {
            InitializeComponent();
            _pseudo = pseudo; // Enregistrer le pseudo
            TxtPseudo.Text = _pseudo;
            
            //Initialment zoom 
            _scaleTransform.ScaleX = 0.24912826983452546;
            _scaleTransform.ScaleY = 0.24912826983452546;

            // Appliquer le ScaleTransform à la grille pour permettre le zoom
            GrilleCanvas.LayoutTransform = _scaleTransform;
            
            // Générer la grille de pixels
            Grille(100, 200, 20); // Grille de 100x200 avec des pixels de taille 20x20
            
            //Instancie la classe ColorPalette pour notre button
            var colorPalette = new ColorPalette();
            
            // S'abonner à l'événement ColorChanged pour récupérer la couleur
            colorPalette.ColorChanged += OnColorChanged;
            
            //Ajouter le bouton Windows Forms au conteneur WindowsFormsHost
            ColorButtonHost.Child = colorPalette.Controls[0];
            
            // Ajouter l'événement MouseWheel pour le zoom
            this.MouseWheel += MainWindow_MouseWheel;
            
            // Ajouter l'événement de double-clic pour zoomer sur un point spécifique
            GrilleCanvas.PreviewMouseDown += GrilleCanvasDoubleClick;
        }

        // Méthode pour recevoir la nouvelle couleur et mettre à jour _currentColor
        private void OnColorChanged(System.Drawing.Color newColor)
        {
            // Convertir la couleur choisie de System.Drawing.Color à System.Windows.Media.Color
            var wpfColor = System.Windows.Media.Color.FromArgb(
                newColor.A, newColor.R, newColor.G, newColor.B);

            // Convertir en SolidColorBrush et mettre à jour _currentColor
            _currentColor = new SolidColorBrush(wpfColor);
        }

        // Méthode pour générer la grille de pixels
        private void Grille(int rows, int cols, double pixelSize)
        {
            // Effacer la grille précédente
            GrilleCanvas.Children.Clear();
            
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    // Créer un rectangle pour chaque pixel
                    Rectangle pixel = new Rectangle
                    {
                        Width = pixelSize,
                        Height = pixelSize,
                        Fill = Brushes.LightGray, // Couleur par défaut du pixel
                        Stroke = Brushes.DarkGray, // Bordure des pixels
                        StrokeThickness = 0.1
                    };

                    // Positionner chaque pixel sur le canvas
                    Canvas.SetLeft(pixel, col * pixelSize);
                    Canvas.SetTop(pixel, row * pixelSize);

                    // Ajouter le pixel au Canvas
                    GrilleCanvas.Children.Add(pixel);

                    // Ajouter un événement MouseDown pour changer la couleur du pixel lorsqu'on clique dessus
                    pixel.MouseDown += Pixel_MouseDown;
                }
            }

            // Ajuster les dimensions du Canvas en fonction de la taille de la grille
            GrilleCanvas.Width = cols * pixelSize;
            GrilleCanvas.Height = rows * pixelSize;
        }

        // Gestion du zoom lors du défilement de la souris
        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoomFactor = e.Delta > 0 ? 1.1 : 0.9; // Facteur de zoom

            // Limite minimale de zoom (20%)
            double minZoom = 0.2;

            // Limite maximale de zoom (200%)
            double maxZoom = 5.0;

            // Appliquer le zoom seulement si la nouvelle échelle est dans les limites
            if (_scaleTransform.ScaleX * zoomFactor >= minZoom && _scaleTransform.ScaleX * zoomFactor <= maxZoom)
            {
                _scaleTransform.ScaleX *= zoomFactor;
                _scaleTransform.ScaleY *= zoomFactor;
                
                //Console.WriteLine($"Zoom X: {_scaleTransform.ScaleX}, Zoom Y: {_scaleTransform.ScaleY}");
            }
        }

        // Gestion du clic sur un pixel pour changer sa couleur
        private void Pixel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle pixel)
            {
                // Changer la couleur du pixel au clic
                pixel.Fill = _currentColor;
            }
        }

        // Méthode pour gérer le double-clic sur la grille
        private void GrilleCanvasDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Vérifier si c'est un double-clic
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                // Calculer la position du clic (les coordonnées du clic dans le canvas)
                Point clickPosition = e.GetPosition(GrilleCanvas);

                // Facteur de zoom de 30% (soit un zoom de 1.3)
                double zoomFactor = 1.3;

                // Calculer les nouvelles échelles de zoom
                double newScaleX = _scaleTransform.ScaleX * zoomFactor;
                double newScaleY = _scaleTransform.ScaleY * zoomFactor;

                // Limiter les valeurs de zoom (par exemple entre 0.2 et 5)
                double minZoom = 0.2;
                double maxZoom = 5.0;
                newScaleX = Math.Max(minZoom, Math.Min(newScaleX, maxZoom));
                newScaleY = Math.Max(minZoom, Math.Min(newScaleY, maxZoom));

                // Appliquer le zoom au ScaleTransform
                _scaleTransform.ScaleX = newScaleX;
                _scaleTransform.ScaleY = newScaleY;

                // Appliquer la transformation de zoom à la grille
                GrilleCanvas.LayoutTransform = _scaleTransform;

                // Déplacer le ScrollViewer pour centrer sur le point cliqué
                ScrollViewer scrollViewer = GrilleScrollViewer;
                double offsetX = clickPosition.X * (zoomFactor - 1);
                double offsetY = clickPosition.Y * (zoomFactor - 1);

                // Ajuster le défilement en fonction du zoom
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + offsetX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offsetY);
            }
        }
    }
}
