using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HaskellTools.EditorMargins
{
    internal static class StatusColors
    {
        private static SolidColorBrush _statusBarBackground;
        public static SolidColorBrush StatusBarBackground() {
            if (_statusBarBackground != null)
                return _statusBarBackground;
            var color = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundBrushKey);
            _statusBarBackground = new SolidColorBrush(new Color() { R = color.R, G = color.G, B = color.B, A = color.A });
            return _statusBarBackground;
        }

        private static SolidColorBrush _statusItemNormalBackground;
        public static SolidColorBrush StatusItemNormalBackground()
        {
            if (_statusItemNormalBackground != null)
                return _statusItemNormalBackground;
            var color = VSColorTheme.GetThemedColor(EnvironmentColors.ScrollBarArrowGlyphBrushKey);
            _statusItemNormalBackground = new SolidColorBrush(new Color() { R = color.R, G = color.G, B = color.B, A = color.A });
            return _statusItemNormalBackground;
        }

        private static SolidColorBrush _statusItemBadBackground;
        public static SolidColorBrush StatusItemBadBackground()
        {
            if (_statusItemBadBackground != null)
                return _statusItemBadBackground;
            var color = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowValidationErrorBorderBrushKey);
            _statusItemBadBackground = new SolidColorBrush(new Color() { R = color.R, G = color.G, B = color.B, A = color.A });
            return _statusItemBadBackground;
        }

        private static SolidColorBrush _statusItemGoodBackground;
        public static SolidColorBrush StatusItemGoodBackground()
        {
            if (_statusItemGoodBackground != null)
                return _statusItemGoodBackground;
            var color = VSColorTheme.GetThemedColor(EnvironmentColors.VizSurfaceGreenLightBrushKey);
            _statusItemGoodBackground = new SolidColorBrush(new Color() { R = color.R, G = color.G, B = color.B, A = color.A });
            return _statusItemGoodBackground;
        }
    }
}
