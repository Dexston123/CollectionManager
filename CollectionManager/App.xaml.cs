using Microsoft.Maui.Controls;
using System;
using System.IO;
using System.Diagnostics;

namespace CollectionManager
{
    public partial class App : Application
    {
        public static string FolderPath { get; private set; }

        public App()
        {
            InitializeComponent();
            FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CollectionManagerData");
            Debug.WriteLine($"Path to the application data folder: {FolderPath}");

            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);

            MainPage = new NavigationPage(new CollectionsPage());
        }
    }
}