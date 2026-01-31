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

            // Loop
            StartBackgroundLoop();
        }

        private void StartBackgroundLoop()
        {
            
            var intervalo = TimeSpan.FromHours(2);
            // Para pruebas
            //var intervalo = TimeSpan.FromSeconds(30);

            _timer = new System.Threading.Timer(async _ =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var service = scope.ServiceProvider.GetRequiredService<TrackerService>();
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await service.CheckAndNotify();
                    });
                }
            }, null, TimeSpan.Zero, intervalo);
        }
    }
}