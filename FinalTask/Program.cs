namespace AdvancedTaskSimulation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Iniciando simulación de planta industrial...\n");

            int numberOfLines = 4;
            int maxIterations = 10;
            TimeSpan maxSimulationTime = TimeSpan.FromSeconds(30);

            var manager = new ProductionManager(numberOfLines, maxIterations);

            var simulationTask = manager.StartSimulationAsync();

            if (await Task.WhenAny(simulationTask, Task.Delay(maxSimulationTime)) == simulationTask)
            {
                Console.WriteLine("\nSimulación completada con éxito.");
            }
            else
            {
                Console.WriteLine("\nSimulación detenida: tiempo límite excedido.");
            }
        }
    }

    public class ProductionLine
    {
        private static readonly Random Randomizer = new();
        private readonly int _lineId;

        public ProductionLine(int lineId)
        {
            _lineId = lineId;
        }

        public async Task SimulateProcessAsync(int iteration)
        {
            Console.WriteLine($"[Línea {_lineId}] Iteración {iteration}: Iniciando proceso...");

            try
            {
                await Task.Delay(Randomizer.Next(500, 2000));

                if (Randomizer.NextDouble() < 0.3) 
                {
                    throw new InvalidOperationException($"[Línea {_lineId}] Iteración {iteration}: Error en el proceso.");
                }

                Console.WriteLine($"[Línea {_lineId}] Iteración {iteration}: Proceso completado con éxito.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    public class ProductionManager
    {
        private readonly List<ProductionLine> _lines;
        private readonly int _maxIterations;

        public ProductionManager(int numberOfLines, int maxIterations)
        {
            _lines = Enumerable.Range(1, numberOfLines).Select(id => new ProductionLine(id)).ToList();
            _maxIterations = maxIterations;
        }

        public async Task StartSimulationAsync()
        {
            var tasks = new List<Task>();

            for (int iteration = 1; iteration <= _maxIterations; iteration++)
            {
                foreach (var line in _lines)
                {
                    var task = Task.Run(() => line.SimulateProcessAsync(iteration))
                        .ContinueWith(t =>
                        {
                            if (t.Exception != null)
                            {
                                Console.WriteLine($"[Error] Línea fallida: {t.Exception.InnerException?.Message}");
                            }
                        }, TaskContinuationOptions.OnlyOnFaulted);

                    tasks.Add(task);
                }

                await Task.WhenAny(tasks);

                Console.WriteLine($"[Gestión] Iteración {iteration} completada.\n");
            }

            await Task.WhenAll(tasks);

            Console.WriteLine("Simulación finalizada para todas las líneas de producción.");
        }
    }
}
