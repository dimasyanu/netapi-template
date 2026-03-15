namespace NetApi.Application.Common.Models;

public enum ExcelCellHorizontalAlign
{
    Inherit,
    Left,
    Center,
    Right,
}

public enum ExcelCellVerticalAlign
{
    Inherit,
    Top,
    Middle,
    Bottom,
}

public enum ExcelCellDataType
{
    Blank,
    Boolean,
    Number,
    Text,
    Error,
    DateTime,
    TimeSpan
}

public class ExcelCell
{
    public ExcelCellHorizontalAlign HorizontalAlign { get; set; } = ExcelCellHorizontalAlign.Left;
    public ExcelCellVerticalAlign VerticalAlign { get; set; } = ExcelCellVerticalAlign.Middle;
    public ExcelCellDataType DataType { get; set; } = ExcelCellDataType.Text;

    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public IEnumerable<ExcelCell> SubValues { get; set; } = [];
}

