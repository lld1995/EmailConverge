using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using EmailConverge.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace EmailConverge
{
    public partial class MainWindow : Window
    {
        private readonly EmailParserService _emailParser;
        private readonly AiSummaryService _aiSummaryService;
        private readonly ObservableCollection<string> _selectedFiles;
        private CancellationTokenSource? _cts;

        public MainWindow()
        {
            InitializeComponent();
            _emailParser = new EmailParserService();
            _aiSummaryService = new AiSummaryService();
            _selectedFiles = new ObservableCollection<string>();
            FileListControl.ItemsSource = _selectedFiles;
            InitializeTemplateComboBox();
            LoadConfig();
        }

        private void InitializeTemplateComboBox()
        {
            var templates = SummaryTemplates.GetAllTypes()
                .Select(t => new TemplateItem { Type = t, Name = SummaryTemplates.GetTemplateName(t) })
                .ToList();
            TemplateComboBox.ItemsSource = templates;
            TemplateComboBox.SelectedIndex = 0;
        }

        private async void LoadConfig()
        {
            var config = _aiSummaryService.GetConfig();
            EndpointTextBox.Text = config.Endpoint;
            ApiKeyTextBox.Text = config.ApiKey;
            
            // 加载模型列表
            await LoadModelsAsync();
            
            // 设置当前选中的模型
            if (!string.IsNullOrEmpty(config.Model))
            {
                ModelComboBox.Text = config.Model;
            }
        }

        private async System.Threading.Tasks.Task LoadModelsAsync()
        {
            try
            {
                LoadModelsButton.IsEnabled = false;
                var endpoint = EndpointTextBox.Text;
                var apiKey = ApiKeyTextBox.Text;
                var models = await _aiSummaryService.GetModelsAsync(endpoint, apiKey);
                
                if (models.Count > 0)
                {
                    ModelComboBox.ItemsSource = models;
                }
            }
            finally
            {
                LoadModelsButton.IsEnabled = true;
            }
        }

        private async void LoadModelsButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadModelsAsync();
        }

        private async void SelectFilesButton_Click(object sender, RoutedEventArgs e)
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "选择邮件文件",
                AllowMultiple = true,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("邮件文件") { Patterns = new[] { "*.msg", "*.eml" } }
                }
            });

            if (files.Count > 0)
            {
                foreach (var file in files)
                {
                    var path = file.Path.LocalPath;
                    if (!_selectedFiles.Contains(path))
                    {
                        _selectedFiles.Add(path);
                    }
                }

                UpdateFileCount();
                ParseAndDisplayEmails();
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedFiles.Clear();
            ResultTextBox.Text = string.Empty;
            AiSummaryTextBox.Text = string.Empty;
            UpdateFileCount();
            _cts?.Cancel();
        }

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigPanel.IsVisible = !ConfigPanel.IsVisible;
        }

        private void SaveConfigButton_Click(object sender, RoutedEventArgs e)
        {
            var config = new AiConfig
            {
                Endpoint = EndpointTextBox.Text ?? "http://localhost:11434/v1",
                ApiKey = ApiKeyTextBox.Text ?? "ollama",
                Model = ModelComboBox.Text ?? "qwen2.5:7b"
            };
            _aiSummaryService.UpdateConfig(config);
            ConfigPanel.IsVisible = false;
        }

        private void CancelConfigButton_Click(object sender, RoutedEventArgs e)
        {
            LoadConfig();
            ConfigPanel.IsVisible = false;
        }

        private async void AiSummaryButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ResultTextBox.Text))
            {
                AiSummaryTextBox.Text = "请先选择邮件文件";
                return;
            }

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            AiSummaryButton.IsEnabled = false;
            AiSummaryTextBox.Text = "正在生成AI总结...";

            try
            {
                AiSummaryTextBox.Text = string.Empty;
                var selectedTemplate = (TemplateComboBox.SelectedItem as TemplateItem)?.Type ?? SummaryTemplateType.KeyPoints;
                await _aiSummaryService.StreamSummarizeAsync(
                    ResultTextBox.Text,
                    selectedTemplate,
                    token => Dispatcher.UIThread.Post(() => AiSummaryTextBox.Text += token),
                    _cts.Token);
            }
            finally
            {
                AiSummaryButton.IsEnabled = true;
            }
        }

        private void UpdateFileCount()
        {
            FileCountText.Text = _selectedFiles.Count > 0 
                ? $"已选择 {_selectedFiles.Count} 个文件" 
                : "未选择文件";
        }

        private void ParseAndDisplayEmails()
        {
            if (_selectedFiles.Count == 0)
            {
                ResultTextBox.Text = string.Empty;
                return;
            }

            var emails = _emailParser.ParseMultipleEmailFiles(_selectedFiles);
            ResultTextBox.Text = _emailParser.GetCombinedText(emails);
        }
    }
}