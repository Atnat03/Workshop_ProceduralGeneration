namespace TreePrint
{
    public enum TreePrintColor
    {
        Red,
        Blue,
        Green,
        Yellow,
        Orange,
        Pink,
        Purple,
        Black,
        Grey,
        White
    }

    public enum ConsoleStyle
    {
        Bold,
        Italic,
        Underline,
    }

    public class TreePrintOption
    {
        public static string GetColor(TreePrintColor treePrintColor)
        {
            switch (treePrintColor)
            {
                case TreePrintColor.Red: return "<color=red>";
                case TreePrintColor.Blue: return "<color=blue>";
                case TreePrintColor.Green: return "<color=green>";
                case TreePrintColor.Yellow: return "<color=yellow>";
                case TreePrintColor.Orange: return "<color=orange>";
                case TreePrintColor.Pink: return "<color=#FFC0CB>";
                case TreePrintColor.Purple: return "<color=purple>";
                case TreePrintColor.Black: return "<color=black>";
                case TreePrintColor.Grey: return "<color=grey>";
                case TreePrintColor.White:
                default: return "<color=white>";
            }
        }

        public static (string, string) GetStyle(ConsoleStyle style)
        {
            switch (style)
            {
                case ConsoleStyle.Bold: return ("<b>", "</b>");
                case ConsoleStyle.Italic: return ("<i>", "</i>");
                default: return ("", "");
            }
        }
    }
}
