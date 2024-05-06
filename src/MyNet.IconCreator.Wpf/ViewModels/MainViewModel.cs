// Copyright (c) Stéphane ANDRE. All Right Reserved.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MyNet.IconCreator.Wpf.Properties;
using MyNet.Observable;
using MyNet.Observable.Translatables;
using MyNet.UI.Busy;
using MyNet.UI.Commands;
using MyNet.UI.Dialogs;
using MyNet.UI.Dialogs.Settings;
using MyNet.UI.Extensions;
using MyNet.Utilities;
using MyNet.Utilities.IO.FileExtensions;
using MyNet.Wpf.Extensions;
using MyNet.Wpf.Helpers;
using MyNet.Xaml.Html;

namespace MyNet.IconCreator.ViewModels
{
    internal class MainViewModel : ObservableObject
    {
        private const string FontFamilyKey = "MyNet.Font.Family.{0}";

        public bool ShowText { get; set; }

        public bool IsBold { get; set; }

        public bool IsItalic { get; set; }

        public double Height { get; set; }

        public double Width { get; set; }

        public uint FontSize { get; set; }

        public string Text { get; set; }

        public FontFamily? FontFamily { get; set; }

        public ObservableCollection<DisplayWrapper<FontFamily>> FontFamilies { get; }

        public ObservableCollection<uint> FontSizes { get; }

        public Color? BackgroundColor { get; set; }

        public Color? ForegroundColor { get; set; }

        public bool SeparateColors { get; set; }

        public Color? TextBackgroundColor { get; set; }

        public Color? TextForegroundColor { get; set; }

        public double CornerRadius { get; set; }

        public double OffsetY { get; set; }

        public double OffsetX { get; set; }

        public double IconSize { get; set; }

        public double TextSize { get; set; }

        public IBusyService BusyService { get; }

        public bool ShowIconsList { get; set; }

        public IconSelectionViewModel IconSelectionViewModel { get; } = new();

        public ICommand GenerateImageCommand { get; }

        public ICommand GenerateIconCommand { get; }

        public ICommand FontSizeDownCommand { get; }

        public ICommand FontSizeUpCommand { get; }

        public MainViewModel(IBusyService busyService)
        {
            BusyService = busyService;
            var families = Fonts.SystemFontFamilies.Select(x => new DisplayWrapper<FontFamily>(x, x.ToString())).ToList();
            var customFonts = new[] { "Nuvel", "Daggersquare", "Roboto", "Jersey" };
            customFonts.Select(x => (Key: x, FontFamily: WpfHelper.GetResourceOrDefault<FontFamily>(string.Format(CultureInfo.InvariantCulture, FontFamilyKey, x))))
                       .Where(x => x.FontFamily is not null)
                       .ForEach(x => families.Add(new DisplayWrapper<FontFamily>(x.FontFamily!, x.Key)));

            FontFamilies = new(families.OrderBy(x => x.DisplayName.ToString()));
            FontSizes = new(new uint[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 });

            ShowText = IconSettings.Default.ShowText;
            IsBold = IconSettings.Default.IsBold;
            IsItalic = IconSettings.Default.IsItalic;
            Height = IconSettings.Default.Height;
            Width = IconSettings.Default.Width;
            FontSize = IconSettings.Default.FontSize;
            Text = IconSettings.Default.Text;
            SeparateColors = IconSettings.Default.SeparateColors;
            FontFamily = !string.IsNullOrEmpty(IconSettings.Default.FontFamily) ? FontFamilies.Select(x => x.Item).FirstOrDefault(x => x.Source == IconSettings.Default.FontFamily) : null;
            BackgroundColor = !string.IsNullOrEmpty(IconSettings.Default.BackgroundColor) ? IconSettings.Default.BackgroundColor.ToColor() : null;
            ForegroundColor = !string.IsNullOrEmpty(IconSettings.Default.ForegroundColor) ? IconSettings.Default.ForegroundColor.ToColor() : null;
            TextBackgroundColor = !string.IsNullOrEmpty(IconSettings.Default.TextBackgroundColor) ? IconSettings.Default.TextBackgroundColor.ToColor() : null;
            TextForegroundColor = !string.IsNullOrEmpty(IconSettings.Default.TextForegroundColor) ? IconSettings.Default.TextForegroundColor.ToColor() : null;
            IconSelectionViewModel.SelectedData = IconSettings.Default.IconData;
            OffsetX = IconSettings.Default.OffsetX;
            OffsetY = IconSettings.Default.OffsetY;
            CornerRadius = IconSettings.Default.CornerRadius;
            IconSize = IconSettings.Default.IconSize;
            TextSize = IconSettings.Default.TextSize;

            GenerateImageCommand = CommandsManager.CreateNotNull<object>(async x => await SaveImageAsync(x).ConfigureAwait(false), x => x is not null);
            GenerateIconCommand = CommandsManager.CreateNotNull<object>(async x => await SaveIconAsync(x).ConfigureAwait(false), x => x is not null);
            FontSizeDownCommand = CommandsManager.Create(() => FontSize--, () => FontSize > 1);
            FontSizeUpCommand = CommandsManager.Create(() => FontSize++, () => FontSize < 200);
        }

        private async Task SaveImageAsync(object x)
        {
            var fe = (FrameworkElement)x;
            var (result, filename) = await DialogManager.ShowSaveFileDialogAsync(new SaveFileDialogSettings
            {
                Filters = FileExtensionFilterBuilderProvider.AllImages.GenerateFilters(),
                DefaultExtension = FileExtensionInfoProvider.Png.GetDefaultExtension(),
                FileName = Text,
                CheckFileExists = false
            }).ConfigureAwait(false);

            if (result.IsTrue())
                await BusyService.WaitIndeterminateAsync(() => Observable.Threading.Scheduler.GetUIOrCurrent().Schedule(() => XamlToImageFileService.SaveImage(fe, filename))).ConfigureAwait(false);
        }

        private async Task SaveIconAsync(object x)
        {
            var fe = (FrameworkElement)x;
            var (result, filename) = await DialogManager.ShowSaveFileDialogAsync(new SaveFileDialogSettings
            {
                Filters = FileExtensionInfoProvider.Ico.GetFileFilters(),
                FileName = Text,
                CheckFileExists = false
            }).ConfigureAwait(false);

            if (result.IsTrue())
                await BusyService.WaitIndeterminateAsync(() => Observable.Threading.Scheduler.UI.Schedule(() => XamlToImageFileService.SaveIcon(fe, filename))).ConfigureAwait(false);
        }

        public void SaveSettings()
        {
            IconSettings.Default.BackgroundColor = BackgroundColor?.ToString(CultureInfo.InvariantCulture);
            IconSettings.Default.FontFamily = FontFamily?.Source;
            IconSettings.Default.FontSize = FontSize;
            IconSettings.Default.ForegroundColor = ForegroundColor?.ToString(CultureInfo.InvariantCulture);
            IconSettings.Default.Height = Height;
            IconSettings.Default.IconData = IconSelectionViewModel.SelectedData;
            IconSettings.Default.IsBold = IsBold;
            IconSettings.Default.IsItalic = IsItalic;
            IconSettings.Default.SeparateColors = SeparateColors;
            IconSettings.Default.ShowText = ShowText;
            IconSettings.Default.Text = Text;
            IconSettings.Default.TextBackgroundColor = TextBackgroundColor?.ToString(CultureInfo.InvariantCulture);
            IconSettings.Default.TextForegroundColor = TextForegroundColor?.ToString(CultureInfo.InvariantCulture);
            IconSettings.Default.Width = Width;
            IconSettings.Default.OffsetX = OffsetX;
            IconSettings.Default.OffsetY = OffsetY;
            IconSettings.Default.CornerRadius = CornerRadius;
            IconSettings.Default.TextSize = TextSize;
            IconSettings.Default.IconSize = IconSize;

            IconSettings.Default.Save();
        }
    }
}
