using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VSDiagnostics;

namespace Brittany_Salon_Backend.Benchmarks
{
    [CPUUsageDiagnoser]
    public class ImageProcessingBenchmark
    {
        private IImageService _imageService;
        private IFormFile _testImage;
        [GlobalSetup]
        public void Setup()
        {
            // Create a test image
            using var image = new Image<Rgba32>(1000, 1000);
            image.Mutate(x => x.BackgroundColor(Color.White));
            var stream = new MemoryStream();
            image.SaveAsJpeg(stream);
            stream.Position = 0;
            var formFile = new FormFile(stream, 0, stream.Length, "test", "test.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
            _testImage = formFile;
            // Setup service
            var services = new ServiceCollection();
            services.AddScoped<IImageService, ImageService>();
            services.AddSingleton<IWebHostEnvironment>(new TestWebHostEnvironment());
            var provider = services.BuildServiceProvider();
            _imageService = provider.GetRequiredService<IImageService>();
        }

        [Benchmark]
        public async Task ProcessImage()
        {
            await _imageService.ProcessAndSaveImageAsync(_testImage, "benchmark", 1);
        }
    }

    public class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Test";
        public IFileProvider ContentRootFileProvider { get; set; }
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public string EnvironmentName { get; set; } = "Development";
        public IFileProvider WebRootFileProvider { get; set; }
        public string WebRootPath { get; set; } = Path.GetTempPath();
    }
}