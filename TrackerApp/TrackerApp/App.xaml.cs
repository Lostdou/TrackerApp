using TrackerApp.Services;

namespace TrackerApp
{
    public partial class App : Application
    {
        private System.Threading.Timer _timer;
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            MainPage = new MainPage();

            // Iniciamos el ciclo de fondo
            StartBackgroundLoop();
        }

        private void StartBackgroundLoop()
        {

            //var intervalo = TimeSpan.FromMinutes(10);
            var intervalo = TimeSpan.FromSeconds(10);


            _timer = new System.Threading.Timer(async _ =>
            {
                // Como estamos en un hilo secundario (Timer), necesitamos crear un scope
                // para acceder a los servicios inyectados
                using (var scope = _serviceProvider.CreateScope())
                {
                    var trackerService = scope.ServiceProvider.GetRequiredService<TrackerService>();

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await trackerService.CheckAndNotify();
                    });
                }

            }, null, TimeSpan.Zero, intervalo);
        }
    }
}