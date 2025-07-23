using System;
using System.Windows;

namespace Navigation
{
    public partial class App : Application
    {
        public App()
        {
            Activated += StartElmish;
        }

        private void StartElmish(object sender, EventArgs e)
        {
            Activated -= StartElmish;
            Elmish.WPF.Samples.Navigation.Program.main(MainWindow);
        }
    }
}