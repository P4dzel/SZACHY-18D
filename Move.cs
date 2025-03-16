public class Move
{
    public int StartRow { get; }
    public int StartCol { get; }
    public int TargetRow { get; }
    public int TargetCol { get; }
    public int PieceType { get; }
    public int? CapturedPiece { get; }

    public Move(int startRow, int startCol, int targetRow, int targetCol, int pieceType, int? capturedPiece = null)
    {
        StartRow = startRow;
        StartCol = startCol;
        TargetRow = targetRow;
        TargetCol = targetCol;
        PieceType = pieceType;
        CapturedPiece = capturedPiece;
    }

    public int GetStartRow()
    {
        return StartRow;
    }
    public int GetStartCol()
    {
        return StartCol;
    }
    public int GetTargetRow()
    {
        return TargetRow;
    }
    public int GetTargetCol()
    {
        return TargetCol;
    }
    public int GetPieceType()
    {
        return PieceType;
    }

}
