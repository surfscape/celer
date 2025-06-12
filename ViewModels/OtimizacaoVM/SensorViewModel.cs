using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using Celer.Models.Sensors;
using CommunityToolkit.Mvvm.ComponentModel;
using LibreHardwareMonitor.Hardware;

namespace Celer.ViewModels.OtimizacaoVM
{
    public partial class SensorViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isLoading = true;
        public ObservableCollection<SensorCategoryModel> Categories { get; } = [];

        private readonly Computer _computer = new()
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMotherboardEnabled = true,
            IsStorageEnabled = true,
            IsMemoryEnabled = true,
        };

        private DispatcherTimer _updateTimer = new() { Interval = TimeSpan.FromSeconds(1) };

        public SensorViewModel()
        {
            _updateTimer.Tick += (_, _) => Update();
        }

        public async Task Initialize()
        {
            try
            {
                await Task.Run(() =>
                {
                    _computer.Open();
                    LoadSensors();
                });
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    $"Error initializing sensors: {e.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                IsLoading = false;
                _updateTimer.Start();
            }
        }

        private void LoadSensors()
        {
            Application.Current.Dispatcher.Invoke(() => Categories.Clear());

            foreach (var hardware in _computer.Hardware)
            {
                hardware.Update();
                var category = new SensorCategoryModel(hardware.HardwareType.ToString());

                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType is SensorType.Temperature or SensorType.Fan)
                        category.AddSensor(sensor);
                }

                if (category.Sensors.Count > 0)
                    Application.Current.Dispatcher.Invoke(() => Categories.Add(category));
            }
        }

        private void Update()
        {
            foreach (var hardware in _computer.Hardware)
                hardware.Update();

            foreach (var category in Categories)
                category.Update();
        }

        public async Task StartTimer()
        {
            if (!_updateTimer.IsEnabled)
            {
                await Task.Run(() =>
                {
                    _computer.Open();
                    LoadSensors();
                });
                _updateTimer.Start();
            }
        }

        public async Task StopTimer()
        {
            _updateTimer.Stop();
            await Task.Run(() =>
            {
                _computer.Close();
            });
        }
    }
}
