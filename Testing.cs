using Xunit;
using Hess;

public class Testing
{
    [Theory]
    [InlineData("A10", new int[] { 1, 10 })]
    [InlineData("B8", new int[] { 2, 8 })]
    [InlineData("C5", new int[] { 3, 5 })]
    [InlineData("F11", new int[] { 6, 11 })]
    [InlineData("J12", new int[] { 10, 12 })]
    public void BoardPosToValueTest(string input, int[] expected)
    {
        //Arrange
        HessVisitor program = new HessVisitor();

        //Act
        var result = program.boardPosToValue(input);

        //Assert
        Assert.Equal(expected, result);
    }
    /*
    [Theory]
    [InlineData()]
    public void PlacePiecesTest(object player, string[,] expected)
    {
        //Arrange
        HessVisitor program = new HessVisitor();
        string playerID = "Player1";

        string[,] board = new string[8, 8];
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                board[i, j] = "0";
            }
        }

        //Act

        //Assert
    }

    [Theory]
    public void GetMovesTest()
    {
        //Arrange
        HessVisitor program = new HessVisitor();

        //Act

        //Assert
    }

    [Theory]
    public void ValidateCheckTest()
    {
        //Arrange
        HessVisitor program = new HessVisitor();

        //Act

        //Assert
    }

    [Theory]
    public void ValidateCheckmateTest()
    {
        //Arrange
        HessVisitor program = new HessVisitor();

        //Act

        //Assert
    } */
}
